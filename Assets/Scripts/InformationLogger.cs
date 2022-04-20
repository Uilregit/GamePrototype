using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Microsoft.Win32.SafeHandles;
using System.Linq;
using Google.Apis.Services;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;

[System.Serializable]
public class SaveFile
{
    public bool newGame;
    public int gameSeed;
    public int stateSeed;
    public string gameID;
    public string patchID;

    public int worldLevel;

    public string player1Color;
    public string player2Color;
    public string player3Color;

    public int level;
    //public int roomRandomizedIndex;
    public float viableRoomX;
    public float viableRoomY;
    public float[] previousRoomsX;
    public float[] previousRoomsY;
    public float[] destroyedRoomsX;
    public float[] destroyedRoomsY;

    /*
    public List<string> drawPileOrder;
    public List<string> discardPileOrder;
    */

    public int lives;
    public Dictionary<Card.CasterColor, bool> deadChars;
    public Dictionary<Card.CasterColor, int> playerVit;
    public Dictionary<Card.CasterColor, int> playerMaxVit;
    public Dictionary<Card.CasterColor, int> playerAtk;
    public Dictionary<Card.CasterColor, int> playerArmor;

    public string[] collectionCardNames;
    public Dictionary<string, string[]> selectedCardNames;
    public string[] newCardNames;
    public string recentRewardsCard;

    public string[] relicNames;
    public int[] validChoices;
    public int gold;

    public int overkill;
    public int damage;
    public int damageArmored;
    public int damageOverhealedProtected;
    public int damageAvoided;
    public int enemiesBroken;
    public int goldUsed;
    public int bossesDefeated;
    public float secondsInGame;
}

[System.Serializable]
public class StoryModeSaveFile
{
    public string patchID;
    public int[] unlockedRoomIds;
    //public Dictionary<int, bool[]> challengeStars;    //Room id, stars 1, 2, 3
    public Dictionary<int, int[]> challengeValues;      //Room id, value 1, 2, 3
    public int lastSelectedRoomId;                      //Id of the last room that was selected
    public int lastSelectedAchievements;                //Number of achievements at the time of the start of the room
    public bool lastSelectedComplete;                   //If the last room was complete before that room was selected
    public int worldNumber;                             //The world that was last selected before save

    public Dictionary<string, string[]> cardSelected;   //<casterColor, cardNames>
    public string[] cardCraftable;
    public Dictionary<string, int> cardUnlocks;         //<cardName, cardAmount>
    public string[] completeEquipments;
    public Dictionary<string, string[]> selectedEquipments;
    public Dictionary<string, int> weaponUnlocks;       //<equipmentName, equipmentAmount>
    public Dictionary<StoryModeController.RewardsType, int> itemsUnlocked;

    public Dictionary<int, bool[]> challengeItemsBought;//<roomID, ifBought>
    public Dictionary<int, List<Card.CasterColor>> colorsCompleted; //<roomId, colorsCompleted>
    public Dictionary<int, bool[]> secretShopItemsBought;//<worldID, ifBought>

    public Dictionary<int, bool[]> dailyBought;         //<dayIDSeed, ifBought>
    public Dictionary<int, bool[]> weeklyBought;        //<weekIDSeed, ifBought>
}

[System.Serializable]
public class PlayerPreferences
{
    public string lastPatchRead = "";
    public int latestDateShopOpened = 0;
    public int dailyRerollsLeft = 0;
    public int weeklyRerollsLeft = 0;
    public bool tutorialCompleted = false;
    public List<int> passiveTutorialCompletedIds = new List<int>();

    public bool[] menuIconEnabled = new bool[] { true, false, false, false, false };

    public string party1;
    public string party2;
    public string party3;

    public int teamLevel;
    public int currentEXP;

    public int redLevel;
    public int redCurrentEXP;
    public int blueLevel;
    public int blueCurrentEXP;
    public int greenLevel;
    public int greenCurrentEXP;
    public int orangeLevel;
    public int orangeCurrentEXP;
    public int whiteLevel;
    public int whiteCurrentEXP;
    public int blackLevel;
    public int blackCurrentEXP;

    public int highestOverkillScore;
    public int highestDamageScore;
    public int highestDamageArmoredScore;
    public int highestDamageOverhealedProtectedScore;
    public int highestDamageAvoidedScore;
    public int highestEnemiesBrokenScore;
    public int highestGoldUsedScore;
    public int highestBossesDefeatedScore;
    public int highestSecondsInGameScore;
    public int highestTotalScore;
}

[System.Serializable]
public class Settings
{
    public int backgroundMusicVolume = 5;
    public bool backgroundMusicMuted = false;
    public int soundEffectsVolume = 5;
    public bool soundEffectsMuted = false;

    public int gameSpeedIndex;
    public int screenShakeIndex;
    public bool remainingMoveRangeIndicator;
}

[System.Serializable]
public class Unlocks
{
    public bool firstTutorialRoomCompleted;
    public Dictionary<UIRevealController.UIElement, bool> uiElementUnlocked;
    public bool classicModeUnlocked;

    public bool orangeUnlocked;
    public bool whiteUnlocked;
    public bool blackUnlocked;

    public int tavernContracts;
    public int largestBoss;

    public int sand;
    public int shards;

    public int redGoldCardNum;
    public int blueGoldCardNum;
    public int greenGoldCardNum;
    public int orangeGoldCardNum;
    public int whiteGoldCardNum;
    public int blackGoldCardNum;

    public string[] unlockedCards;
    public int[] unlockedCardsNumber;
    public string[] unlockedRelics;
    public string[] unlockedWeapons;
    public string[] unlockedArmor;
    public string[] unlockedSkins;

    public bool holdUnlocked;
    public int replaceUnlocked;
    public int livesUnlocked;

    public bool[] redTalentUnlocked;
    public bool[] blueTalentUnlocked;
    public bool[] greenTalentUnlocked;
    public bool[] orangeTalentUnlocked;
    public bool[] whtieTalentUnlocked;
    public bool[] blackTalentUnlocked;

    public int redCustomizableCardUnlocked;
    public int blueCustomizableCardUnlocked;
    public int greenCustomizableCardUnlocked;
    public int orangeCustomizableCardUnlocked;
    public int whiteCustomizableCardUnlocked;
    public int blackCustomizableCardUnlocked;

    public int ascentionTierUnlocked;
    public int totalEXP;
}

public class InformationLogger : MonoBehaviour
{
    public static InformationLogger infoLogger;

    public bool debug;
    public bool debugBossRoomEnabled;
    public bool debugStoryCopyRealSaveFile;
    public bool debugStoryCopyDebugSaveFile;
    public bool isStoryMode = false;
    public string patchID;
    private string lastPatchRead = "";
    private int latestDayShopOpened = 0;
    private int dailyRerollsLeft = 0;
    private int weeklyRerollsLeft = 0;
    private bool tutorialCompleted = false;
    private bool[] menuIconEnabled = new bool[] { true, false, false, false, false };
    public int seed;
    public string gameID;
    public Text versionText;
    public Text seedText;

    private int secondSeed;
    private int dailySeed;
    private int weeklySeed;

