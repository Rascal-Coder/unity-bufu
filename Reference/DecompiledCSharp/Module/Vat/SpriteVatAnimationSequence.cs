using System;
using System.Collections.Generic;
using UnityEngine;

namespace Module.Vat;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public sealed class SpriteVatAnimationSequence : MonoBehaviour
{
	public enum AnimState
	{
		None,
		Walk,
		Attack,
		Hit,
		Die
	}

	public readonly struct BatchRenderData
	{
		public readonly Mesh Mesh;

		public readonly Texture Texture;

		public readonly Vector4 UvRect;

		public readonly bool UseUvRect;

		public readonly Color Color;

		public readonly int SortingOrder;

		public BatchRenderData(Mesh mesh, Texture texture, Vector4 uvRect, bool useUvRect, Color color, int sortingOrder)
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			Mesh = mesh;
			Texture = texture;
			UvRect = uvRect;
			UseUvRect = useUvRect;
			Color = color;
			SortingOrder = sortingOrder;
		}
	}

	private const string DefaultShaderName = "VAT/Sprite Sequence Unlit";

	private static readonly Dictionary<int, Mesh> SpriteMeshCache = new Dictionary<int, Mesh>();

	private static readonly Dictionary<int, Material> SharedMaterialCache = new Dictionary<int, Material>();

	private static readonly int MainTexId = Shader.PropertyToID("_MainTex");

	private static readonly int UvRectId = Shader.PropertyToID("_UvRect");

	private static readonly int UseUvRectId = Shader.PropertyToID("_UseUvRect");

	private static readonly int TintId = Shader.PropertyToID("_Color");

	[Header("Animation")]
	[SerializeField]
	private SpriteVatAsset spriteVatAsset;

	[SerializeField]
	private Sprite[] spriteFrames;

	[Range(0.01f, 1f)]
	[SerializeField]
	private float frameInterval = 0.04f;

	[SerializeField]
	private bool isLooping = true;

	[SerializeField]
	private bool autoPlay = true;

	[SerializeField]
	private Color color = Color.white;

	[Header("Renderer")]
	[SerializeField]
	private Shader vatShader;

	[SerializeField]
	private Material materialOverride;

	[SerializeField]
	private string sortingLayerName;

	[SerializeField]
	private int sortingOrder;

	[SerializeField]
	private bool useRendererSortingOrderByY = true;

	[SerializeField]
	private int rendererSortingOrderRange = 10000;

	[SerializeField]
	private Vector2 depthSortYRange = new Vector2(-600f, 100f);

	[SerializeField]
	private float depthSortPivotOffsetY;

	[Header("State")]
	[SerializeField]
	private bool isPlaying;

	[SerializeField]
	private int currentFrameIndex;

	private MeshFilter meshFilter;

	private MeshRenderer meshRenderer;

	private Mesh runtimeMesh;

	private Material runtimeMaterial;

	private bool ownsRuntimeMaterial;

	private MaterialPropertyBlock propertyBlock;

	private float timer;

	private bool hasInitialized;

	private int startFrame;

	private int endFrame;

	private Action onRangeComplete;

	private AnimState currentAnimState;

	private bool batchRenderingEnabled;

	private bool renderingVisible = true;

	private int lastAppliedSortingOrder = int.MinValue;

	private string lastAppliedSortingLayerName;

	private bool shaderMissingLogged;

	public Action OnAnimationComplete;

	public AnimState CurrentAnimState => currentAnimState;

	public int FrameCount => ActiveFrameCount;

	public bool IsPlaying => isPlaying;

	public bool IsRenderingVisible => renderingVisible;

	public bool UseExternalTick { get; set; }

	public SpriteVatAsset SpriteVatAsset
	{
		get
		{
			return spriteVatAsset;
		}
		set
		{
			if (!((Object)(object)spriteVatAsset == (Object)(object)value))
			{
				spriteVatAsset = value;
				currentFrameIndex = 0;
				RebuildRuntimeFrames();
				ApplyFrame(currentFrameIndex);
			}
		}
	}

	public Sprite[] SpriteFrames
	{
		get
		{
			return spriteFrames;
		}
		set
		{
			if (spriteFrames != value)
			{
				spriteFrames = value;
				currentFrameIndex = 0;
				RebuildRuntimeFrames();
				ApplyFrame(currentFrameIndex);
			}
		}
	}

	public float FrameInterval
	{
		get
		{
			return frameInterval;
		}
		set
		{
			frameInterval = Mathf.Max(0.01f, value);
		}
	}

	public bool IsLooping
	{
		get
		{
			return isLooping;
		}
		set
		{
			isLooping = value;
		}
	}

	public bool AutoPlay
	{
		get
		{
			return autoPlay;
		}
		set
		{
			autoPlay = value;
		}
	}

	public int CurrentFrameIndex
	{
		get
		{
			return currentFrameIndex;
		}
		set
		{
			if (ActiveFrameCount <= 0)
			{
				currentFrameIndex = 0;
				return;
			}
			currentFrameIndex = Mathf.Clamp(value, 0, ActiveFrameCount - 1);
			ApplyFrame(currentFrameIndex);
		}
	}

	public string SortingLayerName
	{
		get
		{
			Initialize();
			if (!((Object)(object)meshRenderer != (Object)null))
			{
				return sortingLayerName;
			}
			return ((Renderer)meshRenderer).sortingLayerName;
		}
		set
		{
			sortingLayerName = value;
			Initialize();
			if ((Object)(object)meshRenderer != (Object)null)
			{
				((Renderer)meshRenderer).sortingLayerName = value;
			}
		}
	}

	public int SortingOrder
	{
		get
		{
			Initialize();
			if (!((Object)(object)meshRenderer != (Object)null))
			{
				return sortingOrder;
			}
			return ((Renderer)meshRenderer).sortingOrder;
		}
		set
		{
			sortingOrder = value;
			Initialize();
			if ((Object)(object)meshRenderer != (Object)null)
			{
				((Renderer)meshRenderer).sortingOrder = value;
			}
		}
	}

	private int ActiveFrameCount
	{
		get
		{
			if ((Object)(object)spriteVatAsset != (Object)null && spriteVatAsset.FrameCount > 0)
			{
				return spriteVatAsset.FrameCount;
			}
			if (spriteFrames == null)
			{
				return 0;
			}
			return spriteFrames.Length;
		}
	}

	private void Awake()
	{
		EnsureRendererReferences();
	}

	private void Start()
	{
		if (autoPlay && ActiveFrameCount > 0)
		{
			Play();
		}
	}

	private void Update()
	{
		if (!UseExternalTick)
		{
			TickExternal(Time.deltaTime);
		}
	}

	private void OnDestroy()
	{
		if ((Object)(object)runtimeMesh != (Object)null)
		{
			Object.Destroy((Object)(object)runtimeMesh);
		}
		if (ownsRuntimeMaterial && (Object)(object)runtimeMaterial != (Object)null)
		{
			Object.Destroy((Object)(object)runtimeMaterial);
		}
	}

	public void PlayRange(int start, int end, bool loop, Action onComplete = null)
	{
		if (ActiveFrameCount <= 0)
		{
			return;
		}
		Initialize();
		int num = Mathf.Clamp(start, 0, ActiveFrameCount - 1);
		int num2 = Mathf.Clamp(end, 0, ActiveFrameCount - 1);
		if (!isPlaying || startFrame != num || endFrame != num2 || isLooping != loop)
		{
			startFrame = Mathf.Min(num, num2);
			endFrame = Mathf.Max(num, num2);
			if (currentFrameIndex < startFrame || currentFrameIndex > endFrame)
			{
				currentFrameIndex = startFrame;
				ApplyFrame(currentFrameIndex);
			}
			isLooping = loop;
			isPlaying = true;
			timer = 0f;
			onRangeComplete = onComplete;
		}
	}

	public void PlayState(AnimState state, bool loop, Action onComplete = null)
	{
		if (ActiveFrameCount > 0 && !(isPlaying && !isLooping && loop) && !(currentAnimState == state && isPlaying && loop))
		{
			currentAnimState = state;
			int start;
			int end;
			switch (state)
			{
			default:
				return;
			case AnimState.Walk:
				start = 0;
				end = 3;
				break;
			case AnimState.Attack:
				start = 4;
				end = 7;
				break;
			case AnimState.Hit:
				start = 8;
				end = 11;
				break;
			case AnimState.Die:
				start = 12;
				end = 15;
				break;
			}
			PlayRange(start, end, loop, onComplete);
		}
	}

	public void Play()
	{
		if (ActiveFrameCount <= 0)
		{
			Debug.LogWarning((object)"[SpriteVatAnimationSequence] No frames to play.", (Object)(object)this);
			return;
		}
		Initialize();
		startFrame = 0;
		endFrame = ActiveFrameCount - 1;
		onRangeComplete = null;
		isPlaying = true;
	}

	public void Pause()
	{
		isPlaying = false;
	}

	public void Stop()
	{
		isPlaying = false;
		currentFrameIndex = 0;
		timer = 0f;
		ApplyFrame(currentFrameIndex);
	}

	public void Restart()
	{
		Stop();
		Play();
	}

	public void JumpToFrame(int frameIndex)
	{
		CurrentFrameIndex = frameIndex;
		timer = 0f;
	}

	public void ShowStaticFrame(int frameIndex)
	{
		Initialize();
		isPlaying = false;
		CurrentFrameIndex = frameIndex;
		timer = 0f;
	}

	public void ShowFrontFace()
	{
		if (FrameCount > 0)
		{
			ShowStaticFrame(FrameCount - 1);
		}
	}

	public void ShowBackFace()
	{
		ShowStaticFrame(0);
	}

	public void SetSpeed(float speedMultiplier)
	{
		if (speedMultiplier > 0f)
		{
			FrameInterval = frameInterval / speedMultiplier;
		}
		else
		{
			Debug.LogWarning((object)"[SpriteVatAnimationSequence] Speed multiplier must be greater than 0.", (Object)(object)this);
		}
	}

	public void SetSortingLayerName(string layerName)
	{
		SortingLayerName = layerName;
	}

	public void SetSortingOrder(int order)
	{
		SortingOrder = order;
	}

	public void SetSortingLayerAndOrder(string layerName, int order)
	{
		SortingLayerName = layerName;
		SortingOrder = order;
	}

	public void SetDepthSortRange(float minY, float maxY)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		depthSortYRange = ((minY <= maxY) ? new Vector2(minY, maxY) : new Vector2(maxY, minY));
		ApplyRendererSortingSettings();
	}

	public void SetSpriteVatAsset(SpriteVatAsset asset)
	{
		SpriteVatAsset = asset;
	}

	public void SetVatShader(Shader shader)
	{
		if (!((Object)(object)shader == (Object)null) && !((Object)(object)vatShader == (Object)(object)shader))
		{
			vatShader = shader;
			shaderMissingLogged = false;
			if ((Object)(object)runtimeMaterial != (Object)null)
			{
				runtimeMaterial = null;
			}
			hasInitialized = false;
			((Behaviour)this).enabled = true;
			Initialize();
		}
	}

	public void SetTint(Color tint)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		color = tint;
		ApplyFrame(currentFrameIndex);
	}

	public void SetBatchRenderingEnabled(bool enabled)
	{
		batchRenderingEnabled = enabled;
		Initialize();
		if ((Object)(object)meshRenderer != (Object)null)
		{
			((Renderer)meshRenderer).enabled = renderingVisible && !enabled;
		}
	}

	public void SetRenderingVisible(bool visible)
	{
		renderingVisible = visible;
		Initialize();
		if ((Object)(object)meshRenderer != (Object)null)
		{
			((Renderer)meshRenderer).enabled = renderingVisible && !batchRenderingEnabled;
		}
	}

	public bool TryGetBatchRenderData(out BatchRenderData data)
	{
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		data = default(BatchRenderData);
		if (!renderingVisible)
		{
			return false;
		}
		Initialize();
		if (ActiveFrameCount <= 0 || (Object)(object)meshFilter == (Object)null)
		{
			return false;
		}
		SpriteVatFrame frame = GetFrame(currentFrameIndex);
		if ((Object)(object)frame.texture == (Object)null)
		{
			return false;
		}
		if ((Object)(object)meshFilter.sharedMesh == (Object)null)
		{
			ApplyFrame(currentFrameIndex);
		}
		Mesh sharedMesh = meshFilter.sharedMesh;
		if ((Object)(object)sharedMesh == (Object)null)
		{
			return false;
		}
		bool useUvRect = frame.useUvRect || (Object)(object)frame.sprite == (Object)null;
		data = new BatchRenderData(sharedMesh, (Texture)(object)frame.texture, frame.uvRect, useUvRect, color, GetCurrentRendererSortingOrder());
		return true;
	}

	public void TickExternal(float deltaTime)
	{
		if (isPlaying && ActiveFrameCount > 0)
		{
			UpdateAnimation(deltaTime);
		}
		if (!batchRenderingEnabled)
		{
			ApplyRendererSortingSettings();
		}
	}

	private void Initialize()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Expected O, but got Unknown
		if (hasInitialized)
		{
			return;
		}
		EnsureRendererReferences();
		if ((Object)(object)vatShader == (Object)null)
		{
			vatShader = Shader.Find("VAT/Sprite Sequence Unlit");
		}
		if ((Object)(object)runtimeMesh == (Object)null)
		{
			runtimeMesh = new Mesh
			{
				name = "SpriteVatSequence_RuntimeMesh"
			};
			meshFilter.sharedMesh = runtimeMesh;
		}
		Material sourceMaterial = (((Object)(object)materialOverride != (Object)null) ? materialOverride : (((Object)(object)spriteVatAsset != (Object)null) ? spriteVatAsset.sharedMaterial : null));
		if ((Object)(object)vatShader == (Object)null)
		{
			if (!shaderMissingLogged)
			{
				shaderMissingLogged = true;
				Debug.LogWarning((object)"[SpriteVatAnimationSequence] Shader is not ready yet: VAT/Sprite Sequence Unlit", (Object)(object)this);
			}
			((Behaviour)this).enabled = false;
			return;
		}
		runtimeMaterial = GetOrCreateSharedMaterial(sourceMaterial, vatShader);
		ownsRuntimeMaterial = false;
		((Renderer)meshRenderer).sharedMaterial = runtimeMaterial;
		((Renderer)meshRenderer).enabled = renderingVisible && !batchRenderingEnabled;
		if (!string.IsNullOrEmpty(sortingLayerName))
		{
			((Renderer)meshRenderer).sortingLayerName = sortingLayerName;
		}
		ApplyRendererSortingSettings();
		RebuildRuntimeFrames();
		ApplyFrame(currentFrameIndex);
		hasInitialized = true;
	}

	private void EnsureRendererReferences()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Expected O, but got Unknown
		if ((Object)(object)meshFilter == (Object)null)
		{
			meshFilter = ((Component)this).GetComponent<MeshFilter>();
		}
		if ((Object)(object)meshRenderer == (Object)null)
		{
			meshRenderer = ((Component)this).GetComponent<MeshRenderer>();
		}
		if (propertyBlock == null)
		{
			propertyBlock = new MaterialPropertyBlock();
		}
	}

	private void ApplyRendererSortingSettings()
	{
		if (!((Object)(object)meshRenderer == (Object)null))
		{
			if (!string.IsNullOrEmpty(sortingLayerName) && lastAppliedSortingLayerName != sortingLayerName)
			{
				((Renderer)meshRenderer).sortingLayerName = sortingLayerName;
				lastAppliedSortingLayerName = sortingLayerName;
			}
			int currentRendererSortingOrder = GetCurrentRendererSortingOrder();
			if (lastAppliedSortingOrder != currentRendererSortingOrder)
			{
				((Renderer)meshRenderer).sortingOrder = currentRendererSortingOrder;
				lastAppliedSortingOrder = currentRendererSortingOrder;
			}
		}
	}

	private int GetCurrentRendererSortingOrder()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if (!useRendererSortingOrderByY || rendererSortingOrderRange <= 0)
		{
			return sortingOrder;
		}
		float num = Mathf.InverseLerp(depthSortYRange.x, depthSortYRange.y, ((Component)this).transform.position.y + depthSortPivotOffsetY);
		int num2 = Mathf.RoundToInt((1f - num) * (float)rendererSortingOrderRange);
		return Mathf.Clamp(sortingOrder + num2, -32768, 32767);
	}

	private void RebuildRuntimeFrames()
	{
		if ((Object)(object)spriteVatAsset != (Object)null && spriteVatAsset.spriteFrames != null && spriteVatAsset.spriteFrames.Length != 0)
		{
			spriteVatAsset.RebuildFromSprites();
		}
	}

	private void UpdateAnimation(float deltaTime)
	{
		timer += deltaTime;
		if (timer < frameInterval)
		{
			return;
		}
		timer = 0f;
		currentFrameIndex++;
		int num = ((endFrame > startFrame && endFrame < ActiveFrameCount) ? endFrame : (ActiveFrameCount - 1));
		int num2 = ((startFrame >= 0 && startFrame < ActiveFrameCount) ? startFrame : 0);
		if (currentFrameIndex > num)
		{
			if (!isLooping)
			{
				currentFrameIndex = num;
				isPlaying = false;
				ApplyFrame(currentFrameIndex);
				Action action = onRangeComplete;
				onRangeComplete = null;
				if (currentAnimState == AnimState.Hit || currentAnimState == AnimState.Attack || currentAnimState == AnimState.Die)
				{
					currentAnimState = AnimState.None;
				}
				action?.Invoke();
				OnAnimationComplete?.Invoke();
				return;
			}
			currentFrameIndex = num2;
		}
		ApplyFrame(currentFrameIndex);
	}

	private void ApplyFrame(int frameIndex)
	{
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		if (ActiveFrameCount <= 0 || (Object)(object)meshRenderer == (Object)null)
		{
			return;
		}
		SpriteVatFrame frame = GetFrame(frameIndex);
		if (!((Object)(object)frame.texture == (Object)null))
		{
			bool flag = frame.useUvRect || (Object)(object)frame.sprite == (Object)null;
			if ((Object)(object)frame.sprite != (Object)null && !flag)
			{
				meshFilter.sharedMesh = GetCachedSpriteMesh(frame.sprite);
			}
			else
			{
				ApplyQuadMesh(frame);
			}
			if (!batchRenderingEnabled)
			{
				((Renderer)meshRenderer).GetPropertyBlock(propertyBlock);
				propertyBlock.SetTexture(MainTexId, (Texture)(object)frame.texture);
				propertyBlock.SetVector(UvRectId, frame.uvRect);
				propertyBlock.SetFloat(UseUvRectId, flag ? 1f : 0f);
				propertyBlock.SetColor(TintId, color);
				((Renderer)meshRenderer).SetPropertyBlock(propertyBlock);
			}
		}
	}

	private SpriteVatFrame GetFrame(int frameIndex)
	{
		if ((Object)(object)spriteVatAsset != (Object)null && spriteVatAsset.FrameCount > 0)
		{
			return spriteVatAsset.GetFrame(frameIndex);
		}
		if (spriteFrames == null || spriteFrames.Length == 0)
		{
			return default(SpriteVatFrame);
		}
		return SpriteVatAsset.BuildFrame(spriteFrames[Mathf.Clamp(frameIndex, 0, spriteFrames.Length - 1)]);
	}

	private void ApplySpriteMesh(Sprite sprite)
	{
		meshFilter.sharedMesh = GetCachedSpriteMesh(sprite);
	}

	private static Mesh GetCachedSpriteMesh(Sprite sprite)
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Expected O, but got Unknown
		if ((Object)(object)sprite == (Object)null)
		{
			return null;
		}
		int instanceID = ((Object)sprite).GetInstanceID();
		if (SpriteMeshCache.TryGetValue(instanceID, out var value) && (Object)(object)value != (Object)null)
		{
			return value;
		}
		Vector2[] vertices = sprite.vertices;
		ushort[] triangles = sprite.triangles;
		Vector2[] uv = sprite.uv;
		if (vertices == null || vertices.Length == 0 || triangles == null || triangles.Length == 0)
		{
			return null;
		}
		Vector3[] array = (Vector3[])(object)new Vector3[vertices.Length];
		for (int i = 0; i < vertices.Length; i++)
		{
			array[i] = Vector2.op_Implicit(vertices[i]);
		}
		int[] array2 = new int[triangles.Length];
		for (int j = 0; j < triangles.Length; j++)
		{
			array2[j] = triangles[j];
		}
		value = new Mesh
		{
			name = ((Object)sprite).name + "_SpriteVatMesh"
		};
		value.vertices = array;
		value.uv = uv;
		value.triangles = array2;
		value.RecalculateBounds();
		SpriteMeshCache[instanceID] = value;
		return value;
	}

	private static Material GetOrCreateSharedMaterial(Material sourceMaterial, Shader shader)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		if ((Object)(object)shader == (Object)null)
		{
			return null;
		}
		int materialCacheKey = GetMaterialCacheKey(sourceMaterial, shader);
		if (SharedMaterialCache.TryGetValue(materialCacheKey, out var value) && (Object)(object)value != (Object)null)
		{
			return value;
		}
		value = (((Object)(object)sourceMaterial != (Object)null) ? new Material(sourceMaterial) : new Material(shader));
		value.shader = shader;
		value.enableInstancing = true;
		((Object)value).name = (((Object)(object)sourceMaterial != (Object)null) ? (((Object)sourceMaterial).name + "_" + ((Object)shader).name + "_SpriteVatShared") : (((Object)shader).name + "_SpriteVatShared"));
		SharedMaterialCache[materialCacheKey] = value;
		return value;
	}

	private static int GetMaterialCacheKey(Material sourceMaterial, Shader shader)
	{
		return ((((Object)(object)shader != (Object)null) ? ((Object)shader).GetInstanceID() : 0) * 397) ^ (((Object)(object)sourceMaterial != (Object)null) ? ((Object)sourceMaterial).GetInstanceID() : 0);
	}

	private void ApplyQuadMesh(SpriteVatFrame frame)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		Vector2 size = frame.size;
		Vector2 pivot = frame.pivot;
		float num = (0f - size.x) * pivot.x;
		float num2 = size.x * (1f - pivot.x);
		float num3 = (0f - size.y) * pivot.y;
		float num4 = size.y * (1f - pivot.y);
		runtimeMesh.Clear();
		runtimeMesh.vertices = (Vector3[])(object)new Vector3[4]
		{
			new Vector3(num, num3, 0f),
			new Vector3(num, num4, 0f),
			new Vector3(num2, num4, 0f),
			new Vector3(num2, num3, 0f)
		};
		runtimeMesh.uv = (Vector2[])(object)new Vector2[4]
		{
			new Vector2(0f, 0f),
			new Vector2(0f, 1f),
			new Vector2(1f, 1f),
			new Vector2(1f, 0f)
		};
		runtimeMesh.triangles = new int[6] { 0, 1, 2, 0, 2, 3 };
		runtimeMesh.RecalculateBounds();
	}
}
