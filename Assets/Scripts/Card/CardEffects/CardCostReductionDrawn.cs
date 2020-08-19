using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardCostReductionDrawn : Effect
{
    public override IEnumerator Process(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex)
    {
        List<CardController> hand = HandController.handController.GetHand();
        hand[hand.Count - 1].SetEnergyCostDiscount(card.effectValue[effectIndex]);
        hand[hand.Count - 1].SetManaCostDiscount(card.effectValue[effectIndex]);
        HandController.handController.ResetCardDisplays();
        HandController.handController.ResetCardPlayability(TurnController.turnController.GetCurrentEnergy(), TurnController.turnController.GetCurrentMana());
        yield return new WaitForSeconds(0);
    }

    public override SimHealthController SimulateProcess(GameObject caster, CardEffectsController effectController, Vector2 location, int value, int duration, SimHealthController simH)
    {
        throw new System.NotImplementedException();
    }
}
