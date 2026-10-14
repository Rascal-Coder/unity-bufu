using System.Collections.Generic;
using Module.Role;
using UnityEngine;

public sealed class VatSpatialIndex
{
	private readonly Dictionary<Vector2Int, List<CoinVat>> cells = new Dictionary<Vector2Int, List<CoinVat>>();

	private readonly List<Vector2Int> usedCells = new List<Vector2Int>();

	private float cellSize = 1f;

	private int indexedCount;

	public float CellSize => cellSize;

	public int IndexedCount => indexedCount;

	public void Rebuild(IReadOnlyList<CoinVat> coins, float newCellSize)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		ClearCells();
		cellSize = Mathf.Max(0.0001f, newCellSize);
		indexedCount = 0;
		if (coins == null)
		{
			return;
		}
		for (int i = 0; i < coins.Count; i++)
		{
			CoinVat coinVat = coins[i];
			if (!((Object)(object)coinVat == (Object)null) && coinVat.State == CoinState.Idle)
			{
				Vector2Int val = ToCell(Vector2.op_Implicit(((Component)coinVat).transform.position));
				if (!cells.TryGetValue(val, out var value))
				{
					value = new List<CoinVat>();
					cells.Add(val, value);
				}
				if (value.Count == 0)
				{
					usedCells.Add(val);
				}
				value.Add(coinVat);
				indexedCount++;
			}
		}
	}

	public void Query(Vector2 center, float radius, List<CoinVat> results)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		if (results == null)
		{
			return;
		}
		results.Clear();
		float num = Mathf.Max(0f, radius);
		float num2 = num * num;
		Vector2Int val = ToCell(center - Vector2.one * num);
		Vector2Int val2 = ToCell(center + Vector2.one * num);
		for (int i = ((Vector2Int)(ref val)).x; i <= ((Vector2Int)(ref val2)).x; i++)
		{
			for (int j = ((Vector2Int)(ref val)).y; j <= ((Vector2Int)(ref val2)).y; j++)
			{
				if (!cells.TryGetValue(new Vector2Int(i, j), out var value))
				{
					continue;
				}
				for (int k = 0; k < value.Count; k++)
				{
					CoinVat coinVat = value[k];
					if (!((Object)(object)coinVat == (Object)null) && coinVat.State == CoinState.Idle)
					{
						Vector2 val3 = Vector2.op_Implicit(((Component)coinVat).transform.position) - center;
						if (((Vector2)(ref val3)).sqrMagnitude <= num2)
						{
							results.Add(coinVat);
						}
					}
				}
			}
		}
	}

	public CoinVat FindNearest(Vector2 center, float radius)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Max(0f, radius);
		float num2 = num * num;
		CoinVat result = null;
		Vector2Int val = ToCell(center - Vector2.one * num);
		Vector2Int val2 = ToCell(center + Vector2.one * num);
		for (int i = ((Vector2Int)(ref val)).x; i <= ((Vector2Int)(ref val2)).x; i++)
		{
			for (int j = ((Vector2Int)(ref val)).y; j <= ((Vector2Int)(ref val2)).y; j++)
			{
				if (!cells.TryGetValue(new Vector2Int(i, j), out var value))
				{
					continue;
				}
				for (int k = 0; k < value.Count; k++)
				{
					CoinVat coinVat = value[k];
					if (!((Object)(object)coinVat == (Object)null) && coinVat.State == CoinState.Idle)
					{
						Vector2 val3 = Vector2.op_Implicit(((Component)coinVat).transform.position) - center;
						float sqrMagnitude = ((Vector2)(ref val3)).sqrMagnitude;
						if (sqrMagnitude <= num2)
						{
							num2 = sqrMagnitude;
							result = coinVat;
						}
					}
				}
			}
		}
		return result;
	}

	public Vector2 GetMostDensePosition()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		Vector2 result = Vector2.zero;
		for (int i = 0; i < usedCells.Count; i++)
		{
			if (!cells.TryGetValue(usedCells[i], out var value) || value.Count <= num)
			{
				continue;
			}
			Vector2 val = Vector2.zero;
			int num2 = 0;
			for (int j = 0; j < value.Count; j++)
			{
				CoinVat coinVat = value[j];
				if (!((Object)(object)coinVat == (Object)null) && coinVat.State == CoinState.Idle)
				{
					val += Vector2.op_Implicit(((Component)coinVat).transform.position);
					num2++;
				}
			}
			if (num2 > num)
			{
				num = num2;
				result = val / (float)num2;
			}
		}
		return result;
	}

	private Vector2Int ToCell(Vector2 position)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2Int(Mathf.FloorToInt(position.x / cellSize), Mathf.FloorToInt(position.y / cellSize));
	}

	private void ClearCells()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < usedCells.Count; i++)
		{
			if (cells.TryGetValue(usedCells[i], out var value))
			{
				value.Clear();
			}
		}
		usedCells.Clear();
	}
}
