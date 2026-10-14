using System;
using UnityEngine;
using UnityEngine.UI;

namespace Module.UI;

public class UISpriteAnimationSequence : MonoBehaviour
{
	[Header("动画设置")]
	[Tooltip("序列帧图片数组")]
	[SerializeField]
	private Sprite[] spriteFrames;

	[Tooltip("帧间隔时间（秒）")]
	[Range(0.01f, 1f)]
	[SerializeField]
	private float frameInterval = 0.04f;

	[Tooltip("是否循环播放")]
	[SerializeField]
	private bool isLooping = true;

	[Tooltip("是否自动播放")]
	[SerializeField]
	private bool autoPlay = true;

	[Header("状态显示")]
	[Tooltip("当前是否播放中")]
	[SerializeField]
	private bool isPlaying;

	[Tooltip("当前帧索引")]
	[SerializeField]
	private int currentFrameIndex;

	private Image image;

	private float timer;

	private bool hasInitialized;

	public Action OnAnimationComplete;

	public Sprite[] SpriteFrames
	{
		get
		{
			return spriteFrames;
		}
		set
		{
			spriteFrames = value;
			currentFrameIndex = 0;
			if (spriteFrames != null && spriteFrames.Length != 0)
			{
				ApplySprite(spriteFrames[0]);
			}
		}
	}

	public float FrameInterval
	{
		get
		{
			return frameInterval;
		}
		set
		{
			frameInterval = Mathf.Max(0.01f, value);
		}
	}

	public bool IsLooping
	{
		get
		{
			return isLooping;
		}
		set
		{
			isLooping = value;
		}
	}

	public bool AutoPlay
	{
		get
		{
			return autoPlay;
		}
		set
		{
			autoPlay = value;
		}
	}

	public bool IsPlaying => isPlaying;

	public int CurrentFrameIndex
	{
		get
		{
			return currentFrameIndex;
		}
		set
		{
			if (spriteFrames != null && spriteFrames.Length != 0)
			{
				currentFrameIndex = Mathf.Clamp(value, 0, spriteFrames.Length - 1);
				ApplySprite(spriteFrames[currentFrameIndex]);
			}
		}
	}

	public int FrameCount
	{
		get
		{
			Sprite[] array = spriteFrames;
			if (array == null)
			{
				return 0;
			}
			return array.Length;
		}
	}

	private void Awake()
	{
		Initialize();
	}

	private void Start()
	{
		if (autoPlay && spriteFrames != null && spriteFrames.Length != 0)
		{
			Play();
		}
	}

	private void Update()
	{
		if (isPlaying && spriteFrames != null && spriteFrames.Length != 0)
		{
			UpdateAnimation();
		}
	}

	private void Initialize()
	{
		if (!hasInitialized)
		{
			image = ((Component)this).gameObject.GetOrAddComponent<Image>();
			if (spriteFrames != null && spriteFrames.Length != 0)
			{
				ApplySprite(spriteFrames[0]);
			}
			hasInitialized = true;
		}
	}

	private void ApplySprite(Sprite sprite)
	{
		if ((Object)(object)image != (Object)null)
		{
			image.sprite = sprite;
		}
	}

	private void UpdateAnimation()
	{
		timer += Time.deltaTime;
		if (!(timer >= frameInterval))
		{
			return;
		}
		timer = 0f;
		currentFrameIndex++;
		if (currentFrameIndex >= spriteFrames.Length)
		{
			if (!isLooping)
			{
				currentFrameIndex = spriteFrames.Length - 1;
				isPlaying = false;
				OnAnimationComplete?.Invoke();
				return;
			}
			currentFrameIndex = 0;
		}
		ApplySprite(spriteFrames[currentFrameIndex]);
	}

	public void Play()
	{
		if (spriteFrames == null || spriteFrames.Length == 0)
		{
			Debug.LogWarning((object)"没有设置序列帧，无法播放动画！", (Object)(object)this);
			return;
		}
		Initialize();
		isPlaying = true;
	}

	public void Pause()
	{
		isPlaying = false;
	}

	public void Stop()
	{
		isPlaying = false;
		currentFrameIndex = 0;
		timer = 0f;
		if (spriteFrames != null && spriteFrames.Length != 0)
		{
			ApplySprite(spriteFrames[0]);
		}
	}

	public void Restart()
	{
		Stop();
		Play();
	}

	public void JumpToFrame(int frameIndex)
	{
		CurrentFrameIndex = frameIndex;
		timer = 0f;
	}

	public void ShowStaticFrame(int frameIndex)
	{
		Initialize();
		isPlaying = false;
		CurrentFrameIndex = frameIndex;
		timer = 0f;
	}

	public void ShowFrontFace()
	{
		if (FrameCount > 0)
		{
			ShowStaticFrame(FrameCount - 1);
		}
	}

	public void ShowBackFace()
	{
		ShowStaticFrame(0);
	}

	public void SetSpeed(float speedMultiplier)
	{
		if (speedMultiplier > 0f)
		{
			FrameInterval = frameInterval / speedMultiplier;
		}
		else
		{
			Debug.LogWarning((object)"速度倍率必须大于0！", (Object)(object)this);
		}
	}
}
