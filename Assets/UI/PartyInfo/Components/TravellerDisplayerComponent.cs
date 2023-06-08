using CustomComponents;
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.Animation;
using UnityEngine.UIElements;

public class TravellerDisplayerComponent : VisualElement
{
    [UnityEngine.Scripting.Preserve]
    public new class UxmlFactory : UxmlFactory<TravellerDisplayerComponent> { }

    private SpriteLibraryAsset spriteLibraryAsset;
    private bool iterateForward = true;
    private int currSprite = -1;

    public TravellerDisplayerComponent() {
        spriteLibraryAsset = Resources.Load<TravellerIdentitySO>(
            "ScriptableObjects/Identities/Travellers/Cat").spriteLibraryAsset;
        UpdateSprite();
    }

    public TravellerDisplayerComponent(SpriteLibraryAsset spriteLibraryAsset) : this() {
        SetSprite(spriteLibraryAsset);
    }

    public void SetSprite(SpriteLibraryAsset spriteLibraryAsset) {
        this.spriteLibraryAsset = spriteLibraryAsset;
    }

    private async UniTask UpdateSprite() {
        while (true) {
            await UniTask.Delay(150);
            style.backgroundImage = new StyleBackground(GetNextSprite());
        }
    }

    private Sprite GetNextSprite() {
        if (iterateForward && currSprite == 3 || !iterateForward && currSprite == 0) {
            iterateForward = !iterateForward;
        }
        currSprite += iterateForward ? 1 : -1;
        return spriteLibraryAsset.GetSprite("Idle", "idle" + currSprite);
    }
}
