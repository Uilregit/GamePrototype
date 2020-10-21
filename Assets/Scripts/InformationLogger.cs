using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Microsoft.Win32.SafeHandles;

[System.Serializable]
public class SaveFile
{
    public bool newGame;
    public int gameSeed;
    public int stateSeed;
    public string gameID;
    public string patchID;

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
    public int damageShielded;
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
}

public class InformationLogger : MonoBehaviour
{
    public static InformationLogger infoLogger;

    public string patchID;
    public int seed;
    public string gameID;
    public Text versionText;
    public Text seedText;
    public int roomRandomizedIndex = -1;

    private bool loadGameOnLevelLoad = false;
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
    }

    private void OnLevelWasLoaded(int level)
    {

    }

    public void SaveCombatInfo(string patchID, string gameID, string encounterLevel, string roomName, string turnID, string castOrder, string cardColor, string cardName, string heldFlag, string replacedFlag,
                               string unplayedFlag, string castFlag, string casterName, string targetCount, string targetName, string vitDamageDone, string shieldDamageDone, string manaGenerated, string manaUsed)
    {
        if (RoomController.roomController.debug)
            return;

        string filePath = GetCombatPath() + "/combat_data_" + SystemInfo.deviceUniqueIdentifier + ".csv";
        DebugPlus.LogOnScreen(filePath).Duration(5);
        DebugPlus.LogOnScreen(SystemInfo.deviceUniqueIdentifier);
        //Debug.Log(filePath);

        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, "deviceID,patchID,gameID,encounterLevel,roomName,turnID,castOrder,cardColor,cardName,heldFlag,replacedFlag,unplayedFlag,castFlag,casterName,targetCount,targetName,vitDamageDone,shieldDamageDone,manaGenerated,manaUsed\n");
        }

        string delimiter = ",";
        string line = SystemInfo.deviceUniqueIdentifier;

        line += delimiter + patchID;
        line += delimiter + gameID;
        line += delimiter + encounterLevel;
        line += delimiter + roomName;
        line += delimiter + turnID;
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
        line += delimiter + shieldDamageDone;
        line += delimiter + manaGenerated;
        line += delimiter + manaUsed;

        File.AppendAllText(filePath, line + "\n");
    }

    public void SaveRewardsCardInfo(string patchID, string gameID, string encounterLevel, string roomName, string cardColor, string cardName, string energyCost, string manaCost, string chosenFlag, string immediatelyUsedFlag)
    {
        if (RoomController.roomController.debug)
            return;

        string filePath = GetCombatPath() + "/rewards_data_" + SystemInfo.deviceUniqueIdentifier + ".csv";
        DebugPlus.LogOnScreen(filePath).Duration(5);
        DebugPlus.LogOnScreen(SystemInfo.deviceUniqueIdentifier);
        //Debug.Log(filePath);

        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, "deviceID,patchID,gameID,encounterLevel,roomName,cardColor,cardName,chosenFlag,immediatelyUsedFlag\n");
        }

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
        if (RoomController.roomController.debug)
            return;

        string filePath = GetCombatPath() + "/shop_data_" + SystemInfo.deviceUniqueIdentifier + ".csv";
        DebugPlus.LogOnScreen(filePath).Duration(5);
        DebugPlus.LogOnScreen(SystemInfo.deviceUniqueIdentifier);
        //Debug.Log(filePath);

        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, "deviceID,patchID,gameID,encounterLevel,roomName,cardColor,cardName,chosenFlag,goldUsed\n");
        }

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
        if (RoomController.roomController.debug)
            return;

        string filePath = GetCombatPath() + "/gold_data_" + SystemInfo.deviceUniqueIdentifier + ".csv";
        DebugPlus.LogOnScreen(filePath).Duration(5);
        DebugPlus.LogOnScreen(SystemInfo.deviceUniqueIdentifier);
        //Debug.Log(filePath);

        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, "deviceID,patchID,gameID,encounterLevel,roomName,passiveGold,overkillGold,totalGoldAtRoomEnd\n");
        }

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

    public void SaveDeckInfo(string patchID, string gameID, string encounterLevel, string cardColor, string cardName, string energyCost, string manaCost, string chosenFlag, string removedFlag, string finalDeckListFlag)
    {
        if (RoomController.roomController.debug)
            return;

        string filePath = GetCombatPath() + "/deck_data_" + SystemInfo.deviceUniqueIdentifier + ".csv";
        DebugPlus.LogOnScreen(filePath).Duration(5);
        DebugPlus.LogOnScreen(SystemInfo.deviceUniqueIdentifier);
        //Debug.Log(filePath);

        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, "deviceID,patchID,gameID,encounterLevel,cardColor,cardName,energyCost,manaCost,chosenFlag,removedFlag,finalDeckListFlag\n");
        }

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
            saveFile.damageShielded = ScoreController.score.GetDamageShielded();
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
            //roomRandomizedIndex = saveFile.roomRandomizedIndex;

            List<Vector2> previousRoom = new List<Vector2>();
            for (int i = 0; i < saveFile.previousRoomsX.Length; i++)
                previousRoom.Add(new Vector2(saveFile.previousRoomsX[i], saveFile.previousRoomsY[i]));
            RoomController.roomController.SetAllPreviousRooms(previousRoom);
            List<Vector2> destroyedRooms = new List<Vector2>();
            for (int i = 0; i < saveFile.destroyedRoomsX.Length; i++)
                destroyedRooms.Add(new Vector2(saveFile.destroyedRoomsX[i], saveFile.destroyedRoomsY[i]));
            RoomController.roomController.SetDestroyedRooms(destroyedRooms);
            RoomController.roomController.LoadRooms();

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
            ScoreController.score.SetDamageShielded(saveFile.damageShielded);
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
        SceneManager.LoadScene("OverworldScene", LoadSceneMode.Single);
    }

    private void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        if (SceneManager.GetActiveScene().name == "OverworldScene" && loadGameOnLevelLoad)
        {
            InformationLogger.infoLogger.LoadGame();
            loadGameOnLevelLoad = false;
        }
        if (SceneManager.GetActiveScene().name == "MainMenuScene")
        {
            try                 //For debugging purposes, prevents crashes when running from overworldScene
            {
                versionText.text = "Version: " + patchID;
                seedText.text = "Seed: " + seed;
                versionText.enabled = true;
                seedText.enabled = true;
            }
            catch { }
        }
        else
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
}
