using System.Collections.Generic;
using Luban;
using SimpleJSON;

namespace cfg;

public sealed class auction : BeanBase
{
	public readonly int AuctionHouseID;

	public readonly string AuctionName;

	public readonly float ConditionLevel;

	public readonly int FreeTicket;

	public readonly int TimeRecover;

	public readonly int ItemValueMin;

	public readonly int ItemValueMax;

	public readonly List<float> StartPriceRange;

	public readonly List<int> AuctionRoundMin;

	public readonly List<int> AuctionRoundMax;

	public readonly List<float> ExpectPriceRange;

	public readonly float PlayerOfferAdd1;

	public readonly float PlayerOfferAdd2;

	public readonly float PlayerOfferAdd3;

	public readonly float QuitOutTime;

	public readonly List<float> NpcOfferAddRange;

	public readonly List<float> NpcOfferRate;

	public readonly List<float> NpcQuitOfferRange;

	public readonly List<float> FinalValueBuff;

	public readonly List<float> RaisePriceStage;

	public readonly List<float> RaiseBuffRange;

	public readonly List<float> RaisePriceIncomeRange;

	public readonly List<int> ResultItemRarity;

	public readonly List<int> ResultItemWeight;

	public readonly List<int> ResultItemPriceWeight;

	public readonly List<float> GiveUpChance1;

	public readonly List<float> GiveUpChance2;

	public readonly List<float> GiveUpChance3;

	public readonly List<float> GiveUpChance4;

	public readonly List<float> AngerProbability;

	public readonly List<float> AngerRound;

	public const int __ID__ = -661856701;

