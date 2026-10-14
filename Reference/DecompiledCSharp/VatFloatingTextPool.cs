using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class VatFloatingTextPool
{
	private sealed class TextItem
	{
		public bool Active;

		public bool Critical;

		public TextMeshProUGUI Text;

		public RectTransform Rect;

		public Vector3 StartPos;

		public float OffsetX;

		public float Elapsed;
	}

	private const float FloatHeight = 120f;

	private const float FloatDuration = 1f;

	private const float FontSize = 48f;

	private const float ScaleCompensation = 5f;

	private const float FadeStartRatio = 0.5f;

	private const float FloatingTextAniScale = 2.8f;

	private readonly List<TextItem> items = new List<TextItem>();

	private MainSceneVat scene;

	private RectTransform parent;

	private GameObject normalPrefab;

	private GameObject criticalPrefab;

	public void Initialize(MainSceneVat owner, RectTransform textParent, GameObject normalTextPrefab, GameObject criticalTextPrefab)
	{
		scene = owner;
		parent = textParent;
		normalPrefab = normalTextPrefab;
		criticalPrefab = criticalTextPrefab;
	}

	public bool TryShow(Vector3 worldPosition, double value)
	{
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)scene == (Object)null)
		{
			return false;
		}
		if (!scene.IsAddText())
		{
			return true;
		}
		bool flag = (Object)(object)SingletonMono<BuffManager>.Instance != (Object)null && SingletonMono<BuffManager>.Instance.GetIsCritical();
		GameObject val = ((flag && (Object)(object)criticalPrefab != (Object)null) ? criticalPrefab : normalPrefab);
		if ((Object)(object)val == (Object)null || (Object)(object)parent == (Object)null)
		{
			return false;
		}
		bool flag2 = flag && (Object)(object)criticalPrefab != (Object)null;
		TextItem item = GetItem(val, flag2);
		if (item == null || (Object)(object)item.Text == (Object)null || (Object)(object)item.Rect == (Object)null)
		{
			return false;
		}
		item.Active = true;
		item.Elapsed = 0f;
		item.StartPos = scene.WorldToTextLocalPosition(worldPosition);
		item.OffsetX = Random.Range(-30f, 30f);
		item.Rect.anchoredPosition = Vector2.op_Implicit(item.StartPos);
		((Transform)item.Rect).localRotation = Quaternion.identity;
		((Graphic)item.Text).color = Color.white;
		((TMP_Text)item.Text).alignment = (TextAlignmentOptions)514;
		((TMP_Text)item.Text).fontSize = 48f * GetTextScale();
		((TMP_Text)item.Text).text = NumberFormatter.ConvertToSpriteText(flag2 ? ("暴" + NumberFormatter.FormatNumber(value)) : ("+" + NumberFormatter.FormatNumber(value)));
		((TMP_Text)item.Text).transform.localScale = Vector3.one * 2.8f;
		((Component)item.Text).gameObject.SetActive(true);
		scene.AddTextCount();
		return true;
	}

	public void Tick(float deltaTime)
	{
		for (int num = items.Count - 1; num >= 0; num--)
		{
			TextItem textItem = items[num];
			if (textItem != null && textItem.Active)
			{
				if ((Object)(object)textItem.Text == (Object)null || (Object)(object)textItem.Rect == (Object)null)
				{
					textItem.Active = false;
				}
				else
				{
					TickItem(textItem, deltaTime);
				}
			}
		}
	}

	public void Clear()
	{
		for (int i = 0; i < items.Count; i++)
		{
			if (items[i] != null && (Object)(object)items[i].Text != (Object)null)
			{
				Object.Destroy((Object)(object)((Component)items[i].Text).gameObject);
			}
		}
		items.Clear();
	}

	private TextItem GetItem(GameObject prefab, bool critical)
	{
		items.RemoveAll((TextItem item) => item == null || (Object)(object)item.Text == (Object)null || (Object)(object)item.Rect == (Object)null);
		for (int num = 0; num < items.Count; num++)
		{
			TextItem textItem = items[num];
			if (textItem != null && !textItem.Active && textItem.Critical == critical)
			{
				return textItem;
			}
		}
		GameObject val = Object.Instantiate<GameObject>(prefab, (Transform)(object)parent);
		TextMeshProUGUI component = val.GetComponent<TextMeshProUGUI>();
		if ((Object)(object)component == (Object)null)
		{
			Object.Destroy((Object)(object)val);
			return null;
		}
		val.SetActive(false);
		TextItem textItem2 = new TextItem
		{
			Critical = critical,
			Text = component,
			Rect = ((Component)component).GetComponent<RectTransform>()
		};
		items.Add(textItem2);
		return textItem2;
	}

	private void TickItem(TextItem item, float deltaTime)
	{
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		item.Elapsed += deltaTime * 2f;
		float num = Mathf.Clamp01(item.Elapsed / 1f);
		float num2 = 1f - (1f - num) * (1f - num);
		((Transform)item.Rect).localPosition = new Vector3(item.StartPos.x + item.OffsetX * (2f - num), item.StartPos.y + 120f * num2, 0f);
		if (num > 0.5f)
		{
			Color white = Color.white;
			white.a = 1f - Mathf.InverseLerp(0.5f, 1f, num);
			((Graphic)item.Text).color = white;
		}
		else
		{
			((Graphic)item.Text).color = Color.white;
		}
		if (item.Elapsed >= 1f)
		{
			Finish(item);
		}
	}

	private void Finish(TextItem item)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		item.Active = false;
		if ((Object)(object)item.Text != (Object)null)
		{
			((Component)item.Text).gameObject.SetActive(false);
			((Graphic)item.Text).color = Color.white;
			((TMP_Text)item.Text).transform.localScale = Vector3.one * 5f;
		}
		if ((Object)(object)item.Rect != (Object)null)
		{
			((Transform)item.Rect).localPosition = item.StartPos;
		}
		if ((Object)(object)scene != (Object)null)
		{
			scene.ReduceTextCount();
		}
	}

	private float GetTextScale()
	{
		int num = (((Object)(object)scene != (Object)null) ? scene.GetMaxTextCount() : 0);
		if (num <= 0)
		{
			return 1f;
		}
		return Mathf.Clamp(1f - 0.4f * ((float)scene.GetCurrentTextCount() / (float)num), 0.6f, 1f);
	}
}
