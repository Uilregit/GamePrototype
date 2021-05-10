using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetDrawnCardEnergy : Effect
{
    public override IEnumerator Process(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex, float waitTimeMultiplier)
    {
        if (waitTimeMultiplier == 0)
            yield break;

        List<CardController> hand = HandController.handController.GetHand();
        card.SetTempEffectValue(hand[hand.Count - 1].GetNetEnergyCost());
        yield return new WaitForSeconds(0);
    }

    public override SimHealthController SimulateProcess(GameObject caster, CardEffectsController effectController, Vector2 location, int value, int duration, SimHealthController simH)
    {
        throw new System.NotImplementedException();
    }
}
