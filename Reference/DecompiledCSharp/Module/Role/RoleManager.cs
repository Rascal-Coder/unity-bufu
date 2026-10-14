using System;
using System.Collections.Generic;
using Spine;
using Spine.Unity;
using UnityEngine;

namespace Module.Role;

public class RoleManager : SingletonMono<RoleManager>
{
	private GameObject[] _rolePrefab = (GameObject[])(object)new GameObject[10];

	private Transform _roleParent;

	private List<Role> _activeRoles = new List<Role>();

	public const int ROLE_UPGRADE_ITEM_ID = 1;

	public const int COPPER_COIN_UPGRADE_ITEM_ID = 2;

	public const int SILVER_COIN_UPGRADE_ITEM_ID = 3;

	public const int GOLD_COIN_UPGRADE_ITEM_ID = 4;

	public const int PURPLE_COIN_UPGRADE_ITEM_ID = 5;

	[Header("帮手生成设置")]
	[Tooltip("帮手之间的最小间隔（像素/单位）")]
	[SerializeField]
	private float minRoleDistance = 50f;

	[Tooltip("帮手生成位置在场景上方的偏移量")]
	[SerializeField]
	private float spawnHeightOffset = 200f;

	private Vector2 _boundaryMin;

	private Vector2 _boundaryMax;

	public int sleepCount;

	public bool isSleepTipActive;

	public override bool DontDestory => false;

	protected override void OnAwake()
	{
		Debug.Log((object)"RoleManager.OnAwake() 被调用");
		TryFindRolePoolAndBoundary();
		LoadPrefab();
	}

