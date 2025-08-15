using Godot;
using System;
namespace USG;

public partial class GameBoard : Node
{
	const int DEFAULT_BOARD_WIDTH = 10;
	const int DEFAULT_BOARD_VISUAL_HEIGHT = 20;
	const int DEFAULT_BOARD_HEIGHT = 40;
	public const int INVALID_TILE_VALUE = 0x7FFFFFFF;
	public const double MAX_GRAVITY = 20.0;

	const float FRAME_TIME = 1f/60f;

	private int BOARD_WIDTH;
	private int BOARD_HIDDEN_PORTION_HEIGHT;
	private int BOARD_HEIGHT;

	private long RNG_SEED;
	private int[,] matrix;
	private double gravityLevel;
	private double gravityMsPerTileCounter; // exclusively used by HandleGravity method
	private string heldPieceName;
	private bool hasHeldPiece;
	private bool isGameActive;
	private int currentComboValue;

	private Piece currentPiece;
	private SpinType latestSpin;
	private PieceQueue pieceQueue;

	[Export] private InputManager input;
	[Export] private BoardSettings settings;
			 private GameInfo info;

	private bool isSoftDropping;
	private TopOutType topOutTypesConsidered;

	public record PiecePlacementInformation
	(
		CellPosition Position,
		RotationState RotationState,
		SpinType Spin = SpinType.NoSpin
	);

	public enum TopOutType
	{
		BlockOut         = 1 << 0,
		GarbageOut       = 1 << 1,
		LockOut          = 1 << 2,
		StrictGarbageOut = 1 << 3,

		Guideline = BlockOut | GarbageOut | LockOut,

		All = BlockOut | GarbageOut | LockOut | StrictGarbageOut
	};

	GameBoard(BoardSettings settings)
	{
		this.settings = settings;
		this.info = new GameInfo();
		BOARD_WIDTH = settings.BoardWidth;
		BOARD_HEIGHT = settings.BoardHeight + settings.OverBoardHeight;
		BOARD_HIDDEN_PORTION_HEIGHT = settings.OverBoardHeight;
		matrix = new int[BoardTrueHeight, BoardWidth];
		for(int i = 0; i < BoardTrueHeight; i++)
		{
			for(int j = 0; j < BoardWidth; j++)
			{
				matrix[i,j] = 0;
			}
		}
		GravityLevel = settings.BeginningGravityLevel;

		currentPiece = null;
		heldPieceName = null;
		hasHeldPiece = false;
		gravityMsPerTileCounter = 0.0;
		if(settings.RNGSeed is null)
		{
			RNG_SEED = System.DateTime.Now.ToBinary();
		} else {
			RNG_SEED = settings.RNGSeed.Value;
		}
		topOutTypesConsidered = settings.TopOutTypesConsidered;
	}


	GameBoard() : this(new BoardSettings())
	{}

	void InitBoardSettings()
	{
		BOARD_WIDTH = settings.BoardWidth;
		BOARD_HEIGHT = settings.BoardHeight + settings.OverBoardHeight;
		BOARD_HIDDEN_PORTION_HEIGHT = settings.OverBoardHeight;
		matrix = new int[BoardTrueHeight, BoardWidth];
		for(int i = 0; i < BoardTrueHeight; i++)
		{
			for(int j = 0; j < BoardWidth; j++)
			{
				matrix[i,j] = 0;
			}
		}
		GravityLevel = settings.BeginningGravityLevel;
		if(settings.RNGSeed is null)
		{
			RNG_SEED = System.DateTime.Now.ToBinary();
		} else {
			RNG_SEED = settings.RNGSeed.Value;
		}
		topOutTypesConsidered = settings.TopOutTypesConsidered;
		if(settings.Generator is RandomPieceGenerator)
		{
			(settings.Generator as RandomPieceGenerator).SetSeed(RNG_SEED);
		}
		pieceQueue = new PieceQueue(settings.Generator);
	}

	public string GetPieceInQueuePos(int nextQueueIdx)
	{
		try
		{
			return pieceQueue.GetNext(nextQueueIdx);
		}
		catch (IndexOutOfRangeException) { throw; }
	}

	public override void _Ready()
	{
		base._Ready();
		//settings ??= new BoardSettings();
		InitBoardSettings();

		if(settings.Generator is BagPlusXPieceGenerator) 
		{
			GD.Print("something's wrong, i can feel it");
		}
		
		hasHeldPiece = false;
		isGameActive = false;
		currentComboValue = -1;

		LineCleared += this.OnLineCleared;
		PiecePlaced += this.OnPiecePlaced;
		ToppedOut   += this.OnToppedOut;

		input.HardDropPressed += OnHardDropPressed;
		input.HoldPiecePressed += OnHoldPiecePressed;
		input.RotateLeftPressed += OnRotatePieceLeftPressed;
		input.RotateRightPressed += OnRotatePieceRightPressed;
		input.RotateFullPressed += OnRotatePieceFullPressed;
		input.SoftDropPressed += OnSoftDropPressed;
		input.SoftDropReleased += OnSoftDropReleased;
	}
	
