using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RewardsItemController : MonoBehaviour
{
    private int value;
    private RewardsMenuController.RewardType type;
    private int index;
    private Relic relic;

    public void SetValues(int newIndex, int newValue, RewardsMenuController.RewardType newType)
    {
        index = newIndex;
        value = newValue;
        type = newType;
    }

    public void OnMouseDown()
    {
        if (type == RewardsMenuController.RewardType.PassiveGold)
        {
            ResourceController.resource.ChangeGold(value);

            InformationLogger.infoLogger.SaveGoldInfo(InformationLogger.infoLogger.patchID,
                            InformationLogger.infoLogger.gameID,
                            RoomController.roomController.worldLevel.ToString(),
                            RoomController.roomController.selectedLevel.ToString(),
                            RoomController.roomController.roomName,
                            value.ToString(),
                            "0",
                            ResourceController.resource.GetGold().ToString());

            RewardsMenuController.rewardsMenu.ReportRewardTaken(RewardsMenuController.RewardType.PassiveGold);
        }
        else if (type == RewardsMenuController.RewardType.OverkillGold)
        {
            ResourceController.resource.ChangeGold(value);

            InformationLogger.infoLogger.SaveGoldInfo(InformationLogger.infoLogger.patchID,
                            InformationLogger.infoLogger.gameID,
                            RoomController.roomController.worldLevel.ToString(),
                            RoomController.roomController.selectedLevel.ToString(),
                            RoomController.roomController.roomName,
                            "0",
                            value.ToString(),
                            ResourceController.resource.GetGold().ToString());

            RewardsMenuController.rewardsMenu.ReportRewardTaken(RewardsMenuController.RewardType.OverkillGold);
        }
        else if (type == RewardsMenuController.RewardType.Card)
        {
            RewardsMenuController.rewardsMenu.SetItemsClickable(false);

            List<Card> currentRewards = new List<Card>();
            Random.InitState(RoomController.roomController.GetCurrentSmallRoom().GetSeed());
            for (int i = 0; i < GameController.gameController.rewardCards.Length; i++)
            {
                for (int j = 0; j < 100; j++)
                {
                    Card reward = LootController.loot.GetCard();
                    if (!currentRewards.Contains(reward) && CollectionController.collectionController.GetCountOfCardInCollection(reward) < 4) //Ensures that all rewards are unique
                    {
                        GameController.gameController.rewardCards[i].transform.parent.GetComponent<CardController>().SetCard(reward, false, true, true);
                        currentRewards.Add(reward);
                        break;
                    }
                }
                GameController.gameController.rewardCards[i].Show();
                GameController.gameController.rewardCards[i].SetHighLight(true);
                GameController.gameController.rewardCards[i].GetComponent<LineRenderer>().enabled = false;
                GameController.gameController.rewardCards[i].transform.parent.gameObject.GetComponent<Collider2D>().enabled = true;
            }

            //Does not report reward taken here, reports in rewardcardcontroller
        }
        else if (type == RewardsMenuController.RewardType.Relic)
        {
            RewardsMenuController.rewardsMenu.SetItemsClickable(false);
            RewardsMenuController.rewardsMenu.ShowRelicRewardMenu(relic);
        }

        Hide();
    }

    public void Hide()
    {
        GetComponent<Image>().enabled = false;
        GetComponent<Collider2D>().enabled = false;

        transform.GetChild(0).GetComponent<Image>().enabled = false;
        transform.GetChild(1).GetComponent<Text>().enabled = false;
    }

    public void SetRelic(Relic newRelic)
    {
        relic = newRelic;
    }
}
