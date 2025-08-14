using Godot;
using System;
namespace USG;

public partial class GameBoard : Node
{
	public string HeldPiece => heldPieceName;

	public int BoardWidth => BOARD_WIDTH; 

	public int BoardHiddenPortionHeight => BOARD_HIDDEN_PORTION_HEIGHT; 

	public int BoardTrueHeight => BOARD_HEIGHT; 

	public int BoardVisibleHeight => BOARD_HEIGHT - BOARD_HIDDEN_PORTION_HEIGHT;

	public double GravityLevel 
	{
		get => gravityLevel;
		set {
			if(0.0 <= value && value <= MAX_GRAVITY)
			{
				gravityLevel = value;
				gravityMsPerTileCounter = 0.0;
			} else {
				throw new ArgumentException($"Tried to set gravity level to {value}. Gravity must be between 0.0 and 20.0 inclusive (in tiles dropped per frame)");
			}
		}
	}
	public double EffectiveGravity {
		get {
			if(isSoftDropping)
			{
				if(settings.SoftDropFactor == BoardSettings.SDF_INFINITE)
				{
					return MAX_GRAVITY;
				} else {
					double adjustedGravity = gravityLevel;
					if(adjustedGravity == 0.0) adjustedGravity = 0.1;
					return Math.Min(
						settings.SoftDropFactor * adjustedGravity,
						MAX_GRAVITY
					);
				}
			} else {
				return gravityLevel;
			}
		}
	}
	public Piece CurrentPiece => currentPiece; 

	public long Score => info.Score;
	public int PiecesPlaced => info.PiecesPlaced;
	public int LinesCleared => info.LinesCleared;
	public decimal TimePassedSeconds => info.TimePassedSeconds;
	public double PiecesPerSecond => info.PiecesPerSecond;

	public BoardSettings Settings 
	{
		get => settings;
		set 
		{
			settings = value;
			InitBoardSettings();
		}
	}


	public bool GameActive => isGameActive;
}
