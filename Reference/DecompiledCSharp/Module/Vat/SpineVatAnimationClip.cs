using System;
using UnityEngine;

namespace Module.Vat;

[Serializable]
public sealed class SpineVatAnimationClip
{
	public string animationName;

	public float durationSeconds;

	public int frameCount;

	public int animationFrameCount;

	public int startFrame;

	public float startTimeSeconds;

	public int segmentIndex;

	public int segmentCount;

	public int vertexCount;

	public int textureSliceIndex;

	public Mesh bakedMesh;

	public Material[] sharedMaterials;
}
