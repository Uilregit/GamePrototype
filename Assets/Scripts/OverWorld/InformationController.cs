using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CombatInfo
{
    public int lives = 1;
    public int[] vit = new int[3];
    public int[] maxVit = new int[3];
    public int[] atk = new int[3];
    public int[] armor = new int[3];
    public bool[] deadChars = new bool[3];
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
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            int index = PartyController.party.GetPartyIndex(player.GetComponent<PlayerController>().GetColorTag());
            if (index == -1)
                continue;

            combatInfo.vit[index] = player.GetComponent<HealthController>().GetCurrentVit();
            combatInfo.maxVit[index] = player.GetComponent<HealthController>().GetMaxVit();
            combatInfo.atk[index] = player.GetComponent<HealthController>().GetStartingAttack();
            combatInfo.armor[index] = player.GetComponent<HealthController>().GetStartingShield();

            playerColors.Add(player.GetComponent<PlayerController>().GetColorTag());
        }

        for (int i = 0; i < 3; i++)
            if (GameController.gameController.GetDeadChars().Contains(PartyController.party.partyColors[i]))
            {
                combatInfo.deadChars[i] = true;
                /*
                if (combatInfo.lives > 0)
                {
                    combatInfo.vit[i] = Mathf.CeilToInt(combatInfo.maxVit[i] * 0.5f);
                    combatInfo.lives -= 1;
                }
                else
                    combatInfo.vit[i] = 1;
                */
            }
            else
                combatInfo.deadChars[i] = false;
        ResourceController.resource.LoadLives(combatInfo.lives);
    }

    public int GetCurrentVit(Card.CasterColor color)
    {
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
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
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
