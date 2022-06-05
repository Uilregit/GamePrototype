using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CombatStatsHighlightController : MonoBehaviour
{
    public Color damageColor;
    public Color buffColor;

    public Sprite armorSprite;
    public Sprite brokenSprite;

    public SpriteRenderer[] charSprite;
    public Text[] health;
    public Image[] healthBar;
    public Image[] healthDamageBar;
    public Text[] armor;
    public Image[] armorIcon;
    public Text[] attack;
    public Text[] moverange;

    public GameObject[] status2CharacterCounts;

    public Text damageArrowStatus;
    public Text damageArrowNumber;
    public Text damageArrowNumberType;

    public GameObject[] statsInfo;
    public GameObject damageArrow;

    private Vector3[] statsInfoOriginalPosition = new Vector3[2];
    private HealthController[] statusObjects = new HealthController[2];

    public enum numberType
    {
        number = 0,
        turn = 1
    }

    private void Awake()
    {
        statsInfoOriginalPosition[0] = statsInfo[0].transform.localPosition;
        statsInfoOriginalPosition[1] = statsInfo[1].transform.localPosition;
    }

    public void SetStatus(int index, HealthController obj, Sprite sprite, int currentVit, int maxVit, int damage, int currentArmor, int armorDamage, int currentAttack, int bonusAttack, int currentMoverange, int maxMoverange)
    {
        health[index].text = currentVit + "/" + maxVit;
        healthBar[index].transform.localScale = new Vector3(Mathf.Clamp((float)currentVit / maxVit, 0, 1), 1, 1);
        healthDamageBar[index].transform.localScale = new Vector3(-(float)damage / currentVit, 1, 1);
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
        if (obj != statusObjects[index])
        {
            charSprite[index].sprite = sprite;
            StartCoroutine(SlideInStatus(index));
        }
        statusObjects[index] = obj;
    }

    public void SetStatus2CharactersCount(int count)
    {
        for (int i = 0; i < status2CharacterCounts.Length; i++)
            status2CharacterCounts[i].SetActive(i < count - 1);
    }

    private IEnumerator SlideInStatus(int index)
    {
        Vector3 offset = new Vector3(-5, 0, 0);
        if (index == 1)
            offset = new Vector3(5, 0, 0);

        for (int i = 0; i < 5; i++)
        {
            statsInfo[index].transform.localPosition = statsInfoOriginalPosition[index] + Vector3.Lerp(offset, Vector3.zero, i / 4f);
            yield return new WaitForSecondsRealtime(0.1f / 4f);
        }
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
    }

    public void SetStatusEnabled(int index, bool state)
    {
        statsInfo[index].gameObject.SetActive(state);
    }

    public void SetDamageArrowEnabled(bool state)
    {
        damageArrow.gameObject.SetActive(state);
    }
}
