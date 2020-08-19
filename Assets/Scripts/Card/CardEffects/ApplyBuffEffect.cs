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
            if (card.effectValue[effectIndex] != 0)
                GetBuff(card.buffType[effectIndex]).OnApply(targetH, card.effectValue[effectIndex], card.effectDuration[effectIndex], false);
            else
                GetBuff(card.buffType[effectIndex]).OnApply(targetH, card.GetTempEffectValue(), card.effectDuration[effectIndex], false);
        }
        yield return new WaitForSeconds(0);
    }

    public override SimHealthController SimulateProcess(GameObject caster, CardEffectsController effectController, Vector2 location, int value, int duration, SimHealthController simH)
    {
        throw new System.NotImplementedException();
    }

    private Buff GetBuff(Card.BuffType buff)
    {
        switch (buff)
        {
            case Card.BuffType.Stun:
                return new StunDebuff();
            case Card.BuffType.AttackChange:
                return new AttackChangeBuff();
            case Card.BuffType.ArmorBuff:
                return new ArmorBuff();
            case Card.BuffType.EnfeebleDebuff:
                return new EnfeebleDebuff();
            case Card.BuffType.MoveRangeBuff:
                return new MoveRangeBuff();
            case Card.BuffType.RetaliateBuff:
                return new RetaliateBuff();
            case Card.BuffType.DoubleDamageBuff:
                return new DoubleDamageDebuff();
            case Card.BuffType.BarrierBuff:
                return new BarrierBuff();
            case Card.BuffType.ProtectBuff:
                return new ProtectBuff();
            case Card.BuffType.EnergyCostCapTurnBuff:
                return new EnergyCostCapTurnBuff();
            case Card.BuffType.ManaCostCapTurnBuff:
                return new ManaCostCapTurnBuff();
            case Card.BuffType.EnergyCostReductionBuff:
                return new EnergyCostReductionBuff();
            case Card.BuffType.ManaCostReductionBuff:
                return new ManaCostReductionBuff();
            default:
                return null;
        }
    }

    public override void RelicProcess(List<GameObject> targets, Card.BuffType buffType, int effectValue, int effectDuration)
    {
        foreach (GameObject targ in targets)
        {
            HealthController targetH = targ.GetComponent<HealthController>();
            GetBuff(buffType).OnApply(targetH, effectValue, effectDuration, true);
        }
    }
}
