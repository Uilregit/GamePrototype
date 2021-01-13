using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuSceneController : MonoBehaviour
{
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
}
