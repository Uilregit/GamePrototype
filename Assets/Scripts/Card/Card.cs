using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Card : ScriptableObject
{
    public enum Rarity { Common, Rare, Legendary, Starter, StarterAttack };
    public bool exhaust = false;
    public bool canCastOnSelf = true;
    public Rarity rarity;
    public new string name;
    public int energyCost;
    public int manaCost;
    public int range = 1;
    public int radius;
    public enum CasterColor { Red, Blue, Green, Orange, White, Black, Enemy, Gray };
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
        VitDamage = 0,
        VitDamageAll = 1,
        VitDamageDivided = 2,
        AbsoluteDamage = 5,
        ArmorDamage = 10,
        ArmorDamageDivided = 11,
        ArmorDamageAll = 12,
        PiercingDamage = 20,
        PiercingDamageAll = 21,
        PiercingDamageDivided = 22,
        _ = 99,
        
        Cleanse = 100,
        Buff = 110,
        ModifyBuffDuration = 115,
        BuffValueAdd = 117,
        BuffValueMultiply = 118,
        CopyBuffEffect = 120,
        GiveBuffEffect = 121,
        CopyStatsEffect = 130,
        AssimilateStatsEffect = 131,
        __ = 199,
        
        SetKnockBackDamage = 200, 
        ForcedMovement = 201,
        ForceMovementFromCenter = 202,
        Swap = 210,
        Teleport = 211,
        GravityEffect = 220,
        ___ = 299,

        TauntEffect = 300,
        ____ = 399,

        SetDuration = 400,
        ModifyTempValue = 401,
        _____ = 499,

        GetMissingHealth = 500,
        GetBonusHealth = 502,
        GetCurrentAttack = 510,
        GetCurrentArmor = 522,
        GetBonusArmor = 521,
        GetDistanceMoved = 527,
        GetDamageDoneEffect = 530,
        GetNumberOfTargetsInRangeEffect = 540,
        GetNumberOfCardsPlayedInTurn= 541,
        GetNumberOfAttackers = 542,
        GetNumberInStack = 543,
        GetNumberOfBuffsOnTarget = 545,
        GetDrawnCardEnergy = 550,
        GetDrawnCardMana = 551,
        GetNumberOfCardsInHand = 560,
        GetHighestHealthAlly = 570,
        GetManaSpentTurn = 590,
        GetEnergySpentTurn = 591,
        ______ = 599,

        ManaGain = 600, 
        EnergyGain = 601,
        _______ = 699,

        DrawCards = 700,
        DrawManaCards = 701, 
        DrawEnergyCards = 702, 
        CardCostReductionDrawn = 710, 
        CardCostCapDrawn = 711,
        CardCostReductionRandom = 720,
        ________ = 799,

        StealCardEffect = 800,
        GetStarterCardEffect = 801,
        DrawLastPlayedCardEffect = 810,
        CreateANYEnergyCard = 820,
        CreateANYManaCard = 821,
        _________ = 899,

        CreateObject = 5000,
        Sacrifice = 5100,
        Resurrect = 9999
    }
    public EffectType[] cardEffectName = new EffectType[1];

    public enum HitEffect
    {
        PlayerAttack = 0,
        MagicAttack = 50,
        EnemyAttack = 100,
        Buff = 200,
        Debuff = 300,
        Heal = 400,
        Cleanse = 500,
        None = 999999
    }
    public HitEffect[] hitEffect = new HitEffect[1];

    public enum ConditionType
    {
        None = 0,
        Odd = 2, 
        Even = 3,
        
        OnPlay = 100,
        
        TargetBroken = 200, 
        TargetNotBroken = 201,
        TargetAttackingCaster = 202,
        TargetNotAttackingCaster = 203,
        
        CasterBroken = 300, 
        CasterNotBroken = 301, 
        CasterHasHigherArmor = 310,
        CasterHasHigherATK = 311,
        CasterHasLowerArmor = 320,
        CasterHasLowerATK = 321,

        CasterHasBonusATK = 400,
        CasterHasNoBonusATK = 401,

        PreviousEffectSuccessful = 900,
        Else = 1000
    }
    public ConditionType[] conditionType = new ConditionType[1];
    public int[] conditionValue = new int[1];

    public Buff[] buff = new Buff[1];

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

    public enum HighlightCondition
    {
        None = 0,
        
        HasBonusATK = 1,
        HasBonusArmor = 2,
        HasBonusVit = 3,

        HasManaCardInDrawDeck = 10,
        HasEnergyCardInDrawDeck = 15,

        PlayedCardsThisTurn = 20
    }

    public HighlightCondition highlightCondition = HighlightCondition.None;

    private Vector2 tempCenter;
    private int tempEffectValue = 0;
    private int tempDuration = 0;
    private GameObject tempObject;
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

    public void SetTempObject(GameObject obj)
    {
        tempObject = obj;
    }

    public GameObject GetTempObject()
    {
        return tempObject;
    }

    public void SetDamageDone(int value)
    {
        damageDone = value;
    }

    public int GetDamageDone()
    {
        return damageDone;
    }

    public Card GetCopy()
    {
        Card output = new Card();
        output.exhaust = exhaust;
        output.canCastOnSelf = canCastOnSelf;
        output.rarity = rarity;
        output.name = name;
        output.energyCost = energyCost;
        output.manaCost = manaCost;
        output.range = range;
        output.radius = radius;
        output.casterColor = casterColor;
        output.description = description;
        output.art = art;
        output.castType = castType;
        output.castShape = castShape;
        output.targetType = targetType;
        output.cardEffectName = cardEffectName;
        output.conditionType = conditionType;
        output.conditionValue = conditionValue;
        output.hitEffect = hitEffect;
        output.effectValue = effectValue;
        output.effectDuration = effectDuration;
        output.spawnObject = spawnObject;
        output.cards = cards;
        output.indicatorType = indicatorType;
        output.targetBehaviour = targetBehaviour;
        output.indicatorMultiplier = indicatorMultiplier;
        output.executionPriority = executionPriority;

        return output;
    }
}
