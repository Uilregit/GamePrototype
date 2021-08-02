using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StoryModeEndSceenController : MonoBehaviour
{
    public Text goldText;
    public Text exitButton;

    public StoryModeEndItemController[] items;

    private int totalGold = 0;

    private Dictionary<StoryModeController.RewardsType, int> boughtItems = new Dictionary<StoryModeController.RewardsType, int>();
    private Dictionary<Card, int> boughtCards = new Dictionary<Card, int>();
    private Dictionary<Equipment, int> boughtEquipiments = new Dictionary<Equipment, int>();
    private bool[] challengeItemsBought = new bool[3] { false, false, false };

    // Start is called before the first frame update
    public virtual void Start()
    {
        totalGold = ResourceController.resource.GetGold();

        StoryRoomSetup setup = StoryModeController.story.GetCurrentRoomSetup();

        if (StoryModeController.story.GetChallengeItemsBought().ContainsKey(StoryModeController.story.GetCurrentRoomID()))
            challengeItemsBought = StoryModeController.story.GetChallengeItemsBought()[StoryModeController.story.GetCurrentRoomID()];
        else
            challengeItemsBought = new bool[3] { false, false, false };

        for (int i = 0; i < 3; i++)
        {
            items[i].SetEnabled(StoryModeController.story.ChallengeSatisfied(i) && !challengeItemsBought[i]);
            items[i].SetValues(setup.rewardTypes[i], setup.rewardAmounts[i], setup.rewardCosts[i], i);
            if (challengeItemsBought[i])
                items[i].SetBought();
        }

        for (int i = 3; i < 5; i++)
        {
            items[i].SetValues(setup.rewardTypes[i], setup.rewardAmounts[i], setup.rewardCosts[i], i);
            if (setup.rewardTypes.Length > i && totalGold >= setup.rewardCosts[i])
                items[i].SetEnabled(true);
            else
                items[i].SetEnabled(false);
        }

        ResetItemEnabled();
    }

    public virtual void ReportItemBought(int gold, StoryModeController.RewardsType name, int amount, bool bought, int index)
    {
        if (bought)
            totalGold -= gold;
        else
            totalGold += gold;

        if (name == StoryModeController.RewardsType.SpecificCard)
        {
            if (bought)
            {
                if (boughtCards.ContainsKey(items[index].GetCard()))
                    boughtCards[items[index].GetCard()] += amount;
                else
                    boughtCards[items[index].GetCard()] = amount;
            }
            else
                boughtCards[items[index].GetCard()] -= amount;
        }
        else if (name == StoryModeController.RewardsType.SpecificEquipment)
        {
            if (bought)
            {
                if (boughtEquipiments.ContainsKey(items[index].GetEquipment()))
                    boughtEquipiments[items[index].GetEquipment()] += amount;
                else
                    boughtEquipiments[items[index].GetEquipment()] = amount;
            }
            else
                boughtEquipiments[items[index].GetEquipment()] -= amount;
        }
        else
        {
            if (bought)
            {
                if (boughtItems.ContainsKey(name))
                    boughtItems[name] += amount;
                else
                    boughtItems[name] = amount;
            }
            else
                boughtItems[name] -= amount;
        }

        if (index < 3)
            challengeItemsBought[index] = bought;

        ResetItemEnabled();
    }

    private void ResetItemEnabled()
    {
        StoryRoomSetup setup = StoryModeController.story.GetCurrentRoomSetup();

        for (int i = 0; i < 3; i++)
        {
            items[i].SetGreyout(!(StoryModeController.story.ChallengeSatisfied(i) && totalGold >= setup.rewardCosts[i]));
            if (challengeItemsBought[i] && !items[i].GetSelected())
                items[i].SetBought();
        }

        for (int i = 3; i < 5; i++)
        {
            if (setup.rewardTypes.Length > i && totalGold >= setup.rewardCosts[i])
            {
                items[i].SetGreyout(false);
            }
            else
                items[i].SetGreyout(true);
        }

        goldText.text = totalGold.ToString();
        exitButton.text = "Exit with " + totalGold.ToString() + "xp";
    }

    public int GetCurrentGold()
    {
        return totalGold;
    }

    public virtual void BuyAndExit()
    {
        ResourceController.resource.ChangeGold(-ResourceController.resource.GetGold());
        AchievementSystem.achieve.ResetAchievements();
        StoryModeController.story.ReportItemsBought(boughtItems);
        StoryModeController.story.ReportCardsBought(boughtCards);
        StoryModeController.story.ReportEquipmentBought(boughtEquipiments);
        StoryModeController.story.AddChallengeItemsBought(StoryModeController.story.GetCurrentRoomID(), challengeItemsBought);
        Destroy(RoomController.roomController.gameObject);
        RoomController.roomController = null;
        InformationLogger.infoLogger.SaveStoryModeGame();   //Must come before reset decks otherwise items will be overwritten
        StoryModeController.story.ResetDecks();
        StoryModeController.story.SetMenuBar(true);
        StoryModeController.story.SetCombatInfoMenu(false);
        SceneManager.LoadScene("StoryModeScene");
    }
}
