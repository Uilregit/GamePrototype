using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardCardController : MonoBehaviour
{
    private void OnMouseDown()
    {
        CollectionController.collectionController.AddRewardsCard(GetComponent<CardDisplay>().GetCard());
        int deckId = 0;
        switch (GetComponent<CardDisplay>().GetCard().GetCard().casterColor)
        {
            case (Card.CasterColor.Red):
                deckId = 0;
                break;
            case (Card.CasterColor.Blue):
                deckId = 1;
                break;
            case (Card.CasterColor.Green):
                deckId = 2;
                break;
        }
        RewardsMenuController.rewardsMenu.SetDeckID(deckId);

        RewardsMenuController.rewardsMenu.ReportRewardTaken(0);
        RewardsMenuController.rewardsMenu.HideRewardCards();
        RewardsMenuController.rewardsMenu.SetItemsClickable(true);
    }
}
