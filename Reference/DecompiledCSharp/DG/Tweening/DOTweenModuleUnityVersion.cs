using System.Threading.Tasks;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace DG.Tweening;

public static class DOTweenModuleUnityVersion
{
	public static Sequence DOGradientColor(this Material target, Gradient gradient, float duration)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		Sequence val = DOTween.Sequence();
		GradientColorKey[] colorKeys = gradient.colorKeys;
		int num = colorKeys.Length;
		for (int i = 0; i < num; i++)
		{
			GradientColorKey val2 = colorKeys[i];
			if (i == 0 && val2.time <= 0f)
			{
				target.color = val2.color;
				continue;
			}
			float num2 = ((i == num - 1) ? (duration - TweenExtensions.Duration((Tween)(object)val, false)) : (duration * ((i == 0) ? val2.time : (val2.time - colorKeys[i - 1].time))));
			TweenSettingsExtensions.Append(val, (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Color, Color, ColorOptions>>(ShortcutExtensions.DOColor(target, val2.color, num2), (Ease)1));
		}
		TweenSettingsExtensions.SetTarget<Sequence>(val, (object)target);
		return val;
	}

	public static Sequence DOGradientColor(this Material target, Gradient gradient, string property, float duration)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		Sequence val = DOTween.Sequence();
		GradientColorKey[] colorKeys = gradient.colorKeys;
		int num = colorKeys.Length;
		for (int i = 0; i < num; i++)
		{
			GradientColorKey val2 = colorKeys[i];
			if (i == 0 && val2.time <= 0f)
			{
				target.SetColor(property, val2.color);
				continue;
			}
			float num2 = ((i == num - 1) ? (duration - TweenExtensions.Duration((Tween)(object)val, false)) : (duration * ((i == 0) ? val2.time : (val2.time - colorKeys[i - 1].time))));
			TweenSettingsExtensions.Append(val, (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Color, Color, ColorOptions>>(ShortcutExtensions.DOColor(target, val2.color, property, num2), (Ease)1));
		}
		TweenSettingsExtensions.SetTarget<Sequence>(val, (object)target);
		return val;
	}

	public static CustomYieldInstruction WaitForCompletion(this Tween t, bool returnCustomYieldInstruction)
	{
		if (!t.active)
		{
			if (Debugger.logPriority > 0)
			{
				Debugger.LogInvalidTween(t);
			}
			return null;
		}
		return (CustomYieldInstruction)(object)new DOTweenCYInstruction.WaitForCompletion(t);
	}

	public static CustomYieldInstruction WaitForRewind(this Tween t, bool returnCustomYieldInstruction)
	{
		if (!t.active)
		{
			if (Debugger.logPriority > 0)
			{
				Debugger.LogInvalidTween(t);
			}
			return null;
		}
		return (CustomYieldInstruction)(object)new DOTweenCYInstruction.WaitForRewind(t);
	}

	public static CustomYieldInstruction WaitForKill(this Tween t, bool returnCustomYieldInstruction)
	{
		if (!t.active)
		{
			if (Debugger.logPriority > 0)
			{
				Debugger.LogInvalidTween(t);
			}
			return null;
		}
		return (CustomYieldInstruction)(object)new DOTweenCYInstruction.WaitForKill(t);
	}

	public static CustomYieldInstruction WaitForElapsedLoops(this Tween t, int elapsedLoops, bool returnCustomYieldInstruction)
	{
		if (!t.active)
		{
			if (Debugger.logPriority > 0)
			{
				Debugger.LogInvalidTween(t);
			}
			return null;
		}
		return (CustomYieldInstruction)(object)new DOTweenCYInstruction.WaitForElapsedLoops(t, elapsedLoops);
	}

	public static CustomYieldInstruction WaitForPosition(this Tween t, float position, bool returnCustomYieldInstruction)
	{
		if (!t.active)
		{
			if (Debugger.logPriority > 0)
			{
				Debugger.LogInvalidTween(t);
			}
			return null;
		}
		return (CustomYieldInstruction)(object)new DOTweenCYInstruction.WaitForPosition(t, position);
	}

