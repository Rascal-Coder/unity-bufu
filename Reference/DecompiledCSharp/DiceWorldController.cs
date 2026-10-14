using System;
using System.Collections;
using System.Collections.Generic;
using Module.Role;
using Module.UI;
using Module.Vat;
using UnityEngine;
using YooAsset;
using cfg;

public sealed class DiceWorldController : MonoBehaviour, IWorldGameplayController, IRoleThrowableWorld
{
	private const string SpineVatShaderPath = "Assets/GameRes/VAT/SpineVatUnlit.shader";

	private static readonly string[] DiceVatAssetPaths = new string[5] { "Assets/GameRes/VAT/dice_copper_SkeletonData_VAT.asset", "Assets/GameRes/VAT/dice_silver_SkeletonData_VAT.asset", "Assets/GameRes/VAT/dice_gold_SkeletonData_VAT.asset", "Assets/GameRes/VAT/dice_purple_SkeletonData_VAT.asset", "Assets/GameRes/VAT/dice_diamond_SkeletonData_VAT.asset" };

	private const int DiceGuideWorldId = 2;

	private const int DiceGuideFirstDiceIndex = 0;

	private const int DiceGuideSecondDiceIndex = 1;

	private const int DiceGuideFirstFixedFace = 1;

	private const int DiceGuideSecondFixedFace = 6;

	private const string DiceGuideClickFirstText = "点击投掷骰子，根据投掷的点数获取奖励";

	private const string DiceGuideFirstLandedText = "投出了1点，获得了1倍奖励";

	private const string DiceGuideSecondLandedText = "6点可以获得6倍奖励！";

	private readonly List<int> qualityIds = new List<int>();

	private readonly List<DiceVat> activeDice = new List<DiceVat>();

	private readonly Dictionary<int, AssetHandle> diceVatHandles = new Dictionary<int, AssetHandle>();

	private readonly Dictionary<int, SpineVatAsset> diceVatAssets = new Dictionary<int, SpineVatAsset>();

	private readonly Dictionary<int, int> diceSyncVersions = new Dictionary<int, int>();

	private MainSceneVat scene;

	private AssetHandle spineShaderHandle;

	private Shader spineVatShader;

	private Shader spriteVatShader;

	private Coroutine contentCoroutine;

	private int contentVersion;

	private bool upgradeSubscribed;

	private bool deskSubscribed;

	private int diceGuideStep;

	private DiceVat diceGuideWaitingDice;

	private const float EvolutionMergeDuration = 1f;

	public int GameplayType => 2;

	public int WorldId { get; private set; }

	public MonoBehaviour ViewBehaviour => (MonoBehaviour)(object)scene;

	public IMainSceneView LegacyMainSceneView => scene;

	public int ActiveDiceCount => activeDice.Count;

	private bool IsDiceGuideActive
	{
		get
		{
			if (diceGuideStep > 0 && WorldId == 2)
			{
				return PlayerPrefs.GetInt("dice_world_guide_step", 0) == 0;
			}
			return false;
		}
	}

	public void BindView(MainSceneVat sceneView)
	{
		scene = sceneView;
	}

	public bool CanHandle(WorldGameplayContext context)
	{
		if (context != null && context.GameplayType == GameplayType)
		{
			return (Object)(object)scene != (Object)null;
		}
		return false;
	}

	public void EnterWorld(WorldGameplayContext context)
	{
		ActivateWorld(context);
	}

	public void ReloadWorld(WorldGameplayContext context)
	{
		ActivateWorld(context);
	}

	public void ExitWorld()
	{
		StopDiceGuide(saveComplete: false);
		UnsubscribeEvents();
		if ((Object)(object)scene != (Object)null)
		{
			scene.ClearRuntimeContent();
			scene.SetDiceWorldController(null);
			scene.SetVatWorldVisible(visible: false);
		}
		else
		{
			ClearThrowables();
		}
		WorldId = 0;
	}

	private void ActivateWorld(WorldGameplayContext context)
	{
		if (CanHandle(context))
		{
			((Component)scene).gameObject.SetActive(true);
			scene.SetVatWorldVisible(visible: true);
			scene.ClearRuntimeContent();
			WorldId = context.WorldId;
			scene.SetDiceWorldController(this);
			SubscribeEvents();
			ResolveQualities();
			scene.InitializeCoins();
			scene.InitializeRoles();
		}
	}

	private void OnDestroy()
	{
		UnsubscribeEvents();
		ClearThrowables();
		foreach (KeyValuePair<int, AssetHandle> diceVatHandle in diceVatHandles)
		{
			AssetHandle value = diceVatHandle.Value;
			if (value != null)
			{
				((HandleBase)value).Release();
			}
		}
		diceVatHandles.Clear();
		diceVatAssets.Clear();
		AssetHandle obj = spineShaderHandle;
		if (obj != null)
		{
			((HandleBase)obj).Release();
		}
		spineShaderHandle = null;
	}

