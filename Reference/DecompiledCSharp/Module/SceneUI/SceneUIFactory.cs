using System;
using UnityEngine;

namespace Module.SceneUI;

public class SceneUIFactory : MonoBehaviour
{
	private static SceneUIFactory _ins;

	private Transform _uiRoot;

	public static SceneUIFactory Instance
	{
		get
		{
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Expected O, but got Unknown
			if ((Object)(object)_ins != (Object)null)
			{
				return _ins;
			}
			_ins = Object.FindObjectOfType<SceneUIFactory>();
			if ((Object)(object)_ins != (Object)null)
			{
				return _ins;
			}
			GameObject val = new GameObject("SceneUIFactory");
			_ins = val.AddComponent<SceneUIFactory>();
			Object.DontDestroyOnLoad((Object)val);
			return _ins;
		}
	}

	public Transform UIRoot
	{
		get
		{
			if ((Object)(object)_uiRoot == (Object)null)
			{
				_uiRoot = FindOrCreateUIRoot();
			}
			return _uiRoot;
		}
	}

	private Transform FindOrCreateUIRoot()
	{
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		Transform val = null;
		UIManager uIManager = Object.FindObjectOfType<UIManager>();
		if ((Object)(object)uIManager != (Object)null)
		{
			val = uIManager.GetLayerTransform(UILayer.Main);
		}
		if ((Object)(object)val == (Object)null)
		{
			GameObject val2 = GameObject.Find("Canvas");
			if ((Object)(object)val2 != (Object)null)
			{
				val = val2.transform.Find(UILayer.Main.ToString());
				if ((Object)(object)val == (Object)null)
				{
					val = val2.transform;
				}
			}
		}
		if ((Object)(object)val == (Object)null)
		{
			return ((Component)this).transform;
		}
		Transform val3 = val.Find("SceneUIObj");
		if ((Object)(object)val3 != (Object)null)
		{
			return val3;
		}
		RectTransform component = new GameObject("SceneUIObj", new Type[1] { typeof(RectTransform) }).GetComponent<RectTransform>();
		((Transform)component).SetParent(val, false);
		component.anchorMin = Vector2.zero;
		component.anchorMax = Vector2.one;
		component.offsetMin = Vector2.zero;
		component.offsetMax = Vector2.zero;
		component.anchoredPosition = Vector2.zero;
		return (Transform)(object)component;
	}

	private void SetupUITransform(Transform t, Vector3 anchoredPosition)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		RectTransform val = (RectTransform)(object)((t is RectTransform) ? t : null);
		if ((Object)(object)val != (Object)null)
		{
			val.anchoredPosition3D = anchoredPosition;
			((Transform)val).localScale = Vector3.one;
			((Transform)val).localRotation = Quaternion.identity;
		}
		else
		{
			t.localPosition = anchoredPosition;
			t.localScale = Vector3.one;
			t.localRotation = Quaternion.identity;
		}
	}
}
