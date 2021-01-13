using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class BattlePassController : MonoBehaviour
{
    public Image back;
    public Text heroName;
    public Text levelText;
    public Text expNumber;
    public Image expBar;
    public GameObject unlockPrefab;
    public GameObject unlockParent;

    private List<GameObject> unlockIcons = new List<GameObject>();

    private Vector2 offset;
    private float parentLeftMostPosition;
    private List<float> unlockLocations = new List<float>();

    public void SetBattlePass(Card.CasterColor hero)
    {
        string name = "";
        int level = 0;
        int numerator = 0;
        int denominator = 0;
        UnlocksController.UnlockTypes[] unlocks;
        Color color = PartyController.party.GetPlayerColor(hero);

        if (hero == Card.CasterColor.Enemy)
        {
            name = "Party";
            level = ScoreController.score.teamLevel;
            numerator = ScoreController.score.currentEXP;
            denominator = ScoreController.score.GetTeamEXPNeededToLevel(level);
            unlocks = UnlocksController.unlock.teamUnlockRewards;
        }
        else
        {
            name = hero.ToString();
            level = PartyController.party.GetPartyLevelInfo(hero)[0];
            numerator = PartyController.party.GetPartyLevelInfo(hero)[1];
            denominator = ScoreController.score.GetHeroEXPNeededToLevel(level);
            unlocks = UnlocksController.unlock.heroUnlockRewards;
        }

        float h, s, v;
        Color.RGBToHSV(color, out h, out s, out v);
        back.color = Color.HSVToRGB(h, 0.15f, v);
        heroName.text = name;
        levelText.text = "Lv." + level.ToString();
        expNumber.text = numerator.ToString() + "/" + denominator.ToString();
        expBar.transform.localScale = new Vector2((float)numerator / denominator, 1);
        expBar.color = color;

        float xLoc = -2.4f;
        float defaultxLoc = 0;
        float unlocxLoc = 0;
        for (int i = 0; i < unlocks.Length; i++)
        {
            unlockIcons.Add(Instantiate(unlockPrefab));
            unlockIcons[i].transform.SetParent(unlockParent.transform);
            unlockIcons[i].transform.localPosition = new Vector2(xLoc, -0.8f);

            Color backgroundColor = color;
            if (i >= level)
                backgroundColor = color * new Color(0.3f, 0.3f, 0.3f);

            unlockIcons[i].GetComponent<Image>().color = backgroundColor;

            if (unlocks[i] != UnlocksController.UnlockTypes.None)
            {
                unlockIcons[i].GetComponent<Image>().transform.localScale = new Vector2(1, 1);
                unlockIcons[i].transform.GetChild(1).GetComponent<Image>().color = backgroundColor;
                unlockIcons[i].transform.GetChild(0).GetComponent<Text>().text = (i + 1).ToString();
                unlockIcons[i].transform.GetChild(2).GetComponent<Image>().sprite = UnlocksController.unlock.GetRewardArt(unlocks[i]);
                unlockIcons[i].transform.GetChild(3).GetComponent<Text>().text = unlocks[i].ToString();

                unlockIcons[i].transform.GetChild(1).GetComponent<Image>().enabled = true;
                unlockIcons[i].transform.GetChild(0).GetComponent<Text>().enabled = true;
                unlockIcons[i].transform.GetChild(2).GetComponent<Image>().enabled = true;
                unlockIcons[i].transform.GetChild(3).GetComponent<Text>().enabled = true;
                xLoc += 0.85f;

                unlockLocations.Add(unlocxLoc);
                unlocxLoc += 0.85f;
                if (i < level - 1)
                    defaultxLoc -= 0.85f;
            }
            else
            {
                unlockIcons[i].GetComponent<Image>().transform.localScale = new Vector2(0.125f, 1);

                unlockIcons[i].transform.GetChild(1).GetComponent<Image>().enabled = false;
                unlockIcons[i].transform.GetChild(0).GetComponent<Text>().enabled = false;
                unlockIcons[i].transform.GetChild(2).GetComponent<Image>().enabled = false;
                unlockIcons[i].transform.GetChild(3).GetComponent<Text>().enabled = false;
                xLoc += 0.15f;

                unlockLocations.Add(unlocxLoc);
                unlocxLoc += 0.15f;
                if (i < level - 1)
                    defaultxLoc -= 0.15f;
            }
        }

        parentLeftMostPosition = unlockParent.transform.position.x;
        unlockParent.transform.localPosition = new Vector2(defaultxLoc, unlockParent.transform.localPosition.y);
    }

    public void OnMouseDown()
    {
        offset = unlockParent.transform.position - Input.mousePosition;
    }

    public void OnMouseDrag()
    {
        unlockParent.transform.position = new Vector2(Mathf.Min(parentLeftMostPosition, offset.x + Input.mousePosition.x), unlockParent.transform.position.y);
    }

    public void OnMouseUp()
    {
        int index = 0;
        float minDistance = 999999;
        for (int i = 0; i < unlockLocations.Count; i++)
        {
            if (Mathf.Abs(unlockParent.transform.localPosition.x + unlockLocations[i]) < minDistance)
            {
                index = i;
                minDistance = Mathf.Abs(unlockParent.transform.localPosition.x + unlockLocations[i]);
            }
        }
        unlockParent.transform.localPosition = new Vector2(-unlockLocations[index], unlockParent.transform.localPosition.y);
    }
}
