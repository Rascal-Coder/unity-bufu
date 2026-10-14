using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Module.Facility;

public class HelperSkill : MonoBehaviour
{
	[Header("UI 元素")]
	[SerializeField]
	private Image skillIcon;

	[SerializeField]
	private Image skillLock;

	[SerializeField]
	private Image borderImage;

	[SerializeField]
	private TextMeshProUGUI skillunLockText;

	[SerializeField]
	private Button skillButton;

	[SerializeField]
	private SkillTips skillTipsPrefab;

	[SerializeField]
	private Transform skillTipsParent;

	private string _awakeSkillText;

	private string _skillName;

	private int _helperId;

	private int _skillLevel;

	private bool _isUnlocked;

	private int _unlockLevel;

	private SkillTips _currentTips;

	public void Awake()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		((UnityEvent)skillButton.onClick).AddListener((UnityAction)delegate
		{
			if (!((Object)(object)_currentTips != (Object)null))
			{
				if ((Object)(object)skillTipsPrefab == (Object)null)
				{
					UIManager.Instance.ShowCommonDialog(_awakeSkillText);
				}
				else
				{
					Transform val = (((Object)(object)skillTipsParent != (Object)null) ? skillTipsParent : UIManager.Instance.GetLayerTransform(UILayer.Popup));
					_currentTips = Object.Instantiate<SkillTips>(skillTipsPrefab, val);
					((Component)_currentTips).transform.SetAsLastSibling();
					_currentTips.Initialize(_isUnlocked, _unlockLevel, _skillName, _awakeSkillText, _helperId, _skillLevel, OnTipsClosed);
				}
			}
		});
	}

	private void OnTipsClosed()
	{
		_currentTips = null;
	}

	public void SetSkillIcon(int helperId, int level)
	{
		_helperId = helperId;
		_skillLevel = level;
		if ((Object)(object)skillIcon != (Object)null)
		{
			((Component)skillIcon).gameObject.SetActive(false);
			SingletonMono<ResManager>.Instance.LoadSpriteAsync(Constants.GetHelperSkillIconPath(helperId, level), delegate(Sprite sprite)
			{
				skillIcon.sprite = sprite;
				((Component)skillIcon).gameObject.SetActive(true);
			});
		}
	}

	public void SetData(bool isUnlocked, int skillLevel, string awakeSkillText, string skillName = "")
	{
		_isUnlocked = isUnlocked;
		_unlockLevel = skillLevel;
		_awakeSkillText = awakeSkillText;
		_skillName = skillName;
		if ((Object)(object)skillLock != (Object)null)
		{
			((Component)skillLock).gameObject.SetActive(!isUnlocked);
		}
		if ((Object)(object)skillunLockText != (Object)null)
		{
			((TMP_Text)skillunLockText).text = $"{skillLevel}级解锁";
			((Component)skillunLockText).gameObject.SetActive(!isUnlocked);
		}
	}
}
