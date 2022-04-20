using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StoryModeEndSceenController : MonoBehaviour
{
    [Header("End Scene")]
    public Text goldText;
    public Text exitButton;
    public Text disabledExitButtonText;

    public StoryModeEndItemController[] items;

    [Header("Level Up Scene")]
    public Text sceneTitle;
    public EXPBarController expBar;
    public Text returnToMapButtonText;
    public CardDisplay[] cards;
    public GameObject cardContainer;
    public GameObject cardPack;
    public Image cardPackFlash;
    public Text cardPackNumber;
    public Image confirmPackButton;

    public Image[] ratingStars;
    public InputField comments;
    public Color unselectedStarsColor;
    public Color selectedStarsColor;

    private int unopenedCardPacks = 0;
    private bool expGainDone = false;

    private int maxGold = 0;
    private int totalGold = 0;

    private Dictionary<StoryModeController.RewardsType, int> boughtItems = new Dictionary<StoryModeController.RewardsType, int>();
    private Dictionary<Card, int> boughtCards = new Dictionary<Card, int>();
    private Dictionary<Equipment, int> boughtEquipiments = new Dictionary<Equipment, int>();
    private bool[] challengeItemsBought = new bool[3] { false, false, false };

    private Vector2 offset = Vector2.zero;
    private Vector2 newLocation;
    private List<bool> cardsFlipped = new List<bool>();

    private int rating = -1;

    private void Awake()
    {
        StoryRoomSetup setup = StoryModeController.story.GetCurrentRoomSetup();
        if (setup.overrideColors != null && setup.overrideColors.Length == 3)
            PartyController.party.SetOverrideParty(false);

        StoryModeController.story.SetAbandonButton(false);

        //Skip final rewards. Used by tutorials
        if (setup.skipFinalRewards)
            BuyAndExit();
    }

    private void Update()
    {
        if (expGainDone && unopenedCardPacks == 0)
            returnToMapButtonText.transform.parent.gameObject.SetActive(true);
    }

    // Start is called before the first frame update
    public virtual void Start()
    {
        totalGold = ResourceController.resource.GetGold();
        maxGold = totalGold;

        StoryRoomSetup setup = StoryModeController.story.GetCurrentRoomSetup();

        if (StoryModeController.story.GetChallengeItemsBought().ContainsKey(StoryModeController.story.GetCurrentRoomID()))
            challengeItemsBought = StoryModeController.story.GetChallengeItemsBought()[StoryModeController.story.GetCurrentRoomID()];
        else
            challengeItemsBought = new bool[3] { false, false, false };

        for (int i = 0; i < 3; i++)
        {
            items[i].SetValues(setup.rewardTypes[i], setup.rewardAmounts[i], setup.rewardCosts[i], i);
            if (RoomController.roomController.GetRoomJustWon())
            {
                items[i].SetEnabled(StoryModeController.story.ChallengeSatisfied(i) && (!challengeItemsBought[i] || setup.allowRewardsRebuy) && totalGold >= setup.rewardCosts[i]);
                if (!StoryModeController.story.ChallengeSatisfied(i))
                    TutorialController.tutorial.TriggerTutorial(Dialogue.Condition.StoryModeEndItemLocked, 1);
            }
            else
                items[i].SetEnabled(false);

            if (challengeItemsBought[i])
                TutorialController.tutorial.TriggerTutorial(Dialogue.Condition.StoryModeEndItemSoldOut, 1);
        }

        for (int i = 3; i < 5; i++)
        {
            items[i].SetValues(setup.rewardTypes[i], setup.rewardAmounts[i], setup.rewardCosts[i], i);
            items[i].SetEnabled(setup.rewardTypes.Length > i && totalGold >= setup.rewardCosts[i]);
        }

        TutorialController.tutorial.TriggerTutorial(Dialogue.Condition.FinalRewardsMenuShown, 1);

        ResetItemEnabled();
    }

    public virtual void ReportItemBought(int gold, StoryModeController.RewardsType name, int amount, bool bought, int index)
    {
        MusicController.music.PlaySFX(MusicController.music.uiUseLowSFX[Random.Range(0, MusicController.music.uiUseLowSFX.Count)]);

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
        else if (name == StoryModeController.RewardsType.UnlockClassicMode)
        {
            if (bought)
            {
                Unlocks unlock = UnlocksController.unlock.GetUnlocks();
                unlock.classicModeUnlocked = true;
                UnlocksController.unlock.SetUnlocks(unlock);
                InformationLogger.infoLogger.SaveUnlocks();
            }
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

        TutorialController.tutorial.TriggerTutorial(Dialogue.Condition.FinalRewardsMenuItemTaken, index + 1);

        ResetItemEnabled();
    }

    private void ResetItemEnabled()
    {
        StoryRoomSetup setup = StoryModeController.story.GetCurrentRoomSetup();

        for (int i = 0; i < 3; i++)
        {
            if (RoomController.roomController.GetRoomJustWon())
                items[i].SetGreyout(!(StoryModeController.story.ChallengeSatisfied(i) && totalGold >= setup.rewardCosts[i]));
            else
                items[i].SetGreyout(true);
            if (challengeItemsBought[i] && !items[i].GetSelected() && !setup.allowRewardsRebuy)
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
        if (RoomController.roomController.GetRoomJustWon())
            exitButton.text = "Exit with " + Mathf.RoundToInt(Mathf.Max(1, RoomController.roomController.GetNumberofWorldLayers()) * 10 * (0.5f + Mathf.Max(1, StoryModeController.story.GetWorldNumber()) * 0.5f)) + "+" + Mathf.RoundToInt(totalGold / 10f) + "xp";
        else
            exitButton.text = "Exit with " + Mathf.RoundToInt(totalGold / 10f) + "xp";
        disabledExitButtonText.text = exitButton.text;
    }

    public int GetCurrentGold()
    {
        return totalGold;
    }

    public void GoToLevelScene()
    {
        CameraController.camera.transform.position = new Vector2(-100, 0);
        returnToMapButtonText.transform.parent.gameObject.SetActive(false);
        StartCoroutine(StartLevelScene());
    }

    private IEnumerator StartLevelScene()
    {
        if (boughtItems.ContainsKey(StoryModeController.RewardsType.CardPack))
        {
            for (int i = 0; i < boughtItems[StoryModeController.RewardsType.CardPack]; i++)
                ReportLevelUp();
            yield return new WaitForSeconds(1);
        }
        int level = ScoreController.score.teamLevel;
        int numerator = ScoreController.score.currentEXP;

        expBar.SetValues(level, numerator, true, Color.black, Card.CasterColor.Enemy);
        expBar.SetStoryModeEndSceneController(this);
        expBar.SetEnabled(true);
        if (RoomController.roomController.GetRoomJustWon())
            yield return expBar.StartCoroutine(expBar.GainEXP(Mathf.RoundToInt(Mathf.Max(1, RoomController.roomController.GetNumberofWorldLayers()) * 10 * (0.5f + Mathf.Max(1, StoryModeController.story.GetWorldNumber()) * 0.5f)) + Mathf.RoundToInt(totalGold / 10f)));
        else
            yield return expBar.StartCoroutine(expBar.GainEXP(Mathf.RoundToInt(totalGold / 10f)));

        expGainDone = true;
    }

    public void OpenPack()
    {
        Random.InitState(InformationLogger.infoLogger.GetSecondSeed());
        for (int i = 0; i < cards.Length; i++)
        {
            if (Random.Range(0f, 1f) < 0.2)                                                 //~1 equipment per pack
            {
                Equipment e = LootController.loot.GetRandomEquipment();
                for (int j = 0; j < 100; j++)
                {
                    if (CollectionController.collectionController.GetCountOfEquipmentInCollection(e) < 4)        //Duplicate protect up to 4 cards
                        break;
                    else
                        e = LootController.loot.GetRandomEquipment();
                }
                cards[i].SetEquipment(e, Card.CasterColor.Enemy);
                if (boughtEquipiments.ContainsKey(e))
                    boughtEquipiments[e]++;
                else
                    boughtEquipiments.Add(e, 1);
            }
            else
            {
                Card c = LootController.loot.GetCard();
                for (int j = 0; j < 100; j++)
                {
                    if (CollectionController.collectionController.GetCountOfCardInCollection(c) < 4)        //Duplicate protect up to 4 cards
                        break;
                    else
                        c = LootController.loot.GetCard();
                }
                CardController cc = gameObject.AddComponent<CardController>();
                cc.SetCardDisplay(cards[i]);
                cc.SetCard(c, false, true, false);
                cards[i].SetCard(cc);
                if (boughtCards.ContainsKey(c))
                    boughtCards[c]++;
                else
                    boughtCards.Add(c, 1);
            }
            cards[i].Show();
            cards[i].PlaceFaceDown();
            cardsFlipped.Add(false);
        }
        confirmPackButton.gameObject.SetActive(false);
        cardContainer.SetActive(true);
        cards[0].FlipOver();
        cardsFlipped[0] = true;
    }

    public void PackConfirmed()
    {
        cardContainer.SetActive(false);
        confirmPackButton.gameObject.SetActive(false);
        unopenedCardPacks--;
        cardPackNumber.text = unopenedCardPacks.ToString();
        if (unopenedCardPacks <= 0)
            cardPack.SetActive(false);
        cardContainer.transform.localPosition = new Vector3(0, cardContainer.transform.localPosition.y, cardContainer.transform.localPosition.z);
        cardsFlipped = new List<bool>();
    }

    public void CardViewOnMouseDown()
    {
        offset = cardContainer.transform.localPosition - CameraController.camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
    }

    public void CardViewOnMouseUp()
    {
        offset = Vector3.zero;
        cardContainer.transform.localPosition = new Vector3(Mathf.Round(cardContainer.transform.localPosition.x / -2.8f) * -2.8f, cardContainer.transform.localPosition.y, 0);
        CheckAndFlipCards();
    }

    public void CardViewOnMouseDrag()
    {
        newLocation = offset + (Vector2)CameraController.camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
        cardContainer.transform.localPosition = new Vector3(Mathf.Clamp(newLocation.x, -11.2f, 0), cardContainer.transform.localPosition.y, 0);
        CheckAndFlipCards();
    }

    private void CheckAndFlipCards()
    {
        for (int i = 0; i < cardsFlipped.Count; i++)
            if (!cardsFlipped[i] && cardContainer.transform.localPosition.x / -2.8f + 0.1f >= i)
            {
                cards[i].FlipOver();
                cardsFlipped[i] = true;
            }
        if (cardsFlipped[cardsFlipped.Count - 1])
            confirmPackButton.gameObject.SetActive(true);
    }

    public void ReportRating(int value)
    {
        rating = value;

        for (int i = 0; i < ratingStars.Length; i++)
            if (i < value)
                ratingStars[i].color = selectedStarsColor;
            else
                ratingStars[i].color = unselectedStarsColor;

        exitButton.transform.parent.gameObject.SetActive(true);
    }

    public virtual void BuyAndExit()
    {
        if (!InformationLogger.infoLogger.debug)
        {
            InformationLogger.infoLogger.SaveSinglePlayerRoomInfo(InformationLogger.infoLogger.patchID,
                InformationLogger.infoLogger.gameID,
                RoomController.roomController.worldLevel.ToString(),
                RoomController.roomController.selectedLevel.ToString(),
                RoomController.roomController.roomName,
                RoomController.roomController.GetRoomJustWon().ToString(),
                maxGold.ToString(),
                totalGold.ToString(),
                ScoreController.score.GetOverKill().ToString(),
                ScoreController.score.GetDamage().ToString(),
                ScoreController.score.GetDamageArmored().ToString(),
                ScoreController.score.GetDamageOverhealProtected().ToString(),
                ScoreController.score.GetDamageAvoided().ToString(),
                ((int)ScoreController.score.GetSecondsInGame()).ToString(),
                "-1",
                AchievementSystem.achieve.GetChallengeValue(StoryModeController.story.GetCurrentRoomSetup().challenges[0]).ToString(),
                AchievementSystem.achieve.GetChallengeValue(StoryModeController.story.GetCurrentRoomSetup().challenges[1]).ToString(),
                AchievementSystem.achieve.GetChallengeValue(StoryModeController.story.GetCurrentRoomSetup().challenges[2]).ToString(),
                PartyController.party.GetPartyString(),
                "True",
                "False",
                rating.ToString(),
                "EndRoomFeedback",
                comments.text,
                TutorialController.tutorial.GetErrorLogs());
        }

        ScoreController.score.SetSecondsInGame(0);

        StoryModeController.story.ReportItemsBought(boughtItems);
        StoryModeController.story.ReportCardsBought(boughtCards);
        StoryModeController.story.ReportEquipmentBought(boughtEquipiments);
        StoryModeController.story.AddChallengeItemsBought(StoryModeController.story.GetCurrentRoomID(), challengeItemsBought);
        if (RoomController.roomController.GetRoomJustWon())
            StoryModeController.story.ReportColorsCompleted(StoryModeController.story.GetCurrentRoomID(), PartyController.party.GetPlayerCasterColors());

        StoryModeController.story.ReturnToMapScene();

        if (!UnlocksController.unlock.GetUnlocks().firstTutorialRoomCompleted)
        {
            UnlocksController.unlock.GetUnlocks().firstTutorialRoomCompleted = true;
            InformationLogger.infoLogger.SaveUnlocks();
        }

        TutorialController.tutorial.TriggerTutorial(Dialogue.Condition.FinalRewardsMenuExit, 1);

        if (boughtCards.Keys.Count > 0)
        {
            StoryModeController.story.EnableMenuIcon(3);
            TutorialController.tutorial.TriggerTutorial(Dialogue.Condition.FirstCardBought, 1);
        }
        if (boughtEquipiments.Keys.Count > 0)
        {
            StoryModeController.story.EnableMenuIcon(2);
            TutorialController.tutorial.TriggerTutorial(Dialogue.Condition.FirstEquipmentBought, 1);
        }
        if (boughtItems.ContainsKey(StoryModeController.RewardsType.TavernContract) && boughtItems[StoryModeController.RewardsType.TavernContract] > 0)
        {
            StoryModeController.story.EnableMenuIcon(1);
            TutorialController.tutorial.TriggerTutorial(Dialogue.Condition.NewCharUnlocked, 1);
        }
        if (boughtItems.ContainsKey(StoryModeController.RewardsType.CommonWildCard) && boughtItems[StoryModeController.RewardsType.CommonWildCard] > 0)
            TutorialController.tutorial.TriggerTutorial(Dialogue.Condition.WildCardUnlocked, 1);
        if (boughtItems.ContainsKey(StoryModeController.RewardsType.RareWildCard) && boughtItems[StoryModeController.RewardsType.RareWildCard] > 0)
            TutorialController.tutorial.TriggerTutorial(Dialogue.Condition.WildCardUnlocked, 1);
        if (boughtItems.ContainsKey(StoryModeController.RewardsType.LegendaryWildCard) && boughtItems[StoryModeController.RewardsType.LegendaryWildCard] > 0)
            TutorialController.tutorial.TriggerTutorial(Dialogue.Condition.WildCardUnlocked, 1);
    }

    public void ReportLevelUp()
    {
        unopenedCardPacks++;
        cardPack.SetActive(true);
        cardPackNumber.text = unopenedCardPacks.ToString();
        StartCoroutine(FlashPack());
    }

    private IEnumerator FlashPack()
    {
        cardPackFlash.enabled = true;
        for (int i = 0; i < 20; i++)
        {
            cardPackFlash.transform.localScale = Vector2.Lerp(new Vector2(0.9f, 0.9f), new Vector2(2, 2), i / 19f);
            cardPackFlash.color = Color.Lerp(Color.white, Color.clear, 1 / 19f);
            yield return new WaitForSeconds(0.3f / 20f);
        }
        cardPackFlash.enabled = false;
    }
}
