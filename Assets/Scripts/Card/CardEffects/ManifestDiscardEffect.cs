﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManifestDiscardEffect : Effect
{
    public override IEnumerator Process(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex)
    {
        List<CardController> discardPile = DeckController.deckController.GetDiscardPile();
        List<CardController> manifestList = new List<CardController>();
        List<int> viableList = new List<int>();
        for (int i = 0; i < discardPile.Count; i++)
            viableList.Add(i);

        for (int i = 0; i < 3; i++)
        {
            if (viableList.Count > 0)
            {
                int index = Random.Range(0, viableList.Count);
                manifestList.Add(discardPile[viableList[index]]);
                viableList.RemoveAt(index);
            }
        }

        if (manifestList.Count > 0)
        {
            UIController.ui.SetManifestCards(manifestList, this);

            while ((object)chosenCard == null)
            {
                yield return null;
            }

            HandController.handController.DrawSpecificCard(chosenCard, false);
            yield return HandController.handController.StartCoroutine(HandController.handController.ResolveDrawQueue());
        }
        else
            yield return new WaitForSeconds(0);
    }

    public override SimHealthController SimulateProcess(GameObject caster, CardEffectsController effectController, Vector2 location, int value, int duration, SimHealthController simH)
    {
        throw new System.NotImplementedException();
    }

    public override void RelicProcess(List<GameObject> targets, Buff buf, int effectValue, int effectDuration, List<Relic> traceList)
    {
    }
}