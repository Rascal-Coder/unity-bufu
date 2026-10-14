using Luban;
using SimpleJSON;

namespace cfg;

public sealed class language : BeanBase
{
	public readonly int Id;

	public readonly string Chinese;

	public readonly string English;

	public const int __ID__ = -1613589672;

	public language(JSONNode _buf)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		if (!_buf["id"].IsNumber)
		{
			throw new SerializationException();
		}
		Id = JSONNode.op_Implicit(_buf["id"]);
		if (!_buf["chinese"].IsString)
		{
			throw new SerializationException();
		}
		Chinese = JSONNode.op_Implicit(_buf["chinese"]);
		if (!_buf["english"].IsString)
		{
			throw new SerializationException();
		}
		English = JSONNode.op_Implicit(_buf["english"]);
	}

	public static language Deserializelanguage(JSONNode _buf)
	{
		return new language(_buf);
	}

	public override int GetTypeId()
	{
		return -1613589672;
	}

	public void ResolveRef(Tables tables)
	{
	}

	public override string ToString()
	{
		return "{ id:" + Id + ",chinese:" + Chinese + ",english:" + English + ",}";
	}
}
