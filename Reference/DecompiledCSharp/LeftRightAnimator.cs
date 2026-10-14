using UnityEngine;

public class LeftRightAnimator : MonoBehaviour
{
	[SerializeField]
	private float moveWidth = 10f;

	[SerializeField]
	private float moveTime = 0.5f;

	private float timer;

	private float startPosX;

	private RectTransform rectTransform;

	private void Awake()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		rectTransform = ((Component)this).GetComponent<RectTransform>();
		if ((Object)(object)rectTransform != (Object)null)
		{
			startPosX = rectTransform.anchoredPosition.x;
		}
	}

	private void Update()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)rectTransform == (Object)null))
		{
			timer += Time.deltaTime;
			float num = timer % (moveTime * 2f);
			Vector2 anchoredPosition = rectTransform.anchoredPosition;
			if (num < moveTime)
			{
				anchoredPosition.x = Mathf.Lerp(startPosX, startPosX + moveWidth, num / moveTime);
			}
			else
			{
				anchoredPosition.x = Mathf.Lerp(startPosX + moveWidth, startPosX, (num - moveTime) / moveTime);
			}
			rectTransform.anchoredPosition = anchoredPosition;
		}
	}
}
