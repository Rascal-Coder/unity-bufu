using System;
using Module.Http;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Module.Invite;

public class InviteUI : MonoBehaviour
{
	[Header("按钮组件")]
	[SerializeField]
	private Button inviteBtn;

	[Header("按钮组件")]
	[SerializeField]
	private Image alreadyInvite;

	[SerializeField]
	private Button refreshBtn;

	[SerializeField]
	private Button closeBtn;

	[Header("文本组件")]
	[SerializeField]
	private TextMeshProUGUI countdownText;

	[SerializeField]
	private TextMeshProUGUI descriptionText;

	[SerializeField]
	private TextMeshProUGUI statusText;

	[SerializeField]
	private TextMeshProUGUI qiyueText;

	private bool _isCountingDown;

	private long _inviteTime;

	private float _lastRefreshTime;

	private const float REFRESH_COOLDOWN = 5f;

	private void Awake()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Expected O, but got Unknown
		if ((Object)(object)inviteBtn != (Object)null)
		{
			((UnityEvent)inviteBtn.onClick).AddListener(new UnityAction(OnInviteBtnClicked));
		}
		if ((Object)(object)refreshBtn != (Object)null)
		{
			((UnityEvent)refreshBtn.onClick).AddListener(new UnityAction(OnRefreshBtnClicked));
		}
		if ((Object)(object)closeBtn != (Object)null)
		{
			((UnityEvent)closeBtn.onClick).AddListener(new UnityAction(Close));
		}
		if (GameDataManager.Instance.GetHelpedTime() == -1)
		{
			GameDataManager.Instance.SetHelpedTime(0L);
		}
		Image obj = alreadyInvite;
		if (obj != null)
		{
			((Component)obj).gameObject.SetActive(false);
		}
	}

	private void OnInviteBtnClicked()
	{
		Debug.Log((object)"[InviteUI] 点击邀请协助按钮");
		GameDataManager.Instance.SetHelpedTime(-1L);
		InviteFriend();
	}

	private void OnRefreshBtnClicked()
	{
		if (Time.time - _lastRefreshTime < 5f)
		{
			Debug.Log((object)$"[InviteUI] 刷新按钮冷却中，请等待 {5f - (Time.time - _lastRefreshTime):F1} 秒。");
			return;
		}
		_lastRefreshTime = Time.time;
		long num = (long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;
		long helpedTime = GameDataManager.Instance.GetHelpedTime();
		long num2 = 86400000L;
		if (helpedTime > 0 && num - helpedTime < num2)
		{
			Debug.Log((object)"[InviteUI] 被助力Buff还在24小时生效时间内，不需要请求刷新");
			CheckInviteStatus();
			return;
		}
		Debug.Log((object)"[InviteUI] 点击刷新按钮，向服务器获取助力状态");
		SingletonMono<HttpManager>.Instance.GetShareStatus(GameDataManager.Instance.GetOpenId(), null, delegate(bool isSuccess, string msg)
		{
			if (isSuccess)
			{
				Debug.Log((object)("[InviteUI] 刷新成功: " + msg));
				PlayerHelpStatusData playerHelpStatusData = JsonUtility.FromJson<PlayerHelpStatusData>(msg);
				if (playerHelpStatusData != null && playerHelpStatusData.isHelped)
				{
					GameDataManager.Instance.SetHelpedTime((long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds);
					Debug.Log((object)"[InviteUI] 检测到有人助力，更新 HelpedTime 为当前时间");
				}
				else
				{
					Debug.Log((object)"[InviteUI] 刷新助力状态成功，但当前无人助力");
				}
				CheckInviteStatus();
			}
			else
			{
				Debug.Log((object)("[InviteUI] 刷新失败: " + msg));
			}
		});
	}

	private void InviteFriend()
	{
		SingletonMono<HttpManager>.Instance.ShareAppMessage(1, delegate(string msg)
		{
			if (string.IsNullOrEmpty(msg))
			{
				SetStatusText("邀请成功");
				GameDataManager.Instance.SetInviteTime((long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds);
				_inviteTime = GameDataManager.Instance.GetInviteTime();
				CheckInviteStatus();
			}
		}, delegate(string msg)
		{
			SetStatusText("邀请失败: " + msg);
		});
	}

	public void Open()
	{
		((Component)this).gameObject.SetActive(true);
		CheckInviteStatus();
		int num = Mathf.RoundToInt(ConfigManager.Instance.GetCommonFactor(26) * 100f);
		if ((Object)(object)descriptionText != (Object)null)
		{
			((TMP_Text)descriptionText).text = $"邀请好友上线后，全局收益+<color=#e66e17>{num}%</color>";
		}
		OnRefreshBtnClicked();
	}

	private void CheckInviteStatus()
	{
		long num = (long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;
		long helpedTime = GameDataManager.Instance.GetHelpedTime();
		long num2 = 86400000L;
		if (helpedTime > 0 && num - helpedTime < num2)
		{
			_isCountingDown = true;
			if ((Object)(object)alreadyInvite != (Object)null)
			{
				((Component)alreadyInvite).gameObject.SetActive(true);
			}
			if ((Object)(object)countdownText != (Object)null)
			{
				((Component)countdownText).gameObject.SetActive(true);
			}
			if ((Object)(object)inviteBtn != (Object)null)
			{
				((Component)inviteBtn).gameObject.SetActive(false);
			}
			if ((Object)(object)statusText != (Object)null)
			{
				((TMP_Text)statusText).text = "助力加速中";
			}
			if ((Object)(object)qiyueText != (Object)null)
			{
				((Component)qiyueText).gameObject.SetActive(false);
			}
		}
		else if (helpedTime == -1)
		{
			_isCountingDown = false;
			if ((Object)(object)alreadyInvite != (Object)null)
			{
				((Component)alreadyInvite).gameObject.SetActive(true);
			}
			if ((Object)(object)countdownText != (Object)null)
			{
				((Component)countdownText).gameObject.SetActive(false);
			}
			if ((Object)(object)inviteBtn != (Object)null)
			{
				((Component)inviteBtn).gameObject.SetActive(false);
			}
			if ((Object)(object)statusText != (Object)null)
			{
				((TMP_Text)statusText).text = "已邀请，等待好友助力";
			}
			if ((Object)(object)qiyueText != (Object)null)
			{
				((Component)qiyueText).gameObject.SetActive(true);
			}
		}
		else
		{
			_isCountingDown = false;
			if ((Object)(object)alreadyInvite != (Object)null)
			{
				((Component)alreadyInvite).gameObject.SetActive(false);
			}
			if ((Object)(object)countdownText != (Object)null)
			{
				((Component)countdownText).gameObject.SetActive(false);
			}
			if ((Object)(object)inviteBtn != (Object)null)
			{
				((Component)inviteBtn).gameObject.SetActive(true);
			}
			if ((Object)(object)statusText != (Object)null)
			{
				((TMP_Text)statusText).text = "未邀请助力";
			}
			if ((Object)(object)qiyueText != (Object)null)
			{
				((Component)qiyueText).gameObject.SetActive(true);
			}
		}
	}

	private void Update()
	{
		if (_isCountingDown)
		{
			long num = (long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;
			long helpedTime = GameDataManager.Instance.GetHelpedTime();
			long num2 = num - helpedTime;
			long num3 = 86400000 - num2;
			if (num3 <= 0)
			{
				_isCountingDown = false;
				CheckInviteStatus();
			}
			else if ((Object)(object)countdownText != (Object)null)
			{
				TimeSpan timeSpan = TimeSpan.FromMilliseconds(num3);
				((TMP_Text)countdownText).text = $"{timeSpan.Days}天{timeSpan.Hours}小时{timeSpan.Minutes}分{timeSpan.Seconds}秒";
			}
		}
	}

	public void Close()
	{
		((Component)this).gameObject.SetActive(false);
	}

	public void SetCountdownText(string text)
	{
		if ((Object)(object)countdownText != (Object)null)
		{
			((TMP_Text)countdownText).text = text;
		}
	}

	public void SetDescriptionText(string text)
	{
		if ((Object)(object)descriptionText != (Object)null)
		{
			((TMP_Text)descriptionText).text = text;
		}
	}

	public void SetStatusText(string text)
	{
		if ((Object)(object)statusText != (Object)null)
		{
			((TMP_Text)statusText).text = text;
		}
	}

	private void OnDestroy()
	{
		if ((Object)(object)inviteBtn != (Object)null)
		{
			((UnityEventBase)inviteBtn.onClick).RemoveAllListeners();
		}
		if ((Object)(object)refreshBtn != (Object)null)
		{
			((UnityEventBase)refreshBtn.onClick).RemoveAllListeners();
		}
		if ((Object)(object)closeBtn != (Object)null)
		{
			((UnityEventBase)closeBtn.onClick).RemoveAllListeners();
		}
	}
}
