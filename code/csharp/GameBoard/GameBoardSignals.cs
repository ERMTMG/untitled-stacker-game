using System;
using Godot;

namespace USG;

public partial class GameBoard : Node
{
    
    public delegate void LineClearedEventHandler(int linesCleared, string pieceID, PiecePlacementInformation info);
    public event LineClearedEventHandler LineCleared;
    
    public delegate void PiecePlacedEventHandler(string pieceID, CellPosition piecePosition, RotationState rotationState, SpinType spin = SpinType.NoSpin);
    public event PiecePlacedEventHandler PiecePlaced;

    public delegate void PieceSpinnedEventHandler(string pieceID, CellPosition piecePosition, RotationState rotationState, SpinType spinType);
    public event PieceSpinnedEventHandler PieceSpinned;

    public delegate void NextPieceSpawnedEventHandler();
    public event NextPieceSpawnedEventHandler NextPieceSpawned;
    
    public delegate void PieceHeldEventHandler();
    public event PieceHeldEventHandler PieceHeld;

    public delegate void ToppedOutEventHandler(TopOutType type);
    public event ToppedOutEventHandler ToppedOut;
}