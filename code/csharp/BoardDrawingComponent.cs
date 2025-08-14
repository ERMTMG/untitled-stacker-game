using Godot;
using System;
using System.Collections.Generic;
namespace USG;

using PieceSet = System.Collections.ObjectModel.ReadOnlyDictionary<string, Piece>;

public partial class BoardDrawingComponent : Node2D
{
	private static readonly Dictionary<int, Godot.Color> TileColors = new()
	{
		[1] = Colors.Crimson,
		[2] = Colors.Coral,
		[3] = Colors.Gold,
		[4] = Colors.GreenYellow,
		[5] = Colors.DarkTurquoise,
		[6] = Colors.SlateBlue,
		[7] = Colors.DarkOrchid,
		[0] = new Color(0f, 0f, 0f, 0f),
		// [-1] = new Color(.25f, .25f, .25f, .25f),
		// [-2] = new Color(.50f, .25f, .25f, .25f)

	};

	[Export] private GameBoard board;
	[Export] public int TileSize { get; set; }
	[Export] public Godot.Texture2D BlockTexture { get; set; }
	[Export] public Godot.Texture2D ShadowPieceTexture { get; set; } 
		= GD.Load<Texture2D>("res://assets/graphics/board/ghostblock.png");
	[Export] public bool DrawShadowPiece { get; set; } = true;
	[Export] public bool DrawGrid { get; set; } = true;

	public int BoardWidth { get => board.BoardWidth; }
	public int BoardHeight { get => board.BoardHiddenPortionHeight; }

	public int BoardEffectiveWidth => board.BoardWidth * TileSize;
	public int BoardEffectiveHeight => (board.BoardTrueHeight - board.BoardHiddenPortionHeight) * TileSize;

	public BoardDrawingComponent()
	{
		
	}

	public override void _Ready()
	{
		base._Ready();
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
		QueueRedraw();
	}

	public override void _Draw()
	{
		int width = board.BoardWidth;
		int heightBegin = board.BoardHiddenPortionHeight;
		int heightEnd = board.BoardTrueHeight;
		DrawRect(
			new Rect2(Vector2.Zero, width*TileSize, (heightEnd - heightBegin)*TileSize), 
			new Color(0.23f, 0.23f, 0.43f, 0.35f)
		);
		if(DrawGrid)
		{
			DrawBoardGrid(width, heightBegin, heightEnd);
		}
		DrawStaticBoard(width, heightBegin, heightEnd);
		if(DrawShadowPiece)
		{
			DrawBoardShadowPiece(width, heightBegin, heightEnd);
		}
		DrawBoardCurrentPiece(width, heightBegin, heightEnd);
		
		base._Draw();
	}

	private void DrawStaticBoard(int width, int heightBegin, int heightEnd)
	{
		for(int i = heightBegin; i < heightEnd; i++)
		{
			for(int j = 0; j < width; j++)
			{
				Rect2 tileRect = new(
					j*TileSize, 
					(i-heightBegin)*TileSize, 
					TileSize*Vector2.One
				);
				int tileValue = board.GetTileAt(i,j);
				if(tileValue != 0 && TileColors.TryGetValue(tileValue, out Color tileColor))
				{
					DrawTextureRect(BlockTexture, tileRect, tile: false, tileColor);
				}
			}
		} 
	}

	private void DrawBoardCurrentPiece(int width, int heightBegin, int heightEnd)
	{
		const float LOCK_DELAY_DARKENING_FACTOR = 0.3333f;

		Piece piece = board.CurrentPiece;
		if(piece is null)
		{
			return;
		}
		float lockDelayProgressPercentage = (float)(piece.LockDelayTimerSeconds / piece.LockDelay);
		if(double.IsNaN(lockDelayProgressPercentage))
		{
			lockDelayProgressPercentage = 0f;
		}
		for(int i = 0; i < piece.Submatrix.GetLength(0); i++)
		{
			for(int j = 0; j < piece.Submatrix.GetLength(1); j++)
			{
				Rect2 tileRect = new(
					(j + piece.RelativeCol)*TileSize, 
					(i + piece.RelativeRow - heightBegin)*TileSize, 
					TileSize*Vector2.One
				);
				int tileValue = piece.Submatrix[i,j];
				if(tileValue > 0 && TileColors.TryGetValue(tileValue, out Color tileColor))
				{
					tileColor = tileColor.Darkened(lockDelayProgressPercentage*LOCK_DELAY_DARKENING_FACTOR);
					DrawTextureRect(BlockTexture, tileRect, tile: false, tileColor);
				}
			}
		}
	}

	private void DrawBoardGrid(int width, int heightBegin, int heightEnd)
	{
		Color GRID_LINE_COLOR = new(1f, 1f, 1f, 0.35f);

		for(int j = 0; j <= width; j++)
		{
			Vector2 gridLineBegin = new(j*TileSize, 0);
			Vector2 gridLineEnd = new(j*TileSize, (heightEnd - heightBegin) * TileSize);
			DrawLine(gridLineBegin, gridLineEnd, GRID_LINE_COLOR);
		}

		for(int i = heightBegin; i <= heightEnd; i++)
		{
			Vector2 gridLineBegin = new(0, (i - heightBegin)*TileSize);
			Vector2 gridLineEnd = new(width*TileSize, (i - heightBegin) * TileSize);
			DrawLine(gridLineBegin, gridLineEnd, GRID_LINE_COLOR);
		}
	}

	private void DrawBoardShadowPiece(int width, int heightBegin, int heightEnd)
	{
		Piece piece = board.CurrentPiece;
		if(piece is null)
		{
			return;
		}
		int pieceHeight = 0;
		while(piece.TryMove(CellPosition.Down, resetLockDelay: false))
		{
			pieceHeight++;
		}
		for(int i = 0; i < piece.Submatrix.GetLength(0); i++)
		{
			for(int j = 0; j < piece.Submatrix.GetLength(1); j++)
			{
				Rect2 tileRect = new(
					(j + piece.RelativeCol)*TileSize, 
					(i + piece.RelativeRow - heightBegin)*TileSize, 
					TileSize*Vector2.One
				);
				int tileValue = piece.Submatrix[i,j];
				if(tileValue > 0)
				{
					DrawTextureRect(ShadowPieceTexture, tileRect, tile: false, Colors.White);
				}
			}
		}
		piece.TryMove(-pieceHeight, 0, resetLockDelay: false);
	}


}
