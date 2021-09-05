using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class SelectedCardController : MonoBehaviour
{
    private CardController thisCard;
    public int index = 0;
    public TextMeshPro cardName;
    public Image backImage;
    public Text manaCost;
    public Image manaIcon;
    public Text energyCost;
    public Image energyIcon;
    public Image highlight;
    public Collider2D collider2d;
    public Image whiteOut;
    public CardDisplay cardDisplay;
    private bool clickable = true;

    private int manacost = -1;

    private bool isShowingCardDisplay = false;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool isShowing = true;

    private int custCardSlots = 0;
    private int cardDragIndex = 0;

    public void SetCard(CardController card)
    {
        thisCard = card;

        backImage.transform.GetComponent<RectTransform>().pivot = new Vector2(0.5f, card.GetCard().art.pivot.y / card.GetCard().art.rect.height);
        backImage.sprite = card.GetCard().art;

        cardName.text = card.GetCard().name;
        manacost = card.GetCard().manaCost;
        if (manacost > 0)
        {
            manaCost.text = card.GetCard().manaCost.ToString();
            manaIcon.enabled = true;
        }
        else
        {
            manaCost.text = "";
            manaIcon.enabled = false;
        }

        if (manacost == 0)
        {
            energyCost.text = card.GetCard().energyCost.ToString();
            energyIcon.enabled = true;
        }
        else
        {
            energyCost.text = "";
            energyIcon.enabled = false;
        }
        highlight.enabled = false;

        cardDisplay.SetCard(card, false);
        cardDisplay.Hide();

        Show();
    }

    public void SetEquipment(Equipment equip)
    {
        thisCard = null;

        backImage.transform.GetComponent<RectTransform>().pivot = new Vector2(0.5f, equip.art.pivot.y / equip.art.rect.height);
        backImage.sprite = equip.art;

        cardName.text = equip.equipmentName;
        manaCost.text = "";
        energyCost.text = "";
        manaIcon.enabled = false;
        energyIcon.enabled = false;

        highlight.enabled = false;

        Show();
    }

    public void Hide()
    {
        isShowing = false;
        cardName.GetComponent<Renderer>().enabled = false;
        backImage.enabled = false;
        manaIcon.enabled = false;
        manaCost.enabled = false;
        energyIcon.enabled = false;
        energyCost.enabled = false;
        //collider2d.enabled = false;
        clickable = false;
    }

    public void Show()
    {
        isShowing = true;
        cardName.GetComponent<Renderer>().enabled = true;
        backImage.enabled = true;
        if (manacost > 0)
        {
            manaIcon.enabled = true;
            manaCost.enabled = true;
        }
        if (manacost == 0)
        {
            energyIcon.enabled = true;
            energyCost.enabled = true;
        }
        //collider2d.enabled = true;
        clickable = true;
    }

    public void Darken()
    {
        Color darkenColor = new Color(0.2f, 0.2f, 0.2f);
        cardName.color = darkenColor;
        backImage.color = darkenColor;
        manaIcon.color = darkenColor;
        manaCost.color = darkenColor;
        energyIcon.color = darkenColor;
        energyCost.color = darkenColor;
        collider2d.enabled = false;
    }

    public void Brighten()
    {
        cardName.color = Color.white;
        backImage.color = Color.white;
        manaIcon.color = Color.white;
        manaCost.color = Color.white;
        energyIcon.color = Color.white;
        energyCost.color = Color.white;
        collider2d.enabled = true;
    }

    public void SetHighlight(bool state)
    {
        highlight.enabled = state;
    }

    public void OnMouseDown()
    {
        if (!clickable)
            return;

        originalPosition = transform.position;
        originalRotation = transform.rotation;
    }

    public void OnMouseDrag()
    {
        if (!clickable)
            return;

        Vector2 offset = new Vector2(0, 1.3f);
        if (isShowingCardDisplay)
            offset = new Vector2(0, HandController.handController.cardHighlightHeight);
        Vector3 newLocation = offset + (Vector2)CameraController.camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
        transform.position = newLocation;

        CollectionController.collectionController.SetSelectAreaWhiteOut(CameraController.camera.ScreenToWorldPoint(Input.mousePosition).y > -0.3);

        if (CameraController.camera.ScreenToWorldPoint(Input.mousePosition).y > -1.3)
        {
            if (isShowing)
            {
                cardDisplay.Show();
                cardDisplay.lineRenderer.enabled = false;
                Hide();
                clickable = true;
                transform.rotation = Quaternion.identity;
                isShowingCardDisplay = true;
            }
        }
        else
        {
            if (!isShowing)
            {
                cardDisplay.Hide();
                Show();
                transform.rotation = originalRotation;
                isShowingCardDisplay = false;
            }
        }

        if (new List<Card.Rarity> { Card.Rarity.StarterAttack, Card.Rarity.StarterDefence, Card.Rarity.StarterSpecial }.Contains(thisCard.GetCard().rarity) || SceneManager.GetActiveScene().name != "StoryModeScene")
            custCardSlots = 8;
        else
        {
            Vector2 ranges = CollectionController.collectionController.GetWeaponAndAccessorySlots();
            custCardSlots = (int)(ranges.x + ranges.y);
        }

        //Find the index location that the card is being dragged over
        if (CameraController.camera.ScreenToWorldPoint(Input.mousePosition).y < -1.3)
        {
            float positionX = Camera.main.ScreenToWorldPoint(Input.mousePosition).x - 8.0f;
            if (positionX == Mathf.Clamp(positionX, -2.4f, 2.4f))                           //If card is dragged inside selected card range, show the card slots it can go into
            {
                cardDragIndex = (int)((positionX + 2.4f) / 0.6f);
                CollectionController.collectionController.SetSelectCardWhiteOut(false, 0, Color.white);
                if (cardDragIndex < custCardSlots)                                                  //Only white out select cards if the card is dragged over slots it can go into
                    CollectionController.collectionController.SetSelectCardWhiteOut(true, cardDragIndex, Color.white);
                else
                    CollectionController.collectionController.SetSelectCardWhiteOut(true, cardDragIndex, Color.red);
            }
            else
                CollectionController.collectionController.SetSelectCardWhiteOut(false, 0, Color.white);  //If the card is outside range, hide all whiteouts
        }
    }

    public void OnMouseUp()
    {
        if (!clickable)
            return;

        isShowingCardDisplay = false;
        cardDisplay.Hide();
        Show();
        transform.rotation = originalRotation;
        transform.position = originalPosition;

        CollectionController.collectionController.SetSelectAreaWhiteOut(false);
        CollectionController.collectionController.SetSelectCardWhiteOut(false, 0, Color.white);

        if (CameraController.camera.ScreenToWorldPoint(Input.mousePosition).y > -0.3)
        {
            clickable = false;
            CollectionController.collectionController.RemoveCard(thisCard, index);
        }
        else if (CameraController.camera.ScreenToWorldPoint(Input.mousePosition).y < -1.3 && cardDragIndex != index)        //If card is dragged to another equipped card's location
        {
            float positionX = Camera.main.ScreenToWorldPoint(Input.mousePosition).x - 8.0f;
            if (positionX == Mathf.Clamp(positionX, -2.4f, 2.4f))                           //If card is dragged inside selected card range, show the card slots it can go into
                if (cardDragIndex < custCardSlots)
                    CollectionController.collectionController.SwapCards(cardDragIndex, index);
                else
                    CollectionController.collectionController.ShowErrorMessage("Outside runs, only starter cards can be placed into locked card slots");
        }
    }

    public void SetWhiteOut(bool state, Color c)
    {
        whiteOut.enabled = state;
        whiteOut.color = new Color(c.r, c.g, c.b, whiteOut.color.a);
    }
}
