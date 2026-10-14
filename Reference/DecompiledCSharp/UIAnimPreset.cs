using DG.Tweening;
using UnityEngine;

[CreateAssetMenu(fileName = "NewUIAnimPreset", menuName = "UI动效库/新建动效预设 (Preset)")]
public class UIAnimPreset : ScriptableObject
{
	[Header("1. Global (时间与曲线)")]
	public float duration = 0.4f;

	public Ease easeCurve = (Ease)27;

	[Header("2. Position (位移)")]
	public bool enableMove = true;

	public Vector2 moveOffset = new Vector2(0f, -800f);

	[Header("3. Fade (透明度)")]
	public bool enableFade = true;

	[Range(0f, 1f)]
	public float startAlpha;

	[Header("4. Scale (基础缩放)")]
	public bool enableScale;

	public Vector3 startScale = Vector3.zero;

	[Header("5. Rotation (旋转)")]
	public bool enableRotation;

	public float rotationDelay;

	public Vector3 startRotation = new Vector3(0f, 0f, -90f);

	[Header("6. Jelly Bounce (果冻弹跳)")]
	public bool enableJellyBounce;

	public float jellyDelay = 0.1f;

	public Vector3 punchStrength = new Vector3(0.2f, -0.2f, 0f);

	public float bounceDuration = 0.5f;

	public int vibrato = 5;

	[Range(0f, 1f)]
	public float elasticity = 1f;
}
