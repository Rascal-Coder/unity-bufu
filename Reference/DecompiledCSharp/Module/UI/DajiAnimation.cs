using System.Collections.Generic;
using Module.Role;
using UnityEngine;

namespace Module.UI;

public class DajiAnimation : MonoBehaviour
{
	private enum State
	{
		MovingForward,
		Paused,
		Returning,
		Finished
	}

	private Vector2 _startPos;

	private Vector2 _direction;

	private float _speed;

	private float _maxDistance;

	private bool _shouldReturn;

	private float _pauseDuration;

	private float _suckRadius;

	private float _movedDistance;

	private float _pauseTimer;

	private RectTransform _rectTransform;

	private HashSet<CoinBase> _hitCoins = new HashSet<CoinBase>();

	private Dictionary<CoinBase, Vector2> _suckTargets = new Dictionary<CoinBase, Vector2>();

	private ChallengeMain _challengeMain;

	private MainSceneVat _vatScene;

	private float _vatDistanceScale = 1f;

	private bool _useVatCoins;

	private readonly List<IRoleThrowableVat> _vatQueryScratch = new List<IRoleThrowableVat>();

	private readonly HashSet<IRoleThrowableVat> _hitVatCoins = new HashSet<IRoleThrowableVat>();

	private readonly Dictionary<IRoleThrowableVat, Vector2> _vatSuckTargets = new Dictionary<IRoleThrowableVat, Vector2>();

	private State _state;

	public void SetChallengeMain(ChallengeMain challengeMain)
	{
		_challengeMain = challengeMain;
		_vatScene = null;
		_useVatCoins = false;
	}

