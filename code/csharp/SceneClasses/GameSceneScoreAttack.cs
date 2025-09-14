using System;
using Godot;

namespace USG;

public partial class GameSceneScoreAttack : GameScene, ISceneDataReceiver
{
	private bool gameEnded = false;
	[Export] private double timeLimitSeconds = 120.0;
	
	public GameSceneScoreAttack() : base() { }

	public override void _Ready()
	{
		base._Ready();
		board.StatsShown = BoardDisplay.Stats.PiecesPlaced | BoardDisplay.Stats.PiecesPerSecond | BoardDisplay.Stats.Time;
		board.StartGameCountdown();
		gameEnded = false;
	}

	public override void _Process(double delta)
	{
		double totalTime = (double)board.BoardTimeSeconds;
		if(!gameEnded && totalTime >= timeLimitSeconds)
		{
			string message = $"Score obtained: {board.BoardScore}";
			board.GameOverWin(message);
			gameEnded = true;
		}
	}

	public void InitData(SceneData data)
	{
		GD.Print("Hi this is dollar store blitz");
	}
}
