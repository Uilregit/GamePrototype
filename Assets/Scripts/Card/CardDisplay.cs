using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardDisplay : MonoBehaviour
{
    public Image cardBack;
    public Image cardGreyOut;
    public Image art;
    public TextMeshPro cardName;
    public Text energyCost;
    public Text description;
    public Text manaCost;
    public Image outline;
    public LineRenderer lineRenderer;

    [SerializeField] private Sprite redAttackBack;
    [SerializeField] private Sprite redSkillBack;
    [SerializeField] private Sprite greenAttackBack;
    [SerializeField] private Sprite greenSkillBack;
    [SerializeField] private Sprite blueAttackBack;
    [SerializeField] private Sprite blueSkillBack;
    [SerializeField] private Sprite blackCardBack;
    [SerializeField] private Sprite greyCardBack;

    private CardController thisCard;

    // Start is called before the first frame update
    void Awake()
    {
        thisCard = new CardController();
        //lineRenderer = GetComponent<LineRenderer>();
    }

    public void Hide()
    {
        art.enabled = false;
        cardBack.enabled = false;
        outline.enabled = false;
        cardGreyOut.enabled = false;
        cardName.GetComponent<MeshRenderer>().enabled = false;
        energyCost.enabled = false;
        description.enabled = false;
        manaCost.enabled = false;
        try
        {
            lineRenderer.enabled = false;
        }
        catch { }
    }

    public void Show()
    {
        art.enabled = true;
        cardBack.enabled = true;
        outline.enabled = true; //Will only be asked to show when cast, therefore always will have enough mana
        cardGreyOut.enabled = false; //^
        cardName.GetComponent<MeshRenderer>().enabled = true;
        manaCost.enabled = true;
        description.enabled = true;
        energyCost.enabled = true;
        lineRenderer.enabled = true;
    }

    public void SetCard(CardController card, bool dynamicNumbers = true)
    {
        thisCard = card;
        Card.CasterColor casterColor;
        if (card.GetCard().manaCost == 0)
            switch (card.GetCard().casterColor)
            {
                case (Card.CasterColor.Blue):
                    cardBack.sprite = blueAttackBack;
                    casterColor = Card.CasterColor.Blue;
                    break;
                case (Card.CasterColor.Red):
                    cardBack.sprite = redAttackBack;
                    casterColor = Card.CasterColor.Red;
                    break;
                case (Card.CasterColor.Green):
                    cardBack.sprite = greenAttackBack;
                    casterColor = Card.CasterColor.Green;
                    break;
                case (Card.CasterColor.Enemy):
                    cardBack.sprite = blackCardBack;
                    casterColor = Card.CasterColor.Enemy;
                    break;
                default:
                    cardBack.sprite = greyCardBack;
                    casterColor = Card.CasterColor.Gray;
                    break;
            }
        else
            switch (card.GetCard().casterColor)
            {
                case (Card.CasterColor.Blue):
                    cardBack.sprite = blueSkillBack;
                    casterColor = Card.CasterColor.Blue;
                    break;
                case (Card.CasterColor.Red):
                    cardBack.sprite = redSkillBack;
                    casterColor = Card.CasterColor.Red;
                    break;
                case (Card.CasterColor.Green):
                    cardBack.sprite = greenSkillBack;
                    casterColor = Card.CasterColor.Green;
                    break;
                case (Card.CasterColor.Enemy):
                    cardBack.sprite = blackCardBack;
                    casterColor = Card.CasterColor.Enemy;
                    break;
                default:
                    cardBack.sprite = greyCardBack;
                    casterColor = Card.CasterColor.Gray;
                    break;
            }
        art.sprite = card.GetCard().art;
        outline.sprite = cardBack.sprite;
        cardGreyOut.sprite = cardBack.sprite;
        cardName.text = card.GetCard().name;

        //Resolve energy and mana cost
        int netManaCost = card.GetCard().manaCost;
        int netEnergyCost = card.GetCard().energyCost;

        if (dynamicNumbers)
            try
            {
                netManaCost = card.GetNetManaCost();
                netEnergyCost = card.GetNetEnergyCost();
            }
            catch { }

        manaCost.text = netManaCost.ToString();
        energyCost.text = netEnergyCost.ToString();

        if (netManaCost < card.GetCard().manaCost)
            manaCost.color = Color.green;
        else if (netManaCost > card.GetCard().manaCost)
            manaCost.color = Color.red;
        else
            manaCost.color = Color.white;

        if (netEnergyCost < card.GetCard().energyCost)
            energyCost.color = Color.green;
        else if (netEnergyCost > card.GetCard().energyCost)
            energyCost.color = Color.red;
        else
            energyCost.color = Color.white;

        if (casterColor != Card.CasterColor.Enemy && casterColor != Card.CasterColor.Gray)
            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Player"))
                if (obj.GetComponent<PlayerController>().GetColorTag() == casterColor)
                {
                    card.SetCaster(obj);
                    break;
                }

        description.text = card.GetCard().description.Replace('|', '\n');
        if (dynamicNumbers)
        {
            string descriptionText = description.text;
            while (descriptionText.IndexOf("<s>") != -1)
            {
                int start = descriptionText.IndexOf("<s>");
                int end = descriptionText.IndexOf("</s>");

                string attackText = descriptionText.Substring(start, end - start + 4);

                int percentage = 100;
                int.TryParse(descriptionText.Substring(start + 3, descriptionText.IndexOf("%") - start - 3), out percentage);

                string finalText = "";
                int bonusAttack = 0;

                finalText = (Mathf.CeilToInt(GetComponent<CardController>().FindCaster(thisCard.GetCard()).GetComponent<HealthController>().GetAttack() * percentage / 100.0f)).ToString();
                bonusAttack = GetComponent<CardController>().FindCaster(thisCard.GetCard()).GetComponent<HealthController>().GetBonusAttack();

                if (bonusAttack > 0)
                    finalText = "*" + finalText + "*";
                else if (bonusAttack < 0)
                    finalText = "-" + finalText + "-";

                descriptionText = descriptionText.Replace(attackText, finalText);
            }
            while (descriptionText.IndexOf("<c>") != -1)
            {
                int start = descriptionText.IndexOf("<c>");
                int end = descriptionText.IndexOf("</c>");

                string cardText = descriptionText.Substring(start, end - start + 4);
                string newCardText = cardText.Replace("x", TurnController.turnController.GetNumerOfCardsPlayedInTurn().ToString())
                                             .Replace("<c>", "")
                                             .Replace("</c>", "");

                descriptionText = descriptionText.Replace(cardText, newCardText);
            }
            while (descriptionText.IndexOf("<ba>") != -1)
            {
                int start = descriptionText.IndexOf("<ba>");
                int end = descriptionText.IndexOf("</ba>");

                string cardText = descriptionText.Substring(start, end - start + 5);
                string newCardText = cardText.Replace("x", card.GetCaster().GetComponent<HealthController>().GetBonusShield().ToString())
                                             .Replace("<ba>", "")
                                             .Replace("</ba>", "");

                descriptionText = descriptionText.Replace(cardText, newCardText);
            }
            description.text = descriptionText;
        }
        else
        {
            if (description.text.Contains("<s>"))
            {
                string descriptionText = description.text;
                int startingCheckIndex = 0;
                while (descriptionText.IndexOf("<s>") != -1)
                {
                    int s = descriptionText.IndexOf("<s>");
                    int e = descriptionText.IndexOf("</s>");

                    string attackText = descriptionText.Substring(s, e - s + 4);

                    int percentage = 100;
                    int.TryParse(descriptionText.Substring(s + 3, descriptionText.IndexOf("%", startingCheckIndex) - s - 3), out percentage);

                    string finalText = "";

                    finalText = attackText.Replace("<s>", "").Replace("</s>", "") + "(" + (Mathf.CeilToInt(InformationController.infoController.GetStartingAttack(card.GetCard().casterColor) * percentage / 100.0f)).ToString() + ")";

                    descriptionText = descriptionText.Replace(attackText, finalText);

                    startingCheckIndex = descriptionText.IndexOf("%", startingCheckIndex) + 1;
                }

                description.text = descriptionText;
            }
            description.text = description.text.Replace("<s>", "").Replace("</s>", "");
            int start = description.text.IndexOf("<c>");
            int end = description.text.IndexOf("</c>");
            if (start != -1)
                description.text = description.text.Replace(description.text.Substring(start, end - start + 4), "");
            start = description.text.IndexOf("<ba>");
            end = description.text.IndexOf("</ba>");
            if (start != -1)
                description.text = description.text.Replace(description.text.Substring(start, end - start + 5), "");
        }
    }

    public void RefreshCardInfo()
    {
        SetCard(thisCard);
    }

    public CardController GetCard()
    {
        return thisCard;
    }

    public void SetHighLight(bool value)
    {
        outline.enabled = value;
        cardGreyOut.enabled = !value;
    }
}
