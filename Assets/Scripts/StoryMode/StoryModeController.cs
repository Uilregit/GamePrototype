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
    public Image[] menuNotifications;

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

    private Vector2 desiredCameraLocation;

    private MenuState menuState = MenuState.MapScreen;
    private bool deckIncomplete = false;

    private int secondSeed;
    private int dailySeed;
    private int weeklySeed;

    public enum MenuState
    {
        MapScreen = 0,
        PartyScreen = 10,
        GearScreen = 20,
        CardScreen = 30,
        SkillsScreen = 40
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

        SuperLike = 9000,
        RomancerPlus = 9001,
        RomancerGold = 9002,
        RomancerPlatinum = 9003,

        PlusXReplace = 10001,
        PlusXGoldPerRoom = 10002,
        PlusXRewardCardRerollPerRun = 10003,
        PlusXWeeklyWaresRerollPerWeek = 10004,
        PlusXDailyDealsRerollPerDay = 10005
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

        System.DateTime epochStart = new System.DateTime(2020, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
        secondSeed = (int)(System.DateTime.UtcNow - epochStart).Seconds;
        dailySeed = (int)(System.DateTime.UtcNow - epochStart).TotalDays;
        weeklySeed = (int)(dailySeed / 7);

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
        return ChallengeSatisfied(index, GetChallengeValues()[currentRoomId][index]);
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
                return value <= currentRoomSetup.challengeValues[index] && value != -1;
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
    }

    public void ReportEquipmentBought(Dictionary<Equipment, int> equipments)
    {
        Dictionary<string, int> names = new Dictionary<string, int>();
        foreach (Equipment e in equipments.Keys)
            names[e.equipmentName] = equipments[e];
        CollectionController.collectionController.SetCompleteEquipments(names, true);
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
    }

    public void SetDailyBought(Dictionary<int, bool[]> daily)
    {
        dailyBought = daily;
        if (daily == null)
        {
            dailyBought = new Dictionary<int, bool[]>();
            dailyBought[GetDailySeed()] = new bool[] { false, false, false };
        }
    }

    public void SetWeeklyBought(Dictionary<int, bool[]> weekly)
    {
        weeklyBought = weekly;
        if (weekly == null)
        {
            weeklyBought = new Dictionary<int, bool[]>();
            weeklyBought[GetWeeklySeed()] = new bool[] { false, false, false, false, false, false };
        }
    }

    public Dictionary<int, bool[]> GetDailyBought()
    {
        if (!dailyBought.ContainsKey(GetDailySeed()))
            dailyBought[GetDailySeed()] = new bool[] { false, false, false };

        return dailyBought;
    }

    public Dictionary<int, bool[]> GetWeeklyBought()
    {
        if (!weeklyBought.ContainsKey(GetWeeklySeed()))
            weeklyBought[GetWeeklySeed()] = new bool[] { false, false, false, false, false, false };

        return weeklyBought;
    }

    public void SetCardBought(bool isDailyCard, int index)
    {
        if (isDailyCard)
            dailyBought[GetDailySeed()][index] = true;
        else
            weeklyBought[GetWeeklySeed()][index] = true;
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

    public int GetSecondSeed()
    {
        return secondSeed;
    }

    public int GetDailySeed()
    {
        return dailySeed;
    }

    public int GetWeeklySeed()
    {
        return weeklySeed;
    }

    public void GoToMapScene()
    {
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
    }

    public void GoToGearScene()
    {
        Debug.Log("go to gear scene");
        if (SceneManager.GetActiveScene().name != "StoryModeScene")
        {
            SceneManager.LoadScene("StoryModeScene");
            desiredCameraLocation = new Vector2(8, 0);
        }
        else
            CameraController.camera.transform.position = new Vector3(8, 0, CameraController.camera.transform.position.z);

        CollectionController.collectionController.SetIsShowingCards(false);
        CollectionController.collectionController.SetPage(0);
        CollectionController.collectionController.ResetDeck();

        menuState = MenuState.GearScreen;
        ShowMenuSelected(2);
    }

    public void GoToCardScene()
    {
        if (SceneManager.GetActiveScene().name != "StoryModeScene")
        {
            SceneManager.LoadScene("StoryModeScene");
            desiredCameraLocation = new Vector2(8, 0);
        }
        else
            CameraController.camera.transform.position = new Vector3(8, 0, CameraController.camera.transform.position.z);

        CollectionController.collectionController.SetIsShowingCards(true);
        CollectionController.collectionController.SetPage(0);
        CollectionController.collectionController.ResetDeck();

        menuState = MenuState.CardScreen;
        ShowMenuSelected(3);
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
    }

    public void SetCombatInfoMenu(bool state)
    {
        combatInfoMenu.gameObject.SetActive(state);
    }

    public void RefreshAchievementValues()
    {
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

    public void RefreshGoldValue()
    {
        goldText.text = ResourceController.resource.GetGold().ToString();
    }

    public void SetAchievementInfoMenu(bool state)
    {
        achievementMenu.gameObject.SetActive(state);
    }

    public void ShowMenuNotification(int index, bool state, bool notificationTypeNew)
    {
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
                return MenuState.SkillsScreen;
        }
        return MenuState.MapScreen;
    }

    public void SetMenuBar(bool state)
    {
        //GetComponent<Canvas>().enabled = state;
        for (int i = 0; i < 5; i++)
        {
            menuIcons[i].gameObject.SetActive(state);
            /*
            menuIcons[i].enabled = state;
            menuIcons[i].transform.GetChild(0).GetComponent<Image>().enabled = state;
            menuIcons[i].transform.GetChild(1).GetComponent<Text>().enabled = state;

            menuNotifications[i].enabled = state;
            */
        }
    }

    private void OnLevelWasLoaded(int level)
    {
        if (StoryModeController.story != this)
            return;
        if (SceneManager.GetActiveScene().name != "CombatScene")
            CameraController.camera.transform.position = new Vector3(desiredCameraLocation.x, desiredCameraLocation.y, CameraController.camera.transform.position.z);
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
            for (int i = 0; i < 3; i++)
                secretShopItemsBought[i] = new bool[] { false, false, false, false, false };

        return secretShopItemsBought;
    }

    public void SetSecretShopItemsBought(Dictionary<int, bool[]> value)
    {
        if (value == null)
            for (int i = 0; i < 3; i++)
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
                if (secretShopItemsBought[worldId][i])
                    spentTokens += secretShops[worldId].rewardCosts[i];

        return StoryModeSceneController.story.GetTotalChallengeTokens() - spentTokens;
    }
}
