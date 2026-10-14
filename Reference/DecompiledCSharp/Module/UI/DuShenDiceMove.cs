using System;
using System.Collections.Generic;
using Module.Role;
using Module.Vat;
using Spine.Unity;
using UnityEngine;
using cfg;

namespace Module.UI;

public class DuShenDiceMove : MonoBehaviour
{
	[Header("移动设置")]
	[Tooltip("移动速度（设计分辨率单位/秒，通过 distanceScale 转为世界单位）")]
	[SerializeField]
	private float moveSpeed = 400f;

	[Tooltip("基础移动距离（设计分辨率单位，通过 distanceScale 转为世界单位）")]
	[SerializeField]
	private float baseMoveDistance = 300f;

	[Header("骰子初始大小")]
	[SerializeField]
	private float sourceScale = 1f;

	[Header("检测硬币的初始半径")]
	[SerializeField]
	private float sourceDetectRadius = 100f;

	private float detectRadius = 100f;

	[Tooltip("检测间隔（秒）")]
	[SerializeField]
	private float checkInterval = 0.02f;

	private Vector2 _startPos;

	private Vector2 _direction;

	private float _maxDistance;

	private float _movedDistance;

	private float _timer;

	private MainSceneVat _vatScene;

	private float _vatDistanceScale = 1f;

	private int _dicePoint;

	private Action<IRoleThrowableVat> _tossCoinCallback;

	private readonly List<IRoleThrowableVat> _vatQueryScratch = new List<IRoleThrowableVat>();

	private readonly HashSet<IRoleThrowableVat> _hitCoins = new HashSet<IRoleThrowableVat>();

	private SpineVat _spineVat;

	public void InitVat(Vector2 startPos, Vector2 direction, int helperId, int helperLevel, int dicePoint, Vector2 boundaryMin, Vector2 boundaryMax, Action<IRoleThrowableVat> tossCoinCallback, MainSceneVat vatScene, float distanceScale, SpineVatAsset vatAsset)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		_startPos = startPos;
		_direction = ((Vector2)(ref direction)).normalized;
		_dicePoint = Mathf.Max(0, dicePoint);
		_tossCoinCallback = tossCoinCallback;
		_vatScene = vatScene;
		_vatDistanceScale = Mathf.Max(0.0001f, distanceScale);
		float num = baseMoveDistance;
		if (ConfigManager.HasInstance() && helperId > 0)
		{
			helperRole helperRoleConfig = ConfigManager.Instance.GetHelperRoleConfig(helperId);
			if (helperRoleConfig != null && helperRoleConfig.BaseSkillNum > 0f)
			{
				int helperSkillLevel = Constants.GetHelperSkillLevel(helperRoleConfig, helperLevel);
				float num2 = helperRoleConfig.UpgradeSkillNum / helperRoleConfig.BaseSkillNum;
				num = baseMoveDistance + (float)helperSkillLevel * num2 * baseMoveDistance;
			}
		}
		_maxDistance = num * _vatDistanceScale;
		_movedDistance = 0f;
		_timer = checkInterval;
		_hitCoins.Clear();
		Vector3 position = ((Component)this).transform.position;
		position.x = startPos.x;
		position.y = startPos.y;
		((Component)this).transform.position = position;
		float zAngleForDirection = GetZAngleForDirection(_direction);
		((Component)this).transform.rotation = Quaternion.Euler(0f, 0f, zAngleForDirection);
		if ((Object)(object)_spineVat == (Object)null)
		{
			_spineVat = ((Component)this).GetComponent<SpineVat>();
			if ((Object)(object)_spineVat == (Object)null)
			{
				_spineVat = ((Component)this).gameObject.AddComponent<SpineVat>();
			}
		}
		SkeletonGraphic component = ((Component)this).GetComponent<SkeletonGraphic>();
		if ((Object)(object)component != (Object)null)
		{
			((Behaviour)component).enabled = false;
		}
		SkeletonGraphic[] componentsInChildren = ((Component)this).GetComponentsInChildren<SkeletonGraphic>(true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			((Behaviour)componentsInChildren[i]).enabled = false;
		}
		if ((Object)(object)_spineVat != (Object)null && (Object)(object)vatAsset != (Object)null)
		{
			_spineVat.UseExternalTick = false;
			_spineVat.SetVatShader(((Object)(object)vatScene != (Object)null) ? vatScene.SpineVatShader : null);
			_spineVat.SetSortingLayerAndOrder(((Object)(object)vatScene != (Object)null) ? vatScene.VatSortingLayerName : string.Empty, ((Object)(object)vatScene != (Object)null) ? vatScene.EffectSortingOrder : 0);
			_spineVat.SetDepthSortRange(boundaryMin.y, boundaryMax.y);
			_spineVat.SetDepthSortEnabled(enabled: true);
			_spineVat.SetRendererSortingOrderByYEnabled(enabled: true);
			_spineVat.SetIndirectRenderingEnabled(enabled: false);
			_spineVat.SetVatAssetNoPlay(vatAsset);
			_spineVat.SetRenderingVisible(visible: true);
		}
		PlayEffectAnimation();
	}

	private void Update()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		if (moveSpeed <= 0f || _direction == Vector2.zero)
		{
			((Component)this).gameObject.SetActive(false);
			return;
		}
		float num = moveSpeed * _vatDistanceScale * Time.deltaTime;
		Vector3 position = ((Component)this).transform.position + Vector2.op_Implicit(_direction * num);
		((Component)this).transform.position = position;
		_movedDistance += num;
		_timer += Time.deltaTime;
		if (_timer >= checkInterval)
		{
			_timer = 0f;
			DetectAndTossCoins();
		}
		if (_movedDistance >= _maxDistance)
		{
			((Component)this).gameObject.SetActive(false);
		}
	}

	private void DetectAndTossCoins()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_vatScene == (Object)null || _tossCoinCallback == null)
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
			if (roleThrowableVat == null || _hitCoins.Contains(roleThrowableVat))
			{
				continue;
			}
			bool flag = true;
			if (roleThrowableVat is CoinVat coinVat)
			{
				flag = CanFlipVatCoin(coinVat.coinType);
			}
			if (flag)
			{
				_hitCoins.Add(roleThrowableVat);
				if (roleThrowableVat is CoinVat coinVat2)
				{
					coinVat2.DicePoint = _dicePoint;
				}
				_tossCoinCallback(roleThrowableVat);
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

	private float GetZAngleForDirection(Vector2 dir)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		if (dir == Vector2.up)
		{
			return 90f;
		}
		if (dir == Vector2.down)
		{
			return -90f;
		}
		if (dir == Vector2.left)
		{
			return 180f;
		}
		_ = dir == Vector2.right;
		return 0f;
	}

	private void PlayEffectAnimation()
	{
		if ((Object)(object)_spineVat != (Object)null)
		{
			_spineVat.Play("effect", shouldLoop: true);
		}
	}

	private void OnEnable()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		float num = TalentDataManager.Instance?.GetDiceTalentBuff(305) ?? 1f;
		((Component)this).transform.localScale = Vector3.one * sourceScale * num;
		detectRadius = sourceDetectRadius * num;
		_movedDistance = 0f;
		_timer = checkInterval;
		_hitCoins.Clear();
		PlayEffectAnimation();
	}
}
