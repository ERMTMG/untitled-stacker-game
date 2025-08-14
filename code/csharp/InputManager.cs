using Godot;
using System;
namespace USG;

public partial class InputManager : Node
{
	public enum HoldingDirection
	{
		Left,
		Right,
		None
	};

	public Action LeftPressed;
	public Action LeftReleased;
	public Action RightPressed;
	public Action RightReleased;
	public Action RotateLeftPressed;
	public Action RotateRightPressed;
	public Action RotateFullPressed;
	public Action HardDropPressed;
	public Action SoftDropPressed;
	public Action SoftDropReleased;
	public Action HoldPiecePressed;

	private HoldingDirection holdingDirection;
	private double timeHeldSecs;
	private double oldTimeHeldSecs;

	public (HoldingDirection direction, double time, double oldTime) HoldingInfo 
	{ 
		get => (holdingDirection, timeHeldSecs, oldTimeHeldSecs);
	}

	static readonly StringName LeftInput = "LEFT";
	static readonly StringName RightInput = "RIGHT";
	static readonly StringName DownInput = "DOWN";
	static readonly StringName HardDropInput = "HARDDROP";
	static readonly StringName RotateLeftInput = "ROTATELEFT";
	static readonly StringName RotateRightInput = "ROTATERIGHT";
	static readonly StringName RotateFullInput = "ROTATEFULL";
	static readonly StringName HoldPieceInput = "HOLD";

	public override void _Process(double delta)
	{
		base._Process(delta);
		oldTimeHeldSecs = timeHeldSecs;
		float dir = Input.GetAxis(LeftInput, RightInput);
		if(dir < 0f) // left
		{
			if(holdingDirection == HoldingDirection.Left)
			{
				timeHeldSecs += delta;
			} else {
				holdingDirection = HoldingDirection.Left;
				timeHeldSecs = 0.0;
			}
		} 
		else if(dir > 0f)
		{
			if(holdingDirection == HoldingDirection.Right)
			{
				timeHeldSecs += delta;
			} else {
				holdingDirection = HoldingDirection.Right;
				timeHeldSecs = 0.0;
			}
		} else {
			if(holdingDirection != HoldingDirection.None)
			{
				holdingDirection = HoldingDirection.None;
				timeHeldSecs = double.NaN;
			}
		}

		if(Input.IsActionJustPressed(LeftInput))
		{
			LeftPressed?.Invoke();
		}
		else if(Input.IsActionJustReleased(LeftInput))
		{
			LeftReleased?.Invoke();
		}

		if(Input.IsActionJustPressed(RightInput))
		{
			RightPressed?.Invoke();
		}
		else if(Input.IsActionJustReleased(RightInput))
		{
			RightReleased?.Invoke();
		}

		if(Input.IsActionJustPressed(DownInput))
		{
			SoftDropPressed?.Invoke();
		}
		else if(Input.IsActionJustReleased(DownInput))
		{
			SoftDropReleased?.Invoke();
		}

		if(Input.IsActionJustPressed(HardDropInput))
		{
			HardDropPressed?.Invoke();
		}

		if(Input.IsActionJustPressed(RotateLeftInput))
		{
			RotateLeftPressed?.Invoke();
		}

		if(Input.IsActionJustPressed(RotateRightInput))
		{
			RotateRightPressed?.Invoke();
		}

		if(Input.IsActionJustPressed(RotateFullInput))
		{
			RotateFullPressed?.Invoke();
		}

		if(Input.IsActionJustPressed(HoldPieceInput))
		{
			HoldPiecePressed?.Invoke();
		}

	}



}
