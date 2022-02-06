using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Conversation
{
    public Dialogue.Emotion[] emoticon;
    public string[] texts;
}

[CreateAssetMenu]
public class Dialogue : ScriptableObject
{
    public Condition condition;
    public int conditionValue = -1;
    public StoryRoomSetup.ChallengeComparisonType comparisonType = StoryRoomSetup.ChallengeComparisonType.EqualTo;
    public Emotion[] emoticon;
    public string[] texts;

    public enum Emotion
    {
        Neutral = 0,
        Happy = 1,
        Wink = 2,
        Kiss = 3,
        Angry = 100,
        Frustrated = 101,
        Confused = 200,
        Surprised = 201,
        Sad = 300,
        Sleep = 400,
        Sunglasses = 500
    }

    public enum Condition
    {
        None = 0,
        turn = 10,

        Replace = 20,

        PlayerPosition = 1001,
        CardsUsed = 1002,

        PlayerMoved = 2001,
        PlayerDeath = 2901,
        AfterRoomReset = 2999,

        EnemyHeld = 3001,
        EnemyTapped = 3005,

        EnemyBroken = 4001,
        EnemyOverkill = 4002,
        EnemyDamageTaken = 4003,

        EnemiesStacked = 4011,
        MultiEnemiesTargeted = 4012,

        RewardsMenuShown = 10001,
        RewardsMenuItemTaken = 10002,
        RewardsMenuCardTaken = 10003,
        RewardsMenuExit = 10004,

        FinalRewardsMenuShown = 10011,
        FinalRewardsMenuItemTaken = 10012,
        FinalRewardsMenuExit = 10013,

        RewardsCardEnlarged = 10021,
        RewardsCardShrunk = 10022,

        MapSceneLoaded = 10051,
        MapSceneEnterButtonPressed = 10052,

        CollectionCardAddedToDeck = 10101,
        CollectionColorSelected = 10201,
        CollectionDoneButtonSelected = 10301,

        AchievementHeld = 11001,
        CharacterInformationMenuOpened = 11002,
        CharacterInformationMenuClosed = 11003,

        EnemyHasPassive = 90001,

        PopupEnded = 99999
    }

    public Conversation GetConversation(Condition con, int value)
    {
        Conversation thisConvo = null;
        if (GetConditionMet(con, value))
        {
            thisConvo = new Conversation();
            thisConvo.emoticon = emoticon;
            thisConvo.texts = texts;
        }

        return thisConvo;
    }

    private bool GetConditionMet(Condition con, int value)
    {
        if (con != condition)
            return false;
        if (comparisonType == StoryRoomSetup.ChallengeComparisonType.EqualTo && conditionValue == -1)
            return true;

        switch (comparisonType)
        {
            case StoryRoomSetup.ChallengeComparisonType.EqualTo:
                return value == conditionValue;
            case StoryRoomSetup.ChallengeComparisonType.GreaterThan:
                return value >= conditionValue;
            case StoryRoomSetup.ChallengeComparisonType.LessThan:
                return value <= conditionValue;
        }

        return false;
    }
}
