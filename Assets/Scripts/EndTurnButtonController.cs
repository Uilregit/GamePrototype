using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndTurnButtonController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnMouseUp()
    {
        if (TurnController.turnController.GetIsPlayerTurn())
            TurnController.turnController.SetPlayerTurn(false);
    }
}
