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

    public Image[] backgroundMusicVolumeOptions;
    public Image[] soundEffectsVolumeOptions;
    public Image backgroundMusicVolumeMuteOption;
    public Image soundEffectsVolumeMuteOption;
    public Text backgroundMusicTitle;
    public Text soundEffectsTitle;
    private int backGroundMusicVolume;
    private int soundEffectsVolume;
    private bool backGroundMusicMuted = false;
    private bool soundEffectsMuted = false;

    private bool finishedInitializing = false;

    private void Awake()
    {
        InformationLogger.infoLogger.LoadSettings();

        backGroundMusicVolume = SettingsController.settings.GetBackGroundMusicVolume();
        soundEffectsVolume = SettingsController.settings.GetSoundEffectsVolume();
        ReportBackgroundMusicVolumeOption(SettingsController.settings.GetBackGroundMusicVolume());
        ReportSoundEffectsVolumeOption(SettingsController.settings.GetSoundEffectsVolume());
        backGroundMusicMuted = !SettingsController.settings.GetBackGroundMusicMuted();      //Set to the opposite of the settings so it flips back in ReportBackGroundMusicVolumeMute()
        soundEffectsMuted = !SettingsController.settings.GetSoundEffectsMuted();            //Set to the opposite of the settings so it flips back in ReportSoundEffectsVolumeMute()
        ReportBackgroundMusicVolumeMute();
        ReportSoundEffectsVolumeMute();

        ReportGameSpeedOption(SettingsController.settings.GetGameSpeedIndex());
        ReportScreenShakeOption(SettingsController.settings.GetScreenShakeIndex());
        ReportRemainingMoveRangeIndicator(SettingsController.settings.GetRemainingMoveRangeIndicator());

        finishedInitializing = true;
    }

    public void ReportBackgroundMusicVolumeOption(int index)
    {
        backGroundMusicVolume = index;
        for (int i = 0; i < backgroundMusicVolumeOptions.Length; i++)
            if (i < index)
            {
                backgroundMusicVolumeOptions[i].color = selectedColor;
                backgroundMusicVolumeOptions[i].transform.GetChild(0).GetComponent<Image>().color = selectedColor;
            }
            else
            {
                backgroundMusicVolumeOptions[i].color = unselectedColor;
                backgroundMusicVolumeOptions[i].transform.GetChild(0).GetComponent<Image>().color = backgroundColor;
            }

        if (backGroundMusicMuted)
            ReportBackgroundMusicVolumeMute();

        SettingsController.settings.SetBackgroundMusicVolume(index);
        backgroundMusicTitle.text = "Background Music Volume: " + index * 10 + "%";
    }

    public void ReportBackgroundMusicVolumeMute()
    {
        backGroundMusicMuted = !backGroundMusicMuted;
        if (backGroundMusicMuted)
            backgroundMusicTitle.text = "Background Music Volume: Muted";
        else
            backgroundMusicTitle.text = "Background Music Volume: " + SettingsController.settings.GetBackGroundMusicVolume() * 10 + "%";

        if (backGroundMusicMuted)
        {
            backgroundMusicVolumeMuteOption.color = selectedColor;
            backgroundMusicVolumeMuteOption.transform.GetChild(0).GetComponent<Image>().color = selectedColor;
            backgroundMusicVolumeMuteOption.transform.GetChild(1).GetComponent<Text>().color = selectedColor;
        }
        else
        {
            backgroundMusicVolumeMuteOption.color = unselectedColor;
            backgroundMusicVolumeMuteOption.transform.GetChild(0).GetComponent<Image>().color = backgroundColor;
            backgroundMusicVolumeMuteOption.transform.GetChild(1).GetComponent<Text>().color = unselectedColor;
        }
        for (int i = 0; i < backgroundMusicVolumeOptions.Length; i++)
            if (i < backGroundMusicVolume)
            {
                Color usedColor = selectedColor;
                if (backGroundMusicMuted)
                    usedColor = unselectedColor;
                backgroundMusicVolumeOptions[i].color = usedColor;
                backgroundMusicVolumeOptions[i].transform.GetChild(0).GetComponent<Image>().color = usedColor;
            }
            else
            {
                backgroundMusicVolumeOptions[i].color = unselectedColor;
                backgroundMusicVolumeOptions[i].transform.GetChild(0).GetComponent<Image>().color = backgroundColor;
            }

        SettingsController.settings.SetBackgroundMusicMuted(backGroundMusicMuted);
    }

    public void ReportSoundEffectsVolumeOption(int index)
    {
        soundEffectsVolume = index;
        for (int i = 0; i < soundEffectsVolumeOptions.Length; i++)
            if (i < index)
            {
                soundEffectsVolumeOptions[i].color = selectedColor;
                soundEffectsVolumeOptions[i].transform.GetChild(0).GetComponent<Image>().color = selectedColor;
            }
            else
            {
                soundEffectsVolumeOptions[i].color = unselectedColor;
                soundEffectsVolumeOptions[i].transform.GetChild(0).GetComponent<Image>().color = backgroundColor;
            }

        if (soundEffectsMuted)
            ReportSoundEffectsVolumeMute();

        SettingsController.settings.SetSoundEffectsVolume(index, finishedInitializing);
        soundEffectsTitle.text = "Sound Effects Volume: " + index * 10 + "%";
    }

    public void ReportSoundEffectsVolumeMute()
    {
        soundEffectsMuted = !soundEffectsMuted;
        if (soundEffectsMuted)
            soundEffectsTitle.text = "Sound Effects Volume: Muted";
        else
            soundEffectsTitle.text = soundEffectsTitle.text = "Sound Effects Volume: " + SettingsController.settings.GetSoundEffectsVolume() * 10 + "%";

        if (soundEffectsMuted)
        {
            soundEffectsVolumeMuteOption.color = selectedColor;
            soundEffectsVolumeMuteOption.transform.GetChild(0).GetComponent<Image>().color = selectedColor;
            soundEffectsVolumeMuteOption.transform.GetChild(1).GetComponent<Text>().color = selectedColor;
        }
        else
        {
            soundEffectsVolumeMuteOption.color = unselectedColor;
            soundEffectsVolumeMuteOption.transform.GetChild(0).GetComponent<Image>().color = backgroundColor;
            soundEffectsVolumeMuteOption.transform.GetChild(1).GetComponent<Text>().color = unselectedColor;
        }
        for (int i = 0; i < soundEffectsVolumeOptions.Length; i++)
            if (i < soundEffectsVolume)
            {
                Color usedColor = selectedColor;
                if (soundEffectsMuted)
                    usedColor = unselectedColor;
                soundEffectsVolumeOptions[i].color = usedColor;
                soundEffectsVolumeOptions[i].transform.GetChild(0).GetComponent<Image>().color = usedColor;
            }
            else
            {
                soundEffectsVolumeOptions[i].color = unselectedColor;
                soundEffectsVolumeOptions[i].transform.GetChild(0).GetComponent<Image>().color = backgroundColor;
            }
        SettingsController.settings.SetSoundEffectsMuted(soundEffectsMuted, finishedInitializing);
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
