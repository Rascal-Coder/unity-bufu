using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
	private static SoundManager instance;

	private Dictionary<string, AudioClip> soundPool = new Dictionary<string, AudioClip>();

	private List<AudioSource> soundSources = new List<AudioSource>();

	private AudioSource bgmSource;

	private float soundVolume = 1f;

	private float bgmVolume = 0.5f;

	private const string SOUND_VOLUME_KEY = "SoundVolume";

	private const string BGM_VOLUME_KEY = "BGMVolume";

	private const string BGM_ISOPEN = "BGMIsOpen";

	private const string Sound_ISOPEN = "SoundIsOpen";

	public bool EnableCoinSound = true;

	private Dictionary<GameObject, float> lastPlayTimeDict = new Dictionary<GameObject, float>();

	private const float SOUND_COOLDOWN = 0.5f;

	private Dictionary<string, int> soundInstanceCountDict = new Dictionary<string, int>();

	private const int MAX_SOUND_INSTANCES = 3;

	private Dictionary<string, List<float>> soundEndTimeDict = new Dictionary<string, List<float>>();

	private List<string> bgSounds = new List<string> { "Assets/GameRes/sound/bg/bg1.mp3", "Assets/GameRes/sound/bg/bg2.mp3", "Assets/GameRes/sound/bg/dungeon1.mp3", "Assets/GameRes/sound/bg/dungeon2.mp3", "Assets/GameRes/sound/bg/dungeon3.mp3" };

	private string currentBgmName = string.Empty;

	public static SoundManager Instance
	{
		get
		{
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Expected O, but got Unknown
			if ((Object)(object)instance == (Object)null)
			{
				instance = Object.FindObjectOfType<SoundManager>();
				if ((Object)(object)instance == (Object)null)
				{
					GameObject val = new GameObject("SoundManager");
					instance = val.AddComponent<SoundManager>();
					Object.DontDestroyOnLoad((Object)val);
				}
			}
			return instance;
		}
	}

	private void Awake()
	{
		if ((Object)(object)instance == (Object)null)
		{
			instance = this;
			Object.DontDestroyOnLoad((Object)(object)((Component)this).gameObject);
			Initialize();
		}
		else if ((Object)(object)instance != (Object)(object)this)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
	}

	public void Initialize()
	{
		LoadVolumeSettings();
		bgmSource = ((Component)this).gameObject.AddComponent<AudioSource>();
		bgmSource.loop = true;
		InitPlayerPrefs();
		bgmSource.volume = bgmVolume;
		for (int i = 0; i < 5; i++)
		{
			AudioSource val = ((Component)this).gameObject.AddComponent<AudioSource>();
			val.volume = soundVolume;
			val.playOnAwake = false;
			soundSources.Add(val);
		}
		if (PlayerPrefs.GetInt("BGMIsOpen", 0) == 0)
		{
			StopBGMNoPlayerPrefs();
		}
		if (PlayerPrefs.GetInt("SoundIsOpen", 0) == 0)
		{
			StopSoundNoPlayerPrefs();
		}
	}

	public void PlaySound(string soundName, float volume = -1f, GameObject sourceObject = null)
	{
		if ((Object)(object)sourceObject != (Object)null)
		{
			if (lastPlayTimeDict.TryGetValue(sourceObject, out var value) && Time.time - value < 0.5f)
			{
				return;
			}
			lastPlayTimeDict[sourceObject] = Time.time;
		}
		int value2 = 0;
		if (soundInstanceCountDict.TryGetValue(soundName, out value2) && value2 >= 3)
		{
			return;
		}
		if (!soundPool.TryGetValue(soundName, out var value3))
		{
			SingletonMono<ResManager>.Instance.LoadAudioClipAsync(soundName, delegate(AudioClip clip)
			{
				if ((Object)(object)clip != (Object)null)
				{
					soundPool[soundName] = clip;
					PlaySound(soundName, volume, sourceObject);
				}
			});
			return;
		}
		AudioSource availableSoundSource = GetAvailableSoundSource();
		if ((Object)(object)availableSoundSource != (Object)null)
		{
			soundInstanceCountDict[soundName] = value2 + 1;
			availableSoundSource.clip = value3;
			availableSoundSource.volume = ((volume >= 0f) ? volume : soundVolume);
			availableSoundSource.Play();
			float item = Time.time + value3.length;
			if (!soundEndTimeDict.ContainsKey(soundName))
			{
				soundEndTimeDict[soundName] = new List<float>();
			}
			soundEndTimeDict[soundName].Add(item);
		}
	}

	private void Update()
	{
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, List<float>> item in soundEndTimeDict)
		{
			string key = item.Key;
			List<float> value = item.Value;
			List<float> list2 = new List<float>();
			foreach (float item2 in value)
			{
				if (!(Time.time >= item2))
				{
					continue;
				}
				list2.Add(item2);
				int value2 = 0;
				if (soundInstanceCountDict.TryGetValue(key, out value2) && value2 > 0)
				{
					value2--;
					if (value2 == 0)
					{
						soundInstanceCountDict.Remove(key);
						list.Add(key);
					}
					else
					{
						soundInstanceCountDict[key] = value2;
					}
				}
			}
			foreach (float item3 in list2)
			{
				value.Remove(item3);
			}
		}
		foreach (string item4 in list)
		{
			if (soundEndTimeDict.ContainsKey(item4) && soundEndTimeDict[item4].Count == 0)
			{
				soundEndTimeDict.Remove(item4);
			}
		}
	}

	public void PlayBGM(string bgmName, float volume = -1f)
	{
		if (string.IsNullOrEmpty(bgmName))
		{
			return;
		}
		currentBgmName = bgmName;
		if (!soundPool.TryGetValue(bgmName, out var value))
		{
			SingletonMono<ResManager>.Instance.LoadAudioClipAsync(bgmName, delegate(AudioClip loadedClip)
			{
				if ((Object)(object)loadedClip != (Object)null && currentBgmName == bgmName)
				{
					soundPool[bgmName] = loadedClip;
					PlayBGM(bgmName, volume);
				}
			});
		}
		else
		{
			bgmSource.clip = value;
			bgmSource.volume = ((volume >= 0f) ? volume : bgmVolume);
			bgmSource.loop = true;
			bgmSource.Play();
		}
	}

	public void PlayBGMForWorld(int worldId)
	{
		string text = ((worldId == 2) ? "Assets/GameRes/sound/bg/diceBGM.mp3" : "Assets/GameRes/sound/bg/BGM.mp3");
		if (!(text == currentBgmName))
		{
			PlayBGM(text);
		}
	}

	public void StopBGM()
	{
		if ((Object)(object)bgmSource != (Object)null)
		{
			bgmSource.Stop();
		}
	}

	public void SetSoundVolume(float volume)
	{
		soundVolume = Mathf.Clamp01(volume);
		foreach (AudioSource soundSource in soundSources)
		{
			if (PlayerPrefs.GetInt("SoundIsOpen", 0) == 1)
			{
				soundSource.volume = soundVolume;
			}
			else
			{
				soundSource.volume = 0f;
			}
		}
		PlayerPrefs.SetFloat("SoundVolume", soundVolume);
		PlayerPrefs.Save();
	}

	public void SetBGMVolume(float volume)
	{
		bgmVolume = Mathf.Clamp(volume, 0f, 0.5f);
		if ((Object)(object)bgmSource != (Object)null)
		{
			if (PlayerPrefs.GetInt("BGMIsOpen", 0) == 1)
			{
				bgmSource.volume = bgmVolume;
			}
			else
			{
				bgmSource.volume = 0f;
			}
		}
		PlayerPrefs.SetFloat("BGMVolume", bgmVolume);
		PlayerPrefs.Save();
	}

	public float GetSoundVolume()
	{
		if (PlayerPrefs.HasKey("SoundVolume"))
		{
			soundVolume = PlayerPrefs.GetFloat("SoundVolume", 0f);
		}
		return soundVolume;
	}

	public float GetBGMVolume()
	{
		if (PlayerPrefs.HasKey("BGMVolume"))
		{
			bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 0f);
		}
		return bgmVolume;
	}

	private void LoadVolumeSettings()
	{
		if (PlayerPrefs.HasKey("SoundVolume"))
		{
			soundVolume = PlayerPrefs.GetFloat("SoundVolume", 0f);
		}
		if (PlayerPrefs.HasKey("BGMVolume"))
		{
			bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 0f);
		}
	}

	private AudioSource GetAvailableSoundSource()
	{
		foreach (AudioSource soundSource in soundSources)
		{
			if (!soundSource.isPlaying)
			{
				return soundSource;
			}
		}
		AudioSource val = ((Component)this).gameObject.AddComponent<AudioSource>();
		val.volume = soundVolume;
		val.playOnAwake = false;
		soundSources.Add(val);
		return val;
	}

	public void StopBGMNoPlayerPrefs()
	{
		bgmVolume = 0f;
		bgmSource.volume = bgmVolume;
		PlayerPrefs.SetInt("BGMIsOpen", 0);
	}

	public void ResetBGMPlayerPrefs()
	{
		if (PlayerPrefs.HasKey("BGMVolume"))
		{
			bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 0f);
		}
		bgmSource.volume = bgmVolume;
		PlayerPrefs.SetInt("BGMIsOpen", 1);
	}

	public void StopSoundNoPlayerPrefs()
	{
		soundVolume = 0f;
		foreach (AudioSource soundSource in soundSources)
		{
			soundSource.volume = 0f;
		}
		PlayerPrefs.SetInt("SoundIsOpen", 0);
	}

	public void ResetSoundPlayerPrefs()
	{
		if (PlayerPrefs.HasKey("SoundVolume"))
		{
			soundVolume = PlayerPrefs.GetFloat("SoundVolume", 0f);
		}
		foreach (AudioSource soundSource in soundSources)
		{
			soundSource.volume = soundVolume;
		}
		PlayerPrefs.SetInt("SoundIsOpen", 1);
	}

	public bool GetMusicIsOn()
	{
		return PlayerPrefs.GetInt("BGMIsOpen", 0) == 1;
	}

	public bool GetSoundIsOn()
	{
		return PlayerPrefs.GetInt("SoundIsOpen", 0) == 1;
	}

	private void InitPlayerPrefs()
	{
		if (PlayerPrefs.HasKey("BGMVolume"))
		{
			bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 0f);
		}
		else
		{
			PlayerPrefs.SetFloat("BGMVolume", bgmVolume);
		}
		if (PlayerPrefs.HasKey("SoundVolume"))
		{
			soundVolume = PlayerPrefs.GetFloat("SoundVolume", 0f);
		}
		else
		{
			PlayerPrefs.SetFloat("SoundVolume", soundVolume);
		}
		if (!PlayerPrefs.HasKey("BGMIsOpen"))
		{
			PlayerPrefs.SetInt("BGMIsOpen", 1);
		}
		if (!PlayerPrefs.HasKey("SoundIsOpen"))
		{
			PlayerPrefs.SetInt("SoundIsOpen", 1);
		}
	}
}
