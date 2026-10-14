using System.Collections.Generic;
using Spine;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using cfg;

public sealed class WorldSelectionPanel : MonoBehaviour
{
	[Header("Top Components")]
	[SerializeField]
	private Button closeButton;

	[SerializeField]
	private Button maskButton;

	[Header("Navigation")]
	[SerializeField]
	private ScrollRect worldScrollView;

	[SerializeField]
	[Min(0.1f)]
	private float guideAutoScrollSmoothTime = 0.65f;

	[Header("World Node Prefab Bindings")]
	[SerializeField]
	private WorldNode worldNodePrefab;

	[SerializeField]
	private WorldNode worldNodePrefab1;

	[SerializeField]
	private WorldNode ComingSoonPrefab1;

	private List<WorldNode> _spawnedNodes = new List<WorldNode>();

	private readonly Dictionary<WorldNode, WorldNode> _nodeSourcePrefab = new Dictionary<WorldNode, WorldNode>();

	[Header("Status UI")]
	[SerializeField]
	private TextMeshProUGUI statusText;

	[Header("Guide")]
	[SerializeField]
	private SkeletonGraphic guideAnimationObj;

	private List<world> _worldConfigs = new List<world>();

	private const bool ShowComingSoonWorldNode = true;

	private const string WorldSelectionGuideShownKey = "HasShownWorldSelectionGuide";

	private const int DiceWorldId = 2;

	private const string ComingSoonWorldName = "敬请期待";

	private const float SnapVelocityThreshold = 40f;

	private const float SnapSmoothTime = 0.14f;

	private const float SnapVisualOffset = 0f;

	private bool _isDragging;

	private bool _snapPending;

	private bool _hasSnapTarget;

	private float _snapTargetNormalizedPosition;

	private float _snapVelocity;

	private float _currentSnapSmoothTime = 0.14f;

	private int _guidePlayCount;

	private int DisplayWorldCount => _worldConfigs.Count + 1;

	private static bool IsComingSoonNodeIndex(int index)
	{
		return index == 0;
	}

	private static int ToConfigIndex(int nodeIndex)
	{
		return nodeIndex - 1;
	}

	private WorldNode GetPrefabForIndex(int index)
	{
		if (IsComingSoonNodeIndex(index))
		{
			if (!((Object)(object)ComingSoonPrefab1 != (Object)null))
			{
				return worldNodePrefab;
			}
			return ComingSoonPrefab1;
		}
		int num = ToConfigIndex(index);
		if (num >= 0 && num < _worldConfigs.Count)
		{
			switch (_worldConfigs[num].Id)
			{
			case 1:
				if (!((Object)(object)worldNodePrefab != (Object)null))
				{
					return worldNodePrefab1;
				}
				return worldNodePrefab;
			case 2:
				if (!((Object)(object)worldNodePrefab1 != (Object)null))
				{
					return worldNodePrefab;
				}
				return worldNodePrefab1;
			}
		}
		if (!((Object)(object)worldNodePrefab1 != (Object)null))
		{
			return worldNodePrefab;
		}
		return worldNodePrefab1;
	}

	private void Awake()
	{
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Expected O, but got Unknown
		if ((Object)(object)closeButton != (Object)null)
		{
			((Component)closeButton).gameObject.SetActive(false);
		}
		Transform val = ((Component)this).transform.Find("Background");
		if ((Object)(object)val != (Object)null)
		{
			Image component = ((Component)val).GetComponent<Image>();
			if ((Object)(object)component != (Object)null)
			{
				((Graphic)component).raycastTarget = true;
			}
		}
		if ((Object)(object)worldNodePrefab != (Object)null)
		{
			((Component)worldNodePrefab).gameObject.SetActive(false);
		}
		if ((Object)(object)worldNodePrefab1 != (Object)null)
		{
			((Component)worldNodePrefab1).gameObject.SetActive(false);
		}
		if ((Object)(object)ComingSoonPrefab1 != (Object)null)
		{
			((Component)ComingSoonPrefab1).gameObject.SetActive(false);
		}
		if ((Object)(object)guideAnimationObj != (Object)null)
		{
			((Component)guideAnimationObj).gameObject.SetActive(false);
		}
		if ((Object)(object)worldScrollView != (Object)null && (Object)(object)worldScrollView.content != (Object)null)
		{
			foreach (Transform item in (Transform)worldScrollView.content)
			{
				Transform val2 = item;
				if ((!((Object)(object)worldNodePrefab != (Object)null) || !((Object)(object)((Component)val2).gameObject == (Object)(object)((Component)worldNodePrefab).gameObject)) && (!((Object)(object)worldNodePrefab1 != (Object)null) || !((Object)(object)((Component)val2).gameObject == (Object)(object)((Component)worldNodePrefab1).gameObject)) && (!((Object)(object)ComingSoonPrefab1 != (Object)null) || !((Object)(object)((Component)val2).gameObject == (Object)(object)((Component)ComingSoonPrefab1).gameObject)))
				{
					Object.Destroy((Object)(object)((Component)val2).gameObject);
				}
			}
		}
		EnsureDragRelay();
	}

