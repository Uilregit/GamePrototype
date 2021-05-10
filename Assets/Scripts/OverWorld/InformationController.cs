using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CombatInfo
{
    public int lives = 0;
    public int[] vit = new int[3];
    public int[] maxVit = new int[3];
    public int[] atk = new int[3];
    public int[] armor = new int[3];
    public bool[] deadChars = new bool[3];
}

public class RNG
{
    public uint Get1dNoiseUnit(int positionX, uint seed)
    {
        uint BIT_Noise1 = 0x68E31DA4;
        uint BIT_Noise2 = 0xB5297A4D;
        uint BIT_Noise3 = 0x1B56C4E9;

        uint mangledBits = (uint) positionX;
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

            combatInfo.vit[index] = player.GetComponent<HealthController>().GetCurrentVit();
            combatInfo.maxVit[index] = player.GetComponent<HealthController>().GetMaxVit();
            combatInfo.atk[index] = player.GetComponent<HealthController>().GetStartingAttack();
            combatInfo.armor[index] = player.GetComponent<HealthController>().GetStartingArmor();

            playerColors.Add(player.GetComponent<PlayerController>().GetColorTag());
        }

        for (int i = 0; i < 3; i++)
            if (GameController.gameController.GetDeadChars().Contains(PartyController.party.partyColors[i]))
                combatInfo.deadChars[i] = true;
            else
                combatInfo.deadChars[i] = false;
        ResourceController.resource.LoadLives(combatInfo.lives);
    }

    public void SaveMultiplayerCombatInformation()
    {
        combatInfo.lives = 3;

        List<Card.CasterColor> playerColors = new List<Card.CasterColor>();
        List<GameObject> players = MultiplayerGameController.gameController.GetLivingPlayers();
        for (int i = 0; i < 3; i ++)
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
    }

    public int GetCurrentVit(Card.CasterColor color)
    {
        if (PartyController.party.GetPartyIndex(color) == -1)
            return 0;
        return combatInfo.vit[PartyController.party.GetPartyIndex(color)];
    }

    public int GetMaxVit(Card.CasterColor color)
    {
        return combatInfo.maxVit[PartyController.party.GetPartyIndex(color)];
    }

    public int GetStartingArmor(Card.CasterColor color)
    {
        return combatInfo.armor[PartyController.party.GetPartyIndex(color)];
    }

    public int GetStartingAttack(Card.CasterColor color)
    {
        return combatInfo.atk[PartyController.party.GetPartyIndex(color)];
    }

    public void ChangeCombatInfo(int livesChange, int attackChange, int armorChange, int maxVitChange)
    {
        combatInfo.lives += livesChange;
        ResourceController.resource.LoadLives(combatInfo.lives);
        for (int i = 0; i < 3; i++)
        {
            combatInfo.atk[i] += attackChange;
            combatInfo.armor[i] += armorChange;
            combatInfo.vit[i] += maxVitChange;
            combatInfo.maxVit[i] += maxVitChange;
        }
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
        return combatInfo.deadChars[PartyController.party.GetPartyIndex(color)];
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
