using System;
using System.Collections;
using System.Collections.Generic;
using Module.Role;
using Module.Vat;
using UnityEngine;
using YooAsset;

public sealed class VatRuntimeAssets
{
	private static VatRuntimeAssets _instance;

	public const string SpineVatShaderPath = "Assets/GameRes/VAT/SpineVatUnlit.shader";

	public const string SpriteVatShaderPath = "Assets/GameRes/VAT/SpriteSequenceVatUnlit.shader";

	public const string SpineVatIndirectShaderPath = "Assets/GameRes/VAT/SpineVatIndirectUnlit.shader";

	public const string CoinNormalTextPath = "Assets/GameRes/Role/coinNormalText.prefab";

	public const string CoinCriticalTextPath = "Assets/GameRes/Role/coinCriticalText.prefab";

	public const string HelperAddVatPath = "Assets/GameRes/VAT/zgygy_fight_calling_SkeletonData_VAT.asset";

	public const string GongfuSkillVatPath = "Assets/GameRes/VAT/zgygy_roleskill_kungfu_SkeletonData_VAT.asset";

	public const string HalandeSkillVatPath = "Assets/GameRes/VAT/zgygy_roleskill_halande_SkeletonData_VAT.asset";

	public const string HammerWakeupVatPath = "Assets/GameRes/VAT/zgygy_hammer_wakeup_SkeletonData_VAT.asset";

	public const string DushenSkillVatPath = "Assets/GameRes/VAT/zgygy_roleskill_dushen_SkeletonData_VAT.asset";

	public const string DushenSkillVatPath1 = "Assets/GameRes/VAT/zgygy_roleskill_dushen_1_SkeletonData_VAT.asset";

	public const string CoinCopperVatPath = "Assets/GameRes/VAT/coin_copper_SkeletonData_VAT.asset";

	public const string CoinSilverVatPath = "Assets/GameRes/VAT/coin_silver_SkeletonData_VAT.asset";

	public const string CoinGoldVatPath = "Assets/GameRes/VAT/coin_gold_SkeletonData_VAT.asset";

	public const string CoinPurpleVatPath = "Assets/GameRes/VAT/coin_purple_SkeletonData_VAT.asset";

	public const int CoinEvolutionSkinMaxTier = 4;

	public const string DiceCopperVatPath = "Assets/GameRes/VAT/dice_copper_SkeletonData_VAT.asset";

	public const string DiceSilverVatPath = "Assets/GameRes/VAT/dice_silver_SkeletonData_VAT.asset";

	public const string DiceGoldVatPath = "Assets/GameRes/VAT/dice_gold_SkeletonData_VAT.asset";

	public const string DicePurpleVatPath = "Assets/GameRes/VAT/dice_purple_SkeletonData_VAT.asset";

	public const int DiceEvolutionSkinMaxTier = 4;

	public static readonly string[] CoinVatAssetPaths = BuildAllCoinVatAssetPaths();

	public static readonly string[] DiceVatAssetPaths = BuildAllDiceVatAssetPaths();

	public static readonly string[] SkillVatAssetPaths = new string[6] { "Assets/GameRes/VAT/zgygy_fight_calling_SkeletonData_VAT.asset", "Assets/GameRes/VAT/zgygy_roleskill_kungfu_SkeletonData_VAT.asset", "Assets/GameRes/VAT/zgygy_roleskill_halande_SkeletonData_VAT.asset", "Assets/GameRes/VAT/zgygy_hammer_wakeup_SkeletonData_VAT.asset", "Assets/GameRes/VAT/zgygy_roleskill_dushen_SkeletonData_VAT.asset", "Assets/GameRes/VAT/zgygy_roleskill_dushen_1_SkeletonData_VAT.asset" };

