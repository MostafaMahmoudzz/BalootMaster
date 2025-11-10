using Pebble;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages loading and retrieving card sprites for the Rassa system
/// </summary>
public class RassaSpriteManager : MonoBehaviour
{
    [Header("Sprite References")]
    public Sprite[] cardSprites;

    [Header("Auto-Load Settings")]
    public string spriteFolderPath = "Cards"; // Path in Resources folder

    private Dictionary<string, Sprite> spriteCache;

    private void Awake()
    {
        BuildSpriteCache();
    }

    /// <summary>
    /// Builds a dictionary cache of all card sprites for quick lookup
    /// </summary>
    private void BuildSpriteCache()
    {
        spriteCache = new Dictionary<string, Sprite>();

        if (cardSprites != null && cardSprites.Length > 0)
        {
            foreach (var sprite in cardSprites)
            {
                if (sprite != null)
                {
                    spriteCache[sprite.name] = sprite;
                }
            }
            Debug.Log($"Loaded {spriteCache.Count} card sprites from array");
        }
    }

    /// <summary>
    /// Attempts to load card sprites from Resources folder
    /// </summary>
    public void LoadSpritesFromResources()
    {
        Sprite[] loadedSprites = Resources.LoadAll<Sprite>(spriteFolderPath);
        if (loadedSprites != null && loadedSprites.Length > 0)
        {
            cardSprites = loadedSprites;
            BuildSpriteCache();
            Debug.Log($"Loaded {loadedSprites.Length} sprites from Resources/{spriteFolderPath}");
        }
        else
        {
            Debug.LogWarning($"No sprites found in Resources/{spriteFolderPath}");
        }
    }

    /// <summary>
    /// Gets a sprite for a specific card
    /// </summary>
    public Sprite GetCardSprite(Card32Value value, Card32Family family)
    {
        CardInfo tempCard = new CardInfo(value, family);
        string spriteName = tempCard.GetSpriteName();
        
        if (spriteCache != null && spriteCache.ContainsKey(spriteName))
        {
            return spriteCache[spriteName];
        }

        Debug.LogWarning($"Sprite not found for: {spriteName}");
        return null;
    }

    /// <summary>
    /// Gets a sprite by card info
    /// </summary>
    public Sprite GetCardSprite(CardInfo cardInfo)
    {
        return GetCardSprite(cardInfo.Value, cardInfo.Family);
    }

    /// <summary>
    /// Gets all 32 card sprites in order
    /// </summary>
    public List<Sprite> GetAllCardSprites()
    {
        List<Sprite> sprites = new List<Sprite>();

        Card32Family[] families = { Card32Family.Clubs, Card32Family.Heart, Card32Family.Diamond, Card32Family.Spade };
        Card32Value[] values = { Card32Value.Seven, Card32Value.Eight, Card32Value.Nine, Card32Value.Jack,
                                 Card32Value.Queen, Card32Value.King, Card32Value.Ten, Card32Value.Ace };

        foreach (Card32Family family in families)
        {
            foreach (Card32Value value in values)
            {
                Sprite sprite = GetCardSprite(value, family);
                if (sprite != null)
                {
                    sprites.Add(sprite);
                }
            }
        }

        return sprites;
    }
}


