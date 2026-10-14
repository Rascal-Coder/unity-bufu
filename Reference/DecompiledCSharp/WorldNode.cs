using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorldNode : MonoBehaviour
{
	public TextMeshProUGUI titleText;

	public Button islandButton;

	public Image islandImage;

	public GameObject lockPanel;

	public TextMeshProUGUI lockText;

	[Header("Unlocks Info")]
	public GameObject unlockPanel;

	public Image roleIcon;

	public List<Image> itemSlots;

	public Image tail;

	public Image currentImage;
}
