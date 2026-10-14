using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace StoneHunter.Module.Setting;

public class SettingUI : MonoBehaviour
{
	[SerializeField]
	private Toggle highQualityToggle;

	[SerializeField]
	private Toggle lowQualityToggle;

	[SerializeField]
	private Toggle vibrationToggle;

	[SerializeField]
	private Toggle soundToggle;

	[SerializeField]
	private Slider soundSlider;

	[SerializeField]
	private Toggle musicToggle;

	[SerializeField]
	private Slider musicSlider;

	[SerializeField]
	private Button closeButton;

	[SerializeField]
	private Button backButton;

	[SerializeField]
	private TextMeshProUGUI gameVersionText;

	private SettingManager settingManager;

	private int backButtonClickCount;

	public void Init(SettingManager manager)
	{
		settingManager = manager;
		RegisterEventListeners();
		UpdateUIDisplay();
		((TMP_Text)gameVersionText).text = "游戏版本：" + SingletonMono<ResManager>.Instance.Version;
	}

	private void RegisterEventListeners()
	{
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Expected O, but got Unknown
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Expected O, but got Unknown
		((UnityEvent<bool>)(object)highQualityToggle.onValueChanged).AddListener((UnityAction<bool>)OnHighQualityToggleChanged);
		((UnityEvent<bool>)(object)lowQualityToggle.onValueChanged).AddListener((UnityAction<bool>)OnLowQualityToggleChanged);
		((UnityEvent<bool>)(object)vibrationToggle.onValueChanged).AddListener((UnityAction<bool>)OnVibrationToggleChanged);
		((UnityEvent<bool>)(object)soundToggle.onValueChanged).AddListener((UnityAction<bool>)OnSoundToggleChanged);
		((UnityEvent<float>)(object)soundSlider.onValueChanged).AddListener((UnityAction<float>)OnSoundSliderChanged);
		((UnityEvent<bool>)(object)musicToggle.onValueChanged).AddListener((UnityAction<bool>)OnMusicToggleChanged);
		((UnityEvent<float>)(object)musicSlider.onValueChanged).AddListener((UnityAction<float>)OnMusicSliderChanged);
		((UnityEvent)closeButton.onClick).AddListener(new UnityAction(OnCloseButtonClicked));
		((UnityEvent)backButton.onClick).AddListener(new UnityAction(OnBackButtonClicked));
	}

	private void OnHighQualityToggleChanged(bool isOn)
	{
		if (isOn)
		{
			lowQualityToggle.isOn = false;
			settingManager.CurrentSetting.qualityLevel = SettingManager.QualityLevel.High;
			ApplyQuality(settingManager.CurrentSetting.qualityLevel);
			settingManager.SaveSetting();
		}
	}

	private void OnLowQualityToggleChanged(bool isOn)
	{
		if (isOn)
		{
			highQualityToggle.isOn = false;
			settingManager.CurrentSetting.qualityLevel = SettingManager.QualityLevel.Low;
			ApplyQuality(settingManager.CurrentSetting.qualityLevel);
			settingManager.SaveSetting();
		}
	}

	private void ApplyQuality(SettingManager.QualityLevel level)
	{
		switch (level)
		{
		case SettingManager.QualityLevel.Low:
			SetResolutionScale(0.75f);
			QualitySettings.antiAliasing = 0;
			QualitySettings.shadows = (ShadowQuality)0;
			break;
		case SettingManager.QualityLevel.High:
			SetResolutionScale(1f);
			QualitySettings.antiAliasing = 2;
			QualitySettings.shadows = (ShadowQuality)1;
			break;
		}
	}

