using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateANYEnergyCard : Effect
{
    public override IEnumerator Process(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex)
    {
        for (int i = 0; i < card.effectValue[effectIndex]; i++) //Draw effectValue number of mana cards
        {
            Card c = LootController.loot.GetANYEnergyCard().GetCopy();
            c.casterColor = caster.GetComponent<PlayerController>().GetColorTag();
            c.exhaust = true;

            CardController cc = HandController.handController.gameObject.AddComponent<CardController>();
            cc.SetCard(c, true, false);
            HandController.handController.DrawSpecificCard(cc);
        }
        yield return new WaitForSeconds(0);
    }

    public override SimHealthController SimulateProcess(GameObject caster, CardEffectsController effectController, Vector2 location, int value, int duration, SimHealthController simH)
    {
        throw new System.NotImplementedException();
    }
}