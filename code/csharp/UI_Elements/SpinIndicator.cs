using Godot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace USG.UI;
using PieceID = string;


public partial class SpinIndicator : Sprite2D
{
	
	private static readonly ReadOnlyDictionary<(PieceID, bool), String> SpinIndicatorSpritePaths = new Dictionary<(PieceID, bool), PieceID>(){
		[("T", false)] = "res://assets/graphics/action_text/spins/tspin.png",
		[("T", true )] = "res://assets/graphics/action_text/spins/tspin_mini.png",
		[("S", false)] = "res://assets/graphics/action_text/spins/sspin.png",
		[("S", true )] = "res://assets/graphics/action_text/spins/sspin_mini.png",
		[("Z", false)] = "res://assets/graphics/action_text/spins/zspin.png",
		[("Z", true )] = "res://assets/graphics/action_text/spins/zspin_mini.png",
		[("L", false)] = "res://assets/graphics/action_text/spins/lspin.png",
		[("L", true )] = "res://assets/graphics/action_text/spins/lspin_mini.png",
		[("J", false)] = "res://assets/graphics/action_text/spins/jspin.png",
		[("J", true )] = "res://assets/graphics/action_text/spins/jspin_mini.png",
		[("I", false)] = "res://assets/graphics/action_text/spins/ispin.png",
		[("I", true )] = "res://assets/graphics/action_text/spins/ispin_mini.png",
	}.AsReadOnly();

	const float DEFAULT_SIZE = 0.24f;
	const string DEFAULT_SPIN_MINI_SPRITE = "res://assets/graphics/action_text/spins/spin_mini.png";
	const string DEFAULT_SPIN_SPRITE = "res://assets/graphics/action_text/spins/spin.png";

	const double TWEEN_ANIM_DURATION = 2.25;

	private static readonly NodePath ScaleProperty = "scale";
	private static readonly NodePath ModulateProperty = "modulate";


	private Tween currentTween;

	public override void _Ready()
	{
		this.Scale = DEFAULT_SIZE * Vector2.One;
		this.Modulate = Colors.Transparent;
		this.currentTween = null;
		base._Ready();
	}


	public void SetSpinInformation(PieceID pieceID, SpinType spinType)
	{
		if(spinType == SpinType.NoSpin) return;
		if(currentTween is not null && currentTween.IsValid())
		{
			currentTween.Kill();
			currentTween = null;
		} 
		bool isSpinMini = (spinType == SpinType.SpinMini);
		string spritePath = SpinIndicatorSpritePaths.GetValueOrDefault((pieceID, isSpinMini), null);
		spritePath ??= (isSpinMini ? DEFAULT_SPIN_MINI_SPRITE : DEFAULT_SPIN_SPRITE);
		this.Texture = GD.Load<Texture2D>(spritePath);
		this.Scale = DEFAULT_SIZE * Vector2.One;
		this.Modulate = Colors.White;
		this.currentTween = GetTree().CreateTween();
		currentTween.SetEase(Tween.EaseType.In);
		currentTween.SetTrans(Tween.TransitionType.Expo);
		currentTween.TweenProperty(this, ModulateProperty, Colors.Transparent, TWEEN_ANIM_DURATION);
		currentTween.SetEase(Tween.EaseType.InOut);
		currentTween.SetTrans(Tween.TransitionType.Quad);
		currentTween.TweenProperty(this, ScaleProperty, Vector2.Zero, TWEEN_ANIM_DURATION * 4);
	}

}
