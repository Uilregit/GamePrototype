using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public Image cardFace;
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
    public Image cardBack;

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

    private bool isFacingUp = true;

    // Start is called before the first frame update
    void Awake()
    {
        thisCard = new CardController();
        conditionHighlight = outline.GetComponent<Outline>();

        art.material = new Material(art.material);
        cardFace.material = new Material(cardFace.material);
        equipmentOutline.material = new Material(equipmentOutline.material);
        equipmentIcon.material = new Material(equipmentIcon.material);
    }

    public void Hide()
    {
        art.enabled = false;
        cardFace.enabled = false;
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
        cardFace.enabled = true;
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
        lineRenderer.SetPositions(new Vector3[] { Vector3.zero, Vector3.zero });
    }

    public void PlaceFaceDown()
    {
        if (isFacingUp)
            transform.rotation = transform.rotation * Quaternion.Euler(new Vector3(0, -180, 0));
        isFacingUp = false;
        Hide();
        cardBack.enabled = true;
    }

    public void PlaceFaceUp()
    {
        if (!isFacingUp)
            transform.rotation = transform.rotation * Quaternion.Euler(new Vector3(0, -180, 0));
        isFacingUp = true;
        Show();
        cardBack.enabled = false;
    }

    public void FlipUp(float duration = 0.5f)
    {
        isFacingUp = true;
        StartCoroutine(FlipUpProcess(duration));
    }

    private IEnumerator FlipUpProcess(float duration)
    {
        Quaternion originalRotation = transform.rotation;
        for (int i = 0; i < 20; i++)
        {
            transform.rotation = Quaternion.Lerp(originalRotation, originalRotation * Quaternion.Euler(new Vector3(0, 180, 0)), i / 19f);
            if (transform.rotation.eulerAngles.y < 90)
            {
                Show();
                cardBack.enabled = false;
            }
            yield return new WaitForSeconds(duration / 20f);
        }
        transform.rotation = originalRotation * Quaternion.Euler(new Vector3(0, 180, 0));
        try
        {
            thisCard.ResetPlayability(TurnController.turnController.GetCurrentEnergy(), TurnController.turnController.GetCurrentMana());
        }
        catch { }
    }

    public void FlipDown(float duration = 0.5f)
    {
        isFacingUp = false;
        StartCoroutine(FlipDownProcess(duration));
    }

    private IEnumerator FlipDownProcess(float duration)
    {
        Quaternion originalRotation = Quaternion.Euler(new Vector3(transform.rotation.eulerAngles.x, Mathf.RoundToInt(transform.rotation.eulerAngles.y / 180f) * 180, transform.rotation.eulerAngles.z));
        for (int i = 0; i < 10; i++)
        {
            transform.rotation = Quaternion.Lerp(originalRotation, originalRotation * Quaternion.Euler(new Vector3(0, 181, 0)), i / 9f);
            if (0 < transform.rotation.eulerAngles.y && transform.rotation.eulerAngles.y < 270)
            {
                Hide();
                cardBack.enabled = true;
            }
            yield return new WaitForSeconds(duration / 10f);
        }
        transform.rotation = originalRotation * Quaternion.Euler(new Vector3(0, 180, 0));
    }

    public void SetEquipment(Equipment equip, Card.CasterColor equipedChar)
    {
        isShowingCard = false;
        thisEquipment = equip;
        if (equip.isWeapon)
            switch (equipedChar)
            {
                case (Card.CasterColor.Blue):
                    cardFace.sprite = blueWeaponBack;
                    break;
                case (Card.CasterColor.Red):
                    cardFace.sprite = redWeaponBack;
                    break;
                case (Card.CasterColor.Green):
                    cardFace.sprite = greenWeaponBack;
                    break;
                case (Card.CasterColor.Orange):
                    cardFace.sprite = orangeWeaponBack;
                    break;
                case (Card.CasterColor.White):
                    cardFace.sprite = whiteWeaponBack;
                    break;
                case (Card.CasterColor.Black):
                    cardFace.sprite = blackWeaponBack;
                    break;
                default:
                    cardFace.sprite = unequippedWeaponBack;
                    break;
            }
        else
            switch (equipedChar)
            {
                case (Card.CasterColor.Blue):
                    cardFace.sprite = blueAccessoryBack;
                    break;
                case (Card.CasterColor.Red):
                    cardFace.sprite = redAccessoryBack;
                    break;
                case (Card.CasterColor.Green):
                    cardFace.sprite = greenAccessoryBack;
                    break;
                case (Card.CasterColor.Orange):
                    cardFace.sprite = orangeAccessoryBack;
                    break;
                case (Card.CasterColor.White):
                    cardFace.sprite = whiteAccessoryBack;
                    break;
                case (Card.CasterColor.Black):
                    cardFace.sprite = blackAccessoryBack;
                    break;
                default:
                    cardFace.sprite = unequippedAccessoryBack;
                    break;
            }
        cardGreyOut.sprite = weaponGreyOut;
        cardWhiteOut.sprite = weaponGreyOut;

        art.sprite = equip.art;
        outline.sprite = cardFace.sprite;
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

    public void SetCard(Card card, bool dynamicNumbers = true)
    {
        CardController c = this.gameObject.AddComponent<CardController>();
        c.SetCardDisplay(this);
        c.SetCard(card, false, true, dynamicNumbers);
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
                    cardFace.sprite = blueAttackBack;
                    break;
                case (Card.CasterColor.Red):
                    cardFace.sprite = redAttackBack;
                    break;
                case (Card.CasterColor.Green):
                    cardFace.sprite = greenAttackBack;
                    break;
                case (Card.CasterColor.Orange):
                    cardFace.sprite = orangeAttackBack;
                    break;
                case (Card.CasterColor.White):
                    cardFace.sprite = whiteAttackBack;
                    break;
                case (Card.CasterColor.Black):
                    cardFace.sprite = blackAttackBack;
                    break;
                case (Card.CasterColor.Enemy):
                    cardFace.sprite = enemyAttackCardBack;
                    break;
                case (Card.CasterColor.Passive):
                    cardFace.sprite = unequippedWeaponBack;
                    break;
                default:
                    cardFace.sprite = enemyAttackCardBack;
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
                    cardFace.sprite = blueSkillBack;
                    break;
                case (Card.CasterColor.Red):
                    cardFace.sprite = redSkillBack;
                    break;
                case (Card.CasterColor.Green):
                    cardFace.sprite = greenSkillBack;
                    break;
                case (Card.CasterColor.Orange):
                    cardFace.sprite = orangeSkillBack;
                    break;
                case (Card.CasterColor.White):
                    cardFace.sprite = whiteSkillBack;
                    break;
                case (Card.CasterColor.Black):
                    cardFace.sprite = blackSkillBack;
                    break;
                case (Card.CasterColor.Enemy):
                    cardFace.sprite = enemySkillCardBack;
                    break;
                case (Card.CasterColor.Passive):
                    cardFace.sprite = unequippedWeaponBack;
                    break;
                default:
                    cardFace.sprite = enemySkillCardBack;
                    break;
            }
            cardGreyOut.sprite = skillGreyOut;
            cardWhiteOut.sprite = skillGreyOut;
        }
        art.sprite = card.GetCard().art;
        outline.sprite = cardFace.sprite;
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
                    if (obj.GetComponent<PlayerController>().GetColorTag() == casterColor && !obj.GetComponent<HealthController>().GetIsSimulation())
                    {
                        card.SetCaster(obj);
                        break;
                    }
                }
                catch
                {
                    if (obj.GetComponent<MultiplayerPlayerController>().GetColorTag() == casterColor)
                    {
                        card.SetCaster(obj);
                        break;
                    }
                }

        description.text = card.GetCard().description.Replace('|', '\n');
        description.color = Color.black;

        if (!card.isResurrectCard && card.GetAttachedEquipment() != null && card.GetAttachedEquipment().equipmentDescription.IndexOf("|") > 0)
        {
            equipmentIcon.color = Color.white;
            equipmentIcon.gameObject.SetActive(true && art.enabled);
            equipmentIcon.transform.GetChild(0).GetComponent<Image>().sprite = card.GetAttachedEquipment().art;
            equipmentOutline.color = Color.white;
            equipmentOutline.enabled = true && art.enabled;
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
        Dictionary<string, int> formattingCodes = GetFormattingCodes();
        List<string> formattingKeys = formattingCodes.Keys.ToList();

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

            for (int i = 0; i < formattingKeys.Count; i++)
                while (descriptionText.IndexOf("<%>".Replace("%", formattingKeys[i])) != -1)
                {
                    int start = descriptionText.IndexOf("<%>".Replace("%", formattingKeys[i]));
                    int end = descriptionText.IndexOf("</%>".Replace("%", formattingKeys[i]));

                    string cardText = descriptionText.Substring(start, end - start + formattingKeys[i].Length + 3);
                    string newCardText = cardText.Replace("x", formattingCodes[formattingKeys[i]].ToString())
                                                 .Replace("<%>".Replace("%", formattingKeys[i]), "")
                                                 .Replace("</%>".Replace("%", formattingKeys[i]), "");

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

            for (int i = 0; i < formattingKeys.Count; i++)
            {
                int start = description.text.IndexOf("<%>".Replace("%", formattingKeys[i]));
                int end = description.text.IndexOf("</%>".Replace("%", formattingKeys[i]));
                if (start != -1)
                    description.text = description.text.Replace(description.text.Substring(start, end - start + 3 + formattingKeys[i].Length), "");
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

    public int GetDynamicNumberOnCard()
    {
        int output = 0;

        if (thisCard.GetCard().description.Contains("<s>"))
        {
            string descriptionText = thisCard.GetCard().description.Replace('|', '\n'); ;
            int startingCheckIndex = 0;
            if (descriptionText.IndexOf("<s>") != -1)
            {
                int s = descriptionText.IndexOf("<s>");

                int percentage = 100;
                int.TryParse(descriptionText.Substring(s + 3, descriptionText.IndexOf("%", startingCheckIndex) - s - 3), out percentage);

                output = Mathf.CeilToInt(transform.parent.GetComponent<CardController>().FindCaster(thisCard.GetCard()).GetComponent<HealthController>().GetAttack() * percentage / 100.0f);
            }
        }
        else
        {
            for (int i = 0; i < thisCard.GetCard().cardEffectName.Length; i++)
            {
                if (thisCard.GetCard().cardEffectName[i] == Card.EffectType.AbsoluteDamage)
                {
                    output = thisCard.GetCard().effectValue[i];
                    break;
                }
                else if (thisCard.GetCard().cardEffectName[i] == Card.EffectType.ArmorDamage || thisCard.GetCard().cardEffectName[i] == Card.EffectType.ArmorDamageAll || thisCard.GetCard().cardEffectName[i] == Card.EffectType.ArmorDamageDivided)
                {
                    output = thisCard.GetCard().effectValue[i];
                    break;
                }
            }

            //If the damage number is 0, and thus calculated from a get effect
            if (output == 0)
            {
                Dictionary<string, int> formattingCodes = GetFormattingCodes();
                List<string> formattingKeys = formattingCodes.Keys.ToList();
                string descriptionText = thisCard.GetCard().description;
                for (int i = 0; i < formattingKeys.Count; i++)
                    if (descriptionText.IndexOf("<%>".Replace("%", formattingKeys[i])) != -1)
                    {
                        output = formattingCodes[formattingKeys[i]];
                        break;
                    }
            }
        }

        return Mathf.Abs(output);
    }

    private Dictionary<string, int> GetFormattingCodes()
    {
        Dictionary<string, int> formattingCodes = new Dictionary<string, int>()
            {
                { "cp" , 0 },
                { "ch", 0},
                { "ba" , 0 },
                { "bh", 0},
                { "ms" , 0 },
                { "es", 0},
                { "dm" , 0 },
                { "l", 0},
                { "d" , 0 },
                { "ar", 0},
            };

        try
        {
            int dm = 0;
            try
            {
                dm = thisCard.FindCaster(thisCard.GetCard()).GetComponent<PlayerMoveController>().GetMovedDistance();
            }
            catch { };

            formattingCodes = new Dictionary<string, int>()
                {
                    { "cp" , TurnController.turnController.GetNumerOfCardsPlayedInTurn() },
                    { "ch", HandController.handController.GetHand().Count},
                    { "ba" , thisCard.FindCaster(thisCard.GetCard()).GetComponent<HealthController>().GetBonusArmor() },
                    { "bh", thisCard.FindCaster(thisCard.GetCard()).GetComponent<HealthController>().GetBonusVit()},
                    { "ms" , TurnController.turnController.GetManaSpent() },
                    { "es", TurnController.turnController.GetEnergySpent()},
                    { "dm" , dm },
                    { "l", ResourceController.resource.GetNumberOfRevivesUsed()},
                    { "d" , GameController.gameController.GetDoomCounter() },
                    { "ar", thisCard.FindCaster(thisCard.GetCard()).GetComponent<HealthController>().GetArmor()},
                };
        }
        catch { }

        return formattingCodes;
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

    public void ClearDisableStats()
    {
        disabledStatusText.text = "";
        disabledStatusText.enabled = false;
    }

    public string GetDisableStats()
    {
        return disabledStatusText.text;
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
            cardFace.material.SetColor("_Color", emissionColor * 0.5f);
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
            cardFace.material.SetFloat("_Dissolve", (9 - i) / 9f);
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
            cardFace.material.SetColor("_Color", emissionColor * 0.5f);
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
            cardFace.material.SetFloat("_Dissolve", i / 9f);
            equipmentIcon.material.SetFloat("_Dissolve", i / 9f);
            equipmentOutline.material.SetFloat("_Dissolve", i / 9f);



            yield return new WaitForSeconds(time / 10f);
        }
    }

    public bool GetIsFacingUp()
    {
        return isFacingUp;
    }
}
