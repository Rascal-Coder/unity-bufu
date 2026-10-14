using Luban;
using SimpleJSON;

namespace cfg;

public struct vector3
{
	public readonly float X;

	public readonly float Y;

	public readonly float Z;

	public vector3(JSONNode _buf)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
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
	}

	public static vector3 Deserializevector3(JSONNode _buf)
	{
		return new vector3(_buf);
	}

	public void ResolveRef(Tables tables)
	{
	}

	public override string ToString()
	{
		return "{ x:" + X + ",y:" + Y + ",z:" + Z + ",}";
	}
}
