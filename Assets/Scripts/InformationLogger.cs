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
    public float[] previousRoomsX;
    public float[] previousRoomsY;
    public float[] destroyedRoomsX;
    public float[] destroyedRoomsY;

    public int lives;
    public bool p1Dead;
    public bool p2Dead;
    public bool p3Dead;
    public int p1Vit;
    public int p2Vit;
    public int p3Vit;
    public int p1MaxVit;
    public int p2MaxVit;
    public int p3MaxVit;
    public int p1Atk;
    public int p2Atk;
    public int p3Atk;
    public int p1Armor;
    public int p2Armor;
    public int p3Armor;

    public string[][] collectionCardNames;
    public string[][] selectedCardNames;
    public string[][] newCardNames;
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
public class PlayerPreferences
{
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
public class Unlocks
{
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
    public string patchID;
    public int seed;
    public string gameID;
    public Text versionText;
    public Text seedText;
    public int roomRandomizedIndex = -1;

    public bool loadGameOnLevelLoad = false;
    private int roomControllerRoomLevel;
    private List<Vector2> roomControllerPreviousRooms;

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

        LoadPlayerPreferences();                //Load player preferences on startup

        SceneManager.sceneLoaded += OnLevelFinishedLoading;

        if (seed == 0)
            seed = Random.Range(1, 1000000000);
        Random.InitState(seed);

        gameID = System.DateTime.Now.ToString();

        try                 //For debugging purposes, prevents crashes when running from overworldScene
        {
            versionText.text = "Version: " + patchID;
            seedText.text = "Seed: " + seed;
            versionText.enabled = true;
            seedText.enabled = true;
        }
        catch { }
    }

    public void SaveCombatInfo(string patchID, string gameID, string encounterLevel, string roomName, string turnID, string castOrder, string cardColor, string cardName, string heldFlag, string replacedFlag,
                               string unplayedFlag, string castFlag, string casterName, string targetCount, string targetName, string vitDamageDone, string armorDamageDone, string manaGenerated, string manaUsed, string buffTriggerCount)
    {
        if (InformationLogger.infoLogger.debug)
            return;

        string filePath = GetCombatPath() + "/combat_data_" + SystemInfo.deviceUniqueIdentifier + ".csv";
        //DebugPlus.LogOnScreen(filePath).Duration(5);
        //DebugPlus.LogOnScreen(SystemInfo.deviceUniqueIdentifier);
        //Debug.Log(filePath);

        string header = "deviceID,patchID,gameID,encounterLevel,roomName,turnID,timestamp,castOrder,cardColor,cardName,heldFlag,replacedFlag,unplayedFlag,castFlag,casterName,targetCount,targetName,vitDamageDone,armorDamageDone,manaGenerated,manaUsed,buffTriggerCount";

        if (!File.Exists(filePath))
            File.WriteAllText(filePath, header + "\n");

        string delimiter = ",";
        string line = SystemInfo.deviceUniqueIdentifier;

        line += delimiter + patchID;
        line += delimiter + gameID;
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

        File.AppendAllText(filePath, line + "\n");
    }

    public void SaveRewardsCardInfo(string patchID, string gameID, string encounterLevel, string roomName, string cardColor, string cardName, string energyCost, string manaCost, string chosenFlag, string immediatelyUsedFlag)
    {
        if (InformationLogger.infoLogger.debug)
            return;

        string filePath = GetCombatPath() + "/rewards_data_" + SystemInfo.deviceUniqueIdentifier + ".csv";
        //DebugPlus.LogOnScreen(filePath).Duration(5);
        //DebugPlus.LogOnScreen(SystemInfo.deviceUniqueIdentifier);
        //Debug.Log(filePath);

        string header = "deviceID,patchID,gameID,encounterLevel,roomName,cardColor,cardName,chosenFlag,immediatelyUsedFlag";

        if (!File.Exists(filePath))
            File.WriteAllText(filePath, header + "\n");

        string delimiter = ",";
        string line = SystemInfo.deviceUniqueIdentifier;

        line += delimiter + patchID;
        line += delimiter + gameID;
        line += delimiter + encounterLevel;
        line += delimiter + roomName;
        line += delimiter + cardColor;
        line += delimiter + cardName;
        line += delimiter + chosenFlag;
        line += delimiter + immediatelyUsedFlag;

        File.AppendAllText(filePath, line + "\n");
    }

