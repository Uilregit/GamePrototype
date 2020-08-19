using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NewGameButton : MonoBehaviour
{
    public Image loadGameButton;

    private Relic thisRelic;

    private void Start()
    {
        if (!InformationLogger.infoLogger.GetHasSaveFile())
        {
            loadGameButton.enabled = false;
            loadGameButton.GetComponent<Collider2D>().enabled = false;
            loadGameButton.transform.GetChild(0).GetComponent<Text>().enabled = false;
        }
    }

    public void StartGame()
    {
        RelicController.relic.AddRelic(thisRelic);
        SceneManager.LoadScene("OverworldScene", LoadSceneMode.Single);
    }

    public void OnMouseDown()
    {
        GetComponent<Collider2D>().enabled = false;
        thisRelic = RelicController.relic.GetRandomRelic();
        RewardsMenuController.rewardsMenu.ShowRelicRewardMenu(thisRelic);
        InformationLogger.infoLogger.SaveGame(true);
    }
}
