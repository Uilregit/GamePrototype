using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingsUIController : MonoBehaviour
{
    public Color selectedColor;
    public Color unselectedColor;
    public Color backgroundColor;

    public Image[] gameSpeedOptions;
    public Image[] screenShakeOptions;
    public Image[] remainingMoveRangeIndicatorOptions;

    private void Awake()
    {
        InformationLogger.infoLogger.LoadSettings();
        ReportGameSpeedOption(SettingsController.settings.GetGameSpeedIndex());
        ReportScreenShakeOption(SettingsController.settings.GetScreenShakeIndex());
        ReportRemainingMoveRangeIndicator(SettingsController.settings.GetRemainingMoveRangeIndicator());
    }

    public void ReportGameSpeedOption(int index)
    {
        SettingsController.settings.SetGameSpeedIndex(index);
        for (int i = 0; i < gameSpeedOptions.Length; i++)
        {
            if (index == i)
            {
                gameSpeedOptions[i].color = selectedColor;
                gameSpeedOptions[i].transform.GetChild(0).GetComponent<Image>().color = selectedColor;
                gameSpeedOptions[i].transform.GetChild(1).GetComponent<Text>().color = selectedColor;
            }
            else
            {
                gameSpeedOptions[i].color = unselectedColor;
                gameSpeedOptions[i].transform.GetChild(0).GetComponent<Image>().color = backgroundColor;
                gameSpeedOptions[i].transform.GetChild(1).GetComponent<Text>().color = unselectedColor;
            }
        }
    }

    public void ReportScreenShakeOption(int index)
    {
        SettingsController.settings.SetScreenShakeIndex(index);
        for (int i = 0; i < gameSpeedOptions.Length; i++)
        {
            if (index == i)
            {
                screenShakeOptions[i].color = selectedColor;
                screenShakeOptions[i].transform.GetChild(0).GetComponent<Image>().color = selectedColor;
                screenShakeOptions[i].transform.GetChild(1).GetComponent<Text>().color = selectedColor;
            }
            else
            {
                screenShakeOptions[i].color = unselectedColor;
                screenShakeOptions[i].transform.GetChild(0).GetComponent<Image>().color = backgroundColor;
                screenShakeOptions[i].transform.GetChild(1).GetComponent<Text>().color = unselectedColor;
            }
        }
    }

    public void ReportRemainingMoveRangeIndicator(bool state)
    {
        SettingsController.settings.SetRemainingMoveRangeIndicator(state);
        for (int i = 0; i < remainingMoveRangeIndicatorOptions.Length; i++)
        {
            if ((!state && i == 0) || (state && i == 1))
            {
                remainingMoveRangeIndicatorOptions[i].color = selectedColor;
                remainingMoveRangeIndicatorOptions[i].transform.GetChild(0).GetComponent<Image>().color = selectedColor;
                remainingMoveRangeIndicatorOptions[i].transform.GetChild(1).GetComponent<Text>().color = selectedColor;
            }
            else
            {
                remainingMoveRangeIndicatorOptions[i].color = unselectedColor;
                remainingMoveRangeIndicatorOptions[i].transform.GetChild(0).GetComponent<Image>().color = backgroundColor;
                remainingMoveRangeIndicatorOptions[i].transform.GetChild(1).GetComponent<Text>().color = unselectedColor;
            }
        }
    }

    public void BackButton()
    {
        InformationLogger.infoLogger.SaveSettings();
        if (StoryModeController.story != null)
        {
            StoryModeController.story.SetMenuBar(true);
            SceneManager.LoadScene("StoryModeScene", LoadSceneMode.Single);
        }
        else
            SceneManager.LoadScene("MainMenuScene", LoadSceneMode.Single);
    }
}
