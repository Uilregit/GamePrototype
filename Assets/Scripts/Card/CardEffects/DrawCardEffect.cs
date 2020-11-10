using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawCardEffect : Effect
{
    public override IEnumerator Process(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex)
    {
        List<Card> spawnCards = new List<Card>();
        foreach (Card spawnCard in card.cards)
            if (spawnCard != null)
                spawnCards.Add(spawnCard);
        if (spawnCards.Count == 0)
            for (int i = 0; i < card.effectValue[effectIndex]; i++) //Draw effectValue number of random cards
                HandController.handController.DrawAnyCard();
        else
            foreach (Card spawnCard in card.cards)   //Draw all the specified cards
            {
                CardController thisCard = new CardController();
                thisCard.SetCard(spawnCard, false, false);
                HandController.handController.DrawSpecificCard(thisCard);
            }
        yield return new WaitForSeconds(0);
    }

    public override SimHealthController SimulateProcess(GameObject caster, CardEffectsController effectController, Vector2 location, int value, int duration, SimHealthController simH)
    {
        throw new System.NotImplementedException();
    }

    public override void RelicProcess(List<GameObject> targets, Buff buf, int effectValue, int effectDuration)
    {
        for (int i = 0; i < effectValue; i++) //Draw effectValue number of random cards
            HandController.handController.DrawAnyCard();
    }
}