using System;

public abstract class SingletonBase<T> where T : class, new()
{
	protected static T _instance;

	private static readonly object _lockObject = new object();

	public static T Instance
	{
		get
		{
			if (_instance == null)
			{
				lock (_lockObject)
				{
					if (_instance == null)
					{
						_instance = new T();
					}
				}
			}
			return _instance;
		}
	}

	protected SingletonBase()
	{
		if (_instance != null)
		{
			throw new InvalidOperationException("单例类不应该直接被实例化。");
		}
	}
}
