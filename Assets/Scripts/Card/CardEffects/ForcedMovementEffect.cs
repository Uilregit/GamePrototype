using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForcedMovementEffect : Effect
{
    public override void Process(GameObject caster, CardEffectsController effectController, GameObject target, Card card, int effectIndex)
    {
        target.GetComponent<HealthController>().ForcedMovement(caster.transform.position, card.effectValue[effectIndex]);
    }

    public override SimHealthController SimulateProcess(GameObject caster, CardEffectsController effectController, Vector2 location, int value, int duration, SimHealthController simH)
    {
        throw new System.NotImplementedException();
    }
}