	public int GetTileAt(int i, int j)
	{
		if(0 <= i && i < BOARD_HEIGHT 
		&& 0 <= j && j < BOARD_WIDTH)
		{
			return matrix[i,j];
		} else {
			throw new IndexOutOfRangeException($"Tile index out of range: ({i}, {j}) at GameBoard.GetTileAt");
		}
	}

	public bool IsTileOccupied(int i, int j)
	{
		if(0 <= i && i < BOARD_HEIGHT 
		&& 0 <= j && j < BOARD_WIDTH)
		{
			return GetTileAt(i,j) != 0;
		} else {
			throw new IndexOutOfRangeException($"Tile index out of range: ({i},{j}) at GameBoard.IsTileOccupied");
		}
	}

	public void PlaceCurrentPiece()
	{
		PrintCurrentPieceInMatrix();
		CellPosition piecePosition = new(currentPiece.RelativePosition);
		int clearedLines = ClearFullRows(
			new PiecePlacementInformation(piecePosition, currentPiece.Rotation, latestSpin)
		);
		info.PiecesPlaced++;
		latestSpin = currentPiece.SpinState == true ? latestSpin : SpinType.NoSpin;
		PiecePlaced?.Invoke(
			currentPiece.ID, 
			piecePosition,
			currentPiece.Rotation,
			latestSpin,
			clearedLines > 0
		);
	}

	private void PrintCurrentPieceInMatrix()
	{
		bool isEntirelyAboveVisibleMatrix = true;
		int pieceSize = currentPiece.Size;
		for(int i = 0; i < pieceSize; i++)
		{
			for(int j = 0; j < pieceSize; j++)
			{
				int matrixRow = currentPiece.RelativeRow + i;
				int matrixCol = currentPiece.RelativeCol + j;
				if(0 <= matrixRow && matrixRow < BoardTrueHeight
				&& 0 <= matrixCol && matrixCol < BoardWidth
				&& currentPiece.Submatrix[i,j] > 0)
				{
					this.matrix[matrixRow, matrixCol] = currentPiece.Submatrix[i,j];
				}
				if(matrixRow >= BoardHiddenPortionHeight && currentPiece.Submatrix[i,j] > 0)
				{
					isEntirelyAboveVisibleMatrix = false;
				}
			}
		}
		if(isEntirelyAboveVisibleMatrix)
		{
			TopOut(TopOutType.LockOut);
		}
	}

	private void SpawnPiece(string pieceID)
	{
		if(currentPiece is not null)
		{
			currentPiece.PieceSpinned -= OnCurrentPieceSpinned; // This is the previous piece object, so we disconnect the signal
		}
		currentPiece = new Piece(pieceID, this);
		currentPiece.SetDefaultSpawningPosition();
		currentPiece.LockDelay = settings.LockDelaySeconds;
		currentPiece.PieceSpinned += OnCurrentPieceSpinned; // and then we connect it back to the _new_ piece object
		if(currentPiece.IsCollidingWithBoard())
		{
			TopOut(TopOutType.BlockOut);
		}
	}

	private void SpawnNextPiece()
	{
		string nextPieceID = pieceQueue.GetNext();
		SpawnPiece(nextPieceID);
		pieceQueue.Pop();
		hasHeldPiece = false;
		NextPieceSpawned?.Invoke();
	}

	private void SwapHeldPiece()
	{
		string currentPieceName = currentPiece.ID;
		if(string.IsNullOrEmpty(heldPieceName))
		{
			heldPieceName = currentPieceName;
			SpawnNextPiece();
		} else {
			string aux = heldPieceName;
			heldPieceName = currentPieceName;
			SpawnPiece(aux);
		}
		hasHeldPiece = true;
		PieceHeld?.Invoke();
	}

	private void MovePiece(InputManager.HoldingDirection direction)
	{
		switch (direction)
		{
			case InputManager.HoldingDirection.Left:
				currentPiece.TryMove(0, -1);
				break;
			case InputManager.HoldingDirection.Right:
				currentPiece.TryMove(0, 1);
				break;
			default: break;
		}
	}

	private void HandleLeftRightInput()
	{
		var holdInfo = input.HoldingInfo;
		if(holdInfo.direction == InputManager.HoldingDirection.None)
		{
			return;
		}
		if(double.IsNaN(holdInfo.oldTime)) // frame before wasn't pressing any direction
		{
			MovePiece(holdInfo.direction);
		} else {
			double DASTime = settings.DasSeconds;
			double ARRTime = settings.ArrSeconds;
			if(holdInfo.time > DASTime)
			{
				double totalARRTime = holdInfo.time - DASTime;
				double totalARRTimeOld = holdInfo.oldTime - DASTime;
				if((int)(totalARRTime/ARRTime) > (int)(totalARRTimeOld/ARRTime))
				{
					MovePiece(holdInfo.direction);
				}
			}
		}
	}

	private bool DropCurrentPieceSingleStep()
	{
		if(currentPiece.TryMove(1,0) == false)
		{
			// TODO: set flag indicating piece is on floor
			return true;
		}
		return false;
	}

