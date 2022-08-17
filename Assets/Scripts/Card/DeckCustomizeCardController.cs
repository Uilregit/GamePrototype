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
    public Image countBackdrop;
    public Text count;
    public Image[] countIcons;
    public Color selectedColor;
    public Color unselectedColor;
    public Image greyOut;
    public Outline highlight;
    public Collider2D col;

    public Canvas selectedCardCanvas;
    private Canvas originalCanvas;
    private Vector3 localScale;
    private Vector3 originalLocation;
    public CardDisplay cardDisplay;
    //private int originalSorterOrder;

    private bool isShowingCard = true;
    private Card.CasterColor color = Card.CasterColor.Passive;

    private bool cardEnlarged = false;
    private bool hasNeverBeenShrunk = true;
    private int custCardSlots = 0;

    private float selectedCardsLeftBorder;
    private float selectedCardsWidth;

    private void Awake()
    {
        DontDestroyOnLoad(selectedCardCanvas);
        //col = GetComponent<Collider2D>();
        localScale = transform.localScale;
        originalLocation = transform.position;
        originalCanvas = transform.parent.parent.GetComponent<Canvas>();
        //originalSorterOrder = transform.GetChild(0).GetComponent<CardDisplay>().Show();.cardName.GetComponent<MeshRenderer>().sortingOrder;
    }

    public void OnMouseDown()
    {
        if (TutorialController.tutorial.GetEnabled())
            return;

        selectedCardsLeftBorder = CollectionController.collectionController.GetSelectedCardLeftBorder();
        selectedCardsWidth = CollectionController.collectionController.GetSelectedCardWidth();

        hasNeverBeenShrunk = true;                              //Used so where the card can potentially go is shown
        transform.SetParent(selectedCardCanvas.transform);
        EnlargeCard();
        Vector3 newLocation = new Vector2(0, HandController.handController.GetCardHighlightHeight()) + (Vector2)CameraController.camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
        transform.position = newLocation;
        highlight.enabled = false;
        if (isShowingCard)
            CollectionController.collectionController.RemoveCardFromNew(card);
        else
            CollectionController.collectionController.RemoveEquipmentFromNew(equipment);

        //Set the stats texts on the collection controller
        if (equipment != null)
            CollectionController.collectionController.ResetStatsTexts(equipment);
        foreach (Image img in countIcons)
            img.enabled = false;

        //Find the card slots that the current card can go to
        if (equipment != null)  //If this card is an equipment card, it can go anywhere
            custCardSlots = 8;
        else if (new List<Card.Rarity> { Card.Rarity.StarterAttack, Card.Rarity.StarterDefence, Card.Rarity.StarterSpecial }.Contains(card.GetCard().rarity) || SceneManager.GetActiveScene().name != "StoryModeScene")
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
        if (TutorialController.tutorial.GetEnabled())
            return;

        int index = 0;
        //Find the index location that the card is being dragged over
        if (CameraController.camera.ScreenToWorldPoint(Input.mousePosition).y < -1.3)
        {
            if (equipment != null)                                                              //For equipments, set all spaces to white
                for (int i = 0; i < 8; i++)
                    CollectionController.collectionController.SetSelectCardWhiteOut(true, i, Color.white);
            else
            {
                CollectionController.collectionController.SetSelectedCardHighlightIndex(false, 0, Color.white);
                float positionX = Camera.main.ScreenToWorldPoint(Input.mousePosition).x - 100.0f;
                if (positionX == Mathf.Clamp(positionX, selectedCardsLeftBorder, -selectedCardsLeftBorder))                           //If card is dragged inside selected card range, show the card slots it can go into
                {
                    index = (int)((positionX - selectedCardsLeftBorder) / selectedCardsWidth);
                    CollectionController.collectionController.SetSelectCardWhiteOut(false, 0, Color.white);
                    if (index < custCardSlots)                                                  //Only white out select cards if the card is dragged over slots it can go into
                    {
                        CollectionController.collectionController.SetSelectCardWhiteOut(true, index, Color.white);
                        CollectionController.collectionController.SetSelectedCardHighlightIndex(true, index, Color.green);
                    }
                    else
                        CollectionController.collectionController.SetSelectCardWhiteOut(true, index, Color.red);
                }
                else
                    CollectionController.collectionController.SetSelectCardWhiteOut(false, 0, Color.white);  //If the card is outside range, hide all whiteouts
            }
        }
        else
        {
            CollectionController.collectionController.SetSelectedCardHighlightIndex(false, 0, Color.white);
        }

        //Enlarge and shink cards
        if (CameraController.camera.ScreenToWorldPoint(Input.mousePosition).y > -0.6)           //If the card is above the select cards threshold
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
        Vector2 offset = new Vector2(0, HandController.handController.GetCardHighlightHeight());
        if (!cardEnlarged)
            offset = new Vector2(0, 1.5f);
        Vector3 newLocation = offset + (Vector2)CameraController.camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
        transform.position = newLocation;
    }

    public void OnMouseUp()
    {
        if (TutorialController.tutorial.GetEnabled())
            return;

        transform.SetParent(originalCanvas.transform);

        if (!isShowingCard)
            CollectionController.collectionController.ResetStatsTexts();
        foreach (Image img in countIcons)
            img.enabled = true;

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

        float positionX = Camera.main.ScreenToWorldPoint(Input.mousePosition).x - 100.0f;
        if (positionX == Mathf.Clamp(positionX, selectedCardsLeftBorder, -selectedCardsLeftBorder) && Camera.main.ScreenToWorldPoint(Input.mousePosition).y < -1.3)         //If card is dragged inside selected card range, show the card slots it can go into
        {
            int index = (int)((positionX - selectedCardsLeftBorder) / selectedCardsWidth);

            if (CollectionController.collectionController.GetIfViableSelectSlot(card.GetCard(), index))  //Only allow cards to be selected in their respective slots
                SelectCard(index);
            else
            {
                TutorialController.tutorial.TriggerTutorial(Dialogue.Condition.CardEquippedLockedSlot, 1);
                CollectionController.collectionController.ShowErrorMessage("Outside runs, only starter cards can be placed into locked card slots");
            }
        }

        CollectionController.collectionController.SetSelectAreaWhiteOut(false);
        CollectionController.collectionController.SetSelectCardWhiteOut(false, 0, Color.white);
        CollectionController.collectionController.SetSelectedCardHighlightIndex(false, 0, Color.white);
        selectedCardCanvas.transform.SetAsLastSibling();
    }

    public void SelectCard(int index)
    {
        cardDisplay.cardSounds.PlaySelectSound();
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
        foreach (Image icon in countIcons)
            icon.enabled = true;
    }

    public void SetEquipment(Equipment equip, Card.CasterColor value)
    {
        equipment = equip;
        color = value;
        selectedCard.SetEquipment(equip);
        selectedCard.Hide();
        card = null;
        if (value != Card.CasterColor.Passive)
            foreach (Image icon in countIcons)
                icon.enabled = false;
    }

    public void SetCount(int newCount)
    {
        count.text = "x" + newCount.ToString();
        int totalCount = 0;
        if (isShowingCard)
            totalCount = CollectionController.collectionController.GetCountOfCardInCollection(card.GetCard());
        else
            totalCount = CollectionController.collectionController.GetCountOfEquipmentInCollection(equipment);

        for (int i = 0; i < countIcons.Length; i++)
        {
            if (i < totalCount)
                countIcons[i].GetComponent<Outline>().effectColor = selectedColor;
            else
                countIcons[i].GetComponent<Outline>().effectColor = unselectedColor;
            if (i >= totalCount - newCount)
                countIcons[i].color = unselectedColor;
            else
                countIcons[i].color = selectedColor;
            /*
            if (i < newCount)
                countIcons[i].color = selectedColor;
            else
                countIcons[i].color = unselectedColor;
            */
        }
    }

    public void Hide()
    {
        for (int i = 0; i < countIcons.Length; i++)
            countIcons[i].enabled = false;
        count.enabled = false;
        countBackdrop.enabled = false;
        if ((object)cardDisplay == null)
            cardDisplay = transform.GetChild(0).GetComponent<CardDisplay>();
        cardDisplay.Hide();
        selectedCard.Hide();
        col.enabled = false;
    }

    public void Show()
    {
        for (int i = 0; i < countIcons.Length; i++)
            countIcons[i].enabled = true;
        count.enabled = true;
        countBackdrop.enabled = true;
        if ((object)cardDisplay == null)
            cardDisplay = transform.GetChild(0).GetComponent<CardDisplay>();
        cardDisplay.Show();
        col.enabled = true;
        cardDisplay.lineRenderer.enabled = false;
    }

    private void EnlargeCard()
    {
        cardDisplay.cardSounds.PlaySelectSound();

        transform.localPosition = new Vector3(0, 0, 0);
        transform.localScale = new Vector3(HandController.handController.GetCardHighlightSize(), HandController.handController.GetCardHighlightSize(), 1);
        cardDisplay.SetToolTip(true, -1, 1, false);

        cardDisplay.Show();
        cardDisplay.lineRenderer.enabled = false;
        selectedCard.Hide();
        for (int i = 0; i < countIcons.Length; i++)
            countIcons[i].enabled = false;
        count.enabled = true;

        cardEnlarged = true;
    }

    private void ShrinkCard()
    {
        cardDisplay.cardSounds.PlayUncastSound();

        transform.localScale = localScale;
        transform.position = originalLocation;
        cardDisplay.SetToolTip(false, -1, 1, false);

        cardDisplay.Hide();
        selectedCard.Show();
        for (int i = 0; i < countIcons.Length; i++)
            countIcons[i].enabled = false;
        count.enabled = false;

        cardEnlarged = false;
    }

    public void SetIsShowingCard(bool value)
    {
        isShowingCard = value;
    }
}
