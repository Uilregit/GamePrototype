using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VitDamageAll : Effect
{
    public override void Process(GameObject caster, CardEffectsController effectController, GameObject target, Card card, int effectIndex)
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject thisTarget in targets)
            thisTarget.GetComponent<HealthController>().TakeVitDamage(caster.GetComponent<HealthController>().GetAttack()+ card.effectValue[effectIndex], caster.GetComponent<HealthController>());
    }

    public override SimHealthController SimulateProcess(GameObject caster, CardEffectsController effectController, Vector2 location, int value, int duration, SimHealthController simH)
    {
        throw new System.NotImplementedException();
    }
}
