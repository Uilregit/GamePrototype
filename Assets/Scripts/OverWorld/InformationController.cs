using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CombatInfo
{
    public int redVit = 0;
    public int blueVit = 0;
    public int greenVit = 0;
    public int redMaxVit = 0;
    public int blueMaxVit = 0;
    public int greenMaxVit = 0;
    public int redAtk = 0;
    public int blueAtk = 0;
    public int greenAtk = 0;
    public int redArmor = 0;
    public int blueArmor = 0;
    public int greenArmor = 0;
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
            if (player.GetComponent<PlayerController>().GetColorTag() == Card.CasterColor.Red)
            {
                combatInfo.redVit = player.GetComponent<HealthController>().GetCurrentVit();
                combatInfo.redMaxVit = player.GetComponent<HealthController>().GetMaxVit();
                combatInfo.redAtk = player.GetComponent<HealthController>().GetStartingAttack();
                combatInfo.redArmor = player.GetComponent<HealthController>().GetStartingShield();
                playerColors.Add(Card.CasterColor.Red);
            }
            else if (player.GetComponent<PlayerController>().GetColorTag() == Card.CasterColor.Blue)
            {
                combatInfo.blueVit = player.GetComponent<HealthController>().GetCurrentVit();
                combatInfo.blueMaxVit = player.GetComponent<HealthController>().GetMaxVit();
                combatInfo.blueAtk = player.GetComponent<HealthController>().GetStartingAttack();
                combatInfo.blueArmor = player.GetComponent<HealthController>().GetStartingShield();
                playerColors.Add(Card.CasterColor.Blue);
            }
            else if (player.GetComponent<PlayerController>().GetColorTag() == Card.CasterColor.Green)
            {
                combatInfo.greenVit = player.GetComponent<HealthController>().GetCurrentVit();
                combatInfo.greenMaxVit = player.GetComponent<HealthController>().GetMaxVit();
                combatInfo.greenAtk = player.GetComponent<HealthController>().GetStartingAttack();
                combatInfo.greenArmor = player.GetComponent<HealthController>().GetStartingShield();
                playerColors.Add(Card.CasterColor.Green);
            }
        }
        if (players.Length != 0)
        {
            if (!playerColors.Contains(Card.CasterColor.Red))
                combatInfo.redVit = 1;
            if (!playerColors.Contains(Card.CasterColor.Blue))
                combatInfo.blueVit = 1;
            if (!playerColors.Contains(Card.CasterColor.Green))
                combatInfo.greenVit = 1;
        }
    }

    public int GetCurrentVit(Card.CasterColor color)
    {
        if (color == Card.CasterColor.Red)
            return combatInfo.redVit;
        if (color == Card.CasterColor.Blue)
            return combatInfo.blueVit;
        if (color == Card.CasterColor.Green)
            return combatInfo.greenVit;
        return 0;
    }

    public int GetMaxVit(Card.CasterColor color)
    {
        if (color == Card.CasterColor.Red)
            return combatInfo.redMaxVit;
        if (color == Card.CasterColor.Blue)
            return combatInfo.blueMaxVit;
        if (color == Card.CasterColor.Green)
            return combatInfo.greenMaxVit;
        return 0;
    }

    public int GetStartingArmor(Card.CasterColor color)
    {
        if (color == Card.CasterColor.Red)
            return combatInfo.redArmor;
        if (color == Card.CasterColor.Blue)
            return combatInfo.blueArmor;
        if (color == Card.CasterColor.Green)
            return combatInfo.greenArmor;
        return 0;
    }

    public int GetStartingAttack(Card.CasterColor color)
    {
        if (color == Card.CasterColor.Red)
            return combatInfo.redAtk;
        if (color == Card.CasterColor.Blue)
            return combatInfo.blueAtk;
        if (color == Card.CasterColor.Green)
            return combatInfo.greenAtk;
        return 0;
    }

    public void ChangeCombatInfo(int attackChange, int armorChange, int maxVitChange)
    {
        combatInfo.redAtk += attackChange;
        combatInfo.blueAtk += attackChange;
        combatInfo.greenAtk += attackChange;
        combatInfo.redArmor += armorChange;
        combatInfo.blueArmor += armorChange;
        combatInfo.greenArmor += armorChange;
        combatInfo.redMaxVit += maxVitChange;
        combatInfo.blueMaxVit += maxVitChange;
        combatInfo.greenMaxVit += maxVitChange;
        combatInfo.redVit += maxVitChange;
        combatInfo.blueVit += maxVitChange;
        combatInfo.greenVit += maxVitChange;
    }

    public CombatInfo GetCombatInfo()
    {
        return combatInfo;
    }

    public void SetCombatInfo(CombatInfo value)
    {
        combatInfo = value;
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
