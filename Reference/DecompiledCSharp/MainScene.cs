using System;
using System.Collections;
using System.Collections.Generic;
using Module.Role;
using Module.UI;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainScene : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IMainSceneView
{
	private struct LuckyEchoTask
	{
		public CoinBase Coin;

		public float Remaining;
	}

	[Header("硬币预制体")]
	[SerializeField]
	private Image bgImage;

	[SerializeField]
	private GameObject coinCopperPrefab;

	[SerializeField]
	private GameObject coinSilverPrefab;

	[SerializeField]
	private GameObject coinGoldPrefab;

	[SerializeField]
	private GameObject coinPurplePrefab;

	[SerializeField]
	private RectTransform guideFinger;

	[Header("场景边界")]
	[SerializeField]
	private RectTransform sceneBoxUI;

	[Header("场景边界")]
	[SerializeField]
	private Collider2D sceneBox;

	[Header("生成设置")]
	[SerializeField]
	private int initialCopperCount = 3;

	[SerializeField]
	private float minSpawnDistance = 60f;

	[Header("抛落范围")]
	[SerializeField]
	private float minLandingRadius = 60f;

	[SerializeField]
	private float maxLandingRadius = 120f;

	[SerializeField]
	private float sceneBoundaryPadding = 40f;

	[Header("组件引用")]
	[SerializeField]
	private bool enableCoinAvoidRects = true;

	[SerializeField]
	private UISpriteAnimationSequence spriteAnimation;

	[Header("对象池")]
	[SerializeField]
	private RectTransform rolePool;

	[SerializeField]
	private RectTransform coinPool;

	[SerializeField]
	private RectTransform effectPool;

	private Dictionary<string, RectTransform> _effectPoolDict = new Dictionary<string, RectTransform>();

	[Header("人物睡觉唤醒提示父物体")]
	[SerializeField]
	private RectTransform huanxingPool;

	[SerializeField]
	private RectTransform TextPool;

	[SerializeField]
	private RectTransform coins_smokePool;

	[SerializeField]
	private RectTransform ParticlePool;

	[Header("字体飘字场景中目前数量")]
	[SerializeField]
	private int TextCurrentCount;

	[Header("字体飘字场景中最大数量")]
	[SerializeField]
	public int TextMaxCount = 60;

	private List<CoinBase> _activeCoins = new List<CoinBase>();

	private Vector2 _boundaryMin;

	private Vector2 _boundaryMax;

	private readonly List<Rect> _coinAvoidRects = new List<Rect>();

	private double _copperValue = 1.0;

	private double _silverValue = 5.0;

	private double _goldValue = 10.0;

	private double _purpleValue = 15.0;

	private HashSet<CoinBase> _swipedCoinsThisFrame = new HashSet<CoinBase>();

	private Vector2 _swipeStartPos;

	private bool _isSwipeTracking;

	private List<LuckyEchoTask> _luckyEchoTasks = new List<LuckyEchoTask>();

	private GameObject _helperAddPrefab;

	private List<GameObject> _helperAddPool = new List<GameObject>();

	private Coroutine _roleSpawnCoroutine;

	private List<CoinBase> _sortedCoinsCache = new List<CoinBase>();

	private Vector2 _cachedDensePos = Vector2.zero;

	private int _lastDensePosFrame = -1;

	private Vector2[] spawnPositions = (Vector2[])(object)new Vector2[3]
	{
		new Vector2(-95f, 295f),
		new Vector2(-87f, -200f),
		new Vector2(213f, -150f)
	};

	public const int STEP_2_MONEY = 25;

	public bool isAddGongfu;

	public bool isAddDashe;

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
				_coinAvoidRects.Clear();
			}
		}
	}

	public event Func<float> coinFlipComplete;

	private GameObject GetFromHelperAddPool(GameObject effectPrefab, Transform parent)
	{
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Expected O, but got Unknown
		_helperAddPool.RemoveAll((GameObject g) => (Object)(object)g == (Object)null);
		foreach (GameObject item in _helperAddPool)
		{
			if (!item.activeSelf)
			{
				item.transform.SetParent(parent);
				return item;
			}
		}
		GameObject newEffect = Object.Instantiate<GameObject>(effectPrefab, parent);
		SkeletonGraphic component = newEffect.GetComponent<SkeletonGraphic>();
		if ((Object)(object)component != (Object)null)
		{
			if (component.AnimationState == null)
			{
				component.Initialize(false);
			}
			if (component.AnimationState != null)
			{
				component.AnimationState.Complete += (TrackEntryDelegate)delegate
				{
					if ((Object)(object)newEffect != (Object)null)
					{
						newEffect.SetActive(false);
					}
				};
			}
		}
		_helperAddPool.Add(newEffect);
		return newEffect;
	}

	private void Awake()
	{
		Debug.Log((object)"MainScene.Awake() 被调用！");
		RectTransform obj = guideFinger;
		if (obj != null)
		{
			((Component)obj).gameObject.SetActive(false);
		}
		CalculateBoundary();
	}

	private void Start()
	{
		Debug.Log((object)"MainScene.Start() 被调用！");
		((MonoBehaviour)this).StartCoroutine(InitializeWithConfig());
		if ((Object)(object)bgImage != (Object)null)
		{
			LoadBgImage(GameDataManager.Instance.GetCurrentDeskId());
		}
		int num = PlayerPrefs.GetInt("guide_step_3", 0);
		if ((Object)(object)SoundManager.Instance != (Object)null)
		{
			int worldId = ((!((Object)(object)GameDataManager.Instance != (Object)null)) ? 1 : GameDataManager.Instance.GetCurrentWorldId());
			SoundManager.Instance.PlayBGMForWorld(worldId);
		}
		if (num == 0)
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
		TextMaxCount = 45;
	}

	private void OnGoldChangedForGuide3(double gold)
	{
		if (gold >= 25.0 && PlayerPrefs.GetInt("guide_step_3", 0) == 0)
		{
			((Component)guideFinger).gameObject.SetActive(false);
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

	public void LoadBgImage(int bgId)
	{
		SingletonMono<ResManager>.Instance.LoadSpriteAsync($"Assets/GameRes/MapRes/fight_bg_{bgId}.png", delegate(Sprite sprite)
		{
			if ((Object)(object)sprite != (Object)null)
			{
				bgImage.sprite = sprite;
			}
		}, isAtlas: false);
	}

	private void Update()
	{
		TickLuckyEcho(Time.deltaTime);
	}

	private void LateUpdate()
	{
		_swipedCoinsThisFrame.Clear();
	}

	private void CheckSwipeToThrow()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		if (Input.GetMouseButtonDown(0))
		{
			_swipeStartPos = Vector2.op_Implicit(Input.mousePosition);
			_isSwipeTracking = true;
		}
		else if (Input.GetMouseButtonUp(0))
		{
			_isSwipeTracking = false;
		}
		else
		{
			if (!_isSwipeTracking)
			{
				return;
			}
			Vector3 val = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			Vector2 val2 = default(Vector2);
			((Vector2)(ref val2))._002Ector(val.x, val.y);
			foreach (CoinBase activeCoin in _activeCoins)
			{
				if (!((Object)(object)activeCoin == (Object)null) && activeCoin.State == CoinState.Idle && !_swipedCoinsThisFrame.Contains(activeCoin) && Vector2.Distance(val2, Vector2.op_Implicit(((Component)activeCoin).transform.position)) < 50f && CanSwipeToThrow(activeCoin.coinType))
				{
					TriggerCoinThrow(activeCoin);
					_swipedCoinsThisFrame.Add(activeCoin);
				}
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

	public void TriggerCoinThrow(CoinBase coin)
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
		}
	}

	public void InitializeRoles()
	{
		int num = 0;
		if ((Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null && SingletonMono<UpgradeManager>.Instance.IsInitialized)
		{
			int currentWorldHelperUpgradeId = Constants.GetCurrentWorldHelperUpgradeId();
			num = SingletonMono<UpgradeManager>.Instance.GetUpgradeLevel(currentWorldHelperUpgradeId, "itemUpgrade1");
			Debug.Log((object)$"[MainScene] 从升级系统读取帮手数量: {num}");
		}
		if (num <= 0)
		{
			Debug.Log((object)$"[MainScene] 使用默认帮手数量: {num}");
		}
		((MonoBehaviour)this).StartCoroutine(SpawnRolesSequentially(num));
	}

	private IEnumerator SpawnRolesSequentially(int count)
	{
		while ((Object)(object)SingletonMono<RoleManager>.Instance == (Object)null)
		{
			Debug.Log((object)"[MainScene] 等待 RoleManager 初始化...");
			yield return null;
		}
		int[] helperIds = GameDataManager.Instance.GetHelperIds(count);
		yield return (object)new WaitUntil((Func<bool>)(() => SingletonMono<RoleManager>.Instance.IsPrefabLoaded()));
		Debug.Log((object)$"[MainScene] 开始生成 {count} 个帮手...");
		for (int i = 0; i < count; i++)
		{
			SingletonMono<RoleManager>.Instance.SpawnRole(delegate(Role role)
			{
				_ = (Object)(object)role != (Object)null;
			}, helperIds[i]);
			yield return (object)new WaitForSeconds(0.2f);
		}
		Debug.Log((object)$"[MainScene] 帮手生成完成，总计: {SingletonMono<RoleManager>.Instance.GetRoleCount()}");
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
		Debug.Log((object)"ConfigManager 初始化完成");
		while (!SingletonMono<UpgradeManager>.Instance.IsInitialized)
		{
			yield return null;
		}
		InitializeCoins();
		InitializeRoles();
		if (PlayerPrefs.GetInt("guide_step_1", 0) == 0)
		{
			((MonoBehaviour)this).StartCoroutine(StartGuideClickCoin());
		}
	}

	private IEnumerator StartGuideClickCoin()
	{
		yield return (object)new WaitForEndOfFrame();
		while (_activeCoins.Count == 0)
		{
			yield return null;
		}
		Time.timeScale = 0f;
		yield return (object)new WaitForEndOfFrame();
		int index = Random.Range(0, _activeCoins.Count);
		CoinBase coin = _activeCoins[index];
		ShowGuideClickCoin(coin, OnGuideFingerClicked);
	}

	private void CalculateBoundary()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			Collider2D obj = sceneBox;
			BoxCollider2D val = (BoxCollider2D)(object)((obj is BoxCollider2D) ? obj : null);
			if ((Object)(object)val != (Object)null && (Object)(object)sceneBoxUI != (Object)null)
			{
				Rect rect = sceneBoxUI.rect;
				val.size = ((Rect)(ref rect)).size;
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning((object)("[MainScene] 动态计算场景边界时出错: " + ex.Message));
		}
		if ((Object)(object)sceneBox != (Object)null)
		{
			Bounds bounds = sceneBox.bounds;
			_boundaryMin = new Vector2(((Bounds)(ref bounds)).min.x, ((Bounds)(ref bounds)).min.y);
			_boundaryMax = new Vector2(((Bounds)(ref bounds)).max.x, ((Bounds)(ref bounds)).max.y);
		}
		else
		{
			_boundaryMin = new Vector2(-400f, -800f);
			_boundaryMax = new Vector2(400f, 0f);
		}
	}

	public void SetCoinAvoidRects(IReadOnlyList<Rect> avoidRects)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		_coinAvoidRects.Clear();
		if (enableCoinAvoidRects && avoidRects != null)
		{
			for (int i = 0; i < avoidRects.Count; i++)
			{
				_coinAvoidRects.Add(avoidRects[i]);
			}
		}
	}

	public void InitializeCoins()
	{
		if ((Object)(object)coinCopperPrefab == (Object)null)
		{
			Debug.LogError((object)"铜币预制体未赋值！");
			return;
		}
		if ((Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null && SingletonMono<UpgradeManager>.Instance.IsInitialized)
		{
			_copperValue = SingletonMono<UpgradeManager>.Instance.GetCopperCoinValue(0);
			_silverValue = SingletonMono<UpgradeManager>.Instance.GetSilverCoinValue(0);
			_goldValue = SingletonMono<UpgradeManager>.Instance.GetGoldCoinValue(0);
			_purpleValue = SingletonMono<UpgradeManager>.Instance.GetPurpleCoinValue(0);
			Debug.Log((object)$"[MainScene] 从配置表读取硬币价值 - 铜币: {_copperValue}, 银币: {_silverValue}, 金币: {_goldValue}, 紫币: {_purpleValue}");
		}
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		if ((Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null && SingletonMono<UpgradeManager>.Instance.IsInitialized)
		{
			num = SingletonMono<UpgradeManager>.Instance.GetUpgradeLevel(2) + initialCopperCount;
			num2 = SingletonMono<UpgradeManager>.Instance.GetUpgradeLevel(3);
			num3 = SingletonMono<UpgradeManager>.Instance.GetUpgradeLevel(4);
			num4 = SingletonMono<UpgradeManager>.Instance.GetUpgradeLevel(5);
			Debug.Log((object)$"[MainScene] 从升级系统读取硬币数量 - 铜币: {num}, 银币: {num2}, 金币: {num3}, 紫币: {num4}");
		}
		if (num <= 0)
		{
			num = SingletonMono<UpgradeManager>.Instance?.GetInitialCopperCount() ?? initialCopperCount;
			Debug.Log((object)$"[MainScene] 使用配置表初始铜币数量: {num}");
		}
		for (int i = 0; i < num; i++)
		{
			if (num <= 3)
			{
				SpawnCoin(CoinType.Copper, i);
			}
			else
			{
				SpawnCoin(CoinType.Copper);
			}
		}
		for (int j = 0; j < num2; j++)
		{
			SpawnCoin(CoinType.Silver);
		}
		for (int k = 0; k < num3; k++)
		{
			SpawnCoin(CoinType.Gold);
		}
		for (int l = 0; l < num4; l++)
		{
			SpawnCoin(CoinType.Purple);
		}
	}

	public CoinBase SpawnCoin(CoinType coinType, int index = -1)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		GameObject coinPrefab = GetCoinPrefab(coinType);
		if ((Object)(object)coinPrefab == (Object)null)
		{
			return null;
		}
		Vector2 val = GetRandomSpawnPosition();
		if (index >= 0 && spawnPositions != null && spawnPositions.Length > index)
		{
			val = spawnPositions[index];
		}
		Transform val2 = (Transform)(((Object)(object)coinPool != (Object)null) ? ((object)coinPool) : ((object)((Component)this).transform));
		GameObject coinObj = Object.Instantiate<GameObject>(coinPrefab, val2);
		CoinBase component = coinObj.GetComponent<CoinBase>();
		Action<GameObject> playEffectAction = delegate(GameObject effectPrefab)
		{
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)this != (Object)null && (Object)(object)coinObj != (Object)null)
			{
				GameObject fromHelperAddPool = GetFromHelperAddPool(effectPrefab, coinObj.transform);
				fromHelperAddPool.transform.localPosition = Vector3.zero;
				fromHelperAddPool.SetActive(true);
				SkeletonGraphic component2 = fromHelperAddPool.GetComponent<SkeletonGraphic>();
				if ((Object)(object)component2 != (Object)null && component2.AnimationState != null)
				{
					if (!string.IsNullOrEmpty(component2.startingAnimation))
					{
						component2.AnimationState.SetAnimation(0, component2.startingAnimation, false);
					}
					else if (component2.AnimationState.GetCurrent(0) != null && component2.AnimationState.GetCurrent(0).Animation != null)
					{
						component2.AnimationState.SetAnimation(0, component2.AnimationState.GetCurrent(0).Animation.Name, false);
					}
				}
			}
		};
		if ((Object)(object)_helperAddPrefab != (Object)null)
		{
			playEffectAction(_helperAddPrefab);
		}
		else
		{
			SingletonMono<ResManager>.Instance.LoadPrefabAsync("Assets/GameRes/Role/helperAdd.prefab", delegate(GameObject effectPrefab)
			{
				_helperAddPrefab = effectPrefab;
				playEffectAction(effectPrefab);
			});
		}
		if ((Object)(object)component != (Object)null)
		{
			component.SetMovementBoundary(_boundaryMin, _boundaryMax);
			component.OnCoinClicked = (Action<CoinBase>)Delegate.Combine(component.OnCoinClicked, new Action<CoinBase>(OnCoinClicked));
			component.SetHoverTriggerEnabled(enabled: true);
			component.OnFlipComplete = (Action<CoinBase, bool>)Delegate.Combine(component.OnFlipComplete, new Action<CoinBase, bool>(OnCoinFlipComplete));
			component.OnLanded = (Action<CoinBase>)Delegate.Combine(component.OnLanded, new Action<CoinBase>(OnCoinLanded));
			component.SetInitialPosition(val);
			_activeCoins.Add(component);
			InsertIntoSortedCache(component);
			SingletonMono<UpgradeManager>.Instance?.NotifyCoinCountChanged();
		}
		if (index != -1)
		{
			coinObj.GetComponent<RectTransform>().anchoredPosition = val;
		}
		return component;
	}

	private void InsertIntoSortedCache(CoinBase coin)
	{
		for (int i = 0; i < _sortedCoinsCache.Count; i++)
		{
			if (coin.coinType > _sortedCoinsCache[i].coinType)
			{
				_sortedCoinsCache.Insert(i, coin);
				return;
			}
		}
		_sortedCoinsCache.Add(coin);
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

	private Vector2 GetRandomSpawnPosition()
	{
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		int num = 50;
		Vector2 val = default(Vector2);
		for (int i = 0; i < num; i++)
		{
			float num2 = Random.Range(_boundaryMin.x + 50f, _boundaryMax.x - 50f);
			float num3 = Random.Range(_boundaryMin.y + 50f, _boundaryMax.y - 50f);
			((Vector2)(ref val))._002Ector(num2, num3);
			bool flag = false;
			foreach (CoinBase activeCoin in _activeCoins)
			{
				if ((Object)(object)activeCoin != (Object)null && Vector2.Distance(val, Vector2.op_Implicit(((Component)activeCoin).transform.position)) < minSpawnDistance)
				{
					flag = true;
					break;
				}
			}
			if (!flag && !IsInCoinAvoidRect(val))
			{
				return val;
			}
		}
		float num4 = Random.Range(_boundaryMin.x + 50f, _boundaryMax.x - 50f);
		float num5 = Random.Range(_boundaryMin.y + 50f, _boundaryMax.y - 50f);
		return PushOutFromCoinAvoidRects(new Vector2(num4, num5));
	}

	private void OnCoinClicked(CoinBase coin)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		Vector2 startPos = Vector2.op_Implicit(((Component)coin).transform.position);
		Vector2 randomLandingPosition = GetRandomLandingPosition(startPos);
		coin.JumpToTarget(startPos, randomLandingPosition, 0);
	}

	private Vector2 GetRandomLandingPosition(Vector2 startPos)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		float num = Random.Range(minLandingRadius, maxLandingRadius);
		Vector2 val = Random.insideUnitCircle;
		if (((Vector2)(ref val)).sqrMagnitude < 0.0001f)
		{
			val = Vector2.right;
		}
		for (int i = 0; i < 20; i++)
		{
			num = Random.Range(minLandingRadius, maxLandingRadius);
			val = Random.insideUnitCircle;
			if (((Vector2)(ref val)).sqrMagnitude < 0.0001f)
			{
				val = Vector2.right;
			}
			Vector2 val2 = ClampToSceneBounds(startPos + ((Vector2)(ref val)).normalized * num);
			if (!IsInCoinAvoidRect(val2))
			{
				return val2;
			}
		}
		Vector2 pos = startPos + ((Vector2)(ref val)).normalized * num;
		return PushOutFromCoinAvoidRects(ClampToSceneBounds(pos));
	}

	private Vector2 ClampToSceneBounds(Vector2 pos)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		float num = _boundaryMin.x + sceneBoundaryPadding;
		float num2 = _boundaryMax.x - sceneBoundaryPadding;
		float num3 = _boundaryMin.y + sceneBoundaryPadding;
		float num4 = _boundaryMax.y - sceneBoundaryPadding;
		return new Vector2(Mathf.Clamp(pos.x, num, num2), Mathf.Clamp(pos.y, num3, num4));
	}

	private bool IsInCoinAvoidRect(Vector2 pos)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		if (!enableCoinAvoidRects || _coinAvoidRects.Count == 0)
		{
			return false;
		}
		for (int i = 0; i < _coinAvoidRects.Count; i++)
		{
			Rect val = _coinAvoidRects[i];
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
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		if (!enableCoinAvoidRects || _coinAvoidRects.Count == 0)
		{
			return ClampToSceneBounds(pos);
		}
		Vector2 val = pos;
		for (int i = 0; i < _coinAvoidRects.Count; i++)
		{
			Rect val2 = _coinAvoidRects[i];
			if (((Rect)(ref val2)).Contains(val))
			{
				float num = Mathf.Abs(val.x - ((Rect)(ref val2)).xMin);
				float num2 = Mathf.Abs(((Rect)(ref val2)).xMax - val.x);
				float num3 = Mathf.Abs(val.y - ((Rect)(ref val2)).yMin);
				float num4 = Mathf.Abs(((Rect)(ref val2)).yMax - val.y);
				float num5 = Mathf.Min(new float[4] { num, num2, num3, num4 });
				if (num5 == num)
				{
					val.x = ((Rect)(ref val2)).xMin - 1f;
				}
				else if (num5 == num2)
				{
					val.x = ((Rect)(ref val2)).xMax + 1f;
				}
				else if (num5 == num3)
				{
					val.y = ((Rect)(ref val2)).yMin - 1f;
				}
				else
				{
					val.y = ((Rect)(ref val2)).yMax + 1f;
				}
			}
		}
		return ClampToSceneBounds(val);
	}

	private void OnCoinFlipComplete(CoinBase coin, bool isFront)
	{
		if (isFront)
		{
			double coinValue = GetCoinValue(coin.coinType, coin.roleType);
			SingletonMono<UpgradeManager>.Instance?.AddGold(coinValue);
		}
		else
		{
			double coinValue2 = GetCoinValue(coin.coinType, coin.roleType);
			coinValue2 *= (double)(this.coinFlipComplete?.Invoke() ?? 0f);
			SingletonMono<UpgradeManager>.Instance?.AddGold(coinValue2);
		}
	}

	private void OnCoinLanded(CoinBase coin)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)coin != (Object)null && IsInCoinAvoidRect(Vector2.op_Implicit(((Component)coin).transform.position)))
		{
			((Component)coin).transform.position = Vector2.op_Implicit(PushOutFromCoinAvoidRects(Vector2.op_Implicit(((Component)coin).transform.position)));
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

	private void ShowGuideStep2(CoinBase coin)
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
		int index = Random.Range(0, _activeCoins.Count);
		CoinBase coin2 = _activeCoins[index];
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

	private void step2(CoinBase coin)
	{
		if (PlayerPrefs.GetInt("guide_step_2", 0) != 0)
		{
			RectTransform obj = guideFinger;
			if (obj != null)
			{
				((Component)obj).gameObject.SetActive(false);
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

	private void TriggerChainReaction(CoinBase coin)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)SingletonMono<UpgradeManager>.Instance == (Object)null)
		{
			return;
		}
		foreach (CoinBase activeCoin in _activeCoins)
		{
			if ((Object)(object)activeCoin == (Object)null || activeCoin.State != CoinState.Idle || (Object)(object)activeCoin == (Object)(object)coin || Vector3.Distance(((Component)coin).transform.position, ((Component)activeCoin).transform.position) > 100f)
			{
				continue;
			}
			CoinType coinType = activeCoin.coinType;
			switch (coin.coinType)
			{
			case CoinType.Silver:
				if (activeCoin.coinType == CoinType.Copper && SingletonMono<UpgradeManager>.Instance.CanSilverTriggerCopper())
				{
					TriggerCoinThrow(activeCoin);
				}
				break;
			case CoinType.Gold:
				if (SingletonMono<UpgradeManager>.Instance.CanGoldTriggerCoin(coinType))
				{
					TriggerCoinThrow(activeCoin);
				}
				break;
			}
		}
	}

	private void TriggerLuckyEcho(CoinBase coin)
	{
		if (!((Object)(object)SingletonMono<UpgradeManager>.Instance == (Object)null))
		{
			float luckyEchoProbability = SingletonMono<UpgradeManager>.Instance.GetLuckyEchoProbability();
			if (luckyEchoProbability > 0f && Random.value < luckyEchoProbability && !coin.isLuckyEcho)
			{
				ScheduleLuckyEcho(coin, 0.3f);
				coin.isLuckyEcho = true;
			}
		}
	}

	private void ScheduleLuckyEcho(CoinBase coin, float delay)
	{
		if ((Object)(object)coin == (Object)null)
		{
			return;
		}
		for (int i = 0; i < _luckyEchoTasks.Count; i++)
		{
			if ((Object)(object)_luckyEchoTasks[i].Coin == (Object)(object)coin)
			{
				LuckyEchoTask value = _luckyEchoTasks[i];
				value.Remaining = delay;
				_luckyEchoTasks[i] = value;
				return;
			}
		}
		_luckyEchoTasks.Add(new LuckyEchoTask
		{
			Coin = coin,
			Remaining = delay
		});
	}

	private void TickLuckyEcho(float deltaTime)
	{
		for (int num = _luckyEchoTasks.Count - 1; num >= 0; num--)
		{
			LuckyEchoTask value = _luckyEchoTasks[num];
			if ((Object)(object)value.Coin == (Object)null)
			{
				_luckyEchoTasks.RemoveAt(num);
			}
			else
			{
				value.Remaining -= deltaTime;
				if (value.Remaining > 0f)
				{
					_luckyEchoTasks[num] = value;
				}
				else
				{
					_luckyEchoTasks.RemoveAt(num);
					if ((Object)(object)value.Coin != (Object)null && value.Coin.State == CoinState.Idle)
					{
						Debug.Log((object)$"[MainScene] 触发幸运回响: {value.Coin.coinType}");
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

	public void RemoveCoin(CoinBase coin)
	{
		if (!((Object)(object)coin == (Object)null))
		{
			coin.OnCoinClicked = (Action<CoinBase>)Delegate.Remove(coin.OnCoinClicked, new Action<CoinBase>(OnCoinClicked));
			coin.OnFlipComplete = (Action<CoinBase, bool>)Delegate.Remove(coin.OnFlipComplete, new Action<CoinBase, bool>(OnCoinFlipComplete));
			coin.OnLanded = (Action<CoinBase>)Delegate.Remove(coin.OnLanded, new Action<CoinBase>(OnCoinLanded));
			_activeCoins.Remove(coin);
			_sortedCoinsCache.Remove(coin);
			Object.Destroy((Object)(object)((Component)coin).gameObject);
		}
	}

	public void RemoveAllCoin()
	{
		CoinBase[] array = _activeCoins.ToArray();
		foreach (CoinBase coin in array)
		{
			RemoveCoin(coin);
		}
		_activeCoins.Clear();
		_sortedCoinsCache.Clear();
	}

	public void ClearRuntimeContent()
	{
		if (_roleSpawnCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(_roleSpawnCoroutine);
			_roleSpawnCoroutine = null;
		}
		_isSwipeTracking = false;
		_swipedCoinsThisFrame.Clear();
		_luckyEchoTasks.Clear();
		RemoveAllCoin();
		SingletonMono<RoleManager>.Instance?.RemoveAllRole();
		ClearPoolChildren((Transform)(object)rolePool);
		ClearPoolChildren((Transform)(object)coinPool);
		ClearPoolChildren((Transform)(object)effectPool);
		ClearPoolChildren((Transform)(object)huanxingPool);
		ClearPoolChildren((Transform)(object)TextPool);
		ClearPoolChildren((Transform)(object)coins_smokePool);
		ClearPoolChildren((Transform)(object)ParticlePool);
		_effectPoolDict.Clear();
		_helperAddPool.Clear();
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

	public Vector2 GetMostDenseCoinPosition(float gridSize = 100f)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		if (Time.frameCount == _lastDensePosFrame)
		{
			return _cachedDensePos;
		}
		if (_activeCoins == null || _activeCoins.Count == 0)
		{
			return Vector2.zero;
		}
		Dictionary<Vector2Int, List<CoinBase>> dictionary = new Dictionary<Vector2Int, List<CoinBase>>();
		Vector2Int key = default(Vector2Int);
		foreach (CoinBase activeCoin in _activeCoins)
		{
			if ((Object)(object)activeCoin != (Object)null && activeCoin.State == CoinState.Idle)
			{
				Vector2 val = Vector2.op_Implicit(((Component)activeCoin).transform.position);
				((Vector2Int)(ref key))._002Ector(Mathf.FloorToInt(val.x / gridSize), Mathf.FloorToInt(val.y / gridSize));
				if (!dictionary.ContainsKey(key))
				{
					dictionary[key] = new List<CoinBase>();
				}
				dictionary[key].Add(activeCoin);
			}
		}
		if (dictionary.Count == 0)
		{
			return Vector2.zero;
		}
		int num = 0;
		Vector2Int key2 = Vector2Int.zero;
		foreach (KeyValuePair<Vector2Int, List<CoinBase>> item in dictionary)
		{
			if (item.Value.Count > num)
			{
				num = item.Value.Count;
				key2 = item.Key;
			}
		}
		List<CoinBase> list = dictionary[key2];
		Vector2 val2 = Vector2.zero;
		foreach (CoinBase item2 in list)
		{
			val2 += Vector2.op_Implicit(((Component)item2).transform.position);
		}
		val2 = (_cachedDensePos = val2 / (float)list.Count);
		_lastDensePosFrame = Time.frameCount;
		return val2;
	}

	private void OnDestroy()
	{
		foreach (CoinBase activeCoin in _activeCoins)
		{
			if ((Object)(object)activeCoin != (Object)null)
			{
				activeCoin.OnCoinClicked = (Action<CoinBase>)Delegate.Remove(activeCoin.OnCoinClicked, new Action<CoinBase>(OnCoinClicked));
				activeCoin.OnFlipComplete = (Action<CoinBase, bool>)Delegate.Remove(activeCoin.OnFlipComplete, new Action<CoinBase, bool>(OnCoinFlipComplete));
				activeCoin.OnLanded = (Action<CoinBase>)Delegate.Remove(activeCoin.OnLanded, new Action<CoinBase>(OnCoinLanded));
			}
		}
		if (this.coinFlipComplete != null)
		{
			this.coinFlipComplete = null;
		}
		_activeCoins.Clear();
		_sortedCoinsCache.Clear();
		_helperAddPool.Clear();
	}

	public int GetActiveCoinCount()
	{
		return _activeCoins.Count;
	}

	public List<CoinBase> GetAllCoins()
	{
		return _activeCoins;
	}

	public List<CoinBase> GetCoinsSortedByValue()
	{
		return _sortedCoinsCache;
	}

	public Transform GetRolePool()
	{
		return (Transform)(object)rolePool;
	}

	public Transform GetCoinPool()
	{
		return (Transform)(object)coinPool;
	}

	public Transform GetEffectPool(string effectPoolName)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Expected O, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		if (_effectPoolDict.ContainsKey(effectPoolName))
		{
			return (Transform)(object)_effectPoolDict[effectPoolName];
		}
		GameObject val = new GameObject(effectPoolName);
		val.transform.SetParent((Transform)(object)effectPool, false);
		val.transform.localScale = Vector3.one;
		RectTransform orAddComponent = ExtendedMono.GetOrAddComponent<RectTransform>(val);
		orAddComponent.anchoredPosition = Vector2.zero;
		_effectPoolDict.Add(effectPoolName, orAddComponent);
		return (Transform)(object)orAddComponent;
	}

	public Transform GetTextPool()
	{
		return (Transform)(object)TextPool;
	}

	public Transform GetHuanXingPool()
	{
		return (Transform)(object)huanxingPool;
	}

	public Transform GetCoinsSmokePool()
	{
		return (Transform)(object)coins_smokePool;
	}

	public Transform GetHuanxingPool()
	{
		return (Transform)(object)huanxingPool;
	}

	public Transform GetParticlePool()
	{
		return (Transform)(object)ParticlePool;
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
		min = _boundaryMin;
		max = _boundaryMax;
	}

	public Vector2 GetRandomPositionInBoundary(float padding = 50f)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		float num = Random.Range(_boundaryMin.x + padding, _boundaryMax.x - padding);
		float num2 = Random.Range(_boundaryMin.y + padding, _boundaryMax.y - padding);
		return new Vector2(num, num2);
	}

	public void ShowGuideClickCoin(CoinBase coin, Action<CoinBase> onClicked, string dialogText = "点击抛起铜币", bool isShow = true)
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)coin == (Object)null) && !((Object)(object)guideFinger == (Object)null))
		{
			if (isShow)
			{
				UIManager.Instance.ShowDialogPanel(dialogText, DialogPosition.Bottom, 0.0001f);
			}
			coin.OnGuideFinger -= onClicked;
			coin.OnGuideFinger += onClicked;
			RectTransform component = ((Component)coin).GetComponent<RectTransform>();
			if ((Object)(object)component != (Object)null)
			{
				((Transform)guideFinger).position = ((Transform)component).position;
			}
			else
			{
				((Transform)guideFinger).position = ((Component)coin).transform.position;
			}
			((Component)guideFinger).gameObject.SetActive(true);
		}
	}

	private void OnGuideFingerClicked(CoinBase coin)
	{
		RectTransform obj = guideFinger;
		if (obj != null)
		{
			((Component)obj).gameObject.SetActive(false);
		}
		coin.OnGuideFinger -= OnGuideFingerClicked;
		UIManager.Instance.HideDialogPanel();
		Time.timeScale = 1f;
		PlayerPrefs.SetInt("guide_step_1", 1);
		PlayerPrefs.Save();
		GameDataManager.Instance.ForceUpdateNewGuide();
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = default(Vector3);
		if (!RectTransformUtility.ScreenPointToWorldPointInRectangle(sceneBoxUI, eventData.position, eventData.pressEventCamera, ref val))
		{
			return;
		}
		Vector2 val2 = default(Vector2);
		((Vector2)(ref val2))._002Ector(val.x, val.y);
		foreach (CoinBase activeCoin in _activeCoins)
		{
			if ((Object)(object)activeCoin != (Object)null && activeCoin.State == CoinState.Idle && Vector2.Distance(val2, Vector2.op_Implicit(((Component)activeCoin).transform.position)) <= 48f)
			{
				activeCoin.SetCoinClick();
			}
		}
	}

	public void AddGongfu()
	{
		if (isAddGongfu)
		{
			return;
		}
		SingletonMono<RoleManager>.Instance.SpawnRole(delegate(Role role)
		{
			isAddGongfu = true;
			PlayerPrefs.SetInt("gongfu_support_time", 1);
			PlayerPrefs.Save();
			_ = (Object)(object)role != (Object)null;
			DownItem downItem = UIManager.Instance?.GetDownItem();
			if ((Object)(object)downItem != (Object)null && (Object)(object)downItem.foldBtn != (Object)null && ((Component)downItem.foldBtn).gameObject.activeSelf)
			{
				downItem.AdSummonDisplay(isOpen: true);
			}
		}, 4);
	}

	public void AddDashe()
	{
		if (isAddDashe)
		{
			return;
		}
		SingletonMono<RoleManager>.Instance.SpawnRole(delegate
		{
			isAddDashe = true;
			PlayerPrefs.SetInt("dashe_support_time", 1);
			PlayerPrefs.Save();
			DownItem downItem = UIManager.Instance?.GetDownItem();
			if ((Object)(object)downItem != (Object)null && (Object)(object)downItem.foldBtn != (Object)null && ((Component)downItem.foldBtn).gameObject.activeSelf)
			{
				downItem.AdSummonDisplay(isOpen: true);
			}
		}, 6);
	}
}
