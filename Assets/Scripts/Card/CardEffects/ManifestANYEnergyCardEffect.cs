using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManifestANYEnergyCardEffect : Effect
{
    public override IEnumerator Process(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex)
    {
        List<CardController> manifestList = new List<CardController>();
        for (int i = 0; i < 3; i++) //Draw effectValue number of mana cards
        {
            Card c = LootController.loot.GetANYEnergyCard().GetCopy();
            c.casterColor = caster.GetComponent<PlayerController>().GetColorTag();
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