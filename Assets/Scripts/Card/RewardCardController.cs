using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardCardController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseDown()
    {
        DeckController.deckController.AddCard(GetComponent<CardDisplay>().GetCard());
        GameController.gameController.LoadMapScene();
    }
}
