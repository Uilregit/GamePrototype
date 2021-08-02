using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

public class StoryModeSceneController : MonoBehaviour
{
    public static StoryModeSceneController story;

    private List<StoryRoomController>[] rooms;

    public Color completedColor;
    public Color unlockedColor;
    public Color lockedColor;
    public Color goldColor;
    public Color shopColor;
    public Color arenaColor;
    public Color newWorldColor;
    public Color iconColor;
    public Color iconAltColor;
    public Color iconLockedColor;
    public Color outlineColor;
    public Color hiddenOutlineColor;
    public Sprite enemyIcon;
    public Sprite bossIcon;
    public Sprite crownIcon;
    public Sprite bossCrownIcon;
    public Sprite arenaIcon;
    public Sprite shopIcon;
    public Sprite lockedIcon;
    public Sprite newWorldIcon;

    public GameObject[] worlds;

    public Text flavorTextbox;
    public Image[] challengeStars;

    public RectTransform stars;
    public RectTransform items;
    public Image starsButton;
    public Image itemsButton;
    public Text[] itemsList;
    public Image[] itemsSoldOutIcons;
    public Image[] itemsIcons;

    public Image enterButton;

    //private List<int> completedRooms = new List<int>();
    private StoryRoomController selectedRoom = null;

    // Start is called before the first frame update
    void Start()
    {
        if (StoryModeSceneController.story == null)
            StoryModeSceneController.story = this;
        else
            Destroy(this.gameObject);

        rooms = new List<StoryRoomController>[worlds.Length];
        for (int i = 0; i < worlds.Length; i++)
            rooms[i] = worlds[i].GetComponentsInChildren<StoryRoomController>().ToList();

        RefreshWorldRooms();

        ReportRoomSelected(selectedRoom.roomId);
        StarsViewSelected();

        StartCoroutine(HighlightRooms());
    }

