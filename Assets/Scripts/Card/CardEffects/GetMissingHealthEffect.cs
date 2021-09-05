using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetMissingHealth : Effect
{
    protected override IEnumerator Process(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex, float waitTimeMultiplier)
    {
        foreach (GameObject targ in target)
        {
            HealthController targetHealth = targ.GetComponent<HealthController>();
            effectController.GetCard().GetCard().SetTempEffectValue(targetHealth.GetCurrentVit() - targetHealth.GetMaxVit());
        }
        yield return new WaitForSeconds(0);
    }

    public override SimHealthController SimulateProcess(GameObject caster, CardEffectsController effectController, Vector2 location, int value, int duration, SimHealthController simH)
    {
        throw new System.NotImplementedException();
    }
}
