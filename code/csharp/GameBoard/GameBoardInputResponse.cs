using System;
using System.Numerics;
using Godot;

namespace USG;

// The part of the GameBoard class that responds to input signals
public partial class GameBoard : Node
{
	private void ConnectInputSignals()
	{
		input.HardDropPressed += OnHardDropPressed;
		input.HoldPiecePressed += OnHoldPiecePressed;
		input.RotateLeftPressed += OnRotatePieceLeftPressed;
		input.RotateRightPressed += OnRotatePieceRightPressed;
		input.RotateFullPressed += OnRotatePieceFullPressed;
		input.SoftDropPressed += OnSoftDropPressed;
	}

	public void DisconnectInputSignals()
	{
		input.HardDropPressed -= OnHardDropPressed;
		input.HoldPiecePressed -= OnHoldPiecePressed;
		input.RotateLeftPressed -= OnRotatePieceLeftPressed;
		input.RotateRightPressed -= OnRotatePieceRightPressed;
		input.RotateFullPressed -= OnRotatePieceFullPressed;
		input.SoftDropPressed -= OnSoftDropPressed;
	}

	public void OnHardDropPressed()
	{
		bool continueDrop = true;
		do
		{
			continueDrop = CurrentPiece.TryMove(CellPosition.Down, resetLastMoveRotation: false);
		} while(continueDrop);
		PlaceCurrentPiece();
	}
	public void OnHoldPiecePressed()
	{
		if(!hasHeldPiece)
		{
			SwapHeldPiece();
		} else {
			// Deny hold
		}
	}
	public void OnRotatePiecePressed(int direction)
	{
		RotationDirection rotationDirection = (RotationDirection)direction;
		currentPiece.RotatePiece(rotationDirection);
	}
	public void OnRotatePieceLeftPressed() => OnRotatePiecePressed((int)RotationDirection.Left);
	public void OnRotatePieceRightPressed() => OnRotatePiecePressed((int)RotationDirection.Right);
	public void OnRotatePieceFullPressed() => OnRotatePiecePressed((int)RotationDirection.FullRotation);
	public void OnLeftPressed()
	{
	}
	public void OnLeftReleased()
	{
	}
	public void OnRightPressed()
	{
	}
	public void OnRightReleased()
	{
	}
	public void OnSoftDropPressed()
	{
		isSoftDropping = true;
		gravityMsPerTileCounter = 0.0;
	}
	public void OnSoftDropReleased()
	{
		isSoftDropping = false;
	}
}
