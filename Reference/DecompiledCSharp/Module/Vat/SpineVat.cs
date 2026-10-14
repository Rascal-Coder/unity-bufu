using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Module.Vat;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public sealed class SpineVat : MonoBehaviour
{
	public readonly struct IndirectRenderData
	{
		public readonly SpineVatAsset Asset;

		public readonly SpineVatAnimationClip Segment;

		public readonly Vector3 Position;

		public readonly float ScaleX;

		public readonly float ScaleY;

		public readonly float SortY;

		public readonly int Frame;

		public readonly int NextFrame;

		public readonly float Blend;

		public IndirectRenderData(SpineVatAsset asset, SpineVatAnimationClip segment, Vector3 position, float scaleX, float scaleY, float sortY, int frame, int nextFrame, float blend)
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			Asset = asset;
			Segment = segment;
			Position = position;
			ScaleX = scaleX;
			ScaleY = scaleY;
			SortY = sortY;
			Frame = frame;
			NextFrame = nextFrame;
			Blend = blend;
		}
	}

	private const string DefaultShaderName = "Spine/VAT Unlit";

	private static readonly Dictionary<int, Mesh> SharedRuntimeMeshCache = new Dictionary<int, Mesh>();

	private static readonly Dictionary<int, Material> SharedRuntimeMaterialCache = new Dictionary<int, Material>();

	private static readonly int PositionTexId = Shader.PropertyToID("_PositionTex");

	private static readonly int VatFrameId = Shader.PropertyToID("_VatFrame");

	private static readonly int VatNextFrameId = Shader.PropertyToID("_VatNextFrame");

	private static readonly int VatFrameBlendId = Shader.PropertyToID("_VatFrameBlend");

	private static readonly int VatSliceId = Shader.PropertyToID("_VatSlice");

	private static readonly int VatFrameCountId = Shader.PropertyToID("_VatFrameCount");

	private static readonly int VatVertexCountId = Shader.PropertyToID("_VatVertexCount");

	private static readonly int VatBoundsMinId = Shader.PropertyToID("_VatBoundsMin");

	private static readonly int VatBoundsSizeId = Shader.PropertyToID("_VatBoundsSize");

	private static readonly int StraightAlphaInputId = Shader.PropertyToID("_StraightAlphaInput");

	private static readonly int AlphaTexId = Shader.PropertyToID("_AlphaTex");

	private static readonly int EnableExternalAlphaId = Shader.PropertyToID("_EnableExternalAlpha");

	private static readonly int ColorId = Shader.PropertyToID("_Color");

	private static readonly int ZWriteId = Shader.PropertyToID("_ZWrite");

	private static readonly int CutoffId = Shader.PropertyToID("_Cutoff");

	private static readonly int UseYDepthSortId = Shader.PropertyToID("_UseYDepthSort");

	private static readonly int SortAxisMinMaxId = Shader.PropertyToID("_SortAxisMinMax");

	private static readonly int SortDepthRangeId = Shader.PropertyToID("_SortDepthRange");

	private static readonly int SortPivotYId = Shader.PropertyToID("_SortPivotY");

	private static readonly int MeshVertexRgbNeedsVatAlphaId = Shader.PropertyToID("_MeshVertexRgbNeedsVatAlpha");

	private const string StraightAlphaKeyword = "_STRAIGHT_ALPHA_INPUT";

	private const string ExternalAlphaKeyword = "ETC1_EXTERNAL_ALPHA";

	[Header("VAT")]
	[SerializeField]
	private SpineVatAsset vatAsset;

	[SerializeField]
	private Shader vatShader;

	[SerializeField]
	private Material materialOverride;

	[Header("Animation")]
	[SerializeField]
	private string startAnimationName = "idle_front";

	[SerializeField]
	private bool playOnAwake = true;

	[SerializeField]
	private bool loop = true;

	[SerializeField]
	private float timeScale = 1f;

	[Header("Renderer Sorting")]
	[SerializeField]
	private string sortingLayerName;

	[SerializeField]
	private int sortingOrder;

	[SerializeField]
	private bool useRendererSortingOrderByY = true;

	[SerializeField]
	private int rendererSortingOrderRange = 10000;

	[SerializeField]
	private bool useYDepthSort = true;

	[SerializeField]
	private Vector2 depthSortYRange = new Vector2(-600f, 100f);

	[SerializeField]
	private Vector2 depthSortZRange = new Vector2(-1f, 1f);

	[SerializeField]
	private float depthSortPivotOffsetY;

	[SerializeField]
	private bool writeDepthForYSort = true;

	[SerializeField]
	[Range(0f, 0.1f)]
	private float alphaClipThreshold = 0.001f;

	[Header("Optional Roaming")]
	[SerializeField]
	private bool randomRoam;

	[SerializeField]
	private string idleAnimationName = "idle_front";

	[SerializeField]
	private string walkAnimationName = "walk_front";

	[SerializeField]
	private Vector2 boundaryMin = new Vector2(-400f, -600f);

	[SerializeField]
	private Vector2 boundaryMax = new Vector2(400f, 100f);

	[SerializeField]
	private float moveSpeed = 180f;

	[SerializeField]
	private float arriveDistance = 12f;

	[SerializeField]
	private Vector2 idleDurationRange = new Vector2(0.4f, 1.2f);

	private MeshFilter meshFilter;

	private MeshRenderer meshRenderer;

	private Mesh runtimeMesh;

	private Material[] runtimeMaterials;

	private MaterialPropertyBlock propertyBlock;

	private SpineVatAnimationClip currentClip;

	private readonly List<SpineVatAnimationClip> currentAnimationSegments = new List<SpineVatAnimationClip>();

	private Mesh currentSourceMesh;

	private int currentRuntimeMeshKey;

	private Material[] currentSourceMaterials;

	private float animationTime;

	private Vector3 targetPosition;

	private float idleTimer;

	private bool moving;

	private bool initialized;

	private bool isPlaying;

	private bool animationCompleteInvoked;

	private bool indirectRenderingEnabled;

	private bool renderingVisible = true;

	private bool hasResolvedFrameData;

	private int resolvedFrame;

	private int resolvedNextFrame;

	private float resolvedFrameBlend;

	private SpineVatAnimationClip resolvedFrameClip;

	private Action onAnimationCompleteOnce;

	private int lastAppliedSortingOrder = int.MinValue;

	private string lastAppliedSortingLayerName;

	public Action OnAnimationComplete;

	public SpineVatAsset VatAsset => vatAsset;

	public string CurrentAnimationName
	{
		get
		{
			if (currentClip == null)
			{
				return string.Empty;
			}
			return currentClip.animationName;
		}
	}

	public bool IsReady
	{
		get
		{
			if (initialized && (Object)(object)vatAsset != (Object)null)
			{
				return currentClip != null;
			}
			return false;
		}
	}

	public bool IsPlaying => isPlaying;

	public bool IsRenderingVisible => renderingVisible;

	public bool UseExternalTick { get; set; }

	public string SortingLayerName
	{
		get
		{
			EnsureRendererReferences();
			if (!((Object)(object)meshRenderer != (Object)null))
			{
				return sortingLayerName;
			}
			return ((Renderer)meshRenderer).sortingLayerName;
		}
		set
		{
			sortingLayerName = value;
			ApplyRendererSortingSettings();
		}
	}

	public int SortingOrder
	{
		get
		{
			EnsureRendererReferences();
			if (!((Object)(object)meshRenderer != (Object)null))
			{
				return sortingOrder;
			}
			return ((Renderer)meshRenderer).sortingOrder;
		}
		set
		{
			sortingOrder = value;
			ApplyRendererSortingSettings();
		}
	}

	private void Awake()
	{
		EnsureRendererReferences();
		if ((Object)(object)vatShader == (Object)null)
		{
			vatShader = Shader.Find("Spine/VAT Unlit");
		}
		if ((Object)(object)vatAsset != (Object)null)
		{
			InitializeRenderer();
		}
		if (playOnAwake && (Object)(object)vatAsset != (Object)null)
		{
			Play(startAnimationName, loop);
		}
	}

	private void OnEnable()
	{
		PickNewTarget();
	}

	private void Update()
	{
		if (!UseExternalTick)
		{
			TickExternal(Time.deltaTime);
		}
	}

	private void OnValidate()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if (depthSortYRange.y < depthSortYRange.x)
		{
			depthSortYRange = new Vector2(depthSortYRange.y, depthSortYRange.x);
		}
		alphaClipThreshold = Mathf.Max(0f, alphaClipThreshold);
		if (Application.isPlaying)
		{
			EnsureRendererReferences();
			ApplyRendererSortingSettings();
			ApplyRuntimeMaterialSettings();
			ApplyDynamicDepthSortProperties();
			if ((Object)(object)runtimeMesh != (Object)null)
			{
				RebuildRuntimeMesh();
			}
		}
	}

	private void OnDestroy()
	{
		DestroyRuntimeMaterials();
	}

	public void Initialize(SpineVatAsset asset, Vector2 min, Vector2 max, bool enableRandomRoam)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		ClearCurrentPlayback();
		vatAsset = asset;
		boundaryMin = min;
		boundaryMax = max;
		depthSortYRange = new Vector2(min.y, max.y);
		randomRoam = enableRandomRoam;
		InitializeRenderer();
		Play(startAnimationName, shouldLoop: true, Random.value);
		PickNewTarget();
	}

	public void SetVatAsset(SpineVatAsset asset)
	{
		ClearCurrentPlayback();
		vatAsset = asset;
		InitializeRenderer();
		Play(startAnimationName, loop);
	}

	public void SetVatAssetNoPlay(SpineVatAsset asset)
	{
		ClearCurrentPlayback();
		vatAsset = asset;
		InitializeRenderer();
	}

	public void SetVatShader(Shader shader)
	{
		if ((Object)(object)shader == (Object)null || (Object)(object)vatShader == (Object)(object)shader)
		{
			return;
		}
		vatShader = shader;
		if (runtimeMaterials == null)
		{
			if ((Object)(object)vatAsset != (Object)null && !initialized)
			{
				InitializeRenderer();
			}
		}
		else
		{
			runtimeMaterials = null;
			currentSourceMaterials = null;
			RebuildRuntimeMaterial();
			ApplyRuntimeMaterialSettings();
		}
	}

	public bool Play(string animationName, bool shouldLoop, float normalizedStartTime = 0f, Action onComplete = null)
	{
		if ((Object)(object)vatAsset == (Object)null)
		{
			Debug.LogWarning((object)"[SpineVat] Cannot play animation because VatAsset is missing.", (Object)(object)this);
			return false;
		}
		if (vatAsset.GetAnimationSegmentsOrFirst(animationName, currentAnimationSegments) == 0)
		{
			Debug.LogWarning((object)("[SpineVat] VatAsset has no animations: " + ((Object)vatAsset).name), (Object)(object)this);
			return false;
		}
		SpineVatAnimationClip spineVatAnimationClip = currentAnimationSegments[0];
		currentClip = spineVatAnimationClip;
		loop = shouldLoop;
		isPlaying = true;
		animationTime = Mathf.Max(0f, normalizedStartTime) * Mathf.Max(0.0001f, currentClip.durationSeconds);
		animationCompleteInvoked = false;
		onAnimationCompleteOnce = onComplete;
		if (initialized)
		{
			ApplyClipRendererData(currentClip);
		}
		ApplyAnimationFrame();
		return true;
	}

	public void Pause()
	{
		isPlaying = false;
		onAnimationCompleteOnce = null;
	}

	public bool FreezeAt(string animationName, float normalizedTime)
	{
		if ((Object)(object)vatAsset == (Object)null || string.IsNullOrEmpty(animationName))
		{
			return false;
		}
		currentAnimationSegments.Clear();
		if (vatAsset.animations != null)
		{
			for (int i = 0; i < vatAsset.animations.Count; i++)
			{
				SpineVatAnimationClip spineVatAnimationClip = vatAsset.animations[i];
				if (spineVatAnimationClip != null && spineVatAnimationClip.animationName == animationName)
				{
					currentAnimationSegments.Add(spineVatAnimationClip);
				}
			}
		}
		if (currentAnimationSegments.Count == 0)
		{
			return false;
		}
		currentClip = currentAnimationSegments[0];
		loop = false;
		isPlaying = false;
		float num = Mathf.Max(0.0001f, currentClip.durationSeconds);
		animationTime = Mathf.Clamp01(normalizedTime) * num;
		if (normalizedTime <= 0f)
		{
			animationTime = 0f;
		}
		else if (normalizedTime >= 1f)
		{
			animationTime = num;
		}
		animationCompleteInvoked = true;
		onAnimationCompleteOnce = null;
		if (initialized)
		{
			ApplyClipRendererData(currentClip);
		}
		ApplyAnimationFrame();
		return true;
	}

	public bool PlayRandom(bool shouldLoop = true, Action onComplete = null)
	{
		if ((Object)(object)vatAsset == (Object)null || vatAsset.animations == null || vatAsset.animations.Count == 0)
		{
			return false;
		}
		string text = PickRandomAnimationName();
		if (!string.IsNullOrEmpty(text))
		{
			return Play(text, shouldLoop, Random.value, onComplete);
		}
		return false;
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
		ApplyDynamicDepthSortProperties();
	}

	public void SetDepthSortZRange(float minZ, float maxZ)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		depthSortZRange = new Vector2(minZ, maxZ);
		ApplyDynamicDepthSortProperties();
	}

	public void SetDepthSortEnabled(bool enabled)
	{
		useYDepthSort = enabled;
		ApplyRuntimeMaterialSettings();
		ApplyDynamicDepthSortProperties();
		if ((Object)(object)runtimeMesh != (Object)null)
		{
			RebuildRuntimeMesh();
		}
	}

	public void SetRendererSortingOrderByYEnabled(bool enabled)
	{
		useRendererSortingOrderByY = enabled;
		ApplyRendererSortingSettings();
	}

	public void SetIndirectRenderingEnabled(bool enabled)
	{
		indirectRenderingEnabled = enabled;
		EnsureRendererReferences();
		if ((Object)(object)meshRenderer != (Object)null)
		{
			((Renderer)meshRenderer).enabled = renderingVisible && !enabled;
		}
	}

	public void SetRenderingVisible(bool visible)
	{
		renderingVisible = visible;
		EnsureRendererReferences();
		if ((Object)(object)meshRenderer != (Object)null)
		{
			((Renderer)meshRenderer).enabled = renderingVisible && !indirectRenderingEnabled;
		}
	}

	public bool TryGetIndirectRenderData(out IndirectRenderData data)
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		data = default(IndirectRenderData);
		if (!indirectRenderingEnabled || !renderingVisible || !IsReady)
		{
			return false;
		}
		if (!hasResolvedFrameData)
		{
			ApplyAnimationFrame();
		}
		SpineVatAnimationClip spineVatAnimationClip = ((resolvedFrameClip != null) ? resolvedFrameClip : currentClip);
		if (!hasResolvedFrameData || spineVatAnimationClip == null || (Object)(object)vatAsset == (Object)null)
		{
			return false;
		}
		Vector3 lossyScale = ((Component)this).transform.lossyScale;
		data = new IndirectRenderData(vatAsset, spineVatAnimationClip, ((Component)this).transform.position, lossyScale.x, lossyScale.y, GetSortPivotY(), resolvedFrame, resolvedNextFrame, resolvedFrameBlend);
		return true;
	}

	public void TickExternal(float deltaTime)
	{
		if (IsReady)
		{
			TickAnimation(deltaTime);
			if (randomRoam)
			{
				TickRoaming(deltaTime);
			}
			ApplyDynamicDepthSortProperties();
		}
	}

	private void InitializeRenderer()
	{
		if ((Object)(object)vatAsset == (Object)null)
		{
			initialized = false;
			return;
		}
		if ((Object)(object)vatAsset.bakedMesh == (Object)null || (Object)(object)vatAsset.positionTextureArray == (Object)null)
		{
			Debug.LogError((object)("[SpineVat] VatAsset is missing mesh or position texture: " + ((Object)vatAsset).name), (Object)(object)this);
			initialized = false;
			return;
		}
		if ((Object)(object)vatShader == (Object)null)
		{
			vatShader = Shader.Find("Spine/VAT Unlit");
		}
		if ((Object)(object)vatShader == (Object)null)
		{
			Debug.LogError((object)"[SpineVat] Shader not found: Spine/VAT Unlit", (Object)(object)this);
			initialized = false;
			return;
		}
		RebuildRuntimeMesh();
		RebuildRuntimeMaterial();
		ApplyStaticMaterialProperties();
		ApplyRendererSortingSettings();
		ApplyRuntimeMaterialSettings();
		ApplyDynamicDepthSortProperties();
		initialized = true;
	}

	private void RebuildRuntimeMesh()
	{
		Mesh val = GetCurrentSourceMesh();
		int sharedRuntimeMeshKey = GetSharedRuntimeMeshKey(val);
		if (!((Object)(object)val == (Object)(object)currentSourceMesh) || currentRuntimeMeshKey != sharedRuntimeMeshKey || !((Object)(object)runtimeMesh != (Object)null))
		{
			currentSourceMesh = val;
			currentRuntimeMeshKey = sharedRuntimeMeshKey;
			runtimeMesh = GetOrCreateSharedRuntimeMesh(val);
			meshFilter.sharedMesh = runtimeMesh;
		}
	}

	private void RebuildRuntimeMaterial()
	{
		Material[] array = GetCurrentSourceMaterials();
		int num = Mathf.Max(1, (!((Object)(object)runtimeMesh != (Object)null)) ? 1 : runtimeMesh.subMeshCount);
		if (runtimeMaterials == null || runtimeMaterials.Length != num || !MaterialsEquivalent(array, currentSourceMaterials, num))
		{
			currentSourceMaterials = array;
			runtimeMaterials = (Material[])(object)new Material[num];
			for (int i = 0; i < runtimeMaterials.Length; i++)
			{
				Material sourceMaterial = (((Object)(object)materialOverride != (Object)null) ? materialOverride : ((array != null && i < array.Length) ? array[i] : null));
				runtimeMaterials[i] = GetOrCreateSharedRuntimeMaterial(sourceMaterial, vatShader, vatAsset, i);
			}
			((Renderer)meshRenderer).sharedMaterials = runtimeMaterials;
			ApplyRuntimeMaterialSettings();
		}
	}

	private void ApplyStaticMaterialProperties()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		ConfigurePositionTexture((Texture)(object)vatAsset.positionTextureArray);
		((Renderer)meshRenderer).GetPropertyBlock(propertyBlock);
		propertyBlock.SetTexture(PositionTexId, (Texture)(object)vatAsset.positionTextureArray);
		propertyBlock.SetFloat(VatFrameCountId, (float)Mathf.Max(1, vatAsset.maxFrameCount));
		propertyBlock.SetFloat(VatVertexCountId, (float)Mathf.Max(1, vatAsset.vertexCount));
		propertyBlock.SetVector(VatBoundsMinId, Vector4.op_Implicit(vatAsset.boundsMin));
		propertyBlock.SetVector(VatBoundsSizeId, Vector4.op_Implicit(vatAsset.boundsMax - vatAsset.boundsMin));
		propertyBlock.SetFloat(MeshVertexRgbNeedsVatAlphaId, vatAsset.meshVertexRgbNeedsVatAlpha ? 1f : 0f);
		((Renderer)meshRenderer).SetPropertyBlock(propertyBlock);
	}

	private static void ConfigurePositionTexture(Texture texture)
	{
		if (!((Object)(object)texture == (Object)null))
		{
			texture.wrapMode = (TextureWrapMode)1;
			texture.filterMode = (FilterMode)0;
		}
	}

	private void ApplyDynamicDepthSortProperties()
	{
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)meshRenderer == (Object)null) && propertyBlock != null)
		{
			float x = depthSortYRange.x;
			float num = (Mathf.Approximately(depthSortYRange.x, depthSortYRange.y) ? (depthSortYRange.x + 0.001f) : depthSortYRange.y);
			((Renderer)meshRenderer).GetPropertyBlock(propertyBlock);
			propertyBlock.SetFloat(UseYDepthSortId, useYDepthSort ? 1f : 0f);
			propertyBlock.SetVector(SortAxisMinMaxId, new Vector4(x, num, 0f, 0f));
			propertyBlock.SetVector(SortDepthRangeId, new Vector4(depthSortZRange.x, depthSortZRange.y, 0f, 0f));
			propertyBlock.SetFloat(SortPivotYId, GetSortPivotY());
			((Renderer)meshRenderer).SetPropertyBlock(propertyBlock);
			ApplyRendererSortingSettings();
		}
	}

	private void ApplyRendererSortingSettings()
	{
		EnsureRendererReferences();
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
		if (!useRendererSortingOrderByY || rendererSortingOrderRange <= 0)
		{
			return sortingOrder;
		}
		float num = Mathf.InverseLerp(depthSortYRange.x, depthSortYRange.y, GetSortPivotY());
		int num2 = Mathf.RoundToInt((1f - num) * (float)rendererSortingOrderRange);
		return Mathf.Clamp(sortingOrder + num2, -32768, 32767);
	}

	private float GetSortPivotY()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return ((Component)this).transform.position.y + depthSortPivotOffsetY;
	}

	private void ApplyRuntimeMaterialSettings()
	{
		if (runtimeMaterials == null)
		{
			return;
		}
		float num = ((useYDepthSort && writeDepthForYSort) ? 1f : 0f);
		float num2 = (((Object)(object)vatAsset != (Object)null && vatAsset.meshVertexRgbNeedsVatAlpha) ? 1f : 0f);
		for (int i = 0; i < runtimeMaterials.Length; i++)
		{
			Material val = runtimeMaterials[i];
			if (!((Object)(object)val == (Object)null))
			{
				if (val.HasProperty(ZWriteId))
				{
					val.SetFloat(ZWriteId, num);
				}
				if (val.HasProperty(CutoffId))
				{
					val.SetFloat(CutoffId, alphaClipThreshold);
				}
				if (val.HasProperty(MeshVertexRgbNeedsVatAlphaId))
				{
					val.SetFloat(MeshVertexRgbNeedsVatAlphaId, num2);
				}
			}
		}
	}

	private void TickAnimation(float deltaTime)
	{
		if (isPlaying)
		{
			animationTime += deltaTime * Mathf.Max(0f, timeScale);
			if (!loop && currentClip.durationSeconds > 0f)
			{
				animationTime = Mathf.Min(animationTime, currentClip.durationSeconds);
			}
			ApplyAnimationFrame();
			if (!loop && !animationCompleteInvoked && currentClip.durationSeconds > 0f && animationTime >= currentClip.durationSeconds)
			{
				animationCompleteInvoked = true;
				isPlaying = false;
				Action action = onAnimationCompleteOnce;
				onAnimationCompleteOnce = null;
				action?.Invoke();
				OnAnimationComplete?.Invoke();
			}
		}
	}

	private void ApplyAnimationFrame()
	{
		if (currentClip != null && !((Object)(object)vatAsset == (Object)null))
		{
			ResolveCurrentFrameData(out var clipForFrame, out var frame, out var nextFrame, out var blend);
			if (clipForFrame != null && clipForFrame != currentClip)
			{
				currentClip = clipForFrame;
				ApplyClipRendererData(currentClip);
			}
			resolvedFrameClip = ((clipForFrame != null) ? clipForFrame : currentClip);
			resolvedFrame = frame;
			resolvedNextFrame = nextFrame;
			resolvedFrameBlend = blend;
			hasResolvedFrameData = resolvedFrameClip != null;
			if (!indirectRenderingEnabled)
			{
				((Renderer)meshRenderer).GetPropertyBlock(propertyBlock);
				propertyBlock.SetFloat(VatFrameId, (float)frame);
				propertyBlock.SetFloat(VatNextFrameId, (float)nextFrame);
				propertyBlock.SetFloat(VatFrameBlendId, blend);
				propertyBlock.SetFloat(VatSliceId, (float)currentClip.textureSliceIndex);
				((Renderer)meshRenderer).SetPropertyBlock(propertyBlock);
			}
		}
	}

	private void ResolveCurrentFrameData(out SpineVatAnimationClip clipForFrame, out int frame, out int nextFrame, out float blend)
	{
		clipForFrame = ResolveClipForCurrentTime(out var localRawFrame);
		SpineVatAnimationClip spineVatAnimationClip = clipForFrame ?? currentClip;
		int num = Mathf.Max(0, (spineVatAnimationClip != null) ? (spineVatAnimationClip.frameCount - 1) : 0);
		localRawFrame = Mathf.Clamp(localRawFrame, 0f, (float)num);
		frame = Mathf.Clamp(Mathf.FloorToInt(localRawFrame), 0, num);
		nextFrame = Mathf.Clamp(frame + 1, 0, num);
		blend = ((nextFrame == frame) ? 0f : Mathf.Clamp01(localRawFrame - (float)frame));
	}

	private SpineVatAnimationClip ResolveClipForCurrentTime(out float localRawFrame)
	{
		localRawFrame = 0f;
		if (currentClip == null)
		{
			return null;
		}
		int currentAnimationFrameCount = GetCurrentAnimationFrameCount();
		int num = Mathf.Max(0, currentAnimationFrameCount - 1);
		float num2 = CalculateRawGlobalFrame(num);
		num2 = ((!loop || num <= 0) ? Mathf.Clamp(num2, 0f, (float)num) : Mathf.Repeat(num2, (float)num));
		int globalFrame = Mathf.Clamp(Mathf.FloorToInt(num2), 0, num);
		SpineVatAnimationClip spineVatAnimationClip = FindSegmentForGlobalFrame(globalFrame);
		int num3 = Mathf.Max(0, spineVatAnimationClip?.startFrame ?? currentClip.startFrame);
		localRawFrame = num2 - (float)num3;
		return spineVatAnimationClip;
	}

	private int GetCurrentAnimationFrameCount()
	{
		if (currentAnimationSegments.Count > 0 && currentAnimationSegments[0] != null)
		{
			SpineVatAnimationClip spineVatAnimationClip = currentAnimationSegments[0];
			return Mathf.Max(1, (spineVatAnimationClip.animationFrameCount > 0) ? spineVatAnimationClip.animationFrameCount : spineVatAnimationClip.frameCount);
		}
		if (currentClip == null)
		{
			return 1;
		}
		return Mathf.Max(1, currentClip.frameCount);
	}

	private SpineVatAnimationClip FindSegmentForGlobalFrame(int globalFrame)
	{
		if (currentAnimationSegments.Count == 0)
		{
			return currentClip;
		}
		SpineVatAnimationClip result = currentAnimationSegments[0];
		for (int i = 0; i < currentAnimationSegments.Count; i++)
		{
			SpineVatAnimationClip spineVatAnimationClip = currentAnimationSegments[i];
			if (spineVatAnimationClip != null)
			{
				int num = Mathf.Max(0, spineVatAnimationClip.startFrame);
				int num2 = num + Mathf.Max(1, spineVatAnimationClip.frameCount) - 1;
				if (globalFrame >= num && globalFrame <= num2)
				{
					return spineVatAnimationClip;
				}
				if (globalFrame >= num)
				{
					result = spineVatAnimationClip;
				}
			}
		}
		return result;
	}

	private float CalculateRawGlobalFrame(int lastGlobalFrame)
	{
		if (lastGlobalFrame <= 0)
		{
			return 0f;
		}
		float num = ((currentClip != null) ? currentClip.durationSeconds : 0f);
		if (num > 0.0001f)
		{
			return animationTime / num * (float)lastGlobalFrame;
		}
		return animationTime * (float)Mathf.Max(1, vatAsset.bakedFrameRate);
	}

	private string PickRandomAnimationName()
	{
		string result = null;
		int num = 0;
		for (int i = 0; i < vatAsset.animations.Count; i++)
		{
			SpineVatAnimationClip spineVatAnimationClip = vatAsset.animations[i];
			if (spineVatAnimationClip != null && !string.IsNullOrEmpty(spineVatAnimationClip.animationName) && !HasEarlierAnimationName(i, spineVatAnimationClip.animationName))
			{
				num++;
				if (Random.Range(0, num) == 0)
				{
					result = spineVatAnimationClip.animationName;
				}
			}
		}
		return result;
	}

	private bool HasEarlierAnimationName(int beforeIndex, string animationName)
	{
		for (int i = 0; i < beforeIndex; i++)
		{
			SpineVatAnimationClip spineVatAnimationClip = vatAsset.animations[i];
			if (spineVatAnimationClip != null && spineVatAnimationClip.animationName == animationName)
			{
				return true;
			}
		}
		return false;
	}

	private void ApplyClipRendererData(SpineVatAnimationClip clip)
	{
		if (clip != null)
		{
			RebuildRuntimeMesh();
			RebuildRuntimeMaterial();
			ApplyStaticMaterialProperties();
			ApplyRuntimeMaterialSettings();
			ApplyDynamicDepthSortProperties();
		}
	}

	private void TickRoaming(float deltaTime)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		if (!moving)
		{
			idleTimer -= deltaTime;
			if (idleTimer <= 0f)
			{
				PickNewTarget();
				moving = true;
				Play(walkAnimationName, shouldLoop: true);
			}
			return;
		}
		Vector3 position = ((Component)this).transform.position;
		Vector3 val = Vector3.MoveTowards(position, targetPosition, moveSpeed * deltaTime);
		((Component)this).transform.position = val;
		Vector3 val2 = targetPosition - position;
		if (Mathf.Abs(val2.x) > 0.01f)
		{
			Vector3 localScale = ((Component)this).transform.localScale;
			localScale.x = Mathf.Abs(localScale.x) * ((val2.x < 0f) ? (-1f) : 1f);
			((Component)this).transform.localScale = localScale;
		}
		if (Vector3.Distance(val, targetPosition) <= arriveDistance)
		{
			moving = false;
			idleTimer = Random.Range(idleDurationRange.x, idleDurationRange.y);
			Play(idleAnimationName, shouldLoop: true, Random.value);
		}
	}

	private void PickNewTarget()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		targetPosition = new Vector3(Random.Range(boundaryMin.x, boundaryMax.x), Random.Range(boundaryMin.y, boundaryMax.y), ((Component)this).transform.position.z);
	}

	private static void WriteVertexIndicesToUv2(Mesh mesh)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		int vertexCount = mesh.vertexCount;
		Vector2[] array = (Vector2[])(object)new Vector2[vertexCount];
		for (int i = 0; i < vertexCount; i++)
		{
			array[i] = new Vector2((float)i, 0f);
		}
		mesh.uv2 = array;
	}

	private void EnsureRendererReferences()
	{
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Expected O, but got Unknown
		if ((Object)(object)meshFilter == (Object)null)
		{
			meshFilter = ((Component)this).GetComponent<MeshFilter>();
		}
		if ((Object)(object)meshRenderer == (Object)null)
		{
			meshRenderer = ((Component)this).GetComponent<MeshRenderer>();
		}
		if ((Object)(object)meshRenderer != (Object)null)
		{
			((Renderer)meshRenderer).enabled = renderingVisible && !indirectRenderingEnabled;
		}
		if (propertyBlock == null)
		{
			propertyBlock = new MaterialPropertyBlock();
		}
	}

	private Mesh GetCurrentSourceMesh()
	{
		if (currentClip != null && (Object)(object)currentClip.bakedMesh != (Object)null)
		{
			return currentClip.bakedMesh;
		}
		return vatAsset.bakedMesh;
	}

	private Material[] GetCurrentSourceMaterials()
	{
		if (currentClip != null && currentClip.sharedMaterials != null && currentClip.sharedMaterials.Length != 0)
		{
			return currentClip.sharedMaterials;
		}
		return vatAsset.sharedMaterials;
	}

	private void DestroyRuntimeMaterials()
	{
		runtimeMaterials = null;
		currentSourceMaterials = null;
	}

	private void ClearCurrentPlayback()
	{
		currentClip = null;
		currentAnimationSegments.Clear();
		currentSourceMesh = null;
		currentRuntimeMeshKey = 0;
		currentSourceMaterials = null;
		animationTime = 0f;
		isPlaying = false;
		animationCompleteInvoked = false;
		hasResolvedFrameData = false;
		resolvedFrame = 0;
		resolvedNextFrame = 0;
		resolvedFrameBlend = 0f;
		resolvedFrameClip = null;
		onAnimationCompleteOnce = null;
	}

	private static void CopyTextureIfPresent(Material source, Material target, string propertyName)
	{
		int propertyId = Shader.PropertyToID(propertyName);
		CopyTextureIfPresent(source, target, propertyId);
	}

	private static void CopyTextureIfPresent(Material source, Material target, int propertyId)
	{
		if ((Object)(object)source != (Object)null && source.HasProperty(propertyId) && target.HasProperty(propertyId))
		{
			target.SetTexture(propertyId, source.GetTexture(propertyId));
		}
	}

	private static void CopyFloatIfPresent(Material source, Material target, int propertyId)
	{
		if ((Object)(object)source != (Object)null && source.HasProperty(propertyId) && target.HasProperty(propertyId))
		{
			target.SetFloat(propertyId, source.GetFloat(propertyId));
		}
	}

	private static void CopyColorIfPresent(Material source, Material target, int propertyId)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)source != (Object)null && source.HasProperty(propertyId) && target.HasProperty(propertyId))
		{
			target.SetColor(propertyId, source.GetColor(propertyId));
		}
	}

	private static Material GetOrCreateSharedRuntimeMaterial(Material sourceMaterial, Shader shader, SpineVatAsset asset, int subMeshIndex)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		if ((Object)(object)shader == (Object)null)
		{
			return null;
		}
		int runtimeMaterialCacheKey = GetRuntimeMaterialCacheKey(sourceMaterial, shader);
		if (SharedRuntimeMaterialCache.TryGetValue(runtimeMaterialCacheKey, out var value) && (Object)(object)value != (Object)null)
		{
			return value;
		}
		value = (((Object)(object)sourceMaterial != (Object)null) ? new Material(sourceMaterial) : new Material(shader));
		((Object)value).name = (((Object)(object)sourceMaterial != (Object)null) ? (((Object)sourceMaterial).name + "_" + ((Object)shader).name + "_SpineVatShared") : string.Format("{0}_{1}_SpineVatShared", ((Object)(object)asset != (Object)null) ? ((Object)asset).name : "SpineVat", subMeshIndex));
		value.shader = shader;
		value.enableInstancing = true;
		if ((Object)(object)sourceMaterial != (Object)null)
		{
			CopyTextureIfPresent(sourceMaterial, value, "_MainTex");
			CopyTextureIfPresent(sourceMaterial, value, AlphaTexId);
			CopyTextureIfPresent(sourceMaterial, value, "_MaskTex");
			CopyColorIfPresent(sourceMaterial, value, ColorId);
			CopyFloatIfPresent(sourceMaterial, value, EnableExternalAlphaId);
			bool flag = sourceMaterial.IsKeywordEnabled("ETC1_EXTERNAL_ALPHA") || (sourceMaterial.HasProperty(EnableExternalAlphaId) && sourceMaterial.GetFloat(EnableExternalAlphaId) > 0.5f) || (sourceMaterial.HasProperty(AlphaTexId) && (Object)(object)sourceMaterial.GetTexture(AlphaTexId) != (Object)null);
			SetKeyword(value, "ETC1_EXTERNAL_ALPHA", flag);
			if (value.HasProperty(EnableExternalAlphaId))
			{
				value.SetFloat(EnableExternalAlphaId, flag ? 1f : 0f);
			}
		}
		EnsureDefaultColor(value, sourceMaterial);
		bool straightAlphaInput = GetStraightAlphaInput(sourceMaterial, value);
		SetKeyword(value, "_STRAIGHT_ALPHA_INPUT", straightAlphaInput);
		if (value.HasProperty(StraightAlphaInputId))
		{
			value.SetFloat(StraightAlphaInputId, straightAlphaInput ? 1f : 0f);
		}
		SharedRuntimeMaterialCache[runtimeMaterialCacheKey] = value;
		return value;
	}

	private static void EnsureDefaultColor(Material material, Material sourceMaterial)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)material != (Object)null && material.HasProperty(ColorId) && ((Object)(object)sourceMaterial == (Object)null || !sourceMaterial.HasProperty(ColorId)))
		{
			material.SetColor(ColorId, Color.white);
		}
	}

	private static bool GetStraightAlphaInput(Material sourceMaterial, Material runtimeMaterial)
	{
		if ((Object)(object)sourceMaterial != (Object)null)
		{
			if (sourceMaterial.HasProperty(StraightAlphaInputId))
			{
				return sourceMaterial.GetFloat(StraightAlphaInputId) > 0.5f;
			}
			return sourceMaterial.IsKeywordEnabled("_STRAIGHT_ALPHA_INPUT");
		}
		if ((Object)(object)runtimeMaterial != (Object)null)
		{
			if (!runtimeMaterial.HasProperty(StraightAlphaInputId) || !(runtimeMaterial.GetFloat(StraightAlphaInputId) > 0.5f))
			{
				return runtimeMaterial.IsKeywordEnabled("_STRAIGHT_ALPHA_INPUT");
			}
			return true;
		}
		return false;
	}

	private static int GetRuntimeMaterialCacheKey(Material sourceMaterial, Shader shader)
	{
		return ((((Object)(object)shader != (Object)null) ? ((Object)shader).GetInstanceID() : 0) * 397) ^ (((Object)(object)sourceMaterial != (Object)null) ? ((Object)sourceMaterial).GetInstanceID() : 0);
	}

	private static void SetKeyword(Material material, string keyword, bool enabled)
	{
		if (enabled)
		{
			material.EnableKeyword(keyword);
		}
		else
		{
			material.DisableKeyword(keyword);
		}
	}

	private static bool MaterialsEquivalent(Material[] a, Material[] b, int materialCount)
	{
		if (a == null || b == null)
		{
			return a == b;
		}
		for (int i = 0; i < materialCount; i++)
		{
			Material obj = ((i < a.Length) ? a[i] : null);
			Material val = ((i < b.Length) ? b[i] : null);
			if ((Object)(object)obj != (Object)(object)val)
			{
				return false;
			}
		}
		return true;
	}

	private static Bounds BuildVatBounds(SpineVatAsset asset, Vector2 depthRange)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)asset == (Object)null)
		{
			return new Bounds(Vector3.zero, Vector3.one * 0.001f);
		}
		Vector3 val = asset.boundsMax - asset.boundsMin;
		if (Mathf.Abs(val.z) < 0.001f)
		{
			val.z = 0.001f;
		}
		Bounds result = default(Bounds);
		((Bounds)(ref result))._002Ector((asset.boundsMin + asset.boundsMax) * 0.5f, val);
		if (!Mathf.Approximately(depthRange.x, depthRange.y))
		{
			Vector3 min = ((Bounds)(ref result)).min;
			Vector3 max = ((Bounds)(ref result)).max;
			min.z += Mathf.Min(depthRange.x, depthRange.y);
			max.z += Mathf.Max(depthRange.x, depthRange.y);
			((Bounds)(ref result)).SetMinMax(min, max);
		}
		return result;
	}

	private Mesh GetOrCreateSharedRuntimeMesh(Mesh sourceMesh)
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)sourceMesh == (Object)null)
		{
			return null;
		}
		int sharedRuntimeMeshKey = GetSharedRuntimeMeshKey(sourceMesh);
		if (SharedRuntimeMeshCache.TryGetValue(sharedRuntimeMeshKey, out var value) && (Object)(object)value != (Object)null)
		{
			return value;
		}
		Mesh val = Object.Instantiate<Mesh>(sourceMesh);
		((Object)val).name = ((Object)vatAsset).name + "_" + ((currentClip != null) ? currentClip.animationName : "Default") + "_RuntimeMesh";
		WriteVertexIndicesToUv2(val);
		val.bounds = BuildVatBounds(vatAsset, useYDepthSort ? depthSortZRange : Vector2.zero);
		SharedRuntimeMeshCache[sharedRuntimeMeshKey] = val;
		return val;
	}

	private int GetSharedRuntimeMeshKey(Mesh sourceMesh)
	{
		if ((Object)(object)sourceMesh == (Object)null)
		{
			return 0;
		}
		return (((((((Object)sourceMesh).GetInstanceID() * 397) ^ (((Object)(object)vatAsset != (Object)null) ? ((Object)vatAsset).GetInstanceID() : 0)) * 397) ^ (useYDepthSort ? 1 : 0)) * 397) ^ ((object)System.Runtime.CompilerServices.Unsafe.As<Vector2, Vector2>(ref depthSortZRange)/*cast due to .constrained prefix*/).GetHashCode();
	}
}
