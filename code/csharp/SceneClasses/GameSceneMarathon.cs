using Godot;
using System;

namespace USG;

public partial class GameSceneMarathon : GameScene, ISceneDataReceiver
{
    private int linesTarget = 150;

    [Export] private int LinesTarget
    {
        get => linesTarget;
        set
        {
            if(value <= 0)
            {
                throw new ArgumentException($"Tried to set LinesTarget to {value}. LinesTarget must be a positive integer.");
            }
            linesTarget = value;
        }
    }
    
    [Export] private bool enableMasterLevelling;
    
    private int totalLinesCleared;
    
    public GameSceneMarathon() : base() {}

    public override void _Ready()
    {
        base._Ready();
        totalLinesCleared = 0;
        board.StatsShown = BoardDisplay.Stats.All;
        board.LineCleared += OnBoardLineCleared;
        board.StartGameCountdown();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
    }
    
    private void OnBoardLineCleared(int linesCleared, string pieceID)
    {
        totalLinesCleared += linesCleared;
        if(!enableMasterLevelling && board.BoardGravityLevel == GameBoard.MAX_GRAVITY)
        {
            board.BoardDisableLevelling();
        }
        if(totalLinesCleared >= LinesTarget)
        {
            TimeSpan time = TimeSpan.FromSeconds((double)board.BoardTimeSeconds);
            long score = board.BoardScore;
            
            board.GameOverWin($"Total time: {time.Minutes}:{time.Seconds:D2}.{time.Milliseconds:D3}\n" +
                              $"Total score: {score}");
        }
    }

    public void InitData(SceneData data)
    {
        if(data is TestMessageSceneData messageSceneData)
        {
            GD.Print(messageSceneData.Message);
        }
    }
}
