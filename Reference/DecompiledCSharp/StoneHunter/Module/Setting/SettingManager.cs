using System;
using UnityEngine;

namespace StoneHunter.Module.Setting;

public class SettingManager : MonoBehaviour
{
	public enum QualityLevel
	{
		High,
		Low
	}

	[Serializable]
	public class SettingData
	{
		public QualityLevel qualityLevel;

		public bool isVibrationEnabled = true;

		public bool isSoundEnabled = true;

		public bool isMusicEnabled = true;

		public float soundVolume = 1f;

		public float musicVolume = 1f;
	}

	private static SettingManager instance;

	private GameObject settingUIObject;

	private SettingUI settingUI;

	public Action OnSettingChanged;

	public static SettingManager Instance
	{
		get
		{
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Expected O, but got Unknown
			if ((Object)(object)instance == (Object)null)
			{
				instance = Object.FindObjectOfType<SettingManager>();
				if ((Object)(object)instance == (Object)null)
				{
					GameObject val = new GameObject("SettingManager");
					instance = val.AddComponent<SettingManager>();
					Object.DontDestroyOnLoad((Object)val);
				}
			}
			return instance;
		}
	}

	public SettingData CurrentSetting { get; private set; }

	private void Awake()
	{
		if ((Object)(object)instance != (Object)null && (Object)(object)instance != (Object)(object)this)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
			return;
		}
		instance = this;
		Object.DontDestroyOnLoad((Object)(object)((Component)this).gameObject);
		LoadSetting();
	}

	public void Init()
	{
		LoadSetting();
	}

	public void OpenSettingUI()
	{
		SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
		if ((Object)(object)settingUIObject == (Object)null)
		{
			SingletonMono<ResManager>.Instance.LoadPrefabAsync("Assets/GameRes/UI/setting/prefabs/setting.prefab", delegate(GameObject gameObject)
			{
				if ((Object)(object)gameObject != (Object)null)
				{
					Transform layerTransform = UIManager.Instance.GetLayerTransform(UILayer.Popup);
					settingUIObject = Object.Instantiate<GameObject>(gameObject, layerTransform);
					settingUI = settingUIObject.GetComponent<SettingUI>();
					if ((Object)(object)settingUI != (Object)null)
					{
						settingUI.Init(this);
					}
				}
			});
		}
		else
		{
			settingUIObject.SetActive(true);
		}
	}

	public void CloseSettingUI()
	{
		if ((Object)(object)settingUIObject != (Object)null)
		{
			settingUIObject.SetActive(false);
			SaveSetting();
		}
	}

	public void SaveSetting()
	{
		PlayerPrefs.SetInt("QualityLevel", (int)CurrentSetting.qualityLevel);
		PlayerPrefs.SetInt("IsVibrationEnabled", CurrentSetting.isVibrationEnabled ? 1 : 0);
		PlayerPrefs.SetInt("IsSoundEnabled", CurrentSetting.isSoundEnabled ? 1 : 0);
		PlayerPrefs.SetInt("IsMusicEnabled", CurrentSetting.isMusicEnabled ? 1 : 0);
		PlayerPrefs.SetFloat("SoundVolume", CurrentSetting.soundVolume);
		PlayerPrefs.SetFloat("MusicVolume", CurrentSetting.musicVolume);
		PlayerPrefs.Save();
		OnSettingChanged?.Invoke();
		Debug.Log((object)"设置已保存");
	}

	public void LoadSetting()
	{
		CurrentSetting = new SettingData();
		CurrentSetting.qualityLevel = (QualityLevel)PlayerPrefs.GetInt("QualityLevel", 0);
		CurrentSetting.isVibrationEnabled = PlayerPrefs.GetInt("IsVibrationEnabled", 1) == 1;
		CurrentSetting.isSoundEnabled = PlayerPrefs.GetInt("IsSoundEnabled", 1) == 1;
		CurrentSetting.isMusicEnabled = PlayerPrefs.GetInt("IsMusicEnabled", 1) == 1;
		CurrentSetting.soundVolume = PlayerPrefs.GetFloat("SoundVolume", 1f);
		CurrentSetting.musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
		ApplySetting();
		Debug.Log((object)"设置已加载");
	}

	private void ApplySetting()
	{
		QualitySettings.SetQualityLevel((int)CurrentSetting.qualityLevel, true);
	}
}
