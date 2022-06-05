using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CombatInfo
{
    public int lives = 0;
    public Dictionary<Card.CasterColor, int> vit = new Dictionary<Card.CasterColor, int> { { Card.CasterColor.Red, -1 }, { Card.CasterColor.Blue, -1 }, { Card.CasterColor.Green, -1 }, { Card.CasterColor.Orange, -1 }, { Card.CasterColor.White, -1 }, { Card.CasterColor.Black, -1 } };
    public Dictionary<Card.CasterColor, int> maxVit = new Dictionary<Card.CasterColor, int> { { Card.CasterColor.Red, -1 }, { Card.CasterColor.Blue, -1 }, { Card.CasterColor.Green, -1 }, { Card.CasterColor.Orange, -1 }, { Card.CasterColor.White, -1 }, { Card.CasterColor.Black, -1 } };
    public Dictionary<Card.CasterColor, int> atk = new Dictionary<Card.CasterColor, int> { { Card.CasterColor.Red, -1 }, { Card.CasterColor.Blue, -1 }, { Card.CasterColor.Green, -1 }, { Card.CasterColor.Orange, -1 }, { Card.CasterColor.White, -1 }, { Card.CasterColor.Black, -1 } };
    public Dictionary<Card.CasterColor, int> armor = new Dictionary<Card.CasterColor, int> { { Card.CasterColor.Red, -1 }, { Card.CasterColor.Blue, -1 }, { Card.CasterColor.Green, -1 }, { Card.CasterColor.Orange, -1 }, { Card.CasterColor.White, -1 }, { Card.CasterColor.Black, -1 } };
    public Dictionary<Card.CasterColor, bool> deadChars = new Dictionary<Card.CasterColor, bool> { { Card.CasterColor.Red, false }, { Card.CasterColor.Blue, false }, { Card.CasterColor.Green, false }, { Card.CasterColor.Orange, false }, { Card.CasterColor.White, false }, { Card.CasterColor.Black, false } };
}

public class RNG
{
    public uint Get1dNoiseUnit(int positionX, uint seed)
    {
        uint BIT_Noise1 = 0x68E31DA4;
        uint BIT_Noise2 = 0xB5297A4D;
        uint BIT_Noise3 = 0x1B56C4E9;

        uint mangledBits = (uint)positionX;
        mangledBits *= BIT_Noise1;
        mangledBits += seed;
        mangledBits ^= (mangledBits >> 8);
        mangledBits += BIT_Noise2;
        mangledBits ^= (mangledBits << 8);
        mangledBits *= BIT_Noise3;
        mangledBits ^= (mangledBits >> 8);
        return mangledBits;
    }

    public uint Get2dNoiseUnit(int positionX, int positionY, uint seed)
    {
        int prime = 198491317;
        return Get1dNoiseUnit(positionX + (prime * positionY), seed);
    }
}

public class InformationController : MonoBehaviour
{
    public static InformationController infoController;
    public bool firstRoom = true;

    private CombatInfo combatInfo;

    // Start is called before the first frame update
    void Awake()
    {
        if (InformationController.infoController == null)
            InformationController.infoController = this;
        else
            Destroy(this.gameObject);

        DontDestroyOnLoad(this.gameObject);

        //Save info for the first iteration in the first room
        combatInfo = new CombatInfo();
    }

    public void SaveCombatInformation()
    {
        firstRoom = false;

        List<Card.CasterColor> playerColors = new List<Card.CasterColor>();
        GameObject[] players = GameController.gameController.GetLivingPlayers().ToArray();
        foreach (GameObject player in players)
        {
            int index = PartyController.party.GetPartyIndex(player.GetComponent<PlayerController>().GetColorTag());
            if (index == -1)
                continue;

            combatInfo.maxVit[player.GetComponent<PlayerController>().GetColorTag()] = player.GetComponent<HealthController>().GetMaxVit();
            combatInfo.vit[player.GetComponent<PlayerController>().GetColorTag()] = player.GetComponent<HealthController>().GetCurrentVit();
            combatInfo.atk[player.GetComponent<PlayerController>().GetColorTag()] = player.GetComponent<HealthController>().GetStartingAttack();
            combatInfo.armor[player.GetComponent<PlayerController>().GetColorTag()] = player.GetComponent<HealthController>().GetStartingArmor();

            playerColors.Add(player.GetComponent<PlayerController>().GetColorTag());
        }

        for (int i = 0; i < 3; i++)
            if (GameController.gameController.GetDeadChars().Contains(PartyController.party.partyColors[i]))
                combatInfo.deadChars[PartyController.party.partyColors[i]] = true;
            else
                combatInfo.deadChars[PartyController.party.partyColors[i]] = false;
        ResourceController.resource.LoadLives(combatInfo.lives);
    }

    public void SaveMultiplayerCombatInformation()
    {
        /*
        combatInfo.lives = 3;

        List<Card.CasterColor> playerColors = new List<Card.CasterColor>();
        List<GameObject> players = MultiplayerGameController.gameController.GetLivingPlayers();
        for (int i = 0; i < 3; i++)
        {
            combatInfo.vit[i] = players[i].GetComponent<HealthController>().GetCurrentVit();
            combatInfo.maxVit[i] = players[i].GetComponent<HealthController>().GetMaxVit();
            combatInfo.atk[i] = players[i].GetComponent<HealthController>().GetStartingAttack();
            combatInfo.armor[i] = players[i].GetComponent<HealthController>().GetStartingArmor();

            playerColors.Add(players[i].GetComponent<MultiplayerPlayerController>().GetColorTag());

            combatInfo.deadChars[i] = false;
        }

        ResourceController.resource.LoadLives(combatInfo.lives);
        ResourceController.resource.GetComponent<Canvas>().enabled = true;
        ResourceController.resource.GetComponent<CanvasScaler>().enabled = false;
        ResourceController.resource.GetComponent<CanvasScaler>().enabled = true;
        */
    }

