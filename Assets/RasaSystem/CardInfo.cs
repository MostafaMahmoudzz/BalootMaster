using Pebble;
using System;
using UnityEngine;

[Serializable]
public class CardInfo
{
    public Card32Value Value;
    public Card32Family Family;

    public CardInfo()
    {
    }

    public CardInfo(Card32Value value, Card32Family family)
    {
        Value = value;
        Family = family;
    }

    // Helper method to get the card's sprite name
    public string GetSpriteName()
    {
        string familyName = GetFamilySpriteName();
        int valueNumber = GetValueSpriteNumber();
        return $"{familyName}{valueNumber:D2}";
    }

    private string GetFamilySpriteName()
    {
        switch (Family)
        {
            case Card32Family.Clubs: return "Club";
            case Card32Family.Heart: return "Heart";
            case Card32Family.Diamond: return "Diamond";
            case Card32Family.Spade: return "Spade";
            default: return "Club";
        }
    }

    private int GetValueSpriteNumber()
    {
        switch (Value)
        {
            case Card32Value.Ace: return 1;
            case Card32Value.Seven: return 7;
            case Card32Value.Eight: return 8;
            case Card32Value.Nine: return 9;
            case Card32Value.Ten: return 10;
            case Card32Value.Jack: return 11;
            case Card32Value.Queen: return 12;
            case Card32Value.King: return 13;
            default: return 1;
        }
    }

    public override string ToString()
    {
        return $"{Value} of {Family}";
    }
}