	public void InitializeThrowables()
	{
		if (WorldId > 0 && !((Object)(object)scene == (Object)null))
		{
			ResolveQualities();
			RebuildWorldContent();
		}
	}

	public bool TryRefreshDicePopulationForSpawnUpgrade(int upgradeItemId, bool? forceSpawnAtCenter = null, int? forcedDesired = null, int? forcedEvolutionTier = null)
	{
		if (WorldId <= 0 || upgradeItemId <= 0)
		{
			return false;
		}
		ResolveQualities();
		int qualityBySpawnUpgradeId = GetQualityBySpawnUpgradeId(upgradeItemId);
		if (qualityBySpawnUpgradeId == 0)
		{
			return false;
		}
		bool flag = forceSpawnAtCenter ?? ((Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null && UpgradeManager.IsEvolvableCurrencyUpgrade(upgradeItemId) && SingletonMono<UpgradeManager>.Instance.IsEvolutionLevel(upgradeItemId, SingletonMono<UpgradeManager>.Instance.GetUpgradeLevel(upgradeItemId, "itemUpgrade1")));
		int currentEvolutionTier = GetCurrentEvolutionTier(qualityBySpawnUpgradeId);
		int num = ((!flag) ? currentEvolutionTier : (forcedEvolutionTier ?? (currentEvolutionTier + 1)));
		bool num2 = !HasLoadedDiceAsset(qualityBySpawnUpgradeId);
		bool flag2 = qualityBySpawnUpgradeId >= 102 && qualityBySpawnUpgradeId <= 105 && num > 0 && VatRuntimeAssets.Instance != null && !VatRuntimeAssets.Instance.IsDiceVatAssetLoaded(qualityBySpawnUpgradeId, num);
		int value;
		int num3 = ((!diceSyncVersions.TryGetValue(qualityBySpawnUpgradeId, out value)) ? 1 : (value + 1));
		diceSyncVersions[qualityBySpawnUpgradeId] = num3;
		if (num2 || flag2)
		{
			((MonoBehaviour)this).StartCoroutine(EnsureDiceAssetsAndSync(qualityBySpawnUpgradeId, flag, forcedDesired, forcedEvolutionTier, currentEvolutionTier, num, num3, contentVersion));
			return true;
		}
		SyncDicePopulation(qualityBySpawnUpgradeId, flag, forcedDesired, forcedEvolutionTier, currentEvolutionTier);
		return true;
	}

	public void ClearThrowables()
	{
		contentVersion++;
		diceSyncVersions.Clear();
		if (contentCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(contentCoroutine);
			contentCoroutine = null;
		}
		for (int num = activeDice.Count - 1; num >= 0; num--)
		{
			RemoveDiceAt(num);
		}
		qualityIds.Clear();
		if (SingletonMono<UpgradeManager>.HasInstance())
		{
			SingletonMono<UpgradeManager>.Instance.NotifyCoinCountChanged();
		}
	}

	public void TickExternal(float deltaTime)
	{
		if (!upgradeSubscribed && (Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null && SingletonMono<UpgradeManager>.Instance.IsInitialized)
		{
			SubscribeEvents();
			InitializeThrowables();
			scene?.ReloadRolesForCurrentWorld();
		}
		for (int i = 0; i < activeDice.Count; i++)
		{
			activeDice[i]?.TickExternal(deltaTime);
		}
	}

	public bool TryManualThrowAt(Vector2 worldPosition, float radius)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if (IsDiceGuideActive)
		{
			return TryHandleDiceGuideClick(worldPosition, radius);
		}
		DiceVat dice = FindNearestIdleDice(worldPosition, radius, requireSwipeUnlock: false);
		return TryThrowDice(dice, WorldSettlementSource.Manual);
	}

	public bool TrySwipeThrowAt(Vector2 worldPosition, float radius)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		if (IsDiceGuideActive)
		{
			return true;
		}
		DiceVat dice = FindNearestIdleDice(worldPosition, radius, requireSwipeUnlock: true);
		return TryThrowDice(dice, WorldSettlementSource.Swipe);
	}

	public IRoleThrowableVat FindThrowableForRole(Vector2 rolePosition, int roleType)
	{
		if (IsDiceGuideActive)
		{
			return null;
		}
		DiceVat result = null;
		int num = 0;
		for (int i = 0; i < activeDice.Count; i++)
		{
			DiceVat diceVat = activeDice[i];
			if ((Object)(object)diceVat != (Object)null && diceVat.IsIdleForRole && CanHelperRoll(diceVat.QualityId))
			{
				num++;
				if (Random.Range(0, num) == 0)
				{
					result = diceVat;
				}
			}
		}
		return result;
	}

