using System;

public struct UInt256 : IEquatable<UInt256>
{
	public long L0;

	public long L1;

	public long L2;

	public long L3;

	public static readonly UInt256 Zero = new UInt256(0L, 0L, 0L, 0L);

	public static readonly UInt256 One = new UInt256(1L, 0L, 0L, 0L);

	public UInt256(long l0, long l1, long l2, long l3)
	{
		L0 = l0;
		L1 = l1;
		L2 = l2;
		L3 = l3;
	}

	public bool Equals(UInt256 other)
	{
		if (L0 == other.L0 && L1 == other.L1 && L2 == other.L2)
		{
			return L3 == other.L3;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is UInt256 other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine<long, long, long, long>(L0, L1, L2, L3);
	}

	public static bool operator ==(UInt256 left, UInt256 right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(UInt256 left, UInt256 right)
	{
		return !left.Equals(right);
	}

	public static implicit operator UInt256(ulong value)
	{
		return new UInt256((long)value, 0L, 0L, 0L);
	}

	public static bool operator >(UInt256 a, UInt256 b)
	{
		if (a.L3 != b.L3)
		{
			return (ulong)a.L3 > (ulong)b.L3;
		}
		if (a.L2 != b.L2)
		{
			return (ulong)a.L2 > (ulong)b.L2;
		}
		if (a.L1 != b.L1)
		{
			return (ulong)a.L1 > (ulong)b.L1;
		}
		return (ulong)a.L0 > (ulong)b.L0;
	}

	public static bool operator <(UInt256 a, UInt256 b)
	{
		return b > a;
	}

	public static bool operator >=(UInt256 a, UInt256 b)
	{
		if (!(a > b))
		{
			return a == b;
		}
		return true;
	}

	public static bool operator <=(UInt256 a, UInt256 b)
	{
		if (!(a < b))
		{
			return a == b;
		}
		return true;
	}

	public static UInt256 operator &(UInt256 a, UInt256 b)
	{
		return new UInt256(a.L0 & b.L0, a.L1 & b.L1, a.L2 & b.L2, a.L3 & b.L3);
	}

	public static UInt256 operator |(UInt256 a, UInt256 b)
	{
		return new UInt256(a.L0 | b.L0, a.L1 | b.L1, a.L2 | b.L2, a.L3 | b.L3);
	}

	public static UInt256 operator ^(UInt256 a, UInt256 b)
	{
		return new UInt256(a.L0 ^ b.L0, a.L1 ^ b.L1, a.L2 ^ b.L2, a.L3 ^ b.L3);
	}

	public static UInt256 operator ~(UInt256 a)
	{
		return new UInt256(~a.L0, ~a.L1, ~a.L2, ~a.L3);
	}

	public static UInt256 operator +(UInt256 a, UInt256 b)
	{
		ulong num = 0uL;
		num = (ulong)(((ulong)(a.L0 + b.L0 + (long)num) < (ulong)a.L0) ? 1 : 0);
		num = (ulong)(((ulong)(a.L1 + b.L1 + (long)num) < (ulong)a.L1) ? 1 : 0);
		num = (ulong)(((ulong)(a.L2 + b.L2 + (long)num) < (ulong)a.L2) ? 1 : 0);
		return new UInt256(a.L0, a.L1, a.L2, a.L3);
	}

	public static UInt256 operator -(UInt256 a, UInt256 b)
	{
		if (a < b)
		{
			return Zero;
		}
		return new UInt256(a.L0 - b.L0, a.L1 - b.L1, a.L2 - b.L2, a.L3 - b.L3);
	}

	public static UInt256 operator <<(UInt256 val, int shift)
	{
		for (int i = 0; i < shift; i++)
		{
			val.L3 = (val.L3 << 1) | (val.L2 >>> 63);
			val.L2 = (val.L2 << 1) | (val.L1 >>> 63);
			val.L1 = (val.L1 << 1) | (val.L0 >>> 63);
			val.L0 <<= 1;
		}
		return val;
	}

	public static UInt256 operator >>(UInt256 val, int shift)
	{
		for (int i = 0; i < shift; i++)
		{
			val.L0 = (val.L0 >>> 1) | (val.L1 << 63);
			val.L1 = (val.L1 >>> 1) | (val.L2 << 63);
			val.L2 = (val.L2 >>> 1) | (val.L3 << 63);
			val.L3 >>>= 1;
		}
		return val;
	}
}
