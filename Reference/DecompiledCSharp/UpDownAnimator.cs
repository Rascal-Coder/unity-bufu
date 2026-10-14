using UnityEngine;

public class UpDownAnimator : MonoBehaviour
{
	[SerializeField]
	private float moveHeight = 0.1f;

	[SerializeField]
	private float moveTime = 0.5f;

	[SerializeField]
	private bool isUI;

	private float timer;

	private float baseY;

	private RectTransform rectTransform;

	private void Awake()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (isUI)
		{
			rectTransform = ((Component)this).GetComponent<RectTransform>();
			if ((Object)(object)rectTransform != (Object)null)
			{
				baseY = rectTransform.anchoredPosition.y;
			}
		}
		else
		{
			baseY = ((Component)this).transform.localPosition.y;
		}
	}

	private void Update()
	{
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		timer += Time.deltaTime;
		float num = timer % (moveTime * 2f);
		float y = ((!(num < moveTime)) ? Mathf.Lerp(baseY + moveHeight, baseY, (num - moveTime) / moveTime) : Mathf.Lerp(baseY, baseY + moveHeight, num / moveTime));
		if (isUI && (Object)(object)rectTransform != (Object)null)
		{
			Vector2 anchoredPosition = rectTransform.anchoredPosition;
			anchoredPosition.y = y;
			rectTransform.anchoredPosition = anchoredPosition;
		}
		else
		{
			Vector3 localPosition = ((Component)this).transform.localPosition;
			localPosition.y = y;
			((Component)this).transform.localPosition = localPosition;
		}
	}
}
