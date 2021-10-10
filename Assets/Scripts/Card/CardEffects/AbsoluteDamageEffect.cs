using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbsoluteDamageEffect : Effect
{
    protected override IEnumerator Process(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex, float waitTimeMultiplier)
    {
        int totalDamageValue = 0;
        int duration = 1;

        if (card.GetTempDuration() != 0)
            duration = card.GetTempDuration();
        else if (card.effectDuration[effectIndex] != 0)
            duration = card.effectDuration[effectIndex];

        for (int i = 0; i < duration; i++)
        {
            if (i != 0 && waitTimeMultiplier != 0)
                caster.GetComponent<HealthController>().charDisplay.onHitSoundController.PlaySound(card.soundEffect[effectIndex]);

            foreach (GameObject targ in target)
            {
                int damageValue = 0;
                HealthController targetHealthController = targ.GetComponent<HealthController>();
                if (card.GetTempEffectValue() == 0)
                    damageValue = card.effectValue[effectIndex];
                else
                    damageValue = card.GetTempEffectValue();

                int simulatedDamage = targetHealthController.GetSimulatedVitDamage(damageValue);
                totalDamageValue += simulatedDamage;

                targetHealthController.TakeVitDamage(damageValue, caster.GetComponent<HealthController>());
            }
            yield return new WaitForSeconds(TimeController.time.attackBufferTime * TimeController.time.timerMultiplier * waitTimeMultiplier);
        }
        if (totalDamageValue > 0 && waitTimeMultiplier != 0)        //Don't trigger on damage dealt on simulations
            caster.GetComponent<BuffController>().StartCoroutine(caster.GetComponent<BuffController>().TriggerBuff(Buff.TriggerType.OnDamageDealt, caster.GetComponent<HealthController>(), totalDamageValue));
        card.SetDamageDone(totalDamageValue);
    }
    /*
    public override int GetSimulatedVitDamage(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex)
    {
        int output = 0;
        int duration = 1;

        if (card.effectDuration[effectIndex] != 0)
            duration = card.effectDuration[effectIndex];
        else if (card.GetTempDuration() != 0)
            duration = card.GetTempDuration();

        for (int i = 0; i < duration; i++)
            foreach (GameObject targ in target)
            {
                int value = 0;
                HealthController targetHealthController = targ.GetComponent<HealthController>();

                if (card.GetTempEffectValue() == 0)
                    value += card.effectValue[effectIndex];
                else
                    value += card.GetTempEffectValue();

                output += targetHealthController.GetSimulatedVitDamage(value);
            }
        return output;
    }

    public override int GetSimulatedArmorDamage(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex)
    {
        int output = 0;
        foreach (GameObject targ in target)
        {
            HealthController targetHealthController = targ.GetComponent<HealthController>();
            output += targetHealthController.GetSimulatedArmorDamage(1);
        }
        return output;
    }
*/
    public override SimHealthController SimulateProcess(GameObject caster, CardEffectsController effectController, Vector2 location, int value, int duration, SimHealthController simH)
    {
        List<GameObject> target = GridController.gridController.GetObjectAtLocation(location);
        return target[0].GetComponent<HealthController>().SimulateTakeVitDamage(simH, caster.GetComponent<HealthController>().GetAttack() + value);
    }
}