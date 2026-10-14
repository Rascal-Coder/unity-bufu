using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class OfflineIncomeUI : MonoBehaviour
{
	[SerializeField]
	private Button bgButton;

	[SerializeField]
	private Button closeButton;

	[Header("UI组件")]
	[SerializeField]
	private TextMeshProUGUI rewardText;

	[SerializeField]
	private TextMeshProUGUI offlineTimeText;

	[SerializeField]
	private Button normalClaimButton;

	[SerializeField]
	private Button doubleClaimButton;

	private OfflineIncomeManager.OfflineRewardData _rewardData;

	private void Awake()
	{
		InitUI();
	}

	protected virtual void InitUI()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Expected O, but got Unknown
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Expected O, but got Unknown
		if ((Object)(object)bgButton != (Object)null)
		{
			((UnityEvent)bgButton.onClick).AddListener(new UnityAction(OnCloseClick));
		}
		if ((Object)(object)closeButton != (Object)null)
		{
			((UnityEvent)closeButton.onClick).AddListener(new UnityAction(OnNormalClaimClick));
		}
		if ((Object)(object)normalClaimButton != (Object)null)
		{
			((UnityEvent)normalClaimButton.onClick).AddListener(new UnityAction(OnNormalClaimClick));
		}
		if ((Object)(object)doubleClaimButton != (Object)null)
		{
			((UnityEvent)doubleClaimButton.onClick).AddListener(new UnityAction(OnDoubleClaimClick));
		}
		((Component)this).gameObject.SetActive(false);
	}

	public virtual void OnOpen()
	{
		UpdateRewardDisplay();
		((Component)this).transform.SetAsLastSibling();
	}

	public virtual void OnClose()
	{
		((Component)this).gameObject.SetActive(false);
	}

	private void UpdateRewardDisplay()
	{
		_rewardData = OfflineIncomeManager.Instance.CurrentRewardData;
		if (_rewardData == null || !_rewardData.hasReward)
		{
			Debug.LogWarning((object)"没有离线收益数据");
			OnClose();
			return;
		}
		((Component)this).gameObject.SetActive(true);
		if ((Object)(object)offlineTimeText != (Object)null)
		{
			((TMP_Text)offlineTimeText).text = "离线时间: " + OfflineIncomeManager.Instance.FormatTime(_rewardData.offlineTime);
		}
		if ((Object)(object)rewardText != (Object)null)
		{
			((TMP_Text)rewardText).text = "离线收益:" + NumberFormatter.FormatNumber(_rewardData.coin);
		}
	}

	private string FormatRewardText(float multiplier)
	{
		if (_rewardData == null)
		{
			return "";
		}
		string text = "";
		if (_rewardData.coin > 0.0)
		{
			text += $"金币: {_rewardData.coin}\n";
		}
		return text.Trim();
	}

	private void OnNormalClaimClick()
	{
		SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
		OfflineIncomeManager.Instance.ClaimOfflineReward();
		OnClose();
	}

	private void OnDoubleClaimClick()
	{
		SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
		FakeVideoManager.Instance.ShowAd(delegate
		{
			OfflineIncomeManager.Instance.ClaimOfflineReward(isDouble: true);
			OnClose();
		}, delegate
		{
			OfflineIncomeManager.Instance.ClaimOfflineReward();
			OnClose();
		}, "离线奖励双倍");
	}

	private void OnCloseClick()
	{
		SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
		OfflineIncomeManager.Instance.ClaimOfflineReward();
		OnClose();
	}

	protected void OnDestroy()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Expected O, but got Unknown
		if ((Object)(object)normalClaimButton != (Object)null)
		{
			((UnityEvent)normalClaimButton.onClick).RemoveListener(new UnityAction(OnNormalClaimClick));
		}
		if ((Object)(object)doubleClaimButton != (Object)null)
		{
			((UnityEvent)doubleClaimButton.onClick).RemoveListener(new UnityAction(OnDoubleClaimClick));
		}
		if ((Object)(object)bgButton != (Object)null)
		{
			((UnityEvent)bgButton.onClick).RemoveListener(new UnityAction(OnCloseClick));
		}
	}
}
