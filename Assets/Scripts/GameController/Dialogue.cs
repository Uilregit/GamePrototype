using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Conversation
{
    public Dialogue.Speaker[] speaker;
    public Dialogue.Emotion[] emoticon;
    public string[] texts;
}

[CreateAssetMenu]
public class Dialogue : ScriptableObject
{
    public int id;  //WRRED (World, Room, Encounter, Dialogue)
    public Condition condition;
    public int conditionValue = -1;
    public StoryRoomSetup.ChallengeComparisonType comparisonType = StoryRoomSetup.ChallengeComparisonType.EqualTo;
    public Speaker[] speaker;
    public Emotion[] emoticon;
    [TextArea]
    public string[] texts;

    public enum Speaker
    {
        Deckromancer = 0,
        Shopkeeper = 1,
    }

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
        StartOfRound = 1,
        DialogueDone = 5,

        turn = 10,
        EndedTurnWithPlayableCards = 15,

        Replace = 20,

        PlayerPosition = 1001,
        CardsUsed = 1002,

        CastTypeNormalSelected = 1500,
        CastTypeTargetedAoESelected = 1501,
        CastTypeAoESelected = 1502,

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

        //Passive tutorials
            //UI unlocks
        StoryModeEndItemLocked = 50001,     //done
        StoryModeEndItemSoldOut = 50002,    //

        FirstCardBought = 50051,            //
        FirstCardEquipped = 50052,
        CardEquippedLockedSlot = 50053,     //
        FirstEquipmentBought = 50061,       //
        FirstEquipmentEquipped = 50062,
        ShopUnlocked = 50071,               //
        ShopOpened = 50072,                 //
        NewCharUnlocked = 50081,            //
        PartyMenuOpened = 50082,            //

        WildCardUnlocked = 51001,           //
        WildCardOptionInShop = 51002,       //

            //Suggestions
        AllUnplayableCards = 60001,         //
        ExcessManaGained = 60002,           //

            //Combat infos
        FirstDeath = 70001,                 //
        FirstRevive = 70002,                //
        SecondRevive = 70003,               //
        DefyDeath = 70004,                  //
        
        Overheal = 70011,                   //
        Break2ndTurn = 70021,               //
        BreakRecovery = 70022,              //

        DiscardPileShuffle = 70051,         //maybe remove?

        FirstClassicModeRelic = 80001,
        FirstArenaRelic = 80002,

        EnemyHasPassive = 90001,            //

            //Game quality of life
        AbandonRunButton = 95001,
        EndTurnToSpeedUpButton = 95002,

            //Game modes
        ClassicModeStart = 99001,
        ArenaStart = 99010,                 //
        NakedArenaStart = 99020,            //

        //Ending popups
        PopupEnded = 99999,                 //
        //Ending demo
        EndOfDemo = 999999                  //
    }

    public Conversation GetConversation(Condition con, int value)
    {
        Conversation thisConvo = null;
        if (GetConditionMet(con, value))
        {
            thisConvo = new Conversation();
            thisConvo.speaker = speaker;
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
