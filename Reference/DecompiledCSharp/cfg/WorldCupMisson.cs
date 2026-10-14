using Luban;
using SimpleJSON;

namespace cfg;

public sealed class WorldCupMisson : BeanBase
{
	public readonly int Id;

	public readonly string MissionName;

	public readonly int MissionNum;

	public readonly int MissionReward;

	public const int __ID__ = -848961433;

	public WorldCupMisson(JSONNode _buf)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		if (!_buf["id"].IsNumber)
		{
			throw new SerializationException();
		}
		Id = JSONNode.op_Implicit(_buf["id"]);
		if (!_buf["missionName"].IsString)
		{
			throw new SerializationException();
		}
		MissionName = JSONNode.op_Implicit(_buf["missionName"]);
		if (!_buf["missionNum"].IsNumber)
		{
			throw new SerializationException();
		}
		MissionNum = JSONNode.op_Implicit(_buf["missionNum"]);
		if (!_buf["missionReward"].IsNumber)
		{
			throw new SerializationException();
		}
		MissionReward = JSONNode.op_Implicit(_buf["missionReward"]);
	}

	public static WorldCupMisson DeserializeWorldCupMisson(JSONNode _buf)
	{
		return new WorldCupMisson(_buf);
	}

	public override int GetTypeId()
	{
		return -848961433;
	}

	public void ResolveRef(Tables tables)
	{
	}

	public override string ToString()
	{
		return "{ id:" + Id + ",missionName:" + MissionName + ",missionNum:" + MissionNum + ",missionReward:" + MissionReward + ",}";
	}
}
