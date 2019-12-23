using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragController : MonoBehaviour
{
    protected Vector2 offset = Vector2.zero;
    protected Vector2 newLocation;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    public virtual void OnMouseDown()
    {
        if (TurnController.turnController.GetIsPlayerTurn())
            offset = transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
    }

    public virtual void OnMouseDrag()
    {
        if (TurnController.turnController.GetIsPlayerTurn())
        {
            newLocation = offset + (Vector2)Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
            transform.position = newLocation;
        }
    }
}
