using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Module.Rank;

public class RankItem : MonoBehaviour
{
	[Header("排名显示")]
	[SerializeField]
	private Image rankIcon;

	[SerializeField]
	private TextMeshProUGUI openIdText;

	[SerializeField]
	private TextMeshProUGUI rankText;

	[SerializeField]
	private Sprite[] rankSprites;

	[Header("玩家信息")]
	[SerializeField]
	private Image playerAvatar;

	[SerializeField]
	private TextMeshProUGUI playerName;

	[Header("等级显示")]
	[SerializeField]
	private TextMeshProUGUI totalAssets;

	[Header("特殊标记")]
	[SerializeField]
	private GameObject myRankTag;

	private int _rank;

	private string _openId;

	public int Rank => _rank;

	public string OpenId => _openId;

	public void Initialize(int rank, string openId, string nickname, long score, bool isMyRank = false, string imageUrl = "")
	{
		_rank = rank;
		_openId = openId;
		UpdateRankDisplay(rank);
		if ((Object)(object)playerName != (Object)null)
		{
			((TMP_Text)playerName).text = nickname;
			if (rank == 0 && string.IsNullOrEmpty(nickname))
			{
				((TMP_Text)playerName).text = "未上榜";
			}
		}
		if ((Object)(object)totalAssets != (Object)null)
		{
			((TMP_Text)totalAssets).text = score.ToString();
		}
		GameObject obj = myRankTag;
		if (obj != null)
		{
			obj.gameObject.SetActive(isMyRank);
		}
		if (isMyRank && rankSprites != null && rank >= 1 && rank <= 3 && rankSprites.Length > rank - 1 && (Object)(object)rankSprites[rank - 1] != (Object)null)
		{
			rankIcon.sprite = rankSprites[rank - 1];
			((Component)rankIcon).gameObject.SetActive(true);
		}
		Debug.Log((object)$"RankItem Initialize: {rank}, {openId}, {nickname}, {score}, {isMyRank}");
	}

	private void UpdateRankDisplay(int rank)
	{
		if (rank >= 1 && rank <= 3)
		{
			if ((Object)(object)rankIcon != (Object)null)
			{
				((Component)rankIcon).gameObject.SetActive(true);
				rankIcon.sprite = rankSprites[rank - 1];
			}
			TextMeshProUGUI obj = rankText;
			if (obj != null)
			{
				((Component)obj).gameObject.SetActive(false);
			}
		}
		else
		{
			if ((Object)(object)rankIcon != (Object)null)
			{
				((Component)rankIcon).gameObject.SetActive(false);
			}
			if ((Object)(object)rankText != (Object)null)
			{
				((Component)rankText).gameObject.SetActive(true);
				((TMP_Text)rankText).text = rank.ToString();
			}
		}
	}

	public void SetAsMyRank(bool isMyRank)
	{
		GameObject obj = myRankTag;
		if (obj != null)
		{
			obj.SetActive(isMyRank);
		}
	}
}
