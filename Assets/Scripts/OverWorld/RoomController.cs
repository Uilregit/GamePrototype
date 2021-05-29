using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEditor;

public class RoomController : MonoBehaviour
{
    public static RoomController roomController;

    //public bool isStoryMode = false;

    public int selectedLevel = -1;
    public int worldLevel = 0;
    public string roomName = "missing";

    public Color previousColor;
    public Color viableColor;
    public Color unviableColor;
    public Color shopColor;
    public Color shrineColor;

    public EditDeckButtonController deckButton;

    [SerializeField]
    private GameObject smallRoomPrefab;
    [SerializeField]
    private BossRoom bossRoom;
    [SerializeField]
    private List<WorldSetup> worldSetups;

    public enum roomType { combat, shop, shrine };

    private List<SmallRoom> smallRooms;
    private List<int> roomSeeds;
    [SerializeField]
    private RoomSetup debugRoom;

    //private RoomSetup currentRoomSetup;
    private int previousRoomIndex = -99999;

    private Dictionary<int, int> numRoomsPerLevel;

    private bool initiated = false;
    private RoomSetup currentRoomSetup;
    private SmallRoom currentSmallRoom;
    private Vector2 viableRoom = new Vector2(-999, -999);
    private List<Vector2> previousRoom;
    private List<Vector2> destroyedRooms;

    private Canvas canvas;

    // Start is called before the first frame update
    void Awake()
    {
        if (RoomController.roomController == null)
            RoomController.roomController = this;
        else
        {
            Destroy(this.gameObject);
            return;
        }

        DontDestroyOnLoad(this.gameObject);
        SceneManager.sceneLoaded += OnLevelFinishedLoading;

        if (!initiated)
        {
            smallRooms = new List<SmallRoom>();
            roomSeeds = new List<int>();
            InitializeWorld();
            if (!InformationLogger.infoLogger.isStoryMode)
                LoadRooms();
            initiated = true;
        }

    }

    public void InitializeWorld(bool load = false)
    {
        Debug.Log("room instantialized");
        Random.InitState(InformationLogger.infoLogger.seed);

        if (InformationLogger.infoLogger.isStoryMode)
        {
            selectedLevel = -1;
            smallRooms = new List<SmallRoom>();
            previousRoom = new List<Vector2>();
            destroyedRooms = new List<Vector2>();
            canvas = GetComponent<Canvas>();
            roomSeeds = new List<int>();

            for (int i = StoryModeController.story.GetCurrentRoomSetup().setups.Count; i > 1; i--)
            {
                GameObject obj = Instantiate(smallRoomPrefab);
                obj.GetComponent<SmallRoom>().SetLocation(new Vector2(0, StoryModeController.story.GetCurrentRoomSetup().setups.Count - i));
                if (InformationLogger.infoLogger.debug)
                    obj.GetComponent<SmallRoom>().SetSetup(debugRoom);
                else
                    obj.GetComponent<SmallRoom>().SetSetup(StoryModeController.story.GetCurrentRoomSetup().setups[StoryModeController.story.GetCurrentRoomSetup().setups.Count - i]);
                smallRooms.Add(obj.GetComponent<SmallRoom>());
                obj.transform.SetParent(transform);
                obj.transform.position = transform.position + new Vector3(0, (bossRoom.transform.position.y - i) * 0.8f + 0.7f, 0);

                obj.GetComponent<SmallRoom>().SetSeed(Random.Range(1, 1000000000));
            }
            bossRoom.GetComponent<SmallRoom>().SetLocation(new Vector2(0, StoryModeController.story.GetCurrentRoomSetup().setups.Count - 1));
            if (InformationLogger.infoLogger.debug && !InformationLogger.infoLogger.debugBossRoomEnabled)
                bossRoom.GetComponent<SmallRoom>().SetSetup(debugRoom);
            else
                bossRoom.GetComponent<SmallRoom>().SetSetup(StoryModeController.story.GetCurrentRoomSetup().setups[StoryModeController.story.GetCurrentRoomSetup().setups.Count - 1]);

            Refresh();
        }
        else
        {
            ResourceController.resource.GetComponent<Canvas>().enabled = true;
            ResourceController.resource.GetComponent<CanvasScaler>().enabled = false;
            ResourceController.resource.GetComponent<CanvasScaler>().enabled = true;

            canvas = GetComponent<Canvas>();

            destroyedRooms = new List<Vector2>();

            if (!load)
            {
                previousRoom = new List<Vector2>();
                selectedLevel = -1;
            }

            foreach (SmallRoom room in smallRooms)
                Destroy(room.gameObject);

            smallRooms = new List<SmallRoom>();
            roomSeeds = new List<int>();

            foreach (Vector2 loc in worldSetups[worldLevel].roomLocations)
            {
                GameObject obj = Instantiate(smallRoomPrefab);
                obj.GetComponent<SmallRoom>().SetLocation(loc);
                smallRooms.Add(obj.GetComponent<SmallRoom>());
                obj.transform.SetParent(transform);
                obj.transform.position = transform.position + new Vector3(loc.x * 0.8f, loc.y * 0.8f - 3.37f, 0);

                obj.GetComponent<SmallRoom>().SetSeed(Random.Range(1, 1000000000));
            }

            numRoomsPerLevel = new Dictionary<int, int>();
            foreach (SmallRoom room in smallRooms)
                if (numRoomsPerLevel.ContainsKey((int)room.GetLocation().y))
                    numRoomsPerLevel[(int)room.GetLocation().y] += 1;
                else
                    numRoomsPerLevel[(int)room.GetLocation().y] = 1;

            RandomizeRooms();
            Refresh();
        }
    }

