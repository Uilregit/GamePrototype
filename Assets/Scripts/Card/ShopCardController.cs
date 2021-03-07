using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    private float clickThreshold = 0.2f;

    public Canvas selectedCardCanvas;
    private Canvas originalCanvas;
    private Vector3 localScale;
    private Vector3 originalLocation;
    //private int originalSorterOrder;

    private void Awake()
    {
        cardDisplay = transform.GetChild(0).GetComponent<CardDisplay>();
        col = GetComponent<Collider2D>();
        localScale = transform.localScale;
        originalLocation = transform.position;
        originalCanvas = transform.parent.GetComponent<Canvas>();
        //originalSorterOrder = GetComponent<CardDisplay>().cardName.GetComponent<MeshRenderer>().sortingOrder;
    }

    public void SetCard(CardController newCard)
    {
        card = newCard;
        transform.GetChild(0).GetComponent<CardDisplay>().SetCard(newCard, true);
    }

    public void SetPrice(int value)
    {
        price = value;
        priceTag.text = value.ToString() + "G";
    }

    public void OnMouseDown()
    {
        clickedTime = Time.time;
        StartCoroutine(EnlargeCard());
    }

    public void OnMouseUp()
    {
        transform.GetChild(0).GetComponent<CardDisplay>().SetCard(card, true);
        StopAllCoroutines();
        transform.SetParent(originalCanvas.transform);
        transform.localScale = localScale;
        transform.position = originalLocation;
        cardDisplay.SetToolTip(false, -1, 1, false);
        //GetComponent<CardDisplay>().cardName.GetComponent<MeshRenderer>().sortingOrder = originalSorterOrder;
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

    private IEnumerator EnlargeCard()
    {
        yield return new WaitForSeconds(0.3f);
        transform.SetParent(selectedCardCanvas.transform);
        //GetComponent<CardDisplay>().cardName.GetComponent<MeshRenderer>().sortingOrder = selectedCardCanvas.sortingOrder + 1;
        transform.position = new Vector3(Mathf.Clamp(originalLocation.x, HandController.handController.cardHighlightXBoarder * -1, HandController.handController.cardHighlightXBoarder), originalLocation.y + HandController.handController.cardHighlightHeight, 0);
        transform.localScale = new Vector3(HandController.handController.cardHighlightSize, HandController.handController.cardHighlightSize, 1);
        transform.GetChild(0).GetComponent<CardDisplay>().SetCard(card, false);
        cardDisplay.SetToolTip(true, -1, 1, false);
    }
}
