using System;

public static class LocalUserId
{
	private const string KEY = "LOCAL_UUID";

	public static string GetExisting()
	{
		if (!PlayerPrefs.HasKey("LOCAL_UUID"))
		{
			return "";
		}
		return PlayerPrefs.GetString("LOCAL_UUID", "");
	}

	public static void Set(string localUserId)
	{
		if (!string.IsNullOrEmpty(localUserId) && !string.Equals(GetExisting(), localUserId, StringComparison.Ordinal))
		{
			PlayerPrefs.SetString("LOCAL_UUID", localUserId);
			PlayerPrefs.Save();
		}
	}

	public static string Get()
	{
		string text = GetExisting();
		if (string.IsNullOrEmpty(text))
		{
			text = Guid.NewGuid().ToString("N");
			Set(text);
		}
		return text;
	}
}
