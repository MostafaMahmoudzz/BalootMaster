using Pebble;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardsInfo", menuName = "ScriptableObjects/CardsInfo", order = 2)]
public class CardsInfoScriptable : ScriptableObject
{
    public List<CardInfo> cardsInfo;
}

