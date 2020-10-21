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
    public Color[] playerImageColors;

    // Start is called before the first frame update
    void Awake()
    {
        if (PartyController.party == null)
            PartyController.party = this;
        else
            Destroy(this.gameObject);

        DontDestroyOnLoad(this.gameObject);
    }

    public Color GetPlayerColor(Card.CasterColor caster)
    {
        return playerImageColors[Array.FindIndex(potentialPlayerColors, x => x == caster)];
    }

    public string[] GetPlayerColorTexts()
    {
        string[] output = new string[3];

        for (int i = 0; i < 3; i++)
        {
            switch (partyColors[i])
            {
                case Card.CasterColor.Red:
                    output[i] = "Red";
                    break;
                case Card.CasterColor.Blue:
                    output[i] = "Blue";
                    break;
                case Card.CasterColor.Green:
                    output[i] = "Green";
                    break;
                case Card.CasterColor.Orange:
                    output[i] = "Orange";
                    break;
                case Card.CasterColor.White:
                    output[i] = "White";
                    break;
                case Card.CasterColor.Black:
                    output[i] = "Black";
                    break;
            }
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
}
