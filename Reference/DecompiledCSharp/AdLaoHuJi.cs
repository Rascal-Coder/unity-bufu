using System;
using System.Collections;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using cfg;

public class AdLaoHuJi : MonoBehaviour
{
	[Header("老虎机动画")]
	[SerializeField]
	private SkeletonGraphic laohujiEffect;

	[Header("提示文本")]
	[SerializeField]
	private GameObject txtTip;

	[Header("普通激活按键")]
	[SerializeField]
	private Button jihuoBtn;

	[Header("普通激活按键上的文字")]
	[SerializeField]
	private TextMeshProUGUI jihuoBtnTxt;

	[Header("看广告激活按键")]
	[SerializeField]
	private Button adjihuoBtn;

	[Header("广告按键上的文本")]
	[SerializeField]
	private TextMeshProUGUI adTxt;

	private void OnEnable()
	{
		if ((Object)(object)laohujiEffect != (Object)null)
		{
			laohujiEffect.AnimationState.SetAnimation(0, "effect", false);
		}
		txtTip.SetActive(false);
		((Component)jihuoBtn).gameObject.SetActive(false);
		((Component)adjihuoBtn).gameObject.SetActive(false);
		((MonoBehaviour)this).StopAllCoroutines();
		((MonoBehaviour)this).StartCoroutine(DisplaySupperBtn(2f, delegate
		{
			txtTip.SetActive(true);
			((Component)jihuoBtn).gameObject.SetActive(true);
			((Component)adjihuoBtn).gameObject.SetActive(true);
			float supperTime = GetSupperTime();
			((TMP_Text)jihuoBtnTxt).text = $"激活({supperTime}s)";
		}));
		Common common = ConfigManager.Instance?.GetCommonConfig(76);
		if (common != null)
		{
			float factor = common.Factor;
			factor += TalentDataManager.Instance?.GetDiceTalentBuff(309) ?? 0f;
			((TMP_Text)adTxt).text = $"{factor}倍时间";
		}
	}

	private void Start()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Expected O, but got Unknown
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		DownItem downitem = UIManager.Instance.GetDownItem();
		((UnityEvent)jihuoBtn.onClick).AddListener((UnityAction)delegate
		{
			if ((Object)(object)downitem != (Object)null)
			{
				downitem.SetIsOpenSuperAni(isOpen: false);
				downitem.SetIsOpenSuperSix(isOpen: true);
				float num = ConfigManager.Instance.GetCommonConfig(75)?.Factor ?? 10f;
				if ((GameDataManager.Instance?.GetCurrentWorldId() ?? 1) == 2)
				{
					num += TalentDataManager.Instance?.GetDiceTalentBuff(312) ?? 0f;
				}
				downitem.SetSupperTime(num);
				((Component)this).gameObject.SetActive(false);
				GameDataManager.Instance.SetSupperSixCount(0f);
				downitem.ResetSupperSixTarget();
			}
		});
		((UnityEvent)adjihuoBtn.onClick).AddListener((UnityAction)delegate
		{
			FakeVideoManager.Instance.ShowAd(delegate
			{
				downitem.SetIsOpenSuperAni(isOpen: false);
				downitem.SetIsOpenSuperSix(isOpen: true);
				float num = ConfigManager.Instance.GetCommonConfig(75)?.Factor ?? 20f;
				int num2 = GameDataManager.Instance?.GetCurrentWorldId() ?? 1;
				if (num2 == 2)
				{
					num += TalentDataManager.Instance?.GetDiceTalentBuff(312) ?? 0f;
				}
				float num3 = ConfigManager.Instance.GetCommonConfig(76)?.Factor ?? 5f;
				if (num2 == 2)
				{
					num3 += TalentDataManager.Instance?.GetDiceTalentBuff(309) ?? 0f;
				}
				float supperTime = num * num3;
				downitem.SetSupperTime(supperTime);
				((Component)this).gameObject.SetActive(false);
				GameDataManager.Instance.SetSupperSixCount(0f);
				downitem.ResetSupperSixTarget();
			}, delegate
			{
			}, "超级666,广告增加激活时长");
		});
	}

	public float GetSupperTime()
	{
		float num = ConfigManager.Instance.GetCommonConfig(75)?.Factor ?? 10f;
		if ((GameDataManager.Instance?.GetCurrentWorldId() ?? 1) == 2)
		{
			num += TalentDataManager.Instance?.GetDiceTalentBuff(312) ?? 0f;
		}
		return Mathf.Round(num);
	}

	private IEnumerator DisplaySupperBtn(float time, Action action)
	{
		yield return (object)new WaitForSeconds(time);
		action();
	}
}
