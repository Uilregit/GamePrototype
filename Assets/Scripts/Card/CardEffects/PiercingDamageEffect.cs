﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PiercingDamageEffect : Effect
{
    public override IEnumerator Process(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex)
    {
        int totalDamageValue = 0;
        foreach (GameObject targ in target)
        {
            int damageValue = 0;
            HealthController targetHealthController = targ.GetComponent<HealthController>();
            if (card.GetTempEffectValue() == 0)
            {
                if (card.effectValue[effectIndex] != 0)
                    damageValue = Mathf.CeilToInt(caster.GetComponent<HealthController>().GetAttack() * Mathf.Abs(card.effectValue[effectIndex]) / 100.0f) * Mathf.RoundToInt(Mathf.Sign(card.effectValue[effectIndex]));
                else
                    damageValue = caster.GetComponent<HealthController>().GetAttack();
            }
            else
            {
                if (card.effectValue[effectIndex] != 0)
                    damageValue = Mathf.CeilToInt(card.GetTempEffectValue() * Mathf.Abs(card.effectValue[effectIndex]) / 100.0f) * Mathf.RoundToInt(Mathf.Sign(card.effectValue[effectIndex]));
                else
                    damageValue = Mathf.CeilToInt(caster.GetComponent<HealthController>().GetAttack() * card.GetTempEffectValue() / 100.0f);
            }
            totalDamageValue += damageValue;
            targetHealthController.TakePiercingDamage(damageValue, caster.GetComponent<HealthController>());
        }
        yield return new WaitForSeconds(TimeController.time.attackBufferTime * TimeController.time.timerMultiplier);
        card.SetDamageDone(totalDamageValue);
    }

    public override int GetSimulatedVitDamage(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex)
    {
        int output = 0;
        foreach (GameObject targ in target)
        {
            HealthController targetHealthController = targ.GetComponent<HealthController>();
            if (card.GetTempEffectValue() == 0)
            {
                if (card.effectValue[effectIndex] != 0)
                    output += Mathf.CeilToInt(caster.GetComponent<HealthController>().GetAttack() * Mathf.Abs(card.effectValue[effectIndex]) / 100.0f) * Mathf.RoundToInt(Mathf.Sign(card.effectValue[effectIndex]));
                else
                    output += caster.GetComponent<HealthController>().GetAttack();
            }
            else
            {
                if (card.effectValue[effectIndex] != 0)
                    output += Mathf.CeilToInt(card.GetTempEffectValue() * Mathf.Abs(card.effectValue[effectIndex]) / 100.0f) * Mathf.RoundToInt(Mathf.Sign(card.effectValue[effectIndex]));
                else
                    output += Mathf.CeilToInt(caster.GetComponent<HealthController>().GetAttack() * card.GetTempEffectValue() / 100.0f);
            }
        }
        return output;
    }

    public override int GetSimulatedShieldDamage(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex)
    {
        return 0;
    }

    public override SimHealthController SimulateProcess(GameObject caster, CardEffectsController effectController, Vector2 location, int value, int duration, SimHealthController simH)
    {
        List<GameObject> target = GridController.gridController.GetObjectAtLocation(location);
        return target[0].GetComponent<HealthController>().SimulateTakePiercingDamage(simH, value);
    }
    public override void RelicProcess(List<GameObject> targets, Card.BuffType buffType, int effectValue, int effectDuration)
    {
        foreach (GameObject targ in targets)
            targ.GetComponent<HealthController>().TakePiercingDamage(effectValue, null);
    }
}