	public static readonly string[] SkillPrefabPaths = new string[13]
	{
		"Assets/GameRes/Role/coins_tornado.prefab", "Assets/GameRes/Role/coins_tornado2.prefab", "Assets/GameRes/Role/magic_ani.prefab", "Assets/GameRes/Role/lanqiu.prefab", "Assets/GameRes/Role/dashehandani.prefab", "Assets/GameRes/Role/aixinqiu.prefab", "Assets/GameRes/Role/iron_bullet.prefab", "Assets/GameRes/Role/iron_boom.prefab", "Assets/GameRes/Role/iron_bullet2.prefab", "Assets/GameRes/Role/iron_boom2.prefab",
		"Assets/GameRes/Role/zuqiu.prefab", "Assets/GameRes/Role/coins_smoke.prefab", "Assets/GameRes/Role/dushen_DiceMove.prefab"
	};

	private readonly List<AssetHandle> shaderHandles = new List<AssetHandle>();

	private readonly Dictionary<int, AssetHandle> roleVatHandles = new Dictionary<int, AssetHandle>();

	private readonly Dictionary<string, AssetHandle> skillVatHandles = new Dictionary<string, AssetHandle>();

	private readonly Dictionary<string, AssetHandle> prefabHandles = new Dictionary<string, AssetHandle>();

	public static VatRuntimeAssets Instance
	{
		get
		{
			return _instance;
		}
		set
		{
			_instance = value;
		}
	}

	public Shader SpineVatShader { get; private set; }

	public Shader SpriteVatShader { get; private set; }

	public Shader SpineVatIndirectShader { get; private set; }

	public IEnumerator LoadShaders()
	{
		if ((Object)(object)SingletonMono<ResManager>.Instance == (Object)null)
		{
			SpineVatShader = Shader.Find("Spine/VAT Unlit");
			SpriteVatShader = Shader.Find("VAT/Sprite Sequence Unlit");
			SpineVatIndirectShader = Shader.Find("Spine/VAT Indirect Unlit");
			yield break;
		}
		yield return LoadShader("Assets/GameRes/VAT/SpineVatUnlit.shader", "Spine/VAT Unlit", delegate(Shader shader)
		{
			SpineVatShader = shader;
		});
		yield return LoadShader("Assets/GameRes/VAT/SpriteSequenceVatUnlit.shader", "VAT/Sprite Sequence Unlit", delegate(Shader shader)
		{
			SpriteVatShader = shader;
		});
		yield return LoadShader("Assets/GameRes/VAT/SpineVatIndirectUnlit.shader", "Spine/VAT Indirect Unlit", delegate(Shader shader)
		{
			SpineVatIndirectShader = shader;
		});
	}

	public IEnumerator LoadRoleVatAssets(IEnumerable<int> helperIds)
	{
		yield return LoadRoleVatAsset(0);
		if (helperIds == null)
		{
			yield break;
		}
		foreach (int helperId in helperIds)
		{
			yield return LoadRoleVatAsset(helperId);
		}
	}

	public IEnumerator EnsureRoleVatAssetLoaded(int helperId)
	{
		yield return LoadRoleVatAsset(helperId);
	}

	public IEnumerator EnsureSkillVatAssetLoaded(string assetPath)
	{
		yield return LoadSkillVatAsset(assetPath);
	}

	public IEnumerator EnsureCoinVatAssetLoaded(CoinType coinType, int evolutionTier)
	{
		yield return LoadSkillVatAsset(GetCoinVatAssetPath(coinType, evolutionTier));
	}

	public IEnumerator EnsureDiceVatAssetLoaded(int qualityId, int evolutionTier)
	{
		yield return LoadSkillVatAsset(GetDiceVatAssetPath(qualityId, evolutionTier));
	}

	public bool IsSkillVatAssetLoaded(string assetPath)
	{
		return (Object)(object)GetSkillVatAsset(assetPath) != (Object)null;
	}

	public bool IsCoinVatAssetLoaded(CoinType coinType, int evolutionTier)
	{
		return IsSkillVatAssetLoaded(GetCoinVatAssetPath(coinType, evolutionTier));
	}

	public bool IsDiceVatAssetLoaded(int qualityId, int evolutionTier)
	{
		return IsSkillVatAssetLoaded(GetDiceVatAssetPath(qualityId, evolutionTier));
	}

	public IEnumerator LoadFloatingTextPrefabs()
	{
		yield return LoadPrefab("Assets/GameRes/Role/coinNormalText.prefab");
		yield return LoadPrefab("Assets/GameRes/Role/coinCriticalText.prefab");
	}

