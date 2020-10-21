using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawEnergyCardEffect : Effect
{
    public override IEnumerator Process(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex)
    {
        for (int i = 0; i < card.effectValue[effectIndex]; i++) //Draw effectValue number of mana cards
        {
            bool successful;
            successful = HandController.handController.DrawEnergyCard(); //Draws the card and logs if it was successful
            card.SetPreviousEffectSuccessful(successful);
        }
        yield return new WaitForSeconds(0);
    }

    public override SimHealthController SimulateProcess(GameObject caster, CardEffectsController effectController, Vector2 location, int value, int duration, SimHealthController simH)
    {
        throw new System.NotImplementedException();
    }
}