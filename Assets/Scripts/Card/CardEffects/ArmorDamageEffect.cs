using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmorDamageEffect : Effect
{
    protected override IEnumerator Process(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex, float waitTimeMultiplier)
    {
        int duration = 1;

        if (card.GetTempDuration() != 0)
            duration = card.GetTempDuration();
        else if (card.effectDuration[effectIndex] != 0)
            duration = card.effectDuration[effectIndex];

        for (int i = 0; i < duration; i++)
        {
            foreach (GameObject targ in target)
            {
                HealthController targetHealthController = targ.GetComponent<HealthController>();
                if (card.GetTempEffectValue() == 0)
                    targetHealthController.TakeArmorDamage(card.effectValue[effectIndex], caster.GetComponent<HealthController>());
                else
                    targetHealthController.TakeArmorDamage(card.GetTempEffectValue(), caster.GetComponent<HealthController>());
            }
            yield return new WaitForSeconds(TimeController.time.attackBufferTime * TimeController.time.timerMultiplier * waitTimeMultiplier);
        }
    }
    /*
    public override int GetSimulatedArmorDamage(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex)
    {
        int output = 0;
        foreach (GameObject targ in target)
        {
            HealthController targetHealthController = targ.GetComponent<HealthController>();
            if (card.GetTempEffectValue() == 0)
                output += targetHealthController.GetSimulatedArmorDamage(card.effectValue[effectIndex]);
            else
                output += targetHealthController.GetSimulatedArmorDamage(Mathf.CeilToInt(card.GetTempEffectValue() * card.effectValue[effectIndex] / 100.0f));
        }
        return output;
    }
    */
    public override SimHealthController SimulateProcess(GameObject caster, CardEffectsController effectController, Vector2 location, int value, int duration, SimHealthController simH)
    {
        List<GameObject> target = GridController.gridController.GetObjectAtLocation(location);
        return target[0].GetComponent<HealthController>().SimulateTakeArmorDamage(simH, value);
    }
}
