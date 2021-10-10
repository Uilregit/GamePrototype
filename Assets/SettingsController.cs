using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsController : MonoBehaviour
{
    public static SettingsController settings;

    public AudioClip SFXSampleClip;

    private int backGroundMusicVolume = 10;
    private int soundEffectsVolume = 10;
    private bool backGroundMusicMuted = false;
    private bool soundEffectsMuted = false;
    private int gameSpeedIndex;
    private int screenShakeIndex;
    private bool remainingMoveRangeIndicator = false;

    private void Start()
    {
        if (SettingsController.settings == null)
            SettingsController.settings = this;
        else
            Destroy(this.gameObject);

        DontDestroyOnLoad(this.gameObject);

        InformationLogger.infoLogger.LoadSettings();
    }

    public void SetBackgroundMusicVolume(int index)
    {
        backGroundMusicVolume = index;
        ApplyBackgroundMusicVolume();
    }

    public void SetSoundEffectsVolume(int index, bool playSample)
    {
        soundEffectsVolume = index;
        ApplySoundEffectsVolume(playSample);
    }

    public void SetBackgroundMusicMuted(bool state)
    {
        backGroundMusicMuted = state;
        ApplyBackgroundMusicVolume();
    }

    public void SetSoundEffectsMuted(bool state, bool playSample)
    {
        soundEffectsMuted = state;
        ApplySoundEffectsVolume(playSample);
    }

    private void ApplyBackgroundMusicVolume()
    {
        if (backGroundMusicMuted)
            MusicController.music.SetBackGroundVolume(0);
        else
            MusicController.music.SetBackGroundVolume(backGroundMusicVolume / 10.0f);
    }

    private void ApplySoundEffectsVolume(bool playSample)
    {
        if (soundEffectsMuted)
            MusicController.music.SetSFXVolume(0);
        else
            MusicController.music.SetSFXVolume(soundEffectsVolume / 10.0f);

        if (playSample)
            MusicController.music.PlaySFX(SFXSampleClip);
    }

    public void SetGameSpeedIndex(int index)
    {
        gameSpeedIndex = index;
        switch (index)
        {
            case 0:
                TimeController.time.timerMultiplier = 1;
                break;
            case 1:
                TimeController.time.timerMultiplier = 0.75f;
                break;
            case 2:
                TimeController.time.timerMultiplier = 0.5f;
                break;
        }
    }

    public int GetGameSpeedIndex()
    {
        return gameSpeedIndex;
    }

    public void SetScreenShakeIndex(int index)
    {
        screenShakeIndex = index;
    }

    public int GetBackGroundMusicVolume()
    {
        return backGroundMusicVolume;
    }

    public int GetSoundEffectsVolume()
    {
        return soundEffectsVolume;
    }

    public bool GetBackGroundMusicMuted()
    {
        return backGroundMusicMuted;
    }

    public bool GetSoundEffectsMuted()
    {
        return soundEffectsMuted;
    }

    public float GetScreenShakeMultiplier()
    {
        switch (screenShakeIndex)
        {
            case 0:
                return 1f;
            case 1:
                return 0.5f;
            case 2:
                return 0f;
        }
        return 1f;
    }

    public int GetScreenShakeIndex()
    {
        return screenShakeIndex;
    }

    public void SetRemainingMoveRangeIndicator(bool state)
    {
        remainingMoveRangeIndicator = state;
    }

    public bool GetRemainingMoveRangeIndicator()
    {
        return remainingMoveRangeIndicator;
    }
}
