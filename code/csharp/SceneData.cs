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