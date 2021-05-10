using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardCostReductionDrawn : Effect
{
    public override IEnumerator Process(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex, float waitTimeMultiplier)
    {
        if (waitTimeMultiplier == 0)
            yield break;

        List<CardController> hand = HandController.handController.GetHand();
        for (int i = hand.Count - 1; i > hand.Count - 1 - card.effectDuration[effectIndex]; i--)
        {
            hand[i].SetEnergyCostDiscount(card.effectValue[effectIndex]);
            hand[i].SetManaCostDiscount(card.effectValue[effectIndex]);
        }
        HandController.handController.ResetCardDisplays();
        HandController.handController.ResetCardPlayability(TurnController.turnController.GetCurrentEnergy(), TurnController.turnController.GetCurrentMana());
        yield return new WaitForSeconds(0);
    }

    public override SimHealthController SimulateProcess(GameObject caster, CardEffectsController effectController, Vector2 location, int value, int duration, SimHealthController simH)
    {
        throw new System.NotImplementedException();
    }
}
