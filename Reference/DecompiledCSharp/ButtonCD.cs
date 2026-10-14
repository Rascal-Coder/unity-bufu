using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ButtonCD : MonoBehaviour
{
	[SerializeField]
	private float cdTime = 0.5f;

	private Button _btn;

	private bool _canClick = true;

	private void Awake()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Expected O, but got Unknown
		_btn = ((Component)this).GetComponent<Button>();
		if ((Object)(object)_btn == (Object)null)
		{
			Debug.LogError((object)"ButtonCD 组件没有绑定 Button 组件");
			Object.Destroy((Object)(object)this);
		}
		else
		{
			((UnityEvent)_btn.onClick).AddListener(new UnityAction(OnBtnClick));
		}
	}

	private void OnBtnClick()
	{
		if (_canClick)
		{
			Debug.Log((object)"按钮被点击了");
			((MonoBehaviour)this).StartCoroutine(ClickCD());
		}
	}

	private IEnumerator ClickCD()
	{
		_canClick = false;
		yield return (object)new WaitForSecondsRealtime(cdTime);
		_canClick = true;
	}
}
