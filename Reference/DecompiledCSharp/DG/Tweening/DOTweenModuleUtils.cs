using System;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Core.PathCore;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.Scripting;

namespace DG.Tweening;

public static class DOTweenModuleUtils
{
	public static class Physics
	{
		public static void SetOrientationOnPath(PathOptions options, Tween t, Quaternion newRot, Transform trans)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			if (options.isRigidbody)
			{
				((Rigidbody)t.target).rotation = newRot;
			}
			else
			{
				trans.rotation = newRot;
			}
		}

		public static bool HasRigidbody2D(Component target)
		{
			return (Object)(object)target.GetComponent<Rigidbody2D>() != (Object)null;
		}

		[Preserve]
		public static bool HasRigidbody(Component target)
		{
			return (Object)(object)target.GetComponent<Rigidbody>() != (Object)null;
		}

		[Preserve]
		public static TweenerCore<Vector3, Path, PathOptions> CreateDOTweenPathTween(MonoBehaviour target, bool tweenRigidbody, bool isLocal, Path path, float duration, PathMode pathMode)
		{
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			TweenerCore<Vector3, Path, PathOptions> result = null;
			bool flag = false;
			if (tweenRigidbody)
			{
				Rigidbody component = ((Component)target).GetComponent<Rigidbody>();
				if ((Object)(object)component != (Object)null)
				{
					flag = true;
					result = (isLocal ? component.DOLocalPath(path, duration, pathMode) : component.DOPath(path, duration, pathMode));
				}
			}
			if (!flag && tweenRigidbody)
			{
				Rigidbody2D component2 = ((Component)target).GetComponent<Rigidbody2D>();
				if ((Object)(object)component2 != (Object)null)
				{
					flag = true;
					result = (isLocal ? component2.DOLocalPath(path, duration, pathMode) : component2.DOPath(path, duration, pathMode));
				}
			}
			if (!flag)
			{
				result = (isLocal ? ShortcutExtensions.DOLocalPath(((Component)target).transform, path, duration, pathMode) : ShortcutExtensions.DOPath(((Component)target).transform, path, duration, pathMode));
			}
			return result;
		}
	}

	private static bool _initialized;

	[Preserve]
	public static void Init()
	{
		if (!_initialized)
		{
			_initialized = true;
			DOTweenExternalCommand.SetOrientationOnPath += Physics.SetOrientationOnPath;
		}
	}

	[Preserve]
	private static void Preserver()
	{
		AppDomain.CurrentDomain.GetAssemblies();
		typeof(MonoBehaviour).GetMethod("Stub");
	}
}
