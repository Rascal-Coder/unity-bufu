using System;
using Module.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Module.Role;

public class CoinBase : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerMoveHandler
{
	[Header("硬币配置")]
	public CoinType coinType;

	[Header("正反面权重")]
	[SerializeField]
	[Range(0f, 1f)]
	[Tooltip("正面权重，默认0.6表示正面60%、反面40%")]
	private float _frontWeight = 0.65f;

	[Header("组件引用")]
	[SerializeField]
	private UISpriteAnimationSequence _spriteAnimation;

	[SerializeField]
	private Rigidbody2D _rigidbody;

	[SerializeField]
	private CircleCollider2D _collider;

	[Header("飘字特效")]
	[SerializeField]
	private TextMeshProUGUI floatingTextUI;

	private TextMeshProUGUI _floatingNormalUI;

	private TextMeshProUGUI _floatingCriticalUI;

	[SerializeField]
	private float floatHeight = 120f;

	[SerializeField]
	private float floatDuration = 1f;

	[SerializeField]
	private float fontSize = 48f;

	[SerializeField]
	[Tooltip("子对象缩放补偿（抵消父对象缩放）")]
	private float scaleCompensation = 5f;

	[SerializeField]
	[Tooltip("后半部分开始淡出")]
	private float fadeStartRatio = 0.5f;

	[Header("飘字特效开始大小设置")]
	[SerializeField]
	public float _floatingTextAniScale;

	protected CoinState _state;

	protected bool _isFrontShowing = true;

	protected Vector2 _movementBoundaryMin;

	protected Vector2 _movementBoundaryMax;

	protected bool _hasMovementBoundary;

	protected Vector3 _originalScale;

	private bool _isThrowAnimating;

	private Vector2 _throwStartPos;

	private Vector2 _throwTargetPos;

	private float _throwElapsed;

	private float _throwDelayTimer;

	private bool _isWaitingForThrowDelay;

	private RectTransform _floatingTextRect;

	private bool _isFloatingTextAnimating;

	private float _floatingTextElapsed;

	private Vector2 _floatingTextStartPos;

	private float _floatingTextRandomOffsetX;

	private bool _hoverTriggerEnabled;

	private UISpriteAnimationSequence _smokeAnimSequence;

	private int _roleType;

	public Action<CoinBase> OnCoinClicked;

	public Action<CoinBase, bool> OnFlipComplete;

	public Action<CoinBase> OnLanded;

	[Header("目前是否触发了幸运回响条件")]
	public bool isLuckyEcho;

	private bool _isChallengeMode;

	private double _challengeCoinValue = 1.0;

	private Transform _floatingTextPool;

	public int roleType => _roleType;

	public CoinState State => _state;

	public bool IsFrontShowing => _isFrontShowing;

	public bool IsHoverTriggerEnabled => _hoverTriggerEnabled;

	public event Action<CoinBase> OnGuideFinger;

	public void SetChallengeMode(bool enable)
	{
		_isChallengeMode = enable;
		if (enable)
		{
			ChallengeMain instance = ChallengeMain.Instance;
			if ((Object)(object)instance != (Object)null)
			{
				LoadFloatingTextFromPool(instance.GetTextPool());
			}
		}
	}

	public void SetChallengeCoinValue(double value)
	{
		_challengeCoinValue = value;
	}

