using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class DrawLastPlayedCardEffect : Effect
{
    protected override IEnumerator Process(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex, float waitTimeMultiplier)
    {
        if (waitTimeMultiplier == 0)
        {
            caster.GetComponent<BuffController>().TriggerBuff(Buff.TriggerType.OnCardDrawn, caster.GetComponent<HealthController>(), card.effectValue[effectIndex]);
            yield break;
        }
        List<Card> cardsPlayedThisTurn = TurnController.turnController.GetCardsPlayedThisTurn();

        if (MultiplayerGameController.gameController != null)
            if (ClientScene.localPlayer.GetComponent<MultiplayerInformationController>().GetPlayerNumber() == 1)
                if (cardsPlayedThisTurn[cardsPlayedThisTurn.Count - 1] == card)
                    cardsPlayedThisTurn.RemoveAt(cardsPlayedThisTurn.Count - 1);

        card.SetPreviousEffectSuccessful(cardsPlayedThisTurn.Count != 0);

        for (int i = cardsPlayedThisTurn.Count - 1; i > cardsPlayedThisTurn.Count - 1 - card.effectValue[effectIndex]; i--) //Draw effectValue number of cards
        {
            Card c = cardsPlayedThisTurn[i].GetCopy();
            try
            {
                c.casterColor = caster.GetComponent<PlayerController>().GetColorTag();
            }
            catch
            {
                c.casterColor = caster.GetComponent<MultiplayerPlayerController>().GetColorTag();
            }
            c.exhaust = true;

            CardController cc = HandController.handController.gameObject.AddComponent<CardController>();
            cc.SetCard(c, true, false);
            cc.SetEnergyCostDiscount(TurnController.turnController.GetCardPlayedEnergyReduction()[i]);
            cc.SetEnergyCostCap(TurnController.turnController.GetCardPlayedEnergyCap()[i]);
            cc.SetManaCostDiscount(TurnController.turnController.GetCardPlayedManaReduction()[i]);
            cc.SetManaCostCap(TurnController.turnController.GetCardPlayedManaCap()[i]);
            HandController.handController.CreateSpecificCard(cc);
        }
        yield return HandController.handController.StartCoroutine(HandController.handController.ResolveDrawQueue());
    }

    public override SimHealthController SimulateProcess(GameObject caster, CardEffectsController effectController, Vector2 location, int value, int duration, SimHealthController simH)
    {
        throw new System.NotImplementedException();
    }
}