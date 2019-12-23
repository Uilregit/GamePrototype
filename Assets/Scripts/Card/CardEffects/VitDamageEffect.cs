using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VitDamageEffect : Effect
{
    public override void Process(GameObject caster, CardEffectsController effectController, GameObject target, Card card, int effectIndex)
    {
        HealthController targetHealthController = target.GetComponent<HealthController>();
        if (card.effectValue[effectIndex] != 0)
            target.GetComponent<HealthController>().TakeVitDamage(caster.GetComponent<HealthController>().GetAttack() + card.effectValue[effectIndex], caster.GetComponent<HealthController>());
        else
            targetHealthController.TakeVitDamage(caster.GetComponent<HealthController>().GetAttack() + card.GetTempEffectValue(), caster.GetComponent<HealthController>());
    }

    public override SimHealthController SimulateProcess(GameObject caster, CardEffectsController effectController, Vector2 location, int value, int duration, SimHealthController simH)
    {
        GameObject target = GridController.gridController.GetObjectAtLocation(location);
        return target.GetComponent<HealthController>().SimulateTakeVitDamage(simH, caster.GetComponent<HealthController>().GetAttack() + value);
    }
}