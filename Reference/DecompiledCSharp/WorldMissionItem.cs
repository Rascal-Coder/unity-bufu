using System;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using cfg;

public class WorldMissionItem : MonoBehaviour
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static UnityAction _003C_003E9__9_0;

		internal void _003CStart_003Eb__9_0()
		{
			BroadcastManager.ShowPopUpTip("任务未完成,请继续加油");
		}
	}

	private int itmeId;

	private int missionMaxNum;

	[Header("物品图标")]
	[SerializeField]
	private Image icon;

	[Header("可获得积分显示")]
	[SerializeField]
	private TextMeshProUGUI scoreTxt;

	[Header("任务介绍")]
	[SerializeField]
	private TextMeshProUGUI title;

	[Header("任务进度文本")]
	[SerializeField]
	private TextMeshProUGUI progressTxt;

	[Header("任务进度条")]
	[SerializeField]
	private Image progress;

	[Header("奖励未完成按键")]
	[SerializeField]
	private Button noCanClampBtn;

	[Header("奖励领取按键")]
	[SerializeField]
	private Button clampBtn;

	private void Start()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		((UnityEvent)clampBtn.onClick).AddListener(new UnityAction(BtnClick));
		ButtonClickedEvent onClick = noCanClampBtn.onClick;
		object obj = _003C_003Ec._003C_003E9__9_0;
		if (obj == null)
		{
			UnityAction val = delegate
			{
				BroadcastManager.ShowPopUpTip("任务未完成,请继续加油");
			};
			_003C_003Ec._003C_003E9__9_0 = val;
			obj = (object)val;
		}
		((UnityEvent)onClick).AddListener((UnityAction)obj);
	}

	public void Init(int id)
	{
		itmeId = id;
		WorldCupMisson oneWorldCupMisson = ConfigManager.Instance.GetOneWorldCupMisson(id);
		if (oneWorldCupMisson != null)
		{
			missionMaxNum = oneWorldCupMisson.MissionNum;
			if ((Object)(object)title != (Object)null)
			{
				((TMP_Text)title).text = oneWorldCupMisson.MissionName.Replace("SSS", oneWorldCupMisson.MissionNum.ToString());
			}
			if ((Object)(object)scoreTxt != (Object)null)
			{
				((TMP_Text)scoreTxt).text = oneWorldCupMisson.MissionReward.ToString();
			}
		}
	}

	public void Refresh()
	{
		int num = 0;
		if (itmeId - 1 >= 0)
		{
			num = WorldCupDataManager.Instance.GetWorldCupMissionProgress(itmeId - 1);
		}
		if ((Object)(object)progressTxt != (Object)null)
		{
			((TMP_Text)progressTxt).text = $"{num}/{missionMaxNum}";
		}
		progress.fillAmount = (float)num / (float)missionMaxNum;
		if (num < missionMaxNum)
		{
			((Component)noCanClampBtn).gameObject.SetActive(true);
			((Component)clampBtn).gameObject.SetActive(false);
		}
		else if (num >= missionMaxNum)
		{
			switch (WorldCupDataManager.Instance.GetWorldCupMissionClamp(itmeId - 1))
			{
			case 0:
				((Component)noCanClampBtn).gameObject.SetActive(false);
				((Component)clampBtn).gameObject.SetActive(true);
				break;
			case 1:
				((Component)noCanClampBtn).gameObject.SetActive(false);
				((Component)clampBtn).gameObject.SetActive(false);
				break;
			}
		}
	}

	public void BtnClick()
	{
		int missionReward = ConfigManager.Instance.GetOneWorldCupMisson(itmeId).MissionReward;
		WorldCupDataManager.Instance.AddWorldScore(missionReward);
		if (itmeId - 1 >= 0)
		{
			WorldCupDataManager.Instance.SetWorldCupMissionClamp(itmeId - 1, 1);
		}
		if ((Object)(object)WorldCupPanel.Instance != (Object)null)
		{
			WorldCupPanel.Instance.RefrashPanel(2);
		}
		BroadcastManager.ShowPopUpTip("任务完成,已获得积分奖励");
	}
}
