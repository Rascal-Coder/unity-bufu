using System;
using System.Collections.Generic;
using Module.Role;
using UnityEngine;
using UnityEngine.Rendering;

namespace Module.Vat;

[AddComponentMenu("VAT/Coin VAT Batch Renderer")]
[DisallowMultipleComponent]
public sealed class CoinVatBatchRenderer : MonoBehaviour
{
	private sealed class Batch
	{
		public readonly int Key;

		public readonly List<Matrix4x4> Matrices = new List<Matrix4x4>(64);

		public Mesh Mesh { get; private set; }

		public Texture Texture { get; private set; }

		public Vector4 UvRect { get; private set; }

		public bool UseUvRect { get; private set; }

		public Color Color { get; private set; }

		public int YBucket { get; private set; }

		public bool IsActive { get; private set; }

		public int Count => Matrices.Count;

		public Batch(int key, SpriteVatAnimationSequence.BatchRenderData renderData, int yBucket)
		{
			Key = key;
			Reset(renderData, yBucket);
		}

		public void Reset(SpriteVatAnimationSequence.BatchRenderData renderData, int yBucket)
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			Mesh = renderData.Mesh;
			Texture = renderData.Texture;
			UvRect = renderData.UvRect;
			UseUvRect = renderData.UseUvRect;
			Color = renderData.Color;
			YBucket = yBucket;
			IsActive = true;
			Matrices.Clear();
		}

