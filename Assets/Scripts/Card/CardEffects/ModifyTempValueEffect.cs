using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModifyTempValueEffect : Effect
{
    public override IEnumerator Process(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex, float waitTimeMultiplier)
    {
        float value = card.GetTempEffectValue() * card.effectValue[effectIndex] / 100.0f;
        if (value > 0)
            card.SetTempEffectValue(Mathf.CeilToInt(value));
        else
            card.SetTempEffectValue(Mathf.FloorToInt(value));

        yield return new WaitForSeconds(0);
    }

    public override SimHealthController SimulateProcess(GameObject caster, CardEffectsController effectController, Vector2 location, int value, int duration, SimHealthController simH)
    {
        throw new System.NotImplementedException();
    }
}