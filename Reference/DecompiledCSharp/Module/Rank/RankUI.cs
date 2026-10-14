using System;
using System.Collections.Generic;
using Module.Http;
using TMPro;
using TTSDK;
using TTSDK.UNBridgeLib.LitJson;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Module.Rank;

public class RankUI : MonoBehaviour
{
	[Serializable]
	private sealed class ServerLeaderboardResponse
	{
		public ServerLeaderboardEntry[] items;
	}

	[Serializable]
	private sealed class ServerLeaderboardEntry
	{
		public int rank;

		public string openId;

		public string nickname;

		public string avatarUrl;

		public long bestScore;
	}

	[SerializeField]
	private Button maskBtn;

	[Header("标签按钮")]
	[SerializeField]
	private Button friendBtn;

	[SerializeField]
	private Button worldBtn;

	[Header("标签按钮选中状态")]
	[SerializeField]
	private Image friendBtnBg;

	[SerializeField]
	private Image worldBtnBg;

	[Header("世界标签按钮")]
	[SerializeField]
	private GameObject worldTypeTabGroup;

	[SerializeField]
	private Button coinBtn;

	[SerializeField]
	private Button diceBtn;

	[Header("世界标签按钮选中状态")]
	[SerializeField]
	private Image coinBtnBg;

	[SerializeField]
	private Image diceBtnBg;

	[Header("列表容器")]
	[SerializeField]
	private Transform listContainer;

	[Header("RankItem预制体")]
	[SerializeField]
	private RankItem rankItemPrefab;

	[Header("RankItem预制体1")]
	[SerializeField]
	private RankItem rankItemPrefab1;

	[Header("RankItem预制体2")]
	[SerializeField]
	private RankItem rankItemPrefab2;

	[Header("RankItem预制体3")]
	[SerializeField]
	private RankItem rankItemPrefab3;

	[Header("我的排名Item")]
	[SerializeField]
	private RankItem myRankItem;

	[Header("关闭按钮")]
	[SerializeField]
	private Button closeBtn;

	[Header("空状态提示")]
	[SerializeField]
	private TextMeshProUGUI emptyStateText;

	[SerializeField]
	private RectTransform emptyStateRectTransform;

	[SerializeField]
	private Button shareToFriendBtn;

	private RankType _currentRankType = RankType.World;

	private RankWorldType _currentWorldType;

	private List<RankItem> _rankItems = new List<RankItem>();

	private string _myOpenId;

	private int _selectedWorldId = 1;

	private int _rankRequestVersion;

	[SerializeField]
	private RawImage wechatMinigameRankImage;