	private void TryFindRolePoolAndBoundary()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Expected O, but got Unknown
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		if (TryGetVatMainScene(out var mainSceneVat))
		{
			_roleParent = mainSceneVat.GetRolePool();
			mainSceneVat.GetSceneBoundary(out _boundaryMin, out _boundaryMax);
			Debug.Log((object)$"RoleManager 获取到 VAT RolePool: {_roleParent}, 边界: {_boundaryMin} ~ {_boundaryMax}");
			return;
		}
		MainScene mainScene = UIManager.Instance.MainScene;
		if ((Object)(object)mainScene != (Object)null)
		{
			_roleParent = mainScene.GetRolePool();
			mainScene.GetSceneBoundary(out _boundaryMin, out _boundaryMax);
			Debug.Log((object)$"RoleManager 获取到 RolePool: {_roleParent}, 边界: {_boundaryMin} ~ {_boundaryMax}");
		}
		if ((Object)(object)_roleParent == (Object)null)
		{
			Debug.LogWarning((object)"RoleManager 未找到 RolePool，使用默认父节点");
			GameObject val = new GameObject("RoleContainer");
			val.transform.SetParent(((Component)this).transform);
			_roleParent = val.transform;
			_boundaryMin = new Vector2(-400f, -800f);
			_boundaryMax = new Vector2(400f, 0f);
		}
	}

	private void LoadPrefab()
	{
	}

	protected override void BeforOnDestroy()
	{
		foreach (Role activeRole in _activeRoles)
		{
			if ((Object)(object)activeRole != (Object)null)
			{
				Object.Destroy((Object)(object)((Component)activeRole).gameObject);
			}
		}
		_activeRoles.Clear();
	}

	public void SpawnRole(Action<Role> onSpawned = null, int helperId = 0)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		if (TryGetVatMainScene(out var mainSceneVat))
		{
			mainSceneVat.SpawnRole(delegate
			{
				onSpawned?.Invoke(null);
			}, helperId);
			return;
		}
		if ((Object)(object)_roleParent == (Object)null || _boundaryMin == Vector2.zero)
		{
			TryFindRolePoolAndBoundary();
		}
		if ((Object)(object)_rolePrefab[helperId] != (Object)null)
		{
			CreateRoleInstance(onSpawned, helperId);
			return;
		}
		SingletonMono<ResManager>.Instance.LoadPrefabAsync(Constants.GetRolePrefabPath(helperId), delegate(GameObject prefab)
		{
			_rolePrefab[helperId] = prefab;
			if ((Object)(object)_rolePrefab[helperId] != (Object)null)
			{
				CreateRoleInstance(onSpawned, helperId);
			}
			else
			{
				Debug.LogError((object)("Role 预制体加载失败: " + Constants.GetRolePrefabPath(helperId)));
				onSpawned?.Invoke(null);
			}
		});
	}

	private void CreateRoleInstance(Action<Role> onSpawned, int helperId = 0)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_roleParent == (Object)null)
		{
			TryFindRolePoolAndBoundary();
		}
		Vector2 spawnPositionWithInterval = GetSpawnPositionWithInterval();
		GameObject val = Object.Instantiate<GameObject>((_rolePrefab.Length > helperId) ? _rolePrefab[helperId] : _rolePrefab[0], Vector2.op_Implicit(spawnPositionWithInterval), Quaternion.identity, _roleParent);
		Role component = val.GetComponent<Role>();
		CreateParticle("Assets/GameRes/Role/helperAdd.prefab", val.transform, Vector3.zero);
		if ((Object)(object)component != (Object)null)
		{
			component.Initialize(helperId, _boundaryMin, _boundaryMax);
			_activeRoles.Add(component);
		}
		else
		{
			Debug.LogError((object)"Role 组件未找到！");
		}
		onSpawned?.Invoke(component);
	}

	private Vector2 GetSpawnPositionWithInterval()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		int num = 50;
		for (int i = 0; i < num; i++)
		{
			Vector2 randomPositionInBoundary = GetRandomPositionInBoundary();
			bool flag = false;
			foreach (Role activeRole in _activeRoles)
			{
				if ((Object)(object)activeRole != (Object)null && Vector2.Distance(randomPositionInBoundary, Vector2.op_Implicit(((Component)activeRole).transform.position)) < minRoleDistance)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return randomPositionInBoundary;
			}
		}
		Debug.LogWarning((object)"无法找到无重叠的帮手位置，使用随机位置");
		return GetRandomPositionInBoundary();
	}

	private Vector2 GetRandomPositionInBoundary()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		float num = 30f;
		float num2 = Random.Range(_boundaryMin.x + num, _boundaryMax.x - num);
		float num3 = Random.Range(_boundaryMin.y + num, _boundaryMax.y - num);
		return new Vector2(num2, num3);
	}

	private Vector2 GetStartPosition(Vector2 targetPos)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(targetPos.x, _boundaryMax.y + spawnHeightOffset);
	}

	public void RemoveRole(Role role)
	{
		if (!((Object)(object)role == (Object)null))
		{
			_activeRoles.Remove(role);
			Object.Destroy((Object)(object)((Component)role).gameObject);
		}
	}

	public void RemoveAllRole()
	{
		if (TryGetVatMainScene(out var mainSceneVat))
		{
			mainSceneVat.RemoveAllRole();
			return;
		}
		Role[] array = _activeRoles.ToArray();
		foreach (Role role in array)
		{
			RemoveRole(role);
		}
		_activeRoles.Clear();
	}

	public int GetRoleCount()
	{
		if (TryGetVatMainScene(out var mainSceneVat))
		{
			return mainSceneVat.GetRoleCount();
		}
		return _activeRoles.Count;
	}

	public List<Role> GetAllRoles()
	{
		return new List<Role>(_activeRoles);
	}

	public bool IsPrefabLoaded()
	{
		return true;
	}

	public void ChangeRole(int pre, int target)
	{
		if (TryGetVatMainScene(out var mainSceneVat))
		{
			mainSceneVat.ChangeRole(pre, target);
			return;
		}
		Role role = null;
		for (int num = _activeRoles.Count - 1; num >= 0; num--)
		{
			if (_activeRoles[num].roleType == pre)
			{
				role = _activeRoles[num];
			}
		}
		if ((Object)(object)role != (Object)null)
		{
			RemoveRole(role);
		}
		SpawnRole(null, target);
	}

	public void RefreshAllRoleSleepProbability()
	{
		if (TryGetVatMainScene(out var mainSceneVat))
		{
			mainSceneVat.RefreshAllRoleSleepProbability();
			return;
		}
		foreach (Role allRole in GetAllRoles())
		{
			if ((Object)(object)allRole != (Object)null)
			{
				allRole.RefreshSleepProbability();
			}
		}
	}

	private bool TryGetVatMainScene(out MainSceneVat mainSceneVat)
	{
		mainSceneVat = null;
		if (!UIManager.HasInstance() || !UIManager.Instance.IsVatMainSceneActive)
		{
			return false;
		}
		mainSceneVat = UIManager.Instance.MainSceneVat;
		return (Object)(object)mainSceneVat != (Object)null;
	}

	public int GetSleepCount()
	{
		return sleepCount;
	}

	public void AddSleepCount()
	{
		sleepCount++;
	}

	public void SetSleepTipActive(bool active)
	{
		isSleepTipActive = active;
	}

	public bool GetSleepTipActive()
	{
		return isSleepTipActive;
	}

	public void CreateParticle(string path, Transform parent, Vector3 localPosition)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		SingletonMono<ResManager>.Instance.LoadPrefabAsync(path, delegate(GameObject prefab)
		{
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Expected O, but got Unknown
			if ((Object)(object)Object.FindObjectOfType<MainScene>() != (Object)null)
			{
				GameObject creatParticle = Object.Instantiate<GameObject>(prefab, parent);
				creatParticle.transform.localPosition = localPosition;
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
}
