using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawManaCardEffect : Effect
{
    public override IEnumerator Process(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex, float waitTimeMultiplier)
    {
        if (waitTimeMultiplier == 0)
        {
            caster.GetComponent<BuffController>().TriggerBuff(Buff.TriggerType.OnCardDrawn, caster.GetComponent<HealthController>(), card.effectValue[effectIndex]);
            yield break;
        }
        for (int i = 0; i < card.effectValue[effectIndex]; i++) //Draw effectValue number of mana cards
        {
            bool successful;
            successful = HandController.handController.DrawManaCard(); //Draws the card and logs if it was successful
            card.SetPreviousEffectSuccessful(successful);
        }
        yield return HandController.handController.StartCoroutine(HandController.handController.ResolveDrawQueue());
    }

    public override SimHealthController SimulateProcess(GameObject caster, CardEffectsController effectController, Vector2 location, int value, int duration, SimHealthController simH)
    {
        throw new System.NotImplementedException();
    }
}