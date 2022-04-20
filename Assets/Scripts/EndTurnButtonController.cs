using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndTurnButtonController : MonoBehaviour
{
    public Text speedUpText;
    public Text buttonText;
    private bool mouseOnButton = false;
    private float lastSpeedUptime = 0;

    private void OnMouseDown()
    {
        if (TutorialController.tutorial.GetEnabled())
            return;

        if (Time.time - lastSpeedUptime > 2f)           //Allows for a 1 second grace period after speeding up the enemy turn where pressing end turn button doesn't accidentally become the enemy turn
        {
            mouseOnButton = true;
        }

        if (!TurnController.turnController.GetIsPlayerTurn())
        {
            Time.timeScale = 3;
            speedUpText.enabled = true;
        }
    }

    private void OnMouseExit()
    {
        if (TutorialController.tutorial.GetEnabled())
            return;

        mouseOnButton = false;

        if (Time.timeScale != 1)
        {
            Time.timeScale = 1;
            speedUpText.enabled = false;
        }
    }

    private void OnMouseUp()
    {
        if (TutorialController.tutorial.GetEnabled())
            return;

        if (TurnController.turnController.GetIsPlayerTurn() && mouseOnButton && Time.timeScale == 1)
            TurnController.turnController.SetPlayerTurn(false);
        else
            lastSpeedUptime = Time.time;

        if (Time.timeScale != 1)
        {
            Time.timeScale = 1;
            speedUpText.enabled = false;
        }

        mouseOnButton = false;
    }
}