	public void Open()
	{
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Expected O, but got Unknown
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Expected O, but got Unknown
		((Component)this).gameObject.SetActive(true);
		RefreshUI();
		int num = ((!((Object)(object)GameDataManager.Instance != (Object)null)) ? 1 : GameDataManager.Instance.GetCurrentWorldId());
		bool flag = PlayerPrefs.GetInt("HasShownWorldSelectionGuide", 0) != 0;
		bool flag2 = !flag && num < 2;
		if (flag2)
		{
			SmoothScrollToWorld(2);
			PlayerPrefs.SetInt("HasShownWorldSelectionGuide", 1);
			PlayerPrefs.Save();
		}
		else if (!flag && num >= 2)
		{
			PlayerPrefs.SetInt("HasShownWorldSelectionGuide", 1);
			PlayerPrefs.Save();
		}
		if (!((Object)(object)guideAnimationObj != (Object)null))
		{
			return;
		}
		if (flag2)
		{
			((Component)guideAnimationObj).gameObject.SetActive(true);
			_guidePlayCount = 0;
			if (guideAnimationObj.AnimationState != null)
			{
				guideAnimationObj.AnimationState.SetAnimation(0, "shangxiatuodong", false);
				guideAnimationObj.AnimationState.Complete -= new TrackEntryDelegate(OnGuideAnimationComplete);
				guideAnimationObj.AnimationState.Complete += new TrackEntryDelegate(OnGuideAnimationComplete);
			}
		}
		else
		{
			((Component)guideAnimationObj).gameObject.SetActive(false);
		}
	}

