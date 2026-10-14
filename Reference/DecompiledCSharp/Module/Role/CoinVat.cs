using System;
using Module.UI;
using Module.Vat;
using Spine;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Module.Role;

public class CoinVat : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerMoveHandler, IRoleThrowableVat
{
	private const string SmokePrefabPath = "Assets/GameRes/Role/coins_smoke.prefab";

	private const string EvolutionUpgradePrefabPath = "Assets/GameRes/Role/coin_upgrade.prefab";

	private const string EvolutionUpgradeAnimName = "effect";

	[Header("Coin")]
	public CoinType coinType;

	[SerializeField]
	[Range(0f, 1f)]
	private float frontWeight = 0.65f;

	private const string FlipAnimationName = "flip";

	private const string FrontAnimationName = "front";

	private const string BackAnimationName = "back";

	[Header("Animation")]
	[SerializeField]
	private SpriteVatAnimationSequence spriteAnimation;

	[SerializeField]
	private SpineVat spineVat;

	[Header("Floating Text")]
	[SerializeField]
	private float floatHeight = 120f;

	[SerializeField]
	private float floatDuration = 1f;

	[SerializeField]
	private float fontSize = 48f;

	[SerializeField]
	private float scaleCompensation = 1f;

	[SerializeField]
	private float fadeStartRatio = 0.5f;

	[SerializeField]
	private float floatingTextAniScale = 2.8f;

	private CoinState state;

	private bool isFrontShowing = true;

	private Vector2 movementBoundaryMin;

	private Vector2 movementBoundaryMax;

	private bool hasMovementBoundary;

	private Vector3 originalScale = Vector3.one;

	private bool isThrowAnimating;

	private Vector2 throwStartPos;

	private Vector2 throwTargetPos;

	private float throwElapsed;

	private float throwDelayTimer;

	private bool isWaitingForThrowDelay;

	private bool suppressLandingEffects;

	private bool isEvolutionMerging;

	private Vector2 evolutionMergeStart;

	private Vector2 evolutionMergeTarget;

	private float evolutionMergeElapsed;

	private float evolutionMergeDuration;

	private bool isEvolutionShaking;

	private Vector2 evolutionShakeBasePos;

	private float evolutionShakeElapsed;

	private float evolutionShakeDuration;

	private int pendingEvolutionTier;

	private bool isEvolutionRevealPending;

	private const float EvolutionShakeAmplitude = 0.12f;

	private const float EvolutionShakeFrequency = 52f;

	private const int EvolutionTopSortingBoost = 30000;

	private const float EvolutionCenterScale = 3f;

	private bool isEvolutionScaleDown;

	private float evolutionScaleDownElapsed;

	private const float EvolutionScaleDownDuration = 0.5f;

	private int roleTypeValue;

	private int dicePoint;

	private bool hoverTriggerEnabled;

	private bool isInitialized;

	private MainSceneVat mainScene;

	private bool useSpineVat;

	private SpineVatAsset spineVatAsset;

	private int evolutionTier;

	private Shader cachedSpriteVatShader;

	private TextMeshProUGUI floatingTextUI;

	private TextMeshProUGUI floatingNormalUI;

	private TextMeshProUGUI floatingCriticalUI;

	private RectTransform floatingTextRect;

	private Transform floatingTextPool;

	private bool isFloatingTextAnimating;

	private float floatingTextElapsed;

	private Vector2 floatingTextStartPos;

	private float floatingTextRandomOffsetX;

	private float worldDistanceScale = 1f;

	private Shader cachedVatShader;

	private string cachedSortingLayerName;

	private int cachedSortingOrder;

	private SpriteVatAnimationSequence smokeAnimation;

	private GameObject smokeEffectObject;

	private bool isLoadingSmokeEffect;

	public Action<CoinVat> OnCoinClicked;

	public Action<CoinVat, bool> OnFlipComplete;

	public Action<CoinVat> OnLanded;

	public bool isLuckyEcho;

	public CoinState State => state;

	public bool IsFrontShowing => isFrontShowing;

	public bool IsHoverTriggerEnabled => hoverTriggerEnabled;

	public int roleType => roleTypeValue;

	public int DicePoint
	{
		get
		{
			return dicePoint;
		}
		set
		{
			dicePoint = value;
		}
	}

	public bool SuppressLandingEffects => suppressLandingEffects;

	public bool UseExternalTick { get; set; }

	public bool UsesSpineVat => useSpineVat;

	public SpineVat SpineVatComponent => spineVat;

	public SpriteVatAnimationSequence SpriteAnimation => spriteAnimation;

	public Transform VatTransform => ((Component)this).transform;

	public bool IsIdleForRole => state == CoinState.Idle;

	public float RoleThrowDuration => 0.8f;

	public int EvolutionTier => evolutionTier;

	public bool IsEvolutionMerging => isEvolutionMerging;

	public bool IsEvolutionShaking => isEvolutionShaking;

	public event Action<CoinVat> OnGuideFinger;

	private void Update()
	{
		if (!UseExternalTick)
		{
			TickExternal(Time.deltaTime);
		}
	}

	private void OnDestroy()
	{
		if ((Object)(object)smokeEffectObject != (Object)null)
		{
			Object.Destroy((Object)(object)smokeEffectObject);
		}
	}

	public void TickExternal(float dt)
	{
		if (useSpineVat)
		{
			if ((Object)(object)spineVat != (Object)null)
			{
				spineVat.TickExternal(dt);
			}
		}
		else if ((Object)(object)spriteAnimation != (Object)null)
		{
			spriteAnimation.TickExternal(dt);
		}
		if (isEvolutionMerging || isEvolutionShaking || isEvolutionScaleDown)
		{
			TickEvolutionEffects(dt);
			if (isFloatingTextAnimating)
			{
				TickFloatingTextAnimation(dt);
			}
			return;
		}
		if (isWaitingForThrowDelay)
		{
			throwDelayTimer -= dt;
			if (throwDelayTimer <= 0f)
			{
				isWaitingForThrowDelay = false;
				if (state == CoinState.Flying && isThrowAnimating)
				{
					PlayFlipAnimation();
				}
			}
		}
		if (isThrowAnimating && state == CoinState.Flying)
		{
			TickThrowAnimation(dt);
		}
		if (isFloatingTextAnimating)
		{
			TickFloatingTextAnimation(dt);
		}
	}

