using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Module.UI;
using Module.Vat;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using YooAsset;

namespace Module.Role;

public class RoleVat : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	private sealed class PooledEffectMarker : MonoBehaviour
	{
		public bool IsConfigured;
	}

	private enum BehaviorStep
	{
		IdleWait,
		NoCoinWait,
		Walking,
		Throwing,
		AfterThrowWait,
		SkillCDWait,
		Sleeping
	}

	private const string IdleFrontAnimName = "idle_front";

	private const string IdleBackAnimName = "idle_back";

	private const string WalkFrontAnimName = "walk_front";

	private const string WalkBackAnimName = "walk_back";

	private const string ThrowFrontAnimName = "throw_front";

	private const string ThrowBackAnimName = "throw_back";

	private const string SkillFrontAnimName = "skill_front";

	private const string SkillBackAnimName = "skill_back";

	private const string SleepAnimName = "sleep";

	private const string CreateParticleVatAssetPath = "Assets/GameRes/VAT/zgygy_fight_calling_SkeletonData_VAT.asset";

	private const string CreateParticleVatAnimName = "animation";

	[Header("VAT")]
	[SerializeField]
	private SpineVat spineVat;

	[Header("Movement")]
	[SerializeField]
	private float baseMoveSpeed = 200f;

	[SerializeField]
	private float reachThreshold = 10f;

	[SerializeField]
	private float coinMinDistance = 60f;

	[SerializeField]
	private float waitAfterThrow = 0.5f;

	[SerializeField]
	private float scatterRadius = 50f;

	[Header("Sleep")]
	[SerializeField]
	private float sleepCheckInterval = 5f;

	[SerializeField]
	[Range(0f, 1f)]
	private float sleepProbability = 0.3f;

	[SerializeField]
	private float sleepDuration = 6f;

	private float sourceSleepDuration;

	private float sourceSleepProbability;

	private MainSceneVat mainScene;

	private RoleState state;

	private BehaviorStep step;

	private Vector2 boundaryMin;

	private Vector2 boundaryMax;

	private Vector3 targetPosition;

	private IRoleThrowableVat targetThrowable;

	private IRoleThrowableWorld throwableWorld;

	private float stepTimer;

	private float sleepTimer;

	private float sleepElapsed;

	private float skillCdDuration;

	private int roleTypeValue;

	private int awakeSkillLevel;

	private bool isInitialized;

	private bool isFacingFront = true;

	private string currentAnimName = string.Empty;

	private bool currentAnimLoop;

	private AssetHandle vatAssetHandle;

	private Coroutine loadCoroutine;

	private AssetHandle createParticleVatHandle;

	private Coroutine createParticleVatCoroutine;

	private float worldDistanceScale = 1f;

	private GameObject huanxing;

	private bool sleepTipLoadRequested;

	private static float lastGlobalIronSkillTime = -60f;

	private GameObject tornadoPrefab;

	private readonly List<GameObject> tornadoPool = new List<GameObject>();

	private GameObject magicPrefab;

	private readonly List<UISpriteAnimationSequence> magicAnimPool = new List<UISpriteAnimationSequence>();

	private readonly List<SpineVat> gongfuSkillVatPool = new List<SpineVat>();

	private GameObject lanqiuPrefab;

	private readonly List<GameObject> lanqiuPool = new List<GameObject>();

	private GameObject dashePrefab;

	private readonly List<DaSheHand> dashePool = new List<DaSheHand>();

	private GameObject dajiPrefab;

	private readonly List<GameObject> dajiPool = new List<GameObject>();

	private GameObject ironBulletPrefab;

	private readonly List<GameObject> ironBulletPool = new List<GameObject>();

	private GameObject ironBoomPrefab;

	private readonly List<UISpriteAnimationSequence> ironBoomPool = new List<UISpriteAnimationSequence>();

	private GameObject ironBullet2Prefab;

	private readonly List<GameObject> ironBullet2Pool = new List<GameObject>();

	private GameObject ironBoom2Prefab;

	private readonly List<UISpriteAnimationSequence> ironBoom2Pool = new List<UISpriteAnimationSequence>();

	private readonly List<SpineVat> halandeSkillVatPool = new List<SpineVat>();

	private readonly List<SpineVat> hammerWakeupVatPool = new List<SpineVat>();

	private readonly List<SpineVat> dushenSkillVatPool = new List<SpineVat>();

	private GameObject zuqiuPrefab;

	private readonly List<GameObject> zuqiuPool = new List<GameObject>();

	private GameObject diceMovePrefab;

	private readonly List<GameObject> diceMovePool = new List<GameObject>();

	private readonly List<IRoleThrowableVat> randomCoinCandidates = new List<IRoleThrowableVat>();

	private readonly List<IRoleThrowableVat> randomThrowableCandidates = new List<IRoleThrowableVat>();

	public int roleType => roleTypeValue;

	public int AwakeSkillLevel => awakeSkillLevel;

	public RoleState State => state;

	public bool IsVatReady
	{
		get
		{
			if ((Object)(object)spineVat != (Object)null)
			{
				return spineVat.IsReady;
			}
			return false;
		}
	}

	public SpineVat SpineVatComponent => spineVat;

	public bool UseExternalTick { get; set; }

	private float CurrentMoveSpeed
	{
		get
		{
			float num = 1f;
			if ((Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null)
			{
				num = SingletonMono<UpgradeManager>.Instance.GetRoleSpeedMultiplier();
			}
			return ScaleDistance(baseMoveSpeed) * num;
		}
	}

	private bool UseSceneSharedEffectPool
	{
		get
		{
			if ((Object)(object)mainScene != (Object)null)
			{
				return mainScene.UseCentralTick;
			}
			return false;
		}
	}

	private void Awake()
	{
		sourceSleepProbability = sleepProbability;
		sourceSleepDuration = sleepDuration;
		EnsureVat();
	}

	private void Update()
	{
		if (!UseExternalTick)
		{
			TickExternal(Time.deltaTime);
		}
	}

	public void TickExternal(float deltaTime)
	{
		if (isInitialized)
		{
			TickSleepTimer(deltaTime);
			TickBehavior(deltaTime);
			if ((Object)(object)spineVat != (Object)null)
			{
				spineVat.TickExternal(deltaTime);
			}
		}
	}

	private void OnDestroy()
	{
		if (sleepTipLoadRequested)
		{
			RoleManager instance = SingletonMono<RoleManager>.Instance;
			if ((Object)(object)instance != (Object)null)
			{
				instance.SetSleepTipActive(active: false);
			}
			sleepTipLoadRequested = false;
		}
		if (loadCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(loadCoroutine);
			loadCoroutine = null;
		}
		if (vatAssetHandle != null)
		{
			((HandleBase)vatAssetHandle).Release();
			vatAssetHandle = null;
		}
		if (createParticleVatCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(createParticleVatCoroutine);
			createParticleVatCoroutine = null;
		}
		if (createParticleVatHandle != null)
		{
			((HandleBase)createParticleVatHandle).Release();
			createParticleVatHandle = null;
		}
		if ((Object)(object)huanxing != (Object)null)
		{
			RoleManager instance2 = SingletonMono<RoleManager>.Instance;
			if ((Object)(object)instance2 != (Object)null && huanxing.activeSelf)
			{
				instance2.SetSleepTipActive(active: false);
			}
			sleepTipLoadRequested = false;
			Object.Destroy((Object)(object)huanxing);
			huanxing = null;
		}
		DestroySpineVatPool(gongfuSkillVatPool);
		DestroySpineVatPool(halandeSkillVatPool);
		DestroySpineVatPool(hammerWakeupVatPool);
		DestroySpineVatPool(dushenSkillVatPool);
		DestroyGameObjectPool(tornadoPool);
		DestroyAnimationPool<UISpriteAnimationSequence>(magicAnimPool);
		DestroyGameObjectPool(lanqiuPool);
		DestroyAnimationPool<DaSheHand>(dashePool);
		DestroyGameObjectPool(dajiPool);
		DestroyGameObjectPool(ironBulletPool);
		DestroyAnimationPool<UISpriteAnimationSequence>(ironBoomPool);
		DestroyGameObjectPool(ironBullet2Pool);
		DestroyAnimationPool<UISpriteAnimationSequence>(ironBoom2Pool);
		DestroyGameObjectPool(zuqiuPool);
		DestroyGameObjectPool(diceMovePool);
	}

	public void Initialize(int helperId, Vector2 min, Vector2 max, MainSceneVat scene, float distanceScale = 1f, SpineVatAsset preloadedVatAsset = null)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		InitializeInternal(helperId, min, max, scene, null, distanceScale, preloadedVatAsset);
	}

	public void InitializeForWorld(int helperId, Vector2 min, Vector2 max, IRoleThrowableWorld world, float distanceScale = 1f, SpineVatAsset preloadedVatAsset = null)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		InitializeInternal(helperId, min, max, null, world, distanceScale, preloadedVatAsset);
	}

	public void InitializeForWorld(int helperId, Vector2 min, Vector2 max, MainSceneVat scene, IRoleThrowableWorld world, float distanceScale = 1f, SpineVatAsset preloadedVatAsset = null)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		InitializeInternal(helperId, min, max, scene, world, distanceScale, preloadedVatAsset);
	}

	private void InitializeInternal(int helperId, Vector2 min, Vector2 max, MainSceneVat scene, IRoleThrowableWorld world, float distanceScale, SpineVatAsset preloadedVatAsset)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		mainScene = scene;
		throwableWorld = world;
		roleTypeValue = helperId;
		boundaryMin = min;
		boundaryMax = max;
		worldDistanceScale = Mathf.Max(0.0001f, distanceScale);
		isFacingFront = Random.value > 0.5f;
		if (roleTypeValue != 0)
		{
			awakeSkillLevel = GameDataManager.Instance.GetHelperAwakeSkillLevel(roleTypeValue);
		}
		else
		{
			awakeSkillLevel = 0;
		}
		RefreshSleepProbability();
		MoveToRandomPosition();
		EnsureVat();
		if (loadCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(loadCoroutine);
		}
		if ((Object)(object)preloadedVatAsset != (Object)null)
		{
			if (vatAssetHandle != null)
			{
				((HandleBase)vatAssetHandle).Release();
				vatAssetHandle = null;
			}
			spineVat.Initialize(preloadedVatAsset, boundaryMin, boundaryMax, enableRandomRoam: false);
			PlayIdle();
		}
		else
		{
			loadCoroutine = ((MonoBehaviour)this).StartCoroutine(LoadVatAssetCoroutine(helperId));
		}
		isInitialized = true;
		state = RoleState.Idle;
		step = BehaviorStep.IdleWait;
		stepTimer = Random.Range(0.1f, 0.6f);
		PlayCreateParticleVat();
	}

	public void SetVatShader(Shader shader)
	{
		if (!((Object)(object)shader == (Object)null))
		{
			EnsureVat();
			spineVat.SetVatShader(shader);
		}
	}

	public void SetSortingLayerAndOrder(string layerName, int order)
	{
		EnsureVat();
		spineVat.SetSortingLayerAndOrder(layerName, order);
	}

	public void SetGpuBatchRenderingEnabled(bool enabled)
	{
		EnsureVat();
		spineVat.SetIndirectRenderingEnabled(enabled);
	}

	public void SetRenderingVisible(bool visible)
	{
		EnsureVat();
		spineVat.SetRenderingVisible(visible);
	}

	public void RefreshDebugVisuals()
	{
		SetRenderingVisible(VatDebugSettings.ShowRoles);
		ApplySkillPoolVisuals();
	}

	public void RefreshSleepProbability()
	{
		if (TalentDataManager.Instance == null)
		{
			sleepProbability = sourceSleepProbability;
			return;
		}
		int num = GameDataManager.Instance?.GetCurrentWorldId() ?? 1;
		if (TalentDataManager.Instance != null)
		{
			switch (num)
			{
			case 1:
			{
				int talentLevel2 = TalentDataManager.Instance.GetTalentLevel(201);
				sleepProbability = ((talentLevel2 > 0) ? (sourceSleepProbability * TalentDataManager.Instance.ShiMian()) : sourceSleepProbability);
				break;
			}
			case 2:
			{
				int talentLevel = TalentDataManager.Instance.GetTalentLevel(202, TalentSceneType.DiceSceneTalent);
				sleepProbability = ((talentLevel > 0) ? (sourceSleepProbability * TalentDataManager.Instance.GetDiceTalentBuff(202)) : sourceSleepProbability);
				break;
			}
			}
		}
	}

	public void ActiveSleepTip()
	{
		RoleManager roleManager = SingletonMono<RoleManager>.Instance;
		if ((Object)(object)roleManager == (Object)null || roleManager.GetSleepCount() >= 3 || roleManager.GetSleepTipActive() || sleepTipLoadRequested)
		{
			return;
		}
		if ((Object)(object)huanxing != (Object)null)
		{
			ShowSleepTip(roleManager);
		}
		else
		{
			if ((Object)(object)SingletonMono<ResManager>.Instance == (Object)null)
			{
				return;
			}
			sleepTipLoadRequested = true;
			roleManager.SetSleepTipActive(active: true);
			SingletonMono<ResManager>.Instance.LoadPrefabAsync("Assets/GameRes/Role/Role_huanxing.prefab", delegate(GameObject prefab)
			{
				//IL_0118: Unknown result type (might be due to invalid IL or missing references)
				//IL_0122: Expected O, but got Unknown
				//IL_0134: Unknown result type (might be due to invalid IL or missing references)
				//IL_013e: Expected O, but got Unknown
				if (sleepTipLoadRequested)
				{
					if ((Object)(object)this == (Object)null)
					{
						if ((Object)(object)roleManager != (Object)null)
						{
							roleManager.SetSleepTipActive(active: false);
						}
					}
					else if ((Object)(object)roleManager == (Object)null || state != RoleState.Sleeping || (Object)(object)prefab == (Object)null || roleManager.GetSleepCount() >= 3)
					{
						sleepTipLoadRequested = false;
						if ((Object)(object)roleManager != (Object)null)
						{
							roleManager.SetSleepTipActive(active: false);
						}
					}
					else
					{
						Transform val = (((Object)(object)mainScene != (Object)null) ? mainScene.GetHuanxingPool() : null);
						if ((Object)(object)val == (Object)null)
						{
							sleepTipLoadRequested = false;
							roleManager.SetSleepTipActive(active: false);
						}
						else
						{
							huanxing = Object.Instantiate<GameObject>(prefab, val);
							Button componentInChildren = huanxing.GetComponentInChildren<Button>();
							if ((Object)(object)componentInChildren != (Object)null)
							{
								((UnityEvent)componentInChildren.onClick).RemoveListener(new UnityAction(OnSleepTipClicked));
								((UnityEvent)componentInChildren.onClick).AddListener(new UnityAction(OnSleepTipClicked));
							}
							ShowSleepTip(roleManager);
						}
					}
				}
			});
		}
	}

	public void CreatHamWakeupEffect()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		Vector2 effectPosition = Vector2.op_Implicit(((Component)this).transform.position) + new Vector2(0f, ScaleDistance(80f));
		LoadSkillVatAsset("Assets/GameRes/VAT/zgygy_hammer_wakeup_SkeletonData_VAT.asset", delegate(SpineVatAsset asset)
		{
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)this != (Object)null && (Object)(object)asset != (Object)null)
			{
				PlaySkillVatEffect(hammerWakeupVatPool, "HammerWakeupPool", asset, effectPosition, 1f, "hit");
			}
		});
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (state == RoleState.Sleeping)
		{
			WakeUp(playEffect: true);
		}
	}

	private IEnumerator LoadVatAssetCoroutine(int helperId)
	{
		string assetPath = GetVatAssetPath(helperId);
		if (string.IsNullOrEmpty(assetPath))
		{
			assetPath = GetVatAssetPath(0);
			Debug.LogWarning((object)$"[RoleVat] Missing VAT asset path for helper {helperId}, fallback to default role.", (Object)(object)this);
		}
		if ((Object)(object)SingletonMono<ResManager>.Instance == (Object)null)
		{
			Debug.LogError((object)"[RoleVat] ResManager is missing, cannot load VAT asset.", (Object)(object)this);
			yield break;
		}
		vatAssetHandle = SingletonMono<ResManager>.Instance.LoadAssetAsyncHandle<Object>(assetPath);
		if (vatAssetHandle == null)
		{
			Debug.LogError((object)("[RoleVat] Failed to start VAT asset load: " + assetPath), (Object)(object)this);
			yield break;
		}
		yield return vatAssetHandle;
		if ((int)((HandleBase)vatAssetHandle).Status != 2)
		{
			Debug.LogError((object)("[RoleVat] Failed to load VAT asset: " + assetPath + ", " + ((HandleBase)vatAssetHandle).LastError), (Object)(object)this);
			yield break;
		}
		SpineVatAsset spineVatAsset = vatAssetHandle.AssetObject as SpineVatAsset;
		if ((Object)(object)spineVatAsset == (Object)null)
		{
			Debug.LogError((object)("[RoleVat] Loaded asset is not SpineVatAsset: " + assetPath), (Object)(object)this);
			yield break;
		}
		EnsureVat();
		spineVat.Initialize(spineVatAsset, boundaryMin, boundaryMax, enableRandomRoam: false);
		PlayIdle();
	}

	private void PlayCreateParticleVat()
	{
		if (VatDebugSettings.AreRoleSkillVisualsVisible(roleTypeValue))
		{
			if (createParticleVatCoroutine != null)
			{
				((MonoBehaviour)this).StopCoroutine(createParticleVatCoroutine);
			}
			createParticleVatCoroutine = ((MonoBehaviour)this).StartCoroutine(PlayCreateParticleVatCoroutine());
		}
	}

	private IEnumerator PlayCreateParticleVatCoroutine()
	{
		if ((Object)(object)SingletonMono<ResManager>.Instance == (Object)null)
		{
			createParticleVatCoroutine = null;
			yield break;
		}
		if (createParticleVatHandle == null)
		{
			createParticleVatHandle = SingletonMono<ResManager>.Instance.LoadAssetAsyncHandle<Object>("Assets/GameRes/VAT/zgygy_fight_calling_SkeletonData_VAT.asset");
			if (createParticleVatHandle == null)
			{
				Debug.LogError((object)"[RoleVat] Failed to start create particle VAT asset load: Assets/GameRes/VAT/zgygy_fight_calling_SkeletonData_VAT.asset", (Object)(object)this);
				createParticleVatCoroutine = null;
				yield break;
			}
		}
		yield return createParticleVatHandle;
		if ((Object)(object)this == (Object)null)
		{
			yield break;
		}
		if ((int)((HandleBase)createParticleVatHandle).Status != 2)
		{
			Debug.LogError((object)("[RoleVat] Failed to load create particle VAT asset: Assets/GameRes/VAT/zgygy_fight_calling_SkeletonData_VAT.asset, " + ((HandleBase)createParticleVatHandle).LastError), (Object)(object)this);
			createParticleVatCoroutine = null;
			yield break;
		}
		SpineVatAsset spineVatAsset = createParticleVatHandle.AssetObject as SpineVatAsset;
		if ((Object)(object)spineVatAsset == (Object)null)
		{
			Debug.LogError((object)"[RoleVat] Loaded create particle asset is not SpineVatAsset: Assets/GameRes/VAT/zgygy_fight_calling_SkeletonData_VAT.asset", (Object)(object)this);
			createParticleVatCoroutine = null;
			yield break;
		}
		GameObject effect = new GameObject("RoleVat_CreateParticle");
		effect.layer = ((Component)this).gameObject.layer;
		effect.transform.SetParent(((Component)this).transform, false);
		effect.transform.localPosition = Vector3.zero;
		effect.transform.localRotation = Quaternion.identity;
		effect.transform.localScale = Vector3.one;
		SpineVat obj = effect.AddComponent<SpineVat>();
		obj.UseExternalTick = false;
		obj.SetVatShader(((Object)(object)mainScene != (Object)null) ? mainScene.SpineVatShader : null);
		obj.SetSortingLayerAndOrder(((Object)(object)mainScene != (Object)null) ? mainScene.VatSortingLayerName : string.Empty, ((Object)(object)mainScene != (Object)null) ? mainScene.EffectSortingOrder : 0);
		obj.SetDepthSortRange(boundaryMin.y, boundaryMax.y);
		obj.SetDepthSortEnabled(enabled: true);
		obj.SetRendererSortingOrderByYEnabled(enabled: true);
		obj.SetIndirectRenderingEnabled(enabled: false);
		obj.SetVatAssetNoPlay(spineVatAsset);
		if (!obj.Play("animation", shouldLoop: false, 0f, delegate
		{
			if ((Object)(object)effect != (Object)null)
			{
				Object.Destroy((Object)(object)effect);
			}
		}))
		{
			Object.Destroy((Object)(object)effect);
		}
		createParticleVatCoroutine = null;
	}

	private void EnsureVat()
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
		spineVat.SetDepthSortRange(boundaryMin.y, boundaryMax.y);
		spineVat.SetDepthSortZRange(-3f, -1.1f);
	}

	private void TickBehavior(float deltaTime)
	{
		switch (step)
		{
		case BehaviorStep.IdleWait:
			state = RoleState.Idle;
			PlayIdle();
			stepTimer -= deltaTime;
			if (stepTimer <= 0f)
			{
				TryFindThrowableAndStart();
			}
			break;
		case BehaviorStep.NoCoinWait:
			state = RoleState.Idle;
			PlayIdle();
			stepTimer -= deltaTime;
			if (stepTimer <= 0f)
			{
				TryFindThrowableAndStart();
			}
			break;
		case BehaviorStep.Walking:
			TickWalking(deltaTime);
			break;
		case BehaviorStep.Throwing:
			stepTimer -= deltaTime;
			if (stepTimer <= 0f)
			{
				step = BehaviorStep.AfterThrowWait;
				stepTimer = waitAfterThrow;
			}
			break;
		case BehaviorStep.AfterThrowWait:
			state = RoleState.Idle;
			PlayIdle();
			stepTimer -= deltaTime;
			if (stepTimer <= 0f)
			{
				EnterSkillCDWait();
			}
			break;
		case BehaviorStep.SkillCDWait:
			state = RoleState.Idle;
			PlayIdle();
			stepTimer -= deltaTime;
			if (stepTimer <= 0f)
			{
				step = BehaviorStep.IdleWait;
				stepTimer = Random.Range(0.1f, 0.4f);
			}
			break;
		case BehaviorStep.Sleeping:
			TickSleeping(deltaTime);
			break;
		}
	}

	private void TickSleepTimer(float deltaTime)
	{
		if (state == RoleState.Sleeping)
		{
			return;
		}
		sleepTimer += deltaTime;
		if (!(sleepTimer < sleepCheckInterval))
		{
			sleepTimer = 0f;
			if (Random.value < sleepProbability)
			{
				EnterSleeping();
			}
		}
	}

	private void TryFindThrowableAndStart()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		IRoleThrowableVat roleThrowableVat = FindTargetThrowable();
		if (roleThrowableVat == null)
		{
			step = BehaviorStep.NoCoinWait;
			stepTimer = 0.5f;
			return;
		}
		targetThrowable = roleThrowableVat;
		targetPosition = GetStandPositionNearThrowable(roleThrowableVat);
		state = RoleState.Walking;
		step = BehaviorStep.Walking;
		PlayWalk(targetPosition.y <= ((Component)this).transform.position.y);
	}

	private void TickWalking(float deltaTime)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		if (!IsThrowableAvailable(targetThrowable))
		{
			step = BehaviorStep.IdleWait;
			stepTimer = 0.2f;
			targetThrowable = null;
			return;
		}
		targetPosition = GetStandPositionNearThrowable(targetThrowable);
		Vector3 position = ((Component)this).transform.position;
		float currentMoveSpeed = CurrentMoveSpeed;
		Vector3 val = Vector3.MoveTowards(position, targetPosition, currentMoveSpeed * deltaTime);
		((Component)this).transform.position = val;
		Vector3 val2 = targetPosition - position;
		if (Mathf.Abs(val2.x) > 0.01f)
		{
			Vector3 localScale = ((Component)this).transform.localScale;
			localScale.x = Mathf.Abs(localScale.x) * ((val2.x < 0f) ? (-1f) : 1f);
			((Component)this).transform.localScale = localScale;
		}
		PlayWalk(isFacingFront = targetPosition.y <= position.y);
		if (Vector3.Distance(val, targetPosition) <= ScaleDistance(reachThreshold))
		{
			EnterThrowing(targetThrowable);
		}
	}

	private void EnterThrowing(IRoleThrowableVat throwable)
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		if (!IsThrowableAvailable(throwable))
		{
			step = BehaviorStep.IdleWait;
			stepTimer = 0.2f;
			return;
		}
		state = RoleState.Throwing;
		step = BehaviorStep.Throwing;
		stepTimer = throwable.RoleThrowDuration + 0.5f;
		isFacingFront = throwable.VatTransform.position.y <= ((Component)this).transform.position.y;
		PlayThrow(isFacingFront);
		Vector2 val = Vector2.op_Implicit(throwable.VatTransform.position);
		Vector2 val2 = ((throwableWorld != null) ? throwableWorld.GetThrowableLandingPosition(val) : (((Object)(object)mainScene != (Object)null) ? mainScene.GetRandomLandingPosition(val) : (val + Random.insideUnitCircle * ScaleDistance(100f))));
		if (!throwable.TryThrowByRole(val, val2, roleTypeValue))
		{
			step = BehaviorStep.IdleWait;
			stepTimer = 0.2f;
			targetThrowable = null;
			return;
		}
		if (throwable != null)
		{
			PlayExtra(throwable);
		}
		CoinVat coinVat = throwable as CoinVat;
		if ((Object)(object)coinVat != (Object)null && roleTypeValue == 9 && awakeSkillLevel >= 1)
		{
			Action<CoinVat, bool> extraFlipHandler = null;
			extraFlipHandler = delegate(CoinVat flippedCoin, bool front)
			{
				//IL_0042: Unknown result type (might be due to invalid IL or missing references)
				//IL_0047: Unknown result type (might be due to invalid IL or missing references)
				//IL_004c: Unknown result type (might be due to invalid IL or missing references)
				//IL_004d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0068: Unknown result type (might be due to invalid IL or missing references)
				//IL_006d: Unknown result type (might be due to invalid IL or missing references)
				//IL_006f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0070: Unknown result type (might be due to invalid IL or missing references)
				//IL_0071: Unknown result type (might be due to invalid IL or missing references)
				//IL_0072: Unknown result type (might be due to invalid IL or missing references)
				flippedCoin.OnFlipComplete = (Action<CoinVat, bool>)Delegate.Remove(flippedCoin.OnFlipComplete, extraFlipHandler);
				if ((Object)(object)flippedCoin != (Object)null && (Object)(object)((Component)flippedCoin).gameObject != (Object)null && flippedCoin.State != CoinState.Flying)
				{
					Vector2 val3 = Vector2.op_Implicit(((Component)flippedCoin).transform.position);
					Vector2 val4 = Random.insideUnitCircle * ScaleDistance(scatterRadius);
					flippedCoin.JumpToTarget(val3, val3 + val4, 9);
				}
			};
			coinVat.OnFlipComplete = (Action<CoinVat, bool>)Delegate.Combine(coinVat.OnFlipComplete, extraFlipHandler);
		}
		targetThrowable = null;
	}

	private IRoleThrowableVat FindTargetThrowable()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if (throwableWorld != null)
		{
			return throwableWorld.FindThrowableForRole(Vector2.op_Implicit(((Component)this).transform.position), roleTypeValue);
		}
		if ((Object)(object)mainScene != (Object)null)
		{
			return FindTargetCoin();
		}
		return null;
	}

	private static bool IsThrowableAvailable(IRoleThrowableVat throwable)
	{
		if (throwable == null)
		{
			return false;
		}
		Object val = (Object)((throwable is Object) ? throwable : null);
		if (val != null && val == (Object)null)
		{
			return false;
		}
		if (throwable.IsIdleForRole)
		{
			return (Object)(object)throwable.VatTransform != (Object)null;
		}
		return false;
	}

	private IRoleThrowableVat FindTargetCoin()
	{
		if (roleTypeValue == 1 && awakeSkillLevel > 0)
		{
			return FindNearestCoin();
		}
		return FindRandomCoin();
	}

	private void EnterSkillCDWait()
	{
		step = BehaviorStep.SkillCDWait;
		CheckAwakeSkillLevelUpdate();
		if (throwableWorld != null)
		{
			skillCdDuration = throwableWorld.GetRoleThrowCooldown(roleTypeValue);
		}
		else if (roleTypeValue > 0 && ConfigManager.HasInstance())
		{
			skillCdDuration = ConfigManager.Instance.GetHelperCD(roleTypeValue);
		}
		else
		{
			skillCdDuration = 0f;
		}
		if (skillCdDuration <= 0f)
		{
			step = BehaviorStep.IdleWait;
			stepTimer = Random.Range(0.1f, 0.4f);
			return;
		}
		if ((GameDataManager.Instance?.GetCurrentWorldId() ?? 1) == 2)
		{
			skillCdDuration *= TalentDataManager.Instance?.GetDiceTalentBuff(207) ?? 1f;
		}
		stepTimer = skillCdDuration;
		state = RoleState.Idle;
		PlayIdle();
	}

	private void EnterSleeping()
	{
		CheckAwakeSkillLevelUpdate();
		state = RoleState.Sleeping;
		step = BehaviorStep.Sleeping;
		sleepElapsed = 0f;
		targetThrowable = null;
		PlayAnimation("sleep", loop: true);
		ActiveSleepTip();
		if ((GameDataManager.Instance?.GetCurrentWorldId() ?? 1) == 2)
		{
			sleepDuration = (sourceSleepDuration * TalentDataManager.Instance?.GetDiceTalentBuff(204)) ?? 1f;
		}
	}

	private void TickSleeping(float deltaTime)
	{
		sleepElapsed += deltaTime;
		if (sleepElapsed >= sleepDuration)
		{
			WakeUp(playEffect: false);
		}
	}

	private void WakeUp(bool playEffect)
	{
		state = RoleState.Idle;
		step = BehaviorStep.IdleWait;
		stepTimer = Random.Range(0.1f, 0.5f);
		sleepElapsed = 0f;
		sleepTimer = 0f;
		HideSleepTip();
		if (playEffect)
		{
			if ((Object)(object)SoundManager.Instance != (Object)null)
			{
				SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/revive.mp3");
			}
			CreatHamWakeupEffect();
		}
		PlayIdle();
	}

	private void OnSleepTipClicked()
	{
		if (state == RoleState.Sleeping)
		{
			WakeUp(playEffect: true);
		}
	}

	private void ShowSleepTip(RoleManager roleManager)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)huanxing == (Object)null) && !((Object)(object)roleManager == (Object)null))
		{
			sleepTipLoadRequested = false;
			huanxing.SetActive(true);
			Vector3 val = ((Component)this).transform.position + new Vector3(0f, ScaleDistance(110f), 0f);
			RectTransform component = huanxing.GetComponent<RectTransform>();
			if ((Object)(object)mainScene != (Object)null && (Object)(object)component != (Object)null)
			{
				mainScene.PlaceUiRectAtWorldPosition(component, val);
			}
			else
			{
				huanxing.transform.position = val;
			}
			Transform sleepTipTweenTarget = GetSleepTipTweenTarget();
			if ((Object)(object)sleepTipTweenTarget != (Object)null)
			{
				ShortcutExtensions.DOKill((Component)(object)sleepTipTweenTarget, false);
				TweenSettingsExtensions.SetLoops<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOMoveY(sleepTipTweenTarget, sleepTipTweenTarget.position.y + 15f, 0.4f, false), -1, (LoopType)1);
			}
			roleManager.SetSleepTipActive(active: true);
			roleManager.AddSleepCount();
		}
	}

	private Transform GetSleepTipTweenTarget()
	{
		if ((Object)(object)huanxing == (Object)null)
		{
			return null;
		}
		Rigidbody2D componentInChildren = huanxing.GetComponentInChildren<Rigidbody2D>();
		if (!((Object)(object)componentInChildren != (Object)null))
		{
			return huanxing.transform;
		}
		return ((Component)componentInChildren).transform;
	}

	private void HideSleepTip()
	{
		sleepTipLoadRequested = false;
		if ((Object)(object)huanxing != (Object)null)
		{
			Transform sleepTipTweenTarget = GetSleepTipTweenTarget();
			if ((Object)(object)sleepTipTweenTarget != (Object)null)
			{
				ShortcutExtensions.DOKill((Component)(object)sleepTipTweenTarget, false);
			}
			huanxing.SetActive(false);
		}
		RoleManager instance = SingletonMono<RoleManager>.Instance;
		if ((Object)(object)instance != (Object)null)
		{
			instance.SetSleepTipActive(active: false);
		}
	}

	private void CheckAwakeSkillLevelUpdate()
	{
		if (roleTypeValue != 0 && !((Object)(object)GameDataManager.Instance == (Object)null))
		{
			int helperAwakeSkillLevel = GameDataManager.Instance.GetHelperAwakeSkillLevel(roleTypeValue);
			if (helperAwakeSkillLevel != awakeSkillLevel)
			{
				awakeSkillLevel = helperAwakeSkillLevel;
				OnAwakeSkillLevelChanged();
			}
		}
	}

	private void OnAwakeSkillLevelChanged()
	{
		if (roleTypeValue == 2)
		{
			DestroyGameObjectPool(tornadoPool);
			tornadoPrefab = null;
		}
		if (awakeSkillLevel <= 0 || (roleTypeValue != 1 && roleTypeValue != 2 && roleTypeValue != 3))
		{
			return;
		}
		string awakeVatAssetPath = GetAwakeVatAssetPath(roleTypeValue);
		if (!string.IsNullOrEmpty(awakeVatAssetPath))
		{
			if (loadCoroutine != null)
			{
				((MonoBehaviour)this).StopCoroutine(loadCoroutine);
			}
			loadCoroutine = ((MonoBehaviour)this).StartCoroutine(LoadAwakeVatAssetCoroutine(awakeVatAssetPath));
		}
	}

	private IEnumerator LoadAwakeVatAssetCoroutine(string assetPath)
	{
		if ((Object)(object)SingletonMono<ResManager>.Instance == (Object)null)
		{
			Debug.LogError((object)"[RoleVat] ResManager is missing, cannot load awake VAT asset.", (Object)(object)this);
			yield break;
		}
		if (vatAssetHandle != null)
		{
			((HandleBase)vatAssetHandle).Release();
			vatAssetHandle = null;
		}
		vatAssetHandle = SingletonMono<ResManager>.Instance.LoadAssetAsyncHandle<Object>(assetPath);
		if (vatAssetHandle == null)
		{
			Debug.LogError((object)("[RoleVat] Failed to start awake VAT asset load: " + assetPath), (Object)(object)this);
			yield break;
		}
		yield return vatAssetHandle;
		if ((int)((HandleBase)vatAssetHandle).Status != 2)
		{
			Debug.LogError((object)("[RoleVat] Failed to load awake VAT asset: " + assetPath + ", " + ((HandleBase)vatAssetHandle).LastError), (Object)(object)this);
			yield break;
		}
		SpineVatAsset spineVatAsset = vatAssetHandle.AssetObject as SpineVatAsset;
		if ((Object)(object)spineVatAsset == (Object)null)
		{
			Debug.LogError((object)("[RoleVat] Loaded awake asset is not SpineVatAsset: " + assetPath), (Object)(object)this);
			yield break;
		}
		EnsureVat();
		spineVat.Initialize(spineVatAsset, boundaryMin, boundaryMax, enableRandomRoam: false);
		PlayIdle();
	}

	public static string GetAwakeVatAssetPath(int helperId)
	{
		return helperId switch
		{
			1 => "Assets/GameRes/VAT/zgygy_role_toughguy_juexing_SkeletonData_VAT.asset", 
			2 => "Assets/GameRes/VAT/zgygy_role_miracle_juexing_SkeletonData_VAT.asset", 
			3 => "Assets/GameRes/VAT/zgygy_role_magician_juexing_SkeletonData_VAT.asset", 
			_ => null, 
		};
	}

	private IRoleThrowableVat FindNearestCoin()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)mainScene == (Object)null)
		{
			return null;
		}
		float radius = Vector2.Distance(boundaryMin, boundaryMax);
		IRoleThrowableVat roleThrowableVat = mainScene.FindNearestIdleVatCoin(Vector2.op_Implicit(((Component)this).transform.position), radius);
		if (roleThrowableVat == null || (roleThrowableVat is CoinVat coinVat && !CanFlipCoin(coinVat.coinType)))
		{
			return FindNearestCoinFallback(radius);
		}
		return roleThrowableVat;
	}

	private IRoleThrowableVat FindRandomCoin()
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)mainScene == (Object)null)
		{
			return null;
		}
		float num = ScaleDistance(900f);
		float num2 = ScaleDistance(300f);
		for (float num3 = ScaleDistance(2000f); num <= num3; num += num2)
		{
			randomCoinCandidates.Clear();
			mainScene.GetIdleThrowablesInRadius(Vector2.op_Implicit(((Component)this).transform.position), num, randomCoinCandidates);
			for (int num4 = randomCoinCandidates.Count - 1; num4 >= 0; num4--)
			{
				IRoleThrowableVat roleThrowableVat = randomCoinCandidates[num4];
				if (roleThrowableVat == null || (roleThrowableVat is CoinVat coinVat && !CanFlipCoin(coinVat.coinType)))
				{
					randomCoinCandidates.RemoveAt(num4);
				}
			}
			if (randomCoinCandidates.Count > 0)
			{
				return randomCoinCandidates[Random.Range(0, randomCoinCandidates.Count)];
			}
		}
		return null;
	}

	private IRoleThrowableVat FindNearestCoinFallback(float radius)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		randomCoinCandidates.Clear();
		mainScene.GetIdleThrowablesInRadius(Vector2.op_Implicit(((Component)this).transform.position), radius, randomCoinCandidates);
		IRoleThrowableVat result = null;
		float num = float.MaxValue;
		Vector3 position = ((Component)this).transform.position;
		for (int i = 0; i < randomCoinCandidates.Count; i++)
		{
			IRoleThrowableVat roleThrowableVat = randomCoinCandidates[i];
			if (roleThrowableVat != null && (!(roleThrowableVat is CoinVat coinVat) || CanFlipCoin(coinVat.coinType)))
			{
				Vector3 val = roleThrowableVat.VatTransform.position - position;
				float sqrMagnitude = ((Vector3)(ref val)).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					result = roleThrowableVat;
				}
			}
		}
		return result;
	}

	private bool CanFlipCoin(CoinType coinType)
	{
		if ((Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null)
		{
			return SingletonMono<UpgradeManager>.Instance.CanRoleFlipCoin(coinType);
		}
		return coinType == CoinType.Copper;
	}

	private Vector3 GetStandPositionNearThrowable(IRoleThrowableVat throwable)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = throwable.VatTransform.position;
		Vector3 val = ((Component)this).transform.position - position;
		if (((Vector3)(ref val)).sqrMagnitude < 0.001f)
		{
			Vector2 insideUnitCircle = Random.insideUnitCircle;
			val = Vector2.op_Implicit(((Vector2)(ref insideUnitCircle)).normalized);
		}
		val.z = 0f;
		((Vector3)(ref val)).Normalize();
		Vector3 pos = position + val * ScaleDistance(coinMinDistance);
		return ClampToBoundary(pos);
	}

	private Vector3 ClampToBoundary(Vector3 pos)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		float safeWorldPadding = GetSafeWorldPadding(30f);
		pos.x = Mathf.Clamp(pos.x, boundaryMin.x + safeWorldPadding, boundaryMax.x - safeWorldPadding);
		pos.y = Mathf.Clamp(pos.y, boundaryMin.y + safeWorldPadding, boundaryMax.y - safeWorldPadding);
		pos.z = 0f;
		return pos;
	}

	private void MoveToRandomPosition()
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		float safeWorldPadding = GetSafeWorldPadding(30f);
		float num = Random.Range(boundaryMin.x + safeWorldPadding, boundaryMax.x - safeWorldPadding);
		float num2 = Random.Range(boundaryMin.y + safeWorldPadding, boundaryMax.y - safeWorldPadding);
		((Component)this).transform.position = new Vector3(num, num2, 0f);
	}

	private float ScaleDistance(float value)
	{
		return value * worldDistanceScale;
	}

	private float GetSafeWorldPadding(float uiPadding)
	{
		float num = Mathf.Max(0f, ScaleDistance(uiPadding));
		float num2 = Mathf.Abs(boundaryMax.x - boundaryMin.x);
		float num3 = Mathf.Abs(boundaryMax.y - boundaryMin.y);
		float num4 = Mathf.Min(num2, num3) * 0.49f;
		return Mathf.Min(num, Mathf.Max(0f, num4));
	}

	private void PlayExtra(IRoleThrowableVat throwable)
	{
		if (roleTypeValue <= 1)
		{
			return;
		}
		PlaySkillAnimation();
		switch (roleTypeValue)
		{
		case 2:
			PlayTornado(throwable);
			break;
		case 3:
			PlayMagic(throwable);
			break;
		case 4:
			PlayGongfu(throwable);
			break;
		case 5:
			PlayLanQiu(throwable);
			break;
		case 6:
			PlayDaShe(throwable);
			break;
		case 7:
			PlayDaji(throwable);
			break;
		case 8:
			if (Time.time - lastGlobalIronSkillTime >= 20f && awakeSkillLevel > 2)
			{
				lastGlobalIronSkillTime = Time.time;
				PlayGlobalIron();
			}
			else
			{
				PlayIron(throwable);
			}
			break;
		case 9:
			PlayHaaland(throwable);
			break;
		case 10:
			PlayDuShen(throwable);
			break;
		}
	}

	private void PlaySkillAnimation()
	{
		string animationName = (isFacingFront ? "skill_front" : "skill_back");
		if (HasVatAnimation(animationName))
		{
			PlayAnimation(animationName, loop: false);
		}
	}

	private bool HasVatAnimation(string animationName)
	{
		if ((Object)(object)spineVat != (Object)null && (Object)(object)spineVat.VatAsset != (Object)null)
		{
			return spineVat.VatAsset.FindAnimation(animationName) != null;
		}
		return false;
	}

	private Transform GetEffectPool(string poolName)
	{
		if (!((Object)(object)mainScene != (Object)null))
		{
			return ((Component)this).transform;
		}
		return mainScene.GetEffectPool(poolName);
	}

	private GameObject GetPreloadedPrefab(string assetPath)
	{
		if (!((Object)(object)mainScene != (Object)null))
		{
			return null;
		}
		return mainScene.GetVatPrefab(assetPath);
	}

	private void LoadSkillPrefab(string assetPath, Action<GameObject> onLoaded)
	{
		GameObject preloadedPrefab = GetPreloadedPrefab(assetPath);
		if ((Object)(object)preloadedPrefab != (Object)null)
		{
			onLoaded?.Invoke(preloadedPrefab);
		}
		else if ((Object)(object)SingletonMono<ResManager>.Instance == (Object)null)
		{
			onLoaded?.Invoke(null);
		}
		else
		{
			SingletonMono<ResManager>.Instance.LoadPrefabAsync(assetPath, onLoaded);
		}
	}

	private SpineVatAsset GetSkillVatAsset(string assetPath)
	{
		if (!((Object)(object)mainScene != (Object)null))
		{
			return null;
		}
		return mainScene.GetSkillVatAsset(assetPath);
	}

	private void LoadSkillVatAsset(string assetPath, Action<SpineVatAsset> onLoaded)
	{
		if ((Object)(object)mainScene == (Object)null)
		{
			onLoaded?.Invoke(null);
			return;
		}
		mainScene.LoadSkillVatAsset(assetPath, delegate(SpineVatAsset asset)
		{
			if ((Object)(object)this != (Object)null)
			{
				onLoaded?.Invoke(asset);
			}
		});
	}

	private void PlaySkillVatEffect(List<SpineVat> pool, string poolName, SpineVatAsset asset, Vector2 position, float uiScale, string animationName, int sortingOrderOffset = 0)
	{
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		if (!VatDebugSettings.AreRoleSkillVisualsVisible(roleTypeValue))
		{
			return;
		}
		if ((Object)(object)asset == (Object)null)
		{
			Debug.LogError((object)("[RoleVat] Missing skill VAT asset: " + poolName), (Object)(object)this);
			return;
		}
		SpineVat vat = GetFromSkillVatPool(pool, poolName, asset);
		if (!((Object)(object)vat == (Object)null))
		{
			if ((Object)(object)vat.VatAsset != (Object)(object)asset)
			{
				vat.SetVatAssetNoPlay(asset);
			}
			vat.SetDepthSortRange(boundaryMin.y, boundaryMax.y);
			vat.SetDepthSortEnabled(enabled: true);
			vat.SetRendererSortingOrderByYEnabled(enabled: true);
			vat.SetSortingOrder(sortingOrderOffset);
			vat.SetRenderingVisible(visible: true);
			((Component)vat).gameObject.SetActive(true);
			((Component)vat).transform.position = new Vector3(position.x, position.y, 0f);
			((Component)vat).transform.localScale = Vector3.one * uiScale;
			if (!vat.Play(animationName, shouldLoop: false, 0f, delegate
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

	private SpineVat GetFromSkillVatPool(List<SpineVat> pool, string poolName, SpineVatAsset asset)
	{
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		if (pool == null)
		{
			return null;
		}
		Transform effectPool = GetEffectPool(poolName);
		pool.RemoveAll((SpineVat v) => (Object)(object)v == (Object)null || (Object)(object)((Component)v).gameObject == (Object)null);
		for (int num = 0; num < pool.Count; num++)
		{
			SpineVat spineVat = pool[num];
			if (!((Component)spineVat).gameObject.activeSelf)
			{
				if ((Object)(object)effectPool != (Object)null && (Object)(object)((Component)spineVat).transform.parent != (Object)(object)effectPool)
				{
					((Component)spineVat).transform.SetParent(effectPool, false);
				}
				return spineVat;
			}
		}
		GameObject val = new GameObject(poolName + "_VatEffect")
		{
			layer = (((Object)(object)effectPool != (Object)null) ? ((Component)effectPool).gameObject.layer : ((Component)this).gameObject.layer)
		};
		val.transform.SetParent(((Object)(object)effectPool != (Object)null) ? effectPool : ((Component)this).transform, false);
		SpineVat spineVat2 = val.AddComponent<SpineVat>();
		spineVat2.UseExternalTick = false;
		spineVat2.SetVatShader(((Object)(object)mainScene != (Object)null) ? mainScene.SpineVatShader : null);
		spineVat2.SetSortingLayerAndOrder(((Object)(object)mainScene != (Object)null) ? mainScene.VatSortingLayerName : string.Empty, ((Object)(object)mainScene != (Object)null) ? mainScene.EffectSortingOrder : 0);
		spineVat2.SetDepthSortRange(boundaryMin.y, boundaryMax.y);
		spineVat2.SetDepthSortEnabled(enabled: true);
		spineVat2.SetRendererSortingOrderByYEnabled(enabled: true);
		spineVat2.SetIndirectRenderingEnabled(enabled: false);
		spineVat2.SetVatAssetNoPlay(asset);
		val.SetActive(false);
		pool.Add(spineVat2);
		return spineVat2;
	}

	private List<CoinVat> GetCoinList()
	{
		if (!((Object)(object)mainScene != (Object)null))
		{
			return null;
		}
		return mainScene.GetAllVatCoins();
	}

	private List<CoinVat> GetCoinsForPrioritySkill()
	{
		if (!((Object)(object)mainScene != (Object)null))
		{
			return null;
		}
		return mainScene.GetCoinsSortedByValue();
	}

	private float EffectScale(float uiScale)
	{
		return uiScale * worldDistanceScale;
	}

	private float GetHelperExtraBuff(int helperId, float fallback = 1f)
	{
		if (!((Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null))
		{
			return fallback;
		}
		return SingletonMono<UpgradeManager>.Instance.GetHelperExtraBuff(helperId);
	}

	private float GetHelperFactor(int helperId, float fallback = 1f)
	{
		if (!ConfigManager.HasInstance())
		{
			return fallback;
		}
		return ConfigManager.Instance.GetHelperFactor(helperId);
	}

	private void CollectCoinsInRadius(Vector2 center, float radius, bool prioritizeByValue)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		randomCoinCandidates.Clear();
		if ((Object)(object)mainScene == (Object)null)
		{
			return;
		}
		if (!prioritizeByValue)
		{
			mainScene.GetIdleThrowablesInRadius(center, radius, randomCoinCandidates);
			return;
		}
		List<CoinVat> coinsForPrioritySkill = GetCoinsForPrioritySkill();
		if (coinsForPrioritySkill == null)
		{
			return;
		}
		float num = radius * radius;
		for (int i = 0; i < coinsForPrioritySkill.Count; i++)
		{
			CoinVat coinVat = coinsForPrioritySkill[i];
			if (!((Object)(object)coinVat == (Object)null) && coinVat.State == CoinState.Idle)
			{
				Vector2 val = Vector2.op_Implicit(coinVat.VatTransform.position) - center;
				if (((Vector2)(ref val)).sqrMagnitude <= num)
				{
					randomCoinCandidates.Add(coinVat);
				}
			}
		}
	}

	private Vector2 GetRandomSkillLandingPosition(Vector2 startPos)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)mainScene != (Object)null)
		{
			return mainScene.GetRandomLandingPosition(startPos);
		}
		return startPos + Random.insideUnitCircle * ScaleDistance(scatterRadius);
	}

	private void TossCoin(IRoleThrowableVat coin, GameObject prefab)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		if (coin == null)
		{
			return;
		}
		Vector2 val = Vector2.op_Implicit(coin.VatTransform.position);
		Vector2 val2 = val + Random.insideUnitCircle * ScaleDistance(scatterRadius);
		coin.TryThrowByRole(val, val2, roleTypeValue);
		if (!VatDebugSettings.AreRoleSkillVisualsVisible(roleTypeValue))
		{
			return;
		}
		Transform effectPool = GetEffectPool("MagicianPool");
		if (!((Object)(object)effectPool == (Object)null) && !((Object)(object)prefab == (Object)null))
		{
			UISpriteAnimationSequence fromMagicPool = GetFromMagicPool(prefab, effectPool);
			if ((Object)(object)fromMagicPool != (Object)null)
			{
				((Component)fromMagicPool).gameObject.SetActive(true);
				((Component)fromMagicPool).transform.position = coin.VatTransform.position;
				((Component)fromMagicPool).transform.localScale = Vector3.one * EffectScale(2f);
				fromMagicPool.Restart();
				fromMagicPool.Play();
			}
		}
	}

	private void TossCoinSimple(IRoleThrowableVat coin)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		if (coin == null)
		{
			return;
		}
		Vector2 val = Vector2.op_Implicit(coin.VatTransform.position);
		Vector2 val2 = val + Random.insideUnitCircle * ScaleDistance(scatterRadius);
		if (roleTypeValue == 6)
		{
			coin.TryThrowByRole(val, val, roleTypeValue);
			if (coin is CoinVat coinVat)
			{
				coinVat.SetThrowDelayTimer(0f);
			}
		}
		else
		{
			coin.TryThrowByRole(val, val2, roleTypeValue);
			if (roleTypeValue == 4 && coin is CoinVat coinVat2)
			{
				coinVat2.SetThrowDelayTimer(0f);
			}
		}
	}

	private void PlayTornado(IRoleThrowableVat throwable)
	{
		int tornadoCount = Mathf.Max(1, Mathf.RoundToInt(GetHelperExtraBuff(2)));
		float distance = ScaleDistance(500f * GetHelperFactor(2));
		float speed = ScaleDistance((awakeSkillLevel > 0) ? 15f : 10f);
		if (awakeSkillLevel > 0)
		{
			distance *= 1.5f;
		}
		Action<GameObject> playAction = delegate(GameObject prefab)
		{
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_00db: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_016c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0174: Unknown result type (might be due to invalid IL or missing references)
			Transform effectPool = GetEffectPool("TornadoPool");
			if (!((Object)(object)effectPool == (Object)null) && !((Object)(object)prefab == (Object)null))
			{
				Vector2 val;
				Vector2 normalized;
				if (throwable == null)
				{
					val = Random.insideUnitCircle;
					normalized = ((Vector2)(ref val)).normalized;
				}
				else
				{
					val = Vector2.op_Implicit(throwable.VatTransform.position) - Vector2.op_Implicit(((Component)this).transform.position);
					normalized = ((Vector2)(ref val)).normalized;
				}
				Vector2 val2 = normalized;
				if (val2 == Vector2.zero)
				{
					val2 = Vector2.right;
				}
				float num = 15f;
				float num2 = (float)(-(tornadoCount - 1)) * num / 2f;
				for (int i = 0; i < tornadoCount; i++)
				{
					GameObject fromTornadoPool = GetFromTornadoPool(prefab, effectPool);
					fromTornadoPool.transform.position = ((Component)this).transform.position;
					fromTornadoPool.transform.localScale = Vector3.one * EffectScale(2f);
					fromTornadoPool.SetActive(true);
					ApplyEffectVisuals(fromTornadoPool);
					UISpriteAnimationSequence component = fromTornadoPool.GetComponent<UISpriteAnimationSequence>();
					if ((Object)(object)component != (Object)null)
					{
						component.IsLooping = true;
						component.Play();
					}
					TornadoAnimation tornadoAnimation = fromTornadoPool.GetComponent<TornadoAnimation>();
					if ((Object)(object)tornadoAnimation == (Object)null)
					{
						tornadoAnimation = fromTornadoPool.AddComponent<TornadoAnimation>();
					}
					tornadoAnimation.SetVatScene(mainScene, worldDistanceScale);
					tornadoAnimation.SetMovement(RotateDirection(val2, num2 + (float)i * num), speed, distance);
				}
			}
		};
		if ((Object)(object)tornadoPrefab != (Object)null)
		{
			playAction(tornadoPrefab);
			return;
		}
		string assetPath = ((awakeSkillLevel > 0) ? "Assets/GameRes/Role/coins_tornado2.prefab" : "Assets/GameRes/Role/coins_tornado.prefab");
		LoadSkillPrefab(assetPath, delegate(GameObject prefab)
		{
			tornadoPrefab = prefab;
			playAction(prefab);
		});
	}

	private void PlayMagic(IRoleThrowableVat throwable)
	{
		float radius = ScaleDistance(GetHelperFactor(3) * 200f);
		int maxTossCount = Mathf.Max(1, Mathf.RoundToInt(GetHelperExtraBuff(3, 3f)));
		Action<GameObject> playAction = delegate(GameObject prefab)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			Vector2 center = Vector2.op_Implicit(((Component)this).transform.position);
			CollectCoinsInRadius(center, radius, awakeSkillLevel > 0);
			for (int i = 0; i < randomCoinCandidates.Count; i++)
			{
				if (maxTossCount <= 0)
				{
					break;
				}
				IRoleThrowableVat roleThrowableVat = randomCoinCandidates[i];
				if (roleThrowableVat != null && roleThrowableVat.IsIdleForRole && (!(roleThrowableVat is CoinVat coinVat) || CanFlipCoin(coinVat.coinType)))
				{
					TossCoin(roleThrowableVat, prefab);
					maxTossCount--;
				}
			}
		};
		if ((Object)(object)magicPrefab != (Object)null)
		{
			playAction(magicPrefab);
			return;
		}
		LoadSkillPrefab("Assets/GameRes/Role/magic_ani.prefab", delegate(GameObject prefab)
		{
			magicPrefab = prefab;
			playAction(prefab);
		});
	}

	private void PlayGongfu(IRoleThrowableVat throwable)
	{
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		if (throwable == null)
		{
			return;
		}
		float animationScale = 1.5f;
		if (awakeSkillLevel > 0)
		{
			animationScale = ((awakeSkillLevel > 1) ? 2.5f : 2f);
		}
		float radius = ScaleDistance(GetHelperExtraBuff(4) * 120f * animationScale);
		int maxTossCount = 10;
		Vector2 center = Vector2.op_Implicit(throwable.VatTransform.position);
		LoadSkillVatAsset("Assets/GameRes/VAT/zgygy_roleskill_kungfu_SkeletonData_VAT.asset", delegate(SpineVatAsset asset)
		{
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)asset != (Object)null)
			{
				PlaySkillVatEffect(gongfuSkillVatPool, "GongfuPool", asset, center, animationScale, "skill", -10);
			}
		});
		TossCoinsInRadius(center, radius, maxTossCount);
	}

	private void PlayLanQiu(IRoleThrowableVat throwable)
	{
		int ballCount = Mathf.Max(1, Mathf.RoundToInt(GetHelperExtraBuff(5)));
		Action<GameObject> playAction = delegate(GameObject prefab)
		{
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_013e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0143: Unknown result type (might be due to invalid IL or missing references)
			//IL_0148: Unknown result type (might be due to invalid IL or missing references)
			//IL_0150: Unknown result type (might be due to invalid IL or missing references)
			//IL_0172: Unknown result type (might be due to invalid IL or missing references)
			//IL_017d: Unknown result type (might be due to invalid IL or missing references)
			Transform effectPool = GetEffectPool("LanqiuPool");
			if (!((Object)(object)effectPool == (Object)null) && !((Object)(object)prefab == (Object)null))
			{
				Vector2 directionToCoinOrRandom = GetDirectionToCoinOrRandom(throwable);
				float num = 30f;
				float num2 = (float)(-(ballCount - 1)) * num / 2f;
				for (int i = 0; i < ballCount; i++)
				{
					GameObject fromLanqiuPool = GetFromLanqiuPool(prefab, effectPool);
					fromLanqiuPool.transform.position = ((Component)this).transform.position;
					fromLanqiuPool.transform.localScale = Vector3.one * EffectScale(1f);
					fromLanqiuPool.SetActive(true);
					ApplyEffectVisuals(fromLanqiuPool);
					UISpriteAnimationSequence component = fromLanqiuPool.GetComponent<UISpriteAnimationSequence>();
					if ((Object)(object)component != (Object)null)
					{
						component.IsLooping = true;
						component.Play();
					}
					LanqiuAnimation lanqiuAnimation = fromLanqiuPool.GetComponent<LanqiuAnimation>();
					if ((Object)(object)lanqiuAnimation == (Object)null)
					{
						lanqiuAnimation = fromLanqiuPool.AddComponent<LanqiuAnimation>();
					}
					int maxBounceCount = 0;
					if (awakeSkillLevel > 0)
					{
						maxBounceCount = ((awakeSkillLevel > 2) ? 3 : ((awakeSkillLevel <= 1) ? 1 : 2));
					}
					lanqiuAnimation.InitVat(Vector2.op_Implicit(((Component)this).transform.position), RotateDirection(directionToCoinOrRandom, num2 + (float)i * num), ScaleDistance(1000f), maxBounceCount, 999, boundaryMin, boundaryMax, TossCoinSimple, mainScene, worldDistanceScale);
				}
			}
		};
		if ((Object)(object)lanqiuPrefab != (Object)null)
		{
			playAction(lanqiuPrefab);
			return;
		}
		LoadSkillPrefab("Assets/GameRes/Role/lanqiu.prefab", delegate(GameObject prefab)
		{
			lanqiuPrefab = prefab;
			playAction(prefab);
		});
	}

	private void PlayDaShe(IRoleThrowableVat throwable)
	{
		float radius = Mathf.Max(ScaleDistance(300f), ScaleDistance(GetHelperFactor(6) * 100f));
		int maxTossCount = Mathf.Max(1, Mathf.RoundToInt(GetHelperExtraBuff(6)));
		Action<GameObject> playAction = delegate(GameObject prefab)
		{
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_00df: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_016d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0172: Unknown result type (might be due to invalid IL or missing references)
			//IL_0187: Unknown result type (might be due to invalid IL or missing references)
			//IL_018c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0191: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
			Transform effectPool = GetEffectPool("DaShePool");
			if (!((Object)(object)effectPool == (Object)null) && !((Object)(object)prefab == (Object)null))
			{
				Vector2 val = Vector2.op_Implicit(((Component)this).transform.position);
				float num = ScaleDistance(100f);
				CollectCoinsInRadius(val, radius, awakeSkillLevel > 2);
				for (int i = 0; i < randomCoinCandidates.Count; i++)
				{
					if (maxTossCount <= 0)
					{
						break;
					}
					IRoleThrowableVat coin = randomCoinCandidates[i];
					if (coin != null && coin.IsIdleForRole && (!(coin is CoinVat coinVat) || CanFlipCoin(coinVat.coinType)))
					{
						float num2 = Vector2.Distance(val, Vector2.op_Implicit(coin.VatTransform.position));
						if (num2 > num && num2 <= radius && maxTossCount > 0)
						{
							DaSheHand hand = GetFromDaShePool(prefab, effectPool);
							((Component)hand).gameObject.SetActive(true);
							ApplyEffectVisuals(((Component)hand).gameObject);
							hand.InitVat(Vector2.op_Implicit(((Component)this).transform.position + Vector3.up * ScaleDistance(100f)), Vector2.op_Implicit(coin.VatTransform.position), worldDistanceScale, delegate
							{
								TossCoinSimple(coin);
								((Component)hand).gameObject.SetActive(false);
							});
							maxTossCount--;
						}
					}
				}
			}
		};
		if ((Object)(object)dashePrefab != (Object)null)
		{
			playAction(dashePrefab);
			return;
		}
		LoadSkillPrefab("Assets/GameRes/Role/dashehandani.prefab", delegate(GameObject prefab)
		{
			dashePrefab = prefab;
			playAction(prefab);
		});
	}

	private void PlayDaji(IRoleThrowableVat throwable)
	{
		int dajiCount = Mathf.Max(1, Mathf.RoundToInt(GetHelperExtraBuff(7)));
		float distance = ScaleDistance(500f * GetHelperFactor(7));
		if (distance <= 0f)
		{
			distance = ScaleDistance(500f);
		}
		bool shouldReturn = awakeSkillLevel > 0;
		float suckRadius = ((awakeSkillLevel > 1) ? ScaleDistance(150f) : 0f);
		Action<GameObject> playAction = delegate(GameObject prefab)
		{
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0108: Unknown result type (might be due to invalid IL or missing references)
			//IL_010d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0112: Unknown result type (might be due to invalid IL or missing references)
			//IL_011a: Unknown result type (might be due to invalid IL or missing references)
			Transform effectPool = GetEffectPool("DajiPool");
			if (!((Object)(object)effectPool == (Object)null) && !((Object)(object)prefab == (Object)null))
			{
				Vector2 directionToCoinOrRandom = GetDirectionToCoinOrRandom(throwable);
				float num = 15f;
				float num2 = (float)(-(dajiCount - 1)) * num / 2f;
				for (int i = 0; i < dajiCount; i++)
				{
					GameObject fromDajiPool = GetFromDajiPool(prefab, effectPool);
					fromDajiPool.transform.position = ((Component)this).transform.position;
					fromDajiPool.transform.localScale = Vector3.one * EffectScale(1.5f);
					fromDajiPool.SetActive(true);
					ApplyEffectVisuals(fromDajiPool);
					UISpriteAnimationSequence component = fromDajiPool.GetComponent<UISpriteAnimationSequence>();
					if ((Object)(object)component != (Object)null)
					{
						component.IsLooping = true;
						component.Play();
					}
					DajiAnimation dajiAnimation = fromDajiPool.GetComponent<DajiAnimation>();
					if ((Object)(object)dajiAnimation == (Object)null)
					{
						dajiAnimation = fromDajiPool.AddComponent<DajiAnimation>();
					}
					dajiAnimation.InitVat(Vector2.op_Implicit(((Component)this).transform.position), RotateDirection(directionToCoinOrRandom, num2 + (float)i * num), ScaleDistance(800f), distance, shouldReturn, 0.5f, suckRadius, mainScene, worldDistanceScale);
				}
			}
		};
		if ((Object)(object)dajiPrefab != (Object)null)
		{
			playAction(dajiPrefab);
			return;
		}
		LoadSkillPrefab("Assets/GameRes/Role/aixinqiu.prefab", delegate(GameObject prefab)
		{
			dajiPrefab = prefab;
			playAction(prefab);
		});
	}

	private void PlayIron(IRoleThrowableVat throwable)
	{
		int bulletCount = Mathf.Max(1, Mathf.RoundToInt(GetHelperExtraBuff(8)));
		Action<GameObject> playAction = delegate(GameObject prefab)
		{
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00da: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_0174: Unknown result type (might be due to invalid IL or missing references)
			//IL_0179: Unknown result type (might be due to invalid IL or missing references)
			//IL_018a: Unknown result type (might be due to invalid IL or missing references)
			//IL_018f: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f9: Expected O, but got Unknown
			//IL_015b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0160: Unknown result type (might be due to invalid IL or missing references)
			//IL_0165: Unknown result type (might be due to invalid IL or missing references)
			Transform effectPool = GetEffectPool("IronPool");
			if (!((Object)(object)effectPool == (Object)null) && !((Object)(object)prefab == (Object)null))
			{
				Vector2 val;
				bool flag = TryPickIronDenseTargetPosition(out val);
				for (int i = 0; i < bulletCount; i++)
				{
					GameObject bulletEffect = GetFromIronBulletPool(prefab, effectPool);
					float num = ((float)i - (float)(bulletCount - 1) / 2f) * ScaleDistance(50f);
					bulletEffect.transform.position = ((Component)this).transform.position + new Vector3(num, 0f, 0f);
					bulletEffect.transform.rotation = Quaternion.identity;
					bulletEffect.transform.localScale = Vector3.one * EffectScale(2f);
					bulletEffect.SetActive(true);
					ApplyEffectVisuals(bulletEffect);
					Vector2 targetPos;
					if (flag && randomCoinCandidates.Count > 0)
					{
						targetPos = Vector2.op_Implicit(randomCoinCandidates[Random.Range(0, randomCoinCandidates.Count)].VatTransform.position);
					}
					else
					{
						targetPos = PickRandomCoinPositionOrSceneCenter();
					}
					Vector3 val2 = bulletEffect.transform.position + Vector3.up * ScaleDistance(1500f);
					float num2 = Random.Range(0.1f, 0.3f);
					TweenCallback val3 = default(TweenCallback);
					TweenSettingsExtensions.OnComplete<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.SetDelay<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOMove(bulletEffect.transform, val2, 0.5f, false), (Ease)5), num2), (TweenCallback)delegate
					{
						//IL_004b: Unknown result type (might be due to invalid IL or missing references)
						//IL_006f: Unknown result type (might be due to invalid IL or missing references)
						//IL_0085: Unknown result type (might be due to invalid IL or missing references)
						//IL_008a: Unknown result type (might be due to invalid IL or missing references)
						//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
						//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
						//IL_00b9: Expected O, but got Unknown
						//IL_00be: Expected O, but got Unknown
						if (!((Object)(object)bulletEffect == (Object)null))
						{
							bulletEffect.transform.position = new Vector3(targetPos.x, targetPos.y + ScaleDistance(1500f), 0f);
							bulletEffect.transform.rotation = Quaternion.Euler(0f, 0f, 180f);
							TweenerCore<Vector3, Vector3, VectorOptions> obj = TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOMove(bulletEffect.transform, Vector2.op_Implicit(targetPos), 0.4f, false), (Ease)1);
							TweenCallback obj2 = val3;
							if (obj2 == null)
							{
								TweenCallback val4 = delegate
								{
									//IL_0026: Unknown result type (might be due to invalid IL or missing references)
									if ((Object)(object)bulletEffect != (Object)null)
									{
										bulletEffect.SetActive(false);
									}
									PlayIronBoom(targetPos);
								};
								TweenCallback val5 = val4;
								val3 = val4;
								obj2 = val5;
							}
							TweenSettingsExtensions.OnComplete<TweenerCore<Vector3, Vector3, VectorOptions>>(obj, obj2);
						}
					});
				}
			}
		};
		if ((Object)(object)ironBulletPrefab != (Object)null)
		{
			playAction(ironBulletPrefab);
			return;
		}
		LoadSkillPrefab("Assets/GameRes/Role/iron_bullet.prefab", delegate(GameObject prefab)
		{
			ironBulletPrefab = prefab;
			playAction(prefab);
		});
	}

	private void PlayIronBoom(Vector2 targetPos)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		Action<GameObject> playBoomAction = delegate(GameObject prefab)
		{
			//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			Transform effectPool = GetEffectPool("IronBoomPool");
			if (!((Object)(object)effectPool == (Object)null) && !((Object)(object)prefab == (Object)null))
			{
				UISpriteAnimationSequence fromIronBoomPool = GetFromIronBoomPool(prefab, effectPool);
				if ((Object)(object)fromIronBoomPool != (Object)null)
				{
					((Component)fromIronBoomPool).gameObject.SetActive(true);
					ApplyEffectVisuals(((Component)fromIronBoomPool).gameObject);
					((Component)fromIronBoomPool).transform.position = Vector2.op_Implicit(targetPos);
					((Component)fromIronBoomPool).transform.localScale = Vector3.one * EffectScale(2f);
					fromIronBoomPool.Restart();
					fromIronBoomPool.Play();
				}
				TossCoinsInRadius(targetPos, ScaleDistance(300f), 5, queueTosses: true, suppressLandingEffects: true);
			}
		};
		if ((Object)(object)ironBoomPrefab != (Object)null)
		{
			playBoomAction(ironBoomPrefab);
			return;
		}
		LoadSkillPrefab("Assets/GameRes/Role/iron_boom.prefab", delegate(GameObject prefab)
		{
			ironBoomPrefab = prefab;
			playBoomAction(prefab);
		});
	}

	private void PlayGlobalIron()
	{
		Action<GameObject> playAction = delegate(GameObject prefab)
		{
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_0118: Unknown result type (might be due to invalid IL or missing references)
			//IL_011d: Unknown result type (might be due to invalid IL or missing references)
			//IL_013a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0144: Expected O, but got Unknown
			Transform effectPool = GetEffectPool("IronHePool");
			if (!((Object)(object)effectPool == (Object)null) && !((Object)(object)prefab == (Object)null))
			{
				GameObject bulletEffect = GetFromIronBullet2Pool(prefab, effectPool);
				Vector2 targetPos = (boundaryMin + boundaryMax) / 2f;
				bulletEffect.transform.position = new Vector3(targetPos.x, targetPos.y + ScaleDistance(1500f), 0f);
				bulletEffect.transform.rotation = Quaternion.identity;
				bulletEffect.transform.localScale = Vector3.one * EffectScale(1f);
				bulletEffect.SetActive(true);
				ApplyEffectVisuals(bulletEffect);
				TweenSettingsExtensions.OnComplete<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOMove(bulletEffect.transform, Vector2.op_Implicit(targetPos), 1f, false), (Ease)1), (TweenCallback)delegate
				{
					//IL_0026: Unknown result type (might be due to invalid IL or missing references)
					if ((Object)(object)bulletEffect != (Object)null)
					{
						bulletEffect.SetActive(false);
					}
					PlayGlobalIronBoom(targetPos);
				});
			}
		};
		if ((Object)(object)ironBullet2Prefab != (Object)null)
		{
			playAction(ironBullet2Prefab);
			return;
		}
		LoadSkillPrefab("Assets/GameRes/Role/iron_bullet2.prefab", delegate(GameObject prefab)
		{
			ironBullet2Prefab = prefab;
			playAction(prefab);
		});
	}

	private void PlayGlobalIronBoom(Vector2 targetPos)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		Action<GameObject> playBoomAction = delegate(GameObject prefab)
		{
			//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			Transform effectPool = GetEffectPool("IronBoomPool");
			if (!((Object)(object)effectPool == (Object)null) && !((Object)(object)prefab == (Object)null))
			{
				UISpriteAnimationSequence fromIronBoom2Pool = GetFromIronBoom2Pool(prefab, effectPool);
				if ((Object)(object)fromIronBoom2Pool != (Object)null)
				{
					((Component)fromIronBoom2Pool).gameObject.SetActive(true);
					ApplyEffectVisuals(((Component)fromIronBoom2Pool).gameObject);
					((Component)fromIronBoom2Pool).transform.position = Vector2.op_Implicit(targetPos);
					((Component)fromIronBoom2Pool).transform.localScale = Vector3.one * EffectScale(2f);
					fromIronBoom2Pool.Restart();
					fromIronBoom2Pool.Play();
				}
				TossCoinsInRadius(targetPos, ScaleDistance(3000f), int.MaxValue, queueTosses: true, suppressLandingEffects: true);
			}
		};
		if ((Object)(object)ironBoom2Prefab != (Object)null)
		{
			playBoomAction(ironBoom2Prefab);
			return;
		}
		LoadSkillPrefab("Assets/GameRes/Role/iron_boom2.prefab", delegate(GameObject prefab)
		{
			ironBoom2Prefab = prefab;
			playBoomAction(prefab);
		});
	}

	private void PlayHaaland(IRoleThrowableVat throwable)
	{
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		if (throwable == null)
		{
			return;
		}
		float animationScale = 1.5f;
		if (awakeSkillLevel > 0)
		{
			animationScale = ((awakeSkillLevel > 1) ? 2.5f : 2f);
		}
		float radius = ScaleDistance(GetHelperExtraBuff(9) * 120f * animationScale);
		int zuqiuCount = ((awakeSkillLevel >= 3) ? 2 : ((awakeSkillLevel >= 2) ? 1 : 0));
		Vector2 center = Vector2.op_Implicit(throwable.VatTransform.position);
		LoadSkillVatAsset("Assets/GameRes/VAT/zgygy_roleskill_halande_SkeletonData_VAT.asset", delegate(SpineVatAsset asset)
		{
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)asset != (Object)null)
			{
				string animationName = ((awakeSkillLevel > 0 && asset.FindAnimation("skill_juexing") != null) ? "skill_juexing" : "skill");
				PlaySkillVatEffect(halandeSkillVatPool, "HalandPool", asset, center, animationScale, animationName);
			}
		});
		TossCoinsInRadius(center, radius, 10);
		LaunchZuqiu(throwable, zuqiuCount);
	}

	private void LaunchZuqiu(IRoleThrowableVat throwable, int zuqiuCount)
	{
		if (zuqiuCount <= 0)
		{
			return;
		}
		Action launch = delegate
		{
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_011c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0121: Unknown result type (might be due to invalid IL or missing references)
			//IL_0126: Unknown result type (might be due to invalid IL or missing references)
			//IL_012e: Unknown result type (might be due to invalid IL or missing references)
			//IL_014e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0159: Unknown result type (might be due to invalid IL or missing references)
			Transform effectPool = GetEffectPool("ZuqiuPool");
			if (!((Object)(object)effectPool == (Object)null) && !((Object)(object)zuqiuPrefab == (Object)null))
			{
				Vector2 directionToCoinOrRandom = GetDirectionToCoinOrRandom(throwable);
				float num = 30f;
				float num2 = (float)(-(zuqiuCount - 1)) * num / 2f;
				for (int i = 0; i < zuqiuCount; i++)
				{
					GameObject fromZuqiuPool = GetFromZuqiuPool(zuqiuPrefab, effectPool);
					fromZuqiuPool.transform.position = ((Component)this).transform.position;
					fromZuqiuPool.transform.localScale = Vector3.one * EffectScale(1f);
					fromZuqiuPool.SetActive(true);
					ApplyEffectVisuals(fromZuqiuPool);
					UISpriteAnimationSequence component = fromZuqiuPool.GetComponent<UISpriteAnimationSequence>();
					if ((Object)(object)component != (Object)null)
					{
						component.IsLooping = true;
						component.Play();
					}
					ZuqiuAnimation zuqiuAnimation = fromZuqiuPool.GetComponent<ZuqiuAnimation>();
					if ((Object)(object)zuqiuAnimation == (Object)null)
					{
						zuqiuAnimation = fromZuqiuPool.AddComponent<ZuqiuAnimation>();
					}
					zuqiuAnimation.InitVat(Vector2.op_Implicit(((Component)this).transform.position), RotateDirection(directionToCoinOrRandom, num2 + (float)i * num), ScaleDistance(1000f), 999, boundaryMin, boundaryMax, TossCoinSimple, mainScene, worldDistanceScale);
				}
			}
		};
		if ((Object)(object)zuqiuPrefab != (Object)null)
		{
			launch();
			return;
		}
		LoadSkillPrefab("Assets/GameRes/Role/zuqiu.prefab", delegate(GameObject prefab)
		{
			zuqiuPrefab = prefab;
			launch();
		});
	}

	private void PlayDuShen(IRoleThrowableVat throwable)
	{
		if (throwable != null)
		{
			if (awakeSkillLevel >= 2)
			{
				PlayDuShenAwake(throwable);
			}
			else
			{
				PlayDuShenNormal(throwable);
			}
		}
	}

	private void PlayDuShenNormal(IRoleThrowableVat throwable)
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		Vector2 hitPos = (((Object)(object)mainScene != (Object)null) ? mainScene.GetRandomPositionInBoundary(100f) : (Vector2.op_Implicit(throwable.VatTransform.position) + Random.insideUnitCircle * ScaleDistance(200f)));
		hitPos = Vector2.op_Implicit(ClampToBoundary(new Vector3(hitPos.x, hitPos.y, 0f)));
		LoadSkillVatAsset("Assets/GameRes/VAT/zgygy_roleskill_dushen_SkeletonData_VAT.asset", delegate(SpineVatAsset asset)
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)asset != (Object)null)
			{
				PlayDuShenNormalLoaded(hitPos, asset);
			}
		});
	}

	private void PlayDuShenNormalLoaded(Vector2 hitPos, SpineVatAsset asset)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Expected O, but got Unknown
		SpineVat diceHit = GetFromSkillVatPool(dushenSkillVatPool, "DiceHitPool", asset);
		if ((Object)(object)diceHit == (Object)null)
		{
			return;
		}
		if ((Object)(object)diceHit.VatAsset != (Object)(object)asset)
		{
			diceHit.SetVatAssetNoPlay(asset);
		}
		diceHit.SetDepthSortRange(boundaryMin.y, boundaryMax.y);
		diceHit.SetDepthSortEnabled(enabled: true);
		diceHit.SetRendererSortingOrderByYEnabled(enabled: true);
		diceHit.SetSortingOrder(0);
		diceHit.SetRenderingVisible(visible: true);
		((Component)diceHit).gameObject.SetActive(true);
		((Component)diceHit).transform.position = new Vector3(hitPos.x, hitPos.y + ScaleDistance(1500f), 0f);
		((Component)diceHit).transform.localScale = Vector3.one * 2f;
		diceHit.Play("idle", shouldLoop: true);
		float num = 0.7f;
		TweenSettingsExtensions.OnComplete<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOMove(((Component)diceHit).transform, Vector2.op_Implicit(hitPos), num, false), (Ease)5), (TweenCallback)delegate
		{
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c2: Expected O, but got Unknown
			int num2 = Random.Range(1, 7);
			if (awakeSkillLevel >= 3)
			{
				num2 = 6;
			}
			string animationName = $"num_{num2}";
			diceHit.Play(animationName, shouldLoop: false, 0f, delegate
			{
				if ((Object)(object)diceHit != (Object)null)
				{
					((Component)diceHit).gameObject.SetActive(false);
				}
			});
			int coinDicePoint = ((awakeSkillLevel >= 1) ? num2 : 0);
			TossCoinsInRadius(hitPos, ScaleDistance(200f), 10, queueTosses: true, suppressLandingEffects: true, coinDicePoint);
			DOVirtual.DelayedCall(0.3f, (TweenCallback)delegate
			{
				//IL_0011: Unknown result type (might be due to invalid IL or missing references)
				SpawnDiceMoves(hitPos, coinDicePoint);
			}, true);
		});
	}

	private void SpawnDiceMoves(Vector2 center, int dicePoint)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		SpineVatAsset moveAsset = GetSkillVatAsset("Assets/GameRes/VAT/zgygy_roleskill_dushen_1_SkeletonData_VAT.asset");
		if ((Object)(object)moveAsset == (Object)null)
		{
			LoadSkillVatAsset("Assets/GameRes/VAT/zgygy_roleskill_dushen_1_SkeletonData_VAT.asset", delegate(SpineVatAsset asset)
			{
				//IL_0010: Unknown result type (might be due to invalid IL or missing references)
				if ((Object)(object)asset != (Object)null)
				{
					SpawnDiceMoves(center, dicePoint);
				}
			});
			return;
		}
		Action<GameObject> playMoveAction = delegate(GameObject prefab)
		{
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0118: Unknown result type (might be due to invalid IL or missing references)
			//IL_0156: Unknown result type (might be due to invalid IL or missing references)
			//IL_015e: Unknown result type (might be due to invalid IL or missing references)
			//IL_017b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0186: Unknown result type (might be due to invalid IL or missing references)
			Transform effectPool = GetEffectPool("DiceMovePool");
			if (!((Object)(object)effectPool == (Object)null) && !((Object)(object)prefab == (Object)null))
			{
				int helperLevel = ((!((Object)(object)GameDataManager.Instance != (Object)null)) ? 1 : GameDataManager.Instance.GetHelperLevel(roleTypeValue));
				Vector2[] array = (Vector2[])(object)new Vector2[4]
				{
					Vector2.up,
					Vector2.down,
					Vector2.left,
					Vector2.right
				};
				Transform val = (((Object)(object)mainScene != (Object)null && (Object)(object)mainScene.GetRolePool() != (Object)null) ? mainScene.GetRolePool().parent : effectPool);
				float num = (((Object)(object)val != (Object)null) ? val.position.z : 0f);
				for (int i = 0; i < array.Length; i++)
				{
					GameObject fromDiceMovePool = GetFromDiceMovePool(prefab, effectPool);
					fromDiceMovePool.transform.position = new Vector3(center.x, center.y, num);
					fromDiceMovePool.SetActive(true);
					ApplyEffectVisuals(fromDiceMovePool);
					DuShenDiceMove duShenDiceMove = fromDiceMovePool.GetComponent<DuShenDiceMove>();
					if ((Object)(object)duShenDiceMove == (Object)null)
					{
						duShenDiceMove = fromDiceMovePool.AddComponent<DuShenDiceMove>();
					}
					duShenDiceMove.InitVat(center, array[i], roleTypeValue, helperLevel, dicePoint, boundaryMin, boundaryMax, TossCoinSimple, mainScene, worldDistanceScale, moveAsset);
				}
			}
		};
		if ((Object)(object)diceMovePrefab != (Object)null)
		{
			playMoveAction(diceMovePrefab);
			return;
		}
		LoadSkillPrefab("Assets/GameRes/Role/dushen_DiceMove.prefab", delegate(GameObject prefab)
		{
			diceMovePrefab = prefab;
			playMoveAction(prefab);
		});
	}

	private void PlayDuShenAwake(IRoleThrowableVat throwable)
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		Vector2 hitPos1 = (((Object)(object)mainScene != (Object)null) ? mainScene.GetRandomPositionInBoundary(100f) : (Vector2.op_Implicit(throwable.VatTransform.position) + Random.insideUnitCircle * ScaleDistance(200f)));
		hitPos1 = Vector2.op_Implicit(ClampToBoundary(new Vector3(hitPos1.x, hitPos1.y, 0f)));
		Vector2 hitPos2 = (((Object)(object)mainScene != (Object)null) ? mainScene.GetRandomPositionInBoundary(100f) : (hitPos1 + Random.insideUnitCircle * ScaleDistance(300f)));
		hitPos2 = Vector2.op_Implicit(ClampToBoundary(new Vector3(hitPos2.x, hitPos2.y, 0f)));
		LoadSkillVatAsset("Assets/GameRes/VAT/zgygy_roleskill_dushen_SkeletonData_VAT.asset", delegate(SpineVatAsset asset)
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)asset != (Object)null)
			{
				PlayDuShenAwakeLoaded(hitPos1, hitPos2, asset);
			}
		});
	}

	private void PlayDuShenAwakeLoaded(Vector2 hitPos1, Vector2 hitPos2, SpineVatAsset asset)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		Action<Vector2, Action> spawnDiceHitVat = delegate(Vector2 targetPos, Action onComplete)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_012d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0142: Unknown result type (might be due to invalid IL or missing references)
			//IL_014c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0180: Unknown result type (might be due to invalid IL or missing references)
			//IL_0185: Unknown result type (might be due to invalid IL or missing references)
			//IL_019e: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a8: Expected O, but got Unknown
			SpineVat diceHit = GetFromSkillVatPool(dushenSkillVatPool, "DiceHitPool", asset);
			if ((Object)(object)diceHit == (Object)null)
			{
				onComplete?.Invoke();
			}
			else
			{
				if ((Object)(object)diceHit.VatAsset != (Object)(object)asset)
				{
					diceHit.SetVatAssetNoPlay(asset);
				}
				diceHit.SetDepthSortRange(boundaryMin.y, boundaryMax.y);
				diceHit.SetDepthSortEnabled(enabled: true);
				diceHit.SetRendererSortingOrderByYEnabled(enabled: true);
				diceHit.SetSortingOrder(0);
				diceHit.SetRenderingVisible(visible: true);
				((Component)diceHit).gameObject.SetActive(true);
				((Component)diceHit).transform.position = new Vector3(targetPos.x, targetPos.y + ScaleDistance(1500f), 0f);
				((Component)diceHit).transform.localScale = Vector3.one * 2f;
				diceHit.Play("idle", shouldLoop: true);
				float num = 0.5f;
				TweenSettingsExtensions.OnComplete<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOMove(((Component)diceHit).transform, Vector2.op_Implicit(targetPos), num, false), (Ease)5), (TweenCallback)delegate
				{
					//IL_0096: Unknown result type (might be due to invalid IL or missing references)
					//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
					//IL_00d6: Expected O, but got Unknown
					int num2 = Random.Range(1, 7);
					if (awakeSkillLevel >= 3)
					{
						num2 = 6;
					}
					string animationName = $"num_{num2}";
					diceHit.Play(animationName, shouldLoop: false, 0f, delegate
					{
						if ((Object)(object)diceHit != (Object)null)
						{
							((Component)diceHit).gameObject.SetActive(false);
						}
					});
					int coinDicePoint = ((awakeSkillLevel >= 1) ? num2 : 0);
					TossCoinsInRadius(targetPos, ScaleDistance(200f), 10, queueTosses: true, suppressLandingEffects: true, coinDicePoint);
					DOVirtual.DelayedCall(0.3f, (TweenCallback)delegate
					{
						//IL_0016: Unknown result type (might be due to invalid IL or missing references)
						SpawnDiceMoves(targetPos, coinDicePoint);
					}, true);
					onComplete?.Invoke();
				});
			}
		};
		TweenCallback val = default(TweenCallback);
		spawnDiceHitVat(hitPos1, delegate
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Expected O, but got Unknown
			//IL_0023: Expected O, but got Unknown
			TweenCallback obj = val;
			if (obj == null)
			{
				TweenCallback val2 = delegate
				{
					//IL_0007: Unknown result type (might be due to invalid IL or missing references)
					spawnDiceHitVat(hitPos2, null);
				};
				TweenCallback val3 = val2;
				val = val2;
				obj = val3;
			}
			DOVirtual.DelayedCall(0.3f, obj, true);
		});
	}

	private GameObject GetFromDiceMovePool(GameObject prefab, Transform parent)
	{
		GameObject sceneSharedEffect = GetSceneSharedEffect("DiceMovePool", prefab, parent);
		if ((Object)(object)sceneSharedEffect != (Object)null)
		{
			return sceneSharedEffect;
		}
		diceMovePool.RemoveAll((GameObject m) => (Object)(object)m == (Object)null);
		for (int num = 0; num < diceMovePool.Count; num++)
		{
			if (!diceMovePool[num].activeSelf)
			{
				if ((Object)(object)parent != (Object)null && (Object)(object)diceMovePool[num].transform.parent != (Object)(object)parent)
				{
					diceMovePool[num].transform.SetParent(parent, false);
				}
				return diceMovePool[num];
			}
		}
		GameObject val = Object.Instantiate<GameObject>(prefab, parent);
		diceMovePool.Add(val);
		return val;
	}

	private void TossCoinsInRadius(Vector2 center, float radius, int maxTossCount, bool queueTosses = false, bool suppressLandingEffects = false, int dicePoint = 0)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)mainScene == (Object)null)
		{
			return;
		}
		randomCoinCandidates.Clear();
		mainScene.GetIdleThrowablesInRadius(center, radius, randomCoinCandidates);
		for (int i = 0; i < randomCoinCandidates.Count; i++)
		{
			IRoleThrowableVat roleThrowableVat = randomCoinCandidates[i];
			if (roleThrowableVat == null || (roleThrowableVat is CoinVat coinVat && !CanFlipCoin(coinVat.coinType)) || maxTossCount <= 0)
			{
				continue;
			}
			if (dicePoint > 0 && roleThrowableVat is CoinVat coinVat2)
			{
				coinVat2.DicePoint = dicePoint;
			}
			if (queueTosses)
			{
				if (roleThrowableVat is CoinVat coinVat3)
				{
					if (!mainScene.EnqueueSkillCoinToss(coinVat3, roleTypeValue, ScaleDistance(scatterRadius), suppressLandingEffects))
					{
						coinVat3.DicePoint = 0;
						continue;
					}
				}
				else
				{
					TossCoinSimple(roleThrowableVat);
				}
			}
			else
			{
				TossCoinSimple(roleThrowableVat);
			}
			maxTossCount--;
		}
	}

	private Vector2 PickRandomCoinPositionOrSceneCenter()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		List<CoinVat> coinList = GetCoinList();
		if (coinList != null && coinList.Count > 0)
		{
			CoinVat coinVat = coinList[Random.Range(0, coinList.Count)];
			if ((Object)(object)coinVat != (Object)null)
			{
				return Vector2.op_Implicit(coinVat.VatTransform.position);
			}
		}
		return (boundaryMin + boundaryMax) / 2f;
	}

	private bool TryPickIronDenseTargetPosition(out Vector2 targetPosition)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		targetPosition = Vector2.zero;
		if (awakeSkillLevel <= 0 || (Object)(object)mainScene == (Object)null)
		{
			return false;
		}
		Vector2 mostDenseCoinPosition = mainScene.GetMostDenseCoinPosition();
		randomCoinCandidates.Clear();
		mainScene.GetIdleThrowablesInRadius(mostDenseCoinPosition, ScaleDistance(300f), randomCoinCandidates);
		if (randomCoinCandidates.Count > 0)
		{
			targetPosition = mostDenseCoinPosition;
			return true;
		}
		return false;
	}

	private Vector2 GetDirectionToCoinOrRandom(IRoleThrowableVat throwable)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val;
		Vector2 normalized;
		if (throwable == null)
		{
			val = Random.insideUnitCircle;
			normalized = ((Vector2)(ref val)).normalized;
		}
		else
		{
			val = Vector2.op_Implicit(throwable.VatTransform.position) - Vector2.op_Implicit(((Component)this).transform.position);
			normalized = ((Vector2)(ref val)).normalized;
		}
		Vector2 val2 = normalized;
		if (!(val2 == Vector2.zero))
		{
			return val2;
		}
		return Vector2.right;
	}

	private static Vector2 RotateDirection(Vector2 direction, float angleDegrees)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (Mathf.Approximately(angleDegrees, 0f))
		{
			return ((Vector2)(ref direction)).normalized;
		}
		float num = angleDegrees * ((float)Math.PI / 180f);
		float num2 = Mathf.Cos(num);
		float num3 = Mathf.Sin(num);
		Vector2 val = new Vector2(direction.x * num2 - direction.y * num3, direction.x * num3 + direction.y * num2);
		return ((Vector2)(ref val)).normalized;
	}

	private void PlayIdle()
	{
		PlayAnimation(isFacingFront ? "idle_front" : "idle_back", loop: true);
	}

	private void PlayWalk(bool front)
	{
		isFacingFront = front;
		PlayAnimation(front ? "walk_front" : "walk_back", loop: true);
	}

	private void PlayThrow(bool front)
	{
		PlayAnimation(front ? "throw_front" : "throw_back", loop: false);
	}

	private void PlayAnimation(string animationName, bool loop)
	{
		if (!((Object)(object)spineVat == (Object)null) && spineVat.IsReady && (!(currentAnimName == animationName) || currentAnimLoop != loop))
		{
			currentAnimName = animationName;
			currentAnimLoop = loop;
			spineVat.Play(animationName, loop);
		}
	}

	private GameObject GetSceneSharedEffect(string poolName, GameObject prefab, Transform parent, int maxActive = 0)
	{
		if (!UseSceneSharedEffectPool)
		{
			return null;
		}
		return mainScene.GetPooledVatEffect(poolName, prefab, parent, maxActive);
	}

	private static T GetOrConfigureScenePooledComponent<T>(GameObject item, Action<GameObject, T> onConfigure = null, bool addIfMissing = false) where T : Component
	{
		if ((Object)(object)item == (Object)null)
		{
			return default(T);
		}
		T val = item.GetComponent<T>();
		if ((Object)(object)val == (Object)null && addIfMissing)
		{
			val = item.AddComponent<T>();
		}
		if ((Object)(object)val == (Object)null)
		{
			return default(T);
		}
		PooledEffectMarker pooledEffectMarker = item.GetComponent<PooledEffectMarker>();
		if ((Object)(object)pooledEffectMarker == (Object)null)
		{
			pooledEffectMarker = item.AddComponent<PooledEffectMarker>();
		}
		if (!pooledEffectMarker.IsConfigured)
		{
			onConfigure?.Invoke(item, val);
			pooledEffectMarker.IsConfigured = true;
		}
		return val;
	}

	private GameObject GetFromTornadoPool(GameObject prefab, Transform parent)
	{
		GameObject sceneSharedEffect = GetSceneSharedEffect("TornadoPool", prefab, parent);
		if ((Object)(object)sceneSharedEffect != (Object)null)
		{
			return sceneSharedEffect;
		}
		tornadoPool.RemoveAll((GameObject t) => (Object)(object)t == (Object)null);
		for (int num = 0; num < tornadoPool.Count; num++)
		{
			if (!tornadoPool[num].activeSelf)
			{
				if ((Object)(object)parent != (Object)null && (Object)(object)tornadoPool[num].transform.parent != (Object)(object)parent)
				{
					tornadoPool[num].transform.SetParent(parent, false);
				}
				return tornadoPool[num];
			}
		}
		GameObject val = Object.Instantiate<GameObject>(prefab, parent);
		tornadoPool.Add(val);
		return val;
	}

	private UISpriteAnimationSequence GetFromMagicPool(GameObject prefab, Transform parent)
	{
		GameObject sceneSharedEffect = GetSceneSharedEffect("MagicianPool", prefab, parent);
		if ((Object)(object)sceneSharedEffect != (Object)null)
		{
			return GetOrConfigureScenePooledComponent<UISpriteAnimationSequence>(sceneSharedEffect, (Action<GameObject, UISpriteAnimationSequence>)delegate(GameObject val, UISpriteAnimationSequence seq)
			{
				seq.IsLooping = false;
				seq.OnAnimationComplete = (Action)Delegate.Combine(seq.OnAnimationComplete, (Action)delegate
				{
					val.SetActive(false);
				});
			}, addIfMissing: false);
		}
		magicAnimPool.RemoveAll((UISpriteAnimationSequence m) => (Object)(object)m == (Object)null || (Object)(object)((Component)m).gameObject == (Object)null);
		for (int num = 0; num < magicAnimPool.Count; num++)
		{
			if (!((Component)magicAnimPool[num]).gameObject.activeSelf)
			{
				if ((Object)(object)parent != (Object)null && (Object)(object)((Component)magicAnimPool[num]).transform.parent != (Object)(object)parent)
				{
					((Component)magicAnimPool[num]).transform.SetParent(parent, false);
				}
				return magicAnimPool[num];
			}
		}
		GameObject item = Object.Instantiate<GameObject>(prefab, parent);
		UISpriteAnimationSequence component = item.GetComponent<UISpriteAnimationSequence>();
		if ((Object)(object)component != (Object)null)
		{
			component.IsLooping = false;
			component.OnAnimationComplete = (Action)Delegate.Combine(component.OnAnimationComplete, (Action)delegate
			{
				item.SetActive(false);
			});
			magicAnimPool.Add(component);
		}
		return component;
	}

	private GameObject GetFromLanqiuPool(GameObject prefab, Transform parent)
	{
		GameObject sceneSharedEffect = GetSceneSharedEffect("LanqiuPool", prefab, parent);
		if ((Object)(object)sceneSharedEffect != (Object)null)
		{
			return sceneSharedEffect;
		}
		lanqiuPool.RemoveAll((GameObject m) => (Object)(object)m == (Object)null);
		for (int num = 0; num < lanqiuPool.Count; num++)
		{
			if (!lanqiuPool[num].activeSelf)
			{
				if ((Object)(object)parent != (Object)null && (Object)(object)lanqiuPool[num].transform.parent != (Object)(object)parent)
				{
					lanqiuPool[num].transform.SetParent(parent, false);
				}
				return lanqiuPool[num];
			}
		}
		GameObject val = Object.Instantiate<GameObject>(prefab, parent);
		lanqiuPool.Add(val);
		return val;
	}

	private DaSheHand GetFromDaShePool(GameObject prefab, Transform parent)
	{
		GameObject sceneSharedEffect = GetSceneSharedEffect("DaShePool", prefab, parent);
		if ((Object)(object)sceneSharedEffect != (Object)null)
		{
			return GetOrConfigureScenePooledComponent<DaSheHand>(sceneSharedEffect, (Action<GameObject, DaSheHand>)null, addIfMissing: true);
		}
		dashePool.RemoveAll((DaSheHand m) => (Object)(object)m == (Object)null || (Object)(object)((Component)m).gameObject == (Object)null);
		for (int num = 0; num < dashePool.Count; num++)
		{
			if (!((Component)dashePool[num]).gameObject.activeSelf)
			{
				if ((Object)(object)parent != (Object)null && (Object)(object)((Component)dashePool[num]).transform.parent != (Object)(object)parent)
				{
					((Component)dashePool[num]).transform.SetParent(parent, false);
				}
				return dashePool[num];
			}
		}
		GameObject val = Object.Instantiate<GameObject>(prefab, parent);
		DaSheHand daSheHand = val.GetComponent<DaSheHand>();
		if ((Object)(object)daSheHand == (Object)null)
		{
			daSheHand = val.AddComponent<DaSheHand>();
		}
		dashePool.Add(daSheHand);
		return daSheHand;
	}

	private GameObject GetFromDajiPool(GameObject prefab, Transform parent)
	{
		GameObject sceneSharedEffect = GetSceneSharedEffect("DajiPool", prefab, parent);
		if ((Object)(object)sceneSharedEffect != (Object)null)
		{
			return sceneSharedEffect;
		}
		dajiPool.RemoveAll((GameObject m) => (Object)(object)m == (Object)null);
		for (int num = 0; num < dajiPool.Count; num++)
		{
			if (!dajiPool[num].activeSelf)
			{
				if ((Object)(object)parent != (Object)null && (Object)(object)dajiPool[num].transform.parent != (Object)(object)parent)
				{
					dajiPool[num].transform.SetParent(parent, false);
				}
				return dajiPool[num];
			}
		}
		GameObject val = Object.Instantiate<GameObject>(prefab, parent);
		dajiPool.Add(val);
		return val;
	}

	private GameObject GetFromIronBulletPool(GameObject prefab, Transform parent)
	{
		GameObject sceneSharedEffect = GetSceneSharedEffect("IronPool", prefab, parent);
		if ((Object)(object)sceneSharedEffect != (Object)null)
		{
			return sceneSharedEffect;
		}
		ironBulletPool.RemoveAll((GameObject m) => (Object)(object)m == (Object)null);
		for (int num = 0; num < ironBulletPool.Count; num++)
		{
			if (!ironBulletPool[num].activeSelf)
			{
				if ((Object)(object)parent != (Object)null && (Object)(object)ironBulletPool[num].transform.parent != (Object)(object)parent)
				{
					ironBulletPool[num].transform.SetParent(parent, false);
				}
				return ironBulletPool[num];
			}
		}
		GameObject val = Object.Instantiate<GameObject>(prefab, parent);
		ironBulletPool.Add(val);
		return val;
	}

	private UISpriteAnimationSequence GetFromIronBoomPool(GameObject prefab, Transform parent)
	{
		GameObject sceneSharedEffect = GetSceneSharedEffect("IronBoomPool", prefab, parent);
		if ((Object)(object)sceneSharedEffect != (Object)null)
		{
			return GetOrConfigureScenePooledComponent<UISpriteAnimationSequence>(sceneSharedEffect, (Action<GameObject, UISpriteAnimationSequence>)delegate(GameObject val, UISpriteAnimationSequence seq)
			{
				seq.IsLooping = false;
				seq.OnAnimationComplete = (Action)Delegate.Combine(seq.OnAnimationComplete, (Action)delegate
				{
					val.SetActive(false);
				});
			}, addIfMissing: false);
		}
		ironBoomPool.RemoveAll((UISpriteAnimationSequence m) => (Object)(object)m == (Object)null || (Object)(object)((Component)m).gameObject == (Object)null);
		for (int num = 0; num < ironBoomPool.Count; num++)
		{
			if (!((Component)ironBoomPool[num]).gameObject.activeSelf)
			{
				if ((Object)(object)parent != (Object)null && (Object)(object)((Component)ironBoomPool[num]).transform.parent != (Object)(object)parent)
				{
					((Component)ironBoomPool[num]).transform.SetParent(parent, false);
				}
				return ironBoomPool[num];
			}
		}
		GameObject item = Object.Instantiate<GameObject>(prefab, parent);
		UISpriteAnimationSequence component = item.GetComponent<UISpriteAnimationSequence>();
		if ((Object)(object)component != (Object)null)
		{
			component.IsLooping = false;
			component.OnAnimationComplete = (Action)Delegate.Combine(component.OnAnimationComplete, (Action)delegate
			{
				item.SetActive(false);
			});
			ironBoomPool.Add(component);
		}
		return component;
	}

	private GameObject GetFromIronBullet2Pool(GameObject prefab, Transform parent)
	{
		GameObject sceneSharedEffect = GetSceneSharedEffect("IronHePool", prefab, parent);
		if ((Object)(object)sceneSharedEffect != (Object)null)
		{
			return sceneSharedEffect;
		}
		ironBullet2Pool.RemoveAll((GameObject m) => (Object)(object)m == (Object)null);
		for (int num = 0; num < ironBullet2Pool.Count; num++)
		{
			if (!ironBullet2Pool[num].activeSelf)
			{
				if ((Object)(object)parent != (Object)null && (Object)(object)ironBullet2Pool[num].transform.parent != (Object)(object)parent)
				{
					ironBullet2Pool[num].transform.SetParent(parent, false);
				}
				return ironBullet2Pool[num];
			}
		}
		GameObject val = Object.Instantiate<GameObject>(prefab, parent);
		ironBullet2Pool.Add(val);
		return val;
	}

	private UISpriteAnimationSequence GetFromIronBoom2Pool(GameObject prefab, Transform parent)
	{
		GameObject sceneSharedEffect = GetSceneSharedEffect("IronBoomPool", prefab, parent);
		if ((Object)(object)sceneSharedEffect != (Object)null)
		{
			return GetOrConfigureScenePooledComponent<UISpriteAnimationSequence>(sceneSharedEffect, (Action<GameObject, UISpriteAnimationSequence>)delegate(GameObject val, UISpriteAnimationSequence seq)
			{
				seq.IsLooping = false;
				seq.OnAnimationComplete = (Action)Delegate.Combine(seq.OnAnimationComplete, (Action)delegate
				{
					val.SetActive(false);
				});
			}, addIfMissing: false);
		}
		ironBoom2Pool.RemoveAll((UISpriteAnimationSequence m) => (Object)(object)m == (Object)null || (Object)(object)((Component)m).gameObject == (Object)null);
		for (int num = 0; num < ironBoom2Pool.Count; num++)
		{
			if (!((Component)ironBoom2Pool[num]).gameObject.activeSelf)
			{
				if ((Object)(object)parent != (Object)null && (Object)(object)((Component)ironBoom2Pool[num]).transform.parent != (Object)(object)parent)
				{
					((Component)ironBoom2Pool[num]).transform.SetParent(parent, false);
				}
				return ironBoom2Pool[num];
			}
		}
		GameObject item = Object.Instantiate<GameObject>(prefab, parent);
		UISpriteAnimationSequence component = item.GetComponent<UISpriteAnimationSequence>();
		if ((Object)(object)component != (Object)null)
		{
			component.IsLooping = false;
			component.OnAnimationComplete = (Action)Delegate.Combine(component.OnAnimationComplete, (Action)delegate
			{
				item.SetActive(false);
			});
			ironBoom2Pool.Add(component);
		}
		return component;
	}

	private GameObject GetFromZuqiuPool(GameObject prefab, Transform parent)
	{
		GameObject sceneSharedEffect = GetSceneSharedEffect("ZuqiuPool", prefab, parent);
		if ((Object)(object)sceneSharedEffect != (Object)null)
		{
			return sceneSharedEffect;
		}
		zuqiuPool.RemoveAll((GameObject m) => (Object)(object)m == (Object)null);
		for (int num = 0; num < zuqiuPool.Count; num++)
		{
			if (!zuqiuPool[num].activeSelf)
			{
				if ((Object)(object)parent != (Object)null && (Object)(object)zuqiuPool[num].transform.parent != (Object)(object)parent)
				{
					zuqiuPool[num].transform.SetParent(parent, false);
				}
				return zuqiuPool[num];
			}
		}
		GameObject val = Object.Instantiate<GameObject>(prefab, parent);
		zuqiuPool.Add(val);
		return val;
	}

	private void ApplySkillPoolVisuals()
	{
		for (int i = 0; i < gongfuSkillVatPool.Count; i++)
		{
			if ((Object)(object)gongfuSkillVatPool[i] != (Object)null)
			{
				gongfuSkillVatPool[i].SetRenderingVisible(VatDebugSettings.AreRoleSkillVisualsVisible(roleTypeValue));
			}
		}
		for (int j = 0; j < halandeSkillVatPool.Count; j++)
		{
			if ((Object)(object)halandeSkillVatPool[j] != (Object)null)
			{
				halandeSkillVatPool[j].SetRenderingVisible(VatDebugSettings.AreRoleSkillVisualsVisible(roleTypeValue));
			}
		}
		for (int k = 0; k < hammerWakeupVatPool.Count; k++)
		{
			if ((Object)(object)hammerWakeupVatPool[k] != (Object)null)
			{
				hammerWakeupVatPool[k].SetRenderingVisible(VatDebugSettings.AreRoleSkillVisualsVisible(roleTypeValue));
			}
		}
		for (int l = 0; l < dushenSkillVatPool.Count; l++)
		{
			if ((Object)(object)dushenSkillVatPool[l] != (Object)null)
			{
				dushenSkillVatPool[l].SetRenderingVisible(VatDebugSettings.AreRoleSkillVisualsVisible(roleTypeValue));
			}
		}
		ApplyEffectPoolVisuals(tornadoPool);
		ApplyEffectPoolVisuals<UISpriteAnimationSequence>(magicAnimPool);
		ApplyEffectPoolVisuals(lanqiuPool);
		ApplyEffectPoolVisuals<DaSheHand>(dashePool);
		ApplyEffectPoolVisuals(dajiPool);
		ApplyEffectPoolVisuals(ironBulletPool);
		ApplyEffectPoolVisuals<UISpriteAnimationSequence>(ironBoomPool);
		ApplyEffectPoolVisuals(ironBullet2Pool);
		ApplyEffectPoolVisuals<UISpriteAnimationSequence>(ironBoom2Pool);
		ApplyEffectPoolVisuals(zuqiuPool);
		ApplyEffectPoolVisuals(diceMovePool);
		if ((Object)(object)huanxing != (Object)null)
		{
			ApplyEffectVisuals(huanxing);
		}
	}

	private void ApplyEffectPoolVisuals<T>(List<T> pool) where T : Component
	{
		for (int i = 0; i < pool.Count; i++)
		{
			if ((Object)(object)pool[i] != (Object)null)
			{
				ApplyEffectVisuals(((Component)pool[i]).gameObject);
			}
		}
	}

	private void ApplyEffectPoolVisuals(List<GameObject> pool)
	{
		for (int i = 0; i < pool.Count; i++)
		{
			ApplyEffectVisuals(pool[i]);
		}
	}

	private void ApplyEffectVisuals(GameObject effectObject)
	{
		if (!((Object)(object)effectObject == (Object)null))
		{
			bool visible = VatDebugSettings.AreRoleSkillVisualsVisible(roleTypeValue);
			VatEffectVisualCache.SetVisible(effectObject, visible);
		}
	}

	private static void DestroyGameObjectPool(List<GameObject> pool)
	{
		for (int i = 0; i < pool.Count; i++)
		{
			if ((Object)(object)pool[i] != (Object)null)
			{
				ShortcutExtensions.DOKill((Component)(object)pool[i].transform, false);
				Object.Destroy((Object)(object)pool[i]);
			}
		}
		pool.Clear();
	}

	private static void DestroySpineVatPool(List<SpineVat> pool)
	{
		for (int i = 0; i < pool.Count; i++)
		{
			if ((Object)(object)pool[i] != (Object)null && (Object)(object)((Component)pool[i]).gameObject != (Object)null)
			{
				ShortcutExtensions.DOKill((Component)(object)((Component)pool[i]).transform, false);
				Object.Destroy((Object)(object)((Component)pool[i]).gameObject);
			}
		}
		pool.Clear();
	}

	private static void DestroyAnimationPool<T>(List<T> pool) where T : Component
	{
		for (int i = 0; i < pool.Count; i++)
		{
			if ((Object)(object)pool[i] != (Object)null && (Object)(object)((Component)pool[i]).gameObject != (Object)null)
			{
				ShortcutExtensions.DOKill((Component)(object)((Component)pool[i]).transform, false);
				Object.Destroy((Object)(object)((Component)pool[i]).gameObject);
			}
		}
		pool.Clear();
	}

	public static string GetVatAssetPath(int helperId)
	{
		return helperId switch
		{
			0 => "Assets/GameRes/VAT/zgygy_role_SkeletonData_VAT.asset", 
			1 => "Assets/GameRes/VAT/zgygy_role_toughguy_SkeletonData_VAT.asset", 
			2 => "Assets/GameRes/VAT/zgygy_role_miracle_SkeletonData_VAT.asset", 
			3 => "Assets/GameRes/VAT/zgygy_role_magician_SkeletonData_VAT.asset", 
			4 => "Assets/GameRes/VAT/zgygy_role_kungfu_SkeletonData_VAT.asset", 
			5 => "Assets/GameRes/VAT/zgygy_role_jikun_SkeletonData_VAT.asset", 
			6 => "Assets/GameRes/VAT/zgygy_role_dashe_SkeletonData_VAT.asset", 
			7 => "Assets/GameRes/VAT/zgygy_role_jiuwei_SkeletonData_VAT.asset", 
			8 => "Assets/GameRes/VAT/zgygy_role_gangtieren_SkeletonData_VAT.asset", 
			9 => "Assets/GameRes/VAT/zgygy_role_halande_SkeletonData_VAT.asset", 
			10 => "Assets/GameRes/VAT/zgygy_role_dushen_SkeletonData_VAT.asset", 
			_ => "Assets/GameRes/VAT/zgygy_role_SkeletonData_VAT.asset", 
		};
	}
}
