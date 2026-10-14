using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace DG.Tweening;

public static class DOTweenModuleSprite
{
	public static TweenerCore<Color, Color, ColorOptions> DOColor(this SpriteRenderer target, Color endValue, float duration)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		TweenerCore<Color, Color, ColorOptions> obj = DOTween.To((DOGetter<Color>)(() => target.color), (DOSetter<Color>)delegate(Color x)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			target.color = x;
		}, endValue, duration);
		TweenSettingsExtensions.SetTarget<TweenerCore<Color, Color, ColorOptions>>(obj, (object)target);
		return obj;
	}

	public static TweenerCore<Color, Color, ColorOptions> DOFade(this SpriteRenderer target, float endValue, float duration)
	{
		TweenerCore<Color, Color, ColorOptions> obj = DOTween.ToAlpha((DOGetter<Color>)(() => target.color), (DOSetter<Color>)delegate(Color x)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			target.color = x;
		}, endValue, duration);
		TweenSettingsExtensions.SetTarget<TweenerCore<Color, Color, ColorOptions>>(obj, (object)target);
		return obj;
	}

	public static Sequence DOGradientColor(this SpriteRenderer target, Gradient gradient, float duration)
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
			float duration2 = ((i == num - 1) ? (duration - TweenExtensions.Duration((Tween)(object)val, false)) : (duration * ((i == 0) ? val2.time : (val2.time - colorKeys[i - 1].time))));
			TweenSettingsExtensions.Append(val, (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Color, Color, ColorOptions>>(target.DOColor(val2.color, duration2), (Ease)1));
		}
		TweenSettingsExtensions.SetTarget<Sequence>(val, (object)target);
		return val;
	}

	public static Tweener DOBlendableColor(this SpriteRenderer target, Color endValue, float duration)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		endValue -= target.color;
		Color to = new Color(0f, 0f, 0f, 0f);
		return (Tweener)(object)TweenSettingsExtensions.SetTarget<TweenerCore<Color, Color, ColorOptions>>(Extensions.Blendable<Color, Color, ColorOptions>(DOTween.To((DOGetter<Color>)(() => to), (DOSetter<Color>)delegate(Color x)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			Color val = x - to;
			to = x;
			SpriteRenderer obj = target;
			obj.color += val;
		}, endValue, duration)), (object)target);
	}
}
