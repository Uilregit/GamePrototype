using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RerollRewardCardButtonController : MonoBehaviour
{
    private void OnMouseUp()
    {
        GameController.gameController.RollAndShowRewardsCards(true);
    }
}
