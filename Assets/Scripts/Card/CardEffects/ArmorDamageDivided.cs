using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmorDamageDivided : Effect
{
    public override IEnumerator Process(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex)
    {
        int originalTempValue = card.GetTempEffectValue();

        if (card.GetTempEffectValue() != 0)
            card.SetTempEffectValue(Mathf.CeilToInt(card.GetTempEffectValue() / (float)target.Count));
        else
            card.SetTempEffectValue(Mathf.CeilToInt(card.effectValue[effectIndex] / (float)target.Count));

        foreach (GameObject t in target)
            new EffectFactory().GetEffect(Card.EffectType.ArmorDamage).Process(caster, effectController, new List<GameObject> { t }, card, effectIndex);

        card.SetTempEffectValue(originalTempValue);
        yield return new WaitForSeconds(0);
    }

    public override int GetSimulatedVitDamage(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex)
    {
        int originalTempValue = card.GetTempEffectValue();

        if (card.GetTempEffectValue() != 0)
            card.SetTempEffectValue(Mathf.CeilToInt(card.GetTempEffectValue() / (float)target.Count));
        else
            card.SetTempEffectValue(Mathf.CeilToInt(card.effectValue[effectIndex] / (float)target.Count));

        int output = 0;
        card.SetTempEffectValue(Mathf.CeilToInt(card.effectValue[effectIndex] / (float)target.Count));
        foreach (GameObject t in target)
            output += new EffectFactory().GetEffect(Card.EffectType.ArmorDamage).GetSimulatedVitDamage(caster, effectController, new List<GameObject> { t }, card, effectIndex);

        card.SetTempEffectValue(originalTempValue);
        return output;
    }

    public override int GetSimulatedArmorDamage(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex)
    {
        int originalTempValue = card.GetTempEffectValue();

        if (card.GetTempEffectValue() != 0)
            card.SetTempEffectValue(Mathf.CeilToInt(card.GetTempEffectValue() / (float)target.Count));
        else
            card.SetTempEffectValue(Mathf.CeilToInt(card.effectValue[effectIndex] / (float)target.Count));

        int output = 0;
        card.SetTempEffectValue(Mathf.CeilToInt(card.effectValue[effectIndex] / (float)target.Count));
        foreach (GameObject t in target)
            output += new EffectFactory().GetEffect(Card.EffectType.ArmorDamage).GetSimulatedArmorDamage(caster, effectController, new List<GameObject> { t }, card, effectIndex);

        card.SetTempEffectValue(originalTempValue);
        return output;
    }

    public override SimHealthController SimulateProcess(GameObject caster, CardEffectsController effectController, Vector2 location, int value, int duration, SimHealthController simH)
    {
        throw new System.NotImplementedException();
    }
}