    public int GetCurrentVit(Card.CasterColor color)
    {
        if (combatInfo.maxVit[color] > 0)
        {
            if (PartyController.party.GetPartyIndex(color) == -1)
                return 0;
            return combatInfo.vit[color];
        }
        return PartyController.party.GetStartingHealth(color);
    }

    public int GetMaxVit(Card.CasterColor color)
    {
        if (combatInfo.maxVit[color] > 0)
            return combatInfo.maxVit[color];
        return PartyController.party.GetStartingHealth(color);
    }

    public int GetStartingArmor(Card.CasterColor color)
    {
        return combatInfo.armor[color];
    }

    public int GetStartingAttack(Card.CasterColor color)
    {
        try
        {
            if (combatInfo.atk[color] <= 0)
                return PartyController.party.GetStartingAttack(color) + CollectionController.collectionController.GetEquipmentAttack(color);
        }
        catch
        {
            return PartyController.party.GetStartingAttack(color) + CollectionController.collectionController.GetEquipmentAttack(color);
        }

        return combatInfo.atk[color];
    }

    public void ChangeCombatInfo(int livesChange, int attackChange, int armorChange, int maxVitChange)
    {
        combatInfo.lives += livesChange;
        ResourceController.resource.LoadLives(combatInfo.lives);
        for (int i = 0; i < 3; i++)
        {
            combatInfo.atk[PartyController.party.partyColors[i]] += attackChange;
            combatInfo.armor[PartyController.party.partyColors[i]] += armorChange;
            combatInfo.vit[PartyController.party.partyColors[i]] += maxVitChange;
            combatInfo.maxVit[PartyController.party.partyColors[i]] += maxVitChange;
        }
    }

    //Used to have story mode combat start with fresh stats, unaffected by old team stats
    public void ResetCombatInfo()
    {
        combatInfo.lives = ResourceController.resource.GetLives();
        combatInfo.atk = new Dictionary<Card.CasterColor, int> { { Card.CasterColor.Red, -1 }, { Card.CasterColor.Blue, -1 }, { Card.CasterColor.Green, -1 }, { Card.CasterColor.Orange, -1 }, { Card.CasterColor.White, -1 }, { Card.CasterColor.Black, -1 } };
        combatInfo.armor = new Dictionary<Card.CasterColor, int> { { Card.CasterColor.Red, -1 }, { Card.CasterColor.Blue, -1 }, { Card.CasterColor.Green, -1 }, { Card.CasterColor.Orange, -1 }, { Card.CasterColor.White, -1 }, { Card.CasterColor.Black, -1 } };
        combatInfo.vit = new Dictionary<Card.CasterColor, int> { { Card.CasterColor.Red, -1 }, { Card.CasterColor.Blue, -1 }, { Card.CasterColor.Green, -1 }, { Card.CasterColor.Orange, -1 }, { Card.CasterColor.White, -1 }, { Card.CasterColor.Black, -1 } };
        combatInfo.maxVit = new Dictionary<Card.CasterColor, int> { { Card.CasterColor.Red, -1 }, { Card.CasterColor.Blue, -1 }, { Card.CasterColor.Green, -1 }, { Card.CasterColor.Orange, -1 }, { Card.CasterColor.White, -1 }, { Card.CasterColor.Black, -1 } };
        combatInfo.deadChars = new Dictionary<Card.CasterColor, bool> { { Card.CasterColor.Red, false }, { Card.CasterColor.Blue, false }, { Card.CasterColor.Green, false }, { Card.CasterColor.Orange, false }, { Card.CasterColor.White, false }, { Card.CasterColor.Black, false } };
    }

    public CombatInfo GetCombatInfo()
    {
        return combatInfo;
    }

    public void SetCombatInfo(CombatInfo value)
    {
        combatInfo = value;
    }

    public bool GetIfDead(Card.CasterColor color)
    {
        return combatInfo.deadChars[color];
    }
    /*
    public void LoadCombatInformation()
    {
        GameObject[] players = GameController.gameController.GetLivingPlayers();
        foreach (GameObject player in players)
        {
            if (player.GetComponent<PlayerController>().GetColorTag() == Card.CasterColor.Red)
                player.GetComponent<HealthController>().SetCurrentVit(combatInfo.redVit);
            else if (player.GetComponent<PlayerController>().GetColorTag() == Card.CasterColor.Blue)
                player.GetComponent<HealthController>().SetCurrentVit(combatInfo.blueVit);
            else if (player.GetComponent<PlayerController>().GetColorTag() == Card.CasterColor.Green)
                player.GetComponent<HealthController>().SetCurrentVit(combatInfo.greenVit);
        }
        Debug.Log("Number of players: " + players.Length);
        Debug.Log("Tried Loading: " + combatInfo.redVit.ToString() + combatInfo.blueVit.ToString() + combatInfo.greenVit.ToString());
    }*/
}
