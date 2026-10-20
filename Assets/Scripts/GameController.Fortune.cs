using Bufu.Rebuild.UI;
using UnityEngine;

namespace Bufu.Rebuild {
    // The original guide continues with upgrade prompts. It does not grant a starter gift.
    public sealed partial class GameController {
        static readonly Color FortuneInk=new Color(.36f,.2f,.08f,1f);
        static Sprite fortuneButton;
        static Sprite FortuneButton() {
            if(fortuneButton)return fortuneButton;
            var source=UiAssets.Sprite("ty_btn_yellow01");if(!source)return null;
            fortuneButton=Sprite.Create(source.texture,source.textureRect,new Vector2(.5f,.5f),source.pixelsPerUnit,0,SpriteMeshType.FullRect,new Vector4(56,56,56,56));
            return fortuneButton;
        }
    }
}
