using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class FacilityData
{
	[SerializeField]
	public List<DeskInfo> deskOwned = new List<DeskInfo>();

	[SerializeField]
	public List<HelperInfo> helperOwned = new List<HelperInfo>();
}
