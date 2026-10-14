using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;
using YooAsset;

public class ResManager : SingletonMono<ResManager>
{
	private class DefaultRemoteServices : IRemoteServices
	{
		private readonly string _defaultHostServer;

		private readonly string _fallbackHostServer;

		public DefaultRemoteServices(string defaultHostServer, string fallbackHostServer)
		{
			_defaultHostServer = defaultHostServer;
			_fallbackHostServer = fallbackHostServer;
		}

		public string GetRemoteMainURL(string fileName)
		{
			Debug.Log((object)("[ResManager] GetRemoteMainURL: " + _defaultHostServer + fileName));
			return _defaultHostServer + "/" + fileName;
		}

		public string GetRemoteFallbackURL(string fileName)
		{
			Debug.Log((object)("[ResManager] GetRemoteFallbackURL: " + _fallbackHostServer + fileName));
			return _fallbackHostServer + "/" + fileName;
		}
	}

	private const string PackageName = "gameres";

	private ResourcePackage _currentPackage;

	private bool _isInitialized;

	public Action<float> OnDownloadProgressCallback;

	public Action<bool, string> OnInitializeCompleteCallback;

	private Dictionary<string, SpriteAtlas> _loadedAtlases = new Dictionary<string, SpriteAtlas>();

	private Dictionary<string, int> _loadedAssets = new Dictionary<string, int>();

	private Dictionary<string, List<Delegate>> _loadingCallbacks = new Dictionary<string, List<Delegate>>();

	public string Version = "";

	private const bool ResourceLoadLogEnabledInBuild = false;

	[SerializeField]
	private bool enableResourceLoadLog;

	private int _resourceLoadLogId;

	private readonly Dictionary<string, int> _resourceLoadRequestCounts = new Dictionary<string, int>();

	public Action<string, string> OnBeforeLoadRemoteResource;

	public Action<string, string, bool> OnAfterLoadRemoteResource;

	public Action OnBeforeRequestPackageVersion;

	public Action<bool, string> OnAfterRequestPackageVersion;

	public Action<string> OnBeforeUpdatePackageManifest;

	public Action<bool, string> OnAfterUpdatePackageManifest;

	public override bool DontDestory => true;

	public bool EnableResourceLoadLog
	{
		get
		{
			return enableResourceLoadLog;
		}
		set
		{
			enableResourceLoadLog = value;
		}
	}

	private int BeginResourceLoadLog<T>(string assetPath, string source, out float startTime) where T : Object
	{
		startTime = Time.realtimeSinceStartup;
		return 0;
	}

	private void EndResourceLoadLog(int logId, string assetPath, string source, float startTime, bool success, string error = null)
	{
	}

	private void LogResourceLoadState(string assetPath, string state)
	{
	}

	private string GetAssetLoadDetailLog<T>(string assetPath) where T : Object
	{
		try
		{
			AssetInfo assetInfo = _currentPackage.GetAssetInfo(assetPath, typeof(T));
			if (assetInfo == null || assetInfo.IsInvalid)
			{
				return " bundle=unknown remote=unknown";
			}
			return GetBundleLog(assetInfo) + GetRemoteDownloadLog(assetInfo);
		}
		catch (Exception ex)
		{
			return " bundle=unknown detailError=" + ex.GetType().Name;
		}
	}

	private string GetRemoteDownloadLog(AssetInfo assetInfo)
	{
		try
		{
			if (!_currentPackage.IsNeedDownloadFromRemote(assetInfo))
			{
				return " remote=0";
			}
			ResourceDownloaderOperation val = _currentPackage.CreateBundleDownloader(assetInfo, 1, 1);
			return $" remote=1 downloadCount={((DownloaderOperation)val).TotalDownloadCount} downloadBytes={((DownloaderOperation)val).TotalDownloadBytes} downloadSize={FormatBytes(((DownloaderOperation)val).TotalDownloadBytes)}";
		}
		catch (Exception ex)
		{
			return " remoteCheckError=" + ex.GetType().Name;
		}
	}

	private string GetBundleLog(AssetInfo assetInfo)
	{
		try
		{
			object fieldValue = GetFieldValue(assetInfo, "_packageAsset", BindingFlags.Instance | BindingFlags.NonPublic);
			object activePackageManifest = GetActivePackageManifest();
			if (fieldValue == null || activePackageManifest == null)
			{
				return " bundle=unknown";
			}
			int num = (int)GetFieldValue(fieldValue, "BundleID", BindingFlags.Instance | BindingFlags.Public);
			int[] array = GetFieldValue(fieldValue, "DependBundleIDs", BindingFlags.Instance | BindingFlags.Public) as int[];
			if (!(GetFieldValue(activePackageManifest, "BundleList", BindingFlags.Instance | BindingFlags.Public) is IList list) || num < 0 || num >= list.Count)
			{
				return " bundle=unknown";
			}
			object bundle = list[num];
			string bundleString = GetBundleString(bundle, "BundleName");
			string bundleString2 = GetBundleString(bundle, "FileName");
			long bundleLong = GetBundleLong(bundle, "FileSize");
			long num2 = 0L;
			List<string> list2 = new List<string>();
			if (array != null)
			{
				int[] array2 = array;
				foreach (int num3 in array2)
				{
					if (num3 >= 0 && num3 < list.Count)
					{
						object bundle2 = list[num3];
						num2 += GetBundleLong(bundle2, "FileSize");
						if (list2.Count < 8)
						{
							list2.Add(GetBundleString(bundle2, "BundleName"));
						}
					}
				}
			}
			string text = ((list2.Count > 0) ? string.Format(" deps={0} depsSize={1} depBundles=[{2}{3}]", (array != null) ? array.Length : 0, FormatBytes(num2), string.Join("|", list2), GetDependBundleOverflowLog(array, 8)) : " deps=0");
			return " bundle=" + bundleString + " bundleFile=" + bundleString2 + " bundleSize=" + FormatBytes(bundleLong) + text;
		}
		catch (Exception ex)
		{
			return " bundle=unknown bundleError=" + ex.GetType().Name;
		}
	}

	private object GetActivePackageManifest()
	{
		object fieldValue = GetFieldValue(_currentPackage, "_playModeImpl", BindingFlags.Instance | BindingFlags.NonPublic);
		return fieldValue?.GetType().GetProperty("ActiveManifest", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(fieldValue);
	}

