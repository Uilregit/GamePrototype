using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyBuffEffect : Effect
{
    public override void Process(GameObject caster, CardEffectsController effectController, GameObject target, Card card, int effectIndex)
    {
        HealthController targetH = target.GetComponent<HealthController>();
        GetBuff(card.buffType[effectIndex]).OnApply(targetH, card.effectValue[effectIndex], card.effectDuration[effectIndex]);
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
            default:
                return null;
        }
    }
}