    public int roomRandomizedIndex = -1;

    public bool loadGameOnLevelLoad = false;
    private int roomControllerRoomLevel;
    private List<Vector2> roomControllerPreviousRooms;

    private string spreadsheetID = "1Pl2QRxb2CzoaFNLdJXfmrJwbNjVxZLPg12QOtmSgoVk";
    private string serviceAccountID = "unity-google-sheets@solar-century-183500.iam.gserviceaccount.com";
    private string key = "-----BEGIN PRIVATE KEY-----\nMIIEvAIBADANBgkqhkiG9w0BAQEFAASCBKYwggSiAgEAAoIBAQC8PoFHQPNvKQkK\nHdRginvFTXpUptXw/jwYgcx1YdsFm9ZY+v1Ufiem8f8okqfn65XcwNu+nJLEiCoR\nULWsGZ1n2PEBk/WtC+9Jnf8/VBkBEEv1B1MkfU6fRKEkqewneaiEFXrASVJ/eVA7\n2bJ3GD8sOyDCrTok9oTz8KIuyy+BoCZRhV7mAH4pAZwFjqMjaF9ap4qt9hvzk6uR\nr4OLfT0QL+SGav4OnnhUOq8kVdjF9ACOpQtH2queD5973bVAYUWlOrRlowFb8dj6\nbjZW5mJoJJjJfa/ecmRa3sEBm+zTivdO7cFv9uTePt75qEs+unyZKTmhngZCA+1K\n9xFJVLbFAgMBAAECggEABYi2Nui16g1Zcq6we6tr5WpYWlISoA8ZUoRzr6USJ0wO\n38whKRwRq7AZluZuK+dFH0RG3DJbg45CjlfCai4mmShLjYb9W0+qgo4z3sy+H3CM\nixuNWi6aN8cbZaneg2/pgFiRT0Mt5bQko7MhXVbyedO91tCjkPVJBf2LsbckzQ63\nL8NlSkFysyP8kPeODmzwO1YdYnaHHFSwQOoMI6rQ+re3Da61YRTT5NChkNhx0K2O\nF+TsvyWLPJmG0oap2HiRsoKZYcktX0w0wFPlOyoFnb9el+dsLGBN2vsQ+R3R4UtT\nK6B4Qy0E1GZF85LJwv66JyygYUve9AIOIRzJrxr6AQKBgQD+jv7HcNzvV0842smr\nwIzKmLDBFH5+MhOqdQ15G3N+fCapVezOZRCZjh9OM6HIH+laC1Yzc25ZSrx0tomq\nVWRIt/wQycbGw9S5hbc65qRpOSK7+doi14DHq7pkoCW8yBak/DVcLVoF5aZxbdbt\nX1JGsm2Sm/RPMwXO+iGtaq3I5QKBgQC9T2GaEqCB/DaW8vJzIwkc9Y+VQn32T8nN\nsupcPHIB35ecns7re42QP+zSqNzQMH1CwUSo30evwntIWZfYt/+2LxwbUdIQuICB\nLcOH4FLglrGQBN/GobhyQpiV7yzMa35brMRHuJwpXjqS2/lnu+ja8P5TJVd6fCsC\nyfrBEku4YQKBgHvBHwHsz6wYAS69xu+V05ym8L9dbEWDqOXktCEdhF+Ike8fE9of\nbhuI6ZVGKq+1O+gHvOeUhaApYkdHetPxYaissYGj5tw41lE/PZ4IBQQWv9ktFax8\nomHwDdTfupj1mXSqHHLspWhahjl80dFi1wgBtJ1i7joWrws5tWeuhkA1AoGAXCP4\n417RlLLHdy0EaJnS4695lTJp0KsBFAdTHlWlP9guGOMK336hZmZWxCnAX+xZ61Xa\nLz+NyrQkARDqYWcdJPVE/t8SGWVT1owJsWazr/BouCpHKIyqE6LqVX+2FED1nXU3\ni5kFGPVuGPDMMXs6WOYXQyzXqRwqaw8X50UaacECgYBCU4gxy0GPXW/G8sg8Oh2F\n3/qWYVomS6NuCSo758nBaUZcNeI0ZIMYp3wDmkSYJOLxR1ej8TsGCGRN1VMtjRwk\n6jMZcET33aHX+kMoG9G2Hlk9XQDjG0CIIz0gcKEUdjJ3zvJsVEjG7QMRHR0lWwV0\nlprroriBtrj4xMQOnW8KKg==\n-----END PRIVATE KEY-----\n";
    private SheetsService service;

