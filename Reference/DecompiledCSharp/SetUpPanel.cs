using System;
using System.Runtime.CompilerServices;
using Module.Http;
using TMPro;
using TTSDK;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SetUpPanel : MonoBehaviour
{
	[Serializable]
	private class ExitRequest
	{
		public GameDataSave gameDataSave;

		public long bestScore;
	}

	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static UnityAction _003C_003E9__15_0;

		public static UnityAction<float> _003C_003E9__21_4;

		public static UnityAction<float> _003C_003E9__21_5;

		public static Action _003C_003E9__22_0;

		public static Action _003C_003E9__22_1;

		public static Action _003C_003E9__23_1;

		public static Action _003C_003E9__24_1;

		internal void _003CAwake_003Eb__15_0()
		{
			if (AppConst.DebugMode)
			{
				SingletonMono<GM>.Instance.ToggleGMUI(show: true);
			}
		}

		internal void _003CStart_003Eb__21_4(float value)
		{
			SoundManager.Instance.SetBGMVolume(value);
		}

		internal void _003CStart_003Eb__21_5(float value)
		{
			SoundManager.Instance.SetSoundVolume(value);
		}

		internal void _003COnResetButtonClicked_003Eb__22_0()
		{
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Invalid comparison between Unknown and I4
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Invalid comparison between Unknown and I4
			if ((Object)(object)SingletonMono<GM>.Instance != (Object)null)
			{
				SingletonMono<GM>.Instance.ResetAllPlayerData(istest: true);
			}
			BroadcastManager.ShowPopUpTip("重置成功，请手动重启或退出游戏");
			if ((int)Application.platform == 50 || (int)Application.platform == 17)
			{
				TT.RestartMiniProgramSync();
			}
		}

		internal void _003COnResetButtonClicked_003Eb__22_1()
		{
			Debug.Log((object)"用户点击了取消");
		}

		internal void _003COnRestoreButtonClicked_003Eb__23_1()
		{
			Debug.Log((object)"用户点击了取消");
		}

		internal void _003COnExitButtonClicked_003Eb__24_1()
		{
			Debug.Log((object)"[SetUpPanel] Cancel player exit request.");
		}
	}

	[Header("音乐开关")]
	[SerializeField]
	private Toggle musicToggle;

	[Header("音效开关")]
	[SerializeField]
	private Toggle soundToggle;

	[SerializeField]
	private Button backBtn;

	[SerializeField]
	private Button mask;

	[Header("目前音乐大小")]
	[SerializeField]
	private float musicVolume;

	[Header("目前音效大小")]
	[SerializeField]
	private float soundVolume;

	[SerializeField]
	private Slider musicSlider;

	[SerializeField]
	private Slider soundSlider;

	[SerializeField]
	private Button resetButton;

	[SerializeField]
	private Button restoreButton;

	[SerializeField]
	private Button exitButton;

	[SerializeField]
	private TextMeshProUGUI gameVersion;

	[SerializeField]
	private Button gmBtn;

	private bool isSubmittingExit;

	private const float COOLDOWN_DURATION = 3600f;

	private void Awake()
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Expected O, but got Unknown
		if ((Object)(object)gameVersion != (Object)null)
		{
			((TMP_Text)gameVersion).text = SingletonMono<ResManager>.Instance.Version;
			((Component)gameVersion).gameObject.SetActive(true);
		}
		if (AppConst.DebugMode)
		{
			Button obj = gmBtn;
			if (obj != null)
			{
				ButtonClickedEvent onClick = obj.onClick;
				object obj2 = _003C_003Ec._003C_003E9__15_0;
				if (obj2 == null)
				{
					UnityAction val = delegate
					{
						if (AppConst.DebugMode)
						{
							SingletonMono<GM>.Instance.ToggleGMUI(show: true);
						}
					};
					_003C_003Ec._003C_003E9__15_0 = val;
					obj2 = (object)val;
				}
				((UnityEvent)onClick).AddListener((UnityAction)obj2);
			}
		}
		if ((Object)(object)exitButton != (Object)null)
		{
			((Component)exitButton).gameObject.SetActive(false);
		}
		if ((Object)(object)restoreButton != (Object)null)
		{
			((Component)restoreButton).gameObject.SetActive(false);
		}
	}

	private float GetRemainingCooldown()
	{
		if (PlayerPrefs.HasKey("RestoreButtonClickedTime") && long.TryParse(PlayerPrefs.GetString("RestoreButtonClickedTime", ""), out var result))
		{
			DateTime dateTime = DateTime.FromBinary(result);
			double totalSeconds = (DateTime.UtcNow - dateTime).TotalSeconds;
			if (totalSeconds < 3600.0)
			{
				return (float)(3600.0 - totalSeconds);
			}
		}
		return 0f;
	}

	private void UpdateRestoreButtonState()
	{
		if ((Object)(object)restoreButton != (Object)null)
		{
			if (GetRemainingCooldown() > 0f)
			{
				((Selectable)restoreButton).interactable = false;
			}
			else
			{
				((Selectable)restoreButton).interactable = true;
			}
		}
	}

	private void OnEnable()
	{
		ShowRecoveryBtn();
		UpdateRestoreButtonState();
	}

	public void ShowRecoveryBtn()
	{
		Button obj = restoreButton;
		if (obj != null)
		{
			((Component)obj).gameObject.SetActive(true);
		}
	}

	private void Start()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Expected O, but got Unknown
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Expected O, but got Unknown
		((UnityEvent)backBtn.onClick).AddListener((UnityAction)delegate
		{
			((Component)this).gameObject.SetActive(false);
		});
		((UnityEvent)mask.onClick).AddListener((UnityAction)delegate
		{
			((Component)this).gameObject.SetActive(false);
		});
		((UnityEvent<bool>)(object)musicToggle.onValueChanged).AddListener((UnityAction<bool>)delegate(bool isOn)
		{
			SetMusicOn(isOn);
		});
		((UnityEvent<bool>)(object)soundToggle.onValueChanged).AddListener((UnityAction<bool>)delegate(bool isOn)
		{
			SetSoundOn(isOn);
		});
		((UnityEvent<float>)(object)musicSlider.onValueChanged).AddListener((UnityAction<float>)delegate(float value)
		{
			SoundManager.Instance.SetBGMVolume(value);
		});
		((UnityEvent<float>)(object)soundSlider.onValueChanged).AddListener((UnityAction<float>)delegate(float value)
		{
			SoundManager.Instance.SetSoundVolume(value);
		});
		musicToggle.isOn = SoundManager.Instance.GetMusicIsOn();
		soundToggle.isOn = SoundManager.Instance.GetSoundIsOn();
		musicSlider.value = SoundManager.Instance.GetBGMVolume();
		soundSlider.value = SoundManager.Instance.GetSoundVolume();
		((UnityEvent)restoreButton.onClick).AddListener(new UnityAction(OnRestoreButtonClicked));
		Button obj = resetButton;
		if (obj != null)
		{
			((Component)obj).gameObject.SetActive(false);
		}
		Button obj2 = exitButton;
		if (obj2 != null)
		{
			((UnityEvent)obj2.onClick).AddListener(new UnityAction(OnExitButtonClicked));
		}
	}

	public void OnResetButtonClicked()
	{
		UIManager.Instance.ShowCommonDialog("尝试清除本地存档,此操作会删除所有数据,操作完成后请重启游戏", delegate
		{
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Invalid comparison between Unknown and I4
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Invalid comparison between Unknown and I4
			if ((Object)(object)SingletonMono<GM>.Instance != (Object)null)
			{
				SingletonMono<GM>.Instance.ResetAllPlayerData(istest: true);
			}
			BroadcastManager.ShowPopUpTip("重置成功，请手动重启或退出游戏");
			if ((int)Application.platform == 50 || (int)Application.platform == 17)
			{
				TT.RestartMiniProgramSync();
			}
		}, delegate
		{
			Debug.Log((object)"用户点击了取消");
		});
		((Component)this).gameObject.SetActive(false);
	}

	public void OnRestoreButtonClicked()
	{
		if (GetRemainingCooldown() > 0f)
		{
			return;
		}
		UIManager.Instance.ShowCommonDialog("尝试请求服务器获取您此前最高等级的存档,请求成功后会覆盖本地,提示成功后请重启小游戏", delegate
		{
			string openId = GameDataManager.Instance.GetOpenId();
			if (string.IsNullOrEmpty(openId))
			{
				BroadcastManager.ShowPopUpTip("用户ID为空,无法恢复存档");
			}
			else
			{
				PlayerPrefs.SetString("RestoreButtonClickedTime", DateTime.UtcNow.ToBinary().ToString());
				PlayerPrefs.Save();
				UpdateRestoreButtonState();
				int worldId = GameDataManager.Instance.GetCurrentWorldId();
				SingletonMono<HttpManager>.Instance.GetHighestWorldBackup(openId, worldId, delegate(bool success, string data)
				{
					if (success)
					{
						if (GameDataManager.Instance.RestoreWorldFromServerResponse(data, out var error))
						{
							GameDataManager.Instance.SetCloudSyncSuspended(suspended: false);
							GameDataManager.Instance.RetryPendingWorldSaves();
							BroadcastManager.ShowPopUpTip("存档恢复成功");
						}
						else
						{
							Debug.LogWarning((object)$"[SetUpPanel] 恢复世界 {worldId} 失败: {error}");
							BroadcastManager.ShowPopUpTip("服务器存档无效，恢复失败");
						}
					}
					else
					{
						BroadcastManager.ShowPopUpTip("没有找到更高级的存档");
					}
				});
			}
		}, delegate
		{
			Debug.Log((object)"用户点击了取消");
		});
	}

	private void OnExitButtonClicked()
	{
		if (!AppConst.DebugMode || isSubmittingExit)
		{
			return;
		}
		string openId = GameDataManager.Instance.GetOpenId();
		if (string.IsNullOrEmpty(openId))
		{
			BroadcastManager.ShowPopUpTip("OpenId为空，无法执行退出");
			return;
		}
		UIManager.Instance.ShowCommonDialog("将先执行存档，再尝试仅携带GameData进行轻量退出，是否继续？", delegate
		{
			SubmitExit(openId);
		}, delegate
		{
			Debug.Log((object)"[SetUpPanel] Cancel player exit request.");
		});
	}

	private void SubmitExit(string openId)
	{
		isSubmittingExit = true;
		SetExitButtonInteractable(interactable: false);
		GameDataManager.Instance.SaveAll2Server(delegate(bool saveSuccess, string saveResponse)
		{
			Debug.Log((object)$"[SetUpPanel] SaveAll2Server success={saveSuccess}, response={saveResponse}");
			TryLightweightExit(openId, saveSuccess);
		});
	}

	private void TryLightweightExit(string openId, bool saveAllSuccess)
	{
		isSubmittingExit = false;
		SetExitButtonInteractable(interactable: true);
		if (saveAllSuccess)
		{
			BroadcastManager.ShowPopUpTip("存档成功");
		}
		else
		{
			BroadcastManager.ShowPopUpTip("存档失败");
		}
	}

	private void SetExitButtonInteractable(bool interactable)
	{
		if ((Object)(object)exitButton != (Object)null)
		{
			((Selectable)exitButton).interactable = interactable;
		}
	}

	private void Update()
	{
		UpdateRestoreButtonState();
	}

	public void SetMusicOn(bool isOn)
	{
		if (isOn)
		{
			SoundManager.Instance.ResetBGMPlayerPrefs();
		}
		else
		{
			SoundManager.Instance.StopBGMNoPlayerPrefs();
		}
	}

	public void SetSoundOn(bool isOn)
	{
		if (isOn)
		{
			SoundManager.Instance.ResetSoundPlayerPrefs();
		}
		else
		{
			SoundManager.Instance.StopSoundNoPlayerPrefs();
		}
	}
}
