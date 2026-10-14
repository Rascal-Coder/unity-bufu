using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SelectSkill : MonoBehaviour
{
	[Header("技能名字")]
	[SerializeField]
	private TextMeshProUGUI name;

	[Header("技能介绍")]
	[SerializeField]
	private TextMeshProUGUI introduce;

	[Header("技能获取按键")]
	[SerializeField]
	private Button selectBtn;

	[Header("技能图标")]
	[SerializeField]
	private Image skillIcon;

	private SkillFunction currentFunction;

	public void Init(string nametxt, string introducetxt, SkillFunction skill)
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Expected O, but got Unknown
		if ((Object)(object)name != (Object)null)
		{
			((TMP_Text)name).text = nametxt;
		}
		if ((Object)(object)introduce != (Object)null)
		{
			((TMP_Text)introduce).text = introducetxt;
		}
		if ((Object)(object)selectBtn != (Object)null)
		{
			((UnityEventBase)selectBtn.onClick).RemoveAllListeners();
			currentFunction = skill;
			((UnityEvent)selectBtn.onClick).AddListener(new UnityAction(SelectBtnFunction));
		}
	}

	public void SelectBtnFunction()
	{
		if (currentFunction != SkillFunction.UpgradeCoinValue)
		{
			_ = 1;
		}
		else if ((Object)(object)ChallengeMain.Instance != (Object)null)
		{
			ChallengeMain instance = ChallengeMain.Instance;
			instance.SetSelectSkillPanel(active: false);
			Time.timeScale = 1f;
			float multiplyBuff = instance.GetMultiplyBuff();
			multiplyBuff *= 1.2f;
			instance.SetMultiplyBuff(multiplyBuff);
			instance.UpdateCoinsValueByType(CoinType.Silver);
			instance.UpdateCoinsValueByType(CoinType.Gold);
		}
	}

	public Sprite GetSkillIcon()
	{
		Sprite spr = null;
		switch (currentFunction)
		{
		case SkillFunction.UpgradeCoinValue:
			SingletonMono<ResManager>.Instance.LoadSpriteAsync("xxxx", delegate(Sprite sprite)
			{
				spr = sprite;
			}, isAtlas: false);
			return spr;
		case SkillFunction.UpgradeHelperSpeed:
			SingletonMono<ResManager>.Instance.LoadSpriteAsync("xxxx", delegate(Sprite sprite)
			{
				spr = sprite;
			}, isAtlas: false);
			return spr;
		default:
			return spr;
		}
	}
}