	private static object GetFieldValue(object target, string fieldName, BindingFlags flags)
	{
		return target?.GetType().GetField(fieldName, flags)?.GetValue(target);
	}

	private static string GetBundleString(object bundle, string memberName)
	{
		try
		{
			FieldInfo field = bundle.GetType().GetField(memberName, BindingFlags.Instance | BindingFlags.Public);
			if (field != null)
			{
				return field.GetValue(bundle)?.ToString() ?? string.Empty;
			}
			return bundle.GetType().GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public)?.GetValue(bundle)?.ToString() ?? string.Empty;
		}
		catch
		{
			return string.Empty;
		}
	}

	private static long GetBundleLong(object bundle, string fieldName)
	{
		object fieldValue = GetFieldValue(bundle, fieldName, BindingFlags.Instance | BindingFlags.Public);
		if (fieldValue is long)
		{
			return (long)fieldValue;
		}
		return 0L;
	}

	private static string GetDependBundleOverflowLog(int[] dependBundleIds, int maxLogCount)
	{
		if (dependBundleIds == null || dependBundleIds.Length <= maxLogCount)
		{
			return string.Empty;
		}
		return $"|...+{dependBundleIds.Length - maxLogCount}";
	}

	private static string FormatBytes(long bytes)
	{
		if (bytes >= 1048576)
		{
			return $"{(float)bytes / 1024f / 1024f:F2}MB";
		}
		if (bytes >= 1024)
		{
			return $"{(float)bytes / 1024f:F1}KB";
		}
		return $"{bytes}B";
	}

	protected override void OnAwake()
	{
		_isInitialized = false;
	}

	protected override void BeforOnDestroy()
	{
		if ((Object)(object)((Component)this).gameObject != (Object)null && ((Component)this).gameObject.activeSelf)
		{
			Uninitialize();
		}
	}

	private bool TryParseVersionToDateTime(string version, out DateTime dateTime)
	{
		dateTime = DateTime.MinValue;
		if (string.IsNullOrEmpty(version))
		{
			Debug.LogError((object)"版本号为空！");
			return false;
		}
		if (version.Length == 12 && long.TryParse(version, out var _))
		{
			try
			{
				int year = int.Parse(version.Substring(0, 4));
				int month = int.Parse(version.Substring(4, 2));
				int day = int.Parse(version.Substring(6, 2));
				int hour = int.Parse(version.Substring(8, 2));
				int minute = int.Parse(version.Substring(10, 2));
				dateTime = new DateTime(year, month, day, hour, minute, 0);
				return true;
			}
			catch (Exception ex)
			{
				Debug.LogError((object)("紧凑格式版本号解析失败：" + ex.Message));
				return false;
			}
		}
		string[] array = version.Replace("-", " ").Replace("/", " ").Replace(":", " ")
			.Replace(".", " ")
			.Trim()
			.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
		if (array.Length >= 5)
		{
			try
			{
				int year2 = int.Parse(array[0]);
				int month2 = int.Parse(array[1]);
				int day2 = int.Parse(array[2]);
				int hour2 = int.Parse(array[3]);
				int minute2 = int.Parse(array[4]);
				dateTime = new DateTime(year2, month2, day2, hour2, minute2, 0);
				return true;
			}
			catch (Exception ex2)
			{
				Debug.LogError((object)("分隔符格式版本号解析失败：" + ex2.Message));
				return false;
			}
		}
		Debug.LogError((object)("不支持的版本号格式：" + version));
		return false;
	}

	public int CompareVersion(string versionA, string versionB)
	{
		if (!TryParseVersionToDateTime(versionA, out var dateTime) || !TryParseVersionToDateTime(versionB, out var dateTime2))
		{
			Debug.LogError((object)"版本号解析失败，格式错误！");
			return -2;
		}
		int num = DateTime.Compare(dateTime, dateTime2);
		if (num > 0)
		{
			return 1;
		}
		if (num < 0)
		{
			return -1;
		}
		return 0;
	}

	public void InitializeInOfflineMode(Action<bool, string> callback = null)
	{
		OnInitializeCompleteCallback = callback;
		((MonoBehaviour)this).StartCoroutine(InitializeOffLineModeCoroutine());
	}

	private IEnumerator InitializeOffLineModeCoroutine()
	{
		YooAssets.Initialize((ILogger)null);
		if (!YooAssets.ContainsPackage("gameres"))
		{
			_currentPackage = YooAssets.CreatePackage("gameres");
			YooAssets.SetDefaultPackage(_currentPackage);
		}
		else
		{
			_currentPackage = YooAssets.GetPackage("gameres");
		}
		OfflinePlayModeParameters val = new OfflinePlayModeParameters();
		val.BuildinFileSystemParameters = FileSystemParameters.CreateDefaultBuildinFileSystemParameters((IDecryptionServices)null, Path.Combine(Application.streamingAssetsPath, "stonehunter", "gameres"));
		InitializationOperation initOperation = _currentPackage.InitializeAsync((InitializeParameters)(object)val);
		yield return initOperation;
		if ((int)((AsyncOperationBase)initOperation).Status == 2)
		{
			_isInitialized = true;
			RequestPackageVersionOperation versionOperation = _currentPackage.RequestPackageVersionAsync(true, 60);
			yield return versionOperation;
			if ((int)((AsyncOperationBase)versionOperation).Status == 2)
			{
				string packageVersion = versionOperation.PackageVersion;
				yield return _currentPackage.UpdatePackageManifestAsync(packageVersion, 60);
			}
			Debug.Log((object)"[ResManager] OffLine模式资源包初始化成功！");
			OnInitializeCompleteCallback?.Invoke(arg1: true, "初始化成功");
		}
		else
		{
			string text = "[ResManager] OffLine模式资源包初始化失败：" + ((AsyncOperationBase)initOperation).Error;
			Debug.LogError((object)text);
			OnInitializeCompleteCallback?.Invoke(arg1: false, text);
		}
	}

	public void InitializeInMiniGameMode(string defaultHostServer, string fallbackHostServer, Action<bool, string> callback = null)
	{
		OnInitializeCompleteCallback = callback;
		((MonoBehaviour)this).StartCoroutine(InitializeMiniGameModeCoroutine(defaultHostServer, fallbackHostServer));
	}

