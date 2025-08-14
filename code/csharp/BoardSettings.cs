using Godot;
using System;

namespace USG;

[Tool]
[GlobalClass]
public partial class BoardSettings : Resource
{
	private double dasSeconds;
	private double arrSeconds;
	private int softDropFactor;
	public const int SDF_INFINITE = -1;
	private int boardWidth;
	private int boardHeight;
	private int overBoardHeight;
	private double beginningGravityLevel;
	private double lockDelaySeconds;
	private GameBoard.TopOutType topOutTypesConsidered;

	[Export] public double DasSeconds { 
		get => dasSeconds; 
		set {
			if(value >= 0.0)
			{
				dasSeconds = value;
			} else {
				throw new ArgumentException($"Tried to set DAS to {value}. DAS value can't be negative");
			}
		} 
	}

	[Export] public double ArrSeconds 
	{ 
		get => arrSeconds;
		set {
			if(value > 0.0)
			{
				arrSeconds = value;
			} else {
				throw new ArgumentException($"Tried to set ARR to {value}. ARR value can't be negative");
			}
		}
	}

	[Export] public int SoftDropFactor { 
		get => softDropFactor; 
		set {
			if(value >= 1 || value == SDF_INFINITE)
			{
				softDropFactor = value;
			} else {
				throw new ArgumentException($"Tried to set SDF to {value}. SDF value must be positive or equal to SDF_INFINITE (-1)");
			}
		}
	}

	[Export] public PieceGenerator Generator { get; set; }

	[Export] public int BoardWidth 
	{ 
		get => boardWidth; 
		set
		{
			if(value >= 1)
			{
				boardWidth = value;
			} else {
				throw new ArgumentException($"Tried to set BoardWidth to {value}. Board width must be positive.");
			}
		} 
	}

	[Export] public int BoardHeight
	{ 
		get => boardHeight; 
		set
		{
			if(value >= 1)
			{
				boardHeight = value;
			} else {
				throw new ArgumentException($"Tried to set BoardHeight to {value}. Board height must be positive.");
			}
		} 
	}

	[Export] public int OverBoardHeight
	{ 
		get => overBoardHeight; 
		set
		{
			if(value >= 0)
			{
				overBoardHeight = value;
			} else {
				throw new ArgumentException($"Tried to set OverBoardHeight to {value}. Over-the-top board height must not be negative.");
			}
		} 
	}

	[Export] public double BeginningGravityLevel
	{
		get => beginningGravityLevel;
		set
		{
			if(0.0 <= value && value <= GameBoard.MAX_GRAVITY)
			{
				beginningGravityLevel = value;
			} else {
				throw new ArgumentException($"Tried to set BeginningGravityLevel to {value}. Beginning gravity level must be between 0G and {GameBoard.MAX_GRAVITY}G.");
			}
		}
	}

	public long? RNGSeed { get; set; }

	[Export] public double LockDelaySeconds
	{ 
		get => lockDelaySeconds; 
		set {
			if(value >= 0.0)
			{
				lockDelaySeconds = value;
			} else {
				throw new ArgumentException($"Tried to set LockDelaySeconds to {value}. Lock delay can't be negative.");
			}
		}
	}

	[Export(PropertyHint.Flags)] public GameBoard.TopOutType TopOutTypesConsidered 
	{ 
		get => topOutTypesConsidered; 
		set
		{
			if(value <= GameBoard.TopOutType.All)
			{
				topOutTypesConsidered = value;
			} else {
				throw new ArgumentException($"Tried to set TopOutTypesConsidered to {value:B}. TopOutTypesConsidered is a bitwise flag varaible and its value can't exceed {GameBoard.TopOutType.All:B4}");
			}
		} 
	}

	public BoardSettings()
	{
		BoardWidth = 10;
		BoardHeight = 20;
		OverBoardHeight = 20;
		DasSeconds = 8.0/60.0; // = 8 frames
		ArrSeconds = 0.00000001; // = ∞ ARR
		SoftDropFactor = 40;
		BeginningGravityLevel = 1f/60f;
		Generator = new BagPieceGenerator(Pieces.TetrominosBag.Clone() as string[]);
		RNGSeed = null;
		LockDelaySeconds = 0.50;
		TopOutTypesConsidered = GameBoard.TopOutType.Guideline;
	}
}
