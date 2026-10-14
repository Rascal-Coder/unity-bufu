using UnityEngine;

namespace Module.Loading;

public class LoadingUITest : MonoBehaviour
{
	[SerializeField]
	private LoadingUI loadingUI;

	[SerializeField]
	private float loadingSpeed = 0.5f;

	private float _currentProgress;

	private bool _isLoading;

	private void Update()
	{
		if (_isLoading)
		{
			_currentProgress += Time.deltaTime * loadingSpeed;
			if ((Object)(object)loadingUI != (Object)null)
			{
				loadingUI.SetProgress(_currentProgress);
			}
			if (_currentProgress >= 1f)
			{
				_isLoading = false;
				Debug.Log((object)"加载完成！");
			}
		}
	}

	public void StartLoadingTest()
	{
		_currentProgress = 0f;
		_isLoading = true;
		if ((Object)(object)loadingUI != (Object)null)
		{
			loadingUI.ResetLoading();
		}
		Debug.Log((object)"开始加载测试...");
	}

	public void ResetLoadingTest()
	{
		_currentProgress = 0f;
		_isLoading = false;
		if ((Object)(object)loadingUI != (Object)null)
		{
			loadingUI.ResetLoading();
		}
		Debug.Log((object)"重置加载测试...");
	}

	public void ManualSetProgress(float progress)
	{
		_isLoading = false;
		_currentProgress = Mathf.Clamp01(progress);
		if ((Object)(object)loadingUI != (Object)null)
		{
			loadingUI.SetProgress(_currentProgress);
		}
		Debug.Log((object)$"手动设置进度：{progress}");
	}

	public void ManualSetProgressByPercentage(int percentage)
	{
		float progress = (float)Mathf.Clamp(percentage, 0, 100) / 100f;
		ManualSetProgress(progress);
	}
}