	public void InitializeInOnlineMode(string defaultHostServer, string fallbackHostServer, Action<bool, string> callback = null)
	{
		OnInitializeCompleteCallback = callback;
		((MonoBehaviour)this).StartCoroutine(InitializeOnlineModeCoroutine(defaultHostServer, fallbackHostServer));
	}

	public void InitializeInAndroidMode(string defaultHostServer, string fallbackHostServer, Action<bool, string> callback = null)
	{
		OnInitializeCompleteCallback = callback;
		((MonoBehaviour)this).StartCoroutine(InitializeAndroidModeCoroutine(defaultHostServer, fallbackHostServer));
	}

	private IEnumerator InitializeMiniGameModeCoroutine(string defaultHostServer, string fallbackHostServer)
	{
		string text = PlayerPrefs.GetString("gameres_version", "");
		Debug.Log((object)("代码版本号:" + text));
		YooAssets.Initialize((ILogger)null);
		Debug.Log((object)("InitializeMiniGameModeCoroutine1defaultHostServer: " + defaultHostServer + ", fallbackHostServer: " + fallbackHostServer));
		if (!YooAssets.ContainsPackage("gameres"))
		{
			_currentPackage = YooAssets.CreatePackage("gameres");
			YooAssets.SetDefaultPackage(_currentPackage);
			Debug.Log((object)"InitializeMiniGameModeCoroutine2");
		}
		else
		{
			_currentPackage = YooAssets.GetPackage("gameres");
			Debug.Log((object)"InitializeMiniGameModeCoroutine3");
		}
		YooAssets.SetOperationSystemMaxTimeSlice(50L);
		DefaultRemoteServices defaultRemoteServices = new DefaultRemoteServices(defaultHostServer, fallbackHostServer);
		FileSystemParameters webServerFileSystemParameters = FileSystemParameters.CreateDefaultWebServerFileSystemParameters2(defaultHostServer, (IWebDecryptionServices)null, false);
		FileSystemParameters webRemoteFileSystemParameters = FileSystemParameters.CreateDefaultWebRemoteFileSystemParameters((IRemoteServices)(object)defaultRemoteServices, (IWebDecryptionServices)null, false);
		WebPlayModeParameters createParameters = new WebPlayModeParameters();
		createParameters.WebServerFileSystemParameters = webServerFileSystemParameters;
		createParameters.WebRemoteFileSystemParameters = webRemoteFileSystemParameters;
		((InitializeParameters)createParameters).BundleLoadingMaxConcurrency = 15;
		string text2 = AppConst.WebUrl + "StreamingAssets/stonehunter/gameres/gameres.json?v=" + Random.value;
		UnityWebRequest www = UnityWebRequest.Get(text2);
		yield return www.SendWebRequest();
		if ((int)www.result != 1)
		{
			Debug.Log((object)www.error);
		}
		if (www.downloadHandler != null)
		{
			Version = www.downloadHandler.text;
		}
		InitializationOperation initOperation = _currentPackage.InitializeAsync((InitializeParameters)(object)createParameters);
		yield return initOperation;
		if ((int)((AsyncOperationBase)initOperation).Status == 2)
		{
			_isInitialized = true;
			RequestPackageVersionOperation versionOperation = _currentPackage.RequestPackageVersionAsync(true, 60);
			yield return versionOperation;
			Debug.Log((object)("InitializeMiniGameVersion:" + versionOperation.PackageVersion));
			if ((int)((AsyncOperationBase)versionOperation).Status == 2)
			{
				string packageVersion = versionOperation.PackageVersion;
				Debug.Log((object)packageVersion);
				yield return (!string.IsNullOrEmpty(Version)) ? _currentPackage.UpdatePackageManifestAsync(Version, 60) : _currentPackage.UpdatePackageManifestAsync(packageVersion, 60);
			}
			Debug.Log((object)"[ResManager] 小游戏模式资源包初始化成功！");
			OnInitializeCompleteCallback?.Invoke(arg1: true, "初始化成功");
		}
		else
		{
			string text3 = "[ResManager] 小游戏模式资源包初始化失败：" + ((AsyncOperationBase)initOperation).Error;
			Debug.Log((object)"InitializeMiniGameModeCoroutine11");
			Debug.LogError((object)text3);
			OnInitializeCompleteCallback?.Invoke(arg1: false, text3);
		}
	}

	private IEnumerator InitializeAndroidModeCoroutine(string defaultHostServer, string fallbackHostServer)
	{
		YooAssets.Initialize((ILogger)null);
		if (!YooAssets.ContainsPackage("gameres"))
		{
			_currentPackage = YooAssets.CreatePackage("gameres");
			YooAssets.SetDefaultPackage(_currentPackage);
		}
		else
		{
			_currentPackage = YooAssets.GetPackage("gameres");
		}
		YooAssets.SetOperationSystemMaxTimeSlice(50L);
		IRemoteServices val = (IRemoteServices)(object)new DefaultRemoteServices(defaultHostServer, fallbackHostServer);
		string text = Path.Combine(Application.streamingAssetsPath, "stonehunter", "gameres");
		string text2 = Path.Combine(Application.persistentDataPath, "YooAssets", "gameres");
		HostPlayModeParameters val2 = new HostPlayModeParameters();
		val2.BuildinFileSystemParameters = FileSystemParameters.CreateDefaultBuildinFileSystemParameters((IDecryptionServices)null, text);
		val2.CacheFileSystemParameters = FileSystemParameters.CreateDefaultCacheFileSystemParameters(val, (IDecryptionServices)null, text2);
		InitializationOperation initOperation = _currentPackage.InitializeAsync((InitializeParameters)(object)val2);
		yield return initOperation;
		if ((int)((AsyncOperationBase)initOperation).Status != 2)
		{
			string text3 = "[ResManager] Android资源包初始化失败：" + ((AsyncOperationBase)initOperation).Error;
			Debug.LogError((object)text3);
			OnInitializeCompleteCallback?.Invoke(arg1: false, text3);
			yield break;
		}
		_isInitialized = true;
		Debug.Log((object)"[ResManager] Android资源包初始化成功，开始请求远程资源版本...");
		RequestPackageVersionOperation versionOperation = _currentPackage.RequestPackageVersionAsync(true, 60);
		yield return versionOperation;
		if ((int)((AsyncOperationBase)versionOperation).Status == 2)
		{
			string remoteVersion = versionOperation.PackageVersion;
			Debug.Log((object)("[ResManager] Android远端资源版本：" + remoteVersion));
			UpdatePackageManifestOperation manifestOperation = _currentPackage.UpdatePackageManifestAsync(remoteVersion, 60);
			yield return manifestOperation;
			if ((int)((AsyncOperationBase)manifestOperation).Status == 2)
			{
				Version = remoteVersion;
				Debug.Log((object)("[ResManager] Android资源清单更新成功：" + Version));
			}
			else
			{
				Debug.LogWarning((object)("[ResManager] Android资源清单更新失败，将继续使用本地资源：" + ((AsyncOperationBase)manifestOperation).Error));
			}
		}
		else
		{
			Debug.LogWarning((object)("[ResManager] Android远端版本请求失败，将继续使用本地资源：" + ((AsyncOperationBase)versionOperation).Error));
		}
		OnInitializeCompleteCallback?.Invoke(arg1: true, "初始化成功");
	}

