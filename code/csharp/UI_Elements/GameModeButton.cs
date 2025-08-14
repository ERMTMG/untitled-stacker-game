using Godot;
using System;

namespace USG.UI;

public partial class GameModeButton : Button
{   
	[ExportGroup("Node Connections")]
	[Export] private Sprite2D buttonBgSprite;
	[Export] private Sprite2D iconSprite;
	[Export] private Sprite2D iconFrame;
	[Export] private Label titleLabel;
	[Export] private RichTextLabel descLabel;

	[ExportGroup("Gamemode Button Options")]
	[Export] public string GameModeName { get; set; }
	[Export(PropertyHint.MultilineText)] public string GameModeDescription { get; set; }
	[Export] public Texture2D GameModeIcon { get; set; }
	[Export] public PackedScene GameModeScene { get; set; }

	private static readonly Vector2 ButtonHoveredScale = new(1.05f, 1f);
	const double ButtonHoverAnimationDuration = 0.1;
	private static readonly NodePath ButtonBgScaleProperty = "scale";
	private static readonly StringName IconTimeShaderParam = "time";
	const double IconTimeShaderParamMax = 0.5;

	bool hoveredLastFrame;

	public override void _Ready()
	{
		base._Ready();
		iconSprite.Texture = GameModeIcon;
		titleLabel.Text = GameModeName;
		descLabel.Text = GameModeDescription;
		PositionButtonText();
		buttonBgSprite.Scale = Vector2.One;
		hoveredLastFrame = false;

		// Can't make this readonly because it's initialized in _Ready(). have to pinky promise i won't change it
		callableSetIconShaderTime = new Callable(this, MethodName.SetIconShaderTime);
		
		this.Pressed += OnPress;
	}

	private void SetIconShaderTime(double value){
		(iconFrame.Material as ShaderMaterial).SetShaderParameter(IconTimeShaderParam, value);
	}

	private Callable callableSetIconShaderTime;

	private void PositionButtonText(){
		const int MaxLines = 2;
		int lines = descLabel.GetLineCount();
		if(lines > MaxLines){
			GD.PushError($"GameModeButton description too long, surpasses maximum line count of {MaxLines}. Text is: \"{descLabel.Text}\"");
		}
		if(lines == 1){
			float yPosIncrement = .1f * Size.Y;
			titleLabel.Position = titleLabel.Position with { Y = titleLabel.Position.Y + yPosIncrement };
			descLabel.Position = descLabel.Position with { Y = descLabel.Position.Y + yPosIncrement };
		}
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		if(IsHovered() && !hoveredLastFrame)
		{
			hoveredLastFrame = true;
			Tween tween = GetTree().CreateTween();
			tween.TweenProperty(
				buttonBgSprite, 
				ButtonBgScaleProperty, 
				ButtonHoveredScale, 
				ButtonHoverAnimationDuration
			);
			tween.TweenMethod(
				callableSetIconShaderTime, 
				0.0, 
				IconTimeShaderParamMax, 
				2.5*ButtonHoverAnimationDuration
			);
			tween.Finished += () => {
				(iconFrame.Material as ShaderMaterial).SetShaderParameter(IconTimeShaderParam, 0.0);
			};
		} 
		else if(!IsHovered() && hoveredLastFrame) 
		{
			hoveredLastFrame = false;
			Tween tween = GetTree().CreateTween();
			tween.TweenProperty(buttonBgSprite, ButtonBgScaleProperty, Vector2.One, ButtonHoverAnimationDuration);
		}
	}

	private void OnPress()
	{
		Modulate = new Color(2f, 2f, 2f);
		Tween tween = GetTree().CreateTween();
		tween.TweenProperty(this, "modulate", Colors.White, 0.2);
	}
}
