using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetStarterCardEffect : Effect
{
    public override IEnumerator Process(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex)
    {
        foreach (GameObject targ in target)
        {
            Card c = LootController.loot.GetStarterAttackCard(targ.GetComponent<PlayerController>().GetColorTag()).GetCopy();
            c.casterColor = caster.GetComponent<PlayerController>().GetColorTag();
            c.exhaust = true;
            c.manaCost = 0;
            c.energyCost = 0;

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

    /*
    public override void RelicProcess(List<GameObject> targets, Card.BuffType buffType, int effectValue, int effectDuration)
    {
        for (int i = 0; i < effectValue; i++) //Draw effectValue number of random cards
            HandController.handController.DrawAnyCard();
    }
    */
}