	private IEnumerator InitializeOnlineModeCoroutine(string defaultHostServer, string fallbackHostServer)
	{
		YooAssets.Initialize((ILogger)null);
		if (!YooAssets.ContainsPackage("gameres"))
		{
			_currentPackage = YooAssets.CreatePackage("gameres");
			YooAssets.SetDefaultPackage(_currentPackage);
		}
		else
		{
			_currentPackage = YooAssets.GetPackage("gameres");
		}
		YooAssets.SetOperationSystemMaxTimeSlice(50L);
		DefaultRemoteServices defaultRemoteServices = new DefaultRemoteServices(defaultHostServer, fallbackHostServer);
		FileSystemParameters webServerFileSystemParameters = FileSystemParameters.CreateDefaultWebServerFileSystemParameters((IWebDecryptionServices)null, false);
		FileSystemParameters webRemoteFileSystemParameters = FileSystemParameters.CreateDefaultWebRemoteFileSystemParameters((IRemoteServices)(object)defaultRemoteServices, (IWebDecryptionServices)null, false);
		WebPlayModeParameters val = new WebPlayModeParameters();
		val.WebServerFileSystemParameters = webServerFileSystemParameters;
		val.WebRemoteFileSystemParameters = webRemoteFileSystemParameters;
		((InitializeParameters)val).BundleLoadingMaxConcurrency = 15;
		InitializationOperation initOperation = _currentPackage.InitializeAsync((InitializeParameters)(object)val);
		yield return initOperation;
		if ((int)((AsyncOperationBase)initOperation).Status == 2)
		{
			_isInitialized = true;
			RequestPackageVersionOperation versionOperation = _currentPackage.RequestPackageVersionAsync(true, 60);
			yield return versionOperation;
			if ((int)((AsyncOperationBase)versionOperation).Status == 2)
			{
				string packageVersion = versionOperation.PackageVersion;
				Debug.Log((object)("[ResManager] 联机模式获取到的远程版本号: " + packageVersion));
				UpdatePackageManifestOperation manifestOperation = _currentPackage.UpdatePackageManifestAsync(packageVersion, 60);
				yield return manifestOperation;
				if ((int)((AsyncOperationBase)manifestOperation).Status != 2)
				{
					Debug.LogWarning((object)("[ResManager] 联机模式更新资源清单失败: " + ((AsyncOperationBase)manifestOperation).Error));
				}
			}
			else
			{
				Debug.LogWarning((object)("[ResManager] 联机模式请求版本号失败: " + ((AsyncOperationBase)versionOperation).Error));
			}
			Debug.Log((object)"[ResManager] 联机模式资源包初始化成功！");
			OnInitializeCompleteCallback?.Invoke(arg1: true, "初始化成功");
		}
		else
		{
			string text = "[ResManager] 联机模式资源包初始化失败：" + ((AsyncOperationBase)initOperation).Error;
			Debug.LogError((object)text);
			OnInitializeCompleteCallback?.Invoke(arg1: false, text);
		}
	}

	private string CheckAssetPath(string assetPath, string suffix = "")
	{
		if (!assetPath.StartsWith("Assets/"))
		{
			assetPath = "Assets/" + assetPath;
		}
		if (!string.IsNullOrEmpty(suffix) && !assetPath.EndsWith(suffix))
		{
			assetPath += suffix;
		}
		return assetPath;
	}

	public void LoadSpriteAsync(string assetPath, Action<Sprite> callback, bool isAtlas = true)
	{
		if (!_isInitialized)
		{
			Debug.LogError((object)"[ResManager] 资源系统尚未初始化，请先调用Initialize方法");
			callback?.Invoke(null);
			return;
		}
		assetPath = CheckAssetPath(assetPath, ".png");
		if (isAtlas)
		{
			((MonoBehaviour)this).StartCoroutine(LoadSpriteFromAtlasAsyncCoroutine(assetPath, callback));
		}
		else
		{
			((MonoBehaviour)this).StartCoroutine(LoadAssetAsyncCoroutine<Sprite>(assetPath, callback));
		}
	}

	private IEnumerator LoadSpriteFromAtlasAsyncCoroutine(string assetPath, Action<Sprite> callback)
	{
		float spriteStartTime;
		int spriteLogId = BeginResourceLoadLog<Sprite>(assetPath, "sprite-atlas", out spriteStartTime);
		string atlasName = ExtractAtlasPathFromPath(assetPath);
		string spriteName = Path.GetFileNameWithoutExtension(assetPath);
		SpriteAtlas atlas = null;
		if (_loadedAtlases.ContainsKey(atlasName))
		{
			atlas = _loadedAtlases[atlasName];
		}
		else
		{
			float atlasStartTime;
			int atlasLogId = BeginResourceLoadLog<SpriteAtlas>(atlasName, "sprite-atlas-file", out atlasStartTime);
			AssetHandle atlasOperation = _currentPackage.LoadAssetAsync<SpriteAtlas>(atlasName, 0u);
			yield return atlasOperation;
			EndResourceLoadLog(atlasLogId, atlasName, "sprite-atlas-file", atlasStartTime, (int)((HandleBase)atlasOperation).Status == 2, ((HandleBase)atlasOperation).LastError);
			if ((int)((HandleBase)atlasOperation).Status == 2)
			{
				Object assetObject = atlasOperation.AssetObject;
				atlas = (SpriteAtlas)(object)((assetObject is SpriteAtlas) ? assetObject : null);
				if ((Object)(object)atlas != (Object)null)
				{
					_loadedAtlases[atlasName] = atlas;
				}
			}
		}
		if ((Object)(object)atlas != (Object)null)
		{
			Sprite sprite = atlas.GetSprite(spriteName);
			if ((Object)(object)sprite != (Object)null)
			{
				EndResourceLoadLog(spriteLogId, assetPath, "sprite-atlas", spriteStartTime, success: true);
				callback?.Invoke(sprite);
				yield break;
			}
		}
		Debug.LogWarning((object)("[ResManager] 从图集" + atlasName + "加载Sprite" + spriteName + "失败，使用原有方式加载"));
		EndResourceLoadLog(spriteLogId, assetPath, "sprite-atlas", spriteStartTime, success: false, "sprite not found in atlas");
		yield return ((MonoBehaviour)this).StartCoroutine(LoadAssetAsyncCoroutine<Sprite>(assetPath, callback));
	}

