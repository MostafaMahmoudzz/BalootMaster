using Sirenix.OdinInspector;
using UnityEngine;
using Pebble;

//----------------------------------------------
// CardStaticData
//----------------------------------------------
// Purpose:
//   Central registry for card visual assets and prefabs. Provides
//   sprite lookup for a given `Card32Value` and `Card32Family`, as
//   well as the card prefab to instantiate.
//
// How it connects to other scripts:
//   - Used by `BeloteCard.Spawn()` and `CardComponent.Init()` to pick
//     the correct visuals.
//----------------------------------------------
public class CardStaticData : Singleton<CardStaticData>
{
    public CardSpriteRef[] CardSprites;

    [AssetsOnly]
    public Sprite BackSprite;

    [AssetsOnly]
    public GameObject Prefab;


    public Sprite GetSprite(Card32Value Value, Card32Family Family)
    {
        foreach(CardSpriteRef cardRef in CardSprites)
        {
            if(cardRef.Value == Value && cardRef.Family == Family)
            {
                return cardRef.Prefab;
            }
        }
        return null;
    }
}