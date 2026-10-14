using UnityEngine;

public class TalentHelper : TalentFramework
{
	private void Awake()
	{
		TalentManager talentManager = TalentDataManager.Instance?.GetCurrentTalentManager();
		if ((Object)(object)talentManager != (Object)null)
		{
			talentManager.talentPanels[1] = this;
		}
	}

	public override void Start()
	{
		talentType = TalentType.talentHelper;
		base.Start();
	}

	public override void ResetTalent()
	{
		if (base.IsCoinScene)
		{
			GameDataManager.Instance.SetLaoDongLi(1f);
		}
		base.ResetTalent();
	}
}
