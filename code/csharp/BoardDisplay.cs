using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using USG.UI;

namespace USG;

using GC = Godot.Collections;
using LineClearNotif = UI.LineClearNotification;

public partial class BoardDisplay : CanvasGroup
{
	public enum Stats {
		Time             = 1 << 0,
		PiecesPlaced     = 1 << 1,
		LinesCleared     = 1 << 2,
		PiecesPerSecond  = 1 << 3,

		All = Time | PiecesPlaced | LinesCleared | PiecesPerSecond
	}

	static readonly PackedScene MinoPlacementEffectScene = GD.Load<PackedScene>("res://scenes/mino_placement_effect.tscn");
	static readonly PackedScene LineClearNotificationScene = GD.Load<PackedScene>("res://scenes/ui/line_clear_notification.tscn");

	[Export] private GameBoard board;
	[Export] private BoardDrawingComponent boardDrawingComponent;
	[Export] private GC.Array<PiecePreviewCell> nextPiecePreviews;
	[Export] private PiecePreviewCell heldPiecePreview;
	[Export] private Label infoLabel;
	[Export] private Label countDownLabel;
	[Export] private RichTextLabel comboLabel;
	[Export] private AnimationPlayer animationPlayer;
	[Export] private RichTextLabel postGameLabel;
	[Export] private SpinIndicator spinIndicator;
	[Export] private BoardSettings boardSettings;

	private Stats statsShown;

	public Vector2 BoardCenterOffset => new Vector2(
		boardDrawingComponent.BoardEffectiveWidth / 2f,
		boardDrawingComponent.BoardEffectiveHeight / 2f
	);

	private static readonly StringName StartCountdownAnimationName = "starting_countdown";

	public Stats StatsShown 
	{ 
		get => statsShown; 
		set
		{
			if(value <= Stats.All)
			{
				statsShown = value;
			} else {
				throw new ArgumentException($"Tried to set StatsShown to {value:B}. StatsShown is a bitwise flag varaible and its value can't exceed {Stats.All:B3}");
			}
		} 
	}

	public decimal BoardTimeSeconds => board.TimePassedSeconds;
	public int BoardLinesCleared => board.LinesCleared;
	public int BoardPiecesPlaced => board.PiecesPlaced;
	public List<LineClearNotif> lineClearNotifications;

	BoardDisplay() {}

	public delegate void BoardToppedOutEventHandler(GameBoard.TopOutType type);
	public event BoardToppedOutEventHandler BoardToppedOut;

	public override void _Ready()
	{
		base._Ready();

		board.PiecePlaced += OnBoardPiecePlaced;
		board.PieceSpinned += OnBoardPieceSpinned;
		board.NextPieceSpawned += OnBoardNextPieceSpawned;
		board.PieceHeld += OnBoardPieceHeld;
		board.ToppedOut += OnBoardToppedOut;
		board.LineCleared += OnBoardLineCleared;
		board.LineClearAreAnimation += OnBoardLineClearAreAnimation;
		animationPlayer.AnimationFinished += OnAnimationPlayerFinished;

		board.Settings = this.boardSettings;
		this.lineClearNotifications = [];

		SetUpBoardElementPositions();
	}

