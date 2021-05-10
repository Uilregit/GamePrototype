using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiveBuffsEffect : Effect
{
    public override IEnumerator Process(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex, float waitTimeMultiplier)
    {
        foreach (GameObject targ in target)
            foreach (BuffFactory buff in caster.GetComponent<BuffController>().GetBuffs())
                buff.GetCopy().OnApply(targ.GetComponent<HealthController>(), caster.GetComponent<HealthController>(), card.effectValue[effectIndex], card.effectDuration[effectIndex], false, null, null);
        //buff.GetCopy().OnApply(targ.GetComponent<HealthController>(), buff.cardValue, buff.duration, false);
        yield return new WaitForSeconds(0);
    }

    public override SimHealthController SimulateProcess(GameObject caster, CardEffectsController effectController, Vector2 location, int value, int duration, SimHealthController simH)
    {
        throw new System.NotImplementedException();
    }
}