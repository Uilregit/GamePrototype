using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CombatStatsHighlightController : MonoBehaviour
{
    public Color healthBarDamageColor;
    public Color healthBarHealingColor;

    public Color damageColor;
    public Color buffColor;
    public Color healingColor;

    public Sprite armorSprite;
    public Sprite brokenSprite;

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

    public GameObject[] status1CharacterCounts;
    public GameObject[] status2CharacterCounts;
    public GameObject[] statusCharacterCountContainer;
    public GameObject[] stackCount;
    public Text[] stackCountText;

    public Text damageArrowStatus;
    public Text damageArrowNumber;
    public Text damageArrowNumberType;

    public GameObject[] statsInfo;
    public GameObject damageArrow;

    private Vector3[] statsInfoOriginalPosition = new Vector3[2];
    private Vector3[] statsStacksOriginalPosition = new Vector3[2];
    private Vector3 damageArrowStartingPosition = new Vector3();
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
    }

    public void SetStatus(int index, HealthController obj, Sprite sprite, int currentVit, int maxVit, int damage, int currentArmor, int armorDamage, int currentAttack, int bonusAttack, int currentMoverange, int maxMoverange, bool refresh = true)
    {
        if (damage != 0)
            health[index].text = (currentVit + damage) + " -> " + currentVit;
        else
            health[index].text = currentVit + "/" + maxVit;
        healthBar[index].transform.localScale = new Vector3(Mathf.Clamp((float)currentVit / maxVit, 0, 1), 1, 1);
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
        if (currentArmor == 0)
        {
            armor[index].text = "BROKEN";
            armorIcon[index].sprite = brokenSprite;
        }

        attack[index].text = currentAttack.ToString();
        if (bonusAttack > 0)
            attack[index].text += "+" + bonusAttack;

        moverange[index].text = currentMoverange + "/" + maxMoverange;

        passiveIcons[0].enabled = false;
        passiveIcons[1].enabled = false;
        passiveTexts[0].enabled = false;
        passiveTexts[1].enabled = false;

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

    public void SetArrow(int number, numberType type, string status = "Damage")
    {
        damageArrowNumber.text = number.ToString();
        if (number == 0)
            damageArrowNumber.text = "---";

        damageArrowStatus.text = status;

        switch (type)
        {
            case numberType.number:
                damageArrowNumberType.text = "pt(s)";
                damageArrowNumber.color = damageColor;
                damageArrowStatus.color = damageColor;
                break;
            case numberType.turn:
                damageArrowNumberType.text = "trn(s)";
                damageArrowNumber.color = buffColor;
                damageArrowStatus.color = buffColor;
                break;
        }

        if (status == "Damage" && number < 0)
        {
            damageArrowStatus.text = "Healing";
            damageArrowNumber.text = (-number).ToString();
            damageArrowStatus.color = healingColor;
            damageArrowNumber.color = healingColor;
        }
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