	private void Awake()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Expected O, but got Unknown
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Expected O, but got Unknown
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Expected O, but got Unknown
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Expected O, but got Unknown
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Expected O, but got Unknown
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Expected O, but got Unknown
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Expected O, but got Unknown
		_myOpenId = GameDataManager.Instance.GetOpenId();
		SetWorldTypeByWorldId(GameDataManager.Instance.GetCurrentWorldId());
		if ((Object)(object)friendBtn != (Object)null)
		{
			((UnityEvent)friendBtn.onClick).AddListener((UnityAction)delegate
			{
				SwitchRankType(RankType.Friend);
			});
		}
		if ((Object)(object)worldBtn != (Object)null)
		{
			((UnityEvent)worldBtn.onClick).AddListener((UnityAction)delegate
			{
				SwitchRankType(RankType.World);
			});
		}
		if ((Object)(object)coinBtn != (Object)null)
		{
			((UnityEvent)coinBtn.onClick).AddListener((UnityAction)delegate
			{
				SwitchWorldType(RankWorldType.Coin);
			});
		}
		if ((Object)(object)diceBtn != (Object)null)
		{
			((UnityEvent)diceBtn.onClick).AddListener((UnityAction)delegate
			{
				SwitchWorldType(RankWorldType.Dice);
			});
		}
		if ((Object)(object)shareToFriendBtn != (Object)null)
		{
			((UnityEvent)shareToFriendBtn.onClick).AddListener((UnityAction)delegate
			{
				ShareToFriend();
			});
		}
		RankItem rankItem = myRankItem;
		if (rankItem != null)
		{
			((Component)rankItem).gameObject.SetActive(false);
		}
		UpdatePlatformTabs();
		UpdateTabButtons();
		if ((Object)(object)closeBtn != (Object)null)
		{
			((UnityEvent)closeBtn.onClick).AddListener(new UnityAction(Close));
		}
		if ((Object)(object)maskBtn != (Object)null)
		{
			((UnityEvent)maskBtn.onClick).AddListener(new UnityAction(Close));
		}
		RankItem rankItem2 = rankItemPrefab1;
		if (rankItem2 != null)
		{
			((Component)rankItem2).gameObject.SetActive(false);
		}
		RankItem rankItem3 = rankItemPrefab2;
		if (rankItem3 != null)
		{
			((Component)rankItem3).gameObject.SetActive(false);
		}
		RankItem rankItem4 = rankItemPrefab3;
		if (rankItem4 != null)
		{
			((Component)rankItem4).gameObject.SetActive(false);
		}
		RankItem rankItem5 = rankItemPrefab;
		if (rankItem5 != null)
		{
			((Component)rankItem5).gameObject.SetActive(false);
		}
		RankItem rankItem6 = myRankItem;
		if (rankItem6 != null)
		{
			((Component)rankItem6).gameObject.SetActive(false);
		}
		RawImage obj = wechatMinigameRankImage;
		if (obj != null)
		{
			((Component)obj).gameObject.SetActive(false);
		}
	}

	private void OnEnable()
	{
		SetWorldTypeByWorldId(GameDataManager.Instance.GetCurrentWorldId());
		UpdateTabButtons();
		RefreshRankData();
	}

	private void OnDisable()
	{
		ClearItems();
	}

	private void ShareToFriend()
	{
		SingletonMono<HttpManager>.Instance.ShareAppMessage(2, delegate(string msg)
		{
			string.IsNullOrEmpty(msg);
		}, delegate
		{
		});
	}

	private void SwitchRankType(RankType type)
	{
		if (_currentRankType != type)
		{
			_currentRankType = type;
			UpdateTabButtons();
			RefreshRankData();
		}
	}

	private void SwitchWorldType(RankWorldType type)
	{
		if (_currentWorldType != type)
		{
			_currentWorldType = type;
			_selectedWorldId = GetWorldIdByWorldType(type);
			UpdateTabButtons();
			RefreshRankData();
		}
	}

	private void UpdateTabButtons()
	{
		if ((Object)(object)friendBtnBg != (Object)null)
		{
			((Component)friendBtnBg).gameObject.SetActive(_currentRankType == RankType.Friend);
		}
		if ((Object)(object)worldBtnBg != (Object)null)
		{
			((Component)worldBtnBg).gameObject.SetActive(_currentRankType == RankType.World);
		}
		if ((Object)(object)coinBtnBg != (Object)null)
		{
			((Component)coinBtnBg).gameObject.SetActive(_currentWorldType == RankWorldType.Coin);
		}
		if ((Object)(object)diceBtnBg != (Object)null)
		{
			((Component)diceBtnBg).gameObject.SetActive(_currentWorldType == RankWorldType.Dice);
		}
	}

	private void SetWorldTypeByWorldId(int worldId)
	{
		_currentWorldType = ((worldId == 2) ? RankWorldType.Dice : RankWorldType.Coin);
		_selectedWorldId = GetWorldIdByWorldType(_currentWorldType);
	}

	private static int GetWorldIdByWorldType(RankWorldType type)
	{
		if (type != RankWorldType.Dice)
		{
			return 1;
		}
		return 2;
	}

	private void UpdatePlatformTabs()
	{
		GameObject obj = worldTypeTabGroup;
		if (obj != null)
		{
			obj.SetActive(true);
		}
	}

	private void RefreshRankData()
	{
		_rankRequestVersion++;
		ClearItems();
		if ((Object)(object)listContainer != (Object)null)
		{
			((Component)listContainer).gameObject.SetActive(true);
		}
		RankItem rankItem = myRankItem;
		if (rankItem != null)
		{
			((Component)rankItem).gameObject.SetActive(false);
		}
		LoadDouyinRankData(_currentRankType, _selectedWorldId, _rankRequestVersion);
	}

	private void LoadServerRankData(int worldId, int requestVersion)
	{
		if ((Object)(object)SingletonMono<HttpManager>.Instance == (Object)null)
		{
			ShowEmptyState(show: true, "排行榜服务未初始化");
			return;
		}
		SingletonMono<HttpManager>.Instance.GetLeaderboard(worldId, 99, delegate(bool success, string response)
		{
			if (!((Object)(object)this == (Object)null) && ((Component)this).gameObject.activeInHierarchy && worldId == _selectedWorldId && requestVersion == _rankRequestVersion)
			{
				if (!success)
				{
					ShowEmptyState(show: true, "排行榜加载失败");
				}
				else
				{
					ServerLeaderboardResponse serverLeaderboardResponse;
					try
					{
						serverLeaderboardResponse = JsonUtility.FromJson<ServerLeaderboardResponse>("{\"items\":" + response + "}");
					}
					catch (Exception ex)
					{
						Debug.LogWarning((object)("[RankUI] Failed to parse server leaderboard: " + ex.Message));
						ShowEmptyState(show: true, "排行榜数据异常");
						return;
					}
					ServerLeaderboardEntry[] array = serverLeaderboardResponse?.items;
					if (array == null || array.Length == 0)
					{
						ShowEmptyState(show: true, "暂无排行榜数据");
						ShowServerMyRank(hasMyRank: false, 0, 0L);
					}
					else
					{
						ClearItems();
						ShowEmptyState(show: false, string.Empty);
						bool hasMyRank = false;
						int rank = 0;
						long score = 0L;
						for (int i = 0; i < array.Length; i++)
						{
							ServerLeaderboardEntry serverLeaderboardEntry = array[i];
							if (serverLeaderboardEntry != null)
							{
								int num = ((serverLeaderboardEntry.rank > 0) ? serverLeaderboardEntry.rank : (i + 1));
								bool flag = !string.IsNullOrEmpty(_myOpenId) && serverLeaderboardEntry.openId == _myOpenId;
								CreateServerRankItem(num, serverLeaderboardEntry, flag);
								if (flag)
								{
									hasMyRank = true;
									rank = num;
									score = serverLeaderboardEntry.bestScore;
								}
							}
						}
						ResizeRankList();
						ShowServerMyRank(hasMyRank, rank, score);
					}
				}
			}
		});
	}

	private void CreateServerRankItem(int rank, ServerLeaderboardEntry entry, bool isMine)
	{
		RankItem rankItem = rankItemPrefab;
		if (rank == 1 && (Object)(object)rankItemPrefab1 != (Object)null)
		{
			rankItem = rankItemPrefab1;
		}
		else if (rank == 2 && (Object)(object)rankItemPrefab2 != (Object)null)
		{
			rankItem = rankItemPrefab2;
		}
		else if (rank == 3 && (Object)(object)rankItemPrefab3 != (Object)null)
		{
			rankItem = rankItemPrefab3;
		}
		if (!((Object)(object)rankItem == (Object)null) && !((Object)(object)listContainer == (Object)null))
		{
			((Component)rankItem).gameObject.SetActive(true);
			RankItem rankItem2 = Object.Instantiate<RankItem>(rankItem, listContainer);
			((Component)rankItem2).gameObject.SetActive(true);
			rankItem2.Initialize(rank, entry.openId, entry.nickname, entry.bestScore, isMine, entry.avatarUrl);
			_rankItems.Add(rankItem2);
			((Component)rankItem).gameObject.SetActive(false);
		}
	}

	private void ResizeRankList()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)listContainer == (Object)null))
		{
			RectTransform component = ((Component)listContainer).GetComponent<RectTransform>();
			if ((Object)(object)component != (Object)null)
			{
				component.sizeDelta = new Vector2(component.sizeDelta.x, (float)_rankItems.Count * 220f);
			}
		}
	}

	private void ShowServerMyRank(bool hasMyRank, int rank, long score)
	{
		if (!((Object)(object)myRankItem == (Object)null))
		{
			GameData gameData = (((Object)(object)GameDataManager.Instance != (Object)null) ? GameDataManager.Instance.GetGameData() : null);
			string openId = ((!string.IsNullOrEmpty(_myOpenId)) ? _myOpenId : GameDataManager.Instance?.GetOpenId());
			string nickname = ((gameData != null) ? gameData.nickName : string.Empty);
			((Component)myRankItem).gameObject.SetActive(true);
			myRankItem.Initialize(hasMyRank ? rank : 0, openId, nickname, hasMyRank ? score : GetSelectedWorldScore(), isMyRank: true);
		}
	}

	private int GetSelectedWorldScore()
	{
		if (!((Object)(object)GameDataManager.Instance != (Object)null))
		{
			return 0;
		}
		return GameDataManager.Instance.GetWorldMainMissionLevel(_selectedWorldId);
	}

	private void LoadDouyinRankData(RankType rankType, int worldId, int requestVersion, int pageNum = 1, int pageSize = 40)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Expected O, but got Unknown
		//IL_00e7: Expected O, but got Unknown
		//IL_00e7: Expected O, but got Unknown
		string text = ((rankType == RankType.Friend) ? "friend" : "all");
		TT.GetImRankData(new JsonData
		{
			["relationType"] = JsonData.op_Implicit(text),
			["dataType"] = JsonData.op_Implicit(0),
			["rankType"] = JsonData.op_Implicit("month"),
			["pageNum"] = JsonData.op_Implicit(pageNum),
			["pageSize"] = JsonData.op_Implicit(pageSize),
			["zoneId"] = JsonData.op_Implicit(WorldRankUtility.GetZoneId(worldId))
		}, (OnGetRankDataSuccessCallback)delegate(ref RankData data)
		{
			if (!((Object)(object)this == (Object)null) && ((Component)this).gameObject.activeInHierarchy)
			{
				if (rankType != _currentRankType || worldId != _selectedWorldId || requestVersion != _rankRequestVersion)
				{
					Debug.LogWarning((object)$"Ignore outdated leaderboard callback. rankType={rankType}, worldId={worldId}, requestVersion={requestVersion}");
				}
				else
				{
					if (data != null)
					{
						Debug.Log((object)$"data.Items;: {data.Items}");
						Debug.Log((object)$"data.PageNum;: {data.PageNum}");
						Debug.Log((object)$"data.SelfItem;: {data.SelfItem}");
						Debug.Log((object)$"data.SelfUserInfo;: {data.SelfUserInfo}");
					}
					Debug.Log((object)$"data: {data}");
					Debug.Log((object)("data: " + ((data != null && data.Items != null) ? data.Items.Count.ToString() : "null")));
					Debug.Log((object)$"data: {data?.SelfItem}");
					if (data?.SelfItem != null)
					{
						Debug.Log((object)$"data: {data.SelfItem.Rank}");
					}
					if (data != null && data.Items != null)
					{
						for (int i = 0; i < data.Items.Count; i++)
						{
							Debug.Log((object)("item-->nickName :" + data.Items[i].Nickname + ", openId:" + data.Items[i].OpenId));
						}
					}
					Debug.Log((object)"GetImRankData success");
					OnRankDataLoaded(data, rankType, worldId, requestVersion, pageNum, pageSize);
				}
			}
		}, (OnGetRankDataFailCallback)delegate(string msg)
		{
			if (!((Object)(object)this == (Object)null) && ((Component)this).gameObject.activeInHierarchy && rankType == _currentRankType && worldId == _selectedWorldId && requestVersion == _rankRequestVersion)
			{
				Debug.Log((object)("GetImRankData fail error message:" + msg));
				if (pageNum == 1)
				{
					ShowEmptyState(show: true, "加载失败: " + msg);
				}
			}
		});
	}

	private void OnRankDataLoaded(RankData rankData, RankType rankType, int worldId, int requestVersion, int pageNum, int pageSize)
	{
		//IL_0298: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
		Debug.Log((object)$"OnRankDataLoaded pageNum:{pageNum}, pageSize:{pageSize}");
		if (pageNum == 1)
		{
			ClearItems();
		}
		if (rankData == null || rankData.Items == null || rankData.Items.Count == 0)
		{
			if (pageNum != 1)
			{
				return;
			}
			Debug.Log((object)"暂无排行数据");
			ShowEmptyState(show: true, "暂无排行数据");
			if (rankData != null && rankData.SelfItem != null)
			{
				Debug.Log((object)$"rankData.SelfItem: {rankData.SelfItem}");
				SetMyRank(rankData.SelfItem);
				return;
			}
			if (rankData != null && rankData.SelfUserInfo != null)
			{
				Debug.Log((object)$"rankData.SelfUserInfo: {rankData.SelfUserInfo}");
				SetMyRank(rankData.SelfUserInfo);
				return;
			}
			Debug.Log((object)"rankData:null2");
			if ((Object)(object)myRankItem != (Object)null)
			{
				((Component)myRankItem).gameObject.SetActive(false);
			}
			return;
		}
		Debug.Log((object)"有数据");
		if (pageNum == 1)
		{
			ShowEmptyState(show: false, "");
		}
		int count = _rankItems.Count;
		for (int i = 0; i < rankData.Items.Count; i++)
		{
			RankResItem val = rankData.Items[i];
			bool flag = !string.IsNullOrEmpty(_myOpenId) && val.OpenId == _myOpenId;
			Debug.Log((object)$"isMyRank: {flag}, item.OpenId: {val.OpenId},item.Nickname:{val.Nickname}");
			long result = 0L;
			if (!string.IsNullOrEmpty(val.Value))
			{
				long.TryParse(val.Value, out result);
			}
			int num = count + i;
			RankItem rankItem = rankItemPrefab;
			if (num == 0 && (Object)(object)rankItemPrefab1 != (Object)null)
			{
				rankItem = rankItemPrefab1;
			}
			else if (num == 1 && (Object)(object)rankItemPrefab2 != (Object)null)
			{
				rankItem = rankItemPrefab2;
			}
			else if (num == 2 && (Object)(object)rankItemPrefab3 != (Object)null)
			{
				rankItem = rankItemPrefab3;
			}
			((Component)rankItem).gameObject.SetActive(true);
			RankItem rankItem2 = Object.Instantiate<RankItem>(rankItem, listContainer);
			((Component)rankItem2).gameObject.SetActive(true);
			rankItem2.Initialize(num + 1, val.OpenId, val.Nickname, result, flag, val.UserImg);
			_rankItems.Add(rankItem2);
			((Component)rankItem).gameObject.SetActive(false);
		}
		if ((Object)(object)listContainer != (Object)null)
		{
			RectTransform component = ((Component)listContainer).GetComponent<RectTransform>();
			if ((Object)(object)component != (Object)null)
			{
				float num2 = (float)_rankItems.Count * 220f;
				component.sizeDelta = new Vector2(component.sizeDelta.x, num2);
			}
		}
		if ((Object)(object)myRankItem != (Object)null)
		{
			Debug.Log((object)$"myRankItem: {myRankItem}");
			if (rankData.SelfItem != null && rankData.SelfItem.Item != null)
			{
				((Component)myRankItem).gameObject.SetActive(true);
				long result2 = 0L;
				if (!string.IsNullOrEmpty(rankData.SelfItem.Item.Value))
				{
					long.TryParse(rankData.SelfItem.Item.Value, out result2);
				}
				int rank = rankData.SelfItem.Rank;
				Debug.Log((object)$"myRank: {rank}");
				myRankItem.Initialize(rank, rankData.SelfItem.Item.OpenId, rankData.SelfItem.Item.Nickname, result2, isMyRank: true);
			}
			else if (rankData.SelfUserInfo != null)
			{
				Debug.Log((object)$"myRank.SelfUserInfo: {0}");
				((Component)myRankItem).gameObject.SetActive(true);
				myRankItem.Initialize(0, rankData.SelfUserInfo.OpenId, rankData.SelfUserInfo.Nickname, GetSelectedWorldScore(), isMyRank: true);
			}
			else if (pageNum == 1)
			{
				Debug.Log((object)"myRank: None");
				((Component)myRankItem).gameObject.SetActive(false);
			}
		}
		if (rankType != RankType.World || rankType != _currentRankType || worldId != _selectedWorldId || requestVersion != _rankRequestVersion)
		{
			return;
		}
		int count2 = _rankItems.Count;
		if (count2 < 99 && rankData.Items.Count == pageSize)
		{
			int num3 = pageNum + 1;
			int num4 = 40;
			if (count2 + num4 > 99)
			{
				num4 = 99 - count2;
			}
			if (num4 > 0)
			{
				Debug.Log((object)$"Actively requesting page {num3} with pageSize {num4} for total/world rank.");
				LoadDouyinRankData(rankType, worldId, requestVersion, num3, num4);
			}
		}
	}

	private void SetMyRank(SelfRankResItem selfItem)
	{
		if (!((Object)(object)myRankItem != (Object)null))
		{
			return;
		}
		Debug.Log((object)$"myRankItem: {myRankItem}");
		if (selfItem != null && selfItem.Item != null)
		{
			((Component)myRankItem).gameObject.SetActive(true);
			long result = 0L;
			if (!string.IsNullOrEmpty(selfItem.Item.Value))
			{
				long.TryParse(selfItem.Item.Value, out result);
			}
			int rank = selfItem.Rank;
			Debug.Log((object)$"myRank: {rank}");
			myRankItem.Initialize(rank, selfItem.Item.OpenId, selfItem.Item.Nickname, result, isMyRank: true);
		}
		else
		{
			Debug.Log((object)"myRank: None");
			((Component)myRankItem).gameObject.SetActive(false);
		}
	}

	private void SetMyRank(UserInfo userInfo)
	{
		if ((Object)(object)myRankItem != (Object)null)
		{
			Debug.Log((object)$"userInfo: {userInfo}");
			if (userInfo != null)
			{
				Debug.Log((object)$"myRank: {0}");
				((Component)myRankItem).gameObject.SetActive(true);
				myRankItem.Initialize(0, userInfo.OpenId, userInfo.Nickname, GetSelectedWorldScore(), isMyRank: true);
			}
			else
			{
				Debug.Log((object)"myRank: None");
				((Component)myRankItem).gameObject.SetActive(false);
			}
		}
	}

	private void ShowEmptyState(bool show, string message)
	{
		if ((Object)(object)emptyStateRectTransform != (Object)null)
		{
			((Component)emptyStateRectTransform).gameObject.SetActive(show);
		}
		if ((Object)(object)emptyStateText != (Object)null)
		{
			((TMP_Text)emptyStateText).text = message;
		}
	}

	private void ClearItems()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		foreach (RankItem rankItem in _rankItems)
		{
			if ((Object)(object)rankItem != (Object)null && (Object)(object)((Component)rankItem).gameObject != (Object)null)
			{
				Object.Destroy((Object)(object)((Component)rankItem).gameObject);
			}
		}
		_rankItems.Clear();
		if ((Object)(object)listContainer != (Object)null)
		{
			RectTransform component = ((Component)listContainer).GetComponent<RectTransform>();
			if ((Object)(object)component != (Object)null)
			{
				component.sizeDelta = new Vector2(component.sizeDelta.x, 0f);
			}
		}
		RectTransform obj = emptyStateRectTransform;
		if (obj != null)
		{
			((Component)obj).gameObject.SetActive(false);
		}
	}

	public void Open()
	{
		((Component)this).gameObject.SetActive(true);
		((Component)this).transform.SetAsLastSibling();
	}

	public void Close()
	{
		((Component)this).gameObject.SetActive(false);
	}

	private void OnDestroy()
	{
		if ((Object)(object)friendBtn != (Object)null)
		{
			((UnityEventBase)friendBtn.onClick).RemoveAllListeners();
		}
		if ((Object)(object)worldBtn != (Object)null)
		{
			((UnityEventBase)worldBtn.onClick).RemoveAllListeners();
		}
		if ((Object)(object)coinBtn != (Object)null)
		{
			((UnityEventBase)coinBtn.onClick).RemoveAllListeners();
		}
		if ((Object)(object)diceBtn != (Object)null)
		{
			((UnityEventBase)diceBtn.onClick).RemoveAllListeners();
		}
		if ((Object)(object)closeBtn != (Object)null)
		{
			((UnityEventBase)closeBtn.onClick).RemoveAllListeners();
		}
		if ((Object)(object)maskBtn != (Object)null)
		{
			((UnityEventBase)maskBtn.onClick).RemoveAllListeners();
		}
		ClearItems();
	}
}
