using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawCardEffect : Effect
{
    protected override IEnumerator Process(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex, float waitTimeMultiplier)
    {
        if (waitTimeMultiplier == 0)
        {
            caster.GetComponent<BuffController>().TriggerBuff(Buff.TriggerType.OnCardDrawn, caster.GetComponent<HealthController>(), card.effectValue[effectIndex]);
            yield break;
        }

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
                Card copy = spawnCard.GetCopy();
                copy.casterColor = card.casterColor;            //Colors of the cards drawn will always be same color as the original card
                thisCard.SetCard(copy, false, false);
                HandController.handController.CreateSpecificCard(thisCard);
            }
        yield return HandController.handController.StartCoroutine(HandController.handController.ResolveDrawQueue()); 
    }

    public override SimHealthController SimulateProcess(GameObject caster, CardEffectsController effectController, Vector2 location, int value, int duration, SimHealthController simH)
    {
        throw new System.NotImplementedException();
    }

    public override void RelicProcess(List<GameObject> targets, Buff buf, int effectValue, int effectDuration, List<Relic> traceList)
    {
        for (int i = 0; i < effectValue; i++) //Draw effectValue number of random cards
            HandController.handController.DrawAnyCard();
    }
}