using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackEffect : Effect
{
    public override void Process(GameObject caster, CardEffectsController effectController, GameObject target, Card card, int effectIndex)
    {
        HealthController targetHealthController = target.GetComponent<HealthController>();
        if (card.effectValue[effectIndex] != 0)
            targetHealthController.TakeVitDamage(caster.GetComponent<HealthController>().GetAttack(), caster.GetComponent<HealthController>());
        else
            targetHealthController.TakeVitDamage(caster.GetComponent<HealthController>().GetAttack() + card.GetTempEffectValue(), caster.GetComponent<HealthController>());
    }

    public override SimHealthController SimulateProcess(GameObject caster, CardEffectsController effectController, Vector2 location, int value, int duration, SimHealthController simH)
    {
        HealthController target = GridController.gridController.GetObjectAtLocation(location).GetComponent<HealthController>();
        return target.SimulateTakeVitDamage(simH, target.GetAttack());
    }
}