	public static CustomYieldInstruction WaitForStart(this Tween t, bool returnCustomYieldInstruction)
	{
		if (!t.active)
		{
			if (Debugger.logPriority > 0)
			{
				Debugger.LogInvalidTween(t);
			}
			return null;
		}
		return (CustomYieldInstruction)(object)new DOTweenCYInstruction.WaitForStart(t);
	}

	public static TweenerCore<Vector2, Vector2, VectorOptions> DOOffset(this Material target, Vector2 endValue, int propertyID, float duration)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		if (!target.HasProperty(propertyID))
		{
			if (Debugger.logPriority > 0)
			{
				Debugger.LogMissingMaterialProperty(propertyID);
			}
			return null;
		}
		TweenerCore<Vector2, Vector2, VectorOptions> obj = DOTween.To((DOGetter<Vector2>)(() => target.GetTextureOffset(propertyID)), (DOSetter<Vector2>)delegate(Vector2 x)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			target.SetTextureOffset(propertyID, x);
		}, endValue, duration);
		TweenSettingsExtensions.SetTarget<TweenerCore<Vector2, Vector2, VectorOptions>>(obj, (object)target);
		return obj;
	}

	public static TweenerCore<Vector2, Vector2, VectorOptions> DOTiling(this Material target, Vector2 endValue, int propertyID, float duration)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		if (!target.HasProperty(propertyID))
		{
			if (Debugger.logPriority > 0)
			{
				Debugger.LogMissingMaterialProperty(propertyID);
			}
			return null;
		}
		TweenerCore<Vector2, Vector2, VectorOptions> obj = DOTween.To((DOGetter<Vector2>)(() => target.GetTextureScale(propertyID)), (DOSetter<Vector2>)delegate(Vector2 x)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			target.SetTextureScale(propertyID, x);
		}, endValue, duration);
		TweenSettingsExtensions.SetTarget<TweenerCore<Vector2, Vector2, VectorOptions>>(obj, (object)target);
		return obj;
	}

	public static async Task AsyncWaitForCompletion(this Tween t)
	{
		if (!t.active)
		{
			if (Debugger.logPriority > 0)
			{
				Debugger.LogInvalidTween(t);
			}
		}
		else
		{
			while (t.active && !TweenExtensions.IsComplete(t))
			{
				await Task.Yield();
			}
		}
	}

	public static async Task AsyncWaitForRewind(this Tween t)
	{
		if (!t.active)
		{
			if (Debugger.logPriority > 0)
			{
				Debugger.LogInvalidTween(t);
			}
		}
		else
		{
			while (t.active && (!t.playedOnce || t.position * (float)(TweenExtensions.CompletedLoops(t) + 1) > 0f))
			{
				await Task.Yield();
			}
		}
	}

	public static async Task AsyncWaitForKill(this Tween t)
	{
		if (!t.active)
		{
			if (Debugger.logPriority > 0)
			{
				Debugger.LogInvalidTween(t);
			}
		}
		else
		{
			while (t.active)
			{
				await Task.Yield();
			}
		}
	}

	public static async Task AsyncWaitForElapsedLoops(this Tween t, int elapsedLoops)
	{
		if (!t.active)
		{
			if (Debugger.logPriority > 0)
			{
				Debugger.LogInvalidTween(t);
			}
		}
		else
		{
			while (t.active && TweenExtensions.CompletedLoops(t) < elapsedLoops)
			{
				await Task.Yield();
			}
		}
	}

	public static async Task AsyncWaitForPosition(this Tween t, float position)
	{
		if (!t.active)
		{
			if (Debugger.logPriority > 0)
			{
				Debugger.LogInvalidTween(t);
			}
		}
		else
		{
			while (t.active && t.position * (float)(TweenExtensions.CompletedLoops(t) + 1) < position)
			{
				await Task.Yield();
			}
		}
	}

	public static async Task AsyncWaitForStart(this Tween t)
	{
		if (!t.active)
		{
			if (Debugger.logPriority > 0)
			{
				Debugger.LogInvalidTween(t);
			}
		}
		else
		{
			while (t.active && !t.playedOnce)
			{
				await Task.Yield();
			}
		}
	}
}
