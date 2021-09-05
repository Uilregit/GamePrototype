using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuSceneController : MonoBehaviour
{
    public Image newGameButton;
    public Image loadGameButton;

    private Relic thisRelic;

    private void Start()
    {
        if (!InformationLogger.infoLogger.GetHasSaveFile())
        {
            loadGameButton.enabled = false;
            loadGameButton.GetComponent<Collider2D>().enabled = false;
            loadGameButton.transform.GetChild(0).GetComponent<Text>().enabled = false;
            loadGameButton.transform.GetChild(1).GetComponent<Image>().enabled = false;
            loadGameButton.transform.GetChild(2).GetComponent<Image>().enabled = false;
            loadGameButton.transform.GetChild(3).GetComponent<Image>().enabled = false;
        }
        else
        {
            //newGameButton.enabled = false;
            string[] partyColors = InformationLogger.infoLogger.GetLoadPartyColors();
            loadGameButton.transform.GetChild(1).GetComponent<Image>().color = PartyController.party.GetPlayerColor(PartyController.party.GetPlayerCasterColor(partyColors[0]));
            loadGameButton.transform.GetChild(2).GetComponent<Image>().color = PartyController.party.GetPlayerColor(PartyController.party.GetPlayerCasterColor(partyColors[1]));
            loadGameButton.transform.GetChild(3).GetComponent<Image>().color = PartyController.party.GetPlayerColor(PartyController.party.GetPlayerCasterColor(partyColors[2]));
        }
    }

    public void StartGame()     //Used by relic menu button
    {
        RelicController.relic.AddRelic(thisRelic);
        ScoreController.score.SetTimerPaused(false);

        SceneManager.LoadScene("OverworldScene", LoadSceneMode.Single);

        InformationLogger.infoLogger.SaveGameScoreInfo(InformationLogger.infoLogger.patchID,
                                                InformationLogger.infoLogger.gameID,
                                                "0",
                                                "0",
                                                "Start of game",
                                                "false",
                                                "false",
                                                "0",
                                                "0",
                                                "0",
                                                "0",
                                                "0",
                                                "0",
                                                "0",
                                                "0",
                                                "0",
                                                "0");
    }

    public void NewGameButton()
    {
        thisRelic = RelicController.relic.GetRandomRelic();
        RewardsMenuController.rewardsMenu.ShowRelicRewardMenu(thisRelic);
        InformationLogger.infoLogger.SaveGame(true);
        InformationLogger.infoLogger.SavePlayerPreferences();
    }

    public void SettingsButton()
    {
        SceneManager.LoadScene("SettingsScene", LoadSceneMode.Single);
    }

    public void LoadGameButton()
    {
        InformationLogger.infoLogger.StartGameAndLoad();
    }

    public void MultiplayerButton()
    {
        SceneManager.LoadScene("MultiplayerSetupScene", LoadSceneMode.Single);
    }

    public void PatchNotesButton()
    {
        SceneManager.LoadScene("PatchNotesScene", LoadSceneMode.Single);
    }

    public void StoryMode()
    {
        InformationLogger.infoLogger.isStoryMode = true;
        SceneManager.LoadScene("StoryModeScene", LoadSceneMode.Single);
    }
}
