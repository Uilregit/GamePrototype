using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffFactory : MonoBehaviour
{
    private Buff buff;

    public int duration;
    public int cardValue;

    public string cardName = "";
    public string casterColor = "";
    public string casterName;
    public int triggerCount = 1;

    public void SetBuff(Buff newBuff)
    {
        buff = newBuff;
    }

    public Buff.BuffEffectType GetTriggerEffectType()
    {
        return buff.onTriggerEffects;
    }

    public virtual void OnApply(HealthController healthController, int value, int newDuration, bool fromRelic, List<BuffFactory> traceList = null)
    {
        healthController.buffController.AddBuff(this);
        cardValue = value;
        duration = newDuration;

        int vitDamage = 0;
        int armorDamage = 0;

        switch (buff.onApplyEffects)
        {
            case Buff.BuffEffectType.None:
                break;
            case Buff.BuffEffectType.VitDamage:
                vitDamage += healthController.GetSimulatedVitDamage(GetValue(value));
                armorDamage += healthController.GetSimulatedArmorDamage(GetValue(value));
                healthController.TakeVitDamage(GetValue(cardValue), healthController, traceList);
                break;
            case Buff.BuffEffectType.PiercingDamage:
                vitDamage += healthController.GetSimulatedPiercingDamage(GetValue(value));
                healthController.TakePiercingDamage(GetValue(cardValue), healthController, traceList);
                break;
            case Buff.BuffEffectType.ArmorDamage:
                armorDamage += healthController.GetSimulatedArmorDamage(GetValue(value));
                healthController.TakeArmorDamage(GetValue(cardValue), healthController, traceList);
                break;
            case Buff.BuffEffectType.VitDamageMultiplier:
                healthController.AddVitDamageMultiplier(cardValue);
                break;
            case Buff.BuffEffectType.HealingMultiplier:
                healthController.AddHealingMultiplier(cardValue);
                break;
            case Buff.BuffEffectType.ArmorDamageMultiplier:
                healthController.AddArmorDamageMultiplier(cardValue);
                break;
            case Buff.BuffEffectType.BonusAttack:
                healthController.SetBonusAttack(cardValue);
                break;
            case Buff.BuffEffectType.BonusArmor:
                healthController.SetBonusArmor(cardValue);
                break;
            case Buff.BuffEffectType.BonusCastRange:
                healthController.SetBonusCastRange(cardValue);
                break;
            case Buff.BuffEffectType.BonusMoveRange:
                healthController.SetBonusMoveRange(cardValue);
                break;
            case Buff.BuffEffectType.CharEnergyCap:
                healthController.SetEnergyCostCap(cardValue);
                break;
            case Buff.BuffEffectType.CharManaCap:
                healthController.SetManaCostCap(cardValue);
                break;
            case Buff.BuffEffectType.CharEnergyReduction:
                healthController.SetEnergyCostReduction(cardValue);
                break;
            case Buff.BuffEffectType.CharManaReduction:
                healthController.SetManaCostReduction(cardValue);
                break;
            case Buff.BuffEffectType.PartyEnergyCap:
                TurnController.turnController.SetEnergyCostCap(cardValue);
                break;
            case Buff.BuffEffectType.PartyManaCap:
                TurnController.turnController.SetManaCostCap(cardValue);
                break;
            case Buff.BuffEffectType.PartyEnergyReduction:
                TurnController.turnController.SetEnergyCostCap(cardValue);
                break;
            case Buff.BuffEffectType.PartyManaReduction:
                TurnController.turnController.SetManaCostCap(cardValue);
                break;
            case Buff.BuffEffectType.Disarm:
                healthController.SetDisarmed(true);
                break;
            case Buff.BuffEffectType.Silence:
                healthController.SetSilenced(true);
                break;
            case Buff.BuffEffectType.Stun:
                healthController.SetStunned(true);
                break;
            case Buff.BuffEffectType.Retaliate:
                healthController.SetRetaliate(cardValue);
                break;
            case Buff.BuffEffectType.Preserve:
                healthController.SetPreserveBonusVit(true);
                break;
            case Buff.BuffEffectType.Enfeeble:
                healthController.SetEnfeeble(cardValue);
                break;
            case Buff.BuffEffectType.ApplyBuff:
                BuffFactory thisBuff = new BuffFactory();
                thisBuff.SetBuff(buff.appliedBuff);
                thisBuff.OnApply(healthController, cardValue, buff.appliedBuffDuration, false);
                break;
            default:
                Debug.Log(buff.onApplyEffects);
                Debug.Log("Apply Not implimented");
                break;
        }

        foreach (EnemyController enemy in TurnController.turnController.GetEnemies())
            enemy.GetComponent<EnemyInformationController>().RefreshIntent();

        InformationLogger.infoLogger.SaveCombatInfo(InformationLogger.infoLogger.patchID,
                                    InformationLogger.infoLogger.gameID,
                                    RoomController.roomController.selectedLevel.ToString(),
                                    RoomController.roomController.roomName,
                                    TurnController.turnController.turnID.ToString(),
                                    TurnController.turnController.GetNumerOfCardsPlayedInTurn().ToString(),
                                    casterColor,
                                    cardName,
                                    "False",
                                    "False",
                                    "False",
                                    "True",
                                    casterName,
                                    1.ToString(),
                                    healthController.name,
                                    vitDamage.ToString(),
                                    armorDamage.ToString(),
                                    0.ToString(),
                                    0.ToString(),
                                    triggerCount.ToString());
    }

    //public abstract IEnumerator Trigger(HealthController selfHealthController, HealthController attackerHealthController, int value, List<Buff> buffTrace);
    public virtual IEnumerator Trigger(HealthController selfHealthController, HealthController attackerHealthController, int value, List<BuffFactory> traceList)
    {
        HealthController target = null;
        if (buff.onTriggerTarget == Buff.TriggerTarget.Self)
            target = selfHealthController;
        else
            target = attackerHealthController;

        if (traceList == null)
            traceList = new List<BuffFactory>();
        traceList.Add(this);

        int vitDamage = 0;
        int armorDamage = 0;

        switch (buff.onTriggerEffects)
        {
            case Buff.BuffEffectType.None:
                break;
            case Buff.BuffEffectType.VitDamage:
                vitDamage += target.GetSimulatedVitDamage(GetValue(value));
                armorDamage += target.GetSimulatedArmorDamage(GetValue(value));
                target.TakeVitDamage(GetValue(value), selfHealthController, traceList);
                break;
            case Buff.BuffEffectType.PiercingDamage:
                vitDamage += target.GetSimulatedPiercingDamage(GetValue(value));
                target.TakePiercingDamage(GetValue(value), selfHealthController, traceList);
                break;
            case Buff.BuffEffectType.ArmorDamage:
                armorDamage += target.GetSimulatedArmorDamage(GetValue(value));
                target.TakeArmorDamage(GetValue(value), selfHealthController, traceList);
                break;
            case Buff.BuffEffectType.VitDamageMultiplier:
                target.AddVitDamageMultiplier(GetValue(value));
                break;
            case Buff.BuffEffectType.HealingMultiplier:
                target.AddHealingMultiplier(GetValue(value));
                break;
            case Buff.BuffEffectType.ArmorDamageMultiplier:
                target.AddArmorDamageMultiplier(GetValue(value));
                break;
            case Buff.BuffEffectType.ApplyBuff:
                BuffFactory thisBuff = new BuffFactory();
                thisBuff.SetBuff(buff.appliedBuff);
                thisBuff.OnApply(target, GetValue(value), buff.appliedBuffDuration, false, traceList);
                break;
            default:
                Debug.Log(buff.onApplyEffects);
                Debug.Log("Trigger Not implimented");
                break;
        }

        InformationLogger.infoLogger.SaveCombatInfo(InformationLogger.infoLogger.patchID,
                                            InformationLogger.infoLogger.gameID,
                                            RoomController.roomController.selectedLevel.ToString(),
                                            RoomController.roomController.roomName,
                                            TurnController.turnController.turnID.ToString(),
                                            TurnController.turnController.GetNumerOfCardsPlayedInTurn().ToString(),
                                            casterColor,
                                            cardName,
                                            "False",
                                            "False",
                                            "False",
                                            "True",
                                            casterName,
                                            1.ToString(),
                                            selfHealthController.name,
                                            vitDamage.ToString(),
                                            armorDamage.ToString(),
                                            0.ToString(),
                                            0.ToString(),
                                            triggerCount.ToString());
        triggerCount += 1;

        yield return new WaitForSeconds(0);
    }
    public virtual void Revert(HealthController healthController)
    {
        int vitDamage = 0;
        int armorDamage = 0;

        switch (buff.onApplyEffects)
        {
            case Buff.BuffEffectType.None:
                break;
            case Buff.BuffEffectType.VitDamage:
                vitDamage += healthController.GetSimulatedVitDamage(-cardValue);
                armorDamage += healthController.GetSimulatedArmorDamage(-cardValue);
                healthController.TakeVitDamage(-cardValue, healthController);
                break;
            case Buff.BuffEffectType.PiercingDamage:
                vitDamage += healthController.GetSimulatedPiercingDamage(-cardValue);
                healthController.TakePiercingDamage(-cardValue, healthController);
                break;
            case Buff.BuffEffectType.ArmorDamage:
                armorDamage += healthController.GetSimulatedArmorDamage(-cardValue);
                healthController.TakeArmorDamage(-cardValue, healthController);
                break;
            case Buff.BuffEffectType.VitDamageMultiplier:
                healthController.RemoveVitDamageMultiplier(-cardValue);
                break;
            case Buff.BuffEffectType.HealingMultiplier:
                healthController.RemoveHealingMultiplier(-cardValue);
                break;
            case Buff.BuffEffectType.ArmorDamageMultiplier:
                healthController.RemoveArmorDamageMultiplier(-cardValue);
                break;
            case Buff.BuffEffectType.BonusAttack:
                healthController.SetBonusAttack(-cardValue);
                break;
            case Buff.BuffEffectType.BonusArmor:
                healthController.SetBonusArmor(-cardValue);
                break;
            case Buff.BuffEffectType.BonusCastRange:
                healthController.SetBonusCastRange(-cardValue);
                break;
            case Buff.BuffEffectType.CharEnergyCap:
                healthController.SetEnergyCostCap(-cardValue);
                break;
            case Buff.BuffEffectType.CharManaCap:
                healthController.SetManaCostCap(-cardValue);
                break;
            case Buff.BuffEffectType.CharEnergyReduction:
                healthController.SetEnergyCostReduction(-cardValue);
                break;
            case Buff.BuffEffectType.CharManaReduction:
                healthController.SetManaCostReduction(-cardValue);
                break;
            case Buff.BuffEffectType.PartyEnergyCap:
                TurnController.turnController.SetEnergyCostCap(-cardValue);
                break;
            case Buff.BuffEffectType.PartyManaCap:
                TurnController.turnController.SetManaCostCap(-cardValue);
                break;
            case Buff.BuffEffectType.PartyEnergyReduction:
                TurnController.turnController.SetEnergyCostCap(-cardValue);
                break;
            case Buff.BuffEffectType.PartyManaReduction:
                TurnController.turnController.SetManaCostCap(-cardValue);
                break;
            case Buff.BuffEffectType.Disarm:
                healthController.SetDisarmed(false);
                break;
            case Buff.BuffEffectType.Silence:
                healthController.SetSilenced(false);
                break;
            case Buff.BuffEffectType.Stun:
                healthController.SetStunned(false);
                break;
            case Buff.BuffEffectType.Retaliate:
                healthController.SetRetaliate(-cardValue);
                break;
            case Buff.BuffEffectType.Preserve:
                healthController.SetPreserveBonusVit(false);
                break;
            case Buff.BuffEffectType.Enfeeble:
                healthController.SetEnfeeble(-cardValue);
                break;
            case Buff.BuffEffectType.BonusMoveRange:
                healthController.SetBonusMoveRange(-cardValue);
                break;
            default:
                Debug.Log(buff.onApplyEffects);
                Debug.Log("Revert Not implimented");
                break;
        }

        InformationLogger.infoLogger.SaveCombatInfo(InformationLogger.infoLogger.patchID,
                            InformationLogger.infoLogger.gameID,
                            RoomController.roomController.selectedLevel.ToString(),
                            RoomController.roomController.roomName,
                            TurnController.turnController.turnID.ToString(),
                            TurnController.turnController.GetNumerOfCardsPlayedInTurn().ToString(),
                            casterColor,
                            cardName,
                            "False",
                            "False",
                            "False",
                            "True",
                            casterName,
                            1.ToString(),
                            healthController.name,
                            vitDamage.ToString(),
                            armorDamage.ToString(),
                            0.ToString(),
                            0.ToString(),
                            triggerCount.ToString());
    }

    private int GetValue(int triggerValue)
    {
        int output = 0;
        switch (buff.valueManipulationType)
        {
            case Buff.ValueManipulationType.TriggerValue:
                output = triggerValue;
                break;
            case Buff.ValueManipulationType.NegativeTriggerValue:
                output = -triggerValue;
                break;
            case Buff.ValueManipulationType.CardValue:
                output = cardValue;
                break;
            case Buff.ValueManipulationType.NegativeCardValue:
                output = -cardValue;
                break;
            case Buff.ValueManipulationType.Percentage:
                output = -Mathf.CeilToInt(triggerValue * cardValue / 100.0f);
                break;
            case Buff.ValueManipulationType.NegativePercentage:
                output = -Mathf.CeilToInt(triggerValue * cardValue / 100.0f);
                break;
            default:
                output = triggerValue;
                break;
        }
        return output;
    }

    public virtual Buff GetCopy()
    {
        Buff output = new Buff();
        output.tempValue = buff.tempValue;
        output.duration = buff.duration;
        //output.value = value;
        output.color = buff.color;
        output.description = buff.description;
        output.SetTriggerType(buff.GetTriggerType());
        output.durationType = buff.durationType;

        return output;
    }

    public Buff.TriggerType GetTriggerType()
    {
        return buff.GetTriggerType();
    }

    public Buff.DurationType GetDurationType()
    {
        return buff.durationType;
    }


    public virtual Color GetIconColor()
    {
        return buff.color;
    }

    public virtual string GetDescription()
    {
        return buff.description.Replace("{+-v}", cardValue.ToString("+#;-#;0")).Replace("{|v|}", Mathf.Abs(cardValue).ToString()).Replace("{v}", cardValue.ToString());
    }

    public virtual void MultiplyValue(HealthController health, int multiplier)
    {
        int diff = cardValue = Mathf.CeilToInt(cardValue * multiplier / 100.0f) - cardValue; 
        switch (buff.onApplyEffects)
        {
            case Buff.BuffEffectType.BonusAttack:
                health.SetBonusArmor(diff);
                break;
            case Buff.BuffEffectType.BonusArmor:
                health.SetBonusAttack(diff);
                break;
            case Buff.BuffEffectType.BonusCastRange:
                health.SetBonusCastRange(diff);
                break;
            case Buff.BuffEffectType.BonusMoveRange:
                health.SetBonusMoveRange(diff);
                break;
            case Buff.BuffEffectType.CharEnergyReduction:
                health.SetEnergyCostReduction(diff);
                break;
            case Buff.BuffEffectType.CharManaReduction:
                health.SetManaCostReduction(diff);
                break;
            case Buff.BuffEffectType.PartyEnergyReduction:
                TurnController.turnController.SetEnergyReduction(diff);
                break;
            case Buff.BuffEffectType.PartyManaReduction:
                TurnController.turnController.SetManaReduction(diff);
                break;
        }
        cardValue = Mathf.CeilToInt(cardValue * multiplier / 100.0f);
    }

    public virtual void AddToValue(HealthController health, int addition)
    {
        int diff = addition;
        switch (buff.onApplyEffects)
        {
            case Buff.BuffEffectType.BonusAttack:
                health.SetBonusArmor(diff);
                break;
            case Buff.BuffEffectType.BonusArmor:
                health.SetBonusAttack(diff);
                break;
            case Buff.BuffEffectType.BonusCastRange:
                health.SetBonusCastRange(diff);
                break;
            case Buff.BuffEffectType.BonusMoveRange:
                health.SetBonusMoveRange(diff);
                break;
            case Buff.BuffEffectType.CharEnergyReduction:
                health.SetEnergyCostReduction(diff);
                break;
            case Buff.BuffEffectType.CharManaReduction:
                health.SetManaCostReduction(diff);
                break;
            case Buff.BuffEffectType.PartyEnergyReduction:
                TurnController.turnController.SetEnergyReduction(diff);
                break;
            case Buff.BuffEffectType.PartyManaReduction:
                TurnController.turnController.SetManaReduction(diff);
                break;
        }
        cardValue += addition;
    }
}
