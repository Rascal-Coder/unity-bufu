using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TalentResetTip : MonoBehaviour
{
	[Header("返回按键")]
	[SerializeField]
	private Button backBtn;

	[Header("回收天赋点文本")]
	[SerializeField]
	private TextMeshProUGUI resetPointTxt;

	[Header("取消按键")]
	[SerializeField]
	private Button cancelBtn;

	[Header("重置按键")]
	[SerializeField]
	private Button resetBtn;

	[Header("遮罩背景")]
	[SerializeField]
	private Button mask;

	[Header("目前所在的天赋面板")]
	private TalentType talentType;

	private void Start()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Expected O, but got Unknown
		((UnityEvent)backBtn.onClick).AddListener((UnityAction)delegate
		{
			SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
			((Component)this).gameObject.SetActive(false);
		});
		((UnityEvent)cancelBtn.onClick).AddListener((UnityAction)delegate
		{
			SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
			((Component)this).gameObject.SetActive(false);
		});
		((UnityEvent)mask.onClick).AddListener((UnityAction)delegate
		{
			SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
			((Component)this).gameObject.SetActive(false);
		});
		((UnityEvent)resetBtn.onClick).AddListener((UnityAction)delegate
		{
			SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
			if ((Object)(object)GameDataManager.Instance != (Object)null)
			{
				TalentManager manager = TalentDataManager.Instance?.GetCurrentTalentManager();
				if (!((Object)(object)manager == (Object)null))
				{
					TalentType currentTalentType = manager.GetCurrentTalentType();
					TalentSceneType sceneType = ((TalentDataManager.Instance == null || !TalentDataManager.Instance.IsCoinScene) ? TalentSceneType.DiceSceneTalent : TalentSceneType.CoinSceneTalent);
					if (GameDataManager.Instance.GetCostPoint(currentTalentType, sceneType) > 0)
					{
						FakeVideoManager.Instance.ShowAd(delegate
						{
							int num = (int)currentTalentType;
							if ((Object)(object)manager.talentPanels[num] != (Object)null)
							{
								manager.talentPanels[num].ResetTalent();
							}
							RefreshTalentResetTip(currentTalentType);
						}, delegate
						{
							BroadcastManager.ShowPopUpTip("广告观看未完成，无法重置天赋");
						}, "重置天赋点");
					}
				}
			}
		});
	}

	public void RefreshTalentResetTip(TalentType talentType)
	{
		if ((Object)(object)GameDataManager.Instance != (Object)null)
		{
			TalentSceneType sceneType = ((TalentDataManager.Instance == null || !TalentDataManager.Instance.IsCoinScene) ? TalentSceneType.DiceSceneTalent : TalentSceneType.CoinSceneTalent);
			((TMP_Text)resetPointTxt).text = GameDataManager.Instance.GetCostPoint(talentType, sceneType).ToString();
		}
		TalentSceneType sceneType2 = ((TalentDataManager.Instance == null || !TalentDataManager.Instance.IsCoinScene) ? TalentSceneType.DiceSceneTalent : TalentSceneType.CoinSceneTalent);
		if (GameDataManager.Instance.GetCostPoint(talentType, sceneType2) > 0)
		{
			((Selectable)resetBtn).interactable = true;
		}
		else
		{
			((Selectable)resetBtn).interactable = false;
		}
		if (!((Selectable)resetBtn).interactable)
		{
			GameObjectClickListener gameObjectClickListener = GameObjectClickListener.Get(((Component)resetBtn).gameObject);
			gameObjectClickListener.onClick = (GameObjectClickListener.VoidDelegate)Delegate.Combine(gameObjectClickListener.onClick, (GameObjectClickListener.VoidDelegate)delegate
			{
				BroadcastManager.ShowPopUpTip("当前可回收天赋点为0,无法重置");
			});
		}
		else
		{
			GameObjectClickListener.Get(((Component)resetBtn).gameObject).onClick = null;
		}
	}
}
