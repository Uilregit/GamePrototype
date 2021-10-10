using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopCardController : MonoBehaviour
{
    private CardController card;
    private Equipment equipment;
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

    public void SetEquipment(Equipment e)
    {
        equipment = e;
        transform.GetChild(0).GetComponent<CardDisplay>().SetEquipment(e, Card.CasterColor.Passive);
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
        if (equipment == null)
            transform.GetChild(0).GetComponent<CardDisplay>().SetCard(card, true);
        else
            transform.GetChild(0).GetComponent<CardDisplay>().SetEquipment(equipment, Card.CasterColor.Passive);
        StopAllCoroutines();
        transform.SetParent(originalCanvas.transform);
        transform.localScale = localScale;
        transform.position = originalLocation;
        cardDisplay.SetToolTip(false, -1, 1, false);
        //GetComponent<CardDisplay>().cardName.GetComponent<MeshRenderer>().sortingOrder = originalSorterOrder;
        if (Time.time - clickedTime <= clickThreshold)
            SelectCard();
        else
            cardDisplay.cardSounds.PlayUncastSound();
    }

    public void SelectCard()
    {
        cardDisplay.cardSounds.PlayCastSound();
        picked = true;
        ResourceController.resource.ChangeGold(-price);
        ScoreController.score.UpdateGoldUsed(price);
        if (equipment != null)
        {
            CollectionController.collectionController.AddRewardsEquipment(equipment, false);
            ShopController.shop.ReportBoughtEquipment(equipment);
        }
        else
        {
            CollectionController.collectionController.AddRewardsCard(card, false);
            ShopController.shop.ReportBoughtCard(card);
        }

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
        cardDisplay.cardSounds.PlaySelectSound();
        transform.SetParent(selectedCardCanvas.transform);
        //GetComponent<CardDisplay>().cardName.GetComponent<MeshRenderer>().sortingOrder = selectedCardCanvas.sortingOrder + 1;
        transform.position = new Vector3(Mathf.Clamp(originalLocation.x, HandController.handController.cardHighlightXBoarder * -1, HandController.handController.cardHighlightXBoarder), originalLocation.y + HandController.handController.GetCardHighlightHeight(), 0);
        transform.localScale = new Vector3(HandController.handController.GetCardHighlightSize(), HandController.handController.GetCardHighlightSize(), 1);
        if (equipment == null)
            transform.GetChild(0).GetComponent<CardDisplay>().SetCard(card, false);
        else
            transform.GetChild(0).GetComponent<CardDisplay>().SetEquipment(equipment, Card.CasterColor.Passive);
        cardDisplay.SetToolTip(true, -1, 1, false);
    }
}
