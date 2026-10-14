using System;

[Serializable]
public class PlayerPrefsData
{
	public string version = "1.0.0";

	public string saveTime;

	public int guide_step_1;

	public int guide_step_2;

	public int guide_step_3;

	public int guide_step_4;

	public int guide_step_5;

	public int guide_step_6;

	public int dice_world_guide_step;

	public int[] shop = new int[0];

	public int guide_step_4_progress;

	public int firstReset;

	public int SideBAR_HAS_GET_M1B;

	public int Subscribe_HAS_GET_M1B;

	public int ADD_DESK_HAS_GET;

	public int gongfu_support_time;

	public int dashe_support_time;

	public int gongfu_popup_triggered;

	public int dashe_popup_triggered;

	public int gongfu_ad_state;

	public int dashe_ad_state;

	public string gongfu_ad_start_time = "";

	public string dashe_ad_start_time = "";
}
