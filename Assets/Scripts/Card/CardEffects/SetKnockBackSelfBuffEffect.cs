using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetKnockBackSelfBuffEffect : Effect
{
    public override IEnumerator Process(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex)
    {
        card.buff[effectIndex].SetDrawnCards(card.cards);
        int value = card.effectValue[effectIndex];
        if (value == 0)
            value = card.GetTempEffectValue();
        int duration = card.effectDuration[effectIndex];
        if (duration == 0)
            duration = card.GetTempDuration();
        caster.GetComponent<HealthController>().SetKnockBackSelfBuff(card.buff[effectIndex], value, duration, card.name);
        yield return new WaitForSeconds(0);
    }

    public override SimHealthController SimulateProcess(GameObject caster, CardEffectsController effectController, Vector2 location, int value, int duration, SimHealthController simH)
    {
        throw new System.NotImplementedException();
    }
}
