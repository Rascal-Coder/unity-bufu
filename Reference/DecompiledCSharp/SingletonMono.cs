using UnityEngine;

public abstract class SingletonMono<T> : MonoBehaviour where T : MonoBehaviour
{
	private static T instance;

	private static string Managers = "Managers";

	public static T Instance
	{
		get
		{
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Expected O, but got Unknown
			if ((Object)(object)instance == (Object)null)
			{
				instance = Object.FindObjectOfType<T>();
				if ((Object)(object)instance == (Object)null)
				{
					if (!Application.isPlaying)
					{
						return default(T);
					}
					instance = ExtendedMono.GetOrAddComponent<T>(new GameObject(typeof(T).ToString()));
				}
			}
			return instance;
		}
	}

	public abstract bool DontDestory { get; }

	private void Awake()
	{
		if ((Object)(object)instance == (Object)null)
		{
			instance = (T)(object)((this is T) ? this : null);
		}
		if (DontDestory)
		{
			if ((Object)(object)((Component)this).transform.root != (Object)(object)((Component)this).transform)
			{
				Object.DontDestroyOnLoad((Object)(object)((Component)((Component)this).transform.root).gameObject);
			}
			else
			{
				Object.DontDestroyOnLoad((Object)(object)((Component)this).gameObject);
			}
		}
		OnAwake();
	}

	public static bool HasInstance()
	{
		return (Object)(object)instance != (Object)null;
	}

	private void OnDestroy()
	{
		BeforOnDestroy();
	}

	protected abstract void OnAwake();

	protected abstract void BeforOnDestroy();
}
