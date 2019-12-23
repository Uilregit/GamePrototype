using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectFactory
{
    //Change card.effectType and this
    public Effect[] GetEffects(Card.EffectType[] effectNames)
    {
        Effect[] effects = new Effect[effectNames.Length];
        for (int i = 0; i < effectNames.Length; i++)
            switch (effectNames[i])
            {
                case Card.EffectType.VitDamage:
                    effects[i] = new VitDamageEffect();
                    break;
                case Card.EffectType.ShieldDamage:
                    effects[i] = new ShieldDamageEffect();
                    break;
                case Card.EffectType.VitDamageAll:
                    effects[i] = new VitDamageAll();
                    break;
                case Card.EffectType.ShieldDamageAll:
                    effects[i] = new ShieldDamageAll();
                    break;
                case Card.EffectType.PiercingDamage:
                    effects[i] = new PiercingDamageEffect();
                    break;
                case Card.EffectType.PiercingDamageAll:
                    effects[i] = new PiercingDamageAll();
                    break;
                case Card.EffectType.SetKnockBackDamage:
                    effects[i] = new SetKnockBackDamage();
                    break;
                case Card.EffectType.ForcedMovement:
                    effects[i] = new ForcedMovementEffect();
                    break;
                case Card.EffectType.TauntEffect:
                    effects[i] = new TauntEffect();
                    break;
                case Card.EffectType.GetMissingHealth:
                    effects[i] = new GetMissingHealth();
                    break;
                case Card.EffectType.Buff:
                    effects[i] = new ApplyBuffEffect();
                    break;
                case Card.EffectType.Cleanse:
                    effects[i] = new CleanseEffect();
                    break;
                case Card.EffectType.CreateObject:
                    effects[i] = new CreateObjectEffect();
                    break;
                case Card.EffectType.GetCurrentAttack:
                    effects[i] = new GetCurrentAttackEffect();
                    break;
                case Card.EffectType.DrawCards:
                    effects[i] = new DrawCardEffect();
                    break;
                default:
                    effects[i] = null;
                    break;
            }
        return effects;
    }

    public Effect GetEffect(Card.EffectType effectName)
    {
        Card.EffectType[] types = new Card.EffectType[1];
        types[0] = effectName;
        return GetEffects(types)[0];
    }
}
