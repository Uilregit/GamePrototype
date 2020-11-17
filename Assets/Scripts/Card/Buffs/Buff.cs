using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[CreateAssetMenu]
public class Buff : ScriptableObject
{
    public Color color = Color.white;
    [TextArea]
    public string description = "";
    [SerializeField]
    private TriggerType triggerType;
    public DurationType durationType;

    public BuffEffectType onApplyEffects;
    public TriggerTarget onTriggerTarget;
    public BuffEffectType onTriggerEffects;
    public ValueManipulationType valueManipulationType;
    //public BuffEffectType onRevertEffects;

    public Buff appliedBuff;
    public int appliedBuffDuration;

    public int effectValue;

    public int tempValue = 0;

    public int duration;
    public int value;

    public virtual void OnApply(HealthController healthController, int value, int newDuration, bool fromRelic)
    {
        /*
        healthController.buffController.AddBuff(this);
        tempValue = value;
        duration = newDuration;
        */
    }

    //public abstract IEnumerator Trigger(HealthController selfHealthController, HealthController attackerHealthController, int value, List<Buff> buffTrace);
    public virtual IEnumerator Trigger(HealthController selfHealthController, HealthController attackerHealthController, int value)
    {
        /*
        Debug.Log("triggered");
        HealthController target = null;
        if (onTriggerTarget == TriggerTarget.Self)
            target = selfHealthController;
        else
            target = attackerHealthController;

        switch (onTriggerEffects)
        {
            case BuffEffectType.VitDamage:
                target.TakeVitDamage(GetValue(value), selfHealthController);
                break;
            case BuffEffectType.PiercingDamage:
                target.TakePiercingDamage(GetValue(value), selfHealthController);
                break;
            case BuffEffectType.ArmorDamage:
                target.TakeArmorDamage(GetValue(value), selfHealthController);
                break;
        }


        yield return new WaitForSeconds(TimeController.time.buffTriggerBufferTime * TimeController.time.timerMultiplier);
        */
        yield return new WaitForSeconds(0);
    }
    public virtual void Revert(HealthController healthController)
    {
    }
    /*
    private int GetValue(int triggerValue)
    {
        int output = 0;
        switch (valueManipulationType)
        {
            case ValueManipulationType.TriggerValue:
                output = triggerValue;
                break;
            case ValueManipulationType.NegativeTriggerValue:
                output = -triggerValue;
                break;
            case ValueManipulationType.CardValue:
                output = tempValue;
                break;
            case ValueManipulationType.NegativeCardValue:
                output = -tempValue;
                break;
            case ValueManipulationType.Percentage:
                output = -Mathf.CeilToInt(triggerValue * tempValue / 100.0f);
                break;
            case ValueManipulationType.NegativePercentage:
                output = -Mathf.CeilToInt(triggerValue * tempValue / 100.0f);
                break;
            default:
                output = triggerValue;
                break;
        }
        return output;
    }
    */

    public virtual Buff GetCopy()
    {
        /*
        Buff output = new LifeStealBuff();
        output.tempValue = tempValue;
        output.duration = duration;
        //output.value = value;
        output.color = color;
        output.description = description;
        output.SetTriggerType(GetTriggerType());
        output.durationType = durationType;

        return output;
        */
        return null;
    }


    public TriggerType GetTriggerType()
    {
        return triggerType;
    }

    public void SetTriggerType(TriggerType type)
    {
        triggerType = type;
    }


    public enum TriggerTarget
    {
        Self = 0,
        Attacker = 2
    }

    public enum ValueManipulationType
    {
        CardValue = 0,
        NegativeCardValue = 1,
        TriggerValue = 5,
        NegativeTriggerValue = 6,
        Percentage = 10,
        NegativePercentage = 11,
    }

    public enum TriggerType
    {
        AtEndOfTurn = 0,
        AtStartOfTurn = 1,
        OnApply = 10,

        OnDamageRecieved = 100,
        OnHealingRecieved = 101,
        OnDamageDealt = 110,

        OnShieldDamageRecieved = 120,

        OnMove = 200,

        OnCardPlayed = 300
    }

    public enum BuffEffectType
    {
        None = 0,
        VitDamage = 1,
        PiercingDamage = 2,
        ArmorDamage = 3,
        VitDamageMultiplier = 10,
        HealingMultiplier = 11,
        ArmorDamageMultiplier = 12,
        //AdditionalVitDamage,
        //AdditionalPiercingDamage,
        //AdditionalHealing,

        BonusAttack = 30,
        BonusArmor = 31,
        BonusCastRange = 35,
        BonusMoveRange = 36,

        CharEnergyCap = 40,
        CharManaCap = 41,
        CharEnergyReduction = 42,
        CharManaReduction = 43,
        PartyEnergyCap = 45,
        PartyManaCap = 46,
        PartyEnergyReduction = 47,
        PartyManaReduction = 48,

        Disarm = 60,
        Silence = 61,
        Stun = 70,
        Retaliate = 71,
        Preserve = 72,
        Taunt = 73,

        ApplyBuff = 100
    }

    public enum DurationType
    {
        Turn = 0,
        Use = 1
    }
}