    public void SaveShopCardInfo(string patchID, string gameID, string encounterLevel, string roomName, string cardColor, string cardName, string energyCost, string manaCost, string chosenFlag, string goldUsed)
    {
        if (InformationLogger.infoLogger.debug)
            return;

        string filePath = GetCombatPath() + "/shop_data_" + SystemInfo.deviceUniqueIdentifier + ".csv";
        //DebugPlus.LogOnScreen(filePath).Duration(5);
        //DebugPlus.LogOnScreen(SystemInfo.deviceUniqueIdentifier);
        //Debug.Log(filePath);

        string header = "deviceID,patchID,gameID,encounterLevel,roomName,cardColor,cardName,chosenFlag,goldUsed";

        if (!File.Exists(filePath))
            File.WriteAllText(filePath, header + "\n");

        string delimiter = ",";
        string line = SystemInfo.deviceUniqueIdentifier;

        line += delimiter + patchID;
        line += delimiter + gameID;
        line += delimiter + encounterLevel;
        line += delimiter + roomName;
        line += delimiter + cardColor;
        line += delimiter + cardName;
        line += delimiter + chosenFlag;
        line += delimiter + goldUsed;

        File.AppendAllText(filePath, line + "\n");
    }

    public void SaveGoldInfo(string patchID, string gameID, string encounterLevel, string roomName, string passiveGold, string overkillGold, string totalGoldAtRoomEnd)
    {
        if (InformationLogger.infoLogger.debug)
            return;

        string filePath = GetCombatPath() + "/gold_data_" + SystemInfo.deviceUniqueIdentifier + ".csv";
        //DebugPlus.LogOnScreen(filePath).Duration(5);
        //DebugPlus.LogOnScreen(SystemInfo.deviceUniqueIdentifier);
        //Debug.Log(filePath);

        string header = "deviceID,patchID,gameID,encounterLevel,roomName,passiveGold,overkillGold,totalGoldAtRoomEnd";

        if (!File.Exists(filePath))
            File.WriteAllText(filePath, header + "\n");

        string delimiter = ",";
        string line = SystemInfo.deviceUniqueIdentifier;

        line += delimiter + patchID;
        line += delimiter + gameID;
        line += delimiter + encounterLevel;
        line += delimiter + roomName;
        line += delimiter + passiveGold;
        line += delimiter + overkillGold;
        line += delimiter + totalGoldAtRoomEnd;

        File.AppendAllText(filePath, line + "\n");
    }

