using Luban;
using SimpleJSON;

namespace cfg;

public sealed class item : BeanBase
{
	public readonly int Id;

	public readonly string Name;

	public readonly string Desc;

	public readonly int Count;

	public const int __ID__ = 3242771;

	public item(JSONNode _buf)
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
		if (!_buf["name"].IsString)
		{
			throw new SerializationException();
		}
		Name = JSONNode.op_Implicit(_buf["name"]);
		if (!_buf["desc"].IsString)
		{
			throw new SerializationException();
		}
		Desc = JSONNode.op_Implicit(_buf["desc"]);
		if (!_buf["count"].IsNumber)
		{
			throw new SerializationException();
		}
		Count = JSONNode.op_Implicit(_buf["count"]);
	}

	public static item Deserializeitem(JSONNode _buf)
	{
		return new item(_buf);
	}

	public override int GetTypeId()
	{
		return 3242771;
	}

	public void ResolveRef(Tables tables)
	{
	}

	public override string ToString()
	{
		return "{ id:" + Id + ",name:" + Name + ",desc:" + Desc + ",count:" + Count + ",}";
	}
}
