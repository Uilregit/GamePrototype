﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManaGainEffect : Effect
{
    public override IEnumerator Process(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex)
    {
        if (card.effectValue[effectIndex] == 0)
            TurnController.turnController.GainMana(card.GetTempEffectValue());
        else
            TurnController.turnController.GainMana(card.effectValue[effectIndex]);
        yield return new WaitForSeconds(0);
    }

    public override SimHealthController SimulateProcess(GameObject caster, CardEffectsController effectController, Vector2 location, int value, int duration, SimHealthController simH)
    {
        throw new System.NotImplementedException();
    }

    public override void RelicProcess(List<GameObject> targets, Card.BuffType buffType, int effectValue, int effectDuration)
    {
        TurnController.turnController.GainMana(effectValue);
    }
}
