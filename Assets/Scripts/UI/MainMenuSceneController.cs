using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuSceneController : MonoBehaviour
{
    public Image newGameButton;
    public Image loadGameButton;
    public Image patchNotification;

    public Image[] visualizersFront;
    public Image[] visualizersBack;

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

        if (!UnlocksController.unlock.GetUnlocks().classicModeUnlocked)
        {
            newGameButton.gameObject.SetActive(false);
            loadGameButton.gameObject.SetActive(false);
        }

        patchNotification.enabled = InformationLogger.infoLogger.GetLastPatchRead() != InformationLogger.infoLogger.patchID;
    }

    private void FixedUpdate()
    {
        float[] amplitudes = MusicController.music.GetBackgroundAmplitude();
        for (int i = 0; i < 8; i++)
        {
            visualizersFront[i].transform.localScale = new Vector2(visualizersFront[i].transform.localScale.x, amplitudes[i]);
            visualizersBack[7 - i].transform.localScale = new Vector2(visualizersBack[7 - i].transform.localScale.x, amplitudes[i]);
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
        MusicController.music.PlaySFX(MusicController.music.uiUseHighSFX);
        thisRelic = RelicController.relic.GetRandomRelic();
        RewardsMenuController.rewardsMenu.ShowRelicRewardMenu(thisRelic);
        InformationLogger.infoLogger.SaveGame(true);
        InformationLogger.infoLogger.SavePlayerPreferences();
    }

    public void SettingsButton()
    {
        MusicController.music.PlaySFX(MusicController.music.uiUseLowSFX[Random.Range(0, MusicController.music.uiUseLowSFX.Count)]);
        SceneManager.LoadScene("SettingsScene", LoadSceneMode.Single);
    }

    public void LoadGameButton()
    {
        MusicController.music.PlaySFX(MusicController.music.uiUseLowSFX[Random.Range(0, MusicController.music.uiUseLowSFX.Count)]);
        InformationLogger.infoLogger.StartGameAndLoad();
    }

    public void MultiplayerButton()
    {
        MusicController.music.PlaySFX(MusicController.music.uiUseLowSFX[Random.Range(0, MusicController.music.uiUseLowSFX.Count)]);
        SceneManager.LoadScene("MultiplayerSetupScene", LoadSceneMode.Single);
    }

    public void PatchNotesButton()
    {
        MusicController.music.PlaySFX(MusicController.music.uiUseLowSFX[Random.Range(0, MusicController.music.uiUseLowSFX.Count)]);
        MusicController.music.SetHighPassFilter(true);
        InformationLogger.infoLogger.SetLastPatchRead(InformationLogger.infoLogger.patchID);
        InformationLogger.infoLogger.SavePlayerPreferences();
        SceneManager.LoadScene("PatchNotesScene", LoadSceneMode.Single);
    }

    public void StoryMode()
    {
        MusicController.music.PlaySFX(MusicController.music.uiUseHighSFX);
        InformationLogger.infoLogger.isStoryMode = true;
        SceneManager.LoadScene("StoryModeScene", LoadSceneMode.Single);
    }
}