	public void SetMainScene(MainSceneVat scene)
	{
		mainScene = scene;
	}

	public void Initialize(CoinType type, SpineVatAsset vatAsset, Vector2 boundaryMin, Vector2 boundaryMax, Transform textPool, float distanceScale = 1f, int evoTier = 0)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		coinType = type;
		evolutionTier = Mathf.Max(0, evoTier);
		originalScale = ((Component)this).transform.localScale;
		worldDistanceScale = Mathf.Max(0.0001f, distanceScale);
		SetMovementBoundary(boundaryMin, boundaryMax);
		useSpineVat = (Object)(object)vatAsset != (Object)null;
		spineVatAsset = vatAsset;
		if (useSpineVat)
		{
			EnsureSpineVat();
			DisableSpriteAnimationIfPresent();
			if ((Object)(object)cachedVatShader != (Object)null)
			{
				spineVat.SetVatShader(cachedVatShader);
			}
			spineVat.SetVatAssetNoPlay(vatAsset);
			spineVat.SetDepthSortRange(boundaryMin.y, boundaryMax.y);
			spineVat.SetSortingLayerAndOrder(cachedSortingLayerName, cachedSortingOrder);
			ShowFaceAnimation(front: true);
		}
		else
		{
			EnsureAnimation();
		}
		if ((Object)(object)mainScene == (Object)null || !mainScene.UseSceneFloatingTextPool)
		{
			LoadFloatingTextFromPool(textPool);
		}
		isInitialized = true;
	}

	public void ApplyEvolutionSkin(SpineVatAsset vatAsset, int evoTier)
	{
		if (!((Object)(object)vatAsset == (Object)null))
		{
			evolutionTier = Mathf.Max(0, evoTier);
			spineVatAsset = vatAsset;
			useSpineVat = true;
			EnsureSpineVat();
			DisableSpriteAnimationIfPresent();
			if ((Object)(object)cachedVatShader != (Object)null)
			{
				spineVat.SetVatShader(cachedVatShader);
			}
			spineVat.SetVatAssetNoPlay(vatAsset);
			ShowFaceAnimation(isFrontShowing);
		}
	}

	public void Initialize(CoinType type, Sprite[] spriteFrames, Vector2 boundaryMin, Vector2 boundaryMax, Transform textPool, float distanceScale = 1f)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		coinType = type;
		originalScale = ((Component)this).transform.localScale;
		worldDistanceScale = Mathf.Max(0.0001f, distanceScale);
		SetMovementBoundary(boundaryMin, boundaryMax);
		useSpineVat = false;
		spineVatAsset = null;
		EnsureAnimation();
		spriteAnimation.SetDepthSortRange(boundaryMin.y, boundaryMax.y);
		if ((Object)(object)spriteAnimation != (Object)null)
		{
			if ((Object)(object)cachedVatShader != (Object)null)
			{
				spriteAnimation.SetVatShader(cachedVatShader);
			}
			spriteAnimation.SetSortingLayerAndOrder(cachedSortingLayerName, cachedSortingOrder);
			spriteAnimation.AutoPlay = false;
			spriteAnimation.IsLooping = false;
			spriteAnimation.SpriteFrames = spriteFrames;
			spriteAnimation.ShowFrontFace();
		}
		if ((Object)(object)mainScene == (Object)null || !mainScene.UseSceneFloatingTextPool)
		{
			LoadFloatingTextFromPool(textPool);
		}
		isInitialized = true;
	}

	public void SetVatShader(Shader shader)
	{
		cachedVatShader = shader;
		if (!((Object)(object)shader == (Object)null))
		{
			if (isInitialized && useSpineVat)
			{
				EnsureSpineVat();
				spineVat.SetVatShader(shader);
			}
			else if (isInitialized)
			{
				EnsureAnimation();
				spriteAnimation.SetVatShader(shader);
			}
			if ((Object)(object)smokeAnimation != (Object)null)
			{
				Shader vatShader = (((Object)(object)cachedSpriteVatShader != (Object)null) ? cachedSpriteVatShader : shader);
				smokeAnimation.SetVatShader(vatShader);
			}
		}
	}

	public void SetSpriteVatShader(Shader shader)
	{
		cachedSpriteVatShader = shader;
		if ((Object)(object)smokeAnimation != (Object)null && (Object)(object)shader != (Object)null)
		{
			smokeAnimation.SetVatShader(shader);
		}
	}

	public void SetSortingLayerAndOrder(string layerName, int order)
	{
		cachedSortingLayerName = layerName;
		cachedSortingOrder = order;
		if (isInitialized && useSpineVat)
		{
			EnsureSpineVat();
			spineVat.SetSortingLayerAndOrder(layerName, order);
		}
		else if (isInitialized)
		{
			EnsureAnimation();
			spriteAnimation.SetSortingLayerAndOrder(layerName, order);
		}
		if ((Object)(object)smokeAnimation != (Object)null)
		{
			smokeAnimation.SetSortingLayerAndOrder(layerName, GetSmokeSortingOrder());
		}
	}

	public void SetGpuBatchRenderingEnabled(bool enabled)
	{
		if (useSpineVat)
		{
			EnsureSpineVat();
			spineVat.SetIndirectRenderingEnabled(enabled);
		}
		else
		{
			EnsureAnimation();
			spriteAnimation.SetBatchRenderingEnabled(enabled);
		}
	}

	public void SetRenderingVisible(bool visible)
	{
		if (useSpineVat)
		{
			EnsureSpineVat();
			if ((Object)(object)spineVat != (Object)null)
			{
				((Behaviour)spineVat).enabled = visible;
				MeshRenderer val = default(MeshRenderer);
				if (((Component)spineVat).TryGetComponent<MeshRenderer>(ref val))
				{
					((Renderer)val).enabled = visible;
				}
			}
		}
		else
		{
			EnsureAnimation();
			spriteAnimation.SetRenderingVisible(visible);
		}
	}

	public void RefreshDebugVisuals()
	{
		SetRenderingVisible(VatDebugSettings.ShowCoins);
		if (!VatDebugSettings.ShowCoinSmoke)
		{
			HideSmokeEffect();
		}
		if (!VatDebugSettings.ShowFloatingText)
		{
			isFloatingTextAnimating = false;
			if ((Object)(object)floatingNormalUI != (Object)null)
			{
				((Component)floatingNormalUI).gameObject.SetActive(false);
			}
			if ((Object)(object)floatingCriticalUI != (Object)null)
			{
				((Component)floatingCriticalUI).gameObject.SetActive(false);
			}
		}
	}

	public void OnCoinClick()
	{
		((Component)this).transform.SetAsLastSibling();
		if (state == CoinState.Idle)
		{
			OnCoinClicked?.Invoke(this);
		}
	}

	public void JumpToTarget(Vector2 startPos, Vector2 targetPos, int sourceRoleType, bool suppressLandingEffects = false)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		if (state != CoinState.Flying && !isWaitingForThrowDelay)
		{
			roleTypeValue = sourceRoleType;
			this.suppressLandingEffects = suppressLandingEffects;
			((Component)this).transform.localScale = originalScale;
			((Component)this).transform.position = Vector2.op_Implicit(startPos);
			state = CoinState.Flying;
			isThrowAnimating = true;
			RaiseSortingForFlight();
			throwStartPos = startPos;
			throwTargetPos = targetPos;
			throwElapsed = 0f;
			throwDelayTimer = 0.3f;
			isWaitingForThrowDelay = true;
			mainScene?.InvalidateCoinSpatialIndex();
			if ((Object)(object)SoundManager.Instance != (Object)null && SoundManager.Instance.EnableCoinSound)
			{
				SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/coinup.mp3");
			}
			PrepareFlipAnimationForThrow();
		}
	}

	public bool TryThrowByRole(Vector2 startPosition, Vector2 targetPosition, int sourceRoleType)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		if (!IsIdleForRole)
		{
			return false;
		}
		JumpToTarget(startPosition, targetPosition, sourceRoleType);
		return true;
	}

	public void SetMovementBoundary(Vector2 min, Vector2 max)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		movementBoundaryMin = min;
		movementBoundaryMax = max;
		hasMovementBoundary = true;
	}

	public void SetInitialPosition(Vector2 pos)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		StopEvolutionEffects();
		((Component)this).transform.position = Vector2.op_Implicit(pos);
		state = CoinState.Idle;
		isFrontShowing = true;
		roleTypeValue = 0;
		suppressLandingEffects = false;
		((Component)this).transform.localScale = originalScale;
		mainScene?.InvalidateCoinSpatialIndex();
		ShowFaceAnimation(front: true);
	}

	public void StartEvolutionMergeMove(Vector2 targetWorldPos, float duration = 1f)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		CancelThrow();
		StopEvolutionEffects();
		evolutionMergeStart = Vector2.op_Implicit(((Component)this).transform.position);
		evolutionMergeTarget = targetWorldPos;
		evolutionMergeElapsed = 0f;
		evolutionMergeDuration = Mathf.Max(0.05f, duration);
		isEvolutionMerging = true;
		state = CoinState.Evolving;
		((Component)this).transform.localScale = originalScale;
		mainScene?.InvalidateCoinSpatialIndex();
	}

	public void StartEvolutionShake(float duration = 1f, int targetEvolutionTier = -1)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		CancelThrow();
		isEvolutionMerging = false;
		evolutionShakeBasePos = Vector2.op_Implicit(((Component)this).transform.position);
		evolutionShakeElapsed = 0f;
		evolutionShakeDuration = Mathf.Max(0.05f, duration);
		pendingEvolutionTier = ((targetEvolutionTier >= 0) ? targetEvolutionTier : evolutionTier);
		isEvolutionRevealPending = true;
		isEvolutionShaking = true;
		state = CoinState.Evolving;
		((Component)this).transform.position = new Vector3(evolutionShakeBasePos.x, evolutionShakeBasePos.y, 10f);
		((Component)this).transform.localScale = originalScale * 3f;
		((Component)this).transform.SetAsLastSibling();
		SetRenderingVisible(visible: true);
		ApplyEvolutionTopSorting(onTop: true);
		ShowFaceAnimation(isFrontShowing);
		mainScene?.InvalidateCoinSpatialIndex();
	}

	public void StopEvolutionShake()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		if (!isEvolutionShaking)
		{
			return;
		}
		isEvolutionShaking = false;
		if (!((Object)(object)this == (Object)null))
		{
			((Component)this).transform.position = new Vector3(evolutionShakeBasePos.x, evolutionShakeBasePos.y, 10f);
			((Component)this).transform.localScale = originalScale * 3f;
			ApplyEvolutionTopSorting(onTop: true);
			if (state == CoinState.Evolving)
			{
				state = CoinState.Idle;
			}
			mainScene?.InvalidateCoinSpatialIndex();
			PlayEvolutionUpgradeEffect();
		}
	}

	public void StopEvolutionEffects()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)this == (Object)null)
		{
			isEvolutionMerging = false;
			isEvolutionShaking = false;
			isEvolutionRevealPending = false;
			isEvolutionScaleDown = false;
			return;
		}
		if (isEvolutionShaking || isEvolutionRevealPending)
		{
			((Component)this).transform.position = new Vector3(evolutionShakeBasePos.x, evolutionShakeBasePos.y, 10f);
			ApplyEvolutionTopSorting(onTop: false);
		}
		isEvolutionMerging = false;
		isEvolutionShaking = false;
		isEvolutionRevealPending = false;
		isEvolutionScaleDown = false;
		evolutionMergeElapsed = 0f;
		evolutionShakeElapsed = 0f;
		evolutionScaleDownElapsed = 0f;
		((Component)this).transform.localScale = originalScale;
		if (state == CoinState.Evolving)
		{
			state = CoinState.Idle;
		}
	}

	private void FinishEvolutionReveal()
	{
		if ((Object)(object)this == (Object)null || !isEvolutionRevealPending)
		{
			return;
		}
		isEvolutionRevealPending = false;
		if (pendingEvolutionTier != evolutionTier && (Object)(object)mainScene != (Object)null)
		{
			SpineVatAsset coinVatAsset = mainScene.GetCoinVatAsset(coinType, pendingEvolutionTier);
			if ((Object)(object)coinVatAsset != (Object)null)
			{
				ApplyEvolutionSkin(coinVatAsset, pendingEvolutionTier);
			}
			else
			{
				evolutionTier = pendingEvolutionTier;
			}
		}
		isEvolutionScaleDown = true;
		evolutionScaleDownElapsed = 0f;
		ApplyEvolutionTopSorting(onTop: false);
		if ((Object)(object)mainScene != (Object)null && useSpineVat)
		{
			SetGpuBatchRenderingEnabled(mainScene.UseGpuBatch);
		}
		mainScene?.InvalidateCoinSpatialIndex();
	}

	private void ApplyEvolutionTopSorting(bool onTop)
	{
		if ((Object)(object)this == (Object)null)
		{
			return;
		}
		int order = (onTop ? (cachedSortingOrder + 30000) : cachedSortingOrder);
		if (useSpineVat && (Object)(object)spineVat != (Object)null)
		{
			if (onTop)
			{
				spineVat.SetIndirectRenderingEnabled(enabled: false);
				spineVat.SetRenderingVisible(visible: true);
			}
			spineVat.SetSortingLayerAndOrder(cachedSortingLayerName, order);
			spineVat.SetDepthSortZRange(onTop ? (-5f) : (-1f), onTop ? (-3.1f) : 1f);
		}
		else if ((Object)(object)spriteAnimation != (Object)null)
		{
			if (onTop)
			{
				spriteAnimation.SetRenderingVisible(visible: true);
			}
			spriteAnimation.SetSortingLayerAndOrder(cachedSortingLayerName, order);
		}
	}

	private void PlayEvolutionUpgradeEffect()
	{
		if ((Object)(object)this == (Object)null)
		{
			return;
		}
		if ((Object)(object)SingletonMono<ResManager>.Instance == (Object)null)
		{
			FinishEvolutionReveal();
			return;
		}
		Transform coinParent = ((Component)this).transform;
		SingletonMono<ResManager>.Instance.LoadPrefabAsync("Assets/GameRes/Role/coin_upgrade.prefab", delegate(GameObject prefab)
		{
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Expected O, but got Unknown
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0162: Unknown result type (might be due to invalid IL or missing references)
			//IL_0177: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_02df: Expected O, but got Unknown
			if (!((Object)(object)this == (Object)null) && !((Object)(object)coinParent == (Object)null))
			{
				if ((Object)(object)prefab == (Object)null)
				{
					Debug.LogWarning((object)"[CoinVat] 加载合成特效失败: Assets/GameRes/Role/coin_upgrade.prefab", (Object)(object)this);
					FinishEvolutionReveal();
				}
				else
				{
					GameObject canvasGo = new GameObject("EvolutionUpgradeCanvas");
					canvasGo.layer = ((Component)coinParent).gameObject.layer;
					canvasGo.transform.SetParent(coinParent, false);
					canvasGo.transform.localPosition = Vector3.zero;
					canvasGo.transform.localRotation = Quaternion.identity;
					float num = Mathf.Max(0.0001f, worldDistanceScale) * Mathf.Max(0.01f, scaleCompensation);
					canvasGo.transform.localScale = Vector3.one * num;
					Canvas obj = canvasGo.AddComponent<Canvas>();
					obj.renderMode = (RenderMode)2;
					obj.overrideSorting = true;
					obj.sortingLayerName = cachedSortingLayerName;
					obj.sortingOrder = cachedSortingOrder + 30000;
					RectTransform component = canvasGo.GetComponent<RectTransform>();
					if ((Object)(object)component != (Object)null)
					{
						component.sizeDelta = new Vector2(200f, 200f);
						component.pivot = new Vector2(0.5f, 0.5f);
					}
					GameObject obj2 = Object.Instantiate<GameObject>(prefab, canvasGo.transform);
					((Object)obj2).name = "coin_upgrade";
					obj2.transform.localPosition = Vector3.zero;
					obj2.transform.localRotation = Quaternion.identity;
					Transform transform = obj2.transform;
					RectTransform val = (RectTransform)(object)((transform is RectTransform) ? transform : null);
					if (val != null)
					{
						val.anchoredPosition = Vector2.zero;
						((Transform)val).localPosition = Vector3.zero;
					}
					SkeletonGraphic componentInChildren = obj2.GetComponentInChildren<SkeletonGraphic>(true);
					if ((Object)(object)componentInChildren == (Object)null)
					{
						Debug.LogWarning((object)"[CoinVat] coin_upgrade 缺少 SkeletonGraphic: Assets/GameRes/Role/coin_upgrade.prefab", (Object)(object)this);
						Object.Destroy((Object)(object)canvasGo);
						FinishEvolutionReveal();
					}
					else
					{
						((Graphic)componentInChildren).raycastTarget = false;
						if (!componentInChildren.IsValid || componentInChildren.AnimationState == null)
						{
							componentInChildren.Initialize(true);
						}
						if (componentInChildren.AnimationState == null)
						{
							Debug.LogWarning((object)"[CoinVat] SkeletonGraphic.AnimationState 为空，无法播放 effect", (Object)(object)this);
							Object.Destroy((Object)(object)canvasGo);
							FinishEvolutionReveal();
						}
						else
						{
							componentInChildren.AnimationState.ClearTracks();
							Skeleton skeleton = componentInChildren.Skeleton;
							if (skeleton != null)
							{
								skeleton.SetToSetupPose();
							}
							if (componentInChildren.AnimationState.SetAnimation(0, "effect", false) == null)
							{
								Debug.LogWarning((object)"[CoinVat] 找不到动画: effect", (Object)(object)this);
								Object.Destroy((Object)(object)canvasGo);
								FinishEvolutionReveal();
							}
							else
							{
								componentInChildren.AnimationState.Complete += (TrackEntryDelegate)delegate(TrackEntry trackEntry)
								{
									if (trackEntry == null || trackEntry.TrackIndex == 0)
									{
										if ((Object)(object)canvasGo != (Object)null)
										{
											Object.Destroy((Object)(object)canvasGo);
										}
										FinishEvolutionReveal();
									}
								};
							}
						}
					}
				}
			}
		});
	}

	public void CancelThrow()
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		isThrowAnimating = false;
		isWaitingForThrowDelay = false;
		throwDelayTimer = 0f;
		throwElapsed = 0f;
		suppressLandingEffects = true;
		if (state == CoinState.Flying || state == CoinState.Landed)
		{
			state = CoinState.Idle;
		}
		((Component)this).transform.localScale = originalScale;
		RestoreSortingAfterFlight();
	}

	public void SetHoverTriggerEnabled(bool enabled)
	{
		hoverTriggerEnabled = enabled;
	}

	public void SetCoinClick()
	{
		OnCoinClick();
		roleTypeValue = -1;
		throwDelayTimer = 0f;
		this.OnGuideFinger?.Invoke(this);
	}

	public void SetThrowDelayTimer(float delay)
	{
		throwDelayTimer = delay;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
	}

	public void OnPointerMove(PointerEventData eventData)
	{
		if ((Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null && SingletonMono<UpgradeManager>.Instance.CanPlayerSwipeToThrow(coinType))
		{
			SetCoinClick();
		}
	}

	private void EnsureAnimation()
	{
		if ((Object)(object)spriteAnimation == (Object)null)
		{
			spriteAnimation = ((Component)this).GetComponent<SpriteVatAnimationSequence>();
		}
		if ((Object)(object)spriteAnimation == (Object)null)
		{
			spriteAnimation = ((Component)this).gameObject.AddComponent<SpriteVatAnimationSequence>();
		}
		spriteAnimation.UseExternalTick = UseExternalTick;
	}

	private void EnsureSpineVat()
	{
		if ((Object)(object)spineVat == (Object)null)
		{
			spineVat = ((Component)this).GetComponent<SpineVat>();
		}
		if ((Object)(object)spineVat == (Object)null)
		{
			spineVat = ((Component)this).gameObject.AddComponent<SpineVat>();
		}
		spineVat.UseExternalTick = UseExternalTick;
		if (hasMovementBoundary)
		{
			spineVat.SetDepthSortRange(movementBoundaryMin.y, movementBoundaryMax.y);
		}
	}

	private void DisableSpriteAnimationIfPresent()
	{
		if ((Object)(object)spriteAnimation == (Object)null)
		{
			spriteAnimation = ((Component)this).GetComponent<SpriteVatAnimationSequence>();
		}
		if ((Object)(object)spriteAnimation != (Object)null)
		{
			((Behaviour)spriteAnimation).enabled = false;
			spriteAnimation.SetRenderingVisible(visible: false);
		}
	}

	private void RaiseSortingForFlight()
	{
		if (useSpineVat)
		{
			if ((Object)(object)spineVat != (Object)null)
			{
				spineVat.SetSortingLayerAndOrder(cachedSortingLayerName, cachedSortingOrder + 20000);
				spineVat.SetDepthSortZRange(-5f, -3.1f);
			}
		}
		else if ((Object)(object)spriteAnimation != (Object)null)
		{
			spriteAnimation.SetSortingLayerAndOrder(cachedSortingLayerName, cachedSortingOrder + 20000);
		}
	}

	private void RestoreSortingAfterFlight()
	{
		if (useSpineVat)
		{
			if ((Object)(object)spineVat != (Object)null)
			{
				spineVat.SetSortingLayerAndOrder(cachedSortingLayerName, cachedSortingOrder);
				spineVat.SetDepthSortZRange(-1f, 1f);
			}
		}
		else if ((Object)(object)spriteAnimation != (Object)null)
		{
			spriteAnimation.SetSortingLayerAndOrder(cachedSortingLayerName, cachedSortingOrder);
		}
	}

	private void PrepareFlipAnimationForThrow()
	{
		if (useSpineVat)
		{
			ShowFaceAnimation(isFrontShowing);
			return;
		}
		float num = 1f;
		if (TalentDataManager.Instance != null)
		{
			num = TalentDataManager.Instance.CoinRain();
		}
		float num2 = 0.8f * num;
		int num3 = (((Object)(object)spriteAnimation != (Object)null) ? Mathf.Max(1, spriteAnimation.FrameCount) : 8);
		int num4 = 5 * num3;
		float frameInterval = num2 / (float)Mathf.Max(1, num4);
		if ((Object)(object)spriteAnimation != (Object)null)
		{
			spriteAnimation.FrameInterval = frameInterval;
			spriteAnimation.IsLooping = true;
			spriteAnimation.Restart();
			spriteAnimation.Pause();
			ShowFaceAnimation(isFrontShowing);
		}
	}

	private void PlayFlipAnimation()
	{
		if (useSpineVat)
		{
			if ((Object)(object)spineVat != (Object)null)
			{
				spineVat.Play("flip", shouldLoop: true);
			}
		}
		else if ((Object)(object)spriteAnimation != (Object)null)
		{
			spriteAnimation.Play();
		}
	}

	private void ShowFaceAnimation(bool front)
	{
		isFrontShowing = front;
		if (useSpineVat)
		{
			if ((Object)(object)spineVat == (Object)null)
			{
				return;
			}
			string animationName = (front ? "front" : "back");
			if ((Object)(object)spineVat.VatAsset != (Object)null && spineVat.VatAsset.FindAnimation(animationName) != null)
			{
				spineVat.Play(animationName, shouldLoop: false);
				spineVat.Pause();
				return;
			}
			float num = (front ? 1f : 0f);
			if (!spineVat.FreezeAt("flip", num))
			{
				spineVat.Play("flip", shouldLoop: false, num);
				spineVat.Pause();
			}
		}
		else if (!((Object)(object)spriteAnimation == (Object)null))
		{
			spriteAnimation.IsLooping = false;
			if (front)
			{
				spriteAnimation.ShowFrontFace();
			}
			else
			{
				spriteAnimation.ShowBackFace();
			}
		}
	}

	private void TickThrowAnimation(float deltaTime)
	{
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		float num = 1f;
		if (TalentDataManager.Instance != null)
		{
			num = TalentDataManager.Instance.CoinRain();
		}
		float num2 = 0.8f * num;
		if (roleTypeValue == 6)
		{
			int helperAwakeSkillLevel = GameDataManager.Instance.GetHelperAwakeSkillLevel(roleTypeValue);
			if (helperAwakeSkillLevel > 0)
			{
				num2 /= 2f;
				if (helperAwakeSkillLevel > 1)
				{
					num2 = 0f;
				}
			}
		}
		if (num2 <= 0f)
		{
			FinishThrowAnimation();
			return;
		}
		throwElapsed += deltaTime;
		float num3 = Mathf.Clamp01(throwElapsed / num2);
		Vector2 val = Vector2.Lerp(throwStartPos, throwTargetPos, num3);
		float num4 = Mathf.Sin(num3 * (float)Math.PI) * ScaleDistance(CoinConfig.JumpHeight);
		((Component)this).transform.position = Vector2.op_Implicit(val + Vector2.up * num4);
		if (throwElapsed >= num2)
		{
			FinishThrowAnimation();
		}
	}

	private void FinishThrowAnimation()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		isThrowAnimating = false;
		isWaitingForThrowDelay = false;
		throwDelayTimer = 0f;
		RestoreSortingAfterFlight();
		((Component)this).transform.position = Vector2.op_Implicit(ClampToBoundary(throwTargetPos));
		((Component)this).transform.localScale = originalScale;
		int completedRoleType = roleTypeValue;
		bool num = suppressLandingEffects;
		Land();
		if (!num)
		{
			PlaySmoke(completedRoleType);
		}
	}

	private Vector2 ClampToBoundary(Vector2 pos)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		if (!hasMovementBoundary)
		{
			return pos;
		}
		float safeWorldPadding = GetSafeWorldPadding(40f);
		return new Vector2(Mathf.Clamp(pos.x, movementBoundaryMin.x + safeWorldPadding, movementBoundaryMax.x - safeWorldPadding), Mathf.Clamp(pos.y, movementBoundaryMin.y + safeWorldPadding, movementBoundaryMax.y - safeWorldPadding));
	}

	private void Land()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		if (state != CoinState.Landed)
		{
			isThrowAnimating = false;
			isWaitingForThrowDelay = false;
			throwDelayTimer = 0f;
			state = CoinState.Landed;
			((Component)this).transform.localScale = originalScale;
			if (!suppressLandingEffects && (Object)(object)SoundManager.Instance != (Object)null && SoundManager.Instance.EnableCoinSound)
			{
				SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/hitground.mp3");
			}
			float num = frontWeight;
			if (TalentDataManager.Instance != null)
			{
				num *= TalentDataManager.Instance.LuckyFlip();
			}
			bool flag = Random.value < num;
			int num2 = PlayerPrefs.GetInt("guide_step_2", 0);
			int num3 = PlayerPrefs.GetInt("guide_step_2_count", 0);
			if (num2 == 0 && num3 < 2)
			{
				flag = num3 == 1;
				PlayerPrefs.SetInt("guide_step_2_count", num3 + 1);
				PlayerPrefs.Save();
			}
			ShowFaceAnimation(flag);
			if (!suppressLandingEffects)
			{
				ShowIncomeText(flag);
			}
			OnFlipComplete?.Invoke(this, flag);
			OnLanded?.Invoke(this);
			ResetToIdle();
		}
	}

	private void ShowIncomeText(bool isFront)
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		if (!VatDebugSettings.ShowFloatingText)
		{
			return;
		}
		if ((Object)(object)mainScene == (Object)null && (Object)(object)UIManager.Instance != (Object)null)
		{
			mainScene = UIManager.Instance.MainSceneVat;
		}
		if ((Object)(object)mainScene == (Object)null)
		{
			return;
		}
		if (isFront)
		{
			if (!mainScene.ShowCoinFloatingText(((Component)this).transform.position, GetCoinValue()) && mainScene.IsAddText())
			{
				ShowFloatingText(GetCoinValue());
				mainScene.AddTextCount();
			}
		}
		else if (TalentDataManager.Instance != null)
		{
			float num = TalentDataManager.Instance.LossBrings();
			if ((!(num > 0f) || !mainScene.ShowCoinFloatingText(((Component)this).transform.position, GetCoinValue() * (double)num)) && num > 0f && mainScene.IsAddText())
			{
				ShowFloatingText(GetCoinValue() * (double)num);
				mainScene.AddTextCount();
			}
		}
	}

	private double GetCoinValue()
	{
		double num = 0.0;
		if ((Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null)
		{
			switch (coinType)
			{
			case CoinType.Copper:
				num = SingletonMono<UpgradeManager>.Instance.GetCopperCoinValue(roleTypeValue);
				break;
			case CoinType.Silver:
				num = SingletonMono<UpgradeManager>.Instance.GetSilverCoinValue(roleTypeValue);
				break;
			case CoinType.Gold:
				num = SingletonMono<UpgradeManager>.Instance.GetGoldCoinValue(roleTypeValue);
				break;
			case CoinType.Purple:
				num = SingletonMono<UpgradeManager>.Instance.GetPurpleCoinValue(roleTypeValue);
				break;
			}
		}
		else
		{
			switch (coinType)
			{
			case CoinType.Copper:
				num = 1.0;
				break;
			case CoinType.Silver:
				num = 5.0;
				break;
			case CoinType.Gold:
				num = 10.0;
				break;
			case CoinType.Purple:
				num = 15.0;
				break;
			}
		}
		if (dicePoint > 0)
		{
			num *= (double)dicePoint;
		}
		return num;
	}

	public int GetEvolutionUpgradeId()
	{
		return UpgradeManager.GetCoinEvolutionUpgradeId(coinType);
	}

	public int GetEvolutionTier()
	{
		if ((Object)(object)SingletonMono<UpgradeManager>.Instance == (Object)null)
		{
			return 0;
		}
		return SingletonMono<UpgradeManager>.Instance.GetEvolutionTier(GetEvolutionUpgradeId());
	}

	public double GetEvolutionIncomeMultiplier()
	{
		if ((Object)(object)SingletonMono<UpgradeManager>.Instance == (Object)null)
		{
			return 1.0;
		}
		return SingletonMono<UpgradeManager>.Instance.GetEvolutionIncomeMultiplier(GetEvolutionUpgradeId());
	}

	private void LoadFloatingTextFromPool(Transform pool)
	{
		if ((Object)(object)pool == (Object)null || (Object)(object)SingletonMono<ResManager>.Instance == (Object)null)
		{
			return;
		}
		floatingTextPool = pool;
		SingletonMono<ResManager>.Instance.LoadPrefabAsync("Assets/GameRes/Role/coinNormalText.prefab", delegate(GameObject prefab)
		{
			if (!((Object)(object)floatingTextPool != (Object)(object)pool) && !((Object)(object)prefab == (Object)null))
			{
				GameObject val = Object.Instantiate<GameObject>(prefab, pool);
				floatingNormalUI = val.GetComponent<TextMeshProUGUI>();
				if ((Object)(object)floatingNormalUI != (Object)null)
				{
					((Component)floatingNormalUI).gameObject.SetActive(false);
					floatingTextUI = floatingNormalUI;
					floatingTextRect = ((Component)floatingNormalUI).GetComponent<RectTransform>();
				}
			}
		});
		SingletonMono<ResManager>.Instance.LoadPrefabAsync("Assets/GameRes/Role/coinCriticalText.prefab", delegate(GameObject prefab)
		{
			if (!((Object)(object)floatingTextPool != (Object)(object)pool) && !((Object)(object)prefab == (Object)null))
			{
				GameObject val = Object.Instantiate<GameObject>(prefab, pool);
				floatingCriticalUI = val.GetComponent<TextMeshProUGUI>();
				if ((Object)(object)floatingCriticalUI != (Object)null)
				{
					((Component)floatingCriticalUI).gameObject.SetActive(false);
				}
			}
		});
	}

	private void ShowFloatingText(double value)
	{
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)floatingNormalUI == (Object)null))
		{
			((Component)floatingNormalUI).gameObject.SetActive(false);
			if ((Object)(object)floatingCriticalUI != (Object)null)
			{
				((Component)floatingCriticalUI).gameObject.SetActive(false);
			}
			if ((Object)(object)floatingCriticalUI != (Object)null && (Object)(object)SingletonMono<BuffManager>.Instance != (Object)null && SingletonMono<BuffManager>.Instance.GetIsCritical())
			{
				floatingTextUI = floatingCriticalUI;
				((TMP_Text)floatingTextUI).text = NumberFormatter.ConvertToSpriteText("暴" + NumberFormatter.FormatNumber(value));
			}
			else
			{
				floatingTextUI = floatingNormalUI;
				((TMP_Text)floatingTextUI).text = NumberFormatter.ConvertToSpriteText("+" + NumberFormatter.FormatNumber(value));
			}
			floatingTextRect = ((Component)floatingTextUI).GetComponent<RectTransform>();
			((Graphic)floatingTextUI).color = Color.white;
			((TMP_Text)floatingTextUI).alignment = (TextAlignmentOptions)514;
			((TMP_Text)floatingTextUI).fontSize = fontSize * GetTextScale();
			SetFloatingTextUiPosition();
			((TMP_Text)floatingTextUI).transform.localScale = Vector3.one * floatingTextAniScale;
			((Component)floatingTextUI).gameObject.SetActive(true);
			StartFloatingTextAnimation();
		}
	}

	private void SetFloatingTextUiPosition()
	{
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)mainScene == (Object)null && (Object)(object)UIManager.Instance != (Object)null)
		{
			mainScene = UIManager.Instance.MainSceneVat;
		}
		if ((Object)(object)mainScene != (Object)null && (Object)(object)floatingTextRect != (Object)null)
		{
			floatingTextRect.anchoredPosition = Vector2.op_Implicit(mainScene.WorldToTextLocalPosition(((Component)this).transform.position));
			((Transform)floatingTextRect).localRotation = Quaternion.identity;
		}
		else
		{
			((TMP_Text)floatingTextUI).transform.position = ((Component)this).transform.position;
		}
	}

	private float GetTextScale()
	{
		if ((Object)(object)mainScene == (Object)null && (Object)(object)UIManager.Instance != (Object)null)
		{
			mainScene = UIManager.Instance.MainSceneVat;
		}
		if ((Object)(object)mainScene == (Object)null)
		{
			return 1f;
		}
		int maxTextCount = mainScene.GetMaxTextCount();
		if (maxTextCount <= 0)
		{
			return 1f;
		}
		return Mathf.Clamp(1f - 0.4f * ((float)mainScene.GetCurrentTextCount() / (float)maxTextCount), 0.6f, 1f);
	}

	private void StartFloatingTextAnimation()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)floatingTextUI == (Object)null || (Object)(object)floatingTextRect == (Object)null)
		{
			isFloatingTextAnimating = false;
			return;
		}
		isFloatingTextAnimating = true;
		floatingTextElapsed = 0f;
		floatingTextStartPos = Vector2.op_Implicit(((Transform)floatingTextRect).localPosition);
		floatingTextRandomOffsetX = Random.Range(-30f, 30f);
		((Graphic)floatingTextUI).color = Color.white;
	}

	private void TickFloatingTextAnimation(float deltaTime)
	{
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)floatingTextUI == (Object)null || (Object)(object)floatingTextRect == (Object)null)
		{
			isFloatingTextAnimating = false;
			return;
		}
		if (floatDuration <= 0f)
		{
			FinishFloatingTextAnimation();
			return;
		}
		floatingTextElapsed += deltaTime * 2f;
		float num = Mathf.Clamp01(floatingTextElapsed / floatDuration);
		float num2 = 1f - (1f - num) * (1f - num);
		((Transform)floatingTextRect).localPosition = new Vector3(floatingTextStartPos.x + floatingTextRandomOffsetX * (2f - num), floatingTextStartPos.y + floatHeight * num2, 0f);
		if (num > fadeStartRatio)
		{
			((Graphic)floatingTextUI).color = Color.white;
		}
		if (floatingTextElapsed >= floatDuration)
		{
			FinishFloatingTextAnimation();
		}
	}

	private void FinishFloatingTextAnimation()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)floatingTextUI == (Object)null || (Object)(object)floatingTextRect == (Object)null)
		{
			isFloatingTextAnimating = false;
			return;
		}
		isFloatingTextAnimating = false;
		((Component)floatingTextUI).gameObject.SetActive(false);
		((Transform)floatingTextRect).localPosition = Vector2.op_Implicit(floatingTextStartPos);
		((Graphic)floatingTextUI).color = Color.white;
		((TMP_Text)floatingTextUI).transform.localScale = Vector3.one * scaleCompensation;
		if ((Object)(object)mainScene == (Object)null && (Object)(object)UIManager.Instance != (Object)null)
		{
			mainScene = UIManager.Instance.MainSceneVat;
		}
		if ((Object)(object)mainScene != (Object)null)
		{
			mainScene.ReduceTextCount();
		}
	}

	private void PlaySmoke(int completedRoleType)
	{
		if (!VatDebugSettings.ShowCoinSmoke)
		{
			HideSmokeEffect();
		}
		else
		{
			if (completedRoleType == 6 && GameDataManager.Instance.GetHelperAwakeSkillLevel(completedRoleType) > 1)
			{
				return;
			}
			if ((Object)(object)smokeAnimation != (Object)null)
			{
				PlaySmokeAnimation();
			}
			else
			{
				if (isLoadingSmokeEffect || (Object)(object)SingletonMono<ResManager>.Instance == (Object)null)
				{
					return;
				}
				isLoadingSmokeEffect = true;
				SingletonMono<ResManager>.Instance.LoadPrefabAsync("Assets/GameRes/Role/coins_smoke.prefab", delegate(GameObject prefab)
				{
					//IL_0055: Unknown result type (might be due to invalid IL or missing references)
					//IL_005f: Expected O, but got Unknown
					//IL_00be: Unknown result type (might be due to invalid IL or missing references)
					//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
					isLoadingSmokeEffect = false;
					if (!((Object)(object)this == (Object)null) && !((Object)(object)prefab == (Object)null))
					{
						UISpriteAnimationSequence component = prefab.GetComponent<UISpriteAnimationSequence>();
						Sprite[] array = (((Object)(object)component != (Object)null) ? component.SpriteFrames : null);
						if (array == null || array.Length == 0)
						{
							Debug.LogWarning((object)"[CoinVat] Missing smoke sprite frames: Assets/GameRes/Role/coins_smoke.prefab", (Object)(object)this);
						}
						else
						{
							Transform smokeParent = GetSmokeParent();
							smokeEffectObject = new GameObject("CoinsSmokeVat");
							smokeEffectObject.layer = (((Object)(object)smokeParent != (Object)null) ? ((Component)smokeParent).gameObject.layer : ((Component)this).gameObject.layer);
							smokeEffectObject.transform.SetParent(((Object)(object)smokeParent != (Object)null) ? smokeParent : ((Component)this).transform.parent, false);
							smokeEffectObject.transform.localScale = Vector3.one * 1.3f;
							smokeAnimation = smokeEffectObject.AddComponent<SpriteVatAnimationSequence>();
							smokeAnimation.UseExternalTick = false;
							smokeAnimation.AutoPlay = false;
							smokeAnimation.IsLooping = false;
							smokeAnimation.FrameInterval = (((Object)(object)component != (Object)null) ? component.FrameInterval : 0.033f);
							Shader val = (((Object)(object)cachedSpriteVatShader != (Object)null) ? cachedSpriteVatShader : cachedVatShader);
							if ((Object)(object)val != (Object)null)
							{
								smokeAnimation.SetVatShader(val);
							}
							smokeAnimation.SetSortingLayerAndOrder(cachedSortingLayerName, GetSmokeSortingOrder());
							smokeAnimation.SpriteFrames = array;
							SpriteVatAnimationSequence spriteVatAnimationSequence = smokeAnimation;
							spriteVatAnimationSequence.OnAnimationComplete = (Action)Delegate.Combine(spriteVatAnimationSequence.OnAnimationComplete, new Action(HideSmokeEffect));
							PlaySmokeAnimation();
						}
					}
				});
			}
		}
	}

	private Transform GetSmokeParent()
	{
		if ((Object)(object)mainScene == (Object)null && (Object)(object)UIManager.Instance != (Object)null)
		{
			mainScene = UIManager.Instance.MainSceneVat;
		}
		Transform val = (((Object)(object)mainScene != (Object)null) ? mainScene.GetCoinsSmokePool() : null);
		if (!((Object)(object)val != (Object)null))
		{
			return ((Component)this).transform.parent;
		}
		return val;
	}

	private int GetSmokeSortingOrder()
	{
		if (!((Object)(object)mainScene != (Object)null))
		{
			return cachedSortingOrder + 1;
		}
		return mainScene.EffectSortingOrder;
	}

	private void PlaySmokeAnimation()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)smokeAnimation == (Object)null) && !((Object)(object)smokeEffectObject == (Object)null))
		{
			smokeEffectObject.SetActive(true);
			smokeEffectObject.transform.position = ((Component)this).transform.position;
			smokeAnimation.IsLooping = false;
			smokeAnimation.Restart();
		}
	}

	private void HideSmokeEffect()
	{
		if ((Object)(object)smokeEffectObject != (Object)null)
		{
			smokeEffectObject.SetActive(false);
		}
	}

	private void ResetToIdle()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		state = CoinState.Idle;
		((Component)this).transform.rotation = Quaternion.identity;
		((Component)this).transform.localScale = originalScale;
		roleTypeValue = 0;
		dicePoint = 0;
		suppressLandingEffects = false;
		mainScene?.InvalidateCoinSpatialIndex();
	}

	private void TickEvolutionEffects(float dt)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		if (isEvolutionMerging)
		{
			evolutionMergeElapsed += dt;
			float num = Mathf.Clamp01(evolutionMergeElapsed / evolutionMergeDuration);
			float num2 = num * num * (3f - 2f * num);
			((Component)this).transform.position = Vector2.op_Implicit(Vector2.Lerp(evolutionMergeStart, evolutionMergeTarget, num2));
			if (num >= 1f)
			{
				isEvolutionMerging = false;
				((Component)this).transform.position = Vector2.op_Implicit(evolutionMergeTarget);
			}
		}
		if (isEvolutionShaking)
		{
			evolutionShakeElapsed += dt;
			float num3 = Mathf.Sin(evolutionShakeElapsed * 52f) * 0.12f;
			float num4 = Mathf.Cos(evolutionShakeElapsed * 52f * 1.3f) * 0.066f;
			((Component)this).transform.position = new Vector3(evolutionShakeBasePos.x + num3, evolutionShakeBasePos.y + num4, 10f);
			((Component)this).transform.localScale = originalScale * 3f;
		}
		if (isEvolutionScaleDown)
		{
			evolutionScaleDownElapsed += dt;
			float num5 = Mathf.Clamp01(evolutionScaleDownElapsed / 0.5f);
			float num6 = 1f - (1f - num5) * (1f - num5);
			((Component)this).transform.localScale = Vector3.Lerp(originalScale * 3f, originalScale, num6);
			if (num5 >= 1f)
			{
				isEvolutionScaleDown = false;
				((Component)this).transform.localScale = originalScale;
			}
		}
	}

	private float ScaleDistance(float value)
	{
		return value * worldDistanceScale;
	}

	private float GetSafeWorldPadding(float uiPadding)
	{
		float num = Mathf.Max(0f, ScaleDistance(uiPadding));
		float num2 = Mathf.Abs(movementBoundaryMax.x - movementBoundaryMin.x);
		float num3 = Mathf.Abs(movementBoundaryMax.y - movementBoundaryMin.y);
		float num4 = Mathf.Min(num2, num3) * 0.49f;
		return Mathf.Min(num, Mathf.Max(0f, num4));
	}
}
