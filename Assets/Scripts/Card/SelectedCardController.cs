using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SelectedCardController : MonoBehaviour
{
    private CardController thisCard;
    public TextMeshPro cardName;
    public Image backImage;
    public Text manaCost;
    public Image manaIcon;
    public Text energyCost;
    public Image energyIcon;
    public Collider2D collider2d;

    public void SetCard(CardController card)
    {
        thisCard = card;

        backImage.transform.GetComponent<RectTransform>().pivot = new Vector2(0.5f, card.GetCard().art.pivot.y/card.GetCard().art.rect.height);
        backImage.sprite = card.GetCard().art;

        cardName.text = card.GetCard().name;
        manaCost.text = card.GetCard().manaCost.ToString();
        energyCost.text = card.GetCard().energyCost.ToString();
        /*
        switch (card.casterColor)
        {
            case Card.CasterColor.Red:
                backImage.color = HandController.handController.redCasterColor;
                break;
            case Card.CasterColor.Blue:
                backImage.color = HandController.handController.blueCasterColor;
                break;
            case Card.CasterColor.Green:
                backImage.color = HandController.handController.greenCasterColor;
                break;
        }
        */
        Show();
    }

    public void Hide()
    {
        cardName.GetComponent<Renderer>().enabled = false;
        backImage.enabled = false;
        manaIcon.enabled = false;
        manaCost.enabled = false;
        energyIcon.enabled = false;
        energyCost.enabled = false;
        collider2d.enabled = false;
    }

    public void Show()
    {
        cardName.GetComponent<Renderer>().enabled = true;
        backImage.enabled = true;
        manaIcon.enabled = true;
        manaCost.enabled = true;
        energyIcon.enabled = true;
        energyCost.enabled = true;
        collider2d.enabled = true;
    }

    public void Darken()
    {
        Color darkenColor = new Color(0.2f, 0.2f, 0.2f);
        cardName.color = darkenColor ;
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

    public void OnMouseDown()
    {
        CollectionController.collectionController.RemoveCard(thisCard);
    }
}
