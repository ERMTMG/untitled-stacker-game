using Godot;
using System;
using System.Threading.Tasks;

public partial class SceneTransitionScreen : CanvasLayer
{
	public override void _Ready()
	{
		base._Ready();
		endAnimationName = null;
	}

	public enum TransitionKind 
	{
		None = 0,
		FadeToBlack,
		//...
	}

	[Signal] public delegate void TransitionMidpointReachedEventHandler();

	[Export] private AnimationPlayer animationPlayer;
	private StringName endAnimationName;

	private (StringName beginAnimation, StringName endAnimation) GetTransitionAnimationNames(TransitionKind kind)
	{
		
		return kind switch
		{
			TransitionKind.None => (new("RESET"), new("RESET")),
			TransitionKind.FadeToBlack => (new("fade_black_begin"), new("fade_black_end")),
			_ => throw new ArgumentException($"{kind} is not a valid screen transition kind."),
		};
	}

	public void BeginTransition(TransitionKind kind)
	{
		(StringName beginAnimation, StringName endAnimation) = GetTransitionAnimationNames(kind);
		this.endAnimationName = endAnimation;
		animationPlayer.Play(beginAnimation);
	}

	public void EmitTransitionMidpoint()
	{
		EmitSignal(SignalName.TransitionMidpointReached);
	}

	public void EndStartedTransition()
	{
		animationPlayer.Play(this.endAnimationName);
		animationPlayer.AnimationFinished += (_) => QueueFree();
	}

	
}
