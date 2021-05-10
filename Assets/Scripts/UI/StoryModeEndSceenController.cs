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

    // Start is called before the first frame update
    void Start()
    {
        totalGold = ResourceController.resource.GetGold();

        StoryRoomSetup setup = StoryModeController.story.GetCurrentRoomSetup();

        for (int i = 0; i < 3; i++)
        {
            items[i].SetEnabled(StoryModeController.story.ChallengeSatisfied(i));
            items[i].SetValues(setup.rewardTypes[i].ToString() + " x" + setup.rewardAmounts[i].ToString(), setup.rewardCosts[i]);
        }

        for (int i = 3; i < 5; i++)
        {
            if (setup.rewardTypes.Length > i && totalGold >= setup.rewardCosts[i])
            {
                items[i].SetEnabled(true);
                items[i].SetValues(setup.rewardTypes[i].ToString() + " x" + setup.rewardAmounts[i].ToString(), setup.rewardCosts[i]);
            }
            else
                items[i].SetEnabled(false);
        }

        ResetItemEnabled();
    }

    public void ReportItemBought(int gold, bool bought)
    {
        if (bought)
            totalGold -= gold;
        else
            totalGold += gold;

        ResetItemEnabled();
    }

    private void ResetItemEnabled()
    {
        StoryRoomSetup setup = StoryModeController.story.GetCurrentRoomSetup();

        for (int i = 0; i < 3; i++)
        {
            items[i].SetGreyout(!(StoryModeController.story.ChallengeSatisfied(i) && totalGold >= setup.rewardCosts[i]));
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

    public void BuyAndExit()
    {
        SceneManager.LoadScene("StoryModeScene");
    }
}
