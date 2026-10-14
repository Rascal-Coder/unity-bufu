using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace UI.Component;

public class CountdownTimer : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI timeText;

	private float _remainingSeconds;

	private Coroutine _countdownCoroutine;

	private long _endTimeMs;

	public event Action OnFinished;

	private void Awake()
	{
		((Component)this).gameObject.SetActive(false);
	}

	public void StartCountdown(float seconds)
	{
		if (_countdownCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(_countdownCoroutine);
			_countdownCoroutine = null;
		}
		_remainingSeconds = seconds;
		_endTimeMs = (long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds + (long)(seconds * 1000f);
		((Component)this).gameObject.SetActive(true);
		UpdateTimeText();
		if (_countdownCoroutine == null && ((Component)this).gameObject.activeInHierarchy)
		{
			_countdownCoroutine = ((MonoBehaviour)this).StartCoroutine(CountdownCoroutine());
		}
	}

	private void OnEnable()
	{
		if (_endTimeMs <= 0)
		{
			return;
		}
		long num = (long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;
		float num2 = (float)(_endTimeMs - num) / 1000f;
		if (num2 > 0f)
		{
			_remainingSeconds = num2;
			UpdateTimeText();
			if (_countdownCoroutine == null)
			{
				_countdownCoroutine = ((MonoBehaviour)this).StartCoroutine(CountdownCoroutine());
			}
		}
		else
		{
			_remainingSeconds = 0f;
			UpdateTimeText();
			_endTimeMs = 0L;
			((Component)this).gameObject.SetActive(false);
			this.OnFinished?.Invoke();
		}
	}

	private void OnDisable()
	{
		if (_countdownCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(_countdownCoroutine);
			_countdownCoroutine = null;
		}
	}

	private IEnumerator CountdownCoroutine()
	{
		while (_remainingSeconds > 0f)
		{
			yield return (object)new WaitForSecondsRealtime(1f);
			_remainingSeconds -= 1f;
			UpdateTimeText();
		}
		((Component)this).gameObject.SetActive(false);
		_countdownCoroutine = null;
		_endTimeMs = 0L;
		this.OnFinished?.Invoke();
	}

	private void UpdateTimeText()
	{
		if ((Object)(object)timeText != (Object)null)
		{
			((TMP_Text)timeText).text = FormatTime(Mathf.RoundToInt(_remainingSeconds));
		}
	}

	private string FormatTime(int totalSeconds)
	{
		int num = totalSeconds / 3600;
		int num2 = totalSeconds % 3600 / 60;
		int num3 = totalSeconds % 60;
		if (num > 0)
		{
			return $"{num:D2}:{num2:D2}:{num3:D2}";
		}
		return $"{num2:D2}:{num3:D2}";
	}

	public void Stop()
	{
		if (_countdownCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(_countdownCoroutine);
			_countdownCoroutine = null;
		}
		_endTimeMs = 0L;
		((Component)this).gameObject.SetActive(false);
	}

	private void OnDestroy()
	{
		Stop();
	}
}
