using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackEffect : Effect
{
    public override IEnumerator Process(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex)
    {
        foreach (GameObject targ in target)
        {
            HealthController targetHealthController = targ.GetComponent<HealthController>();
            if (card.effectValue[effectIndex] != 0)
                targetHealthController.TakeVitDamage(caster.GetComponent<HealthController>().GetAttack(), caster.GetComponent<HealthController>());
            else
                targetHealthController.TakeVitDamage(caster.GetComponent<HealthController>().GetAttack() + card.GetTempEffectValue(), caster.GetComponent<HealthController>());
        }
        yield return new WaitForSeconds(0);
    }

    public override SimHealthController SimulateProcess(GameObject caster, CardEffectsController effectController, Vector2 location, int value, int duration, SimHealthController simH)
    {
        HealthController target = GridController.gridController.GetObjectAtLocation(location)[0].GetComponent<HealthController>();
        return target.SimulateTakeVitDamage(simH, target.GetAttack());
    }
}
