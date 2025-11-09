using Pebble;
using UnityEngine;

/// <summary>
/// MonoBehaviour component to attach CardInfo to GameObjects (like buttons)
/// </summary>
public class CardInfoComponent : MonoBehaviour
{
    public CardInfo cardInfo;

    public void SetCardInfo(Card32Value value, Card32Family family)
    {
        cardInfo = new CardInfo(value, family);
    }
}

