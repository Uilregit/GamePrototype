using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndTurnButtonController : MonoBehaviour
{
    private bool mouseOnButton = false;

    private void OnMouseDown()
    {
        mouseOnButton = true;
    }

    private void OnMouseExit()
    {
        mouseOnButton = false;
    }

    private void OnMouseUp()
    {
        if (TurnController.turnController.GetIsPlayerTurn() && mouseOnButton)
            TurnController.turnController.SetPlayerTurn(false);
    }
}
