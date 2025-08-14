using Godot;
using System;

namespace USG;

public partial class PiecePreviewCell : Sprite2D
{
	[Export]
	private Sprite2D pieceSprite;

	const float SIZE_MARGIN = 0.8f;

	private Vector2 EffectiveSize 
	{
		get => Scale * Texture.GetSize();
	}
	
	private Vector2 MaxInternalSpriteSize 
	{
		get => SIZE_MARGIN * EffectiveSize;
	}

	static readonly StringName EnabledShaderParameter = "enabled";

	public bool PieceDisabled
	{
		get => (bool)(pieceSprite.Material as ShaderMaterial).GetShaderParameter(EnabledShaderParameter);
		set {
			(pieceSprite.Material as ShaderMaterial).SetShaderParameter(EnabledShaderParameter, value);
		}
	}

	public void SetPiece(string pieceID)
	{
		if(String.IsNullOrEmpty(pieceID))
		{
			pieceSprite.Texture = null;
			return;
		}
		Texture2D texture = Pieces.PiecePreviewSprites[pieceID];
		pieceSprite.Texture = texture;
		if(texture.GetSize() > MaxInternalSpriteSize)
		{
			Vector2 tightFitScale = MaxInternalSpriteSize / texture.GetSize();
			pieceSprite.Scale = Math.Min(tightFitScale.X, tightFitScale.Y)*Vector2.One/this.Scale;
		} else {
			pieceSprite.Scale = Vector2.One/this.Scale;
		}
	}

	public void PlaceUpperLeftCornerAt(Vector2 position)
	{
		this.Position = position + (EffectiveSize / 2f);
	}

	public void PlaceUpperRightCornerAt(Vector2 position)
	{
		Vector2 offset = EffectiveSize / 2f;
		offset = offset with { X = -offset.X };
		this.Position = position + offset;
	}

	public override void _Ready()
	{
		base._Ready();
	}




}
