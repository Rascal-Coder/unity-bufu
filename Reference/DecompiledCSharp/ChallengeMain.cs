using System;
using System.Collections;
using System.Collections.Generic;
using Module.Role;
using Spine;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using cfg;

public class ChallengeMain : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	private struct LuckyEchoTask
	{
		public CoinBase Coin;

		public float Remaining;
	}

	private class SelectTime
	{
		public float displayTime;

		public bool isClamped;
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

	[Header("场景边界")]
	[SerializeField]
	private RectTransform sceneBoxUI;

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

	[Header("对象池")]
	[SerializeField]
	private RectTransform rolePool;

	[SerializeField]
	private RectTransform coinPool;

	[SerializeField]
	private RectTransform effectPool;

	[SerializeField]
	private RectTransform textPool;

	[Header("字体飘字场景中目前数量")]
	[SerializeField]
	private int TextCurrentCount;

	[Header("字体飘字场景中最大数量")]
	[SerializeField]
	public int TextMaxCount = 60;

	[Header("胜利弹窗的确定按键")]
	[SerializeField]
	private Button victoryBtn;

	[Header("胜利弹窗的广告按键")]
	[SerializeField]
	private Button victoryAd;

	[Header("失败弹窗的确定按键")]
	[SerializeField]
	private Button failBtn;

	[Header("失败弹窗的广告复活按键")]
	[SerializeField]
	private Button adFailBtn;

	[Header("胜利面板物体")]
	[SerializeField]
	private GameObject victoryPanel;

	[Header("胜利面板下创建奖励物体")]
	[SerializeField]
	private Transform victoryGrid;

	[Header("失败面板物体")]
	[SerializeField]
	private GameObject failPanel;

	[Header("退出挑战场景按键")]
	[SerializeField]
	private Button backBtn;

	private bool isRespawn;

	private Dictionary<string, RectTransform> _effectPoolDict = new Dictionary<string, RectTransform>();

	private List<CoinBase> _activeCoins = new List<CoinBase>();

	private Vector2 _boundaryMin;

	private Vector2 _boundaryMax;

	private double _copperValue = 1.0;

	private double _silverValue = 5.0;

	private double _goldValue = 10.0;

	private double _purpleValue = 15.0;

	private HashSet<CoinBase> _swipedCoinsThisFrame = new HashSet<CoinBase>();

	private Vector2 _swipeStartPos;

	private bool _isSwipeTracking;

	[Header("挑战进度条")]
	[SerializeField]
	private Image progress;

	[Header("进度条跟随")]
	[SerializeField]
	private RectTransform follow;

	[Header("目前收集金钱提示文本")]
	[SerializeField]
	private TextMeshProUGUI sum;

	[Header("剩余时间文本显示")]
	[SerializeField]
	private TextMeshProUGUI time;

	[Header("升级项父物体")]
	[SerializeField]
	private Transform upgradeCont;

	private double collectCoin;

	private double targetCoin;

	private float leftTime;

	private float skillTime;

	private Vector2 position;

	private List<int> rewardType = new List<int>();

	private List<double> rewardCount = new List<double>();

	private bool _challengeCompleted;

	private List<ChallengeUpgradeItem> _upgradeItems = new List<ChallengeUpgradeItem>();

	private int _silverCountLevel;

	private int _silverValueLevel;

	private int _goldCountLevel;

	private int _goldValueLevel;

	private int _silverExtraCount;

	private int _goldExtraCount;

	private float _silverValueMultiplier = 1f;

	private float _goldValueMultiplier = 1f;

	public static ChallengeMain Instance;

	private List<LuckyEchoTask> _luckyEchoTasks = new List<LuckyEchoTask>();

	private GameObject _helperAddPrefab;

	private List<GameObject> _helperAddPool = new List<GameObject>();

	private List<CoinBase> _sortedCoinsCache = new List<CoinBase>();

	private Vector2 _cachedDensePos = Vector2.zero;

	private int _lastDensePosFrame = -1;

	private challenge _currentChallengeConfig;

	private List<Role> _activeRoles = new List<Role>();

	[Header("三选一技能物体面板")]
	[SerializeField]
	private GameObject selectSkillPanel;

	[Header("技能物体生成父物体")]
	[SerializeField]
	private Transform skillCont;

	[Header("技能预制体")]
	[SerializeField]
	private GameObject selectSkill;

	private List<SelectTime> selectTime;

	private List<SelectSkill> skillList;

	private float multiplyBuff = 1f;

	private Vector2[] spawnPositions = (Vector2[])(object)new Vector2[3]
	{
		new Vector2(-95f, 295f),
		new Vector2(-87f, -200f),
		new Vector2(213f, -150f)
	};

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

	private void PlaySpawnEffect(Transform target)
	{
		if ((Object)(object)target == (Object)null)
		{
			return;
		}
		Action<GameObject> playEffect = delegate(GameObject effectPrefab)
		{
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)this != (Object)null && (Object)(object)target != (Object)null)
			{
				GameObject fromHelperAddPool = GetFromHelperAddPool(effectPrefab, target);
				fromHelperAddPool.transform.localPosition = Vector3.zero;
				fromHelperAddPool.SetActive(true);
				SkeletonGraphic component = fromHelperAddPool.GetComponent<SkeletonGraphic>();
				if ((Object)(object)component != (Object)null && component.AnimationState != null)
				{
					if (!string.IsNullOrEmpty(component.startingAnimation))
					{
						component.AnimationState.SetAnimation(0, component.startingAnimation, false);
					}
					else if (component.AnimationState.GetCurrent(0) != null && component.AnimationState.GetCurrent(0).Animation != null)
					{
						component.AnimationState.SetAnimation(0, component.AnimationState.GetCurrent(0).Animation.Name, false);
					}
				}
			}
		};
		if ((Object)(object)_helperAddPrefab != (Object)null)
		{
			playEffect(_helperAddPrefab);
			return;
		}
		SingletonMono<ResManager>.Instance.LoadPrefabAsync("Assets/GameRes/Role/helperAdd.prefab", delegate(GameObject effectPrefab)
		{
			_helperAddPrefab = effectPrefab;
			playEffect(effectPrefab);
		});
	}

	private void Awake()
	{
		if ((Object)(object)Instance != (Object)null)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
		else
		{
			Instance = this;
		}
		CalculateBoundary();
		selectTime = new List<SelectTime>();
		skillList = new List<SelectSkill>();
	}

	private void Start()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		BtnInit();
		if ((Object)(object)bgImage != (Object)null)
		{
			LoadBgImage(GameDataManager.Instance.GetCurrentDeskId());
		}
		position = Vector2.zero;
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
		Follow();
		TimeCalculate();
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
		((MonoBehaviour)this).StartCoroutine(SpawnChallengeRoles());
	}

	private IEnumerator SpawnChallengeRoles()
	{
		if ((Object)(object)ChallengeDataManager.Instance?.challengePanel == (Object)null)
		{
			Debug.LogWarning((object)"[ChallengeMain] ChallengeDataManager.Instance.challengePanel 为空，无法生成挑战帮手");
			yield break;
		}
		Dictionary<int, int> battleHelperCounts = ChallengeDataManager.Instance.challengePanel.GetBattleHelperCounts();
		if (battleHelperCounts == null || battleHelperCounts.Count == 0)
		{
			Debug.LogWarning((object)"[ChallengeMain] 没有配置出战帮手");
			yield break;
		}
		foreach (KeyValuePair<int, int> item in battleHelperCounts)
		{
			int helperId = item.Key;
			int count = item.Value;
			if (count <= 0)
			{
				continue;
			}
			string prefabPath = Constants.GetRolePrefabPath(helperId);
			GameObject prefab = null;
			bool loaded = false;
			SingletonMono<ResManager>.Instance.LoadPrefabAsync(prefabPath, delegate(GameObject p)
			{
				prefab = p;
				loaded = true;
			});
			yield return (object)new WaitUntil((Func<bool>)(() => loaded));
			if ((Object)(object)prefab == (Object)null)
			{
				Debug.LogError((object)("[ChallengeMain] 帮手预制体加载失败: " + prefabPath));
				continue;
			}
			for (int i = 0; i < count; i++)
			{
				Vector2 randomPositionInBoundary = GetRandomPositionInBoundary(80f);
				GameObject val = Object.Instantiate<GameObject>(prefab, Vector2.op_Implicit(randomPositionInBoundary), Quaternion.identity, (Transform)(object)rolePool);
				PlaySpawnEffect(val.transform);
				Role role = val.GetComponent<Role>();
				if ((Object)(object)role == (Object)null)
				{
					role = val.AddComponent<Role>();
				}
				role.SetChallengeMode(enable: true, this);
				role.Initialize(helperId, _boundaryMin, _boundaryMax);
				_activeRoles.Add(role);
				yield return (object)new WaitForSeconds(0.2f);
			}
		}
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
		if (_currentChallengeConfig == null)
		{
			int id = GameDataManager.Instance?.GetChallengeLevel() ?? 0;
			_currentChallengeConfig = ConfigManager.Instance.GetChallengeCofig(id);
		}
		if (_currentChallengeConfig == null)
		{
			Debug.LogError((object)"[ChallengeMain] 未找到挑战关卡配置");
			yield break;
		}
		InitializeCoins();
		InitializeRoles();
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
			Debug.LogWarning((object)("[ChallengeMain] 动态计算场景边界时出错: " + ex.Message));
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

	public void InitializeCoins()
	{
		if ((Object)(object)coinCopperPrefab == (Object)null)
		{
			Debug.LogError((object)"铜币预制体未赋值！");
			return;
		}
		if (_currentChallengeConfig == null)
		{
			Debug.LogWarning((object)"[ChallengeMain] 未找到挑战配置，无法生成硬币");
			return;
		}
		CoinType coinTypeFromConfig = GetCoinTypeFromConfig(_currentChallengeConfig.CoinType1);
		int num = _currentChallengeConfig.CoinNum1;
		if (coinTypeFromConfig == CoinType.Silver)
		{
			num += _silverExtraCount;
		}
		if (coinTypeFromConfig == CoinType.Gold)
		{
			num += _goldExtraCount;
		}
		for (int i = 0; i < num; i++)
		{
			SpawnCoin(coinTypeFromConfig);
		}
		CoinType coinTypeFromConfig2 = GetCoinTypeFromConfig(_currentChallengeConfig.CoinType2);
		int num2 = _currentChallengeConfig.CoinNum2;
		if (coinTypeFromConfig2 == CoinType.Silver)
		{
			num2 += _silverExtraCount;
		}
		if (coinTypeFromConfig2 == CoinType.Gold)
		{
			num2 += _goldExtraCount;
		}
		for (int j = 0; j < num2; j++)
		{
			SpawnCoin(coinTypeFromConfig2);
		}
	}

	public CoinBase SpawnCoin(CoinType coinType, int index = -1)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
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
		GameObject val3 = Object.Instantiate<GameObject>(coinPrefab, val2);
		CoinBase component = val3.GetComponent<CoinBase>();
		PlaySpawnEffect(val3.transform);
		if ((Object)(object)component != (Object)null)
		{
			component.SetMovementBoundary(_boundaryMin, _boundaryMax);
			component.OnCoinClicked = (Action<CoinBase>)Delegate.Combine(component.OnCoinClicked, new Action<CoinBase>(OnCoinClicked));
			component.SetHoverTriggerEnabled(enabled: true);
			component.OnFlipComplete = (Action<CoinBase, bool>)Delegate.Combine(component.OnFlipComplete, new Action<CoinBase, bool>(OnCoinFlipComplete));
			component.OnLanded = (Action<CoinBase>)Delegate.Combine(component.OnLanded, new Action<CoinBase>(OnCoinLanded));
			component.SetChallengeMode(enable: true);
			component.SetChallengeCoinValue(GetChallengeCoinValue(coinType));
			component.SetInitialPosition(val);
			_activeCoins.Add(component);
			InsertIntoSortedCache(component);
		}
		if (index != -1)
		{
			val3.GetComponent<RectTransform>().anchoredPosition = val;
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

	private CoinType GetCoinTypeFromConfig(int configCoinType)
	{
		return configCoinType switch
		{
			1 => CoinType.Copper, 
			2 => CoinType.Silver, 
			3 => CoinType.Gold, 
			4 => CoinType.Purple, 
			_ => CoinType.Copper, 
		};
	}

	private Vector2 GetRandomSpawnPosition()
	{
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
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
			if (!flag)
			{
				return val;
			}
		}
		float num4 = Random.Range(_boundaryMin.x + 50f, _boundaryMax.x - 50f);
		float num5 = Random.Range(_boundaryMin.y + 50f, _boundaryMax.y - 50f);
		return new Vector2(num4, num5);
	}

	public void OnCoinClicked(CoinBase coin)
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

	public Vector2 GetRandomLandingPosition(Vector2 startPos)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		float num = Random.Range(minLandingRadius, maxLandingRadius);
		Vector2 val = Random.insideUnitCircle;
		if (((Vector2)(ref val)).sqrMagnitude < 0.0001f)
		{
			val = Vector2.right;
		}
		Vector2 pos = startPos + ((Vector2)(ref val)).normalized * num;
		return ClampToSceneBounds(pos);
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

	private void OnCoinFlipComplete(CoinBase coin, bool isFront)
	{
		if (isFront)
		{
			double challengeCoinValue = GetChallengeCoinValue(coin.coinType);
			ChallengeAddCoin(challengeCoinValue);
		}
	}

	private void OnCoinLanded(CoinBase coin)
	{
		TriggerChainReaction(coin);
		if (!coin.isLuckyEcho)
		{
			TriggerLuckyEcho(coin);
		}
		else
		{
			coin.isLuckyEcho = false;
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
						TriggerCoinThrow(value.Coin);
					}
				}
			}
		}
	}

	public double GetChallengeCoinValue(CoinType coinType)
	{
		List<challenge> allChallenge = ConfigManager.Instance.GetAllChallenge();
		double[] array = new double[4] { 1.0, 1.0, 1.0, 1.0 };
		foreach (challenge item in allChallenge)
		{
			switch (item.CoinType1)
			{
			case 1:
				array[0] = item.CoinBaseValue1;
				break;
			case 2:
				array[1] = item.CoinBaseValue1;
				break;
			case 3:
				array[2] = item.CoinBaseValue1;
				break;
			case 4:
				array[3] = item.CoinBaseValue1;
				break;
			}
			switch (item.CoinType2)
			{
			case 1:
				array[0] = item.CoinBaseValue2;
				break;
			case 2:
				array[1] = item.CoinBaseValue2;
				break;
			case 3:
				array[2] = item.CoinBaseValue2;
				break;
			case 4:
				array[3] = item.CoinBaseValue2;
				break;
			}
		}
		return coinType switch
		{
			CoinType.Copper => array[0] * (double)multiplyBuff, 
			CoinType.Silver => array[1] * (double)_silverValueMultiplier * (double)multiplyBuff, 
			CoinType.Gold => array[2] * (double)_goldValueMultiplier * (double)multiplyBuff, 
			CoinType.Purple => array[3] * (double)multiplyBuff, 
			_ => 1.0, 
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
		_activeCoins.Clear();
		_sortedCoinsCache.Clear();
		_helperAddPool.Clear();
		foreach (Role activeRole in _activeRoles)
		{
			if ((Object)(object)activeRole != (Object)null)
			{
				Object.Destroy((Object)(object)((Component)activeRole).gameObject);
			}
		}
		_activeRoles.Clear();
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
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)textPool != (Object)null)
		{
			return (Transform)(object)textPool;
		}
		GameObject val = new GameObject("Textpool");
		val.transform.SetParent(((Component)this).transform, false);
		val.transform.localScale = Vector3.one;
		textPool = val.GetOrAddComponent<RectTransform>();
		textPool.anchoredPosition = Vector2.zero;
		return (Transform)(object)textPool;
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

	private void Follow()
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)progress != (Object)null && (Object)(object)follow != (Object)null)
		{
			position.x = -360f + 720f * progress.fillAmount / 1f;
			follow.anchoredPosition = position;
		}
	}

	public void ChallengeInit()
	{
		DestoryCoinAndRole();
		((MonoBehaviour)this).StopAllCoroutines();
		SetMultiplyBuff(1f);
		collectCoin = 0.0;
		_challengeCompleted = false;
		isRespawn = false;
		_silverCountLevel = 0;
		_silverValueLevel = 0;
		_goldCountLevel = 0;
		_goldValueLevel = 0;
		_silverExtraCount = 0;
		_goldExtraCount = 0;
		_silverValueMultiplier = 1f;
		_goldValueMultiplier = 1f;
		LoadChallengeConfig();
		InitializeUpgradeItems();
		((MonoBehaviour)this).StartCoroutine(InitializeWithConfig());
		int challengeLevel = GameDataManager.Instance.GetChallengeLevel();
		challenge challengeCofig = ConfigManager.Instance.GetChallengeCofig(challengeLevel);
		targetCoin = 0.0;
		leftTime = 0f;
		skillTime = 0f;
		if (challengeCofig != null)
		{
			targetCoin = challengeCofig.LimitGoalFactor;
			if (targetCoin <= 0.0)
			{
				targetCoin = 1.0;
			}
			leftTime = challengeCofig.LimitTIme;
		}
		RefrshUI();
		if (this.selectTime == null)
		{
			return;
		}
		if (this.selectTime.Count <= 0)
		{
			for (int i = 0; i < 2; i++)
			{
				SelectTime selectTime = new SelectTime();
				selectTime.displayTime = 20 + i * 20;
				selectTime.isClamped = false;
				this.selectTime.Add(selectTime);
			}
		}
		else
		{
			for (int j = 0; j < this.selectTime.Count; j++)
			{
				this.selectTime[j].displayTime = 20 + j * 20;
				this.selectTime[j].isClamped = false;
			}
		}
	}

	private void TimeCalculate()
	{
		if (leftTime > 0f)
		{
			leftTime -= Time.deltaTime;
			skillTime += Time.deltaTime;
		}
		else if (leftTime <= 0f)
		{
			IsCompliteChallenge(isComplite: false);
		}
		int num = 0;
		int num2 = 0;
		if (leftTime >= 60f)
		{
			num = (int)leftTime / 60;
			num2 = Mathf.CeilToInt(leftTime % 60f);
		}
		else
		{
			num = 0;
			num2 = Mathf.CeilToInt(leftTime);
		}
		((TMP_Text)time).text = num.ToString("00") + ":" + num2.ToString("00");
		if (selectTime == null || selectTime.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < selectTime.Count; i++)
		{
			if (skillTime >= selectTime[i].displayTime && !selectTime[i].isClamped)
			{
				selectTime[i].isClamped = true;
				OpenSkillSelect();
			}
		}
	}

	public void ChallengeAddCoin(double count)
	{
		if (((Component)this).gameObject.activeSelf && !_challengeCompleted)
		{
			collectCoin += count;
			if (collectCoin >= targetCoin)
			{
				collectCoin = targetCoin;
				IsCompliteChallenge(isComplite: true);
			}
			RefrshUI();
			RefreshUpgradeItems();
		}
	}

	public void RefrshUI()
	{
		progress.fillAmount = (float)(collectCoin / targetCoin);
		((TMP_Text)sum).text = $"<color=#40FE84>{NumberFormatter.FormatNumber(collectCoin)}</color>" + "/" + NumberFormatter.FormatNumber(targetCoin);
	}

	public void IsCompliteChallenge(bool isComplite)
	{
		if (isComplite)
		{
			if (_challengeCompleted)
			{
				return;
			}
			_challengeCompleted = true;
			if ((Object)(object)GameDataManager.Instance != (Object)null && (Object)(object)ConfigManager.Instance != (Object)null)
			{
				int challengeLevel = GameDataManager.Instance.GetChallengeLevel();
				challenge challengeCofig = ConfigManager.Instance.GetChallengeCofig(challengeLevel);
				double count = 0.0;
				if (challengeCofig != null)
				{
					count = ((challengeCofig.RewardType != 1) ? challengeCofig.RewardNum : (challengeCofig.RewardNum * SingletonMono<UpgradeManager>.Instance.GetAverageGoldPerMinuteFromLast30Seconds()));
				}
				GetSuipian(challengeCofig.RewardType, count);
				ChallengeVictory(rewardType, rewardCount);
				int num = challengeLevel + 1;
				int count2 = ConfigManager.Instance.GetAllChallenge().Count;
				if (num > count2)
				{
					num = count2;
				}
				GameDataManager.Instance.SetChallengeLevel(num);
			}
		}
		else if (!_challengeCompleted)
		{
			ChallengeFail();
		}
	}

	public void GetSuipian(int Id, double count)
	{
		rewardType.Clear();
		rewardCount.Clear();
		if (Id == 3 || Id == 4 || Id == 5)
		{
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			for (int i = 0; (double)i < count; i++)
			{
				int key = ConfigManager.Instance.TransferSuipianRealId(Id);
				if (dictionary.ContainsKey(key))
				{
					dictionary[key]++;
				}
				else
				{
					dictionary[key] = 1;
				}
			}
			{
				foreach (KeyValuePair<int, int> item in dictionary)
				{
					rewardType.Add(item.Key);
					rewardCount.Add(item.Value);
				}
				return;
			}
		}
		rewardType.Add(Id);
		rewardCount.Add(count);
	}

	public void BtnInit()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Expected O, but got Unknown
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Expected O, but got Unknown
		((UnityEvent)backBtn.onClick).AddListener((UnityAction)delegate
		{
			if ((Object)(object)ChallengeDataManager.Instance?.challengePanel != (Object)null)
			{
				ChallengeDataManager.Instance.challengePanel.OffChallengeScene();
				DestoryCoinAndRole();
			}
		});
		((UnityEvent)victoryBtn.onClick).AddListener((UnityAction)delegate
		{
			GetReward(1);
			Time.timeScale = 1f;
			if ((Object)(object)ChallengeDataManager.Instance?.challengePanel != (Object)null)
			{
				ChallengeDataManager.Instance.challengePanel.OffChallengeScene();
				DestoryCoinAndRole();
			}
		});
		((UnityEvent)failBtn.onClick).AddListener((UnityAction)delegate
		{
			failPanel.SetActive(false);
			Time.timeScale = 1f;
			if ((Object)(object)ChallengeDataManager.Instance?.challengePanel != (Object)null)
			{
				ChallengeDataManager.Instance.challengePanel.OffChallengeScene();
				DestoryCoinAndRole();
			}
		});
		((UnityEvent)adFailBtn.onClick).AddListener((UnityAction)delegate
		{
			FakeVideoManager.Instance.ShowAd(delegate
			{
				isRespawn = true;
				leftTime = 20f;
				Time.timeScale = 1f;
				failPanel.SetActive(false);
			});
		});
		((UnityEvent)victoryAd.onClick).AddListener((UnityAction)delegate
		{
			FakeVideoManager.Instance.ShowAd(delegate
			{
				GetReward(2);
				Time.timeScale = 1f;
				if ((Object)(object)ChallengeDataManager.Instance?.challengePanel != (Object)null)
				{
					ChallengeDataManager.Instance.challengePanel.OffChallengeScene();
					DestoryCoinAndRole();
				}
			}, delegate
			{
			}, "限时挑战奖励双倍");
		});
	}

	public void ChallengeVictory(List<int> itemId, List<double> itemCount)
	{
		for (int num = victoryGrid.childCount - 1; num >= 0; num--)
		{
			Object.Destroy((Object)(object)((Component)victoryGrid.GetChild(num)).gameObject);
		}
		victoryPanel.SetActive(true);
		for (int i = 0; i < itemId.Count; i++)
		{
			int index2 = i;
			SingletonMono<ResManager>.Instance.LoadPrefabAsync("Assets/GameRes/NewUI/Common/prefabs/rewardKuang.prefab", delegate(GameObject prefab)
			{
				GameObject val = Object.Instantiate<GameObject>(prefab, victoryGrid);
				Image itemIcon = ((Component)val.transform.Find("itemIcon")).GetComponent<Image>();
				TextMeshProUGUI component = ((Component)val.transform.Find("countTmp")).GetComponent<TextMeshProUGUI>();
				if (itemId[index2] == 1)
				{
					SingletonMono<ResManager>.Instance.LoadSpriteAsync(Constants.GetMoneyPath(1), delegate(Sprite sprite)
					{
						if ((Object)(object)sprite != (Object)null)
						{
							itemIcon.sprite = sprite;
							((Graphic)itemIcon).SetNativeSize();
						}
					}, isAtlas: false);
				}
				else if (itemId[index2] == 2)
				{
					SingletonMono<ResManager>.Instance.LoadSpriteAsync(Constants.GetMoneyPath(2), delegate(Sprite sprite)
					{
						if ((Object)(object)sprite != (Object)null)
						{
							itemIcon.sprite = sprite;
							((Graphic)itemIcon).SetNativeSize();
						}
					}, isAtlas: false);
				}
				else if (itemId[index2] == 6)
				{
					SingletonMono<ResManager>.Instance.LoadSpriteAsync(Constants.GetSuipianIconPath(6), delegate(Sprite sprite)
					{
						if ((Object)(object)sprite != (Object)null)
						{
							itemIcon.sprite = sprite;
							((Graphic)itemIcon).SetNativeSize();
						}
					}, isAtlas: false);
				}
				else
				{
					SingletonMono<ResManager>.Instance.LoadSpriteAsync(Constants.GetSuipianIconPath(itemId[index2]), delegate(Sprite sprite)
					{
						if ((Object)(object)sprite != (Object)null)
						{
							itemIcon.sprite = sprite;
							((Graphic)itemIcon).SetNativeSize();
						}
					});
				}
				if ((Object)(object)component != (Object)null)
				{
					((TMP_Text)component).text = NumberFormatter.FormatNumber(itemCount[index2]);
				}
			});
		}
		Time.timeScale = 0f;
	}

	public void ChallengeFail()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		failPanel.SetActive(true);
		if (!isRespawn)
		{
			Transform transform = ((Component)failBtn).transform;
			((RectTransform)((transform is RectTransform) ? transform : null)).anchoredPosition = new Vector2(-325f, -862f);
			Transform transform2 = ((Component)adFailBtn).transform;
			((RectTransform)((transform2 is RectTransform) ? transform2 : null)).anchoredPosition = new Vector2(325f, -862f);
			((Component)adFailBtn).gameObject.SetActive(true);
		}
		else
		{
			Transform transform3 = ((Component)failBtn).transform;
			((RectTransform)((transform3 is RectTransform) ? transform3 : null)).anchoredPosition = new Vector2(0f, -862f);
			((Component)adFailBtn).gameObject.SetActive(false);
		}
		Time.timeScale = 0f;
	}

	private void LoadChallengeConfig()
	{
		int num = GameDataManager.Instance?.GetChallengeLevel() ?? 0;
		if (num > 0 && ConfigManager.HasInstance() && ConfigManager.isInitialized)
		{
			_currentChallengeConfig = ConfigManager.Instance.GetChallengeCofig(num);
		}
	}

	private void InitializeUpgradeItems()
	{
		if ((Object)(object)upgradeCont != (Object)null)
		{
			for (int num = upgradeCont.childCount - 1; num >= 0; num--)
			{
				Object.Destroy((Object)(object)((Component)upgradeCont.GetChild(num)).gameObject);
			}
		}
		_upgradeItems.Clear();
		if ((Object)(object)upgradeCont == (Object)null)
		{
			return;
		}
		SingletonMono<ResManager>.Instance.LoadPrefabAsync("Assets/GameRes/NewUI/Challenge/prefabs/challengeUpgradeItem.prefab", delegate(GameObject prefab)
		{
			if (!((Object)(object)prefab == (Object)null))
			{
				ChallengeUpgradeType[] array = new ChallengeUpgradeType[4]
				{
					ChallengeUpgradeType.SilverCount,
					ChallengeUpgradeType.SilverValue,
					ChallengeUpgradeType.GoldCount,
					ChallengeUpgradeType.GoldValue
				};
				for (int i = 0; i < array.Length; i++)
				{
					GameObject val = Object.Instantiate<GameObject>(prefab, upgradeCont);
					ChallengeUpgradeItem challengeUpgradeItem = val.GetComponent<ChallengeUpgradeItem>();
					if ((Object)(object)challengeUpgradeItem == (Object)null)
					{
						challengeUpgradeItem = val.AddComponent<ChallengeUpgradeItem>();
					}
					challengeUpgradeItem.Initialize(array[i], this);
					_upgradeItems.Add(challengeUpgradeItem);
				}
			}
		});
	}

	public void RefreshUpgradeItems()
	{
		for (int i = 0; i < _upgradeItems.Count; i++)
		{
			_upgradeItems[i]?.RefreshUI();
		}
	}

	public double GetCollectCoin()
	{
		return collectCoin;
	}

	public void SpendCollectCoin(double amount)
	{
		collectCoin -= amount;
		if (collectCoin < 0.0)
		{
			collectCoin = 0.0;
		}
		RefrshUI();
	}

	public int GetUpgradeLevel(ChallengeUpgradeType type)
	{
		return type switch
		{
			ChallengeUpgradeType.SilverCount => _silverCountLevel, 
			ChallengeUpgradeType.SilverValue => _silverValueLevel, 
			ChallengeUpgradeType.GoldCount => _goldCountLevel, 
			ChallengeUpgradeType.GoldValue => _goldValueLevel, 
			_ => 0, 
		};
	}

	public void SetUpgradeLevel(ChallengeUpgradeType type, int level)
	{
		challenge currentChallengeConfig = _currentChallengeConfig;
		switch (type)
		{
		case ChallengeUpgradeType.SilverCount:
			_silverCountLevel = level;
			_silverExtraCount = level;
			break;
		case ChallengeUpgradeType.SilverValue:
		{
			_silverValueLevel = level;
			float num2 = currentChallengeConfig?.CoinBuff1 ?? 1.1f;
			_silverValueMultiplier = Mathf.Pow(num2, (float)level);
			UpdateCoinsValueByType(CoinType.Silver);
			break;
		}
		case ChallengeUpgradeType.GoldCount:
			_goldCountLevel = level;
			_goldExtraCount = level;
			break;
		case ChallengeUpgradeType.GoldValue:
		{
			_goldValueLevel = level;
			float num = currentChallengeConfig?.CoinBuff2 ?? 1.1f;
			_goldValueMultiplier = Mathf.Pow(num, (float)level);
			UpdateCoinsValueByType(CoinType.Gold);
			break;
		}
		}
	}

	public double GetUpgradeCost(ChallengeUpgradeType type, int level)
	{
		challenge currentChallengeConfig = _currentChallengeConfig;
		if (currentChallengeConfig == null)
		{
			return 0.0;
		}
		int num;
		List<int> list;
		if (type != ChallengeUpgradeType.SilverCount)
		{
			num = ((type == ChallengeUpgradeType.SilverValue) ? 1 : 0);
			if (num == 0)
			{
				list = currentChallengeConfig.CostMoneyLevel2;
				goto IL_002f;
			}
		}
		else
		{
			num = 1;
		}
		list = currentChallengeConfig.CostMoneyLevel1;
		goto IL_002f;
		IL_002f:
		List<int> list2 = list;
		List<int> list3 = ((num != 0) ? currentChallengeConfig.BaseCostMoney1 : currentChallengeConfig.BaseCostMoney2);
		List<float> list4 = ((num != 0) ? currentChallengeConfig.CostMoneyBuff1 : currentChallengeConfig.CostMoneyBuff2);
		if (list2 == null || list2.Count == 0)
		{
			return 0.0;
		}
		if (list3 == null || list3.Count == 0)
		{
			return 0.0;
		}
		int num2 = 0;
		for (int i = 0; i < list2.Count; i++)
		{
			if (level <= list2[i])
			{
				num2 = i;
				break;
			}
			num2 = i;
		}
		double num3 = list3[0];
		double num4 = ((list4 != null && num2 < list4.Count) ? ((double)list4[num2]) : 1.0);
		return num3 * (double)Mathf.Pow((float)num4, (float)level);
	}

	public void UpdateCoinsValueByType(CoinType coinType)
	{
		double challengeCoinValue = GetChallengeCoinValue(coinType);
		for (int i = 0; i < _activeCoins.Count; i++)
		{
			CoinBase coinBase = _activeCoins[i];
			if ((Object)(object)coinBase != (Object)null && coinBase.coinType == coinType)
			{
				coinBase.SetChallengeCoinValue(challengeCoinValue);
			}
		}
	}

	private void GetReward(int multiple)
	{
		victoryPanel.SetActive(false);
		if (rewardType.Count <= 0 || rewardCount.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < rewardType.Count; i++)
		{
			if (rewardType[i] == 1)
			{
				SingletonMono<UpgradeManager>.Instance?.AddGold(rewardCount[i] * (double)multiple);
			}
			else if (rewardType[i] == 2)
			{
				GameDataManager.Instance?.AddDiamond((long)rewardCount[i] * multiple);
			}
			else if (rewardType[i] == 6)
			{
				GameDataManager.Instance.ChangeTalentPoint(isAdd: true, (int)rewardCount[i] * multiple);
			}
			else
			{
				GameDataManager.Instance.AddItem(rewardType[i], (long)rewardCount[i] * multiple);
			}
		}
		UIManager.Instance.ShowCoinDimondTween(rewardType);
	}

	public void DestoryCoinAndRole()
	{
		RemoveAllCoin();
		if ((Object)(object)coinPool != (Object)null)
		{
			for (int num = ((Transform)coinPool).childCount - 1; num >= 0; num--)
			{
				Object.Destroy((Object)(object)((Component)((Transform)coinPool).GetChild(num)).gameObject);
			}
		}
		foreach (Role activeRole in _activeRoles)
		{
			if ((Object)(object)activeRole != (Object)null)
			{
				Object.Destroy((Object)(object)((Component)activeRole).gameObject);
			}
		}
		for (int i = 0; i < ((Transform)textPool).childCount; i++)
		{
			Object.Destroy((Object)(object)((Component)((Transform)textPool).GetChild(i)).gameObject);
		}
		_activeRoles.Clear();
		((MonoBehaviour)this).StopAllCoroutines();
	}

	private void OpenSkillSelect()
	{
		if (skillList != null)
		{
			if (skillList.Count <= 0)
			{
				for (int i = 0; i < 3; i++)
				{
					SelectSkill component = ((Component)Object.Instantiate<GameObject>(selectSkill, skillCont).transform).GetComponent<SelectSkill>();
					if ((Object)(object)component != (Object)null)
					{
						component.Init("技能" + (i + 1), "技能可增加硬币价值", SkillFunction.UpgradeCoinValue);
						skillList.Add(component);
					}
				}
			}
			else
			{
				for (int j = 0; j < skillList.Count; j++)
				{
					skillList[j].Init("技能" + (j + 1), "技能可增加硬币价值+20%", SkillFunction.UpgradeCoinValue);
				}
			}
		}
		Time.timeScale = 0f;
		selectSkillPanel.SetActive(true);
	}

	public void SetMultiplyBuff(float sum)
	{
		multiplyBuff = sum;
	}

	public float GetMultiplyBuff()
	{
		return multiplyBuff;
	}

	public void SetSelectSkillPanel(bool active)
	{
		selectSkillPanel.SetActive(active);
	}
}
