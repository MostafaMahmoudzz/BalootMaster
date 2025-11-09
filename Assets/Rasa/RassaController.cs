using Pebble;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RassaController : MonoBehaviour
{

    public Button[] RassaInitialButtons; 
    public GameObject[] RassaFinalImages;
    

    public CardsInfoScriptable RassaCardsFinalOrderScriptable;

    private int cardsCounter = 0;


    private List<CardInfo> orderedRassaCards = new List<CardInfo>();

    private void Start()
    {

        for (int i = 0; i < RassaInitialButtons.Length; i++)
        {
            Button button = RassaInitialButtons[i]; 
            button.onClick.AddListener(() => AddCardToRassa(button));
        }
    }
    private void AddCardToRassa(Button clickedButton)
    {
        if (cardsCounter > RassaFinalImages.Length - 1) return;

        clickedButton.image.enabled = false;
        RassaFinalImages[cardsCounter].GetComponent<Image>().sprite = clickedButton.image.sprite;
        RassaFinalImages[cardsCounter].GetComponent<Image>().enabled = true;
        cardsCounter++;


        orderedRassaCards.Add(clickedButton.GetComponent<CardInfo>());
    }
    



    public void ResetRassa_UIEventHandler()
    {
        cardsCounter = 0;
        orderedRassaCards.Clear();

        for (int i = 0;i < RassaFinalImages.Length;i++)
        {
            RassaFinalImages[i].GetComponent<Image>().enabled=false;
            RassaInitialButtons[i].GetComponent<Image>().enabled = true;
        }
    }

    public void DoneRassa_UIEventHandler()
    {

        RassaCardsFinalOrderScriptable.cardsInfo.Clear();

        foreach (var card in orderedRassaCards)
        {
            CardInfo cardInfo = new CardInfo();
            cardInfo.Value = card.Value;
            cardInfo.Family = card.Family;
            RassaCardsFinalOrderScriptable.cardsInfo.Add(cardInfo); //Todo : check the serialiazable value , it is holding the value and family but the base is none ,, may cause problems later
        }

       
    }
}
