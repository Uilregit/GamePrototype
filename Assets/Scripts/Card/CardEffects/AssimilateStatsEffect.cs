﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssimilateStatsEffect : Effect
{
    public override IEnumerator Process(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex)
    {
        foreach (GameObject targ in target)
        {
            HealthController targetHealthController = targ.GetComponent<HealthController>();
            HealthController casterHealthController = caster.GetComponent<HealthController>();
            if (casterHealthController.GetAttack() != targetHealthController.GetAttack())
                new AttackChangeBuff().OnApply(targetHealthController, casterHealthController.GetAttack() - targetHealthController.GetAttack(), card.effectDuration[effectIndex], false);
            if (casterHealthController.GetShield() != targetHealthController.GetShield())
                new ArmorBuff().OnApply(targetHealthController, casterHealthController.GetShield() - targetHealthController.GetShield(), card.effectDuration[effectIndex], false);
        }
        yield return new WaitForSeconds(0);
    }

    public override SimHealthController SimulateProcess(GameObject caster, CardEffectsController effectController, Vector2 location, int value, int duration, SimHealthController simH)
    {
        HealthController target = GridController.gridController.GetObjectAtLocation(location)[0].GetComponent<HealthController>();
        return target.SimulateTakeVitDamage(simH, target.GetAttack());
    }
}