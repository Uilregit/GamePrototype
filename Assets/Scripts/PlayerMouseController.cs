using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMouseController : DragController
{
    private PlayerMoveController moveController;

    // Start is called before the first frame update
    void Start()
    {
        moveController = GetComponent<PlayerMoveController>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void OnMouseDown()
    {
        moveController.CreateMoveRangeIndicator();
        TileCreator.tileCreator.SetCommitment(true);
        base.OnMouseDown();
    }

    public override void OnMouseDrag()
    {
        if (moveController.GetMoveable())
        {
            base.OnMouseDrag();
            moveController.UpdateMovePosition(newLocation);
        }
    }
    private void OnMouseUp()
    {
        moveController.MoveTo(moveController.GetMoveLocation());
        moveController.DestroyMoveRrangeIndicator();
        TileCreator.tileCreator.SetCommitment(false);
    }
}
