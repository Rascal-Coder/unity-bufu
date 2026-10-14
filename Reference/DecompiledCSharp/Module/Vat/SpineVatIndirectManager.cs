using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace Module.Vat;

[AddComponentMenu("VAT/Spine VAT Indirect Manager")]
[DisallowMultipleComponent]
public sealed class SpineVatIndirectManager : MonoBehaviour
{
	private struct TemplateSource
	{
		public SpineVatAsset Asset;

		public Vector3 Scale;

		public GameObject TemplateObject;
	}

	private sealed class AssetRuntime
	{
		public SpineVatAsset Asset;

		public AnimationDescriptor[] Animations;
	}

	private sealed class AnimationDescriptor
	{
		public string Name;

		public float DurationSeconds;

		public int TotalFrameCount;

		public SpineVatAnimationClip[] Segments;
	}

	private sealed class VatInstance
	{
		public AssetRuntime Runtime;

		public AnimationDescriptor Animation;

		public Vector3 Position;

		public Vector3 TargetPosition;

		public Vector3 BaseScale = Vector3.one;

		public float ScaleMagnitude = 1f;

		public float FacingSign = 1f;

		public float AnimationTime;

		public float IdleTimer;

		public bool Moving;
	}

	private struct GpuInstanceData
	{
		public Vector4 PositionScaleX;

		public Vector4 FrameData;

		public Vector4 ScaleYSort;
	}

	private sealed class IndirectBatch
	{
		private readonly uint[] args = new uint[5];

		public SpineVatAsset Asset;

		public SpineVatAnimationClip Segment;

		public Mesh Mesh;

		public int SubMeshIndex;

		public Material Material;

		public MaterialPropertyBlock PropertyBlock;

		public ComputeBuffer InstanceBuffer;

		public ComputeBuffer ArgumentsBuffer;

		public GpuInstanceData[] Data;

		public int Capacity;

		public int Count;

		public void Add(GpuInstanceData data)
		{
			EnsureCapacity(Count + 1);
			Data[Count] = data;
			Count++;
		}

		public void EnsureCapacity(int requiredCapacity)
		{
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Expected O, but got Unknown
			if (Capacity < requiredCapacity)
			{
				int num;
				for (num = Mathf.Max(64, (Capacity <= 0) ? requiredCapacity : (Capacity * 2)); num < requiredCapacity; num *= 2)
				{
				}
				GpuInstanceData[] array = new GpuInstanceData[num];
				if (Data != null && Count > 0)
				{
					Array.Copy(Data, array, Mathf.Min(Count, Data.Length));
				}
				Data = array;
				if (InstanceBuffer != null)
				{
					InstanceBuffer.Release();
				}
				InstanceBuffer = new ComputeBuffer(num, 48, (ComputeBufferType)16);
				Capacity = num;
			}
		}

		public void UploadArguments()
		{
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Expected O, but got Unknown
			if (Count > 0)
			{
				InstanceBuffer.SetData((Array)Data, 0, 0, Count);
				if (ArgumentsBuffer == null)
				{
					ArgumentsBuffer = new ComputeBuffer(1, args.Length * 4, (ComputeBufferType)256);
				}
				args[0] = Mesh.GetIndexCount(SubMeshIndex);
				args[1] = (uint)Count;
				args[2] = Mesh.GetIndexStart(SubMeshIndex);
				args[3] = Mesh.GetBaseVertex(SubMeshIndex);
				args[4] = 0u;
				ArgumentsBuffer.SetData((Array)args);
			}
		}

		public void Release()
		{
			if (InstanceBuffer != null)
			{
				InstanceBuffer.Release();
				InstanceBuffer = null;
			}
			if (ArgumentsBuffer != null)
			{
				ArgumentsBuffer.Release();
				ArgumentsBuffer = null;
			}
			if ((Object)(object)Material != (Object)null)
			{
				Object.Destroy((Object)(object)Material);
				Material = null;
			}
		}
	}

	private const string DefaultShaderName = "Spine/VAT Indirect Unlit";

	private const int GpuInstanceStride = 48;

	private static readonly int PositionTexId = Shader.PropertyToID("_PositionTex");

	private static readonly int VatInstancesId = Shader.PropertyToID("_VatInstances");

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

	private static readonly int MeshVertexRgbNeedsVatAlphaId = Shader.PropertyToID("_MeshVertexRgbNeedsVatAlpha");

	private const string StraightAlphaKeyword = "_STRAIGHT_ALPHA_INPUT";

	[Header("Sources")]
	[SerializeField]
	[FormerlySerializedAs("roleTemplates")]
	private SpineVat[] spineVatTemplates;

	[SerializeField]
	private SpineVatAsset[] vatAssets;

	[SerializeField]
	private Shader indirectShader;

	[SerializeField]
	[FormerlySerializedAs("autoCollectRoleTemplates")]
	private bool autoCollectSpineVatTemplates = true;

	[SerializeField]
	private bool hideTemplateObjectsOnPlay = true;

	[SerializeField]
	private bool pickTemplateRandomly;

	[SerializeField]
	private bool useRegisteredInstances;

	[Header("Stress Counts")]
	[SerializeField]
	private int[] testCounts = new int[5] { 1000, 2000, 5000, 10000, 20000 };

	[SerializeField]
	private int startCountIndex;

	[Header("Spawn Area")]
	[SerializeField]
	private Vector2 boundaryMin = new Vector2(-400f, -600f);