	public void Init(Vector2 startPos, Vector2 direction, float speed, float maxDistance, bool shouldReturn, float pauseDuration, float suckRadius)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_rectTransform == (Object)null)
		{
			_rectTransform = ((Component)this).GetComponent<RectTransform>();
		}
		((Transform)_rectTransform).position = Vector2.op_Implicit(startPos);
		_startPos = startPos;
		_direction = ((Vector2)(ref direction)).normalized;
		_speed = speed;
		_maxDistance = maxDistance;
		_shouldReturn = shouldReturn;
		_pauseDuration = pauseDuration;
		_suckRadius = suckRadius;
		_movedDistance = 0f;
		_pauseTimer = 0f;
		_state = State.MovingForward;
		_useVatCoins = false;
		_vatScene = null;
		_hitCoins.Clear();
		_hitVatCoins.Clear();
		_suckTargets.Clear();
		_vatSuckTargets.Clear();
		UpdateRotation();
	}

	public void InitVat(Vector2 startPos, Vector2 direction, float speed, float maxDistance, bool shouldReturn, float pauseDuration, float suckRadius, MainSceneVat vatScene, float distanceScale)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_rectTransform == (Object)null)
		{
			_rectTransform = ((Component)this).GetComponent<RectTransform>();
		}
		((Transform)_rectTransform).position = Vector2.op_Implicit(startPos);
		_startPos = startPos;
		_direction = ((Vector2)(ref direction)).normalized;
		_speed = speed;
		_maxDistance = maxDistance;
		_shouldReturn = shouldReturn;
		_pauseDuration = pauseDuration;
		_suckRadius = suckRadius;
		_vatScene = vatScene;
		_vatDistanceScale = Mathf.Max(0.0001f, distanceScale);
		_challengeMain = null;
		_useVatCoins = true;
		_movedDistance = 0f;
		_pauseTimer = 0f;
		_state = State.MovingForward;
		_hitCoins.Clear();
		_hitVatCoins.Clear();
		_suckTargets.Clear();
		_vatSuckTargets.Clear();
		UpdateRotation();
	}

	private void UpdateRotation()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		if (_direction != Vector2.zero)
		{
			Mathf.Atan2(_direction.y, _direction.x);
		}
	}

	private void Update()
	{
		switch (_state)
		{
		case State.MovingForward:
			MoveForward();
			break;
		case State.Paused:
			PauseAndSuck();
			break;
		case State.Returning:
			ReturnToStart();
			break;
		}
	}

	private void MoveForward()
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		float num = _speed * Time.deltaTime;
		if ((Object)(object)_rectTransform != (Object)null)
		{
			((Transform)_rectTransform).position = Vector2.op_Implicit(Vector2.op_Implicit(((Transform)_rectTransform).position) + _direction * num);
		}
		else
		{
			((Component)this).transform.Translate(Vector2.op_Implicit(_direction * num), (Space)0);
		}
		_movedDistance += num;
		HitCoins();
		if (_movedDistance >= _maxDistance)
		{
			_state = State.Paused;
		}
	}

	private void PauseAndSuck()
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_0266: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0273: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_0295: Unknown result type (might be due to invalid IL or missing references)
		//IL_029a: Unknown result type (might be due to invalid IL or missing references)
		//IL_029f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0303: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_036b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0375: Unknown result type (might be due to invalid IL or missing references)
		//IL_037a: Unknown result type (might be due to invalid IL or missing references)
		//IL_037f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0383: Unknown result type (might be due to invalid IL or missing references)
		//IL_038a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0391: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_040b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0412: Unknown result type (might be due to invalid IL or missing references)
		//IL_0419: Unknown result type (might be due to invalid IL or missing references)
		//IL_0431: Unknown result type (might be due to invalid IL or missing references)
		//IL_0433: Unknown result type (might be due to invalid IL or missing references)
		//IL_0327: Unknown result type (might be due to invalid IL or missing references)
		//IL_032c: Unknown result type (might be due to invalid IL or missing references)
		//IL_032e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0330: Unknown result type (might be due to invalid IL or missing references)
		//IL_034d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0361: Unknown result type (might be due to invalid IL or missing references)
		//IL_033c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0341: Unknown result type (might be due to invalid IL or missing references)
		_pauseTimer += Time.deltaTime;
		if (_useVatCoins)
		{
			PauseAndSuckVatCoins();
			if (_pauseTimer >= _pauseDuration)
			{
				FinishPauseState();
			}
			return;
		}
		if ((Object)(object)_challengeMain != (Object)null)
		{
			_challengeMain.GetSceneBoundary(out var min, out var max);
			float num = 40f;
			min += new Vector2(num, num);
			max -= new Vector2(num, num);
			List<CoinBase> allCoins = _challengeMain.GetAllCoins();
			if (allCoins != null)
			{
				Vector2 val = Vector2.op_Implicit(((Component)this).transform.position);
				for (int i = 0; i < allCoins.Count; i++)
				{
					CoinBase coinBase = allCoins[i];
					if (!((Object)(object)coinBase != (Object)null) || coinBase.State != CoinState.Idle || _hitCoins.Contains(coinBase) || !(Vector2.Distance(val, Vector2.op_Implicit(((Component)coinBase).transform.position)) <= _suckRadius))
					{
						continue;
					}
					if (!_suckTargets.ContainsKey(coinBase))
					{
						Vector2 val2 = Random.insideUnitCircle;
						if (val2 == Vector2.zero)
						{
							val2 = Vector2.right;
						}
						_suckTargets[coinBase] = ((Vector2)(ref val2)).normalized * Random.Range(40f, 100f);
					}
					Vector2 val3 = val + _suckTargets[coinBase];
					val3.x = Mathf.Clamp(val3.x, min.x, max.x);
					val3.y = Mathf.Clamp(val3.y, min.y, max.y);
					Vector2 val4 = Vector2.MoveTowards(Vector2.op_Implicit(((Component)coinBase).transform.position), val3, 400f * Time.deltaTime);
					val4.x = Mathf.Clamp(val4.x, min.x, max.x);
					val4.y = Mathf.Clamp(val4.y, min.y, max.y);
					((Component)coinBase).transform.position = Vector2.op_Implicit(val4);
				}
			}
		}
		else
		{
			MainScene mainScene = UIManager.Instance.MainScene;
			if ((Object)(object)mainScene != (Object)null)
			{
				mainScene.GetSceneBoundary(out var min2, out var max2);
				float num2 = 40f;
				min2 += new Vector2(num2, num2);
				max2 -= new Vector2(num2, num2);
				List<CoinBase> allCoins2 = mainScene.GetAllCoins();
				if (allCoins2 != null)
				{
					Vector2 val5 = Vector2.op_Implicit(((Component)this).transform.position);
					for (int j = 0; j < allCoins2.Count; j++)
					{
						CoinBase coinBase2 = allCoins2[j];
						if (!((Object)(object)coinBase2 != (Object)null) || coinBase2.State != CoinState.Idle || _hitCoins.Contains(coinBase2) || !SingletonMono<UpgradeManager>.Instance.CanRoleFlipCoin(coinBase2.coinType) || !(Vector2.Distance(val5, Vector2.op_Implicit(((Component)coinBase2).transform.position)) <= _suckRadius))
						{
							continue;
						}
						if (!_suckTargets.ContainsKey(coinBase2))
						{
							Vector2 val6 = Random.insideUnitCircle;
							if (val6 == Vector2.zero)
							{
								val6 = Vector2.right;
							}
							_suckTargets[coinBase2] = ((Vector2)(ref val6)).normalized * Random.Range(40f, 100f);
						}
						Vector2 val7 = val5 + _suckTargets[coinBase2];
						val7.x = Mathf.Clamp(val7.x, min2.x, max2.x);
						val7.y = Mathf.Clamp(val7.y, min2.y, max2.y);
						Vector2 val8 = Vector2.MoveTowards(Vector2.op_Implicit(((Component)coinBase2).transform.position), val7, 400f * Time.deltaTime);
						val8.x = Mathf.Clamp(val8.x, min2.x, max2.x);
						val8.y = Mathf.Clamp(val8.y, min2.y, max2.y);
						((Component)coinBase2).transform.position = Vector2.op_Implicit(val8);
					}
				}
			}
		}
		if (_pauseTimer >= _pauseDuration)
		{
			if (_shouldReturn)
			{
				_state = State.Returning;
				_hitCoins.Clear();
				_suckTargets.Clear();
			}
			else
			{
				_state = State.Finished;
				((Component)this).gameObject.SetActive(false);
			}
		}
	}

	private void ReturnToStart()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = (((Object)(object)_rectTransform != (Object)null) ? Vector2.op_Implicit(((Transform)_rectTransform).position) : Vector2.op_Implicit(((Component)this).transform.position));
		Vector2 val2 = _startPos - val;
		Vector2 normalized = ((Vector2)(ref val2)).normalized;
		float num = _speed * Time.deltaTime;
		if (Vector2.Distance(val, _startPos) <= num)
		{
			if ((Object)(object)_rectTransform != (Object)null)
			{
				((Transform)_rectTransform).position = Vector2.op_Implicit(_startPos);
			}
			else
			{
				((Component)this).transform.position = Vector2.op_Implicit(_startPos);
			}
			_state = State.Finished;
			((Component)this).gameObject.SetActive(false);
		}
		else if ((Object)(object)_rectTransform != (Object)null)
		{
			((Transform)_rectTransform).position = Vector2.op_Implicit(val + normalized * num);
		}
		else
		{
			((Component)this).transform.position = Vector2.op_Implicit(val + normalized * num);
		}
		HitCoins();
	}

	private void HitCoins()
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		if (_useVatCoins)
		{
			HitVatCoins();
			return;
		}
		List<CoinBase> allCoins;
		if ((Object)(object)_challengeMain != (Object)null)
		{
			allCoins = _challengeMain.GetAllCoins();
		}
		else
		{
			MainScene mainScene = UIManager.Instance.MainScene;
			if ((Object)(object)mainScene == (Object)null)
			{
				return;
			}
			allCoins = mainScene.GetAllCoins();
		}
		if (allCoins == null)
		{
			return;
		}
		Vector2 val = Vector2.op_Implicit(((Component)this).transform.position);
		float num = 80f;
		for (int i = 0; i < allCoins.Count; i++)
		{
			CoinBase coinBase = allCoins[i];
			bool flag = (Object)(object)_challengeMain != (Object)null || ((Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null && SingletonMono<UpgradeManager>.Instance.CanRoleFlipCoin(coinBase.coinType));
			if ((Object)(object)coinBase != (Object)null && coinBase.State == CoinState.Idle && !_hitCoins.Contains(coinBase) && flag && Vector2.Distance(val, Vector2.op_Implicit(((Component)coinBase).transform.position)) <= num)
			{
				_hitCoins.Add(coinBase);
				Vector2 targetPos = Vector2.op_Implicit(((Component)coinBase).transform.position + Vector2.op_Implicit(Random.insideUnitCircle * 50f));
				coinBase.JumpToTarget(Vector2.op_Implicit(((Component)coinBase).transform.position), targetPos, 7);
			}
		}
	}

	private void FinishPauseState()
	{
		if (_shouldReturn)
		{
			_state = State.Returning;
			_hitCoins.Clear();
			_hitVatCoins.Clear();
			_suckTargets.Clear();
			_vatSuckTargets.Clear();
		}
		else
		{
			_state = State.Finished;
			((Component)this).gameObject.SetActive(false);
		}
	}

	private void PauseAndSuckVatCoins()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_vatScene == (Object)null)
		{
			return;
		}
		_vatScene.GetSceneBoundary(out var min, out var max);
		float num = ScaleVatDistance(40f);
		min += new Vector2(num, num);
		max -= new Vector2(num, num);
		Vector2 val = Vector2.op_Implicit(((Component)this).transform.position);
		float radius = ScaleVatDistance(_suckRadius);
		_vatQueryScratch.Clear();
		_vatScene.GetIdleThrowablesInRadius(val, radius, _vatQueryScratch);
		bool flag = false;
		for (int i = 0; i < _vatQueryScratch.Count; i++)
		{
			IRoleThrowableVat roleThrowableVat = _vatQueryScratch[i];
			if (roleThrowableVat == null || _hitVatCoins.Contains(roleThrowableVat))
			{
				continue;
			}
			bool flag2 = true;
			if (roleThrowableVat is CoinVat coinVat)
			{
				flag2 = CanFlipVatCoin(coinVat.coinType);
			}
			if (!flag2)
			{
				continue;
			}
			if (!_vatSuckTargets.ContainsKey(roleThrowableVat))
			{
				Vector2 val2 = Random.insideUnitCircle;
				if (val2 == Vector2.zero)
				{
					val2 = Vector2.right;
				}
				_vatSuckTargets[roleThrowableVat] = ((Vector2)(ref val2)).normalized * Random.Range(ScaleVatDistance(40f), ScaleVatDistance(100f));
			}
			Vector2 val3 = val + _vatSuckTargets[roleThrowableVat];
			val3.x = Mathf.Clamp(val3.x, min.x, max.x);
			val3.y = Mathf.Clamp(val3.y, min.y, max.y);
			Vector2 val4 = Vector2.MoveTowards(Vector2.op_Implicit(roleThrowableVat.VatTransform.position), val3, ScaleVatDistance(400f) * Time.deltaTime);
			val4.x = Mathf.Clamp(val4.x, min.x, max.x);
			val4.y = Mathf.Clamp(val4.y, min.y, max.y);
			roleThrowableVat.VatTransform.position = Vector2.op_Implicit(val4);
			flag = true;
		}
		if (flag)
		{
			_vatScene.InvalidateCoinSpatialIndex();
		}
	}

	private void HitVatCoins()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_vatScene == (Object)null)
		{
			return;
		}
		Vector2 center = Vector2.op_Implicit(((Component)this).transform.position);
		float radius = ScaleVatDistance(80f);
		_vatQueryScratch.Clear();
		_vatScene.GetIdleThrowablesInRadius(center, radius, _vatQueryScratch);
		for (int i = 0; i < _vatQueryScratch.Count; i++)
		{
			IRoleThrowableVat roleThrowableVat = _vatQueryScratch[i];
			if (roleThrowableVat != null && !_hitVatCoins.Contains(roleThrowableVat))
			{
				bool flag = true;
				if (roleThrowableVat is CoinVat coinVat)
				{
					flag = CanFlipVatCoin(coinVat.coinType);
				}
				if (flag)
				{
					_hitVatCoins.Add(roleThrowableVat);
					Vector2 targetPosition = Vector2.op_Implicit(roleThrowableVat.VatTransform.position) + Random.insideUnitCircle * ScaleVatDistance(50f);
					roleThrowableVat.TryThrowByRole(Vector2.op_Implicit(roleThrowableVat.VatTransform.position), targetPosition, 7);
				}
			}
		}
	}

	private bool CanFlipVatCoin(CoinType coinType)
	{
		if (!((Object)(object)SingletonMono<UpgradeManager>.Instance == (Object)null))
		{
			return SingletonMono<UpgradeManager>.Instance.CanRoleFlipCoin(coinType);
		}
		return true;
	}

	private float ScaleVatDistance(float value)
	{
		return value * _vatDistanceScale;
	}
}