		public void Add(Matrix4x4 matrix)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			Matrices.Add(matrix);
		}

		public void Clear()
		{
			Matrices.Clear();
			IsActive = false;
		}
	}

	private const string DefaultShaderName = "VAT/Sprite Sequence Unlit";

	private const int MaxInstancesPerDraw = 1023;

	private static readonly int MainTexId = Shader.PropertyToID("_MainTex");

	private static readonly int UvRectId = Shader.PropertyToID("_UvRect");

	private static readonly int UseUvRectId = Shader.PropertyToID("_UseUvRect");

	private static readonly int TintId = Shader.PropertyToID("_Color");

	private static readonly int ZWriteId = Shader.PropertyToID("_ZWrite");

	private static readonly Comparison<Batch> BatchComparison = CompareBatches;

	[SerializeField]
	private Shader vatShader;

	[SerializeField]
	[Range(4f, 128f)]
	private int yBucketCount = 24;

	[SerializeField]
	private int transparentQueueBase = 3000;

	[SerializeField]
	private ShadowCastingMode shadowCastingMode;

	[SerializeField]
	private bool receiveShadows;

	private readonly List<CoinVat> registeredCoins = new List<CoinVat>();

	private readonly Dictionary<int, Batch> batchesByKey = new Dictionary<int, Batch>();

	private readonly List<Batch> activeBatches = new List<Batch>();

	private readonly Dictionary<int, Material> materialsByBucket = new Dictionary<int, Material>();

	private readonly Matrix4x4[] matrixBuffer = (Matrix4x4[])(object)new Matrix4x4[1023];

	private MaterialPropertyBlock propertyBlock;

	private Vector2 boundaryMin;

	private Vector2 boundaryMax;

	private int renderLayer;

	private int baseSortingOrder;

	private bool initialized;

	private bool hasConfigured;

	private bool missingShaderLogged;

	public bool IsAvailable
	{
		get
		{
			if (initialized && (Object)(object)vatShader != (Object)null)
			{
				return SystemInfo.supportsInstancing;
			}
			return false;
		}
	}

	public int RegisteredCoinCount => registeredCoins.Count;

	public int ActiveBatchCount => activeBatches.Count;

	public void Configure(Shader shader, Vector2 min, Vector2 max, int layer, int sortingOrder, int bucketCount)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		vatShader = (((Object)(object)shader != (Object)null) ? shader : vatShader);
		boundaryMin = min;
		boundaryMax = max;
		renderLayer = layer;
		baseSortingOrder = sortingOrder;
		yBucketCount = Mathf.Clamp(bucketCount, 4, 128);
		((Component)this).gameObject.layer = layer;
		hasConfigured = true;
		InitializeRuntime();
	}

	public void RegisterCoin(CoinVat coin)
	{
		if (!((Object)(object)coin == (Object)null) && !registeredCoins.Contains(coin))
		{
			registeredCoins.Add(coin);
		}
	}

	public void UnregisterCoin(CoinVat coin)
	{
		if (!((Object)(object)coin == (Object)null))
		{
			registeredCoins.Remove(coin);
		}
	}

	public void ClearRegisteredCoins()
	{
		registeredCoins.Clear();
		ResetBatches();
	}

	private void Awake()
	{
		InitializeRuntime();
	}

	private void LateUpdate()
	{
		if (IsAvailable && registeredCoins.Count != 0)
		{
			ResetBatches();
			FillBatches();
			DrawBatches();
		}
	}

	private void OnDestroy()
	{
		ClearRegisteredCoins();
		ReleaseMaterials();
	}

	private void OnValidate()
	{
		yBucketCount = Mathf.Clamp(yBucketCount, 4, 128);
		transparentQueueBase = Mathf.Clamp(transparentQueueBase, 2500, 3990);
	}

	private void InitializeRuntime()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Expected O, but got Unknown
		if (propertyBlock == null)
		{
			propertyBlock = new MaterialPropertyBlock();
		}
		if ((Object)(object)vatShader == (Object)null)
		{
			vatShader = Shader.Find("VAT/Sprite Sequence Unlit");
		}
		initialized = (Object)(object)vatShader != (Object)null && SystemInfo.supportsInstancing;
		if (hasConfigured && !initialized && !missingShaderLogged)
		{
			missingShaderLogged = true;
			Debug.LogWarning((object)"[CoinVatBatchRenderer] GPU instancing or VAT sprite shader is not available.", (Object)(object)this);
		}
	}

	private void FillBatches()
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		for (int num = registeredCoins.Count - 1; num >= 0; num--)
		{
			CoinVat coinVat = registeredCoins[num];
			if ((Object)(object)coinVat == (Object)null)
			{
				registeredCoins.RemoveAt(num);
			}
			else if (((Behaviour)coinVat).isActiveAndEnabled && ((Component)coinVat).gameObject.activeInHierarchy)
			{
				SpriteVatAnimationSequence spriteAnimation = coinVat.SpriteAnimation;
				if (!((Object)(object)spriteAnimation == (Object)null) && spriteAnimation.TryGetBatchRenderData(out var data))
				{
					int yBucket = GetYBucket(((Component)coinVat).transform.position.y);
					int key = CreateBatchKey(data, yBucket);
					GetOrCreateBatch(key, data, yBucket).Add(((Component)coinVat).transform.localToWorldMatrix);
				}
			}
		}
	}

	private Batch GetOrCreateBatch(int key, SpriteVatAnimationSequence.BatchRenderData renderData, int yBucket)
	{
		if (!batchesByKey.TryGetValue(key, out var value))
		{
			value = new Batch(key, renderData, yBucket);
			batchesByKey.Add(key, value);
		}
		if (!value.IsActive)
		{
			value.Reset(renderData, yBucket);
			activeBatches.Add(value);
		}
		return value;
	}

	private void DrawBatches()
	{
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		activeBatches.Sort(BatchComparison);
		for (int i = 0; i < activeBatches.Count; i++)
		{
			Batch batch = activeBatches[i];
			if (batch.Count <= 0 || (Object)(object)batch.Mesh == (Object)null || (Object)(object)batch.Texture == (Object)null)
			{
				continue;
			}
			Material material = GetMaterial(batch.YBucket);
			if ((Object)(object)material == (Object)null)
			{
				continue;
			}
			propertyBlock.Clear();
			propertyBlock.SetTexture(MainTexId, batch.Texture);
			propertyBlock.SetVector(UvRectId, batch.UvRect);
			propertyBlock.SetFloat(UseUvRectId, batch.UseUvRect ? 1f : 0f);
			propertyBlock.SetColor(TintId, batch.Color);
			List<Matrix4x4> matrices = batch.Matrices;
			for (int j = 0; j < matrices.Count; j += 1023)
			{
				int num = Mathf.Min(1023, matrices.Count - j);
				for (int k = 0; k < num; k++)
				{
					matrixBuffer[k] = matrices[j + k];
				}
				Graphics.DrawMeshInstanced(batch.Mesh, 0, material, matrixBuffer, num, propertyBlock, shadowCastingMode, receiveShadows, renderLayer, (Camera)null, (LightProbeUsage)0);
			}
		}
	}

	private Material GetMaterial(int yBucket)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Expected O, but got Unknown
		if (materialsByBucket.TryGetValue(yBucket, out var value) && (Object)(object)value != (Object)null)
		{
			return value;
		}
		if ((Object)(object)vatShader == (Object)null)
		{
			return null;
		}
		value = new Material(vatShader)
		{
			name = $"CoinVatBatch_Bucket_{yBucket}",
			enableInstancing = true,
			renderQueue = Mathf.Clamp(transparentQueueBase + baseSortingOrder + yBucket, 2500, 3999)
		};
		if (value.HasProperty(ZWriteId))
		{
			value.SetFloat(ZWriteId, 0f);
		}
		materialsByBucket[yBucket] = value;
		return value;
	}

	private void ResetBatches()
	{
		for (int i = 0; i < activeBatches.Count; i++)
		{
			activeBatches[i].Clear();
		}
		activeBatches.Clear();
	}

	private int GetYBucket(float y)
	{
		if (yBucketCount <= 1)
		{
			return 0;
		}
		float num = Mathf.Min(boundaryMin.y, boundaryMax.y);
		float num2 = Mathf.Max(boundaryMin.y, boundaryMax.y);
		if (Mathf.Approximately(num, num2))
		{
			return 0;
		}
		float num3 = Mathf.InverseLerp(num, num2, y);
		return Mathf.Clamp(Mathf.RoundToInt((1f - num3) * (float)(yBucketCount - 1)), 0, yBucketCount - 1);
	}

	private void ReleaseMaterials()
	{
		foreach (KeyValuePair<int, Material> item in materialsByBucket)
		{
			if ((Object)(object)item.Value != (Object)null)
			{
				Object.Destroy((Object)(object)item.Value);
			}
		}
		materialsByBucket.Clear();
	}

	private static int CompareBatches(Batch left, Batch right)
	{
		int num = left.YBucket.CompareTo(right.YBucket);
		if (num != 0)
		{
			return num;
		}
		return left.Key.CompareTo(right.Key);
	}

	private static int CreateBatchKey(SpriteVatAnimationSequence.BatchRenderData renderData, int yBucket)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		int num = (renderData.UseUvRect ? 1 : 0);
		Vector4 val = (renderData.UseUvRect ? renderData.UvRect : Vector4.zero);
		Color32 val2 = Color32.op_Implicit(renderData.Color);
		int num2 = val2.r | (val2.g << 8) | (val2.b << 16) | (val2.a << 24);
		return ((((((((((((((((((Object)(object)renderData.Mesh != (Object)null) ? ((Object)renderData.Mesh).GetInstanceID() : 0) * 397) ^ (((Object)(object)renderData.Texture != (Object)null) ? ((Object)renderData.Texture).GetInstanceID() : 0)) * 397) ^ Quantize(val.x)) * 397) ^ Quantize(val.y)) * 397) ^ Quantize(val.z)) * 397) ^ Quantize(val.w)) * 397) ^ num2) * 397) ^ num) * 397) ^ yBucket;
	}

	private static int Quantize(float value)
	{
		return Mathf.RoundToInt(value * 100000f);
	}
}
