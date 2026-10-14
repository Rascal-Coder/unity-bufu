using System;
using System.Collections;
using System.Collections.Generic;
using Module.UI;
using Module.Vat;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace Module.Role;

public sealed class DiceVat : MonoBehaviour, IRoleThrowableVat
{
	private const string RollingAnimationName = "rolling";

	private const string SmokePrefabPath = "Assets/GameRes/Role/coins_smoke.prefab";

	private GameObject smokeEffectObject;

	private SpriteVatAnimationSequence smokeAnimation;

	private bool isLoadingSmokeEffect;

	[SerializeField]
	private SpineVat spineVat;

	[SerializeField]
	private float throwDuration = 0.8f;

	private float sourceThrowDuration = 0.8f;

	[SerializeField]
	private float throwDelay = 0.15f;

	private float jumpHeight = 3.6f;

	private DiceVatState state;

	private int qualityId;

	private Vector2 boundaryMin;

	private Vector2 boundaryMax;

	private Vector2 throwStartPosition;

	private Vector2 throwTargetPosition;

	private float throwElapsed;

	private float throwDelayRemaining;

	private DiceRollResult pendingResult;

	private WorldSettlementSource pendingSource;

	private int sourceRoleType;

	private Func<WorldSettlementSource, double> valueMultiplierProvider;

	private Func<IReadOnlyList<float>> weightMultiplierProvider;

	private int forcedNextFace;

	private string cachedSortingLayerName;

	private int cachedSortingOrder;

	private Shader cachedVatShader;

	private Shader cachedSpriteVatShader;

	private bool isEvolutionMerging;

	private Vector2 evolutionMergeStart;

	private Vector2 evolutionMergeTarget;

	private float evolutionMergeElapsed;

	private float evolutionMergeDuration;

	private bool isEvolutionShaking;

	private Vector2 evolutionShakeBasePos;

	private float evolutionShakeElapsed;

	private bool isEvolutionRevealPending;

	private Vector3 evolutionOriginalScale = Vector3.one;

	private const float EvolutionShakeAmplitude = 0.12f;

	private const float EvolutionShakeFrequency = 52f;

	private const int EvolutionTopSortingBoost = 30000;

	private const float EvolutionCenterScale = 3f;

	private bool isEvolutionScaleDown;

	private float evolutionScaleDownElapsed;

	private const float EvolutionScaleDownDuration = 0.5f;

	private const string EvolutionUpgradePrefabPath = "Assets/GameRes/Role/coin_upgrade.prefab";

	private const string EvolutionUpgradeAnimName = "effect";

	private int evolutionTier;

	private int pendingEvolutionTier;

	public int QualityId => qualityId;

	public DiceVatState State => state;

	public int Face { get; private set; } = 1;

	public int SourceRoleType => sourceRoleType;

	public bool UseExternalTick { get; set; }

	public SpineVat SpineVatComponent => spineVat;

	public bool IsLuckyEcho { get; private set; }

	public bool IsResonanceTriggered { get; set; }

	public Transform VatTransform => ((Component)this).transform;

	public bool IsIdleForRole => state == DiceVatState.Idle;

	public float RoleThrowDuration => throwDelay + throwDuration;

	public bool IsEvolutionMerging => isEvolutionMerging;

	public bool IsEvolutionShaking => isEvolutionShaking;

	public event Action<DiceRollResult> OnRolled;

	public event Action<DiceVat, DiceRollResult, WorldSettlementSource, int> OnLanded;

	public event Action<DiceVat, int> OnResonanceTrigger;

	public void ForceNextFace(int face)
	{
		forcedNextFace = Mathf.Clamp(face, 1, 6);
	}

	private void Awake()
	{
		sourceThrowDuration = throwDuration;
		EnsureSpineVat();
	}

	private void Update()
	{
		if (!UseExternalTick)
		{
			TickExternal(Time.deltaTime);
		}
	}

