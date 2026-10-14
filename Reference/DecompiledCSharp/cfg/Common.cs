using Luban;
using SimpleJSON;

namespace cfg;

public sealed class Common : BeanBase
{
	public readonly int Id;

	public readonly string ItemDesc;

	public readonly float Factor;

	public const int __ID__ = 2024019467;

	public Common(JSONNode _buf)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		if (!_buf["id"].IsNumber)
		{
			throw new SerializationException();
		}
		Id = JSONNode.op_Implicit(_buf["id"]);
		if (!_buf["itemDesc"].IsString)
		{
			throw new SerializationException();
		}
		ItemDesc = JSONNode.op_Implicit(_buf["itemDesc"]);
		if (!_buf["factor"].IsNumber)
		{
			throw new SerializationException();
		}
		Factor = JSONNode.op_Implicit(_buf["factor"]);
	}

	public static Common DeserializeCommon(JSONNode _buf)
	{
		return new Common(_buf);
	}

	public override int GetTypeId()
	{
		return 2024019467;
	}

	public void ResolveRef(Tables tables)
	{
	}

	public override string ToString()
	{
		return "{ id:" + Id + ",itemDesc:" + ItemDesc + ",factor:" + Factor + ",}";
	}
}
