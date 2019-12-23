using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SmallRoom : MonoBehaviour
{
    [SerializeField]
    private Outline highlight;

    private bool selectable = false;
    private RoomController.roomType type = RoomController.roomType.combat;
    private Collider2D collider;

    // Start is called before the first frame update
    void Awake()
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
        RoomController.roomController.Hide();
        if (type == RoomController.roomType.combat)
        {
            RoomController.roomController.SetPreviousRoom(this);
            RoomController.roomController.EnterRoom("CombatScene");
        }
    }

    public void SetSelectable(bool state)
    {
        selectable = state;
        highlight.enabled = state;
    }

    public void SetType(RoomController.roomType value)
    {
        type = value;
    }

    public void Hide()
    {
        collider.enabled = false;
    }

    public void Show()
    {
        collider.enabled = true;
    }

    public void SetColor (Color color)
    {
        GetComponent<Image>().color = color;
    }
}
