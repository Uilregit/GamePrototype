using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentDragCardController : DragController
{
    public CardDisplay cardDisplay;

    private Vector2 originalPosition;
    private Equipment thisEquip;
    private Card.CasterColor thisCasterColor;

    public void Start()
    {
        cardDisplay.Hide();
        originalPosition = transform.position;
    }

    public void SetEquipment(Equipment equipment, Card.CasterColor casterColor)
    {
        cardDisplay.SetEquipment(equipment, casterColor);
        thisEquip = equipment;
        thisCasterColor = casterColor;
    }

    public void SetSelectable(bool state)
    {
        GetComponent<Collider2D>().enabled = state;
    }

    public override void OnMouseDown()
    {
        base.OnMouseDown();
        cardDisplay.Show();
        cardDisplay.GetComponent<LineRenderer>().enabled = false;
    }

    public override void OnMouseDrag()
    {
        newLocation = offset + (Vector2)CameraController.camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
        transform.position = newLocation;

        CollectionController.collectionController.SetSelectAreaWhiteOut(CameraController.camera.ScreenToWorldPoint(Input.mousePosition).y > -0.3);
    }

    public void OnMouseUp()
    {
        cardDisplay.Hide();
        if (CameraController.camera.ScreenToWorldPoint(Input.mousePosition).y > -0.3)
            CollectionController.collectionController.AddEquipment(thisEquip, thisCasterColor);
        transform.position = originalPosition;
        CollectionController.collectionController.SetSelectAreaWhiteOut(false);
    }
}
