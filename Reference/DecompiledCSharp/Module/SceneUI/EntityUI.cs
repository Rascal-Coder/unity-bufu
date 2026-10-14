using System;
using System.Collections.Generic;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace Module.SceneUI;

public class EntityUI : MonoBehaviour
{
	private float _health;

	private float _maxHealth;

	[SerializeField]
	protected SkeletonDataAsset skeletonDataAsset;

	public SkeletonGraphic skeletonGraphic;

	protected Skeleton skeleton;

	protected AnimationState animationState;

	protected ScheduleManager scheduleManager;

	protected string preAnimationName;

	[SerializeField]
	private ExposedList<Skin> allSkins;

	protected Dictionary<string, Skin> _skinDic = new Dictionary<string, Skin>();

	public virtual float health
	{
		get
		{
			return _health;
		}
		set
		{
			_health = value;
		}
	}

	public virtual float maxHealth
	{
		get
		{
			return _maxHealth;
		}
		set
		{
			_maxHealth = value;
		}
	}

	public virtual int level { get; set; } = 1;

	public event Action<float, GameObject> OnTakeDamage;

	protected virtual void Awake()
	{
		scheduleManager = new ScheduleManager();
		allSkins = new ExposedList<Skin>();
	}

	protected virtual void Start()
	{
	}

	protected virtual void Update()
	{
		scheduleManager?.Update(Time.deltaTime);
	}

	protected virtual void OnDestroy()
	{
		scheduleManager?.ClearAllSchedules();
	}

	public void AddSchedule(ISchedule schedule)
	{
		scheduleManager?.AddSchedule(schedule);
	}

	public void RemoveSchedule(ISchedule schedule)
	{
		scheduleManager?.RemoveSchedule(schedule);
	}

	public virtual float TakeDamage(float damage, bool isCritical = false)
	{
		this.OnTakeDamage?.Invoke(damage, null);
		return 0f;
	}

	public virtual void Die(float time = 1f)
	{
		Object.Destroy((Object)(object)((Component)this).gameObject, time);
	}

	public virtual void InitializeAnimation()
	{
		if ((Object)(object)skeletonDataAsset != (Object)null)
		{
			InitializeAnimation(skeletonDataAsset);
		}
	}

	public virtual void InitializeAnimation(SkeletonDataAsset dataAsset)
	{
		if ((Object)(object)skeletonGraphic == (Object)null)
		{
			skeletonGraphic = ((Component)this).gameObject.GetOrAddComponent<SkeletonGraphic>();
		}
		skeletonGraphic.skeletonDataAsset = dataAsset;
		skeletonGraphic.Initialize(true);
		skeleton = skeletonGraphic.Skeleton;
		animationState = skeletonGraphic.AnimationState;
		allSkins = skeleton.Data.Skins;
		AnalyzeAllSkins();
	}

	public virtual void PlayAnimation(string animationName, bool loop = false, float timeScale = 1f)
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Expected O, but got Unknown
		if (animationState != null)
		{
			string currentAnimationName = GetCurrentAnimationName();
			TrackEntry val;
			if (currentAnimationName == "attack" || currentAnimationName == "hit")
			{
				val = animationState.AddAnimation(0, animationName, loop, 0.2f);
			}
			else
			{
				val = animationState.SetAnimation(0, animationName, loop);
				animationState.TimeScale = timeScale;
			}
			if (!loop)
			{
				preAnimationName = animationName;
				val.Complete += new TrackEntryDelegate(OnFinishAnimation);
			}
		}
	}

	public virtual void OnFinishAnimation(TrackEntry trackEntry)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		if (trackEntry.Animation.Name == preAnimationName)
		{
			PlayAnimation("idle", loop: true);
		}
		trackEntry.Complete -= new TrackEntryDelegate(OnFinishAnimation);
		preAnimationName = string.Empty;
	}

	public virtual string GetCurrentAnimationName()
	{
		if (animationState != null && animationState.GetCurrent(0) != null)
		{
			return animationState.GetCurrent(0).Animation.Name;
		}
		return null;
	}

	public virtual void SetAnimationSpeed(float timeScale)
	{
		if (animationState != null)
		{
			animationState.TimeScale = timeScale;
		}
	}

	public virtual void SetAnimationFill(float value)
	{
		if (!((Object)(object)skeletonGraphic == (Object)null))
		{
			Material materialForRendering = ((Graphic)skeletonGraphic).materialForRendering;
			if ((Object)(object)materialForRendering != (Object)null)
			{
				materialForRendering.SetFloat("_FillPhase", value);
			}
		}
	}

	public virtual void SetAnimationFill(float value, Color color)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)skeletonGraphic == (Object)null))
		{
			Material materialForRendering = ((Graphic)skeletonGraphic).materialForRendering;
			if (!((Object)(object)materialForRendering == (Object)null))
			{
				materialForRendering.SetFloat("_FillPhase", value);
				materialForRendering.SetColor("_FillColor", color);
			}
		}
	}

	public virtual void WarpPosition(Vector3 position)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		Transform transform = ((Component)this).transform;
		RectTransform val = (RectTransform)(object)((transform is RectTransform) ? transform : null);
		if ((Object)(object)val != (Object)null)
		{
			val.anchoredPosition3D = position;
		}
		else
		{
			((Component)this).transform.position = position;
		}
	}

	public virtual void AnalyzeAllSkins()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)skeletonGraphic == (Object)null || skeletonGraphic.Skeleton == null)
		{
			return;
		}
		SkeletonData data = skeletonGraphic.Skeleton.Data;
		Enumerator<Skin> enumerator = data.Skins.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				Skin current = enumerator.Current;
				_skinDic[current.Name] = current;
				Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>();
				foreach (KeyValuePair<SkinEntry, Attachment> attachment in current.Attachments)
				{
					SkinEntry key = attachment.Key;
					_ = attachment.Value;
					string name = data.Slots.Items[((SkinEntry)(ref key)).SlotIndex].Name;
					if (!dictionary.ContainsKey(name))
					{
						dictionary[name] = new List<string>();
					}
					dictionary[name].Add(((SkinEntry)(ref key)).Name);
				}
				foreach (KeyValuePair<string, List<string>> item in dictionary)
				{
					string key2 = item.Key;
					List<string> value = item.Value;
					string text = string.Join(", ", value);
					Debug.Log((object)("  部位 " + key2 + ": " + text));
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to .constrained prefix*/).Dispose();
		}
		Debug.Log((object)"\n=== 分析完成 ===");
	}

	protected void ChangeCharacterPart(string partName, Skin partSkin)
	{
		if (!((Object)(object)skeletonGraphic == (Object)null))
		{
			Skeleton val = skeletonGraphic.Skeleton;
			_ = val.Data;
			if (partSkin == null)
			{
				Debug.LogError((object)("Skin " + partName + " not found!"));
				return;
			}
			val.SetSkin(partSkin);
			val.SetSlotsToSetupPose();
			skeletonGraphic.AnimationState.Apply(val);
			skeletonGraphic.Update(0f);
		}
	}
}
