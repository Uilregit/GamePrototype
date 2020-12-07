using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyBuffEffect : Effect
{
    public override IEnumerator Process(GameObject caster, CardEffectsController effectController, List<GameObject> target, Card card, int effectIndex)
    {
        foreach (GameObject targ in target)
        {
            HealthController targetH = targ.GetComponent<HealthController>();
            //Buff buff = GetBuff(card.buffType[effectIndex]);
            BuffFactory buff = new BuffFactory();
            buff.SetBuff(card.buff[effectIndex]);
            buff.cardName = card.name;
            buff.casterColor = card.casterColor.ToString();
            buff.casterName = caster.name;
            HealthController originator = caster.GetComponent<HealthController>();
            if (card.GetTempObject() != null)
                originator = card.GetTempObject().GetComponent<HealthController>();
            if (card.effectValue[effectIndex] != 0)
                buff.OnApply(targetH, originator, card.effectValue[effectIndex], card.effectDuration[effectIndex], false, null, null);
            else
                buff.OnApply(targetH, originator, card.GetTempEffectValue(), card.effectDuration[effectIndex], false, null, null);
            if (buff.GetTriggerEffectType() == Buff.BuffEffectType.VitDamage || buff.GetTriggerEffectType() == Buff.BuffEffectType.PiercingDamage)
                caster.GetComponent<BuffController>().StartCoroutine(caster.GetComponent<BuffController>().TriggerBuff(Buff.TriggerType.OnDamageDealt, caster.GetComponent<HealthController>(), 0));
        }
        yield return new WaitForSeconds(0);
    }

    public override SimHealthController SimulateProcess(GameObject caster, CardEffectsController effectController, Vector2 location, int value, int duration, SimHealthController simH)
    {
        throw new System.NotImplementedException();
    }

    public override void RelicProcess(List<GameObject> targets, Buff buff, int effectValue, int effectDuration, List<Relic> traceList)
    {
        foreach (GameObject targ in targets)
        {
            HealthController targetH = targ.GetComponent<HealthController>();
            BuffFactory buffFactory = new BuffFactory();
            buffFactory.SetBuff(buff);
            buffFactory.OnApply(targetH, null, effectValue, effectDuration, true, null, traceList);
        }
    }
}
