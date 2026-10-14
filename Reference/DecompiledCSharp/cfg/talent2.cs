using System.Collections.Generic;
using Luban;
using SimpleJSON;

namespace cfg;

public sealed class talent2 : BeanBase
{
	public readonly int Id;

	public readonly int TalentType;

	public readonly string TalentName;

	public readonly string TalentDesc;

	public readonly int Position;

	public readonly int BaseTalentPoint;

	public readonly int CostPointAdd;

	public readonly List<float> UpgradeResult;

	public readonly int UpgradeLevelMax;

	public const int __ID__ = -1543869242;

	public talent2(JSONNode _buf)
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
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		if (!_buf["id"].IsNumber)
		{
			throw new SerializationException();
		}
		Id = JSONNode.op_Implicit(_buf["id"]);
		if (!_buf["talentType"].IsNumber)
		{
			throw new SerializationException();
		}
		TalentType = JSONNode.op_Implicit(_buf["talentType"]);
		if (!_buf["talentName"].IsString)
		{
			throw new SerializationException();
		}
		TalentName = JSONNode.op_Implicit(_buf["talentName"]);
		if (!_buf["talentDesc"].IsString)
		{
			throw new SerializationException();
		}
		TalentDesc = JSONNode.op_Implicit(_buf["talentDesc"]);
		if (!_buf["position"].IsNumber)
		{
			throw new SerializationException();
		}
		Position = JSONNode.op_Implicit(_buf["position"]);
		if (!_buf["baseTalentPoint"].IsNumber)
		{
			throw new SerializationException();
		}
		BaseTalentPoint = JSONNode.op_Implicit(_buf["baseTalentPoint"]);
		if (!_buf["costPointAdd"].IsNumber)
		{
			throw new SerializationException();
		}
		CostPointAdd = JSONNode.op_Implicit(_buf["costPointAdd"]);
		JSONNode val = _buf["upgradeResult"];
		if (!val.IsArray)
		{
			throw new SerializationException();
		}
		UpgradeResult = new List<float>(val.Count);
		foreach (JSONNode child in val.Children)
		{
			if (!child.IsNumber)
			{
				throw new SerializationException();
			}
			float num = JSONNode.op_Implicit(child);
			UpgradeResult.Add(num);
		}
		if (!_buf["upgradeLevelMax"].IsNumber)
		{
			throw new SerializationException();
		}
		UpgradeLevelMax = JSONNode.op_Implicit(_buf["upgradeLevelMax"]);
	}

	public static talent2 Deserializetalent2(JSONNode _buf)
	{
		return new talent2(_buf);
	}

	public override int GetTypeId()
	{
		return -1543869242;
	}

	public void ResolveRef(Tables tables)
	{
	}

	public override string ToString()
	{
		return "{ id:" + Id + ",talentType:" + TalentType + ",talentName:" + TalentName + ",talentDesc:" + TalentDesc + ",position:" + Position + ",baseTalentPoint:" + BaseTalentPoint + ",costPointAdd:" + CostPointAdd + ",upgradeResult:" + StringUtil.CollectionToString<float>((IEnumerable<float>)UpgradeResult) + ",upgradeLevelMax:" + UpgradeLevelMax + ",}";
	}
}
