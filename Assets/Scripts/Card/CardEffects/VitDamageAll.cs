using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VitDamageAll : Effect
{
    public override IEnumerator Process(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex)
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject t in targets)
            new EffectFactory().GetEffect(Card.EffectType.VitDamage).Process(caster, effectController, new List<GameObject> { t }, card, effectIndex);
        yield return new WaitForSeconds(0);
    }

    public override SimHealthController SimulateProcess(GameObject caster, CardEffectsController effectController, Vector2 location, int value, int duration, SimHealthController simH)
    {
        throw new System.NotImplementedException();
    }
}
