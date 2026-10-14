using UnityEngine;

public class NumberFormatterTest : MonoBehaviour
{
	private void Start()
	{
		TestFormatting(0.5);
		TestFormatting(0.123);
		TestFormatting(0.999);
		TestFormatting(1.0);
		TestFormatting(123.0);
		TestFormatting(1230.0);
		TestFormatting(12300.0);
		TestFormatting(123000.0);
		TestFormatting(1230000.0);
		TestFormatting(12300000.0);
		TestFormatting(123000000.0);
		TestFormatting(1230000000.0);
		TestFormatting(1234567890.0);
	}

	private void TestFormatting(double value)
	{
		string arg = NumberFormatter.FormatNumber(value);
		Debug.Log((object)$"{value} 格式化后: {arg}");
	}
}
