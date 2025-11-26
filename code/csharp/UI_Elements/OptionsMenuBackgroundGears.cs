using Godot;
using System;

namespace USG;

public partial class OptionsMenuBackgroundGears : Control
{
    [Export] private TextureRect gear1;
    [Export] private TextureRect gear2;
    [Export] private double RotationSpeed;

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        float rotationIncrement = (float)(RotationSpeed * delta);
        gear1.Rotation += rotationIncrement;
        gear2.Rotation -= rotationIncrement;
    }
}
