using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StoryModeController : MonoBehaviour
{
    public static StoryModeController story;
    private List<int> completedIds = new List<int>();
    private Dictionary<int, int[]> challengeValues = new Dictionary<int, int[]>();              //<roomId, bestChallengeValue>
    private int worldNumber;
    private int currentRoomId = -1;
    private StoryRoomSetup currentRoomSetup;
    private StoryRoomController.StoryRoomType currentRoomType;
    private int lastSelectedRoomID = -1;
    private int lastSelectedAchievements = -1;
    private bool lastSelectedComplete = false;

    private List<string> cardCraftable = new List<string>();
    //private Dictionary<string, int> cardUnlocked = new Dictionary<string, int>();                                                           //<cardName, cardAmount>
    //private Dictionary<string, int> equipmentUnlocked = new Dictionary<string, int>() { { "Small Pocket", 5 }, { "Messenger Bag", 5 }, { "Short Sword", 1 }, { "Stout Shield", 1 } };    //<equipmentName, equipmentAmount>
    private Dictionary<int, bool[]> dailyBought = new Dictionary<int, bool[]>();                                        //<dayIDSeed, bought[]>
    private Dictionary<int, bool[]> weeklyBought = new Dictionary<int, bool[]>();                                       //<weekIDSeed, bought[]>

    private Dictionary<RewardsType, int> unlockedItems = new Dictionary<RewardsType, int>();
    private Dictionary<int, bool[]> challengeItemsbought = new Dictionary<int, bool[]>();                               //<roomID, ifItemBought[]>
    private Dictionary<int, bool[]> secretShopItemsBought = new Dictionary<int, bool[]>();                              //<worldID, ifItemBought[]>
    private Dictionary<int, List<Card.CasterColor>> colorsCompleted = new Dictionary<int, List<Card.CasterColor>>();    //<roomID, colorsCompleted>
    public StoryRoomSetup[] secretShops;

    public Color defaultMenuColor;
    public Color selectedMenuColor;
    public Color warningNotificationColor;
    public Color newNotificationColor;
    public Image[] menuIcons;
    public Image[] menuBlankOuts;
    public Image[] menuSelected;
    public Image[] menuUnselected;
    public Text[] menuNames;
    public Image[] menuNotifications;

    public Image abandonRunButton;
    public Image abandonRunWarning;

    public Image combatInfoMenu;
    public Text goldText;
    public Image achievementMenu;
    public Image[] achievementIcons;
    public Image[] achievementPassiveIcons;
    public Text[] achievementTexts;

    public Color completedColor;
    public Color shopColor;
    public Color goldColor;

    public Sprite blankCardSprite;
    public Sprite cardSlotSprite;
    public Sprite weaponBlueprintSprite;

    public Sprite EnergyShard;
    public Sprite EnergyGem;
    public Sprite EnergyCrystal;

    public Sprite ManaShard;
    public Sprite ManaGem;
    public Sprite ManaCrystal;

    public Sprite shardSprite;
    public Sprite gemSprite;
    public Sprite crystalSprite;

    public Sprite oreSprite;
    public Sprite ingotSprite;
    public Sprite blockSprite;

    public Sprite wildcardSprite;
    public Sprite cardPackSprite;
    public Sprite tavernContractSprite;
    public Sprite errorSprite;

    public Color cardColor;
    public Color weaponColor;
    public Color energyColor;
    public Color manaColor;
    public Color bronzeColor;
    public Color ironColor;
    public Color platinumColor;
    public Color mythrilColor;
    public Color orichalcumColor;
    public Color AdamantiteColor;
    public Color commonColor;
    public Color rareColor;
    public Color legndaryColor;

    public Color cardPackColor;
    public Color tavernContractColor;

    private Vector2 desiredCameraLocation;

    private MenuState menuState = MenuState.MapScreen;
    private bool deckIncomplete = false;

    private int rewardsRerollCount = 0;

    public enum MenuState
    {
        MapScreen = 0,
        PartyScreen = 10,
        GearScreen = 20,
        CardScreen = 30,
        ShopScreen = 40
    }

    public enum RewardsType
    {
        BlankCard = 0,
        CardSlot = 2,
        WeaponBlueprint = 5,
        SpecificCard = 7,
        SpecificEquipment = 8,

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

        TopazShard = 60,
        TopazGem = 65,
        TopazCrystal = 69,

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

        CardPack = 500,
        RedCardPack = 510,
        BlueCardPack = 520,
        GreenCardPack = 530,
        OrangeCardPack = 540,
        WhiteCardPack = 550,
        BlackCardPack = 560,

        CommonWildCard = 700,
        RareWildCard = 710,
        LegendaryWildCard = 730,

        TavernContract = 9000,

        PlusXReplace = 10001,
        PlusXGoldPerRoom = 10002,
        PlusXRewardCardRerollPerRun = 10003,
        PlusXWeeklyWaresRerollPerWeek = 10004,
        PlusXDailyDealsRerollPerDay = 10005,

        UnlockLeveling = 90000,
        UnlockPostEncounterCards = 90001,
        UnlockAchievements = 90002,

        UnlockClassicMode = 99001,
    }

    // Start is called before the first frame update
    void Awake()
    {
        if (StoryModeController.story == null)
            StoryModeController.story = this;
        else
        {
            Destroy(this.gameObject);
            return;
        }

        DontDestroyOnLoad(this.gameObject);

        completedIds = new List<int>();

        ShowMenuSelected(0);
        SetCombatInfoMenu(false);
        SetAchievementInfoMenu(false);

        if (InformationLogger.infoLogger.debug && InformationLogger.infoLogger.debugStoryCopyRealSaveFile)
        {
            InformationLogger.infoLogger.debug = false;
            InformationLogger.infoLogger.LoadStoryModeGame();
            InformationLogger.infoLogger.debug = true;
            InformationLogger.infoLogger.SaveStoryModeGame();
        }
        else if (!InformationLogger.infoLogger.debug && InformationLogger.infoLogger.debugStoryCopyDebugSaveFile)
        {
            InformationLogger.infoLogger.debug = true;
            InformationLogger.infoLogger.LoadStoryModeGame();
            InformationLogger.infoLogger.debug = false;
            InformationLogger.infoLogger.SaveStoryModeGame();
        }

        RefreshMenuIconBlanOuts();
        if (InformationLogger.infoLogger.GetLatestDayShopOpened() != InformationLogger.infoLogger.GetRawDailySeed())
            ShowMenuNotification(4, true, false);

        ResetRewardsRerollLeft();

        ResetDecks();
    }

    public void ResetDecks()
    {
        if (!InformationLogger.infoLogger.GetHasStoryModeSaveFile())
        {
            //Instantiate all decks based off of storymode decks for the collection controller
            List<string> cardCraftable = new List<string>();
            Dictionary<string, int> completeDeck = new Dictionary<string, int>();
            Dictionary<string, string[]> selectedcards = new Dictionary<string, string[]>();
            foreach (EditorCardsWrapper cardWrapper in CollectionController.collectionController.storyModeDeck)
            {
                string color = cardWrapper.deck[0].casterColor.ToString();
                List<string> cardNames = new List<string>();
                foreach (Card c in cardWrapper.deck)
                {
                    cardCraftable.Add(c.name);
                    if (completeDeck.ContainsKey(c.name))
                        completeDeck[c.name] += 1;
                    else
                        completeDeck[c.name] = 1;

                    cardNames.Add(c.name);
                }
                selectedcards[color] = cardNames.ToArray();
            }

            CollectionController.collectionController.SetCompleteDeck(completeDeck);
            CollectionController.collectionController.SetSelectedDeck(selectedcards);

            //Instantiate all equipments
            Dictionary<string, string[]> selectedEquipments = new Dictionary<string, string[]>();
            foreach (string color in PartyController.party.GetPotentialPlayerTexts())
                selectedEquipments[color] = new string[0];
            CollectionController.collectionController.SetCompleteEquipments(new Dictionary<string, int>());
            CollectionController.collectionController.SetSelectedEquipments(selectedEquipments);

            if (!InformationLogger.infoLogger.debug)
                InformationLogger.infoLogger.SaveStoryModeGame();

            Debug.Log("no save file");
        }
        else
        {
            InformationLogger.infoLogger.LoadStoryModeGame();
            Debug.Log("had save file");
        }
        CollectionController.collectionController.SetupStoryModeDeck();
    }

    public void ReportRoomCompleted()
    {
        if (!completedIds.Contains(currentRoomId))
            completedIds.Add(currentRoomId);

        challengeValues[currentRoomId] = new int[3];

        Debug.Log("######### Final values ##########");

        for (int i = 0; i < 3; i++)
        {
            challengeValues[currentRoomId][i] = AchievementSystem.achieve.GetChallengeValue(currentRoomSetup.challenges[i]);
            Debug.Log(currentRoomSetup.challenges[i] + " | " + challengeValues[currentRoomId][i]);
        }

        currentRoomSetup.SetBestValues(challengeValues[currentRoomId]);         //Prevent overriding of old completed achievements if current achievements isn't good
        challengeValues[currentRoomId] = currentRoomSetup.bestChallengeValues;

        Debug.Log("-----");
        for (int i = 0; i < 3; i++)
            Debug.Log(currentRoomSetup.challenges[i] + " | " + challengeValues[currentRoomId][i]);

        InformationLogger.infoLogger.LoadStoryModeDecksAndEquipments();
        InformationLogger.infoLogger.SaveStoryModeGame();
    }

    public List<int> GetCompletedRooms()
    {
        return completedIds;
    }

    public void SetCompletedRooms(List<int> value)
    {
        completedIds = value;
    }

    //Used by patchcontroller to add completed rooms for testing
    public void AddCompletedRooms(List<int> value)
    {
        completedIds.AddRange(value);
    }

    public void SetChallengeValues(Dictionary<int, int[]> value)
    {
        challengeValues = value;
    }

    public Dictionary<int, int[]> GetChallengeValues()
    {
        return challengeValues;
    }

    public void SetCurrentRoomID(int value)
    {
        currentRoomId = value;
    }

    public int GetCurrentRoomID()
    {
        return currentRoomId;
    }

    public void SetCurrentRoomSetup(StoryRoomSetup setup)
    {
        currentRoomSetup = setup;
    }

    public StoryRoomSetup GetCurrentRoomSetup()
    {
        return currentRoomSetup;
    }

    public void SetCurrentRoomType(StoryRoomController.StoryRoomType type)
    {
        currentRoomType = type;
    }

    public StoryRoomController.StoryRoomType GetCurrentRoomType()
    {
        return currentRoomType;
    }

    public void ReportNewCraftableCard(string name)
    {
        if (!cardCraftable.Contains(name))
            cardCraftable.Add(name);
    }

    public void SetCardCraftable(List<string> value)
    {
        cardCraftable = value;
    }

    public List<string> GetCardCraftable()
    {
        return cardCraftable;
    }

    public bool ChallengeSatisfied(int index)
    {
        if (GetChallengeValues().ContainsKey(currentRoomId))
            return ChallengeSatisfied(index, GetChallengeValues()[currentRoomId][index]);
        else
            return false;
    }

    public bool ChallengeSatisfied(int index, int value)
    {
        switch (currentRoomSetup.challengeComparisonType[index])
        {
            case StoryRoomSetup.ChallengeComparisonType.GreaterThan:
                return value >= currentRoomSetup.challengeValues[index];
            case StoryRoomSetup.ChallengeComparisonType.EqualTo:
                return value == currentRoomSetup.challengeValues[index];
            case StoryRoomSetup.ChallengeComparisonType.LessThan:
                return value <= currentRoomSetup.challengeValues[index];
        }
        return false;
    }

    public void ReportItemsBought(Dictionary<RewardsType, int> items)
    {
        foreach (RewardsType item in items.Keys)
        {
            if (unlockedItems.ContainsKey(item))
                unlockedItems[item] += items[item];
            else
                unlockedItems[item] = items[item];
        }
    }

    public void ReportCardsBought(Dictionary<Card, int> cards)
    {
        Dictionary<string, int> names = new Dictionary<string, int>();
        foreach (Card c in cards.Keys)
            names[c.name] = cards[c];
        CollectionController.collectionController.SetCompleteDeck(names, true);
        EnableMenuIcon(3);
    }

    public void ReportEquipmentBought(Dictionary<Equipment, int> equipments)
    {
        Dictionary<string, int> names = new Dictionary<string, int>();
        foreach (Equipment e in equipments.Keys)
            names[e.equipmentName] = equipments[e];
        CollectionController.collectionController.SetCompleteEquipments(names, true);
        EnableMenuIcon(2);
    }

    public Dictionary<RewardsType, int> GetItemsBought()
    {
        /*
        Debug.Log("Get Items Bought: " + unlockedItems.Keys.Count + " items");
        foreach (RewardsType name in unlockedItems.Keys)
        {
            Debug.Log("    " + name + ": " + unlockedItems[name]);
        }
        */
        return unlockedItems;
    }

    public void SetItemsBought(Dictionary<RewardsType, int> value)
    {
        unlockedItems = value;
        /*
        Debug.Log("Set Items Bought: " + value.Keys.Count + " items");
        foreach (RewardsType name in unlockedItems.Keys)
        {
            Debug.Log("    " + name + ": " + unlockedItems[name]);
        }
        */
    }

    public void SetChallengeItemsBought(Dictionary<int, bool[]> value)
    {
        challengeItemsbought = value;
    }

    public Dictionary<int, bool[]> GetChallengeItemsBought()
    {
        return challengeItemsbought;
    }

    public void AddChallengeItemsBought(int roomId, bool[] items)
    {
        if (!challengeItemsbought.ContainsKey(roomId))
            challengeItemsbought[roomId] = items;
        else
            for (int i = 0; i < 3; i++)
                challengeItemsbought[roomId][i] = challengeItemsbought[roomId][i] || items[i];
    }

    public int GetNumberOfChallengeItemsBought(int roomId)
    {
        int output = 0;

        if (!challengeItemsbought.ContainsKey(roomId))
            return 0;

        foreach (bool value in challengeItemsbought[roomId])
            if (value)
                output++;

        return output;
    }

    public void ReportCardBought(string cardName, Dictionary<RewardsType, int> usedMaterials)
    {
        CollectionController.collectionController.SetCompleteDeck(new Dictionary<string, int>() { { cardName, 1 } }, true);
        CollectionController.collectionController.ReCountUniqueCards();
        CollectionController.collectionController.RefreshDecks();

        if (!InformationLogger.infoLogger.debug)            //Only use materials if not in debug mode
            foreach (StoryModeController.RewardsType m in usedMaterials.Keys)
                unlockedItems[m] -= usedMaterials[m];

        InformationLogger.infoLogger.SaveStoryModeGame();
        EnableMenuIcon(3);
    }

    public void ReportEquipmentBought(string equipmentName, Dictionary<RewardsType, int> usedMaterials)
    {
        CollectionController.collectionController.SetCompleteEquipments(new Dictionary<string, int>() { { equipmentName, 1 } }, true);
        CollectionController.collectionController.ReCountUniqueEquipments();
        CollectionController.collectionController.RefreshEquipments();

        if (!InformationLogger.infoLogger.debug)            //Only use materials if not in debug mode
            foreach (StoryModeController.RewardsType m in usedMaterials.Keys)
                unlockedItems[m] -= usedMaterials[m];

        InformationLogger.infoLogger.SaveStoryModeGame();
        EnableMenuIcon(2);
    }

    public void SetDailyBought(Dictionary<int, bool[]> daily)
    {
        dailyBought = daily;
        if (daily == null)
        {
            dailyBought = new Dictionary<int, bool[]>();
            dailyBought[InformationLogger.infoLogger.GetDailySeed()] = new bool[] { false, false, false };
        }
    }

    public void SetWeeklyBought(Dictionary<int, bool[]> weekly)
    {
        weeklyBought = weekly;
        if (weekly == null)
        {
            weeklyBought = new Dictionary<int, bool[]>();
            weeklyBought[InformationLogger.infoLogger.GetWeeklySeed()] = new bool[] { false, false, false, false, false, false };
        }
    }

    public Dictionary<int, bool[]> GetDailyBought()
    {
        if (!dailyBought.ContainsKey(InformationLogger.infoLogger.GetDailySeed()))
            dailyBought[InformationLogger.infoLogger.GetDailySeed()] = new bool[] { false, false, false };

        return dailyBought;
    }

    public Dictionary<int, bool[]> GetWeeklyBought()
    {
        if (!weeklyBought.ContainsKey(InformationLogger.infoLogger.GetWeeklySeed()))
            weeklyBought[InformationLogger.infoLogger.GetWeeklySeed()] = new bool[] { false, false, false, false, false, false };

        return weeklyBought;
    }

    public void SetCardBought(bool isDailyCard, int seed, int index)
    {
        if (!dailyBought.ContainsKey(seed))
            dailyBought[seed] = new bool[] { false, false, false };
        if (!weeklyBought.ContainsKey(seed))
            weeklyBought[InformationLogger.infoLogger.GetWeeklySeed()] = new bool[] { false, false, false, false, false, false };

        if (isDailyCard)
            dailyBought[seed][index] = true;
        else
            weeklyBought[seed][index] = true;
    }

    public Sprite GetRewardSprite(RewardsType reward, int index)
    {
        if (reward == RewardsType.SpecificCard)
            return StoryModeController.story.GetCurrentRoomSetup().rewardCards[index].art;
        else if (reward == RewardsType.SpecificEquipment)
            return StoryModeController.story.GetCurrentRoomSetup().rewardEquipment[index].art;
        List<RewardsType> shards = new List<RewardsType>() { RewardsType.OnyxShard, RewardsType.QuartzShard, RewardsType.RubyShard, RewardsType.SapphireShard, RewardsType.TopazShard, RewardsType.EmeraldShard };
        List<RewardsType> gems = new List<RewardsType>() { RewardsType.OnyxGem, RewardsType.QuartzGem, RewardsType.RubyGem, RewardsType.SapphireGem, RewardsType.TopazGem, RewardsType.EmeraldGem };
        List<RewardsType> crystals = new List<RewardsType>() { RewardsType.OnyxCrystal, RewardsType.QuartzCrystal, RewardsType.RubyCrystal, RewardsType.SapphireCrystal, RewardsType.TopazCrystal, RewardsType.EmeraldCrystal };
        List<RewardsType> ores = new List<RewardsType>() { RewardsType.AdamantiteOre, RewardsType.BronzeOre, RewardsType.IronOre, RewardsType.MythrilOre, RewardsType.OrichalcumOre, RewardsType.PlatinumOre };
        List<RewardsType> ingots = new List<RewardsType>() { RewardsType.AdamantiteIngot, RewardsType.BronzeIngot, RewardsType.IronIngot, RewardsType.MythrilIngot, RewardsType.OrichalcumIngot, RewardsType.PlatinumIngot };
        List<RewardsType> blocks = new List<RewardsType>() { RewardsType.AdamantiteBlock, RewardsType.BronzeBlock, RewardsType.IronBlock, RewardsType.MythrilBlock, RewardsType.OrichalcumBlock, RewardsType.PlatinumBlock };

        switch (reward)
        {
            case RewardsType.BlankCard:
                return blankCardSprite;
            case RewardsType.CardSlot:
                return cardSlotSprite;
            case RewardsType.WeaponBlueprint:
                return weaponBlueprintSprite;
            case RewardsType.EnergyShard:
                return EnergyShard;
            case RewardsType.EnergyGem:
                return EnergyGem;
            case RewardsType.EnergyCrystal:
                return EnergyCrystal;
            case RewardsType.ManaShard:
                return ManaShard;
            case RewardsType.ManaGem:
                return ManaGem;
            case RewardsType.ManaCrystal:
                return ManaCrystal;
            case RewardsType.CommonWildCard:
                return wildcardSprite;
            case RewardsType.RareWildCard:
                return wildcardSprite;
            case RewardsType.LegendaryWildCard:
                return wildcardSprite;
            case RewardsType.CardPack:
                return cardPackSprite;
            case RewardsType.TavernContract:
                return tavernContractSprite;
        }

        if (shards.Contains(reward))
            return shardSprite;
        else if (gems.Contains(reward))
            return gemSprite;
        else if (crystals.Contains(reward))
            return crystalSprite;
        else if (ores.Contains(reward))
            return oreSprite;
        else if (ingots.Contains(reward))
            return ingotSprite;
        else if (blocks.Contains(reward))
            return ingotSprite;

        Debug.Log("ERROR RETRIEVING REWARD SPRITE: " + reward);
        return errorSprite;
    }

    public Color GetRewardsColor(RewardsType reward)
    {
        if (reward == RewardsType.SpecificCard || reward == RewardsType.SpecificEquipment)
            return Color.white;

        List<RewardsType> energy = new List<RewardsType>() { RewardsType.EnergyCrystal, RewardsType.EnergyGem, RewardsType.EnergyShard };
        List<RewardsType> mana = new List<RewardsType>() { RewardsType.ManaCrystal, RewardsType.ManaGem, RewardsType.ManaShard };
        List<RewardsType> red = new List<RewardsType>() { RewardsType.RubyCrystal, RewardsType.RubyGem, RewardsType.RubyShard };
        List<RewardsType> green = new List<RewardsType>() { RewardsType.EmeraldCrystal, RewardsType.EmeraldGem, RewardsType.EmeraldShard };
        List<RewardsType> blue = new List<RewardsType>() { RewardsType.SapphireCrystal, RewardsType.SapphireGem, RewardsType.SapphireShard };
        List<RewardsType> orange = new List<RewardsType>() { RewardsType.TopazCrystal, RewardsType.TopazGem, RewardsType.TopazShard };
        List<RewardsType> white = new List<RewardsType>() { RewardsType.QuartzCrystal, RewardsType.QuartzGem, RewardsType.QuartzShard };
        List<RewardsType> black = new List<RewardsType>() { RewardsType.OnyxCrystal, RewardsType.OnyxGem, RewardsType.OnyxShard };
        List<RewardsType> bronze = new List<RewardsType>() { RewardsType.BronzeBlock, RewardsType.BronzeIngot, RewardsType.BronzeOre };
        List<RewardsType> iron = new List<RewardsType>() { RewardsType.IronBlock, RewardsType.IronIngot, RewardsType.IronOre };
        List<RewardsType> platinum = new List<RewardsType>() { RewardsType.PlatinumBlock, RewardsType.PlatinumIngot, RewardsType.PlatinumOre };
        List<RewardsType> mythril = new List<RewardsType>() { RewardsType.MythrilBlock, RewardsType.MythrilIngot, RewardsType.MythrilOre };
        List<RewardsType> orichalcum = new List<RewardsType>() { RewardsType.OrichalcumBlock, RewardsType.OrichalcumIngot, RewardsType.OrichalcumOre };
        List<RewardsType> adamantite = new List<RewardsType>() { RewardsType.AdamantiteBlock, RewardsType.AdamantiteIngot, RewardsType.AdamantiteOre };

        switch (reward)
        {
            case RewardsType.BlankCard:
                return cardColor;
            case RewardsType.CardSlot:
                return cardColor;
            case RewardsType.WeaponBlueprint:
                return weaponColor;
            case RewardsType.CommonWildCard:
                return commonColor;
            case RewardsType.RareWildCard:
                return rareColor;
            case RewardsType.LegendaryWildCard:
                return legndaryColor;
            case RewardsType.CardPack:
                return cardPackColor;
            case RewardsType.TavernContract:
                return tavernContractColor;
        }

        if (energy.Contains(reward))
            return energyColor;
        else if (mana.Contains(reward))
            return manaColor;
        else if (red.Contains(reward))
            return PartyController.party.GetPlayerColor(Card.CasterColor.Red);
        else if (green.Contains(reward))
            return PartyController.party.GetPlayerColor(Card.CasterColor.Green);
        else if (blue.Contains(reward))
            return PartyController.party.GetPlayerColor(Card.CasterColor.Blue);
        else if (orange.Contains(reward))
            return PartyController.party.GetPlayerColor(Card.CasterColor.Orange);
        else if (white.Contains(reward))
            return PartyController.party.GetPlayerColor(Card.CasterColor.White);
        else if (black.Contains(reward))
            return PartyController.party.GetPlayerColor(Card.CasterColor.Black);
        else if (bronze.Contains(reward))
            return bronzeColor;
        else if (iron.Contains(reward))
            return ironColor;
        else if (platinum.Contains(reward))
            return platinumColor;
        else if (mythril.Contains(reward))
            return mythrilColor;
        else if (orichalcum.Contains(reward))
            return orichalcumColor;
        else if (adamantite.Contains(reward))
            return AdamantiteColor;

        return new Color(1, 0, 1);
    }

    public void GoToMapScene()
    {
        if (!InformationLogger.infoLogger.GetMenuIconsEnabled()[0])
            return;

        MusicController.music.PlaySFX(MusicController.music.uiUseLowSFX[Random.Range(0, MusicController.music.uiUseLowSFX.Count)]);
        MusicController.music.SetHighPassFilter(false);
        if (deckIncomplete)
        {
            StopAllCoroutines();
            StartCoroutine(NotificationFlashRed(3));
            return;
        }

        if (SceneManager.GetActiveScene().name != "StoryModeScene")
        {
            SceneManager.LoadScene("StoryModeScene");
            desiredCameraLocation = new Vector2(0, 0);
        }
        else
            CameraController.camera.transform.position = new Vector3(0, 0, CameraController.camera.transform.position.z);

        menuState = MenuState.MapScreen;
        ShowMenuSelected(0);
    }

    public void GoToPartyScene()
    {
        if (!InformationLogger.infoLogger.GetMenuIconsEnabled()[1])
            return;

        MusicController.music.PlaySFX(MusicController.music.uiUseLowSFX[Random.Range(0, MusicController.music.uiUseLowSFX.Count)]);
        MusicController.music.SetHighPassFilter(true);
        if (deckIncomplete)
        {
            StopAllCoroutines();
            StartCoroutine(NotificationFlashRed(3));
            return;
        }

        if (SceneManager.GetActiveScene().name != "TavernScene")
            SceneManager.LoadScene("TavernScene");

        desiredCameraLocation = new Vector2(0, 0);

        menuState = MenuState.PartyScreen;
        ShowMenuSelected(1);

        TutorialController.tutorial.TriggerTutorial(Dialogue.Condition.PartyMenuOpened, 1);
    }

    public void GoToGearScene()
    {
        if (!InformationLogger.infoLogger.GetMenuIconsEnabled()[2])
            return;

        MusicController.music.PlaySFX(MusicController.music.uiUseLowSFX[Random.Range(0, MusicController.music.uiUseLowSFX.Count)]);
        MusicController.music.SetHighPassFilter(true);
        if (SceneManager.GetActiveScene().name != "StoryModeScene")
        {
            SceneManager.LoadScene("StoryModeScene");
            desiredCameraLocation = new Vector2(100, 0);
        }
        else
            CameraController.camera.transform.position = new Vector3(100, 0, CameraController.camera.transform.position.z);

        CollectionController.collectionController.SetIsShowingCards(false);
        CollectionController.collectionController.SetPage(0);
        CollectionController.collectionController.ResetDeck();

        menuState = MenuState.GearScreen;
        ShowMenuSelected(2);
    }

    public void GoToCardScene()
    {
        if (!InformationLogger.infoLogger.GetMenuIconsEnabled()[3])
            return;

        MusicController.music.PlaySFX(MusicController.music.uiUseLowSFX[Random.Range(0, MusicController.music.uiUseLowSFX.Count)]);
        MusicController.music.SetHighPassFilter(true);
        if (SceneManager.GetActiveScene().name != "StoryModeScene")
        {
            SceneManager.LoadScene("StoryModeScene");
            desiredCameraLocation = new Vector2(100, 0);
        }
        else
            CameraController.camera.transform.position = new Vector3(100, 0, CameraController.camera.transform.position.z);

        CollectionController.collectionController.SetIsShowingCards(true);
        CollectionController.collectionController.SetPage(0);
        CollectionController.collectionController.ResetDeck();

        menuState = MenuState.CardScreen;
        ShowMenuSelected(3);
    }

    public void GoToShopScene()
    {
        if (!InformationLogger.infoLogger.GetMenuIconsEnabled()[4])
            return;

        MusicController.music.PlaySFX(MusicController.music.uiUseLowSFX[Random.Range(0, MusicController.music.uiUseLowSFX.Count)]);
        MusicController.music.SetHighPassFilter(true);
        if (SceneManager.GetActiveScene().name != "StoryModeShopScene")
        {
            SceneManager.LoadScene("StoryModeShopScene", LoadSceneMode.Single);
            TutorialController.tutorial.TriggerTutorial(Dialogue.Condition.ShopOpened, 1);
            desiredCameraLocation = new Vector2(0, 0);
        }

        menuState = MenuState.ShopScreen;
        ShowMenuSelected(4);
    }

    public MenuState GetMenuState()
    {
        return menuState;
    }

    public bool GetIsDeckIncomplete()
    {
        return deckIncomplete;
    }

    public void ShowMenuSelected(int index)
    {
        foreach (Image icon in menuIcons)
            icon.color = defaultMenuColor;
        menuIcons[index].color = selectedMenuColor;
        for (int i = 0; i < menuSelected.Length; i++)
            menuSelected[i].gameObject.SetActive(i == index);
    }

    public void RefreshMenuIconBlanOuts()
    {
        for (int i = 0; i < InformationLogger.infoLogger.GetMenuIconsEnabled().Length; i++)
        {
            menuBlankOuts[i].enabled = !InformationLogger.infoLogger.GetMenuIconsEnabled()[i];
            menuNames[i].enabled = InformationLogger.infoLogger.GetMenuIconsEnabled()[i];
        }
    }

    public void SetCombatInfoMenu(bool state)
    {
        combatInfoMenu.gameObject.SetActive(state);
    }

    public void RefreshAchievementValues()
    {
        if (achievementIcons[0] == null)
            return;

        for (int i = 0; i < 3; i++)
        {
            Color c = Color.cyan;
            int bestValue = currentRoomSetup.GetBestValues(currentRoomSetup.bestChallengeValues[i], AchievementSystem.achieve.GetChallengeValue(currentRoomSetup.challenges[i]), i, currentRoomSetup.challengeComparisonType[i]);
            if (ChallengeSatisfied(i, bestValue))
            {
                if (GetChallengeValues().ContainsKey(currentRoomId) && GetChallengeItemsBought().ContainsKey(currentRoomId) && GetChallengeItemsBought()[currentRoomId][i])
                    c = shopColor;
                else
                    c = goldColor;
            }
            else
            {
                c = completedColor;
                bestValue = AchievementSystem.achieve.GetChallengeValue(currentRoomSetup.challenges[i]);
            }

            achievementIcons[i].color = c;
            achievementPassiveIcons[i].color = c;

            achievementTexts[i].text = currentRoomSetup.GetChallengeText(currentRoomId, i, bestValue, true);
        }
    }

    public void SetAchievementImages(List<Image> values)
    {
        achievementIcons = values.ToArray();
    }

    public void RefreshGoldValue()
    {
        goldText.text = ResourceController.resource.GetGold().ToString();
    }

    public void SetAchievementInfoMenu(bool state)
    {
        if (state == true)
            TutorialController.tutorial.TriggerTutorial(Dialogue.Condition.AchievementHeld, 1);
        achievementMenu.gameObject.SetActive(state);
    }

    public void ShowMenuNotification(int index, bool state, bool notificationTypeNew)
    {
        if (!InformationLogger.infoLogger.GetMenuIconsEnabled()[index])
            return;

        if (notificationTypeNew)
            menuNotifications[index].color = newNotificationColor;
        else
            menuNotifications[index].color = warningNotificationColor;
        menuNotifications[index].enabled = state;

        if (index == 3 && !notificationTypeNew)
            deckIncomplete = state;
    }

    public IEnumerator NotificationFlashRed(int index)
    {
        float startingTime = Time.time;
        Color finalColor = defaultMenuColor;
        if (GetMenuType(index) == menuState)
            finalColor = selectedMenuColor;

        for (int i = 0; i < 20; i++)
        {
            menuIcons[index].color = Color.Lerp(warningNotificationColor, finalColor, i / 20.0f);
            yield return new WaitForSeconds(0.01f);
        }
        menuIcons[index].color = finalColor;
    }

    private MenuState GetMenuType(int index)
    {
        switch (index)
        {
            case 0:
                return MenuState.MapScreen;
            case 1:
                return MenuState.PartyScreen;
            case 2:
                return MenuState.GearScreen;
            case 3:
                return MenuState.CardScreen;
            case 4:
                return MenuState.ShopScreen;
        }
        return MenuState.MapScreen;
    }

    public void SetMenuBar(bool state)
    {
        for (int i = 0; i < 5; i++)
        {
            menuIcons[i].gameObject.SetActive(state);
        }
    }

    private void OnLevelWasLoaded(int level)
    {
        if (StoryModeController.story != this)
            return;
        if (new List<string> { "OverworldScene", "StoryModeScene" }.Contains(SceneManager.GetActiveScene().name))
            CameraController.camera.transform.position = new Vector3(desiredCameraLocation.x, desiredCameraLocation.y, CameraController.camera.transform.position.z);
    }

    public void SetDesiredCameraLocation(Vector2 loc)
    {
        desiredCameraLocation = loc;
    }

    public void SetLastSelectedRoomID(int value)
    {
        lastSelectedRoomID = value;
    }

    public int GetLastSelectedRoomID()
    {
        return lastSelectedRoomID;
    }

    public void SetLastSelectedAchievemnts(int value)
    {
        lastSelectedAchievements = value;
    }

    public int GetLastSelectedAchievents()
    {
        return lastSelectedAchievements;
    }

    public void SetLastSelectedComplete(bool value)
    {
        lastSelectedComplete = value;
    }

    public bool GetLastSelectedComplete()
    {
        return lastSelectedComplete;
    }

    public void SetWorldNumber(int value)
    {
        worldNumber = value;
    }

    public int GetWorldNumber()
    {
        return worldNumber;
    }

    public Dictionary<int, bool[]> GetSecretShopItemsBought()
    {
        if (secretShopItemsBought == null || secretShopItemsBought.Keys.Count == 0)
            for (int i = 0; i < 4; i++)
                secretShopItemsBought[i] = new bool[] { false, false, false, false, false };

        return secretShopItemsBought;
    }

    public void SetSecretShopItemsBought(Dictionary<int, bool[]> value)
    {
        if (value == null)
            for (int i = 0; i < 4; i++)
                secretShopItemsBought[i] = new bool[] { false, false, false, false, false };
        else
            secretShopItemsBought = value;
    }

    public void ReportSecretShopItemBought(int worldID, int index, bool bought)
    {
        secretShopItemsBought[worldID][index] = bought;
    }

    public void ReportColorsCompleted(int roomId, List<Card.CasterColor> colors)
    {
        if (colorsCompleted == null)
            colorsCompleted = new Dictionary<int, List<Card.CasterColor>>();
        if (!colorsCompleted.ContainsKey(roomId))
            colorsCompleted[roomId] = colors;
        else
        {
            List<Card.CasterColor> currentColors = colorsCompleted[roomId];
            currentColors.AddRange(colors);
            currentColors = currentColors.Distinct().ToList();
            colorsCompleted[roomId] = currentColors;
        }
    }

    public Dictionary<int, List<Card.CasterColor>> GetColorsCompleted()
    {
        return colorsCompleted;
    }

    public void SetColorsCompleted(Dictionary<int, List<Card.CasterColor>> value)
    {
        colorsCompleted = value;
    }

    public int GetUnspentChallengeTokens()
    {
        int spentTokens = 0;
        foreach (int worldId in secretShopItemsBought.Keys)
            for (int i = 0; i < secretShopItemsBought[worldId].Length; i++)
                if (secretShopItemsBought[worldId][i] && secretShops[worldId] != null)
                    spentTokens += secretShops[worldId].rewardCosts[i];

        return StoryModeSceneController.story.GetTotalChallengeTokens() - spentTokens;
    }

    public void EnableMenuIcon(int position)
    {
        if (InformationLogger.infoLogger.GetMenuIconsEnabled() != null && !InformationLogger.infoLogger.GetMenuIconsEnabled()[position])
        {
            bool[] menuIconsEnabled = InformationLogger.infoLogger.GetMenuIconsEnabled();
            menuIconsEnabled[position] = true;
            InformationLogger.infoLogger.SetMenuIconsEnabled(menuIconsEnabled);
            RefreshMenuIconBlanOuts();
        }
    }

    public int GetRewardsRerollLeft()
    {
        return rewardsRerollCount;
    }

    public void UseRewardsReroll(int value)
    {
        rewardsRerollCount -= value;
    }

    public void ResetRewardsRerollLeft()
    {
        rewardsRerollCount = 0;
        if (GetItemsBought().ContainsKey(RewardsType.PlusXRewardCardRerollPerRun))
            rewardsRerollCount += GetItemsBought()[RewardsType.PlusXRewardCardRerollPerRun];
    }

    public bool GetCanAbandon()
    {
        if (currentRoomType == StoryRoomController.StoryRoomType.Arena || currentRoomType == StoryRoomController.StoryRoomType.NakedArena)
            return true;                                        //Always allow for abandoning of arenas
        else if (currentRoomType == StoryRoomController.StoryRoomType.Combat || currentRoomType == StoryRoomController.StoryRoomType.Boss)
            if (GetCompletedRooms().Contains(currentRoomId))
                return true;                                    //Allow for abandoning of all completed combat rooms
        return false;
    }

    public void SetAbandonButton(bool state)
    {
        abandonRunButton.gameObject.SetActive(state);
        if (state)
            TutorialController.tutorial.TriggerTutorial(Dialogue.Condition.AbandonRunButton, 1);
    }

    public void SetAbondonButtonColor(Color color)
    {
        abandonRunButton.color = color;
        abandonRunButton.transform.GetChild(0).GetComponent<Image>().color = color;
    }

    public void SetAbandonWarningMenu(bool state)
    {
        abandonRunWarning.gameObject.SetActive(state);
    }

    public void AbandonRun()
    {
        SetAbandonWarningMenu(false);

        if (!InformationLogger.infoLogger.debug)
        {
            InformationLogger.infoLogger.SaveSinglePlayerRoomInfo(InformationLogger.infoLogger.patchID,
                InformationLogger.infoLogger.gameID,
                RoomController.roomController.worldLevel.ToString(),
                RoomController.roomController.selectedLevel.ToString(),
                RoomController.roomController.roomName,
                "Abandoned",
                ResourceController.resource.GetGold().ToString(),
                "0",
                ScoreController.score.GetOverKill().ToString(),
                ScoreController.score.GetDamage().ToString(),
                ScoreController.score.GetDamageArmored().ToString(),
                ScoreController.score.GetDamageOverhealProtected().ToString(),
                ScoreController.score.GetDamageAvoided().ToString(),
                ((int)ScoreController.score.GetSecondsInGame()).ToString(),
                "-1",
                AchievementSystem.achieve.GetChallengeValue(StoryModeController.story.GetCurrentRoomSetup().challenges[0]).ToString(),
                AchievementSystem.achieve.GetChallengeValue(StoryModeController.story.GetCurrentRoomSetup().challenges[1]).ToString(),
                AchievementSystem.achieve.GetChallengeValue(StoryModeController.story.GetCurrentRoomSetup().challenges[2]).ToString(),
                PartyController.party.GetPartyString(),
                "False",
                "False",
                "-1",
                "None",
                "",
                TutorialController.tutorial.GetErrorLogs());
        }

        MusicController.music.SetHighPassFilter(false);
        MusicController.music.SetLowPassFilter(false);
        HandController.handController.EmptyHand();
        DeckController.deckController.ResetCardValues();
        RelicController.relic.ResetRelics();
        TutorialController.tutorial.DestroyAndReset();
        TutorialController.tutorial.ResetCompletedTutorialIDs();

        ReturnToMapScene();
    }

    public void ReturnToMapScene()
    {
        MusicController.music.PlaySFX(MusicController.music.uiUseHighSFX);

        ResourceController.resource.ChangeGold(-ResourceController.resource.GetGold());
        ResourceController.resource.ResetReviveUsed();
        AchievementSystem.achieve.ResetAchievements();

        Destroy(RoomController.roomController.gameObject);
        RoomController.roomController = null;
        InformationLogger.infoLogger.SaveStoryModeGame();   //Must come before reset decks otherwise items will be overwritten
        InformationController.infoController.ResetCombatInfo();     //Reset all team stats tracking
        InformationController.infoController.firstRoom = true;
        ResetDecks();
        ResetRewardsRerollLeft();
        SetMenuBar(true);
        SetCombatInfoMenu(false);
        ShowMenuSelected(0);
        ResourceController.resource.EnableStoryModeRelicsMenu(false);

        SceneManager.LoadScene("StoryModeScene");

        ScoreController.score.SetTimerPaused(true);
        ScoreController.score.EnableTimerText(false);
        ScoreController.score.SetSecondsInGame(0);

        abandonRunButton.gameObject.SetActive(false);
    }

    public Color GetCompletedColor()
    {
        return completedColor;
    }

    public Color GetShopColor()
    {
        return shopColor;
    }

    public Color GetGoldColor()
    {
        return goldColor;
    }
}
