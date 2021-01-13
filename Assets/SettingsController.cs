using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsController : MonoBehaviour
{
    public static SettingsController settings;

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

    public void SetGameSpeedIndex(int index)
    {
        gameSpeedIndex = index;
        switch(index)
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
