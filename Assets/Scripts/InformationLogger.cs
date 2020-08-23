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

    public int level;
    //public int roomRandomizedIndex;
    public float[] previousRoomsX;
    public float[] previousRoomsY;
    public float[] destroyedRoomsX;
    public float[] destroyedRoomsY;

    public int redVit;
    public int blueVit;
    public int greenVit;
    public int redMaxVit;
    public int blueMaxVit;
    public int greenMaxVit;
    public int redAtk;
    public int blueAtk;
    public int greenAtk;
    public int redArmor;
    public int blueArmor;
    public int greenArmor;

    public string[][] collectionCardNames;
    public string[][] selectedCardNames;
    public string[][] newCardNames;
    public string recentRewardsCard;

    public string[] relicNames;
    public int[] validChoices;
    public int gold;
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

        SceneManager.sceneLoaded += OnLevelFinishedLoading;

        versionText.text = "Version: " + patchID;
        if (seed == 0)
            seed = Random.Range(1, 1000000000);
        Random.InitState(seed);
        seedText.text = "Seed: " + seed;
        gameID = System.DateTime.Now.ToString();
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
            saveFile.redVit = combatInfo.redVit;
            saveFile.blueVit = combatInfo.blueVit;
            saveFile.greenVit = combatInfo.greenVit;
            saveFile.redMaxVit = combatInfo.redMaxVit;
            saveFile.blueMaxVit = combatInfo.blueMaxVit;
            saveFile.greenMaxVit = combatInfo.greenMaxVit;
            saveFile.redAtk = combatInfo.redAtk;
            saveFile.blueAtk = combatInfo.blueAtk;
            saveFile.greenAtk = combatInfo.greenAtk;
            saveFile.redArmor = combatInfo.redArmor;
            saveFile.blueArmor = combatInfo.blueArmor;
            saveFile.greenArmor = combatInfo.greenArmor;

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

    public void LoadGame()
    {
        SaveFile saveFile = LoadGameFile();

        if (!saveFile.newGame)
        {
            seed = saveFile.gameSeed;
            gameID = saveFile.gameID;

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
            combatInfo.redVit = saveFile.redVit;
            combatInfo.blueVit = saveFile.blueVit;
            combatInfo.greenVit = saveFile.greenVit;
            combatInfo.redMaxVit = saveFile.redMaxVit;
            combatInfo.blueMaxVit = saveFile.blueMaxVit;
            combatInfo.greenMaxVit = saveFile.greenMaxVit;
            combatInfo.redAtk = saveFile.redAtk;
            combatInfo.blueAtk = saveFile.blueAtk;
            combatInfo.greenAtk = saveFile.greenAtk;
            combatInfo.redArmor = saveFile.redArmor;
            combatInfo.blueArmor = saveFile.blueArmor;
            combatInfo.greenArmor = saveFile.greenArmor;
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
    }
}
