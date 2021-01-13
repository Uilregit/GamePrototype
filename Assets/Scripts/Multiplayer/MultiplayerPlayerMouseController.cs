using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MultiplayerPlayerMouseController : NetworkBehaviour
{
    private MultiplayerPlayerMoveController multiMoveController;
    protected Vector2 offset = Vector2.zero;
    protected Vector2 newLocation;

    // Start is called before the first frame update
    void Start()
    {
        multiMoveController = GetComponent<MultiplayerPlayerMoveController>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnMouseDown()
    {
        if (isServer && gameObject.tag == "Enemy" || !isServer && gameObject.tag == "Player")
            return;
        multiMoveController.CreateMoveRangeIndicator();
        if (TurnController.turnController.GetIsPlayerTurn())
            offset = transform.position - CameraController.camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
    }

    public void OnMouseDrag()
    {
        if (isServer && gameObject.tag == "Enemy" || !isServer && gameObject.tag == "Player")
            return;
        if (multiMoveController.GetMoveable())
        {
            if (TurnController.turnController.GetIsPlayerTurn())
            {
                newLocation = offset + (Vector2)CameraController.camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
                transform.position = newLocation;
            }
            multiMoveController.UpdateMovePosition(newLocation);
        }
    }
    private void OnMouseUp()
    {
        if (isServer && gameObject.tag == "Enemy" || !isServer && gameObject.tag == "Player")
            return;
        multiMoveController.MoveTo(multiMoveController.GetMoveLocation());
        multiMoveController.DestroyMoveRrangeIndicator();
    }
}
