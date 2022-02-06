using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackQueueCardController : MonoBehaviour
{
    private void OnMouseDown()
    {
        if (TutorialController.tutorial.GetEnabled())
            return;

        CardController cardController = GetComponent<CardController>();
        TurnController.turnController.UseResources(-cardController.GetCard().energyCost, -cardController.GetCard().manaCost);
        //CombatController.combatController.RemoveCard(GetComponent<CardController>());
    }
}
