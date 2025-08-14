using System;
using Godot;

namespace USG;

public partial class BoardDisplay : CanvasGroup {
    public delegate void LineClearedEventHandler(int linesCleared, string pieceID);
    public LineClearedEventHandler LineCleared;
}