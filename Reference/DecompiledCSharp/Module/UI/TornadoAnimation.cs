using System.Collections.Generic;
using Module.Role;
using UnityEngine;

namespace Module.UI;

public class TornadoAnimation : MonoBehaviour
{
	[Header("旋风设置")]
	[Tooltip("检测硬币的半径")]
	[SerializeField]
	private float detectRadius = 150f;

	[Tooltip("硬币落地时的最大偏移半径（0表示原地落下）")]
	[SerializeField]
	private float scatterRadius = 50f;

	[Tooltip("检测间隔（秒），避免每帧检测带来的性能消耗")]
	[SerializeField]
	private float checkInterval = 0.1f;

	[Header("移动设置")]
	[Tooltip("移动方向（内部会自动归一化）")]
	[SerializeField]
	private Vector2 moveDirection = Vector2.zero;

	[Tooltip("移动速度（单位/秒）")]
	[SerializeField]
	private float moveSpeed;

	[Tooltip("最大移动距离（小于等于0表示不限制距离）")]
	[SerializeField]
	private float maxMoveDistance = 1500f;

	private float _timer;

	private float _movedDistance;

	private RectTransform _rectTransform;

	private ChallengeMain _challengeMain;

	private MainSceneVat _vatScene;

	private float _vatDistanceScale = 1f;

	private readonly List<IRoleThrowableVat> _vatQueryScratch = new List<IRoleThrowableVat>();

	public void SetChallengeMain(ChallengeMain challengeMain)
	{
		_challengeMain = challengeMain;
		_vatScene = null;
	}

	public void SetVatScene(MainSceneVat vatScene, float distanceScale)
	{
		_vatScene = vatScene;
		_vatDistanceScale = Mathf.Max(0.0001f, distanceScale);
		_challengeMain = null;
	}

	public void SetMovement(Vector2 direction, float speed, float maxDistance = -1f)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		moveDirection = ((Vector2)(ref direction)).normalized;
		moveSpeed = speed;
		if (maxDistance > 0f)
		{
			maxMoveDistance = maxDistance;
		}
		_movedDistance = 0f;
		_timer = 0f;
	}

	private void Awake()
	{
		_rectTransform = ((Component)this).GetComponent<RectTransform>();
	}

	private void Update()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		if (moveSpeed > 0f && moveDirection != Vector2.zero)
		{
			float num = moveSpeed * Time.deltaTime * 100f;
			if ((Object)(object)_rectTransform != (Object)null)
			{
				Vector2 anchoredPosition = _rectTransform.anchoredPosition + moveDirection * num;
				_rectTransform.anchoredPosition = anchoredPosition;
			}
			else
			{
				((Component)this).transform.Translate(Vector2.op_Implicit(moveDirection * num), (Space)0);
			}
			if (maxMoveDistance > 0f)
			{
				_movedDistance += num;
				if (_movedDistance >= maxMoveDistance)
				{
					((Component)this).gameObject.SetActive(false);
					return;
				}
			}
		}
		_timer += Time.deltaTime;
		if (_timer >= checkInterval)
		{
			_timer = 0f;
			DetectAndTossCoins();
		}
	}

	private void DetectAndTossCoins()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_vatScene != (Object)null)
		{
			DetectAndTossVatCoins();
			return;
		}
		Vector2 val = Vector2.op_Implicit(((Component)this).transform.position);
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
		for (int i = 0; i < allCoins.Count; i++)
		{
			CoinBase coinBase = allCoins[i];
			bool flag = (Object)(object)_challengeMain != (Object)null || ((Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null && SingletonMono<UpgradeManager>.Instance.CanRoleFlipCoin(coinBase.coinType));
			if ((Object)(object)coinBase != (Object)null && coinBase.State == CoinState.Idle && flag && Vector2.Distance(val, Vector2.op_Implicit(((Component)coinBase).transform.position)) <= detectRadius)
			{
				TossCoin(coinBase);
			}
		}
	}

	private void TossCoin(CoinBase coin)
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
		Vector2 val = Vector2.op_Implicit(((Component)coin).transform.position);
		Vector2 val2 = Random.insideUnitCircle * scatterRadius;
		Vector2 targetPos = val + val2;
		coin.JumpToTarget(val, targetPos, 2);
	}

	private void DetectAndTossVatCoins()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_vatScene == (Object)null)
		{
			return;
		}
		Vector2 center = Vector2.op_Implicit(((Component)this).transform.position);
		float radius = ScaleVatDistance(detectRadius);
		_vatQueryScratch.Clear();
		_vatScene.GetIdleThrowablesInRadius(center, radius, _vatQueryScratch);
		for (int i = 0; i < _vatQueryScratch.Count; i++)
		{
			IRoleThrowableVat roleThrowableVat = _vatQueryScratch[i];
			if (roleThrowableVat != null)
			{
				bool flag = true;
				if (roleThrowableVat is CoinVat coinVat)
				{
					flag = CanFlipVatCoin(coinVat.coinType);
				}
				if (flag)
				{
					TossVatCoin(roleThrowableVat);
				}
			}
		}
	}

	private void TossVatCoin(IRoleThrowableVat throwable)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = Vector2.op_Implicit(throwable.VatTransform.position);
		Vector2 val2 = Random.insideUnitCircle * ScaleVatDistance(scatterRadius);
		Vector2 targetPosition = val + val2;
		throwable.TryThrowByRole(val, targetPosition, 2);
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

	private void OnDrawGizmosSelected()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
		Gizmos.DrawSphere(((Component)this).transform.position, detectRadius);
	}
}
