using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Module.Vat;

[DisallowMultipleComponent]
public sealed class SpineVatStressTester : MonoBehaviour
{
	[Header("Templates")]
	[SerializeField]
	[FormerlySerializedAs("roleTemplates")]
	private SpineVat[] spineVatTemplates;

	[SerializeField]
	private bool autoCollectTemplates = true;

	[SerializeField]
	private bool collectTemplatesFromChildrenOnly;

	[SerializeField]
	private bool hideTemplateObjectsOnPlay = true;

	[SerializeField]
	private bool pickTemplateRandomly;

	[Header("Stress Counts")]
	[SerializeField]
	private int[] testCounts = new int[8] { 9, 50, 100, 200, 500, 1000, 1500, 2000 };

	[SerializeField]
	private int startCountIndex;

	[SerializeField]
	private int spawnBatchSize = 100;

	[Header("Spawn Area")]
	[SerializeField]
	private Vector2 boundaryMin = new Vector2(-8f, -12f);

	[SerializeField]
	private Vector2 boundaryMax = new Vector2(8f, 12f);

	[SerializeField]
	private float spawnZ;

	[SerializeField]
	private Vector2 randomScaleRange = Vector2.one;

	[SerializeField]
	private Transform instanceRoot;

	[Header("Spine VAT Behaviour")]
	[SerializeField]
	private bool enableRandomRoam = true;

	[SerializeField]
	private bool playRandomAnimationOnSpawn = true;

	[SerializeField]
	private string sortingLayerName;

	[SerializeField]
	private int baseSortingOrder;

	[Header("Controls")]
	[SerializeField]
	private KeyCode previousCountKey = (KeyCode)91;

	[SerializeField]
	private KeyCode nextCountKey = (KeyCode)93;

	[SerializeField]
	private KeyCode rebuildKey = (KeyCode)114;

	[SerializeField]
	private KeyCode clearKey = (KeyCode)8;

	[SerializeField]
	private KeyCode toggleOverlayKey = (KeyCode)282;

	[Header("Overlay")]
	[SerializeField]
	private bool showOverlay = true;

	[SerializeField]
	private int overlayFontSize = 18;

	[SerializeField]
	private bool autoScaleOverlay = true;

	[SerializeField]
	private float overlayScale = 1f;

	private readonly List<SpineVat> instances = new List<SpineVat>();

	private readonly List<SpineVat> validTemplates = new List<SpineVat>();

	private readonly List<GameObject> hiddenTemplateObjects = new List<GameObject>();

	private Coroutine spawnCoroutine;

	private GUIStyle overlayStyle;

	private GUIStyle buttonStyle;

	private float smoothedDeltaTime;

	private int currentCountIndex;

	private int targetCount;

	private bool isSpawning;

	public int CurrentInstanceCount => instances.Count;

	public int TargetCount => targetCount;

	public bool IsSpawning => isSpawning;

	private void Awake()
	{
		currentCountIndex = Mathf.Max(0, startCountIndex);
		CacheTemplates();
	}

	private void Start()
	{
		EnsureValidCounts();
		currentCountIndex = Mathf.Clamp(currentCountIndex, 0, testCounts.Length - 1);
		if (hideTemplateObjectsOnPlay)
		{
			SetTemplateObjectsActive(active: false);
		}
		Rebuild(testCounts[currentCountIndex]);
	}

	private void Update()
	{
		UpdateFrameStats();
		HandleKeyboardInput();
	}

	private void OnDestroy()
	{
		if (spawnCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(spawnCoroutine);
			spawnCoroutine = null;
		}
		ClearInstances();
		SetTemplateObjectsActive(active: true);
	}

	private void OnValidate()
	{
		startCountIndex = Mathf.Max(0, startCountIndex);
		spawnBatchSize = Mathf.Max(1, spawnBatchSize);
		overlayFontSize = Mathf.Max(10, overlayFontSize);
		overlayScale = Mathf.Clamp(overlayScale, 0.5f, 5f);
		if (boundaryMax.x < boundaryMin.x)
		{
			float x = boundaryMin.x;
			boundaryMin.x = boundaryMax.x;
			boundaryMax.x = x;
		}
		if (boundaryMax.y < boundaryMin.y)
		{
			float y = boundaryMin.y;
			boundaryMin.y = boundaryMax.y;
			boundaryMax.y = y;
		}
		randomScaleRange.x = Mathf.Max(0.001f, randomScaleRange.x);
		randomScaleRange.y = Mathf.Max(randomScaleRange.x, randomScaleRange.y);
		if (testCounts == null || testCounts.Length == 0)
		{
			testCounts = new int[6] { 9, 50, 100, 200, 500, 1000 };
		}
		for (int i = 0; i < testCounts.Length; i++)
		{
			testCounts[i] = Mathf.Max(0, testCounts[i]);
		}
	}

