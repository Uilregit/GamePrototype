using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomController : MonoBehaviour
{
    public static RoomController roomController;

    public int selectedLevel = -1;
    public string roomName = "missing";

    public Color previousColor;
    public Color viableColor;
    public Color unviableColor;
    public Color shopColor;
    public Color shrineColor;

    public float dissapearPercentage;
    public int numOfShopPerWorld;
    public int minShopLocation;
    public int maxShopLocation;
    public int numOfShrinePerWorld;
    public int minShrineLocation;
    public int maxShrineLocation;
    public float shopPercentage;
    public float shrinePercentage;

    public EditDeckButtonController deckButton;

    public enum roomType { combat, shop, shrine };

    [SerializeField]
    private List<SmallRoom> smallRooms;
    [SerializeField]
    private BossRoom bossRoom;
    [SerializeField]
    private RoomSetup[] firstRoomSetups;
    [SerializeField]
    private RoomSetup[] roomSetups;
    [SerializeField]
    private RoomSetup[] hardRoomSetups;
    [SerializeField]
    private RoomSetup[] bossRoomSetups;
    public bool debug;
    [SerializeField]
    private RoomSetup debugRoom;

    //private RoomSetup currentRoomSetup;
    private int previousRoomIndex = -99999;

    private Dictionary<int, int> numRoomsPerLevel;

    private bool initiated = false;
    private RoomSetup currentRoomSetup;
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

        if (!initiated)
        {
            canvas = GetComponent<Canvas>();

            previousRoom = new List<Vector2>();
            destroyedRooms = new List<Vector2>();

            selectedLevel = -1;

            numRoomsPerLevel = new Dictionary<int, int>();
            foreach (SmallRoom room in smallRooms)
                if (numRoomsPerLevel.ContainsKey((int)room.GetLocation().y))
                    numRoomsPerLevel[(int)room.GetLocation().y] += 1;
                else
                    numRoomsPerLevel[(int)room.GetLocation().y] = 1;

            RandomizeRooms();
            Refresh();
            initiated = true;
        }
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
        Debug.Log(selectedLevel);
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
                    room.SetSelectable(true);
                else                                                                    //For the rest, set yet to be reached levels one color, set past levels another
                {
                    room.SetSelectable(false);
                    if (room.GetLocation().y <= lastRoom.y + 1)
                        room.SetColor(unviableColor);
                    else
                        room.SetColor(viableColor);
                }
            }
            if (selectedLevel == 7)             //Enable the boss room when viable
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
        int backupShopLocation = Random.Range(0, numRoomsPerLevel[maxShopLocation]);
        int backupShrineLocation = Random.Range(0, numRoomsPerLevel[maxShrineLocation]);
        foreach (SmallRoom room in smallRooms)
        {
            if (numOfShrines < numOfShrinePerWorld && room.GetLocation().y >= minShrineLocation && !shrineLevels.Contains(room.GetLocation().y))
            {
                if (room.GetLocation().y == maxShrineLocation && room.GetLocation().x + 0.5 * (numRoomsPerLevel[maxShrineLocation] - 1) == backupShrineLocation)
                {
                    room.SetType(roomType.shrine);
                    numOfShrines += 1;
                    shrineLevels.Add(room.GetLocation().y);
                    continue;
                }
                else if (Random.Range(0.0f, 1.0f) <= shrinePercentage)
                {
                    room.SetType(roomType.shrine);
                    numOfShrines += 1;
                    shrineLevels.Add(room.GetLocation().y);
                    continue;
                }
            }
            if (numOfShops < numOfShopPerWorld && room.GetLocation().y >= minShopLocation && !shopLevels.Contains(room.GetLocation().y))
            {
                if (room.GetLocation().y == maxShopLocation && room.GetLocation().x + 0.5 * (numRoomsPerLevel[maxShopLocation] - 1) == backupShopLocation)
                {
                    room.SetType(roomType.shop);
                    numOfShops += 1;
                    shopLevels.Add(room.GetLocation().y);
                    continue;
                }
                else if (Random.Range(0.0f, 1.0f) <= shopPercentage)
                {
                    room.SetType(roomType.shop);
                    numOfShops += 1;
                    shopLevels.Add(room.GetLocation().y);
                    continue;
                }
            }
            if (Random.Range(0.0f, 1.0f) <= dissapearPercentage && room.GetLocation().y != 0 && !dissapearedLevels.Contains(room.GetLocation().y))//Never remove first levels and only 1 removed room per level
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
        bossRoom.SetSetup(bossRoomSetups[Random.Range(0, bossRoomSetups.Length)]);
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
        if (debug)
            return debugRoom;

        RoomSetup setup;

        int index = -1;

        if (level < 3)
        {
            index = Random.Range(0, firstRoomSetups.Length);
            while (index == previousRoomIndex)
                index = Random.Range(0, firstRoomSetups.Length);
            setup = firstRoomSetups[index];
        }
        else if (level < 7)
        {
            if (level == 3)
                previousRoomIndex = -99999;
            index = Random.Range(0, roomSetups.Length);
            while (index == previousRoomIndex)
                index = Random.Range(0, roomSetups.Length);
            setup = roomSetups[index];
        }
        else if (level < 8)
        {
            if (level == 7)
                previousRoomIndex = -99999;
            index = Random.Range(0, hardRoomSetups.Length);
            while (index == previousRoomIndex)
                index = Random.Range(0, hardRoomSetups.Length);
            setup = hardRoomSetups[index];
        }
        else
        {
            index = Random.Range(0, bossRoomSetups.Length);
            setup = bossRoomSetups[index];
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

    public RoomSetup GetCurrentRoomSetup()
    {
        return currentRoomSetup;
    }
}
