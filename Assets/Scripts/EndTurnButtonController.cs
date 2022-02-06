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
    }

    private void OnMouseExit()
    {
        if (TutorialController.tutorial.GetEnabled())
            return;

        mouseOnButton = false;
    }

    private void OnMouseUp()
    {
        if (TutorialController.tutorial.GetEnabled())
            return;

        if (TurnController.turnController.GetIsPlayerTurn() && mouseOnButton)
            TurnController.turnController.SetPlayerTurn(false);
    }
}
