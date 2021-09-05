using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResurrectEffect : Effect
{
    protected override IEnumerator Process(GameObject caster, CardEffectsController effectController, List<Vector2> target, Card card, int effectIndex, float waitTimeMultiplier)
    {
        if (waitTimeMultiplier == 0)
            yield break;

        HealthController player = null;

        if (MultiplayerGameController.gameController != null)
            player = caster.GetComponent<HealthController>();
        else
            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Player"))
                if (obj.GetComponent<PlayerController>().GetColorTag() == card.casterColor && !obj.GetComponent<HealthController>().GetIsSimulation())
                    player = obj.GetComponent<HealthController>();

        player.SetCurrentAttack(InformationController.infoController.GetStartingAttack(card.casterColor));
        player.SetCurrentArmor(InformationController.infoController.GetStartingArmor(card.casterColor), false);
        player.SetCurrentVit(InformationController.infoController.GetMaxVit(card.casterColor));

        player.transform.position = target[0];
        player.GetComponent<HealthController>().charDisplay.transform.position = target[0];
        player.GetComponent<HealthController>().ReportResurrect();
        
        if (MultiplayerGameController.gameController != null)
        {
            player.GetComponent<MultiplayerPlayerMoveController>().UpdateOrigin(player.transform.position);
            player.GetComponent<MultiplayerPlayerMoveController>().ResetMoveDistance(0);
        }
        else
        {
            player.GetComponent<PlayerMoveController>().UpdateOrigin(player.transform.position);
            player.GetComponent<PlayerMoveController>().ResetMoveDistance(0);
            player.GetComponent<PlayerMoveController>().SetMoveable(true);
        }
        GridController.gridController.RemoveDeathLocation(card.casterColor);
        GridController.gridController.ReportPosition(player.gameObject, player.transform.position);

        if (MultiplayerGameController.gameController != null)
            MultiplayerGameController.gameController.ReportResurrectedChar(card.casterColor);
        else
            GameController.gameController.ReportResurrectedChar(card.casterColor, caster);

        InformationController.infoController.ChangeCombatInfo(-1, 0, 0, 0);
        ResourceController.resource.ReportReviveUsed();
        //HandController.handController.ResetCardPlayability(TurnController.turnController.GetCurrentEnergy(), TurnController.turnController.GetCurrentMana());

        yield return new WaitForSeconds(0);
    }

    protected override IEnumerator Process(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex, float waitTimeMultiplier)
    {
        throw new System.NotImplementedException();
    }

    public override SimHealthController SimulateProcess(GameObject caster, CardEffectsController effectController, Vector2 location, int value, int duration, SimHealthController simH)
    {
        throw new System.NotImplementedException();
    }
}