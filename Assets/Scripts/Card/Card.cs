using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Card : ScriptableObject
{
    public enum Rarity { Common, Rare, Legendary, Starter };
    public bool exhaust = false;
    public bool returnOnCancel = true;
    public Rarity rarity;
    public new string name;
    public int energyCost;
    public int manaCost;
    public int range = 1;
    public int radius;
    public enum CasterColor { Red, Blue, Green, Enemy, Gray };
    public CasterColor casterColor;
    [TextArea]
    public string description;
    public Sprite art;
    //Who viable targets of the card during cast
    public enum CastType { Any, Enemy, Player, None, All, AoE, EmptySpace, TargetedAoE };
    public CastType castType;
    public enum CastShape { Circle, Plus, None };
    public CastShape castShape;
    //Who the target of the effect is
    public enum TargetType { Enemy, Player, Self, Any, AllEnemies, AllPlayers, None };
    public TargetType[] targetType = new TargetType[1];
    //Name of the effect of the card
    public enum EffectType
    {
        VitDamage = 0, ShieldDamage = 1, VitDamageAll = 2, ShieldDamageAll = 3, PiercingDamage = 4, PiercingDamageAll = 5,
        SetKnockBackDamage = 7, ForcedMovement = 8, TauntEffect = 9, GetMissingHealth = 10, Buff = 13, Cleanse = 14,
        CreateObject = 15, GetCurrentAttack = 11, DrawCards = 16, GetCurrentShield = 12, GetNumberOfTargetsInRangeEffect = 17,
        GetDamageDoneEffect, GetNumberOfCardsPlayedInTurn, Swap, Teleport, ManaGain, EnergyGain, GetBonusShield, SetDuration, GetNumberInStack,
        GravityEffect, DrawManaCards, DrawEnergyCards, CardCostReductionDrawn, CardCostReductionRandom, Sacrifice, GetNumberOfAttackers, ModifyTempValue
    }
    public EffectType[] cardEffectName = new EffectType[1];

    public enum ConditionType
    {
        None, Odd, Even, OnPlay, TargetBroken, TargetNotBroken, CasterBroken, CasterNotBroken, PreviousEffectSuccessful, CasterHasHigherShield, CasterHasLowerShield, Else
    }
    public ConditionType[] conditionType = new ConditionType[1];
    public int[] conditionValue = new int[1];

    //The buff used if the card bestoes a buff or debuff
    public enum BuffType
    {
        None, Stun, AttackChange, ArmorBuff, EnfeebleDebuff, MoveRangeBuff, RetaliateBuff, DoubleDamageBuff, BarrierBuff, ProtectBuff,
        EnergyCostCapTurnBuff, ManaCostCapTurnBuff, EnergyCostReductionBuff, ManaCostReductionBuff
    }
    public BuffType[] buffType = new BuffType[1];

    //Value of the effect. ie. deal *1* damage
    public int[] effectValue = new int[1];
    public int[] effectDuration = new int[1];

    public GameObject[] spawnObject = new GameObject[1];

    public Card[] cards = new Card[1];

    public enum IndicatorType
    {
        Attack, Guard, Buff, Debuff, Other
    }
    public IndicatorType indicatorType;
    public EnemyController.TargetType targetBehaviour = EnemyController.TargetType.Default;
    public string indicatorMultiplier;
    public int executionPriority = 0;

    private Vector2 tempCenter;
    private int tempEffectValue = 0;
    private int tempDuration = 0;
    private bool previousEffectSuccessful = true;
    private bool previousConditionTrue = true;
    private int damageDone = 0;

    public void SetPreviousConditionTrue (bool value)
    {
        previousConditionTrue = value;
    }

    public bool GetPreviousConditionTrue ()
    {
        return previousConditionTrue;
    }

    public void SetPreviousEffectSuccessful(bool newBool)
    {
        previousEffectSuccessful = newBool;
    }

    public bool GetPreviousEffectSuccessful()
    {
        return previousEffectSuccessful;
    }

    public void SetCenter(Vector2 loc)
    {
        tempCenter = loc;
    }

    public Vector2 GetCenter()
    {
        return tempCenter;
    }

    public void SetTempEffectValue(int value)
    {
        tempEffectValue = value;
    }

    public int GetTempEffectValue()
    {
        return tempEffectValue;
    }

    public void SetTempDuration(int value)
    {
        tempDuration = value;
    }

    public int GetTempDuration()
    {
        return tempDuration;
    }

    public void SetDamageDone(int value)
    {
        damageDone = value;
    }

    public int GetDamageDone()
    {
        return damageDone;
    }
}
