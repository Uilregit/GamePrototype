using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManifestDrawEffect : Effect
{
    protected override IEnumerator Process(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex, float waitTimeMultiplier)
    {
        if (waitTimeMultiplier == 0)
            yield break;

        List<CardController> drawPile = DeckController.deckController.GetDrawPile();
        List<CardController> manifestList = new List<CardController>();
        List<int> viableList = new List<int>();
        for (int i = 0; i < drawPile.Count; i++)
            viableList.Add(i);

        for (int i = 0; i < 3; i++)
        {
            if (viableList.Count > 0)
            {
                int index = Random.Range(0, viableList.Count);
                manifestList.Add(drawPile[viableList[index]]);
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

            Card c = chosenCard.GetCard().GetCopy();
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
            cc.SetEnergyCostDiscount(chosenCard.GetEnergyCostDiscount());
            cc.SetEnergyCostCap(chosenCard.GetEnergyCostCap());
            cc.SetManaCostDiscount(chosenCard.GetManaCostDiscount());
            cc.SetManaCostCap(chosenCard.GetManaCostCap());
            HandController.handController.CreateSpecificCard(cc);
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