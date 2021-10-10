using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SmallRoom : MonoBehaviour
{
    private Outline highlight;
    private Collider2D collider;
    private Vector2 location;

    private bool destroyed = false;

    private bool selectable = false;
    private RoomController.roomType type = RoomController.roomType.combat;

    private RoomSetup setup;

    private int seed;

    private void Awake()
    {
        highlight = GetComponent<Outline>();
        collider = GetComponent<Collider2D>();
    }

    private void OnMouseDown()
    {
        if (selectable)
        {
            Enter();
        }
    }

    public void Enter()
    {
        RoomController.roomController.SetCurrentRoomSetup(setup);
        if (setup != null)
            RoomController.roomController.roomName = setup.roomName;

        RoomController.roomController.SetViableRoom(location);
        InformationLogger.infoLogger.SaveGame(false);

        RoomController.roomController.selectedLevel = (int)location.y;
        //setup = RoomController.roomController.GetRoomSetup();
        //RoomController.roomController.SetCurrentRoomSetup(setup);

        RoomController.roomController.Hide();
        RoomController.roomController.SetPreviousRoom(this);
        RoomController.roomController.SetCurrentSmallRoom(this);

        Random.InitState(seed);

        string roomString = "Error";
        if (type == RoomController.roomType.combat)
            roomString = "CombatScene";
        else if (type == RoomController.roomType.shop)
            roomString = "ShopScene";
        else if (type == RoomController.roomType.shrine)
            roomString = "ShrineScene";

        if (roomString == "CombatScene")
            InformationLogger.infoLogger.SaveTimeInfo(InformationLogger.infoLogger.patchID,
                                InformationLogger.infoLogger.gameID,
                                RoomController.roomController.worldLevel.ToString(),
                                RoomController.roomController.selectedLevel.ToString(),
                                roomString,
                                RoomController.roomController.roomName,
                                "0P");
        else
            InformationLogger.infoLogger.SaveTimeInfo(InformationLogger.infoLogger.patchID,
                                InformationLogger.infoLogger.gameID,
                                RoomController.roomController.worldLevel.ToString(),
                                RoomController.roomController.selectedLevel.ToString(),
                                roomString,
                                roomString);

        if (roomString == "ShopScene" || roomString == "ShrineScene")
            MusicController.music.SetHighPassFilter(true);

        RoomController.roomController.EnterRoom(roomString);
    }

    public void SetRoomType(RoomController.roomType newType)
    {
        type = newType;
    }

    public RoomController.roomType GetRoomType()
    {
        return type;
    }

    public void SetSetup(RoomSetup newSetup)
    {
        setup = newSetup;
    }

    public void SetSelectable(bool state)
    {
        if (highlight == null)
            highlight = GetComponent<Outline>();
        if (collider == null)
            collider = GetComponent<Collider2D>();

        selectable = state;
        highlight.enabled = state;
        collider.enabled = state;
    }

    public void SetType(RoomController.roomType value)
    {
        type = value;
    }

    public void Hide()
    {
        GetComponent<Image>().enabled = false;
        selectable = false;
        highlight.enabled = false;
        collider.enabled = false;
    }

    public void Show()
    {
        GetComponent<Image>().enabled = true;
        //selectable = true;
        //highlight.enabled = true;
        //collider.enabled = true;
    }

    public void SetColor(Color color)
    {
        GetComponent<Image>().color = color;
    }

    public Vector2 GetLocation()
    {
        return location;
    }

    public void SetLocation(Vector2 vec)
    {
        location = vec;
    }

    public void SetDestroyed(bool value)
    {
        destroyed = value;
    }

    public void SetSeed(int value)
    {
        seed = value;
    }

    public int GetSeed()
    {
        return seed;
    }
}
