using Godot;
using System;

namespace USG;

public partial class GameScene40Lines : GameScene
{
	private int totalLinesCleared;
	[Export] private int targetLines = 40;

	public GameScene40Lines() : base()
	{ }

	public override void _Ready()
	{
		base._Ready();
		board.StatsShown = BoardDisplay.Stats.All;
		board.LineCleared += OnBoardLineCleared;
		board.StartGameCountdown();
		
		totalLinesCleared = 0;
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
	}

	private void OnBoardLineCleared(int linesCleared, string _)
	{
		totalLinesCleared += linesCleared;
		if(totalLinesCleared >= targetLines)
		{	
			TimeSpan time = TimeSpan.FromSeconds((double)board.BoardTimeSeconds);
			string message = $"Total time: {time.Minutes}:{time.Seconds:D2}.{time.Milliseconds:D3}";
			board.GameOverWin(message);
		}
	}
}
