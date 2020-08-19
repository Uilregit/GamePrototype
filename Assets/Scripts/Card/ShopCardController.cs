using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopCardController : MonoBehaviour
{
    private CardController card;
    private int price;
    private float clickedTime;
    private Collider2D col;
    private CardDisplay cardDisplay;
    private bool picked = false;

    public Text priceTag;
    public float clickThreshold;

    private void Awake()
    {
        cardDisplay = GetComponent<CardDisplay>();
        col = GetComponent<Collider2D>();
    }

    public void SetCard(CardController newCard)
    {
        card = newCard;
        GetComponent<CardDisplay>().SetCard(newCard, false);
    }

    public void SetPrice(int value)
    {
        price = value;
        priceTag.text = value.ToString() + "G";
    }

    public void OnMouseDown()
    {
        clickedTime = Time.time;
    }

    public void OnMouseUp()
    {
        if (Time.time - clickedTime <= clickThreshold)
            SelectCard();
    }

    public void SelectCard()
    {
        picked = true;
        CollectionController.collectionController.AddRewardsCard(card, false);
        ResourceController.resource.ChangeGold(-price);
        ShopController.shop.ReportBoughtCard(card);
        ScoreController.score.UpdateGoldUsed(price);

        col.enabled = false;
        cardDisplay.Hide();
        priceTag.enabled = false;
    }

    public void ResetBuyable()
    {
        if (picked)
            return;

        if (price > ResourceController.resource.GetGold())
            Hide();
        else
            Show();
    }

    public void Hide()
    {
        cardDisplay.SetHighLight(false);
        col.enabled = false;
    }

    public void Show()
    {
        cardDisplay.SetHighLight(true);
        col.enabled = true;
    }
}
