using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeHandSizeEffect : Effect
{
    public override IEnumerator Process(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex, float waitTimeMultiplier)
    {
        if (waitTimeMultiplier == 0)
        {
            yield break;
        }

        HandController.handController.SetBonusHandSize(card.effectValue[effectIndex], true);

        yield return new WaitForSeconds(0);
    }

    public override SimHealthController SimulateProcess(GameObject caster, CardEffectsController effectController, Vector2 location, int value, int duration, SimHealthController simH)
    {
        throw new System.NotImplementedException();
    }

    public override void RelicProcess(List<GameObject> targets, Buff buf, int effectValue, int effectDuration, List<Relic> traceList)
    {
        HandController.handController.SetBonusHandSize(effectValue, true);
    }
}