	private void SetUpBoardElementPositions()
	{
		heldPiecePreview.PlaceUpperRightCornerAt(Vector2.Zero);
		Vector2 boardUpperRightCorner = new(boardDrawingComponent.BoardEffectiveWidth,0);
		if(nextPiecePreviews.Count != 0)
		{
			float piecePreviewSpriteHeight = 
				nextPiecePreviews[0].Texture.GetHeight()*nextPiecePreviews[0].Scale.Y;
				// nextPiecePreviews[0] acts as a stand-in for any of the next piece previews. It's assumed they're all equal in size
			for(int i = 0; i < nextPiecePreviews.Count; i++)
			{
				PiecePreviewCell cell = nextPiecePreviews[i];
				cell.PlaceUpperLeftCornerAt(
					boardUpperRightCorner + i*piecePreviewSpriteHeight*Vector2.Down
				);
				cell.SetPiece(null);
			}
		}

		heldPiecePreview.SetPiece(null);

		Vector2 heldPieceUpperLeftCorner = heldPiecePreview.Position - heldPiecePreview.Scale * heldPiecePreview.Texture.GetSize() / 2;
		infoLabel.Position = heldPieceUpperLeftCorner + new Vector2(-5, 300);
		infoLabel.Size = infoLabel.Size with 
		{
			X = (heldPiecePreview.Scale.X * heldPiecePreview.Texture.GetWidth()) - 5
		};
		comboLabel.Position = infoLabel.Position + new Vector2(0, -50);
		comboLabel.Size = comboLabel.Size with
		{
			X = (heldPiecePreview.Scale.X * heldPiecePreview.Texture.GetWidth()) - 10
		};
		comboLabel.Text = string.Empty;
		countDownLabel.Position = (new Vector2(
									boardDrawingComponent.BoardEffectiveWidth, 
									boardDrawingComponent.BoardEffectiveHeight - countDownLabel.Size.Y
								  ) - countDownLabel.Size) / 2;
		postGameLabel.Size = new Vector2(
								boardDrawingComponent.BoardEffectiveWidth * 0.9f,
								70
							 );
		postGameLabel.Position = (new Vector2(
									boardDrawingComponent.BoardEffectiveWidth, 
									boardDrawingComponent.BoardEffectiveHeight + 0.5f*countDownLabel.Size.Y
								  ) - postGameLabel.Size) / 2;
		
		postGameLabel.Hide();

		Vector2 heldPieceLowerLeftCorner = heldPieceUpperLeftCorner + heldPiecePreview.Scale.Y * heldPiecePreview.Texture.GetHeight() * Vector2.Down;
		spinIndicator.Position = heldPieceLowerLeftCorner 
			+ (spinIndicator.Scale.Y * spinIndicator.Texture.GetHeight() / 2)*Vector2.Down
			+ (spinIndicator.Scale.X * spinIndicator.Texture.GetWidth() / 2)*Vector2.Left;
	}

	public void StartGameCountdown()
	{
		board.SetPause(GameBoard.PauseMode.AllPaused);
		animationPlayer.Play(StartCountdownAnimationName);
	}

	private void OnAnimationPlayerFinished(StringName animationName)
	{
		if(animationName == StartCountdownAnimationName)
		{
			SetUpNextQueuePreviews();
		}
	}

	protected void SetLabel()
	{
		StringBuilder labelText = new("");
		if((statsShown | Stats.Time) != 0)
		{
			TimeSpan time = TimeSpan.FromSeconds((double)board.TimePassedSeconds);
			labelText.Append($"{time.Minutes}:{time.Seconds:D2}:{time.Milliseconds % 1000:D3}s\n\n");
		}
		if((statsShown | Stats.PiecesPlaced) != 0)
		{
			labelText.Append($"pieces: {board.PiecesPlaced}\n\n");
		}
		if((statsShown | Stats.LinesCleared) != 0)
		{
			labelText.Append($"lines: {board.LinesCleared}\n\n");
		}
		if((statsShown | Stats.PiecesPerSecond) != 0)
		{
			labelText.Append($"{board.PiecesPerSecond:F2}PPS");
		}

		//GD.Print(labelText.ToString());
		infoLabel.Text = labelText.ToString();
	}

	private void HideExcessiveLineClearNotifs()
	{
		const int LINE_CLEAR_NOTIF_LIMIT = 6;
		int activeLineClearNotifs = lineClearNotifications.Count((notif) => !notif.FastDecayEnabled);
		if(activeLineClearNotifs > LINE_CLEAR_NOTIF_LIMIT)
		{
			for(int i = 0; i < lineClearNotifications.Count; i++)
			{
				if(!lineClearNotifications[i].FastDecayEnabled)
				{
					lineClearNotifications[i].EnableFastDecayAnimation();
					break; // Only enable fast decay on the oldest notification
				}
			}
		}
	}

