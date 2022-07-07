using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CombatStatsHighlightController : MonoBehaviour
{
    [Header("Health Bar Colors")]
    public Color healthBarDamageColor;
    public Color healthBarHealingColor;

    [Header("ArmorColors")]
    public Color armorColor;
    public Color armorShadowColor;
    public Color armorBreakColor;
    public Color armorBreakShadowColor;

    [Header("Arrow Text Colors")]
    public Color damageColor;
    public Color buffColor;
    public Color healingColor;

    [Header("Stats Back Colors")]
    public Color redBack;
    public Color redMid;
    public Color redFront;
    public Color redText;
    public Color redOutline;
    public Color redShadow;
    public Color blueBack;
    public Color blueMid;
    public Color blueFront;
    public Color blueText;
    public Color blueOutline;
    public Color blueShadow;

    [Header("Stats Color Images")]
    public List<Image> stat1BackImages;
    public List<Image> stat1MidImages;
    public List<Image> stat1FrontImages;
    public List<Image> stat1ShadowImages;
    public List<Text> stat1Texts;
    public List<Image> stat2BackImages;
    public List<Image> stat2MidImages;
    public List<Image> stat2FrontImages;
    public List<Image> stat2ShadowImages;
    public List<Text> stat2Texts;

    [Header("Armor Sprites")]
    public Sprite armorSprite;
    public Sprite brokenSprite;

    [Header("Statuses")]
    public CanvasGroup[] canvasGroup;
    public GameObject status1Background;
    public SpriteRenderer[] charSprite;
    public Text[] health;
    public Image[] healthBar;
    public Image[] healthDamageBar;
    public Text[] armor;
    public Image[] armorIcon;
    public Text[] attack;
    public Text[] moverange;
    public Text[] passiveTexts;
    public Image[] passiveIcons;
    public Image[] intents;

    [Header("Stacked Counts")]
    public GameObject[] status1CharacterCounts;
    public GameObject[] status2CharacterCounts;
    public GameObject[] statusCharacterCountContainer;
    public GameObject[] stackCount;
    public Text[] stackCountText;

    [Header("Damage Arrow")]
    public Text damageArrowStatus;
    public TextMeshProUGUI damageArrowCalculation;
    public Text damageArrowNumber;
    public Text damageArrowNumberType;
    public Image damageArrowMask;

    [Header("Game Objects")]
    public GameObject[] statsInfo;
    public GameObject damageArrow;

    private Vector3[] statsInfoOriginalPosition = new Vector3[2];
    private Vector3[] statsStacksOriginalPosition = new Vector3[2];
    private Vector3 damageArrowStartingPosition = new Vector3();
    private Vector2 damageArrowMaskStartingWidth = Vector2.zero;
    private HealthController[] statusObjects = new HealthController[2];
    private int[] stackSize = new int[2];

    public enum numberType
    {
        number = 0,
        turn = 1
    }

    private void Awake()
    {
        statsInfoOriginalPosition[0] = statsInfo[0].transform.localPosition;
        statsInfoOriginalPosition[1] = statsInfo[1].transform.localPosition;
        statsStacksOriginalPosition[0] = statusCharacterCountContainer[0].transform.localPosition;
        statsStacksOriginalPosition[1] = statusCharacterCountContainer[1].transform.localPosition;
        damageArrowStartingPosition = damageArrow.transform.localPosition;
        damageArrowMaskStartingWidth = damageArrowMask.rectTransform.sizeDelta;
    }

    public void SetStatus(int index, HealthController obj, Sprite sprite, int currentVit, int maxVit, int damage, int currentArmor, int armorDamage, int currentAttack, int bonusAttack, int currentMoverange, int maxMoverange, bool refresh = true)
    {
        if (damage != 0)
            health[index].text = (currentVit + damage) + " -> " + currentVit;
        else
            health[index].text = currentVit + "/" + maxVit;
        healthBar[index].transform.localScale = new Vector3(Mathf.Clamp((float)currentVit / maxVit, 0, 1), 1, 1);
        if (damage >= 0)
            healthDamageBar[index].transform.localScale = new Vector3(Mathf.Clamp(-(float)damage / currentVit, -1, 0), 1, 1);
        else
            healthDamageBar[index].transform.localScale = new Vector3(-(float)damage / currentVit, 1, 1);
        if (currentVit <= 0)
        {
            healthBar[index].transform.localScale = new Vector3(Mathf.Clamp((float)(currentVit + damage) / maxVit, 0, 1), 1, 1);
            healthDamageBar[index].transform.localScale = new Vector3(1, 1, 1);
        }

        if (damage < 0)
            healthDamageBar[index].color = healthBarHealingColor;
        else
            healthDamageBar[index].color = healthBarDamageColor;

        armor[index].text = currentArmor.ToString();
        if (armorDamage != 0)
            armor[index].text = (currentArmor + armorDamage) + " -> " + currentArmor.ToString();
        armorIcon[index].sprite = armorSprite;
        armorIcon[index].color = armorColor;
        armorIcon[index].GetComponent<Shadow>().effectColor = armorShadowColor;
        if (currentArmor == 0)
        {
            armor[index].text = "BREAK";
            armorIcon[index].sprite = brokenSprite;
            armorIcon[index].color = armorBreakColor;
            armorIcon[index].GetComponent<Shadow>().effectColor = armorBreakShadowColor;
        }

        attack[index].text = currentAttack.ToString();
        if (bonusAttack > 0)
            attack[index].text += "+" + bonusAttack;

        moverange[index].text = currentMoverange + "/" + maxMoverange;

        passiveIcons[0].enabled = false;
        passiveIcons[1].enabled = false;
        passiveTexts[0].enabled = false;
        passiveTexts[1].enabled = false;

        try
        {
            int counter = 0;
            foreach (Equipment e in obj.GetComponent<PlayerController>().GetEquippedEquipments())
            {
                passiveIcons[counter].enabled = true;
                passiveTexts[counter].enabled = true;
                passiveTexts[counter].text = e.equipmentName;
                counter++;
                if (counter >= passiveIcons.Length)
                    break;
            }
        }
        catch
        {
            int counter = 0;
            foreach (string abilityName in obj.GetAbilityController().GetAbilityNames())
            {
                passiveIcons[counter].enabled = true;
                passiveTexts[counter].enabled = true;
                passiveTexts[counter].text = abilityName;
                counter++;
                if (counter >= passiveIcons.Length)
                    break;
            }
        }

        charSprite[index].sprite = sprite;

        if (obj != statusObjects[index])
        {
            if (refresh)
            {
                StartCoroutine(SlideInStatus(index));
            }
            else
            {
                ScrubStatus(index, true);
            }
        }

        try
        {
            Image enemyIntent;
            if (index == 0)
                enemyIntent = obj.GetComponent<EnemyInformationController>().GetIntent();
            else
                enemyIntent = obj.GetOriginalSimulationTarget().GetComponent<EnemyInformationController>().GetIntent();
            intents[index].gameObject.SetActive(obj.GetVit() > 0);
            intents[index].transform.GetChild(0).localScale = enemyIntent.transform.GetChild(0).localScale;
            intents[index].transform.GetChild(0).gameObject.SetActive(enemyIntent.transform.GetChild(3).GetComponent<Text>().text != "");
            intents[index].transform.GetChild(0).GetComponent<Image>().enabled = enemyIntent.transform.GetChild(0).GetComponent<Image>().enabled;
            intents[index].transform.GetChild(1).GetComponent<Image>().color = enemyIntent.transform.GetChild(1).GetComponent<Image>().color;
            intents[index].transform.GetChild(2).GetComponent<Image>().sprite = enemyIntent.transform.GetChild(2).GetComponent<Image>().sprite;
            intents[index].transform.GetChild(3).GetComponent<Text>().text = enemyIntent.transform.GetChild(3).GetComponent<Text>().text;
            intents[index].transform.GetChild(3).GetComponent<Text>().color = enemyIntent.transform.GetChild(3).GetComponent<Text>().color;
            intents[index].transform.GetChild(4).gameObject.SetActive(enemyIntent.transform.GetChild(4).gameObject.active);
        }
        catch
        {
            intents[index].gameObject.SetActive(false);
        }

        //Coloring the statuses to blue for players, red for enemies
        if (obj.isPlayer)
        {
            if (index == 0)
            {
                foreach (Image img in stat1BackImages)
                    img.color = new Color(blueBack.r, blueBack.g, blueBack.b, img.color.a);
                foreach (Image img in stat1MidImages)
                    img.color = new Color(blueMid.r, blueMid.g, blueMid.b, img.color.a);
                foreach (Image img in stat1FrontImages)
                    img.color = new Color(blueFront.r, blueFront.g, blueFront.b, img.color.a);
                foreach (Image img in stat1ShadowImages)
                    img.GetComponent<Shadow>().effectColor = blueOutline;
                foreach (Text text in stat1Texts)
                {
                    text.color = blueText;
                    text.GetComponent<Outline>().effectColor = blueOutline;
                    text.transform.GetComponent<Shadow>().effectColor = blueShadow;
                }
            }
            else
            {
                foreach (Image img in stat2BackImages)
                    img.color = new Color(blueBack.r, blueBack.g, blueBack.b, img.color.a);
                foreach (Image img in stat2MidImages)
                    img.color = new Color(blueMid.r, blueMid.g, blueMid.b, img.color.a);
                foreach (Image img in stat2FrontImages)
                    img.color = new Color(blueFront.r, blueFront.g, blueFront.b, img.color.a);
                foreach (Image img in stat2ShadowImages)
                    img.GetComponent<Shadow>().effectColor = blueOutline;
                foreach (Text text in stat2Texts)
                {
                    text.color = blueText;
                    text.GetComponent<Outline>().effectColor = blueOutline;
                    text.transform.GetComponent<Shadow>().effectColor = blueShadow;
                }
            }
        }
        else
        {
            if (index == 0)
            {
                foreach (Image img in stat1BackImages)
                    img.color = new Color(redBack.r, redBack.g, redBack.b, img.color.a);
                foreach (Image img in stat1MidImages)
                    img.color = new Color(redMid.r, redMid.g, redMid.b, img.color.a);
                foreach (Image img in stat1FrontImages)
                    img.color = new Color(redFront.r, redFront.g, redFront.b, img.color.a);
                foreach (Image img in stat1ShadowImages)
                    img.GetComponent<Shadow>().effectColor = redOutline;
                foreach (Text text in stat1Texts)
                {
                    text.color = redText;
                    text.GetComponent<Outline>().effectColor = redOutline;
                    text.transform.GetComponent<Shadow>().effectColor = redShadow;
                }
            }
            else
            {
                foreach (Image img in stat2BackImages)
                    img.color = new Color(redBack.r, redBack.g, redBack.b, img.color.a);
                foreach (Image img in stat2MidImages)
                    img.color = new Color(redMid.r, redMid.g, redMid.b, img.color.a);
                foreach (Image img in stat2FrontImages)
                    img.color = new Color(redFront.r, redFront.g, redFront.b, img.color.a);
                foreach (Image img in stat2ShadowImages)
                    img.GetComponent<Shadow>().effectColor = redOutline;
                foreach (Text text in stat2Texts)
                {
                    text.color = redText;
                    text.GetComponent<Outline>().effectColor = redOutline;
                    text.transform.GetComponent<Shadow>().effectColor = redShadow;
                }
            }
        }

        statusObjects[index] = obj;
    }

    public void SetStatusCharactersCount(int index, int count)
    {
        /*
        if (index == 0)
            for (int i = 0; i < status1CharacterCounts.Length; i++)
                status1CharacterCounts[i].SetActive(i < count - 1);
        else
            for (int i = 0; i < status2CharacterCounts.Length; i++)
                status2CharacterCounts[i].SetActive(i < count - 1);
        */
        stackCount[index].SetActive(count > 1);
        stackCountText[index].text = "1/" + count;
        stackSize[index] = count;
    }

    public void SetStatusCharactersIndex(int index, int count)
    {
        stackCountText[index].text = count + "/" + stackSize[index];
    }

    private IEnumerator SlideInStatus(int index)
    {
        Vector3 offset = new Vector3(-10, 0, 0);
        if (index == 1)
            offset = new Vector3(10, 0, 0);

        for (int i = 0; i < 5; i++)
        {
            statsInfo[index].transform.localPosition = statsInfoOriginalPosition[index] + Vector3.Lerp(offset, Vector3.zero, i / 4f);
            statusCharacterCountContainer[index].transform.localPosition = statsStacksOriginalPosition[index] + Vector3.Lerp(offset, Vector3.zero, i / 4f);
            yield return new WaitForSecondsRealtime(0.1f / 4f);
        }
    }

    public void ScrubStatus(int index, bool scrubForward)
    {
        StartCoroutine(ScrubStatusProcess(index, scrubForward));
    }

    private IEnumerator ScrubStatusProcess(int index, bool scrubForward)
    {
        /*
        for (int i = 0; i < 3; i++)
        {
            canvasGroup[index].alpha = Mathf.Lerp(1, 0, i / 2f);
            charSprite[index].color = Color.Lerp(Color.white, Color.clear, i / 2f);
            yield return new WaitForSecondsRealtime(0.05f / 2f);
        }

        statusCharacterCountContainer[index].transform.localPosition = new Vector3(-0.1f, 0.1f, 0);
        statsInfo[index].SetActive(false);

        for (int i = 0; i < 3; i++)
        {
            statusCharacterCountContainer[index].transform.localPosition = Vector3.Lerp(new Vector3(-0.1f, 0.1f, 0), new Vector3(0, 0, 0), i / 2f);
            yield return new WaitForSecondsRealtime(0.1f / 2f);
        }

        statsInfo[index].transform.localPosition = statsInfoOriginalPosition[index] + new Vector3(0, 1, 0);
        canvasGroup[index].alpha = 1;
        charSprite[index].color = Color.white;
        statsInfo[index].SetActive(true);

        for (int i = 0; i < 3; i++)
        {
            statsInfo[index].transform.localPosition = statsInfoOriginalPosition[index] + Vector3.Lerp(new Vector3(0, 1, 0), new Vector3(0, 0, 0), i / 2f);
            yield return new WaitForSecondsRealtime(0.05f / 2f);
        }
        */
        canvasGroup[index].alpha = 0;
        charSprite[index].color = Color.clear;

        yield return new WaitForSecondsRealtime(0.1f);

        canvasGroup[index].alpha = 1;
        charSprite[index].color = Color.white;
    }

    public void SetArrow(int number, numberType type, string status = "Damage", string cardDamageText = "0", int cardDamage = 0, int armor = 0)
    {
        damageArrowNumber.text = number.ToString();
        if (number == 0)
            damageArrowNumber.text = "---";

        damageArrowStatus.text = status;
        string calculationText = cardDamageText + "<sprite=0> ";
        if (armor > 0)
            calculationText += "- " + armor.ToString() + "<sprite=1>=";
        else
            calculationText += "x 2<sprite=2>=";
        damageArrowCalculation.text = calculationText;

        switch (type)
        {
            case numberType.number:
                damageArrowNumberType.text = "pt(s)";
                if (number > 0)
                    damageArrowNumberType.text = "dmg";
                if (cardDamage <= armor && number == 1)
                    damageArrowNumberType.text = "min dmg";
                damageArrowNumber.color = damageColor;
                damageArrowStatus.color = damageColor;
                damageArrowNumberType.color = damageColor;
                damageArrowCalculation.gameObject.SetActive(number > 0 && UIRevealController.UIReveal.GetElementState(UIRevealController.UIElement.Armor));
                damageArrowStatus.gameObject.SetActive(number <= 0 || !UIRevealController.UIReveal.GetElementState(UIRevealController.UIElement.Armor));
                break;
            case numberType.turn:
                damageArrowNumberType.text = "trn(s)";
                damageArrowNumber.color = buffColor;
                damageArrowStatus.color = buffColor;
                damageArrowNumberType.color = buffColor;
                damageArrowCalculation.gameObject.SetActive(false);
                damageArrowStatus.gameObject.SetActive(true);
                break;
        }

        if (status == "Damage" && number < 0)
        {
            damageArrowStatus.text = "Healing";
            damageArrowNumber.text = (-number).ToString();
            damageArrowStatus.color = healingColor;
            damageArrowNumber.color = healingColor;
            damageArrowNumberType.color = healingColor;
            damageArrowCalculation.gameObject.SetActive(false);
            damageArrowStatus.gameObject.SetActive(true);
        }

        if (status == "Damage" && number > 0)
            damageArrowMask.rectTransform.sizeDelta = new Vector2(damageArrowMaskStartingWidth.x * number / cardDamage, damageArrowMaskStartingWidth.y);
        else
            damageArrowMask.rectTransform.sizeDelta = damageArrowMaskStartingWidth;
    }

    public void SetStatusEnabled(int index, bool state)
    {
        if (index == 0)
        {
            statsInfo[index].gameObject.SetActive(true);
            status1Background.SetActive(state);
            charSprite[index].color = Color.white;
            canvasGroup[index].alpha = 0;
            if (state)
                canvasGroup[index].alpha = 1;
        }
        else if (index == 1)
        {
            statsInfo[index].gameObject.SetActive(state);
            status1Background.SetActive(!state);
            canvasGroup[0].alpha = 0;
            if (!state)
                canvasGroup[0].alpha = 1;
        }
        //statusCharacterCountContainer[index].gameObject.SetActive(state);
    }

    public void SetDamageArrowEnabled(bool state)
    {
        if (damageArrow.gameObject.active)
            StartCoroutine(SlideInArrow());
        damageArrow.gameObject.SetActive(state);
    }

    private IEnumerator SlideInArrow()
    {
        Vector3 offset = new Vector3(-5, 0, 0);

        for (int i = 0; i < 5; i++)
        {
            damageArrow.transform.localPosition = damageArrowStartingPosition + Vector3.Lerp(offset, Vector3.zero, i / 4f);
            yield return new WaitForSecondsRealtime(0.1f / 4f);
        }
    }

    public void OnMouseUp()
    {
        ShowCharacterInfo();
    }

    public void ShowCharacterInfo()
    {
        if (!statsInfo[1].gameObject.active && statusObjects[0] != null)
        {
            if (statusObjects[0].isPlayer)
                statusObjects[0].GetComponent<PlayerMoveController>().SetCharacterInfoDescription();
            else
                statusObjects[0].GetComponent<EnemyInformationController>().SetCharacterInfoDescription();
        }
    }
}
