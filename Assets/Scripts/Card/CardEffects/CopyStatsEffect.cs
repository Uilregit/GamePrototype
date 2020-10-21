using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyStatsEffect : Effect
{
    public override IEnumerator Process(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex)
    {
        int maxATK = -999999;
        int maxShield = -9999999;
        foreach (GameObject targ in target)
        {
            HealthController targetHealthController = targ.GetComponent<HealthController>();
            if (targetHealthController.GetAttack() > maxATK)
                maxATK = targetHealthController.GetAttack();
            if (targetHealthController.GetShield() > maxShield)
                maxShield = targetHealthController.GetShield();
        }

        HealthController casterHealthController = caster.GetComponent<HealthController>();
        if (casterHealthController.GetAttack() != maxATK)
            new AttackChangeBuff().OnApply(casterHealthController, maxATK - casterHealthController.GetAttack(), card.effectDuration[effectIndex], false);
        if (casterHealthController.GetShield() != maxShield)
            new ArmorBuff().OnApply(casterHealthController, maxShield - casterHealthController.GetShield(), card.effectDuration[effectIndex], false);
        yield return new WaitForSeconds(0);
    }

    public override SimHealthController SimulateProcess(GameObject caster, CardEffectsController effectController, Vector2 location, int value, int duration, SimHealthController simH)
    {
        HealthController target = GridController.gridController.GetObjectAtLocation(location)[0].GetComponent<HealthController>();
        return target.SimulateTakeVitDamage(simH, target.GetAttack());
    }
}
