using Godot;
using System;
using System.Collections.Generic;

namespace USG;

public record struct CellPosition
{
	public int Row;
	public int Col;

	public CellPosition(int row, int col)
	{
		Row = row;
		Col = col;
	}

	public CellPosition(Godot.Vector2I vec)
	{
		Row = vec.X;
		Col = vec.Y;
	}

	public CellPosition((int row, int col) pos)
	{
		Row = pos.row;
		Col = pos.col;
	}

	public static CellPosition Zero => new(0,0);
	public static CellPosition Down => new(1,0);
	public static CellPosition Up => new(-1,0);
	public static CellPosition Right => new(0,1);
	public static CellPosition Left => new(0,-1);

	public static CellPosition operator + (CellPosition pos1, CellPosition pos2)
	{
		return new(pos1.Row + pos2.Row, pos1.Col + pos2.Col);
	}

	public static CellPosition operator - (CellPosition pos1, CellPosition pos2)
	{
		return new(pos1.Row - pos2.Row, pos1.Col - pos2.Col);
	}
}

public enum RotationDirection
{
	Left = 0,
	Right = 1,
	FullRotation = 2,
};

public enum RotationState
{
	North = 0,
	East = 1,
	South = 2,
	West = 3,
};

public enum SpinType
{
	NoSpin = 0,
	SpinMini = 1,
	TrueSpin = 2,
}

public partial class Piece {

	
	public enum MovementType;
	private static RotationState Add(RotationState state, int i)
	{
		return (RotationState)(((int)state + (i % 4)) % 4);
	}

	const int MAX_LOCK_DELAY_RESETS = 15;

	private readonly string id;
	private int order;
	private int[,] submatrix;
	private CellPosition relativePosition;
	private GameBoard board;
	private int spawningRowOffset; // indicates how many rows above the board's top row the piece should spawn.
	private RotationState rotationState;
	private double lockDelay;
	private double lockDelayTimerSeconds;
	private int lockDelayResets;
	private bool isLastMoveRotation;
	public bool SpinState { get; private set; }

	public GameBoard Board { get => board; set { board = value; } }
	public int RelativeRow { get => relativePosition.Row; set { 
		relativePosition = relativePosition with 
		{
			Row = value
		};
	}
	}
	public int RelativeCol { get => relativePosition.Col; set { 
		relativePosition = relativePosition with 
		{
			Col = value
		};
	}
	}
	public (int Row, int Col) RelativePosition { get => (relativePosition.Row, relativePosition.Col); 
	set {
		relativePosition = new CellPosition(value);
	}
	} 
	public int SpawningRowOffset => spawningRowOffset;
	public int[,] Submatrix => submatrix;
	public int Size => submatrix.GetLength(0); // returns the order of the piece's submatrix
	public RotationState Rotation => rotationState;
	public string ID  => id;

	public delegate void PieceSpinnedEventHandler(SpinType spinType);
	public event PieceSpinnedEventHandler PieceSpinned;

	public double LockDelay 
	{ 
		get => lockDelay; 
		set {
			if(value >= 0.0)
			{
				lockDelay = value;
			} else {
				throw new ArgumentException($"Tried to set LockDelay to {value}. Lock delay can't be negative.");
			}
		}
	}

	public double LockDelayTimerSeconds => lockDelayTimerSeconds;

	public Piece(Piece other)
	{
		this.id = other.id;
		this.order = other.order;
		this.submatrix = other.submatrix.Clone() as int[,];
		this.relativePosition = CellPosition.Zero;
		this.board = other.board;
		this.spawningRowOffset = other.spawningRowOffset;
		this.rotationState = other.rotationState;
		this.LockDelay = other.LockDelay;
		this.lockDelayTimerSeconds = other.lockDelayTimerSeconds;
		this.lockDelayResets = other.lockDelayResets;
	}

	public Piece(String name, GameBoard board) : this(Pieces.PiecesMap[name])
	{
		this.board = board;
	} 

