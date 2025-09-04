using Godot;
using System;

public partial class LineClearAnimationTest : Sprite2D
{
	public Vector2 Size
	{
		get => Texture.GetSize() * Scale;
		set
		{
			Scale = value / Texture.GetSize();
		}
	}
}
