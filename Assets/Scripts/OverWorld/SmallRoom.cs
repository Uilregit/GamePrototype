using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SmallRoom : MonoBehaviour
{
    [SerializeField]
    private Outline highlight;
    [SerializeField]
    private Collider2D collider;
    [SerializeField]
    private Vector2 location;

    private bool destroyed = false;

    private bool selectable = false;
    private RoomController.roomType type = RoomController.roomType.combat;

    private RoomSetup setup;

    // Start is called before the first frame update
    void Awake()
    {
        //highlight = GetComponent<Outline>();
        //collider = GetComponent<Collider2D>();
        setup = ScriptableObject.CreateInstance<RoomSetup>();
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
        InformationLogger.infoLogger.SaveGame(false);

        RoomController.roomController.selectedLevel = (int)location.y;
        //setup = RoomController.roomController.GetRoomSetup();
        //RoomController.roomController.SetCurrentRoomSetup(setup);

        RoomController.roomController.Hide();
        RoomController.roomController.SetPreviousRoom(this);

        if (type == RoomController.roomType.combat)
            RoomController.roomController.EnterRoom("CombatScene");
        else if (type == RoomController.roomType.shop)
            RoomController.roomController.EnterRoom("ShopScene");
        else if (type == RoomController.roomType.shrine)
            RoomController.roomController.EnterRoom("ShrineScene");
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

    public void SetDestroyed(bool value)
    {
        destroyed = value;
    }

    public bool GetDestroyed()
    {
        return destroyed;
    }
}
