using Luban;
using SimpleJSON;

namespace cfg;

public sealed class limitMission2 : BeanBase
{
	public readonly int Id;

	public readonly double LimitMissionCondition;

	public readonly int LimitTIme;

	public readonly float LimitGoalFactor;

	public readonly int RewardType;

	public readonly double RewardNum;

	public const int __ID__ = 1234269441;

	public limitMission2(JSONNode _buf)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		if (!_buf["id"].IsNumber)
		{
			throw new SerializationException();
		}
		Id = JSONNode.op_Implicit(_buf["id"]);
		if (!_buf["limitMissionCondition"].IsNumber)
		{
			throw new SerializationException();
		}
		LimitMissionCondition = JSONNode.op_Implicit(_buf["limitMissionCondition"]);
		if (!_buf["limitTIme"].IsNumber)
		{
			throw new SerializationException();
		}
		LimitTIme = JSONNode.op_Implicit(_buf["limitTIme"]);
		if (!_buf["limitGoalFactor"].IsNumber)
		{
			throw new SerializationException();
		}
		LimitGoalFactor = JSONNode.op_Implicit(_buf["limitGoalFactor"]);
		if (!_buf["rewardType"].IsNumber)
		{
			throw new SerializationException();
		}
		RewardType = JSONNode.op_Implicit(_buf["rewardType"]);
		if (!_buf["rewardNum"].IsNumber)
		{
			throw new SerializationException();
		}
		RewardNum = JSONNode.op_Implicit(_buf["rewardNum"]);
	}

	public static limitMission2 DeserializelimitMission2(JSONNode _buf)
	{
		return new limitMission2(_buf);
	}

	public override int GetTypeId()
	{
		return 1234269441;
	}

	public void ResolveRef(Tables tables)
	{
	}

	public override string ToString()
	{
		return "{ id:" + Id + ",limitMissionCondition:" + LimitMissionCondition + ",limitTIme:" + LimitTIme + ",limitGoalFactor:" + LimitGoalFactor + ",rewardType:" + RewardType + ",rewardNum:" + RewardNum + ",}";
	}
}
