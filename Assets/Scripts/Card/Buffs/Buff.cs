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
    public bool triggerModificationProtection = false;
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

    public GameObject obj;
    public Card card;
    private Card[] drawnCards;

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
        Attacker = 2,
        Caster = 3
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

        OnCardPlayed = 300,

        Dummy = 99999
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

        PhasedMovement = 20,

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

        CardDrawChange = 50,
        BonusCast = 55,

        Disarm = 60,
        Silence = 61,
        Stun = 70,
        //Retaliate = 71,
        Preserve = 72,
        Taunt = 73,

        DrawCard = 99,
        ApplyBuff = 100,

        CreateFireTrap = 999
    }

    public enum DurationType
    {
        Turn = 0,
        Use = 1
    }

    public void SetDrawnCards(Card[] cards)
    {
        drawnCards = cards;
    }

    public Card[] GetDrawnCards()
    {
        return drawnCards;
    }
}