	[SerializeField]
	private Vector2 boundaryMax = new Vector2(400f, 100f);

	[SerializeField]
	private float spawnZ;

	[SerializeField]
	private Vector2 randomScaleRange = Vector2.one;

	[Header("Animation")]
	[SerializeField]
	private bool playRandomAnimationOnSpawn = true;

	[SerializeField]
	private string startAnimationName = "idle_front";

	[SerializeField]
	private float animationTimeScale = 1f;

	[SerializeField]
	private bool randomizeStartTime = true;

	[Header("Roaming")]
	[SerializeField]
	private bool enableRandomRoam = true;

	[SerializeField]
	private bool switchAnimationWhenRoaming = true;

	[SerializeField]
	private string idleAnimationName = "idle_front";

	[SerializeField]
	private string walkAnimationName = "walk_front";

	[SerializeField]
	private float moveSpeed = 180f;

	[SerializeField]
	private float arriveDistance = 12f;

	[SerializeField]
	private Vector2 idleDurationRange = new Vector2(0.4f, 1.2f);

	[Header("Rendering")]
	[SerializeField]
	private bool useYDepthSort = true;

	[SerializeField]
	private Vector2 depthSortZRange = new Vector2(-1f, 1f);

	[SerializeField]
	private bool writeDepthForYSort = true;

	[SerializeField]
	[Range(0f, 0.1f)]
	private float alphaClipThreshold = 0.001f;

	[SerializeField]
	private ShadowCastingMode shadowCastingMode;

	[SerializeField]
	private bool receiveShadows;

	[SerializeField]
	private bool useCustomDrawBounds;

	[SerializeField]
	private Bounds customDrawBounds = new Bounds(Vector3.zero, new Vector3(4000f, 4000f, 1000f));

	[SerializeField]
	private float drawBoundsPadding = 1000f;

	[Header("Controls")]
	[SerializeField]
	private KeyCode previousCountKey = (KeyCode)91;

	[SerializeField]
	private KeyCode nextCountKey = (KeyCode)93;

	[SerializeField]
	private KeyCode rebuildKey = (KeyCode)114;

	[SerializeField]
	private KeyCode clearKey = (KeyCode)8;

	[SerializeField]
	private KeyCode toggleOverlayKey = (KeyCode)282;

	[Header("Overlay")]
	[SerializeField]
	private bool showOverlay = true;

	[SerializeField]
	private int overlayFontSize = 18;

	[SerializeField]
	private bool autoScaleOverlay = true;

	[SerializeField]
	private float overlayScale = 1f;

	private readonly List<TemplateSource> templateSources = new List<TemplateSource>();

	private readonly List<GameObject> hiddenTemplateObjects = new List<GameObject>();

	private readonly List<VatInstance> instances = new List<VatInstance>();

	private readonly List<SpineVat> registeredSpineVats = new List<SpineVat>();

	private readonly List<IndirectBatch> batches = new List<IndirectBatch>();

	private readonly Dictionary<int, IndirectBatch> batchesByKey = new Dictionary<int, IndirectBatch>();

	private readonly Dictionary<SpineVatAsset, AssetRuntime> runtimesByAsset = new Dictionary<SpineVatAsset, AssetRuntime>();

	private readonly List<SpineVatAnimationClip> segmentScratch = new List<SpineVatAnimationClip>();

	private GUIStyle overlayStyle;

	private GUIStyle buttonStyle;

	private float smoothedDeltaTime;

	private int currentCountIndex;

	private int targetCount;

	private bool initialized;

	private int baseSortingOrder;

	public int CurrentInstanceCount
	{
		get
		{
			if (!useRegisteredInstances)
			{
				return instances.Count;
			}
			return registeredSpineVats.Count;
		}
	}

	public int TargetCount => targetCount;

	public int DrawBatchCount => batches.Count;

	private void Awake()
	{
		currentCountIndex = Mathf.Max(0, startCountIndex);
		InitializeRuntime();
	}

	private void Start()
	{
		if (!useRegisteredInstances)
		{
			EnsureValidCounts();
			currentCountIndex = Mathf.Clamp(currentCountIndex, 0, testCounts.Length - 1);
			if (hideTemplateObjectsOnPlay)
			{
				SetTemplateObjectsActive(active: false);
			}
			Rebuild(testCounts[currentCountIndex]);
		}
	}

	private void Update()
	{
		UpdateFrameStats();
		if (!useRegisteredInstances)
		{
			HandleKeyboardInput();
		}
	}

	private void LateUpdate()
	{
		if (initialized)
		{
			if (useRegisteredInstances)
			{
				ResetBatches();
				FillBatchesFromRegisteredInstances();
				UploadAndDrawBatches();
			}
			else if (instances.Count != 0)
			{
				float deltaTime = Time.deltaTime * Mathf.Max(0f, animationTimeScale);
				ResetBatches();
				SimulateAndFillBatches(deltaTime);
				UploadAndDrawBatches();
			}
		}
	}

	private void OnDestroy()
	{
		Clear();
		ReleaseBatches();
		SetTemplateObjectsActive(active: true);
	}