    void Awake()
    {
        if (InformationLogger.infoLogger == null)
            InformationLogger.infoLogger = this;
        else
        {
            Destroy(this.gameObject);
            return;
        }

        DontDestroyOnLoad(this.gameObject);

        SceneManager.sceneLoaded += OnLevelFinishedLoading;

        if (seed == 0)
            seed = Random.Range(1, 1000000000);
        Random.InitState(seed);

        gameID = System.DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");

        System.DateTime epochStart = new System.DateTime(2020, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
        secondSeed = (int)(System.DateTime.UtcNow - epochStart).TotalSeconds;
        dailySeed = (int)(System.DateTime.UtcNow - epochStart).TotalDays;
        weeklySeed = (int)(dailySeed / 7);

        try                 //For debugging purposes, prevents crashes when running from overworldScene
        {
            versionText.text = "Version: " + patchID;
            seedText.text = "Seed: " + seed;
            versionText.enabled = true;
            seedText.enabled = true;
        }
        catch { }

        ServiceAccountCredential.Initializer initializer = new ServiceAccountCredential.Initializer(serviceAccountID);

        ServiceAccountCredential credential = new ServiceAccountCredential(initializer.FromPrivateKey(key));
        service = new SheetsService(new BaseClientService.Initializer() { HttpClientInitializer = credential });
    }

    public void SaveCombatInfo(string patchID, string gameID, string worldLevel, string encounterLevel, string roomName, string turnID, string castOrder, string cardColor, string cardName, string heldFlag, string replacedFlag,
                               string unplayedFlag, string castFlag, string casterName, string targetCount, string targetName, string vitDamageDone, string armorDamageDone, string manaGenerated, string manaUsed, string buffTriggerCount,
                               string attachedEquipment)
    {
        if (InformationLogger.infoLogger.debug)
            return;

        string filePath = GetCombatPath() + "/combat_data_" + SystemInfo.deviceUniqueIdentifier + ".csv";
        //DebugPlus.LogOnScreen(filePath).Duration(5);
        //DebugPlus.LogOnScreen(SystemInfo.deviceUniqueIdentifier);
        //Debug.Log(filePath);

        string header = "deviceID,patchID,gameID,worldLevel,encounterLevel,roomName,turnID,timestamp,castOrder,cardColor,cardName,heldFlag,replacedFlag,unplayedFlag,castFlag,casterName,targetCount,targetName,vitDamageDone,armorDamageDone,manaGenerated,manaUsed,buffTriggerCount,attachedEquipment";

        if (!File.Exists(filePath))
            File.WriteAllText(filePath, header + "\n");

        string delimiter = ",";
        string line = SystemInfo.deviceUniqueIdentifier;

        line += delimiter + patchID;
        line += delimiter + gameID;
        line += delimiter + worldLevel;
        line += delimiter + encounterLevel;
        line += delimiter + roomName;
        line += delimiter + turnID;
        line += delimiter + (int)ScoreController.score.GetSecondsInGame();
        line += delimiter + castOrder;
        line += delimiter + cardColor;
        line += delimiter + cardName;
        line += delimiter + heldFlag;
        line += delimiter + replacedFlag;
        line += delimiter + unplayedFlag;
        line += delimiter + castFlag;
        line += delimiter + casterName;
        line += delimiter + targetCount;
        line += delimiter + targetName;
        line += delimiter + vitDamageDone;
        line += delimiter + armorDamageDone;
        line += delimiter + manaGenerated;
        line += delimiter + manaUsed;
        line += delimiter + buffTriggerCount;
        line += delimiter + attachedEquipment;

        File.AppendAllText(filePath, line + "\n");
    }

    public void SaveRewardsCardInfo(string patchID, string gameID, string worldLevel, string encounterLevel, string roomName, string cardColor, string cardName, string energyCost, string manaCost, string chosenFlag, string immediatelyUsedFlag)
    {
        if (InformationLogger.infoLogger.debug)
            return;

        string filePath = GetCombatPath() + "/rewards_data_" + SystemInfo.deviceUniqueIdentifier + ".csv";
        //DebugPlus.LogOnScreen(filePath).Duration(5);
        //DebugPlus.LogOnScreen(SystemInfo.deviceUniqueIdentifier);
        //Debug.Log(filePath);

        string header = "deviceID,patchID,gameID,worldLevel,encounterLevel,roomName,cardColor,cardName,chosenFlag,immediatelyUsedFlag";

        if (!File.Exists(filePath))
            File.WriteAllText(filePath, header + "\n");

        string delimiter = ",";
        string line = SystemInfo.deviceUniqueIdentifier;

        line += delimiter + patchID;
        line += delimiter + gameID;
        line += delimiter + worldLevel;
        line += delimiter + encounterLevel;
        line += delimiter + roomName;
        line += delimiter + cardColor;
        line += delimiter + cardName;
        line += delimiter + chosenFlag;
        line += delimiter + immediatelyUsedFlag;

        File.AppendAllText(filePath, line + "\n");
    }

    public void SaveShopCardInfo(string patchID, string gameID, string worldLevel, string encounterLevel, string roomName, string cardColor, string cardName, string energyCost, string manaCost, string chosenFlag, string goldUsed)
    {
        if (InformationLogger.infoLogger.debug)
            return;

        string filePath = GetCombatPath() + "/shop_data_" + SystemInfo.deviceUniqueIdentifier + ".csv";
        //DebugPlus.LogOnScreen(filePath).Duration(5);
        //DebugPlus.LogOnScreen(SystemInfo.deviceUniqueIdentifier);
        //Debug.Log(filePath);

        string header = "deviceID,patchID,gameID,worldLevel,encounterLevel,roomName,cardColor,cardName,chosenFlag,goldUsed";

        if (!File.Exists(filePath))
            File.WriteAllText(filePath, header + "\n");

        string delimiter = ",";
        string line = SystemInfo.deviceUniqueIdentifier;

        line += delimiter + patchID;
        line += delimiter + gameID;
        line += delimiter + worldLevel;
        line += delimiter + encounterLevel;
        line += delimiter + roomName;
        line += delimiter + cardColor;
        line += delimiter + cardName;
        line += delimiter + chosenFlag;
        line += delimiter + goldUsed;

        File.AppendAllText(filePath, line + "\n");
    }

    public void SaveGoldInfo(string patchID, string gameID, string worldLevel, string encounterLevel, string roomName, string passiveGold, string overkillGold, string totalGoldAtRoomEnd)
    {
        if (InformationLogger.infoLogger.debug)
            return;

        string filePath = GetCombatPath() + "/gold_data_" + SystemInfo.deviceUniqueIdentifier + ".csv";
        //DebugPlus.LogOnScreen(filePath).Duration(5);
        //DebugPlus.LogOnScreen(SystemInfo.deviceUniqueIdentifier);
        //Debug.Log(filePath);

        string header = "deviceID,patchID,gameID,worldLevel,encounterLevel,roomName,passiveGold,overkillGold,totalGoldAtRoomEnd";

        if (!File.Exists(filePath))
            File.WriteAllText(filePath, header + "\n");

        string delimiter = ",";
        string line = SystemInfo.deviceUniqueIdentifier;

        line += delimiter + patchID;
        line += delimiter + gameID;
        line += delimiter + worldLevel;
        line += delimiter + encounterLevel;
        line += delimiter + roomName;
        line += delimiter + passiveGold;
        line += delimiter + overkillGold;
        line += delimiter + totalGoldAtRoomEnd;

        File.AppendAllText(filePath, line + "\n");
    }

    public void SaveGameScoreInfo(string patchID, string gameID, string worldLevel, string encounterLevel, string roomName, string gameWon, string gameLost, string totalScore, string overkill, string damage,
                                    string damageArmored, string damageOverhealedProtected, string damageAvoided, string enemiesBroken, string goldUSed, string bossesDefeated, string secondsInGame)
    {
        if (InformationLogger.infoLogger.debug)
            return;

        string filePath = GetCombatPath() + "/gameScore_data_" + SystemInfo.deviceUniqueIdentifier + ".csv";
        //DebugPlus.LogOnScreen(filePath).Duration(5);
        //DebugPlus.LogOnScreen(SystemInfo.deviceUniqueIdentifier);
        //Debug.Log(filePath);

        string header = "deviceID,patchID,gameID,worldLevel,encounterLevel,roomName,gameWon,gameLost,totalScore,overkill,damage,damageArmored,damageOverhealedProtected,damageAvoided,enemiesBroken,goldUSed,bossesDefeated,secondsInGame";

        if (!File.Exists(filePath))
            File.WriteAllText(filePath, header + "\n");

        string delimiter = ",";
        string line = SystemInfo.deviceUniqueIdentifier;

        line += delimiter + patchID;
        line += delimiter + gameID;
        line += delimiter + worldLevel;
        line += delimiter + encounterLevel;
        line += delimiter + roomName;
        line += delimiter + gameWon;
        line += delimiter + gameLost;
        line += delimiter + totalScore;
        line += delimiter + overkill;
        line += delimiter + damage;
        line += delimiter + damageArmored;
        line += delimiter + damageOverhealedProtected;
        line += delimiter + damageAvoided;
        line += delimiter + enemiesBroken;
        line += delimiter + goldUSed;
        line += delimiter + bossesDefeated;
        line += delimiter + secondsInGame;

        File.AppendAllText(filePath, line + "\n");
    }

    public void SaveSinglePlayerRoomInfo(string patchID, string gameID, string worldLevel, string encounterLevel, string roomName, string gameWon, string totalGold, string expGained, string overkill, string totalDamage,
                                string damageArmored, string damageOverhealedProtected, string damageAvoided, string secondsInGame, string numCardUsed, string achievement1Value, string achievement2Value, string achievement3Value,
                                string partyUsed, string isEndRoom, string isImpromptuFeedback, string ratingOutOf5, string feedbackType, string freeTextField, string errorLogs)
    {
        if (InformationLogger.infoLogger.debug)
            return;

        string filePath = GetCombatPath() + "/singlePlayerRoom_data_" + SystemInfo.deviceUniqueIdentifier + ".csv";

        string header = "deviceID,patchID,gameID,worldLevel,encounterLevel,roomName,gameWon,totalGold,expGained,overkill,totalDamage,damageArmored,damageOverhealedProtected,damageAvoided,secondsInGame,numCardUsed,achievement1Value,achievement2Value,achievement3Value,partyUsed,isEndRoom,isImpromptuFeedback,ratingOutOf5,feedbackType,freeTextField,errorLogs";

        if (!File.Exists(filePath))
            File.WriteAllText(filePath, header + "\n");

        List<object> value = new List<object>();
        value.Add(SystemInfo.deviceUniqueIdentifier);
        value.Add(patchID);
        value.Add(gameID);
        value.Add(worldLevel);
        value.Add(encounterLevel);
        value.Add(roomName);
        value.Add(gameWon);
        value.Add(totalGold);
        value.Add(expGained);
        value.Add(overkill);
        value.Add(totalDamage);
        value.Add(damageArmored);
        value.Add(damageOverhealedProtected);
        value.Add(damageAvoided);
        value.Add(secondsInGame);
        value.Add(numCardUsed);
        value.Add(achievement1Value);
        value.Add(achievement2Value);
        value.Add(achievement3Value);
        value.Add(partyUsed);
        value.Add(isEndRoom);
        value.Add(isImpromptuFeedback);
        value.Add(ratingOutOf5);
        value.Add(feedbackType);
        value.Add(freeTextField);
        value.Add(errorLogs);

        string delimiter = ",";
        string line = SystemInfo.deviceUniqueIdentifier;

        for (int i = 1; i < value.Count; i++)
            line += delimiter + (string)value[i];

        File.AppendAllText(filePath, line + "\n");

        //try
        //{
        string range = "Sheet1!A:Z";
        SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum valueInputOption = (SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum)1;
        SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum insertDataOption = (SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum)1;
        Google.Apis.Sheets.v4.Data.ValueRange requestBody = new Google.Apis.Sheets.v4.Data.ValueRange();
        requestBody.Range = "Sheet1!A:Z";
        requestBody.Values = new List<IList<object>>();
        requestBody.Values.Add(value);
        requestBody.MajorDimension = "ROWS";
        SpreadsheetsResource.ValuesResource.AppendRequest request = service.Spreadsheets.Values.Append(requestBody, spreadsheetID, range);
        request.ValueInputOption = valueInputOption;
        request.InsertDataOption = insertDataOption;
        request.ExecuteAsync();
        //}
        //catch { }
    }

    public void SaveDeckInfo(string patchID, string gameID, string worldLevel, string encounterLevel, string cardColor, string cardName, string energyCost, string manaCost, string chosenFlag, string removedFlag, string finalDeckListFlag, string attachedEquipment)
    {
        if (InformationLogger.infoLogger.debug)
            return;

        string filePath = GetCombatPath() + "/deck_data_" + SystemInfo.deviceUniqueIdentifier + ".csv";
        //DebugPlus.LogOnScreen(filePath).Duration(5);
        //DebugPlus.LogOnScreen(SystemInfo.deviceUniqueIdentifier);
        //Debug.Log(filePath);

        string header = "deviceID,patchID,gameID,worldLevel,encounterLevel,cardColor,cardName,energyCost,manaCost,chosenFlag,removedFlag,finalDeckListFlag,attachedEquipment";

        if (!File.Exists(filePath))
            File.WriteAllText(filePath, header + "\n");

        string delimiter = ",";
        string line = SystemInfo.deviceUniqueIdentifier;

        line += delimiter + patchID;
        line += delimiter + gameID;
        line += delimiter + worldLevel;
        line += delimiter + encounterLevel;
        line += delimiter + cardColor;
        line += delimiter + cardName;
        line += delimiter + energyCost;
        line += delimiter + manaCost;
        line += delimiter + chosenFlag;
        line += delimiter + removedFlag;
        line += delimiter + finalDeckListFlag;
        line += delimiter + attachedEquipment;

        File.AppendAllText(filePath, line + "\n");
    }

    public void SaveTimeInfo(string patchID, string gameID, string worldLevel, string encounterLevel, string roomType, string roomName, string turnId = "0")
    {
        if (InformationLogger.infoLogger.debug)
            return;

        string filePath = GetCombatPath() + "/time_data_" + SystemInfo.deviceUniqueIdentifier + ".csv";
        //DebugPlus.LogOnScreen(filePath).Duration(5);
        //DebugPlus.LogOnScreen(SystemInfo.deviceUniqueIdentifier);
        //Debug.Log(filePath);

        string header = "deviceID,patchID,gameID,worldLevel,encounterLevel,roomType,roomName,turnId,timestamp";

        if (!File.Exists(filePath))
            File.WriteAllText(filePath, header + "\n");

        string delimiter = ",";
        string line = SystemInfo.deviceUniqueIdentifier;

        line += delimiter + patchID;
        line += delimiter + gameID;
        line += delimiter + worldLevel;
        line += delimiter + encounterLevel;
        line += delimiter + roomType;
        line += delimiter + roomName;
        line += delimiter + turnId;
        line += delimiter + (int)ScoreController.score.GetSecondsInGame();

        File.AppendAllText(filePath, line + "\n");
    }

    private string GetCombatPath()
    {
        //if UNITY_EDITOR
        if (Application.isEditor)
            return Application.dataPath;
        //elif UNITY_ANDROID
        return Application.persistentDataPath;
        //elif UNITY_IPHONE
        //return Application.persistentDataPath + "/" + "Saved_data.csv";
        //else
        //return Application.dataPath + "/" + "Saved_data.csv";
    }

    //Save/Load Game
    private SaveFile GetSaveFile(bool endOfGame)
    {
        SaveFile saveFile = new SaveFile();
        saveFile.newGame = endOfGame;
        if (!endOfGame)
        {
            saveFile.gameSeed = seed;
            saveFile.stateSeed = Random.seed;
            saveFile.gameID = gameID;
            saveFile.patchID = patchID;

            string[] playerColors = PartyController.party.GetPlayerColorTexts();
            saveFile.player1Color = playerColors[0];
            saveFile.player2Color = playerColors[1];
            saveFile.player3Color = playerColors[2];

            saveFile.level = RoomController.roomController.selectedLevel;
            saveFile.worldLevel = RoomController.roomController.GetWorldLevel();
            //saveFile.roomRandomizedIndex = RoomController.roomController.GetRandomizedRoomIndex();

            saveFile.viableRoomX = RoomController.roomController.GetViableRoom().x;
            saveFile.viableRoomY = RoomController.roomController.GetViableRoom().y;

            List<Vector2> previousRoom = RoomController.roomController.GetPreviousRoom();
            float[] previousRoomX = new float[previousRoom.Count];
            float[] previousRoomY = new float[previousRoom.Count];
            for (int i = 0; i < previousRoom.Count; i++)
            {
                previousRoomX[i] = previousRoom[i].x;
                previousRoomY[i] = previousRoom[i].y;
            }
            saveFile.previousRoomsX = previousRoomX;
            saveFile.previousRoomsY = previousRoomY;

            List<Vector2> destroyedRooms = RoomController.roomController.GetDestroyRooms();
            float[] destroyedRoomsX = new float[destroyedRooms.Count];
            float[] destroyedRoomsY = new float[destroyedRooms.Count];
            for (int i = 0; i < destroyedRooms.Count; i++)
            {
                destroyedRoomsX[i] = destroyedRooms[i].x;
                destroyedRoomsY[i] = destroyedRooms[i].y;
            }
            saveFile.destroyedRoomsX = destroyedRoomsX;
            saveFile.destroyedRoomsY = destroyedRoomsY;

            CombatInfo combatInfo = InformationController.infoController.GetCombatInfo();
            saveFile.lives = combatInfo.lives;
            saveFile.deadChars = combatInfo.deadChars;
            saveFile.playerVit = combatInfo.vit;
            saveFile.playerMaxVit = combatInfo.maxVit;
            saveFile.playerAtk = combatInfo.atk;
            saveFile.playerArmor = combatInfo.armor;

            saveFile.collectionCardNames = CollectionController.collectionController.GetCompleteDeckNames();
            saveFile.selectedCardNames = CollectionController.collectionController.GetSelectedDeckNames();
            saveFile.newCardNames = CollectionController.collectionController.GetNewCardDeckNames();
            saveFile.recentRewardsCard = CollectionController.collectionController.GetRecentRewardsCard();

            string[] relicNames = new string[RelicController.relic.relics.Count];
            for (int i = 0; i < RelicController.relic.relics.Count; i++)
                relicNames[i] = RelicController.relic.relics[i].relicName;
            saveFile.relicNames = relicNames;
            saveFile.validChoices = RelicController.relic.GetValidChoices();
            saveFile.gold = ResourceController.resource.GetGold();

            saveFile.overkill = ScoreController.score.GetOverKill();
            saveFile.damage = ScoreController.score.GetDamage();
            saveFile.damageArmored = ScoreController.score.GetDamageArmored();
            saveFile.damageOverhealedProtected = ScoreController.score.GetDamageOverhealProtected();
            saveFile.damageAvoided = ScoreController.score.GetDamageAvoided();
            saveFile.enemiesBroken = ScoreController.score.GetEnemiesBroken();
            saveFile.goldUsed = ScoreController.score.GetGoldUsed();
            saveFile.bossesDefeated = ScoreController.score.GetBossesDefeated();
            saveFile.secondsInGame = ScoreController.score.GetSecondsInGame();
        }
        return saveFile;
    }

    public void SaveGame(bool endOfGame)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = GetCombatPath() + "/saveFile" + SystemInfo.deviceUniqueIdentifier + ".sav";
        FileStream stream = new FileStream(path, FileMode.OpenOrCreate);

        formatter.Serialize(stream, GetSaveFile(endOfGame));
        stream.Close();
    }