	static readonly string[] COMBO_COLOR_HEX_CODES = [
		"ff9aa2", "ffb7b2", "ffdac1", "e2f0cb", "b5ead7", "c7ceea"
	];
	public void SetComboLabel()
	{
		if(!board.ComboActive || board.ComboValue == 0)
		{
			comboLabel.Text = string.Empty;
			return;
		}
		int comboNumber = board.ComboValue;
		string comboHexCode = COMBO_COLOR_HEX_CODES[comboNumber % COMBO_COLOR_HEX_CODES.Length];
		string comboString = $"[b][color={comboHexCode}]{comboNumber}[/color][/b]-COMBO";
		
		if(comboString != comboLabel.Text)
		{
			comboLabel.Text = comboString;
		}
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		SetLabel();
		HideExcessiveLineClearNotifs();
		SetComboLabel();
	}

	private void SetUpNextQueuePreviews()
	{
		for(int i = 0; i < nextPiecePreviews.Count; i++)
		{
			string ithPiece = board.GetPieceInQueuePos(i);
			nextPiecePreviews[i].SetPiece(ithPiece);
		}
	}

	private void OnBoardNextPieceSpawned()
	{
		SetUpNextQueuePreviews();
		heldPiecePreview.PieceDisabled = false;
	}

	private void OnBoardPieceHeld()
	{
		string heldPiece = board.HeldPiece;
		heldPiecePreview.SetPiece(heldPiece);
		heldPiecePreview.PieceDisabled = true;
	}

	public void PlaceBoardCenterAt(Vector2 position)
	{
		this.Position = position - BoardCenterOffset;
	}

	public void GameOverWin(string message = null)
	{
		board.SetPause(GameBoard.PauseMode.GameOver);
		countDownLabel.Modulate = Colors.White;
		if(!String.IsNullOrWhiteSpace(message))
		{
			postGameLabel.Text = message;
			postGameLabel.Show();
			Vector2 targetPos = postGameLabel.Position;
			postGameLabel.Modulate = Colors.Transparent;
			postGameLabel.Position = postGameLabel.Position with {
				Y = postGameLabel.Position.Y + 3*postGameLabel.Size.Y
			};

			Tween tween = GetTree().CreateTween();
			tween.TweenProperty(postGameLabel, "modulate", Colors.White, 0.33);
			tween.TweenProperty(postGameLabel, "position", targetPos,    0.33);
		}
		countDownLabel.Text = "YOU WIN!";
	}

	private void SpawnPiecePlacementEffect(string pieceID, CellPosition position, RotationState rotationState)
	{
		Piece piece = new Piece(pieceID, null);
		switch(rotationState)
		{
			case RotationState.North: break;
			case RotationState.West: 
				piece.RotateMatrix(RotationDirection.Left);
				break;
			case RotationState.South:
				piece.RotateMatrix(RotationDirection.FullRotation);
				break;
			case RotationState.East:
				piece.RotateMatrix(RotationDirection.Right);
				break;
			default:
				throw new Exception($"Invalid piece rotation state ({rotationState}) at BoardDisplay.OnBoardPiecePlaced");
		}
		for(int i = 0; i < piece.Size; i++)
		{
			for(int j = 0; j < piece.Size; j++)
			{
				
				if(piece.Submatrix[i,j] > 0)
				{
					Vector2 minoPosition = new(
						boardDrawingComponent.TileSize*(j + position.Col),
						boardDrawingComponent.TileSize*(i + position.Row - board.BoardHiddenPortionHeight)
					);
					MinoPlacementEffect effect = MinoPlacementEffectScene.Instantiate<MinoPlacementEffect>();
					effect.Position = minoPosition;
					effect.SetSize((float)boardDrawingComponent.TileSize);
					boardDrawingComponent.AddChild(effect);
				}
			}
		}
	}

	private void OnBoardPiecePlaced(string pieceID, CellPosition position, RotationState rotationState, SpinType spin, bool clearedLines)
	{
		if(!clearedLines)
		{
			foreach(LineClearNotif notif in lineClearNotifications)
			{
				notif.EnableFastDecayAnimation();
			}
		}
		SpawnPiecePlacementEffect(pieceID, position, rotationState);
		if(spin != SpinType.NoSpin)
		{
			spinIndicator.SetSpinInformation(pieceID, spin);
		}
	}

