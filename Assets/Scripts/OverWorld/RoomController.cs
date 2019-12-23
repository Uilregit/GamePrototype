using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomController : MonoBehaviour
{
    public static RoomController roomController;

    public Color previousColor;
    public Color viableColor;
    public Color unviableColor;

    public float dissapearPercentage;
    public float shopPercentage;
    public float restPercentage;

    public enum roomType { combat, shop, rest };

    [SerializeField]
    private SmallRoom firstRoom;
    [SerializeField]
    private SmallRoom[] smallRooms;
    [SerializeField]
    private BossRoom bossRoom;
    [SerializeField]
    private RoomSetup[] roomSetups;

    private bool initiated = false;
    private SmallRoom previousRoom;

    private Canvas canvas;

    // Start is called before the first frame update
    void Start()
    {
        if (RoomController.roomController == null)
            RoomController.roomController = this;
        else
            Destroy(this.gameObject);

        DontDestroyOnLoad(this.gameObject);
        canvas = GetComponent<Canvas>();

        if (!initiated)
        {
            RandomizeRooms();
            Refresh();
            initiated = true;
        }
    }

    //Set clickable rooms and set all cooresponding colors
    public void Refresh()
    {
        if (previousRoom == null)
            firstRoom.SetSelectable(true);
        else
        {
            firstRoom.SetSelectable(false);
            if (firstRoom == previousRoom)
                firstRoom.SetColor(previousColor);
            else
                firstRoom.SetColor(unviableColor);
            foreach (SmallRoom room in smallRooms)
            {
                if (room.transform.position.y - previousRoom.transform.position.y == 1 &&
                    Mathf.Abs(room.transform.position.x - previousRoom.transform.position.x) <= 1)
                    room.SetSelectable(true);
                else
                {
                    room.SetSelectable(false);
                    if (room.transform.position.y <= previousRoom.transform.position.y +1)
                        room.SetColor(unviableColor);
                    else
                        room.SetColor(viableColor);
                }
                if (room == previousRoom)
                    room.SetColor(previousColor);
            }
        }
    }

    public void RandomizeRooms()
    {
        foreach(SmallRoom room in smallRooms)
        {
            if (Random.Range(0, 1) <= dissapearPercentage)
                room.SetSelectable(false);
        }
    }

    public void EnterRoom(string sceneName)
    {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    public RoomSetup GetRoomSetup()
    {
        RoomSetup setup = roomSetups[Random.Range(0, roomSetups.Length)];
        return setup;
    }

    public void SetPreviousRoom(SmallRoom value)
    {
        previousRoom = value;
    }

    public void Hide()
    {
        canvas.enabled = false;
        firstRoom.Hide();
        foreach (SmallRoom room in smallRooms)
            room.Hide();
    }

    public void Show()
    {
        canvas.enabled = true;
        firstRoom.Show();
        foreach (SmallRoom room in smallRooms)
            room.Show();
    }
}
