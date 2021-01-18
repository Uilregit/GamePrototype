using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragController : MonoBehaviour
{
    protected Vector2 offset = Vector2.zero;
    protected Vector2 newLocation;

    public virtual void OnMouseDown()
    {
        offset = transform.position - CameraController.camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
    }

    public virtual void OnMouseDrag()
    {
        if (TurnController.turnController.GetIsPlayerTurn())
        {
            newLocation = offset + (Vector2)CameraController.camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
            transform.position = newLocation;
        }
    }
}
