using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetNumberOfBuffsEffect : Effect
{
    public override IEnumerator Process(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex, float waitTimeMultiplier)
    {
        int highestNumberOfBuffs = -99999;
        foreach (GameObject targ in target)
            if (targ.GetComponent<BuffController>().GetBuffs().Count > highestNumberOfBuffs)
                highestNumberOfBuffs = targ.GetComponent<BuffController>().GetBuffs().Count;
        card.SetTempEffectValue(highestNumberOfBuffs);
        yield return new WaitForSeconds(0);
    }

    public override SimHealthController SimulateProcess(GameObject caster, CardEffectsController effectController, Vector2 location, int value, int duration, SimHealthController simH)
    {
        throw new System.NotImplementedException();
    }
}