    public void LoadNewWorld(int level)
    {
        worldLevel = level;
        selectedLevel = -1;
        foreach (SmallRoom room in smallRooms)
            Destroy(room.gameObject);
        InitializeWorld();
    }

    public void LoadRooms()
    {
        foreach (SmallRoom room in smallRooms)
        {
            room.Show();
            room.SetType(roomType.combat);
            room.SetColor(Color.white);
        }
        RandomizeRooms();
        Refresh();
        initiated = true;
    }

    //Set clickable rooms and set all cooresponding colors
    public void Refresh()
    {
        if (previousRoom.Count == 0 && selectedLevel == -1)
        {
            foreach (SmallRoom firstRoom in smallRooms)
                if (firstRoom.GetLocation().y == 0)
                    firstRoom.SetSelectable(true);
        }
        else
        {
            Vector2 lastRoom = previousRoom[previousRoom.Count - 1];
            foreach (SmallRoom room in smallRooms)
            {
                if (room.GetLocation().y == 0)
                    room.SetSelectable(false);
                if (destroyedRooms.Contains(room.GetLocation()))
                {
                    room.Hide();
                    room.SetDestroyed(true);
                }
                else if (previousRoom.Contains(room.GetLocation())) //Sets trail of past rooms
                    room.SetColor(previousColor);
                else if (room.GetLocation().y - lastRoom.y == 1 &&   //If within selection criteria, make rooms clickable
                    Mathf.Abs(room.GetLocation().x - lastRoom.x) <= 1)
                {
                    if (viableRoom == room.GetLocation() || viableRoom == new Vector2(-999, -999))
                        room.SetSelectable(true);
                    else
                        room.SetColor(unviableColor);
                }
                else                                                                    //For the rest, set yet to be reached levels one color, set past levels another
                {
                    room.SetSelectable(false);
                    if (room.GetLocation().y <= lastRoom.y + 1)
                        room.SetColor(unviableColor);
                    else
                        room.SetColor(viableColor);
                }
            }
            if ((!InformationLogger.infoLogger.isStoryMode && selectedLevel == GetNumberofWorldLayers()) || (InformationLogger.infoLogger.isStoryMode && selectedLevel == StoryModeController.story.GetCurrentRoomSetup().setups.Count - 2))             //Enable the boss room when viable
                bossRoom.SetSelectable(true);
            else
                bossRoom.SetSelectable(false);
        }

        foreach (SmallRoom room in smallRooms)
        {
            if (room.GetRoomType() == roomType.shop)
                room.SetColor(shopColor);
            else if (room.GetRoomType() == roomType.shrine)
                room.SetColor(shrineColor);
        }
    }