    public void RefreshWorldRooms()
    {
        for (int i = 0; i < worlds.Length; i++)
            worlds[i].gameObject.SetActive(i == StoryModeController.story.GetWorldNumber());

        selectedRoom = null;
        int maxID = 0;

        foreach (StoryRoomController r in rooms[StoryModeController.story.GetWorldNumber()])
        {
            //Used to default selected room to last room if all rooms in world have been completed
            if ((r.roomType == StoryRoomController.StoryRoomType.Combat || r.roomType == StoryRoomController.StoryRoomType.Boss) && !r.startHidden && r.roomId > maxID)
                maxID = r.roomId;

            //Handles hidden rooms first
            if (r.startHidden)
            {
                if (StoryModeController.story.GetCompletedRooms().Contains(r.unlockRequirementID))
                {
                    if (r.unlockRequire3Stars)
                    {
                        StoryRoomController previousRoom = null;
                        foreach (StoryRoomController s in rooms[StoryModeController.story.GetWorldNumber()])
                            if (s.roomId == r.unlockRequirementID)
                            {
                                previousRoom = s;
                                break;
                            }
                        if (GetNumChallengeSatisfied(previousRoom) < 3)
                        {
                            r.gameObject.SetActive(false);
                            r.connector.gameObject.SetActive(false);
                            continue;
                        }
                    }
                }
                else
                {
                    r.gameObject.SetActive(false);
                    r.connector.gameObject.SetActive(false);
                    continue;
                }
            }

            //Change the room icon to the appropriate icon
            r.transform.GetChild(0).GetComponent<Image>().color = iconColor;
            switch (r.roomType)
            {
                case StoryRoomController.StoryRoomType.Combat:
                    r.transform.GetChild(0).GetComponent<Image>().sprite = enemyIcon;
                    break;
                case StoryRoomController.StoryRoomType.Boss:
                    r.transform.GetChild(0).GetComponent<Image>().sprite = bossIcon;
                    break;
                case StoryRoomController.StoryRoomType.Arena:
                    r.transform.GetChild(0).GetComponent<Image>().sprite = arenaIcon;
                    break;
                case StoryRoomController.StoryRoomType.NakedArena:
                    r.transform.GetChild(0).GetComponent<Image>().sprite = arenaIcon;
                    break;
                case StoryRoomController.StoryRoomType.Shop:
                    r.transform.GetChild(0).GetComponent<Image>().sprite = shopIcon;
                    break;
                case StoryRoomController.StoryRoomType.NewWorld:
                    if (StoryModeController.story.GetCompletedRooms().Contains(r.unlockRequirementID))
                        r.transform.GetChild(0).GetComponent<Image>().sprite = newWorldIcon;
                    break;
            }

            //Change the background color of the room depending on completion and achievement criteria
            if (StoryModeController.story.GetCompletedRooms().Contains(r.roomId))
            {
                if (GetNumChallengeSatisfied(r) == 3)
                {
                    if (r.roomType == StoryRoomController.StoryRoomType.Boss)
                        r.transform.GetChild(0).GetComponent<Image>().sprite = bossCrownIcon;
                    else
                        r.transform.GetChild(0).GetComponent<Image>().sprite = crownIcon;

                    if (StoryModeController.story.GetNumberOfChallengeItemsBought(r.roomId) == 3)
                        r.GetComponent<Image>().color = shopColor;
                    else
                        r.GetComponent<Image>().color = goldColor;
                }
                else
                {
                    if (r.startHidden)
                        r.GetComponent<Image>().color = hiddenOutlineColor;
                    else if (r.roomType == StoryRoomController.StoryRoomType.Arena || r.roomType == StoryRoomController.StoryRoomType.NakedArena)
                        r.GetComponent<Image>().color = arenaColor;
                    else
                        r.GetComponent<Image>().color = completedColor;
                    r.transform.GetChild(0).GetComponent<Image>().color = iconAltColor;
                }
                r.GetComponent<Collider2D>().enabled = true;
            }
            //Change the background color of the room that is unlocked but not completed
            else if (StoryModeController.story.GetCompletedRooms().Contains(r.unlockRequirementID) || (r.roomId == 1 && StoryModeController.story.GetWorldNumber() == 0) || (r.roomId == 201 && StoryModeController.story.GetWorldNumber() == 1))
            {
                if (r.roomType == StoryRoomController.StoryRoomType.Shop)
                    r.GetComponent<Image>().color = shopColor;
                else if (r.roomType == StoryRoomController.StoryRoomType.NewWorld || r.roomType == StoryRoomController.StoryRoomType.PreviousWorld)
                    r.GetComponent<Image>().color = newWorldColor;
                else if (r.roomType == StoryRoomController.StoryRoomType.Arena || r.roomType == StoryRoomController.StoryRoomType.NakedArena)
                    r.GetComponent<Image>().color = arenaColor;
                else
                {
                    if (r.startHidden)
                        r.GetComponent<Image>().color = hiddenOutlineColor;
                    else                            //Doesn't default select to hidden rooms
                    {
                        r.GetComponent<Image>().color = unlockedColor;
                        selectedRoom = r;       //Set the default select to be the latest unlocked room
                    }
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

        if (selectedRoom == null)
            selectedRoom = GetRoomWithID(maxID);

        enterButton.color = lockedColor;
        enterButton.GetComponent<Collider2D>().enabled = false;
    }

    IEnumerator HighlightRooms()
    {
        StoryRoomController lastSelectedRoom = GetRoomWithID(StoryModeController.story.GetLastSelectedRoomID());
        List<StoryRoomController> newlyUnlockedRooms = new List<StoryRoomController>();
        List<StoryRoomController> newly3AchievementsUnlockedRooms = new List<StoryRoomController>();

        foreach (StoryRoomController room in rooms[StoryModeController.story.GetWorldNumber()])
            if (room.unlockRequirementID == lastSelectedRoom.roomId && StoryModeController.story.GetCompletedRooms().Contains(lastSelectedRoom.roomId))
            {
                if (!room.unlockRequire3Stars && !StoryModeController.story.GetLastSelectedComplete())
                    newlyUnlockedRooms.Add(room);
                else if (room.unlockRequire3Stars && GetNumChallengeSatisfied(lastSelectedRoom) == 3)
                    newly3AchievementsUnlockedRooms.Add(room);
            }

        //Code for highlighting the room going from not complete to complete
        if (!StoryModeController.story.GetLastSelectedComplete() && StoryModeController.story.GetCompletedRooms().Contains(lastSelectedRoom.roomId))
            yield return StartCoroutine(HighlightSpecificRoom(lastSelectedRoom, lockedColor, 1f));
        //Code for highlingthing the new rooms being unlocked
        foreach (StoryRoomController room in newlyUnlockedRooms)
            yield return StartCoroutine(HighlightSpecificRoom(room, lockedColor, 3f));
        //Code for highlighting the rrom from complete to challenge satisfied
        if (StoryModeController.story.GetLastSelectedAchievents() != 3 && GetNumChallengeSatisfied(lastSelectedRoom) == 3)
            yield return StartCoroutine(HighlightSpecificRoom(lastSelectedRoom, unlockedColor, 1f));
        //Code for highlighting the new rooms from being unlocked
        foreach (StoryRoomController room in newly3AchievementsUnlockedRooms)
            yield return StartCoroutine(HighlightSpecificRoom(room, new Color(hiddenOutlineColor.r, hiddenOutlineColor.g, hiddenOutlineColor.b, 0), 3f));
    }

    private IEnumerator HighlightSpecificRoom(StoryRoomController room, Color startingColor, float sizeMultiplier)
    {
        Image originalImage = room.GetComponent<Image>();
        Vector2 originalScale = originalImage.transform.localScale;
        Color originalColor = originalImage.color;
        for (int i = 0; i < 10; i++)
        {
            originalImage.transform.localScale = Vector2.Lerp(originalScale * sizeMultiplier, originalScale, i / 9f);
            originalImage.color = Color.Lerp(startingColor, originalColor, i / 9f);
            yield return new WaitForSeconds(0.05f);
        }
        yield return new WaitForSeconds(0.3f);
    }

    public void ReportRoomSelected(int roomId)
    {
        foreach (StoryRoomController r in rooms[StoryModeController.story.GetWorldNumber()])
            if (r.roomId == roomId)
            {
                //If the selected room isn't viable, end this method call
                if (!StoryModeController.story.GetCompletedRooms().Contains(r.roomId) && !StoryModeController.story.GetCompletedRooms().Contains(r.unlockRequirementID) && roomId != 1)
                    return;

                selectedRoom.GetComponent<Outline>().enabled = false;
                selectedRoom = r;
                break;
            }

        if (selectedRoom.roomType == StoryRoomController.StoryRoomType.NewWorld)
        {
            SetWorldNumber(StoryModeController.story.GetWorldNumber() + 1);
        }
        else if (selectedRoom.roomType == StoryRoomController.StoryRoomType.PreviousWorld)
        {
            SetWorldNumber(StoryModeController.story.GetWorldNumber() - 1);
        }
        else if (selectedRoom.roomType == StoryRoomController.StoryRoomType.Shop)
        {
            SceneManager.LoadScene("StoryModeShopScene", LoadSceneMode.Single);
        }
        else if (selectedRoom.roomType == StoryRoomController.StoryRoomType.SecretShop)
        {
            StoryModeController.story.SetMenuBar(false);
            SceneManager.LoadScene("StoryModeSecretShopScene", LoadSceneMode.Single);
        }

        selectedRoom.GetComponent<Outline>().effectColor = outlineColor;
        selectedRoom.GetComponent<Outline>().enabled = true;

        flavorTextbox.text = selectedRoom.setup.flavorText;

        StoryModeController.story.SetCurrentRoomSetup(selectedRoom.setup);

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

        for (int i = 0; i < 5; i++)
        {
            if (selectedRoom.setup.rewardTypes[i] == StoryModeController.RewardsType.SpecificCard)
                itemsList[i].text = selectedRoom.setup.rewardCards[i].name + " x" + selectedRoom.setup.rewardAmounts[i];
            else if (selectedRoom.setup.rewardTypes[i] == StoryModeController.RewardsType.SpecificEquipment)
                itemsList[i].text = selectedRoom.setup.rewardEquipment[i].equipmentName + " x" + selectedRoom.setup.rewardAmounts[i];
            else
                itemsList[i].text = selectedRoom.setup.rewardTypes[i] + " x" + selectedRoom.setup.rewardAmounts[i];
            itemsIcons[i].sprite = StoryModeController.story.GetRewardSprite(selectedRoom.setup.rewardTypes[i], i);
            itemsIcons[i].color = StoryModeController.story.GetRewardsColor(selectedRoom.setup.rewardTypes[i]);

            if (i < 3 && StoryModeController.story.GetChallengeItemsBought().ContainsKey(roomId))
                itemsSoldOutIcons[i].gameObject.SetActive(StoryModeController.story.GetChallengeItemsBought()[roomId][i]);
            else
                itemsSoldOutIcons[i].gameObject.SetActive(false);
        }

        enterButton.color = unlockedColor;
        enterButton.GetComponent<Collider2D>().enabled = true;
    }

    public void StarsViewSelected()
    {
        stars.gameObject.SetActive(true);
        items.gameObject.SetActive(false);

        starsButton.color = goldColor;
        itemsButton.color = shopColor;
    }

    public void ItemsViewSelected()
    {
        items.gameObject.SetActive(true);
        stars.gameObject.SetActive(false);

        starsButton.color = shopColor;
        itemsButton.color = goldColor;
    }

    private bool ChallengeSatisfied(StoryRoomController setup, int index)
    {
        if (StoryModeController.story.GetChallengeValues().ContainsKey(setup.roomId))
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

    public StoryRoomController GetRoomWithID(int id)
    {
        for (int i = 0; i < worlds.Length; i++)
            foreach (StoryRoomController room in rooms[i])
                if (room.roomId == id)
                    return room;
        return null;
    }

    public void SettingsButton()
    {
        StoryModeController.story.SetMenuBar(false);
        SceneManager.LoadScene("SettingsScene", LoadSceneMode.Single);
    }

    public void EnterRoom()
    {
        StoryModeController.story.SetLastSelectedRoomID(selectedRoom.roomId);
        StoryModeController.story.SetLastSelectedAchievemnts(GetNumChallengeSatisfied(selectedRoom));
        StoryModeController.story.SetLastSelectedComplete(StoryModeController.story.GetCompletedRooms().Contains(selectedRoom.roomId));

        InformationLogger.infoLogger.SaveStoryModeGame();

        StoryModeController.story.SetCurrentRoomID(selectedRoom.roomId);
        StoryModeController.story.SetCurrentRoomSetup(selectedRoom.setup);
        StoryModeController.story.SetCurrentRoomType(selectedRoom.roomType);

        if (selectedRoom.roomType == StoryRoomController.StoryRoomType.Combat || selectedRoom.roomType == StoryRoomController.StoryRoomType.Boss)
        {
            SetCollection();
            SceneManager.LoadScene("OverworldScene", LoadSceneMode.Single);
        }
        else if (selectedRoom.roomType == StoryRoomController.StoryRoomType.Arena)
        {
            SetCollection();
            ScoreController.score.timerPaused = false;
            SceneManager.LoadScene("OverworldScene", LoadSceneMode.Single);
        }

        StoryModeController.story.SetMenuBar(false);
        StoryModeController.story.SetCombatInfoMenu(true);
        StoryModeController.story.RefreshAchievementValues();
        StoryModeController.story.RefreshGoldValue();
    }

    private void SetCollection()
    {
        if (selectedRoom.setup.useDefaultCardsAndEquipments)
            CollectionController.collectionController.SetStoryModeDefaultDeckAsSelectedDeck();
        else
            CollectionController.collectionController.SetInCombatDecks();       //Set the complete and selected decks before the combat starts

        if (StoryModeController.story.GetChallengeValues().ContainsKey(selectedRoom.roomId))
            selectedRoom.setup.SetBestValues(StoryModeController.story.GetChallengeValues()[selectedRoom.roomId]);
        else
            selectedRoom.setup.SetBestValues(new int[3] { -1, -1, -1 });
        LootController.loot.ResetPartyLootTable();
        CollectionController.collectionController.FinalizeDeck();
    }

    private void SetWorldNumber(int value)
    {
        if (Mathf.Clamp(value, 0, worlds.Length - 1) == value)
        {
            StoryModeController.story.SetWorldNumber(value);
            for (int i = 0; i < worlds.Length; i++)
                worlds[i].SetActive(i == value);

            RefreshWorldRooms();

            ReportRoomSelected(selectedRoom.roomId);
        }
    }

    public int GetTotalChallengeTokens()
    {
        int output = 0;
        foreach (StoryRoomController r in rooms[StoryModeController.story.GetWorldNumber()])
            if (StoryModeController.story.GetCompletedRooms().Contains(r.roomId))
                output += GetNumChallengeSatisfied(r);
        return output;
    }
}
