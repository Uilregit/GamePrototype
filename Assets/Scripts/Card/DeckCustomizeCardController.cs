using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DeckCustomizeCardController : MonoBehaviour
{
    public float clickThreshold;

    public SelectedCardController selectedCard;
    private CardController card;
    private Equipment equipment;
    private float clickedTime;
    public Text count;
    public Image greyOut;
    public Outline highlight;
    public Collider2D col;

    public Canvas selectedCardCanvas;
    private Canvas originalCanvas;
    private Vector3 localScale;
    private Vector3 originalLocation;
    private CardDisplay cardDisplay;
    //private int originalSorterOrder;

    private bool isShowingCard = true;
    private Card.CasterColor color = Card.CasterColor.Passive;

    private bool cardEnlarged = false;
    private bool hasNeverBeenShrunk = true;
    private int custCardSlots = 0;

    private void Awake()
    {
        DontDestroyOnLoad(selectedCardCanvas);
        //col = GetComponent<Collider2D>();
        localScale = transform.localScale;
        originalLocation = transform.position;
        originalCanvas = transform.parent.GetComponent<Canvas>();
        cardDisplay = transform.GetChild(0).GetComponent<CardDisplay>();
        //originalSorterOrder = transform.GetChild(0).GetComponent<CardDisplay>().Show();.cardName.GetComponent<MeshRenderer>().sortingOrder;
    }

    public void OnMouseDown()
    {
        hasNeverBeenShrunk = true;                              //Used so where the card can potentially go is shown
        transform.SetParent(selectedCardCanvas.transform);
        EnlargeCard();
        Vector3 newLocation = new Vector2(0, HandController.handController.cardHighlightHeight) + (Vector2)CameraController.camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
        transform.position = newLocation;
        highlight.enabled = false;
        if (isShowingCard)
            CollectionController.collectionController.RemoveCardFromNew(card);

        //Find the card slots that the current card can go to
        if (equipment != null)  //If this card is an equipment card, it can go anywhere
            custCardSlots = 8;
        else if (card.GetCard().rarity == Card.Rarity.Starter || card.GetCard().rarity == Card.Rarity.StarterAttack || SceneManager.GetActiveScene().name != "StoryModeScene")
            custCardSlots = 8;
        else
        {
            Vector2 ranges = CollectionController.collectionController.GetWeaponAndAccessorySlots();
            custCardSlots = (int)(ranges.x + ranges.y);
        }

        //Highlight the card slots that the current card can go to
        for (int i = 0; i < custCardSlots; i++)
            CollectionController.collectionController.SetSelectCardWhiteOut(true, i, Color.white);
    }

    public void OnMouseDrag()
    {
        int index = 0;
        //Find the index location that the card is being dragged over
        if (CameraController.camera.ScreenToWorldPoint(Input.mousePosition).y < -1.3)
        {
            if (equipment != null)                                                              //For equipments, set all spaces to white
                for (int i = 0; i < 8; i++)
                    CollectionController.collectionController.SetSelectCardWhiteOut(true, i, Color.white);
            else
            {
                float positionX = Camera.main.ScreenToWorldPoint(Input.mousePosition).x - 8.0f;
                if (positionX == Mathf.Clamp(positionX, -2.4f, 2.4f))                           //If card is dragged inside selected card range, show the card slots it can go into
                {
                    index = (int)((positionX + 2.4f) / 0.6f);
                    CollectionController.collectionController.SetSelectCardWhiteOut(false, 0, Color.white);
                    if (index < custCardSlots)                                                  //Only white out select cards if the card is dragged over slots it can go into
                        CollectionController.collectionController.SetSelectCardWhiteOut(true, index, Color.white);
                    else
                        CollectionController.collectionController.SetSelectCardWhiteOut(true, index, Color.red);
                }
                else
                    CollectionController.collectionController.SetSelectCardWhiteOut(false, 0, Color.white);  //If the card is outside range, hide all whiteouts
            }
        }

        //Enlarge and shink cards
        if (CameraController.camera.ScreenToWorldPoint(Input.mousePosition).y > -0.3)           //If the card is above the select cards threshold
        {
            if (!cardEnlarged)
                EnlargeCard();
            if (!hasNeverBeenShrunk)
            {
                CollectionController.collectionController.SetSelectCardWhiteOut(false, 0, Color.white);
                for (int i = 0; i < custCardSlots; i++)
                    CollectionController.collectionController.SetSelectCardWhiteOut(true, i, Color.white);
            }
        }
        else if (CameraController.camera.ScreenToWorldPoint(Input.mousePosition).y < -1.3)      //If the card is below the select cards threshold
        {
            hasNeverBeenShrunk = false;
            if (cardEnlarged)
            {
                if (index < custCardSlots)
                    ShrinkCard();
            }
            else
            {
                if (index >= custCardSlots)
                    EnlargeCard();
            }
        }

        //Handle the transform movement part
        Vector2 offset = new Vector2(0, HandController.handController.cardHighlightHeight);
        if (!cardEnlarged)
            offset = new Vector2(0, 1.5f);
        Vector3 newLocation = offset + (Vector2)CameraController.camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
        transform.position = newLocation;
    }

    public void OnMouseUp()
    {
        transform.SetParent(originalCanvas.transform);

        //Weapons
        if (!isShowingCard && !cardEnlarged)
        {
            ShrinkCard();
            selectedCard.Hide();
            SelectCard(0);
            return;
        }

        ShrinkCard();
        Show();
        selectedCard.Hide();

        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 9999, LayerMask.GetMask("Raycast"));

        if (hit.transform != null && hit.transform.GetComponent<SelectedCardController>() != null)
        {
            float positionX = Camera.main.ScreenToWorldPoint(Input.mousePosition).x - 8.0f;
            int index = (int)((positionX + 2.4f) / 0.6f);

            if (CollectionController.collectionController.GetIfViableSelectSlot(card.GetCard(), index))  //Only allow cards to be selected in their respective slots
                SelectCard(index);
            else
                CollectionController.collectionController.ShowErrorMessage();
        }

        CollectionController.collectionController.SetSelectAreaWhiteOut(false);
        CollectionController.collectionController.SetSelectCardWhiteOut(false, 0, Color.white);
        selectedCardCanvas.transform.SetAsLastSibling();
    }

    public void SelectCard(int index)
    {
        if (isShowingCard)
            CollectionController.collectionController.AddCard(card, index);
        else
            CollectionController.collectionController.AddEquipment(equipment, color);
    }

    public void SetCard(CardController newCard)
    {
        card = newCard;
        cardDisplay.SetCard(newCard, false);
        selectedCard.SetCard(newCard);
        selectedCard.Hide();
        equipment = null;
    }

    public void SetEquipment(Equipment equip, Card.CasterColor value)
    {
        equipment = equip;
        color = value;
        selectedCard.SetEquipment(equip);
        selectedCard.Hide();
        card = null;
    }

    public void SetCount(int newCount)
    {
        count.text = "x" + newCount.ToString();
    }

    public void Hide()
    {
        count.enabled = false;
        if ((object)cardDisplay == null)
            cardDisplay = transform.GetChild(0).GetComponent<CardDisplay>();
        cardDisplay.Hide();
        selectedCard.Hide();
        col.enabled = false;
    }

    public void Show()
    {
        count.enabled = true;
        if ((object)cardDisplay == null)
            cardDisplay = transform.GetChild(0).GetComponent<CardDisplay>();
        cardDisplay.Show();
        col.enabled = true;
        cardDisplay.lineRenderer.enabled = false;
    }

    private void EnlargeCard()
    {
        transform.localPosition = new Vector3(0, 0, 0);
        transform.localScale = new Vector3(HandController.handController.cardHighlightSize, HandController.handController.cardHighlightSize, 1);
        cardDisplay.SetToolTip(true, -1, 1, false);

        cardDisplay.Show();
        cardDisplay.lineRenderer.enabled = false;
        selectedCard.Hide();
        count.enabled = true;

        cardEnlarged = true;
    }

    private void ShrinkCard()
    {
        transform.localScale = localScale;
        transform.position = originalLocation;
        cardDisplay.SetToolTip(false, -1, 1, false);

        cardDisplay.Hide();
        selectedCard.Show();
        count.enabled = false;

        cardEnlarged = false;
    }

    public void SetIsShowingCard(bool value)
    {
        isShowingCard = value;
    }
}
