using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StoryModeSceneController : MonoBehaviour
{
    public List<StoryRoomController> rooms;

    public Color completedColor;
    public Color unlockedColor;
    public Color lockedColor;
    public Color goldColor;
    public Color shopColor;
    public Color newWorldColor;
    public Color iconColor;
    public Color iconAltColor;
    public Color iconLockedColor;
    public Sprite enemyIcon;
    public Sprite bossIcon;
    public Sprite crownIcon;
    public Sprite shopIcon;
    public Sprite lockedIcon;
    public Sprite newWorldIcon;

    public Text flavorTextbox;
    public Image[] challengeStars;

    public Image enterButton;

    //private List<int> completedRooms = new List<int>();
    private StoryRoomController selectedRoom = null;

    // Start is called before the first frame update
    void Start()
    {
        foreach (StoryRoomController r in rooms)
        {
            r.transform.GetChild(0).GetComponent<Image>().color = iconColor;
            switch (r.roomType)
            {
                case StoryRoomController.StoryRoomType.Combat:
                    r.transform.GetChild(0).GetComponent<Image>().sprite = enemyIcon;
                    break;
                case StoryRoomController.StoryRoomType.Boss:
                    r.transform.GetChild(0).GetComponent<Image>().sprite = bossIcon;
                    break;
                case StoryRoomController.StoryRoomType.Shop:
                    r.transform.GetChild(0).GetComponent<Image>().sprite = shopIcon;
                    break;
                case StoryRoomController.StoryRoomType.NewWorld:
                    if (StoryModeController.story.GetCompletedRooms().Contains(r.unlockRequirementID))
                        r.transform.GetChild(0).GetComponent<Image>().sprite = newWorldIcon;
                    break;
            }

            if (StoryModeController.story.GetCompletedRooms().Contains(r.roomId))
            {
                if (GetNumChallengeSatisfied(r) == 3)
                {
                    r.transform.GetChild(0).GetComponent<Image>().sprite = crownIcon;
                    if (StoryModeController.story.GetNumberOfChallengeItemsBought(r.roomId) == 3)
                        r.GetComponent<Image>().color = shopColor;
                    else
                        r.GetComponent<Image>().color = goldColor;
                }
                else
                {
                    r.GetComponent<Image>().color = completedColor;
                    r.transform.GetChild(0).GetComponent<Image>().color = iconAltColor;
                }
                r.GetComponent<Collider2D>().enabled = true;
            }
            else if (StoryModeController.story.GetCompletedRooms().Contains(r.unlockRequirementID) || r.roomId == 1)
            {
                if (r.roomType == StoryRoomController.StoryRoomType.Shop)
                    r.GetComponent<Image>().color = shopColor;
                else
                {
                    r.GetComponent<Image>().color = unlockedColor;
                    selectedRoom = r;
                }
                r.GetComponent<Collider2D>().enabled = true;
            }
            else
            {
                r.GetComponent<Image>().color = lockedColor;
                r.transform.GetChild(0).GetComponent<Image>().color = iconLockedColor;
                r.transform.GetChild(0).GetComponent<Image>().sprite = lockedIcon;
                r.GetComponent<Collider2D>().enabled = false;
            }
        }

        enterButton.color = lockedColor;
        enterButton.GetComponent<Collider2D>().enabled = false;

        ReportRoomSelected(selectedRoom.roomId);
    }

    public void ReportRoomSelected(int roomId)
    {
        foreach (StoryRoomController r in rooms)
            if (r.roomId == roomId)
            {
                if (!StoryModeController.story.GetCompletedRooms().Contains(r.roomId) && !StoryModeController.story.GetCompletedRooms().Contains(r.unlockRequirementID) && roomId != 1)
                    return;

                selectedRoom.GetComponent<Outline>().enabled = false;
                selectedRoom = r;
                break;
            }

        selectedRoom.GetComponent<Outline>().enabled = true;

        flavorTextbox.text = selectedRoom.setup.flavorText;
        int stars = 0;
        for (int i = 0; i < 3; i++)
        {
            if (StoryModeController.story.GetChallengeValues().ContainsKey(selectedRoom.roomId) && ChallengeSatisfied(selectedRoom, i))
            {
                if (StoryModeController.story.GetChallengeItemsBought()[roomId][i])
                    challengeStars[i].transform.GetComponent<Image>().color = shopColor;
                else
                    challengeStars[i].transform.GetComponent<Image>().color = goldColor;
                stars++;
            }
            else
                challengeStars[i].transform.GetComponent<Image>().color = completedColor;


            challengeStars[i].transform.GetChild(0).GetComponent<Text>().text = selectedRoom.setup.GetChallengeText(selectedRoom.roomId, i);
        }

        enterButton.color = unlockedColor;
        enterButton.GetComponent<Collider2D>().enabled = true;

        StoryModeController.story.SetCurrentRoomSetup(selectedRoom.setup);
    }

    private bool ChallengeSatisfied(StoryRoomController setup, int index)
    {
        switch (setup.setup.challengeComparisonType[index])
        {
            case StoryRoomSetup.ChallengeComparisonType.GreaterThan:
                return StoryModeController.story.GetChallengeValues()[setup.roomId][index] >= setup.setup.challengeValues[index];
            case StoryRoomSetup.ChallengeComparisonType.EqualTo:
                return StoryModeController.story.GetChallengeValues()[setup.roomId][index] == setup.setup.challengeValues[index];
            case StoryRoomSetup.ChallengeComparisonType.LessThan:
                return StoryModeController.story.GetChallengeValues()[setup.roomId][index] <= setup.setup.challengeValues[index];
        }
        return false;
    }

    private int GetNumChallengeSatisfied(StoryRoomController setup)
    {
        int output = 0;

        for (int i = 0; i < 3; i++)
            if (ChallengeSatisfied(setup, i))
                output++;
        return output;
    }

    public void EnterRoom()
    {
        StoryModeController.story.SetCurrentRoomID(selectedRoom.roomId);
        StoryModeController.story.SetCurrentRoomSetup(selectedRoom.setup);

        if (selectedRoom.roomType == StoryRoomController.StoryRoomType.Combat || selectedRoom.roomType == StoryRoomController.StoryRoomType.Boss)
        {
            if (StoryModeController.story.GetChallengeValues().ContainsKey(selectedRoom.roomId))
                selectedRoom.setup.SetBestValues(StoryModeController.story.GetChallengeValues()[selectedRoom.roomId]);
            else
                selectedRoom.setup.SetBestValues(new int[3] { -1, -1, -1 });
            LootController.loot.ResetPartyLootTable();
            CollectionController.collectionController.FinalizeDeck();
            SceneManager.LoadScene("OverworldScene", LoadSceneMode.Single);
        }
        else if (selectedRoom.roomType == StoryRoomController.StoryRoomType.Shop)
        {
            SceneManager.LoadScene("StoryModeShopScene", LoadSceneMode.Single);
        }
        else if (selectedRoom.roomType == StoryRoomController.StoryRoomType.Arena)
        {

        }
        else if (selectedRoom.roomType == StoryRoomController.StoryRoomType.NewWorld)
        {

        }
    }
}
