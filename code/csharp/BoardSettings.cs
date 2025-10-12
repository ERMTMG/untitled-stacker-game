using Godot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace USG;

using KickTable = ReadOnlyDictionary<(RotationState, RotationDirection), CellPosition[]>;

public enum KickTableValue
{
	SRSKickTable,
	TechminoKickTable,
	ASCRotationSystem,
	NoKicks,
}

[Tool]
[GlobalClass]
public partial class BoardSettings : Resource
{
	private double dasSeconds;
	private double arrSeconds;
	private int softDropFactor;
	public const int SDF_INFINITE = -1;
	private double lineClearAreSeconds;
	private int boardWidth;
	private int boardHeight;
	private int overBoardHeight;
	private double beginningGravityLevel;
	private double lockDelaySeconds;
	private GameBoard.TopOutType topOutTypesConsidered;
	private int linesRequiredToLevelUp;
	private KickTableValue setKickTable;

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
			if(value >= 0.0)
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
			if(value is >= 1 or SDF_INFINITE)
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

	[Export] public double LineClearAreSeconds
	{
		get => lineClearAreSeconds;
		set
		{
			if(value < 0)
			{
				throw new ArgumentException($"Tried to set LineClearAreSeconds to {value}. ARE must be positive or equal to zero.");
			}
			lineClearAreSeconds = value;
		}
	}
	
	[Export] public bool LevellingEnabled { get; set; }

	[Export]
	public int LinesRequiredToLevelUp
	{
		get => linesRequiredToLevelUp;
		set
		{
			if(value > 0 || (value == 0 && !LevellingEnabled))
			{
				linesRequiredToLevelUp = value;
			} else
			{
				throw new ArgumentException($"Tried to set LinesRequiredToLevelUp to {value}. Number of lines required to level up must be positive or 0 only if levelling is disabled.");
			}
		}
	}
	
	[Export] public bool AbsoluteMadness { get; set; }

	[Export]
	public KickTableValue SetKickTable
	{
		get => setKickTable;
		set 
		{
			setKickTable = value;
			switch(value)
			{
				case KickTableValue.SRSKickTable:
					KickTables = Pieces.SRSKickTables; 
					break;
				case KickTableValue.TechminoKickTable:
					break;
				case KickTableValue.ASCRotationSystem:
					break;
				case KickTableValue.NoKicks:
					KickTables = new (new Dictionary<string, KickTable>());
					break;
				default:
					KickTables = null;
					break;
			}
		}
	}

	public ReadOnlyDictionary<string, KickTable> KickTables { get; private set; }

	public BoardSettings()
	{
		BoardWidth = 10;
		BoardHeight = 20;
		OverBoardHeight = 20;
		DasSeconds = 8.0/60.0; // = 8 frames
		ArrSeconds = 0.00000001; // = ∞ ARR
		SoftDropFactor = 40;
		LineClearAreSeconds = 0.0;
		BeginningGravityLevel = 1f/60f;
		Generator = new BagPieceGenerator(Pieces.TetrominosBag.Clone() as string[]);
		RNGSeed = null;
		LockDelaySeconds = 0.50;
		TopOutTypesConsidered = GameBoard.TopOutType.Guideline;
		LevellingEnabled = false;
		linesRequiredToLevelUp = 10;
		AbsoluteMadness = false;
		SetKickTable = KickTableValue.SRSKickTable;
	}
}
