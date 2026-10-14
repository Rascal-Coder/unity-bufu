using System;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Module.UI;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Module.Role;

public class Role : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	private enum BehaviorStep
	{
		IdleWait,
		NoCoinWait,
		Walking,
		ThrowingWaitFlip,
		ThrowingWaitAnim,
		AfterThrowWait,
		SkillCDWait,
		Sleeping
	}

	[Header("Spine 动画名称 - 待机")]
	[SerializeField]
	private string idleAnimName = "idle_front";

	[SerializeField]
	private string idleBackAnimName = "idle_back";

	[Header("Spine 动画名称 - 走路")]
	[SerializeField]
	private string walkFrontAnimName = "walk_front";

	[SerializeField]
	private string walkBackAnimName = "walk_back";

	[Header("Spine 动画名称 - 抛硬币")]
	[SerializeField]
	private string throwFrontAnimName = "throw_front";

	[SerializeField]
	private string throwBackAnimName = "throw_back";

	[Header("Spine 动画名称 - 技能")]
	[SerializeField]
	private string skillFrontAnimName = "skill_front";

	[SerializeField]
	private string skillBackAnimName = "skill_back";

	[Header("Spine 动画名称 - 睡觉")]
	[SerializeField]
	private string sleepAnimName = "sleep";

	[Header("移动设置")]
	[Tooltip("移动速度（像素/秒）- 基础值，会被 UpgradeManager 升级效果影响")]
	[SerializeField]
	private float baseMoveSpeed = 500f;

	[Tooltip("到达目标判定距离")]
	[SerializeField]
	private float reachThreshold = 10f;

	[Header("硬币交互距离")]
	[Tooltip("与硬币的最小交互距离（帮手不会走到硬币正上方）")]
	[SerializeField]
	private float coinMinDistance = 60f;

	[Header("到达目标后停留时间")]
	[Tooltip("到达硬币位置后停留时间（秒）")]
	[SerializeField]
	private float waitAfterThrow = 0.5f;

	[Header("睡觉设置")]
	[Tooltip("睡觉检测间隔（秒）")]
	[SerializeField]
	private float sleepCheckInterval = 5f;

	[Tooltip("睡觉概率（0-1）")]
	[Range(0f, 1f)]
	[SerializeField]
	private float sleepProbability = 0.3f;

	private float sourceSleepProbability;

	[Tooltip("睡觉持续时间（秒）")]
	[SerializeField]
	private float sleepDuration = 6f;

	private GameObject huanxing;

	private BoxCollider2D collider;

	[Header("组件引用")]
	[SerializeField]
	private SkeletonGraphic skeletonAnimation;

	private RoleState _state;

	private bool _isInitialized;

	private Vector3 _targetPosition;

	private Vector3 _coinPosition;

	private Vector2 _boundaryMin;

	private Vector2 _boundaryMax;

	private bool _isFacingFront = true;

	private string _currentAnimName = "";

	private bool _currentAnimLoop;

	private static float _lastGlobalIronSkillTime = -60f;

	private bool _forceSleepRequested;

	private bool _playerWakeRequested;

	private float _sleepTimer;

	private Action<CoinBase, bool> _flipCompleteHandler;

	private BehaviorStep _step;

	private float _stepTimer;

	private CoinBase _targetCoin;

	private bool _throwFlipCompleted;

	private bool _throwIsFront;

	private float _throwTimeoutElapsed;

	private float _throwAnimElapsed;

	private float _sleepElapsed;

	private float _skillCdDuration;

	private int _roleType;

	private int awakeSkillLevel;

	private bool _isChallengeMode;

	private ChallengeMain _challengeMain;

	private float _challengeSkillCd = 5f;

	private GameObject _tornadoPrefab;

	private List<GameObject> _tornadoPool = new List<GameObject>();

	private GameObject _magicPrefab;

	private List<UISpriteAnimationSequence> _magicAnimPool = new List<UISpriteAnimationSequence>();

	private GameObject _gongfuPrefab;

	private List<SkeletonGraphic> _gongfuAnimPool = new List<SkeletonGraphic>();

	private GameObject _lanqiuPrefab;

	private List<GameObject> _lanqiuPool = new List<GameObject>();

	private GameObject _dashePrefab;

	private List<DaSheHand> _dashePool = new List<DaSheHand>();

	private GameObject _dajiPrefab;

	private List<GameObject> _dajiPool = new List<GameObject>();

	private GameObject _ironBulletPrefab;

	private List<GameObject> _ironBulletPool = new List<GameObject>();

	private GameObject _ironBoomPrefab;

	private List<UISpriteAnimationSequence> _ironBoomPool = new List<UISpriteAnimationSequence>();

	private GameObject _ironBullet2Prefab;

	private List<GameObject> _ironBullet2Pool = new List<GameObject>();

	private GameObject _ironBoom2Prefab;

	private List<UISpriteAnimationSequence> _ironBoom2Pool = new List<UISpriteAnimationSequence>();

	private GameObject _halandePrefab;

	private List<SkeletonGraphic> _halandePool = new List<SkeletonGraphic>();

	private GameObject _zuqiuPrefab;

	private List<GameObject> _zuqiuPool = new List<GameObject>();

	[SerializeField]
	private float scatterRadius = 50f;

	private float handradius = 120f;

	public float halandradius = 120f;

	public int roleType => _roleType;

	public int AwakeSkillLevel => awakeSkillLevel;

	public RoleState State => _state;

	private float CurrentMoveSpeed
	{
		get
		{
			float num = 1f;
			if ((Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null)
			{
				num = SingletonMono<UpgradeManager>.Instance.GetRoleSpeedMultiplier();
			}
			return baseMoveSpeed * num;
		}
	}

	public void SetChallengeMode(bool enable, ChallengeMain challengeMain = null)
	{
		_isChallengeMode = enable;
		_challengeMain = challengeMain;
	}

	public void SetChallengeSkillCd(float cd)
	{
		_challengeSkillCd = cd;
	}

	private void Awake()
	{
		if ((Object)(object)skeletonAnimation == (Object)null)
		{
			skeletonAnimation = ((Component)this).GetComponent<SkeletonGraphic>();
		}
		collider = ((Component)this).GetComponent<BoxCollider2D>();
		((Collider2D)collider).isTrigger = true;
	}

	public void ChangeSkin(string skinName)
	{
		Skeleton skeleton = skeletonAnimation.Skeleton;
		skeleton.SetSkin(skinName);
		skeleton.SetToSetupPose();
		skeletonAnimation.Update(0f);
		_currentAnimName = "";
	}

	private void Start()
	{
		sourceSleepProbability = sleepProbability;
		if (!_isChallengeMode)
		{
			RefreshSleepProbability();
		}
	}

	private void OnDestroy()
	{
		StopBehaviorLoop();
		if ((Object)(object)huanxing != (Object)null)
		{
			Object.Destroy((Object)(object)huanxing.gameObject);
		}
		if (_tornadoPool != null)
		{
			foreach (GameObject item in _tornadoPool)
			{
				if ((Object)(object)item != (Object)null)
				{
					Object.Destroy((Object)(object)item);
				}
			}
			_tornadoPool.Clear();
		}
		if (_magicAnimPool != null)
		{
			foreach (UISpriteAnimationSequence item2 in _magicAnimPool)
			{
				if ((Object)(object)item2 != (Object)null && (Object)(object)((Component)item2).gameObject != (Object)null)
				{
					Object.Destroy((Object)(object)((Component)item2).gameObject);
				}
			}
			_magicAnimPool.Clear();
		}
		if (_gongfuAnimPool != null)
		{
			foreach (SkeletonGraphic item3 in _gongfuAnimPool)
			{
				if ((Object)(object)item3 != (Object)null && (Object)(object)((Component)item3).gameObject != (Object)null)
				{
					Object.Destroy((Object)(object)((Component)item3).gameObject);
				}
			}
			_gongfuAnimPool.Clear();
		}
		if (_lanqiuPool != null)
		{
			foreach (GameObject item4 in _lanqiuPool)
			{
				if ((Object)(object)item4 != (Object)null)
				{
					Object.Destroy((Object)(object)item4);
				}
			}
			_lanqiuPool.Clear();
		}
		if (_dashePool != null)
		{
			foreach (DaSheHand item5 in _dashePool)
			{
				if ((Object)(object)item5 != (Object)null && (Object)(object)((Component)item5).gameObject != (Object)null)
				{
					Object.Destroy((Object)(object)((Component)item5).gameObject);
				}
			}
			_dashePool.Clear();
		}
		if (_dajiPool != null)
		{
			foreach (GameObject item6 in _dajiPool)
			{
				if ((Object)(object)item6 != (Object)null && (Object)(object)item6.gameObject != (Object)null)
				{
					Object.Destroy((Object)(object)item6.gameObject);
				}
			}
			_dajiPool.Clear();
		}
		if (_ironBulletPool != null)
		{
			foreach (GameObject item7 in _ironBulletPool)
			{
				if ((Object)(object)item7 != (Object)null)
				{
					Object.Destroy((Object)(object)item7);
				}
			}
			_ironBulletPool.Clear();
		}
		if (_ironBoomPool != null)
		{
			foreach (UISpriteAnimationSequence item8 in _ironBoomPool)
			{
				if ((Object)(object)item8 != (Object)null && (Object)(object)((Component)item8).gameObject != (Object)null)
				{
					Object.Destroy((Object)(object)((Component)item8).gameObject);
				}
			}
			_ironBoomPool.Clear();
		}
		if (_ironBullet2Pool != null)
		{
			foreach (GameObject item9 in _ironBullet2Pool)
			{
				if ((Object)(object)item9 != (Object)null)
				{
					Object.Destroy((Object)(object)item9);
				}
			}
			_ironBullet2Pool.Clear();
		}
		if (_ironBoom2Pool != null)
		{
			foreach (UISpriteAnimationSequence item10 in _ironBoom2Pool)
			{
				if ((Object)(object)item10 != (Object)null && (Object)(object)((Component)item10).gameObject != (Object)null)
				{
					Object.Destroy((Object)(object)((Component)item10).gameObject);
				}
			}
			_ironBoom2Pool.Clear();
		}
		if (_halandePool != null)
		{
			foreach (SkeletonGraphic item11 in _halandePool)
			{
				if ((Object)(object)item11 != (Object)null && (Object)(object)((Component)item11).gameObject != (Object)null)
				{
					Object.Destroy((Object)(object)((Component)item11).gameObject);
				}
			}
			_halandePool.Clear();
		}
		if (_zuqiuPool == null)
		{
			return;
		}
		foreach (GameObject item12 in _zuqiuPool)
		{
			if ((Object)(object)item12 != (Object)null)
			{
				Object.Destroy((Object)(object)item12);
			}
		}
		_zuqiuPool.Clear();
	}

	private void Update()
	{
		if (_isInitialized)
		{
			TickBehavior(Time.deltaTime);
		}
	}

	public void Initialize(int roleType, Vector2 boundaryMin, Vector2 boundaryMax)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		_roleType = roleType;
		_boundaryMin = boundaryMin;
		_boundaryMax = boundaryMax;
		float num = 30f;
		float num2 = Random.Range(_boundaryMin.x + num, _boundaryMax.x - num);
		float num3 = Random.Range(_boundaryMin.y + num, _boundaryMax.y - num);
		_targetPosition = new Vector3(num2, num3, 0f);
		((Component)this).transform.position = _targetPosition;
		_isFacingFront = Random.value > 0.5f;
		UpdateFacingSprite();
		_isInitialized = true;
		_state = RoleState.Idle;
		_forceSleepRequested = false;
		_sleepTimer = 0f;
		StartBehaviorLoop();
		if (roleType != 0)
		{
			awakeSkillLevel = GameDataManager.Instance.GetHelperAwakeSkillLevel(roleType);
			if (awakeSkillLevel > 0 && (roleType == 1 || roleType == 2 || roleType == 3))
			{
				ChangeSkin("juexing");
			}
		}
	}

	private bool CanFlipCoin(CoinType coinType)
	{
		if (_isChallengeMode)
		{
			return true;
		}
		if ((Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null)
		{
			return SingletonMono<UpgradeManager>.Instance.CanRoleFlipCoin(coinType);
		}
		return coinType == CoinType.Copper;
	}

	private void StartBehaviorLoop()
	{
		EnterIdleWait();
	}

	private void StopBehaviorLoop()
	{
		CleanupCoinFlipSubscription();
		_isInitialized = false;
	}

	private void TickBehavior(float deltaTime)
	{
		switch (_step)
		{
		case BehaviorStep.IdleWait:
			if (_forceSleepRequested || CheckSleepTimer())
			{
				EnterSleeping();
				break;
			}
			_stepTimer += deltaTime;
			if (_stepTimer >= 0.2f)
			{
				TryFindCoinAndStart();
			}
			break;
		case BehaviorStep.NoCoinWait:
			if (_forceSleepRequested || CheckSleepTimer())
			{
				EnterSleeping();
				break;
			}
			_stepTimer += deltaTime;
			if (_stepTimer >= 1f)
			{
				EnterIdleWait();
			}
			break;
		case BehaviorStep.Walking:
			if (_forceSleepRequested)
			{
				EnterSleeping();
			}
			else if (TickWalking(deltaTime))
			{
				if (_forceSleepRequested || CheckSleepTimer())
				{
					EnterSleeping();
				}
				else
				{
					EnterThrowing(_targetCoin);
				}
			}
			break;
		case BehaviorStep.ThrowingWaitFlip:
			TickThrowingWaitFlip(deltaTime);
			break;
		case BehaviorStep.ThrowingWaitAnim:
			_throwAnimElapsed += deltaTime;
			if (_throwAnimElapsed >= 0.3f)
			{
				if (_forceSleepRequested)
				{
					EnterSleeping();
				}
				else
				{
					EnterAfterThrowWait();
				}
			}
			break;
		case BehaviorStep.AfterThrowWait:
			if (_forceSleepRequested || CheckSleepTimer())
			{
				EnterSleeping();
				break;
			}
			_stepTimer += deltaTime;
			if (_stepTimer >= waitAfterThrow)
			{
				EnterSkillCDWait();
			}
			break;
		case BehaviorStep.SkillCDWait:
			if (_forceSleepRequested || CheckSleepTimer())
			{
				EnterSleeping();
				break;
			}
			_stepTimer += deltaTime;
			if (_stepTimer >= _skillCdDuration)
			{
				EnterIdleWait();
			}
			break;
		case BehaviorStep.Sleeping:
			TickSleeping(deltaTime);
			break;
		}
	}

	private bool CheckSleepTimer()
	{
		_sleepTimer += Time.deltaTime;
		if (_sleepTimer >= sleepCheckInterval)
		{
			_sleepTimer = 0f;
			if (Random.value < sleepProbability)
			{
				_forceSleepRequested = true;
				return true;
			}
		}
		return false;
	}

	private void CheckAwakeSkillLevelUpdate()
	{
		if (roleType == 0)
		{
			return;
		}
		int helperAwakeSkillLevel = GameDataManager.Instance.GetHelperAwakeSkillLevel(roleType);
		if (helperAwakeSkillLevel != awakeSkillLevel)
		{
			awakeSkillLevel = helperAwakeSkillLevel;
			if (roleType == 1 || roleType == 2 || roleType == 3)
			{
				ChangeSkin("juexing");
			}
			OnAwakeSkillLevelChanged();
		}
	}

	private void OnAwakeSkillLevelChanged()
	{
		if (roleType != 2 || !((Object)(object)_tornadoPrefab != (Object)null))
		{
			return;
		}
		foreach (GameObject item in _tornadoPool)
		{
			if ((Object)(object)item != (Object)null)
			{
				Object.Destroy((Object)(object)item);
			}
		}
		_tornadoPool.Clear();
		_tornadoPrefab = null;
	}

	private void EnterIdleWait()
	{
		CheckAwakeSkillLevelUpdate();
		_step = BehaviorStep.IdleWait;
		_stepTimer = 0f;
		_state = RoleState.Idle;
		string animName = (_isFacingFront ? idleAnimName : idleBackAnimName);
		PlayAnimation(animName, loop: true);
	}

	private void EnterSkillCDWait()
	{
		_step = BehaviorStep.SkillCDWait;
		_stepTimer = 0f;
		if (_roleType > 0 && ConfigManager.HasInstance())
		{
			_skillCdDuration = ConfigManager.Instance.GetHelperCD(_roleType);
		}
		else
		{
			_skillCdDuration = 0f;
		}
		if (_skillCdDuration <= 0f)
		{
			EnterIdleWait();
			return;
		}
		_state = RoleState.Idle;
		string animName = (_isFacingFront ? idleAnimName : idleBackAnimName);
		PlayAnimation(animName, loop: true);
	}

	private void EnterNoCoinWait()
	{
		_step = BehaviorStep.NoCoinWait;
		_stepTimer = 0f;
		_state = RoleState.Idle;
		string animName = (_isFacingFront ? idleAnimName : idleBackAnimName);
		PlayAnimation(animName, loop: true);
	}

	private void TryFindCoinAndStart()
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		if (_forceSleepRequested)
		{
			EnterSleeping();
			return;
		}
		CoinBase coinBase = null;
		coinBase = ((_roleType != 1 || awakeSkillLevel <= 0) ? FindRandomCoin() : FindNearestCoin());
		if ((Object)(object)coinBase == (Object)null)
		{
			EnterNoCoinWait();
			return;
		}
		_targetCoin = coinBase;
		_coinPosition = ((Component)coinBase).transform.position;
		_targetPosition = CalculateStopPosition(_coinPosition);
		UpdateFacingDirection(_coinPosition);
		EnterWalking();
	}

	private void EnterWalking()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		_step = BehaviorStep.Walking;
		_state = RoleState.Walking;
		UpdateFacingDirection(_targetPosition);
		SetHorizontalFlip(_targetPosition);
		string animName = (_isFacingFront ? walkFrontAnimName : walkBackAnimName);
		PlayAnimation(animName, loop: true);
	}

	private bool TickWalking(float deltaTime)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)this).transform.position;
		Vector3 val = _targetPosition - position;
		if (((Vector3)(ref val)).magnitude < reachThreshold)
		{
			float num = 30f;
			((Component)this).transform.position = new Vector3(Mathf.Clamp(_targetPosition.x, _boundaryMin.x + num, _boundaryMax.x - num), Mathf.Clamp(_targetPosition.y, _boundaryMin.y + num, _boundaryMax.y - num), 0f);
			return true;
		}
		((Vector3)(ref val)).Normalize();
		Vector3 val2 = default(Vector3);
		((Vector3)(ref val2))._002Ector(val.x, val.y * 0.5f, 0f);
		Vector3 val3 = position + val2 * CurrentMoveSpeed * deltaTime;
		float num2 = 30f;
		val3.x = Mathf.Clamp(val3.x, _boundaryMin.x + num2, _boundaryMax.x - num2);
		val3.y = Mathf.Clamp(val3.y, _boundaryMin.y + num2, _boundaryMax.y - num2);
		((Component)this).transform.position = val3;
		return false;
	}

	private void EnterThrowing(CoinBase coin)
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		CleanupCoinFlipSubscription();
		_state = RoleState.Throwing;
		_step = BehaviorStep.ThrowingWaitFlip;
		_throwFlipCompleted = false;
		_throwIsFront = false;
		_throwTimeoutElapsed = 0f;
		_throwAnimElapsed = 0f;
		if ((Object)(object)coin == (Object)null)
		{
			_step = BehaviorStep.ThrowingWaitAnim;
			return;
		}
		((Component)coin).transform.SetAsLastSibling();
		Vector3 position = ((Component)coin).transform.position;
		_isFacingFront = ((Component)this).transform.position.y >= position.y;
		SetHorizontalFlip(position);
		_flipCompleteHandler = delegate(CoinBase c, bool front)
		{
			_throwIsFront = front;
			_throwFlipCompleted = true;
		};
		coin.OnFlipComplete = (Action<CoinBase, bool>)Delegate.Combine(coin.OnFlipComplete, _flipCompleteHandler);
		coin.JumpToTarget(Vector2.op_Implicit(((Component)coin).transform.position), Vector2.op_Implicit(((Component)coin).transform.position), _roleType);
		PlayExtra(coin);
		if (_roleType == 9 && awakeSkillLevel >= 1)
		{
			Action<CoinBase, bool> extraFlipHandler = null;
			extraFlipHandler = delegate(CoinBase flippedCoin, bool front)
			{
				//IL_0042: Unknown result type (might be due to invalid IL or missing references)
				//IL_0047: Unknown result type (might be due to invalid IL or missing references)
				//IL_004c: Unknown result type (might be due to invalid IL or missing references)
				//IL_004d: Unknown result type (might be due to invalid IL or missing references)
				//IL_005d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0062: Unknown result type (might be due to invalid IL or missing references)
				//IL_0064: Unknown result type (might be due to invalid IL or missing references)
				//IL_0065: Unknown result type (might be due to invalid IL or missing references)
				//IL_0066: Unknown result type (might be due to invalid IL or missing references)
				//IL_0067: Unknown result type (might be due to invalid IL or missing references)
				flippedCoin.OnFlipComplete = (Action<CoinBase, bool>)Delegate.Remove(flippedCoin.OnFlipComplete, extraFlipHandler);
				if ((Object)(object)flippedCoin != (Object)null && (Object)(object)((Component)flippedCoin).gameObject != (Object)null && flippedCoin.State != CoinState.Flying)
				{
					Vector2 val = Vector2.op_Implicit(((Component)flippedCoin).transform.position);
					Vector2 val2 = Random.insideUnitCircle * scatterRadius;
					flippedCoin.JumpToTarget(val, val + val2, 9);
				}
			};
			coin.OnFlipComplete = (Action<CoinBase, bool>)Delegate.Combine(coin.OnFlipComplete, extraFlipHandler);
		}
		string animName = (_isFacingFront ? throwFrontAnimName : throwBackAnimName);
		PlayAnimation(animName, loop: false);
	}

	private void TickThrowingWaitFlip(float deltaTime)
	{
		_throwTimeoutElapsed += deltaTime;
		float num = 1f;
		if (!_isChallengeMode && TalentDataManager.Instance != null)
		{
			num = TalentDataManager.Instance.CoinRain();
		}
		float num2 = 0.8f * num + 2f;
		if (_throwFlipCompleted || _throwTimeoutElapsed >= num2)
		{
			CleanupCoinFlipSubscription();
			_step = BehaviorStep.ThrowingWaitAnim;
			_throwAnimElapsed = 0f;
		}
	}

	private void EnterAfterThrowWait()
	{
		_step = BehaviorStep.AfterThrowWait;
		_stepTimer = 0f;
		_state = RoleState.Idle;
		string animName = (_isFacingFront ? idleAnimName : idleBackAnimName);
		PlayAnimation(animName, loop: true);
	}

	private void EnterSleeping()
	{
		CleanupCoinFlipSubscription();
		CheckAwakeSkillLevelUpdate();
		_step = BehaviorStep.Sleeping;
		_state = RoleState.Sleeping;
		_sleepElapsed = 0f;
		_playerWakeRequested = false;
		PlayAnimation(sleepAnimName, loop: true);
		if (!_isChallengeMode)
		{
			ActiveSleepTip();
		}
		((Collider2D)collider).isTrigger = false;
	}

	private void TickSleeping(float deltaTime)
	{
		_sleepElapsed += deltaTime;
		if (_playerWakeRequested || _sleepElapsed >= sleepDuration)
		{
			_forceSleepRequested = false;
			_playerWakeRequested = false;
			_sleepTimer = 0f;
			EnterIdleWait();
			if ((Object)(object)huanxing != (Object)null && !_isChallengeMode)
			{
				huanxing.SetActive(false);
				SingletonMono<RoleManager>.Instance.SetSleepTipActive(active: false);
			}
			((Collider2D)collider).isTrigger = true;
		}
	}

	private void CleanupCoinFlipSubscription()
	{
		if (_flipCompleteHandler != null && (Object)(object)_targetCoin != (Object)null)
		{
			CoinBase targetCoin = _targetCoin;
			targetCoin.OnFlipComplete = (Action<CoinBase, bool>)Delegate.Remove(targetCoin.OnFlipComplete, _flipCompleteHandler);
		}
		_flipCompleteHandler = null;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (_state == RoleState.Sleeping)
		{
			Debug.Log((object)"[Role] ★收到点击事件（睡眠中），设置唤醒标志");
			_playerWakeRequested = true;
			if ((Object)(object)SoundManager.Instance != (Object)null)
			{
				SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/revive.mp3");
			}
			else
			{
				Debug.LogError((object)"[Role] SoundManager.Instance 为 null，无法播放唤醒音效");
			}
			if (!_isChallengeMode)
			{
				CreatHamWakeupEffect();
			}
			if ((Object)(object)huanxing != (Object)null && !_isChallengeMode)
			{
				huanxing.SetActive(false);
				SingletonMono<RoleManager>.Instance.SetSleepTipActive(active: false);
			}
			((Collider2D)collider).isTrigger = true;
		}
	}

	private Vector3 CalculateStopPosition(Vector3 coinPos)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ((Component)this).transform.position - coinPos;
		if (((Vector3)(ref val)).magnitude < coinMinDistance)
		{
			return ((Component)this).transform.position;
		}
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		return coinPos + normalized * coinMinDistance;
	}

	private void UpdateFacingDirection(Vector3 targetPos)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		_isFacingFront = ((Component)this).transform.position.y >= targetPos.y;
	}

	private void UpdateFacingSprite()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)skeletonAnimation != (Object)null) || skeletonAnimation.Skeleton == null)
		{
			((Component)this).transform.localScale = new Vector3((float)(_isFacingFront ? 1 : (-1)), 1f, 1f);
		}
	}

	private void SetHorizontalFlip(Vector3 targetPos)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)skeletonAnimation != (Object)null && skeletonAnimation.Skeleton != null)
		{
			bool flipX = targetPos.x < ((Component)this).transform.position.x;
			skeletonAnimation.Skeleton.FlipX = flipX;
		}
	}

	private CoinBase FindRandomCoin()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		float num = 900f;
		CoinBase coinBase = null;
		for (; num <= 2000f; num += 300f)
		{
			List<CoinBase> coinList = GetCoinList();
			if (coinList == null)
			{
				break;
			}
			List<CoinBase> list = new List<CoinBase>();
			for (int i = 0; i < coinList.Count; i++)
			{
				CoinBase coinBase2 = coinList[i];
				if ((Object)(object)coinBase2 != (Object)null && coinBase2.State == CoinState.Idle && CanFlipCoin(coinBase2.coinType) && Vector3.Distance(((Component)this).transform.position, ((Component)coinBase2).transform.position) <= num)
				{
					list.Add(coinBase2);
				}
			}
			if (list.Count > 0)
			{
				coinBase = list[Random.Range(0, list.Count)];
				break;
			}
		}
		if ((Object)(object)coinBase == (Object)null)
		{
			Debug.Log((object)$"[Role] 在最大半径 {2000f} 像素内未找到任何硬币");
		}
		return coinBase;
	}

	private CoinBase FindNearestCoin()
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		List<CoinBase> coinList = GetCoinList();
		if (coinList == null)
		{
			return null;
		}
		CoinBase result = null;
		float num = float.MaxValue;
		for (int i = 0; i < coinList.Count; i++)
		{
			CoinBase coinBase = coinList[i];
			if ((Object)(object)coinBase != (Object)null && coinBase.State == CoinState.Idle && (Object)(object)coinBase != (Object)(object)_targetCoin && CanFlipCoin(coinBase.coinType))
			{
				float num2 = Vector3.Distance(((Component)this).transform.position, ((Component)coinBase).transform.position);
				if (num2 < num)
				{
					num = num2;
					result = coinBase;
				}
			}
		}
		return result;
	}

	private List<CoinBase> GetCoinList()
	{
		if (_isChallengeMode)
		{
			return _challengeMain?.GetAllCoins();
		}
		return UIManager.Instance.MainScene?.GetAllCoins();
	}

	private Transform GetEffectPool(string poolName)
	{
		if (_isChallengeMode)
		{
			return _challengeMain?.GetEffectPool(poolName);
		}
		return UIManager.Instance.MainScene?.GetEffectPool(poolName);
	}

	private void PlayAnimation(string animName, bool loop)
	{
		if (!((Object)(object)skeletonAnimation == (Object)null) && !string.IsNullOrEmpty(animName) && (!(animName == _currentAnimName) || loop != _currentAnimLoop))
		{
			AnimationState animationState = skeletonAnimation.AnimationState;
			SkeletonData skeletonData = skeletonAnimation.SkeletonData;
			if (skeletonData == null || skeletonData.FindAnimation(animName) == null)
			{
				Debug.LogError((object)("[Role] ★动画不存在: " + animName + "，请检查 Spine 动画资源命名"));
				return;
			}
			animationState.SetAnimation(0, animName, loop);
			_currentAnimName = animName;
			_currentAnimLoop = loop;
		}
	}

	public void RefreshSleepProbability()
	{
		if (!_isChallengeMode && TalentDataManager.Instance != null)
		{
			switch (GameDataManager.Instance?.GetCurrentWorldId() ?? 1)
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

	public void PlayExtra(CoinBase coin)
	{
		if (roleType == 2)
		{
			PlayTornado(coin);
		}
		else if (roleType == 3)
		{
			PlayMagic(coin);
		}
		else if (roleType == 4)
		{
			PlayGongfu(coin);
		}
		else if (roleType == 5)
		{
			PlayLanQiu(coin);
		}
		else if (roleType == 6)
		{
			PlayDaShe(coin);
		}
		else if (roleType == 7)
		{
			PlayDaji(coin);
		}
		else if (roleType == 8)
		{
			if (Time.time - _lastGlobalIronSkillTime >= 20f && awakeSkillLevel > 2)
			{
				_lastGlobalIronSkillTime = Time.time;
				PlayGlobalIron();
			}
			else
			{
				PlayIron(coin);
			}
		}
		else if (roleType == 9)
		{
			PlayHaaland(coin);
		}
	}

	private GameObject GetFromTornadoPool(GameObject prefab, Transform parent)
	{
		_tornadoPool.RemoveAll((GameObject t) => (Object)(object)t == (Object)null);
		foreach (GameObject item in _tornadoPool)
		{
			if (!item.activeSelf)
			{
				return item;
			}
		}
		GameObject val = Object.Instantiate<GameObject>(prefab, parent);
		_tornadoPool.Add(val);
		return val;
	}

	private UISpriteAnimationSequence GetFromMagicPool(GameObject prefab, Transform parent)
	{
		_magicAnimPool.RemoveAll((UISpriteAnimationSequence m) => (Object)(object)m == (Object)null || (Object)(object)((Component)m).gameObject == (Object)null);
		foreach (UISpriteAnimationSequence item in _magicAnimPool)
		{
			if (!((Component)item).gameObject.activeSelf)
			{
				return item;
			}
		}
		GameObject newMagic = Object.Instantiate<GameObject>(prefab, parent);
		UISpriteAnimationSequence component = newMagic.GetComponent<UISpriteAnimationSequence>();
		if ((Object)(object)component != (Object)null)
		{
			component.IsLooping = false;
			component.OnAnimationComplete = (Action)Delegate.Combine(component.OnAnimationComplete, (Action)delegate
			{
				newMagic.SetActive(false);
			});
			_magicAnimPool.Add(component);
		}
		return component;
	}

	private SkeletonGraphic GetFromGongfuPool(GameObject prefab, Transform parent)
	{
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Expected O, but got Unknown
		_gongfuAnimPool.RemoveAll((SkeletonGraphic m) => (Object)(object)m == (Object)null || (Object)(object)((Component)m).gameObject == (Object)null);
		foreach (SkeletonGraphic item in _gongfuAnimPool)
		{
			if (!((Component)item).gameObject.activeSelf)
			{
				return item;
			}
		}
		GameObject newGongfu = Object.Instantiate<GameObject>(prefab, parent);
		SkeletonGraphic component = newGongfu.GetComponent<SkeletonGraphic>();
		if ((Object)(object)component != (Object)null)
		{
			if (component.AnimationState == null)
			{
				component.Initialize(true);
			}
			if (component.AnimationState != null)
			{
				component.AnimationState.Complete += (TrackEntryDelegate)delegate
				{
					newGongfu.SetActive(false);
				};
			}
			_gongfuAnimPool.Add(component);
		}
		return component;
	}

	private GameObject GetFromLanqiuPool(GameObject prefab, Transform parent)
	{
		_lanqiuPool.RemoveAll((GameObject m) => (Object)(object)m == (Object)null);
		foreach (GameObject item in _lanqiuPool)
		{
			if (!item.activeSelf)
			{
				return item;
			}
		}
		GameObject val = Object.Instantiate<GameObject>(prefab, parent);
		_lanqiuPool.Add(val);
		return val;
	}

	private DaSheHand GetFromDaShePool(GameObject prefab, Transform parent)
	{
		_dashePool.RemoveAll((DaSheHand m) => (Object)(object)m == (Object)null || (Object)(object)((Component)m).gameObject == (Object)null);
		foreach (DaSheHand item in _dashePool)
		{
			if (!((Component)item).gameObject.activeSelf)
			{
				return item;
			}
		}
		GameObject val = Object.Instantiate<GameObject>(prefab, parent);
		DaSheHand daSheHand = val.GetComponent<DaSheHand>();
		if ((Object)(object)daSheHand == (Object)null)
		{
			daSheHand = val.AddComponent<DaSheHand>();
		}
		_dashePool.Add(daSheHand);
		return daSheHand;
	}

	private GameObject GetFromDajiPool(GameObject prefab, Transform parent)
	{
		_dajiPool.RemoveAll((GameObject d) => (Object)(object)d == (Object)null);
		foreach (GameObject item in _dajiPool)
		{
			if (!item.activeSelf)
			{
				return item;
			}
		}
		GameObject val = Object.Instantiate<GameObject>(prefab, parent);
		_dajiPool.Add(val);
		return val;
	}

	private GameObject GetFromIronBulletPool(GameObject prefab, Transform parent)
	{
		_ironBulletPool.RemoveAll((GameObject t) => (Object)(object)t == (Object)null);
		foreach (GameObject item in _ironBulletPool)
		{
			if (!item.activeSelf)
			{
				return item;
			}
		}
		GameObject val = Object.Instantiate<GameObject>(prefab, parent);
		_ironBulletPool.Add(val);
		return val;
	}

	private UISpriteAnimationSequence GetFromIronBoomPool(GameObject prefab, Transform parent)
	{
		_ironBoomPool.RemoveAll((UISpriteAnimationSequence m) => (Object)(object)m == (Object)null || (Object)(object)((Component)m).gameObject == (Object)null);
		foreach (UISpriteAnimationSequence item in _ironBoomPool)
		{
			if (!((Component)item).gameObject.activeSelf)
			{
				return item;
			}
		}
		GameObject newBoom = Object.Instantiate<GameObject>(prefab, parent);
		UISpriteAnimationSequence component = newBoom.GetComponent<UISpriteAnimationSequence>();
		if ((Object)(object)component != (Object)null)
		{
			component.IsLooping = false;
			component.OnAnimationComplete = (Action)Delegate.Combine(component.OnAnimationComplete, (Action)delegate
			{
				newBoom.SetActive(false);
			});
			_ironBoomPool.Add(component);
		}
		return component;
	}

	private GameObject GetFromIronBullet2Pool(GameObject prefab, Transform parent)
	{
		_ironBullet2Pool.RemoveAll((GameObject t) => (Object)(object)t == (Object)null);
		foreach (GameObject item in _ironBullet2Pool)
		{
			if (!item.activeSelf)
			{
				return item;
			}
		}
		GameObject val = Object.Instantiate<GameObject>(prefab, parent);
		_ironBullet2Pool.Add(val);
		return val;
	}

	private UISpriteAnimationSequence GetFromIronBoom2Pool(GameObject prefab, Transform parent)
	{
		_ironBoom2Pool.RemoveAll((UISpriteAnimationSequence m) => (Object)(object)m == (Object)null || (Object)(object)((Component)m).gameObject == (Object)null);
		foreach (UISpriteAnimationSequence item in _ironBoom2Pool)
		{
			if (!((Component)item).gameObject.activeSelf)
			{
				return item;
			}
		}
		GameObject newBoom = Object.Instantiate<GameObject>(prefab, parent);
		UISpriteAnimationSequence component = newBoom.GetComponent<UISpriteAnimationSequence>();
		if ((Object)(object)component != (Object)null)
		{
			component.IsLooping = false;
			component.OnAnimationComplete = (Action)Delegate.Combine(component.OnAnimationComplete, (Action)delegate
			{
				newBoom.SetActive(false);
			});
			_ironBoom2Pool.Add(component);
		}
		return component;
	}

	private SkeletonGraphic GetFromHalandPool(GameObject prefab, Transform parent)
	{
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Expected O, but got Unknown
		_halandePool.RemoveAll((SkeletonGraphic m) => (Object)(object)m == (Object)null || (Object)(object)((Component)m).gameObject == (Object)null);
		foreach (SkeletonGraphic item in _halandePool)
		{
			if (!((Component)item).gameObject.activeSelf)
			{
				return item;
			}
		}
		GameObject newHaland = Object.Instantiate<GameObject>(prefab, parent);
		SkeletonGraphic component = newHaland.GetComponent<SkeletonGraphic>();
		if ((Object)(object)component != (Object)null)
		{
			if (component.AnimationState == null)
			{
				component.Initialize(true);
			}
			if (component.AnimationState != null)
			{
				component.AnimationState.Complete += (TrackEntryDelegate)delegate
				{
					newHaland.SetActive(false);
				};
			}
			_halandePool.Add(component);
		}
		return component;
	}

	private GameObject GetFromZuqiuPool(GameObject prefab, Transform parent)
	{
		_zuqiuPool.RemoveAll((GameObject m) => (Object)(object)m == (Object)null);
		foreach (GameObject item in _zuqiuPool)
		{
			if (!item.activeSelf)
			{
				return item;
			}
		}
		GameObject val = Object.Instantiate<GameObject>(prefab, parent);
		_zuqiuPool.Add(val);
		return val;
	}

	private void PlayTornado(CoinBase coin)
	{
		int tornadoCount = Mathf.RoundToInt(SingletonMono<UpgradeManager>.Instance.GetHelperExtraBuff(2));
		tornadoCount = Mathf.Max(1, tornadoCount);
		float distance = 500f * ConfigManager.Instance.GetHelperFactor(2);
		float speed = 10f;
		if (awakeSkillLevel > 0)
		{
			distance *= 1.5f;
			speed = 15f;
		}
		Action<GameObject> playAction = delegate(GameObject prefab)
		{
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_0129: Unknown result type (might be due to invalid IL or missing references)
			//IL_012a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0180: Unknown result type (might be due to invalid IL or missing references)
			//IL_0153: Unknown result type (might be due to invalid IL or missing references)
			//IL_015c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0166: Unknown result type (might be due to invalid IL or missing references)
			//IL_016f: Unknown result type (might be due to invalid IL or missing references)
			Transform effectPool = GetEffectPool("TornadoPool");
			if ((Object)(object)effectPool != (Object)null && (Object)(object)prefab != (Object)null)
			{
				Vector3 val = ((Component)coin).transform.position - ((Component)this).transform.position;
				Vector2 val2 = Vector2.op_Implicit(((Vector3)(ref val)).normalized);
				float num = 15f;
				float num2 = (float)(-(tornadoCount - 1)) * num / 2f;
				for (int i = 0; i < tornadoCount; i++)
				{
					GameObject fromTornadoPool = GetFromTornadoPool(prefab, effectPool);
					fromTornadoPool.transform.position = ((Component)this).transform.position;
					fromTornadoPool.transform.localScale = Vector3.one * 2f;
					fromTornadoPool.SetActive(true);
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
					if (_isChallengeMode)
					{
						tornadoAnimation.SetChallengeMain(_challengeMain);
					}
					Vector2 direction = val2;
					if (tornadoCount > 1)
					{
						float num3 = (num2 + (float)i * num) * ((float)Math.PI / 180f);
						float num4 = Mathf.Cos(num3);
						float num5 = Mathf.Sin(num3);
						((Vector2)(ref direction))._002Ector(val2.x * num4 - val2.y * num5, val2.x * num5 + val2.y * num4);
					}
					tornadoAnimation.SetMovement(direction, speed, distance);
				}
			}
		};
		if ((Object)(object)_tornadoPrefab != (Object)null)
		{
			playAction(_tornadoPrefab);
			return;
		}
		string assetPath = "Assets/GameRes/Role/coins_tornado.prefab";
		if (awakeSkillLevel > 0)
		{
			assetPath = "Assets/GameRes/Role/coins_tornado2.prefab";
		}
		SingletonMono<ResManager>.Instance.LoadPrefabAsync(assetPath, delegate(GameObject prefab)
		{
			_tornadoPrefab = prefab;
			playAction(prefab);
		});
	}

	private void PlayMagic(CoinBase coin2)
	{
		float radius = ConfigManager.Instance.GetHelperFactor(3) * 200f;
		int maxTossCount = Mathf.RoundToInt(SingletonMono<UpgradeManager>.Instance.GetHelperExtraBuff(3));
		maxTossCount = ((maxTossCount > 0) ? maxTossCount : 3);
		Action<GameObject> playAction = delegate(GameObject prefab)
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)prefab != (Object)null)
			{
				Vector2 val = Vector2.op_Implicit(((Component)this).transform.position);
				List<CoinBase> list = null;
				list = ((awakeSkillLevel <= 0 || _isChallengeMode) ? GetCoinList() : UIManager.Instance.MainScene?.GetCoinsSortedByValue());
				if (list != null)
				{
					for (int i = 0; i < list.Count; i++)
					{
						CoinBase coinBase = list[i];
						if ((Object)(object)coinBase != (Object)null && coinBase.State == CoinState.Idle && CanFlipCoin(coinBase.coinType) && Vector2.Distance(val, Vector2.op_Implicit(((Component)coinBase).transform.position)) <= radius && maxTossCount > 0)
						{
							TossCoin(coinBase, prefab);
							maxTossCount--;
						}
					}
				}
			}
		};
		if ((Object)(object)_magicPrefab != (Object)null)
		{
			playAction(_magicPrefab);
			return;
		}
		SingletonMono<ResManager>.Instance.LoadPrefabAsync("Assets/GameRes/Role/magic_ani.prefab", delegate(GameObject prefab)
		{
			_magicPrefab = prefab;
			playAction(prefab);
		});
	}

	private void TossCoin(CoinBase coin, GameObject prefab)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = Vector2.op_Implicit(((Component)coin).transform.position);
		Vector2 val2 = Random.insideUnitCircle * scatterRadius;
		Vector2 targetPos = val + val2;
		coin.JumpToTarget(val, targetPos, roleType);
		Transform effectPool = GetEffectPool("MagicianPool");
		if ((Object)(object)effectPool != (Object)null)
		{
			UISpriteAnimationSequence fromMagicPool = GetFromMagicPool(prefab, effectPool);
			if ((Object)(object)fromMagicPool != (Object)null)
			{
				((Component)fromMagicPool).gameObject.SetActive(true);
				((Component)fromMagicPool).transform.position = ((Component)coin).transform.position;
				((Component)fromMagicPool).transform.localScale = Vector3.one * 2f;
				fromMagicPool.Restart();
				fromMagicPool.Play();
			}
		}
	}

	private void TossCoinSimple(CoinBase coin)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = Vector2.op_Implicit(((Component)coin).transform.position);
		Vector2 val2 = Random.insideUnitCircle * scatterRadius;
		Vector2 targetPos = val + val2;
		if (_roleType == 6)
		{
			coin.JumpToTarget(val, val, _roleType);
			coin.SetThrowDelayTimer(0f);
			return;
		}
		coin.JumpToTarget(val, targetPos, _roleType);
		if (_roleType == 4)
		{
			coin.SetThrowDelayTimer(0f);
		}
	}

	public void ActiveSleepTip()
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		if (SingletonMono<RoleManager>.Instance.GetSleepCount() >= 3 || SingletonMono<RoleManager>.Instance.GetSleepTipActive())
		{
			return;
		}
		if ((Object)(object)huanxing == (Object)null)
		{
			SingletonMono<ResManager>.Instance.LoadPrefabAsync("Assets/GameRes/Role/Role_huanxing.prefab", delegate(GameObject prefab)
			{
				//IL_0048: Unknown result type (might be due to invalid IL or missing references)
				//IL_005c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0061: Unknown result type (might be due to invalid IL or missing references)
				//IL_007c: Unknown result type (might be due to invalid IL or missing references)
				//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
				//IL_00c0: Expected O, but got Unknown
				Transform val = UIManager.Instance?.MainScene?.GetHuanxingPool();
				if ((Object)(object)val != (Object)null)
				{
					huanxing = Object.Instantiate<GameObject>(prefab, val);
					huanxing.transform.position = ((Component)this).transform.position + new Vector3(0f, 110f, 0f);
					Rigidbody2D componentInChildren2 = huanxing.GetComponentInChildren<Rigidbody2D>();
					TweenSettingsExtensions.SetLoops<TweenerCore<Vector2, Vector2, VectorOptions>>(componentInChildren2.DOMoveY(((Component)componentInChildren2).transform.position.y + 15f, 0.4f), -1, (LoopType)1);
					((UnityEvent)huanxing.GetComponentInChildren<Button>().onClick).AddListener((UnityAction)delegate
					{
						_forceSleepRequested = false;
						_playerWakeRequested = false;
						_sleepTimer = 0f;
						EnterIdleWait();
						if ((Object)(object)huanxing != (Object)null)
						{
							huanxing.SetActive(false);
							SingletonMono<RoleManager>.Instance.SetSleepTipActive(active: false);
						}
						if ((Object)(object)SoundManager.Instance != (Object)null)
						{
							SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/revive.mp3");
						}
						CreatHamWakeupEffect();
					});
					SingletonMono<RoleManager>.Instance.SetSleepTipActive(active: true);
					SingletonMono<RoleManager>.Instance.AddSleepCount();
				}
			});
		}
		else
		{
			huanxing.SetActive(true);
			huanxing.transform.position = ((Component)this).transform.position + new Vector3(0f, 110f, 0f);
			Rigidbody2D componentInChildren = huanxing.GetComponentInChildren<Rigidbody2D>();
			ShortcutExtensions.DOPause((Component)(object)componentInChildren);
			TweenSettingsExtensions.SetLoops<TweenerCore<Vector2, Vector2, VectorOptions>>(componentInChildren.DOMoveY(((Component)componentInChildren).transform.position.y + 15f, 0.4f), -1, (LoopType)1);
			SingletonMono<RoleManager>.Instance.SetSleepTipActive(active: true);
			SingletonMono<RoleManager>.Instance.AddSleepCount();
		}
	}

	public void CreatHamWakeupEffect()
	{
		SingletonMono<ResManager>.Instance.LoadPrefabAsync("Assets/GameRes/Role/hammer_wakeup.prefab", delegate(GameObject prefab)
		{
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Expected O, but got Unknown
			if ((Object)(object)Object.FindObjectOfType<MainScene>() != (Object)null)
			{
				GameObject creatParticle = Object.Instantiate<GameObject>(prefab, ((Component)this).transform);
				creatParticle.transform.localPosition = new Vector3(0f, 80f, 0f);
				SkeletonGraphic component = ((Component)creatParticle.transform).GetComponent<SkeletonGraphic>();
				if (component.AnimationState != null)
				{
					component.AnimationState.Complete += (TrackEntryDelegate)delegate
					{
						Object.Destroy((Object)(object)creatParticle);
					};
				}
			}
		});
	}

	public void PlayGongfu(CoinBase coinBase)
	{
		float animationScale = 1.5f;
		if (awakeSkillLevel > 0)
		{
			animationScale = 2f;
			if (awakeSkillLevel > 1)
			{
				animationScale = 2.5f;
			}
		}
		float radius = SingletonMono<UpgradeManager>.Instance.GetHelperExtraBuff(4) * handradius * animationScale;
		int maxTossCount = 10;
		maxTossCount = ((maxTossCount > 0) ? maxTossCount : 3);
		Action<GameObject> playAction = delegate(GameObject prefab)
		{
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0103: Unknown result type (might be due to invalid IL or missing references)
			//IL_010b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0110: Unknown result type (might be due to invalid IL or missing references)
			Transform effectPool = GetEffectPool("GongfuPool");
			if ((Object)(object)effectPool != (Object)null && (Object)(object)prefab != (Object)null)
			{
				Vector2 val = Vector2.op_Implicit(((Component)coinBase).transform.position);
				SkeletonGraphic fromGongfuPool = GetFromGongfuPool(prefab, effectPool);
				if ((Object)(object)fromGongfuPool != (Object)null && fromGongfuPool.AnimationState != null)
				{
					((Component)fromGongfuPool).gameObject.SetActive(true);
					((Component)fromGongfuPool).transform.position = Vector2.op_Implicit(val + new Vector2(0f, 0f));
					((Component)fromGongfuPool).transform.localScale = Vector3.one * animationScale;
					fromGongfuPool.AnimationState.SetAnimation(0, "skill", false);
				}
				List<CoinBase> coinList = GetCoinList();
				if (coinList != null)
				{
					for (int i = 0; i < coinList.Count; i++)
					{
						CoinBase coinBase2 = coinList[i];
						if ((Object)(object)coinBase2 != (Object)null && coinBase2.State == CoinState.Idle && CanFlipCoin(coinBase2.coinType) && Vector2.Distance(val, Vector2.op_Implicit(((Component)coinBase2).transform.position)) <= radius && maxTossCount > 0)
						{
							TossCoinSimple(coinBase2);
							maxTossCount--;
						}
					}
				}
			}
		};
		if ((Object)(object)_gongfuPrefab != (Object)null)
		{
			playAction(_gongfuPrefab);
			return;
		}
		SingletonMono<ResManager>.Instance.LoadPrefabAsync("Assets/GameRes/Role/rulaishenzhang.prefab", delegate(GameObject prefab)
		{
			_gongfuPrefab = prefab;
			playAction(prefab);
		});
	}

	private void PlayHaaland(CoinBase coin)
	{
		float animationScale = 1.5f;
		if (awakeSkillLevel > 0)
		{
			animationScale = 2f;
			if (awakeSkillLevel > 1)
			{
				animationScale = 2.5f;
			}
		}
		float radius = SingletonMono<UpgradeManager>.Instance.GetHelperExtraBuff(9) * halandradius * animationScale;
		int maxTossCount = 10;
		maxTossCount = ((maxTossCount > 0) ? maxTossCount : 3);
		int num = 0;
		if (awakeSkillLevel >= 2)
		{
			num = 1;
		}
		if (awakeSkillLevel >= 3)
		{
			num = 2;
		}
		int capturedZuqiuCount = num;
		Action<GameObject> playAction = delegate(GameObject prefab)
		{
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0227: Unknown result type (might be due to invalid IL or missing references)
			//IL_022c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0230: Unknown result type (might be due to invalid IL or missing references)
			//IL_0235: Unknown result type (might be due to invalid IL or missing references)
			//IL_023c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0241: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0206: Unknown result type (might be due to invalid IL or missing references)
			//IL_020b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0210: Unknown result type (might be due to invalid IL or missing references)
			//IL_0214: Unknown result type (might be due to invalid IL or missing references)
			//IL_0219: Unknown result type (might be due to invalid IL or missing references)
			//IL_021e: Unknown result type (might be due to invalid IL or missing references)
			//IL_010f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0117: Unknown result type (might be due to invalid IL or missing references)
			//IL_011c: Unknown result type (might be due to invalid IL or missing references)
			//IL_024f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0254: Unknown result type (might be due to invalid IL or missing references)
			Transform effectPool = GetEffectPool("HalandPool");
			if ((Object)(object)effectPool != (Object)null && (Object)(object)prefab != (Object)null)
			{
				Vector2 val = Vector2.op_Implicit(((Component)coin).transform.position);
				SkeletonGraphic fromHalandPool = GetFromHalandPool(prefab, effectPool);
				if ((Object)(object)fromHalandPool != (Object)null && fromHalandPool.AnimationState != null)
				{
					((Component)fromHalandPool).gameObject.SetActive(true);
					((Component)fromHalandPool).transform.position = Vector2.op_Implicit(val + new Vector2(0f, 0f));
					((Component)fromHalandPool).transform.localScale = Vector3.one * animationScale;
					fromHalandPool.AnimationState.SetAnimation(0, "skill", false);
				}
				List<CoinBase> coinList = GetCoinList();
				if (coinList != null)
				{
					for (int i = 0; i < coinList.Count; i++)
					{
						CoinBase coinBase = coinList[i];
						if ((Object)(object)coinBase != (Object)null && coinBase.State == CoinState.Idle && CanFlipCoin(coinBase.coinType) && Vector2.Distance(val, Vector2.op_Implicit(((Component)coinBase).transform.position)) <= radius && maxTossCount > 0)
						{
							TossCoinSimple(coinBase);
							maxTossCount--;
							if (awakeSkillLevel >= 1)
							{
								Action<CoinBase, bool> extraFlipHandler = null;
								extraFlipHandler = delegate(CoinBase flippedCoin, bool front)
								{
									//IL_0042: Unknown result type (might be due to invalid IL or missing references)
									//IL_0047: Unknown result type (might be due to invalid IL or missing references)
									//IL_004c: Unknown result type (might be due to invalid IL or missing references)
									//IL_004d: Unknown result type (might be due to invalid IL or missing references)
									//IL_0062: Unknown result type (might be due to invalid IL or missing references)
									//IL_0067: Unknown result type (might be due to invalid IL or missing references)
									//IL_0069: Unknown result type (might be due to invalid IL or missing references)
									//IL_006a: Unknown result type (might be due to invalid IL or missing references)
									//IL_006b: Unknown result type (might be due to invalid IL or missing references)
									//IL_006c: Unknown result type (might be due to invalid IL or missing references)
									flippedCoin.OnFlipComplete = (Action<CoinBase, bool>)Delegate.Remove(flippedCoin.OnFlipComplete, extraFlipHandler);
									if ((Object)(object)flippedCoin != (Object)null && (Object)(object)((Component)flippedCoin).gameObject != (Object)null && flippedCoin.State != CoinState.Flying)
									{
										Vector2 val3 = Vector2.op_Implicit(((Component)flippedCoin).transform.position);
										Vector2 val4 = Random.insideUnitCircle * scatterRadius;
										flippedCoin.JumpToTarget(val3, val3 + val4, 9);
									}
								};
								coinBase.OnFlipComplete = (Action<CoinBase, bool>)Delegate.Combine(coinBase.OnFlipComplete, extraFlipHandler);
							}
						}
					}
					if (capturedZuqiuCount > 0)
					{
						Vector2 baseDirection;
						if ((Object)(object)coin != (Object)null)
						{
							Vector3 val2 = ((Component)coin).transform.position - ((Component)this).transform.position;
							baseDirection = Vector2.op_Implicit(((Vector3)(ref val2)).normalized);
						}
						else
						{
							Vector2 insideUnitCircle = Random.insideUnitCircle;
							baseDirection = ((Vector2)(ref insideUnitCircle)).normalized;
							if (baseDirection == Vector2.zero)
							{
								baseDirection = Vector2.right;
							}
						}
						Action launchZuqiu = delegate
						{
							//IL_0097: Unknown result type (might be due to invalid IL or missing references)
							//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
							//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
							//IL_012c: Unknown result type (might be due to invalid IL or missing references)
							//IL_0131: Unknown result type (might be due to invalid IL or missing references)
							//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
							//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
							//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
							//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
							//IL_0196: Unknown result type (might be due to invalid IL or missing references)
							//IL_019b: Unknown result type (might be due to invalid IL or missing references)
							//IL_019f: Unknown result type (might be due to invalid IL or missing references)
							//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
							//IL_020b: Unknown result type (might be due to invalid IL or missing references)
							//IL_0210: Unknown result type (might be due to invalid IL or missing references)
							//IL_0215: Unknown result type (might be due to invalid IL or missing references)
							//IL_021b: Unknown result type (might be due to invalid IL or missing references)
							//IL_021d: Unknown result type (might be due to invalid IL or missing references)
							Transform effectPool2 = GetEffectPool("ZuqiuPool");
							if (!((Object)(object)effectPool2 == (Object)null) && !((Object)(object)_zuqiuPrefab == (Object)null))
							{
								float num2 = 30f;
								float num3 = (float)(-(capturedZuqiuCount - 1)) * num2 / 2f;
								for (int j = 0; j < capturedZuqiuCount; j++)
								{
									GameObject fromZuqiuPool = GetFromZuqiuPool(_zuqiuPrefab, effectPool2);
									fromZuqiuPool.transform.position = ((Component)this).transform.position;
									fromZuqiuPool.transform.localScale = Vector3.one * 1f;
									fromZuqiuPool.SetActive(true);
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
									if (_isChallengeMode)
									{
										zuqiuAnimation.SetChallengeMain(_challengeMain);
									}
									Vector2 direction = baseDirection;
									if (capturedZuqiuCount > 1)
									{
										float num4 = (num3 + (float)j * num2) * ((float)Math.PI / 180f);
										float num5 = Mathf.Cos(num4);
										float num6 = Mathf.Sin(num4);
										Vector2 val3 = new Vector2(baseDirection.x * num5 - baseDirection.y * num6, baseDirection.x * num6 + baseDirection.y * num5);
										direction = ((Vector2)(ref val3)).normalized;
									}
									float speed = 1000f;
									int maxHitCount = 999;
									Vector2 min = _boundaryMin;
									Vector2 max = _boundaryMax;
									MainScene mainScene = UIManager.Instance.MainScene;
									if ((Object)(object)mainScene != (Object)null)
									{
										mainScene.GetSceneBoundary(out min, out max);
									}
									zuqiuAnimation.Init(Vector2.op_Implicit(((Component)this).transform.position), direction, speed, maxHitCount, min, max, TossCoinSimple);
								}
							}
						};
						if ((Object)(object)_zuqiuPrefab != (Object)null)
						{
							launchZuqiu();
						}
						else
						{
							SingletonMono<ResManager>.Instance.LoadPrefabAsync("Assets/GameRes/Role/zuqiu.prefab", delegate(GameObject zp)
							{
								_zuqiuPrefab = zp;
								launchZuqiu();
							});
						}
					}
				}
			}
		};
		if ((Object)(object)_halandePrefab != (Object)null)
		{
			playAction(_halandePrefab);
			return;
		}
		SingletonMono<ResManager>.Instance.LoadPrefabAsync("Assets/GameRes/Role/halandeskill.prefab", delegate(GameObject prefab)
		{
			_halandePrefab = prefab;
			playAction(prefab);
		});
	}

	private void PlayLanQiu(CoinBase coin)
	{
		int ballCount = Mathf.RoundToInt(SingletonMono<UpgradeManager>.Instance.GetHelperExtraBuff(5));
		ballCount = Mathf.Max(1, ballCount);
		Action<GameObject> playAction = delegate(GameObject prefab)
		{
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_015b: Unknown result type (might be due to invalid IL or missing references)
			//IL_015c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0183: Unknown result type (might be due to invalid IL or missing references)
			//IL_018c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0196: Unknown result type (might be due to invalid IL or missing references)
			//IL_019f: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0203: Unknown result type (might be due to invalid IL or missing references)
			//IL_0208: Unknown result type (might be due to invalid IL or missing references)
			//IL_0210: Unknown result type (might be due to invalid IL or missing references)
			//IL_0215: Unknown result type (might be due to invalid IL or missing references)
			//IL_0245: Unknown result type (might be due to invalid IL or missing references)
			//IL_024a: Unknown result type (might be due to invalid IL or missing references)
			//IL_024f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0257: Unknown result type (might be due to invalid IL or missing references)
			//IL_0259: Unknown result type (might be due to invalid IL or missing references)
			Transform effectPool = GetEffectPool("LanqiuPool");
			if ((Object)(object)effectPool != (Object)null && (Object)(object)prefab != (Object)null)
			{
				Vector2 val2;
				Vector2 val3;
				if ((Object)(object)coin != (Object)null)
				{
					Vector3 val = ((Component)coin).transform.position - ((Component)this).transform.position;
					val2 = Vector2.op_Implicit(((Vector3)(ref val)).normalized);
				}
				else
				{
					val3 = Random.insideUnitCircle;
					val2 = ((Vector2)(ref val3)).normalized;
					if (val2 == Vector2.zero)
					{
						val2 = Vector2.right;
					}
				}
				float num = 30f;
				float num2 = (float)(-(ballCount - 1)) * num / 2f;
				for (int i = 0; i < ballCount; i++)
				{
					GameObject fromLanqiuPool = GetFromLanqiuPool(prefab, effectPool);
					fromLanqiuPool.transform.position = ((Component)this).transform.position;
					fromLanqiuPool.transform.localScale = Vector3.one * 1f;
					fromLanqiuPool.SetActive(true);
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
					if (_isChallengeMode)
					{
						lanqiuAnimation.SetChallengeMain(_challengeMain);
					}
					Vector2 direction = val2;
					if (ballCount > 1)
					{
						float num3 = (num2 + (float)i * num) * ((float)Math.PI / 180f);
						float num4 = Mathf.Cos(num3);
						float num5 = Mathf.Sin(num3);
						val3 = new Vector2(val2.x * num4 - val2.y * num5, val2.x * num5 + val2.y * num4);
						direction = ((Vector2)(ref val3)).normalized;
					}
					int maxHitCount = 999;
					float speed = 1000f;
					int maxBounceCount = 0;
					if (awakeSkillLevel > 0)
					{
						maxBounceCount = 1;
						if (awakeSkillLevel > 1)
						{
							maxBounceCount = 2;
							if (awakeSkillLevel > 2)
							{
								maxBounceCount = 3;
							}
						}
					}
					Vector2 min = _boundaryMin;
					Vector2 max = _boundaryMax;
					MainScene mainScene = UIManager.Instance.MainScene;
					if ((Object)(object)mainScene != (Object)null)
					{
						mainScene.GetSceneBoundary(out min, out max);
					}
					lanqiuAnimation.Init(Vector2.op_Implicit(((Component)this).transform.position), direction, speed, maxBounceCount, maxHitCount, min, max, TossCoinSimple);
				}
			}
		};
		if ((Object)(object)_lanqiuPrefab != (Object)null)
		{
			playAction(_lanqiuPrefab);
			return;
		}
		SingletonMono<ResManager>.Instance.LoadPrefabAsync("Assets/GameRes/Role/lanqiu.prefab", delegate(GameObject prefab)
		{
			_lanqiuPrefab = prefab;
			playAction(prefab);
		});
	}

	private void PlayDaShe(CoinBase coinBase)
	{
		float radius = ConfigManager.Instance.GetHelperFactor(6) * 100f;
		if (radius <= 300f)
		{
			radius = 300f;
		}
		int maxTossCount = Mathf.RoundToInt(SingletonMono<UpgradeManager>.Instance.GetHelperExtraBuff(6));
		maxTossCount = ((maxTossCount <= 0) ? 1 : maxTossCount);
		Action<GameObject> playAction = delegate(GameObject prefab)
		{
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
			//IL_017c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0190: Unknown result type (might be due to invalid IL or missing references)
			//IL_0195: Unknown result type (might be due to invalid IL or missing references)
			//IL_019a: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
			Transform effectPool = GetEffectPool("DaShePool");
			if ((Object)(object)effectPool != (Object)null && (Object)(object)prefab != (Object)null)
			{
				Vector2 val = Vector2.op_Implicit(((Component)this).transform.position);
				List<CoinBase> list = null;
				list = ((awakeSkillLevel <= 2 || _isChallengeMode) ? GetCoinList() : UIManager.Instance.MainScene?.GetCoinsSortedByValue());
				if (list != null)
				{
					for (int i = 0; i < list.Count; i++)
					{
						CoinBase coin = list[i];
						if ((Object)(object)coin != (Object)null && coin.State == CoinState.Idle && CanFlipCoin(coin.coinType))
						{
							float num = Vector2.Distance(val, Vector2.op_Implicit(((Component)coin).transform.position));
							if (num <= radius && maxTossCount > 0 && num > 100f && num <= radius && maxTossCount > 0)
							{
								DaSheHand hand = GetFromDaShePool(prefab, effectPool);
								((Component)hand).gameObject.SetActive(true);
								hand.Init(Vector2.op_Implicit(((Component)this).transform.position + new Vector3(0f, 100f, 0f)), Vector2.op_Implicit(((Component)coin).transform.position), delegate
								{
									TossCoinSimple(coin);
									((Component)hand).gameObject.SetActive(false);
								});
								maxTossCount--;
							}
						}
					}
				}
			}
		};
		if ((Object)(object)_dashePrefab != (Object)null)
		{
			playAction(_dashePrefab);
			return;
		}
		SingletonMono<ResManager>.Instance.LoadPrefabAsync("Assets/GameRes/Role/dashehandani.prefab", delegate(GameObject prefab)
		{
			_dashePrefab = prefab;
			playAction(prefab);
		});
	}

	private void PlayDaji(CoinBase coin)
	{
		int dajiCount = Mathf.RoundToInt(SingletonMono<UpgradeManager>.Instance.GetHelperExtraBuff(7));
		dajiCount = Mathf.Max(1, dajiCount);
		float distance = 500f * ConfigManager.Instance.GetHelperFactor(7);
		if (distance <= 0f)
		{
			distance = 500f;
		}
		float speed = 800f;
		bool shouldReturn = false;
		float pauseDuration = 0.5f;
		float suckRadius = 0f;
		if (awakeSkillLevel > 0)
		{
			shouldReturn = true;
			if (awakeSkillLevel > 1)
			{
				suckRadius = 150f;
			}
		}
		Action<GameObject> playAction = delegate(GameObject prefab)
		{
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_015b: Unknown result type (might be due to invalid IL or missing references)
			//IL_015c: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0183: Unknown result type (might be due to invalid IL or missing references)
			//IL_018c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0196: Unknown result type (might be due to invalid IL or missing references)
			//IL_019f: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
			Transform effectPool = GetEffectPool("DajiPool");
			if ((Object)(object)effectPool != (Object)null && (Object)(object)prefab != (Object)null)
			{
				Vector2 val2;
				Vector2 val3;
				if ((Object)(object)coin != (Object)null)
				{
					Vector3 val = ((Component)coin).transform.position - ((Component)this).transform.position;
					val2 = Vector2.op_Implicit(((Vector3)(ref val)).normalized);
				}
				else
				{
					val3 = Random.insideUnitCircle;
					val2 = ((Vector2)(ref val3)).normalized;
					if (val2 == Vector2.zero)
					{
						val2 = Vector2.right;
					}
				}
				float num = 15f;
				float num2 = (float)(-(dajiCount - 1)) * num / 2f;
				for (int i = 0; i < dajiCount; i++)
				{
					GameObject fromDajiPool = GetFromDajiPool(prefab, effectPool);
					fromDajiPool.transform.position = ((Component)this).transform.position;
					fromDajiPool.transform.localScale = Vector3.one * 1.5f;
					fromDajiPool.SetActive(true);
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
					if (_isChallengeMode)
					{
						dajiAnimation.SetChallengeMain(_challengeMain);
					}
					Vector2 direction = val2;
					if (dajiCount > 1)
					{
						float num3 = (num2 + (float)i * num) * ((float)Math.PI / 180f);
						float num4 = Mathf.Cos(num3);
						float num5 = Mathf.Sin(num3);
						val3 = new Vector2(val2.x * num4 - val2.y * num5, val2.x * num5 + val2.y * num4);
						direction = ((Vector2)(ref val3)).normalized;
					}
					dajiAnimation.Init(Vector2.op_Implicit(((Component)this).transform.position), direction, speed, distance, shouldReturn, pauseDuration, suckRadius);
				}
			}
		};
		if ((Object)(object)_dajiPrefab != (Object)null)
		{
			playAction(_dajiPrefab);
			return;
		}
		SingletonMono<ResManager>.Instance.LoadPrefabAsync("Assets/GameRes/Role/aixinqiu.prefab", delegate(GameObject prefab)
		{
			_dajiPrefab = prefab;
			playAction(prefab);
		});
	}

	private void PlayIron(CoinBase coin)
	{
		int bulletCount = Mathf.RoundToInt(SingletonMono<UpgradeManager>.Instance.GetHelperExtraBuff(8));
		bulletCount = Mathf.Max(1, bulletCount);
		Action<GameObject> playAction = delegate(GameObject prefab)
		{
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00be: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_0186: Unknown result type (might be due to invalid IL or missing references)
			//IL_018b: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01de: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0206: Expected O, but got Unknown
			//IL_0117: Unknown result type (might be due to invalid IL or missing references)
			//IL_011c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0121: Unknown result type (might be due to invalid IL or missing references)
			Transform effectPool = GetEffectPool("IronPool");
			if ((Object)(object)effectPool != (Object)null && (Object)(object)prefab != (Object)null)
			{
				for (int i = 0; i < bulletCount; i++)
				{
					GameObject bulletEffect = GetFromIronBulletPool(prefab, effectPool);
					float num = ((float)i - (float)(bulletCount - 1) / 2f) * 50f;
					bulletEffect.transform.position = ((Component)this).transform.position + new Vector3(num, 0f, 0f);
					bulletEffect.transform.rotation = Quaternion.identity;
					bulletEffect.transform.localScale = Vector3.one * 2f;
					bulletEffect.SetActive(true);
					List<CoinBase> coinList = GetCoinList();
					Vector2 targetPos;
					if (coinList != null && coinList.Count > 0)
					{
						int index = Random.Range(0, coinList.Count);
						targetPos = Vector2.op_Implicit(((Component)coinList[index]).transform.position);
					}
					else
					{
						float num2 = 100f;
						targetPos = new Vector2(Random.Range(_boundaryMin.x + num2, _boundaryMax.x - num2), Random.Range(_boundaryMin.y + num2, _boundaryMax.y - num2));
					}
					float num3 = 0.5f;
					Vector3 val = bulletEffect.transform.position + new Vector3(0f, 1500f, 0f);
					float num4 = Random.Range(0.1f, 0.3f);
					TweenCallback val2 = default(TweenCallback);
					TweenSettingsExtensions.OnComplete<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.SetDelay<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOMove(bulletEffect.transform, val, num3, false), (Ease)5), num4), (TweenCallback)delegate
					{
						//IL_003b: Unknown result type (might be due to invalid IL or missing references)
						//IL_005f: Unknown result type (might be due to invalid IL or missing references)
						//IL_007b: Unknown result type (might be due to invalid IL or missing references)
						//IL_0080: Unknown result type (might be due to invalid IL or missing references)
						//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
						//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
						//IL_00ab: Expected O, but got Unknown
						//IL_00b0: Expected O, but got Unknown
						if (!((Object)(object)bulletEffect == (Object)null))
						{
							bulletEffect.transform.position = new Vector3(targetPos.x, targetPos.y + 1500f, 0f);
							bulletEffect.transform.rotation = Quaternion.Euler(0f, 0f, 180f);
							float num5 = 0.4f;
							TweenerCore<Vector3, Vector3, VectorOptions> obj = TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOMove(bulletEffect.transform, Vector2.op_Implicit(targetPos), num5, false), (Ease)1);
							TweenCallback obj2 = val2;
							if (obj2 == null)
							{
								TweenCallback val3 = delegate
								{
									//IL_0026: Unknown result type (might be due to invalid IL or missing references)
									if ((Object)(object)bulletEffect != (Object)null)
									{
										bulletEffect.SetActive(false);
									}
									PlayIronBoom(targetPos);
								};
								TweenCallback val4 = val3;
								val2 = val3;
								obj2 = val4;
							}
							TweenSettingsExtensions.OnComplete<TweenerCore<Vector3, Vector3, VectorOptions>>(obj, obj2);
						}
					});
				}
			}
		};
		if ((Object)(object)_ironBulletPrefab != (Object)null)
		{
			playAction(_ironBulletPrefab);
			return;
		}
		SingletonMono<ResManager>.Instance.LoadPrefabAsync("Assets/GameRes/Role/iron_bullet.prefab", delegate(GameObject prefab)
		{
			_ironBulletPrefab = prefab;
			playAction(prefab);
		});
	}

	private void PlayIronBoom(Vector2 targetPos)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		Action<GameObject> playBoomAction = delegate(GameObject prefab)
		{
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0102: Unknown result type (might be due to invalid IL or missing references)
			Transform effectPool = GetEffectPool("IronBoomPool");
			if ((Object)(object)effectPool != (Object)null && (Object)(object)prefab != (Object)null)
			{
				UISpriteAnimationSequence fromIronBoomPool = GetFromIronBoomPool(prefab, effectPool);
				if ((Object)(object)fromIronBoomPool != (Object)null)
				{
					((Component)fromIronBoomPool).gameObject.SetActive(true);
					((Component)fromIronBoomPool).transform.position = Vector2.op_Implicit(targetPos);
					((Component)fromIronBoomPool).transform.localScale = Vector3.one * 2f;
					fromIronBoomPool.Restart();
					fromIronBoomPool.Play();
				}
				float num = 300f;
				if (num <= 0f)
				{
					num = 300f;
				}
				int num2 = 5;
				num2 = ((num2 > 0) ? num2 : 5);
				List<CoinBase> coinList = GetCoinList();
				if (coinList != null)
				{
					for (int i = 0; i < coinList.Count; i++)
					{
						CoinBase coinBase = coinList[i];
						if ((Object)(object)coinBase != (Object)null && coinBase.State == CoinState.Idle && CanFlipCoin(coinBase.coinType) && Vector2.Distance(targetPos, Vector2.op_Implicit(((Component)coinBase).transform.position)) <= num && num2 > 0)
						{
							TossCoinSimple(coinBase);
							num2--;
						}
					}
				}
			}
		};
		if ((Object)(object)_ironBoomPrefab != (Object)null)
		{
			playBoomAction(_ironBoomPrefab);
			return;
		}
		SingletonMono<ResManager>.Instance.LoadPrefabAsync("Assets/GameRes/Role/iron_boom.prefab", delegate(GameObject prefab)
		{
			_ironBoomPrefab = prefab;
			playBoomAction(prefab);
		});
	}

	private void PlayGlobalIron()
	{
		Action<GameObject> playAction = delegate(GameObject prefab)
		{
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0101: Unknown result type (might be due to invalid IL or missing references)
			//IL_011a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0124: Expected O, but got Unknown
			Transform effectPool = GetEffectPool("IronHePool");
			if ((Object)(object)effectPool != (Object)null && (Object)(object)prefab != (Object)null)
			{
				GameObject bulletEffect = GetFromIronBullet2Pool(prefab, effectPool);
				Vector2 targetPos = (_boundaryMin + _boundaryMax) / 2f;
				bulletEffect.transform.position = new Vector3(targetPos.x, targetPos.y + 1500f, 0f);
				bulletEffect.transform.rotation = Quaternion.identity;
				bulletEffect.transform.localScale = Vector3.one * 1f;
				bulletEffect.SetActive(true);
				float num = 1f;
				TweenSettingsExtensions.OnComplete<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOMove(bulletEffect.transform, Vector2.op_Implicit(targetPos), num, false), (Ease)1), (TweenCallback)delegate
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
		if ((Object)(object)_ironBullet2Prefab != (Object)null)
		{
			playAction(_ironBullet2Prefab);
			return;
		}
		SingletonMono<ResManager>.Instance.LoadPrefabAsync("Assets/GameRes/Role/iron_bullet2.prefab", delegate(GameObject prefab)
		{
			_ironBullet2Prefab = prefab;
			playAction(prefab);
		});
	}

	private void PlayGlobalIronBoom(Vector2 targetPos)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		Action<GameObject> playBoomAction = delegate(GameObject prefab)
		{
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_00de: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
			Transform effectPool = GetEffectPool("IronBoomPool");
			if ((Object)(object)effectPool != (Object)null && (Object)(object)prefab != (Object)null)
			{
				UISpriteAnimationSequence fromIronBoom2Pool = GetFromIronBoom2Pool(prefab, effectPool);
				if ((Object)(object)fromIronBoom2Pool != (Object)null)
				{
					((Component)fromIronBoom2Pool).gameObject.SetActive(true);
					((Component)fromIronBoom2Pool).transform.position = Vector2.op_Implicit(targetPos);
					((Component)fromIronBoom2Pool).transform.localScale = Vector3.one * 2f;
					fromIronBoom2Pool.Restart();
					fromIronBoom2Pool.Play();
				}
				float num = 3000f;
				int num2 = int.MaxValue;
				List<CoinBase> coinList = GetCoinList();
				if (coinList != null)
				{
					for (int i = 0; i < coinList.Count; i++)
					{
						CoinBase coinBase = coinList[i];
						if ((Object)(object)coinBase != (Object)null && coinBase.State == CoinState.Idle && CanFlipCoin(coinBase.coinType) && Vector2.Distance(targetPos, Vector2.op_Implicit(((Component)coinBase).transform.position)) <= num && num2 > 0)
						{
							TossCoinSimple(coinBase);
							num2--;
						}
					}
				}
			}
		};
		if ((Object)(object)_ironBoom2Prefab != (Object)null)
		{
			playBoomAction(_ironBoom2Prefab);
			return;
		}
		SingletonMono<ResManager>.Instance.LoadPrefabAsync("Assets/GameRes/Role/iron_boom2.prefab", delegate(GameObject prefab)
		{
			_ironBoom2Prefab = prefab;
			playBoomAction(prefab);
		});
	}
}
