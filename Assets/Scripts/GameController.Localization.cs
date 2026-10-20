using Bufu.Rebuild.UI;
using UnityEngine;

namespace Bufu.Rebuild {
    public sealed partial class GameController {
        void RefreshLanguage(){RefreshAll();RefreshSupportDialog();if(canvasRect)UiAssets.ApplyLanguage(canvasRect.gameObject);}
        void OnDestroy(){purchases?.Dispose();L10n.Changed-=RefreshLanguage;if(Active==this)Active=null;}
    }
}
