using Module.ChallengeNew;
using UnityEngine;

public class ChallengeDataManager
{
	private GameObject challengePanelObj;

	public ChallengePanel challengePanel;

	private static ChallengeDataManager instance;

	public static ChallengeDataManager Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new ChallengeDataManager();
			}
			return instance;
		}
	}

	public void OpenChallengePanel()
	{
		if ((Object)(object)challengePanelObj == (Object)null)
		{
			if (!((Object)(object)UpgradePanel.Instance != (Object)null))
			{
				return;
			}
			GameObject parent = UpgradePanel.Instance.GetPanelByIndex(4);
			if (!((Object)(object)parent != (Object)null))
			{
				return;
			}
			SingletonMono<ResManager>.Instance.LoadPrefabAsync("Assets/GameRes/NewUI/Challenge/prefabs/ChallengePanel.prefab", delegate(GameObject prefab)
			{
				//IL_002c: Unknown result type (might be due to invalid IL or missing references)
				challengePanelObj = Object.Instantiate<GameObject>(prefab, parent.transform);
				challengePanelObj.transform.localPosition = Vector3.zero;
				ChallengePanel component = challengePanelObj.GetComponent<ChallengePanel>();
				if ((Object)(object)component != (Object)null)
				{
					challengePanel = component;
					challengePanelObj.SetActive(true);
					challengePanel.RefreshHelperList();
					challengePanel.SelectOpenPanel(1);
				}
			});
		}
		else if ((Object)(object)challengePanel != (Object)null)
		{
			challengePanelObj.SetActive(true);
			challengePanel.RefreshHelperList();
			challengePanel.SelectOpenPanel(1);
		}
	}
}
