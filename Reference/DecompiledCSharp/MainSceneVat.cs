using System;
using System.Collections;
using System.Collections.Generic;
using Module.Role;
using Module.UI;
using Module.Vat;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using YooAsset;
using cfg;

public class MainSceneVat : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IMainSceneView
{
	private enum VatCameraScaleMode
	{
		None,
		MatchCanvasScaler,
		FixedDesignHeight,
		FixedDesignWidth
	}

	private struct LuckyEchoTask
	{
		public CoinVat Coin;

		public float Remaining;
	}

	private struct QueuedSkillCoinToss
	{
		public CoinVat Coin;

		public int SourceRoleType;

		public float ScatterRadius;

		public bool SuppressLandingEffects;
	}

	private const string SpineVatShaderPath = "Assets/GameRes/VAT/SpineVatUnlit.shader";

	private const string SpriteVatShaderPath = "Assets/GameRes/VAT/SpriteSequenceVatUnlit.shader";

	private const string HelperAddVatAnimName = "animation";

	[Header("Scene")]
	[SerializeField]
	private SpriteRenderer bgImage;

	[SerializeField]
	private RectTransform guideFinger;

	[SerializeField]
	private Transform sceneBoxUI;

	[SerializeField]
	private Collider2D sceneBox;

	[Header("VAT World")]
	[SerializeField]
	private Transform vatWorldRoot;

	[SerializeField]
	private bool moveVatRenderPoolsToWorld = true;

	[SerializeField]
	private float vatWorldZ;

	[Header("VAT Coordinates")]
	[SerializeField]
	private float uiToWorldScale = 0.01f;

	[SerializeField]
	private bool allowVatCameraScale;

	[SerializeField]
	private VatCameraScaleMode cameraScaleMode;

	[SerializeField]
	private Vector2 cameraReferenceResolution = new Vector2(1440f, 2880f);

	[SerializeField]
	private bool updateCameraScaleOnResolutionChange = true;

	[SerializeField]
	private bool restoreCameraOnDestroy = true;

	[SerializeField]
	private Vector2 cameraOrthographicSizeLimit = new Vector2(0.1f, 100f);

	[Header("Performance")]
	[SerializeField]
	private VatPerformanceMode performanceMode = VatPerformanceMode.CentralTick;

	[SerializeField]
	private float spatialIndexCellSize = 120f;

	private bool preloadAllHelperVatAssets;

	private bool preloadVatSkillPrefabs;

	[SerializeField]
	private bool enableCoinGpuBatch = true;

	[SerializeField]
	[Range(4f, 128f)]
	private int coinGpuBatchYBuckets = 24;

	[SerializeField]
	[Range(1f, 128f)]
	private int skillCoinTossesPerFrame = 16;

	[SerializeField]
	[Range(1f, 32f)]
	private int initialCoinSpawnsPerFrame = 8;

	[Header("VAT Sorting")]
	[SerializeField]
	private string vatSortingLayerName;

	[SerializeField]
	private int backgroundSortingOrder;

	[SerializeField]
	private int coinSortingOrder = 1;

	[SerializeField]
	private int roleSortingOrder = 1;

	[SerializeField]
	private bool forceRoleAboveThrowables = true;

	[Header("Input")]
	[SerializeField]
	private float coinClickRadius = 50f;

	[SerializeField]
	private float roleClickRadius = 90f;

	[SerializeField]
	private float clickMaxScreenDistance = 20f;

	[Header("Coin Prefabs For Frames")]
	[SerializeField]
	private GameObject coinCopperPrefab;

	[SerializeField]
	private GameObject coinSilverPrefab;

	[SerializeField]
	private GameObject coinGoldPrefab;

	[SerializeField]
	private GameObject coinPurplePrefab;

	[Header("Pools")]
	[SerializeField]
	private Transform rolePool;

	[SerializeField]
	private Transform coinPool;

	[SerializeField]
	private Transform effectPool;

	[SerializeField]
	private RectTransform huanxingPool;

	[SerializeField]
	private RectTransform TextPool;

	[SerializeField]
	private Transform coins_smokePool;

	[SerializeField]
	private RectTransform ParticlePool;

	[Header("Spawn")]
	[SerializeField]
	private int initialCopperCount = 3;

	[SerializeField]
	private float minSpawnDistance = 60f;

	[SerializeField]
	private float minLandingRadius = 60f;

	[SerializeField]
	private float maxLandingRadius = 120f;

	[SerializeField]
	private float sceneBoundaryPadding = 40f;

	[SerializeField]
	private float roleRenderScale = 1f;

	[SerializeField]
	private float coinRenderScale = 1f;

	private bool enableCoinAvoidRects;

	[Header("Floating Text")]
	[SerializeField]
	private int TextCurrentCount;

	[SerializeField]
	public int TextMaxCount = 45;

	private readonly List<CoinVat> activeCoins = new List<CoinVat>();

	private readonly List<RoleVat> activeRoles = new List<RoleVat>();

	private readonly HashSet<RoleVat> changingRoles = new HashSet<RoleVat>();

	private readonly List<SpineVat> helperAddVatPool = new List<SpineVat>();

	private readonly List<CoinVat> sortedCoinsCache = new List<CoinVat>();

	private readonly List<Rect> coinAvoidRects = new List<Rect>();

	private readonly HashSet<CoinVat> swipedCoinsThisFrame = new HashSet<CoinVat>();

	private readonly Dictionary<string, Transform> effectPoolDict = new Dictionary<string, Transform>();

	private readonly Dictionary<string, List<GameObject>> sceneEffectObjectPools = new Dictionary<string, List<GameObject>>();

	private readonly Dictionary<string, List<Action<SpineVatAsset>>> pendingSkillVatCallbacks = new Dictionary<string, List<Action<SpineVatAsset>>>();

	private readonly Dictionary<CoinType, int> coinSyncVersions = new Dictionary<CoinType, int>();

	private readonly Queue<QueuedSkillCoinToss> queuedSkillCoinTosses = new Queue<QueuedSkillCoinToss>();

	private readonly HashSet<CoinVat> queuedSkillCoinSet = new HashSet<CoinVat>();

	private readonly VatSpatialIndex coinSpatialIndex = new VatSpatialIndex();

	private readonly List<CoinVat> coinQueryScratch = new List<CoinVat>();

	private readonly List<CoinVat> chainReactionScratch = new List<CoinVat>();

	private readonly VatRuntimeAssets runtimeAssets = new VatRuntimeAssets();

	private readonly VatFloatingTextPool floatingTextPool = new VatFloatingTextPool();

	private Vector2 boundaryMin;

	private Vector2 boundaryMax;

	private Vector2 swipeStartPos;

	private bool isSwipeTracking;

	private bool pointerStartedOverBlockedUi;

	private int activeTouchFingerId = -1;

	private int lastHandledPointerClickFrame = -1;

	private Vector2 cachedDensePos = Vector2.zero;

	private int lastDensePosFrame = -1;

	private Shader spineVatShader;

	private Shader spriteVatShader;

	private AssetHandle spineVatShaderHandle;

	private AssetHandle spriteVatShaderHandle;

	private bool ownsVatWorldRoot;

	private Camera vatCamera;

	private float originalCameraOrthographicSize;

	private bool hasOriginalCameraOrthographicSize;

	private int lastCameraScaleScreenWidth = -1;

	private int lastCameraScaleScreenHeight = -1;

	private VatCameraScaleMode lastCameraScaleMode = (VatCameraScaleMode)(-1);

	private Vector2 lastCameraReferenceResolution;

	private float lastCameraUiToWorldScale = -1f;

	private VatPerformanceMode activePerformanceMode;

	private SpineVatIndirectManager spineVatBatchRenderer;

	private CoinVatBatchRenderer coinVatBatchRenderer;

	private DiceWorldController diceWorldController;

	private Coroutine coinSpawnCoroutine;

	private int coinSpawnVersion;

	private Coroutine roleSpawnCoroutine;

	private bool coinSpatialIndexDirty = true;

	private int coinSpatialIndexFrame = -1;

	private bool gpuBatchFallbackLogged;

	private readonly List<LuckyEchoTask> luckyEchoTasks = new List<LuckyEchoTask>();

	private readonly List<RaycastResult> uiRaycastResults = new List<RaycastResult>();

	private static readonly Vector2[] SpawnPositions = (Vector2[])(object)new Vector2[3]
	{
		new Vector2(-95f, 295f),
		new Vector2(-87f, -200f),
		new Vector2(213f, -150f)
	};

	private const float EvolutionMergeDuration = 1f;

	public bool isAddGongfu;

	public bool isAddDashe;

	public const int STEP_2_MONEY = 25;

	public bool IsInitialized { get; private set; }

	public float UiToWorldScale => Mathf.Max(0.0001f, uiToWorldScale);

	public VatPerformanceMode PerformanceMode => activePerformanceMode;

	public bool UseCentralTick => activePerformanceMode != VatPerformanceMode.Compatibility;

	public bool UseGpuBatch => activePerformanceMode == VatPerformanceMode.GpuBatch;

	public bool UseSceneFloatingTextPool => UseCentralTick;

	public Shader SpineVatShader => spineVatShader;

	public Shader SpriteVatShader => spriteVatShader;

	public string VatSortingLayerName => vatSortingLayerName;

	public int ThrowableSortingOrder => coinSortingOrder;

	public float ThrowableRenderScale => coinRenderScale;

	public int EffectSortingOrder => Mathf.Max(coinSortingOrder, roleSortingOrder) + 1;

	public bool EnableCoinAvoidRects
	{
		get
		{
			return enableCoinAvoidRects;
		}
		set
		{
			enableCoinAvoidRects = value;
			if (!enableCoinAvoidRects)
			{
				coinAvoidRects.Clear();
			}
		}
	}

	public event Func<float> coinFlipComplete;

	public void ApplyDebugVisualSettings()
	{
		for (int i = 0; i < activeCoins.Count; i++)
		{
			if ((Object)(object)activeCoins[i] != (Object)null)
			{
				activeCoins[i].RefreshDebugVisuals();
			}
		}
		for (int j = 0; j < activeRoles.Count; j++)
		{
			if ((Object)(object)activeRoles[j] != (Object)null)
			{
				activeRoles[j].RefreshDebugVisuals();
			}
		}
		if ((Object)(object)TextPool != (Object)null)
		{
			((Component)TextPool).gameObject.SetActive(VatDebugSettings.ShowFloatingText);
		}
		if ((Object)(object)coins_smokePool != (Object)null)
		{
			((Component)coins_smokePool).gameObject.SetActive(VatDebugSettings.ShowCoinSmoke);
		}
		ApplySharedEffectPoolVisuals();
	}

	private void ApplySharedEffectPoolVisuals()
	{
		foreach (KeyValuePair<string, List<GameObject>> sceneEffectObjectPool in sceneEffectObjectPools)
		{
			bool visible = VatDebugSettings.AreRoleSkillVisualsVisible(GetEffectPoolRoleType(sceneEffectObjectPool.Key));
			List<GameObject> value = sceneEffectObjectPool.Value;
			for (int num = value.Count - 1; num >= 0; num--)
			{
				GameObject val = value[num];
				if ((Object)(object)val == (Object)null)
				{
					value.RemoveAt(num);
				}
				else
				{
					VatEffectVisualCache.SetVisible(val, visible);
				}
			}
		}
	}

	private static int GetEffectPoolRoleType(string poolKey)
	{
		string text = poolKey;
		int num = poolKey.IndexOf(':');
		if (num >= 0)
		{
			text = poolKey.Substring(0, num);
		}
		switch (text)
		{
		case "TornadoPool":
			return 2;
		case "MagicianPool":
			return 3;
		case "GongfuPool":
			return 4;
		case "LanqiuPool":
			return 5;
		case "DaShePool":
			return 6;
		case "DajiPool":
			return 7;
		case "IronPool":
		case "IronBoomPool":
		case "IronHePool":
			return 8;
		case "HalandPool":
		case "ZuqiuPool":
			return 9;
		case "DiceHitPool":
		case "DiceMovePool":
			return 10;
		default:
			return 0;
		}
	}

	public float UiToWorldDistance(float value)
	{
		return value * UiToWorldScale;
	}

