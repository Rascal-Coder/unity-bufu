using Luban;
using SimpleJSON;

namespace cfg;

public sealed class mainMission : BeanBase
{
	public readonly int Id;

	public readonly int WorldId;

	public readonly double MainMissionNum;

	public readonly string TitleName;

	public readonly int RewardType;

	public readonly double RewardNum;

	public readonly int PicId;

	public const int __ID__ = 184410515;

	public mainMission(JSONNode _buf)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		if (!_buf["id"].IsNumber)
		{
			throw new SerializationException();
		}
		Id = JSONNode.op_Implicit(_buf["id"]);
		if (!_buf["worldId"].IsNumber)
		{
			throw new SerializationException();
		}
		WorldId = JSONNode.op_Implicit(_buf["worldId"]);
		if (!_buf["mainMissionNum"].IsNumber)
		{
			throw new SerializationException();
		}
		MainMissionNum = JSONNode.op_Implicit(_buf["mainMissionNum"]);
		if (!_buf["titleName"].IsString)
		{
			throw new SerializationException();
		}
		TitleName = JSONNode.op_Implicit(_buf["titleName"]);
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
		if (!_buf["picId"].IsNumber)
		{
			throw new SerializationException();
		}
		PicId = JSONNode.op_Implicit(_buf["picId"]);
	}

	public static mainMission DeserializemainMission(JSONNode _buf)
	{
		return new mainMission(_buf);
	}

	public override int GetTypeId()
	{
		return 184410515;
	}

	public void ResolveRef(Tables tables)
	{
	}

	public override string ToString()
	{
		return "{ id:" + Id + ",worldId:" + WorldId + ",mainMissionNum:" + MainMissionNum + ",titleName:" + TitleName + ",rewardType:" + RewardType + ",rewardNum:" + RewardNum + ",picId:" + PicId + ",}";
	}
}
