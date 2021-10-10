using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckButtonController : MonoBehaviour
{
    public int deckNumber;

    private void OnMouseDown()
    {
        MusicController.music.PlaySFX(MusicController.music.paperMoveSFX[Random.Range(0, MusicController.music.paperMoveSFX.Count)]);
        CollectionController.collectionController.SetDeck(deckNumber);
    }
}
