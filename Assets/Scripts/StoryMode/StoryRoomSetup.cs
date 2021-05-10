using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class StoryRoomSetup : ScriptableObject
{
    public string roomName;
    public List<RoomSetup> setups;
    public ChallengeType[] challenges = new ChallengeType[3];
    public ChallengeValueType[] valueType = new ChallengeValueType[3];
    public int[] challengeValues = new int[3];
    public ChallengeComparisonType[] challengeComparisonType = new ChallengeComparisonType[3];

    public RewardsType[] rewardTypes;
    public int[] rewardAmounts;
    public int[] rewardCosts;
    public bool[] challengeRewardBought = new bool[3] { false, false, false };

    [TextArea]
    public string flavorText;

    public enum RewardsType
    {
        BlankCard = 0,
        WeaponBlueprint = 5,

        EnergyShard = 10,
        EnergyGem = 15,
        EnergyCrystal = 19,

        ManaShard = 20,
        ManaGem = 25,
        ManaCrystal = 29,

        RubyShard = 30,
        RubyGem = 35,
        RubyCrystal = 39,

        SapphireShard = 40,
        SapphireGem = 45,
        SapphireCrystal = 49,

        EmeraldShard = 50,
        EmeraldGem = 55,
        EmeraldCrystal = 59,

        SpessartineShard = 60,
        SpessartineGem = 65,
        SpessartineCrystal = 69,

        QuartzShard = 70,
        QuartzGem = 75,
        QuartzCrystal = 79,

        OnyxShard = 80,
        OnyxGem = 85,
        OnyxCrystal = 89,

        BronzeOre = 100,
        BronzeIngot = 105,
        BronzeBlock = 109,

        IronOre = 110,
        IronIngot = 115,
        IronBlock = 119,

        PlatinumOre = 120,
        PlatinumIngot = 125,
        PlatinumBlock = 129,

        MythrilOre = 130,
        MythrilIngot = 135,
        MythrilBlock = 139,

        OrichalcumOre = 140,
        OrichalcumIngot = 145,
        OrichalcumBlock = 149,

        AdamantiteOre = 150,
        AdamantiteIngot = 155,
        AdamantiteBlock = 159,
    }

    public enum ChallengeType
    {
        Complete = 0,

        CastLocationsPerTurn = 1012,
        CharDistMovedPerTurn = 1013,

        AddCardsToDeck = 1022,
        ReplaceCards = 1023,

        SpendManaPerTurn = 1032,
        UnspentEnergyPerTurn = 1033,

        BreakEnemies = 1042,
        TotalOverkillGold = 1043,

        CharsStacked = 1052,
        EnemyFriendlyKill = 1053,

        TauntAwayFromAlly = 1062,
        HealedByEnemy = 1063,

        BonusHealthCompleteBlock = 1072,
        HealthAndArmorCombinedPerTurn = 1073,

        DamageDealtWithSingeCard = 1082,
        KillWithSingleCard = 1083,

        ArmorRemovedFromEnemy = 1092,
        AromrRemovedWithSingleCard = 1093,

        SacrificeNothing = 1102,
        DefeatBossInTurn = 1103,
    }

    public enum ChallengeValueType
    {
        ValueAsIs = 0,
        Max = 1,
        Min = 2,
        Sum = 10,
        Count = 20,
    }

    public enum ChallengeComparisonType
    {
        GreaterThan = 0,
        EqualTo = 10,
        LessThan = 20
    }

    public string GetChallengeText(int roomID, int index)
    {
        string output = "";
        switch (challenges[index])
        {
            case ChallengeType.Complete:
                output += "Complete The Run";
                break;
            case ChallengeType.CastLocationsPerTurn:
                output += "Have 1 character cast from X locations in 1 turn";
                break;
            case ChallengeType.CharDistMovedPerTurn:
                output += "Have 1 character move X spaces in 1 turn";
                break;
            case ChallengeType.AddCardsToDeck:
                output += "Add X cards that didn't start in your deck to your deck";
                break;
            case ChallengeType.ReplaceCards:
                output += "Replace X cards";
                break;
            case ChallengeType.SpendManaPerTurn:
                output += "Spend X mana in 1 turn";
                break;
            case ChallengeType.UnspentEnergyPerTurn:
                output += "Use all available energy every turn";
                break;
            case ChallengeType.BreakEnemies:
                output += "Break X Enemies";
                break;
            case ChallengeType.TotalOverkillGold:
                output += "Earn X overkill gold";
                break;
            case ChallengeType.CharsStacked:
                output += "Stack X characters together";
                break;
            case ChallengeType.EnemyFriendlyKill:
                output += "Make X enemy kill another";
                break;
            case ChallengeType.TauntAwayFromAlly:
                output += "Taun X enemies targeting an ally";
                break;
            case ChallengeType.HealedByEnemy:
                output += "Be healed by an enemy";
                break;
            case ChallengeType.BonusHealthCompleteBlock:
                output += "Entirely block 1 instance of damage with overheal’s bonus health";
                break;
            case ChallengeType.HealthAndArmorCombinedPerTurn:
                output += "End the turn with a character with more than X total health plus armor";
                break;
            case ChallengeType.DamageDealtWithSingeCard:
                output += "Deal X Damage With A Single Card";
                break;
            case ChallengeType.KillWithSingleCard:
                output += "Bring X characters below 0 health with 1 card";
                break;
            case ChallengeType.ArmorRemovedFromEnemy:
                output += "Remove X total armor from enemies";
                break;
            case ChallengeType.AromrRemovedWithSingleCard:
                output += "Remove X armor from enemies with a single card";
                break;
            case ChallengeType.SacrificeNothing:
                output += "Defeat all boss summons before they’re sacrificed";
                break;
            case ChallengeType.DefeatBossInTurn:
                output += "Defeat the boss in less than X turns";
                break;
            default:
                output += "";
                output += "Not implemented";
                break;
        }

        output = output.Replace("X", challengeValues[index].ToString());
        if (challenges[index] != ChallengeType.Complete)
        {
            if (StoryModeController.story.GetChallengeValues().ContainsKey(roomID))
                output += " (" + StoryModeController.story.GetChallengeValues()[roomID][index] + "/" + challengeValues[index] + ")";
            else
                output += " (0/" + challengeValues[index] + ")";
        }
        return output;
    }
}
