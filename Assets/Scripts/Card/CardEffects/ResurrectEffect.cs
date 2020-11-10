using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResurrectEffect : Effect
{
    public override IEnumerator Process(GameObject caster, CardEffectsController effectController, List<Vector2> target, Card card, int effectIndex)
    {
        HealthController player = null;

        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Player"))
            if (obj.GetComponent<PlayerController>().GetColorTag() == card.casterColor)
                player = obj.GetComponent<HealthController>();

        player.SetAttack(InformationController.infoController.GetStartingAttack(card.casterColor));
        player.SetCurrentArmor(InformationController.infoController.GetStartingArmor(card.casterColor), false);
        player.SetCurrentVit(InformationController.infoController.GetMaxVit(card.casterColor));

        Debug.Log(target[0]);
        player.transform.position = target[0];
        player.GetComponent<PlayerMoveController>().UpdateOrigin(player.transform.position);
        player.GetComponent<PlayerMoveController>().ResetMoveDistance(0);
        GridController.gridController.RemoveDeathLocation(card.casterColor);
        GridController.gridController.ReportPosition(player.gameObject, player.transform.position);

        GameController.gameController.ReportResurrectedChar(card.casterColor);

        InformationController.infoController.ChangeCombatInfo(-1, 0, 0, 0);
        //HandController.handController.ResetCardPlayability(TurnController.turnController.GetCurrentEnergy(), TurnController.turnController.GetCurrentMana());

        yield return new WaitForSeconds(0);
    }

    public override IEnumerator Process(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex)
    {
        throw new System.NotImplementedException();
    }

    public override SimHealthController SimulateProcess(GameObject caster, CardEffectsController effectController, Vector2 location, int value, int duration, SimHealthController simH)
    {
        throw new System.NotImplementedException();
    }
}