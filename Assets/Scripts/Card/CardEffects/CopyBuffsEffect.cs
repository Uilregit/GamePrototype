using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyBuffsEffect : Effect
{
    public override IEnumerator Process(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex, float waitTimeMultiplier)
    {
        if (waitTimeMultiplier == 0)
            yield break;

        foreach (GameObject targ in target)
            foreach (BuffFactory buff in targ.GetComponent<BuffController>().GetBuffs())
                buff.GetCopy().OnApply(caster.GetComponent<HealthController>(), targ.GetComponent<HealthController>(), buff.cardValue, buff.duration, false, null, null);
                //buff.GetCopy().OnApply(caster.GetComponent<HealthController>(), buff.cardValue, buff.duration, false);
        yield return new WaitForSeconds(0);
    }

    public override SimHealthController SimulateProcess(GameObject caster, CardEffectsController effectController, Vector2 location, int value, int duration, SimHealthController simH)
    {
        throw new System.NotImplementedException();
    }
}