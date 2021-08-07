using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class StoryRoomSetup : ScriptableObject
{
    public string roomName;
    public bool useDefaultCardsAndEquipments = false;
    public List<RoomSetup> setups;
    public WorldSetup arenaSetup;
    public ChallengeType[] challenges = new ChallengeType[3];
    public ChallengeValueType[] valueType = new ChallengeValueType[3];
    public int[] challengeValues = new int[3];
    public ChallengeComparisonType[] challengeComparisonType = new ChallengeComparisonType[3];
    public int[] bestChallengeValues = new int[3] { -1, -1, -1 };

    public StoryModeController.RewardsType[] rewardTypes;
    public Card[] rewardCards;
    public Equipment[] rewardEquipment;
    public int[] rewardAmounts;
    public int[] rewardCosts;
    public bool[] challengeRewardBought = new bool[3] { false, false, false };

    [TextArea]
    public string flavorText;


    public enum ChallengeType
    {
        Complete = 0,
        TotalTurnsUsed = 10,
        TotalTimeUsed = 20,

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

        DefeatBossInTurn = 1102,
        DefeatBossOnEnemyTurn = 1103,

        PlayMoreThanXCardsPerTurn = 1901,
        TakeLessThanXTotalDamage = 1902,

        EnemiesTravelLessThanXSpaces = 1911,

        SacrificeNothing = 9999,
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

    public string GetChallengeText(int roomID, int index, int bestValue = -1, bool useValueAsIs = false)
    {
        string output = "";
        switch (challenges[index])
        {
            case ChallengeType.Complete:
                output += "Complete The Run";
                break;
            case ChallengeType.TotalTurnsUsed:
                output += "Complete in less than X turns";
                break;
            case ChallengeType.TotalTimeUsed:
                output += "Complete in less than X minsY";
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
                output += "Leave less than X total energy unsent at the end of turns";
                break;
            case ChallengeType.BreakEnemies:
                output += "Break X Enemies";
                break;
            case ChallengeType.TotalOverkillGold:
                output += "Earn X total overkill gold";
                break;
            case ChallengeType.CharsStacked:
                output += "Stack X characters together";
                break;
            case ChallengeType.EnemyFriendlyKill:
                output += "Make X enemies kill another";
                break;
            case ChallengeType.TauntAwayFromAlly:
                output += "Taun X enemies targeting an other ally";
                break;
            case ChallengeType.HealedByEnemy:
                output += "Be healed by an enemy";
                break;
            case ChallengeType.BonusHealthCompleteBlock:
                output += "Entirely block X instance of damage with overheal’s bonus health";
                break;
            case ChallengeType.HealthAndArmorCombinedPerTurn:
                output += "End a turn with more than X combined health and armor on 1 character";
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
            case ChallengeType.DefeatBossInTurn:
                output += "Defeat the boss in less than X turns";
                break;
            case ChallengeType.DefeatBossOnEnemyTurn:
                output += "Defeat the boss on the enemy's turn";
                break;
            case ChallengeType.PlayMoreThanXCardsPerTurn:
                output += "Play more than X cards in 1 turn";
                break;
            case ChallengeType.TakeLessThanXTotalDamage:
                output += "Take less than X total damage";
                break;
            case ChallengeType.EnemiesTravelLessThanXSpaces:
                output += "Ensure enemies travel less than X total spaces";
                break;
            case ChallengeType.SacrificeNothing:
                output += "Defeat all boss summons before they’re sacrificed";
                break;
            default:
                output += "";
                output += "Not implemented";
                break;
        }

        if (challenges[index] == ChallengeType.TotalTimeUsed)
        {
            output = output.Replace("X", (challengeValues[index] / 60).ToString());      //Special formatting for time based achievements
            if (challengeValues[index] % 60 > 0)
                output = output.Replace("Y", " " + (challengeValues[index] % 60).ToString() + "secs");
            else
                output = output.Replace("Y", "");
        }
        else
            output = output.Replace("X", challengeValues[index].ToString());

        if (challenges[index] != ChallengeType.Complete && challengeValues[index] != 0)
        {
            if (bestValue != -1)
                output += " (" + bestValue + "/" + challengeValues[index] + ")";
            else if (StoryModeController.story.GetChallengeValues().ContainsKey(roomID) && !useValueAsIs)
            {
                if (StoryModeController.story.GetChallengeValues()[roomID][index] == -1)
                    output += " (0/" + challengeValues[index] + ")";
                else
                    output += " (" + StoryModeController.story.GetChallengeValues()[roomID][index] + "/" + challengeValues[index] + ")";
            }
            else
                output += " (0/" + challengeValues[index] + ")";
        }
        return output;
    }

    public void SetBestValues(int[] values)
    {
        for (int i = 0; i < 3; i++)
            bestChallengeValues[i] = GetBestValues(bestChallengeValues[i], values[i], i, challengeComparisonType[i]);
    }

    public int GetBestValues(int oldValue, int newValue, int index, ChallengeComparisonType comparisonType)
    {
        int output = oldValue;
        if (newValue == -1)
            output = oldValue;
        else
            switch (comparisonType)
            {
                case ChallengeComparisonType.GreaterThan:
                    output = Mathf.Max(oldValue, newValue);
                    break;
                case ChallengeComparisonType.LessThan:
                    if (oldValue == -1)
                        output = newValue;
                    else
                        output = Mathf.Min(oldValue, newValue);
                    break;
                case ChallengeComparisonType.EqualTo:
                    int oldDiff = Mathf.Abs(oldValue - challengeValues[index]);
                    int newDiff = Mathf.Abs(newValue - challengeValues[index]);
                    if (oldDiff > newDiff)
                        output = newValue;
                    break;
            }
        return output;
    }

    public void SetReardsBought(bool item1, bool item2, bool item3)
    {
        challengeRewardBought = new bool[3] { item1, item2, item3 };
    }
}
