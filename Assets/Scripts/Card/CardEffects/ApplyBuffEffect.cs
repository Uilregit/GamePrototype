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
            if (card.effectValue[effectIndex] != 0)
                buff.OnApply(targetH, card.effectValue[effectIndex], card.effectDuration[effectIndex], false);
            else
                buff.OnApply(targetH, card.GetTempEffectValue(), card.effectDuration[effectIndex], false);
            if (buff.GetTriggerEffectType() == Buff.BuffEffectType.VitDamage || buff.GetTriggerEffectType() == Buff.BuffEffectType.PiercingDamage)
                caster.GetComponent<BuffController>().StartCoroutine(caster.GetComponent<BuffController>().TriggerBuff(Buff.TriggerType.OnDamageDealt, caster.GetComponent<HealthController>(), 0));
        }
        yield return new WaitForSeconds(0);
    }

    public override SimHealthController SimulateProcess(GameObject caster, CardEffectsController effectController, Vector2 location, int value, int duration, SimHealthController simH)
    {
        throw new System.NotImplementedException();
    }

    /*
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
            case Card.BuffType.PartyEnergyCostCapTurnBuff:
                return new PartyEnergyCostCapTurnBuff();
            case Card.BuffType.PartyManaCostCapTurnBuff:
                return new PartyManaCostCapTurnBuff();
            case Card.BuffType.PartyEnergyCostReductionBuff:
                return new PartyEnergyCostReductionBuff();
            case Card.BuffType.PartyManaCostReductionBuff:
                return new PartyManaCostReductionBuff();
            case Card.BuffType.VitDamageOverTime:
                return new VitDamageOverTime();
            case Card.BuffType.ArmorDamageOverTime:
                return new ArmorDamageOverTime();
            case Card.BuffType.PiercingDamageOverTime:
                return new PiercingDamageOverTime();
            case Card.BuffType.LifestealBuff:
                return new LifeStealBuff();
            case Card.BuffType.DivineArmorBuff:
                return new DivineArmorBuff();
            case Card.BuffType.CastRangeBuff:
                return new CastRangeBuff();
            case Card.BuffType.BonusHealing:
                return new BonusHealingBuff();
            case Card.BuffType.AmplifyDamageTurn:
                return new AmplifyVitDamageTurn();
            case Card.BuffType.AmplifyHealingTurn:
                return new AmplifyHealingTurn();
            case Card.BuffType.HealAttacker:
                return new HealAttacker();
            case Card.BuffType.Silence:
                return new SilenceBuff();
            case Card.BuffType.Disarm:
                return new DisarmBuff();
            case Card.BuffType.AttackChangeOnHeal:
                return new AttackChangeOnHealBuff();
            case Card.BuffType.RuptureBuff:
                return new RuptureBuff();
            case Card.BuffType.CriticalStrike:
                return new CriticalStrikeBuff();
            case Card.BuffType.AdditionalPiercingDamage:
                return new AdditionalPiercingDamageBuff();
            case Card.BuffType.Preserve:
                return new PreserveBuff();
            case Card.BuffType.AdditionalHealing:
                return new AdditionalHealingBuff();
            case Card.BuffType.CharEnergyCostCapTurnBuff:
                return new CharEnergyCostCapTurnBuff();
            case Card.BuffType.CharManaCostCapTurnBuff:
                return new CharManaCostCapTurnBuff();
            case Card.BuffType.CharEnergyCostReductionBuff:
                return new CharEnergyCostReductionBuff();
            case Card.BuffType.CharManaCostReductionBuff:
                return new CharManaCostReductionBuff();
            default:
                return null;
        }
    }
    */

    public override void RelicProcess(List<GameObject> targets, Buff buff, int effectValue, int effectDuration)
    {
        foreach (GameObject targ in targets)
        {
            HealthController targetH = targ.GetComponent<HealthController>();
            BuffFactory buffFactory = new BuffFactory();
            buffFactory.SetBuff(buff);
            buffFactory.OnApply(targetH, effectValue, effectDuration, true);
        }
    }
}