	public IEnumerator LoadSkillPrefabs()
	{
		for (int i = 0; i < SkillPrefabPaths.Length; i++)
		{
			yield return LoadPrefab(SkillPrefabPaths[i]);
		}
	}

	public IEnumerator LoadSkillVatAssets()
	{
		for (int i = 0; i < SkillVatAssetPaths.Length; i++)
		{
			yield return LoadSkillVatAsset(SkillVatAssetPaths[i]);
		}
	}

	public IEnumerator LoadCoinVatAssets()
	{
		for (int i = 0; i < CoinVatAssetPaths.Length; i++)
		{
			yield return LoadSkillVatAsset(CoinVatAssetPaths[i]);
		}
	}

	public IEnumerator LoadDiceVatAssets()
	{
		for (int i = 0; i < DiceVatAssetPaths.Length; i++)
		{
			yield return LoadSkillVatAsset(DiceVatAssetPaths[i]);
		}
	}

	public SpineVatAsset GetCoinVatAsset(CoinType coinType)
	{
		return GetCoinVatAsset(coinType, 0);
	}

	public SpineVatAsset GetCoinVatAsset(CoinType coinType, int evolutionTier)
	{
		string coinVatAssetPath = GetCoinVatAssetPath(coinType, evolutionTier);
		SpineVatAsset spineVatAsset = (string.IsNullOrEmpty(coinVatAssetPath) ? null : GetSkillVatAsset(coinVatAssetPath));
		if ((Object)(object)spineVatAsset != (Object)null)
		{
			return spineVatAsset;
		}
		if (evolutionTier > 0)
		{
			string coinVatAssetPath2 = GetCoinVatAssetPath(coinType, 0);
			if (!string.IsNullOrEmpty(coinVatAssetPath2))
			{
				return GetSkillVatAsset(coinVatAssetPath2);
			}
			return null;
		}
		return null;
	}

	public static string GetCoinVatAssetPath(CoinType coinType)
	{
		return GetCoinVatAssetPath(coinType, 0);
	}

	public static string GetCoinVatAssetPath(CoinType coinType, int evolutionTier)
	{
		int num = Mathf.Clamp(evolutionTier, 0, 4);
		string text = coinType switch
		{
			CoinType.Silver => "silver", 
			CoinType.Gold => "gold", 
			CoinType.Purple => "purple", 
			_ => "copper", 
		};
		if (num <= 0)
		{
			return "Assets/GameRes/VAT/coin_" + text + "_SkeletonData_VAT.asset";
		}
		int num2 = num + 1;
		return $"Assets/GameRes/VAT/coin_{text}_{num2}_SkeletonData_VAT.asset";
	}

	private static string[] BuildAllCoinVatAssetPaths()
	{
		CoinType[] array = new CoinType[4]
		{
			CoinType.Copper,
			CoinType.Silver,
			CoinType.Gold,
			CoinType.Purple
		};
		string[] array2 = new string[array.Length * 5];
		int num = 0;
		for (int i = 0; i < array.Length; i++)
		{
			for (int j = 0; j <= 4; j++)
			{
				array2[num++] = GetCoinVatAssetPath(array[i], j);
			}
		}
		return array2;
	}

	public SpineVatAsset GetDiceVatAsset(int qualityId)
	{
		return GetDiceVatAsset(qualityId, 0);
	}

	public SpineVatAsset GetDiceVatAsset(int qualityId, int evolutionTier)
	{
		string diceVatAssetPath = GetDiceVatAssetPath(qualityId, evolutionTier);
		SpineVatAsset spineVatAsset = (string.IsNullOrEmpty(diceVatAssetPath) ? null : GetSkillVatAsset(diceVatAssetPath));
		if ((Object)(object)spineVatAsset != (Object)null)
		{
			return spineVatAsset;
		}
		if (evolutionTier > 0)
		{
			string diceVatAssetPath2 = GetDiceVatAssetPath(qualityId, 0);
			if (!string.IsNullOrEmpty(diceVatAssetPath2))
			{
				return GetSkillVatAsset(diceVatAssetPath2);
			}
			return null;
		}
		return null;
	}

