using Godot;
using System;

namespace USG;

public partial class MinoPlacementEffect : AnimatedSprite2D
{
	static readonly StringName DefaultAnimName = "default";

	public override void _Ready()
	{
		base._Ready();
		this.AnimationFinished += OnAnimationFinished;
		this.Frame = 0;
		this.Play();
		if(GetParent() is not BoardDrawingComponent)
		{
			throw new Exception("my dads not my dad :(");
		}
	}

	public MinoPlacementEffect() {}

	public void SetSize(Vector2 targetSize)
	{
		Texture2D currTexture = SpriteFrames.GetFrameTexture(this.Animation, this.Frame);
		this.Scale = targetSize / currTexture.GetSize();
	}

	public void SetSize(float targetSize) => SetSize(targetSize * Vector2.One);

	private void OnAnimationFinished()
	{
		this.AnimationFinished -= OnAnimationFinished;
		QueueFree();
	}

}