	private void OnValidate()
	{
		startCountIndex = Mathf.Max(0, startCountIndex);
		overlayFontSize = Mathf.Max(10, overlayFontSize);
		overlayScale = Mathf.Clamp(overlayScale, 0.5f, 5f);
		moveSpeed = Mathf.Max(0f, moveSpeed);
		arriveDistance = Mathf.Max(0.01f, arriveDistance);
		drawBoundsPadding = Mathf.Max(0f, drawBoundsPadding);
		alphaClipThreshold = Mathf.Max(0f, alphaClipThreshold);
		if (boundaryMax.x < boundaryMin.x)
		{
			float x = boundaryMin.x;
			boundaryMin.x = boundaryMax.x;
			boundaryMax.x = x;
		}
		if (boundaryMax.y < boundaryMin.y)
		{
			float y = boundaryMin.y;
			boundaryMin.y = boundaryMax.y;
			boundaryMax.y = y;
		}
		randomScaleRange.x = Mathf.Max(0.001f, randomScaleRange.x);
		randomScaleRange.y = Mathf.Max(randomScaleRange.x, randomScaleRange.y);
		if (testCounts == null || testCounts.Length == 0)
		{
			testCounts = new int[4] { 1000, 2000, 5000, 10000 };
		}
		for (int i = 0; i < testCounts.Length; i++)
		{
			testCounts[i] = Mathf.Max(0, testCounts[i]);
		}
	}

