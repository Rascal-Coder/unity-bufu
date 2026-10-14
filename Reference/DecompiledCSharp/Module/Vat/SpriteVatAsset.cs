using System;
using UnityEngine;

namespace Module.Vat;

[CreateAssetMenu(fileName = "SpriteVatAsset", menuName = "VAT/Sprite VAT Asset")]
public sealed class SpriteVatAsset : ScriptableObject
{
	public Sprite[] spriteFrames;

	public Material sharedMaterial;

	public SpriteVatFrame[] frames;

	public int FrameCount
	{
		get
		{
			if (frames != null && frames.Length != 0)
			{
				return frames.Length;
			}
			if (spriteFrames == null)
			{
				return 0;
			}
			return spriteFrames.Length;
		}
	}

	public void RebuildFromSprites()
	{
		if (spriteFrames == null)
		{
			frames = Array.Empty<SpriteVatFrame>();
			return;
		}
		frames = new SpriteVatFrame[spriteFrames.Length];
		for (int i = 0; i < spriteFrames.Length; i++)
		{
			frames[i] = BuildFrame(spriteFrames[i]);
		}
	}

	public SpriteVatFrame GetFrame(int frameIndex)
	{
		if ((frames == null || frames.Length == 0) && spriteFrames != null && spriteFrames.Length != 0)
		{
			RebuildFromSprites();
		}
		if (frames == null || frames.Length == 0)
		{
			return default(SpriteVatFrame);
		}
		return frames[Mathf.Clamp(frameIndex, 0, frames.Length - 1)];
	}

	public static SpriteVatFrame BuildFrame(Sprite sprite)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)sprite == (Object)null || (Object)(object)sprite.texture == (Object)null)
		{
			return default(SpriteVatFrame);
		}
		Texture2D texture = sprite.texture;
		SpriteVatFrame result = new SpriteVatFrame
		{
			sprite = sprite,
			texture = texture,
			uvRect = CalculateUvRect(sprite)
		};
		Rect rect = sprite.rect;
		result.size = ((Rect)(ref rect)).size / sprite.pixelsPerUnit;
		float x = sprite.pivot.x;
		rect = sprite.rect;
		float num = x / Mathf.Max(1f, ((Rect)(ref rect)).width);
		float y = sprite.pivot.y;
		rect = sprite.rect;
		result.pivot = new Vector2(num, y / Mathf.Max(1f, ((Rect)(ref rect)).height));
		result.pixelsPerUnit = sprite.pixelsPerUnit;
		result.useUvRect = true;
		return result;
	}

	private void OnValidate()
	{
		if (spriteFrames != null && spriteFrames.Length != 0)
		{
			RebuildFromSprites();
		}
		else
		{
			frames = Array.Empty<SpriteVatFrame>();
		}
	}

	private static Vector4 CalculateUvRect(Sprite sprite)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		Texture2D texture = sprite.texture;
		try
		{
			Rect textureRect = sprite.textureRect;
			return new Vector4(((Rect)(ref textureRect)).x / (float)((Texture)texture).width, ((Rect)(ref textureRect)).y / (float)((Texture)texture).height, ((Rect)(ref textureRect)).width / (float)((Texture)texture).width, ((Rect)(ref textureRect)).height / (float)((Texture)texture).height);
		}
		catch (UnityException)
		{
			Vector2[] uv = sprite.uv;
			if (uv == null || uv.Length == 0)
			{
				return new Vector4(0f, 0f, 1f, 1f);
			}
			Vector2 val = uv[0];
			Vector2 val2 = uv[0];
			for (int i = 1; i < uv.Length; i++)
			{
				val = Vector2.Min(val, uv[i]);
				val2 = Vector2.Max(val2, uv[i]);
			}
			return new Vector4(val.x, val.y, val2.x - val.x, val2.y - val.y);
		}
	}
}
