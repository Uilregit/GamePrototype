using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetDurationEffect : Effect
{
    protected override IEnumerator Process(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex, float waitTimeMultiplier)
    {
        if (waitTimeMultiplier == 0)
            yield break;

        if (card.effectDuration[effectIndex] != 0)
            card.SetTempDuration(card.effectDuration[effectIndex]);
        else
        {
            card.SetTempDuration(card.GetTempEffectValue());
            card.SetTempEffectValue(0);
        }
        yield return new WaitForSeconds(0);
    }

    public override SimHealthController SimulateProcess(GameObject caster, CardEffectsController effectController, Vector2 location, int value, int duration, SimHealthController simH)
    {
        throw new System.NotImplementedException();
    }
}
