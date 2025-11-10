using Pebble;
using UnityEngine;

/// <summary>
/// Manages applying the Rassa card order to the game deck
/// </summary>
public class RassaDeckManager : MonoBehaviour
{
    [Header("Rassa Data")]
    public CardsInfoScriptable savedRassaOrder;

    /// <summary>
    /// Check if a valid Rassa order has been saved
    /// </summary>
    public bool HasSavedRassaOrder()
    {
        if (savedRassaOrder == null)
        {
            Debug.LogWarning("[RassaDeckManager] No ScriptableObject assigned!");
            return false;
        }

        if (savedRassaOrder.cardsInfo == null || savedRassaOrder.cardsInfo.Count != 32)
        {
            Debug.LogWarning($"[RassaDeckManager] Invalid saved order. Expected 32 cards, found {savedRassaOrder.cardsInfo?.Count ?? 0}");
            return false;
        }

        Debug.Log("[RassaDeckManager] Valid Rassa order found with 32 cards");
        return true;
    }

    /// <summary>
    /// Arrange the BeloteDeck according to the saved Rassa order
    /// Returns true if successful, false if failed
    /// </summary>
    public bool ArrangeDeckWithRassaOrder(BeloteDeck deck)
    {
        if (!HasSavedRassaOrder())
        {
            Debug.LogError("[RassaDeckManager] Cannot arrange deck - no valid Rassa order!");
            return false;
        }

        if (deck == null || deck.Size != 32)
        {
            Debug.LogError($"[RassaDeckManager] Invalid deck! Expected 32 cards, found {deck?.Size ?? 0}");
            return false;
        }

        Debug.Log("[RassaDeckManager] === ARRANGING DECK WITH RASSA ORDER ===");

        // Create a temporary list to hold cards in the new order
        BeloteCard[] arrangedCards = new BeloteCard[32];

        // For each position in the Rassa order, find the matching card in the deck
        for (int i = 0; i < savedRassaOrder.cardsInfo.Count; i++)
        {
            CardInfo targetCard = savedRassaOrder.cardsInfo[i];
            
            // Find this card in the current deck
            BeloteCard foundCard = FindCardInDeck(deck, targetCard.Value, targetCard.Family);
            
            if (foundCard == null)
            {
                Debug.LogError($"[RassaDeckManager] Could not find card: {targetCard.Value} of {targetCard.Family}");
                return false;
            }

            arrangedCards[i] = foundCard;
            Debug.Log($"[RassaDeckManager] Position {i}: {foundCard.Value} of {foundCard.Family}");
        }

        // Clear the deck and add cards in the new order
        deck.Clear();
        
        foreach (BeloteCard card in arrangedCards)
        {
            deck.AddCard(card);
        }

        Debug.Log($"[RassaDeckManager] Deck arranged successfully! Size: {deck.Size}");
        Debug.Log($"[RassaDeckManager] First card will be: {deck.Cards[0].Value} of {deck.Cards[0].Family}");
        Debug.Log($"[RassaDeckManager] Last card will be: {deck.Cards[31].Value} of {deck.Cards[31].Family}");

        return true;
    }

    /// <summary>
    /// Find a specific card in the deck
    /// </summary>
    private BeloteCard FindCardInDeck(BeloteDeck deck, Card32Value value, Card32Family family)
    {
        foreach (BeloteCard card in deck.Cards)
        {
            if (card.Value == value && card.Family == family)
            {
                return card;
            }
        }
        return null;
    }

    /// <summary>
    /// Load Rassa order from PlayerPrefs as backup
    /// </summary>
    public bool LoadRassaOrderFromPlayerPrefs()
    {
        if (!PlayerPrefs.HasKey("RassaCardOrder"))
        {
            Debug.Log("[RassaDeckManager] No Rassa order found in PlayerPrefs");
            return false;
        }

        string jsonData = PlayerPrefs.GetString("RassaCardOrder");
        CardListWrapper wrapper = JsonUtility.FromJson<CardListWrapper>(jsonData);

        if (wrapper == null || wrapper.cards == null || wrapper.cards.Count != 32)
        {
            Debug.LogWarning("[RassaDeckManager] Invalid PlayerPrefs data");
            return false;
        }

        // If we have a ScriptableObject, update it
        if (savedRassaOrder != null)
        {
            savedRassaOrder.cardsInfo.Clear();
            savedRassaOrder.cardsInfo.AddRange(wrapper.cards);
            Debug.Log("[RassaDeckManager] Loaded Rassa order from PlayerPrefs to ScriptableObject");
            
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(savedRassaOrder);
            #endif
        }

        return true;
    }

    /// <summary>
    /// Log the current Rassa order for debugging
    /// </summary>
    public void DebugLogRassaOrder()
    {
        if (!HasSavedRassaOrder()) return;

        Debug.Log("[RassaDeckManager] === SAVED RASSA ORDER ===");
        for (int i = 0; i < savedRassaOrder.cardsInfo.Count; i++)
        {
            CardInfo card = savedRassaOrder.cardsInfo[i];
            Debug.Log($"[RassaDeckManager] {i + 1}. {card.Value} of {card.Family}");
        }
    }
}


