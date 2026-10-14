using Luban;
using SimpleJSON;

namespace cfg;

public sealed class desk : BeanBase
{
	public readonly int Id;

	public readonly int WorldId;

	public readonly string DeskName;

	public readonly string DeskDesc;

	public readonly float SkillNum;

	public readonly float SkillUpgradeBuff;

	public readonly int SkillLevelMax;

	public readonly int BuyType;

	public readonly double BuyNum;

	public readonly float BuyNumCostBuff;

	public readonly int DeskUnlockCondition;

	public const int __ID__ = 3079833;

	public desk(JSONNode _buf)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		if (!_buf["id"].IsNumber)
		{
			throw new SerializationException();
		}
		Id = JSONNode.op_Implicit(_buf["id"]);
		if (!_buf["WorldId"].IsNumber)
		{
			throw new SerializationException();
		}
		WorldId = JSONNode.op_Implicit(_buf["WorldId"]);
		if (!_buf["deskName"].IsString)
		{
			throw new SerializationException();
		}
		DeskName = JSONNode.op_Implicit(_buf["deskName"]);
		if (!_buf["deskDesc"].IsString)
		{
			throw new SerializationException();
		}
		DeskDesc = JSONNode.op_Implicit(_buf["deskDesc"]);
		if (!_buf["skillNum"].IsNumber)
		{
			throw new SerializationException();
		}
		SkillNum = JSONNode.op_Implicit(_buf["skillNum"]);
		if (!_buf["skillUpgradeBuff"].IsNumber)
		{
			throw new SerializationException();
		}
		SkillUpgradeBuff = JSONNode.op_Implicit(_buf["skillUpgradeBuff"]);
		if (!_buf["skillLevelMax"].IsNumber)
		{
			throw new SerializationException();
		}
		SkillLevelMax = JSONNode.op_Implicit(_buf["skillLevelMax"]);
		if (!_buf["buyType"].IsNumber)
		{
			throw new SerializationException();
		}
		BuyType = JSONNode.op_Implicit(_buf["buyType"]);
		if (!_buf["buyNum"].IsNumber)
		{
			throw new SerializationException();
		}
		BuyNum = JSONNode.op_Implicit(_buf["buyNum"]);
		if (!_buf["buyNumCostBuff"].IsNumber)
		{
			throw new SerializationException();
		}
		BuyNumCostBuff = JSONNode.op_Implicit(_buf["buyNumCostBuff"]);
		if (!_buf["deskUnlockCondition"].IsNumber)
		{
			throw new SerializationException();
		}
		DeskUnlockCondition = JSONNode.op_Implicit(_buf["deskUnlockCondition"]);
	}

	public static desk Deserializedesk(JSONNode _buf)
	{
		return new desk(_buf);
	}

	public override int GetTypeId()
	{
		return 3079833;
	}

	public void ResolveRef(Tables tables)
	{
	}

	public override string ToString()
	{
		return "{ id:" + Id + ",WorldId:" + WorldId + ",deskName:" + DeskName + ",deskDesc:" + DeskDesc + ",skillNum:" + SkillNum + ",skillUpgradeBuff:" + SkillUpgradeBuff + ",skillLevelMax:" + SkillLevelMax + ",buyType:" + BuyType + ",buyNum:" + BuyNum + ",buyNumCostBuff:" + BuyNumCostBuff + ",deskUnlockCondition:" + DeskUnlockCondition + ",}";
	}
}
