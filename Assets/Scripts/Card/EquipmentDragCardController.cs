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
        originalPosition = transform.localPosition;
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
        if (TutorialController.tutorial.GetEnabled())
            return;

        cardDisplay.cardSounds.PlaySelectSound();
        base.OnMouseDown();
        cardDisplay.Show();
        cardDisplay.GetComponent<LineRenderer>().enabled = false;
    }

    public override void OnMouseDrag()
    {
        if (TutorialController.tutorial.GetEnabled())
            return;

        newLocation = offset + (Vector2)CameraController.camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
        transform.position = newLocation;

        CollectionController.collectionController.SetSelectAreaWhiteOut(CameraController.camera.ScreenToWorldPoint(Input.mousePosition).y > -0.6);
        if (CameraController.camera.ScreenToWorldPoint(Input.mousePosition).y > -0.6)
            CollectionController.collectionController.ResetStatsTexts(thisEquip, false);
        else
            CollectionController.collectionController.ResetStatsTexts();
    }

    public void OnMouseUp()
    {
        if (TutorialController.tutorial.GetEnabled())
            return;

        cardDisplay.cardSounds.PlayCastSound();
        cardDisplay.Hide();
        if (CameraController.camera.ScreenToWorldPoint(Input.mousePosition).y > -0.6)
        {
            CollectionController.collectionController.AddEquipment(thisEquip, thisCasterColor);
            CollectionController.collectionController.SetIsShowingCards(false);
        }

        transform.localPosition = originalPosition;
        CollectionController.collectionController.SetSelectAreaWhiteOut(false);
        CollectionController.collectionController.ResetStatsTexts();
    }
}
