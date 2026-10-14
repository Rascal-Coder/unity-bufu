using System.Collections;
using UnityEngine;

public class VibrationController : SingletonMono<VibrationController>
{
	private bool isWechatGame;

	private bool isDouyinGame;

	private bool isKuaishouGame;

	private bool isAndroidApp;

	private bool isIOSApp;

	private const int LIGHT_VIBRATION_DURATION = 10;

	private const int HEAVY_VIBRATION_DURATION = 30;

	private const int ANDROID_HEAVY_VIBRATION_PATTERN = 2;

	public override bool DontDestory => true;

	protected override void OnAwake()
	{
		DetectPlatform();
	}

	protected override void BeforOnDestroy()
	{
	}

	private void DetectPlatform()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Invalid comparison between Unknown and I4
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Invalid comparison between Unknown and I4
		if (Application.isEditor)
		{
			Debug.Log((object)"[VibrationController] 运行在Unity编辑器中，震动功能将被禁用");
			return;
		}
		RuntimePlatform platform = Application.platform;
		if ((int)platform == 11)
		{
			if (IsWechatGame())
			{
				isWechatGame = true;
				Debug.Log((object)"[VibrationController] 检测到微信小游戏平台");
			}
			else if (IsDouyinGame())
			{
				isDouyinGame = true;
				Debug.Log((object)"[VibrationController] 检测到抖音小游戏平台");
			}
			else if (IsKuaishouGame())
			{
				isKuaishouGame = true;
				Debug.Log((object)"[VibrationController] 检测到快手小游戏平台");
			}
			else
			{
				isAndroidApp = true;
				Debug.Log((object)"[VibrationController] 检测到Android应用平台");
			}
		}
		else if ((int)platform == 8)
		{
			isIOSApp = true;
			Debug.Log((object)"[VibrationController] 检测到iOS应用平台");
		}
	}

	private bool IsWechatGame()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		if ((int)Application.platform == 17)
		{
			if (!SystemInfo.deviceModel.Contains("MicroMessenger"))
			{
				return SystemInfo.deviceName.Contains("WeChat");
			}
			return true;
		}
		return false;
	}

	private bool IsDouyinGame()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		if ((int)Application.platform == 17)
		{
			if (!SystemInfo.deviceModel.Contains("Douyin"))
			{
				return SystemInfo.deviceName.Contains("TikTok");
			}
			return true;
		}
		return false;
	}

	private bool IsKuaishouGame()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		if ((int)Application.platform == 17)
		{
			if (!SystemInfo.deviceModel.Contains("Kuaishou"))
			{
				return SystemInfo.deviceName.Contains("Kwai");
			}
			return true;
		}
		return false;
	}

	public void VibrateLight()
	{
		if (Application.isEditor)
		{
			Debug.Log((object)"[VibrationController] 轻微震动（编辑器模式）");
			return;
		}
		Debug.Log((object)"[VibrationController] 执行轻微震动");
		if (isWechatGame)
		{
			VibrateWechatGame(10);
		}
		else if (isDouyinGame)
		{
			VibrateDouyinGame(10);
		}
		else if (isKuaishouGame)
		{
			VibrateKuaishouGame(10);
		}
		else if (isAndroidApp)
		{
			VibrateAndroid(10);
		}
		else if (isIOSApp)
		{
			VibrateIOSLight();
		}
		else
		{
			VibrateGeneric(10);
		}
	}

	public void VibrateHeavy()
	{
		if (Application.isEditor)
		{
			Debug.Log((object)"[VibrationController] 强烈震动（编辑器模式）");
			return;
		}
		Debug.Log((object)"[VibrationController] 执行强烈震动");
		if (isWechatGame)
		{
			VibrateWechatGame(30);
		}
		else if (isDouyinGame)
		{
			VibrateDouyinGame(30);
		}
		else if (isKuaishouGame)
		{
			VibrateKuaishouGame(30);
		}
		else if (isAndroidApp)
		{
			VibrateAndroidPattern(2);
		}
		else if (isIOSApp)
		{
			VibrateIOSHeavy();
		}
		else
		{
			VibrateGeneric(30);
		}
	}

	private void VibrateWechatGame(int duration)
	{
		Application.ExternalCall("WeChatVibrateShort", new object[1] { duration });
	}

	private void VibrateDouyinGame(int duration)
	{
		Application.ExternalCall("DouyinVibrate", new object[1] { duration });
	}

	private void VibrateKuaishouGame(int duration)
	{
		Application.ExternalCall("KuaishouVibrate", new object[1] { duration });
	}

	private void VibrateAndroid(int duration)
	{
	}

	private void VibrateAndroidPattern(int pattern)
	{
	}

	private void VibrateIOSLight()
	{
	}

	private void VibrateIOSHeavy()
	{
	}

	private void VibrateGeneric(int duration)
	{
	}

	public void VibratePattern(int[] pattern)
	{
		((MonoBehaviour)this).StartCoroutine(PlayVibrationPattern(pattern));
	}

	private IEnumerator PlayVibrationPattern(int[] pattern)
	{
		if (pattern == null || pattern.Length == 0)
		{
			yield break;
		}
		for (int i = 0; i < pattern.Length; i++)
		{
			if (i % 2 == 0)
			{
				VibrateLight();
			}
			yield return (object)new WaitForSeconds((float)pattern[i] / 1000f);
		}
		yield return null;
	}
}
