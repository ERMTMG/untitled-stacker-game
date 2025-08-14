using Godot;
using System;
using USG.UI;

using GC = Godot.Collections;

namespace USG;

public partial class MainMenuTemp : Control
{
	[Export] public GC.Array<GameModeButton> buttons;
	
	private static Action GetButtonSpecificEventHandler(Action<GameModeButton> genericEventHandler, GameModeButton button)
	{
		return () => genericEventHandler(button);
	}

	private void OnGameModeButtonPressed(GameModeButton button)
	{
		PackedScene scene = button.GameModeScene;
		if(scene is not null)
		{
			GetTree().CreateTimer(0.25).Timeout += () => {
				SceneManager.Instance.SwitchScene(scene, transitionKind: SceneTransitionScreen.TransitionKind.FadeToBlack);
			};
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

}