	private void SetResolutionScale(float scale)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		if ((int)Application.platform == 50)
		{
			Debug.Log((object)$"小游戏设置分辨率{scale}");
			Screen.SetResolution((int)((float)Screen.width * scale), (int)((float)Screen.height * scale), true);
		}
	}

	private void OnVibrationToggleChanged(bool isOn)
	{
		settingManager.CurrentSetting.isVibrationEnabled = isOn;
		settingManager.SaveSetting();
		SingletonMono<VibrationManager>.Instance.SetVibrationEnabled(isOn);
	}

	private void OnSoundToggleChanged(bool isOn)
	{
		settingManager.CurrentSetting.isSoundEnabled = isOn;
		((Selectable)soundSlider).interactable = isOn;
		if (isOn)
		{
			OnSoundSliderChanged(0.3f);
		}
		else
		{
			OnSoundSliderChanged(0f);
		}
		settingManager.SaveSetting();
	}

	private void OnSoundSliderChanged(float value)
	{
		settingManager.CurrentSetting.soundVolume = value;
		settingManager.SaveSetting();
		SoundManager.Instance.SetSoundVolume(value);
	}

	private void OnMusicToggleChanged(bool isOn)
	{
		settingManager.CurrentSetting.isMusicEnabled = isOn;
		((Selectable)musicSlider).interactable = isOn;
		if (isOn)
		{
			OnMusicSliderChanged(0.3f);
		}
		else
		{
			OnMusicSliderChanged(0f);
		}
		settingManager.SaveSetting();
	}

	private void OnMusicSliderChanged(float value)
	{
		settingManager.CurrentSetting.musicVolume = value;
		settingManager.SaveSetting();
		SoundManager.Instance.SetBGMVolume(value);
	}

	private void OnCloseButtonClicked()
	{
		SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
		backButtonClickCount = 0;
		settingManager.CloseSettingUI();
	}

	private void OnBackButtonClicked()
	{
		backButtonClickCount++;
		if (backButtonClickCount >= 2)
		{
			backButtonClickCount = 0;
			((Component)this).gameObject.SetActive(false);
			settingManager.CloseSettingUI();
		}
	}

	private void UpdateUIDisplay()
	{
		switch (settingManager.CurrentSetting.qualityLevel)
		{
		case SettingManager.QualityLevel.High:
			highQualityToggle.isOn = true;
			lowQualityToggle.isOn = false;
			break;
		case SettingManager.QualityLevel.Low:
			highQualityToggle.isOn = false;
			lowQualityToggle.isOn = true;
			break;
		}
		vibrationToggle.isOn = settingManager.CurrentSetting.isVibrationEnabled;
		soundToggle.isOn = settingManager.CurrentSetting.isSoundEnabled;
		soundSlider.value = settingManager.CurrentSetting.soundVolume;
		((Selectable)soundSlider).interactable = settingManager.CurrentSetting.isSoundEnabled;
		musicToggle.isOn = settingManager.CurrentSetting.isMusicEnabled;
		musicSlider.value = settingManager.CurrentSetting.musicVolume;
		((Selectable)musicSlider).interactable = settingManager.CurrentSetting.isMusicEnabled;
	}

	private void OnDestroy()
	{
		UnregisterEventListeners();
	}

	private void UnregisterEventListeners()
	{
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Expected O, but got Unknown
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Expected O, but got Unknown
		if ((Object)(object)highQualityToggle != (Object)null)
		{
			((UnityEvent<bool>)(object)highQualityToggle.onValueChanged).RemoveListener((UnityAction<bool>)OnHighQualityToggleChanged);
		}
		if ((Object)(object)lowQualityToggle != (Object)null)
		{
			((UnityEvent<bool>)(object)lowQualityToggle.onValueChanged).RemoveListener((UnityAction<bool>)OnLowQualityToggleChanged);
		}
		if ((Object)(object)vibrationToggle != (Object)null)
		{
			((UnityEvent<bool>)(object)vibrationToggle.onValueChanged).RemoveListener((UnityAction<bool>)OnVibrationToggleChanged);
		}
		if ((Object)(object)soundToggle != (Object)null)
		{
			((UnityEvent<bool>)(object)soundToggle.onValueChanged).RemoveListener((UnityAction<bool>)OnSoundToggleChanged);
		}
		if ((Object)(object)soundSlider != (Object)null)
		{
			((UnityEvent<float>)(object)soundSlider.onValueChanged).RemoveListener((UnityAction<float>)OnSoundSliderChanged);
		}
		if ((Object)(object)musicToggle != (Object)null)
		{
			((UnityEvent<bool>)(object)musicToggle.onValueChanged).RemoveListener((UnityAction<bool>)OnMusicToggleChanged);
		}
		if ((Object)(object)musicSlider != (Object)null)
		{
			((UnityEvent<float>)(object)musicSlider.onValueChanged).RemoveListener((UnityAction<float>)OnMusicSliderChanged);
		}
		if ((Object)(object)closeButton != (Object)null)
		{
			((UnityEvent)closeButton.onClick).RemoveListener(new UnityAction(OnCloseButtonClicked));
		}
		if ((Object)(object)backButton != (Object)null)
		{
			((UnityEvent)backButton.onClick).RemoveListener(new UnityAction(OnBackButtonClicked));
		}
	}
}
