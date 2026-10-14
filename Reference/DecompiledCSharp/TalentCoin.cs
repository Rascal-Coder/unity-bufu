using UnityEngine;

public class TalentCoin : TalentFramework
{
	private float currentBuffValue;

	[HideInInspector]
	public GameObject talentRebornTipsItem;

	[Header("重生面板父物体")]
	[SerializeField]
	private Transform rebornPanel;

	private TalentRebornTip talentRebornTip;

	private void Awake()
	{
		TalentManager talentManager = TalentDataManager.Instance?.GetCurrentTalentManager();
		if ((Object)(object)talentManager != (Object)null)
		{
			talentManager.talentPanels[0] = this;
		}
	}

	public override void Start()
	{
		talentType = TalentType.talentCoin;
		currentBuffValue = 0f;
		if ((Object)(object)talentRebornTipsItem == (Object)null)
		{
			SingletonMono<ResManager>.Instance.LoadPrefabAsync("Assets/GameRes/NewUI/Talent/prefabs/talentRebornTipsItem.prefab", delegate(GameObject prefab)
			{
				talentRebornTipsItem = Object.Instantiate<GameObject>(prefab, rebornPanel);
				talentRebornTipsItem.SetActive(false);
				talentRebornTip = talentRebornTipsItem.GetComponent<TalentRebornTip>();
			});
		}
		_ = base.IsCoinScene;
		base.Start();
	}

	public void RebornInvok()
	{
		SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
		if ((Object)(object)talentRebornTipsItem == (Object)null)
		{
			SingletonMono<ResManager>.Instance.LoadPrefabAsync("Assets/GameRes/NewUI/Talent/prefabs/talentRebornTipsItem.prefab", delegate(GameObject prefab)
			{
				talentRebornTipsItem = Object.Instantiate<GameObject>(prefab, rebornPanel);
				talentRebornTipsItem.SetActive(false);
				talentRebornTip = talentRebornTipsItem.GetComponent<TalentRebornTip>();
				talentRebornTipsItem.SetActive(true);
				talentRebornTip?.RefreshTalentRebornTip(0.0);
			});
		}
		else
		{
			talentRebornTipsItem.SetActive(true);
			if ((Object)(object)talentRebornTip != (Object)null)
			{
				talentRebornTip.RefreshTalentRebornTip(0.0);
			}
		}
	}

	public override void ResetTalent()
	{
		TalentSceneType sceneType = (((GameDataManager.Instance?.GetCurrentWorldId() ?? 1) != 1) ? TalentSceneType.DiceSceneTalent : TalentSceneType.CoinSceneTalent);
		GameDataManager.Instance.ResetTalentPoint(sceneType);
		if (base.IsCoinScene)
		{
			GameDataManager.Instance.SetSoundSleep(0f);
			GameDataManager.Instance.SetGoldAge(0f);
		}
		base.ResetTalent();
	}

	public override void OffWindow()
	{
		if ((Object)(object)talentResetTipsItem != (Object)null)
		{
			talentResetTipsItem.gameObject.SetActive(false);
		}
		if ((Object)(object)SkillTip != (Object)null)
		{
			((Component)SkillTip).gameObject.SetActive(false);
		}
		if ((Object)(object)talentRebornTipsItem != (Object)null)
		{
			talentRebornTipsItem.gameObject.SetActive(false);
		}
	}
}
