using System;
using Godot;

namespace USG;

using PieceID = string;


[Tool] [GlobalClass]
public partial class BagPlusXPieceGenerator : RandomPieceGenerator
{
	private int xValue;
	private int remainingBags; 
	public const int INFINITE_BAG_PLUS_X = -1;

	[Export] public int X
	{
		get => xValue;
		set 
		{
			if(value >= 0)
			{
				xValue = value;
			} else {
				throw new ArgumentException($"Tried to set BagPlusXPieceGenerator's X value to {value}. Value must be nonnegative.");
			}
		}
	}

	[Export] public int RemainingBags
	{
		get => remainingBags;
		set
		{
			if(value > 0 || value == INFINITE_BAG_PLUS_X)
			{
				remainingBags = value;
			} else {
				throw new ArgumentException($"Tried to set BagPlusXPieceGenerator's RemainingBags value to {value}. Value must be positive or equal to INFINITE_BAG_PLUS_X ({INFINITE_BAG_PLUS_X})");
			}
		}
	}

	[Export] private PieceID[] extraAvailablePieces;

	public BagPlusXPieceGenerator(PieceID[] availablePieces, int X, PieceID[] extraAvailablePieces, int remainingBags) : base(availablePieces)
	{
		this.extraAvailablePieces = extraAvailablePieces.Clone() as PieceID[];
		this.X = X;
		this.RemainingBags = remainingBags;
	}

	public BagPlusXPieceGenerator(PieceID[] availablePieces, int X, PieceID[] extraAvailablePieces) : this(availablePieces, X, extraAvailablePieces, INFINITE_BAG_PLUS_X)
	{}

	public BagPlusXPieceGenerator(PieceID[] availablePieces, int X) : this(availablePieces, X, availablePieces, INFINITE_BAG_PLUS_X)
	{}

	public BagPlusXPieceGenerator(PieceID[] availablePieces) : this(availablePieces, 0, availablePieces, INFINITE_BAG_PLUS_X)
	{}

	public BagPlusXPieceGenerator() : this([])
	{}

	public override void GeneratePieces()
	{
        GD.Print("BagPlusXPieceGenerator.GeneratePieces called!");
		if(remainingBags > 0 || remainingBags == INFINITE_BAG_PLUS_X)
		{
			PieceID[] bagPieces = availablePieces.Clone() as PieceID[];
			PieceID[] extraPieces = new PieceID[X];
			for(int i = 0; i < X; i++)
			{
				int idx = rng.Next(extraAvailablePieces.Length);
				PieceID extraPiece = extraAvailablePieces[idx];
				extraPieces[i] = extraPiece;
			}
			
			PieceID[] newPieces = new PieceID[bagPieces.Length + X];
			for(int i = 0; i < newPieces.Length; i++)
			{
				if(i < bagPieces.Length)
				{
					newPieces[i] = bagPieces[i];
				} else {
					newPieces[i] = extraPieces[i - bagPieces.Length];
				}
			}
			rng.Shuffle(newPieces);
			AddToBuffer(newPieces);
			if(remainingBags > 0) remainingBags--;
		} else {
			PieceID[] newPieces = availablePieces.Clone() as PieceID[];
			rng.Shuffle(newPieces);
			AddToBuffer(newPieces);
		}
		
		
	}
}
