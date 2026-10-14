using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using cfg;

namespace Module.Facility;

public class HelperPanel : MonoBehaviour
{
	[Header("可分配帮手数量")]
	[SerializeField]
	private TextMeshProUGUI totalCountText;

	[Header("ScrollView设置")]
	[SerializeField]
	private ScrollRect scrollRect;

	[SerializeField]
	private Transform contentParent;

	[Header("帮手Item预制体")]
	[SerializeField]
	private GameObject helperItemPrefab;

	private List<HelperItem> helperItems = new List<HelperItem>();

	private int CanIUseHelperCount = 88;

	private int totalHelperCount = 88;

	public void Initialize()
	{
		int currentWorldHelperUpgradeId = Constants.GetCurrentWorldHelperUpgradeId();
		CanIUseHelperCount = SingletonMono<UpgradeManager>.Instance.GetUpgradeLevel(currentWorldHelperUpgradeId, "itemUpgrade1");
		UpdateTotalCount();
		ClearHelperItems();
		LoadHelperConfig();
	}

	private void UpdateTotalCount()
	{
		int currentWorldHelperUpgradeId = Constants.GetCurrentWorldHelperUpgradeId();
		totalHelperCount = SingletonMono<UpgradeManager>.Instance.GetUpgradeLevel(currentWorldHelperUpgradeId, "itemUpgrade1");
		totalHelperCount = Mathf.Max(1, totalHelperCount);
		if ((Object)(object)totalCountText != (Object)null)
		{
			((TMP_Text)totalCountText).text = $"{CanIUseHelperCount}/{totalHelperCount}";
		}
	}

	private void LoadHelperConfig()
	{
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		if (!ConfigManager.HasInstance())
		{
			Debug.LogError((object)"ConfigManager not initialized!");
			return;
		}
		List<helperRole> allHelperRole = ConfigManager.Instance.GetAllHelperRole();
		if (allHelperRole == null || allHelperRole.Count == 0)
		{
			Debug.LogWarning((object)"No helper config found!");
			return;
		}
		List<helperRole> list = new List<helperRole>(allHelperRole);
		list.Sort((helperRole a, helperRole b) => a.Sorting.CompareTo(b.Sorting));
		int num = 0;
		foreach (helperRole item in list)
		{
			if (item.Id == 9)
			{
				if (GameDataManager.Instance.IsHelperOwned(9))
				{
					CreateHelperItem(item);
					num++;
				}
				else if (WorldCupDataManager.Instance.IsWorldCupActive())
				{
					CreateHelperItem(item);
					num++;
				}
			}
			else
			{
				CreateHelperItem(item);
				num++;
			}
		}
		float num2 = 430f;
		float num3 = (float)num * num2;
		((Component)contentParent).GetComponent<RectTransform>().sizeDelta = new Vector2(((Component)contentParent).GetComponent<RectTransform>().sizeDelta.x, num3);
	}

	private void CreateHelperItem(helperRole helperConfig)
	{
		if ((Object)(object)helperItemPrefab == (Object)null)
		{
			Debug.LogError((object)"helperItemPrefab is null!");
			return;
		}
		helperItemPrefab.gameObject.SetActive(true);
		HelperItem component = Object.Instantiate<GameObject>(helperItemPrefab, contentParent).GetComponent<HelperItem>();
		HelperInfo helperInfo = GameDataManager.Instance.GetHelperInfo(helperConfig.Id);
		if (helperInfo != null)
		{
			OnHelperCountChanged(helperInfo.helperCount);
		}
		if ((Object)(object)component != (Object)null)
		{
			component.Initialize(helperConfig, helperInfo);
			component.onCountChanged = (Action<int>)Delegate.Combine(component.onCountChanged, new Action<int>(OnHelperCountChanged));
			helperItems.Add(component);
		}
		helperItemPrefab.gameObject.SetActive(false);
	}

	private void OnHelperCountChanged(int delta)
	{
		CanIUseHelperCount -= delta;
		UpdateTotalCount();
		for (int i = 0; i < helperItems.Count; i++)
		{
			helperItems[i].UpdateUI();
		}
	}

	private void ClearHelperItems()
	{
		foreach (HelperItem helperItem in helperItems)
		{
			if ((Object)(object)helperItem != (Object)null)
			{
				helperItem.onCountChanged = (Action<int>)Delegate.Remove(helperItem.onCountChanged, new Action<int>(OnHelperCountChanged));
				if ((Object)(object)((Component)helperItem).gameObject != (Object)null)
				{
					Object.Destroy((Object)(object)((Component)helperItem).gameObject);
				}
			}
		}
		helperItems.Clear();
	}

	private void OnDestroy()
	{
		ClearHelperItems();
	}
}