	private void OnGUI()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		if (showOverlay)
		{
			EnsureGuiStyles();
			Matrix4x4 matrix = GUI.matrix;
			float num = GetOverlayScale();
			GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(num, num, 1f)) * matrix;
			GUILayout.BeginArea(new Rect(12f, 12f, 430f, 270f), GUI.skin.box);
			GUILayout.Label("Spine VAT Stress Tester", overlayStyle, Array.Empty<GUILayoutOption>());
			GUILayout.Label($"FPS: {GetFps():F1}    Frame: {GetFrameMs():F2} ms", overlayStyle, Array.Empty<GUILayoutOption>());
			GUILayout.Label($"Instances: {instances.Count} / {targetCount}", overlayStyle, Array.Empty<GUILayoutOption>());
			GUILayout.Label(string.Format("Templates: {0}    Spawning: {1}", validTemplates.Count, isSpawning ? "Yes" : "No"), overlayStyle, Array.Empty<GUILayoutOption>());
			GUILayout.Label($"Keys: {previousCountKey} / {nextCountKey} switch, {rebuildKey} rebuild, {clearKey} clear, {toggleOverlayKey} overlay", overlayStyle, Array.Empty<GUILayoutOption>());
			GUILayout.Space(6f);
			GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
			if (GUILayout.Button("Prev", buttonStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(78f) }))
			{
				SelectCountOffset(-1);
			}
			if (GUILayout.Button("Rebuild", buttonStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(96f) }))
			{
				RebuildCurrentCount();
			}
			if (GUILayout.Button("Next", buttonStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(78f) }))
			{
				SelectCountOffset(1);
			}
			if (GUILayout.Button("Clear", buttonStyle, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(78f) }))
			{
				Clear();
			}
			GUILayout.EndHorizontal();
			GUILayout.EndArea();
			GUI.matrix = matrix;
		}
	}

	[ContextMenu("Rebuild Current Count")]
	public void RebuildCurrentCount()
	{
		EnsureValidCounts();
		currentCountIndex = Mathf.Clamp(currentCountIndex, 0, testCounts.Length - 1);
		Rebuild(testCounts[currentCountIndex]);
	}

	[ContextMenu("Clear Instances")]
	public void Clear()
	{
		if (spawnCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(spawnCoroutine);
			spawnCoroutine = null;
		}
		isSpawning = false;
		targetCount = 0;
		ClearInstances();
	}

	public void SetCountIndex(int index)
	{
		EnsureValidCounts();
		currentCountIndex = Mathf.Clamp(index, 0, testCounts.Length - 1);
		Rebuild(testCounts[currentCountIndex]);
	}

	public void SetCount(int count)
	{
		Rebuild(Mathf.Max(0, count));
	}

	public void Rebuild(int count)
	{
		CacheTemplates();
		targetCount = Mathf.Max(0, count);
		if (spawnCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(spawnCoroutine);
		}
		ClearInstances();
		if (validTemplates.Count == 0)
		{
			isSpawning = false;
			Debug.LogWarning((object)"[SpineVatStressTester] No valid SpineVat templates found. Drag scene SpineVat objects into Spine VAT Templates, or enable Auto Collect Templates.", (Object)(object)this);
		}
		else
		{
			spawnCoroutine = ((MonoBehaviour)this).StartCoroutine(SpawnRoutine(targetCount));
		}
	}

	private IEnumerator SpawnRoutine(int count)
	{
		isSpawning = true;
		int spawnedThisFrame = 0;
		for (int i = 0; i < count; i++)
		{
			SpineVat spineVat = PickTemplate(i);
			if (!((Object)(object)spineVat == (Object)null) && !((Object)(object)spineVat.VatAsset == (Object)null))
			{
				SpineVat spineVat2 = SpawnInstance(spineVat, i);
				if ((Object)(object)spineVat2 != (Object)null)
				{
					instances.Add(spineVat2);
				}
				spawnedThisFrame++;
				if (spawnedThisFrame >= spawnBatchSize)
				{
					spawnedThisFrame = 0;
					yield return null;
				}
			}
		}
		isSpawning = false;
		spawnCoroutine = null;
	}

	private SpineVat SpawnInstance(SpineVat template, int index)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = Object.Instantiate<GameObject>(((Component)template).gameObject);
		((Object)val).name = $"SpineVat_Stress_{index:0000}_{((Object)template).name}";
		Transform transform = val.transform;
		Transform val2 = (((Object)(object)instanceRoot != (Object)null) ? instanceRoot : ((Component)this).transform);
		transform.SetParent(val2, true);
		transform.position = PickSpawnPosition();
		transform.localScale = ((Component)template).transform.localScale * Random.Range(randomScaleRange.x, randomScaleRange.y);
		val.SetActive(true);
		SpineVat component = val.GetComponent<SpineVat>();
		if ((Object)(object)component == (Object)null)
		{
			Object.Destroy((Object)(object)val);
			return null;
		}
		component.Initialize(template.VatAsset, boundaryMin, boundaryMax, enableRandomRoam);
		component.SetDepthSortRange(boundaryMin.y, boundaryMax.y);
		component.SetSortingLayerAndOrder(sortingLayerName, baseSortingOrder);
		if (playRandomAnimationOnSpawn)
		{
			component.PlayRandom();
		}
		return component;
	}

	private SpineVat PickTemplate(int index)
	{
		if (validTemplates.Count == 0)
		{
			return null;
		}
		if (pickTemplateRandomly)
		{
			return validTemplates[Random.Range(0, validTemplates.Count)];
		}
		return validTemplates[index % validTemplates.Count];
	}

	private Vector3 PickSpawnPosition()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		return new Vector3(Random.Range(boundaryMin.x, boundaryMax.x), Random.Range(boundaryMin.y, boundaryMax.y), spawnZ);
	}

	private void CacheTemplates()
	{
		validTemplates.Clear();
		if (spineVatTemplates != null)
		{
			for (int i = 0; i < spineVatTemplates.Length; i++)
			{
				AddTemplateIfValid(spineVatTemplates[i]);
			}
		}
		if (autoCollectTemplates && validTemplates.Count <= 0)
		{
			SpineVat[] array = (collectTemplatesFromChildrenOnly ? ((Component)this).GetComponentsInChildren<SpineVat>(true) : Object.FindObjectsOfType<SpineVat>(true));
			for (int j = 0; j < array.Length; j++)
			{
				AddTemplateIfValid(array[j]);
			}
		}
	}

	private void AddTemplateIfValid(SpineVat template)
	{
		if (!((Object)(object)template == (Object)null) && !((Object)(object)template.VatAsset == (Object)null) && !instances.Contains(template) && !validTemplates.Contains(template))
		{
			validTemplates.Add(template);
		}
	}

	private void SetTemplateObjectsActive(bool active)
	{
		if (active)
		{
			for (int i = 0; i < hiddenTemplateObjects.Count; i++)
			{
				if ((Object)(object)hiddenTemplateObjects[i] != (Object)null)
				{
					hiddenTemplateObjects[i].SetActive(true);
				}
			}
			hiddenTemplateObjects.Clear();
			return;
		}
		hiddenTemplateObjects.Clear();
		for (int j = 0; j < validTemplates.Count; j++)
		{
			SpineVat spineVat = validTemplates[j];
			if (!((Object)(object)spineVat == (Object)null) && ((Component)spineVat).gameObject.activeSelf)
			{
				hiddenTemplateObjects.Add(((Component)spineVat).gameObject);
				((Component)spineVat).gameObject.SetActive(false);
			}
		}
	}

	private void ClearInstances()
	{
		for (int num = instances.Count - 1; num >= 0; num--)
		{
			SpineVat spineVat = instances[num];
			if ((Object)(object)spineVat != (Object)null)
			{
				Object.Destroy((Object)(object)((Component)spineVat).gameObject);
			}
		}
		instances.Clear();
	}

	private void HandleKeyboardInput()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		if (Input.GetKeyDown(toggleOverlayKey))
		{
			showOverlay = !showOverlay;
		}
		if (Input.GetKeyDown(previousCountKey))
		{
			SelectCountOffset(-1);
		}
		if (Input.GetKeyDown(nextCountKey))
		{
			SelectCountOffset(1);
		}
		if (Input.GetKeyDown(rebuildKey))
		{
			RebuildCurrentCount();
		}
		if (Input.GetKeyDown(clearKey))
		{
			Clear();
		}
	}

	private void SelectCountOffset(int offset)
	{
		EnsureValidCounts();
		currentCountIndex = Mathf.Clamp(currentCountIndex + offset, 0, testCounts.Length - 1);
		Rebuild(testCounts[currentCountIndex]);
	}

	private void UpdateFrameStats()
	{
		float unscaledDeltaTime = Time.unscaledDeltaTime;
		if (smoothedDeltaTime <= 0f)
		{
			smoothedDeltaTime = unscaledDeltaTime;
		}
		else
		{
			smoothedDeltaTime += (unscaledDeltaTime - smoothedDeltaTime) * 0.1f;
		}
	}

	private float GetFps()
	{
		if (!(smoothedDeltaTime > 0f))
		{
			return 0f;
		}
		return 1f / smoothedDeltaTime;
	}

	private float GetFrameMs()
	{
		return smoothedDeltaTime * 1000f;
	}

	private float GetOverlayScale()
	{
		float num = Mathf.Max(0.5f, overlayScale);
		if (autoScaleOverlay)
		{
			float num2 = ((Screen.height > 0) ? ((float)Screen.height / 1080f) : 1f);
			num *= Mathf.Clamp(num2, 1f, 3f);
		}
		return num;
	}

	private void EnsureValidCounts()
	{
		if (testCounts == null || testCounts.Length == 0)
		{
			testCounts = new int[6] { 9, 50, 100, 200, 500, 1000 };
		}
		for (int i = 0; i < testCounts.Length; i++)
		{
			testCounts[i] = Mathf.Max(0, testCounts[i]);
		}
	}

	private void EnsureGuiStyles()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Expected O, but got Unknown
		if (overlayStyle == null || overlayStyle.fontSize != overlayFontSize)
		{
			overlayStyle = new GUIStyle(GUI.skin.label)
			{
				fontSize = overlayFontSize
			};
			overlayStyle.normal.textColor = Color.white;
			buttonStyle = new GUIStyle(GUI.skin.button)
			{
				fontSize = Mathf.Max(12, overlayFontSize - 2)
			};
		}
	}
}
