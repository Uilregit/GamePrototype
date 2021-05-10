using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoryModeController : MonoBehaviour
{
    public static StoryModeController story;

    private StoryRoomSetup setup;
    private List<int> completedIds = new List<int>();
    private Dictionary<int, int[]> challengeValues = new Dictionary<int, int[]>();
    private int currentRoomId = -1;
    private StoryRoomSetup currentRoomSetup;

    private List<string> cardCraftable = new List<string>();
    private Dictionary<string, int> cardUnlocked = new Dictionary<string, int>();
    private List<string> selectedcards = new List<string>();
    
    // Start is called before the first frame update
    void Awake()
    {
        if (StoryModeController.story == null)
            StoryModeController.story = this;
        else
            Destroy(this.gameObject);

        DontDestroyOnLoad(this.gameObject);

        completedIds = new List<int>();
        //cardUnlocked = CollectionController.collectionController.GetCompleteDeckDict();
        //cardCraftable = new List<string>(CollectionController.collectionController.GetCompleteDeckDict().Keys);

        if (!InformationLogger.infoLogger.GetHasStoryModeSaveFile())
        {
            cardCraftable = new List<string>(CollectionController.collectionController.GetSelectedDeckDict().Keys);
            cardUnlocked = CollectionController.collectionController.GetSelectedDeckDict();

            foreach (string key in cardUnlocked.Keys)
                for (int i = 0; i < cardUnlocked[key]; i++)
                    selectedcards.Add(key);

            if (!InformationLogger.infoLogger.debug)
                InformationLogger.infoLogger.SaveStoryModeGame();
        }
        else
            InformationLogger.infoLogger.LoadStoryModeGame();

        CollectionController.collectionController.SetSelectedDeck(selectedcards.ToArray());
        List<string> completeDeck = new List<string>();
        foreach (string name in cardUnlocked.Keys)
            for (int i = 0; i < cardUnlocked[name]; i++)
                completeDeck.Add(name);
        CollectionController.collectionController.SetCompleteDeck(completeDeck.ToArray());
        CollectionController.collectionController.SetupStoryModeDeck();
    }

    public void SetSetup(StoryRoomSetup value)
    {
        setup = value;
    }

    public StoryRoomSetup GetSetup()
    {
        return setup;
    }

    public void ReportRoomCompleted()
    {
        if (!completedIds.Contains(currentRoomId))
            completedIds.Add(currentRoomId);

        challengeValues[currentRoomId] = new int[3];
        for (int i = 0; i < 3; i++)
            challengeValues[currentRoomId][i] = AchievementSystem.achieve.GetChallengeValue(setup.challenges[i]);

        InformationLogger.infoLogger.SaveStoryModeGame();
    }

    public List<int> GetCompletedRooms()
    {
        return completedIds;
    }

    public void SetCompletedRooms(List<int> value)
    {
        completedIds = value;
    }

    public void SetChallengeValues(Dictionary<int, int[]> value)
    {
        challengeValues = value;
    }

    public Dictionary<int, int[]> GetChallengeValues()
    {
        return challengeValues;
    }

    public void SetCurrentRoomID(int value)
    {
        currentRoomId = value;
    }

    public int GetCurrentRoomID()
    {
        return currentRoomId;
    }

    public void SetCurrentRoomSetup(StoryRoomSetup setup)
    {
        currentRoomSetup = setup;
    }

    public StoryRoomSetup GetCurrentRoomSetup()
    {
        return currentRoomSetup;
    }

    public void ReportNewCraftableCard(string name)
    {
        if (!cardCraftable.Contains(name))
            cardCraftable.Add(name);
    }

    public void SetCardCraftable(List<string> value)
    {
        cardCraftable = value;
    }

    public List<string> GetCardCraftable()
    {
        return cardCraftable;
    }

    public void SetCardUnlocked(Dictionary<string, int> value)
    {
        cardUnlocked = value;
    }

    public Dictionary<string, int> GetCardUnlocked()
    {
        return cardUnlocked;
    }

    public void SetCardSelected(List<string> value)
    {
        selectedcards = value;
    }

    public List<string> GetCardSelected()
    {
        return selectedcards;
    }

    public bool ChallengeSatisfied(int index)
    {
        switch (currentRoomSetup.challengeComparisonType[index])
        {
            case StoryRoomSetup.ChallengeComparisonType.GreaterThan:
                return StoryModeController.story.GetChallengeValues()[currentRoomId][index] >= currentRoomSetup.challengeValues[index];
            case StoryRoomSetup.ChallengeComparisonType.EqualTo:
                return StoryModeController.story.GetChallengeValues()[currentRoomId][index] == currentRoomSetup.challengeValues[index];
            case StoryRoomSetup.ChallengeComparisonType.LessThan:
                return StoryModeController.story.GetChallengeValues()[currentRoomId][index] <= currentRoomSetup.challengeValues[index];
        }
        return false;
    }
}
