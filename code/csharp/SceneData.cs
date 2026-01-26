using System;
using Godot;

namespace USG;

public record class SceneData
{
    
}

public record class TestMessageSceneData : SceneData
{
    public string Message { get; init; }

    public TestMessageSceneData(string message)
    {
        Message = message;
    }
}

public record class GamemodeSettingsSceneData : SceneData
{
    public PackedScene GameScene { get; init; }
    public BoardSettings Settings { get; set; }
    
    public GamemodeSettingsSceneData(PackedScene gameScene, BoardSettings settings)
    {
        GameScene = gameScene;
        Settings = settings;
    }
    
    public GamemodeSettingsSceneData(PackedScene gameScene)
    {
        GameScene = gameScene;
        Settings = new BoardSettings();
    }
}