using System;
using System.Collections.Generic;

[Serializable]
public sealed class DiceWorldProgressData
{
	public float valueMultiplier = 1f;

	public List<float> faceWeightMultipliers = new List<float> { 1f, 1f, 1f, 1f, 1f, 1f };

	public void Normalize()
	{
		valueMultiplier = Math.Max(0f, valueMultiplier);
		if (faceWeightMultipliers == null)
		{
			faceWeightMultipliers = new List<float>();
		}
		while (faceWeightMultipliers.Count < 6)
		{
			faceWeightMultipliers.Add(1f);
		}
		if (faceWeightMultipliers.Count > 6)
		{
			faceWeightMultipliers.RemoveRange(6, faceWeightMultipliers.Count - 6);
		}
		for (int i = 0; i < faceWeightMultipliers.Count; i++)
		{
			faceWeightMultipliers[i] = Math.Max(0f, faceWeightMultipliers[i]);
		}
	}
}