	public Piece(String name, int[,] submatrix, GameBoard board = null, int spawningRowOffset = 2)
	{
		this.id = name;
		if(submatrix.GetLength(0) != submatrix.GetLength(1))
		{
			throw new ArgumentException("Invalid piece submatrix at Piece constructor - submatrix is not square");
		}
		this.submatrix = submatrix;
		this.order = 0;
		foreach(int tile in submatrix)
		{
			if(tile > 0){
				order++;
			}
		}
		this.relativePosition = CellPosition.Zero;
		this.board = board;
		this.spawningRowOffset = spawningRowOffset;
		this.lockDelayTimerSeconds = 0.0;
		this.lockDelayResets = 0;
		this.isLastMoveRotation = false;
		this.SpinState = false;
	}

	private int GetRelativeTileValueInBoard(int i, int j)
	{
		if(0 <= i && i < Size
		&& 0 <= j && j < Size)
		{
			int trueRow = i + relativePosition.Row;
			int trueCol = j + relativePosition.Col;
			if(0 <= trueRow && trueRow < board.BoardTrueHeight
			&& 0 <= trueCol && trueCol < board.BoardWidth)
			{
				return board.GetTileAt(trueRow, trueCol);
			} else {
				return GameBoard.INVALID_TILE_VALUE;
			}
		} else {
			throw new IndexOutOfRangeException($"Tile index out of range: ({i},{j}) in Piece.GetRelativeTileValueInBoard");
		}
	}

	public bool IsCollidingWithBoard()
	{
		for(int i = 0; i < Size; i++)
		{
			for(int j = 0; j < Size; j++)
			{
				if(GetRelativeTileValueInBoard(i, j) != 0
				&& submatrix[i,j] > 0)
				{
					return true;
				}
			}
		}
		return false;
	}
	
