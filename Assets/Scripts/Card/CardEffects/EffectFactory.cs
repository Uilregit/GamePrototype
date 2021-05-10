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
                case Card.EffectType.ArmorDamage:
                    effects[i] = new ArmorDamageEffect();
                    break;
                case Card.EffectType.VitDamageAll:
                    effects[i] = new VitDamageAll();
                    break;
                case Card.EffectType.ArmorDamageAll:
                    effects[i] = new ArmorDamageAll();
                    break;
                case Card.EffectType.PiercingDamage:
                    effects[i] = new PiercingDamageEffect();
                    break;
                case Card.EffectType.PiercingDamageAll:
                    effects[i] = new PiercingDamageAll();
                    break;
                case Card.EffectType.VitDamageDivided:
                    effects[i] = new VitDamageDivided();
                    break;
                case Card.EffectType.PiercingDamageDivided:
                    effects[i] = new PiercingDamageDivided();
                    break;
                case Card.EffectType.ArmorDamageDivided:
                    effects[i] = new ArmorDamageDivided();
                    break;
                case Card.EffectType.SetKnockBackDamage:
                    effects[i] = new SetKnockBackDamage();
                    break;
                case Card.EffectType.ForcedMovement:
                    effects[i] = new ForcedMovementEffect();
                    break;
                case Card.EffectType.SelfForcedMovement:
                    effects[i] = new SelfForcedMovementEffect();
                    break;
                case Card.EffectType.SetKnockBackSelfBuff:
                    effects[i] = new SetKnockBackSelfBuffEffect();
                    break;
                case Card.EffectType.SetKnockBackOtherBuff:
                    effects[i] = new SetKnockBackOtherBuffEffect();
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
                case Card.EffectType.CreateDuplicateObject:
                    effects[i] = new CreateDuplicateObjectsEffect();
                    break;
                case Card.EffectType.GetCurrentAttack:
                    effects[i] = new GetCurrentAttackEffect();
                    break;
                case Card.EffectType.DrawCards:
                    effects[i] = new DrawCardEffect();
                    break;
                case Card.EffectType.GetCurrentArmor:
                    effects[i] = new GetCurrentArmor();
                    break;
                case Card.EffectType.GetBonusArmor:
                    effects[i] = new GetBonusArmorEffect();
                    break;
                case Card.EffectType.GetNumberOfTargetsInRangeEffect:
                    effects[i] = new GetNumberOfTargetsInRangeEffect();
                    break;
                case Card.EffectType.GetDamageDoneEffect:
                    effects[i] = new GetDamageDoneEffect();
                    break;
                case Card.EffectType.GetNumberOfCardsPlayedInTurn:
                    effects[i] = new GetNumberOfCardsPlayedEffect();
                    break;
                case Card.EffectType.Swap:
                    effects[i] = new SwapEffect();
                    break;
                case Card.EffectType.Teleport:
                    effects[i] = new TeleportEffect();
                    break;
                case Card.EffectType.ManaGain:
                    effects[i] = new ManaGainEffect();
                    break;
                case Card.EffectType.EnergyGain:
                    effects[i] = new EnergyGainEffect();
                    break;
                case Card.EffectType.SetDuration:
                    effects[i] = new SetDurationEffect();
                    break;
                case Card.EffectType.GetNumberInStack:
                    effects[i] = new GetNumberInStackEffect();
                    break;
                case Card.EffectType.GravityEffect:
                    effects[i] = new GravityEffect();
                    break;
                case Card.EffectType.CardCostReductionDrawn:
                    effects[i] = new CardCostReductionDrawn();
                    break;
                case Card.EffectType.DrawManaCards:
                    effects[i] = new DrawManaCardEffect();
                    break;
                case Card.EffectType.DrawEnergyCards:
                    effects[i] = new DrawEnergyCardEffect();
                    break;
                case Card.EffectType.ChangeHandSizeEffect:
                    effects[i] = new ChangeHandSizeEffect();
                    break;
                case Card.EffectType.Sacrifice:
                    effects[i] = new SacrificeEffect();
                    break;
                case Card.EffectType.GetNumberOfAttackers:
                    effects[i] = new GetNumberOfAttackersEffect();
                    break;
                case Card.EffectType.ModifyTempValue:
                    effects[i] = new ModifyTempValueEffect();
                    break;
                case Card.EffectType.StealCardEffect:
                    effects[i] = new StealCardEffect();
                    break;
                case Card.EffectType.GetStarterCardEffect:
                    effects[i] = new GetStarterCardEffect();
                    break;
                case Card.EffectType.GetDrawnCardEnergy:
                    effects[i] = new GetDrawnCardEnergy();
                    break;
                case Card.EffectType.GetDrawnCardMana:
                    effects[i] = new GetDrawnCardMana();
                    break;
                case Card.EffectType.CopyBuffEffect:
                    effects[i] = new CopyBuffsEffect();
                    break;
                case Card.EffectType.GiveBuffEffect:
                    effects[i] = new GiveBuffsEffect();
                    break;
                case Card.EffectType.GetNumberOfCardsInHand:
                    effects[i] = new GetNumberOfCardsInHand();
                    break;
                case Card.EffectType.GetBonusHealth:
                    effects[i] = new GetBonusVit();
                    break;
                case Card.EffectType.ForceMovementFromCenter:
                    effects[i] = new ForceMovementFromCastLocation();
                    break;
                case Card.EffectType.GetManaSpentTurn:
                    effects[i] = new GetManaSpentTurn();
                    break;
                case Card.EffectType.GetEnergySpentTurn:
                    effects[i] = new GetEnergySpentTurn();
                    break;
                case Card.EffectType.GetHighestHealthAlly:
                    effects[i] = new GetHighestHealthAlly();
                    break;
                case Card.EffectType.AssimilateStatsEffect:
                    effects[i] = new AssimilateStatsEffect();
                    break;
                case Card.EffectType.CopyStatsEffect:
                    effects[i] = new CopyStatsEffect();
                    break;
                case Card.EffectType.GetDistanceMoved:
                    effects[i] = new GetDistanceMovedEffect();
                    break;
                case Card.EffectType.DrawLastPlayedCardEffect:
                    effects[i] = new DrawLastPlayedCardEffect();
                    break;
                case Card.EffectType.CardCostCapDrawn:
                    effects[i] = new CardCostCapDrawn();
                    break;
                case Card.EffectType.CreateANYEnergyCard:
                    effects[i] = new CreateANYEnergyCard();
                    break;
                case Card.EffectType.CreateANYManaCard:
                    effects[i] = new CreateANYManaCard();
                    break;
                case Card.EffectType.ManifestDrawCards:
                    effects[i] = new ManifestDrawEffect();
                    break;
                case Card.EffectType.ManifestDiscardCards:
                    effects[i] = new ManifestDiscardEffect();
                    break;
                case Card.EffectType.ManifestANYEnergyCardEffect:
                    effects[i] = new ManifestANYEnergyCardEffect();
                    break;
                case Card.EffectType.ModifyBuffDuration:
                    effects[i] = new ModifyBuffDurationEffect();
                    break;
                case Card.EffectType.GetNumberOfBuffsOnTarget:
                    effects[i] = new GetNumberOfBuffsEffect();
                    break;
                case Card.EffectType.BuffValueAdd:
                    effects[i] = new BuffValueAdd();
                    break;
                case Card.EffectType.BuffValueMultiply:
                    effects[i] = new BuffValueMultiply();
                    break;
                case Card.EffectType.AbsoluteDamage:
                    effects[i] = new AbsoluteDamageEffect();
                    break;
                case Card.EffectType.Resurrect:
                    effects[i] = new ResurrectEffect();
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
