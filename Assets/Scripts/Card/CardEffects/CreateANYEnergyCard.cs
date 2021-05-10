using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateANYEnergyCard : Effect
{
    public override IEnumerator Process(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex, float waitTimeMultiplier)
    {
        if (waitTimeMultiplier == 0)
        {
            //caster.GetComponent<BuffController>().TriggerBuff(Buff.TriggerType.OnCardDrawn, caster.GetComponent<HealthController>(), card.effectValue[effectIndex]);
            yield break;
        }
        for (int i = 0; i < card.effectValue[effectIndex]; i++) //Draw effectValue number of mana cards
        {
            Card c = LootController.loot.GetANYEnergyCard().GetCopy();
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