    private SaveFile LoadGameFile()
    {
        string path = GetCombatPath() + "/saveFile" + SystemInfo.deviceUniqueIdentifier + ".sav";
        SaveFile saveFile = null;
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            saveFile = formatter.Deserialize(stream) as SaveFile;

            stream.Close();
        }
        else
        {
            Debug.Log("save file not found in: " + path);
        }

        return saveFile;
    }

    //Save in singleplayer
    private StoryModeSaveFile GetStoryModeSaveFile()
    {
        StoryModeSaveFile output = new StoryModeSaveFile();

        output.patchID = patchID;
        output.unlockedRoomIds = StoryModeController.story.GetCompletedRooms().ToArray();
        output.challengeValues = StoryModeController.story.GetChallengeValues();
        output.lastSelectedRoomId = StoryModeController.story.GetLastSelectedRoomID();
        output.lastSelectedAchievements = StoryModeController.story.GetLastSelectedAchievents();
        output.lastSelectedComplete = StoryModeController.story.GetLastSelectedComplete();
        output.worldNumber = StoryModeController.story.GetWorldNumber();
        output.cardCraftable = StoryModeController.story.GetCardCraftable().ToArray();
        output.cardUnlocks = CollectionController.collectionController.GetCompleteDeckDict();
        output.cardSelected = CollectionController.collectionController.GetSelectedDeckNames();
        output.selectedEquipments = CollectionController.collectionController.GetSelectEquipments();
        output.completeEquipments = CollectionController.collectionController.GetCompleteEquipments();
        output.itemsUnlocked = StoryModeController.story.GetItemsBought();
        output.challengeItemsBought = StoryModeController.story.GetChallengeItemsBought();
        output.colorsCompleted = StoryModeController.story.GetColorsCompleted();
        output.secretShopItemsBought = StoryModeController.story.GetSecretShopItemsBought();
        output.dailyBought = StoryModeController.story.GetDailyBought();
        output.weeklyBought = StoryModeController.story.GetWeeklyBought();

        return output;
    }

    public void SaveStoryModeGame()
    {
        Debug.Log("#### saving story mode game ####");
        BinaryFormatter formatter = new BinaryFormatter();
        string path = GetCombatPath() + "/storyModeSaveFile" + SystemInfo.deviceUniqueIdentifier + ".sav";
        if (debug)
            path = GetCombatPath() + "/storyModeDebugSaveFile" + SystemInfo.deviceUniqueIdentifier + ".sav";
        FileStream stream = new FileStream(path, FileMode.OpenOrCreate);

        formatter.Serialize(stream, GetStoryModeSaveFile());
        stream.Close();
    }

    public void LoadStoryModeGame()
    {
        Debug.Log("#### Loading story mode game ####" + GetCombatPath());
        StoryModeSaveFile file = LoadStoryModeGameFile();

        StoryModeController.story.SetCompletedRooms(file.unlockedRoomIds.ToList());
        StoryModeController.story.SetChallengeValues(file.challengeValues);
        StoryModeController.story.SetLastSelectedRoomID(file.lastSelectedRoomId);
        StoryModeController.story.SetLastSelectedAchievemnts(file.lastSelectedAchievements);
        StoryModeController.story.SetLastSelectedComplete(file.lastSelectedComplete);
        StoryModeController.story.SetWorldNumber(file.worldNumber);
        StoryModeController.story.SetCardCraftable(file.cardCraftable.ToList());
        CollectionController.collectionController.SetCompleteDeck(file.cardUnlocks);
        CollectionController.collectionController.SetSelectedDeck(file.cardSelected);
        CollectionController.collectionController.SetCompleteEquipments(file.completeEquipments);
        CollectionController.collectionController.SetSelectedEquipments(file.selectedEquipments);
        CollectionController.collectionController.ReCountUniqueCards();
        CollectionController.collectionController.RefreshDecks();
        CollectionController.collectionController.ReCountUniqueEquipments();
        CollectionController.collectionController.RefreshEquipments();
        StoryModeController.story.SetItemsBought(file.itemsUnlocked);
        StoryModeController.story.SetChallengeItemsBought(file.challengeItemsBought);
        StoryModeController.story.SetColorsCompleted(file.colorsCompleted);
        StoryModeController.story.SetSecretShopItemsBought(file.secretShopItemsBought);
        StoryModeController.story.SetDailyBought(file.dailyBought);
        StoryModeController.story.SetWeeklyBought(file.weeklyBought);
    }

    //Used to reset decks and equipments after a story mode combat sequence
    public void LoadStoryModeDecksAndEquipments()
    {
        Debug.Log("#### Reset decks and equipment ####");
        StoryModeSaveFile file = LoadStoryModeGameFile();

        CollectionController.collectionController.SetCompleteDeck(file.cardUnlocks);
        CollectionController.collectionController.SetSelectedDeck(file.cardSelected);
        CollectionController.collectionController.SetCompleteEquipments(file.completeEquipments);
        CollectionController.collectionController.SetSelectedEquipments(file.selectedEquipments);
        CollectionController.collectionController.ReCountUniqueCards();
        CollectionController.collectionController.ReCountUniqueEquipments();
    }

    private StoryModeSaveFile LoadStoryModeGameFile()
    {
        string path = GetCombatPath() + "/storyModeSaveFile" + SystemInfo.deviceUniqueIdentifier + ".sav";
        if (debug)
            path = GetCombatPath() + "/storyModeDebugSaveFile" + SystemInfo.deviceUniqueIdentifier + ".sav";

        StoryModeSaveFile saveFile = null;
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            saveFile = formatter.Deserialize(stream) as StoryModeSaveFile;

            stream.Close();
        }
        else
        {
            Debug.Log("save file not found in: " + path);
        }

        return saveFile;
    }

    public bool GetHasStoryModeSaveFile()
    {
        string path = GetCombatPath() + "/storyModeSaveFile" + SystemInfo.deviceUniqueIdentifier + ".sav";
        if (debug)
            path = GetCombatPath() + "/storyModeDebugSaveFile" + SystemInfo.deviceUniqueIdentifier + ".sav";
        return File.Exists(path);
    }

    public string GetStoryModeSaveFilePatchID()
    {
        string path = GetCombatPath() + "/storyModeSaveFile" + SystemInfo.deviceUniqueIdentifier + ".sav";
        if (debug)
            path = GetCombatPath() + "/storyModeDebugSaveFile" + SystemInfo.deviceUniqueIdentifier + ".sav";
        StoryModeSaveFile saveFile = null;
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            saveFile = formatter.Deserialize(stream) as StoryModeSaveFile;

            stream.Close();
        }
        else
        {
            Debug.Log("save file not found in: " + path);
        }

        if (saveFile == null)
            return null;
        return saveFile.patchID;
    }

    public string[] GetLoadPartyColors()
    {
        SaveFile savefile = LoadGameFile();
        string[] output = new string[3];
        output[0] = savefile.player1Color;
        output[1] = savefile.player2Color;
        output[2] = savefile.player3Color;

        return output;
    }

    public void LoadGame()
    {
        SaveFile saveFile = LoadGameFile();

        if (!saveFile.newGame)
        {
            seed = saveFile.gameSeed;
            gameID = saveFile.gameID;

            string[] playerColors = new string[3];
            playerColors[0] = saveFile.player1Color;
            playerColors[1] = saveFile.player2Color;
            playerColors[2] = saveFile.player3Color;
            PartyController.party.SetPlayerColors(playerColors);

            RoomController.roomController.selectedLevel = saveFile.level;
            RoomController.roomController.SetWorldLevel(saveFile.worldLevel);
            //roomRandomizedIndex = saveFile.roomRandomizedIndex;

            RoomController.roomController.SetViableRoom(new Vector2(saveFile.viableRoomX, saveFile.viableRoomY));

            List<Vector2> previousRoom = new List<Vector2>();
            for (int i = 0; i < saveFile.previousRoomsX.Length; i++)
                previousRoom.Add(new Vector2(saveFile.previousRoomsX[i], saveFile.previousRoomsY[i]));
            RoomController.roomController.SetAllPreviousRooms(previousRoom);
            List<Vector2> destroyedRooms = new List<Vector2>();
            for (int i = 0; i < saveFile.destroyedRoomsX.Length; i++)
                destroyedRooms.Add(new Vector2(saveFile.destroyedRoomsX[i], saveFile.destroyedRoomsY[i]));
            RoomController.roomController.SetDestroyedRooms(destroyedRooms);

            Random.InitState(saveFile.stateSeed);   //Set seed back for all other purposes after room generation

            CombatInfo combatInfo = new CombatInfo();
            combatInfo.lives = saveFile.lives;
            combatInfo.deadChars = saveFile.deadChars;
            combatInfo.vit = saveFile.playerVit;
            combatInfo.maxVit = saveFile.playerMaxVit;
            combatInfo.atk = saveFile.playerAtk;
            combatInfo.armor = saveFile.playerArmor;

            InformationController.infoController.SetCombatInfo(combatInfo);

            CollectionController.collectionController.SetCompleteDeck(saveFile.collectionCardNames);
            CollectionController.collectionController.SetSelectedDeck(saveFile.selectedCardNames);
            CollectionController.collectionController.SetNewCardsDeck(saveFile.newCardNames);
            CollectionController.collectionController.SetRecentRewardsCard(saveFile.recentRewardsCard);

            CollectionController.collectionController.ReCountUniqueCards();
            CollectionController.collectionController.SetDeck(0);
            CollectionController.collectionController.FinalizeDeck();
            CollectionController.collectionController.CheckDeckComplete();
            CollectionController.collectionController.CheckPageButtons();
            CollectionController.collectionController.RefreshDecks();

            RelicController.relic.SetRelics(saveFile.relicNames);
            RelicController.relic.SetValidChoices(saveFile.validChoices);

            ResourceController.resource.LoadGold(saveFile.gold);

            ScoreController.score.SetOverkill(saveFile.overkill);
            ScoreController.score.SetDamage(saveFile.damage);
            ScoreController.score.SetDamageArmored(saveFile.damageArmored);
            ScoreController.score.SetDamageOverhealProtected(saveFile.damageOverhealedProtected);
            ScoreController.score.SetDamageAvoided(saveFile.damageAvoided);
            ScoreController.score.SetEnemiesBroken(saveFile.enemiesBroken);
            ScoreController.score.SetGoldUsed(saveFile.goldUsed);
            ScoreController.score.SetBossesDefeated(saveFile.bossesDefeated);
            ScoreController.score.SetSecondsInGame(saveFile.secondsInGame);
        }
    }

    public bool GetHasSaveFile()
    {
        string path = GetCombatPath() + "/saveFile" + SystemInfo.deviceUniqueIdentifier + ".sav";
        SaveFile saveFile = null;
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            saveFile = formatter.Deserialize(stream) as SaveFile;

            stream.Close();

            if (!saveFile.newGame && saveFile.patchID == patchID)
                return true;
        }
        return false;
    }

    public void StartGameAndLoad()
    {
        loadGameOnLevelLoad = true;
        ScoreController.score.SetTimerPaused(false);
        SceneManager.LoadScene("OverworldScene", LoadSceneMode.Single);
    }

    private void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        if (SceneManager.GetActiveScene().name == "OverworldScene" && loadGameOnLevelLoad)
        {
            InformationLogger.infoLogger.LoadGame();
            RoomController.roomController.InitializeWorld(true);
            RoomController.roomController.LoadRooms();
            loadGameOnLevelLoad = false;
        }
        if (SceneManager.GetActiveScene().name != "MainMenuScene")
        {
            versionText.enabled = false;
            seedText.enabled = false;
        }
    }

    //Player Preferences
    private PlayerPreferences GetPlayerPreferences()
    {
        PlayerPreferences preferences = new PlayerPreferences();
        string[] colors = PartyController.party.GetPlayerColorTexts();

        preferences.lastPatchRead = lastPatchRead;
        preferences.latestDateShopOpened = GetDailySeed();
        preferences.dailyRerollsLeft = dailyRerollsLeft;
        preferences.weeklyRerollsLeft = weeklyRerollsLeft;
        preferences.tutorialCompleted = tutorialCompleted;
        preferences.passiveTutorialCompletedIds = TutorialController.tutorial.GetCompletedPassiveTutorials();

        preferences.menuIconEnabled = menuIconEnabled;

        preferences.party1 = colors[0];
        preferences.party2 = colors[1];
        preferences.party3 = colors[2];

        preferences.teamLevel = ScoreController.score.teamLevel;
        preferences.currentEXP = ScoreController.score.currentEXP;

        preferences.redLevel = PartyController.party.GetPartyLevelInfo(Card.CasterColor.Red)[0];
        preferences.redCurrentEXP = PartyController.party.GetPartyLevelInfo(Card.CasterColor.Red)[1];
        preferences.blueLevel = PartyController.party.GetPartyLevelInfo(Card.CasterColor.Blue)[0];
        preferences.blueCurrentEXP = PartyController.party.GetPartyLevelInfo(Card.CasterColor.Blue)[1];
        preferences.greenLevel = PartyController.party.GetPartyLevelInfo(Card.CasterColor.Green)[0];
        preferences.greenCurrentEXP = PartyController.party.GetPartyLevelInfo(Card.CasterColor.Green)[1];
        preferences.orangeLevel = PartyController.party.GetPartyLevelInfo(Card.CasterColor.Orange)[0];
        preferences.orangeCurrentEXP = PartyController.party.GetPartyLevelInfo(Card.CasterColor.Orange)[1];
        preferences.whiteLevel = PartyController.party.GetPartyLevelInfo(Card.CasterColor.White)[0];
        preferences.whiteCurrentEXP = PartyController.party.GetPartyLevelInfo(Card.CasterColor.White)[1];
        preferences.blackLevel = PartyController.party.GetPartyLevelInfo(Card.CasterColor.Black)[0];
        preferences.blackCurrentEXP = PartyController.party.GetPartyLevelInfo(Card.CasterColor.Black)[1];

        try
        {
            preferences.highestOverkillScore = ScoreController.score.highestScores[0];
            preferences.highestDamageScore = ScoreController.score.highestScores[1];
            preferences.highestDamageArmoredScore = ScoreController.score.highestScores[2];
            preferences.highestDamageOverhealedProtectedScore = ScoreController.score.highestScores[3];
            preferences.highestDamageAvoidedScore = ScoreController.score.highestScores[4];
            preferences.highestEnemiesBrokenScore = ScoreController.score.highestScores[5];
            preferences.highestGoldUsedScore = ScoreController.score.highestScores[6];
            preferences.highestBossesDefeatedScore = ScoreController.score.highestScores[7];
            preferences.highestSecondsInGameScore = ScoreController.score.highestScores[8];
            preferences.highestTotalScore = ScoreController.score.highestTotalScore;
        }
        catch { }

        return preferences;
    }

    public void SavePlayerPreferences()
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = GetCombatPath() + "/playerpreferences" + SystemInfo.deviceUniqueIdentifier + ".sav";
        FileStream stream = new FileStream(path, FileMode.OpenOrCreate);

        formatter.Serialize(stream, GetPlayerPreferences());
        stream.Close();
    }

    public void LoadPlayerPreferences()
    {
        PlayerPreferences preferences = GetHasPlayerPreferences();

        lastPatchRead = preferences.lastPatchRead;
        tutorialCompleted = preferences.tutorialCompleted;
        latestDayShopOpened = preferences.latestDateShopOpened;
        dailyRerollsLeft = preferences.dailyRerollsLeft;
        weeklyRerollsLeft = preferences.weeklyRerollsLeft;
        TutorialController.tutorial.SetCompletedassiveTutorials(preferences.passiveTutorialCompletedIds);

        menuIconEnabled = preferences.menuIconEnabled;

        string[] colors = new string[3];
        colors[0] = preferences.party1;
        colors[1] = preferences.party2;
        colors[2] = preferences.party3;

        PartyController.party.SetPlayerColors(colors);
        PartyController.party.SetPartyLevelInfo(Card.CasterColor.Red, preferences.redLevel, preferences.redCurrentEXP);
        PartyController.party.SetPartyLevelInfo(Card.CasterColor.Blue, preferences.blueLevel, preferences.blueCurrentEXP);
        PartyController.party.SetPartyLevelInfo(Card.CasterColor.Green, preferences.greenLevel, preferences.greenCurrentEXP);
        PartyController.party.SetPartyLevelInfo(Card.CasterColor.Orange, preferences.orangeLevel, preferences.orangeCurrentEXP);
        PartyController.party.SetPartyLevelInfo(Card.CasterColor.White, preferences.whiteLevel, preferences.whiteCurrentEXP);
        PartyController.party.SetPartyLevelInfo(Card.CasterColor.Black, preferences.blackLevel, preferences.blackCurrentEXP);

        ScoreController.score.teamLevel = preferences.teamLevel;
        ScoreController.score.currentEXP = preferences.currentEXP;

        List<int> highestScores = new List<int>();

        highestScores.Add(preferences.highestOverkillScore);
        highestScores.Add(preferences.highestDamageScore);
        highestScores.Add(preferences.highestDamageArmoredScore);
        highestScores.Add(preferences.highestDamageOverhealedProtectedScore);
        highestScores.Add(preferences.highestDamageAvoidedScore);
        highestScores.Add(preferences.highestEnemiesBrokenScore);
        highestScores.Add(preferences.highestGoldUsedScore);
        highestScores.Add(preferences.highestBossesDefeatedScore);
        highestScores.Add(preferences.highestSecondsInGameScore);

        ScoreController.score.highestScores = highestScores;
        ScoreController.score.highestTotalScore = preferences.highestTotalScore;
    }
    private PlayerPreferences GetHasPlayerPreferences()
    {
        string path = GetCombatPath() + "/playerpreferences" + SystemInfo.deviceUniqueIdentifier + ".sav";
        PlayerPreferences playerPreferences = null;
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            playerPreferences = formatter.Deserialize(stream) as PlayerPreferences;

            stream.Close();
        }
        else
        {
            Debug.Log("player preferences file not found in: " + path);
            playerPreferences = new PlayerPreferences();
            playerPreferences.lastPatchRead = "";
            playerPreferences.latestDateShopOpened = 0;
            playerPreferences.dailyRerollsLeft = 0;
            playerPreferences.weeklyRerollsLeft = 0;

            playerPreferences.tutorialCompleted = false;
            playerPreferences.passiveTutorialCompletedIds = new List<int>();

            playerPreferences.menuIconEnabled = new bool[] { true, false, false, false, false };

            playerPreferences.party1 = "Red";
            playerPreferences.party2 = "Blue";
            playerPreferences.party3 = "Green";

            playerPreferences.teamLevel = 0;
            playerPreferences.currentEXP = 0;

            playerPreferences.redLevel = 0;
            playerPreferences.redCurrentEXP = 0;
            playerPreferences.blueLevel = 0;
            playerPreferences.blueCurrentEXP = 0;
            playerPreferences.greenLevel = 0;
            playerPreferences.greenCurrentEXP = 0;
            playerPreferences.orangeLevel = 0;
            playerPreferences.orangeCurrentEXP = 0;
            playerPreferences.whiteLevel = 0;
            playerPreferences.whiteCurrentEXP = 0;
            playerPreferences.blackLevel = 0;
            playerPreferences.blackCurrentEXP = 0;

            playerPreferences.highestOverkillScore = 0;
            playerPreferences.highestDamageScore = 0;
            playerPreferences.highestDamageArmoredScore = 0;
            playerPreferences.highestDamageOverhealedProtectedScore = 0;
            playerPreferences.highestDamageAvoidedScore = 0;
            playerPreferences.highestEnemiesBrokenScore = 0;
            playerPreferences.highestGoldUsedScore = 0;
            playerPreferences.highestBossesDefeatedScore = 0;
            playerPreferences.highestSecondsInGameScore = 0;
            playerPreferences.highestTotalScore = 0;
        }

        return playerPreferences;
    }

    //Unlocks
    public void SaveUnlocks()
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = GetCombatPath() + "/unlocks" + SystemInfo.deviceUniqueIdentifier + ".sav";
        FileStream stream = new FileStream(path, FileMode.OpenOrCreate);

        formatter.Serialize(stream, UnlocksController.unlock.GetUnlocks());
        stream.Close();
    }

    public void LoadUnlocks()
    {
        UnlocksController.unlock.SetUnlocks(GetHasUnlocks());
    }
    private Unlocks GetHasUnlocks()
    {
        string path = GetCombatPath() + "/unlocks" + SystemInfo.deviceUniqueIdentifier + ".sav";
        Unlocks unlocks = null;
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            unlocks = formatter.Deserialize(stream) as Unlocks;

            stream.Close();
        }
        else
        {
            Debug.Log("unlocks file not found in: " + path);
            unlocks = UnlocksController.unlock.GetUnlocks();
        }

        return unlocks;
    }

    //Settings
    private Settings GetSettings()
    {
        Settings settings = new Settings();

        settings.backgroundMusicMuted = SettingsController.settings.GetBackGroundMusicMuted();
        settings.backgroundMusicVolume = SettingsController.settings.GetBackGroundMusicVolume();
        settings.soundEffectsMuted = SettingsController.settings.GetSoundEffectsMuted();
        settings.soundEffectsVolume = SettingsController.settings.GetSoundEffectsVolume();

        settings.gameSpeedIndex = SettingsController.settings.GetGameSpeedIndex();
        settings.screenShakeIndex = SettingsController.settings.GetScreenShakeIndex();
        settings.remainingMoveRangeIndicator = SettingsController.settings.GetRemainingMoveRangeIndicator();

        return settings;
    }

    public void SaveSettings()
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = GetCombatPath() + "/settings" + SystemInfo.deviceUniqueIdentifier + ".sav";
        FileStream stream = new FileStream(path, FileMode.OpenOrCreate);

        formatter.Serialize(stream, GetSettings());
        stream.Close();
    }

    public void LoadSettings()
    {
        Settings settings = GetHasSettings();

        SettingsController.settings.SetBackgroundMusicMuted(settings.backgroundMusicMuted);
        SettingsController.settings.SetBackgroundMusicVolume(settings.backgroundMusicVolume);
        SettingsController.settings.SetSoundEffectsMuted(settings.soundEffectsMuted, false);
        SettingsController.settings.SetSoundEffectsVolume(settings.soundEffectsVolume, false);

        SettingsController.settings.SetGameSpeedIndex(settings.gameSpeedIndex);
        SettingsController.settings.SetScreenShakeIndex(settings.screenShakeIndex);
        SettingsController.settings.SetRemainingMoveRangeIndicator(settings.remainingMoveRangeIndicator);
    }
    private Settings GetHasSettings()
    {
        string path = GetCombatPath() + "/settings" + SystemInfo.deviceUniqueIdentifier + ".sav";
        Settings settings = null;
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            settings = formatter.Deserialize(stream) as Settings;

            stream.Close();
        }
        else
        {
            Debug.Log("settings file not found in: " + path);
            settings = GetSettings();
            SaveSettings();
        }

        return settings;
    }

    public void SetLastPatchRead(string value)
    {
        lastPatchRead = value;
    }

    public string GetLastPatchRead()
    {
        return lastPatchRead;
    }

    public int GetLatestDayShopOpened()
    {
        return latestDayShopOpened;
    }

    public void SetLatestDayShopOpened(int value)
    {
        latestDayShopOpened = value;
    }

    public int GetDailyRerollsLeft()
    {
        return dailyRerollsLeft;
    }

    public int GetWeeklyRerollsLeft()
    {
        return weeklyRerollsLeft;
    }

    public void SetDailyRerollsLeft(int value)
    {
        dailyRerollsLeft = value;
    }

    public void SetWeeklyRerollsLeft(int value)
    {
        weeklyRerollsLeft = value;
    }

    public void SetTutorialCompleted(bool state)
    {
        tutorialCompleted = state;
    }

    public bool GetTutorialCompleted()
    {
        return tutorialCompleted;
    }

    public bool[] GetMenuIconsEnabled()
    {
        return menuIconEnabled;
    }

    public void SetMenuIconsEnabled(bool[] value)
    {
        menuIconEnabled = value;
    }

    public int GetSecondSeed()
    {
        return secondSeed;
    }

    //Used for accounting for daily card randomization. Takes rerolls into account
    public int GetDailySeed()
    {
        return dailySeed + InformationLogger.infoLogger.GetDailyRerollsLeft() * 999999;
    }

    //Used for accounting for weekly card randomization. Takes rerolls into account
    public int GetWeeklySeed()
    {
        return weeklySeed + InformationLogger.infoLogger.GetWeeklyRerollsLeft() * 999999;
    }

    //Used for day of week calculations
    public int GetRawDailySeed()
    {
        return dailySeed;
    }

    //Used for week of month calculations
    public int GetRawWeeklySeed()
    {
        return weeklySeed;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }
}
