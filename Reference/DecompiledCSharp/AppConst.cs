using UnityEngine;

public class AppConst
{
	public static bool DebugMode = false;

	public const int TimerInterval = 1;

	public const int GameFrameRate = 60;

	public const string AppName = "m1b";

	public const string LuaTempDir = "Lua/";

	public const string ExtName = ".unity3d";

	public const string AssetDir = "StreamingAssets";

	public static string WebUrl = "https://bsls-cdn.jvplay.cn/m1b/v2/release/douyin/webgl/";

	public static string FrameworkRoot => Application.dataPath + "/game/m1b";
}
