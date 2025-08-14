using System;
using System.IO;
using Godot;

namespace USG;

using TransitionKind = SceneTransitionScreen.TransitionKind;

public partial class SceneManager : Node
{
	private static readonly StringName TransitionMidpointSignalName = "TransitionMidpointReached";

	public static SceneManager Instance { get; private set; }
	private static PackedScene TransitionPackedScene => GD.Load<PackedScene>("res://scenes/ui/scene_transition_screen.tscn");

	public override void _Ready()
	{
		base._Ready();
		Instance = this;
	}

	public void SwitchScene(string pathToScene, TransitionKind transitionKind = TransitionKind.None, Node fromScene = null)
	{
		PackedScene sceneToLoad = GD.Load<PackedScene>(pathToScene);
		if(sceneToLoad is null)
		{
			throw new FileLoadException($"Couldn't load scene from \"{pathToScene}\"");
		} else {
			SwitchScene(sceneToLoad, transitionKind, fromScene);
		}
	}

	public async void SwitchScene(PackedScene toScene, TransitionKind transitionKind = TransitionKind.None, Node fromScene = null)
	{
		if(fromScene is null)
		{
			fromScene = GetTree().CurrentScene;
		}
		SceneTransitionScreen transition = CreateTransition(transitionKind);
		await ToSignal(transition, TransitionMidpointSignalName);
		Node newCurrentScene = toScene.Instantiate();
		GetTree().Root.AddChild(newCurrentScene);
		if(fromScene != GetTree().Root)
		{
			fromScene.QueueFree();
		}
		GetTree().CurrentScene = newCurrentScene;
		transition.EndStartedTransition();
	}

	private SceneTransitionScreen CreateTransition(TransitionKind transitionKind)
	{
		SceneTransitionScreen transition = TransitionPackedScene.Instantiate<SceneTransitionScreen>();
		GetTree().Root.AddChild(transition);
		transition.BeginTransition(transitionKind);
		return transition;
	}
}
