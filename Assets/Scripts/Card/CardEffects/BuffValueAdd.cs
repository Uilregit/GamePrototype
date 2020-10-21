using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffValueAdd : Effect
{
    public override IEnumerator Process(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex)
    {
        foreach (GameObject targ in target)
            foreach (Buff buff in targ.GetComponent<BuffController>().GetBuffs())
                buff.AddToValue(targ.GetComponent<HealthController>(), card.effectValue[effectIndex]);

        yield return new WaitForSeconds(0);
    }

    public override SimHealthController SimulateProcess(GameObject caster, CardEffectsController effectController, Vector2 location, int value, int duration, SimHealthController simH)
    {
        HealthController target = GridController.gridController.GetObjectAtLocation(location)[0].GetComponent<HealthController>();
        return target.SimulateTakeVitDamage(simH, target.GetAttack());
    }
}