	private string ExtractAtlasPathFromPath(string assetPath)
	{
		string directoryName = Path.GetDirectoryName(assetPath);
		if (string.IsNullOrEmpty(directoryName))
		{
			return assetPath;
		}
		string fileName = Path.GetFileName(directoryName);
		string fileName2 = Path.GetFileName(assetPath);
		return assetPath.Replace(fileName2, fileName);
	}

	public void LoadSpritesAsync(string[] assetPaths, Action<Sprite[]> callback)
	{
		if (!_isInitialized)
		{
			Debug.LogError((object)"[ResManager] 资源系统尚未初始化，请先调用Initialize方法");
			callback?.Invoke(null);
			return;
		}
		if (assetPaths == null || assetPaths.Length == 0)
		{
			Debug.LogError((object)"[ResManager] 图片资源路径数组为空");
			callback?.Invoke(null);
			return;
		}
		string[] array = new string[assetPaths.Length];
		for (int i = 0; i < assetPaths.Length; i++)
		{
			array[i] = CheckAssetPath(assetPaths[i], ".png");
		}
		((MonoBehaviour)this).StartCoroutine(LoadAssetsAsyncCoroutine<Sprite>(array, callback));
	}

	public void LoadPrefabSceneAsync(string assetPath, Action<GameObject> callback)
	{
		if (!_isInitialized)
		{
			Debug.LogError((object)"[ResManager] 资源系统尚未初始化，请先调用Initialize方法");
			callback?.Invoke(null);
		}
		else
		{
			assetPath = CheckAssetPath(assetPath);
			((MonoBehaviour)this).StartCoroutine(LoadAssetAsyncCoroutine<GameObject>(assetPath, callback));
		}
	}

	public void LoadPrefabAsync(string assetPath, Action<GameObject> callback)
	{
		if (!_isInitialized)
		{
			Debug.LogError((object)"[ResManager] 资源系统尚未初始化，请先调用Initialize方法");
			callback?.Invoke(null);
		}
		else
		{
			assetPath = CheckAssetPath(assetPath, ".prefab");
			((MonoBehaviour)this).StartCoroutine(LoadAssetAsyncCoroutine<GameObject>(assetPath, callback));
		}
	}

	public void LoadListPrefabAsync(string[] assetPaths, Action<GameObject[]> callback)
	{
		if (!_isInitialized)
		{
			Debug.LogError((object)"[ResManager] 资源系统尚未初始化，请先调用Initialize方法");
			callback?.Invoke(null);
			return;
		}
		if (assetPaths == null || assetPaths.Length == 0)
		{
			Debug.LogError((object)"[ResManager] 预制体资源路径数组为空");
			callback?.Invoke(null);
			return;
		}
		string[] array = new string[assetPaths.Length];
		for (int i = 0; i < assetPaths.Length; i++)
		{
			array[i] = CheckAssetPath(assetPaths[i], ".prefab");
		}
		((MonoBehaviour)this).StartCoroutine(LoadAssetsAsyncCoroutine<GameObject>(array, callback));
	}