	private void OnGUI()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		if (showOverlay)
		{
			EnsureGuiStyles();
			Matrix4x4 matrix = GUI.matrix;
			float num = GetOverlayScale();
			GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(num, num, 1f)) * matrix;
			GUILayout.BeginArea(new Rect(12f, 12f, 470f, 295f), GUI.skin.box);
			GUILayout.Label("Spine VAT Indirect Manager", overlayStyle, Array.Empty<GUILayoutOption>());
			GUILayout.Label($"FPS: {GetFps():F1}    Frame: {GetFrameMs():F2} ms", overlayStyle, Array.Empty<GUILayoutOption>());
			GUILayout.Label($"Instances: {instances.Count} / {targetCount}", overlayStyle, Array.Empty<GUILayoutOption>());
			GUILayout.Label($"Templates: {templateSources.Count}    Draw Batches: {GetActiveBatchCount()} / {batches.Count}", overlayStyle, Array.Empty<GUILayoutOption>());
			GUILayout.Label($"Keys: {previousCountKey} / {nextCountKey} switch, {rebuildKey} rebuild, {clearKey} clear, {toggleOverlayKey} overlay", overlayStyle, Array.Empty<GUILayoutOption>());
			GUILayout.Space(6f);
			GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
			if (GUILayout.Button("Prev", buttonStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(78f) }))
			{
				SelectCountOffset(-1);
			}
			if (GUILayout.Button("Rebuild", buttonStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(96f) }))
			{
				RebuildCurrentCount();
			}
			if (GUILayout.Button("Next", buttonStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(78f) }))
			{
				SelectCountOffset(1);
			}
			if (GUILayout.Button("Clear", buttonStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(78f) }))
			{
				Clear();
			}
			GUILayout.EndHorizontal();
			GUILayout.EndArea();
			GUI.matrix = matrix;
		}
	}

	[ContextMenu("Rebuild Current Count")]
	public void RebuildCurrentCount()
	{
		EnsureValidCounts();
		currentCountIndex = Mathf.Clamp(currentCountIndex, 0, testCounts.Length - 1);
		Rebuild(testCounts[currentCountIndex]);
	}

	[ContextMenu("Clear Instances")]
	public void Clear()
	{
		targetCount = 0;
		instances.Clear();
		registeredSpineVats.Clear();
		ResetBatches();
	}

	public void SetCountIndex(int index)
	{
		EnsureValidCounts();
		currentCountIndex = Mathf.Clamp(index, 0, testCounts.Length - 1);
		Rebuild(testCounts[currentCountIndex]);
	}

	public void SetCount(int count)
	{
		Rebuild(Mathf.Max(0, count));
	}

	public void ConfigureRegisteredRuntime(Shader shader, Vector2 min, Vector2 max, int renderLayer, int sortingOrder = 0)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		indirectShader = (((Object)(object)shader != (Object)null) ? shader : indirectShader);
		useRegisteredInstances = true;
		showOverlay = false;
		boundaryMin = min;
		boundaryMax = max;
		((Component)this).gameObject.layer = renderLayer;
		baseSortingOrder = sortingOrder;
		InitializeRuntime();
	}

	public void RegisterSpineVat(SpineVat spineVat)
	{
		if (!((Object)(object)spineVat == (Object)null))
		{
			useRegisteredInstances = true;
			if (!registeredSpineVats.Contains(spineVat))
			{
				registeredSpineVats.Add(spineVat);
			}
		}
	}

	public void UnregisterSpineVat(SpineVat spineVat)
	{
		if (!((Object)(object)spineVat == (Object)null))
		{
			registeredSpineVats.Remove(spineVat);
		}
	}

	public void ClearRegisteredSpineVats()
	{
		registeredSpineVats.Clear();
	}

	public void Rebuild(int count)
	{
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		if (useRegisteredInstances)
		{
			Clear();
			return;
		}
		targetCount = Mathf.Max(0, count);
		instances.Clear();
		InitializeRuntime();
		if (!initialized || templateSources.Count == 0)
		{
			Debug.LogWarning((object)"[SpineVatIndirectManager] No valid VAT sources found. Drag SpineVat templates or SpineVatAsset assets into the manager.", (Object)(object)this);
			return;
		}
		for (int i = 0; i < targetCount; i++)
		{
			TemplateSource templateSource = PickTemplate(i);
			AssetRuntime runtime = GetRuntime(templateSource.Asset);
			if (runtime != null)
			{
				VatInstance vatInstance = new VatInstance
				{
					Runtime = runtime,
					Position = PickSpawnPosition(),
					BaseScale = templateSource.Scale,
					ScaleMagnitude = Random.Range(randomScaleRange.x, randomScaleRange.y),
					IdleTimer = Random.Range(idleDurationRange.x, idleDurationRange.y)
				};
				PlayInitialAnimation(vatInstance);
				PickNewTarget(vatInstance);
				instances.Add(vatInstance);
			}
		}
	}

	private void InitializeRuntime()
	{
		if ((Object)(object)indirectShader == (Object)null)
		{
			indirectShader = Shader.Find("Spine/VAT Indirect Unlit");
		}
		if ((Object)(object)indirectShader == (Object)null)
		{
			initialized = false;
			Debug.LogError((object)"[SpineVatIndirectManager] Shader not found: Spine/VAT Indirect Unlit", (Object)(object)this);
			return;
		}
		if (!SystemInfo.supportsInstancing || !SystemInfo.supportsComputeShaders)
		{
			Debug.LogWarning((object)"[SpineVatIndirectManager] This platform may not support DrawMeshInstancedIndirect well. Use desktop/Vulkan/Metal/D3D11+ for the real stress test.", (Object)(object)this);
		}
		ReleaseBatches();
		if (useRegisteredInstances)
		{
			templateSources.Clear();
			runtimesByAsset.Clear();
			initialized = (Object)(object)indirectShader != (Object)null;
		}
		else
		{
			CacheTemplateSources();
			BuildAssetRuntimes();
			initialized = templateSources.Count > 0;
		}
	}

	private void CacheTemplateSources()
	{
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		templateSources.Clear();
		if (spineVatTemplates != null)
		{
			for (int i = 0; i < spineVatTemplates.Length; i++)
			{
				SpineVat spineVat = spineVatTemplates[i];
				if (!((Object)(object)spineVat == (Object)null))
				{
					AddTemplateSource(spineVat.VatAsset, ((Component)spineVat).transform.localScale, ((Component)spineVat).gameObject);
				}
			}
		}
		if (vatAssets != null)
		{
			for (int j = 0; j < vatAssets.Length; j++)
			{
				AddTemplateSource(vatAssets[j], Vector3.one, null);
			}
		}
		if (autoCollectSpineVatTemplates && templateSources.Count <= 0)
		{
			SpineVat[] array = Object.FindObjectsOfType<SpineVat>(true);
			foreach (SpineVat spineVat2 in array)
			{
				AddTemplateSource(spineVat2.VatAsset, ((Component)spineVat2).transform.localScale, ((Component)spineVat2).gameObject);
			}
		}
	}

	private void AddTemplateSource(SpineVatAsset asset, Vector3 scale, GameObject templateObject)
	{
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		if (!IsValidAsset(asset))
		{
			return;
		}
		for (int i = 0; i < templateSources.Count; i++)
		{
			if ((Object)(object)templateSources[i].Asset == (Object)(object)asset && (Object)(object)templateSources[i].TemplateObject == (Object)(object)templateObject)
			{
				return;
			}
		}
		templateSources.Add(new TemplateSource
		{
			Asset = asset,
			Scale = scale,
			TemplateObject = templateObject
		});
	}

	private void BuildAssetRuntimes()
	{
		runtimesByAsset.Clear();
		for (int i = 0; i < templateSources.Count; i++)
		{
			SpineVatAsset asset = templateSources[i].Asset;
			if (!((Object)(object)asset == (Object)null) && !runtimesByAsset.ContainsKey(asset))
			{
				AssetRuntime assetRuntime = BuildRuntime(asset);
				if (assetRuntime != null)
				{
					runtimesByAsset.Add(asset, assetRuntime);
				}
			}
		}
	}

	private AssetRuntime BuildRuntime(SpineVatAsset asset)
	{
		List<AnimationDescriptor> list = new List<AnimationDescriptor>();
		if (asset.animations != null)
		{
			for (int i = 0; i < asset.animations.Count; i++)
			{
				SpineVatAnimationClip spineVatAnimationClip = asset.animations[i];
				if (spineVatAnimationClip != null && !string.IsNullOrEmpty(spineVatAnimationClip.animationName) && !HasDescriptor(list, spineVatAnimationClip.animationName))
				{
					segmentScratch.Clear();
					asset.GetAnimationSegmentsOrFirst(spineVatAnimationClip.animationName, segmentScratch);
					if (segmentScratch.Count != 0)
					{
						list.Add(new AnimationDescriptor
						{
							Name = spineVatAnimationClip.animationName,
							DurationSeconds = Mathf.Max(0.0001f, segmentScratch[0].durationSeconds),
							TotalFrameCount = Mathf.Max(1, (segmentScratch[0].animationFrameCount > 0) ? segmentScratch[0].animationFrameCount : segmentScratch[0].frameCount),
							Segments = segmentScratch.ToArray()
						});
					}
				}
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		return new AssetRuntime
		{
			Asset = asset,
			Animations = list.ToArray()
		};
	}

	private static bool HasDescriptor(List<AnimationDescriptor> descriptors, string animationName)
	{
		for (int i = 0; i < descriptors.Count; i++)
		{
			if (descriptors[i].Name == animationName)
			{
				return true;
			}
		}
		return false;
	}

	private void SetTemplateObjectsActive(bool active)
	{
		if (active)
		{
			for (int i = 0; i < hiddenTemplateObjects.Count; i++)
			{
				if ((Object)(object)hiddenTemplateObjects[i] != (Object)null)
				{
					hiddenTemplateObjects[i].SetActive(true);
				}
			}
			hiddenTemplateObjects.Clear();
			return;
		}
		hiddenTemplateObjects.Clear();
		for (int j = 0; j < templateSources.Count; j++)
		{
			GameObject templateObject = templateSources[j].TemplateObject;
			if (!((Object)(object)templateObject == (Object)null) && templateObject.activeSelf)
			{
				hiddenTemplateObjects.Add(templateObject);
				templateObject.SetActive(false);
			}
		}
	}

	private void SimulateAndFillBatches(float deltaTime)
	{
		for (int i = 0; i < instances.Count; i++)
		{
			VatInstance vatInstance = instances[i];
			if (enableRandomRoam)
			{
				TickRoaming(vatInstance, deltaTime);
			}
			vatInstance.AnimationTime += deltaTime;
			if (!TryResolveFrame(vatInstance, out var segment, out var gpuData))
			{
				continue;
			}
			Mesh val = (((Object)(object)segment.bakedMesh != (Object)null) ? segment.bakedMesh : vatInstance.Runtime.Asset.bakedMesh);
			if (!((Object)(object)val == (Object)null))
			{
				int num = Mathf.Max(1, val.subMeshCount);
				for (int j = 0; j < num; j++)
				{
					GetOrCreateBatch(vatInstance.Runtime.Asset, segment, j)?.Add(gpuData);
				}
			}
		}
	}

	private bool TryResolveFrame(VatInstance instance, out SpineVatAnimationClip segment, out GpuInstanceData gpuData)
	{
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		segment = null;
		gpuData = default(GpuInstanceData);
		AnimationDescriptor animation = instance.Animation;
		if (animation == null || animation.Segments == null || animation.Segments.Length == 0)
		{
			return false;
		}
		int num = Mathf.Max(0, animation.TotalFrameCount - 1);
		float num2 = 0f;
		if (num > 0)
		{
			num2 = instance.AnimationTime / Mathf.Max(0.0001f, animation.DurationSeconds) * (float)num;
			num2 = Mathf.Repeat(num2, (float)num);
		}
		int globalFrame = Mathf.Clamp(Mathf.FloorToInt(num2), 0, num);
		segment = FindSegment(animation, globalFrame);
		if (segment == null)
		{
			return false;
		}
		int num3 = Mathf.Max(0, segment.startFrame);
		int num4 = Mathf.Max(0, segment.frameCount - 1);
		float num5 = Mathf.Clamp(num2 - (float)num3, 0f, (float)num4);
		int num6 = Mathf.Clamp(Mathf.FloorToInt(num5), 0, num4);
		int num7 = Mathf.Clamp(num6 + 1, 0, num4);
		float num8 = ((num7 == num6) ? 0f : Mathf.Clamp01(num5 - (float)num6));
		Vector3 baseScale = instance.BaseScale;
		float scaleMagnitude = instance.ScaleMagnitude;
		float num9 = baseScale.x * scaleMagnitude * instance.FacingSign;
		float num10 = baseScale.y * scaleMagnitude;
		gpuData.PositionScaleX = new Vector4(instance.Position.x, instance.Position.y, instance.Position.z, num9);
		gpuData.FrameData = new Vector4((float)num6, (float)num7, num8, (float)segment.textureSliceIndex);
		gpuData.ScaleYSort = new Vector4(num10, instance.Position.y, 0f, 0f);
		return true;
	}

	private static SpineVatAnimationClip FindSegment(AnimationDescriptor animation, int globalFrame)
	{
		SpineVatAnimationClip result = animation.Segments[0];
		for (int i = 0; i < animation.Segments.Length; i++)
		{
			SpineVatAnimationClip spineVatAnimationClip = animation.Segments[i];
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

	private void TickRoaming(VatInstance instance, float deltaTime)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		if (!instance.Moving)
		{
			instance.IdleTimer -= deltaTime;
			if (instance.IdleTimer <= 0f)
			{
				PickNewTarget(instance);
				instance.Moving = true;
				if (switchAnimationWhenRoaming)
				{
					PlayAnimation(instance, walkAnimationName, randomStart: false);
				}
			}
			return;
		}
		Vector3 position = instance.Position;
		Vector3 val = Vector3.MoveTowards(position, instance.TargetPosition, moveSpeed * deltaTime);
		Vector3 val2 = instance.TargetPosition - position;
		instance.Position = val;
		if (Mathf.Abs(val2.x) > 0.01f)
		{
			instance.FacingSign = ((val2.x < 0f) ? (-1f) : 1f);
		}
		if (Vector3.Distance(val, instance.TargetPosition) <= arriveDistance)
		{
			instance.Moving = false;
			instance.IdleTimer = Random.Range(idleDurationRange.x, idleDurationRange.y);
			if (switchAnimationWhenRoaming)
			{
				PlayAnimation(instance, idleAnimationName, randomStart: false);
			}
		}
	}

	private void PlayInitialAnimation(VatInstance instance)
	{
		if (playRandomAnimationOnSpawn)
		{
			PlayRandomAnimation(instance, randomizeStartTime);
		}
		else if (!PlayAnimation(instance, startAnimationName, randomizeStartTime))
		{
			PlayRandomAnimation(instance, randomizeStartTime);
		}
	}

	private bool PlayAnimation(VatInstance instance, string animationName, bool randomStart)
	{
		AnimationDescriptor animationDescriptor = FindAnimation(instance.Runtime, animationName);
		if (animationDescriptor == null)
		{
			return false;
		}
		instance.Animation = animationDescriptor;
		instance.AnimationTime = (randomStart ? (Random.value * animationDescriptor.DurationSeconds) : 0f);
		return true;
	}

	private void PlayRandomAnimation(VatInstance instance, bool randomStart)
	{
		if (instance.Runtime != null && instance.Runtime.Animations != null && instance.Runtime.Animations.Length != 0)
		{
			AnimationDescriptor animationDescriptor = (instance.Animation = instance.Runtime.Animations[Random.Range(0, instance.Runtime.Animations.Length)]);
			instance.AnimationTime = (randomStart ? (Random.value * animationDescriptor.DurationSeconds) : 0f);
		}
	}

	private static AnimationDescriptor FindAnimation(AssetRuntime runtime, string animationName)
	{
		if (runtime == null || runtime.Animations == null || string.IsNullOrEmpty(animationName))
		{
			return null;
		}
		for (int i = 0; i < runtime.Animations.Length; i++)
		{
			if (runtime.Animations[i].Name == animationName)
			{
				return runtime.Animations[i];
			}
		}
		return null;
	}

	private void PickNewTarget(VatInstance instance)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		instance.TargetPosition = new Vector3(Random.Range(boundaryMin.x, boundaryMax.x), Random.Range(boundaryMin.y, boundaryMax.y), spawnZ);
	}

	private TemplateSource PickTemplate(int index)
	{
		if (pickTemplateRandomly)
		{
			return templateSources[Random.Range(0, templateSources.Count)];
		}
		return templateSources[index % templateSources.Count];
	}

	private Vector3 PickSpawnPosition()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		return new Vector3(Random.Range(boundaryMin.x, boundaryMax.x), Random.Range(boundaryMin.y, boundaryMax.y), spawnZ);
	}

	private AssetRuntime GetRuntime(SpineVatAsset asset)
	{
		if ((Object)(object)asset == (Object)null)
		{
			return null;
		}
		runtimesByAsset.TryGetValue(asset, out var value);
		return value;
	}

	private IndirectBatch GetOrCreateBatch(SpineVatAsset asset, SpineVatAnimationClip segment, int subMeshIndex)
	{
		int batchKey = GetBatchKey(asset, segment, subMeshIndex);
		if (batchesByKey.TryGetValue(batchKey, out var value))
		{
			return value;
		}
		value = CreateBatch(asset, segment, subMeshIndex);
		if (value == null)
		{
			return null;
		}
		batchesByKey.Add(batchKey, value);
		batches.Add(value);
		return value;
	}

	private static int GetBatchKey(SpineVatAsset asset, SpineVatAnimationClip segment, int subMeshIndex)
	{
		return ((((((Object)(object)asset != (Object)null) ? ((Object)asset).GetInstanceID() : 0) * 397) ^ (segment?.GetHashCode() ?? 0)) * 397) ^ subMeshIndex;
	}

	private IndirectBatch CreateBatch(SpineVatAsset asset, SpineVatAnimationClip segment, int subMeshIndex)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Expected O, but got Unknown
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Expected O, but got Unknown
		Mesh val = (((Object)(object)segment.bakedMesh != (Object)null) ? segment.bakedMesh : asset.bakedMesh);
		if ((Object)(object)val == (Object)null || subMeshIndex < 0 || subMeshIndex >= val.subMeshCount)
		{
			return null;
		}
		Material sourceMaterial = GetSourceMaterial(asset, segment, subMeshIndex);
		Material val2 = (((Object)(object)sourceMaterial != (Object)null) ? new Material(sourceMaterial) : new Material(indirectShader));
		((Object)val2).name = $"{((Object)asset).name}_{segment.animationName}_{subMeshIndex}_IndirectMaterial";
		val2.shader = indirectShader;
		val2.enableInstancing = true;
		CopyTextureIfPresent(sourceMaterial, val2, "_MainTex");
		CopyTextureIfPresent(sourceMaterial, val2, AlphaTexId);
		CopyTextureIfPresent(sourceMaterial, val2, "_MaskTex");
		CopyColorIfPresent(sourceMaterial, val2, ColorId);
		CopyFloatIfPresent(sourceMaterial, val2, EnableExternalAlphaId);
		EnsureDefaultColor(val2, sourceMaterial);
		bool straightAlphaInput = GetStraightAlphaInput(sourceMaterial, val2);
		SetKeyword(val2, "_STRAIGHT_ALPHA_INPUT", straightAlphaInput);
		if (val2.HasProperty(StraightAlphaInputId))
		{
			val2.SetFloat(StraightAlphaInputId, straightAlphaInput ? 1f : 0f);
		}
		bool flag = (Object)(object)sourceMaterial != (Object)null && ((sourceMaterial.HasProperty(EnableExternalAlphaId) && sourceMaterial.GetFloat(EnableExternalAlphaId) > 0.5f) || (sourceMaterial.HasProperty(AlphaTexId) && (Object)(object)sourceMaterial.GetTexture(AlphaTexId) != (Object)null));
		if (val2.HasProperty(EnableExternalAlphaId))
		{
			val2.SetFloat(EnableExternalAlphaId, flag ? 1f : 0f);
		}
		ApplyMaterialRuntimeSettings(val2, asset);
		if (baseSortingOrder > 0)
		{
			int num = baseSortingOrder;
			if (num > 1000)
			{
				num = 100;
			}
			val2.renderQueue = Mathf.Clamp(3000 + num, 2500, 5000);
		}
		return new IndirectBatch
		{
			Asset = asset,
			Segment = segment,
			Mesh = val,
			SubMeshIndex = subMeshIndex,
			Material = val2,
			PropertyBlock = new MaterialPropertyBlock()
		};
	}

	private static Material GetSourceMaterial(SpineVatAsset asset, SpineVatAnimationClip segment, int subMeshIndex)
	{
		if (segment.sharedMaterials != null && subMeshIndex < segment.sharedMaterials.Length && (Object)(object)segment.sharedMaterials[subMeshIndex] != (Object)null)
		{
			return segment.sharedMaterials[subMeshIndex];
		}
		if (asset.sharedMaterials != null && subMeshIndex < asset.sharedMaterials.Length)
		{
			return asset.sharedMaterials[subMeshIndex];
		}
		return null;
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

	private void ResetBatches()
	{
		for (int i = 0; i < batches.Count; i++)
		{
			batches[i].Count = 0;
		}
	}

	private void FillBatchesFromRegisteredInstances()
	{
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		for (int num = registeredSpineVats.Count - 1; num >= 0; num--)
		{
			SpineVat spineVat = registeredSpineVats[num];
			SpineVat.IndirectRenderData data;
			if ((Object)(object)spineVat == (Object)null)
			{
				registeredSpineVats.RemoveAt(num);
			}
			else if (((Behaviour)spineVat).isActiveAndEnabled && ((Component)spineVat).gameObject.activeInHierarchy && spineVat.TryGetIndirectRenderData(out data))
			{
				Mesh val = ((data.Segment != null && (Object)(object)data.Segment.bakedMesh != (Object)null) ? data.Segment.bakedMesh : (((Object)(object)data.Asset != (Object)null) ? data.Asset.bakedMesh : null));
				if (!((Object)(object)val == (Object)null))
				{
					GpuInstanceData data2 = new GpuInstanceData
					{
						PositionScaleX = new Vector4(data.Position.x, data.Position.y, data.Position.z, data.ScaleX),
						FrameData = new Vector4((float)data.Frame, (float)data.NextFrame, data.Blend, (float)data.Segment.textureSliceIndex),
						ScaleYSort = new Vector4(data.ScaleY, data.SortY, 0f, 0f)
					};
					int num2 = Mathf.Max(1, val.subMeshCount);
					for (int i = 0; i < num2; i++)
					{
						GetOrCreateBatch(data.Asset, data.Segment, i)?.Add(data2);
					}
				}
			}
		}
	}

	private void UploadAndDrawBatches()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		Bounds drawBounds = GetDrawBounds();
		for (int i = 0; i < batches.Count; i++)
		{
			IndirectBatch indirectBatch = batches[i];
			if (indirectBatch.Count > 0)
			{
				indirectBatch.EnsureCapacity(indirectBatch.Count);
				indirectBatch.UploadArguments();
				ApplyBatchPropertyBlock(indirectBatch);
				ApplyMaterialRuntimeSettings(indirectBatch.Material, indirectBatch.Asset);
				Graphics.DrawMeshInstancedIndirect(indirectBatch.Mesh, indirectBatch.SubMeshIndex, indirectBatch.Material, drawBounds, indirectBatch.ArgumentsBuffer, 0, indirectBatch.PropertyBlock, shadowCastingMode, receiveShadows, ((Component)this).gameObject.layer, (Camera)null, (LightProbeUsage)1);
			}
		}
	}

	private void ApplyBatchPropertyBlock(IndirectBatch batch)
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		ConfigurePositionTexture((Texture)(object)batch.Asset.positionTextureArray);
		batch.PropertyBlock.Clear();
		batch.PropertyBlock.SetBuffer(VatInstancesId, batch.InstanceBuffer);
		batch.PropertyBlock.SetTexture(PositionTexId, (Texture)(object)batch.Asset.positionTextureArray);
		batch.PropertyBlock.SetFloat(VatFrameCountId, (float)Mathf.Max(1, batch.Asset.maxFrameCount));
		batch.PropertyBlock.SetFloat(VatVertexCountId, (float)Mathf.Max(1, batch.Asset.vertexCount));
		batch.PropertyBlock.SetVector(VatBoundsMinId, Vector4.op_Implicit(batch.Asset.boundsMin));
		batch.PropertyBlock.SetVector(VatBoundsSizeId, Vector4.op_Implicit(batch.Asset.boundsMax - batch.Asset.boundsMin));
		batch.PropertyBlock.SetFloat(UseYDepthSortId, useYDepthSort ? 1f : 0f);
		batch.PropertyBlock.SetVector(SortAxisMinMaxId, new Vector4(boundaryMin.y, Mathf.Max(boundaryMin.y + 0.001f, boundaryMax.y), 0f, 0f));
		batch.PropertyBlock.SetVector(SortDepthRangeId, new Vector4(depthSortZRange.x, depthSortZRange.y, 0f, 0f));
		batch.PropertyBlock.SetFloat(MeshVertexRgbNeedsVatAlphaId, batch.Asset.meshVertexRgbNeedsVatAlpha ? 1f : 0f);
	}

	private static void ConfigurePositionTexture(Texture texture)
	{
		if (!((Object)(object)texture == (Object)null))
		{
			texture.wrapMode = (TextureWrapMode)1;
			texture.filterMode = (FilterMode)0;
		}
	}

	private void ApplyMaterialRuntimeSettings(Material material, SpineVatAsset asset)
	{
		if (!((Object)(object)material == (Object)null))
		{
			if (material.HasProperty(ZWriteId))
			{
				material.SetFloat(ZWriteId, (useYDepthSort && writeDepthForYSort) ? 1f : 0f);
			}
			if (material.HasProperty(CutoffId))
			{
				material.SetFloat(CutoffId, alphaClipThreshold);
			}
			if (material.HasProperty(MeshVertexRgbNeedsVatAlphaId))
			{
				material.SetFloat(MeshVertexRgbNeedsVatAlphaId, ((Object)(object)asset != (Object)null && asset.meshVertexRgbNeedsVatAlpha) ? 1f : 0f);
			}
		}
	}

	private Bounds GetDrawBounds()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if (useCustomDrawBounds)
		{
			return customDrawBounds;
		}
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(boundaryMin.x, boundaryMin.y, spawnZ - drawBoundsPadding);
		Vector3 val2 = default(Vector3);
		((Vector3)(ref val2))._002Ector(boundaryMax.x, boundaryMax.y, spawnZ + drawBoundsPadding);
		val.x -= drawBoundsPadding;
		val.y -= drawBoundsPadding;
		val2.x += drawBoundsPadding;
		val2.y += drawBoundsPadding;
		Bounds result = default(Bounds);
		((Bounds)(ref result)).SetMinMax(val, val2);
		return result;
	}

	private void ReleaseBatches()
	{
		for (int i = 0; i < batches.Count; i++)
		{
			batches[i].Release();
		}
		batches.Clear();
		batchesByKey.Clear();
	}

	private int GetActiveBatchCount()
	{
		int num = 0;
		for (int i = 0; i < batches.Count; i++)
		{
			if (batches[i].Count > 0)
			{
				num++;
			}
		}
		return num;
	}

	private void HandleKeyboardInput()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		if (Input.GetKeyDown(toggleOverlayKey))
		{
			showOverlay = !showOverlay;
		}
		if (Input.GetKeyDown(previousCountKey))
		{
			SelectCountOffset(-1);
		}
		if (Input.GetKeyDown(nextCountKey))
		{
			SelectCountOffset(1);
		}
		if (Input.GetKeyDown(rebuildKey))
		{
			RebuildCurrentCount();
		}
		if (Input.GetKeyDown(clearKey))
		{
			Clear();
		}
	}

	private void SelectCountOffset(int offset)
	{
		EnsureValidCounts();
		currentCountIndex = Mathf.Clamp(currentCountIndex + offset, 0, testCounts.Length - 1);
		Rebuild(testCounts[currentCountIndex]);
	}

	private void UpdateFrameStats()
	{
		float unscaledDeltaTime = Time.unscaledDeltaTime;
		if (smoothedDeltaTime <= 0f)
		{
			smoothedDeltaTime = unscaledDeltaTime;
		}
		else
		{
			smoothedDeltaTime += (unscaledDeltaTime - smoothedDeltaTime) * 0.1f;
		}
	}

	private float GetFps()
	{
		if (!(smoothedDeltaTime > 0f))
		{
			return 0f;
		}
		return 1f / smoothedDeltaTime;
	}

	private float GetFrameMs()
	{
		return smoothedDeltaTime * 1000f;
	}

	private float GetOverlayScale()
	{
		float num = Mathf.Max(0.5f, overlayScale);
		if (autoScaleOverlay)
		{
			float num2 = ((Screen.height > 0) ? ((float)Screen.height / 1080f) : 1f);
			num *= Mathf.Clamp(num2, 1f, 3f);
		}
		return num;
	}

	private void EnsureValidCounts()
	{
		if (testCounts == null || testCounts.Length == 0)
		{
			testCounts = new int[4] { 1000, 2000, 5000, 10000 };
		}
		for (int i = 0; i < testCounts.Length; i++)
		{
			testCounts[i] = Mathf.Max(0, testCounts[i]);
		}
	}

	private void EnsureGuiStyles()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Expected O, but got Unknown
		if (overlayStyle == null || overlayStyle.fontSize != overlayFontSize)
		{
			overlayStyle = new GUIStyle(GUI.skin.label)
			{
				fontSize = overlayFontSize
			};
			overlayStyle.normal.textColor = Color.white;
			buttonStyle = new GUIStyle(GUI.skin.button)
			{
				fontSize = Mathf.Max(12, overlayFontSize - 2)
			};
		}
	}

	private static bool IsValidAsset(SpineVatAsset asset)
	{
		if ((Object)(object)asset != (Object)null && (Object)(object)asset.positionTextureArray != (Object)null && (Object)(object)asset.bakedMesh != (Object)null && asset.animations != null)
		{
			return asset.animations.Count > 0;
		}
		return false;
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
}
