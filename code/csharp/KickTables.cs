using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace USG;

using KickTable = ReadOnlyDictionary<(RotationState, RotationDirection), CellPosition[]>;
using PieceID = string;

public static partial class Pieces 
{
    const RotationState North = RotationState.North;
    const RotationState East = RotationState.East;
    const RotationState South = RotationState.South;
    const RotationState West = RotationState.West;

    const RotationDirection Right = RotationDirection.Right;
    const RotationDirection Left = RotationDirection.Left;
    const RotationDirection FullRotation = RotationDirection.FullRotation;

    private static KickTable EmptyKickTable = new Dictionary<(RotationState, RotationDirection), CellPosition[]>
    {
        
    }.AsReadOnly();

    private static KickTable SRSGenericTetrominoKickTable = new Dictionary<(RotationState, RotationDirection), CellPosition[]>()
    {
        [(North, Right)] = [new (0,-1), new (-1,-1), new (2,0), new (2,-1)],
        [(North, Left)] = [new (0,1), new (-1,1), new (2,0), new (2,1)],

        [(East, Right)] = [new (0,1), new (1,1), new (-2,0), new (-2,1)],
        [(East, Left)] = [new (0,1), new (1,1), new (-2,0), new (-2,1)],

        [(South, Right)] = [new (0,1), new (-1,1), new (2,0), new (2,1)],
        [(South, Left)] = [new (0,-1), new (-1,-1), new (2,0), new (2,-1)],

        [(West, Right)] = [new (0,-1), new (1,-1), new (-2,0), new (-2,-1)],
        [(West, Left)] = [new (0,-1), new (1,-1), new (-2,0), new (-2,-1)],
    }.AsReadOnly();

    private static KickTable SRSITetrominoKickTable = new Dictionary<(RotationState, RotationDirection), CellPosition[]>()
    {
        [(North, Right)] = [new (0,-2), new (0,1), new (1,-2), new (-2,1)],
        [(North, Left)] = [new (0,-1), new (0,2), new (-2,-1), new (1,2)],

        [(East, Right)] = [new (0,-1), new (0,2), new (-2,-1), new (1,2)],
        [(East, Left)] = [new (0,2), new (0,-1), new (-1,2), new (2,-1)],

        [(South, Right)] = [new (0,2), new (0,-1), new (-1,2), new (2,-1)],
        [(South, Left)] = [new (0,1), new (0,-2), new (2,1), new (-1,-2)],

        [(West, Right)] = [new (0,1), new (0,-2), new (2,1), new (-1,-2)],
        [(West, Left)] = [new (0,-2), new (0,1), new (1,-2), new (-2,1)],
    }.AsReadOnly();

    public static readonly ReadOnlyDictionary<PieceID, KickTable> SRSKickTables = new Dictionary<PieceID, KickTable>()
    {
        ["T"] = SRSGenericTetrominoKickTable,
        ["I"] = SRSITetrominoKickTable,
        ["S"] = SRSGenericTetrominoKickTable,
        ["Z"] = SRSGenericTetrominoKickTable,
        ["L"] = SRSGenericTetrominoKickTable,
        ["J"] = SRSGenericTetrominoKickTable,
        ["O"] = EmptyKickTable
    }.AsReadOnly();
}