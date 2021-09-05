using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManifestANYEnergyCardEffect : Effect
{
    protected override IEnumerator Process(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex, float waitTimeMultiplier)
    {
        if (waitTimeMultiplier == 0)
            yield break;

        List<CardController> manifestList = new List<CardController>();
        for (int i = 0; i < 3; i++) //Draw effectValue number of mana cards
        {
            Card c = card;
            while (c == card)   //Ensure you never manifest the card gotten
                c = LootController.loot.GetANYEnergyCard().GetCopy();

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
            manifestList.Add(cc);
        }
        yield return new WaitForSeconds(0);

        if (manifestList.Count > 0)
        {
            UIController.ui.SetManifestCards(manifestList, this);

            while ((object)chosenCard == null)
            {
                yield return null;
            }

            HandController.handController.CreateSpecificCard(chosenCard);
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