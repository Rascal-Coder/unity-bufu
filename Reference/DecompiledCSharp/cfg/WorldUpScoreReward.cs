using Luban;
using SimpleJSON;

namespace cfg;

public sealed class WorldUpScoreReward : BeanBase
{
	public readonly int Id;

	public readonly string ExchangRewardId;

	public readonly int RewardNum;

	public readonly int ScoreProgress;

	public const int __ID__ = -1482331020;

	public WorldUpScoreReward(JSONNode _buf)
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
		if (!_buf["exchangRewardId"].IsString)
		{
			throw new SerializationException();
		}
		ExchangRewardId = JSONNode.op_Implicit(_buf["exchangRewardId"]);
		if (!_buf["rewardNum"].IsNumber)
		{
			throw new SerializationException();
		}
		RewardNum = JSONNode.op_Implicit(_buf["rewardNum"]);
		if (!_buf["scoreProgress"].IsNumber)
		{
			throw new SerializationException();
		}
		ScoreProgress = JSONNode.op_Implicit(_buf["scoreProgress"]);
	}

	public static WorldUpScoreReward DeserializeWorldUpScoreReward(JSONNode _buf)
	{
		return new WorldUpScoreReward(_buf);
	}

	public override int GetTypeId()
	{
		return -1482331020;
	}

	public void ResolveRef(Tables tables)
	{
	}

	public override string ToString()
	{
		return "{ id:" + Id + ",exchangRewardId:" + ExchangRewardId + ",rewardNum:" + RewardNum + ",scoreProgress:" + ScoreProgress + ",}";
	}
}
