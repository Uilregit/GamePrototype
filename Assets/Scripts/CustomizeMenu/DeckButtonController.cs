using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckButtonController : MonoBehaviour
{
    public int deckNumber;

    private void Awake()
    {
        GetComponent<Image>().color = PartyController.party.GetPlayerColor(PartyController.party.partyColors[deckNumber]);
    }

    private void OnMouseDown()
    {
        CollectionController.collectionController.SetDeck(deckNumber);
    }
}
