using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Module.Vat;

[MovedFrom(true, "SpineVat.Editor", "Assembly-CSharp-Editor", "SpineVatBakerWindow/SpineVatAsset")]
public sealed class SpineVatAsset : ScriptableObject
{
	public SkeletonDataAsset skeletonDataAsset;

	public string sourceAssetPath;

	public string initialSkinName;

	public int bakedFrameRate;

	public int vertexCount;

	public int maxFrameCount;

	public Vector3 boundsMin;

	public Vector3 boundsMax;

	public bool meshVertexRgbNeedsVatAlpha;

	public Texture2DArray positionTextureArray;

	public Mesh bakedMesh;

	public Material[] sharedMaterials;

	public List<SpineVatAnimationClip> animations = new List<SpineVatAnimationClip>();

	public SpineVatAnimationClip FindAnimation(string animationName)
	{
		if (string.IsNullOrEmpty(animationName) || animations == null)
		{
			return null;
		}
		for (int i = 0; i < animations.Count; i++)
		{
			SpineVatAnimationClip spineVatAnimationClip = animations[i];
			if (spineVatAnimationClip != null && spineVatAnimationClip.animationName == animationName)
			{
				return spineVatAnimationClip;
			}
		}
		return null;
	}

	public SpineVatAnimationClip GetAnimationOrFirst(string animationName)
	{
		SpineVatAnimationClip spineVatAnimationClip = FindAnimation(animationName);
		if (spineVatAnimationClip != null)
		{
			return spineVatAnimationClip;
		}
		if (animations == null || animations.Count <= 0)
		{
			return null;
		}
		return animations[0];
	}

	public int GetAnimationSegmentsOrFirst(string animationName, List<SpineVatAnimationClip> results)
	{
		if (results == null)
		{
			return 0;
		}
		results.Clear();
		AddAnimationSegments(animationName, results);
		if (results.Count == 0 && animations != null && animations.Count > 0 && animations[0] != null)
		{
			AddAnimationSegments(animations[0].animationName, results);
		}
		results.Sort(CompareClipSegment);
		return results.Count;
	}

	private void AddAnimationSegments(string animationName, List<SpineVatAnimationClip> results)
	{
		if (string.IsNullOrEmpty(animationName) || animations == null)
		{
			return;
		}
		for (int i = 0; i < animations.Count; i++)
		{
			SpineVatAnimationClip spineVatAnimationClip = animations[i];
			if (spineVatAnimationClip != null && spineVatAnimationClip.animationName == animationName)
			{
				results.Add(spineVatAnimationClip);
			}
		}
	}

	private static int CompareClipSegment(SpineVatAnimationClip a, SpineVatAnimationClip b)
	{
		int num = a.startFrame.CompareTo(b.startFrame);
		if (num == 0)
		{
			return a.textureSliceIndex.CompareTo(b.textureSliceIndex);
		}
		return num;
	}
}
