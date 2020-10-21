using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetEnergySpentTurn : Effect
{
    public override IEnumerator Process(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex)
    {
        card.SetTempEffectValue(TurnController.turnController.GetEnergySpent());
        yield return new WaitForSeconds(0);
    }

    public override SimHealthController SimulateProcess(GameObject caster, CardEffectsController effectController, Vector2 location, int value, int duration, SimHealthController simH)
    {
        throw new System.NotImplementedException();
    }
}
