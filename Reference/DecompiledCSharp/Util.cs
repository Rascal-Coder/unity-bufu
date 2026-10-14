using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class Util
{
	private static List<string> luaPaths = new List<string>();

	public static string DataPath
	{
		get
		{
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Invalid comparison between I4 and Unknown
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Invalid comparison between I4 and Unknown
			string text = "m1b".ToLower();
			if (Application.isMobilePlatform)
			{
				return Application.persistentDataPath + "/" + text + "/";
			}
			Debug.Log((object)Application.platform);
			if (17 == (int)Application.platform || 50 == (int)Application.platform)
			{
				return AppConst.WebUrl;
			}
			if (AppConst.DebugMode)
			{
				return Application.dataPath + "/StreamingAssets/";
			}
			return Application.dataPath + "/StreamingAssets/";
		}
	}

	public static bool NetAvailable => (int)Application.internetReachability > 0;

	public static bool IsWifi => (int)Application.internetReachability == 2;

	public static float countFactor(float original, int level, float factor)
	{
		for (int i = 0; i < level; i++)
		{
			original += original * (factor - 1f);
		}
		return original;
	}

	public static float countDeFactor(float original, int level, float factor)
	{
		for (int i = 0; i < level; i++)
		{
			original -= original * (1f - factor);
		}
		return original;
	}

	public static int Int(object o)
	{
		return Convert.ToInt32(o);
	}

	public static float Float(object o)
	{
		return (float)Math.Round(Convert.ToSingle(o), 2);
	}

	public static long Long(object o)
	{
		return Convert.ToInt64(o);
	}

	public static int Random(int min, int max)
	{
		return Random.Range(min, max);
	}

	public static float Random(float min, float max)
	{
		return Random.Range(min, max);
	}

	public static string Uid(string uid)
	{
		int num = uid.LastIndexOf('_');
		return uid.Remove(0, num + 1);
	}

	public static long GetTime()
	{
		TimeSpan timeSpan = new TimeSpan(DateTime.UtcNow.Ticks - new DateTime(1970, 1, 1, 0, 0, 0).Ticks);
		return (long)timeSpan.TotalMilliseconds;
	}

	public static T Get<T>(GameObject go, string subnode) where T : Component
	{
		if ((Object)(object)go != (Object)null)
		{
			Transform val = go.transform.Find(subnode);
			if ((Object)(object)val != (Object)null)
			{
				return ((Component)val).GetComponent<T>();
			}
		}
		return default(T);
	}

	public static T Get<T>(Transform go, string subnode) where T : Component
	{
		if ((Object)(object)go != (Object)null)
		{
			Transform val = go.Find(subnode);
			if ((Object)(object)val != (Object)null)
			{
				return ((Component)val).GetComponent<T>();
			}
		}
		return default(T);
	}

	public static T Get<T>(Component go, string subnode) where T : Component
	{
		return ((Component)go.transform.Find(subnode)).GetComponent<T>();
	}

	public static T Add<T>(GameObject go) where T : Component
	{
		if ((Object)(object)go != (Object)null)
		{
			T[] components = go.GetComponents<T>();
			for (int i = 0; i < components.Length; i++)
			{
				if ((Object)(object)components[i] != (Object)null)
				{
					Object.Destroy((Object)(object)components[i]);
				}
			}
			return go.gameObject.AddComponent<T>();
		}
		return default(T);
	}

	public static T Add<T>(Transform go) where T : Component
	{
		return Add<T>(((Component)go).gameObject);
	}

	public static GameObject Child(GameObject go, string subnode)
	{
		return Child(go.transform, subnode);
	}

	public static GameObject Child(Transform go, string subnode)
	{
		Transform val = go.Find(subnode);
		if ((Object)(object)val == (Object)null)
		{
			return null;
		}
		return ((Component)val).gameObject;
	}

	public static GameObject Peer(GameObject go, string subnode)
	{
		return Peer(go.transform, subnode);
	}

	public static GameObject Peer(Transform go, string subnode)
	{
		Transform val = go.parent.Find(subnode);
		if ((Object)(object)val == (Object)null)
		{
			return null;
		}
		return ((Component)val).gameObject;
	}

	public static void ClearChild(Transform go)
	{
		if (!((Object)(object)go == (Object)null))
		{
			for (int num = go.childCount - 1; num >= 0; num--)
			{
				Object.Destroy((Object)(object)((Component)go.GetChild(num)).gameObject);
			}
		}
	}

	public static bool IsMiniGamePlatform()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Invalid comparison between Unknown and I4
		if ((int)Application.platform != 50)
		{
			return (int)Application.platform == 50;
		}
		return true;
	}

	public static string GetRelativePath()
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Invalid comparison between Unknown and I4
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Invalid comparison between Unknown and I4
		if (Application.isEditor)
		{
			return "file://" + Environment.CurrentDirectory.Replace("\\", "/") + "/Assets/StreamingAssets/";
		}
		if (Application.isMobilePlatform || Application.isConsolePlatform)
		{
			return "file:///" + DataPath;
		}
		if ((int)Application.platform == 17)
		{
			return DataPath;
		}
		if ((int)Application.platform == 50)
		{
			return DataPath;
		}
		return "file://" + Application.streamingAssetsPath + "/";
	}

	public static string GetFileText(string path)
	{
		return File.ReadAllText(path);
	}

	public string md5(string source)
	{
		MD5CryptoServiceProvider mD5CryptoServiceProvider = new MD5CryptoServiceProvider();
		byte[] bytes = Encoding.UTF8.GetBytes(source);
		byte[] array = mD5CryptoServiceProvider.ComputeHash(bytes, 0, bytes.Length);
		mD5CryptoServiceProvider.Clear();
		string text = "";
		for (int i = 0; i < array.Length; i++)
		{
			text += Convert.ToString(array[i], 16).PadLeft(2, '0');
		}
		return text.PadLeft(32, '0');
	}

	public static string md5file(string file)
	{
		try
		{
			FileStream fileStream = new FileStream(file, FileMode.Open);
			byte[] array = new MD5CryptoServiceProvider().ComputeHash(fileStream);
			fileStream.Close();
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < array.Length; i++)
			{
				stringBuilder.Append(array[i].ToString("x2"));
			}
			return stringBuilder.ToString();
		}
		catch (Exception ex)
		{
			throw new Exception("md5file() fail, error:" + ex.Message);
		}
	}

	public static string AppContentPath()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Invalid comparison between Unknown and I4
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Invalid comparison between Unknown and I4
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Invalid comparison between Unknown and I4
		string empty = string.Empty;
		RuntimePlatform platform = Application.platform;
		if ((int)platform != 8)
		{
			if ((int)platform != 11)
			{
				if ((int)platform == 17)
				{
					return DataPath;
				}
				return Application.dataPath + "/StreamingAssets/";
			}
			return "jar:file://" + Application.dataPath + "!/assets/";
		}
		return Application.dataPath + "/Raw/";
	}

	public static void Log(string str)
	{
		Debug.Log((object)str);
	}

	public static void LogWarning(string str)
	{
		Debug.LogWarning((object)str);
	}

	public static void LogError(string str)
	{
		Debug.LogError((object)str);
	}

	public static int CheckRuntimeFile()
	{
		if (!Application.isEditor)
		{
			return 0;
		}
		string text = Application.dataPath + "/StreamingAssets/";
		if (!Directory.Exists(text))
		{
			return -1;
		}
		if (Directory.GetFiles(text).Length == 0)
		{
			return -1;
		}
		if (!File.Exists(text + "files.txt"))
		{
			return -1;
		}
		string path = AppConst.FrameworkRoot + "/ToLua/Source/Generate/";
		if (!Directory.Exists(path))
		{
			return -2;
		}
		if (Directory.GetFiles(path).Length == 0)
		{
			return -2;
		}
		return 0;
	}

	public static bool CheckEnvironment()
	{
		return true;
	}
}
