using UnityEngine;

public class TalentSpecial : TalentFramework
{
	private void Awake()
	{
		TalentManager talentManager = TalentDataManager.Instance?.GetCurrentTalentManager();
		if ((Object)(object)talentManager != (Object)null)
		{
			talentManager.talentPanels[2] = this;
		}
	}

	public override void Start()
	{
		talentType = TalentType.talentSpecial;
		if (base.IsCoinScene)
		{
			IMainSceneView mainSceneView = (((Object)(object)UIManager.Instance != (Object)null) ? UIManager.Instance.ActiveMainScene : null);
			if (mainSceneView != null)
			{
				mainSceneView.coinFlipComplete += TalentDataManager.Instance.LossBrings;
			}
		}
		base.Start();
	}

	public void OnDestroy()
	{
		if (base.IsCoinScene)
		{
			IMainSceneView mainSceneView = (((Object)(object)UIManager.Instance != (Object)null) ? UIManager.Instance.ActiveMainScene : null);
			if (mainSceneView != null)
			{
				mainSceneView.coinFlipComplete -= TalentDataManager.Instance.LossBrings;
			}
		}
	}
}
