using System;
using Godot;

namespace USG;

public class GameInfo 
{
    const double SECONDS_PER_MINUTE = 60.0;
    const double TETRIO_VERSUS_MULTIPLIER = 100.0;

    public long Score { get; set; }
    public int GameLevel { get; set; }
    public int PiecesPlaced { get; set; }
    public int AttackSent { get; set; }
    public decimal TimePassedSeconds { get; set; }
    public int LinesCleared { get; set; }
    public int GarbageLinesCleared { get; set; } 

    public double PiecesPerSecond => PiecesPlaced / (double)TimePassedSeconds;
    public double AttackPerMinute => AttackSent / (double)TimePassedSeconds / SECONDS_PER_MINUTE;
    public double AttackPerPiece => AttackSent / PiecesPlaced;
    public double TetrioVersusScore => (AttackSent + GarbageLinesCleared) / (double)TimePassedSeconds * TETRIO_VERSUS_MULTIPLIER;
    
    public GameInfo()
    {
        Score = 0L;
        GameLevel = 0;
        PiecesPlaced = 0;
        AttackSent = 0;
        TimePassedSeconds = 0m;
        LinesCleared = 0;
        GarbageLinesCleared = 0;
    }

}