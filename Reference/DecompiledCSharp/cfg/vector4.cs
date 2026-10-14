using Luban;
using SimpleJSON;

namespace cfg;

public struct vector4
{
	public readonly float X;

	public readonly float Y;

	public readonly float Z;

	public readonly float W;

	public vector4(JSONNode _buf)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
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
		if (!_buf["z"].IsNumber)
		{
			throw new SerializationException();
		}
		Z = JSONNode.op_Implicit(_buf["z"]);
		if (!_buf["w"].IsNumber)
		{
			throw new SerializationException();
		}
		W = JSONNode.op_Implicit(_buf["w"]);
	}

	public static vector4 Deserializevector4(JSONNode _buf)
	{
		return new vector4(_buf);
	}

	public void ResolveRef(Tables tables)
	{
	}

	public override string ToString()
	{
		return "{ x:" + X + ",y:" + Y + ",z:" + Z + ",w:" + W + ",}";
	}
}