	protected virtual void Awake()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		_originalScale = Vector3.one * 0.6f;
		((Component)this).transform.localScale = _originalScale;
		_rigidbody = ((Component)this).GetComponent<Rigidbody2D>();
		_collider = ((Component)this).GetComponent<CircleCollider2D>();
		_spriteAnimation = ((Component)this).GetComponent<UISpriteAnimationSequence>();
		if ((Object)(object)_rigidbody != (Object)null)
		{
			_rigidbody.gravityScale = 0f;
			_rigidbody.freezeRotation = true;
			_rigidbody.bodyType = (RigidbodyType2D)1;
			_rigidbody.velocity = Vector2.zero;
		}
		if ((Object)(object)_spriteAnimation != (Object)null)
		{
			_spriteAnimation.AutoPlay = false;
			_spriteAnimation.IsLooping = false;
			_spriteAnimation.ShowFrontFace();
		}
		MainScene mainScene = UIManager.Instance.MainScene;
		if ((Object)(object)mainScene != (Object)null)
		{
			LoadFloatingTextFromPool(mainScene.GetTextPool());
		}
	}

	private void LoadFloatingTextFromPool(Transform pool)
	{
		if ((Object)(object)pool == (Object)null)
		{
			return;
		}
		_floatingTextPool = pool;
		SingletonMono<ResManager>.Instance.LoadPrefabAsync("Assets/GameRes/Role/coinNormalText.prefab", delegate(GameObject prefab)
		{
			if (!((Object)(object)_floatingTextPool != (Object)(object)pool))
			{
				GameObject val = Object.Instantiate<GameObject>(prefab, pool);
				_floatingNormalUI = val.GetComponent<TextMeshProUGUI>();
				((Component)_floatingNormalUI).gameObject.SetActive(false);
				_floatingTextRect = ((Component)_floatingNormalUI).GetComponent<RectTransform>();
				floatingTextUI = _floatingNormalUI;
			}
		});
		SingletonMono<ResManager>.Instance.LoadPrefabAsync("Assets/GameRes/Role/coinCriticalText.prefab", delegate(GameObject prefab)
		{
			if (!((Object)(object)_floatingTextPool != (Object)(object)pool))
			{
				GameObject val = Object.Instantiate<GameObject>(prefab, pool);
				_floatingCriticalUI = val.GetComponent<TextMeshProUGUI>();
				((Component)_floatingCriticalUI).gameObject.SetActive(false);
				_floatingTextRect = ((Component)_floatingCriticalUI).GetComponent<RectTransform>();
			}
		});
	}

	protected virtual void OnDestroy()
	{
		if ((Object)(object)_smokeAnimSequence != (Object)null && (Object)(object)((Component)_smokeAnimSequence).gameObject != (Object)null)
		{
			Object.Destroy((Object)(object)((Component)_smokeAnimSequence).gameObject);
		}
	}

	protected virtual void Update()
	{
		float deltaTime = Time.deltaTime;
		if (_isWaitingForThrowDelay)
		{
			_throwDelayTimer -= deltaTime;
			if (_throwDelayTimer <= 0f)
			{
				_isWaitingForThrowDelay = false;
				_state = CoinState.Flying;
				if ((Object)(object)SoundManager.Instance != (Object)null && SoundManager.Instance.EnableCoinSound)
				{
					SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/coinup.mp3");
				}
				if ((Object)(object)_spriteAnimation != (Object)null)
				{
					_spriteAnimation.Play();
				}
			}
		}
		if (_isThrowAnimating && _state == CoinState.Flying)
		{
			TickThrowAnimation(deltaTime);
		}
		if (_isFloatingTextAnimating)
		{
			TickFloatingTextAnimation(deltaTime);
		}
	}

	public virtual void OnCoinClick()
	{
		((Component)this).transform.SetAsLastSibling();
		if (_state == CoinState.Idle)
		{
			OnCoinClicked?.Invoke(this);
		}
	}

	public virtual void JumpToTarget(Vector2 startPos, Vector2 targetPos, int roleType)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		if (_state == CoinState.Flying || _isWaitingForThrowDelay)
		{
			return;
		}
		_roleType = roleType;
		((Component)this).transform.localScale = _originalScale;
		((Component)this).transform.position = Vector2.op_Implicit(startPos);
		_isThrowAnimating = true;
		_throwStartPos = startPos;
		_throwTargetPos = targetPos;
		_throwElapsed = 0f;
		_throwDelayTimer = 0.3f;
		_isWaitingForThrowDelay = true;
		float num = 1f;
		if (!_isChallengeMode && TalentDataManager.Instance != null)
		{
			num = TalentDataManager.Instance.CoinRain();
		}
		float num2 = 0.8f * num;
		int num3 = (((Object)(object)_spriteAnimation != (Object)null) ? _spriteAnimation.FrameCount : 8);
		int num4 = 5 * num3;
		float frameInterval = num2 / (float)num4;
		if ((Object)(object)_spriteAnimation != (Object)null)
		{
			_spriteAnimation.FrameInterval = frameInterval;
			_spriteAnimation.IsLooping = true;
			_spriteAnimation.Restart();
			_spriteAnimation.Pause();
			if (_isFrontShowing)
			{
				_spriteAnimation.ShowFrontFace();
			}
			else
			{
				_spriteAnimation.ShowBackFace();
			}
		}
	}

	private void TickThrowAnimation(float deltaTime)
	{
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		float num = 1f;
		if (!_isChallengeMode && TalentDataManager.Instance != null)
		{
			num = TalentDataManager.Instance.CoinRain();
		}
		float num2 = 0.8f * num;
		if (_roleType == 6)
		{
			int helperAwakeSkillLevel = GameDataManager.Instance.GetHelperAwakeSkillLevel(_roleType);
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
		_throwElapsed += deltaTime;
		float num3 = Mathf.Clamp01(_throwElapsed / num2);
		Vector2 val = Vector2.Lerp(_throwStartPos, _throwTargetPos, num3);
		float num4 = Mathf.Sin(num3 * (float)Math.PI) * CoinConfig.JumpHeight;
		Vector2 val2 = val + Vector2.up * num4;
		((Component)this).transform.position = Vector2.op_Implicit(val2);
		if (_throwElapsed >= num2)
		{
			FinishThrowAnimation();
		}
	}

	private void FinishThrowAnimation()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		_isThrowAnimating = false;
		((Component)this).transform.position = Vector2.op_Implicit(ClampToBoundary(_throwTargetPos));
		((Component)this).transform.localScale = _originalScale;
		Land();
		PlaySmoke();
	}

	public void SetMovementBoundary(Vector2 min, Vector2 max)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		_movementBoundaryMin = min;
		_movementBoundaryMax = max;
		_hasMovementBoundary = true;
	}

	protected virtual Vector2 ClampToBoundary(Vector2 pos)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		if (!_hasMovementBoundary)
		{
			return pos;
		}
		float num = 40f;
		return new Vector2(Mathf.Clamp(pos.x, _movementBoundaryMin.x + num, _movementBoundaryMax.x - num), Mathf.Clamp(pos.y, _movementBoundaryMin.y + num, _movementBoundaryMax.y - num));
	}

	protected virtual void Land()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (_state == CoinState.Landed)
		{
			return;
		}
		_isThrowAnimating = false;
		_state = CoinState.Landed;
		((Component)this).transform.localScale = _originalScale;
		if ((Object)(object)SoundManager.Instance != (Object)null && SoundManager.Instance.EnableCoinSound)
		{
			SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/hitground.mp3");
		}
		float num = _frontWeight;
		if (!_isChallengeMode && TalentDataManager.Instance != null)
		{
			num *= TalentDataManager.Instance.LuckyFlip();
		}
		bool flag = Random.value < num;
		if (!_isChallengeMode)
		{
			int num2 = PlayerPrefs.GetInt("guide_step_2", 0);
			int num3 = PlayerPrefs.GetInt("guide_step_2_count", 0);
			if (num2 == 0 && num3 < 2)
			{
				flag = num3 == 1;
				PlayerPrefs.SetInt("guide_step_2_count", num3 + 1);
				PlayerPrefs.Save();
			}
		}
		_isFrontShowing = flag;
		if ((Object)(object)_spriteAnimation != (Object)null)
		{
			_spriteAnimation.IsLooping = false;
			if (flag)
			{
				_spriteAnimation.ShowFrontFace();
			}
			else
			{
				_spriteAnimation.ShowBackFace();
			}
		}
		if (flag)
		{
			if (_isChallengeMode)
			{
				if ((Object)(object)ChallengeMain.Instance != (Object)null)
				{
					ChallengeMain instance = ChallengeMain.Instance;
					if (instance.IsAddText())
					{
						double coinValue = GetCoinValue();
						ShowFloatingText(coinValue);
						instance.AddTextCount();
					}
				}
			}
			else
			{
				MainScene mainScene = UIManager.Instance.MainScene;
				if (mainScene.IsAddText())
				{
					double coinValue2 = GetCoinValue();
					ShowFloatingText(coinValue2);
					mainScene.AddTextCount();
				}
			}
		}
		else if (!_isChallengeMode && TalentDataManager.Instance != null)
		{
			float num4 = TalentDataManager.Instance.LossBrings();
			MainScene mainScene2 = UIManager.Instance.MainScene;
			if (mainScene2.IsAddText() && num4 > 0f)
			{
				double sum = GetCoinValue() * (double)num4;
				ShowFloatingText(sum);
				mainScene2.AddTextCount();
			}
		}
		OnFlipComplete?.Invoke(this, flag);
		OnLanded?.Invoke(this);
		ResetToIdle();
	}

	private void ShowFloatingText(double sum)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)floatingTextUI == (Object)null)
		{
			return;
		}
		((TMP_Text)floatingTextUI).transform.localScale = Vector3.one * scaleCompensation;
		((Component)_floatingNormalUI).gameObject.SetActive(false);
		((Component)_floatingCriticalUI).gameObject.SetActive(false);
		if (!_isChallengeMode && SingletonMono<BuffManager>.Instance.GetIsCritical())
		{
			floatingTextUI = _floatingCriticalUI;
			_floatingTextRect = ((Component)_floatingCriticalUI).GetComponent<RectTransform>();
			((TMP_Text)floatingTextUI).text = NumberFormatter.ConvertToSpriteText("暴" + NumberFormatter.FormatNumber(sum));
		}
		else
		{
			floatingTextUI = _floatingNormalUI;
			_floatingTextRect = ((Component)_floatingNormalUI).GetComponent<RectTransform>();
			((TMP_Text)floatingTextUI).text = NumberFormatter.ConvertToSpriteText("+" + NumberFormatter.FormatNumber(sum));
		}
		((Graphic)floatingTextUI).color = Color.white;
		((TMP_Text)floatingTextUI).alignment = (TextAlignmentOptions)514;
		((TMP_Text)floatingTextUI).transform.position = ((Component)this).transform.position;
		float num = 1f;
		if (_isChallengeMode)
		{
			ChallengeMain instance = ChallengeMain.Instance;
			if ((Object)(object)instance != (Object)null)
			{
				int currentTextCount = instance.GetCurrentTextCount();
				int maxTextCount = instance.GetMaxTextCount();
				if (maxTextCount > 0)
				{
					num = 1f - 0.4f * ((float)currentTextCount / (float)maxTextCount);
					num = Mathf.Clamp(num, 0.6f, 1f);
				}
			}
		}
		else
		{
			MainScene mainScene = UIManager.Instance.MainScene;
			if ((Object)(object)mainScene != (Object)null)
			{
				int currentTextCount2 = mainScene.GetCurrentTextCount();
				int maxTextCount2 = mainScene.GetMaxTextCount();
				if (maxTextCount2 > 0)
				{
					num = 1f - 0.4f * ((float)currentTextCount2 / (float)maxTextCount2);
					num = Mathf.Clamp(num, 0.6f, 1f);
				}
			}
		}
		((TMP_Text)floatingTextUI).fontSize = fontSize * num;
		((TMP_Text)floatingTextUI).transform.localScale = Vector3.one * _floatingTextAniScale;
		((Component)floatingTextUI).gameObject.SetActive(true);
		StartFloatingTextAnimation();
	}

	private double GetCoinValue()
	{
		if (_isChallengeMode)
		{
			return _challengeCoinValue;
		}
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

	private void StartFloatingTextAnimation()
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)floatingTextUI == (Object)null)
		{
			_isFloatingTextAnimating = false;
			return;
		}
		if ((Object)(object)_floatingTextRect == (Object)null)
		{
			_floatingTextRect = ((Component)floatingTextUI).GetComponent<RectTransform>();
		}
		if ((Object)(object)_floatingTextRect == (Object)null)
		{
			_isFloatingTextAnimating = false;
			return;
		}
		_isFloatingTextAnimating = true;
		_floatingTextElapsed = 0f;
		_floatingTextStartPos = Vector2.op_Implicit(((Transform)_floatingTextRect).localPosition);
		_floatingTextRandomOffsetX = Random.Range(-30f, 30f);
		((Graphic)floatingTextUI).color = Color.white;
	}

	private void TickFloatingTextAnimation(float deltaTime)
	{
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)floatingTextUI == (Object)null || (Object)(object)_floatingTextRect == (Object)null)
		{
			_isFloatingTextAnimating = false;
			return;
		}
		if (floatDuration <= 0f)
		{
			FinishFloatingTextAnimation();
			return;
		}
		_floatingTextElapsed += deltaTime * 2f;
		float num = Mathf.Clamp01(_floatingTextElapsed / floatDuration);
		float num2 = EaseOutQuad(num);
		((Transform)_floatingTextRect).localPosition = new Vector3(_floatingTextStartPos.x + _floatingTextRandomOffsetX * (2f - num), _floatingTextStartPos.y + floatHeight * num2, 0f);
		if (num > fadeStartRatio)
		{
			_ = (num - fadeStartRatio) / (1f - fadeStartRatio);
			((Graphic)floatingTextUI).color = new Color(1f, 1f, 1f, 1f);
		}
		if (_floatingTextElapsed >= floatDuration)
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
		if ((Object)(object)floatingTextUI == (Object)null || (Object)(object)_floatingTextRect == (Object)null)
		{
			_isFloatingTextAnimating = false;
			return;
		}
		_isFloatingTextAnimating = false;
		((Component)floatingTextUI).gameObject.SetActive(false);
		((Transform)_floatingTextRect).localPosition = Vector2.op_Implicit(_floatingTextStartPos);
		((Graphic)floatingTextUI).color = Color.white;
		((TMP_Text)floatingTextUI).transform.localScale = Vector3.one * scaleCompensation;
		if (!_isChallengeMode)
		{
			MainScene mainScene = UIManager.Instance.MainScene;
			if ((Object)(object)mainScene != (Object)null)
			{
				mainScene.ReduceTextCount();
			}
		}
		else if ((Object)(object)ChallengeMain.Instance != (Object)null)
		{
			ChallengeMain instance = ChallengeMain.Instance;
			if ((Object)(object)instance != (Object)null)
			{
				instance.ReduceTextCount();
			}
		}
	}

	private float EaseOutQuad(float t)
	{
		return 1f - (1f - t) * (1f - t);
	}

	protected virtual void ResetToIdle()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		_state = CoinState.Idle;
		((Component)this).transform.rotation = Quaternion.identity;
		((Component)this).transform.localScale = _originalScale;
		_roleType = 0;
	}

	public virtual void SetInitialPosition(Vector2 pos)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.position = Vector2.op_Implicit(pos);
		_state = CoinState.Idle;
		_isFrontShowing = true;
		((Component)this).transform.localScale = _originalScale;
		if ((Object)(object)_spriteAnimation != (Object)null)
		{
			_spriteAnimation.ShowFrontFace();
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
	}

	public void OnPointerMove(PointerEventData eventData)
	{
		if (!_isChallengeMode && SingletonMono<UpgradeManager>.Instance.CanPlayerSwipeToThrow(coinType))
		{
			OnCoinClick();
			_roleType = -1;
			_throwDelayTimer = 0f;
			this.OnGuideFinger?.Invoke(this);
		}
	}

	public void SetHoverTriggerEnabled(bool enabled)
	{
		_hoverTriggerEnabled = enabled;
	}

	private void PlaySmoke()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_smokeAnimSequence != (Object)null)
		{
			((Component)_smokeAnimSequence).gameObject.SetActive(true);
			((Component)_smokeAnimSequence).transform.position = ((Component)this).transform.position;
			_smokeAnimSequence.Restart();
			_smokeAnimSequence.Play();
		}
		else
		{
			if (_roleType == 6 && GameDataManager.Instance.GetHelperAwakeSkillLevel(_roleType) > 1)
			{
				return;
			}
			SingletonMono<ResManager>.Instance.LoadPrefabAsync("Assets/GameRes/Role/coins_smoke.prefab", delegate(GameObject prefab)
			{
				//IL_0066: Unknown result type (might be due to invalid IL or missing references)
				Transform val;
				if (_isChallengeMode)
				{
					ChallengeMain instance = ChallengeMain.Instance;
					if ((Object)(object)instance == (Object)null)
					{
						return;
					}
					val = instance.GetEffectPool("SmokePool");
				}
				else
				{
					MainScene mainScene = UIManager.Instance.MainScene;
					if ((Object)(object)mainScene == (Object)null)
					{
						return;
					}
					val = mainScene.GetCoinsSmokePool();
				}
				GameObject smokeEffect = Object.Instantiate<GameObject>(prefab, val);
				smokeEffect.transform.position = ((Component)this).transform.position;
				_smokeAnimSequence = smokeEffect.GetComponent<UISpriteAnimationSequence>();
				if ((Object)(object)_smokeAnimSequence != (Object)null)
				{
					_smokeAnimSequence.IsLooping = false;
					_smokeAnimSequence.Play();
					UISpriteAnimationSequence smokeAnimSequence = _smokeAnimSequence;
					smokeAnimSequence.OnAnimationComplete = (Action)Delegate.Combine(smokeAnimSequence.OnAnimationComplete, (Action)delegate
					{
						smokeEffect.SetActive(false);
					});
				}
			});
		}
	}

	public void SetCoinClick()
	{
		OnCoinClick();
		_roleType = -1;
		_throwDelayTimer = 0f;
		this.OnGuideFinger?.Invoke(this);
	}

	public void SetThrowDelayTimer(float delay)
	{
		_throwDelayTimer = delay;
	}
}
