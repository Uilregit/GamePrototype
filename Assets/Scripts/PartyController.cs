using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PartyController : MonoBehaviour
{
    public static PartyController party;

    public Card.CasterColor[] partyColors;

    public Card.CasterColor[] potentialPlayerColors;
    public List<Card.CasterColor> unlockedPlayerColors;
    public Color[] playerImageColors;
    public Color teamColor;
    public Dictionary<Card.CasterColor, List<int>> partyLevelInfo;

    // Start is called before the first frame update
    void Awake()
    {
        if (PartyController.party == null)
            PartyController.party = this;
        else
            Destroy(this.gameObject);

        DontDestroyOnLoad(this.gameObject);

        partyLevelInfo = new Dictionary<Card.CasterColor, List<int>>();

        unlockedPlayerColors = potentialPlayerColors.ToList();
    }

    public void ResolveUnlockedColors()
    {
        Unlocks unlocked = UnlocksController.unlock.GetUnlocks();
        if (!InformationLogger.infoLogger.debug)
        {
            if (!unlocked.blackUnlocked)
                unlockedPlayerColors.Remove(Card.CasterColor.Black);
            if (!unlocked.whiteUnlocked)
                unlockedPlayerColors.Remove(Card.CasterColor.White);
            if (!unlocked.orangeUnlocked)
                unlockedPlayerColors.Remove(Card.CasterColor.Orange);
        }
    }

    public Color GetPlayerColor(Card.CasterColor caster)
    {
        if (caster == Card.CasterColor.Enemy)
            return teamColor;
        return playerImageColors[Array.FindIndex(potentialPlayerColors, x => x == caster)];
    }

    public string[] GetPlayerColorTexts()
    {
        string[] output = new string[3];

        for (int i = 0; i < 3; i++)
            output[i] = GetPlayerColorText(partyColors[i]);

        return output;
    }

    public string[] GetPotentialPlayerTexts()
    {
        string[] output = new string[potentialPlayerColors.Length];

        for(int i = 0; i < potentialPlayerColors.Length; i ++)
            output[i] = GetPlayerColorText(potentialPlayerColors[i]);

        return output;
    }

    public string GetPlayerColorText(Card.CasterColor color)
    {
        string output = "";

        switch (color)
        {
            case Card.CasterColor.Red:
                output = "Red";
                break;
            case Card.CasterColor.Blue:
                output = "Blue";
                break;
            case Card.CasterColor.Green:
                output = "Green";
                break;
            case Card.CasterColor.Orange:
                output = "Orange";
                break;
            case Card.CasterColor.White:
                output = "White";
                break;
            case Card.CasterColor.Black:
                output = "Black";
                break;
        }

        return output;
    }

    public Card.CasterColor GetPlayerCasterColor(string color)
    {
        Card.CasterColor output = Card.CasterColor.Enemy;
        switch (color)
        {
            case "Red":
                output = Card.CasterColor.Red;
                break;
            case "Blue":
                output = Card.CasterColor.Blue;
                break;
            case "Green":
                output = Card.CasterColor.Green;
                break;
            case "Orange":
                output = Card.CasterColor.Orange;
                break;
            case "White":
                output = Card.CasterColor.White;
                break;
            case "Black":
                output = Card.CasterColor.Black;
                break;
        }
        return output;
    }

    public void SetPlayerColors(string[] colors)
    {
        for (int i = 0; i < 3; i++)
            partyColors[i] = GetPlayerCasterColor(colors[i]);
    }

    public int GetPartyIndex(Card.CasterColor caster)
    {
        return Array.FindIndex(partyColors, x => x == caster);
    }

    public void SetPartyLevelInfo(Card.CasterColor color, int level, int currentExp)
    {
        partyLevelInfo[color] = new List<int>() { level, currentExp };
    }

    public List<int> GetPartyLevelInfo(Card.CasterColor color)
    {
        return partyLevelInfo[color];
    }
}
