using System;
using SimpleJSON;

namespace cfg;

public class Tables
{
	public Tbauction Tbauction { get; }

	public TbauctionItem TbauctionItem { get; }

	public Tbchallenge Tbchallenge { get; }

	public TbCommon TbCommon { get; }

	public Tbdesk Tbdesk { get; }

	public TbhelperRole TbhelperRole { get; }

	public Tbitem Tbitem { get; }

	public TbitemUpgrade1 TbitemUpgrade1 { get; }

	public TbitemUpgrade2 TbitemUpgrade2 { get; }

	public TbitemUpgrade3 TbitemUpgrade3 { get; }

	public Tblanguage Tblanguage { get; }

	public TblimitMission TblimitMission { get; }

	public TblimitMission2 TblimitMission2 { get; }

	public TbmainMission TbmainMission { get; }

	public Tbshop Tbshop { get; }

	public TbsignReward TbsignReward { get; }

	public TbsignWorldCup TbsignWorldCup { get; }

	public Tbtalent Tbtalent { get; }

	public Tbtalent2 Tbtalent2 { get; }

	public Tbtalent3 Tbtalent3 { get; }

	public Tbtalent4 Tbtalent4 { get; }

	public TbtalentPoint TbtalentPoint { get; }

	public TbtalentPoint2 TbtalentPoint2 { get; }

	public Tbworld Tbworld { get; }

	public TbWorldCupMisson TbWorldCupMisson { get; }

	public TbWorldUpScoreReward TbWorldUpScoreReward { get; }

	public static string[] GetAllConfigFileNames()
	{
		return new string[26]
		{
			"tbauction", "tbauctionitem", "tbchallenge", "tbcommon", "tbdesk", "tbhelperrole", "tbitem", "tbitemupgrade1", "tbitemupgrade2", "tbitemupgrade3",
			"tblanguage", "tblimitmission", "tblimitmission2", "tbmainmission", "tbshop", "tbsignreward", "tbsignworldcup", "tbtalent", "tbtalent2", "tbtalent3",
			"tbtalent4", "tbtalentpoint", "tbtalentpoint2", "tbworld", "tbworldcupmisson", "tbworldupscorereward"
		};
	}

	public Tables(Func<string, JSONNode> loader)
	{
		Tbauction = new Tbauction(loader("tbauction"));
		TbauctionItem = new TbauctionItem(loader("tbauctionitem"));
		Tbchallenge = new Tbchallenge(loader("tbchallenge"));
		TbCommon = new TbCommon(loader("tbcommon"));
		Tbdesk = new Tbdesk(loader("tbdesk"));
		TbhelperRole = new TbhelperRole(loader("tbhelperrole"));
		Tbitem = new Tbitem(loader("tbitem"));
		TbitemUpgrade1 = new TbitemUpgrade1(loader("tbitemupgrade1"));
		TbitemUpgrade2 = new TbitemUpgrade2(loader("tbitemupgrade2"));
		TbitemUpgrade3 = new TbitemUpgrade3(loader("tbitemupgrade3"));
		Tblanguage = new Tblanguage(loader("tblanguage"));
		TblimitMission = new TblimitMission(loader("tblimitmission"));
		TblimitMission2 = new TblimitMission2(loader("tblimitmission2"));
		TbmainMission = new TbmainMission(loader("tbmainmission"));
		Tbshop = new Tbshop(loader("tbshop"));
		TbsignReward = new TbsignReward(loader("tbsignreward"));
		TbsignWorldCup = new TbsignWorldCup(loader("tbsignworldcup"));
		Tbtalent = new Tbtalent(loader("tbtalent"));
		Tbtalent2 = new Tbtalent2(loader("tbtalent2"));
		Tbtalent3 = new Tbtalent3(loader("tbtalent3"));
		Tbtalent4 = new Tbtalent4(loader("tbtalent4"));
		TbtalentPoint = new TbtalentPoint(loader("tbtalentpoint"));
		TbtalentPoint2 = new TbtalentPoint2(loader("tbtalentpoint2"));
		Tbworld = new Tbworld(loader("tbworld"));
		TbWorldCupMisson = new TbWorldCupMisson(loader("tbworldcupmisson"));
		TbWorldUpScoreReward = new TbWorldUpScoreReward(loader("tbworldupscorereward"));
		ResolveRef();
	}

	private void ResolveRef()
	{
		Tbauction.ResolveRef(this);
		TbauctionItem.ResolveRef(this);
		Tbchallenge.ResolveRef(this);
		TbCommon.ResolveRef(this);
		Tbdesk.ResolveRef(this);
		TbhelperRole.ResolveRef(this);
		Tbitem.ResolveRef(this);
		TbitemUpgrade1.ResolveRef(this);
		TbitemUpgrade2.ResolveRef(this);
		TbitemUpgrade3.ResolveRef(this);
		Tblanguage.ResolveRef(this);
		TblimitMission.ResolveRef(this);
		TblimitMission2.ResolveRef(this);
		TbmainMission.ResolveRef(this);
		Tbshop.ResolveRef(this);
		TbsignReward.ResolveRef(this);
		TbsignWorldCup.ResolveRef(this);
		Tbtalent.ResolveRef(this);
		Tbtalent2.ResolveRef(this);
		Tbtalent3.ResolveRef(this);
		Tbtalent4.ResolveRef(this);
		TbtalentPoint.ResolveRef(this);
		TbtalentPoint2.ResolveRef(this);
		Tbworld.ResolveRef(this);
		TbWorldCupMisson.ResolveRef(this);
		TbWorldUpScoreReward.ResolveRef(this);
	}
}