	public bool IsCollidingWithBoardLimit()
	{
		for(int i = 0; i < Size; i++)
		{
			for(int j = 0; j < Size; j++)
			{
				if(GetRelativeTileValueInBoard(i,j) == GameBoard.INVALID_TILE_VALUE
				&& submatrix[i,j] > 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	private void Move((int rowOffset, int colOffset) offset)
	{
		relativePosition = relativePosition with {
			Row = relativePosition.Row + offset.rowOffset,
			Col = relativePosition.Col + offset.colOffset,
		};
	}

	private void Move(CellPosition offset)
	{
		relativePosition = relativePosition with {
			Row = relativePosition.Row + offset.Row,
			Col = relativePosition.Col + offset.Col,
		};
	}

	private void Move(int rowOffset, int colOffset)
	{
		relativePosition = relativePosition with {
			Row = relativePosition.Row + rowOffset,
			Col = relativePosition.Col + colOffset,
		};
	}

	public bool TryMove((int rowOffset, int colOffset) offset, bool resetLastMoveRotation = false, bool resetLockDelay = true)
	{
		bool wasOnFloor = (IsOnFloor() && offset.rowOffset <= 0);

		Move(offset);
		if(IsCollidingWithBoard())
		{
			Move((-offset.rowOffset, -offset.colOffset));
			return false;
		}
		if(wasOnFloor && resetLockDelay)
		{
			ResetLockDelay();
		}
		if(resetLastMoveRotation)
		{
			isLastMoveRotation = false;
		}
		return true;
	}

	public bool TryMove(CellPosition offset, bool resetLastMoveRotation = true, bool resetLockDelay = true)
	{
		bool wasOnFloor = (IsOnFloor() && offset.Row <= 0);
		
		Move(offset);
		if(IsCollidingWithBoard())
		{
			Move(offset with { Row = -offset.Row, Col = -offset.Col });
			return false;
		}
		if(wasOnFloor && resetLockDelay)
		{
			ResetLockDelay();
		}
		if(resetLastMoveRotation)
		{
			isLastMoveRotation = false;
		}
		return true;
	}

	public bool TryMove(int rowOffset, int colOffset, bool resetLastMoveRotation = false, bool resetLockDelay = true)
	{
		bool wasOnFloor = (IsOnFloor() && rowOffset <= 0);
		
		Move(rowOffset, colOffset);
		if(IsCollidingWithBoard())
		{
			Move(-rowOffset, -colOffset);
			return false;
		}
		if(wasOnFloor && resetLockDelay)
		{
			ResetLockDelay();
		}
		if(resetLastMoveRotation)
		{
			isLastMoveRotation = false;
		}
		return true;
	}

	public bool IsOnFloor()
	{
		for(int i = this.Size - 1; i >= 0; i--)
		{ // iterates in inverse row order in hopes that scaning the bottom minos first will detect a collision earlier
			for(int j = 0; j < this.Size; j++)
			{
				if(Submatrix[i,j] > 0)
				{
					int boardRow = relativePosition.Row + i + 1; // + 1 offset to check the tile immediately below
					int boardCol = relativePosition.Col + j;

					if(boardRow < 0 || boardRow >= board.BoardTrueHeight // tile below is out of bounds
					|| boardCol < 0 || boardCol >= board.BoardWidth
					|| board.GetTileAt(boardRow, boardCol) != 0) // tile below is occupied
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public void SetDefaultSpawningPosition()
	{
		if(board is null)
		{

		} else {
			int boardWidth = board.BoardWidth;
			int boardTopRow = board.BoardHiddenPortionHeight;
			relativePosition.Row = boardTopRow - spawningRowOffset;
			relativePosition.Col = (boardWidth - this.Size)/2;
		}
	}

	private void ResetLockDelay()
	{
		if(lockDelayResets < MAX_LOCK_DELAY_RESETS)
		{
			lockDelayTimerSeconds = 0.0;
			lockDelayResets++;
		}
	}

	private void TransposeMatrix()
	{
		for(int i = 0; i < Size; i++)
		{
			for(int j = i; j < Size; j++)
			{
				(submatrix[i,j], submatrix[j,i]) 
				= (submatrix[j,i], submatrix[i,j]);
			}
		}
	}
	private void FlipMatrixHorizontal()
	{
		for(int i = 0; i < Size; i++)
		{
			for(int j = 0; j < Size / 2; j++)
			{
				(submatrix[i,j], submatrix[i, Size - 1 -  j]) 
				= (submatrix[i, Size - 1 - j], submatrix[i,j]);
			}
		}
	}
	private void FlipMatrixVertical()
	{
		for(int i = 0; i < Size / 2; i++)
		{
			for(int j = 0; j < Size; j++)
			{
				(submatrix[i,j], submatrix[Size - 1 - i, j]) 
				= (submatrix[Size - 1 - i, j], submatrix[i,j]);
			}
		}
	}

	/// <summary>
	/// Rotates the current piece's submatrix in the specified direction
	/// </summary>
	/// <returns>The previous state of the submatrix</returns>
	/// <example>If this.submatrix is [[1,2],[3,4]] and RotateMatrix(RotationDirection.Right) is called,
	/// then the method will return [[1,2],[3,4]] and the new state of this.submatrix will be [[3,1],[4,2]]</example>
	public int[,] RotateMatrix(RotationDirection direction)
	{
		int[,] tempSubmatrix = this.submatrix.Clone() as int[,];
		if(direction != RotationDirection.FullRotation)
		{
			TransposeMatrix();
		}
		switch (direction)
		{
			case RotationDirection.Left:
				FlipMatrixVertical();
				rotationState = Add(rotationState, 3);
				break;
			case RotationDirection.Right:
				FlipMatrixHorizontal();
				rotationState = Add(rotationState, 1);
				break;
			case RotationDirection.FullRotation:
				FlipMatrixHorizontal();
				FlipMatrixVertical();
				rotationState = Add(rotationState, 2);
				break;
			default: 
				throw new ArgumentException("Parameter direction is not a valid enum value");
		}
		return tempSubmatrix;
	}

	public bool RotatePiece(RotationDirection direction)
	{
		if(IsOnFloor())
		{
			ResetLockDelay();
		}
		isLastMoveRotation = true;
		RotationState prevRotationState = this.rotationState;
		int[,] prevSubmatrix = RotateMatrix(direction);
		if(IsCollidingWithBoard())
		{
			var kickTable = Pieces.SRSKickTables.GetValueOrDefault(this.id, new(new Dictionary<(RotationState, RotationDirection), CellPosition[]>()));
			var rotationKey = (prevRotationState, direction);
			CellPosition[] possibleOffsets = kickTable.GetValueOrDefault(rotationKey, null);
			possibleOffsets ??= [];
			bool kickSuccessful = false;
			foreach(CellPosition offset in possibleOffsets)
			{
				relativePosition += offset;
				if(IsCollidingWithBoard())
				{
					relativePosition -= offset;
				} else {
					kickSuccessful = true;
					break;
				}
			}
			if(!kickSuccessful)
			{
				this.submatrix = prevSubmatrix;
				this.rotationState = prevRotationState;
				return false; // No kick was able to be performed, piece didn't rotate
			} else {
				CheckSpinAndSendSignal();
				return true; // One of the kicks was correctly applied, piece rotated
			}
			
		} else {
			CheckSpinAndSendSignal();
			return true; // Piece rotation valid in its initial state, no kicks necessary
		}
	}

	private void CheckSpinAndSendSignal()
	{
		SpinType spin = CheckSpin();
		if(spin != SpinType.NoSpin)
		{
			SpinState = true;
			PieceSpinned?.Invoke(spin);
		} else {
			SpinState = false;
		}
	}

	public bool IsImmobile()
	{
		Move(CellPosition.Down);
		if(!IsCollidingWithBoard())
		{
			Move(CellPosition.Up);
			return false;
		}
		Move(CellPosition.Up);
		
		Move(CellPosition.Left);
		if(!IsCollidingWithBoard())
		{
			Move(CellPosition.Right);
			return false;
		}
		Move(CellPosition.Right);
		
		Move(CellPosition.Up);
		if(!IsCollidingWithBoard())
		{
			Move(CellPosition.Down);
			return false;
		}
		Move(CellPosition.Down);
		
		Move(CellPosition.Right);
		if(!IsCollidingWithBoard())
		{
			Move(CellPosition.Left);
			return false;
		}
		Move(CellPosition.Left);
		return true;
	}

	private (int total, int covered) GetSpinPoints(int spinPointNumber)
	{
		(int total, int covered) output = (0,0);

		for(int i = 0; i < this.Size; i++)
		{
			for(int j = 0; j < this.Size; j++)
			{
				if(Submatrix[i,j] == spinPointNumber)
				{
					output.total++;
					int boardTile = GetRelativeTileValueInBoard(i,j);
					{if(boardTile != 0)
					
						output.covered++;
					}
				}
			}
		}

		return output;
	}

	private SpinType CheckSpinDefault()
	{
		if(!isLastMoveRotation)
		{
			return SpinType.NoSpin;
		} 
		else if(!IsImmobile())
		{
			return SpinType.NoSpin;
		}

		(int totalSpinPoints, int coveredSpinPoints) = GetSpinPoints(-1);

		if(coveredSpinPoints == totalSpinPoints)
		{
			return SpinType.TrueSpin;
		} 
		else if(coveredSpinPoints < totalSpinPoints)
		{
			return SpinType.SpinMini;
		} else {
			throw new Exception($"coveredSpinPoints ({coveredSpinPoints}) > totalSpinPoints({totalSpinPoints}). How the fuck?");
		}
	}

	private SpinType CheckSpinPieceT()
	{
		const int TARGET_COVERED_SPIN_POINTS = 3;
		if(!isLastMoveRotation)
		{
			return SpinType.NoSpin;
		}
		
		(int _, int coveredPrimarySpinPoints) = GetSpinPoints(-1);
		(int _, int coveredSecondarySpinPoints) = GetSpinPoints(-2);
		if(coveredPrimarySpinPoints + coveredSecondarySpinPoints >= TARGET_COVERED_SPIN_POINTS)
		{
			if(coveredPrimarySpinPoints >= coveredSecondarySpinPoints)
			{
				return SpinType.TrueSpin;
			} else {
				return SpinType.SpinMini;
			}
		} else {
			return SpinType.NoSpin;
		}
	}

	public SpinType CheckSpin()
	{
		switch(this.ID)
		{
			case "T": return CheckSpinPieceT();
			default: return CheckSpinDefault();
		}
	}

	public void _Process(double delta)
	{
		if(IsOnFloor())
		{
			lockDelayTimerSeconds += delta;
			if(lockDelayTimerSeconds > lockDelay)
			{
				board.PlaceCurrentPiece();
			}
		} else {
			if(lockDelayTimerSeconds > 0.0)
			{ // piece has landed before and has risen somehow, reset lock timer
				ResetLockDelay();
			}
		}
	}


}