	private void OnBoardToppedOut(GameBoard.TopOutType type)
	{
		BoardToppedOut?.Invoke(type);
	}

	private void OnBoardLineCleared(int linesCleared, string pieceID, GameBoard.PiecePlacementInformation info)
	{
		LineClearNotif lineClearNotif = LineClearNotificationScene.Instantiate<LineClearNotif>();
		this.AddChild(lineClearNotif);
		lineClearNotif.Position = 
			BoardCenterOffset 
			- new Vector2(boardDrawingComponent.BoardEffectiveWidth / 2f, 0)
			- heldPiecePreview.Scale*heldPiecePreview.Texture.GetSize() / 2
			+ 40 * Vector2.Up + 30 * Vector2.Left
			+ 20f * GD.Randf() * Vector2.FromAngle(GD.Randf() * float.Tau);
		lineClearNotif.Scale = (float)GD.RandRange(0.25, 0.3) * Vector2.One;
		lineClearNotif.Rotation = (float)GD.Randfn(0, double.Pi/20);
		lineClearNotif.SetLineClearTexture(linesCleared);
		lineClearNotifications.Add(lineClearNotif);
		lineClearNotif.TreeExiting += () => {
			lineClearNotifications.Remove(lineClearNotif);
		};
		this.LineCleared?.Invoke(linesCleared, pieceID);
	}
	
	private void OnBoardLineClearAreAnimation(List<int> rowsCleared, double areAnimationDuration)
	{
		double currentAnimationDuration = (rowsCleared.Count == 1 ? areAnimationDuration : areAnimationDuration / 2);
		double animationDurationIncrement = (areAnimationDuration / 2) / rowsCleared.Count;
		foreach(int rowIdx in rowsCleared)
		{
			Vector2 rowPosition = (rowIdx - board.BoardHiddenPortionHeight) * new Vector2(0, boardDrawingComponent.TileSize);
			float halfRowWidth = boardDrawingComponent.BoardEffectiveWidth / 2f;
			ColorRect animationRectLeft = new ColorRect();
				animationRectLeft.Color = Colors.White;
				animationRectLeft.Position = rowPosition;
				animationRectLeft.Size = new(halfRowWidth, boardDrawingComponent.TileSize);
			ColorRect animationRectRight = new ColorRect();
				animationRectRight.Color = Colors.White;
				animationRectRight.Position = rowPosition + halfRowWidth * Vector2.Right;
				animationRectRight.Size = new(halfRowWidth, boardDrawingComponent.TileSize);
			this.AddChild(animationRectLeft);
			this.AddChild(animationRectRight);
			Tween tween = GetTree().CreateTween();
			tween.SetTrans(Tween.TransitionType.Quad)
				 .SetEase(Tween.EaseType.Out)
				 .SetParallel();
			tween.TweenProperty(
				animationRectLeft, "color", Colors.Transparent, currentAnimationDuration
			);
			tween.TweenProperty(
				animationRectRight, "color", Colors.Transparent, currentAnimationDuration
			);
			tween.TweenProperty(
				animationRectLeft, "size", new Vector2(0, boardDrawingComponent.TileSize), currentAnimationDuration
			);
			tween.TweenProperty(
				animationRectRight, "size", new Vector2(0, boardDrawingComponent.TileSize), currentAnimationDuration
			);
			tween.TweenProperty(
				animationRectRight, 
				"position", 
				animationRectRight.Position with {
					X = boardDrawingComponent.BoardEffectiveWidth
				}, 
				currentAnimationDuration
			);
			tween.Finished += () => {
				animationRectLeft.QueueFree();
				animationRectRight.QueueFree();
			};
			currentAnimationDuration += animationDurationIncrement;
		} 
	}

	private void OnBoardPieceSpinned(string pieceID, CellPosition piecePosition, RotationState rotationState, SpinType spinType)
	{
		
	}

	public Texture2D GetTexture()
	{
		// TODO: how do i do this?
		return new();
	}
}
