using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckButtonController : MonoBehaviour
{
    public int deckNumber;

    private void OnMouseDown()
    {
        CollectionController.collectionController.SetDeck(deckNumber);
    }
}