	private void HandleGravity(double delta)
	{
		const double G_UNITS_TO_TILES_PER_MS = 0.06;

		double deltaInMs = delta*1000.0;
		double gravityInGUnits = EffectiveGravity;
		double gravityInTilesPerMs = gravityInGUnits * G_UNITS_TO_TILES_PER_MS;
		double gravityInMsPerTile = 1.0/gravityInTilesPerMs;
		gravityMsPerTileCounter += deltaInMs;
		// This loops because pieces may fall more than one tile in a single step
		while(gravityMsPerTileCounter > gravityInMsPerTile) 
		{
			bool onFloor = DropCurrentPieceSingleStep();
			gravityMsPerTileCounter -= gravityInMsPerTile;
			if(onFloor)
			{
				break; // no need to keep falling
			}
		}

	}

	private void RemoveRow(int rowIdx)
	{
		if(rowIdx < 0 || rowIdx >= BoardTrueHeight)
		{
			throw new IndexOutOfRangeException($"Row index out of range: {rowIdx} at GameBoard.RemoveRow");
		} else {
			for(int i = rowIdx; i > 0; i--)
			{
				// copy row i-1 to row i
				for(int j = 0; j < BoardWidth; j++)
				{
					matrix[i,j] = matrix[i-1,j];
				}
			}		
			// copy empty row to row 0 (see, PPT devs? it wasn't that hard)
			for(int j = 0; j < BoardWidth; j++)
			{
				matrix[0,j] = 0;
			}
		}
	}

	private bool IsRowFull(int rowIdx)
	{
		if(rowIdx < 0 || rowIdx >= BoardTrueHeight)
		{
			throw new IndexOutOfRangeException($"Row index out of range: {rowIdx} at GameBoard.IsRowFull");
		} else {
			for(int j = 0; j < BoardWidth; j++)
			{
				if(matrix[rowIdx,j] == 0)
				{
					return false;
				}
			}
			return true;
		}
	}

	private int ClearFullRows(PiecePlacementInformation pieceInfo = null)
	{
		int totalRowsCleared = 0;
		for(int i = 0; i < BoardTrueHeight; i++)
		{
			if(IsRowFull(i))
			{
				RemoveRow(i);
				totalRowsCleared++;
			}
		}
		if(totalRowsCleared > 0)
		{
			LineCleared?.Invoke(
				totalRowsCleared,
				currentPiece.ID,
				pieceInfo
			);
			info.LinesCleared += totalRowsCleared;
		}
		return totalRowsCleared;
	}

	public void UnPause()
	{
		isGameActive = true;
	}

	public void Pause()
	{
		isGameActive = false;
	}

	public override void _Process(double delta)
	{
		if(isGameActive)
		{
			if(currentPiece is null)
			{
				SpawnNextPiece();
			}
			ClearFullRows();
			base._Process(delta);
			currentPiece._Process(delta);
			HandleLeftRightInput();
			HandleGravity(delta);
			info.TimePassedSeconds += (decimal) Math.Round(delta, 3);
		}
	}

	// TODO: delete these later

	private void OnLineCleared(int linesCleared, string pieceID, PiecePlacementInformation info)
	{
		GD.Print($"{linesCleared} lines cleared with piece {pieceID}!");
	}

	private void OnPiecePlaced(string pieceID, CellPosition piecePosition, RotationState rotationState, SpinType spin, bool clearedLines)
	{
		GD.Print($"Piece {pieceID} placed on row {piecePosition.Row}, column {piecePosition.Col}!");
		if(spin != SpinType.NoSpin)
		{
			GD.Print($"({(spin == SpinType.SpinMini ? "Mini " : "")}Spin registered!)");
		}
		ClearFullRows();
		SpawnNextPiece();
	}

	private void OnCurrentPieceSpinned(SpinType spinType)
	{
		PieceSpinned?.Invoke(
			currentPiece.ID,
			new CellPosition(CurrentPiece.RelativePosition),
			CurrentPiece.Rotation,
			spinType
		);
		latestSpin = spinType;
		GD.Print($"Piece {currentPiece.ID} performed a {spinType}!");
	}

	public void TopOut(TopOutType type)
	{
		if(type > TopOutType.All || int.PopCount((int)type) != 1)
		{
			throw new ArgumentException($"Invalid TopOutType value: {type:B} at GameBoard.TopOut");			
		}
		if((type | this.topOutTypesConsidered) != 0)
		{
			ToppedOut?.Invoke(type);
		}
	}
	private void OnToppedOut(TopOutType type)
	{
		string deathMessage = type switch
		{
			TopOutType.BlockOut => "Block out",
			TopOutType.GarbageOut => "Garbage out",
			TopOutType.LockOut => "Lock out",
			TopOutType.StrictGarbageOut => "Garbage out (strict)",
			_ => throw new ArgumentException($"Invalid top out type: {type:B} in GameBoard.GameOver")
		};
		GD.Print($"Game over! Reason: {deathMessage}");
		QueueFree();
	}

}
