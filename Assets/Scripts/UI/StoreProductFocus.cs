using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Bufu.Rebuild.UI {
    // Keyboard/controller selection keeps a long product list inside its viewport.
    public sealed class StoreProductFocus:MonoBehaviour,ISelectHandler {
        public ScrollRect Scroll;public Action Selected;
        public void OnSelect(BaseEventData data) {
            Selected?.Invoke();if(!Scroll)return;Canvas.ForceUpdateCanvases();
            var bounds=RectTransformUtility.CalculateRelativeRectTransformBounds(Scroll.viewport,(RectTransform)transform);
            var viewport=Scroll.viewport.rect;float delta=bounds.max.y>viewport.yMax?viewport.yMax-bounds.max.y:
                bounds.min.y<viewport.yMin?viewport.yMin-bounds.min.y:0;
            var position=Scroll.content.anchoredPosition;position.y+=delta;Scroll.StopMovement();Scroll.content.anchoredPosition=position;
        }
    }
}
