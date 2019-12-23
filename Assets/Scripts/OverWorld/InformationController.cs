using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CombatInfo
{
    public int redVit = 0;
    public int blueVit = 0;
    public int greenVit = 0;
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
                combatInfo.redVit = player.GetComponent<PlayerController>().GetVit();
                playerColors.Add(Card.CasterColor.Red);
            }
            else if (player.GetComponent<PlayerController>().GetColorTag() == Card.CasterColor.Blue)
            {
                combatInfo.blueVit = player.GetComponent<PlayerController>().GetVit();
                playerColors.Add(Card.CasterColor.Blue);
            }
            else if (player.GetComponent<PlayerController>().GetColorTag() == Card.CasterColor.Green)
            {
                combatInfo.greenVit = player.GetComponent<PlayerController>().GetVit();
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
