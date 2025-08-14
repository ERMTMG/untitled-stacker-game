using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

namespace USG;

using PieceID = string;

[Tool] [GlobalClass]
public partial class BagPieceGenerator : RandomPieceGenerator
{
	public BagPieceGenerator() : base()
	{}

	public BagPieceGenerator(PieceID[] availablePieces) : base(availablePieces)
	{}

	public override void GeneratePieces()
	{
		PieceID[] newPieces = availablePieces.Clone() as PieceID[];
		rng.Shuffle(newPieces);
		AddToBuffer(newPieces);
	}
}
