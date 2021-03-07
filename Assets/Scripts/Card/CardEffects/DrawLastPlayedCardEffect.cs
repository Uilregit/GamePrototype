using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class DrawLastPlayedCardEffect : Effect
{
    public override IEnumerator Process(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex)
    {
        List<Card> cardsPlayedThisTurn = TurnController.turnController.GetCardsPlayedThisTurn();

        if (MultiplayerGameController.gameController != null)
            if (ClientScene.localPlayer.GetComponent<MultiplayerInformationController>().GetPlayerNumber() == 1)
                if (cardsPlayedThisTurn[cardsPlayedThisTurn.Count - 1] == card)
                    cardsPlayedThisTurn.RemoveAt(cardsPlayedThisTurn.Count - 1);

        card.SetPreviousEffectSuccessful(cardsPlayedThisTurn.Count != 0);

        for (int i = cardsPlayedThisTurn.Count - 1; i > cardsPlayedThisTurn.Count - 1 - card.effectValue[effectIndex]; i--) //Draw effectValue number of mana cards
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
            HandController.handController.CreateSpecificCard(cc);
        }
        yield return HandController.handController.StartCoroutine(HandController.handController.ResolveDrawQueue());
    }

    public override SimHealthController SimulateProcess(GameObject caster, CardEffectsController effectController, Vector2 location, int value, int duration, SimHealthController simH)
    {
        throw new System.NotImplementedException();
    }
}