	public void LoadSceneAsync(string scenePath, LoadSceneMode mode = (LoadSceneMode)0, Action<bool, string> callback = null, bool isSuspend = false)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if (!_isInitialized)
		{
			Debug.LogError((object)"[ResManager] 资源系统尚未初始化，请先调用Initialize方法");
			callback?.Invoke(arg1: false, "资源系统未初始化");
		}
		else
		{
			scenePath = CheckAssetPath(scenePath, ".scene");
			((MonoBehaviour)this).StartCoroutine(LoadSceneAsyncCoroutine(scenePath, mode, callback, isSuspend));
		}
	}

	public void LoadAudioClipAsync(string assetPath, Action<AudioClip> callback)
	{
		if (!_isInitialized)
		{
			Debug.LogError((object)"[ResManager] 资源系统尚未初始化，请先调用Initialize方法");
			callback?.Invoke(null);
		}
		else
		{
			assetPath = CheckAssetPath(assetPath);
			((MonoBehaviour)this).StartCoroutine(LoadAssetAsyncCoroutine<AudioClip>(assetPath, callback));
		}
	}

	public void LoadTextAssetAsync(string assetPath, Action<TextAsset> callback)
	{
		if (!_isInitialized)
		{
			Debug.LogError((object)"[ResManager] 资源系统尚未初始化，请先调用Initialize方法");
			callback?.Invoke(null);
		}
		else
		{
			assetPath = CheckAssetPath(assetPath);
			((MonoBehaviour)this).StartCoroutine(LoadAssetAsyncCoroutine<TextAsset>(assetPath, callback));
		}
	}

	public void LoadTMPFontAsync(string assetPath, Action<TMP_FontAsset> callback)
	{
		if (!_isInitialized)
		{
			Debug.LogError((object)"[ResManager] 资源系统尚未初始化，请先调用Initialize方法");
			callback?.Invoke(null);
		}
		else
		{
			assetPath = CheckAssetPath(assetPath);
			((MonoBehaviour)this).StartCoroutine(LoadAssetAsyncCoroutine<TMP_FontAsset>(assetPath, callback));
		}
	}

	public AssetHandle LoadAssetAsyncHandle<T>(string assetPath) where T : Object
	{
		if (!_isInitialized)
		{
			Debug.LogError((object)"[ResManager] 资源系统尚未初始化，请先调用Initialize方法");
			return null;
		}
		assetPath = CheckAssetPath(assetPath);
		float startTime;
		int logId = BeginResourceLoadLog<T>(assetPath, "handle", out startTime);
		AssetHandle val = _currentPackage.LoadAssetAsync<T>(assetPath, 0u);
		if (logId != 0)
		{
			val.Completed += delegate(AssetHandle completedHandle)
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0007: Invalid comparison between Unknown and I4
				bool success = (int)((HandleBase)completedHandle).Status == 2;
				EndResourceLoadLog(logId, assetPath, "handle", startTime, success, ((HandleBase)completedHandle).LastError);
			};
		}
		return val;
	}

	public void PreloadAssetBundle(string assetPath, Action<bool> callback)
	{
		if (!_isInitialized)
		{
			Debug.LogError((object)"[ResManager] 资源系统尚未初始化，请先调用Initialize方法");
			callback?.Invoke(obj: false);
		}
		else
		{
			assetPath = CheckAssetPath(assetPath);
			Debug.Log((object)("预加载路径" + assetPath));
			((MonoBehaviour)this).StartCoroutine(PreloadAssetBundleCoroutine(assetPath, callback));
		}
	}

	public int IsAssetLoaded(string assetPath)
	{
		if (!_isInitialized)
		{
			Debug.LogError((object)"[ResManager] 资源系统尚未初始化，请先调用Initialize方法");
			return 0;
		}
		assetPath = CheckAssetPath(assetPath);
		string key = ExtractAtlasPathFromPath(assetPath);
		if (_loadedAtlases.ContainsKey(key))
		{
			return 2;
		}
		if (_loadedAssets.ContainsKey(assetPath))
		{
			return _loadedAssets[assetPath];
		}
		return 0;
	}

	private IEnumerator PreloadAssetBundleCoroutine(string assetPath, Action<bool> callback)
	{
		ResourceDownloaderOperation val = YooAssets.CreateBundleDownloader(assetPath, 1, 3);
		((DownloaderOperation)val).DownloadFinishCallback = (DownloaderFinish)Delegate.Combine((Delegate)(object)((DownloaderOperation)val).DownloadFinishCallback, (Delegate)(DownloaderFinish)delegate(DownloaderFinishData status)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			Debug.Log((object)$"[ResManager] 预加载1：{assetPath} 状态：{status}");
			Debug.Log((object)("[ResManager] 预加载成功：" + assetPath));
			callback?.Invoke(obj: true);
		});
		((DownloaderOperation)val).DownloadErrorCallback = (DownloadError)Delegate.Combine((Delegate)(object)((DownloaderOperation)val).DownloadErrorCallback, (Delegate)(DownloadError)delegate(DownloadErrorData errorMsg)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			Debug.LogError((object)$"[ResManager] 预加载1：{assetPath} 错误：{errorMsg}");
			callback?.Invoke(obj: false);
		});
		((DownloaderOperation)val).BeginDownload();
		yield return null;
	}

	public IEnumerator LoadAllConfigCoroutine(string filePath, Action<Dictionary<string, TextAsset>> callback, string suffix = ".txt")
	{
		if (!_isInitialized)
		{
			Debug.LogError((object)"[ResManager] 资源系统尚未初始化，请先调用Initialize方法");
			callback?.Invoke(null);
			yield break;
		}
		Dictionary<string, TextAsset> allConfig = new Dictionary<string, TextAsset>();
		if (!filePath.EndsWith(suffix))
		{
			filePath += suffix;
		}
		AssetInfo assetInfo = _currentPackage.GetAssetInfo(filePath);
		if (assetInfo == null)
		{
			Debug.LogError((object)"[ResManager] 在路径 Assets/GameRes/Config/ 下未找到任何配置文件");
			callback?.Invoke(null);
			yield break;
		}
		float startTime;
		int logId = BeginResourceLoadLog<TextAsset>(filePath, "config-all", out startTime);
		AllAssetsHandle handle = _currentPackage.LoadAllAssetsAsync(assetInfo, 0u);
		yield return handle;
		if ((int)((HandleBase)handle).Status == 2)
		{
			EndResourceLoadLog(logId, filePath, "config-all", startTime, success: true);
			IReadOnlyList<Object> allAssetObjects = handle.AllAssetObjects;
			for (int i = 0; i < allAssetObjects.Count; i++)
			{
				string key = allAssetObjects[i].name.Replace(".json", "");
				Object obj = allAssetObjects[i];
				allConfig[key] = (TextAsset)(object)((obj is TextAsset) ? obj : null);
			}
			callback?.Invoke(allConfig);
		}
		else
		{
			EndResourceLoadLog(logId, filePath, "config-all", startTime, success: false, ((HandleBase)handle).LastError);
		}
	}

	public IEnumerator LoadConfigCoroutine(string assetPath, Action<TextAsset> callback, string suffix = ".txt")
	{
		if (!_isInitialized)
		{
			Debug.LogError((object)"[ResManager] 资源系统尚未初始化，请先调用Initialize方法");
			callback?.Invoke(null);
			yield break;
		}
		assetPath = CheckAssetPath(assetPath, suffix).Trim();
		float startTime;
		int logId = BeginResourceLoadLog<TextAsset>(assetPath, "config", out startTime);
		AssetHandle operation = _currentPackage.LoadAssetAsync<TextAsset>(assetPath, 0u);
		yield return operation;
		if ((int)((HandleBase)operation).Status == 2)
		{
			EndResourceLoadLog(logId, assetPath, "config", startTime, success: true);
			callback?.Invoke((TextAsset)/*isinst with value type is only supported in some contexts*/);
		}
		else
		{
			Debug.LogError((object)("[ResManager] 加载配置失败: " + assetPath + ", 错误: " + ((HandleBase)operation).LastError));
			EndResourceLoadLog(logId, assetPath, "config", startTime, success: false, ((HandleBase)operation).LastError);
			callback?.Invoke(null);
		}
	}

	private IEnumerator LoadAssetAsyncCoroutine<T>(string assetPath, Action<T> callback) where T : Object
	{
		if (_loadedAssets.ContainsKey(assetPath) && _loadedAssets[assetPath] == 1 && _loadedAssets.ContainsKey(assetPath) && _loadedAssets[assetPath] == 1)
		{
			LogResourceLoadState(assetPath, "wait-existing-load");
			if (!_loadingCallbacks.ContainsKey(assetPath))
			{
				_loadingCallbacks[assetPath] = new List<Delegate>();
			}
			_loadingCallbacks[assetPath].Add(callback);
			yield break;
		}
		if (_loadedAssets.ContainsKey(assetPath) && _loadedAssets[assetPath] == 2)
		{
			float cachedStartTime;
			int cachedLogId = BeginResourceLoadLog<T>(assetPath, "asset-cache", out cachedStartTime);
			AssetHandle cachedOperation = _currentPackage.LoadAssetAsync<T>(assetPath, 0u);
			yield return cachedOperation;
			if ((int)((HandleBase)cachedOperation).Status == 2)
			{
				Object assetObject = cachedOperation.AssetObject;
				T obj = (T)(object)((assetObject is T) ? assetObject : null);
				EndResourceLoadLog(cachedLogId, assetPath, "asset-cache", cachedStartTime, success: true);
				callback?.Invoke(obj);
			}
			else
			{
				EndResourceLoadLog(cachedLogId, assetPath, "asset-cache", cachedStartTime, success: false, ((HandleBase)cachedOperation).LastError);
				callback?.Invoke(default(T));
			}
			yield break;
		}
		_loadedAssets[assetPath] = 1;
		_loadedAssets[assetPath] = 1;
		float startTime;
		int logId = BeginResourceLoadLog<T>(assetPath, "asset", out startTime);
		AssetHandle operation = _currentPackage.LoadAssetAsync<T>(assetPath, 0u);
		yield return operation;
		if ((int)((HandleBase)operation).Status == 2)
		{
			Object assetObject2 = operation.AssetObject;
			T val = (T)(object)((assetObject2 is T) ? assetObject2 : null);
			EndResourceLoadLog(logId, assetPath, "asset", startTime, (Object)(object)val != (Object)null, ((Object)(object)val == (Object)null) ? "asset object is null" : null);
			callback?.Invoke(val);
			if ((Object)(object)val != (Object)null)
			{
				_loadedAssets[assetPath] = 2;
			}
			ExecuteAllWaitingCallbacks(assetPath, val);
		}
		else
		{
			Debug.LogError((object)("[ResManager] 加载资源失败: " + assetPath + ", 错误: " + ((HandleBase)operation).LastError));
			EndResourceLoadLog(logId, assetPath, "asset", startTime, success: false, ((HandleBase)operation).LastError);
			callback?.Invoke(default(T));
			_loadedAssets[assetPath] = 0;
			ExecuteAllWaitingCallbacks(assetPath, default(T));
		}
	}

	private void ExecuteAllWaitingCallbacks<T>(string assetPath, T asset) where T : Object
	{
		if (!_loadingCallbacks.ContainsKey(assetPath))
		{
			return;
		}
		List<Delegate> list = _loadingCallbacks[assetPath];
		if (list != null && list.Count > 0)
		{
			foreach (Delegate item in list)
			{
				if (item is Action<T> action)
				{
					action?.Invoke(asset);
				}
			}
		}
		_loadingCallbacks.Remove(assetPath);
	}

	private IEnumerator LoadAssetsAsyncCoroutine<T>(string[] assetPaths, Action<T[]> callback) where T : Object
	{
		if (assetPaths == null || assetPaths.Length == 0)
		{
			Debug.LogError((object)"[ResManager] 资源路径数组为空");
			callback?.Invoke(null);
			yield break;
		}
		float startTime = Time.time;
		Debug.Log((object)$"[ResManager] 开始批量加载 {assetPaths.Length} 个资源");
		T[] results = new T[assetPaths.Length];
		bool allSuccess = true;
		for (int i = 0; i < assetPaths.Length; i++)
		{
			string assetPath = assetPaths[i];
			if (!_loadedAssets.ContainsKey(assetPath))
			{
				_loadedAssets[assetPath] = 1;
			}
			Debug.Log((object)$"[ResManager] 开始加载队列资源 {i + 1}/{assetPaths.Length}: {assetPath}");
			float itemStartTime;
			int logId = BeginResourceLoadLog<T>(assetPath, "asset-list", out itemStartTime);
			AssetHandle operation = _currentPackage.LoadAssetAsync<T>(assetPath, 0u);
			yield return operation;
			bool flag;
			if ((int)((HandleBase)operation).Status == 2)
			{
				Object assetObject = operation.AssetObject;
				T val = (T)(object)((assetObject is T) ? assetObject : null);
				if ((Object)(object)val != (Object)null)
				{
					flag = true;
					results[i] = val;
					EndResourceLoadLog(logId, assetPath, "asset-list", itemStartTime, success: true);
					Debug.Log((object)$"[ResManager] 队列资源 {i + 1}/{assetPaths.Length} 加载成功: {assetPath}");
					_loadedAssets[assetPath] = 2;
				}
				else
				{
					flag = false;
					EndResourceLoadLog(logId, assetPath, "asset-list", itemStartTime, success: false, "asset object is null");
					Debug.LogError((object)$"[ResManager] 队列资源 {i + 1}/{assetPaths.Length} 加载失败：资源对象为空 - {assetPath}");
				}
			}
			else
			{
				flag = false;
				EndResourceLoadLog(logId, assetPath, "asset-list", itemStartTime, success: false, ((HandleBase)operation).LastError);
				Debug.LogError((object)$"[ResManager] 队列资源 {i + 1}/{assetPaths.Length} 加载失败: {assetPath}, 错误: {((HandleBase)operation).LastError}");
			}
			if (!flag)
			{
				allSuccess = false;
				break;
			}
		}
		float num = Time.time - startTime;
		if (allSuccess)
		{
			Debug.Log((object)$"[ResManager] 批量加载完成，共 {assetPaths.Length} 个资源，耗时 {num:F2} 秒，全部加载成功");
			callback?.Invoke(results);
		}
		else
		{
			Debug.LogError((object)$"[ResManager] 批量加载失败，部分资源加载错误，耗时 {num:F2} 秒");
			callback?.Invoke(null);
		}
	}

	private IEnumerator LoadSceneAsyncCoroutine(string scenePath, LoadSceneMode mode, Action<bool, string> callback, bool isSuspend = false)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		SceneHandle operation = _currentPackage.LoadSceneAsync(scenePath, mode, (LocalPhysicsMode)0, isSuspend, 0u);
		Debug.Log((object)"EnableAsyncOperationProgress");
		while (!((HandleBase)operation).IsDone)
		{
			OnDownloadProgressCallback?.Invoke(((HandleBase)operation).Progress);
			yield return null;
		}
		if ((int)((HandleBase)operation).Status == 2)
		{
			Debug.Log((object)("[ResManager] 场景加载成功: " + scenePath));
			callback?.Invoke(arg1: true, "场景加载成功");
		}
		else
		{
			string text = "[ResManager] 场景加载失败: " + scenePath + ", 错误: " + ((HandleBase)operation).LastError;
			Debug.LogError((object)text);
			callback?.Invoke(arg1: false, text);
		}
	}

	public void UnloadAsset(Object assetObject)
	{
		if (!_isInitialized || _currentPackage == null)
		{
			Debug.LogWarning((object)"[ResManager] 资源系统未初始化或资源包为空，无法卸载资源");
		}
	}

	public void UnloadUnusedAssets()
	{
		if (!_isInitialized || _currentPackage == null)
		{
			Debug.LogWarning((object)"[ResManager] 资源系统未初始化或资源包为空，无法卸载未使用资源");
		}
		else
		{
			((MonoBehaviour)this).StartCoroutine(UnloadUnusedAssetsCoroutine());
		}
	}

	private IEnumerator UnloadUnusedAssetsCoroutine()
	{
		UnloadUnusedAssetsOperation operation = _currentPackage.UnloadUnusedAssetsAsync(10);
		yield return operation;
		if ((int)((AsyncOperationBase)operation).Status == 2)
		{
			Debug.Log((object)"[ResManager] 未使用资源卸载成功");
		}
		else
		{
			Debug.LogError((object)("[ResManager] 未使用资源卸载失败: " + ((AsyncOperationBase)operation).Error));
		}
	}

	public void ClearCache()
	{
		if (!_isInitialized || _currentPackage == null)
		{
			Debug.LogWarning((object)"[ResManager] 资源系统未初始化或资源包为空，无法清理缓存");
		}
		else
		{
			((MonoBehaviour)this).StartCoroutine(ClearCacheCoroutine());
		}
	}

	private IEnumerator ClearCacheCoroutine()
	{
		ClearCacheFilesOperation operation = _currentPackage.ClearCacheFilesAsync((EFileClearMode)0, (object)null);
		yield return operation;
		if ((int)((AsyncOperationBase)operation).Status == 2)
		{
			Debug.Log((object)"[ResManager] 缓存清理成功");
		}
		else
		{
			Debug.LogError((object)("[ResManager] 缓存清理失败: " + ((AsyncOperationBase)operation).Error));
		}
	}

	public void Uninitialize()
	{
		if (_isInitialized && _currentPackage != null)
		{
			((MonoBehaviour)this).StartCoroutine(UninitializeCoroutine());
		}
	}

	private IEnumerator UninitializeCoroutine()
	{
		yield return _currentPackage.DestroyAsync();
		if (YooAssets.RemovePackage(_currentPackage))
		{
			Debug.Log((object)"[ResManager] 资源包移除成功");
		}
		_currentPackage = null;
		_isInitialized = false;
		_loadingCallbacks.Clear();
		_loadedAssets.Clear();
		_resourceLoadRequestCounts.Clear();
		_resourceLoadLogId = 0;
		YooAssets.Destroy();
		Debug.Log((object)"[ResManager] YooAssets系统已销毁");
	}

	public void RequestPackageVersion(Action<bool, string> callback)
	{
		if (!_isInitialized || _currentPackage == null)
		{
			Debug.LogError((object)"[ResManager] 资源系统尚未初始化，请先调用Initialize方法");
			callback?.Invoke(arg1: false, "资源系统未初始化");
		}
		else
		{
			((MonoBehaviour)this).StartCoroutine(RequestPackageVersionCoroutine(callback));
		}
	}

	private IEnumerator RequestPackageVersionCoroutine(Action<bool, string> callback)
	{
		OnBeforeRequestPackageVersion?.Invoke();
		RequestPackageVersionOperation operation = _currentPackage.RequestPackageVersionAsync(true, 60);
		yield return operation;
		if ((int)((AsyncOperationBase)operation).Status == 2)
		{
			string packageVersion = operation.PackageVersion;
			Debug.Log((object)("[ResManager] 资源版本获取成功: " + packageVersion));
			callback?.Invoke(arg1: true, packageVersion);
			OnAfterRequestPackageVersion?.Invoke(arg1: true, packageVersion);
		}
		else
		{
			string text = "[ResManager] 资源版本获取失败: " + ((AsyncOperationBase)operation).Error;
			Debug.LogError((object)text);
			callback?.Invoke(arg1: false, text);
			OnAfterRequestPackageVersion?.Invoke(arg1: false, text);
		}
	}

	public void UpdatePackageManifest(string packageVersion, Action<bool, string> callback)
	{
		if (!_isInitialized || _currentPackage == null)
		{
			Debug.LogError((object)"[ResManager] 资源系统尚未初始化，请先调用Initialize方法");
			callback?.Invoke(arg1: false, "资源系统未初始化");
		}
		else
		{
			((MonoBehaviour)this).StartCoroutine(UpdatePackageManifestCoroutine(packageVersion, callback));
		}
	}

	private IEnumerator UpdatePackageManifestCoroutine(string packageVersion, Action<bool, string> callback)
	{
		OnBeforeUpdatePackageManifest?.Invoke(packageVersion);
		UpdatePackageManifestOperation operation = _currentPackage.UpdatePackageManifestAsync(packageVersion, 60);
		yield return operation;
		if ((int)((AsyncOperationBase)operation).Status == 2)
		{
			Debug.Log((object)"[ResManager] 资源清单更新成功");
			callback?.Invoke(arg1: true, "资源清单更新成功");
			OnAfterUpdatePackageManifest?.Invoke(arg1: true, "资源清单更新成功");
		}
		else
		{
			string text = "[ResManager] 资源清单更新失败: " + ((AsyncOperationBase)operation).Error;
			Debug.LogError((object)text);
			callback?.Invoke(arg1: false, text);
			OnAfterUpdatePackageManifest?.Invoke(arg1: false, text);
		}
	}
}
