using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetMissingHealth : Effect
{
    public override void Process(GameObject caster, CardEffectsController effectController, GameObject target, Card card, int effectIndex)
    {
        HealthController targetHealth = target.GetComponent<HealthController>();
        effectController.GetCard().SetTempEffectValue(targetHealth.GetCurrentVit() - targetHealth.GetMaxVit());
    }

    public override SimHealthController SimulateProcess(GameObject caster, CardEffectsController effectController, Vector2 location, int value, int duration, SimHealthController simH)
    {
        throw new System.NotImplementedException();
    }
}
