using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PatchSceneController : MonoBehaviour
{
    public void GoToMainMenu()
    {
        MusicController.music.PlaySFX(MusicController.music.uiUseLowSFX[Random.Range(0, MusicController.music.uiUseLowSFX.Count)]);
        MusicController.music.SetHighPassFilter(false);
        SceneManager.LoadScene("MainMenuScene");
    }
}
