using System;
using System.Collections.Generic;
using Spine;
using Spine.Unity;
using UnityEngine;

public static class SpineSkinHelper
{
	public struct SkinAttachmentInfo
	{
		public string attachmentName;

		public string slotName;

		public int slotIndex;

		public Attachment attachment;
	}

	public static Dictionary<string, List<SkinAttachmentInfo>> GetAllSkinsWithAttachments(SkeletonAnimation skeletonAnimation)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)skeletonAnimation == (Object)null || ((SkeletonRenderer)skeletonAnimation).Skeleton == null)
		{
			return null;
		}
		Dictionary<string, List<SkinAttachmentInfo>> dictionary = new Dictionary<string, List<SkinAttachmentInfo>>();
		SkeletonData data = ((SkeletonRenderer)skeletonAnimation).Skeleton.Data;
		Enumerator<Skin> enumerator = data.Skins.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				Skin current = enumerator.Current;
				List<SkinAttachmentInfo> skinAttachmentsWithSlotInfo = GetSkinAttachmentsWithSlotInfo(current, data);
				dictionary.Add(current.Name, skinAttachmentsWithSlotInfo);
			}
			return dictionary;
		}
		finally
		{
			((IDisposable)enumerator/*cast due to .constrained prefix*/).Dispose();
		}
	}

	public static List<SkinAttachmentInfo> GetSkinAttachmentsWithSlotInfo(Skin skin, SkeletonData skeletonData)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		List<SkinAttachmentInfo> list = new List<SkinAttachmentInfo>();
		foreach (KeyValuePair<SkinEntry, Attachment> attachment in skin.Attachments)
		{
			SkinEntry key = attachment.Key;
			Attachment value = attachment.Value;
			string name = skeletonData.Slots.Items[((SkinEntry)(ref key)).SlotIndex].Name;
			SkinAttachmentInfo item = new SkinAttachmentInfo
			{
				attachmentName = ((SkinEntry)(ref key)).Name,
				slotName = name,
				slotIndex = ((SkinEntry)(ref key)).SlotIndex,
				attachment = value
			};
			list.Add(item);
		}
		return list;
	}

	public static List<SkinAttachmentInfo> GetAttachmentsByPartName(Skin skin, SkeletonData skeletonData, string partName)
	{
		List<SkinAttachmentInfo> skinAttachmentsWithSlotInfo = GetSkinAttachmentsWithSlotInfo(skin, skeletonData);
		List<SkinAttachmentInfo> list = new List<SkinAttachmentInfo>();
		foreach (SkinAttachmentInfo item in skinAttachmentsWithSlotInfo)
		{
			if (item.slotName.Contains(partName) || item.attachmentName.Contains(partName))
			{
				list.Add(item);
			}
		}
		return list;
	}

	public static void PrintSkinDetails(SkeletonAnimation skeletonAnimation)
	{
		if ((Object)(object)skeletonAnimation == (Object)null || ((SkeletonRenderer)skeletonAnimation).Skeleton == null)
		{
			return;
		}
		Debug.Log((object)"=== Spine皮肤详细信息 ===");
		foreach (KeyValuePair<string, List<SkinAttachmentInfo>> allSkinsWithAttachment in GetAllSkinsWithAttachments(skeletonAnimation))
		{
			string key = allSkinsWithAttachment.Key;
			List<SkinAttachmentInfo> value = allSkinsWithAttachment.Value;
			Debug.Log((object)$"\n皮肤: {key}，包含 {value.Count} 个附件:");
			Dictionary<string, List<SkinAttachmentInfo>> dictionary = new Dictionary<string, List<SkinAttachmentInfo>>();
			foreach (SkinAttachmentInfo item in value)
			{
				if (!dictionary.ContainsKey(item.slotName))
				{
					dictionary[item.slotName] = new List<SkinAttachmentInfo>();
				}
				dictionary[item.slotName].Add(item);
			}
			foreach (KeyValuePair<string, List<SkinAttachmentInfo>> item2 in dictionary)
			{
				string key2 = item2.Key;
				List<SkinAttachmentInfo> value2 = item2.Value;
				string text = string.Join(", ", value2.ConvertAll((SkinAttachmentInfo a) => a.attachmentName));
				Debug.Log((object)("  部位 " + key2 + ": " + text));
			}
		}
		Debug.Log((object)"\n=== 皮肤信息打印完成 ===");
	}
}
