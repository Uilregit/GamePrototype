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

    public Text flavorTextbox;
    public Image[] challengeStars;

    public Image enterButton;

    //private List<int> completedRooms = new List<int>();
    private StoryRoomController selectedRoom = null;

    // Start is called before the first frame update
    void Start()
    {
        if (!InformationLogger.infoLogger.GetHasStoryModeSaveFile())
            InformationLogger.infoLogger.SaveStoryModeGame();
        InformationLogger.infoLogger.LoadStoryModeGame();

        foreach (StoryRoomController r in rooms)
        {
            if (StoryModeController.story.GetCompletedRooms().Contains(r.roomId))
            {
                r.GetComponent<Image>().color = completedColor;
                r.GetComponent<Collider2D>().enabled = true;
            }
            else if (StoryModeController.story.GetCompletedRooms().Contains(r.unlockRequirementID) || r.roomId == 1)
            {
                r.GetComponent<Image>().color = unlockedColor;
                r.GetComponent<Collider2D>().enabled = true;
                selectedRoom = r;
            }
            else
            {
                r.GetComponent<Image>().color = lockedColor;
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
            if (StoryModeController.story.GetChallengeValues().ContainsKey(selectedRoom.roomId) && ChallengeSatisfied(i))
            {
                challengeStars[i].transform.GetChild(0).GetComponent<Image>().color = goldColor;
                stars++;
            }
            else
                challengeStars[i].transform.GetChild(0).GetComponent<Image>().color = completedColor;


            challengeStars[i].transform.GetChild(1).GetComponent<Text>().text = selectedRoom.setup.GetChallengeText(selectedRoom.roomId, i);
        }
        if (stars >= 3)
            selectedRoom.GetComponent<Image>().color = goldColor;

        enterButton.color = unlockedColor;
        enterButton.GetComponent<Collider2D>().enabled = true;

        StoryModeController.story.SetSetup(selectedRoom.setup);
    }

    private bool ChallengeSatisfied(int index)
    {
        switch (selectedRoom.setup.challengeComparisonType[index])
        {
            case StoryRoomSetup.ChallengeComparisonType.GreaterThan:
                return StoryModeController.story.GetChallengeValues()[selectedRoom.roomId][index] >= selectedRoom.setup.challengeValues[index];
            case StoryRoomSetup.ChallengeComparisonType.EqualTo:
                return StoryModeController.story.GetChallengeValues()[selectedRoom.roomId][index] == selectedRoom.setup.challengeValues[index];
            case StoryRoomSetup.ChallengeComparisonType.LessThan:
                return StoryModeController.story.GetChallengeValues()[selectedRoom.roomId][index] <= selectedRoom.setup.challengeValues[index];
        }
        return false;
    }

    public void EnterRoom()
    {
        StoryModeController.story.SetCurrentRoomID(selectedRoom.roomId);
        StoryModeController.story.SetCurrentRoomSetup(selectedRoom.setup);
        LootController.loot.ResetPartyLootTable();
        CollectionController.collectionController.FinalizeDeck();
        SceneManager.LoadScene("OverworldScene", LoadSceneMode.Single);
    }
}
