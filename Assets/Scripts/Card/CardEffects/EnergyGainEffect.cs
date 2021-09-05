using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyGainEffect : Effect
{
    protected override IEnumerator Process(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex, float waitTimeMultiplier)
    {
        if (waitTimeMultiplier == 0)
            yield break;

        if (card.effectValue[effectIndex] == 0)
            TurnController.turnController.GainEnergy(card.GetTempEffectValue());
        else
            TurnController.turnController.GainEnergy(card.effectValue[effectIndex]);
        yield return new WaitForSeconds(0);
    }

    public override SimHealthController SimulateProcess(GameObject caster, CardEffectsController effectController, Vector2 location, int value, int duration, SimHealthController simH)
    {
        throw new System.NotImplementedException();
    }
}
