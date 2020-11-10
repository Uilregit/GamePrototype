using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PiercingDamageAll : Effect
{
    public override IEnumerator Process(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex)
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject t in targets)
            new EffectFactory().GetEffect(Card.EffectType.PiercingDamage).Process(caster, effectController, new List<GameObject> { t }, card, effectIndex);
        yield return new WaitForSeconds(0);
    }

    public override SimHealthController SimulateProcess(GameObject caster, CardEffectsController effectController, Vector2 location, int value, int duration, SimHealthController simH)
    {
        throw new System.NotImplementedException();
    }

    public override void RelicProcess(List<GameObject> targets, Buff buf, int effectValue, int effectDuration)
    {
        foreach (GameObject targ in targets)
            targ.GetComponent<HealthController>().TakePiercingDamage(effectValue, null);
    }
}
