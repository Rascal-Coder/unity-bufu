using Module.Vat;
using UnityEngine;
using UnityEngine.UI;

namespace Module.UI;

public sealed class VatEffectVisualCache : MonoBehaviour
{
	private Renderer[] renderers;

	private Graphic[] graphics;

	private SpineVat[] spineVats;

	private SpriteVatAnimationSequence[] spriteVats;

	private bool isInitialized;

	public static void SetVisible(GameObject effectObject, bool visible)
	{
		if (!((Object)(object)effectObject == (Object)null))
		{
			VatEffectVisualCache vatEffectVisualCache = effectObject.GetComponent<VatEffectVisualCache>();
			if ((Object)(object)vatEffectVisualCache == (Object)null)
			{
				vatEffectVisualCache = effectObject.AddComponent<VatEffectVisualCache>();
			}
			vatEffectVisualCache.SetVisible(visible);
		}
	}

	public void Refresh()
	{
		renderers = ((Component)this).GetComponentsInChildren<Renderer>(true);
		graphics = ((Component)this).GetComponentsInChildren<Graphic>(true);
		spineVats = ((Component)this).GetComponentsInChildren<SpineVat>(true);
		spriteVats = ((Component)this).GetComponentsInChildren<SpriteVatAnimationSequence>(true);
		isInitialized = true;
	}

	public void SetVisible(bool visible)
	{
		if (!isInitialized)
		{
			Refresh();
		}
		for (int i = 0; i < renderers.Length; i++)
		{
			if ((Object)(object)renderers[i] != (Object)null)
			{
				renderers[i].enabled = visible;
			}
		}
		for (int j = 0; j < graphics.Length; j++)
		{
			if ((Object)(object)graphics[j] != (Object)null)
			{
				((Behaviour)graphics[j]).enabled = visible;
			}
		}
		for (int k = 0; k < spineVats.Length; k++)
		{
			if ((Object)(object)spineVats[k] != (Object)null)
			{
				spineVats[k].SetRenderingVisible(visible);
			}
		}
		for (int l = 0; l < spriteVats.Length; l++)
		{
			if ((Object)(object)spriteVats[l] != (Object)null)
			{
				spriteVats[l].SetRenderingVisible(visible);
			}
		}
	}
}
