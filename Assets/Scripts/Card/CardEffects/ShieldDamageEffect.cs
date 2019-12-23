using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldDamageEffect : Effect
{
    public override void Process(GameObject caster, CardEffectsController effectController, GameObject target, Card card, int effectIndex)
    {
        target.GetComponent<HealthController>().TakeShieldDamage(card.effectValue[effectIndex]);
    }

    public override SimHealthController SimulateProcess(GameObject caster, CardEffectsController effectController, Vector2 location, int value, int duration, SimHealthController simH)
    {
        GameObject target = GridController.gridController.GetObjectAtLocation(location);
        return target.GetComponent<HealthController>().SimulateTakeShieldDamage(simH, value);
    }
}
