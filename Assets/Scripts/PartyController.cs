using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PartyController : MonoBehaviour
{
    public static PartyController party;

    public Card.CasterColor[] partyColors;
    private Card.CasterColor[] backupPartyColors;

    public Card.CasterColor[] potentialPlayerColors;
    public List<Card.CasterColor> unlockedPlayerColors;
    public Color[] playerImageColors;
    public Color teamColor;
    public Dictionary<Card.CasterColor, List<int>> partyLevelInfo;

    [SerializeField] private string redPlayerName;
    [SerializeField] private string bluePlayerName;
    [SerializeField] private string greenPlayerName;
    [SerializeField] private string orangePlayerName;
    [SerializeField] private string whitePlayerName;
    [SerializeField] private string blackPlayerName;

    [SerializeField] private Sprite redPlayerSprites;
    [SerializeField] private Sprite redPlayerSplashImages;

    [SerializeField] private Sprite bluePlayerSprites;
    [SerializeField] private Sprite bluePlayerSplashImages;

    [SerializeField] private Sprite greenPlayerSprites;
    [SerializeField] private Sprite greenPlayerSplashImages;

    [SerializeField] private Sprite orangePlayerSprites;
    [SerializeField] private Sprite orangePlayerSplashImages;

    [SerializeField] private Sprite whitePlayerSprites;
    [SerializeField] private Sprite whitePlayerSplashImages;

    [SerializeField] private Sprite blackPlayerSprites;
    [SerializeField] private Sprite blackPlayerSplashImages;

    [SerializeField] private Sprite errorSprite;

    [SerializeField] private int redAtk;
    [SerializeField] private int redArmor;
    [SerializeField] private int redHealth;

    [SerializeField] private int blueAtk;
    [SerializeField] private int blueArmor;
    [SerializeField] private int blueHealth;

    [SerializeField] private int greenAtk;
    [SerializeField] private int greenArmor;
    [SerializeField] private int greenHealth;

    [SerializeField] private int orangeAtk;
    [SerializeField] private int orangeArmor;
    [SerializeField] private int orangeHealth;

    [SerializeField] private int whiteAtk;
    [SerializeField] private int whiteArmor;
    [SerializeField] private int whiteHealth;

    [SerializeField] private int blackAtk;
    [SerializeField] private int blackArmor;
    [SerializeField] private int blackHealth;

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
        if (StoryModeController.story != null && (unlocked.blackUnlocked || unlocked.whiteUnlocked || unlocked.orangeUnlocked))
            StoryModeController.story.EnableMenuIcon(1);
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

    public List<Card.CasterColor> GetPlayerCasterColors()
    {
        return partyColors.ToList();
    }

    public string[] GetPotentialPlayerTexts()
    {
        string[] output = new string[potentialPlayerColors.Length];

        for (int i = 0; i < potentialPlayerColors.Length; i++)
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
        if (partyLevelInfo.ContainsKey(color))
            return partyLevelInfo[color];
        else
            return new List<int> { 0, 0 };
    }

    public string GetPartyString()
    {
        return GetPlayerColorText(partyColors[0]) + "|" + GetPlayerColorText(partyColors[1]) + "|" + GetPlayerColorText(partyColors[2]);
    }

    public int GetStartingAttack(Card.CasterColor color)
    {
        switch (color)
        {
            case Card.CasterColor.Red:
                return redAtk;
            case Card.CasterColor.Blue:
                return blueAtk;
            case Card.CasterColor.Green:
                return greenAtk;
            case Card.CasterColor.Orange:
                return orangeAtk;
            case Card.CasterColor.Black:
                return blackAtk;
            case Card.CasterColor.White:
                return whiteAtk;
        }
        return -1;
    }

    public int GetStartingArmor(Card.CasterColor color)
    {
        switch (color)
        {
            case Card.CasterColor.Red:
                return redArmor;
            case Card.CasterColor.Blue:
                return blueArmor;
            case Card.CasterColor.Green:
                return greenArmor;
            case Card.CasterColor.Orange:
                return orangeArmor;
            case Card.CasterColor.Black:
                return blackArmor;
            case Card.CasterColor.White:
                return whiteArmor;
        }
        return -1;
    }

    public int GetStartingHealth(Card.CasterColor color)
    {
        switch (color)
        {
            case Card.CasterColor.Red:
                return redHealth;
            case Card.CasterColor.Blue:
                return blueHealth;
            case Card.CasterColor.Green:
                return greenHealth;
            case Card.CasterColor.Orange:
                return orangeHealth;
            case Card.CasterColor.Black:
                return blackHealth;
            case Card.CasterColor.White:
                return whiteHealth;
        }
        return -1;
    }

    public Sprite GetPlayerSprite(Card.CasterColor color)
    {
        switch (color)
        {
            case Card.CasterColor.Red:
                return redPlayerSprites;
            case Card.CasterColor.Blue:
                return bluePlayerSprites;
            case Card.CasterColor.Green:
                return greenPlayerSprites;
            case Card.CasterColor.Orange:
                return orangePlayerSprites;
            case Card.CasterColor.Black:
                return blackPlayerSprites;
            case Card.CasterColor.White:
                return whitePlayerSprites;
        }
        return errorSprite;
    }

    public Sprite GetPlayerSplashImage(Card.CasterColor color)
    {
        switch (color)
        {
            case Card.CasterColor.Red:
                return redPlayerSplashImages;
            case Card.CasterColor.Blue:
                return bluePlayerSplashImages;
            case Card.CasterColor.Green:
                return greenPlayerSplashImages;
            case Card.CasterColor.Orange:
                return orangePlayerSplashImages;
            case Card.CasterColor.Black:
                return blackPlayerSplashImages;
            case Card.CasterColor.White:
                return whitePlayerSplashImages;
        }
        return errorSprite;
    }

    public string GetPlayerName(Card.CasterColor color)
    {
        switch (color)
        {
            case Card.CasterColor.Red:
                return redPlayerName;
            case Card.CasterColor.Blue:
                return bluePlayerName;
            case Card.CasterColor.Green:
                return greenPlayerName;
            case Card.CasterColor.Orange:
                return orangePlayerName;
            case Card.CasterColor.White:
                return whitePlayerName;
            case Card.CasterColor.Black:
                return blackPlayerName;
        }
        return "error";
    }

    public void SetOverrideParty(bool state)
    {
        Debug.Log("override party called");
        if (state)
            SetOverrideParty(RoomController.roomController.GetCurrentRoomSetup().overrideParty);
        else
            partyColors = backupPartyColors;
    }

    public void SetOverrideParty(Card.CasterColor[] colors)
    {
        backupPartyColors = partyColors;
        partyColors = new Card.CasterColor[3];
        for (int i = 0; i < colors.Length; i++)
            partyColors[i] = colors[i];

        List<Card.CasterColor> reserves = new List<Card.CasterColor> { Card.CasterColor.Red, Card.CasterColor.Blue, Card.CasterColor.Green, Card.CasterColor.Orange, Card.CasterColor.White, Card.CasterColor.Black };
        for (int i = colors.Length; i < 3; i++)
        {
            foreach (Card.CasterColor color in reserves)
                if (!partyColors.Contains(color))
                {
                    partyColors[i] = color;
                    continue;
                }
        }
    }
}
