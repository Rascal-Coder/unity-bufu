using System.Collections;
using TMPro;
using UnityEngine;

namespace Module.UI.Component;

[RequireComponent(typeof(TextMeshProUGUI))]
[ExecuteInEditMode]
public class TextMeshProOutline : MonoBehaviour
{
	[Header("描边设置")]
	[Tooltip("是否启用描边")]
	[SerializeField]
	private bool enableOutline = true;

	[Tooltip("描边颜色")]
	[SerializeField]
	private Color outlineColor = Color.black;

	[Tooltip("描边宽度")]
	[Range(0f, 3f)]
	[SerializeField]
	private float outlineWidth = 0.2f;

	private TextMeshProUGUI textMeshPro;

	public bool EnableOutline
	{
		get
		{
			return enableOutline;
		}
		set
		{
			enableOutline = value;
			UpdateOutline();
		}
	}

	public Color OutlineColor
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return outlineColor;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			outlineColor = value;
			UpdateOutline();
		}
	}

	public float OutlineWidth
	{
		get
		{
			return outlineWidth;
		}
		set
		{
			outlineWidth = value;
			UpdateOutline();
		}
	}

	private void Awake()
	{
		Debug.Log((object)"TextMeshProOutline: Awake 开始执行");
		textMeshPro = ((Component)this).GetComponent<TextMeshProUGUI>();
		if ((Object)(object)textMeshPro == (Object)null)
		{
			Debug.LogError((object)"TextMeshProOutline: 未找到 TextMeshProUGUI 组件");
			return;
		}
		Debug.Log((object)("TextMeshProOutline: 找到 TextMeshProUGUI: " + ((Object)textMeshPro).name));
		((MonoBehaviour)this).StartCoroutine(DelayedInitialize());
	}

	private IEnumerator DelayedInitialize()
	{
		yield return null;
		UpdateOutline();
	}

	private void Start()
	{
		Debug.Log((object)"TextMeshProOutline: Start 开始执行");
		UpdateOutline();
	}

	private void UpdateOutline()
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)textMeshPro == (Object)null)
		{
			Debug.LogWarning((object)"TextMeshProOutline: textMeshPro 为 null，跳过更新");
		}
		else if (enableOutline)
		{
			((TMP_Text)textMeshPro).outlineColor = Color32.op_Implicit(outlineColor);
			((TMP_Text)textMeshPro).outlineWidth = outlineWidth;
			((TMP_Text)textMeshPro).UpdateMeshPadding();
		}
		else
		{
			((TMP_Text)textMeshPro).outlineColor = Color32.op_Implicit(Color.clear);
			((TMP_Text)textMeshPro).outlineWidth = 0f;
			((TMP_Text)textMeshPro).UpdateMeshPadding();
		}
	}

	private void OnValidate()
	{
		if (!((Object)(object)textMeshPro == (Object)null))
		{
			UpdateOutline();
		}
	}

	public void SetOutline(Color color, float width)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		outlineColor = color;
		outlineWidth = width;
		enableOutline = true;
		UpdateOutline();
	}

	public void EnableOutlineEffect(bool enable)
	{
		enableOutline = enable;
		UpdateOutline();
	}
}
