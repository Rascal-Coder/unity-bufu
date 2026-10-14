using System;

[Serializable]
public class WorldCupCompactData
{
	public int v = 1;

	public int t;

	public int d;

	public int c;

	public int s;

	public int l = -1;

	public int p;

	public int m;

	public int r;

	private static readonly int[] MissionProgressBits = new int[6] { 1, 4, 1, 1, 7, 3 };

	private static readonly DateTime UnixEpochUtc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

	public bool HasData
	{
		get
		{
			if (t <= 0 && d <= 0 && c == 0 && s == 0 && l < 0 && p == 0 && m == 0)
			{
				return r != 0;
			}
			return true;
		}
	}

	public static WorldCupCompactData FromWorldCupData(WorldCupData source)
	{
		WorldCupCompactData worldCupCompactData = new WorldCupCompactData();
		if (source == null)
		{
			return worldCupCompactData;
		}
		worldCupCompactData.t = ToBeijingDay(source.signFirstShowTime);
		worldCupCompactData.d = Clamp(source.signDay, 0, 7);
		worldCupCompactData.c = PackFlags(source.clamp, 14);
		worldCupCompactData.s = Clamp(source.score, 0, 100);
		worldCupCompactData.l = Clamp(source.scoreLastDay, -1, 7);
		worldCupCompactData.p = PackMissionProgress(source.missionProgress);
		worldCupCompactData.m = PackFlags(source.missionClamp, 6);
		worldCupCompactData.r = PackFlags(source.scoreClamp, 4);
		return worldCupCompactData;
	}

	public WorldCupData ToWorldCupData()
	{
		return new WorldCupData
		{
			signFirstShowTime = FromBeijingDay(t),
			signDay = ((d > 0) ? Clamp(d, 1, 7) : CalculateSignDay(t)),
			score = Clamp(s, 0, 100),
			scoreLastDay = Clamp(l, -1, 7),
			clamp = UnpackFlags(c, 14),
			missionProgress = UnpackMissionProgress(p),
			missionClamp = UnpackFlags(m, 6),
			scoreClamp = UnpackFlags(r, 4)
		};
	}

	private static int PackFlags(int[] values, int count)
	{
		int num = 0;
		if (values == null)
		{
			return num;
		}
		int num2 = Math.Min(values.Length, count);
		for (int i = 0; i < num2; i++)
		{
			if (values[i] != 0)
			{
				num |= 1 << i;
			}
		}
		return num;
	}

	private static int[] UnpackFlags(int mask, int count)
	{
		int[] array = new int[count];
		for (int i = 0; i < count; i++)
		{
			array[i] = (((mask & (1 << i)) != 0) ? 1 : 0);
		}
		return array;
	}

	private static int PackMissionProgress(int[] values)
	{
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < MissionProgressBits.Length; i++)
		{
			int num3 = MissionProgressBits[i];
			int value = ((values != null && values.Length > i) ? values[i] : 0);
			value = Clamp(value, 0, (1 << num3) - 1);
			num |= value << num2;
			num2 += num3;
		}
		return num;
	}

	private static int[] UnpackMissionProgress(int packed)
	{
		int[] array = new int[MissionProgressBits.Length];
		int num = 0;
		for (int i = 0; i < MissionProgressBits.Length; i++)
		{
			int num2 = MissionProgressBits[i];
			array[i] = (packed >> num) & ((1 << num2) - 1);
			num += num2;
		}
		return array;
	}

	private static int ToBeijingDay(long timestampMs)
	{
		if (timestampMs <= 0)
		{
			return 0;
		}
		return (int)(UnixEpochUtc.AddMilliseconds(timestampMs).AddHours(8.0).Date - UnixEpochUtc.Date).TotalDays;
	}

	private static long FromBeijingDay(int day)
	{
		if (day <= 0)
		{
			return 0L;
		}
		return (long)(UnixEpochUtc.Date.AddDays(day).AddHours(-8.0) - UnixEpochUtc).TotalMilliseconds;
	}

	private static int CalculateSignDay(int firstShowDay)
	{
		if (firstShowDay <= 0)
		{
			return 0;
		}
		return Clamp(ToBeijingDay((long)(DateTime.UtcNow - UnixEpochUtc).TotalMilliseconds) - firstShowDay + 1, 1, 7);
	}

	private static int Clamp(int value, int min, int max)
	{
		return Math.Max(min, Math.Min(max, value));
	}
}
