using Luban;
using SimpleJSON;

namespace cfg;

public struct vector2
{
	public readonly float X;

	public readonly float Y;

	public vector2(JSONNode _buf)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		if (!_buf["x"].IsNumber)
		{
			throw new SerializationException();
		}
		X = JSONNode.op_Implicit(_buf["x"]);
		if (!_buf["y"].IsNumber)
		{
			throw new SerializationException();
		}
		Y = JSONNode.op_Implicit(_buf["y"]);
	}

	public static vector2 Deserializevector2(JSONNode _buf)
	{
		return new vector2(_buf);
	}

	public void ResolveRef(Tables tables)
	{
	}

	public override string ToString()
	{
		return "{ x:" + X + ",y:" + Y + ",}";
	}
}
