﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmorDamageAll : Effect
{
    public override IEnumerator Process(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex)
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject thisTarget in targets)
            thisTarget.GetComponent<HealthController>().TakeArmorDamage(card.effectValue[effectIndex], caster.GetComponent<HealthController>());
        yield return new WaitForSeconds(0);
    }

    public override SimHealthController SimulateProcess(GameObject caster, CardEffectsController effectController, Vector2 location, int value, int duration, SimHealthController simH)
    {
        throw new System.NotImplementedException();
    }

    public override void RelicProcess(List<GameObject> targets, Buff buf, int effectValue, int effectDuration)
    {
        foreach (GameObject targ in targets)
            targ.GetComponent<HealthController>().TakeArmorDamage(effectValue, null);
    }
}