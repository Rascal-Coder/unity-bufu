using System;

public struct Int256 : IEquatable<Int256>
{
	public long L0;

	public long L1;

	public long L2;

	public long L3;

	public static readonly Int256 Zero = new Int256(0L, 0L, 0L, 0L);

	public static readonly Int256 One = new Int256(1L, 0L, 0L, 0L);

	public Int256(long l0, long l1, long l2, long l3)
	{
		L0 = l0;
		L1 = l1;
		L2 = l2;
		L3 = l3;
	}

	public bool Equals(Int256 other)
	{
		if (L0 == other.L0 && L1 == other.L1 && L2 == other.L2)
		{
			return L3 == other.L3;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is Int256 other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine<long, long, long, long>(L0, L1, L2, L3);
	}

	public static bool operator ==(Int256 left, Int256 right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(Int256 left, Int256 right)
	{
		return !left.Equals(right);
	}

	public static Int256 operator -(Int256 val)
	{
		return new Int256(~val.L0, ~val.L1, ~val.L2, ~val.L3) + One;
	}

	public static Int256 operator +(Int256 a, Int256 b)
	{
		return new Int256(a.L0 + b.L0, a.L1 + b.L1, a.L2 + b.L2, a.L3 + b.L3);
	}

	public static Int256 operator -(Int256 a, Int256 b)
	{
		return a + -b;
	}
}
