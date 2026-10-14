using System;
using System.IO;
using System.Text;
using UnityEngine;

public static class BiliFileSystem
{
	public static bool IsNativeFileSystemAvailable => true;

	public static string GetUserDataPath()
	{
		return Application.persistentDataPath;
	}

	public static string ResolvePath(string path)
	{
		if (string.IsNullOrEmpty(path))
		{
			return path;
		}
		return path.Replace("\\", "/");
	}

	public static void WriteFileSync(string path, string data)
	{
		try
		{
			WriteLocalFile(path, data);
		}
		catch (Exception ex)
		{
			Debug.LogWarning((object)("[BiliFileSystem] 写入文件失败: " + path + ", " + ex.Message));
		}
	}

	public static string ReadFileSync(string path)
	{
		try
		{
			return File.Exists(path) ? File.ReadAllText(path, Encoding.UTF8) : string.Empty;
		}
		catch (Exception ex)
		{
			Debug.LogWarning((object)("[BiliFileSystem] 读取文件失败，按空存档处理: " + path + ", " + ex.Message));
			return string.Empty;
		}
	}

	public static void UnlinkSync(string path)
	{
		if (File.Exists(path))
		{
			File.Delete(path);
		}
	}

	public static bool ExistsSync(string path)
	{
		return File.Exists(path);
	}

	private static void WriteLocalFile(string path, string data)
	{
		string directoryName = Path.GetDirectoryName(path);
		if (!string.IsNullOrEmpty(directoryName))
		{
			Directory.CreateDirectory(directoryName);
		}
		File.WriteAllText(path, data ?? string.Empty, Encoding.UTF8);
	}
}
