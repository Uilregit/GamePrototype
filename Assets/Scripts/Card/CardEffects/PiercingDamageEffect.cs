using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PiercingDamageEffect : Effect
{
    public override void Process(GameObject caster, CardEffectsController effectController, GameObject target, Card card, int effectIndex)
    {
        target.GetComponent<HealthController>().TakePiercingDamage(card.effectValue[effectIndex], caster.GetComponent<HealthController>());
    }

    public override SimHealthController SimulateProcess(GameObject caster, CardEffectsController effectController, Vector2 location, int value, int duration, SimHealthController simH)
    {
        GameObject target = GridController.gridController.GetObjectAtLocation(location);
        return target.GetComponent<HealthController>().SimulateTakePiercingDamage(simH, value);
    }
}
