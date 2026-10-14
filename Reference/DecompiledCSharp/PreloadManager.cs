using System;
using System.Collections.Generic;
using Module.Loading;
using UnityEngine;

public class PreloadManager : SingletonMono<PreloadManager>
{
	private int count;

	private Dictionary<string, GameObject> preloadObjects;

	private List<string> preloadList = new List<string> { "Assets/GameRes/NewUI/Common/prefabs/upgradePanel.prefab", "Assets/GameRes/NewUI/Facility/prefabs/facility.prefab", "Assets/GameRes/NewUI/Shop/prefabs/shop.prefab", "Assets/GameRes/NewUI/Talent/prefabs/talent.prefab" };

	private Action _onFinish;

	private static float BASE_PROGRESS = 0.2f;

	private float startTime;

	private LoadingUI loadingUI;

	public override bool DontDestory => true;

	public GameObject GetPreloadObject(string path)
	{
		if (preloadObjects.ContainsKey(path))
		{
			return preloadObjects[path];
		}
		return null;
	}

	public void Init()
	{
		InitializeLoading();
		SetProgress(0.2f);
	}

	public void StartPreload(Action onFinish)
	{
		_onFinish = onFinish;
		startTime = Time.time;
		preloadObjects = new Dictionary<string, GameObject>();
		count = 0;
		List<string> list = new List<string>(preloadList);
		if ((Object)(object)GameDataManager.Instance != (Object)null && GameDataManager.Instance.GetGameData() != null && GameDataManager.Instance.GetGameData().currentMainMissionLevel <= 3)
		{
			list.Clear();
			list.Add("Assets/GameRes/NewUI/Common/prefabs/upgradePanel.prefab");
		}
		if (list.Count == 0)
		{
			_onFinish?.Invoke();
			return;
		}
		for (int i = 0; i < list.Count; i++)
		{
			LoadPrefab(list[i], i, list.Count);
		}
	}

	private void LoadPrefab(string path, int index, int totalCount)
	{
		if (path.EndsWith("scene"))
		{
			SingletonMono<ResManager>.Instance.PreloadAssetBundle(path, delegate
			{
				Debug.Log((object)("有callback 加载场景" + path));
				count++;
				SetProgress((float)count * 0.1f + BASE_PROGRESS);
				if (count >= totalCount)
				{
					_onFinish?.Invoke();
				}
			});
			return;
		}
		SingletonMono<ResManager>.Instance.LoadPrefabAsync(path, delegate(GameObject gameobj)
		{
			preloadObjects[path] = gameobj;
			count++;
			SetProgress((float)count * 0.1f + BASE_PROGRESS);
			if (count >= totalCount)
			{
				_onFinish?.Invoke();
			}
		});
	}

	public void LoadingFadeOut()
	{
		loadingUI?.FadeOut(1.5f, delegate
		{
			LoadingUI obj = loadingUI;
			if (obj != null)
			{
				((Component)obj).gameObject.SetActive(false);
			}
		});
	}

	private void InitializeLoading()
	{
		GameObject val = GameObject.Find("Canvas");
		if ((Object)(object)val != (Object)null)
		{
			Transform val2 = val.transform.Find("Loading");
			if ((Object)(object)val2 != (Object)null)
			{
				((Component)val2).gameObject.SetActive(true);
				loadingUI = val2.GetOrAddComponent<LoadingUI>();
			}
		}
	}

	public void SetProgress(float progress)
	{
		if ((Object)(object)loadingUI != (Object)null)
		{
			loadingUI.SetProgress(progress);
		}
	}

	public void SetLoadingUpMax()
	{
		if ((Object)(object)loadingUI != (Object)null)
		{
			loadingUI.Set9999();
		}
	}

	protected override void OnAwake()
	{
	}

	protected override void BeforOnDestroy()
	{
	}

	public void PreloadIcon()
	{
		if (SingletonMono<ResManager>.Instance.IsAssetLoaded("Assets/GameRes/icon/food/101.png") == 0)
		{
			SingletonMono<ResManager>.Instance.LoadSpriteAsync("Assets/GameRes/icon/food/101.png", null);
		}
		if (SingletonMono<ResManager>.Instance.IsAssetLoaded("Assets/GameRes/icon/seasoning/501.png") == 0)
		{
			SingletonMono<ResManager>.Instance.LoadSpriteAsync("Assets/GameRes/icon/seasoning/501.png", null);
		}
	}

	public void PreloadFinish()
	{
		if (SingletonMono<ResManager>.Instance.IsAssetLoaded("Assets/GameRes/UI/dungeon/prefabs/DungeonFinishUI.prefab") == 0)
		{
			SingletonMono<ResManager>.Instance.LoadPrefabAsync("Assets/GameRes/UI/dungeon/prefabs/DungeonFinishUI.prefab", null);
		}
	}

	public void PreloadFacility()
	{
		if (SingletonMono<ResManager>.Instance.IsAssetLoaded("Assets/GameRes/NewUI/Facility/prefabs/facility.prefab") == 0)
		{
			SingletonMono<ResManager>.Instance.LoadPrefabAsync("Assets/GameRes/NewUI/Facility/prefabs/facility.prefab", null);
		}
	}

	public void PreloadLevelSelect()
	{
		if (SingletonMono<ResManager>.Instance.IsAssetLoaded("Assets/GameRes/UI/levelSected/prefabs/LevelSelect.prefab") == 0)
		{
			SingletonMono<ResManager>.Instance.LoadPrefabAsync("Assets/GameRes/UI/levelSected/prefabs/LevelSelect.prefab", null);
		}
	}

	public void PreloadMall()
	{
		if (SingletonMono<ResManager>.Instance.IsAssetLoaded("Assets/GameRes/UI/mall/prefabs/MallScene.prefab") == 0)
		{
			SingletonMono<ResManager>.Instance.LoadPrefabAsync("Assets/GameRes/UI/mall/prefabs/MallScene.prefab", null);
		}
	}

	public void PreloadBuff()
	{
		if (SingletonMono<ResManager>.Instance.IsAssetLoaded("Assets/GameRes/icon/bufficon/0.png") == 0)
		{
			SingletonMono<ResManager>.Instance.LoadSpriteAsync("Assets/GameRes/icon/bufficon/0.png", null);
		}
	}

	public void PreloadRoleBag()
	{
		if (SingletonMono<ResManager>.Instance.IsAssetLoaded("Assets/GameRes/UI/rolebag/prefabs/RoleBag.prefab") == 0)
		{
			SingletonMono<ResManager>.Instance.LoadPrefabAsync("Assets/GameRes/UI/rolebag/prefabs/RoleBag.prefab", null);
		}
	}

	public void PreloadFaqi()
	{
		if (SingletonMono<ResManager>.Instance.IsAssetLoaded("Assets/GameRes/UI/Artifact/prefabs/Artifact.prefab") == 0)
		{
			SingletonMono<ResManager>.Instance.LoadPrefabAsync("Assets/GameRes/UI/Artifact/prefabs/Artifact.prefab", null);
		}
	}
}
