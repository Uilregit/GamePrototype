﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardDisplay : MonoBehaviour
{
    public CardSoundController cardSounds;

    public Image toolTip;
    private Dictionary<string, string> toolTipDict = new Dictionary<string, string>()
    {
        { "Knockback", "Move target away from the caster" },
        { "Knock Away", "Move target's neighbours away from the target" },
        { "Fire Trap", "Deals 3 damage twice to ALL on trap at start and end of turn" },
        { "Targeted AoE", "Effects targets in 1 radius around the cast location" },
        { "Stun", "Target unable to move or play cards" },
        { "Taunt", "Target must move towards and target cards at the caster" },
        { "Manifest", "Choose a card from 3 options to add to your hand" },
        { "Temporary Copy", "Card only playable this turn, then card dissapears" },
        { "Piercing", "Damage not blocked by or affects armor" },
        { "Silence", "Target cannot play Mana cards" },
        { "Disarm", "Target cannot play Energy cards" },
        { "Sacrifice", "Destroy ALL summons and trigger their <b>Sacrifice</b> effects"},
        { "Hand Size", "Always draw a full hand at the start of every turn"},
    };
    private List<Image> thisToolTips = new List<Image>();

    public Animator anim;
    public Image cardBack;
    public Image cardGreyOut;
    public Image cardWhiteOut;
    public Image art;
    public TextMeshProUGUI cardName;
    public Text energyCost;
    public Text description;
    public Text manaCost;
    public Text atkChange;
    public Text armorChange;
    public Text healthChange;
    public GameObject[] statIcons;
    public Image equipmentIcon;
    public Image equipmentOutline;
    public Image outline;
    public Text disabledStatusText;
    private Outline conditionHighlight;
    public LineRenderer lineRenderer;

    private List<Material> materials = new List<Material>();

    [SerializeField] private Sprite attackGreyOut;
    [SerializeField] private Sprite skillGreyOut;
    [SerializeField] private Sprite weaponGreyOut;
    [SerializeField] private Sprite accessoryGreyOut;
    [SerializeField] private Sprite redAttackBack;
    [SerializeField] private Sprite redSkillBack;
    [SerializeField] private Sprite redWeaponBack;
    [SerializeField] private Sprite redAccessoryBack;
    [SerializeField] private Sprite greenAttackBack;
    [SerializeField] private Sprite greenSkillBack;
    [SerializeField] private Sprite greenWeaponBack;
    [SerializeField] private Sprite greenAccessoryBack;
    [SerializeField] private Sprite blueAttackBack;
    [SerializeField] private Sprite blueSkillBack;
    [SerializeField] private Sprite blueWeaponBack;
    [SerializeField] private Sprite blueAccessoryBack;
    [SerializeField] private Sprite orangeAttackBack;
    [SerializeField] private Sprite orangeSkillBack;
    [SerializeField] private Sprite orangeWeaponBack;
    [SerializeField] private Sprite orangeAccessoryBack;
    [SerializeField] private Sprite whiteAttackBack;
    [SerializeField] private Sprite whiteSkillBack;
    [SerializeField] private Sprite whiteWeaponBack;
    [SerializeField] private Sprite whiteAccessoryBack;
    [SerializeField] private Sprite blackAttackBack;
    [SerializeField] private Sprite blackSkillBack;
    [SerializeField] private Sprite blackWeaponBack;
    [SerializeField] private Sprite blackAccessoryBack;
    [SerializeField] private Sprite enemyAttackCardBack;
    [SerializeField] private Sprite enemySkillCardBack;
    [SerializeField] private Sprite enemyWeaponBack;
    [SerializeField] private Sprite enemyAccessoryBack;
    [SerializeField] private Sprite unequippedWeaponBack;
    [SerializeField] private Sprite unequippedAccessoryBack;
    //[SerializeField] private Sprite greyCardBack;

    private bool isShowingCard = true;
    private CardController thisCard;
    private Equipment thisEquipment;

    // Start is called before the first frame update
    void Awake()
    {
        thisCard = new CardController();
        conditionHighlight = outline.GetComponent<Outline>();

        art.material = new Material(art.material);
        cardBack.material = new Material(cardBack.material);
        equipmentOutline.material = new Material(equipmentOutline.material);
        equipmentIcon.material = new Material(equipmentIcon.material);
    }

    public void Hide()
    {
        art.enabled = false;
        cardBack.enabled = false;
        outline.enabled = false;
        cardGreyOut.enabled = false;
        cardWhiteOut.enabled = false;
        cardName.GetComponent<TextMeshProUGUI>().enabled = false;
        energyCost.enabled = false;
        description.enabled = false;
        manaCost.enabled = false;
        atkChange.enabled = false;
        armorChange.enabled = false;
        healthChange.enabled = false;
        foreach (GameObject obj in statIcons)
            obj.SetActive(false);
        equipmentIcon.gameObject.SetActive(false);
        equipmentOutline.enabled = false;
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
        cardWhiteOut.enabled = false;
        cardName.GetComponent<TextMeshProUGUI>().enabled = true;
        manaCost.enabled = true;
        atkChange.enabled = true;
        armorChange.enabled = true;
        healthChange.enabled = true;
        if (!isShowingCard)
            foreach (GameObject obj in statIcons)
                obj.SetActive(true);
        equipmentIcon.gameObject.SetActive(true);
        equipmentOutline.enabled = true;
        description.enabled = true;
        energyCost.enabled = true;
        lineRenderer.enabled = true;
    }

    public void SetEquipment(Equipment equip, Card.CasterColor equipedChar)
    {
        isShowingCard = false;
        thisEquipment = equip;
        if (equip.isWeapon)
            switch (equipedChar)
            {
                case (Card.CasterColor.Blue):
                    cardBack.sprite = blueWeaponBack;
                    break;
                case (Card.CasterColor.Red):
                    cardBack.sprite = redWeaponBack;
                    break;
                case (Card.CasterColor.Green):
                    cardBack.sprite = greenWeaponBack;
                    break;
                case (Card.CasterColor.Orange):
                    cardBack.sprite = orangeWeaponBack;
                    break;
                case (Card.CasterColor.White):
                    cardBack.sprite = whiteWeaponBack;
                    break;
                case (Card.CasterColor.Black):
                    cardBack.sprite = blackWeaponBack;
                    break;
                default:
                    cardBack.sprite = unequippedWeaponBack;
                    break;
            }
        else
            switch (equipedChar)
            {
                case (Card.CasterColor.Blue):
                    cardBack.sprite = blueAccessoryBack;
                    break;
                case (Card.CasterColor.Red):
                    cardBack.sprite = redAccessoryBack;
                    break;
                case (Card.CasterColor.Green):
                    cardBack.sprite = greenAccessoryBack;
                    break;
                case (Card.CasterColor.Orange):
                    cardBack.sprite = orangeAccessoryBack;
                    break;
                case (Card.CasterColor.White):
                    cardBack.sprite = whiteAccessoryBack;
                    break;
                case (Card.CasterColor.Black):
                    cardBack.sprite = blackAccessoryBack;
                    break;
                default:
                    cardBack.sprite = unequippedAccessoryBack;
                    break;
            }
        cardGreyOut.sprite = weaponGreyOut;
        cardWhiteOut.sprite = weaponGreyOut;

        art.sprite = equip.art;
        outline.sprite = cardBack.sprite;
        cardName.text = equip.equipmentName;

        if (equip.isWeapon)
        {
            energyCost.text = equip.numOfCardSlots.ToString();
            manaCost.text = "";
        }
        else
        {
            energyCost.text = "";
            manaCost.text = equip.numOfCardSlots.ToString();
        }

        atkChange.text = equip.atkChange.ToString();
        armorChange.text = equip.armorChange.ToString();
        healthChange.text = equip.healthChange.ToString();

        foreach (GameObject obj in statIcons)
            obj.SetActive(art.enabled);

        equipmentIcon.color = Color.clear;
        equipmentIcon.gameObject.SetActive(false);
        equipmentOutline.color = Color.clear;
        equipmentOutline.enabled = false;

        string desc = "";
        if (equip.isWeapon)
            desc += "<b>Weapon</b>\n";
        else
            desc += "<b>Accessory</b>\n";

        /*
        if (equip.numOfCardSlots == 1)
            desc += equip.numOfCardSlots.ToString() + " card slot. ";
        else
            desc += equip.numOfCardSlots.ToString() + " card slots. ";
        */
        desc += equip.equipmentDescription.Replace("|", "");
        description.text = desc;
        if (description.text.Contains("<s>"))
        {
            string descriptionText = description.text;
            int startingCheckIndex = 0;
            while (descriptionText.IndexOf("<s>") != -1)
            {
                int s = descriptionText.IndexOf("<s>");
                int e = descriptionText.IndexOf("</s>");
                int a = descriptionText.IndexOf("ATK");

                string attackText = descriptionText.Substring(s, e - s + 4);

                int percentage = 100;
                int.TryParse(descriptionText.Substring(s + 3, descriptionText.IndexOf("%", startingCheckIndex) - s - 3), out percentage);

                string finalText = "";

                finalText = attackText.Replace("<s>", "").Replace("</s>", "");

                if (finalText.IndexOf("ATK as") != -1 && equipedChar != Card.CasterColor.Enemy)
                    try
                    {
                        finalText = finalText.Replace("ATK as", "ATK (" + (Mathf.CeilToInt(InformationController.infoController.GetStartingAttack(equipedChar) * percentage / 100.0f)).ToString() + ") as");
                    }
                    catch { }

                descriptionText = descriptionText.Replace(attackText, finalText);

                startingCheckIndex = descriptionText.IndexOf("%", startingCheckIndex) + 1;
            }

            description.text = descriptionText;
        }
        description.text = description.text.Replace("<s>", "").Replace("</s>", "");
        description.color = Color.white;

        //Tooltips
        foreach (Image img in thisToolTips)
            Destroy(img.gameObject);
        thisToolTips = new List<Image>();

        foreach (string name in toolTipDict.Keys)
        {
            if (description.text.ToLower().Contains(name.ToLower()))
            {
                Image tt = Instantiate(toolTip);
                tt.transform.SetParent(transform);
                tt.transform.localPosition = new Vector3(1.25f, 0.8f, 0);
                tt.transform.localScale = new Vector3(0.4f, 0.4f, 1f);
                tt.transform.GetChild(0).GetComponent<Text>().text = "<b>" + name + "</b>\n" + toolTipDict[name];
                tt.GetComponent<Outline>().effectColor = tt.color;
                tt.gameObject.SetActive(false);
                thisToolTips.Add(tt);
            }
        }
    }

    public void SetCard(CardController card, bool dynamicNumbers = true)
    {
        isShowingCard = true;
        thisCard = card;
        Card.CasterColor casterColor = card.GetCard().casterColor;
        if (card.GetCard().manaCost == 0)
        {
            switch (card.GetCard().casterColor)
            {
                case (Card.CasterColor.Blue):
                    cardBack.sprite = blueAttackBack;
                    break;
                case (Card.CasterColor.Red):
                    cardBack.sprite = redAttackBack;
                    break;
                case (Card.CasterColor.Green):
                    cardBack.sprite = greenAttackBack;
                    break;
                case (Card.CasterColor.Orange):
                    cardBack.sprite = orangeAttackBack;
                    break;
                case (Card.CasterColor.White):
                    cardBack.sprite = whiteAttackBack;
                    break;
                case (Card.CasterColor.Black):
                    cardBack.sprite = blackAttackBack;
                    break;
                case (Card.CasterColor.Enemy):
                    cardBack.sprite = enemyAttackCardBack;
                    break;
                case (Card.CasterColor.Passive):
                    cardBack.sprite = unequippedWeaponBack;
                    break;
                default:
                    cardBack.sprite = enemyAttackCardBack;
                    break;
            }
            cardGreyOut.sprite = attackGreyOut;
            cardWhiteOut.sprite = attackGreyOut;
        }
        else
        {
            switch (card.GetCard().casterColor)
            {
                case (Card.CasterColor.Blue):
                    cardBack.sprite = blueSkillBack;
                    break;
                case (Card.CasterColor.Red):
                    cardBack.sprite = redSkillBack;
                    break;
                case (Card.CasterColor.Green):
                    cardBack.sprite = greenSkillBack;
                    break;
                case (Card.CasterColor.Orange):
                    cardBack.sprite = orangeSkillBack;
                    break;
                case (Card.CasterColor.White):
                    cardBack.sprite = whiteSkillBack;
                    break;
                case (Card.CasterColor.Black):
                    cardBack.sprite = blackSkillBack;
                    break;
                case (Card.CasterColor.Enemy):
                    cardBack.sprite = enemySkillCardBack;
                    break;
                case (Card.CasterColor.Passive):
                    cardBack.sprite = unequippedWeaponBack;
                    break;
                default:
                    cardBack.sprite = enemySkillCardBack;
                    break;
            }
            cardGreyOut.sprite = skillGreyOut;
            cardWhiteOut.sprite = skillGreyOut;
        }
        art.sprite = card.GetCard().art;
        outline.sprite = cardBack.sprite;
        cardName.text = card.GetCard().name;
        foreach (GameObject obj in statIcons)
            obj.SetActive(false);

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

        if (card.GetCard().manaCost > 0)
        {
            manaCost.text = netManaCost.ToString();
            energyCost.text = "";
        }
        else
        {
            manaCost.text = "";
            energyCost.text = netEnergyCost.ToString();
        }

        atkChange.text = "";
        armorChange.text = "";
        healthChange.text = "";

        if (netManaCost < card.GetCard().manaCost)
            manaCost.color = Color.green;
        else if (netManaCost > card.GetCard().manaCost)
            manaCost.color = new Color(255, 186, 0);
        else
            manaCost.color = Color.white;

        if (netEnergyCost < card.GetCard().energyCost)
            energyCost.color = Color.green;
        else if (netEnergyCost > card.GetCard().energyCost)
            energyCost.color = new Color(255, 186, 0);
        else
            energyCost.color = Color.white;

        if (casterColor != Card.CasterColor.Enemy && casterColor != Card.CasterColor.Gray)
            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Player"))
                try
                {
                    //Debug.Log("tried setting caster");
                    if (obj.GetComponent<PlayerController>().GetColorTag() == casterColor && !obj.GetComponent<HealthController>().GetIsSimulation())
                    {

                        card.SetCaster(obj);
                        break;
                    }
                }
                catch
                {
                    //Debug.Log("tried setting caster");
                    if (obj.GetComponent<MultiplayerPlayerController>().GetColorTag() == casterColor)
                    {

                        //Debug.Log(obj);
                        card.SetCaster(obj);
                        break;
                    }
                }

        description.text = card.GetCard().description.Replace('|', '\n');
        description.color = Color.black;

        if (!card.isResurrectCard && card.GetAttachedEquipment() != null && card.GetAttachedEquipment().equipmentDescription.IndexOf("|") > 0)
        {
            equipmentIcon.color = Color.white;
            equipmentIcon.gameObject.SetActive(true);
            equipmentIcon.transform.GetChild(0).GetComponent<Image>().sprite = card.GetAttachedEquipment().art;
            equipmentOutline.color = Color.white;
            equipmentOutline.enabled = true;
            if (card.GetAttachedEquipment().isWeapon)
                equipmentOutline.color = Color.red;
            else
                equipmentOutline.color = Color.blue;
            if (card.GetCard().manaCost == 0)
            {
                equipmentIcon.transform.localPosition = new Vector2(-1.22f, 1.03f);
                equipmentOutline.transform.localPosition = new Vector2(-1.22f, 1.03f);
            }
            else
            {
                equipmentIcon.transform.localPosition = new Vector2(1.22f, 1.03f);
                equipmentOutline.transform.localPosition = new Vector2(1.22f, 1.03f);
            }
        }
        else
        {
            equipmentIcon.color = Color.clear;
            equipmentIcon.gameObject.SetActive(false);
            equipmentOutline.color = Color.clear;
            equipmentOutline.enabled = false;
        }

        //Formatting Codes for dynamic card text
        string[] formattingCodes = new string[] { "cp", "ch", "ba", "bh", "ms", "es", "dm", "l", "d" };

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

                try
                {
                    finalText = (Mathf.CeilToInt(transform.parent.GetComponent<CardController>().FindCaster(thisCard.GetCard()).GetComponent<HealthController>().GetAttack() * percentage / 100.0f)).ToString();
                    bonusAttack = transform.parent.GetComponent<CardController>().FindCaster(thisCard.GetCard()).GetComponent<HealthController>().GetBonusAttack();
                }
                catch
                {
                    finalText = (Mathf.CeilToInt(InformationController.infoController.GetStartingAttack(thisCard.GetCard().casterColor) * percentage / 100.0f)).ToString();
                }

                if (bonusAttack > 0)
                    finalText = "*" + finalText + "*";
                else if (bonusAttack < 0)
                    finalText = "-" + finalText + "-";

                descriptionText = descriptionText.Replace(attackText, finalText);
            }

            int dm = 0;
            try { dm = card.FindCaster(thisCard.GetCard()).GetComponent<PlayerMoveController>().GetMovedDistance(); } catch { };

            //Formatting Nums for dynamic card text
            int[] formattingNums = new int[0];
            try
            {
                formattingNums = new int[] { TurnController.turnController.GetNumerOfCardsPlayedInTurn(),
                                               HandController.handController.GetHand().Count,
                                               card.FindCaster(thisCard.GetCard()).GetComponent<HealthController>().GetBonusArmor(),
                                               card.FindCaster(thisCard.GetCard()).GetComponent<HealthController>().GetBonusVit(),
                                               TurnController.turnController.GetManaSpent(),
                                               TurnController.turnController.GetEnergySpent(),
                                               dm,
                                               ResourceController.resource.GetNumberOfRevivesUsed(),
                                               GameController.gameController.GetDoomCounter()};
            }
            catch
            {
                formattingNums = new int[] { 0,
                                             0,
                                             0,
                                             0,
                                             0,
                                             0,
                                             0,
                                             0};
            }
            for (int i = 0; i < formattingCodes.Length; i++)
                while (descriptionText.IndexOf("<%>".Replace("%", formattingCodes[i])) != -1)
                {
                    int start = descriptionText.IndexOf("<%>".Replace("%", formattingCodes[i]));
                    int end = descriptionText.IndexOf("</%>".Replace("%", formattingCodes[i]));

                    string cardText = descriptionText.Substring(start, end - start + formattingCodes[i].Length + 3);
                    string newCardText = cardText.Replace("x", formattingNums[i].ToString())
                                                 .Replace("<%>".Replace("%", formattingCodes[i]), "")
                                                 .Replace("</%>".Replace("%", formattingCodes[i]), "");

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
                    int a = descriptionText.IndexOf("ATK");

                    string attackText = descriptionText.Substring(s, e - s + 4);

                    int percentage = 100;
                    int.TryParse(descriptionText.Substring(s + 3, descriptionText.IndexOf("%", startingCheckIndex) - s - 3), out percentage);

                    string finalText = "";

                    finalText = attackText.Replace("<s>", "").Replace("</s>", "");

                    if (finalText.IndexOf("ATK as") != -1)
                        try
                        {
                            finalText = finalText.Replace("ATK as", "ATK (" + (Mathf.CeilToInt(InformationController.infoController.GetStartingAttack(card.GetCard().casterColor) * percentage / 100.0f)).ToString() + ") as");
                        }
                        catch { }

                    descriptionText = descriptionText.Replace(attackText, finalText);

                    startingCheckIndex = descriptionText.IndexOf("%", startingCheckIndex) + 1;
                }

                description.text = descriptionText;
            }
            description.text = description.text.Replace("<s>", "").Replace("</s>", "");

            for (int i = 0; i < formattingCodes.Length; i++)
            {
                int start = description.text.IndexOf("<%>".Replace("%", formattingCodes[i]));
                int end = description.text.IndexOf("</%>".Replace("%", formattingCodes[i]));
                if (start != -1)
                    description.text = description.text.Replace(description.text.Substring(start, end - start + 3 + formattingCodes[i].Length), "");
            }
        }

        //Tooltips
        foreach (Image img in thisToolTips)
            Destroy(img.gameObject);
        thisToolTips = new List<Image>();

        if (!card.isResurrectCard && card.GetAttachedEquipment() != null && card.GetAttachedEquipment().equipmentDescription.IndexOf("|") > 0)
        {
            Image tt = Instantiate(toolTip);
            tt.transform.SetParent(transform);
            tt.transform.localPosition = new Vector3(1.25f, 0.8f, 0);
            tt.transform.localScale = new Vector3(0.4f, 0.4f, 1f);
            int length = card.GetAttachedEquipment().equipmentDescription.IndexOf("|");
            string descriptionText = "<b>" + card.GetAttachedEquipment().equipmentName + "</b>\n" + card.GetAttachedEquipment().equipmentDescription.Substring(0, length);
            while (descriptionText.IndexOf("<s>") != -1)
            {
                int start = descriptionText.IndexOf("<s>");
                int end = descriptionText.IndexOf("</s>");

                string attackText = descriptionText.Substring(start, end - start + 4);

                int percentage = 100;
                int.TryParse(descriptionText.Substring(start + 3, descriptionText.IndexOf("%") - start - 3), out percentage);

                string finalText = "";
                int bonusAttack = 0;

                try
                {
                    finalText = (Mathf.CeilToInt(transform.parent.GetComponent<CardController>().FindCaster(thisCard.GetCard()).GetComponent<HealthController>().GetAttack() * percentage / 100.0f)).ToString();
                    bonusAttack = transform.parent.GetComponent<CardController>().FindCaster(thisCard.GetCard()).GetComponent<HealthController>().GetBonusAttack();
                }
                catch
                {
                    finalText = (Mathf.CeilToInt(InformationController.infoController.GetStartingAttack(thisCard.GetCard().casterColor) * percentage / 100.0f)).ToString();
                }

                if (bonusAttack > 0)
                    finalText = "*" + finalText + "*";
                else if (bonusAttack < 0)
                    finalText = "-" + finalText + "-";

                descriptionText = descriptionText.Replace(attackText, finalText);
            }
            tt.transform.GetChild(0).GetComponent<Text>().text = descriptionText;
            if (card.GetAttachedEquipment().isWeapon)
                tt.GetComponent<Outline>().effectColor = Color.red;
            else
                tt.GetComponent<Outline>().effectColor = Color.blue;
            tt.gameObject.SetActive(false);
            thisToolTips.Add(tt);
        }
        foreach (string name in toolTipDict.Keys)
        {
            if (description.text.ToLower().Contains(name.ToLower()))
            {
                Image tt = Instantiate(toolTip);
                tt.transform.SetParent(transform);
                tt.transform.localPosition = new Vector3(1.25f, 0.8f, 0);
                tt.transform.localScale = new Vector3(0.4f, 0.4f, 1f);
                tt.transform.GetChild(0).GetComponent<Text>().text = "<b>" + name + "</b>\n" + toolTipDict[name];
                tt.GetComponent<Outline>().effectColor = tt.color;
                tt.gameObject.SetActive(false);
                thisToolTips.Add(tt);
            }
        }
    }

    public void SetConditionHighlight(bool value)
    {
        outline.enabled = value;
        conditionHighlight.enabled = value;
    }

    public void RefreshCardInfo()
    {
        SetCard(thisCard);
    }

    public CardController GetCard()
    {
        return thisCard;
    }

    public Equipment GetEquipment()
    {
        return thisEquipment;
    }

    public void SetHighLight(bool value)
    {
        //outline.enabled = value;
        cardGreyOut.enabled = !value;
    }

    public bool GetHighlight()
    {
        return !cardGreyOut.enabled;
    }

    public void SetToolTip(bool value, int index = -1, int total = 1, bool inCombat = true)
    {
        for (int i = 0; i < thisToolTips.Count; i++)
        {
            if (thisToolTips[i].isActiveAndEnabled != value)
            {
                if (total == 1)
                {
                    if (inCombat)
                    {
                        if (transform.position.x > 0)
                            thisToolTips[i].transform.localPosition = new Vector3(-1.25f, 0.8f - 0.45f * i, 0);
                        else
                            thisToolTips[i].transform.localPosition = new Vector3(1.25f, 0.8f - 0.45f * i, 0);
                    }
                    else
                        thisToolTips[i].transform.localPosition = new Vector3(0, -0.95f - 0.45f * i, 0);
                }
                else
                    thisToolTips[i].transform.localPosition = new Vector3(0, -0.95f - 0.45f * i, 0);
                thisToolTips[i].transform.localRotation = Quaternion.identity;
                thisToolTips[i].gameObject.SetActive(value);
            }
        }
    }

    public void SetDisableStats(string status)
    {
        if (disabledStatusText.text.Contains(status))   //If the status is already reflected, skip
            return;

        disabledStatusText.enabled = true;

        if (disabledStatusText.text == "")
            disabledStatusText.text += status;
        else
            disabledStatusText.text += "\n" + status;
    }

    public void FadeOut(float time, Color emissionColor)
    {
        outline.enabled = false;
        StartCoroutine(ExecuteFadeOut(time, emissionColor));
    }

    public void FadeIn(float time, Color emissionColor)
    {
        StartCoroutine(ExecuteFadeIn(time, emissionColor));
    }

    private IEnumerator ExecuteFadeOut(float time, Color emissionColor)
    {
        if (emissionColor != Color.clear)
        {
            art.material.SetColor("_Color", emissionColor * 0.5f);
            cardBack.material.SetColor("_Color", emissionColor * 0.5f);
            equipmentIcon.material.SetColor("_Color", emissionColor * 0.5f);
            equipmentOutline.material.SetColor("_Color", emissionColor * 0.5f);
        }

        Color originalManaColor = manaCost.color;
        Color originalEnergyColor = energyCost.color;
        Color originalTitleColor = cardName.color;
        Color originalDescriptionColor = description.color;
        for (int i = 0; i < 10; i++)
        {
            manaCost.color = Color.Lerp(originalManaColor, Color.clear, i / 9f);
            energyCost.color = Color.Lerp(originalEnergyColor, Color.clear, i / 9f);
            cardName.color = Color.Lerp(originalTitleColor, Color.clear, i / 9f);
            description.color = Color.Lerp(originalDescriptionColor, Color.clear, i / 9f);

            art.material.SetFloat("_Dissolve", (9 - i) / 9f);
            cardBack.material.SetFloat("_Dissolve", (9 - i) / 9f);
            equipmentIcon.material.SetFloat("_Dissolve", (9 - i) / 9f);
            equipmentOutline.material.SetFloat("_Dissolve", (9 - i) / 9f);

            yield return new WaitForSeconds(time / 10f);
        }
    }

    private IEnumerator ExecuteFadeIn(float time, Color emissionColor)
    {
        if (emissionColor != Color.clear)
        {
            art.material.SetColor("_Color", emissionColor * 0.5f);
            cardBack.material.SetColor("_Color", emissionColor * 0.5f);
            equipmentIcon.material.SetColor("_Color", emissionColor * 0.5f);
            equipmentOutline.material.SetColor("_Color", emissionColor * 0.5f);
        }

        Color originalManaColor = manaCost.color;
        Color originalEnergyColor = energyCost.color;
        Color originalTitleColor = cardName.color;
        Color originalDescriptionColor = description.color;
        for (int i = 0; i < 10; i++)
        {
            manaCost.color = Color.Lerp(Color.clear, originalManaColor, i / 9f);
            energyCost.color = Color.Lerp(Color.clear, originalEnergyColor, i / 9f);
            cardName.color = Color.Lerp(Color.clear, originalTitleColor, i / 9f);
            description.color = Color.Lerp(Color.clear, originalDescriptionColor, i / 9f);

            art.material.SetFloat("_Dissolve", i / 9f);
            cardBack.material.SetFloat("_Dissolve", i / 9f);
            equipmentIcon.material.SetFloat("_Dissolve", i / 9f);
            equipmentOutline.material.SetFloat("_Dissolve", i / 9f);



            yield return new WaitForSeconds(time / 10f);
        }
    }
}
