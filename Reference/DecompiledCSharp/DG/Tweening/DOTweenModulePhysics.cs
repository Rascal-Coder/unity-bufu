using DG.Tweening.Core;
using DG.Tweening.Core.Enums;
using DG.Tweening.Plugins;
using DG.Tweening.Plugins.Core.PathCore;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace DG.Tweening;

public static class DOTweenModulePhysics
{
	public static TweenerCore<Vector3, Vector3, VectorOptions> DOMove(this Rigidbody target, Vector3 endValue, float duration, bool snapping = false)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		TweenerCore<Vector3, Vector3, VectorOptions> obj = DOTween.To((DOGetter<Vector3>)(() => target.position), (DOSetter<Vector3>)target.MovePosition, endValue, duration);
		TweenSettingsExtensions.SetTarget<Tweener>(TweenSettingsExtensions.SetOptions(obj, snapping), (object)target);
		return obj;
	}

	public static TweenerCore<Vector3, Vector3, VectorOptions> DOMoveX(this Rigidbody target, float endValue, float duration, bool snapping = false)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		TweenerCore<Vector3, Vector3, VectorOptions> obj = DOTween.To((DOGetter<Vector3>)(() => target.position), (DOSetter<Vector3>)target.MovePosition, new Vector3(endValue, 0f, 0f), duration);
		TweenSettingsExtensions.SetTarget<Tweener>(TweenSettingsExtensions.SetOptions(obj, (AxisConstraint)2, snapping), (object)target);
		return obj;
	}

	public static TweenerCore<Vector3, Vector3, VectorOptions> DOMoveY(this Rigidbody target, float endValue, float duration, bool snapping = false)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		TweenerCore<Vector3, Vector3, VectorOptions> obj = DOTween.To((DOGetter<Vector3>)(() => target.position), (DOSetter<Vector3>)target.MovePosition, new Vector3(0f, endValue, 0f), duration);
		TweenSettingsExtensions.SetTarget<Tweener>(TweenSettingsExtensions.SetOptions(obj, (AxisConstraint)4, snapping), (object)target);
		return obj;
	}

	public static TweenerCore<Vector3, Vector3, VectorOptions> DOMoveZ(this Rigidbody target, float endValue, float duration, bool snapping = false)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		TweenerCore<Vector3, Vector3, VectorOptions> obj = DOTween.To((DOGetter<Vector3>)(() => target.position), (DOSetter<Vector3>)target.MovePosition, new Vector3(0f, 0f, endValue), duration);
		TweenSettingsExtensions.SetTarget<Tweener>(TweenSettingsExtensions.SetOptions(obj, (AxisConstraint)8, snapping), (object)target);
		return obj;
	}

	public static TweenerCore<Quaternion, Vector3, QuaternionOptions> DORotate(this Rigidbody target, Vector3 endValue, float duration, RotateMode mode = (RotateMode)0)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		TweenerCore<Quaternion, Vector3, QuaternionOptions> obj = DOTween.To((DOGetter<Quaternion>)(() => target.rotation), (DOSetter<Quaternion>)target.MoveRotation, endValue, duration);
		TweenSettingsExtensions.SetTarget<TweenerCore<Quaternion, Vector3, QuaternionOptions>>(obj, (object)target);
		obj.plugOptions.rotateMode = mode;
		return obj;
	}

	public static TweenerCore<Quaternion, Vector3, QuaternionOptions> DOLookAt(this Rigidbody target, Vector3 towards, float duration, AxisConstraint axisConstraint = (AxisConstraint)0, Vector3? up = null)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		TweenerCore<Quaternion, Vector3, QuaternionOptions> obj = Extensions.SetSpecialStartupMode<TweenerCore<Quaternion, Vector3, QuaternionOptions>>(TweenSettingsExtensions.SetTarget<TweenerCore<Quaternion, Vector3, QuaternionOptions>>(DOTween.To((DOGetter<Quaternion>)(() => target.rotation), (DOSetter<Quaternion>)target.MoveRotation, towards, duration), (object)target), (SpecialStartupMode)1);
		obj.plugOptions.axisConstraint = axisConstraint;
		obj.plugOptions.up = ((!up.HasValue) ? Vector3.up : up.Value);
		return obj;
	}

	public static Sequence DOJump(this Rigidbody target, Vector3 endValue, float jumpPower, int numJumps, float duration, bool snapping = false)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Expected O, but got Unknown
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Expected O, but got Unknown
		if (numJumps < 1)
		{
			numJumps = 1;
		}
		float startPosY = 0f;
		float offsetY = -1f;
		bool offsetYSet = false;
		Sequence s = DOTween.Sequence();
		Tween yTween = (Tween)(object)TweenSettingsExtensions.OnStart<Tweener>(TweenSettingsExtensions.SetLoops<Tweener>(TweenSettingsExtensions.SetRelative<Tweener>(TweenSettingsExtensions.SetEase<Tweener>(TweenSettingsExtensions.SetOptions(DOTween.To((DOGetter<Vector3>)(() => target.position), (DOSetter<Vector3>)target.MovePosition, new Vector3(0f, jumpPower, 0f), duration / (float)(numJumps * 2)), (AxisConstraint)4, snapping), (Ease)6)), numJumps * 2, (LoopType)1), (TweenCallback)delegate
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			startPosY = target.position.y;
		});
		TweenSettingsExtensions.SetEase<Sequence>(TweenSettingsExtensions.SetTarget<Sequence>(TweenSettingsExtensions.Join(TweenSettingsExtensions.Join(TweenSettingsExtensions.Append(s, (Tween)(object)TweenSettingsExtensions.SetEase<Tweener>(TweenSettingsExtensions.SetOptions(DOTween.To((DOGetter<Vector3>)(() => target.position), (DOSetter<Vector3>)target.MovePosition, new Vector3(endValue.x, 0f, 0f), duration), (AxisConstraint)2, snapping), (Ease)1)), (Tween)(object)TweenSettingsExtensions.SetEase<Tweener>(TweenSettingsExtensions.SetOptions(DOTween.To((DOGetter<Vector3>)(() => target.position), (DOSetter<Vector3>)target.MovePosition, new Vector3(0f, 0f, endValue.z), duration), (AxisConstraint)8, snapping), (Ease)1)), yTween), (object)target), DOTween.defaultEaseType);
		TweenSettingsExtensions.OnUpdate<Tween>(yTween, (TweenCallback)delegate
		{
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			if (!offsetYSet)
			{
				offsetYSet = true;
				offsetY = (((Tween)s).isRelative ? endValue.y : (endValue.y - startPosY));
			}
			Vector3 position = target.position;
			position.y += DOVirtual.EasedValue(0f, offsetY, TweenExtensions.ElapsedPercentage(yTween, true), (Ease)6);
			target.MovePosition(position);
		});
		return s;
	}

	public static TweenerCore<Vector3, Path, PathOptions> DOPath(this Rigidbody target, Vector3[] path, float duration, PathType pathType = (PathType)0, PathMode pathMode = (PathMode)1, int resolution = 10, Color? gizmoColor = null)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Expected O, but got Unknown
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		if (resolution < 1)
		{
			resolution = 1;
		}
		TweenerCore<Vector3, Path, PathOptions> obj = TweenSettingsExtensions.SetUpdate<TweenerCore<Vector3, Path, PathOptions>>(TweenSettingsExtensions.SetTarget<TweenerCore<Vector3, Path, PathOptions>>(DOTween.To<Vector3, Path, PathOptions>(PathPlugin.Get(), (DOGetter<Vector3>)(() => target.position), (DOSetter<Vector3>)target.MovePosition, new Path(pathType, path, resolution, gizmoColor), duration), (object)target), (UpdateType)2);
		obj.plugOptions.isRigidbody = true;
		obj.plugOptions.mode = pathMode;
		return obj;
	}

	public static TweenerCore<Vector3, Path, PathOptions> DOLocalPath(this Rigidbody target, Vector3[] path, float duration, PathType pathType = (PathType)0, PathMode pathMode = (PathMode)1, int resolution = 10, Color? gizmoColor = null)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		if (resolution < 1)
		{
			resolution = 1;
		}
		Transform trans = ((Component)target).transform;
		TweenerCore<Vector3, Path, PathOptions> obj = TweenSettingsExtensions.SetUpdate<TweenerCore<Vector3, Path, PathOptions>>(TweenSettingsExtensions.SetTarget<TweenerCore<Vector3, Path, PathOptions>>(DOTween.To<Vector3, Path, PathOptions>(PathPlugin.Get(), (DOGetter<Vector3>)(() => trans.localPosition), (DOSetter<Vector3>)delegate(Vector3 x)
		{
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			target.MovePosition(((Object)(object)trans.parent == (Object)null) ? x : trans.parent.TransformPoint(x));
		}, new Path(pathType, path, resolution, gizmoColor), duration), (object)target), (UpdateType)2);
		obj.plugOptions.isRigidbody = true;
		obj.plugOptions.mode = pathMode;
		obj.plugOptions.useLocalPosition = true;
		return obj;
	}

	internal static TweenerCore<Vector3, Path, PathOptions> DOPath(this Rigidbody target, Path path, float duration, PathMode pathMode = (PathMode)1)
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		TweenerCore<Vector3, Path, PathOptions> obj = TweenSettingsExtensions.SetTarget<TweenerCore<Vector3, Path, PathOptions>>(DOTween.To<Vector3, Path, PathOptions>(PathPlugin.Get(), (DOGetter<Vector3>)(() => target.position), (DOSetter<Vector3>)target.MovePosition, path, duration), (object)target);
		obj.plugOptions.isRigidbody = true;
		obj.plugOptions.mode = pathMode;
		return obj;
	}

	internal static TweenerCore<Vector3, Path, PathOptions> DOLocalPath(this Rigidbody target, Path path, float duration, PathMode pathMode = (PathMode)1)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		Transform trans = ((Component)target).transform;
		TweenerCore<Vector3, Path, PathOptions> obj = TweenSettingsExtensions.SetTarget<TweenerCore<Vector3, Path, PathOptions>>(DOTween.To<Vector3, Path, PathOptions>(PathPlugin.Get(), (DOGetter<Vector3>)(() => trans.localPosition), (DOSetter<Vector3>)delegate(Vector3 x)
		{
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			target.MovePosition(((Object)(object)trans.parent == (Object)null) ? x : trans.parent.TransformPoint(x));
		}, path, duration), (object)target);
		obj.plugOptions.isRigidbody = true;
		obj.plugOptions.mode = pathMode;
		obj.plugOptions.useLocalPosition = true;
		return obj;
	}
}
