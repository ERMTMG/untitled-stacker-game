using System;
using Godot;

namespace USG;

public interface ISceneDataReceiver
{
    public void InitData(SceneData data);
}