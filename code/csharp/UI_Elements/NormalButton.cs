using Godot;
using System;

namespace USG.UI;

public partial class NormalButton : Button
{
    [ExportGroup("Node Connections")]
    [Export] private Label textLabel;
    [Export] private Sprite2D buttonSprite;
    [ExportGroup("Options")]
    [Export] private string buttonText = "TEXT";
    [Export] private int fontSize = 42;
    
    private static readonly Color ButtonHoveredModulate = new(1.1f, 1.1f, 1.1f);
	const double ButtonHoverAnimationDuration = 0.1;
	private static readonly NodePath ButtonColorProperty = "modulate";
	private static readonly StringName IconTimeShaderParam = "time";
	const double IconTimeShaderParamMax = 0.5;
	
	private bool hoveredLastFrame;

	private Callable callableSetShaderTime;
	
	public override void _Ready()
	{
		hoveredLastFrame = false;
		textLabel.Text = buttonText;
		textLabel.LabelSettings.FontSize = fontSize;
		base._Ready();
		
		this.Pressed += OnPress;
	}
	
	public override void _Process(double delta)
	{
		base._Process(delta);
		if(IsHovered() && !hoveredLastFrame)
		{
			hoveredLastFrame = true;
			Tween tween = GetTree().CreateTween();
			tween.TweenProperty(
				this, 
				ButtonColorProperty, 
				ButtonHoveredModulate, 
				ButtonHoverAnimationDuration
			);
		} 
		else if(!IsHovered() && hoveredLastFrame)
		{
			hoveredLastFrame = false;
			Tween tween = GetTree().CreateTween();
			tween.TweenProperty(
				this, 
				ButtonColorProperty, 
				Colors.White, 
				ButtonHoverAnimationDuration
			);
		}
	}
	
	private void OnPress()
	{
		this.Modulate = new Color(2f, 2f, 2f);
		Tween tween = GetTree().CreateTween();
		tween.TweenProperty(
			this,
			"modulate",
			Colors.White,
			0.25
		);
	}
}
