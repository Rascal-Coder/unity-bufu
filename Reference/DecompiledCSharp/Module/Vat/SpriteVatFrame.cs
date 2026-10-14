using System;
using UnityEngine;

namespace Module.Vat;

[Serializable]
public struct SpriteVatFrame
{
	public Sprite sprite;

	public Texture2D texture;

	public Vector4 uvRect;

	public Vector2 size;

	public Vector2 pivot;

	public float pixelsPerUnit;

	public bool useUvRect;
}
