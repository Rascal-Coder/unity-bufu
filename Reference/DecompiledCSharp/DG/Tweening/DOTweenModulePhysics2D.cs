using DG.Tweening.Core;
using DG.Tweening.Plugins;
using DG.Tweening.Plugins.Core.PathCore;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace DG.Tweening;

public static class DOTweenModulePhysics2D
{
	public static TweenerCore<Vector2, Vector2, VectorOptions> DOMove(this Rigidbody2D target, Vector2 endValue, float duration, bool snapping = false)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		TweenerCore<Vector2, Vector2, VectorOptions> obj = DOTween.To((DOGetter<Vector2>)(() => target.position), (DOSetter<Vector2>)target.MovePosition, endValue, duration);
		TweenSettingsExtensions.SetTarget<Tweener>(TweenSettingsExtensions.SetOptions(obj, snapping), (object)target);
		return obj;
	}

	public static TweenerCore<Vector2, Vector2, VectorOptions> DOMoveX(this Rigidbody2D target, float endValue, float duration, bool snapping = false)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		TweenerCore<Vector2, Vector2, VectorOptions> obj = DOTween.To((DOGetter<Vector2>)(() => target.position), (DOSetter<Vector2>)target.MovePosition, new Vector2(endValue, 0f), duration);
		TweenSettingsExtensions.SetTarget<Tweener>(TweenSettingsExtensions.SetOptions(obj, (AxisConstraint)2, snapping), (object)target);
		return obj;
	}

	public static TweenerCore<Vector2, Vector2, VectorOptions> DOMoveY(this Rigidbody2D target, float endValue, float duration, bool snapping = false)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		TweenerCore<Vector2, Vector2, VectorOptions> obj = DOTween.To((DOGetter<Vector2>)(() => target.position), (DOSetter<Vector2>)target.MovePosition, new Vector2(0f, endValue), duration);
		TweenSettingsExtensions.SetTarget<Tweener>(TweenSettingsExtensions.SetOptions(obj, (AxisConstraint)4, snapping), (object)target);
		return obj;
	}

	public static TweenerCore<float, float, FloatOptions> DORotate(this Rigidbody2D target, float endValue, float duration)
	{
		TweenerCore<float, float, FloatOptions> obj = DOTween.To((DOGetter<float>)(() => target.rotation), (DOSetter<float>)target.MoveRotation, endValue, duration);
		TweenSettingsExtensions.SetTarget<TweenerCore<float, float, FloatOptions>>(obj, (object)target);
		return obj;
	}

	public static Sequence DOJump(this Rigidbody2D target, Vector2 endValue, float jumpPower, int numJumps, float duration, bool snapping = false)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Expected O, but got Unknown
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Expected O, but got Unknown
		if (numJumps < 1)
		{
			numJumps = 1;
		}
		float startPosY = 0f;
		float offsetY = -1f;
		bool offsetYSet = false;
		Sequence s = DOTween.Sequence();
		Tween yTween = (Tween)(object)TweenSettingsExtensions.OnStart<Tweener>(TweenSettingsExtensions.SetLoops<Tweener>(TweenSettingsExtensions.SetRelative<Tweener>(TweenSettingsExtensions.SetEase<Tweener>(TweenSettingsExtensions.SetOptions(DOTween.To((DOGetter<Vector2>)(() => target.position), (DOSetter<Vector2>)delegate(Vector2 x)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			target.position = x;
		}, new Vector2(0f, jumpPower), duration / (float)(numJumps * 2)), (AxisConstraint)4, snapping), (Ease)6)), numJumps * 2, (LoopType)1), (TweenCallback)delegate
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			startPosY = target.position.y;
		});
		TweenSettingsExtensions.SetEase<Sequence>(TweenSettingsExtensions.SetTarget<Sequence>(TweenSettingsExtensions.Join(TweenSettingsExtensions.Append(s, (Tween)(object)TweenSettingsExtensions.SetEase<Tweener>(TweenSettingsExtensions.SetOptions(DOTween.To((DOGetter<Vector2>)(() => target.position), (DOSetter<Vector2>)delegate(Vector2 x)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			target.position = x;
		}, new Vector2(endValue.x, 0f), duration), (AxisConstraint)2, snapping), (Ease)1)), yTween), (object)target), DOTween.defaultEaseType);
		TweenSettingsExtensions.OnUpdate<Tween>(yTween, (TweenCallback)delegate
		{
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			if (!offsetYSet)
			{
				offsetYSet = true;
				offsetY = (((Tween)s).isRelative ? endValue.y : (endValue.y - startPosY));
			}
			Vector3 val = Vector2.op_Implicit(target.position);
			val.y += DOVirtual.EasedValue(0f, offsetY, TweenExtensions.ElapsedPercentage(yTween, true), (Ease)6);
			target.MovePosition(Vector2.op_Implicit(val));
		});
		return s;
	}

	public static TweenerCore<Vector3, Path, PathOptions> DOPath(this Rigidbody2D target, Vector2[] path, float duration, PathType pathType = (PathType)0, PathMode pathMode = (PathMode)1, int resolution = 10, Color? gizmoColor = null)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Expected O, but got Unknown
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		if (resolution < 1)
		{
			resolution = 1;
		}
		int num = path.Length;
		Vector3[] array = (Vector3[])(object)new Vector3[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = Vector2.op_Implicit(path[i]);
		}
		TweenerCore<Vector3, Path, PathOptions> obj = TweenSettingsExtensions.SetUpdate<TweenerCore<Vector3, Path, PathOptions>>(TweenSettingsExtensions.SetTarget<TweenerCore<Vector3, Path, PathOptions>>(DOTween.To<Vector3, Path, PathOptions>(PathPlugin.Get(), (DOGetter<Vector3>)(() => Vector2.op_Implicit(target.position)), (DOSetter<Vector3>)delegate(Vector3 x)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			target.MovePosition(Vector2.op_Implicit(x));
		}, new Path(pathType, array, resolution, gizmoColor), duration), (object)target), (UpdateType)2);
		obj.plugOptions.isRigidbody2D = true;
		obj.plugOptions.mode = pathMode;
		return obj;
	}

	public static TweenerCore<Vector3, Path, PathOptions> DOLocalPath(this Rigidbody2D target, Vector2[] path, float duration, PathType pathType = (PathType)0, PathMode pathMode = (PathMode)1, int resolution = 10, Color? gizmoColor = null)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Expected O, but got Unknown
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		if (resolution < 1)
		{
			resolution = 1;
		}
		int num = path.Length;
		Vector3[] array = (Vector3[])(object)new Vector3[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = Vector2.op_Implicit(path[i]);
		}
		Transform trans = ((Component)target).transform;
		TweenerCore<Vector3, Path, PathOptions> obj = TweenSettingsExtensions.SetUpdate<TweenerCore<Vector3, Path, PathOptions>>(TweenSettingsExtensions.SetTarget<TweenerCore<Vector3, Path, PathOptions>>(DOTween.To<Vector3, Path, PathOptions>(PathPlugin.Get(), (DOGetter<Vector3>)(() => trans.localPosition), (DOSetter<Vector3>)delegate(Vector3 x)
		{
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			target.MovePosition(Vector2.op_Implicit(((Object)(object)trans.parent == (Object)null) ? x : trans.parent.TransformPoint(x)));
		}, new Path(pathType, array, resolution, gizmoColor), duration), (object)target), (UpdateType)2);
		obj.plugOptions.isRigidbody2D = true;
		obj.plugOptions.mode = pathMode;
		obj.plugOptions.useLocalPosition = true;
		return obj;
	}

	internal static TweenerCore<Vector3, Path, PathOptions> DOPath(this Rigidbody2D target, Path path, float duration, PathMode pathMode = (PathMode)1)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		TweenerCore<Vector3, Path, PathOptions> obj = TweenSettingsExtensions.SetTarget<TweenerCore<Vector3, Path, PathOptions>>(DOTween.To<Vector3, Path, PathOptions>(PathPlugin.Get(), (DOGetter<Vector3>)(() => Vector2.op_Implicit(target.position)), (DOSetter<Vector3>)delegate(Vector3 x)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			target.MovePosition(Vector2.op_Implicit(x));
		}, path, duration), (object)target);
		obj.plugOptions.isRigidbody2D = true;
		obj.plugOptions.mode = pathMode;
		return obj;
	}

	internal static TweenerCore<Vector3, Path, PathOptions> DOLocalPath(this Rigidbody2D target, Path path, float duration, PathMode pathMode = (PathMode)1)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		Transform trans = ((Component)target).transform;
		TweenerCore<Vector3, Path, PathOptions> obj = TweenSettingsExtensions.SetTarget<TweenerCore<Vector3, Path, PathOptions>>(DOTween.To<Vector3, Path, PathOptions>(PathPlugin.Get(), (DOGetter<Vector3>)(() => trans.localPosition), (DOSetter<Vector3>)delegate(Vector3 x)
		{
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			target.MovePosition(Vector2.op_Implicit(((Object)(object)trans.parent == (Object)null) ? x : trans.parent.TransformPoint(x)));
		}, path, duration), (object)target);
		obj.plugOptions.isRigidbody2D = true;
		obj.plugOptions.mode = pathMode;
		obj.plugOptions.useLocalPosition = true;
		return obj;
	}
}