	public static string GetDiceVatAssetPath(int qualityId)
	{
		return GetDiceVatAssetPath(qualityId, 0);
	}

	public static string GetDiceVatAssetPath(int qualityId, int evolutionTier)
	{
		int num = ((qualityId != 106) ? Mathf.Clamp(evolutionTier, 0, 4) : 0);
		string text = qualityId switch
		{
			103 => "silver", 
			104 => "gold", 
			105 => "purple", 
			106 => "diamond", 
			_ => "copper", 
		};
		if (num <= 0)
		{
			return "Assets/GameRes/VAT/dice_" + text + "_SkeletonData_VAT.asset";
		}
		int num2 = num + 1;
		return $"Assets/GameRes/VAT/dice_{text}_{num2}_SkeletonData_VAT.asset";
	}

	private static string[] BuildAllDiceVatAssetPaths()
	{
		int[] array = new int[4] { 102, 103, 104, 105 };
		string[] array2 = new string[array.Length * 5];
		int num = 0;
		for (int i = 0; i < array.Length; i++)
		{
			for (int j = 0; j <= 4; j++)
			{
				array2[num++] = GetDiceVatAssetPath(array[i], j);
			}
		}
		return array2;
	}

	public SpineVatAsset GetRoleVatAsset(int helperId)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Invalid comparison between Unknown and I4
		if (!roleVatHandles.TryGetValue(helperId, out var value) || value == null)
		{
			return null;
		}
		if ((int)((HandleBase)value).Status != 2)
		{
			return null;
		}
		if (value == null)
		{
			return null;
		}
		return value.AssetObject as SpineVatAsset;
	}

	public SpineVatAsset GetSkillVatAsset(string assetPath)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Invalid comparison between Unknown and I4
		if (string.IsNullOrEmpty(assetPath) || !skillVatHandles.TryGetValue(assetPath, out var value) || value == null)
		{
			return null;
		}
		if ((int)((HandleBase)value).Status != 2)
		{
			return null;
		}
		return value.AssetObject as SpineVatAsset;
	}

	public GameObject GetPrefab(string assetPath)
	{
		if (string.IsNullOrEmpty(assetPath) || !prefabHandles.TryGetValue(assetPath, out var value) || value == null)
		{
			return null;
		}
		Object assetObject = value.AssetObject;
		return (GameObject)(object)((assetObject is GameObject) ? assetObject : null);
	}

	public void Release()
	{
		for (int i = 0; i < shaderHandles.Count; i++)
		{
			AssetHandle obj = shaderHandles[i];
			if (obj != null)
			{
				((HandleBase)obj).Release();
			}
		}
		foreach (KeyValuePair<int, AssetHandle> roleVatHandle in roleVatHandles)
		{
			AssetHandle value = roleVatHandle.Value;
			if (value != null)
			{
				((HandleBase)value).Release();
			}
		}
		foreach (KeyValuePair<string, AssetHandle> skillVatHandle in skillVatHandles)
		{
			AssetHandle value2 = skillVatHandle.Value;
			if (value2 != null)
			{
				((HandleBase)value2).Release();
			}
		}
		foreach (KeyValuePair<string, AssetHandle> prefabHandle in prefabHandles)
		{
			AssetHandle value3 = prefabHandle.Value;
			if (value3 != null)
			{
				((HandleBase)value3).Release();
			}
		}
		shaderHandles.Clear();
		roleVatHandles.Clear();
		skillVatHandles.Clear();
		prefabHandles.Clear();
		SpineVatShader = null;
		SpriteVatShader = null;
		SpineVatIndirectShader = null;
	}

	private IEnumerator LoadShader(string assetPath, string fallbackShaderName, Action<Shader> onLoaded)
	{
		AssetHandle handle = SingletonMono<ResManager>.Instance.LoadAssetAsyncHandle<Object>(assetPath);
		if (handle == null)
		{
			Shader val = Shader.Find(fallbackShaderName);
			onLoaded?.Invoke(val);
			if ((Object)(object)val == (Object)null)
			{
				Debug.LogWarning((object)("[VatRuntimeAssets] Failed to create shader load handle: " + assetPath));
			}
			yield break;
		}
		shaderHandles.Add(handle);
		yield return handle;
		if ((int)((HandleBase)handle).Status == 2)
		{
			Object assetObject = handle.AssetObject;
			Shader val2 = (Shader)(object)((assetObject is Shader) ? assetObject : null);
			if ((Object)(object)val2 == (Object)null)
			{
				val2 = Shader.Find(fallbackShaderName);
			}
			onLoaded?.Invoke(val2);
			if ((Object)(object)val2 == (Object)null)
			{
				Debug.LogWarning((object)("[VatRuntimeAssets] Loaded asset is not a shader or fallback missing: " + assetPath + ", " + fallbackShaderName));
			}
		}
		else
		{
			Shader val3 = Shader.Find(fallbackShaderName);
			onLoaded?.Invoke(val3);
			if ((Object)(object)val3 == (Object)null)
			{
				Debug.LogWarning((object)("[VatRuntimeAssets] Failed to load shader: " + assetPath + ", " + ((HandleBase)handle).LastError));
			}
		}
	}

	private IEnumerator LoadRoleVatAsset(int helperId)
	{
		if (roleVatHandles.TryGetValue(helperId, out var existingHandle))
		{
			if (existingHandle != null)
			{
				yield return existingHandle;
				if ((int)((HandleBase)existingHandle).Status == 2)
				{
					yield break;
				}
				Debug.LogWarning((object)$"[VatRuntimeAssets] Failed to load cached role VAT asset for helper {helperId}: {((HandleBase)existingHandle).LastError}");
				((HandleBase)existingHandle).Release();
			}
			roleVatHandles.Remove(helperId);
		}
		if ((Object)(object)SingletonMono<ResManager>.Instance == (Object)null)
		{
			yield break;
		}
		string assetPath = RoleVat.GetVatAssetPath(helperId);
		if (string.IsNullOrEmpty(assetPath))
		{
			yield break;
		}
		AssetHandle handle = SingletonMono<ResManager>.Instance.LoadAssetAsyncHandle<Object>(assetPath);
		if (handle != null)
		{
			roleVatHandles[helperId] = handle;
			yield return handle;
			if ((int)((HandleBase)handle).Status != 2)
			{
				Debug.LogWarning((object)$"[VatRuntimeAssets] Failed to load role VAT asset for helper {helperId}: {assetPath}, {((HandleBase)handle).LastError}");
			}
		}
	}

	private IEnumerator LoadSkillVatAsset(string assetPath)
	{
		if (string.IsNullOrEmpty(assetPath) || (Object)(object)SingletonMono<ResManager>.Instance == (Object)null)
		{
			yield break;
		}
		if (skillVatHandles.TryGetValue(assetPath, out var existingHandle))
		{
			if (existingHandle != null)
			{
				yield return existingHandle;
				if ((int)((HandleBase)existingHandle).Status == 2)
				{
					yield break;
				}
				Debug.LogWarning((object)("[VatRuntimeAssets] Failed to load cached skill VAT asset: " + assetPath + ", " + ((HandleBase)existingHandle).LastError));
				((HandleBase)existingHandle).Release();
			}
			skillVatHandles.Remove(assetPath);
		}
		AssetHandle handle = SingletonMono<ResManager>.Instance.LoadAssetAsyncHandle<Object>(assetPath);
		if (handle != null)
		{
			skillVatHandles[assetPath] = handle;
			yield return handle;
			if ((int)((HandleBase)handle).Status != 2)
			{
				Debug.LogWarning((object)("[VatRuntimeAssets] Failed to load skill VAT asset: " + assetPath + ", " + ((HandleBase)handle).LastError));
			}
		}
	}

	private IEnumerator LoadPrefab(string assetPath)
	{
		if (!string.IsNullOrEmpty(assetPath) && !prefabHandles.ContainsKey(assetPath) && !((Object)(object)SingletonMono<ResManager>.Instance == (Object)null))
		{
			AssetHandle val = SingletonMono<ResManager>.Instance.LoadAssetAsyncHandle<Object>(assetPath);
			if (val != null)
			{
				prefabHandles.Add(assetPath, val);
				yield return val;
			}
		}
	}
}
