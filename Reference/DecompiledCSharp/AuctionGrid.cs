using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class AuctionGrid : MonoBehaviour
{
	[Header("关卡存放父物体")]
	[SerializeField]
	private RectTransform cont;

	[Header("cont的layout组件")]
	[SerializeField]
	private HorizontalLayoutGroup layout;

	[Header("滑动条viewport")]
	[SerializeField]
	private RectTransform viewport;

	[Header("每个关卡的半径大小")]
	[SerializeField]
	private float redius;

	[Header("左点击按键")]
	[SerializeField]
	private Button leftBtn;

	[Header("右点击按键")]
	[SerializeField]
	private Button rightBtn;

	private float sourceposX;

	private float distance;

	private bool isMove;

	private float moveTargetX;

	private float moveSpeed;

	private float downpos;

	private int selectCount;

	private int firstOpen;

	private bool istouch;

	private void Start()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		if ((Object)(object)leftBtn != (Object)null)
		{
			((UnityEvent)leftBtn.onClick).AddListener((UnityAction)delegate
			{
				//IL_0033: Unknown result type (might be due to invalid IL or missing references)
				//IL_0097: Unknown result type (might be due to invalid IL or missing references)
				//IL_009c: Unknown result type (might be due to invalid IL or missing references)
				//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
				//IL_00af: Unknown result type (might be due to invalid IL or missing references)
				SoundManager.Instance?.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
				if (!isMove)
				{
					isMove = true;
					float x = cont.anchoredPosition.x;
					int moveIndex = GetMoveIndex(x, (x <= downpos) ? true : false);
					moveIndex--;
					moveIndex = Mathf.Clamp(moveIndex, 0, selectCount - 1);
					moveTargetX = sourceposX - (float)moveIndex * (redius * 2f + distance);
					float num = moveTargetX;
					Rect rect = viewport.rect;
					float width = ((Rect)(ref rect)).width;
					rect = cont.rect;
					moveTargetX = Mathf.Clamp(num, width - ((Rect)(ref rect)).width + sourceposX, sourceposX);
					AuctionDataManager.Instance?.SetCurrentSelect(moveIndex);
					AuctionDataManager.Instance?.auctionSelect?.RefreshPanel();
				}
			});
		}
		if (!((Object)(object)rightBtn != (Object)null))
		{
			return;
		}
		((UnityEvent)rightBtn.onClick).AddListener((UnityAction)delegate
		{
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			SoundManager.Instance?.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
			if (!isMove)
			{
				isMove = true;
				float x = cont.anchoredPosition.x;
				int moveIndex = GetMoveIndex(x, (x <= downpos) ? true : false);
				moveIndex++;
				moveIndex = Mathf.Clamp(moveIndex, 0, selectCount - 1);
				moveTargetX = sourceposX - (float)moveIndex * (redius * 2f + distance);
				float num = moveTargetX;
				Rect rect = viewport.rect;
				float width = ((Rect)(ref rect)).width;
				rect = cont.rect;
				moveTargetX = Mathf.Clamp(num, width - ((Rect)(ref rect)).width + sourceposX, sourceposX);
				AuctionDataManager.Instance?.SetCurrentSelect(moveIndex);
				AuctionDataManager.Instance?.auctionSelect?.RefreshPanel();
			}
		});
	}

	public void Init()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		firstOpen = 0;
		sourceposX = cont.anchoredPosition.x;
		distance = ((HorizontalOrVerticalLayoutGroup)layout).spacing;
		isMove = false;
		moveTargetX = 0f;
		moveSpeed = 2500f;
		if (AuctionDataManager.Instance != null)
		{
			selectCount = AuctionDataManager.Instance.GetSelectCount();
		}
	}

	private void LateUpdate()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		if (isMove)
		{
			if (Mathf.Abs(cont.anchoredPosition.x - moveTargetX) > 0.1f)
			{
				float num = Mathf.MoveTowards(cont.anchoredPosition.x, moveTargetX, moveSpeed * Time.deltaTime);
				cont.anchoredPosition = new Vector2(num, 0f);
			}
			else
			{
				isMove = false;
				cont.anchoredPosition = new Vector2(moveTargetX, 0f);
			}
		}
	}

	private void Update()
	{
	}

	public int GetMoveIndex(float posX, bool leftMove)
	{
		int num = Mathf.FloorToInt(Mathf.Abs(posX - sourceposX) / (redius * 2f + distance));
		float num2 = Mathf.Abs(posX - sourceposX) % (redius * 2f + distance);
		if (leftMove)
		{
			if (num2 >= (redius * 2f + distance) / 4f)
			{
				num++;
			}
		}
		else if (num2 >= (redius * 2f + distance) * 0.75f)
		{
			num++;
		}
		num = Mathf.Clamp(num, 0, selectCount - 1);
		AuctionDataManager.Instance?.SetCurrentSelect(num);
		return num;
	}

	public void MoveTargetPosX(int count)
	{
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		if (selectCount <= 0 && AuctionDataManager.Instance != null)
		{
			selectCount = AuctionDataManager.Instance.GetSelectCount();
		}
		count = Mathf.Clamp(count, 0, selectCount - 1);
		float targetX = sourceposX - (float)count * (redius * 2f + distance);
		isMove = false;
		if (firstOpen <= 0)
		{
			((MonoBehaviour)this).StartCoroutine(YanChi(0.1f, delegate
			{
				//IL_0016: Unknown result type (might be due to invalid IL or missing references)
				cont.anchoredPosition = new Vector2(targetX, 0f);
			}));
			firstOpen++;
		}
		else
		{
			cont.anchoredPosition = new Vector2(targetX, 0f);
		}
		AuctionDataManager.Instance?.SetCurrentSelect(count);
	}

	public IEnumerator YanChi(float shijian, Action ac)
	{
		yield return (object)new WaitForSeconds(shijian);
		ac();
	}
}
