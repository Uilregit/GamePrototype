using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldDamageEffect : Effect
{
    public override IEnumerator Process(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex)
    {
        foreach (GameObject targ in target)
        {
            HealthController targetHealthController = targ.GetComponent<HealthController>();
            if (card.GetTempEffectValue() == 0)
                targetHealthController.TakeShieldDamage(card.effectValue[effectIndex], caster.GetComponent<HealthController>());
            else
                targetHealthController.TakeShieldDamage(Mathf.CeilToInt(card.GetTempEffectValue() * card.effectValue[effectIndex] / 100.0f), caster.GetComponent<HealthController>());
        }
        yield return new WaitForSeconds(0);
    }

    public override int GetSimulatedShieldDamage(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex)
    {
        int output = 0;
        foreach (GameObject targ in target)
        {
            HealthController targetHealthController = targ.GetComponent<HealthController>();
            if (card.GetTempEffectValue() == 0)
                output += targetHealthController.GetSimulatedShieldDamage(card.effectValue[effectIndex]);
            else
                output += targetHealthController.GetSimulatedShieldDamage(Mathf.CeilToInt(card.GetTempEffectValue() * card.effectValue[effectIndex] / 100.0f));
        }
        return output;
    }

    public override SimHealthController SimulateProcess(GameObject caster, CardEffectsController effectController, Vector2 location, int value, int duration, SimHealthController simH)
    {
        List<GameObject> target = GridController.gridController.GetObjectAtLocation(location);
        return target[0].GetComponent<HealthController>().SimulateTakeShieldDamage(simH, value);
    }
}