	public Vector2 UiToWorldPoint(Vector2 point)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return point * UiToWorldScale;
	}

	public Vector3 WorldToUiPosition(Vector3 worldPosition)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		Transform transform = ((Component)this).transform;
		RectTransform val = (RectTransform)(object)((transform is RectTransform) ? transform : null);
		if ((Object)(object)val != (Object)null)
		{
			Vector2 val2 = WorldToRectLocalPosition(worldPosition, val);
			return new Vector3(val2.x, val2.y, 0f);
		}
		float num = UiToWorldScale;
		return new Vector3(worldPosition.x / num, worldPosition.y / num, 0f);
	}

	public Vector3 WorldToTextLocalPosition(Vector3 worldPosition)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		RectTransform val = (RectTransform)(((Object)(object)TextPool != (Object)null) ? ((object)TextPool) : ((object)/*isinst with value type is only supported in some contexts*/));
		if ((Object)(object)val == (Object)null)
		{
			return WorldToUiPosition(worldPosition);
		}
		Vector2 val2 = WorldToRectLocalPosition(worldPosition, val);
		return new Vector3(val2.x, val2.y, 0f);
	}

	public void PlaceUiRectAtWorldPosition(RectTransform rect, Vector3 worldPosition)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)rect == (Object)null))
		{
			Transform parent = ((Transform)rect).parent;
			RectTransform val = (RectTransform)(object)((parent is RectTransform) ? parent : null);
			if ((Object)(object)val != (Object)null)
			{
				Vector2 val2 = WorldToRectLocalPosition(worldPosition, val);
				((Transform)rect).localPosition = new Vector3(val2.x, val2.y, ((Transform)rect).localPosition.z);
			}
			else
			{
				Vector2 val3 = WorldToScreenPoint(worldPosition);
				((Transform)rect).position = new Vector3(val3.x, val3.y, ((Transform)rect).position.z);
			}
		}
	}

	public Rect UiToWorldRect(Rect rect)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		float num = UiToWorldScale;
		return Rect.MinMaxRect(((Rect)(ref rect)).xMin * num, ((Rect)(ref rect)).yMin * num, ((Rect)(ref rect)).xMax * num, ((Rect)(ref rect)).yMax * num);
	}

	private void ApplyVatCameraScale(bool force)
	{
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		if (!allowVatCameraScale)
		{
			cameraScaleMode = VatCameraScaleMode.None;
		}
		else
		{
			if (cameraScaleMode == VatCameraScaleMode.None || (!force && !updateCameraScaleOnResolutionChange))
			{
				return;
			}
			int num = Mathf.Max(1, Screen.width);
			int num2 = Mathf.Max(1, Screen.height);
			if (!force && num == lastCameraScaleScreenWidth && num2 == lastCameraScaleScreenHeight && cameraScaleMode == lastCameraScaleMode && cameraReferenceResolution == lastCameraReferenceResolution && Mathf.Approximately(UiToWorldScale, lastCameraUiToWorldScale))
			{
				return;
			}
			Camera val = GetVatCamera();
			if (!((Object)(object)val == (Object)null) && val.orthographic)
			{
				float num3 = CalculateVatCameraOrthographicSize(val, num, num2);
				if (!(num3 <= 0f))
				{
					val.orthographicSize = num3;
					lastCameraScaleScreenWidth = num;
					lastCameraScaleScreenHeight = num2;
					lastCameraScaleMode = cameraScaleMode;
					lastCameraReferenceResolution = cameraReferenceResolution;
					lastCameraUiToWorldScale = UiToWorldScale;
				}
			}
		}
	}

	private Camera GetVatCamera()
	{
		if ((Object)(object)vatCamera != (Object)null)
		{
			return vatCamera;
		}
		vatCamera = Camera.main;
		if ((Object)(object)vatCamera != (Object)null && vatCamera.orthographic && !hasOriginalCameraOrthographicSize)
		{
			originalCameraOrthographicSize = vatCamera.orthographicSize;
			hasOriginalCameraOrthographicSize = true;
		}
		return vatCamera;
	}

	private float CalculateVatCameraOrthographicSize(Camera camera, int screenWidth, int screenHeight)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = GetCameraReferenceResolution();
		float num = UiToWorldScale;
		switch (cameraScaleMode)
		{
		case VatCameraScaleMode.FixedDesignHeight:
			return ClampVatCameraOrthographicSize(val.y * num * 0.5f);
		case VatCameraScaleMode.FixedDesignWidth:
			return ClampVatCameraOrthographicSize(val.x * num * 0.5f / Mathf.Max(0.0001f, camera.aspect));
		case VatCameraScaleMode.MatchCanvasScaler:
		{
			float canvasScaleFactor = GetCanvasScaleFactor(screenWidth, screenHeight, val);
			if (canvasScaleFactor <= 0f)
			{
				return 0f;
			}
			return ClampVatCameraOrthographicSize((float)screenHeight * num * 0.5f / canvasScaleFactor);
		}
		default:
			return camera.orthographicSize;
		}
	}

	private Vector2 GetCameraReferenceResolution()
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		float num = ((cameraReferenceResolution.x > 0f) ? cameraReferenceResolution.x : 1440f);
		float num2 = ((cameraReferenceResolution.y > 0f) ? cameraReferenceResolution.y : 2880f);
		return new Vector2(num, num2);
	}

	private float GetCanvasScaleFactor(int screenWidth, int screenHeight, Vector2 fallbackReferenceResolution)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Invalid comparison between Unknown and I4
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Invalid comparison between Unknown and I4
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Invalid comparison between Unknown and I4
		CanvasScaler componentInParent = ((Component)this).GetComponentInParent<CanvasScaler>();
		if ((Object)(object)componentInParent == (Object)null)
		{
			return (float)screenWidth / Mathf.Max(1f, fallbackReferenceResolution.x);
		}
		if ((int)componentInParent.uiScaleMode == 0)
		{
			return Mathf.Max(0.0001f, componentInParent.scaleFactor);
		}
		if ((int)componentInParent.uiScaleMode != 1)
		{
			return Mathf.Max(0.0001f, componentInParent.scaleFactor);
		}
		Vector2 val = componentInParent.referenceResolution;
		if (val.x <= 0f || val.y <= 0f)
		{
			val = fallbackReferenceResolution;
		}
		float num = (float)screenWidth / Mathf.Max(1f, val.x);
		float num2 = (float)screenHeight / Mathf.Max(1f, val.y);
		ScreenMatchMode screenMatchMode = componentInParent.screenMatchMode;
		if ((int)screenMatchMode != 1)
		{
			if ((int)screenMatchMode == 2)
			{
				return Mathf.Max(num, num2);
			}
			float num3 = Mathf.Log(Mathf.Max(0.0001f, num), 2f);
			float num4 = Mathf.Log(Mathf.Max(0.0001f, num2), 2f);
			float num5 = Mathf.Lerp(num3, num4, componentInParent.matchWidthOrHeight);
			return Mathf.Pow(2f, num5);
		}
		return Mathf.Min(num, num2);
	}

	private float ClampVatCameraOrthographicSize(float value)
	{
		float num = Mathf.Max(0.0001f, Mathf.Min(cameraOrthographicSizeLimit.x, cameraOrthographicSizeLimit.y));
		float num2 = Mathf.Max(num, Mathf.Max(cameraOrthographicSizeLimit.x, cameraOrthographicSizeLimit.y));
		return Mathf.Clamp(value, num, num2);
	}

	private void RestoreVatCameraScale()
	{
		if (restoreCameraOnDestroy && hasOriginalCameraOrthographicSize && !((Object)(object)vatCamera == (Object)null))
		{
			vatCamera.orthographicSize = originalCameraOrthographicSize;
		}
	}

	private void Awake()
	{
		Debug.Log((object)"[MainSceneVat] Awake");
		VatRuntimeAssets.Instance = runtimeAssets;
		if (forceRoleAboveThrowables && roleSortingOrder <= coinSortingOrder + 10000)
		{
			roleSortingOrder = coinSortingOrder + 10000 + 10;
		}
		activePerformanceMode = performanceMode;
		if (!allowVatCameraScale)
		{
			cameraScaleMode = VatCameraScaleMode.None;
		}
		if ((Object)(object)guideFinger != (Object)null)
		{
			((Component)guideFinger).gameObject.SetActive(false);
		}
		EnsurePools();
		ApplyBackgroundSorting();
		CalculateBoundary();
		ApplyVatCameraScale(force: true);
	}

	private void Start()
	{
		Debug.Log((object)"[MainSceneVat] Start");
		ApplyVatCameraScale(force: true);
		((MonoBehaviour)this).StartCoroutine(InitializeVatScene());
		if ((Object)(object)GameDataManager.Instance != (Object)null)
		{
			GameDataManager.Instance.OnWorldChanged += OnWorldChanged;
		}
		RefreshBackground();
		if ((Object)(object)SoundManager.Instance != (Object)null)
		{
			int worldId = ((!((Object)(object)GameDataManager.Instance != (Object)null)) ? 1 : GameDataManager.Instance.GetCurrentWorldId());
			SoundManager.Instance.PlayBGMForWorld(worldId);
		}
		if (PlayerPrefs.GetInt("guide_step_3", 0) == 0)
		{
			int currentWorldHelperUpgradeId = Constants.GetCurrentWorldHelperUpgradeId();
			if ((Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null && SingletonMono<UpgradeManager>.Instance.GetUpgradeLevel(currentWorldHelperUpgradeId, "itemUpgrade1") > 1)
			{
				CompleteGuide3();
				return;
			}
			SingletonMono<UpgradeManager>.Instance.OnGoldChanged -= OnGoldChangedForGuide3;
			SingletonMono<UpgradeManager>.Instance.OnGoldChanged += OnGoldChangedForGuide3;
		}
	}

	private void OnEnable()
	{
		SetVatWorldVisible(visible: true);
	}

	private void OnDisable()
	{
		SetVatWorldVisible(visible: false);
	}

	public void SetVatWorldVisible(bool visible)
	{
		if ((Object)(object)vatWorldRoot != (Object)null && (Object)(object)vatWorldRoot != (Object)(object)((Component)this).transform)
		{
			((Component)vatWorldRoot).gameObject.SetActive(visible);
		}
	}

	private void OnDestroy()
	{
		if ((Object)(object)GameDataManager.Instance != (Object)null)
		{
			GameDataManager.Instance.OnWorldChanged -= OnWorldChanged;
		}
		RestoreVatCameraScale();
		ReleaseVatShaderHandles();
		pendingSkillVatCallbacks.Clear();
		runtimeAssets.Release();
		floatingTextPool.Clear();
		ReleaseVatWorldRoot();
		if ((Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null)
		{
			SingletonMono<UpgradeManager>.Instance.OnGoldChanged -= OnGoldChangedForGuide3;
		}
	}

	private void OnWorldChanged(int previousWorldId, int currentWorldId)
	{
		RefreshBackground();
		if ((Object)(object)SoundManager.Instance != (Object)null)
		{
			SoundManager.Instance.PlayBGMForWorld(currentWorldId);
		}
	}

	private void RefreshBackground()
	{
		if ((Object)(object)bgImage != (Object)null && (Object)(object)GameDataManager.Instance != (Object)null)
		{
			LoadBgImage(GameDataManager.Instance.GetCurrentDeskId());
		}
	}

	private void Update()
	{
		ApplyVatCameraScale(force: false);
		float deltaTime = Time.deltaTime;
		CheckSwipeToThrow();
		if (UseCentralTick)
		{
			TickVatObjects(deltaTime);
		}
		TickQueuedSkillCoinTosses();
		TickLuckyEcho(deltaTime);
		if (UseSceneFloatingTextPool)
		{
			floatingTextPool.Tick(deltaTime);
		}
	}

	private void LateUpdate()
	{
		swipedCoinsThisFrame.Clear();
	}

	private void TickVatObjects(float deltaTime)
	{
		for (int i = 0; i < activeRoles.Count; i++)
		{
			RoleVat roleVat = activeRoles[i];
			if ((Object)(object)roleVat != (Object)null)
			{
				roleVat.TickExternal(deltaTime);
			}
		}
		for (int j = 0; j < activeCoins.Count; j++)
		{
			CoinVat coinVat = activeCoins[j];
			if ((Object)(object)coinVat != (Object)null)
			{
				coinVat.TickExternal(deltaTime);
			}
		}
		diceWorldController?.TickExternal(deltaTime);
	}

	public void LoadBgImage(int bgId)
	{
		SingletonMono<ResManager>.Instance.LoadSpriteAsync($"Assets/GameRes/MapRes/fight_bg_{bgId}.png", delegate(Sprite sprite)
		{
			if ((Object)(object)bgImage != (Object)null && (Object)(object)sprite != (Object)null)
			{
				bgImage.sprite = sprite;
			}
		}, isAtlas: false);
	}

	public void InitializeCoins()
	{
		if (!IsInitialized)
		{
			return;
		}
		CancelInitialCoinSpawn();
		if ((Object)(object)diceWorldController != (Object)null)
		{
			diceWorldController.InitializeThrowables();
			return;
		}
		int num = 0;
		int silverCount = 0;
		int goldCount = 0;
		int purpleCount = 0;
		if ((Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null && SingletonMono<UpgradeManager>.Instance.IsInitialized)
		{
			num = SingletonMono<UpgradeManager>.Instance.GetCurrencyFieldCount(2);
			silverCount = SingletonMono<UpgradeManager>.Instance.GetCurrencyFieldCount(3);
			goldCount = SingletonMono<UpgradeManager>.Instance.GetCurrencyFieldCount(4);
			purpleCount = SingletonMono<UpgradeManager>.Instance.GetCurrencyFieldCount(5);
		}
		if (num <= 0)
		{
			num = (((Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null) ? SingletonMono<UpgradeManager>.Instance.GetInitialCopperCount() : initialCopperCount);
		}
		int spawnVersion = coinSpawnVersion;
		coinSpawnCoroutine = ((MonoBehaviour)this).StartCoroutine(SpawnInitialCoinsSequentially(num, silverCount, goldCount, purpleCount, spawnVersion));
	}

	private IEnumerator SpawnInitialCoinsSequentially(int copperCount, int silverCount, int goldCount, int purpleCount, int spawnVersion)
	{
		int spawnsThisFrame = 0;
		int spawnBudget = Mathf.Max(1, initialCoinSpawnsPerFrame);
		if (copperCount > 0)
		{
			yield return runtimeAssets.EnsureCoinVatAssetLoaded(CoinType.Copper, GetCoinEvolutionTier(CoinType.Copper));
			if (spawnVersion != coinSpawnVersion)
			{
				yield break;
			}
		}
		for (int i = 0; i < copperCount; i++)
		{
			SpawnCoin(CoinType.Copper, (copperCount <= 3) ? i : (-1), null, null, playSpawnEffect: false, notifyCoinCountChanged: false);
			int num = spawnsThisFrame + 1;
			spawnsThisFrame = num;
			if (num >= spawnBudget)
			{
				spawnsThisFrame = 0;
				yield return null;
				if (spawnVersion != coinSpawnVersion)
				{
					yield break;
				}
			}
		}
		CoinType[] remainingCoinTypes = new CoinType[3]
		{
			CoinType.Silver,
			CoinType.Gold,
			CoinType.Purple
		};
		int[] remainingCoinCounts = new int[3] { silverCount, goldCount, purpleCount };
		for (int i = 0; i < remainingCoinTypes.Length; i++)
		{
			if (remainingCoinCounts[i] > 0)
			{
				yield return runtimeAssets.EnsureCoinVatAssetLoaded(remainingCoinTypes[i], GetCoinEvolutionTier(remainingCoinTypes[i]));
				if (spawnVersion != coinSpawnVersion)
				{
					yield break;
				}
			}
			for (int j = 0; j < remainingCoinCounts[i]; j++)
			{
				SpawnCoin(remainingCoinTypes[i], -1, null, null, playSpawnEffect: false, notifyCoinCountChanged: false);
				int num = spawnsThisFrame + 1;
				spawnsThisFrame = num;
				if (num >= spawnBudget)
				{
					spawnsThisFrame = 0;
					yield return null;
					if (spawnVersion != coinSpawnVersion)
					{
						yield break;
					}
				}
			}
		}
		yield return null;
		if (spawnVersion == coinSpawnVersion)
		{
			coinSpawnCoroutine = null;
			SingletonMono<UpgradeManager>.Instance?.NotifyCoinCountChanged();
		}
	}

	private void CancelInitialCoinSpawn()
	{
		coinSpawnVersion++;
		if (coinSpawnCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(coinSpawnCoroutine);
			coinSpawnCoroutine = null;
		}
	}

	public void SyncCoinPopulation(CoinType coinType, bool spawnAtCenter = false, int? forcedDesired = null, int? forcedEvolutionTier = null)
	{
		int value;
		int num = ((!coinSyncVersions.TryGetValue(coinType, out value)) ? 1 : (value + 1));
		coinSyncVersions[coinType] = num;
		int coinEvolutionTier = GetCoinEvolutionTier(coinType);
		int num2 = ((!spawnAtCenter) ? coinEvolutionTier : (forcedEvolutionTier ?? (coinEvolutionTier + 1)));
		bool num3 = !runtimeAssets.IsCoinVatAssetLoaded(coinType, coinEvolutionTier);
		bool flag = num2 != coinEvolutionTier && !runtimeAssets.IsCoinVatAssetLoaded(coinType, num2);
		if (num3 || flag)
		{
			((MonoBehaviour)this).StartCoroutine(EnsureCoinVatAssetsAndSync(coinType, spawnAtCenter, forcedDesired, forcedEvolutionTier, coinEvolutionTier, num2, num));
		}
		else
		{
			SyncCoinPopulationLoaded(coinType, spawnAtCenter, forcedDesired, forcedEvolutionTier, coinEvolutionTier);
		}
	}

	private IEnumerator EnsureCoinVatAssetsAndSync(CoinType coinType, bool spawnAtCenter, int? forcedDesired, int? forcedEvolutionTier, int currentTier, int targetTier, int requestVersion)
	{
		yield return runtimeAssets.EnsureCoinVatAssetLoaded(coinType, currentTier);
		if (targetTier != currentTier)
		{
			yield return runtimeAssets.EnsureCoinVatAssetLoaded(coinType, targetTier);
		}
		if (coinSyncVersions.TryGetValue(coinType, out var value) && value == requestVersion)
		{
			SyncCoinPopulationLoaded(coinType, spawnAtCenter, forcedDesired, forcedEvolutionTier, currentTier);
		}
	}

	private void SyncCoinPopulationLoaded(CoinType coinType, bool spawnAtCenter, int? forcedDesired, int? forcedEvolutionTier, int currentTier)
	{
		if ((Object)(object)SingletonMono<UpgradeManager>.Instance == (Object)null)
		{
			return;
		}
		int coinEvolutionUpgradeId = UpgradeManager.GetCoinEvolutionUpgradeId(coinType);
		int num = forcedDesired ?? Mathf.Max(0, SingletonMono<UpgradeManager>.Instance.GetCurrencyFieldCount(coinEvolutionUpgradeId));
		int num2 = CountEffectiveCoins(coinType);
		bool flag = HasEvolutionMergingCoins(coinType);
		if (spawnAtCenter && !flag)
		{
			PlayCoinEvolutionMerge(coinType, num, forcedEvolutionTier, currentTier);
		}
		else
		{
			if (flag)
			{
				return;
			}
			if (num2 < num)
			{
				int num3 = num - num2;
				for (int i = 0; i < num3; i++)
				{
					SpawnCoin(coinType);
				}
			}
			else
			{
				if (num2 <= num)
				{
					return;
				}
				int num4 = num2 - num;
				for (int j = 0; j < 2; j++)
				{
					if (num4 <= 0)
					{
						break;
					}
					int num5 = activeCoins.Count - 1;
					while (num5 >= 0 && num4 > 0)
					{
						CoinVat coinVat = activeCoins[num5];
						if (!((Object)(object)coinVat == (Object)null) && coinVat.coinType == coinType && coinVat.State != CoinState.Evolving && !coinVat.IsEvolutionMerging && !coinVat.IsEvolutionShaking)
						{
							bool flag2 = coinVat.State == CoinState.Idle;
							if (j != 0 || flag2)
							{
								RemoveCoin(coinVat, notifyCoinCountChanged: false);
								num4--;
							}
						}
						num5--;
					}
				}
				SingletonMono<UpgradeManager>.Instance.NotifyCoinCountChanged();
			}
		}
	}

	private int CountEffectiveCoins(CoinType coinType)
	{
		int num = 0;
		for (int i = 0; i < activeCoins.Count; i++)
		{
			CoinVat coinVat = activeCoins[i];
			if ((Object)(object)coinVat != (Object)null && coinVat.coinType == coinType && !coinVat.IsEvolutionMerging)
			{
				num++;
			}
		}
		return num;
	}

	private bool HasEvolutionMergingCoins(CoinType coinType)
	{
		for (int i = 0; i < activeCoins.Count; i++)
		{
			CoinVat coinVat = activeCoins[i];
			if ((Object)(object)coinVat != (Object)null && coinVat.coinType == coinType && coinVat.IsEvolutionMerging)
			{
				return true;
			}
		}
		return false;
	}

	private void PlayCoinEvolutionMerge(CoinType coinType, int desiredCount, int? forcedEvolutionTier = null, int? forcedCurrentTier = null)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		Vector2 sceneCenterPosition = GetSceneCenterPosition();
		int num = forcedCurrentTier ?? GetCoinEvolutionTier(coinType);
		int num2 = forcedEvolutionTier ?? (num + 1);
		num2 = Mathf.Max(num, num2);
		List<CoinVat> list = new List<CoinVat>(desiredCount);
		for (int i = 0; i < activeCoins.Count; i++)
		{
			CoinVat coinVat = activeCoins[i];
			if ((Object)(object)coinVat != (Object)null && coinVat.coinType == coinType && coinVat.IsEvolutionShaking)
			{
				list.Add(coinVat);
			}
		}
		int num3 = Mathf.Max(0, desiredCount - list.Count);
		for (int j = 0; j < num3; j++)
		{
			CoinVat coinVat2 = SpawnCoin(coinType, -1, sceneCenterPosition, num);
			if ((Object)(object)coinVat2 != (Object)null)
			{
				coinVat2.StartEvolutionShake(1f, num2);
				list.Add(coinVat2);
			}
		}
		List<CoinVat> list2 = new List<CoinVat>();
		for (int k = 0; k < activeCoins.Count; k++)
		{
			CoinVat coinVat3 = activeCoins[k];
			if ((Object)(object)coinVat3 != (Object)null && coinVat3.coinType == coinType && !coinVat3.IsEvolutionShaking)
			{
				list2.Add(coinVat3);
			}
		}
		if (list2.Count == 0)
		{
			for (int l = 0; l < list.Count; l++)
			{
				CoinVat coinVat4 = list[l];
				if ((Object)(object)coinVat4 != (Object)null)
				{
					coinVat4.StopEvolutionShake();
				}
			}
			SingletonMono<UpgradeManager>.Instance?.NotifyCoinCountChanged();
			return;
		}
		for (int m = 0; m < list2.Count; m++)
		{
			CoinVat coinVat5 = list2[m];
			if ((Object)(object)coinVat5 != (Object)null)
			{
				coinVat5.StartEvolutionMergeMove(sceneCenterPosition);
			}
		}
		((MonoBehaviour)this).StartCoroutine(FinishCoinEvolutionMerge(list2, list));
	}

	private IEnumerator FinishCoinEvolutionMerge(List<CoinVat> mergingCoins, List<CoinVat> centerCoins)
	{
		yield return (object)new WaitForSeconds(1f);
		for (int i = 0; i < mergingCoins.Count; i++)
		{
			CoinVat coinVat = mergingCoins[i];
			if ((Object)(object)coinVat != (Object)null)
			{
				RemoveCoin(coinVat, notifyCoinCountChanged: false);
			}
		}
		for (int j = 0; j < centerCoins.Count; j++)
		{
			CoinVat coinVat2 = centerCoins[j];
			if ((Object)(object)coinVat2 != (Object)null)
			{
				coinVat2.StopEvolutionShake();
			}
		}
		if (centerCoins.Count > 0 && (Object)(object)centerCoins[0] != (Object)null)
		{
			CoinType coinType = centerCoins[0].coinType;
			SyncCoinPopulation(coinType);
		}
		SingletonMono<UpgradeManager>.Instance?.NotifyCoinCountChanged();
	}

	public Vector2 GetSceneCenterPosition()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		Vector2 pos = (boundaryMin + boundaryMax) * 0.5f;
		return ClampToSceneBounds(pos);
	}

	public void InitializeRoles()
	{
		if (!IsInitialized)
		{
			return;
		}
		int count = 0;
		if ((Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null && SingletonMono<UpgradeManager>.Instance.IsInitialized)
		{
			int itemId = 1;
			if (ConfigManager.HasInstance() && (Object)(object)GameDataManager.Instance != (Object)null)
			{
				world worldConfig = ConfigManager.Instance.GetWorldConfig(GameDataManager.Instance.GetCurrentWorldId());
				if (worldConfig != null && worldConfig.HelperUpgradeId > 0)
				{
					itemId = worldConfig.HelperUpgradeId;
				}
			}
			count = SingletonMono<UpgradeManager>.Instance.GetUpgradeLevel(itemId, "itemUpgrade1");
		}
		if (roleSpawnCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(roleSpawnCoroutine);
		}
		roleSpawnCoroutine = ((MonoBehaviour)this).StartCoroutine(SpawnRolesSequentially(count));
	}

	public void ReloadForCurrentWorld()
	{
		ClearRuntimeContent();
		InitializeCoins();
		InitializeRoles();
	}

	public void SetDiceWorldController(DiceWorldController controller)
	{
		diceWorldController = controller;
	}

	public bool TryRefreshDicePopulationForSpawnUpgrade(int upgradeItemId, bool? forceSpawnAtCenter = null, int? forcedDesired = null, int? forcedEvolutionTier = null)
	{
		if ((Object)(object)diceWorldController != (Object)null)
		{
			return diceWorldController.TryRefreshDicePopulationForSpawnUpgrade(upgradeItemId, forceSpawnAtCenter, forcedDesired, forcedEvolutionTier);
		}
		return false;
	}

	public void ReloadRolesForCurrentWorld()
	{
		if (roleSpawnCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(roleSpawnCoroutine);
			roleSpawnCoroutine = null;
		}
		RemoveAllRole();
		InitializeRoles();
	}

	public void ClearRuntimeContent()
	{
		CancelInitialCoinSpawn();
		coinSyncVersions.Clear();
		if (roleSpawnCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(roleSpawnCoroutine);
			roleSpawnCoroutine = null;
		}
		isSwipeTracking = false;
		pointerStartedOverBlockedUi = false;
		activeTouchFingerId = -1;
		swipedCoinsThisFrame.Clear();
		luckyEchoTasks.Clear();
		queuedSkillCoinTosses.Clear();
		queuedSkillCoinSet.Clear();
		diceWorldController?.ClearThrowables();
		RemoveAllCoin();
		RemoveAllRole();
		floatingTextPool.Clear();
		ClearPoolChildren(rolePool);
		ClearPoolChildren(coinPool);
		ClearPoolChildren(effectPool);
		ClearPoolChildren((Transform)(object)huanxingPool);
		ClearPoolChildren((Transform)(object)TextPool);
		ClearPoolChildren(coins_smokePool);
		ClearPoolChildren((Transform)(object)ParticlePool);
		helperAddVatPool.Clear();
		effectPoolDict.Clear();
		sceneEffectObjectPools.Clear();
		TextCurrentCount = 0;
	}

	private static void ClearPoolChildren(Transform pool)
	{
		if (!((Object)(object)pool == (Object)null))
		{
			for (int num = pool.childCount - 1; num >= 0; num--)
			{
				Object.Destroy((Object)(object)((Component)pool.GetChild(num)).gameObject);
			}
		}
	}

	public RoleVat SpawnRole(Action<RoleVat> onSpawned = null, int helperId = 0)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		EnsurePools();
		Transform val = (((Object)(object)rolePool != (Object)null) ? rolePool : ((Component)this).transform);
		GameObject val2 = new GameObject($"RoleVat_{helperId}")
		{
			layer = ((Component)val).gameObject.layer
		};
		val2.transform.SetParent(val, false);
		val2.transform.localScale = Vector3.one * roleRenderScale;
		RoleVat roleVat = val2.AddComponent<RoleVat>();
		roleVat.UseExternalTick = UseCentralTick;
		roleVat.SetVatShader(spineVatShader);
		roleVat.SetSortingLayerAndOrder(vatSortingLayerName, roleSortingOrder);
		if ((Object)(object)diceWorldController != (Object)null)
		{
			roleVat.InitializeForWorld(helperId, boundaryMin, boundaryMax, this, diceWorldController, UiToWorldScale, runtimeAssets.GetRoleVatAsset(helperId));
		}
		else
		{
			roleVat.Initialize(helperId, boundaryMin, boundaryMax, this, UiToWorldScale, runtimeAssets.GetRoleVatAsset(helperId));
		}
		ConfigureRoleRenderingMode(roleVat);
		roleVat.RefreshDebugVisuals();
		activeRoles.Add(roleVat);
		onSpawned?.Invoke(roleVat);
		return roleVat;
	}

	public CoinVat SpawnCoin(CoinType coinType, int index = -1, Vector2? forcedSpawnPosition = null, int? evolutionTierOverride = null, bool playSpawnEffect = true, bool notifyCoinCountChanged = true)
	{
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		EnsurePools();
		Transform val = (((Object)(object)coinPool != (Object)null) ? coinPool : ((Component)this).transform);
		int num = evolutionTierOverride ?? GetCoinEvolutionTier(coinType);
		SpineVatAsset coinVatAsset = GetCoinVatAsset(coinType, num);
		Sprite[] array = null;
		if ((Object)(object)coinVatAsset == (Object)null)
		{
			array = GetCoinSpriteFrames(coinType);
			if (array == null || array.Length == 0)
			{
				Debug.LogError((object)$"[MainSceneVat] Missing coin VAT/sprite frames: {coinType}, tier={num}");
				return null;
			}
		}
		Vector2 val2;
		if (forcedSpawnPosition.HasValue)
		{
			val2 = ClampToSceneBounds(forcedSpawnPosition.Value);
		}
		else
		{
			val2 = GetRandomSpawnPosition();
			if (TryGetInitialThrowableSpawnPosition(index, out var worldPosition))
			{
				val2 = worldPosition;
			}
		}
		GameObject val3 = new GameObject($"CoinVat_{coinType}")
		{
			layer = ((Component)val).gameObject.layer
		};
		val3.transform.SetParent(val, false);
		val3.transform.localScale = Vector3.one * coinRenderScale;
		CoinVat coinVat = val3.AddComponent<CoinVat>();
		coinVat.UseExternalTick = UseCentralTick;
		coinVat.SetMainScene(this);
		coinVat.SetVatShader(((Object)(object)spineVatShader != (Object)null) ? spineVatShader : spriteVatShader);
		coinVat.SetSpriteVatShader(spriteVatShader);
		coinVat.SetSortingLayerAndOrder(vatSortingLayerName, coinSortingOrder);
		if ((Object)(object)coinVatAsset != (Object)null)
		{
			coinVat.Initialize(coinType, coinVatAsset, boundaryMin, boundaryMax, (Transform)(object)TextPool, UiToWorldScale, num);
		}
		else
		{
			coinVat.Initialize(coinType, array, boundaryMin, boundaryMax, (Transform)(object)TextPool, UiToWorldScale);
		}
		coinVat.SetHoverTriggerEnabled(enabled: true);
		coinVat.OnCoinClicked = (Action<CoinVat>)Delegate.Combine(coinVat.OnCoinClicked, new Action<CoinVat>(OnCoinClicked));
		coinVat.OnFlipComplete = (Action<CoinVat, bool>)Delegate.Combine(coinVat.OnFlipComplete, new Action<CoinVat, bool>(OnCoinFlipComplete));
		coinVat.OnLanded = (Action<CoinVat>)Delegate.Combine(coinVat.OnLanded, new Action<CoinVat>(OnCoinLanded));
		coinVat.SetInitialPosition(val2);
		if (playSpawnEffect && index < 0)
		{
			PlaySpawnEffect(val2);
		}
		ConfigureCoinRenderingMode(coinVat);
		coinVat.RefreshDebugVisuals();
		activeCoins.Add(coinVat);
		InsertIntoSortedCache(coinVat);
		InvalidateCoinSpatialIndex();
		if (notifyCoinCountChanged)
		{
			SingletonMono<UpgradeManager>.Instance?.NotifyCoinCountChanged();
		}
		return coinVat;
	}

	public bool TryGetInitialThrowableSpawnPosition(int index, out Vector2 worldPosition)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		worldPosition = default(Vector2);
		if (index < 0 || index >= SpawnPositions.Length || (Object)(object)bgImage == (Object)null)
		{
			return false;
		}
		worldPosition = UiToWorldPoint(SpawnPositions[index]) + Vector2.op_Implicit(((Component)bgImage).transform.position);
		return true;
	}

	public void PlaySpawnEffect(Vector2 position)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		if (!VatDebugSettings.ShowSkillEffects)
		{
			return;
		}
		SpineVatAsset skillVatAsset = runtimeAssets.GetSkillVatAsset("Assets/GameRes/VAT/zgygy_fight_calling_SkeletonData_VAT.asset");
		if ((Object)(object)skillVatAsset == (Object)null)
		{
			int requestVersion = coinSpawnVersion;
			LoadSkillVatAsset("Assets/GameRes/VAT/zgygy_fight_calling_SkeletonData_VAT.asset", delegate(SpineVatAsset loadedAsset)
			{
				//IL_0031: Unknown result type (might be due to invalid IL or missing references)
				if ((Object)(object)this != (Object)null && (Object)(object)loadedAsset != (Object)null && requestVersion == coinSpawnVersion)
				{
					PlaySpawnEffectLoaded(position, loadedAsset);
				}
			});
		}
		else
		{
			PlaySpawnEffectLoaded(position, skillVatAsset);
		}
	}

	private void PlaySpawnEffectLoaded(Vector2 position, SpineVatAsset asset)
	{
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		SpineVat vat = GetFromHelperAddVatPool(asset);
		if (!((Object)(object)vat == (Object)null))
		{
			if ((Object)(object)vat.VatAsset != (Object)(object)asset)
			{
				vat.SetVatAssetNoPlay(asset);
			}
			vat.SetDepthSortRange(boundaryMin.y, boundaryMax.y);
			vat.SetDepthSortEnabled(enabled: true);
			vat.SetRendererSortingOrderByYEnabled(enabled: true);
			vat.SetRenderingVisible(visible: true);
			((Component)vat).gameObject.SetActive(true);
			((Component)vat).transform.position = new Vector3(position.x, position.y, 0f);
			((Component)vat).transform.localScale = Vector3.one;
			if (!vat.Play("animation", shouldLoop: false, 0f, delegate
			{
				if ((Object)(object)vat != (Object)null)
				{
					((Component)vat).gameObject.SetActive(false);
				}
			}))
			{
				((Component)vat).gameObject.SetActive(false);
			}
		}
	}

	private SpineVat GetFromHelperAddVatPool(SpineVatAsset asset)
	{
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		Transform val = GetEffectPool("HelperAddVatPool");
		helperAddVatPool.RemoveAll((SpineVat v) => (Object)(object)v == (Object)null || (Object)(object)((Component)v).gameObject == (Object)null);
		for (int num = 0; num < helperAddVatPool.Count; num++)
		{
			SpineVat spineVat = helperAddVatPool[num];
			if (!((Component)spineVat).gameObject.activeSelf)
			{
				if ((Object)(object)val != (Object)null && (Object)(object)((Component)spineVat).transform.parent != (Object)(object)val)
				{
					((Component)spineVat).transform.SetParent(val, false);
				}
				return spineVat;
			}
		}
		GameObject val2 = new GameObject("HelperAddVatEffect")
		{
			layer = (((Object)(object)val != (Object)null) ? ((Component)val).gameObject.layer : ((Component)this).gameObject.layer)
		};
		val2.transform.SetParent(((Object)(object)val != (Object)null) ? val : ((Component)this).transform, false);
		SpineVat spineVat2 = val2.AddComponent<SpineVat>();
		spineVat2.UseExternalTick = false;
		spineVat2.SetVatShader(spineVatShader);
		spineVat2.SetSortingLayerAndOrder(vatSortingLayerName, EffectSortingOrder);
		spineVat2.SetDepthSortRange(boundaryMin.y, boundaryMax.y);
		spineVat2.SetDepthSortEnabled(enabled: true);
		spineVat2.SetRendererSortingOrderByYEnabled(enabled: true);
		spineVat2.SetIndirectRenderingEnabled(enabled: false);
		spineVat2.SetVatAssetNoPlay(asset);
		val2.SetActive(false);
		helperAddVatPool.Add(spineVat2);
		return spineVat2;
	}

	public void TriggerCoinThrow(CoinVat coin)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)coin == (Object)null) && coin.State == CoinState.Idle)
		{
			Vector2 startPos = Vector2.op_Implicit(((Component)coin).transform.position);
			Vector2 randomLandingPosition = GetRandomLandingPosition(startPos);
			coin.JumpToTarget(startPos, randomLandingPosition, 0);
			InvalidateCoinSpatialIndex();
		}
	}

	public bool EnqueueSkillCoinToss(CoinVat coin, int sourceRoleType, float scatterRadius, bool suppressLandingEffects)
	{
		if ((Object)(object)coin == (Object)null || coin.State != CoinState.Idle || queuedSkillCoinSet.Contains(coin))
		{
			return false;
		}
		queuedSkillCoinSet.Add(coin);
		queuedSkillCoinTosses.Enqueue(new QueuedSkillCoinToss
		{
			Coin = coin,
			SourceRoleType = sourceRoleType,
			ScatterRadius = Mathf.Max(0f, scatterRadius),
			SuppressLandingEffects = suppressLandingEffects
		});
		return true;
	}

	private void TickQueuedSkillCoinTosses()
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		int num = Mathf.Max(1, skillCoinTossesPerFrame);
		while (num > 0 && queuedSkillCoinTosses.Count > 0)
		{
			QueuedSkillCoinToss queuedSkillCoinToss = queuedSkillCoinTosses.Dequeue();
			queuedSkillCoinSet.Remove(queuedSkillCoinToss.Coin);
			CoinVat coin = queuedSkillCoinToss.Coin;
			if (!((Object)(object)coin == (Object)null) && coin.State == CoinState.Idle)
			{
				Vector2 val = Vector2.op_Implicit(((Component)coin).transform.position);
				Vector2 targetPos = val + Random.insideUnitCircle * queuedSkillCoinToss.ScatterRadius;
				coin.JumpToTarget(val, targetPos, queuedSkillCoinToss.SourceRoleType, queuedSkillCoinToss.SuppressLandingEffects);
				num--;
			}
		}
	}

	public Vector2 GetRandomLandingPosition(Vector2 startPos)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 20; i++)
		{
			float num = Random.Range(UiToWorldDistance(minLandingRadius), UiToWorldDistance(maxLandingRadius));
			Vector2 insideUnitCircle = Random.insideUnitCircle;
			Vector2 val = ((Vector2)(ref insideUnitCircle)).normalized * num;
			Vector2 val2 = ClampToSceneBounds(startPos + val);
			if (!IsInCoinAvoidRect(val2))
			{
				return val2;
			}
		}
		return PushOutFromCoinAvoidRects(ClampToSceneBounds(startPos + Random.insideUnitCircle * UiToWorldDistance(maxLandingRadius)));
	}

	public void RemoveCoin(CoinVat coin, bool notifyCoinCountChanged = true)
	{
		if (!((Object)(object)coin == (Object)null))
		{
			coin.OnCoinClicked = (Action<CoinVat>)Delegate.Remove(coin.OnCoinClicked, new Action<CoinVat>(OnCoinClicked));
			coin.OnFlipComplete = (Action<CoinVat, bool>)Delegate.Remove(coin.OnFlipComplete, new Action<CoinVat, bool>(OnCoinFlipComplete));
			coin.OnLanded = (Action<CoinVat>)Delegate.Remove(coin.OnLanded, new Action<CoinVat>(OnCoinLanded));
			coinVatBatchRenderer?.UnregisterCoin(coin);
			if (coin.UsesSpineVat && (Object)(object)coin.SpineVatComponent != (Object)null)
			{
				spineVatBatchRenderer?.UnregisterSpineVat(coin.SpineVatComponent);
			}
			activeCoins.Remove(coin);
			sortedCoinsCache.Remove(coin);
			InvalidateCoinSpatialIndex();
			Object.Destroy((Object)(object)((Component)coin).gameObject);
			if (notifyCoinCountChanged)
			{
				SingletonMono<UpgradeManager>.Instance?.NotifyCoinCountChanged();
			}
		}
	}

	public void RemoveAllCoin()
	{
		coinVatBatchRenderer?.ClearRegisteredCoins();
		CoinVat[] array = activeCoins.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			RemoveCoin(array[i], notifyCoinCountChanged: false);
		}
		activeCoins.Clear();
		sortedCoinsCache.Clear();
		InvalidateCoinSpatialIndex();
		if (array.Length != 0)
		{
			SingletonMono<UpgradeManager>.Instance?.NotifyCoinCountChanged();
		}
	}

	public void RemoveAllRole()
	{
		spineVatBatchRenderer?.ClearRegisteredSpineVats();
		RoleVat[] array = activeRoles.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			if ((Object)(object)array[i] != (Object)null)
			{
				Object.Destroy((Object)(object)((Component)array[i]).gameObject);
			}
		}
		activeRoles.Clear();
		changingRoles.Clear();
	}

	public void RemoveRole(RoleVat role)
	{
		if (!((Object)(object)role == (Object)null))
		{
			if ((Object)(object)role.SpineVatComponent != (Object)null)
			{
				spineVatBatchRenderer?.UnregisterSpineVat(role.SpineVatComponent);
			}
			activeRoles.Remove(role);
			changingRoles.Remove(role);
			Object.Destroy((Object)(object)((Component)role).gameObject);
		}
	}

	public void ChangeRole(int pre, int target)
	{
		((MonoBehaviour)this).StartCoroutine(ChangeRoleCoroutine(pre, target));
	}

	private IEnumerator ChangeRoleCoroutine(int pre, int target)
	{
		RoleVat role = null;
		for (int num = activeRoles.Count - 1; num >= 0; num--)
		{
			if ((Object)(object)activeRoles[num] != (Object)null && activeRoles[num].roleType == pre && !changingRoles.Contains(activeRoles[num]))
			{
				role = activeRoles[num];
				break;
			}
		}
		if ((Object)(object)role == (Object)null)
		{
			Debug.LogWarning((object)$"[MainSceneVat] ChangeRole skipped, missing source role type: {pre} -> {target}", (Object)(object)this);
			yield break;
		}
		changingRoles.Add(role);
		if (runtimeAssets != null)
		{
			yield return runtimeAssets.EnsureRoleVatAssetLoaded(target);
		}
		if ((Object)(object)role == (Object)null || !activeRoles.Contains(role))
		{
			changingRoles.Remove(role);
			yield break;
		}
		RemoveRole(role);
		SpawnRole(null, target);
	}

	public void RefreshAllRoleSleepProbability()
	{
		for (int i = 0; i < activeRoles.Count; i++)
		{
			if ((Object)(object)activeRoles[i] != (Object)null)
			{
				activeRoles[i].RefreshSleepProbability();
			}
		}
	}

	public int GetActiveCoinCount()
	{
		if (!((Object)(object)diceWorldController != (Object)null))
		{
			return activeCoins.Count;
		}
		return diceWorldController.ActiveDiceCount;
	}

	public int GetRoleCount()
	{
		return activeRoles.Count;
	}

	public List<CoinVat> GetAllVatCoins()
	{
		return activeCoins;
	}

	public List<RoleVat> GetAllVatRoles()
	{
		return new List<RoleVat>(activeRoles);
	}

	public List<CoinVat> GetCoinsSortedByValue()
	{
		return sortedCoinsCache;
	}

	public void GetIdleVatCoinsInRadius(Vector2 center, float radius, List<CoinVat> results)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (results != null)
		{
			EnsureCoinSpatialIndex();
			coinSpatialIndex.Query(center, radius, results);
		}
	}

	public void GetIdleThrowablesInRadius(Vector2 center, float radius, List<IRoleThrowableVat> results)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (results == null)
		{
			return;
		}
		if ((Object)(object)diceWorldController != (Object)null)
		{
			diceWorldController.GetIdleThrowablesInRadius(center, radius, results);
		}
		EnsureCoinSpatialIndex();
		coinQueryScratch.Clear();
		coinSpatialIndex.Query(center, radius, coinQueryScratch);
		for (int i = 0; i < coinQueryScratch.Count; i++)
		{
			if ((Object)(object)coinQueryScratch[i] != (Object)null)
			{
				results.Add(coinQueryScratch[i]);
			}
		}
	}

	public CoinVat FindNearestIdleVatCoin(Vector2 center, float radius)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		EnsureCoinSpatialIndex();
		return coinSpatialIndex.FindNearest(center, radius);
	}

	public void InvalidateCoinSpatialIndex()
	{
		coinSpatialIndexDirty = true;
		lastDensePosFrame = -1;
	}

	private void EnsureCoinSpatialIndex()
	{
		if (coinSpatialIndexDirty && coinSpatialIndexFrame != Time.frameCount)
		{
			coinSpatialIndex.Rebuild(activeCoins, UiToWorldDistance(spatialIndexCellSize));
			coinSpatialIndexFrame = Time.frameCount;
			coinSpatialIndexDirty = false;
		}
	}

	public Transform GetRolePool()
	{
		return rolePool;
	}

	public Transform GetCoinPool()
	{
		return coinPool;
	}

	public Transform GetTextPool()
	{
		return (Transform)(object)TextPool;
	}

	public Transform GetHuanXingPool()
	{
		return (Transform)(object)huanxingPool;
	}

	public Transform GetHuanxingPool()
	{
		return (Transform)(object)huanxingPool;
	}

	public Transform GetCoinsSmokePool()
	{
		return coins_smokePool;
	}

	public Transform GetParticlePool()
	{
		return (Transform)(object)ParticlePool;
	}

	public Transform GetEffectPool(string effectPoolName)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		EnsurePools();
		if (effectPoolDict.ContainsKey(effectPoolName))
		{
			return effectPoolDict[effectPoolName];
		}
		Transform val = (((Object)(object)effectPool != (Object)null) ? effectPool : ((Component)this).transform);
		GameObject val2 = new GameObject(effectPoolName);
		val2.transform.SetParent(val, false);
		val2.transform.localScale = Vector3.one;
		val2.transform.localPosition = Vector3.zero;
		effectPoolDict.Add(effectPoolName, val2.transform);
		return val2.transform;
	}

	public GameObject GetVatPrefab(string assetPath)
	{
		return runtimeAssets.GetPrefab(assetPath);
	}

	public SpineVatAsset GetSkillVatAsset(string assetPath)
	{
		SpineVatAsset skillVatAsset = runtimeAssets.GetSkillVatAsset(assetPath);
		if ((Object)(object)skillVatAsset == (Object)null)
		{
			RequestSkillVatAsset(assetPath, null);
		}
		return skillVatAsset;
	}

	public void LoadSkillVatAsset(string assetPath, Action<SpineVatAsset> onLoaded)
	{
		SpineVatAsset skillVatAsset = runtimeAssets.GetSkillVatAsset(assetPath);
		if ((Object)(object)skillVatAsset != (Object)null)
		{
			onLoaded?.Invoke(skillVatAsset);
		}
		else
		{
			RequestSkillVatAsset(assetPath, onLoaded);
		}
	}

	private void RequestSkillVatAsset(string assetPath, Action<SpineVatAsset> onLoaded)
	{
		if (string.IsNullOrEmpty(assetPath))
		{
			onLoaded?.Invoke(null);
			return;
		}
		if (pendingSkillVatCallbacks.TryGetValue(assetPath, out var value))
		{
			if (onLoaded != null)
			{
				value.Add(onLoaded);
			}
			return;
		}
		value = new List<Action<SpineVatAsset>>();
		if (onLoaded != null)
		{
			value.Add(onLoaded);
		}
		pendingSkillVatCallbacks.Add(assetPath, value);
		((MonoBehaviour)this).StartCoroutine(LoadSkillVatAssetOnDemand(assetPath));
	}

	private IEnumerator LoadSkillVatAssetOnDemand(string assetPath)
	{
		yield return runtimeAssets.EnsureSkillVatAssetLoaded(assetPath);
		SpineVatAsset skillVatAsset = runtimeAssets.GetSkillVatAsset(assetPath);
		if (pendingSkillVatCallbacks.TryGetValue(assetPath, out var value))
		{
			pendingSkillVatCallbacks.Remove(assetPath);
			for (int i = 0; i < value.Count; i++)
			{
				value[i]?.Invoke(skillVatAsset);
			}
		}
	}

	public GameObject GetPooledVatEffect(string poolName, GameObject prefab, Transform parent, int maxActive = 0)
	{
		if ((Object)(object)prefab == (Object)null)
		{
			return null;
		}
		Transform val = (((Object)(object)parent != (Object)null) ? parent : GetEffectPool(poolName));
		string key = $"{poolName}:{((Object)prefab).GetInstanceID()}";
		if (!sceneEffectObjectPools.TryGetValue(key, out var value))
		{
			value = new List<GameObject>();
			sceneEffectObjectPools.Add(key, value);
		}
		int num = 0;
		for (int num2 = value.Count - 1; num2 >= 0; num2--)
		{
			GameObject val2 = value[num2];
			if ((Object)(object)val2 == (Object)null)
			{
				value.RemoveAt(num2);
			}
			else
			{
				if (!val2.activeSelf)
				{
					if ((Object)(object)val2.transform.parent != (Object)(object)val)
					{
						val2.transform.SetParent(val, false);
					}
					VatEffectVisualCache.SetVisible(val2, VatDebugSettings.AreRoleSkillVisualsVisible(GetEffectPoolRoleType(poolName)));
					return val2;
				}
				num++;
			}
		}
		if (maxActive > 0 && num >= maxActive)
		{
			return null;
		}
		GameObject val3 = Object.Instantiate<GameObject>(prefab, val);
		VatEffectVisualCache.SetVisible(val3, VatDebugSettings.AreRoleSkillVisualsVisible(GetEffectPoolRoleType(poolName)));
		value.Add(val3);
		return val3;
	}

	public bool ShowCoinFloatingText(Vector3 worldPosition, double value)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (!UseSceneFloatingTextPool)
		{
			return false;
		}
		return floatingTextPool.TryShow(worldPosition, value);
	}

	public bool IsAddText()
	{
		return TextCurrentCount <= TextMaxCount;
	}

	public void ReduceTextCount()
	{
		TextCurrentCount--;
		if (TextCurrentCount < 0)
		{
			TextCurrentCount = 0;
		}
	}

	public void AddTextCount()
	{
		TextCurrentCount++;
	}

	public int GetCurrentTextCount()
	{
		return TextCurrentCount;
	}

	public int GetMaxTextCount()
	{
		return TextMaxCount;
	}

	public void GetSceneBoundary(out Vector2 min, out Vector2 max)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		min = boundaryMin;
		max = boundaryMax;
	}

	public Vector2 GetRandomPositionInBoundary(float padding = 50f)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		float safeWorldPadding = GetSafeWorldPadding(padding);
		float num = Random.Range(boundaryMin.x + safeWorldPadding, boundaryMax.x - safeWorldPadding);
		float num2 = Random.Range(boundaryMin.y + safeWorldPadding, boundaryMax.y - safeWorldPadding);
		return new Vector2(num, num2);
	}

	public Vector2 GetMostDenseCoinPosition(float gridSize = 100f)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		if (lastDensePosFrame == Time.frameCount)
		{
			return cachedDensePos;
		}
		lastDensePosFrame = Time.frameCount;
		if (activeCoins.Count == 0)
		{
			cachedDensePos = Vector2.zero;
			return cachedDensePos;
		}
		EnsureCoinSpatialIndex();
		cachedDensePos = coinSpatialIndex.GetMostDensePosition();
		return cachedDensePos;
	}

	public void SetCoinAvoidRects(IReadOnlyList<Rect> avoidRects)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		coinAvoidRects.Clear();
		if (enableCoinAvoidRects && avoidRects != null)
		{
			for (int i = 0; i < avoidRects.Count; i++)
			{
				coinAvoidRects.Add(UiToWorldRect(avoidRects[i]));
			}
		}
	}

	public void ShowGuideClickCoin(CoinVat coin, Action<CoinVat> onClicked, string dialogText = "点击抛起铜币", bool isShow = true)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)coin == (Object)null) && !((Object)(object)guideFinger == (Object)null))
		{
			if (isShow)
			{
				UIManager.Instance.ShowDialogPanel(dialogText, DialogPosition.Bottom, 0.0001f);
			}
			coin.OnGuideFinger -= onClicked;
			coin.OnGuideFinger += onClicked;
			PlaceUiRectAtWorldPosition(guideFinger, ((Component)coin).transform.position);
			((Component)guideFinger).gameObject.SetActive(true);
		}
	}

	public void ShowGuideClickDice(DiceVat dice, string dialogText = null)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)dice == (Object)null) && !((Object)(object)guideFinger == (Object)null))
		{
			if (!string.IsNullOrEmpty(dialogText))
			{
				ShowGuideDialog(dialogText);
			}
			PlaceUiRectAtWorldPosition(guideFinger, ((Component)dice).transform.position);
			((Component)guideFinger).gameObject.SetActive(true);
		}
	}

	public void HideGuideFinger()
	{
		if ((Object)(object)guideFinger != (Object)null)
		{
			((Component)guideFinger).gameObject.SetActive(false);
		}
	}

	public void ShowGuideDialog(string dialogText)
	{
		if (!string.IsNullOrEmpty(dialogText) && UIManager.HasInstance())
		{
			UIManager.Instance.ShowDialogPanel(dialogText, DialogPosition.Bottom, 0.0001f);
		}
	}

	public void HideGuideDialog()
	{
		if (UIManager.HasInstance())
		{
			UIManager.Instance.HideDialogPanel();
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		if (lastHandledPointerClickFrame != Time.frameCount)
		{
			Vector2 screenPos = ((eventData != null) ? eventData.position : Vector2.op_Implicit(Input.mousePosition));
			if (!IsPointerBlockedByUi(screenPos))
			{
				Camera camera = ((eventData != null) ? eventData.pressEventCamera : Camera.main);
				lastHandledPointerClickFrame = Time.frameCount;
				HandleSceneClick(screenPos, camera);
			}
		}
	}

	private void HandleSceneClick(Vector2 screenPos, Camera camera)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		Vector2 worldPos = ScreenToWorldPoint(screenPos, camera);
		if (!TryClickCoinAt(worldPos))
		{
			TryWakeSleepingRoleAt(worldPos);
		}
	}

	private bool TryClickCoinAt(Vector2 worldPos)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		float radius = UiToWorldDistance(coinClickRadius);
		if ((Object)(object)diceWorldController != (Object)null && diceWorldController.TryManualThrowAt(worldPos, radius))
		{
			return true;
		}
		CoinVat coinVat = FindNearestIdleVatCoin(worldPos, radius);
		if ((Object)(object)coinVat == (Object)null)
		{
			return false;
		}
		coinVat.SetCoinClick();
		return true;
	}

	private bool TryWakeSleepingRoleAt(Vector2 worldPos)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		float num = UiToWorldDistance(roleClickRadius);
		RoleVat roleVat = null;
		float num2 = num * num;
		for (int i = 0; i < activeRoles.Count; i++)
		{
			RoleVat roleVat2 = activeRoles[i];
			if (!((Object)(object)roleVat2 == (Object)null) && roleVat2.State == RoleState.Sleeping)
			{
				Vector2 val = Vector2.op_Implicit(((Component)roleVat2).transform.position) - worldPos;
				float sqrMagnitude = ((Vector2)(ref val)).sqrMagnitude;
				if (sqrMagnitude <= num2)
				{
					num2 = sqrMagnitude;
					roleVat = roleVat2;
				}
			}
		}
		if ((Object)(object)roleVat == (Object)null)
		{
			return false;
		}
		roleVat.OnPointerClick(null);
		return true;
	}

	public void AddGongfu()
	{
		if (!isAddGongfu)
		{
			SpawnRole(delegate
			{
				isAddGongfu = true;
				PlayerPrefs.SetInt("gongfu_support_time", 1);
				PlayerPrefs.Save();
				RefreshSupportAdDisplay();
			}, 4);
		}
	}

	public void AddDashe()
	{
		if (!isAddDashe)
		{
			SpawnRole(delegate
			{
				isAddDashe = true;
				PlayerPrefs.SetInt("dashe_support_time", 1);
				PlayerPrefs.Save();
				RefreshSupportAdDisplay();
			}, 6);
		}
	}

	private static void RefreshSupportAdDisplay()
	{
		DownItem downItem = UIManager.Instance?.GetDownItem();
		if ((Object)(object)downItem?.foldBtn != (Object)null && ((Component)downItem.foldBtn).gameObject.activeSelf)
		{
			downItem.AdSummonDisplay(isOpen: true);
		}
	}

	private IEnumerator InitializeVatScene()
	{
		yield return LoadVatShaders();
		yield return InitializeWithConfig();
	}

	private IEnumerator LoadVatShaders()
	{
		if ((Object)(object)SingletonMono<ResManager>.Instance == (Object)null)
		{
			Debug.LogWarning((object)"[MainSceneVat] ResManager is missing, VAT shaders will use Shader.Find fallback.", (Object)(object)this);
		}
		yield return runtimeAssets.LoadShaders();
		spineVatShader = runtimeAssets.SpineVatShader;
		spriteVatShader = runtimeAssets.SpriteVatShader;
		if ((Object)(object)spriteVatShader == (Object)null)
		{
			Debug.LogWarning((object)"[MainSceneVat] Sprite VAT shader is missing. Check YooAssets package for Assets/GameRes/VAT/SpriteSequenceVatUnlit.shader.", (Object)(object)this);
		}
		ResolvePerformanceMode();
	}

	private void ResolvePerformanceMode()
	{
		activePerformanceMode = performanceMode;
		if (activePerformanceMode == VatPerformanceMode.GpuBatch && (!SystemInfo.supportsInstancing || !SystemInfo.supportsComputeShaders || !((Object)(object)runtimeAssets.SpineVatIndirectShader != (Object)null)))
		{
			activePerformanceMode = VatPerformanceMode.CentralTick;
			if (!gpuBatchFallbackLogged)
			{
				gpuBatchFallbackLogged = true;
				Debug.LogWarning((object)"[MainSceneVat] GpuBatch is not available on this runtime, fallback to CentralTick.", (Object)(object)this);
			}
		}
	}

	private void ReleaseVatShaderHandles()
	{
		if (spineVatShaderHandle != null)
		{
			((HandleBase)spineVatShaderHandle).Release();
			spineVatShaderHandle = null;
		}
		if (spriteVatShaderHandle != null)
		{
			((HandleBase)spriteVatShaderHandle).Release();
			spriteVatShaderHandle = null;
		}
		spineVatShader = null;
		spriteVatShader = null;
	}

	private void ReleaseVatWorldRoot()
	{
		if (ownsVatWorldRoot && (Object)(object)vatWorldRoot != (Object)null)
		{
			Object.Destroy((Object)(object)((Component)vatWorldRoot).gameObject);
		}
		vatWorldRoot = null;
		ownsVatWorldRoot = false;
	}

	private IEnumerator InitializeWithConfig()
	{
		while (!ConfigManager.HasInstance())
		{
			yield return null;
		}
		while (!ConfigManager.isInitialized)
		{
			yield return null;
		}
		while ((Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null && !SingletonMono<UpgradeManager>.Instance.IsInitialized)
		{
			yield return null;
		}
		yield return PreloadVatRuntimeAssets();
		EnsureGpuBatchRenderer();
		IsInitialized = true;
		InitializeCoins();
		InitializeRoles();
		if ((GameDataManager.Instance?.GetCurrentWorldId() ?? 1) == 1 && PlayerPrefs.GetInt("guide_step_1", 0) == 0)
		{
			((MonoBehaviour)this).StartCoroutine(StartGuideClickCoin());
		}
	}

	private IEnumerator PreloadVatRuntimeAssets()
	{
		if (UseCentralTick)
		{
			yield return runtimeAssets.LoadFloatingTextPrefabs();
			floatingTextPool.Initialize(this, TextPool, runtimeAssets.GetPrefab("Assets/GameRes/Role/coinNormalText.prefab"), runtimeAssets.GetPrefab("Assets/GameRes/Role/coinCriticalText.prefab"));
		}
		if (preloadAllHelperVatAssets)
		{
			int count = 0;
			if ((Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null && SingletonMono<UpgradeManager>.Instance.IsInitialized)
			{
				int currentWorldHelperUpgradeId = Constants.GetCurrentWorldHelperUpgradeId();
				count = SingletonMono<UpgradeManager>.Instance.GetUpgradeLevel(currentWorldHelperUpgradeId, "itemUpgrade1");
			}
			int[] helperIds = (((Object)(object)GameDataManager.Instance != (Object)null) ? GameDataManager.Instance.GetHelperIds(count) : null);
			yield return runtimeAssets.LoadRoleVatAssets(helperIds);
		}
		if (preloadVatSkillPrefabs)
		{
			yield return runtimeAssets.LoadSkillPrefabs();
		}
	}

	public SpineVatAsset GetCoinVatAsset(CoinType coinType)
	{
		return GetCoinVatAsset(coinType, 0);
	}

	public SpineVatAsset GetCoinVatAsset(CoinType coinType, int evolutionTier)
	{
		if (runtimeAssets == null)
		{
			return null;
		}
		return runtimeAssets.GetCoinVatAsset(coinType, evolutionTier);
	}

	public int GetCoinEvolutionTier(CoinType coinType)
	{
		if ((Object)(object)SingletonMono<UpgradeManager>.Instance == (Object)null)
		{
			return 0;
		}
		int coinEvolutionUpgradeId = UpgradeManager.GetCoinEvolutionUpgradeId(coinType);
		return SingletonMono<UpgradeManager>.Instance.GetEvolutionTier(coinEvolutionUpgradeId);
	}

	private IEnumerator SpawnRolesSequentially(int count)
	{
		int[] helperIds = GameDataManager.Instance.GetHelperIds(count);
		for (int i = 0; i < count; i++)
		{
			int helperId = ((helperIds != null && helperIds.Length > i) ? helperIds[i] : 0);
			SpawnRole(null, helperId);
			yield return (object)new WaitForSeconds(0.2f);
		}
		roleSpawnCoroutine = null;
	}

	private void ConfigureRoleRenderingMode(RoleVat role)
	{
		if ((Object)(object)role == (Object)null)
		{
			return;
		}
		bool useGpuBatch = UseGpuBatch;
		role.SetGpuBatchRenderingEnabled(useGpuBatch);
		if (useGpuBatch)
		{
			EnsureGpuBatchRenderer();
			if ((Object)(object)spineVatBatchRenderer != (Object)null && (Object)(object)role.SpineVatComponent != (Object)null)
			{
				spineVatBatchRenderer.RegisterSpineVat(role.SpineVatComponent);
			}
		}
	}

	private void ConfigureCoinRenderingMode(CoinVat coin)
	{
		if ((Object)(object)coin == (Object)null)
		{
			return;
		}
		if (coin.UsesSpineVat)
		{
			bool useGpuBatch = UseGpuBatch;
			coin.SetGpuBatchRenderingEnabled(useGpuBatch);
			if (useGpuBatch)
			{
				EnsureGpuBatchRenderer();
				if ((Object)(object)spineVatBatchRenderer != (Object)null && (Object)(object)coin.SpineVatComponent != (Object)null)
				{
					spineVatBatchRenderer.RegisterSpineVat(coin.SpineVatComponent);
				}
			}
		}
		else if (!UseGpuBatch || !enableCoinGpuBatch)
		{
			coin.SetGpuBatchRenderingEnabled(enabled: false);
		}
		else
		{
			EnsureGpuBatchRenderer();
			bool flag = (Object)(object)coinVatBatchRenderer != (Object)null && coinVatBatchRenderer.IsAvailable;
			coin.SetGpuBatchRenderingEnabled(flag);
			if (flag)
			{
				coinVatBatchRenderer.RegisterCoin(coin);
			}
		}
	}

	private void EnsureGpuBatchRenderer()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Expected O, but got Unknown
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Expected O, but got Unknown
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		if (UseGpuBatch)
		{
			if ((Object)(object)spineVatBatchRenderer == (Object)null)
			{
				Transform val = (moveVatRenderPoolsToWorld ? EnsureVatWorldRoot() : ((Component)this).transform);
				GameObject val2 = new GameObject("SpineVatBatchRenderer");
				val2.transform.SetParent(val, false);
				spineVatBatchRenderer = val2.AddComponent<SpineVatIndirectManager>();
			}
			int renderLayer = (((Object)(object)rolePool != (Object)null) ? ((Component)rolePool).gameObject.layer : ((Component)this).gameObject.layer);
			int sortingOrder = (forceRoleAboveThrowables ? roleSortingOrder : 0);
			spineVatBatchRenderer.ConfigureRegisteredRuntime(runtimeAssets.SpineVatIndirectShader, boundaryMin, boundaryMax, renderLayer, sortingOrder);
			if (enableCoinGpuBatch && (Object)(object)coinVatBatchRenderer == (Object)null)
			{
				Transform val3 = (moveVatRenderPoolsToWorld ? EnsureVatWorldRoot() : ((Component)this).transform);
				GameObject val4 = new GameObject("CoinVatBatchRenderer");
				val4.transform.SetParent(val3, false);
				coinVatBatchRenderer = val4.AddComponent<CoinVatBatchRenderer>();
			}
			if (enableCoinGpuBatch && (Object)(object)coinVatBatchRenderer != (Object)null)
			{
				int layer = (((Object)(object)coinPool != (Object)null) ? ((Component)coinPool).gameObject.layer : ((Component)this).gameObject.layer);
				coinVatBatchRenderer.Configure(spriteVatShader, boundaryMin, boundaryMax, layer, coinSortingOrder, coinGpuBatchYBuckets);
			}
		}
	}

	private void EnsurePools()
	{
		EnsureVatWorldObjects();
		rolePool = EnsureWorldPool(rolePool, "RoleVatPool");
		coinPool = EnsureWorldPool(coinPool, "CoinVatPool");
		effectPool = EnsureWorldCanvasPool(effectPool, "EffectVatPool");
		coins_smokePool = EnsureWorldPool(coins_smokePool, "CoinsSmokeVatPool");
		ParticlePool = EnsureUiPool(ParticlePool, "ParticleVatPool");
		ConfigureFullScreenUiPool(ParticlePool);
		huanxingPool = EnsureUiPool(huanxingPool, "HuanxingVatPool");
		TextPool = EnsureUiPool(TextPool, "TextVatPool");
		ConfigureFullScreenUiPool(TextPool);
	}

	private void EnsureVatWorldObjects()
	{
		if (moveVatRenderPoolsToWorld)
		{
			Transform parent = EnsureVatWorldRoot();
			MoveWorldObjectToParent(((Object)(object)bgImage != (Object)null) ? ((Component)bgImage).transform : null, parent);
			MoveWorldObjectToParent(((Object)(object)sceneBox != (Object)null) ? ((Component)sceneBox).transform : null, parent);
		}
	}

	private Transform EnsureWorldPool(Transform pool, string poolName)
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		Transform parent = (moveVatRenderPoolsToWorld ? EnsureVatWorldRoot() : ((Component)this).transform);
		if ((Object)(object)pool != (Object)null)
		{
			MovePoolToParent(pool, parent);
			return pool;
		}
		Transform val = ((Component)this).transform.Find(poolName);
		if ((Object)(object)val == (Object)null && (Object)(object)vatWorldRoot != (Object)null)
		{
			val = vatWorldRoot.Find(poolName);
		}
		if ((Object)(object)val != (Object)null)
		{
			MovePoolToParent(val, parent);
			return val;
		}
		GameObject val2 = new GameObject(poolName);
		MovePoolToParent(val2.transform, parent);
		return val2.transform;
	}

	private Transform EnsureWorldCanvasPool(Transform pool, string poolName)
	{
		Transform val = EnsureWorldPool(pool, poolName);
		ConfigureWorldEffectCanvas(val);
		return val;
	}

	private void ConfigureWorldEffectCanvas(Transform canvasPool)
	{
		if (!((Object)(object)canvasPool == (Object)null))
		{
			Canvas val = ((Component)canvasPool).GetComponent<Canvas>();
			if ((Object)(object)val == (Object)null)
			{
				val = ((Component)canvasPool).gameObject.AddComponent<Canvas>();
			}
			val.renderMode = (RenderMode)2;
			val.overrideSorting = true;
			if (!string.IsNullOrEmpty(vatSortingLayerName))
			{
				val.sortingLayerName = vatSortingLayerName;
			}
			val.sortingOrder = Mathf.Max(coinSortingOrder, roleSortingOrder) + 10000 + 10;
			if ((Object)(object)val.worldCamera == (Object)null)
			{
				val.worldCamera = Camera.main;
			}
		}
	}

	private RectTransform EnsureUiPool(RectTransform pool, string poolName)
	{
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Expected O, but got Unknown
		RectTransform uiPoolParent = GetUiPoolParent();
		if ((Object)(object)pool != (Object)null)
		{
			if ((Object)(object)uiPoolParent != (Object)null && (Object)(object)((Transform)pool).parent != (Object)(object)uiPoolParent)
			{
				((Transform)pool).SetParent((Transform)(object)uiPoolParent, false);
			}
			return pool;
		}
		Transform val = ((Component)this).transform.Find(poolName);
		if ((Object)(object)val == (Object)null && (Object)(object)uiPoolParent != (Object)null)
		{
			val = ((Transform)uiPoolParent).Find(poolName);
		}
		if ((Object)(object)val != (Object)null)
		{
			RectTransform orAddComponent = val.GetOrAddComponent<RectTransform>();
			if ((Object)(object)uiPoolParent != (Object)null && (Object)(object)((Transform)orAddComponent).parent != (Object)(object)uiPoolParent)
			{
				((Transform)orAddComponent).SetParent((Transform)(object)uiPoolParent, false);
			}
			return orAddComponent;
		}
		GameObject val2 = new GameObject(poolName);
		val2.transform.SetParent((Transform)(((Object)(object)uiPoolParent != (Object)null) ? ((object)uiPoolParent) : ((object)((Component)this).transform)), false);
		val2.transform.localScale = Vector3.one;
		return ExtendedMono.GetOrAddComponent<RectTransform>(val2);
	}

	private RectTransform GetUiPoolParent()
	{
		Transform parent = ((Component)this).transform.parent;
		RectTransform val = (RectTransform)(object)((parent is RectTransform) ? parent : null);
		if ((Object)(object)val != (Object)null)
		{
			return val;
		}
		Canvas componentInParent = ((Component)this).GetComponentInParent<Canvas>();
		if ((Object)(object)componentInParent != (Object)null)
		{
			Transform transform = ((Component)componentInParent).transform;
			return (RectTransform)(object)((transform is RectTransform) ? transform : null);
		}
		if (UIManager.HasInstance() && (Object)(object)UIManager.Instance.GetCanvas() != (Object)null)
		{
			Transform transform2 = ((Component)UIManager.Instance.GetCanvas()).transform;
			return (RectTransform)(object)((transform2 is RectTransform) ? transform2 : null);
		}
		GameObject val2 = GameObject.Find("Canvas");
		if ((Object)(object)val2 != (Object)null)
		{
			Transform transform3 = val2.transform;
			return (RectTransform)(object)((transform3 is RectTransform) ? transform3 : null);
		}
		return null;
	}

	private static void ConfigureFullScreenUiPool(RectTransform pool)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)pool == (Object)null))
		{
			pool.anchorMin = Vector2.zero;
			pool.anchorMax = Vector2.one;
			pool.pivot = new Vector2(0.5f, 0.5f);
			pool.anchoredPosition = Vector2.zero;
			pool.sizeDelta = Vector2.zero;
			((Transform)pool).localRotation = Quaternion.identity;
			((Transform)pool).localScale = Vector3.one;
			((Transform)pool).SetAsLastSibling();
		}
	}

	private Transform EnsureVatWorldRoot()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		if (!moveVatRenderPoolsToWorld)
		{
			return ((Component)this).transform;
		}
		if ((Object)(object)vatWorldRoot == (Object)null)
		{
			GameObject val = new GameObject("MainSceneVat_WorldRoot");
			vatWorldRoot = val.transform;
			ownsVatWorldRoot = true;
		}
		if ((Object)(object)((Component)vatWorldRoot).GetComponentInParent<Canvas>() != (Object)null)
		{
			vatWorldRoot.SetParent((Transform)null, false);
		}
		vatWorldRoot.position = new Vector3(0f, 0f, vatWorldZ);
		vatWorldRoot.rotation = Quaternion.identity;
		vatWorldRoot.localScale = Vector3.one;
		return vatWorldRoot;
	}

	private static void MovePoolToParent(Transform pool, Transform parent)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)pool == (Object)null) && !((Object)(object)parent == (Object)null))
		{
			if ((Object)(object)pool.parent != (Object)(object)parent)
			{
				pool.SetParent(parent, false);
			}
			pool.localPosition = Vector3.zero;
			pool.localRotation = Quaternion.identity;
			pool.localScale = Vector3.one;
			RectTransform val = (RectTransform)(object)((pool is RectTransform) ? pool : null);
			if ((Object)(object)val != (Object)null)
			{
				val.anchorMin = new Vector2(0.5f, 0.5f);
				val.anchorMax = new Vector2(0.5f, 0.5f);
				val.pivot = new Vector2(0.5f, 0.5f);
				val.anchoredPosition = Vector2.zero;
			}
		}
	}

	private static void MoveWorldObjectToParent(Transform target, Transform parent)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)target == (Object)null) && !((Object)(object)parent == (Object)null) && !((Object)(object)target.parent == (Object)(object)parent))
		{
			Vector3 localPosition = target.localPosition;
			Quaternion localRotation = target.localRotation;
			Vector3 localScale = target.localScale;
			target.SetParent(parent, false);
			target.localPosition = localPosition;
			target.localRotation = localRotation;
			target.localScale = localScale;
		}
	}

	private void CalculateBoundary()
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			Collider2D obj = sceneBox;
			if ((Object)(object)((obj is BoxCollider2D) ? obj : null) != (Object)null)
			{
				_ = (Object)(object)sceneBoxUI != (Object)null;
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning((object)("[MainSceneVat] Failed to calculate scene bounds: " + ex.Message));
		}
		Bounds usableBounds;
		if (TryGetSceneBoxBounds(out var bounds))
		{
			boundaryMin = new Vector2(((Bounds)(ref bounds)).min.x, ((Bounds)(ref bounds)).min.y);
			boundaryMax = new Vector2(((Bounds)(ref bounds)).max.x, ((Bounds)(ref bounds)).max.y);
		}
		else if ((Object)(object)bgImage != (Object)null && TryGetUsableBounds(((Renderer)bgImage).bounds, out usableBounds))
		{
			boundaryMin = new Vector2(((Bounds)(ref usableBounds)).min.x, ((Bounds)(ref usableBounds)).min.y);
			boundaryMax = new Vector2(((Bounds)(ref usableBounds)).max.x, ((Bounds)(ref usableBounds)).max.y);
		}
		else
		{
			boundaryMin = UiToWorldPoint(new Vector2(-400f, -800f));
			boundaryMax = UiToWorldPoint(new Vector2(400f, 0f));
		}
	}

	private static bool TryGetUsableBounds(Bounds bounds, out Bounds usableBounds)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		usableBounds = bounds;
		if (((Bounds)(ref bounds)).size.x > 1f)
		{
			return ((Bounds)(ref bounds)).size.y > 1f;
		}
		return false;
	}

	private bool TryGetSceneBoxBounds(out Bounds bounds)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		bounds = default(Bounds);
		Collider2D obj = sceneBox;
		BoxCollider2D val = (BoxCollider2D)(object)((obj is BoxCollider2D) ? obj : null);
		if ((Object)(object)val != (Object)null)
		{
			Vector2 val2 = val.size * 0.5f;
			Vector2 offset = ((Collider2D)val).offset;
			Vector3 val3 = ((Component)val).transform.TransformPoint(Vector2.op_Implicit(offset + new Vector2(0f - val2.x, 0f - val2.y)));
			Vector3 val4 = ((Component)val).transform.TransformPoint(Vector2.op_Implicit(offset + new Vector2(0f - val2.x, val2.y)));
			Vector3 val5 = ((Component)val).transform.TransformPoint(Vector2.op_Implicit(offset + new Vector2(val2.x, val2.y)));
			Vector3 val6 = ((Component)val).transform.TransformPoint(Vector2.op_Implicit(offset + new Vector2(val2.x, 0f - val2.y)));
			Vector3 val7 = Vector3.Min(Vector3.Min(val3, val4), Vector3.Min(val5, val6));
			Vector3 val8 = Vector3.Max(Vector3.Max(val3, val4), Vector3.Max(val5, val6));
			Bounds bounds2 = default(Bounds);
			((Bounds)(ref bounds2)).SetMinMax(val7, val8);
			return TryGetUsableBounds(bounds2, out bounds);
		}
		if ((Object)(object)sceneBox != (Object)null)
		{
			return TryGetUsableBounds(sceneBox.bounds, out bounds);
		}
		return false;
	}

	private void ApplyBackgroundSorting()
	{
		if (!((Object)(object)bgImage == (Object)null))
		{
			if (!string.IsNullOrEmpty(vatSortingLayerName))
			{
				((Renderer)bgImage).sortingLayerName = vatSortingLayerName;
			}
			((Renderer)bgImage).sortingOrder = backgroundSortingOrder;
		}
	}

	private void CheckSwipeToThrow()
	{
		if (Input.touchCount > 0)
		{
			CheckTouchToThrow();
		}
		else
		{
			CheckMouseToThrow();
		}
	}

	private void CheckMouseToThrow()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		if (Input.GetMouseButtonDown(0))
		{
			BeginPointer(Vector2.op_Implicit(Input.mousePosition));
		}
		else if (Input.GetMouseButtonUp(0))
		{
			EndPointer(Vector2.op_Implicit(Input.mousePosition), Camera.main);
		}
		else if (Input.GetMouseButton(0))
		{
			MovePointer(Vector2.op_Implicit(Input.mousePosition), Camera.main);
		}
	}

	private void CheckTouchToThrow()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Expected I4, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		if (activeTouchFingerId < 0)
		{
			for (int i = 0; i < Input.touchCount; i++)
			{
				Touch touch = Input.GetTouch(i);
				if ((int)((Touch)(ref touch)).phase == 0)
				{
					activeTouchFingerId = ((Touch)(ref touch)).fingerId;
					BeginPointer(((Touch)(ref touch)).position);
					break;
				}
			}
		}
		if (activeTouchFingerId < 0)
		{
			return;
		}
		for (int j = 0; j < Input.touchCount; j++)
		{
			Touch touch2 = Input.GetTouch(j);
			if (((Touch)(ref touch2)).fingerId == activeTouchFingerId)
			{
				TouchPhase phase = ((Touch)(ref touch2)).phase;
				switch (phase - 1)
				{
				case 0:
				case 1:
					MovePointer(((Touch)(ref touch2)).position, Camera.main);
					break;
				case 2:
					EndPointer(((Touch)(ref touch2)).position, Camera.main);
					activeTouchFingerId = -1;
					break;
				case 3:
					CancelPointer();
					activeTouchFingerId = -1;
					break;
				}
				break;
			}
		}
	}

	private void BeginPointer(Vector2 screenPos)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		swipeStartPos = screenPos;
		isSwipeTracking = true;
		pointerStartedOverBlockedUi = IsPointerBlockedByUi(screenPos);
	}

	private void MovePointer(Vector2 screenPos, Camera camera)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		if (isSwipeTracking && !pointerStartedOverBlockedUi && !(Vector2.Distance(screenPos, swipeStartPos) <= clickMaxScreenDistance))
		{
			Vector2 worldPos = ScreenToWorldPoint(screenPos, camera);
			TrySwipeCoinAt(worldPos);
		}
	}

	private void EndPointer(Vector2 screenPos, Camera camera)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		if (isSwipeTracking)
		{
			bool num = !pointerStartedOverBlockedUi && Vector2.Distance(screenPos, swipeStartPos) <= clickMaxScreenDistance;
			CancelPointer();
			if (num && lastHandledPointerClickFrame != Time.frameCount)
			{
				lastHandledPointerClickFrame = Time.frameCount;
				HandleSceneClick(screenPos, camera);
			}
		}
	}

	private void CancelPointer()
	{
		isSwipeTracking = false;
		pointerStartedOverBlockedUi = false;
	}

	private void TrySwipeCoinAt(Vector2 worldPos)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		float radius = UiToWorldDistance(coinClickRadius);
		if ((Object)(object)diceWorldController != (Object)null)
		{
			diceWorldController.TrySwipeThrowAt(worldPos, radius);
		}
		GetIdleVatCoinsInRadius(worldPos, radius, coinQueryScratch);
		for (int i = 0; i < coinQueryScratch.Count; i++)
		{
			CoinVat coinVat = coinQueryScratch[i];
			if (!((Object)(object)coinVat == (Object)null) && !swipedCoinsThisFrame.Contains(coinVat) && CanSwipeToThrow(coinVat.coinType))
			{
				TriggerCoinThrow(coinVat);
				swipedCoinsThisFrame.Add(coinVat);
			}
		}
	}

	private bool CanSwipeToThrow(CoinType coinType)
	{
		if ((Object)(object)SingletonMono<UpgradeManager>.Instance == (Object)null)
		{
			return false;
		}
		return SingletonMono<UpgradeManager>.Instance.CanPlayerSwipeToThrow(coinType);
	}

	private Vector2 ScreenToWorldPoint(Vector2 screenPos, Camera camera)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		Camera val = (((Object)(object)camera != (Object)null) ? camera : Camera.main);
		if ((Object)(object)val == (Object)null)
		{
			return UiToWorldPoint(screenPos);
		}
		Ray val2 = val.ScreenPointToRay(Vector2.op_Implicit(screenPos));
		Plane val3 = default(Plane);
		((Plane)(ref val3))._002Ector(Vector3.forward, new Vector3(0f, 0f, vatWorldZ));
		float num = default(float);
		if (((Plane)(ref val3)).Raycast(val2, ref num))
		{
			Vector3 point = ((Ray)(ref val2)).GetPoint(num);
			return new Vector2(point.x, point.y);
		}
		Vector3 val4 = default(Vector3);
		((Vector3)(ref val4))._002Ector(screenPos.x, screenPos.y, Mathf.Abs(((Component)val).transform.position.z - vatWorldZ));
		Vector3 val5 = val.ScreenToWorldPoint(val4);
		return new Vector2(val5.x, val5.y);
	}

	private Vector2 WorldToScreenPoint(Vector3 worldPosition)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		Camera main = Camera.main;
		if ((Object)(object)main == (Object)null)
		{
			float num = UiToWorldScale;
			return new Vector2(worldPosition.x / num, worldPosition.y / num);
		}
		return RectTransformUtility.WorldToScreenPoint(main, worldPosition);
	}

	private Vector2 WorldToRectLocalPosition(Vector3 worldPosition, RectTransform targetRect)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)targetRect == (Object)null)
		{
			return WorldToScreenPoint(worldPosition);
		}
		Vector2 val = WorldToScreenPoint(worldPosition);
		Camera canvasEventCamera = GetCanvasEventCamera(targetRect);
		Vector2 result = default(Vector2);
		if (RectTransformUtility.ScreenPointToLocalPointInRectangle(targetRect, val, canvasEventCamera, ref result))
		{
			return result;
		}
		return val;
	}

	private Camera GetCanvasEventCamera(RectTransform rect)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		Canvas val = (((Object)(object)rect != (Object)null) ? ((Component)rect).GetComponentInParent<Canvas>() : ((Component)this).GetComponentInParent<Canvas>());
		if ((Object)(object)val == (Object)null || (int)val.renderMode == 0)
		{
			return null;
		}
		if (!((Object)(object)val.worldCamera != (Object)null))
		{
			return Camera.main;
		}
		return val.worldCamera;
	}

	private bool IsPointerBlockedByUi(Vector2 screenPos)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)EventSystem.current == (Object)null)
		{
			return false;
		}
		PointerEventData val = new PointerEventData(EventSystem.current)
		{
			position = screenPos
		};
		uiRaycastResults.Clear();
		EventSystem.current.RaycastAll(val, uiRaycastResults);
		for (int i = 0; i < uiRaycastResults.Count; i++)
		{
			RaycastResult val2 = uiRaycastResults[i];
			GameObject gameObject = ((RaycastResult)(ref val2)).gameObject;
			if (!((Object)(object)gameObject == (Object)null))
			{
				Transform transform = gameObject.transform;
				if (!((Object)(object)transform == (Object)(object)((Component)this).transform) && !transform.IsChildOf(((Component)this).transform))
				{
					return true;
				}
			}
		}
		return false;
	}

	private Vector2 GetRandomSpawnPosition()
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		float safeWorldPadding = GetSafeWorldPadding(50f);
		float radius = UiToWorldDistance(minSpawnDistance);
		Vector2 val = default(Vector2);
		for (int i = 0; i < 50; i++)
		{
			float num = Random.Range(boundaryMin.x + safeWorldPadding, boundaryMax.x - safeWorldPadding);
			float num2 = Random.Range(boundaryMin.y + safeWorldPadding, boundaryMax.y - safeWorldPadding);
			((Vector2)(ref val))._002Ector(num, num2);
			if (!((Object)(object)FindNearestIdleVatCoin(val, radius) != (Object)null) && !IsInCoinAvoidRect(val))
			{
				return val;
			}
		}
		float num3 = Random.Range(boundaryMin.x + safeWorldPadding, boundaryMax.x - safeWorldPadding);
		float num4 = Random.Range(boundaryMin.y + safeWorldPadding, boundaryMax.y - safeWorldPadding);
		return PushOutFromCoinAvoidRects(new Vector2(num3, num4));
	}

	private Vector2 ClampToSceneBounds(Vector2 pos)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		float safeWorldPadding = GetSafeWorldPadding(sceneBoundaryPadding);
		return new Vector2(Mathf.Clamp(pos.x, boundaryMin.x + safeWorldPadding, boundaryMax.x - safeWorldPadding), Mathf.Clamp(pos.y, boundaryMin.y + safeWorldPadding, boundaryMax.y - safeWorldPadding));
	}

	private float GetSafeWorldPadding(float uiPadding)
	{
		float num = Mathf.Max(0f, UiToWorldDistance(uiPadding));
		float num2 = Mathf.Abs(boundaryMax.x - boundaryMin.x);
		float num3 = Mathf.Abs(boundaryMax.y - boundaryMin.y);
		float num4 = Mathf.Min(num2, num3) * 0.49f;
		return Mathf.Min(num, Mathf.Max(0f, num4));
	}

	private bool IsInCoinAvoidRect(Vector2 pos)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		if (!enableCoinAvoidRects || coinAvoidRects.Count == 0)
		{
			return false;
		}
		for (int i = 0; i < coinAvoidRects.Count; i++)
		{
			Rect val = coinAvoidRects[i];
			if (((Rect)(ref val)).Contains(pos))
			{
				return true;
			}
		}
		return false;
	}

	private Vector2 PushOutFromCoinAvoidRects(Vector2 pos)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		if (!enableCoinAvoidRects || coinAvoidRects.Count == 0)
		{
			return ClampToSceneBounds(pos);
		}
		Vector2 val = pos;
		for (int i = 0; i < coinAvoidRects.Count; i++)
		{
			Rect val2 = coinAvoidRects[i];
			if (((Rect)(ref val2)).Contains(val))
			{
				float num = Mathf.Abs(val.x - ((Rect)(ref val2)).xMin);
				float num2 = Mathf.Abs(((Rect)(ref val2)).xMax - val.x);
				float num3 = Mathf.Abs(val.y - ((Rect)(ref val2)).yMin);
				float num4 = Mathf.Abs(((Rect)(ref val2)).yMax - val.y);
				float num5 = Mathf.Min(new float[4] { num, num2, num3, num4 });
				float num6 = Mathf.Max(0.001f, UiToWorldDistance(1f));
				if (Mathf.Approximately(num5, num))
				{
					val.x = ((Rect)(ref val2)).xMin - num6;
				}
				else if (Mathf.Approximately(num5, num2))
				{
					val.x = ((Rect)(ref val2)).xMax + num6;
				}
				else if (Mathf.Approximately(num5, num3))
				{
					val.y = ((Rect)(ref val2)).yMin - num6;
				}
				else
				{
					val.y = ((Rect)(ref val2)).yMax + num6;
				}
			}
		}
		return ClampToSceneBounds(val);
	}

	private void OnCoinClicked(CoinVat coin)
	{
		TriggerCoinThrow(coin);
	}

	private void OnCoinFlipComplete(CoinVat coin, bool isFront)
	{
		if (!((Object)(object)coin == (Object)null))
		{
			double multiplier = 1.0;
			if (!isFront)
			{
				multiplier = this.coinFlipComplete?.Invoke() ?? 0f;
			}
			int worldId = (((Object)(object)GameDataManager.Instance != (Object)null) ? GameDataManager.Instance.GetCurrentWorldId() : 0);
			WorldSettlementEntry entry = new WorldSettlementEntry((int)coin.coinType, isFront ? 1 : 0, GetCoinValue(coin.coinType, coin.roleType), multiplier);
			WorldSettlementRequest request = new WorldSettlementRequest(worldId, 1, (coin.roleType <= 0) ? WorldSettlementSource.Manual : WorldSettlementSource.Helper, entry);
			WorldSettlementService.Instance.TrySettle(in request, out var _);
		}
	}

	private void OnCoinLanded(CoinVat coin)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)coin == (Object)null)
		{
			return;
		}
		if (IsInCoinAvoidRect(Vector2.op_Implicit(((Component)coin).transform.position)))
		{
			((Component)coin).transform.position = Vector2.op_Implicit(PushOutFromCoinAvoidRects(Vector2.op_Implicit(((Component)coin).transform.position)));
		}
		if (coin.SuppressLandingEffects)
		{
			coin.isLuckyEcho = false;
			return;
		}
		TriggerChainReaction(coin);
		if (!coin.isLuckyEcho)
		{
			TriggerLuckyEcho(coin);
		}
		else
		{
			coin.isLuckyEcho = false;
		}
		ShowGuideStep2(coin);
	}

	private void TriggerChainReaction(CoinVat coin)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)SingletonMono<UpgradeManager>.Instance == (Object)null || (Object)(object)coin == (Object)null)
		{
			return;
		}
		GetIdleVatCoinsInRadius(Vector2.op_Implicit(((Component)coin).transform.position), UiToWorldDistance(100f), chainReactionScratch);
		for (int i = 0; i < chainReactionScratch.Count; i++)
		{
			CoinVat coinVat = chainReactionScratch[i];
			if ((Object)(object)coinVat == (Object)null || (Object)(object)coinVat == (Object)(object)coin)
			{
				continue;
			}
			switch (coin.coinType)
			{
			case CoinType.Silver:
				if (coinVat.coinType == CoinType.Copper && SingletonMono<UpgradeManager>.Instance.CanSilverTriggerCopper())
				{
					TriggerCoinThrow(coinVat);
				}
				break;
			case CoinType.Gold:
				if (SingletonMono<UpgradeManager>.Instance.CanGoldTriggerCoin(coinVat.coinType))
				{
					TriggerCoinThrow(coinVat);
				}
				break;
			}
		}
	}

	private void TriggerLuckyEcho(CoinVat coin)
	{
		if (!((Object)(object)SingletonMono<UpgradeManager>.Instance == (Object)null) && !((Object)(object)coin == (Object)null))
		{
			float luckyEchoProbability = SingletonMono<UpgradeManager>.Instance.GetLuckyEchoProbability();
			if (luckyEchoProbability > 0f && Random.value < luckyEchoProbability && !coin.isLuckyEcho)
			{
				ScheduleLuckyEcho(coin, 0.3f);
				coin.isLuckyEcho = true;
			}
		}
	}

	private void ScheduleLuckyEcho(CoinVat coin, float delay)
	{
		for (int i = 0; i < luckyEchoTasks.Count; i++)
		{
			if ((Object)(object)luckyEchoTasks[i].Coin == (Object)(object)coin)
			{
				LuckyEchoTask value = luckyEchoTasks[i];
				value.Remaining = delay;
				luckyEchoTasks[i] = value;
				return;
			}
		}
		luckyEchoTasks.Add(new LuckyEchoTask
		{
			Coin = coin,
			Remaining = delay
		});
	}

	private void TickLuckyEcho(float deltaTime)
	{
		for (int num = luckyEchoTasks.Count - 1; num >= 0; num--)
		{
			LuckyEchoTask value = luckyEchoTasks[num];
			if ((Object)(object)value.Coin == (Object)null)
			{
				luckyEchoTasks.RemoveAt(num);
			}
			else
			{
				value.Remaining -= deltaTime;
				if (value.Remaining > 0f)
				{
					luckyEchoTasks[num] = value;
				}
				else
				{
					luckyEchoTasks.RemoveAt(num);
					if (value.Coin.State == CoinState.Idle)
					{
						TriggerCoinThrow(value.Coin);
					}
				}
			}
		}
	}

	private double GetCoinValue(CoinType coinType, int roleType)
	{
		if ((Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null)
		{
			switch (coinType)
			{
			case CoinType.Copper:
				return SingletonMono<UpgradeManager>.Instance.GetCopperCoinValue(roleType);
			case CoinType.Silver:
				return SingletonMono<UpgradeManager>.Instance.GetSilverCoinValue(roleType);
			case CoinType.Gold:
				return SingletonMono<UpgradeManager>.Instance.GetGoldCoinValue(roleType);
			case CoinType.Purple:
				return SingletonMono<UpgradeManager>.Instance.GetPurpleCoinValue(roleType);
			}
		}
		return coinType switch
		{
			CoinType.Copper => 1.0, 
			CoinType.Silver => 5.0, 
			CoinType.Gold => 10.0, 
			CoinType.Purple => 15.0, 
			_ => 0.0, 
		};
	}

	private void InsertIntoSortedCache(CoinVat coin)
	{
		for (int i = 0; i < sortedCoinsCache.Count; i++)
		{
			if (coin.coinType > sortedCoinsCache[i].coinType)
			{
				sortedCoinsCache.Insert(i, coin);
				return;
			}
		}
		sortedCoinsCache.Add(coin);
	}

	private GameObject GetCoinPrefab(CoinType coinType)
	{
		return (GameObject)(coinType switch
		{
			CoinType.Copper => coinCopperPrefab, 
			CoinType.Silver => coinSilverPrefab, 
			CoinType.Gold => coinGoldPrefab, 
			CoinType.Purple => coinPurplePrefab, 
			_ => coinCopperPrefab, 
		});
	}

	private Sprite[] GetCoinSpriteFrames(CoinType coinType)
	{
		GameObject coinPrefab = GetCoinPrefab(coinType);
		if ((Object)(object)coinPrefab == (Object)null)
		{
			return null;
		}
		UISpriteAnimationSequence component = coinPrefab.GetComponent<UISpriteAnimationSequence>();
		if (!((Object)(object)component != (Object)null))
		{
			return null;
		}
		return component.SpriteFrames;
	}

	private IEnumerator StartGuideClickCoin()
	{
		if ((GameDataManager.Instance?.GetCurrentWorldId() ?? 1) != 1)
		{
			yield break;
		}
		yield return (object)new WaitForEndOfFrame();
		while (activeCoins.Count == 0)
		{
			if ((GameDataManager.Instance?.GetCurrentWorldId() ?? 1) != 1)
			{
				yield break;
			}
			yield return null;
		}
		Time.timeScale = 0f;
		yield return (object)new WaitForEndOfFrame();
		int index = Random.Range(0, activeCoins.Count);
		CoinVat coin = activeCoins[index];
		ShowGuideClickCoin(coin, OnGuideFingerClicked);
	}

	private void OnGuideFingerClicked(CoinVat coin)
	{
		if ((Object)(object)guideFinger != (Object)null)
		{
			((Component)guideFinger).gameObject.SetActive(false);
		}
		coin.OnGuideFinger -= OnGuideFingerClicked;
		UIManager.Instance.HideDialogPanel();
		Time.timeScale = 1f;
		PlayerPrefs.SetInt("guide_step_1", 1);
		PlayerPrefs.Save();
		GameDataManager.Instance.ForceUpdateNewGuide();
	}

	private void ShowGuideStep2(CoinVat coin)
	{
		if (PlayerPrefs.GetInt("guide_step_2", 0) != 0)
		{
			return;
		}
		int num = PlayerPrefs.GetInt("guide_step_2_count", 0);
		if (num < 1 || num > 2)
		{
			return;
		}
		Time.timeScale = 0f;
		string dialogText = (coin.IsFrontShowing ? $"赚到钱啦！接下来先赚{25}块钱" : "反面没有收益，正面才有钱哦");
		int index = Random.Range(0, activeCoins.Count);
		CoinVat coin2 = activeCoins[index];
		ShowGuideClickCoin(coin2, step2, dialogText, isShow: false);
		UIManager.Instance.ShowDialogPanel(dialogText, DialogPosition.Bottom, 0.001f, delegate
		{
			UIManager.Instance.HideDialogPanel();
			Time.timeScale = 1f;
			if (PlayerPrefs.GetInt("guide_step_2_count", 0) >= 2)
			{
				PlayerPrefs.SetInt("guide_step_2", 1);
				PlayerPrefs.Save();
				GameDataManager.Instance.ForceUpdateNewGuide();
			}
		});
	}

	private void step2(CoinVat coin)
	{
		if (PlayerPrefs.GetInt("guide_step_2", 0) != 0)
		{
			if ((Object)(object)guideFinger != (Object)null)
			{
				((Component)guideFinger).gameObject.SetActive(false);
			}
			coin.OnGuideFinger -= step2;
			UIManager.Instance.HideDialogPanel();
			Time.timeScale = 1f;
		}
		else
		{
			coin.OnGuideFinger -= step2;
			ShowGuideClickCoin(coin, step2, "", isShow: false);
		}
	}

	private void OnGoldChangedForGuide3(double gold)
	{
		if (gold >= 25.0 && PlayerPrefs.GetInt("guide_step_3", 0) == 0)
		{
			if ((Object)(object)guideFinger != (Object)null)
			{
				((Component)guideFinger).gameObject.SetActive(false);
			}
			int currentWorldHelperUpgradeId = Constants.GetCurrentWorldHelperUpgradeId();
			if ((Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null && SingletonMono<UpgradeManager>.Instance.GetUpgradeLevel(currentWorldHelperUpgradeId, "itemUpgrade1") > 1)
			{
				CompleteGuide3();
				return;
			}
			SingletonMono<UpgradeManager>.Instance.OnGoldChanged -= OnGoldChangedForGuide3;
			UIManager.Instance.upgradePanel?.ShowGuideFinger(1, 1);
			UIManager.Instance.ShowDialogPanel("现在该雇佣一个帮手，为你打工啦！", DialogPosition.Top, 0.0001f);
		}
	}

	private void CompleteGuide3()
	{
		SingletonMono<UpgradeManager>.Instance.OnGoldChanged -= OnGoldChangedForGuide3;
		PlayerPrefs.SetInt("guide_step_3", 1);
		PlayerPrefs.Save();
		GameDataManager.Instance.ForceUpdateNewGuide();
	}
}
