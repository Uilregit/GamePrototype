using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StoryModeSecretShopSceneController : StoryModeEndSceenController
{
    private int totalGoals = 0;
    private Dictionary<StoryModeController.RewardsType, int> boughtItems = new Dictionary<StoryModeController.RewardsType, int>();
    private List<int> boughtIndexes = new List<int>();

    public override void Start()
    {
        StoryRoomSetup setup = StoryModeController.story.GetCurrentRoomSetup();
        totalGoals = StoryModeController.story.GetUnspentChallengeTokens();

        for (int i = 0; i < items.Length; i++)
        {
            items[i].SetValues(setup.rewardTypes[i], setup.rewardAmounts[i], setup.rewardCosts[i], i);
            if (setup.rewardTypes.Length > i && totalGoals >= setup.rewardCosts[i])
                items[i].SetEnabled(true);
            else
                items[i].SetEnabled(false);
        }

        ResetItemEnabled();
    }

    public override void ReportItemBought(int gold, StoryModeController.RewardsType name, int amount, bool bought, int index)
    {
        MusicController.music.PlaySFX(MusicController.music.uiUseLowSFX[Random.Range(0, MusicController.music.uiUseLowSFX.Count)]);
        if (bought)
            totalGoals -= gold;
        else
            totalGoals += gold;

        if (bought)
        {
            boughtIndexes.Add(index);
            if (boughtItems.ContainsKey(name))
                boughtItems[name] += amount;
            else
                boughtItems[name] = amount;
        }
        else
        {
            boughtIndexes.Remove(index);
            boughtItems[name] -= amount;
        }

        ResetItemEnabled();
    }

    private void ResetItemEnabled()
    {
        StoryRoomSetup setup = StoryModeController.story.GetCurrentRoomSetup();
        Dictionary<int, bool[]> secretShopItemsbought = StoryModeController.story.GetSecretShopItemsBought();

        bool anySelected = false;

        for (int i = 0; i < 5; i++)
        {
            items[i].SetGreyout(totalGoals < setup.rewardCosts[i]);
            if (secretShopItemsbought[StoryModeController.story.GetWorldNumber()][i] && !items[i].GetSelected())
                items[i].SetBought();

            if (items[i].GetSelected())
                anySelected = true;
        }

        goldText.text = totalGoals.ToString();
        if (anySelected)
            exitButton.text = "Confirm";
        else
            exitButton.text = "Back";
    }

    public int GetCurrentGoals()
    {
        return totalGoals;
    }

    public override void BuyAndExit()
    {
        MusicController.music.PlaySFX(MusicController.music.uiUseLowSFX[Random.Range(0, MusicController.music.uiUseLowSFX.Count)]);
        MusicController.music.SetHighPassFilter(false);
        //ResourceController.resource.ChangeGold(-ResourceController.resource.GetGold());
        AchievementSystem.achieve.ResetAchievements();
        StoryModeController.story.ReportItemsBought(boughtItems);
        foreach (int index in boughtIndexes)
            StoryModeController.story.ReportSecretShopItemBought(StoryModeController.story.GetWorldNumber(), index, true);
        //Destroy(RoomController.roomController.gameObject);
        //RoomController.roomController = null;
        InformationLogger.infoLogger.SaveStoryModeGame();   //Must come before reset decks otherwise items will be overwritten
        //StoryModeController.story.ResetDecks();
        StoryModeController.story.SetMenuBar(true);
        StoryModeController.story.SetCombatInfoMenu(false);
        SceneManager.LoadScene("StoryModeScene");
    }
}
