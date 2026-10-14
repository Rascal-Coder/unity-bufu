using System;
using System.Collections;
using TTSDK;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Video;

public class IntroVideoPlayer : MonoBehaviour
{
	[Header("组件引用 (请直接拖入预制体中的子级物体)")]
	[SerializeField]
	private VideoPlayer videoPlayer;

	[SerializeField]
	private RawImage videoImage;

	[SerializeField]
	private GameObject videoPlayerObject;

	[SerializeField]
	private GameObject skipButtonObject;

	[SerializeField]
	private Button skipButton;

	private bool isVideoPlaying;

	private bool isVideoFinished;

	private Action onVideoFinished;

	private static readonly string VIDEO_PLAYED_KEY = "IntroVideoPlayed";

	private static readonly string PRELOAD_COMPLETE_KEY = "PreloadComplete";

	private GameObject videoContainer;

	private bool isOldversion;

	public bool IsVideoPlaying => isVideoPlaying;

	public bool IsVideoFinished => isVideoFinished;

	public static bool HasVideoPlayed => PlayerPrefs.GetInt(VIDEO_PLAYED_KEY, 0) == 1;

	public static bool IsPreloadComplete => PlayerPrefs.GetInt(PRELOAD_COMPLETE_KEY, 0) == 1;

	private void Awake()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		if ((Object)(object)skipButton != (Object)null)
		{
			((UnityEventBase)skipButton.onClick).RemoveAllListeners();
			((UnityEvent)skipButton.onClick).AddListener(new UnityAction(OnSkipButtonClick));
		}
		if ((Object)(object)videoImage != (Object)null)
		{
			((Component)videoImage).gameObject.SetActive(false);
		}
		if ((Object)(object)skipButtonObject != (Object)null)
		{
			skipButtonObject.SetActive(false);
		}
	}

	public void SetVideoContainer(GameObject container)
	{
		videoContainer = container;
	}

	private void EnsureContainer()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)videoContainer != (Object)null)
		{
			return;
		}
		if (((Object)((Component)this).gameObject).name == "IntroVideoContainer")
		{
			videoContainer = ((Component)this).gameObject;
			return;
		}
		Transform val = ((Component)this).transform.Find("IntroVideoContainer");
		if ((Object)(object)val == (Object)null)
		{
			videoContainer = new GameObject("IntroVideoContainer");
			videoContainer.transform.SetParent(((Component)this).transform, false);
			RectTransform obj = videoContainer.AddComponent<RectTransform>();
			obj.anchorMin = Vector2.zero;
			obj.anchorMax = Vector2.one;
			obj.sizeDelta = Vector2.zero;
			((Transform)obj).localPosition = Vector3.zero;
		}
		else
		{
			videoContainer = ((Component)val).gameObject;
		}
		videoContainer.SetActive(true);
	}

	public void PlayVideo(string videoUrl, Action onFinished = null)
	{
		if (HasVideoPlayed)
		{
			Debug.Log((object)"开场视频已播放过，跳过");
			onFinished?.Invoke();
			return;
		}
		if ((Object)(object)videoImage != (Object)null)
		{
			((Component)videoImage).gameObject.SetActive(false);
		}
		if ((Object)(object)skipButtonObject != (Object)null)
		{
			skipButtonObject.SetActive(false);
		}
		onVideoFinished = onFinished;
		((MonoBehaviour)this).StartCoroutine(PlayVideoCoroutine(videoUrl));
	}

	private IEnumerator PlayVideoCoroutine(string videoUrl)
	{
		TTSystemInfo systemInfo = TT.GetSystemInfo();
		string gamePublishVersion = TT.GamePublishVersion;
		Debug.Log((object)("抖音版本: " + TT.GameVersion + "，发布版本：" + TT.GamePublishVersion));
		Debug.Log((object)("抖音系统平台: " + systemInfo.platform));
		switch ("tt_" + systemInfo.platform)
		{
		case "ios":
		case "android":
		case "ohos":
			yield return ((MonoBehaviour)this).StartCoroutine(PlayVideoWeChatMiniGame(videoUrl));
			yield break;
		}
		if (IsVersionGreaterThan(gamePublishVersion, "0.1.2"))
		{
			yield return ((MonoBehaviour)this).StartCoroutine(PlayVideoUnity(videoUrl));
			yield break;
		}
		isOldversion = true;
		yield return ((MonoBehaviour)this).StartCoroutine(PlayVideoUnity2(videoUrl));
	}

	private IEnumerator PlayVideoUnity2(string videoUrl)
	{
		Debug.Log((object)"WebGL 端（包含抖音、微信PC）：基础包已裁剪 VideoPlayer，安全绕过 Native 接口，直接显示确认弹窗");
		FinishVideo();
		yield break;
	}

	private IEnumerator PlayVideoUnity(string videoUrl)
	{
		EnsureContainer();
		bool flag = true;
		try
		{
			SetupUnityVideo(videoUrl);
		}
		catch (Exception ex)
		{
			Debug.LogWarning((object)("原生 VideoPlayer 启动/初始化失败（可能是基础包裁剪），安全跳过播放: " + ex.Message));
			flag = false;
		}
		if (!flag)
		{
			Debug.Log((object)"不支持原生播放，直接跳过并展示目标弹窗");
			FinishVideo();
			yield break;
		}
		bool flag2 = false;
		try
		{
			videoPlayer.Prepare();
			flag2 = true;
		}
		catch (Exception ex2)
		{
			Debug.LogWarning((object)("调用 videoPlayer.Prepare() 失败（可能接口仍被裁剪），将降级到直接 Play 流程: " + ex2.Message));
		}
		if (flag2)
		{
			bool isPreparedError = false;
			while (true)
			{
				bool flag3 = false;
				try
				{
					flag3 = videoPlayer.isPrepared;
				}
				catch (Exception ex3)
				{
					Debug.LogWarning((object)("访问 isPrepared 发生异常，将直接播放: " + ex3.Message));
					isPreparedError = true;
				}
				if (isPreparedError || flag3)
				{
					break;
				}
				yield return null;
			}
		}
		try
		{
			if ((Object)(object)videoImage != (Object)null)
			{
				((Component)videoImage).gameObject.SetActive(true);
			}
			videoPlayer.Play();
		}
		catch (Exception ex4)
		{
			Debug.LogError((object)("播放视频失败: " + ex4.Message));
			FinishVideo();
			yield break;
		}
		yield return (object)new WaitForSeconds(2f);
		CreateSkipButton();
		while (isVideoPlaying)
		{
			bool flag4;
			try
			{
				flag4 = (Object)(object)videoPlayer != (Object)null && videoPlayer.isPlaying;
			}
			catch
			{
				break;
			}
			if (!flag4)
			{
				break;
			}
			yield return null;
		}
		FinishVideo();
	}

	private void SetupUnityVideo(string videoUrl)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Expected O, but got Unknown
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Expected O, but got Unknown
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Expected O, but got Unknown
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)videoPlayer == (Object)null || (Object)(object)videoPlayerObject == (Object)null)
		{
			Transform val = videoContainer.transform.Find("VideoPlayerObject");
			if ((Object)(object)val == (Object)null)
			{
				videoPlayerObject = new GameObject("VideoPlayerObject");
				videoPlayerObject.transform.SetParent(videoContainer.transform, false);
				videoPlayer = videoPlayerObject.AddComponent<VideoPlayer>();
			}
			else
			{
				videoPlayerObject = ((Component)val).gameObject;
				videoPlayer = videoPlayerObject.GetComponent<VideoPlayer>();
				if ((Object)(object)videoPlayer == (Object)null)
				{
					videoPlayer = videoPlayerObject.AddComponent<VideoPlayer>();
				}
			}
		}
		videoPlayerObject.SetActive(true);
		videoPlayer.playOnAwake = false;
		videoPlayer.isLooping = false;
		videoPlayer.renderMode = (VideoRenderMode)2;
		RenderTexture val2 = new RenderTexture(1440, 2880, 32);
		videoPlayer.targetTexture = val2;
		if ((Object)(object)videoImage == (Object)null)
		{
			Transform val3 = videoContainer.transform.Find("VideoImage");
			if ((Object)(object)val3 == (Object)null)
			{
				GameObject val4 = new GameObject("VideoImage");
				val4.transform.SetParent(videoContainer.transform, false);
				RectTransform obj = val4.AddComponent<RectTransform>();
				obj.anchorMin = Vector2.zero;
				obj.anchorMax = Vector2.one;
				obj.sizeDelta = Vector2.zero;
				videoImage = val4.AddComponent<RawImage>();
				((Graphic)videoImage).color = Color.white;
			}
			else
			{
				videoImage = ((Component)val3).GetComponent<RawImage>();
				if ((Object)(object)videoImage == (Object)null)
				{
					videoImage = ((Component)val3).gameObject.AddComponent<RawImage>();
				}
			}
		}
		videoImage.texture = (Texture)(object)val2;
		((Component)videoImage).gameObject.SetActive(false);
		videoPlayer.url = videoUrl;
		isVideoPlaying = true;
		isVideoFinished = false;
	}

	private IEnumerator PlayVideoWeChatMiniGame(string videoUrl)
	{
		Debug.Log((object)("微信小游戏视频播放: " + videoUrl));
		EnsureContainer();
		if ((Object)(object)videoPlayerObject == (Object)null)
		{
			Transform val = videoContainer.transform.Find("VideoPlayerObject");
			if ((Object)(object)val != (Object)null)
			{
				videoPlayerObject = ((Component)val).gameObject;
			}
		}
		if ((Object)(object)videoPlayerObject != (Object)null)
		{
			videoPlayerObject.SetActive(false);
		}
		if ((Object)(object)videoImage == (Object)null)
		{
			Transform val2 = videoContainer.transform.Find("VideoImage");
			if ((Object)(object)val2 != (Object)null)
			{
				videoImage = ((Component)val2).GetComponent<RawImage>();
			}
		}
		if ((Object)(object)videoImage != (Object)null)
		{
			((Component)videoImage).gameObject.SetActive(false);
		}
		isVideoPlaying = true;
		isVideoFinished = false;
		CreateSkipButton();
		while (isVideoPlaying && !isVideoFinished)
		{
			yield return null;
		}
		if (!isVideoFinished)
		{
			FinishVideo();
		}
	}

	private void FinishVideo()
	{
		if (!isVideoFinished)
		{
			isVideoFinished = true;
			isVideoPlaying = false;
			if (isOldversion)
			{
				onVideoFinished?.Invoke();
				return;
			}
			Debug.Log((object)"视频播放结束/被跳过，清理播放器组件");
			CleanUpVideo();
			onVideoFinished?.Invoke();
		}
	}

	public void StopVideo()
	{
		try
		{
			if ((Object)(object)videoPlayer != (Object)null)
			{
				videoPlayer.Stop();
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning((object)("StopVideo 异常（可能是未包含VideoModule）: " + ex.Message));
		}
		FinishVideo();
	}

	private void OnSkipButtonClick()
	{
		Debug.Log((object)"点击跳过按钮");
		StopVideo();
	}

	private void CreateSkipButton()
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Expected O, but got Unknown
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Expected O, but got Unknown
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Expected O, but got Unknown
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Expected O, but got Unknown
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		//IL_026e: Unknown result type (might be due to invalid IL or missing references)
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		EnsureContainer();
		if ((Object)(object)skipButton != (Object)null && (Object)(object)skipButtonObject != (Object)null)
		{
			skipButtonObject.SetActive(true);
			((UnityEventBase)skipButton.onClick).RemoveAllListeners();
			((UnityEvent)skipButton.onClick).AddListener(new UnityAction(OnSkipButtonClick));
		}
		else
		{
			if ((Object)(object)skipButtonObject != (Object)null)
			{
				return;
			}
			Transform val = videoContainer.transform.Find("SkipButton");
			if ((Object)(object)val != (Object)null)
			{
				skipButtonObject = ((Component)val).gameObject;
				skipButton = skipButtonObject.GetComponent<Button>();
				if ((Object)(object)skipButton == (Object)null)
				{
					skipButton = skipButtonObject.AddComponent<Button>();
				}
				((UnityEventBase)skipButton.onClick).RemoveAllListeners();
				((UnityEvent)skipButton.onClick).AddListener(new UnityAction(OnSkipButtonClick));
				Debug.Log((object)"从视频预制体中找到了已配置好的跳过按钮");
				return;
			}
			skipButtonObject = new GameObject("SkipButton");
			skipButtonObject.transform.SetParent(videoContainer.transform, false);
			RectTransform obj = skipButtonObject.AddComponent<RectTransform>();
			obj.anchorMin = new Vector2(1f, 1f);
			obj.anchorMax = new Vector2(1f, 1f);
			obj.pivot = new Vector2(1f, 1f);
			obj.anchoredPosition = new Vector2(-50f, -50f);
			obj.sizeDelta = new Vector2(180f, 70f);
			Image val2 = skipButtonObject.AddComponent<Image>();
			Sprite val3 = Resources.Load<Sprite>("ty_zi_tiaoguo01");
			if ((Object)(object)val3 == (Object)null)
			{
				Debug.LogWarning((object)"未找到跳过按钮图片: Resources/ty_zi_tiaoguo01");
				((Graphic)val2).color = new Color(0.12f, 0.13f, 0.18f, 0.85f);
			}
			else
			{
				val2.sprite = val3;
				((Graphic)val2).SetNativeSize();
			}
			skipButton = skipButtonObject.AddComponent<Button>();
			((UnityEvent)skipButton.onClick).AddListener(new UnityAction(OnSkipButtonClick));
			ColorBlock colors = ((Selectable)skipButton).colors;
			((ColorBlock)(ref colors)).normalColor = Color.white;
			((ColorBlock)(ref colors)).highlightedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
			((ColorBlock)(ref colors)).pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
			((Selectable)skipButton).colors = colors;
			Debug.Log((object)"运行时动态创建了降级跳过按钮（纯图片，无文字）");
		}
	}

	public void CleanUpVideo()
	{
		try
		{
			if ((Object)(object)videoPlayer != (Object)null)
			{
				if ((Object)(object)videoPlayer.targetTexture != (Object)null)
				{
					videoPlayer.targetTexture.Release();
					Object.Destroy((Object)(object)videoPlayer.targetTexture);
				}
				Object.Destroy((Object)(object)videoPlayer);
				videoPlayer = null;
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning((object)("CleanUpVideo 异常（可能是未包含VideoModule）: " + ex.Message));
		}
		if ((Object)(object)videoContainer != (Object)null)
		{
			Object.Destroy((Object)(object)videoContainer);
			videoContainer = null;
		}
		videoPlayerObject = null;
		videoImage = null;
		skipButtonObject = null;
	}

	public void ShowConfirmUI(string title, string content, string buttonText, Action onConfirm)
	{
		((MonoBehaviour)this).StartCoroutine(ShowConfirmDialogCoroutine(content, onConfirm));
	}

	private IEnumerator ShowConfirmDialogCoroutine(string content, Action onConfirm)
	{
		while ((Object)(object)UIManager.Instance == (Object)null)
		{
			yield return null;
		}
		UIManager.Instance.ShowGameTargetDialog("", delegate
		{
			onConfirm?.Invoke();
		}, UIManager.Instance.GetLayerTransform(UILayer.System));
	}

	private void OnDestroy()
	{
		CleanUpVideo();
	}

	private bool IsVersionGreaterThan(string versionStr, string targetVersion)
	{
		if (string.IsNullOrEmpty(versionStr))
		{
			Debug.LogWarning((object)"版本号为空，返回 false");
			return false;
		}
		string[] array = versionStr.Split('.', StringSplitOptions.None);
		string[] array2 = targetVersion.Split('.', StringSplitOptions.None);
		int[] array3 = new int[3];
		int[] array4 = new int[3];
		for (int i = 0; i < 3; i++)
		{
			if (i >= array.Length || !int.TryParse(array[i], out array3[i]))
			{
				array3[i] = 0;
			}
			if (i >= array2.Length || !int.TryParse(array2[i], out array4[i]))
			{
				array4[i] = 0;
			}
		}
		Debug.Log((object)$"版本号 {versionStr} 解析为 ({array3[0]}, {array3[1]}, {array3[2]})，目标版本 {targetVersion} 为 ({array4[0]}, {array4[1]}, {array4[2]})");
		for (int j = 0; j < 3; j++)
		{
			if (array3[j] > array4[j])
			{
				Debug.Log((object)$"第 {j + 1} 位 {array3[j]} > {array4[j]}，返回 true");
				return true;
			}
			if (array3[j] < array4[j])
			{
				Debug.Log((object)$"第 {j + 1} 位 {array3[j]} < {array4[j]}，返回 false");
				return false;
			}
		}
		Debug.Log((object)("版本号 " + versionStr + " == " + targetVersion + "，返回 false"));
		return false;
	}
}