	public auction(JSONNode _buf)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_027b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0334: Unknown result type (might be due to invalid IL or missing references)
		//IL_0362: Unknown result type (might be due to invalid IL or missing references)
		//IL_0390: Unknown result type (might be due to invalid IL or missing references)
		//IL_03be: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0425: Unknown result type (might be due to invalid IL or missing references)
		//IL_0469: Unknown result type (might be due to invalid IL or missing references)
		//IL_049e: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0517: Unknown result type (might be due to invalid IL or missing references)
		//IL_055b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0590: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0609: Unknown result type (might be due to invalid IL or missing references)
		//IL_064d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0682: Unknown result type (might be due to invalid IL or missing references)
		//IL_06c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_06fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_073f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0774: Unknown result type (might be due to invalid IL or missing references)
		//IL_07b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_07ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0831: Unknown result type (might be due to invalid IL or missing references)
		//IL_0866: Unknown result type (might be due to invalid IL or missing references)
		//IL_08aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_08df: Unknown result type (might be due to invalid IL or missing references)
		//IL_0923: Unknown result type (might be due to invalid IL or missing references)
		//IL_0958: Unknown result type (might be due to invalid IL or missing references)
		//IL_099c: Unknown result type (might be due to invalid IL or missing references)
		//IL_09d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a15: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a4a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a8e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ac3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b07: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b3c: Unknown result type (might be due to invalid IL or missing references)
		if (!_buf["auctionHouseID"].IsNumber)
		{
			throw new SerializationException();
		}
		AuctionHouseID = JSONNode.op_Implicit(_buf["auctionHouseID"]);
		if (!_buf["auctionName"].IsString)
		{
			throw new SerializationException();
		}
		AuctionName = JSONNode.op_Implicit(_buf["auctionName"]);
		if (!_buf["conditionLevel"].IsNumber)
		{
			throw new SerializationException();
		}
		ConditionLevel = JSONNode.op_Implicit(_buf["conditionLevel"]);
		if (!_buf["freeTicket"].IsNumber)
		{
			throw new SerializationException();
		}
		FreeTicket = JSONNode.op_Implicit(_buf["freeTicket"]);
		if (!_buf["timeRecover"].IsNumber)
		{
			throw new SerializationException();
		}
		TimeRecover = JSONNode.op_Implicit(_buf["timeRecover"]);
		if (!_buf["itemValueMin"].IsNumber)
		{
			throw new SerializationException();
		}
		ItemValueMin = JSONNode.op_Implicit(_buf["itemValueMin"]);
		if (!_buf["itemValueMax"].IsNumber)
		{
			throw new SerializationException();
		}
		ItemValueMax = JSONNode.op_Implicit(_buf["itemValueMax"]);
		JSONNode val = _buf["startPriceRange"];
		if (!val.IsArray)
		{
			throw new SerializationException();
		}
		StartPriceRange = new List<float>(val.Count);
		foreach (JSONNode child in val.Children)
		{
			if (!child.IsNumber)
			{
				throw new SerializationException();
			}
			float num = JSONNode.op_Implicit(child);
			StartPriceRange.Add(num);
		}
		JSONNode val2 = _buf["auctionRoundMin"];
		if (!val2.IsArray)
		{
			throw new SerializationException();
		}
		AuctionRoundMin = new List<int>(val2.Count);
		foreach (JSONNode child2 in val2.Children)
		{
			if (!child2.IsNumber)
			{
				throw new SerializationException();
			}
			int num2 = JSONNode.op_Implicit(child2);
			AuctionRoundMin.Add(num2);
		}
		JSONNode val3 = _buf["auctionRoundMax"];
		if (!val3.IsArray)
		{
			throw new SerializationException();
		}
		AuctionRoundMax = new List<int>(val3.Count);
		foreach (JSONNode child3 in val3.Children)
		{
			if (!child3.IsNumber)
			{
				throw new SerializationException();
			}
			int num3 = JSONNode.op_Implicit(child3);
			AuctionRoundMax.Add(num3);
		}
		JSONNode val4 = _buf["expectPriceRange"];
		if (!val4.IsArray)
		{
			throw new SerializationException();
		}
		ExpectPriceRange = new List<float>(val4.Count);
		foreach (JSONNode child4 in val4.Children)
		{
			if (!child4.IsNumber)
			{
				throw new SerializationException();
			}
			float num4 = JSONNode.op_Implicit(child4);
			ExpectPriceRange.Add(num4);
		}
		if (!_buf["playerOfferAdd1"].IsNumber)
		{
			throw new SerializationException();
		}
		PlayerOfferAdd1 = JSONNode.op_Implicit(_buf["playerOfferAdd1"]);
		if (!_buf["playerOfferAdd2"].IsNumber)
		{
			throw new SerializationException();
		}
		PlayerOfferAdd2 = JSONNode.op_Implicit(_buf["playerOfferAdd2"]);
		if (!_buf["playerOfferAdd3"].IsNumber)
		{
			throw new SerializationException();
		}
		PlayerOfferAdd3 = JSONNode.op_Implicit(_buf["playerOfferAdd3"]);
		if (!_buf["quitOutTime"].IsNumber)
		{
			throw new SerializationException();
		}
		QuitOutTime = JSONNode.op_Implicit(_buf["quitOutTime"]);
		JSONNode val5 = _buf["npcOfferAddRange"];
		if (!val5.IsArray)
		{
			throw new SerializationException();
		}
		NpcOfferAddRange = new List<float>(val5.Count);
		foreach (JSONNode child5 in val5.Children)
		{
			if (!child5.IsNumber)
			{
				throw new SerializationException();
			}
			float num5 = JSONNode.op_Implicit(child5);
			NpcOfferAddRange.Add(num5);
		}
		JSONNode val6 = _buf["npcOfferRate"];
		if (!val6.IsArray)
		{
			throw new SerializationException();
		}
		NpcOfferRate = new List<float>(val6.Count);
		foreach (JSONNode child6 in val6.Children)
		{
			if (!child6.IsNumber)
			{
				throw new SerializationException();
			}
			float num6 = JSONNode.op_Implicit(child6);
			NpcOfferRate.Add(num6);
		}
		JSONNode val7 = _buf["npcQuitOfferRange"];
		if (!val7.IsArray)
		{
			throw new SerializationException();
		}
		NpcQuitOfferRange = new List<float>(val7.Count);
		foreach (JSONNode child7 in val7.Children)
		{
			if (!child7.IsNumber)
			{
				throw new SerializationException();
			}
			float num7 = JSONNode.op_Implicit(child7);
			NpcQuitOfferRange.Add(num7);
		}
		JSONNode val8 = _buf["finalValueBuff"];
		if (!val8.IsArray)
		{
			throw new SerializationException();
		}
		FinalValueBuff = new List<float>(val8.Count);
		foreach (JSONNode child8 in val8.Children)
		{
			if (!child8.IsNumber)
			{
				throw new SerializationException();
			}
			float num8 = JSONNode.op_Implicit(child8);
			FinalValueBuff.Add(num8);
		}
		JSONNode val9 = _buf["raisePriceStage"];
		if (!val9.IsArray)
		{
			throw new SerializationException();
		}
		RaisePriceStage = new List<float>(val9.Count);
		foreach (JSONNode child9 in val9.Children)
		{
			if (!child9.IsNumber)
			{
				throw new SerializationException();
			}
			float num9 = JSONNode.op_Implicit(child9);
			RaisePriceStage.Add(num9);
		}
		JSONNode val10 = _buf["raiseBuffRange"];
		if (!val10.IsArray)
		{
			throw new SerializationException();
		}
		RaiseBuffRange = new List<float>(val10.Count);
		foreach (JSONNode child10 in val10.Children)
		{
			if (!child10.IsNumber)
			{
				throw new SerializationException();
			}
			float num10 = JSONNode.op_Implicit(child10);
			RaiseBuffRange.Add(num10);
		}
		JSONNode val11 = _buf["raisePriceIncomeRange"];
		if (!val11.IsArray)
		{
			throw new SerializationException();
		}
		RaisePriceIncomeRange = new List<float>(val11.Count);
		foreach (JSONNode child11 in val11.Children)
		{
			if (!child11.IsNumber)
			{
				throw new SerializationException();
			}
			float num11 = JSONNode.op_Implicit(child11);
			RaisePriceIncomeRange.Add(num11);
		}
		JSONNode val12 = _buf["resultItemRarity"];
		if (!val12.IsArray)
		{
			throw new SerializationException();
		}
		ResultItemRarity = new List<int>(val12.Count);
		foreach (JSONNode child12 in val12.Children)
		{
			if (!child12.IsNumber)
			{
				throw new SerializationException();
			}
			int num12 = JSONNode.op_Implicit(child12);
			ResultItemRarity.Add(num12);
		}
		JSONNode val13 = _buf["resultItemWeight"];
		if (!val13.IsArray)
		{
			throw new SerializationException();
		}
		ResultItemWeight = new List<int>(val13.Count);
		foreach (JSONNode child13 in val13.Children)
		{
			if (!child13.IsNumber)
			{
				throw new SerializationException();
			}
			int num13 = JSONNode.op_Implicit(child13);
			ResultItemWeight.Add(num13);
		}
		JSONNode val14 = _buf["resultItemPriceWeight"];
		if (!val14.IsArray)
		{
			throw new SerializationException();
		}
		ResultItemPriceWeight = new List<int>(val14.Count);
		foreach (JSONNode child14 in val14.Children)
		{
			if (!child14.IsNumber)
			{
				throw new SerializationException();
			}
			int num14 = JSONNode.op_Implicit(child14);
			ResultItemPriceWeight.Add(num14);
		}
		JSONNode val15 = _buf["giveUpChance1"];
		if (!val15.IsArray)
		{
			throw new SerializationException();
		}
		GiveUpChance1 = new List<float>(val15.Count);
		foreach (JSONNode child15 in val15.Children)
		{
			if (!child15.IsNumber)
			{
				throw new SerializationException();
			}
			float num15 = JSONNode.op_Implicit(child15);
			GiveUpChance1.Add(num15);
		}
		JSONNode val16 = _buf["giveUpChance2"];
		if (!val16.IsArray)
		{
			throw new SerializationException();
		}
		GiveUpChance2 = new List<float>(val16.Count);
		foreach (JSONNode child16 in val16.Children)
		{
			if (!child16.IsNumber)
			{
				throw new SerializationException();
			}
			float num16 = JSONNode.op_Implicit(child16);
			GiveUpChance2.Add(num16);
		}
		JSONNode val17 = _buf["giveUpChance3"];
		if (!val17.IsArray)
		{
			throw new SerializationException();
		}
		GiveUpChance3 = new List<float>(val17.Count);
		foreach (JSONNode child17 in val17.Children)
		{
			if (!child17.IsNumber)
			{
				throw new SerializationException();
			}
			float num17 = JSONNode.op_Implicit(child17);
			GiveUpChance3.Add(num17);
		}
		JSONNode val18 = _buf["giveUpChance4"];
		if (!val18.IsArray)
		{
			throw new SerializationException();
		}
		GiveUpChance4 = new List<float>(val18.Count);
		foreach (JSONNode child18 in val18.Children)
		{
			if (!child18.IsNumber)
			{
				throw new SerializationException();
			}
			float num18 = JSONNode.op_Implicit(child18);
			GiveUpChance4.Add(num18);
		}
		JSONNode val19 = _buf["angerProbability"];
		if (!val19.IsArray)
		{
			throw new SerializationException();
		}
		AngerProbability = new List<float>(val19.Count);
		foreach (JSONNode child19 in val19.Children)
		{
			if (!child19.IsNumber)
			{
				throw new SerializationException();
			}
			float num19 = JSONNode.op_Implicit(child19);
			AngerProbability.Add(num19);
		}
		JSONNode val20 = _buf["angerRound"];
		if (!val20.IsArray)
		{
			throw new SerializationException();
		}
		AngerRound = new List<float>(val20.Count);
		foreach (JSONNode child20 in val20.Children)
		{
			if (!child20.IsNumber)
			{
				throw new SerializationException();
			}
			float num20 = JSONNode.op_Implicit(child20);
			AngerRound.Add(num20);
		}
	}

	public static auction Deserializeauction(JSONNode _buf)
	{
		return new auction(_buf);
	}

	public override int GetTypeId()
	{
		return -661856701;
	}

	public void ResolveRef(Tables tables)
	{
	}

	public override string ToString()
	{
		return "{ auctionHouseID:" + AuctionHouseID + ",auctionName:" + AuctionName + ",conditionLevel:" + ConditionLevel + ",freeTicket:" + FreeTicket + ",timeRecover:" + TimeRecover + ",itemValueMin:" + ItemValueMin + ",itemValueMax:" + ItemValueMax + ",startPriceRange:" + StringUtil.CollectionToString<float>((IEnumerable<float>)StartPriceRange) + ",auctionRoundMin:" + StringUtil.CollectionToString<int>((IEnumerable<int>)AuctionRoundMin) + ",auctionRoundMax:" + StringUtil.CollectionToString<int>((IEnumerable<int>)AuctionRoundMax) + ",expectPriceRange:" + StringUtil.CollectionToString<float>((IEnumerable<float>)ExpectPriceRange) + ",playerOfferAdd1:" + PlayerOfferAdd1 + ",playerOfferAdd2:" + PlayerOfferAdd2 + ",playerOfferAdd3:" + PlayerOfferAdd3 + ",quitOutTime:" + QuitOutTime + ",npcOfferAddRange:" + StringUtil.CollectionToString<float>((IEnumerable<float>)NpcOfferAddRange) + ",npcOfferRate:" + StringUtil.CollectionToString<float>((IEnumerable<float>)NpcOfferRate) + ",npcQuitOfferRange:" + StringUtil.CollectionToString<float>((IEnumerable<float>)NpcQuitOfferRange) + ",finalValueBuff:" + StringUtil.CollectionToString<float>((IEnumerable<float>)FinalValueBuff) + ",raisePriceStage:" + StringUtil.CollectionToString<float>((IEnumerable<float>)RaisePriceStage) + ",raiseBuffRange:" + StringUtil.CollectionToString<float>((IEnumerable<float>)RaiseBuffRange) + ",raisePriceIncomeRange:" + StringUtil.CollectionToString<float>((IEnumerable<float>)RaisePriceIncomeRange) + ",resultItemRarity:" + StringUtil.CollectionToString<int>((IEnumerable<int>)ResultItemRarity) + ",resultItemWeight:" + StringUtil.CollectionToString<int>((IEnumerable<int>)ResultItemWeight) + ",resultItemPriceWeight:" + StringUtil.CollectionToString<int>((IEnumerable<int>)ResultItemPriceWeight) + ",giveUpChance1:" + StringUtil.CollectionToString<float>((IEnumerable<float>)GiveUpChance1) + ",giveUpChance2:" + StringUtil.CollectionToString<float>((IEnumerable<float>)GiveUpChance2) + ",giveUpChance3:" + StringUtil.CollectionToString<float>((IEnumerable<float>)GiveUpChance3) + ",giveUpChance4:" + StringUtil.CollectionToString<float>((IEnumerable<float>)GiveUpChance4) + ",angerProbability:" + StringUtil.CollectionToString<float>((IEnumerable<float>)AngerProbability) + ",angerRound:" + StringUtil.CollectionToString<float>((IEnumerable<float>)AngerRound) + ",}";
	}
}
