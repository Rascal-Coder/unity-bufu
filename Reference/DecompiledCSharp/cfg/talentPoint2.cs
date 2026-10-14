using Luban;
using SimpleJSON;

namespace cfg;

public sealed class talentPoint2 : BeanBase
{
	public readonly int Id;

	public readonly double TalentName;

	public readonly string TalentDesc;

	public readonly float RebornBuff;

	public const int __ID__ = -1626892114;

	public talentPoint2(JSONNode _buf)
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
		if (!_buf["talentName"].IsNumber)
		{
			throw new SerializationException();
		}
		TalentName = JSONNode.op_Implicit(_buf["talentName"]);
		if (!_buf["talentDesc"].IsString)
		{
			throw new SerializationException();
		}
		TalentDesc = JSONNode.op_Implicit(_buf["talentDesc"]);
		if (!_buf["rebornBuff"].IsNumber)
		{
			throw new SerializationException();
		}
		RebornBuff = JSONNode.op_Implicit(_buf["rebornBuff"]);
	}

	public static talentPoint2 DeserializetalentPoint2(JSONNode _buf)
	{
		return new talentPoint2(_buf);
	}

	public override int GetTypeId()
	{
		return -1626892114;
	}

	public void ResolveRef(Tables tables)
	{
	}

	public override string ToString()
	{
		return "{ id:" + Id + ",talentName:" + TalentName + ",talentDesc:" + TalentDesc + ",rebornBuff:" + RebornBuff + ",}";
	}
}
