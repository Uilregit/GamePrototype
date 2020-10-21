using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModifyBuffDurationEffect : Effect
{
    public override IEnumerator Process(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex)
    {
        foreach(GameObject targ in target)
        {
            foreach (Buff buff in targ.GetComponent<BuffController>().GetBuffs())
                buff.duration += card.effectValue[effectIndex];
        }
        yield return new WaitForSeconds(0);
    }

    public override SimHealthController SimulateProcess(GameObject caster, CardEffectsController effectController, Vector2 location, int value, int duration, SimHealthController simH)
    {
        throw new System.NotImplementedException();
    }
}