	public void Initialize(int qualityId, SpineVatAsset vatAsset, Vector2 min, Vector2 max, Func<WorldSettlementSource, double> resolveValueMultiplier, Func<IReadOnlyList<float>> resolveWeightMultipliers, int evoTier = 0)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		this.qualityId = qualityId;
		evolutionTier = Mathf.Max(0, evoTier);
		boundaryMin = min;
		boundaryMax = max;
		valueMultiplierProvider = resolveValueMultiplier;
		weightMultiplierProvider = resolveWeightMultipliers;
		EnsureSpineVat();
		spineVat.UseExternalTick = UseExternalTick;
		spineVat.Initialize(vatAsset, min, max, enableRandomRoam: false);
		spineVat.Play(Face.ToString(), shouldLoop: false);
		state = DiceVatState.Idle;
	}

	public void SetVatShader(Shader shader)
	{
		if (!((Object)(object)shader == (Object)null))
		{
			cachedVatShader = shader;
			EnsureSpineVat();
			spineVat.SetVatShader(shader);
		}
	}

	public void SetSpriteVatShader(Shader shader)
	{
		if (!((Object)(object)shader == (Object)null))
		{
			cachedSpriteVatShader = shader;
		}
	}

	public void SetSortingLayerAndOrder(string layerName, int order)
	{
		cachedSortingLayerName = layerName;
		cachedSortingOrder = order;
		EnsureSpineVat();
		spineVat.SetSortingLayerAndOrder(layerName, order);
	}

	public void SetInitialPosition(Vector2 position)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		StopEvolutionEffects();
		((Component)this).transform.position = Vector2.op_Implicit(ClampToBoundary(position));
		state = DiceVatState.Idle;
		Face = Random.Range(1, 7);
		sourceRoleType = 0;
		if ((Object)(object)spineVat != (Object)null && (Object)(object)spineVat.VatAsset != (Object)null)
		{
			spineVat.Play(Face.ToString(), shouldLoop: false);
		}
	}

	public void StartEvolutionMergeMove(Vector2 targetWorldPos, float duration = 1f)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		CancelThrow();
		StopEvolutionEffects();
		evolutionMergeStart = Vector2.op_Implicit(((Component)this).transform.position);
		evolutionMergeTarget = targetWorldPos;
		evolutionMergeElapsed = 0f;
		evolutionMergeDuration = Mathf.Max(0.05f, duration);
		isEvolutionMerging = true;
		state = DiceVatState.Evolving;
	}

	public void StartEvolutionShake(float duration = 1f, int targetEvolutionTier = -1)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		CancelThrow();
		isEvolutionMerging = false;
		evolutionShakeBasePos = Vector2.op_Implicit(((Component)this).transform.position);
		evolutionShakeElapsed = 0f;
		if (!isEvolutionShaking && !isEvolutionRevealPending)
		{
			evolutionOriginalScale = ((Component)this).transform.localScale;
		}
		pendingEvolutionTier = ((targetEvolutionTier >= 0) ? targetEvolutionTier : evolutionTier);
		isEvolutionRevealPending = true;
		isEvolutionShaking = true;
		state = DiceVatState.Evolving;
		((Component)this).transform.position = new Vector3(evolutionShakeBasePos.x, evolutionShakeBasePos.y, 10f);
		((Component)this).transform.localScale = evolutionOriginalScale * 3f;
		((Component)this).transform.SetAsLastSibling();
		ApplyEvolutionTopSorting(onTop: true);
	}

	public void StopEvolutionShake()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		if (!isEvolutionShaking)
		{
			return;
		}
		isEvolutionShaking = false;
		if (!((Object)(object)this == (Object)null))
		{
			((Component)this).transform.position = new Vector3(evolutionShakeBasePos.x, evolutionShakeBasePos.y, 10f);
			((Component)this).transform.localScale = evolutionOriginalScale * 3f;
			ApplyEvolutionTopSorting(onTop: true);
			if (state == DiceVatState.Evolving)
			{
				state = DiceVatState.Idle;
			}
			PlayEvolutionUpgradeEffect();
		}
	}

	public void StopEvolutionEffects()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)this == (Object)null)
		{
			isEvolutionMerging = false;
			isEvolutionShaking = false;
			isEvolutionRevealPending = false;
			isEvolutionScaleDown = false;
			return;
		}
		if (isEvolutionShaking || isEvolutionRevealPending)
		{
			((Component)this).transform.position = new Vector3(evolutionShakeBasePos.x, evolutionShakeBasePos.y, 10f);
			ApplyEvolutionTopSorting(onTop: false);
		}
		isEvolutionMerging = false;
		isEvolutionShaking = false;
		isEvolutionRevealPending = false;
		isEvolutionScaleDown = false;
		evolutionMergeElapsed = 0f;
		evolutionShakeElapsed = 0f;
		evolutionScaleDownElapsed = 0f;
		((Component)this).transform.localScale = evolutionOriginalScale;
		if (state == DiceVatState.Evolving)
		{
			state = DiceVatState.Idle;
		}
	}

	private void FinishEvolutionReveal()
	{
		if ((Object)(object)this == (Object)null || !isEvolutionRevealPending)
		{
			return;
		}
		isEvolutionRevealPending = false;
		if (pendingEvolutionTier != evolutionTier)
		{
			SpineVatAsset diceVatAssetForTier = GetDiceVatAssetForTier(qualityId, pendingEvolutionTier);
			if ((Object)(object)diceVatAssetForTier != (Object)null)
			{
				ApplyEvolutionSkin(diceVatAssetForTier, pendingEvolutionTier);
			}
			else
			{
				evolutionTier = pendingEvolutionTier;
			}
		}
		isEvolutionScaleDown = true;
		evolutionScaleDownElapsed = 0f;
		ApplyEvolutionTopSorting(onTop: false);
	}

	private void ApplyEvolutionTopSorting(bool onTop)
	{
		if (!((Object)(object)this == (Object)null) && !((Object)(object)spineVat == (Object)null))
		{
			if (onTop)
			{
				spineVat.SetIndirectRenderingEnabled(enabled: false);
				spineVat.SetRenderingVisible(visible: true);
			}
			int order = (onTop ? (cachedSortingOrder + 30000) : cachedSortingOrder);
			spineVat.SetSortingLayerAndOrder(cachedSortingLayerName, order);
			spineVat.SetDepthSortZRange(onTop ? (-5f) : (-1f), onTop ? (-3.1f) : 1f);
		}
	}

	private void PlayEvolutionUpgradeEffect()
	{
		if ((Object)(object)this == (Object)null)
		{
			return;
		}
		if ((Object)(object)SingletonMono<ResManager>.Instance == (Object)null)
		{
			FinishEvolutionReveal();
			return;
		}
		Transform diceParent = ((Component)this).transform;
		SingletonMono<ResManager>.Instance.LoadPrefabAsync("Assets/GameRes/Role/coin_upgrade.prefab", delegate(GameObject prefab)
		{
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Expected O, but got Unknown
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_010f: Unknown result type (might be due to invalid IL or missing references)
			//IL_01be: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_017c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0192: Unknown result type (might be due to invalid IL or missing references)
			//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_02fe: Expected O, but got Unknown
			if (!((Object)(object)this == (Object)null) && !((Object)(object)diceParent == (Object)null))
			{
				if ((Object)(object)prefab == (Object)null)
				{
					Debug.LogWarning((object)"[DiceVat] 加载合成特效失败: Assets/GameRes/Role/coin_upgrade.prefab", (Object)(object)this);
					FinishEvolutionReveal();
				}
				else
				{
					GameObject canvasGo = new GameObject("EvolutionUpgradeCanvas");
					canvasGo.layer = ((Component)diceParent).gameObject.layer;
					canvasGo.transform.SetParent(diceParent, false);
					canvasGo.transform.localPosition = Vector3.zero;
					canvasGo.transform.localRotation = Quaternion.identity;
					float num = 0.01f;
					float num2 = 1f;
					MainSceneVat mainSceneVat = (((Object)(object)UIManager.Instance != (Object)null) ? UIManager.Instance.MainSceneVat : null);
					if ((Object)(object)mainSceneVat != (Object)null)
					{
						num = mainSceneVat.UiToWorldScale;
					}
					canvasGo.transform.localScale = Vector3.one * (Mathf.Max(0.0001f, num) * num2);
					Canvas obj = canvasGo.AddComponent<Canvas>();
					obj.renderMode = (RenderMode)2;
					obj.overrideSorting = true;
					obj.sortingLayerName = cachedSortingLayerName;
					obj.sortingOrder = cachedSortingOrder + 30000;
					RectTransform component = canvasGo.GetComponent<RectTransform>();
					if ((Object)(object)component != (Object)null)
					{
						component.sizeDelta = new Vector2(200f, 200f);
						component.pivot = new Vector2(0.5f, 0.5f);
					}
					GameObject obj2 = Object.Instantiate<GameObject>(prefab, canvasGo.transform);
					((Object)obj2).name = "coin_upgrade";
					obj2.transform.localPosition = Vector3.zero;
					obj2.transform.localRotation = Quaternion.identity;
					Transform transform = obj2.transform;
					RectTransform val = (RectTransform)(object)((transform is RectTransform) ? transform : null);
					if (val != null)
					{
						val.anchoredPosition = Vector2.zero;
						((Transform)val).localPosition = Vector3.zero;
					}
					SkeletonGraphic componentInChildren = obj2.GetComponentInChildren<SkeletonGraphic>(true);
					if ((Object)(object)componentInChildren == (Object)null)
					{
						Debug.LogWarning((object)"[DiceVat] coin_upgrade 缺少 SkeletonGraphic: Assets/GameRes/Role/coin_upgrade.prefab", (Object)(object)this);
						Object.Destroy((Object)(object)canvasGo);
						FinishEvolutionReveal();
					}
					else
					{
						((Graphic)componentInChildren).raycastTarget = false;
						if (!componentInChildren.IsValid || componentInChildren.AnimationState == null)
						{
							componentInChildren.Initialize(true);
						}
						if (componentInChildren.AnimationState == null)
						{
							Debug.LogWarning((object)"[DiceVat] SkeletonGraphic.AnimationState 为空，无法播放 effect", (Object)(object)this);
							Object.Destroy((Object)(object)canvasGo);
							FinishEvolutionReveal();
						}
						else
						{
							componentInChildren.AnimationState.ClearTracks();
							Skeleton skeleton = componentInChildren.Skeleton;
							if (skeleton != null)
							{
								skeleton.SetToSetupPose();
							}
							if (componentInChildren.AnimationState.SetAnimation(0, "effect", false) == null)
							{
								Debug.LogWarning((object)"[DiceVat] 找不到动画: effect", (Object)(object)this);
								Object.Destroy((Object)(object)canvasGo);
								FinishEvolutionReveal();
							}
							else
							{
								componentInChildren.AnimationState.Complete += (TrackEntryDelegate)delegate(TrackEntry trackEntry)
								{
									if (trackEntry == null || trackEntry.TrackIndex == 0)
									{
										if ((Object)(object)canvasGo != (Object)null)
										{
											Object.Destroy((Object)(object)canvasGo);
										}
										FinishEvolutionReveal();
									}
								};
							}
						}
					}
				}
			}
		});
	}

	public bool TryThrowByRole(Vector2 startPosition, Vector2 targetPosition, int roleType)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return TryThrow(startPosition, targetPosition, WorldSettlementSource.Helper, roleType);
	}

	public bool TryThrow(Vector2 startPosition, Vector2 targetPosition, WorldSettlementSource source, int roleType = -1)
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		if (qualityId <= 0 || state != DiceVatState.Idle)
		{
			return false;
		}
		pendingSource = source;
		sourceRoleType = roleType;
		pendingResult = Roll(qualityId, valueMultiplierProvider?.Invoke(source) ?? 1.0, weightMultiplierProvider?.Invoke());
		throwStartPosition = startPosition;
		throwTargetPosition = ClampToBoundary(targetPosition);
		((Component)this).transform.position = Vector2.op_Implicit(startPosition);
		throwElapsed = 0f;
		throwDelayRemaining = Mathf.Max(0f, throwDelay);
		state = ((throwDelayRemaining > 0f) ? DiceVatState.WaitingForThrow : DiceVatState.Flying);
		if (state == DiceVatState.Flying)
		{
			PlayDiceUpSound();
		}
		EnsureSpineVat();
		spineVat.Play("rolling", shouldLoop: true);
		return true;
	}

	public void CancelThrow()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		if (state != DiceVatState.Idle)
		{
			state = DiceVatState.Idle;
			throwElapsed = 0f;
			throwDelayRemaining = 0f;
			((Component)this).transform.position = Vector2.op_Implicit(ClampToBoundary(Vector2.op_Implicit(((Component)this).transform.position)));
			if ((Object)(object)spineVat != (Object)null && (Object)(object)spineVat.VatAsset != (Object)null)
			{
				spineVat.Play(Face.ToString(), shouldLoop: false);
				spineVat.SetSortingLayerAndOrder(cachedSortingLayerName, cachedSortingOrder);
				spineVat.SetDepthSortZRange(-1f, 1f);
			}
		}
	}

	public void TickExternal(float deltaTime)
	{
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)spineVat != (Object)null)
		{
			spineVat.TickExternal(deltaTime);
		}
		if (isEvolutionMerging || isEvolutionShaking || isEvolutionScaleDown)
		{
			TickEvolutionEffects(deltaTime);
			return;
		}
		if (state == DiceVatState.WaitingForThrow)
		{
			throwDelayRemaining -= deltaTime;
			if (throwDelayRemaining <= 0f)
			{
				state = DiceVatState.Flying;
				spineVat.SetSortingLayerAndOrder(cachedSortingLayerName, cachedSortingOrder + 20000);
				spineVat.SetDepthSortZRange(-5f, -3.1f);
				PlayDiceUpSound();
			}
		}
		if (state != DiceVatState.Flying)
		{
			return;
		}
		if (throwDuration <= 0f)
		{
			FinishThrow();
			return;
		}
		if ((GameDataManager.Instance?.GetCurrentWorldId() ?? 1) == 2)
		{
			throwDuration = (sourceThrowDuration * TalentDataManager.Instance?.GetDiceTalentBuff(107)) ?? 1f;
		}
		throwElapsed += deltaTime;
		float num = Mathf.Clamp01(throwElapsed / throwDuration);
		Vector2 val = Vector2.Lerp(throwStartPosition, throwTargetPosition, num);
		val.y += Mathf.Sin(num * (float)Math.PI) * Mathf.Max(0f, jumpHeight);
		((Component)this).transform.position = Vector2.op_Implicit(val);
		if (num >= 1f)
		{
			FinishThrow();
		}
	}

	public DiceRollResult Roll(int qualityId, double valueMultiplier, IReadOnlyList<float> weightMultipliers)
	{
		float[] array = new float[6];
		for (int i = 0; i < array.Length; i++)
		{
			float num = ((weightMultipliers != null && i < weightMultipliers.Count) ? weightMultipliers[i] : 1f);
			float num2 = 1f;
			array[i] = Mathf.Max(0f, num2) * Mathf.Max(0f, num);
		}
		if ((Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null)
		{
			float itemUpgrade3Effect = SingletonMono<UpgradeManager>.Instance.GetItemUpgrade3Effect(101);
			if (itemUpgrade3Effect < 1f)
			{
				for (int j = 0; j < 3; j++)
				{
					array[j] *= itemUpgrade3Effect;
				}
			}
		}
		int num3 = RollFace(array);
		DownItem downItem = UIManager.Instance.GetDownItem();
		if ((Object)(object)downItem != (Object)null)
		{
			num3 = (downItem.GetIsOpenSuperSix() ? 6 : num3);
		}
		if (forcedNextFace > 0)
		{
			num3 = forcedNextFace;
			forcedNextFace = 0;
		}
		if (num3 == 6 && (Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null)
		{
			float itemUpgrade3Effect2 = SingletonMono<UpgradeManager>.Instance.GetItemUpgrade3Effect(105);
			if (itemUpgrade3Effect2 > 1f)
			{
				valueMultiplier *= (double)itemUpgrade3Effect2;
			}
		}
		int num4 = GameDataManager.Instance?.GetCurrentWorldId() ?? 1;
		if ((Object)(object)downItem != (Object)null && downItem.GetIsOpenSuperSix() && num4 == 2)
		{
			valueMultiplier += (double)(TalentDataManager.Instance?.GetDiceTalentBuff(308) ?? 0f);
		}
		UpgradeManager instance = SingletonMono<UpgradeManager>.Instance;
		double baseValue = (((Object)(object)instance != (Object)null) ? instance.GetDiceBaseValue(qualityId, num3) : (1.0 * (double)num3));
		if (qualityId == 105)
		{
			valueMultiplier += (double)(TalentDataManager.Instance?.GetDiceTalentBuff(106) ?? 0f);
		}
		DiceRollResult diceRollResult = new DiceRollResult(qualityId, num3, baseValue, valueMultiplier);
		this.OnRolled?.Invoke(diceRollResult);
		return diceRollResult;
	}

	public void ApplyEvolutionSkin(SpineVatAsset vatAsset, int evoTier)
	{
		if (!((Object)(object)vatAsset == (Object)null))
		{
			evolutionTier = Mathf.Max(0, evoTier);
			EnsureSpineVat();
			if ((Object)(object)cachedVatShader != (Object)null)
			{
				spineVat.SetVatShader(cachedVatShader);
			}
			spineVat.SetVatAssetNoPlay(vatAsset);
			spineVat.Play(Face.ToString(), shouldLoop: false);
		}
	}

	private SpineVatAsset GetDiceVatAssetForTier(int qId, int tier)
	{
		if (VatRuntimeAssets.Instance != null)
		{
			SpineVatAsset diceVatAsset = VatRuntimeAssets.Instance.GetDiceVatAsset(qId, tier);
			if ((Object)(object)diceVatAsset != (Object)null)
			{
				return diceVatAsset;
			}
		}
		return null;
	}

	public int GetEvolutionTier()
	{
		if ((Object)(object)SingletonMono<UpgradeManager>.Instance == (Object)null || !UpgradeManager.IsEvolvableCurrencyUpgrade(qualityId))
		{
			return 0;
		}
		return SingletonMono<UpgradeManager>.Instance.GetEvolutionTier(qualityId);
	}

	public double GetEvolutionIncomeMultiplier()
	{
		if ((Object)(object)SingletonMono<UpgradeManager>.Instance == (Object)null || !UpgradeManager.IsEvolvableCurrencyUpgrade(qualityId))
		{
			return 1.0;
		}
		return SingletonMono<UpgradeManager>.Instance.GetEvolutionIncomeMultiplier(qualityId);
	}

	private void TickEvolutionEffects(float dt)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		if (isEvolutionMerging)
		{
			evolutionMergeElapsed += dt;
			float num = Mathf.Clamp01(evolutionMergeElapsed / evolutionMergeDuration);
			float num2 = num * num * (3f - 2f * num);
			((Component)this).transform.position = Vector2.op_Implicit(Vector2.Lerp(evolutionMergeStart, evolutionMergeTarget, num2));
			if (num >= 1f)
			{
				isEvolutionMerging = false;
				((Component)this).transform.position = Vector2.op_Implicit(evolutionMergeTarget);
			}
		}
		if (isEvolutionShaking)
		{
			evolutionShakeElapsed += dt;
			float num3 = Mathf.Sin(evolutionShakeElapsed * 52f) * 0.12f;
			float num4 = Mathf.Cos(evolutionShakeElapsed * 52f * 1.3f) * 0.066f;
			((Component)this).transform.position = new Vector3(evolutionShakeBasePos.x + num3, evolutionShakeBasePos.y + num4, 10f);
			((Component)this).transform.localScale = evolutionOriginalScale * 3f;
		}
		if (isEvolutionScaleDown)
		{
			evolutionScaleDownElapsed += dt;
			float num5 = Mathf.Clamp01(evolutionScaleDownElapsed / 0.5f);
			float num6 = 1f - (1f - num5) * (1f - num5);
			((Component)this).transform.localScale = Vector3.Lerp(evolutionOriginalScale * 3f, evolutionOriginalScale, num6);
			if (num5 >= 1f)
			{
				isEvolutionScaleDown = false;
				((Component)this).transform.localScale = evolutionOriginalScale;
			}
		}
	}

	private void PlayDiceUpSound()
	{
		if ((Object)(object)SoundManager.Instance != (Object)null)
		{
			SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/diceUp.mp3");
		}
	}

	private void PlayDiceHitGroundSound()
	{
		if ((Object)(object)SoundManager.Instance != (Object)null)
		{
			SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/diceHitGround.mp3");
		}
	}

	private void FinishThrow()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		state = DiceVatState.Landed;
		PlayDiceHitGroundSound();
		((Component)this).transform.position = Vector2.op_Implicit(throwTargetPosition);
		Face = pendingResult.Face;
		if ((Object)(object)spineVat != (Object)null)
		{
			spineVat.Play(Face.ToString(), shouldLoop: false);
			spineVat.SetSortingLayerAndOrder(cachedSortingLayerName, cachedSortingOrder);
			spineVat.SetDepthSortZRange(-1f, 1f);
		}
		this.OnLanded?.Invoke(this, pendingResult, pendingSource, sourceRoleType);
		PlaySmoke(sourceRoleType);
		state = DiceVatState.Idle;
		if (!IsLuckyEcho && (Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null)
		{
			float itemUpgrade3Effect = SingletonMono<UpgradeManager>.Instance.GetItemUpgrade3Effect(103);
			if (itemUpgrade3Effect > 0f && Random.value < itemUpgrade3Effect)
			{
				IsLuckyEcho = true;
				((MonoBehaviour)this).StartCoroutine(DoLuckyEchoThrow(0.3f));
			}
		}
		else
		{
			IsLuckyEcho = false;
		}
		if (!IsResonanceTriggered && (Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null)
		{
			float itemUpgrade3Effect2 = SingletonMono<UpgradeManager>.Instance.GetItemUpgrade3Effect(104);
			if (itemUpgrade3Effect2 > 0f && Random.value < itemUpgrade3Effect2)
			{
				this.OnResonanceTrigger?.Invoke(this, Face);
			}
		}
		else
		{
			IsResonanceTriggered = false;
		}
		if (Face == 6)
		{
			DownItem downItem = UIManager.Instance.GetDownItem();
			if ((Object)(object)downItem != (Object)null && downItem.IsDispaySuperSix())
			{
				downItem.AddSuperSixProgress();
			}
		}
	}

	private IEnumerator DoLuckyEchoThrow(float delay)
	{
		yield return (object)new WaitForSeconds(delay);
		if ((Object)(object)this == (Object)null || state != DiceVatState.Idle)
		{
			IsLuckyEcho = false;
			yield break;
		}
		Vector2 val = default(Vector2);
		((Vector2)(ref val))._002Ector(Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f));
		TryThrow(Vector2.op_Implicit(((Component)this).transform.position), ClampToBoundary(throwTargetPosition + val), pendingSource, sourceRoleType);
	}

	private void PlaySmoke(int completedRoleType)
	{
		if (!VatDebugSettings.ShowCoinSmoke)
		{
			HideSmokeEffect();
		}
		else
		{
			if (completedRoleType == 6 && GameDataManager.Instance.GetHelperAwakeSkillLevel(completedRoleType) > 1)
			{
				return;
			}
			if ((Object)(object)smokeAnimation != (Object)null)
			{
				PlaySmokeAnimation();
			}
			else
			{
				if (isLoadingSmokeEffect || (Object)(object)SingletonMono<ResManager>.Instance == (Object)null)
				{
					return;
				}
				isLoadingSmokeEffect = true;
				SingletonMono<ResManager>.Instance.LoadPrefabAsync("Assets/GameRes/Role/coins_smoke.prefab", delegate(GameObject prefab)
				{
					//IL_0055: Unknown result type (might be due to invalid IL or missing references)
					//IL_005f: Expected O, but got Unknown
					//IL_00be: Unknown result type (might be due to invalid IL or missing references)
					//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
					isLoadingSmokeEffect = false;
					if (!((Object)(object)this == (Object)null) && !((Object)(object)prefab == (Object)null))
					{
						UISpriteAnimationSequence component = prefab.GetComponent<UISpriteAnimationSequence>();
						Sprite[] array = (((Object)(object)component != (Object)null) ? component.SpriteFrames : null);
						if (array == null || array.Length == 0)
						{
							Debug.LogWarning((object)"[DiceVat] Missing smoke sprite frames: Assets/GameRes/Role/coins_smoke.prefab", (Object)(object)this);
						}
						else
						{
							Transform smokeParent = GetSmokeParent();
							smokeEffectObject = new GameObject("CoinsSmokeVat");
							smokeEffectObject.layer = (((Object)(object)smokeParent != (Object)null) ? ((Component)smokeParent).gameObject.layer : ((Component)this).gameObject.layer);
							smokeEffectObject.transform.SetParent(((Object)(object)smokeParent != (Object)null) ? smokeParent : ((Component)this).transform.parent, false);
							smokeEffectObject.transform.localScale = Vector3.one * 2f;
							smokeAnimation = smokeEffectObject.AddComponent<SpriteVatAnimationSequence>();
							smokeAnimation.UseExternalTick = false;
							smokeAnimation.AutoPlay = false;
							smokeAnimation.IsLooping = false;
							smokeAnimation.FrameInterval = (((Object)(object)component != (Object)null) ? component.FrameInterval : 0.033f);
							if ((Object)(object)cachedSpriteVatShader != (Object)null)
							{
								smokeAnimation.SetVatShader(cachedSpriteVatShader);
							}
							smokeAnimation.SetSortingLayerAndOrder(cachedSortingLayerName, GetSmokeSortingOrder());
							smokeAnimation.SpriteFrames = array;
							SpriteVatAnimationSequence spriteVatAnimationSequence = smokeAnimation;
							spriteVatAnimationSequence.OnAnimationComplete = (Action)Delegate.Combine(spriteVatAnimationSequence.OnAnimationComplete, new Action(HideSmokeEffect));
							PlaySmokeAnimation();
						}
					}
				});
			}
		}
	}

	private Transform GetSmokeParent()
	{
		MainSceneVat mainSceneVat = null;
		if ((Object)(object)UIManager.Instance != (Object)null)
		{
			mainSceneVat = UIManager.Instance.MainSceneVat;
		}
		Transform val = (((Object)(object)mainSceneVat != (Object)null) ? mainSceneVat.GetCoinsSmokePool() : null);
		if (!((Object)(object)val != (Object)null))
		{
			return ((Component)this).transform.parent;
		}
		return val;
	}

	private int GetSmokeSortingOrder()
	{
		MainSceneVat mainSceneVat = (((Object)(object)UIManager.Instance != (Object)null) ? UIManager.Instance.MainSceneVat : null);
		if (!((Object)(object)mainSceneVat != (Object)null))
		{
			return cachedSortingOrder + 1;
		}
		return mainSceneVat.EffectSortingOrder;
	}

	private void PlaySmokeAnimation()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)smokeAnimation == (Object)null) && !((Object)(object)smokeEffectObject == (Object)null))
		{
			smokeEffectObject.SetActive(true);
			smokeEffectObject.transform.position = ((Component)this).transform.position;
			smokeAnimation.IsLooping = false;
			smokeAnimation.Restart();
		}
	}

	private void HideSmokeEffect()
	{
		if ((Object)(object)smokeEffectObject != (Object)null)
		{
			smokeEffectObject.SetActive(false);
		}
	}

	private void EnsureSpineVat()
	{
		if ((Object)(object)spineVat == (Object)null)
		{
			spineVat = ((Component)this).GetComponent<SpineVat>();
		}
		if ((Object)(object)spineVat == (Object)null)
		{
			spineVat = ((Component)this).gameObject.AddComponent<SpineVat>();
		}
		spineVat.UseExternalTick = UseExternalTick;
		spineVat.SetDepthSortRange(boundaryMin.y, boundaryMax.y);
	}

	private Vector2 ClampToBoundary(Vector2 position)
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Abs(boundaryMax.x - boundaryMin.x);
		float num2 = Mathf.Abs(boundaryMax.y - boundaryMin.y);
		float num3 = Mathf.Min(0.4f, Mathf.Min(num, num2) * 0.2f);
		return new Vector2(Mathf.Clamp(position.x, boundaryMin.x + num3, boundaryMax.x - num3), Mathf.Clamp(position.y, boundaryMin.y + num3, boundaryMax.y - num3));
	}

	private static int RollFace(IReadOnlyList<float> weights)
	{
		float num = 0f;
		for (int i = 0; i < weights.Count; i++)
		{
			num += Mathf.Max(0f, weights[i]);
		}
		if (num <= 0f)
		{
			return Random.Range(1, 7);
		}
		float num2 = Random.value * num;
		for (int j = 0; j < weights.Count; j++)
		{
			num2 -= Mathf.Max(0f, weights[j]);
			if (num2 <= 0f)
			{
				return j + 1;
			}
		}
		return Mathf.Min(6, weights.Count);
	}
}
