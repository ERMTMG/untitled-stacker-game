using System;
using Godot;

namespace USG;

using PieceID = string;

[Tool] [GlobalClass]
public abstract partial class RandomPieceGenerator : PieceGenerator
{
	protected Random rng;

	public RandomPieceGenerator() : base()
	{
		rng = new Random();
	}
	
	public RandomPieceGenerator(PieceID[] availablePieces) : base(availablePieces)
	{
		rng = new Random();
	}

	public void SetSeed(long seed)
	{
		rng = new Random( (int)seed );
	}
}
