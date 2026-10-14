using System;
using System.Text;

public static class NumberFormatter
{
	private static readonly string[] InternationalUnits = new string[83]
	{
		"", "K", "M", "B", "T", "AA", "AB", "AC", "AD", "AE",
		"AF", "AG", "AH", "AI", "AJ", "AK", "AL", "AM", "AN", "AO",
		"AP", "AQ", "AR", "AS", "AT", "AU", "AV", "AW", "AX", "AY",
		"AZ", "BA", "BB", "BC", "BD", "BE", "BF", "BG", "BH", "BI",
		"BJ", "BK", "BL", "BM", "BN", "BO", "BP", "BQ", "BR", "BS",
		"BT", "BU", "BV", "BW", "BX", "BY", "BZ", "CA", "CB", "CC",
		"CD", "CE", "CF", "CG", "CH", "CI", "CJ", "CK", "CL", "CM",
		"CN", "CO", "CP", "CQ", "CR", "CS", "CT", "CU", "CV", "CW",
		"CX", "CY", "CZ"
	};

	private static readonly string[] ChineseUnits = new string[4] { "千", "万", "亿", "兆" };

	public static string FormatNumber(double value, bool isInt = false)
	{
		bool flag = value < 0.0;
		if (flag)
		{
			value = Math.Abs(value);
		}
		if (!isInt)
		{
			if (value < 10.0)
			{
				return (flag ? "-" : "") + value.ToString("F0");
			}
			if (value < 100.0)
			{
				return (flag ? "-" : "") + value.ToString("F0");
			}
		}
		if (value < 1000.0)
		{
			return (flag ? "-" : "") + (int)value;
		}
		int num = 0;
		double num2 = value;
		while (num2 >= 1000.0 && num < InternationalUnits.Length - 1)
		{
			num2 /= 1000.0;
			num++;
		}
		string text = InternationalUnits[num];
		string text2 = ((num2 < 10.0) ? (num2.ToString("F2") + text) : ((!(num2 < 100.0)) ? ((int)num2 + text) : (num2.ToString("F1") + text)));
		if (!flag)
		{
			return text2;
		}
		return "-" + text2;
	}

	public static string FormatNumber(long value)
	{
		return FormatNumber(value, isInt: false);
	}

	public static string FormatNumber(int value)
	{
		return FormatNumber(value, isInt: false);
	}

	public static string FormatDamageNumber(double value)
	{
		if (value < 10000.0)
		{
			return value.ToString();
		}
		double num = value / 10000.0;
		if (value < 1000000.0)
		{
			return num.ToString("F1") + "万";
		}
		if (value < 10000000.0)
		{
			return num + "万";
		}
		double num2 = value / 100000000.0;
		if (num2 % 1.0 == 0.0)
		{
			return num2 + "亿";
		}
		return num2.ToString("F1") + "亿";
	}

	public static string FormatDamageNumber(long value)
	{
		return FormatDamageNumber((double)value);
	}

	public static string FormatDamageNumber(int value)
	{
		return FormatDamageNumber((double)value);
	}

	public static string ConvertToSpriteText(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return text;
		}
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = false;
		flag = (text.Contains("暴") ? true : false);
		foreach (char c in text)
		{
			int num;
			if (char.IsDigit(c))
			{
				num = ((!flag) ? (39 + (c - 48)) : (c - 48));
			}
			else
			{
				switch (c)
				{
				case '.':
					num = ((!flag) ? 49 : 10);
					break;
				case '+':
					num = ((!flag) ? 50 : 11);
					break;
				case '暴':
					num = 12;
					break;
				default:
					if (!char.IsUpper(c))
					{
						stringBuilder.Append(c);
						continue;
					}
					num = ((!text.Contains("暴")) ? (51 + (c - 65)) : (13 + (c - 65)));
					break;
				}
			}
			stringBuilder.AppendFormat("<sprite={0}>", num);
		}
		return stringBuilder.ToString();
	}
}
