using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Card : ScriptableObject
{
    public enum Rarity { Common, Rare};
    public bool exhaust = false;
    public Rarity rarity;
    public new string name;
    public int manaCost;
    public int range;
    public int radius;
    public enum CasterColor { Red, Blue, Green, Enemy, Gray };
    public CasterColor casterColor;
    public string description;
    public Sprite art;
    //Who viable targets of the card during cast
    public enum CastType { Any, Enemy, Player, None, All, AoE , EmptySpace};
    public CastType castType;
    public enum CastShape { Circle, Plus, None };
    public CastShape castShape;
    //Who the target of the effect is
    public enum TargetType { Enemy, Player, Self, Any, AllEnemies, AllPlayers, None};
    public TargetType[] targetType = new TargetType[1];
    //Name of the effect of the card
    public enum EffectType
    {
        VitDamage = 0, ShieldDamage = 1, VitDamageAll = 2, ShieldDamageAll = 3, PiercingDamage = 4, PiercingDamageAll = 5,
        SetKnockBackDamage = 7, ForcedMovement = 8, TauntEffect = 9, GetMissingHealth = 10, Buff = 12, Cleanse = 13,
        CreateObject = 14, GetCurrentAttack = 11, DrawCards = 15
    }
    public EffectType[] cardEffectName = new EffectType[1];

    public enum ConditionType
    {
        None, Odd, Even
    }
    public ConditionType[] conditionType = new ConditionType[1];
    public int[] conditionValue = new int[1];

    //The buff used if the card bestoes a buff or debuff
    public enum BuffType { None, Stun, AttackChange, ArmorBuff, EnfeebleDebuff, MoveRangeBuff, RetaliateBuff }
    public BuffType[] buffType = new BuffType[1];

    //Value of the effect. ie. deal *1* damage
    public int[] effectValue = new int[1];
    public int[] effectDuration = new int[1];

    public GameObject[] spawnObject = new GameObject[1];

    public Card[] cards = new Card[1];

    private int tempEffectValue = 0;
    public void SetTempEffectValue(int value)
    {
        tempEffectValue = value;
    }

    public int GetTempEffectValue()
    {
        return tempEffectValue;
    }
}
