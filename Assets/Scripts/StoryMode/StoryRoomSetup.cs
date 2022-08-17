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

    public bool skipFinalRewards = false;
    public bool allowRewardsRebuy = false;
    public bool noAchievements = false;
    public StoryModeController.RewardsType[] rewardTypes;
    public Card[] rewardCards;
    public Equipment[] rewardEquipment;
    public int[] rewardAmounts;
    public int[] rewardCosts;
    public bool[] challengeRewardBought = new bool[3] { false, false, false };

    [TextArea]
    public string flavorText;

    public Card.CasterColor[] overrideColors;

    [Header("Boss Passives")]
    public List<string> abilityNames = new List<string>();
    public List<Sprite> abilitySprites = new List<Sprite>();
    public List<AbilitiesController.TargetType> targetTypes = new List<AbilitiesController.TargetType>();
    public List<AbilitiesController.ConditionType> conditionTypes = new List<AbilitiesController.ConditionType>();
    public List<AbilitiesController.TriggerType> triggerTypes = new List<AbilitiesController.TriggerType>();
    public List<AbilitiesController.AbilityType> abilityTypes = new List<AbilitiesController.AbilityType>();
    public List<int> abilityValue = new List<int>();

    public enum ChallengeType
    {
        Complete = 0,
        TotalTurnsUsed = 10,
        TurinInRound1Used = 11,
        TurinInRound2Used = 12,
        TurinInRound3Used = 13,
        TotalTimeUsed = 20,

        CastLocationsPerTurn = 1012,
        CharDistMovedPerTurn = 1013,

        AddCardsToDeck = 1022,
        ReplaceCards = 1023,

        SpendManaPerTurn = 1032,
        UnspentEnergyPerTurn = 1033,

        BeBroken = 1041,
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
        DamageDealtInSingleTurn = 1085,

        ArmorRemovedFromEnemy = 1092,
        AromrRemovedWithSingleCard = 1093,

        DefeatBossInTurn = 1102,
        DefeatBossOnEnemyTurn = 1103,
        UseLessThanXCards = 1104,

        PlayMoreThanXCardsPerTurn = 1901,
        TakeLessThanXTotalDamage = 1902,

        XNumOfImmunedCards = 1905,

        EnemiesTravelLessThanXSpaces = 1911,
        EndTurnWithXEnemies = 1912,

        CastOnAnotherally = 1921,
        CastFromAllColorsForXTurns = 1922,

        UseOriginalTeam = 1950,

        //Tutorial only
        DamageTakenRound1 = 9001,

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
        LessThan = 20,
        NotEqualTo = 30
    }

    public string GetChallengeText(int roomID, int index, int bestValue = -1, bool useValueAsIs = false, bool containsProgressText = true)
    {
        string output = "";
        switch (challenges[index])
        {
            case ChallengeType.Complete:
                output += "Complete the room";
                break;
            case ChallengeType.TotalTurnsUsed:
                output += "Complete in {ct} X total turn{s}";
                break;
            case ChallengeType.TurinInRound1Used:
                output += "Complete round 1 in {ct} X turn{s}";
                break;
            case ChallengeType.TurinInRound2Used:
                output += "Complete round 2 in {ct} X turn{s}";
                break;
            case ChallengeType.TurinInRound3Used:
                output += "Complete round 3 in {ct} X turn{s}";
                break;
            case ChallengeType.TotalTimeUsed:
                output += "Complete in {ct} X minsY";
                break;
            case ChallengeType.CastLocationsPerTurn:
                output += "Have any character cast from {ct} X location{s} in 1 turn";
                break;
            case ChallengeType.CharDistMovedPerTurn:
                output += "Have any character move {ct} X space{s} in 1 turn";
                break;
            case ChallengeType.AddCardsToDeck:
                output += "Add {ct} X new card{s} into your deck";
                break;
            case ChallengeType.ReplaceCards:
                output += "Replace {ct} X card{s}";
                break;
            case ChallengeType.SpendManaPerTurn:
                output += "Spend {ct} X mana in 1 turn";
                break;
            case ChallengeType.UnspentEnergyPerTurn:
                output += "Leave {ct} X total energy unspent at the end of your turns";
                break;
            case ChallengeType.BeBroken:
                output += "Be broken {ct} X time{s}";
                break;
            case ChallengeType.BreakEnemies:
                output += "Break {ct} X enem{ies}";
                break;
            case ChallengeType.TotalOverkillGold:
                output += "Earn {ct} X total overkill gold";
                break;
            case ChallengeType.CharsStacked:
                output += "Stack {ct} X character{s} together";
                break;
            case ChallengeType.EnemyFriendlyKill:
                output += "Make {ct} X enem{ies} kill another";
                break;
            case ChallengeType.TauntAwayFromAlly:
                output += "Taunt {ct} X enem{ies} targeting an other ally";
                break;
            case ChallengeType.HealedByEnemy:
                output += "Be healed by {ct} X enem{ies}";
                break;
            case ChallengeType.BonusHealthCompleteBlock:
                output += "Entirely block {ct} X instance{s} of damage with overheal";
                break;
            case ChallengeType.HealthAndArmorCombinedPerTurn:
                output += "End a turn with {ct} X combined health and armor on 1 character";
                break;
            case ChallengeType.DamageDealtWithSingeCard:
                output += "Deal {ct} X damage with a single card";
                break;
            case ChallengeType.KillWithSingleCard:
                output += "Bring {ct} X character{s} below 0 health with 1 card";
                break;
            case ChallengeType.DamageDealtInSingleTurn:
                output += "Deal {ct} X damage with a single turn";
                break;
            case ChallengeType.ArmorRemovedFromEnemy:
                output += "Remove {ct} X total armor from enemies";
                break;
            case ChallengeType.AromrRemovedWithSingleCard:
                output += "Remove {ct} X armor from enemies with a single card";
                break;
            case ChallengeType.DefeatBossInTurn:
                output += "Defeat the boss in {ct} X turn{s}";
                break;
            case ChallengeType.DefeatBossOnEnemyTurn:
                output += "Defeat the boss on the enemy's turn";
                break;
            case ChallengeType.PlayMoreThanXCardsPerTurn:
                output += "Play {ct} X card{s} in 1 turn";
                break;
            case ChallengeType.TakeLessThanXTotalDamage:
                output += "Take {ct} X total damage";
                break;
            case ChallengeType.EnemiesTravelLessThanXSpaces:
                output += "Ensure enemies travel {ct} X total space{s}";
                break;
            case ChallengeType.EndTurnWithXEnemies:
                output += "End a turn with {ct} X enem{ies}";
                break;
            case ChallengeType.SacrificeNothing:
                output += "Defeat all boss summons before they’re sacrificed";
                break;
            case ChallengeType.UseLessThanXCards:
                output += "Use {ct} X card{s}";
                break;
            case ChallengeType.UseOriginalTeam:
                output += "Complete with the original team (Red, Green, Blue)";
                break;
            case ChallengeType.CastOnAnotherally:
                output += "Cast {ct} X card{s} on another ally";
                break;
            case ChallengeType.CastFromAllColorsForXTurns:
                output += "Use cards from all three colors in {ct} X turn{s}";
                break;
            case ChallengeType.XNumOfImmunedCards:
                output += "Have {ct} X card{s} be blocked by immunity";
                break;
            case ChallengeType.DamageTakenRound1:
                output += "Take {ct} X damage in round 1";
                break;
            default:
                output += "";
                output += "Not implemented";
                break;
        }

        if (challengeValues[index] == 1)
        {
            output = output.Replace("{s}", "");
            output = output.Replace("{ies}", "y");
        }
        else
        {
            output = output.Replace("{s}", "s");
            output = output.Replace("{ies}", "ies");
        }

        switch (challengeComparisonType[index])
        {
            case ChallengeComparisonType.EqualTo:
                output = output.Replace("{ct}", "exactly");
                break;
            case ChallengeComparisonType.GreaterThan:
                output = output.Replace("{ct}", "more than");
                break;
            case ChallengeComparisonType.LessThan:
                output = output.Replace("{ct}", "less than");
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

        if (containsProgressText)
            output += " " + GetChallengeProgressText(roomID, index, bestValue, useValueAsIs);

        return output;
    }

    public string GetChallengeProgressText(int roomID, int index, int bestValue = -1, bool useValueAsIs = false)
    {
        string output = "";
        if (challenges[index] != ChallengeType.Complete && challengeValues[index] != 0)
        {
            if (bestValue != -1)
                if (challenges[index] == ChallengeType.TotalTimeUsed)
                    output = "(" + bestValue / 60 + ":" + (bestValue % 60).ToString("00") + "/" + challengeValues[index] / 60 + ":" + (challengeValues[index] % 60).ToString("00") + ")";
                else
                    output = "(" + bestValue + "/" + challengeValues[index] + ")";
            else if (StoryModeController.story.GetChallengeValues().ContainsKey(roomID) && !useValueAsIs)
            {
                if (StoryModeController.story.GetChallengeValues()[roomID][index] == -1)
                    output = "(0/" + challengeValues[index] + ")";
                else if (challenges[index] == ChallengeType.TotalTimeUsed)
                    output = "(" + StoryModeController.story.GetChallengeValues()[roomID][index] / 60 + ":" + (StoryModeController.story.GetChallengeValues()[roomID][index] % 60).ToString("00") + "/" + challengeValues[index] / 60 + ":" + (challengeValues[index] % 60).ToString("00") + ")";
                else
                    output = "(" + StoryModeController.story.GetChallengeValues()[roomID][index] + "/" + challengeValues[index] + ")";
            }
            else
            {
                if (challenges[index] == ChallengeType.TotalTimeUsed)
                    output = "(0:00/" + challengeValues[index] / 60 + ":" + (challengeValues[index] % 60).ToString("00") + ")";
                else
                    output = "(0/" + challengeValues[index] + ")";
            }
        }
        else if (challenges[index] == ChallengeType.Complete)
        {
            if (bestValue == 1)
                output += "(1/1)";
            else
                output += "(0/1)";
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
                    output = Mathf.Max(0, output);              //Prevent -1s from registering as invalid best values
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