    public void RandomizeRooms()
    {
        Random.InitState(InformationLogger.infoLogger.seed); //Always randomize with the starting seed

        List<SmallRoom> removedRooms = new List<SmallRoom>();
        List<float> dissapearedLevels = new List<float>();
        List<float> shopLevels = new List<float>();
        List<float> shrineLevels = new List<float>();
        int numOfShops = 0;
        int numOfShrines = 0;
        int backupShopLocation = Random.Range(0, numRoomsPerLevel[worldSetups[worldLevel].maxShopLocation]);
        int backupShrineLocation = Random.Range(0, numRoomsPerLevel[worldSetups[worldLevel].maxShrineLocation]);
        foreach (SmallRoom room in smallRooms)
        {
            if (numOfShrines < worldSetups[worldLevel].numOfShrinePerWorld && room.GetLocation().y >= worldSetups[worldLevel].minShrineLocation && !shrineLevels.Contains(room.GetLocation().y))
            {
                if (room.GetLocation().y == worldSetups[worldLevel].maxShrineLocation && room.GetLocation().x + 0.5 * (numRoomsPerLevel[worldSetups[worldLevel].maxShrineLocation] - 1) == backupShrineLocation)
                {
                    room.SetType(roomType.shrine);
                    numOfShrines += 1;
                    shrineLevels.Add(room.GetLocation().y);
                    continue;
                }
                else if (Random.Range(0.0f, 1.0f) <= worldSetups[worldLevel].shrinePercentage)
                {
                    room.SetType(roomType.shrine);
                    numOfShrines += 1;
                    shrineLevels.Add(room.GetLocation().y);
                    continue;
                }
            }
            if (numOfShops < worldSetups[worldLevel].numOfShopPerWorld && room.GetLocation().y >= worldSetups[worldLevel].minShopLocation && !shopLevels.Contains(room.GetLocation().y))
            {
                if (room.GetLocation().y == worldSetups[worldLevel].maxShopLocation && room.GetLocation().x + 0.5 * (numRoomsPerLevel[worldSetups[worldLevel].maxShopLocation] - 1) == backupShopLocation)
                {
                    room.SetType(roomType.shop);
                    numOfShops += 1;
                    shopLevels.Add(room.GetLocation().y);
                    continue;
                }
                else if (Random.Range(0.0f, 1.0f) <= worldSetups[worldLevel].shopPercentage)
                {
                    room.SetType(roomType.shop);
                    numOfShops += 1;
                    shopLevels.Add(room.GetLocation().y);
                    continue;
                }
            }
            if (Random.Range(0.0f, 1.0f) <= worldSetups[worldLevel].dissapearPercentage && room.GetLocation().y != 0 && !dissapearedLevels.Contains(room.GetLocation().y))//Never remove first levels and only 1 removed room per level
            {
                removedRooms.Add(room);
                dissapearedLevels.Add(room.GetLocation().y);
            }
        }
        foreach (SmallRoom room in smallRooms)
            if (room.GetRoomType() == roomType.combat)
                room.SetSetup(GetRoomSetup((int)room.GetLocation().y));

        foreach (SmallRoom room in removedRooms)                                        //Set the boss room's setup
        {
            room.Hide();
            room.SetDestroyed(true);
            destroyedRooms.Add(room.GetLocation());
            //smallRooms.Remove(room);
            //Destroy(room.gameObject);
        }
        if (InformationLogger.infoLogger.debug && !InformationLogger.infoLogger.debugBossRoomEnabled)
            bossRoom.SetSetup(debugRoom);
        else
            bossRoom.SetSetup(worldSetups[worldLevel].bossRooms[Random.Range(0, worldSetups[worldLevel].bossRooms.Count)]);
    }
    /*
    public void SetCurrentRoomSetup(RoomSetup newSetup)
    {
        if (currentRoomSetup.roomName == newSetup.roomName)
            currentRoomSetup = 
        currentRoomSetup = newSetup;
        roomName = newSetup.roomName;
    }
    */
    public void EnterRoom(string sceneName)
    {
        //GameController.gameController.LoadScene(sceneName, false, 0);
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    public RoomSetup GetRoomSetup(int level)
    {
        if (InformationLogger.infoLogger.debug)
            return debugRoom;

        RoomSetup setup;
        int maxRoomLevel = GetNumberofWorldLayers();
        int index = -1;

        if (level < maxRoomLevel / 3)       //first 3rd of rooms are initial rooms
        {
            index = Random.Range(0, worldSetups[worldLevel].innitialRooms.Count);
            while (index == previousRoomIndex)
                index = Random.Range(0, worldSetups[worldLevel].innitialRooms.Count);
            setup = worldSetups[worldLevel].innitialRooms[index];
        }
        else if (level < maxRoomLevel - 1)      //All but last 2 rooms are mid rooms
        {
            if (level == maxRoomLevel / 3)
                previousRoomIndex = -99999;
            index = Random.Range(0, worldSetups[worldLevel].midRooms.Count);
            while (index == previousRoomIndex)
                index = Random.Range(0, worldSetups[worldLevel].midRooms.Count);
            setup = worldSetups[worldLevel].midRooms[index];
        }
        else if (level < maxRoomLevel + 1)      //Rest of the rooms are end rooms
        {
            if (level == maxRoomLevel - 1)
                previousRoomIndex = -99999;
            index = Random.Range(0, worldSetups[worldLevel].lateRooms.Count);
            while (index == previousRoomIndex)
                index = Random.Range(0, worldSetups[worldLevel].lateRooms.Count);
            setup = worldSetups[worldLevel].lateRooms[index];
        }
        else                                    //Last layer is the boss room
        {
            index = Random.Range(0, worldSetups[worldLevel].bossRooms.Count);
            setup = worldSetups[worldLevel].bossRooms[index];
            return setup;
        }

        previousRoomIndex = index;

        return setup;
    }

    public void SetPreviousRoom(SmallRoom value)
    {
        previousRoom.Add(value.GetLocation());
    }

    public List<Vector2> GetPreviousRoom()
    {
        return previousRoom;
    }

    public void Hide()
    {
        canvas.enabled = false;
        foreach (SmallRoom room in smallRooms)
            room.Hide();
        bossRoom.Hide();
        deckButton.Hide();
    }

    public void Show()
    {
        canvas.enabled = true;
        foreach (SmallRoom room in smallRooms)
            if (!destroyedRooms.Contains(room.GetLocation()))
                room.Show();
        bossRoom.Show();
        deckButton.Show();
    }

    public void RestartGame()
    {
        selectedLevel = -1;
        roomName = "missing";
        initiated = false;

        previousRoom = new List<Vector2>();

        selectedLevel = -1;

        if (!initiated)
        {
            RandomizeRooms();
            Refresh();
            initiated = true;
        }
    }

    public void DisableColliders()
    {
        foreach (SmallRoom room in smallRooms)
            room.GetComponent<Collider2D>().enabled = false;
        bossRoom.GetComponent<Collider2D>().enabled = false;
        deckButton.GetComponent<Collider2D>().enabled = false;
    }

    public void SetAllPreviousRooms(List<Vector2> value)
    {
        previousRoom = value;
    }

    public List<Vector2> GetDestroyRooms()
    {
        return destroyedRooms;
    }

    public void SetDestroyedRooms(List<Vector2> value)
    {
        destroyedRooms = value;
    }

    public void SetCurrentRoomSetup(RoomSetup value)
    {
        currentRoomSetup = value;
    }

    public void SetCurrentSmallRoom(SmallRoom value)
    {
        currentSmallRoom = value;
    }

    public SmallRoom GetCurrentSmallRoom()
    {
        return currentSmallRoom;
    }

    public RoomSetup GetCurrentRoomSetup()
    {
        return currentRoomSetup;
    }

    public Sprite GetCombatBackground()
    {
        return worldSetups[worldLevel].combatBackground;
    }

    public int GetWorldLevel()
    {
        return worldLevel;
    }

    public WorldSetup GetCurrentWorldSetup()
    {
        return worldSetups[worldLevel];
    }

    public void SetWorldLevel(int newLevel)
    {
        worldLevel = newLevel;
        Camera.main.backgroundColor = worldSetups[worldLevel].cameraBackground;
    }

    public void SetViableRoom(Vector2 loc)
    {
        viableRoom = loc;
    }

    public Vector2 GetViableRoom()
    {
        return viableRoom;
    }

    private void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        if (SceneManager.GetActiveScene().name == "OverworldScene")
        {
            transform.position = new Vector3(0, 0, 0);
            roomName = "Overworld";
            InformationLogger.infoLogger.SaveTimeInfo(InformationLogger.infoLogger.patchID,
                    InformationLogger.infoLogger.gameID,
                    RoomController.roomController.worldLevel.ToString(),
                    RoomController.roomController.selectedLevel.ToString(),
                    "Overworld",
                    "Overworld");
        }
        else
            transform.position = new Vector3(-10, 0, 0);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }

    public GameObject GetObjectPrefab(GameObject obj)
    {
        foreach (GameObject o in currentRoomSetup.enemies)
        {
            if (obj.gameObject.name.Contains(o.gameObject.name))
                return o;
        }

        return null;
    }

    public int GetNumberOfWorlds()
    {
        return worldSetups.Count;
    }

    public int GetNumberofWorldLayers()
    {
        int maxRoomLevel = 0;
        foreach (Vector2 loc in worldSetups[worldLevel].roomLocations)
            if (loc.y > maxRoomLevel)
                maxRoomLevel = (int)loc.y;

        return maxRoomLevel;
    }
}