	private void OnGuideAnimationComplete(TrackEntry trackEntry)
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		if (trackEntry.Animation.Name != "shangxiatuodong")
		{
			return;
		}
		_guidePlayCount++;
		if (_guidePlayCount >= 2)
		{
			if ((Object)(object)guideAnimationObj != (Object)null)
			{
				((Component)guideAnimationObj).gameObject.SetActive(false);
				if (guideAnimationObj.AnimationState != null)
				{
					guideAnimationObj.AnimationState.Complete -= new TrackEntryDelegate(OnGuideAnimationComplete);
				}
			}
		}
		else if ((Object)(object)guideAnimationObj != (Object)null && guideAnimationObj.AnimationState != null)
		{
			guideAnimationObj.AnimationState.SetAnimation(0, "shangxiatuodong", false);
		}
	}

	public void Close()
	{
		ResetSnapState();
		if ((Object)(object)worldScrollView != (Object)null)
		{
			worldScrollView.StopMovement();
		}
		((Component)this).gameObject.SetActive(false);
	}

	private void Update()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if (!_snapPending || _isDragging || (Object)(object)worldScrollView == (Object)null)
		{
			return;
		}
		if (!_hasSnapTarget)
		{
			Vector2 velocity = worldScrollView.velocity;
			if (((Vector2)(ref velocity)).sqrMagnitude > 1600f)
			{
				return;
			}
		}
		if (!_hasSnapTarget)
		{
			_snapTargetNormalizedPosition = FindNearestNodeScrollPosition();
			_hasSnapTarget = true;
			_snapVelocity = 0f;
			worldScrollView.StopMovement();
		}
		float num = Mathf.SmoothDamp(worldScrollView.verticalNormalizedPosition, _snapTargetNormalizedPosition, ref _snapVelocity, _currentSnapSmoothTime, float.PositiveInfinity, Time.unscaledDeltaTime);
		worldScrollView.verticalNormalizedPosition = num;
		if (Mathf.Abs(num - _snapTargetNormalizedPosition) <= 0.001f && Mathf.Abs(_snapVelocity) <= 0.01f)
		{
			worldScrollView.verticalNormalizedPosition = _snapTargetNormalizedPosition;
			ResetSnapState();
		}
	}

	private void RefreshUI()
	{
		//IL_032f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0334: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0422: Unknown result type (might be due to invalid IL or missing references)
		//IL_0427: Unknown result type (might be due to invalid IL or missing references)
		//IL_043b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0440: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)ConfigManager.Instance == (Object)null || (Object)(object)GameDataManager.Instance == (Object)null)
		{
			Debug.LogError((object)"[WorldSelectionPanel] ConfigManager or GameDataManager instance is missing.");
			return;
		}
		if ((Object)(object)statusText != (Object)null)
		{
			((TMP_Text)statusText).text = string.Empty;
		}
		_worldConfigs = new List<world>(ConfigManager.Instance.GetAllWorldConfigs());
		_worldConfigs.Sort(delegate(world left, world right)
		{
			int num13 = right.Sort.CompareTo(left.Sort);
			return (num13 == 0) ? right.Id.CompareTo(left.Id) : num13;
		});
		if ((Object)(object)worldScrollView != (Object)null && (Object)(object)worldScrollView.content != (Object)null)
		{
			VerticalLayoutGroup component = ((Component)worldScrollView.content).GetComponent<VerticalLayoutGroup>();
			if ((Object)(object)component != (Object)null)
			{
				((Behaviour)component).enabled = true;
				((HorizontalOrVerticalLayoutGroup)component).reverseArrangement = false;
			}
		}
		int currentWorldId = GameDataManager.Instance.GetCurrentWorldId();
		int displayWorldCount = DisplayWorldCount;
		for (int num = 0; num < displayWorldCount; num++)
		{
			WorldNode prefabForIndex = GetPrefabForIndex(num);
			if ((Object)(object)prefabForIndex == (Object)null)
			{
				continue;
			}
			if (num < _spawnedNodes.Count && (Object)(object)_spawnedNodes[num] != (Object)null)
			{
				if (!_nodeSourcePrefab.TryGetValue(_spawnedNodes[num], out var value) || (Object)(object)value != (Object)(object)prefabForIndex)
				{
					_nodeSourcePrefab.Remove(_spawnedNodes[num]);
					Object.Destroy((Object)(object)((Component)_spawnedNodes[num]).gameObject);
					WorldNode worldNode = Object.Instantiate<WorldNode>(prefabForIndex, (Transform)(object)worldScrollView.content);
					_spawnedNodes[num] = worldNode;
					_nodeSourcePrefab[worldNode] = prefabForIndex;
				}
			}
			else
			{
				WorldNode worldNode2 = Object.Instantiate<WorldNode>(prefabForIndex, (Transform)(object)worldScrollView.content);
				if (num < _spawnedNodes.Count)
				{
					_spawnedNodes[num] = worldNode2;
				}
				else
				{
					_spawnedNodes.Add(worldNode2);
				}
				_nodeSourcePrefab[worldNode2] = prefabForIndex;
			}
			((Component)_spawnedNodes[num]).transform.SetSiblingIndex(num);
		}
		for (int num2 = 0; num2 < _spawnedNodes.Count; num2++)
		{
			WorldNode worldNode3 = _spawnedNodes[num2];
			if ((Object)(object)worldNode3 == (Object)null)
			{
				continue;
			}
			if (num2 < displayWorldCount)
			{
				((Component)worldNode3).gameObject.SetActive(true);
				if (IsComingSoonNodeIndex(num2))
				{
					ConfigureComingSoonNode(worldNode3, num2);
					continue;
				}
				int num3 = ToConfigIndex(num2);
				if (num3 >= 0 && num3 < _worldConfigs.Count)
				{
					world config = _worldConfigs[num3];
					ConfigureWorldNode(worldNode3, config, currentWorldId, num2);
				}
				else
				{
					((Component)worldNode3).gameObject.SetActive(false);
				}
			}
			else
			{
				((Component)worldNode3).gameObject.SetActive(false);
			}
		}
		Canvas.ForceUpdateCanvases();
		if ((Object)(object)worldScrollView != (Object)null && (Object)(object)worldScrollView.content != (Object)null && (Object)(object)worldScrollView.viewport != (Object)null)
		{
			Rect rect = worldScrollView.viewport.rect;
			float height = ((Rect)(ref rect)).height;
			float num4 = 0f;
			float num5 = 0f;
			float num6 = 260f;
			VerticalLayoutGroup component2 = ((Component)worldScrollView.content).GetComponent<VerticalLayoutGroup>();
			if ((Object)(object)component2 != (Object)null)
			{
				((Behaviour)component2).enabled = true;
				((HorizontalOrVerticalLayoutGroup)component2).reverseArrangement = false;
				((LayoutGroup)component2).padding.top = Mathf.RoundToInt(height * 0.35f);
				((LayoutGroup)component2).padding.bottom = Mathf.RoundToInt(height * 0.45f);
				num4 = ((LayoutGroup)component2).padding.top;
				num5 = ((LayoutGroup)component2).padding.bottom;
				num6 = ((HorizontalOrVerticalLayoutGroup)component2).spacing;
			}
			int num7 = displayWorldCount;
			float num8 = 0f;
			for (int num9 = 0; num9 < num7 && num9 < _spawnedNodes.Count; num9++)
			{
				if (!((Object)(object)_spawnedNodes[num9] != (Object)null))
				{
					continue;
				}
				Transform transform = ((Component)_spawnedNodes[num9]).transform;
				RectTransform val = (RectTransform)(object)((transform is RectTransform) ? transform : null);
				if ((Object)(object)val != (Object)null)
				{
					rect = val.rect;
					if (((Rect)(ref rect)).height > 0f)
					{
						float num10 = num8;
						rect = val.rect;
						num8 = num10 + ((Rect)(ref rect)).height;
						continue;
					}
				}
				num8 += 1200f;
			}
			float num11 = num4 + num5 + num8 + (float)Mathf.Max(0, num7 - 1) * num6;
			num11 = Mathf.Max(height + 100f, num11);
			worldScrollView.content.sizeDelta = new Vector2(worldScrollView.content.sizeDelta.x, num11);
			LayoutRebuilder.ForceRebuildLayoutImmediate(worldScrollView.content);
		}
		int pageIndex = 1;
		for (int num12 = 0; num12 < _worldConfigs.Count; num12++)
		{
			if (_worldConfigs[num12].Id == currentWorldId)
			{
				pageIndex = num12 + 1;
				break;
			}
		}
		ScrollToPage(pageIndex, displayWorldCount);
	}

	private void SmoothScrollToWorld(int worldId)
	{
		if ((Object)(object)worldScrollView == (Object)null || (Object)(object)worldScrollView.content == (Object)null || (Object)(object)worldScrollView.viewport == (Object)null)
		{
			return;
		}
		int num = _worldConfigs.FindIndex((world config) => config.Id == worldId);
		int num2 = num + 1;
		if (num >= 0 && num2 >= 0 && num2 < _spawnedNodes.Count)
		{
			WorldNode worldNode = _spawnedNodes[num2];
			if (!((Object)(object)worldNode == (Object)null) && ((Component)worldNode).gameObject.activeInHierarchy)
			{
				ResetSnapState();
				worldScrollView.StopMovement();
				_snapTargetNormalizedPosition = GetScrollPositionForNode(worldNode);
				_currentSnapSmoothTime = guideAutoScrollSmoothTime;
				_hasSnapTarget = true;
				_snapPending = true;
			}
		}
	}

	private void EnsureDragRelay()
	{
		if (!((Object)(object)worldScrollView == (Object)null))
		{
			WorldSelectionScrollDragRelay worldSelectionScrollDragRelay = ((Component)worldScrollView).GetComponent<WorldSelectionScrollDragRelay>();
			if ((Object)(object)worldSelectionScrollDragRelay == (Object)null)
			{
				worldSelectionScrollDragRelay = ((Component)worldScrollView).gameObject.AddComponent<WorldSelectionScrollDragRelay>();
			}
			worldSelectionScrollDragRelay.Initialize(HandleBeginDrag, HandleEndDrag);
		}
	}

	private void HandleBeginDrag()
	{
		_isDragging = true;
		_snapPending = false;
		_hasSnapTarget = false;
		_snapVelocity = 0f;
		_currentSnapSmoothTime = 0.14f;
	}

	private void HandleEndDrag()
	{
		_isDragging = false;
		_snapPending = DisplayWorldCount > 0;
		_hasSnapTarget = false;
		_snapVelocity = 0f;
		_currentSnapSmoothTime = 0.14f;
	}

	private float FindNearestNodeScrollPosition()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)worldScrollView == (Object)null || (Object)(object)worldScrollView.viewport == (Object)null)
		{
			return 1f;
		}
		RectTransform viewport = worldScrollView.viewport;
		Rect rect = worldScrollView.viewport.rect;
		Vector3 val = ((Transform)viewport).TransformPoint(Vector2.op_Implicit(((Rect)(ref rect)).center));
		WorldNode worldNode = null;
		float num = float.MaxValue;
		for (int i = 0; i < DisplayWorldCount && i < _spawnedNodes.Count; i++)
		{
			WorldNode worldNode2 = _spawnedNodes[i];
			if ((Object)(object)worldNode2 == (Object)null || !((Component)worldNode2).gameObject.activeInHierarchy)
			{
				continue;
			}
			Transform transform = ((Component)worldNode2).transform;
			RectTransform val2 = (RectTransform)(object)((transform is RectTransform) ? transform : null);
			if (!((Object)(object)val2 == (Object)null))
			{
				rect = val2.rect;
				float num2 = Mathf.Abs(((Transform)val2).TransformPoint(Vector2.op_Implicit(((Rect)(ref rect)).center)).y - val.y);
				if (num2 < num)
				{
					num = num2;
					worldNode = worldNode2;
				}
			}
		}
		if (!((Object)(object)worldNode == (Object)null))
		{
			return GetScrollPositionForNode(worldNode);
		}
		return worldScrollView.verticalNormalizedPosition;
	}

	private float GetScrollPositionForNode(WorldNode node, float visualOffset = 0f)
	{
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)worldScrollView == (Object)null || (Object)(object)worldScrollView.content == (Object)null || (Object)(object)worldScrollView.viewport == (Object)null || (Object)(object)node == (Object)null)
		{
			return 1f;
		}
		RectTransform content = worldScrollView.content;
		RectTransform viewport = worldScrollView.viewport;
		Transform transform = ((Component)node).transform;
		RectTransform val = (RectTransform)(object)((transform is RectTransform) ? transform : null);
		if ((Object)(object)val == (Object)null)
		{
			return worldScrollView.verticalNormalizedPosition;
		}
		Rect rect = val.rect;
		Vector3 val2 = ((Transform)content).InverseTransformPoint(((Transform)val).TransformPoint(Vector2.op_Implicit(((Rect)(ref rect)).center)));
		rect = content.rect;
		float num = ((Rect)(ref rect)).yMax - val2.y;
		rect = viewport.rect;
		float num2 = num - ((Rect)(ref rect)).height * 0.5f + visualOffset;
		rect = content.rect;
		float height = ((Rect)(ref rect)).height;
		rect = viewport.rect;
		float num3 = Mathf.Max(0f, height - ((Rect)(ref rect)).height);
		if (num3 <= 0f)
		{
			return 1f;
		}
		return Mathf.Clamp01(1f - num2 / num3);
	}

	private void ResetSnapState()
	{
		_isDragging = false;
		_snapPending = false;
		_hasSnapTarget = false;
		_snapVelocity = 0f;
		_currentSnapSmoothTime = 0.14f;
	}

	private void ConfigureComingSoonNode(WorldNode binding, int index)
	{
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)binding.titleText != (Object)null)
		{
			((TMP_Text)binding.titleText).text = "敬请期待";
		}
		if ((Object)(object)binding.islandButton != (Object)null)
		{
			((UnityEventBase)binding.islandButton.onClick).RemoveAllListeners();
			((Selectable)binding.islandButton).interactable = false;
		}
		if ((Object)(object)binding.lockPanel != (Object)null)
		{
			binding.lockPanel.SetActive(true);
		}
		if ((Object)(object)binding.unlockPanel != (Object)null)
		{
			binding.unlockPanel.SetActive(false);
		}
		if ((Object)(object)binding.lockText != (Object)null)
		{
			((TMP_Text)binding.lockText).text = "敬请期待";
		}
		if ((Object)(object)binding.islandImage != (Object)null)
		{
			((Graphic)binding.islandImage).color = new Color(0f, 0f, 0f, 1f);
		}
		if ((Object)(object)binding.tail != (Object)null)
		{
			((Component)binding.tail).gameObject.SetActive(index > 0);
		}
		if ((Object)(object)binding.currentImage != (Object)null)
		{
			((Component)binding.currentImage).gameObject.SetActive(false);
		}
	}

	private void ConfigureWorldNode(WorldNode binding, world config, int currentWorldId, int index)
	{
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Expected O, but got Unknown
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		bool flag = GameDataManager.Instance.IsWorldUnlocked(config.Id);
		bool flag2 = currentWorldId == config.Id;
		if ((Object)(object)binding.titleText != (Object)null)
		{
			((TMP_Text)binding.titleText).text = (flag2 ? (config.WorldName ?? "") : config.WorldName);
		}
		if ((Object)(object)binding.islandButton != (Object)null)
		{
			((UnityEventBase)binding.islandButton.onClick).RemoveAllListeners();
			((Selectable)binding.islandButton).interactable = true;
			((UnityEvent)binding.islandButton.onClick).AddListener((UnityAction)delegate
			{
				OnSelectWorld(config.Id);
			});
		}
		if (flag)
		{
			if ((Object)(object)binding.lockPanel != (Object)null)
			{
				binding.lockPanel.SetActive(false);
			}
			if ((Object)(object)binding.unlockPanel != (Object)null)
			{
				binding.unlockPanel.SetActive(true);
			}
		}
		else
		{
			if ((Object)(object)binding.lockPanel != (Object)null)
			{
				binding.lockPanel.SetActive(true);
			}
			if ((Object)(object)binding.unlockPanel != (Object)null)
			{
				binding.unlockPanel.SetActive(false);
			}
			if ((Object)(object)binding.lockText != (Object)null)
			{
				if (config.PreWorldId != 0)
				{
					world worldConfig = ConfigManager.Instance.GetWorldConfig(config.PreWorldId);
					string arg = ((worldConfig != null) ? worldConfig.WorldName : "前置世界");
					((TMP_Text)binding.lockText).text = $"{arg}完成{config.RequiredPreWorldLevel}级后解锁";
				}
				else
				{
					((TMP_Text)binding.lockText).text = "敬请期待";
				}
			}
		}
		if ((Object)(object)binding.islandImage != (Object)null)
		{
			if (flag2)
			{
				((Graphic)binding.islandImage).color = Color.white;
			}
			else if (flag)
			{
				((Graphic)binding.islandImage).color = Color.white;
			}
			else
			{
				((Graphic)binding.islandImage).color = new Color(0f, 0f, 0f, 1f);
			}
		}
		if ((Object)(object)binding.tail != (Object)null)
		{
			((Component)binding.tail).gameObject.SetActive(index > 0);
		}
		if ((Object)(object)binding.currentImage != (Object)null)
		{
			((Component)binding.currentImage).gameObject.SetActive(flag2);
		}
	}

	private void OnSelectWorld(int worldId)
	{
		if ((Object)(object)GameDataManager.Instance == (Object)null)
		{
			return;
		}
		if (GameDataManager.Instance.GetCurrentWorldId() == worldId)
		{
			SetStatus("当前已处于该世界");
			return;
		}
		if (!GameDataManager.Instance.IsWorldUnlocked(worldId))
		{
			SetStatus("该世界尚未解锁");
			return;
		}
		world world = _worldConfigs.Find((world w) => w.Id == worldId);
		string text = ((world != null) ? world.WorldName : "该世界");
		UIManager.Instance.ShowCommonDialog("是否确定切换至" + text + "？", delegate
		{
			if (!GameDataManager.Instance.TrySwitchWorld(worldId, out var error))
			{
				SetStatus(error);
			}
			else
			{
				Close();
				if ((Object)(object)UpgradePanel.Instance != (Object)null)
				{
					UpgradePanel.Instance.OnButtonClicked(1);
				}
			}
		}, delegate
		{
		});
	}

	private void SetStatus(string text)
	{
		BroadcastManager.ShowPopUpTip(text);
	}

	private void ClearStatus()
	{
		if ((Object)(object)statusText != (Object)null)
		{
			((TMP_Text)statusText).text = string.Empty;
		}
	}

	private void ScrollToPage(int pageIndex, int totalPages)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)worldScrollView == (Object)null || (Object)(object)worldScrollView.content == (Object)null || (Object)(object)worldScrollView.viewport == (Object)null)
		{
			return;
		}
		Canvas.ForceUpdateCanvases();
		LayoutRebuilder.ForceRebuildLayoutImmediate(worldScrollView.content);
		Rect rect = worldScrollView.content.rect;
		float height = ((Rect)(ref rect)).height;
		rect = worldScrollView.viewport.rect;
		float height2 = ((Rect)(ref rect)).height;
		if (height <= height2)
		{
			worldScrollView.verticalNormalizedPosition = 1f;
		}
		else if (pageIndex >= 0 && pageIndex < _spawnedNodes.Count)
		{
			WorldNode worldNode = _spawnedNodes[pageIndex];
			if (!((Object)(object)worldNode == (Object)null))
			{
				worldScrollView.verticalNormalizedPosition = GetScrollPositionForNode(worldNode);
			}
		}
	}
}
