using Godot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;

namespace USG.UI;

public partial class LineClearNotification : Sprite2D
{
	private static readonly ReadOnlyDictionary<int, Texture2D> LineClearTextures = new Dictionary<int, Texture2D>{
		[1] = GD.Load<Texture2D>("res://assets/graphics/action_text/line_clears/single.png"),
		[2] = GD.Load<Texture2D>("res://assets/graphics/action_text/line_clears/double.png"),
		[3] = GD.Load<Texture2D>("res://assets/graphics/action_text/line_clears/triple.png"),
		[4] = GD.Load<Texture2D>("res://assets/graphics/action_text/line_clears/tessera.png"),
		[5] = GD.Load<Texture2D>("res://assets/graphics/action_text/line_clears/penta.png"),
	}.AsReadOnly();
	public int Lines { get; private set; }

	private double vanishSpeed;
	private double shrinkSpeed;
	private bool fastDecayEnabled;

	const double BASE_VANISH_SPEED = 0.85;
	const double BASE_SHRINK_SPEED = 0.03;
	const double FAST_VANISH_SPEED = 1.25;
	const double FAST_SHRINK_SPEED = 0.05;

	const double ANIMATION_SWITCHUP_LERP_SPEED = 0.20;
	const double SLOW_ANIMATION_SPEED_DECREASE_FACTOR = 0.995;


	public override void _Ready()
	{
		base._Ready();
		vanishSpeed = BASE_VANISH_SPEED;
		shrinkSpeed = BASE_SHRINK_SPEED;
		fastDecayEnabled = false;
	}

	public void SetLineClearTexture(int linesCleared)
	{
		this.Texture = LineClearTextures.GetValueOrDefault(linesCleared, null);
		if(this.Texture is null)
		{
			GD.PushError($"No texture available for {linesCleared}-line clears");
			this.Lines = -1;
		} else {
			this.Lines = linesCleared;
		}
	}

	public void EnableFastDecayAnimation()
	{
		fastDecayEnabled = true;
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		Modulate = Modulate with {
			A = Modulate.A - (float)(vanishSpeed * delta)
		};

		Scale = (float)(Scale.X - shrinkSpeed * delta) * Vector2.One;

		if(fastDecayEnabled)
		{
			vanishSpeed += ANIMATION_SWITCHUP_LERP_SPEED * (FAST_VANISH_SPEED - vanishSpeed);
			shrinkSpeed += ANIMATION_SWITCHUP_LERP_SPEED * (FAST_SHRINK_SPEED - shrinkSpeed);
		} else {
			vanishSpeed *= SLOW_ANIMATION_SPEED_DECREASE_FACTOR;
			shrinkSpeed *= SLOW_ANIMATION_SPEED_DECREASE_FACTOR;
		}

		if(Scale <= Vector2.Zero || Modulate.A < 0f)
		{
			QueueFree();
		}
	}


}