    public void SaveGameScoreInfo(string patchID, string gameID, string encounterLevel, string roomName, string gameWon, string gameLost, string totalScore, string overkill, string damage,
                                    string damageArmored, string damageOverhealedProtected, string damageAvoided, string enemiesBroken, string goldUSed, string bossesDefeated, string secondsInGame)
    {
        if (InformationLogger.infoLogger.debug)
            return;

        string filePath = GetCombatPath() + "/gameScore_data_" + SystemInfo.deviceUniqueIdentifier + ".csv";
        //DebugPlus.LogOnScreen(filePath).Duration(5);
        //DebugPlus.LogOnScreen(SystemInfo.deviceUniqueIdentifier);
        //Debug.Log(filePath);

        string header = "deviceID,patchID,gameID,encounterLevel,roomName,gameWon,gameLost,totalScore,overkill,damage,damageArmored,damageOverhealedProtected,damageAvoided,enemiesBroken,goldUSed,bossesDefeated,secondsInGame";

        if (!File.Exists(filePath))
            File.WriteAllText(filePath, header + "\n");

        string delimiter = ",";
        string line = SystemInfo.deviceUniqueIdentifier;

        line += delimiter + patchID;
        line += delimiter + gameID;
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

    public void SaveDeckInfo(string patchID, string gameID, string encounterLevel, string cardColor, string cardName, string energyCost, string manaCost, string chosenFlag, string removedFlag, string finalDeckListFlag)
    {
        if (InformationLogger.infoLogger.debug)
            return;

        string filePath = GetCombatPath() + "/deck_data_" + SystemInfo.deviceUniqueIdentifier + ".csv";
        //DebugPlus.LogOnScreen(filePath).Duration(5);
        //DebugPlus.LogOnScreen(SystemInfo.deviceUniqueIdentifier);
        //Debug.Log(filePath);

        string header = "deviceID,patchID,gameID,encounterLevel,cardColor,cardName,energyCost,manaCost,chosenFlag,removedFlag,finalDeckListFlag";

        if (!File.Exists(filePath))
            File.WriteAllText(filePath, header + "\n");

        string delimiter = ",";
        string line = SystemInfo.deviceUniqueIdentifier;

        line += delimiter + patchID;
        line += delimiter + gameID;
        line += delimiter + encounterLevel;
        line += delimiter + cardColor;
        line += delimiter + cardName;
        line += delimiter + energyCost;
        line += delimiter + manaCost;
        line += delimiter + chosenFlag;
        line += delimiter + removedFlag;
        line += delimiter + finalDeckListFlag;

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
            saveFile.p1Dead = combatInfo.deadChars[0];
            saveFile.p2Dead = combatInfo.deadChars[1];
            saveFile.p3Dead = combatInfo.deadChars[2];
            saveFile.p1Vit = combatInfo.vit[0];
            saveFile.p2Vit = combatInfo.vit[1];
            saveFile.p3Vit = combatInfo.vit[2];
            saveFile.p1MaxVit = combatInfo.maxVit[0];
            saveFile.p2MaxVit = combatInfo.maxVit[1];
            saveFile.p3MaxVit = combatInfo.maxVit[2];
            saveFile.p1Atk = combatInfo.atk[0];
            saveFile.p2Atk = combatInfo.atk[1];
            saveFile.p3Atk = combatInfo.atk[2];
            saveFile.p1Armor = combatInfo.armor[0];
            saveFile.p2Armor = combatInfo.armor[1];
            saveFile.p3Armor = combatInfo.armor[2];

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
            bool[] deadChars = new bool[3];
            deadChars[0] = saveFile.p1Dead;
            deadChars[1] = saveFile.p2Dead;
            deadChars[2] = saveFile.p3Dead;
            combatInfo.deadChars = deadChars;
            combatInfo.vit[0] = saveFile.p1Vit;
            combatInfo.vit[1] = saveFile.p2Vit;
            combatInfo.vit[2] = saveFile.p3Vit;
            combatInfo.maxVit[0] = saveFile.p1MaxVit;
            combatInfo.maxVit[1] = saveFile.p2MaxVit;
            combatInfo.maxVit[2] = saveFile.p3MaxVit;
            combatInfo.atk[0] = saveFile.p1Atk;
            combatInfo.atk[1] = saveFile.p2Atk;
            combatInfo.atk[2] = saveFile.p3Atk;
            combatInfo.armor[0] = saveFile.p1Armor;
            combatInfo.armor[1] = saveFile.p2Armor;
            combatInfo.armor[2] = saveFile.p3Armor;
            InformationController.infoController.SetCombatInfo(combatInfo);

            CollectionController.collectionController.SetCompleteDeck(saveFile.collectionCardNames);
            CollectionController.collectionController.SetSelectedDeck(saveFile.selectedCardNames);
            CollectionController.collectionController.SetNewCardsDeck(saveFile.newCardNames);
            CollectionController.collectionController.SetRecentRewardsCard(saveFile.recentRewardsCard);

            CollectionController.collectionController.ReCountUniqueCards();
            CollectionController.collectionController.SetDeck(0);
            CollectionController.collectionController.ResolveSelectedList();
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
        ScoreController.score.timerPaused = false;
        SceneManager.LoadScene("OverworldScene", LoadSceneMode.Single);
    }

    private void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        if (SceneManager.GetActiveScene().name == "OverworldScene" && loadGameOnLevelLoad)
        {
            InformationLogger.infoLogger.LoadGame();
            RoomController.roomController.InitializeWorld();
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
            playerPreferences = GetPlayerPreferences();
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

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }
}
