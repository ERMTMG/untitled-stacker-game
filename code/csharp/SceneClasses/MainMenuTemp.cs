using Godot;
using System;
using USG.UI;

using GC = Godot.Collections;

namespace USG;

public partial class MainMenuTemp : Control, ISceneDataEmitter
{
	[Export] private GC.Array<GameModeButton> buttons;
	private PackedScene targetScene;
	[Export] private PackedScene SettingsScene;
	
	private static Action GetButtonSpecificEventHandler(Action<GameModeButton> genericEventHandler, GameModeButton button)
	{
		return () => genericEventHandler(button);
	}

	private void OnGameModeButtonPressed(GameModeButton button)
	{
		PackedScene scene = button.GameModeScene;
		if(scene is not null)
		{
			targetScene = scene;
			GetTree().CreateTimer(0.25).Timeout += () => {
				SceneManager.Instance.SwitchScene(SettingsScene, transitionKind: SceneTransitionScreen.TransitionKind.WipeToRight);
			};
			foreach(GameModeButton otherButton in buttons)
			{
				otherButton.Disabled = true;
			}
		}
	}

	public override void _Ready()
	{
		base._Ready();
		foreach(GameModeButton button in buttons)
		{
			button.Pressed += GetButtonSpecificEventHandler(OnGameModeButtonPressed, button);
		}
	}

	public SceneData GetData()
	{
		return new GamemodeSettingsSceneData(targetScene);
	}
}
