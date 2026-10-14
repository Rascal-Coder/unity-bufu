public class PrefsUtil
{
	public const string KEY_GUIDE_ENABLED = "guide_enabled";

	public const string KEY_GUIDE_CURRENT_STEP = "guide_current_step";

	public const string KEY_GUIDE_COMPLETED = "guide_completed";

	public const string KEY_GUIDE_SINGLE_PREFIX = "guide_single_";

	public const string KEY_GUIDE_MODE = "guide_mode";

	public const string KEY_GAME_FIRST_LAUNCH = "game_first_launch";

	public static void SetBool(string key, bool value)
	{
		PlayerPrefs.SetInt(key, value ? 1 : 0);
	}

	public static bool GetBool(string key, bool defaultValue = false)
	{
		return PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1;
	}

	public static void SetInt(string key, int value)
	{
		PlayerPrefs.SetInt(key, value);
	}

	public static int GetInt(string key, int defaultValue = 0)
	{
		return PlayerPrefs.GetInt(key, defaultValue);
	}

	public static void SetString(string key, string value)
	{
		PlayerPrefs.SetString(key, value);
	}

	public static string GetString(string key, string defaultValue = "")
	{
		return PlayerPrefs.GetString(key, defaultValue);
	}

	public static void SetFloat(string key, float value)
	{
		PlayerPrefs.SetFloat(key, value);
	}

	public static float GetFloat(string key, float defaultValue = 0f)
	{
		return PlayerPrefs.GetFloat(key, defaultValue);
	}

	public static void DeleteKey(string key)
	{
		PlayerPrefs.DeleteKey(key);
	}

	public static bool HasKey(string key)
	{
		return PlayerPrefs.HasKey(key);
	}

	public static void ClearAll()
	{
		PlayerPrefs.DeleteAll();
	}

	public static void Save()
	{
		PlayerPrefs.Save();
	}
}
