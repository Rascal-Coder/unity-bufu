using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using cfg;

public class AuctionMatchPanel : MonoBehaviour
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static UnityAction _003C_003E9__32_1;

		internal void _003CStart_003Eb__32_1()
		{
			SoundManager.Instance?.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
			UIManager.Instance?.topPanel.ShowSetUpPanel();
		}
	}

	[Header("匹配时间文本")]
	[SerializeField]
	private TextMeshProUGUI matchTime;

	[Header("匹配人数")]
	[SerializeField]
	private TextMeshProUGUI reshuTxt;

	[Header("匹配的人物头像,不包括玩家")]
	[SerializeField]
	private Image[] icons;

	[Header("匹配的人物的名字文本")]
	[SerializeField]
	private TextMeshProUGUI[] npcNames;

	[Header("已准备字体")]
	[SerializeField]
	private GameObject already;

	[Header("准备按键")]
	[SerializeField]
	private Button readyBtn;

	[Header("拍卖场金钱文本")]
	[SerializeField]
	private TextMeshProUGUI auctionCoinTxt;

	[Header("拍卖场门票文本")]
	[SerializeField]
	private TextMeshProUGUI ticketTxt;

	[Header("拍卖行退出按键")]
	[SerializeField]
	private Button setupBtn;

	[Header("光效旋转图片")]
	[SerializeField]
	private Image lightrotateImg;

	[Header("等级icon")]
	[SerializeField]
	private Image levelImg;

	private float lightRotateSpeed = 140f;

	private float baseMatchTime = 5f;

	private float minMatchInterval = 0.6f;

	private float maxMatchInterval = 1.5f;

	private float jellyStartScale = 1.8f;

	private float jellyDuration = 0.5f;

	private float jellyBounceIntensity = 0.3f;

	private float breatheMinScale = 0.9f;

	private float breatheMaxScale = 1.1f;

	private float breatheSpeed = 2f;

	private bool isPlayReady;

	private int npcReadyCount;

	private int npcMaxCount;

	private float allMatchTime;

	private float matchjishi;

	private List<int> chouqvlist;

	private List<int> chouqvlist2;

	private List<float> precomputedIntervals;

	private int currentIntervalIndex;

	private GameObject auctionPlayerPanel;

	private string[] names;

	private void Start()
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		allMatchTime = 0f;
		chouqvlist = new List<int>();
		chouqvlist2 = new List<int>();
		precomputedIntervals = new List<float>();
		names = AuctionDataManager.Instance?.GetRandomNames();
		npcMaxCount = 5;
		((UnityEvent)readyBtn.onClick).AddListener((UnityAction)delegate
		{
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			SoundManager.Instance?.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
			if (npcReadyCount >= npcMaxCount)
			{
				EnterAuctionScene();
			}
			((Component)readyBtn).gameObject.SetActive(false);
			((Component)readyBtn).transform.localScale = Vector3.one;
			already.SetActive(true);
			isPlayReady = true;
		});
		if ((Object)(object)setupBtn != (Object)null)
		{
			ButtonClickedEvent onClick = setupBtn.onClick;
			object obj = _003C_003Ec._003C_003E9__32_1;
			if (obj == null)
			{
				UnityAction val = delegate
				{
					SoundManager.Instance?.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
					UIManager.Instance?.topPanel.ShowSetUpPanel();
				};
				_003C_003Ec._003C_003E9__32_1 = val;
				obj = (object)val;
			}
			((UnityEvent)onClick).AddListener((UnityAction)obj);
		}
		ResetPanelInfo();
	}

	private void Update()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)lightrotateImg != (Object)null && ((Component)lightrotateImg).gameObject.activeInHierarchy)
		{
			((Component)lightrotateImg).transform.Rotate(0f, 0f, (0f - lightRotateSpeed) * Time.deltaTime);
		}
		if ((Object)(object)readyBtn != (Object)null && ((Component)readyBtn).gameObject.activeInHierarchy)
		{
			float num = Mathf.Lerp(breatheMinScale, breatheMaxScale, (Mathf.Sin(Time.time * breatheSpeed) + 1f) * 0.5f);
			((Component)readyBtn).transform.localScale = Vector3.one * num;
		}
		if (npcReadyCount < npcMaxCount)
		{
			if (matchjishi > 0f)
			{
				matchjishi -= Time.deltaTime;
			}
			else if (matchjishi <= 0f)
			{
				if (currentIntervalIndex < precomputedIntervals.Count)
				{
					matchjishi = precomputedIntervals[currentIntervalIndex];
					currentIntervalIndex++;
				}
				else
				{
					matchjishi = Random.Range(minMatchInterval, maxMatchInterval);
				}
				npcReadyCount++;
				RefreshPanel();
				if (npcReadyCount >= npcMaxCount && isPlayReady)
				{
					EnterAuctionScene();
				}
			}
		}
		if (((Component)this).gameObject.activeSelf)
		{
			allMatchTime += Time.deltaTime;
			((TMP_Text)matchTime).text = allMatchTime.ToString("0");
		}
	}

	public void RefreshPanel()
	{
		((TMP_Text)reshuTxt).text = $"{npcReadyCount}/{npcMaxCount}";
		int index = GetNpcIndex();
		int npcIconIndex = GetNpcIconIndex();
		if (icons.Length <= npcIconIndex)
		{
			return;
		}
		SingletonMono<ResManager>.Instance.LoadSpriteAsync($"Assets/GameRes/icon/auction_avatar/challenge_pnl_match_avatar_0{npcIconIndex + 2}.png", delegate(Sprite sprite)
		{
			if (icons.Length > index)
			{
				icons[index].sprite = sprite;
				((Component)icons[index]).gameObject.SetActive(true);
				RectTransform component = ((Component)icons[index]).GetComponent<RectTransform>();
				if ((Object)(object)component != (Object)null && ((Component)this).gameObject.activeInHierarchy)
				{
					((MonoBehaviour)this).StartCoroutine(JellyInAnimation(component));
				}
			}
			if (names != null && names.Length > index)
			{
				((TMP_Text)npcNames[index]).text = names[index];
			}
		}, isAtlas: false);
	}

	public void ResetPanelInfo()
	{
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		if (chouqvlist != null)
		{
			chouqvlist.Clear();
			for (int i = 0; i < npcMaxCount; i++)
			{
				chouqvlist.Add(i);
			}
		}
		if (chouqvlist2 != null)
		{
			chouqvlist2.Clear();
			for (int j = 0; j < npcMaxCount; j++)
			{
				chouqvlist2.Add(j);
			}
		}
		PrecomputeMatchIntervals();
		allMatchTime = 0f;
		isPlayReady = false;
		npcReadyCount = 0;
		currentIntervalIndex = 0;
		matchjishi = ((precomputedIntervals.Count > 0) ? precomputedIntervals[0] : Random.Range(minMatchInterval, maxMatchInterval));
		currentIntervalIndex = 1;
		((Component)readyBtn).gameObject.SetActive(true);
		((Component)readyBtn).transform.localScale = Vector3.one;
		already.SetActive(false);
		if (icons != null && icons.Length != 0)
		{
			for (int k = 0; k < icons.Length; k++)
			{
				((Component)icons[k]).gameObject.SetActive(false);
				RectTransform component = ((Component)icons[k]).GetComponent<RectTransform>();
				if ((Object)(object)component != (Object)null)
				{
					((Transform)component).localScale = Vector3.one;
				}
			}
		}
		if (npcNames != null && npcNames.Length != 0)
		{
			for (int l = 0; l < npcNames.Length; l++)
			{
				((TMP_Text)npcNames[l]).text = "等待中";
				((Component)npcNames[l]).gameObject.SetActive(true);
			}
		}
		names = AuctionDataManager.Instance?.GetRandomNames();
		RefrshAuctionTxt();
		int num = AuctionDataManager.Instance?.GetCurrentSelect() ?? 0;
		num++;
		auction auctionConfig = ConfigManager.Instance.GetAuctionConfig(num);
		SingletonMono<ResManager>.Instance.LoadSpriteAsync($"Assets/GameRes/icon/auction_title/challenge_pic_title_{auctionConfig.AuctionHouseID}.png", delegate(Sprite sprite)
		{
			levelImg.sprite = sprite;
		}, isAtlas: false);
	}

	private void PrecomputeMatchIntervals()
	{
		if (precomputedIntervals == null)
		{
			precomputedIntervals = new List<float>();
		}
		precomputedIntervals.Clear();
		float num = baseMatchTime * Random.Range(0.8f, 1.2f);
		float num2 = 0f;
		float[] array = new float[npcMaxCount];
		for (int i = 0; i < npcMaxCount; i++)
		{
			array[i] = Random.Range(minMatchInterval, maxMatchInterval);
			num2 += array[i];
		}
		float num3 = num / num2;
		float num4 = 0f;
		for (int j = 0; j < npcMaxCount - 1; j++)
		{
			float num5 = array[j] * num3;
			num5 = Mathf.Clamp(num5, minMatchInterval * 0.5f, maxMatchInterval * 1.5f);
			precomputedIntervals.Add(num5);
			num4 += num5;
		}
		float num6 = num - num4;
		precomputedIntervals.Add(Mathf.Max(minMatchInterval * 0.3f, num6));
	}

	private IEnumerator JellyInAnimation(RectTransform target)
	{
		if (!((Object)(object)target == (Object)null))
		{
			float elapsed = 0f;
			((Transform)target).localScale = Vector3.one * jellyStartScale;
			while (elapsed < jellyDuration)
			{
				elapsed += Time.deltaTime;
				float num = elapsed / jellyDuration;
				float num2 = Mathf.Exp(-5f * num);
				float num3 = Mathf.Cos(12f * num);
				float num4 = 1f + (jellyStartScale - 1f) * num2 * (1f + jellyBounceIntensity * num3);
				((Transform)target).localScale = Vector3.one * num4;
				yield return null;
			}
			((Transform)target).localScale = Vector3.one;
		}
	}

	public void EnterAuctionScene()
	{
		if ((Object)(object)auctionPlayerPanel == (Object)null)
		{
			SingletonMono<ResManager>.Instance.LoadPrefabAsync("Assets/GameRes/NewUI/Auction/prefabs/auctionPlayPanel.prefab", delegate(GameObject prefab)
			{
				Transform layerTransform = UIManager.Instance.GetLayerTransform(UILayer.Main);
				auctionPlayerPanel = Object.Instantiate<GameObject>(prefab, layerTransform);
			});
		}
		else
		{
			auctionPlayerPanel.SetActive(true);
			auctionPlayerPanel.GetComponent<AuctionPlayPanel>()?.ResetSceneInfo();
		}
		((Component)this).gameObject.SetActive(false);
	}

	public int GetNpcIndex()
	{
		int result = 0;
		if (chouqvlist.Count > 0)
		{
			int index = Random.Range(0, chouqvlist.Count);
			result = chouqvlist[index];
			chouqvlist.RemoveAt(index);
		}
		return result;
	}

	public int GetNpcIconIndex()
	{
		int result = 0;
		if (chouqvlist2.Count > 0)
		{
			int index = Random.Range(0, chouqvlist2.Count);
			result = chouqvlist2[index];
			chouqvlist2.RemoveAt(index);
		}
		return result;
	}

	public void RefrshAuctionTxt()
	{
		((TMP_Text)auctionCoinTxt).text = NumberFormatter.FormatNumber(AuctionDataManager.Instance?.GetAuctionCoin() ?? 0.0);
		((TMP_Text)ticketTxt).text = NumberFormatter.FormatNumber(AuctionDataManager.Instance?.GetTicketCount() ?? 0);
	}
}
