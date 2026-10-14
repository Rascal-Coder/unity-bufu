using System.Collections.Generic;
using UnityEngine;

public class UIStaggerGroup : MonoBehaviour
{
	[Header("1. Stagger Settings (错帧设置)")]
	[Tooltip("每个子物体弹出的时间间隔（秒）。推荐0.05到0.1之间，太长会显得拖沓")]
	public float interval = 0.08f;

	[Tooltip("退场时是否倒序播放？（开启后，底部的物体先消失，顶部的后消失，非常高级）")]
	public bool reverseExit = true;

	[Header("2. Target Animators (成员列表)")]
	[Tooltip("需要排队的子物体。留空的话，右键菜单或运行一瞬间会自动抓取下方所有挂了组件的物体")]
	public List<UIAnimatorElite> childAnimators = new List<UIAnimatorElite>();

	[ContextMenu("⚡ 自动抓取子物体")]
	private void Reset()
	{
		childAnimators = new List<UIAnimatorElite>(((Component)this).GetComponentsInChildren<UIAnimatorElite>(true));
		foreach (UIAnimatorElite childAnimator in childAnimators)
		{
			if ((Object)(object)childAnimator != (Object)null)
			{
				childAnimator.autoPlayOnEnable = false;
			}
		}
	}

	private void OnEnable()
	{
		PlayGroupEnter();
	}

	public void PlayGroupEnter()
	{
		if (childAnimators.Count == 0)
		{
			Reset();
		}
		for (int i = 0; i < childAnimators.Count; i++)
		{
			if (!((Object)(object)childAnimators[i] == (Object)null))
			{
				float customDelay = (float)i * interval;
				childAnimators[i].PlayEnter(customDelay);
			}
		}
	}

	public void PlayGroupExit()
	{
		if (childAnimators.Count == 0)
		{
			Reset();
		}
		int count = childAnimators.Count;
		for (int i = 0; i < count; i++)
		{
			int index = (reverseExit ? (count - 1 - i) : i);
			if (!((Object)(object)childAnimators[index] == (Object)null))
			{
				float customDelay = (float)i * interval;
				childAnimators[index].PlayExit(customDelay);
			}
		}
	}
}
