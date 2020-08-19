using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopDoneButtonController : MonoBehaviour
{
    private void OnMouseDown()
    {
        ShopController.shop.RecordShopInformation();
        GameController.gameController.LoadScene("OverworldScene", ShopController.shop.GetBoughtCard(), ShopController.shop.GetLatestDeckID()); //Don't go to deck editing if a card was not bought
    }
}