	public Vector2 GetThrowableLandingPosition(Vector2 startPosition)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)scene != (Object)null))
		{
			return startPosition;
		}
		return scene.GetRandomLandingPosition(startPosition);
	}

	public float GetRoleThrowCooldown(int roleType)
	{
		if (roleType > 0 && ConfigManager.HasInstance())
		{
			return ConfigManager.Instance.GetHelperCD(roleType);
		}
		return GetHelperRollInterval();
	}

	private void RebuildWorldContent()
	{
		contentVersion++;
		if (contentCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(contentCoroutine);
		}
		contentCoroutine = ((MonoBehaviour)this).StartCoroutine(EnsureAssetsAndSync(contentVersion));
	}

	private IEnumerator EnsureAssetsAndSync(int version)
	{
		yield return EnsureSpineShader();
		for (int index = 0; index < qualityIds.Count; index++)
		{
			if (version != contentVersion || WorldId <= 0)
			{
				yield break;
			}
			int qualityId = qualityIds[index];
			if (GetDiceCount(qualityId) > 0)
			{
				yield return EnsureDiceVatAsset(qualityId, index);
				int currentEvolutionTier = GetCurrentEvolutionTier(qualityId);
				if (qualityId >= 102 && qualityId <= 105 && currentEvolutionTier > 0 && VatRuntimeAssets.Instance != null)
				{
					yield return VatRuntimeAssets.Instance.EnsureDiceVatAssetLoaded(qualityId, currentEvolutionTier);
				}
			}
		}
		if (version == contentVersion && WorldId > 0)
		{
			SyncDicePopulation(playSpawnEffects: false);
			contentCoroutine = null;
		}
	}

	private IEnumerator EnsureDiceAssetsAndSync(int qualityId, bool spawnAtCenter, int? forcedDesired, int? forcedEvolutionTier, int currentTier, int requiredTier, int requestVersion, int worldVersion)
	{
		int qualityIndex = qualityIds.IndexOf(qualityId);
		yield return EnsureDiceVatAsset(qualityId, qualityIndex);
		if (qualityId >= 102 && qualityId <= 105 && VatRuntimeAssets.Instance != null)
		{
			yield return VatRuntimeAssets.Instance.EnsureDiceVatAssetLoaded(qualityId, requiredTier);
		}
		if (WorldId > 0 && contentVersion == worldVersion && diceSyncVersions.TryGetValue(qualityId, out var value) && value == requestVersion)
		{
			SyncDicePopulation(qualityId, spawnAtCenter, forcedDesired, forcedEvolutionTier, currentTier);
		}
	}

	private IEnumerator EnsureSpineShader()
	{
		if ((Object)(object)spineVatShader != (Object)null)
		{
			yield break;
		}
		spineVatShader = (((Object)(object)scene != (Object)null) ? scene.SpineVatShader : null);
		spriteVatShader = (((Object)(object)scene != (Object)null) ? scene.SpriteVatShader : null);
		if ((Object)(object)spineVatShader != (Object)null)
		{
			yield break;
		}
		if ((Object)(object)SingletonMono<ResManager>.Instance != (Object)null)
		{
			if (spineShaderHandle == null)
			{
				spineShaderHandle = SingletonMono<ResManager>.Instance.LoadAssetAsyncHandle<Object>("Assets/GameRes/VAT/SpineVatUnlit.shader");
			}
			if (spineShaderHandle != null)
			{
				yield return spineShaderHandle;
				if ((int)((HandleBase)spineShaderHandle).Status == 2)
				{
					ref Shader reference = ref spineVatShader;
					Object assetObject = spineShaderHandle.AssetObject;
					reference = (Shader)(object)((assetObject is Shader) ? assetObject : null);
				}
			}
		}
		if (spineVatShader == null)
		{
			spineVatShader = Shader.Find("Spine/VAT Unlit");
		}
	}

	private IEnumerator EnsureDiceVatAsset(int qualityId, int qualityIndex)
	{
		if (diceVatAssets.ContainsKey(qualityId))
		{
			yield break;
		}
		if (qualityIndex < 0 || qualityIndex >= DiceVatAssetPaths.Length || (Object)(object)SingletonMono<ResManager>.Instance == (Object)null)
		{
			Debug.LogError((object)$"[DiceWorldController] Missing VAT path for dice quality {qualityId}.");
			yield break;
		}
		if (!diceVatHandles.TryGetValue(qualityId, out var handle) || handle == null)
		{
			handle = SingletonMono<ResManager>.Instance.LoadAssetAsyncHandle<Object>(DiceVatAssetPaths[qualityIndex]);
			if (handle == null)
			{
				yield break;
			}
			diceVatHandles[qualityId] = handle;
		}
		if (!((HandleBase)handle).IsDone)
		{
			yield return handle;
		}
		if ((int)((HandleBase)handle).Status == 2 && handle.AssetObject is SpineVatAsset value)
		{
			diceVatAssets[qualityId] = value;
		}
		else
		{
			Debug.LogError((object)("[DiceWorldController] Failed to load dice VAT: " + DiceVatAssetPaths[qualityIndex] + ", " + ((HandleBase)handle).LastError));
		}
	}

	private void SyncDicePopulation(bool playSpawnEffects = true)
	{
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		foreach (int qualityId in qualityIds)
		{
			dictionary[qualityId] = GetDiceCount(qualityId);
		}
		Dictionary<int, int> dictionary2 = new Dictionary<int, int>();
		for (int num = activeDice.Count - 1; num >= 0; num--)
		{
			DiceVat diceVat = activeDice[num];
			int key = diceVat?.QualityId ?? 0;
			int value;
			int num2 = (dictionary2.TryGetValue(key, out value) ? value : 0);
			int value2;
			int num3 = (dictionary.TryGetValue(key, out value2) ? value2 : 0);
			if ((Object)(object)diceVat == (Object)null || num2 >= num3)
			{
				RemoveDiceAt(num);
			}
			else
			{
				dictionary2[key] = num2 + 1;
			}
		}
		for (int i = 0; i < qualityIds.Count; i++)
		{
			int num4 = qualityIds[i];
			int value3;
			int j = (dictionary2.TryGetValue(num4, out value3) ? value3 : 0);
			int num5 = dictionary[num4];
			int currentEvolutionTier = GetCurrentEvolutionTier(num4);
			for (; j < num5; j++)
			{
				int evoTier = currentEvolutionTier;
				bool playSpawnEffect = playSpawnEffects;
				if (!SpawnDice(num4, null, evoTier, playSpawnEffect))
				{
					break;
				}
			}
		}
		if (SingletonMono<UpgradeManager>.HasInstance())
		{
			SingletonMono<UpgradeManager>.Instance.NotifyCoinCountChanged();
		}
		TryStartDiceGuide();
	}

	private void SyncDicePopulation(int qualityId, bool spawnAtCenter = false, int? forcedDesired = null, int? forcedEvolutionTier = null, int? forcedCurrentTier = null)
	{
		if (qualityId == 0)
		{
			return;
		}
		int num = forcedDesired ?? GetDiceCount(qualityId);
		bool flag = HasEvolutionMergingDice(qualityId);
		if (spawnAtCenter && !flag)
		{
			PlayDiceEvolutionMerge(qualityId, num, forcedEvolutionTier, forcedCurrentTier);
		}
		else
		{
			if (flag)
			{
				return;
			}
			int i = 0;
			for (int num2 = activeDice.Count - 1; num2 >= 0; num2--)
			{
				DiceVat diceVat = activeDice[num2];
				if ((Object)(object)diceVat == (Object)null)
				{
					RemoveDiceAt(num2);
				}
				else if (diceVat.QualityId == qualityId && !diceVat.IsEvolutionMerging)
				{
					if (diceVat.State == DiceVatState.Evolving || diceVat.IsEvolutionShaking)
					{
						i++;
					}
					else if (i >= num)
					{
						RemoveDiceAt(num2);
					}
					else
					{
						i++;
					}
				}
			}
			int currentEvolutionTier = GetCurrentEvolutionTier(qualityId);
			for (; i < num; i++)
			{
				int evoTier = currentEvolutionTier;
				if (!SpawnDice(qualityId, null, evoTier))
				{
					break;
				}
			}
			if (SingletonMono<UpgradeManager>.HasInstance())
			{
				SingletonMono<UpgradeManager>.Instance.NotifyCoinCountChanged();
			}
		}
	}

	private bool HasEvolutionMergingDice(int qualityId)
	{
		for (int i = 0; i < activeDice.Count; i++)
		{
			DiceVat diceVat = activeDice[i];
			if ((Object)(object)diceVat != (Object)null && diceVat.QualityId == qualityId && diceVat.IsEvolutionMerging)
			{
				return true;
			}
		}
		return false;
	}

	private void PlayDiceEvolutionMerge(int qualityId, int desiredCount, int? forcedEvolutionTier = null, int? forcedCurrentTier = null)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = (((Object)(object)scene != (Object)null) ? scene.GetSceneCenterPosition() : Vector2.zero);
		int num = forcedCurrentTier ?? GetCurrentEvolutionTier(qualityId);
		int num2 = forcedEvolutionTier ?? (num + 1);
		num2 = Mathf.Max(num, num2);
		List<DiceVat> list = new List<DiceVat>(desiredCount);
		for (int i = 0; i < activeDice.Count; i++)
		{
			DiceVat diceVat = activeDice[i];
			if ((Object)(object)diceVat != (Object)null && diceVat.QualityId == qualityId && diceVat.IsEvolutionShaking)
			{
				list.Add(diceVat);
			}
		}
		int num3 = Mathf.Max(0, desiredCount - list.Count);
		for (int j = 0; j < num3; j++)
		{
			if (!SpawnDice(qualityId, val, num))
			{
				break;
			}
			DiceVat diceVat2 = ((activeDice.Count > 0) ? activeDice[activeDice.Count - 1] : null);
			if ((Object)(object)diceVat2 != (Object)null && diceVat2.QualityId == qualityId)
			{
				diceVat2.StartEvolutionShake(1f, num2);
				list.Add(diceVat2);
			}
		}
		List<DiceVat> list2 = new List<DiceVat>();
		for (int k = 0; k < activeDice.Count; k++)
		{
			DiceVat diceVat3 = activeDice[k];
			if ((Object)(object)diceVat3 != (Object)null && diceVat3.QualityId == qualityId && !diceVat3.IsEvolutionShaking)
			{
				list2.Add(diceVat3);
			}
		}
		if (list2.Count == 0)
		{
			for (int l = 0; l < list.Count; l++)
			{
				DiceVat diceVat4 = list[l];
				if ((Object)(object)diceVat4 != (Object)null)
				{
					diceVat4.StopEvolutionShake();
				}
			}
			if (SingletonMono<UpgradeManager>.HasInstance())
			{
				SingletonMono<UpgradeManager>.Instance.NotifyCoinCountChanged();
			}
			return;
		}
		for (int m = 0; m < list2.Count; m++)
		{
			DiceVat diceVat5 = list2[m];
			if ((Object)(object)diceVat5 != (Object)null)
			{
				diceVat5.StartEvolutionMergeMove(val);
			}
		}
		if ((Object)(object)scene != (Object)null)
		{
			((MonoBehaviour)scene).StartCoroutine(FinishDiceEvolutionMerge(list2, list));
		}
		else
		{
			CompleteDiceEvolutionMerge(list2, list);
		}
	}

	private IEnumerator FinishDiceEvolutionMerge(List<DiceVat> mergingDice, List<DiceVat> centerDiceList)
	{
		yield return (object)new WaitForSeconds(1f);
		CompleteDiceEvolutionMerge(mergingDice, centerDiceList);
	}

	private void CompleteDiceEvolutionMerge(List<DiceVat> mergingDice, List<DiceVat> centerDiceList)
	{
		for (int i = 0; i < mergingDice.Count; i++)
		{
			DiceVat diceVat = mergingDice[i];
			if (!((Object)(object)diceVat == (Object)null))
			{
				int num = activeDice.IndexOf(diceVat);
				if (num >= 0)
				{
					RemoveDiceAt(num);
				}
			}
		}
		for (int j = 0; j < centerDiceList.Count; j++)
		{
			DiceVat diceVat2 = centerDiceList[j];
			if ((Object)(object)diceVat2 != (Object)null)
			{
				diceVat2.StopEvolutionShake();
			}
		}
		if (centerDiceList.Count > 0 && (Object)(object)centerDiceList[0] != (Object)null)
		{
			int qualityId = centerDiceList[0].QualityId;
			SyncDicePopulation(qualityId);
		}
		if (SingletonMono<UpgradeManager>.HasInstance())
		{
			SingletonMono<UpgradeManager>.Instance.NotifyCoinCountChanged();
		}
	}

	private int GetQualityBySpawnUpgradeId(int upgradeItemId)
	{
		if (qualityIds.Contains(upgradeItemId))
		{
			return upgradeItemId;
		}
		return 0;
	}

	private bool HasLoadedDiceAsset(int qualityId)
	{
		if (qualityId != 0 && diceVatAssets.TryGetValue(qualityId, out var value))
		{
			return (Object)(object)value != (Object)null;
		}
		return false;
	}

	private int GetCurrentEvolutionTier(int qualityId)
	{
		if (SingletonMono<UpgradeManager>.HasInstance())
		{
			int evolutionTier = SingletonMono<UpgradeManager>.Instance.GetEvolutionTier(qualityId);
			if (evolutionTier >= 0)
			{
				return evolutionTier;
			}
		}
		int num = 0;
		for (int i = 0; i < activeDice.Count; i++)
		{
			DiceVat diceVat = activeDice[i];
			if ((Object)(object)diceVat != (Object)null && diceVat.QualityId == qualityId)
			{
				int evolutionTier2 = diceVat.GetEvolutionTier();
				if (evolutionTier2 > num)
				{
					num = evolutionTier2;
				}
			}
		}
		return num;
	}

	private SpineVatAsset GetDiceVatAsset(int qualityId, int evolutionTier)
	{
		if (VatRuntimeAssets.Instance != null)
		{
			SpineVatAsset diceVatAsset = VatRuntimeAssets.Instance.GetDiceVatAsset(qualityId, evolutionTier);
			if ((Object)(object)diceVatAsset != (Object)null)
			{
				return diceVatAsset;
			}
		}
		diceVatAssets.TryGetValue(qualityId, out var value);
		return value;
	}

	private bool SpawnDice(int qualityId, Vector2? forcedSpawnPosition = null, int evoTier = 0, bool playSpawnEffect = true)
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		SpineVatAsset value = GetDiceVatAsset(qualityId, evoTier);
		if ((Object)(object)value == (Object)null && !diceVatAssets.TryGetValue(qualityId, out value))
		{
			return false;
		}
		Transform coinPool = scene.GetCoinPool();
		GameObject val = new GameObject($"DiceVat_{qualityId}")
		{
			layer = (((Object)(object)coinPool != (Object)null) ? ((Component)coinPool).gameObject.layer : ((Component)scene).gameObject.layer)
		};
		val.transform.SetParent(((Object)(object)coinPool != (Object)null) ? coinPool : ((Component)scene).transform, false);
		val.transform.localScale = Vector3.one * scene.ThrowableRenderScale;
		scene.GetSceneBoundary(out var min, out var max);
		DiceVat diceVat = val.AddComponent<DiceVat>();
		diceVat.UseExternalTick = true;
		diceVat.SetVatShader(spineVatShader);
		diceVat.SetSpriteVatShader(spriteVatShader);
		diceVat.SetSortingLayerAndOrder(scene.VatSortingLayerName, scene.ThrowableSortingOrder);
		diceVat.Initialize(qualityId, value, min, max, (WorldSettlementSource _) => GetValueMultiplier(qualityId), () => GetFaceWeightMultipliers(qualityId), evoTier);
		int num = -1;
		Vector2 val2;
		if (forcedSpawnPosition.HasValue)
		{
			val2 = forcedSpawnPosition.Value;
		}
		else
		{
			val2 = scene.GetRandomPositionInBoundary();
			num = ((activeDice.Count < 3) ? activeDice.Count : (-1));
			if (num >= 0 && scene.TryGetInitialThrowableSpawnPosition(num, out var worldPosition))
			{
				val2 = worldPosition;
			}
		}
		diceVat.SetInitialPosition(val2);
		if (playSpawnEffect && (forcedSpawnPosition.HasValue || num < 0) && (Object)(object)scene != (Object)null)
		{
			scene.PlaySpawnEffect(val2);
		}
		diceVat.OnLanded += HandleDiceLanded;
		diceVat.OnResonanceTrigger += HandleDiceResonance;
		activeDice.Add(diceVat);
		return true;
	}

	private void RemoveDiceAt(int index)
	{
		DiceVat diceVat = activeDice[index];
		activeDice.RemoveAt(index);
		if ((Object)(object)diceVat != (Object)null)
		{
			diceVat.OnLanded -= HandleDiceLanded;
			diceVat.OnResonanceTrigger -= HandleDiceResonance;
			diceVat.CancelThrow();
			Object.Destroy((Object)(object)((Component)diceVat).gameObject);
		}
	}

	private DiceVat FindNearestIdleDice(Vector2 position, float radius, bool requireSwipeUnlock)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		DiceVat result = null;
		float num = radius * radius;
		for (int i = 0; i < activeDice.Count; i++)
		{
			DiceVat diceVat = activeDice[i];
			if (!((Object)(object)diceVat == (Object)null) && diceVat.IsIdleForRole && (!requireSwipeUnlock || IsInteractionUnlocked(diceVat.QualityId)))
			{
				Vector2 val = Vector2.op_Implicit(((Component)diceVat).transform.position) - position;
				float sqrMagnitude = ((Vector2)(ref val)).sqrMagnitude;
				if (sqrMagnitude <= num)
				{
					num = sqrMagnitude;
					result = diceVat;
				}
			}
		}
		return result;
	}

	private bool TryThrowDice(DiceVat dice, WorldSettlementSource source)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)dice != (Object)null && dice.IsIdleForRole)
		{
			return dice.TryThrow(Vector2.op_Implicit(((Component)dice).transform.position), GetThrowableLandingPosition(Vector2.op_Implicit(((Component)dice).transform.position)), source);
		}
		return false;
	}

	private void HandleDiceLanded(DiceVat dice, DiceRollResult rollResult, WorldSettlementSource source, int sourceRoleType)
	{
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)dice == (Object)null) && WorldId > 0)
		{
			double diceFinalValue = SingletonMono<UpgradeManager>.Instance.GetDiceFinalValue(rollResult.Value, sourceRoleType);
			WorldSettlementRequest request = new WorldSettlementRequest(entry: new WorldSettlementEntry(rollResult.QualityId, rollResult.Face, diceFinalValue), worldId: WorldId, gameplayType: GameplayType, source: source);
			if (WorldSettlementService.Instance.TrySettle(in request, out var result))
			{
				scene?.ShowCoinFloatingText(((Component)dice).transform.position + new Vector3(0.5f, 1.2f, 0f), result.TotalReward);
				HandleDiceGuideLanded(dice, rollResult);
			}
		}
	}

	private void TryStartDiceGuide()
	{
		if (WorldId == 2 && diceGuideStep <= 0 && PlayerPrefs.GetInt("dice_world_guide_step", 0) == 0 && !((Object)(object)scene == (Object)null) && activeDice.Count > 1)
		{
			ShowDiceGuideClickStep(1, 0, "点击投掷骰子，根据投掷的点数获取奖励");
		}
	}

	private void ShowDiceGuideClickStep(int step, int diceIndex, string dialogText)
	{
		DiceVat guideDice = GetGuideDice(diceIndex);
		if ((Object)(object)guideDice == (Object)null)
		{
			CompleteDiceGuide();
			return;
		}
		diceGuideStep = step;
		diceGuideWaitingDice = null;
		Time.timeScale = 0f;
		scene.ShowGuideClickDice(guideDice, dialogText);
	}

	private bool TryHandleDiceGuideClick(Vector2 worldPosition, float radius)
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		if (diceGuideStep != 1 && diceGuideStep != 3)
		{
			return true;
		}
		int num = ((diceGuideStep != 1) ? 1 : 0);
		int face = ((diceGuideStep == 1) ? 1 : 6);
		DiceVat guideDice = GetGuideDice(num);
		if ((Object)(object)guideDice == (Object)null)
		{
			CompleteDiceGuide();
			return true;
		}
		float num2 = radius * radius;
		Vector2 val = Vector2.op_Implicit(((Component)guideDice).transform.position) - worldPosition;
		if (((Vector2)(ref val)).sqrMagnitude > num2)
		{
			return true;
		}
		scene.HideGuideFinger();
		scene.HideGuideDialog();
		Time.timeScale = 1f;
		diceGuideWaitingDice = guideDice;
		diceGuideStep = ((diceGuideStep == 1) ? 2 : 4);
		guideDice.ForceNextFace(face);
		if (!TryThrowDice(guideDice, WorldSettlementSource.Manual))
		{
			ShowDiceGuideClickStep((diceGuideStep == 2) ? 1 : 3, num, (diceGuideStep == 2) ? "点击投掷骰子，根据投掷的点数获取奖励" : null);
		}
		return true;
	}

	private void HandleDiceGuideLanded(DiceVat dice, DiceRollResult rollResult)
	{
		if (!IsDiceGuideActive || (Object)(object)diceGuideWaitingDice != (Object)(object)dice)
		{
			return;
		}
		diceGuideWaitingDice = null;
		scene.HideGuideFinger();
		Time.timeScale = 0f;
		if (diceGuideStep == 2)
		{
			ShowDiceGuideMessage("投出了1点，获得了1倍奖励", delegate
			{
				ShowDiceGuideClickStep(3, 1, null);
			});
		}
		else if (diceGuideStep == 4)
		{
			ShowDiceGuideMessage("6点可以获得6倍奖励！", CompleteDiceGuide);
		}
	}

	private void ShowDiceGuideMessage(string text, Action onConfirmed)
	{
		if (UIManager.HasInstance())
		{
			UIManager.Instance.ShowDialogPanel(text, DialogPosition.Bottom, 0.001f, delegate
			{
				UIManager.Instance.HideDialogPanel();
				onConfirmed?.Invoke();
			});
		}
		else
		{
			onConfirmed?.Invoke();
		}
	}

	private DiceVat GetGuideDice(int index)
	{
		if (index < 0 || index >= activeDice.Count)
		{
			return null;
		}
		return activeDice[index];
	}

	private void CompleteDiceGuide()
	{
		PlayerPrefs.SetInt("dice_world_guide_step", 1);
		PlayerPrefs.Save();
		StopDiceGuide(saveComplete: false);
		GameDataManager.Instance?.ForceUpdateNewGuide();
	}

	private void StopDiceGuide(bool saveComplete)
	{
		if (saveComplete)
		{
			PlayerPrefs.SetInt("dice_world_guide_step", 1);
			PlayerPrefs.Save();
		}
		diceGuideStep = 0;
		diceGuideWaitingDice = null;
		Time.timeScale = 1f;
		scene?.HideGuideFinger();
		scene?.HideGuideDialog();
	}

	private void SubscribeEvents()
	{
		if (!upgradeSubscribed && (Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null)
		{
			SingletonMono<UpgradeManager>.Instance.OnUpgradeSuccess += HandleUpgradeSuccess;
			upgradeSubscribed = true;
		}
		if (!deskSubscribed)
		{
			deskSubscribed = true;
		}
	}

	private void UnsubscribeEvents()
	{
		if (upgradeSubscribed && (Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null)
		{
			SingletonMono<UpgradeManager>.Instance.OnUpgradeSuccess -= HandleUpgradeSuccess;
		}
		upgradeSubscribed = false;
		_ = deskSubscribed;
		deskSubscribed = false;
	}

	private void HandleUpgradeSuccess(int itemId, int level)
	{
		if (WorldId > 0 && (itemId < 102 || itemId > 106))
		{
			ResolveQualities();
			RebuildWorldContent();
		}
	}

	private void HandleWorldFeatureChanged()
	{
		if (WorldId > 0)
		{
			ResolveQualities();
			RebuildWorldContent();
		}
	}

	private void ResolveQualities()
	{
		qualityIds.Clear();
		qualityIds.Add(102);
		qualityIds.Add(103);
		qualityIds.Add(104);
		qualityIds.Add(105);
		qualityIds.Add(106);
	}

	private int GetDiceCount(int qualityId)
	{
		if (qualityId > 0 && (Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null && UpgradeManager.IsEvolvableCurrencyUpgrade(qualityId))
		{
			return Mathf.Max(0, SingletonMono<UpgradeManager>.Instance.GetCurrencyFieldCount(qualityId));
		}
		int num = ((qualityId > 0 && (Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null) ? SingletonMono<UpgradeManager>.Instance.GetUpgradeLevel(qualityId, "itemUpgrade1") : 0);
		return ((IsStarterQuality(qualityId) && SingletonMono<UpgradeManager>.HasInstance()) ? SingletonMono<UpgradeManager>.Instance.GetInitialDiceCount() : 0) + Mathf.Max(0, num);
	}

	private bool CanHelperRoll(int qualityId)
	{
		if ((Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null)
		{
			return SingletonMono<UpgradeManager>.Instance.CanRoleFlipDice(qualityId);
		}
		return qualityId == 102;
	}

	private bool IsInteractionUnlocked(int qualityId)
	{
		if ((Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null)
		{
			return SingletonMono<UpgradeManager>.Instance.CanPlayerSwipeToThrowDice(qualityId);
		}
		return false;
	}

	private float GetHelperRollInterval()
	{
		world world = (ConfigManager.HasInstance() ? ConfigManager.Instance.GetWorldConfig(WorldId) : null);
		if (world == null)
		{
			return 2f;
		}
		float num = Mathf.Max(0.1f, world.HelperRollInterval);
		if (world.HelperSpeedUpgradeId > 0 && (Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null)
		{
			int upgradeLevel = SingletonMono<UpgradeManager>.Instance.GetUpgradeLevel(world.HelperSpeedUpgradeId, "itemUpgrade2");
			num /= Mathf.Max(0.1f, 1f + ConfigManager.Instance.GetUpgrade2Result(world.HelperSpeedUpgradeId) * (float)upgradeLevel);
		}
		return num * 1f;
	}

	private double GetValueMultiplier(int qualityId)
	{
		DiceWorldProgressData currentDiceProgress = GameDataManager.Instance.GetCurrentDiceProgress();
		return 1.0 * (double)currentDiceProgress.valueMultiplier;
	}

	private float[] GetFaceWeightMultipliers(int qualityId)
	{
		float[] array = new float[6] { 1f, 1f, 1f, 1f, 1f, 1f };
		DiceWorldProgressData currentDiceProgress = GameDataManager.Instance.GetCurrentDiceProgress();
		for (int i = 0; i < array.Length; i++)
		{
			float num = ((i < currentDiceProgress.faceWeightMultipliers.Count) ? currentDiceProgress.faceWeightMultipliers[i] : 1f);
			array[i] = Mathf.Max(0f, num);
		}
		if ((Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null)
		{
			array[5] *= SingletonMono<UpgradeManager>.Instance.GetItemUpgrade3Effect(102);
		}
		return array;
	}

	private bool IsStarterQuality(int qualityId)
	{
		return qualityId == 102;
	}

	private void HandleDiceResonance(DiceVat sourceDice, int sourceFace)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)sourceDice == (Object)null || activeDice == null || activeDice.Count <= 1)
		{
			return;
		}
		Vector2 val = Vector2.op_Implicit(((Component)sourceDice).transform.position);
		float num = 3f;
		for (int i = 0; i < activeDice.Count; i++)
		{
			DiceVat diceVat = activeDice[i];
			if (!((Object)(object)diceVat == (Object)null) && !((Object)(object)diceVat == (Object)(object)sourceDice) && diceVat.IsIdleForRole)
			{
				Vector2 val2 = Vector2.op_Implicit(((Component)diceVat).transform.position) - val;
				if (((Vector2)(ref val2)).sqrMagnitude <= num && diceVat.Face == sourceFace)
				{
					diceVat.IsResonanceTriggered = true;
					TryThrowDice(diceVat, WorldSettlementSource.System);
				}
			}
		}
	}

	public void GetIdleThrowablesInRadius(Vector2 center, float radius, List<IRoleThrowableVat> results)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		float num = radius * radius;
		for (int i = 0; i < activeDice.Count; i++)
		{
			DiceVat diceVat = activeDice[i];
			if ((Object)(object)diceVat != (Object)null && diceVat.IsIdleForRole)
			{
				Vector2 val = Vector2.op_Implicit(((Component)diceVat).transform.position) - center;
				if (((Vector2)(ref val)).sqrMagnitude <= num)
				{
					results.Add(diceVat);
				}
			}
		}
	}
}
