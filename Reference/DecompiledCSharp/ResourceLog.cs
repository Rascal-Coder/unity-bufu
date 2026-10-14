using System;

[Serializable]
public class ResourceLog
{
	public string operationType;

	public int amount;

	public string source;

	public string timestamp;

	public ResourceLog(string type, int amt, string src)
	{
		operationType = type;
		amount = amt;
		source = src;
		timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
	}
}
