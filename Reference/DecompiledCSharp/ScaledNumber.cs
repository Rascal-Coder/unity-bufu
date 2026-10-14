using System;

[Serializable]
public struct ScaledNumber
{
	public double scaledValue;

	public int unitIndex;

	private static readonly double[] UnitMultipliers = new double[21]
	{
		1.0, 1000.0, 1000000.0, 1000000000.0, 1000000000000.0, 1000000000000000.0, 1E+18, 1E+21, 1E+24, 1E+27,
		1E+30, 1E+33, 1E+36, 1E+39, 1E+42, 1E+45, 1E+48, 1E+51, 1E+54, 1E+57,
		1E+60
	};

	private static readonly string[] UnitStrings = new string[21]
	{
		"", "K", "M", "B", "T", "AA", "AB", "AC", "AD", "AE",
		"AF", "AG", "AH", "AI", "AJ", "AK", "AL", "AM", "AN", "AO",
		"AP"
	};

	public static ScaledNumber FromDouble(double originalValue)
	{
		ScaledNumber result = default(ScaledNumber);
		if (originalValue == 0.0)
		{
			result.scaledValue = 0.0;
			result.unitIndex = 0;
			return result;
		}
		bool flag = originalValue < 0.0;
		if (flag)
		{
			originalValue = Math.Abs(originalValue);
		}
		int num = 0;
		while (originalValue >= 1000.0 && num < UnitMultipliers.Length - 1)
		{
			originalValue /= 1000.0;
			num++;
		}
		result.scaledValue = (flag ? (0.0 - originalValue) : originalValue);
		result.unitIndex = num;
		return result;
	}

	public double ToDouble()
	{
		if (unitIndex >= 0 && unitIndex < UnitMultipliers.Length)
		{
			return scaledValue * UnitMultipliers[unitIndex];
		}
		return scaledValue;
	}

	public override string ToString()
	{
		string text = ((unitIndex >= 0 && unitIndex < UnitStrings.Length) ? UnitStrings[unitIndex] : "");
		double num = Math.Abs(scaledValue);
		if (num < 10.0)
		{
			return scaledValue.ToString("F2") + text;
		}
		if (num < 100.0)
		{
			return scaledValue.ToString("F1") + text;
		}
		return (int)scaledValue + text;
	}

	public static ScaledNumber operator +(ScaledNumber a, ScaledNumber b)
	{
		double num = a.ToDouble();
		double num2 = b.ToDouble();
		return FromDouble(num + num2);
	}

	public static ScaledNumber operator -(ScaledNumber a, ScaledNumber b)
	{
		double num = a.ToDouble();
		double num2 = b.ToDouble();
		return FromDouble(num - num2);
	}

	public static ScaledNumber operator *(ScaledNumber a, double b)
	{
		return FromDouble(a.ToDouble() * b);
	}

	public static ScaledNumber operator /(ScaledNumber a, double b)
	{
		if (b == 0.0)
		{
			return new ScaledNumber
			{
				scaledValue = 0.0,
				unitIndex = 0
			};
		}
		return FromDouble(a.ToDouble() / b);
	}

	public static implicit operator ScaledNumber(double value)
	{
		return FromDouble(value);
	}

	public static implicit operator double(ScaledNumber value)
	{
		return value.ToDouble();
	}

	public static bool operator >(ScaledNumber a, ScaledNumber b)
	{
		return a.ToDouble() > b.ToDouble();
	}

	public static bool operator <(ScaledNumber a, ScaledNumber b)
	{
		return a.ToDouble() < b.ToDouble();
	}

	public static bool operator >=(ScaledNumber a, ScaledNumber b)
	{
		return a.ToDouble() >= b.ToDouble();
	}

	public static bool operator <=(ScaledNumber a, ScaledNumber b)
	{
		return a.ToDouble() <= b.ToDouble();
	}

	public static bool operator ==(ScaledNumber a, ScaledNumber b)
	{
		return a.ToDouble() == b.ToDouble();
	}

	public static bool operator !=(ScaledNumber a, ScaledNumber b)
	{
		return a.ToDouble() != b.ToDouble();
	}

	public override bool Equals(object obj)
	{
		if (obj is ScaledNumber scaledNumber)
		{
			return this == scaledNumber;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return ToDouble().GetHashCode();
	}
}
