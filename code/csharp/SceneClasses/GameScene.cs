using Godot;
using System;

namespace USG;

public partial class GameScene : Control
{
	[Export] protected BoardDisplay board;
	[Export] private SubViewport boardSubViewport;
	[Export] private SubViewportContainer boardViewportContainer;
	[Export] private AnimationPlayer animationPlayer;

	public override void _Ready()
	{
		base._Ready();
		Vector2 screenSize = DisplayServer.WindowGetSize();
		board.PlaceBoardCenterAt(screenSize/2);

		board.BoardToppedOut += OnBoardToppedOut;
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
	}

	public void OnBoardToppedOut(GameBoard.TopOutType type)
	{
		Texture2D viewportTexture = boardSubViewport.GetTexture();
		ShaderMaterial shader = boardViewportContainer.Material as ShaderMaterial;
		shader.SetShaderParameter("sprite_texture", viewportTexture);
		shader.SetShaderParameter("enabled", true);
		animationPlayer.Play("shader_vanish");
	}
}
