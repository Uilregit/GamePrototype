using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeAttackEffect : Effect
{
    public override void Process(GameObject caster, CardEffectsController effectController, GameObject target, Card card, int effectIndex)
    {
        HealthController targetHealth = target.GetComponent<HealthController>();
        if (card.effectValue[effectIndex] != 0)
            targetHealth.ChangeAttack(card.effectValue[effectIndex]);
        else
            targetHealth.ChangeAttack(effectController.GetCard().GetTempEffectValue());
    }

    public override SimHealthController SimulateProcess(GameObject caster, CardEffectsController effectController, Vector2 location, int value, int duration, SimHealthController simH)
    {
        throw new System.NotImplementedException();
    }
}
