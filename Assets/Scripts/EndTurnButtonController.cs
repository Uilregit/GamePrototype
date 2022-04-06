using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndTurnButtonController : MonoBehaviour
{
    private bool mouseOnButton = false;

    private void OnMouseDown()
    {
        if (TutorialController.tutorial.GetEnabled())
            return;

        mouseOnButton = true;

        if (!TurnController.turnController.GetIsPlayerTurn())
            Time.timeScale = 3;
    }

    private void OnMouseExit()
    {
        if (TutorialController.tutorial.GetEnabled())
            return;

        mouseOnButton = false;

        if (Time.timeScale != 1)
            Time.timeScale = 1;
    }

    private void OnMouseUp()
    {
        if (TutorialController.tutorial.GetEnabled())
            return;

        if (TurnController.turnController.GetIsPlayerTurn() && mouseOnButton && Time.timeScale == 1)
            TurnController.turnController.SetPlayerTurn(false);

        if (Time.timeScale != 1)
            Time.timeScale = 1;
    }
}
