using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetBonusArmorEffect : Effect
{
    public override IEnumerator Process(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex, float waitTimeMultiplier)
    {
        int maxValue = 0;
        foreach (GameObject targ in target)
        {
            HealthController targetHealth = targ.GetComponent<HealthController>();
            if (targetHealth.GetBonusVit() > maxValue)
                maxValue = targetHealth.GetBonusVit();
        }
        card.SetTempEffectValue(maxValue);
        yield return new WaitForSeconds(0);
    }

    public override SimHealthController SimulateProcess(GameObject caster, CardEffectsController effectController, Vector2 location, int value, int duration, SimHealthController simH)
    {
        throw new System.NotImplementedException();
    }
}
