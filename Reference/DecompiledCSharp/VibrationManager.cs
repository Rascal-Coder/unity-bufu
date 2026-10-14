using System;
using System.Runtime.InteropServices;
using TTSDK;
using UnityEngine;

public class VibrationManager : SingletonMono<VibrationManager>
{
	public enum VibrationLevel
	{
		Light,
		Medium,
		Heavy,
		Custom
	}

	private const string VIBRATION_ENABLED_KEY = "VibrationEnabled";

	private bool _isVibrationEnabled = true;

	private readonly int[] _vibrationDurations = new int[3] { 100, 200, 300 };

	public override bool DontDestory => true;

	public void SetVibrationEnabled(bool enabled)
	{
		_isVibrationEnabled = enabled;
		PlayerPrefs.SetInt("VibrationEnabled", enabled ? 1 : 0);
		PlayerPrefs.Save();
	}

	public bool IsVibrationEnabled()
	{
		return _isVibrationEnabled;
	}

	public void Vibrate(VibrationLevel level = VibrationLevel.Medium, int duration = 200)
	{
		if (_isVibrationEnabled)
		{
			VibrateDouyin(level, duration);
		}
	}

	private void VibrateWeChat(VibrationLevel level, int duration)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			_ = Application.platform;
			_ = 50;
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("微信震动失败: " + ex.Message));
		}
	}

	private void VibrateDouyin(VibrationLevel level, int duration)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Expected O, but got Unknown
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		try
		{
			if ((int)Application.platform == 50)
			{
				if (level == VibrationLevel.Light || level == VibrationLevel.Medium)
				{
					TT.VibrateShort(new VibrateShortParam());
				}
				else
				{
					TT.VibrateLong(new VibrateLongParam());
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("抖音震动失败: " + ex.Message));
		}
	}

	[DllImport("__Internal")]
	private static extern void CallWeChatVibrateShort();

	[DllImport("__Internal")]
	private static extern void CallWeChatVibrateLong();

	private void VibrateIOS(VibrationLevel level, int duration)
	{
	}

	private void VibrateAndroid(VibrationLevel level, int duration)
	{
	}

	private void VibrateUnity(VibrationLevel level, int duration)
	{
	}

	private int GetVibrationDuration(VibrationLevel level, int customDuration)
	{
		if (level == VibrationLevel.Custom)
		{
			return Mathf.Clamp(customDuration, 50, 1000);
		}
		if (level >= VibrationLevel.Light && (int)level < _vibrationDurations.Length)
		{
			return _vibrationDurations[(int)level];
		}
		return _vibrationDurations[1];
	}

	public void VibrateLight()
	{
		Vibrate(VibrationLevel.Light);
	}

	public void VibrateMedium()
	{
		Vibrate();
	}

	public void VibrateHeavy()
	{
		Vibrate(VibrationLevel.Heavy);
	}

	public void VibrateCustom(int duration)
	{
		Vibrate(VibrationLevel.Custom, duration);
	}

	protected override void OnAwake()
	{
		LoadVibrationSettings();
	}

	private void LoadVibrationSettings()
	{
		if (PlayerPrefs.HasKey("VibrationEnabled"))
		{
			_isVibrationEnabled = PlayerPrefs.GetInt("VibrationEnabled", 0) == 1;
		}
	}

	protected override void BeforOnDestroy()
	{
	}
}
