using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using cfg;

namespace Module.Facility;

public class DeskPanel : MonoBehaviour
{
	[Header("ScrollView设置")]
	[SerializeField]
	private ScrollRect scrollRect;

	[SerializeField]
	private Transform contentParent;

	[Header("设施Item预制体")]
	[SerializeField]
	private GameObject deskItemPrefab;

	private List<DeskItem> deskItems = new List<DeskItem>();

	public void Initialize()
	{
		ClearDeskItems();
		LoadDeskConfig();
	}

	private void LoadDeskConfig()
	{
		if (!ConfigManager.HasInstance())
		{
			Debug.LogError((object)"ConfigManager not initialized!");
			return;
		}
		int currentWorldId = GameDataManager.Instance.GetCurrentWorldId();
		List<desk> allDeskByWorld = ConfigManager.Instance.GetAllDeskByWorld(currentWorldId);
		if (allDeskByWorld == null || allDeskByWorld.Count == 0)
		{
			Debug.LogWarning((object)$"No desk config found for world {currentWorldId}!");
			return;
		}
		foreach (desk item in allDeskByWorld)
		{
			CreateDeskItem(item);
		}
	}

	private void CreateDeskItem(desk deskConfig)
	{
		if ((Object)(object)deskItemPrefab == (Object)null)
		{
			Debug.LogError((object)"deskItemPrefab is null!");
			return;
		}
		deskItemPrefab.gameObject.SetActive(true);
		DeskItem component = Object.Instantiate<GameObject>(deskItemPrefab, contentParent).GetComponent<DeskItem>();
		if ((Object)(object)component != (Object)null)
		{
			component.Initialize(deskConfig);
			deskItems.Add(component);
			component.onUseChanged = (Action<int>)Delegate.Combine(component.onUseChanged, new Action<int>(OnUseChanged));
		}
		deskItemPrefab.gameObject.SetActive(false);
	}

	private void OnUseChanged(int deskItemId)
	{
		foreach (DeskItem deskItem in deskItems)
		{
			if ((Object)(object)deskItem != (Object)null)
			{
				deskItem.OnUseChanged(deskItemId);
			}
		}
	}

	private void ClearDeskItems()
	{
		foreach (DeskItem deskItem in deskItems)
		{
			if ((Object)(object)deskItem != (Object)null && (Object)(object)((Component)deskItem).gameObject != (Object)null)
			{
				deskItem.onUseChanged = (Action<int>)Delegate.Remove(deskItem.onUseChanged, new Action<int>(OnUseChanged));
				Object.Destroy((Object)(object)((Component)deskItem).gameObject);
			}
		}
		deskItems.Clear();
	}

	private void OnDestroy()
	{
		ClearDeskItems();
	}
}
