using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TalentData
{
	public int IsNeedResetTalent;

	public int talentPoint;

	public int rebornCurrentLevel = -1;

	public int[] costTalentPoint = new int[3];

	public List<int> talentLevel = new List<int>();

	public List<bool> talentUnlock = new List<bool>();

	public float soundSleep;

	public float goldAge;

	public float laodongli = 1f;

	public int diceTalentPoint;

	public int diceRebornCurrentLevel = -1;

	public int[] diceCostTalentPoint = new int[3];

	public List<int> diceTalentLevel = new List<int>();

	public List<bool> diceTalentUnlock = new List<bool>();

	public WorldCupCompactData worldCupCompactData = new WorldCupCompactData();

	public TalentData()
	{
		Debug.Log((object)"天赋被初始化");
	}
}
