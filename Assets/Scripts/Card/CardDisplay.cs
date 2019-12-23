using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardDisplay : MonoBehaviour
{
    public Image cardBack;
    public new Text name;
    public Text manaCost;
    public Text description;
    public Text range;
    public Image outline;

    [SerializeField] private Sprite redCardBack;
    [SerializeField] private Sprite greenCardBack;
    [SerializeField] private Sprite blueCardBack;
    [SerializeField] private Sprite blackCardBack;
    [SerializeField] private Sprite greyCardBack;

    private Card thisCard;

    // Start is called before the first frame update
    void Start()
    {
    }

    public void Hide()
    {
        cardBack.enabled = false;
        outline.enabled = false;
        name.enabled = false;
        manaCost.enabled = false;
        description.enabled = false;
        range.enabled = false;
    }

    public void Show()
    {
        cardBack.enabled = true;
        outline.enabled = true; //Will only be asked to show when cast, therefore always will have enough mana
        name.enabled = true;
        manaCost.enabled = true;
        description.enabled = true;
        range.enabled = true;
    }

    public void SetCard(Card card)
    {
        thisCard = card;
        switch (card.casterColor)
        {
            case (Card.CasterColor.Blue):
                cardBack.sprite = blueCardBack;
                break;
            case (Card.CasterColor.Red):
                cardBack.sprite = redCardBack;
                break;
            case (Card.CasterColor.Green):
                cardBack.sprite = greenCardBack;
                break;
            case (Card.CasterColor.Enemy):
                cardBack.sprite = blackCardBack;
                break;
            case (Card.CasterColor.Gray):
                cardBack.sprite = greyCardBack;
                break;
        }
        name.text = card.name;
        manaCost.text = card.manaCost.ToString();
        description.text = card.description.Replace('|', '\n');
        range.text = card.range.ToString();
    }

    public Card GetCard()
    {
        return thisCard;
    }

    public void SetHighLight(bool value)
    {
        outline.enabled = value;
    }
}
