using Luban;
using SimpleJSON;

namespace cfg;

public sealed class world : BeanBase
{
	public readonly int Id;

	public readonly string WorldName;

	public readonly int Sort;

	public readonly int IsOpen;

	public readonly int PreWorldId;

	public readonly int RequiredPreWorldLevel;

	public readonly int EntryUnlockLevel;

	public readonly int GameplayType;

	public readonly string SceneAssetPath;

	public readonly bool RankEnabled;

	public readonly int HelperUpgradeId;

	public readonly int HelperSpeedUpgradeId;

	public readonly float HelperRollInterval;

	public const int __ID__ = 113318802;

	public world(JSONNode _buf)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_0240: Unknown result type (might be due to invalid IL or missing references)
		if (!_buf["id"].IsNumber)
		{
			throw new SerializationException();
		}
		Id = JSONNode.op_Implicit(_buf["id"]);
		if (!_buf["worldName"].IsString)
		{
			throw new SerializationException();
		}
		WorldName = JSONNode.op_Implicit(_buf["worldName"]);
		if (!_buf["sort"].IsNumber)
		{
			throw new SerializationException();
		}
		Sort = JSONNode.op_Implicit(_buf["sort"]);
		if (!_buf["isOpen"].IsNumber)
		{
			throw new SerializationException();
		}
		IsOpen = JSONNode.op_Implicit(_buf["isOpen"]);
		if (!_buf["preWorldId"].IsNumber)
		{
			throw new SerializationException();
		}
		PreWorldId = JSONNode.op_Implicit(_buf["preWorldId"]);
		if (!_buf["requiredPreWorldLevel"].IsNumber)
		{
			throw new SerializationException();
		}
		RequiredPreWorldLevel = JSONNode.op_Implicit(_buf["requiredPreWorldLevel"]);
		if (!_buf["entryUnlockLevel"].IsNumber)
		{
			throw new SerializationException();
		}
		EntryUnlockLevel = JSONNode.op_Implicit(_buf["entryUnlockLevel"]);
		if (!_buf["gameplayType"].IsNumber)
		{
			throw new SerializationException();
		}
		GameplayType = JSONNode.op_Implicit(_buf["gameplayType"]);
		if (!_buf["sceneAssetPath"].IsString)
		{
			throw new SerializationException();
		}
		SceneAssetPath = JSONNode.op_Implicit(_buf["sceneAssetPath"]);
		if (!_buf["rankEnabled"].IsBoolean)
		{
			throw new SerializationException();
		}
		RankEnabled = JSONNode.op_Implicit(_buf["rankEnabled"]);
		if (!_buf["helperUpgradeId"].IsNumber)
		{
			throw new SerializationException();
		}
		HelperUpgradeId = JSONNode.op_Implicit(_buf["helperUpgradeId"]);
		if (!_buf["helperSpeedUpgradeId"].IsNumber)
		{
			throw new SerializationException();
		}
		HelperSpeedUpgradeId = JSONNode.op_Implicit(_buf["helperSpeedUpgradeId"]);
		if (!_buf["helperRollInterval"].IsNumber)
		{
			throw new SerializationException();
		}
		HelperRollInterval = JSONNode.op_Implicit(_buf["helperRollInterval"]);
	}

	public static world Deserializeworld(JSONNode _buf)
	{
		return new world(_buf);
	}

	public override int GetTypeId()
	{
		return 113318802;
	}

	public void ResolveRef(Tables tables)
	{
	}

	public override string ToString()
	{
		return "{ id:" + Id + ",worldName:" + WorldName + ",sort:" + Sort + ",isOpen:" + IsOpen + ",preWorldId:" + PreWorldId + ",requiredPreWorldLevel:" + RequiredPreWorldLevel + ",entryUnlockLevel:" + EntryUnlockLevel + ",gameplayType:" + GameplayType + ",sceneAssetPath:" + SceneAssetPath + ",rankEnabled:" + RankEnabled + ",helperUpgradeId:" + HelperUpgradeId + ",helperSpeedUpgradeId:" + HelperSpeedUpgradeId + ",helperRollInterval:" + HelperRollInterval + ",}";
	}
}
