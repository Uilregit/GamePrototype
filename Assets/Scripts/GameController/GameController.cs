using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

public class GameController : MonoBehaviour
{
    public static GameController gameController;

    //public Sprite background;
    public Image damageOverlay;
    public Image circleHighlight;

    public Image characterDamagePreviewBackdrop;

    private bool showingDamageOverlay = false;

    public BackgroundMusicController music;
    public AudioClip combatMusic;
    public AudioClip bossMusic;
    public AudioClip victoryFanfair;
    public AudioClip bossDefeatMusic;

    public Image endTurnButton;
    public Color notYetDoneColor;
    public Color doneColor;
    public Image replaceIcon;
    public Text replaceCount;

    public CardDisplay[] rewardCards;
    public Image rewardCardRerolls;
    private int numOfRerolls = 2;
    private List<Card> pastRewards = new List<Card>();

    public NewAbilitiesMenu abilitiesMenu;

    [SerializeField]
    GameObject block;
    List<GameObject> blocks;

    [Header("(x1, x2, y1, y2)")]
    public int[] playerSpawnBox = new int[4]; //(x1,x2,y1,y2)
    public int[] enemySpawnBox = new int[4];

    [Header("Buttons")]
    public Image abandonRunButton;

    [Header("Buffs")]
    public Buff attackBuff;
    public Buff armorBuff;
    public Buff stunBuff;

    [Header("IntroSplash")]
    public CombatIntroSplashController splashController;

    [Header("Boss Intro Anims")]
    public Image bossIntroBackground;
    public SpriteRenderer bossIntroSprite;
    public SpriteRenderer bossIntroSpriteBack;
    public Image bossIntroFlash;
    public Image bossIntroNamePlate;
    public Text bossName;
    public Text bossTitle;

    [Header("Boss Death Anims")]
    public Image bossDeathBackground;
    public Image bossDeathFlash;
    public SpriteRenderer bossDeathSprite;
    public List<LineRenderer> bossDeathRays;

    private Sprite bossSprite;
    private string bossNameText;
    private string bossTitleText;
    private Color bossSpriteBackColor;

    private Vector3 cameraLocation;
    private int deckID;
    private RoomSetup setup;

    private List<Card.CasterColor> deadChars;
    private List<GameObject> deadPlayers = new List<GameObject>();

    //Stats
    private int totalOverkillGold;

    //[SerializeField]
    //private HealthController simulationCharacter;
    [SerializeField]
    private GameObject simulationPrefab;
    private Queue<HealthController> simulationCharacters;

    private int doomCounter = 5;

    private void Start()
    {
        if (GameController.gameController == null)
            GameController.gameController = this;
        else
            Destroy(this.gameObject);

        endTurnButton.GetComponent<Collider2D>().enabled = false;
        SetAbandonButton(StoryModeController.story.GetCanAbandon());

        deadChars = new List<Card.CasterColor>();
        simulationCharacters = new Queue<HealthController>();
        for (int i = 0; i < 49; i++)
        {
            GameObject obj = Instantiate(simulationPrefab);
            //obj.SetActive(false);
            obj.transform.position = new Vector2(100, 100);
            simulationCharacters.Enqueue(obj.GetComponent<HealthController>());
        }

        //Hide the reward cards
        for (int i = 0; i < rewardCards.Length; i++)
        {
            rewardCards[i].Hide();
            rewardCards[i].transform.parent.GetComponent<Collider2D>().enabled = false;
        }

        setup = RoomController.roomController.GetCurrentRoomSetup();
        TutorialController.tutorial.SetDialogue(setup.dialogues);
        TutorialController.tutorial.SetTutorialOverlays(setup.overlays);

        if (setup.GetAnyPlayerPositions().Count >= 3 && setup.GetAnyEnemyPositions().Count >= setup.enemies.Length ||                              //If level setup satisfies basic requiremnts, use level plan
            setup.GetAnyPlayerPositions().Count >= setup.overrideParty.Length && setup.GetAnyEnemyPositions().Count >= setup.enemies.Length)       //Or if it's a override, use those requirements instead
            InitializeRoom();
        else                                                                                                                                    //If level setup doesn't satisfy basic requirements, randomize
            RandomizeRoom();

        SetupDeckAndHand();

        InformationController.infoController.SaveCombatInformation();           //Save combat information at start of room as well in case char dies in the first room

        cameraLocation = new Vector3(0, 0, -10);

        SceneManager.sceneLoaded += OnLevelFinishedLoading;

        if (InformationLogger.infoLogger.debug)
            GridController.gridController.DebugGrid();

        RelicController.relic.OnNotify(this, Relic.NotificationType.OnCombatStart, null);

        foreach (UIRevealController.UIElement element in setup.hiddenUIElements)
            UIRevealController.UIReveal.SetElementState(element, false);

        if (setup.isBossRoom)
            music.music = bossMusic;
        else
            music.music = combatMusic;
        music.PlayMusic();

        TutorialController.tutorial.TriggerTutorial(Dialogue.Condition.StartOfRound, 1);

        StartCoroutine(StartOfGame());

        UpdatePlayerDamage();
    }

    private IEnumerator StartOfGame()
    {
        //If this is a boss room, start with the boss splash first
        if (bossSprite != null)
            yield return StartCoroutine(BossIntroProcess(bossSprite, bossNameText, bossTitleText, RoomController.roomController.GetCurrentWorldSetup().cameraBackground));

        //Splash the round number
        int round = RoomController.roomController.selectedLevel + 1;
        if (setup.isBossRoom)
            splashController.SetSplashImage(CombatIntroSplashController.icons.exclamation, "Boss Round", StoryModeController.story.GetCurrentRoomSetup().roomName.ToUpper(), CombatIntroSplashController.colors.Red);
        else
            splashController.SetSplashImage(CombatIntroSplashController.icons.exclamation, "Round " + round.ToString(), StoryModeController.story.GetCurrentRoomSetup().roomName.ToUpper(), CombatIntroSplashController.colors.Red);
        yield return splashController.AnimateSplashImage(9999);

        //Splash the optional goals for the room
        StoryRoomSetup currentStoryRoomSetup = StoryModeController.story.GetCurrentRoomSetup();
        int currentRoomId = StoryModeController.story.GetCurrentRoomID();
        for (int i = 0; i < 3; i++)
        {
            Color c = Color.cyan;
            int bestValue = currentStoryRoomSetup.GetBestValues(currentStoryRoomSetup.bestChallengeValues[i], AchievementSystem.achieve.GetChallengeValue(currentStoryRoomSetup.challenges[i]), i, currentStoryRoomSetup.challengeComparisonType[i]);
            bool goalSatisfied = StoryModeController.story.ChallengeSatisfied(i, bestValue);
            if (goalSatisfied)
            {
                if (StoryModeController.story.GetChallengeValues().ContainsKey(currentRoomId) && StoryModeController.story.GetChallengeItemsBought().ContainsKey(currentRoomId) && StoryModeController.story.GetChallengeItemsBought()[currentRoomId][i])
                    c = StoryModeController.story.GetShopColor();
                else
                    c = StoryModeController.story.GetGoldColor();
            }
            else
            {
                c = StoryModeController.story.GetCompletedColor();
                bestValue = AchievementSystem.achieve.GetChallengeValue(currentStoryRoomSetup.challenges[i]);
            }
            float progress = Mathf.Clamp((float)bestValue / currentStoryRoomSetup.challengeValues[i], 0f, 1f);
            if (currentStoryRoomSetup.challengeValues[i] == 0)
                progress = Mathf.Clamp(bestValue, 0, 1);
            splashController.SetGoalsImage(i, c, currentStoryRoomSetup.GetChallengeText(currentRoomId, i, bestValue, true, false), currentStoryRoomSetup.GetChallengeProgressText(currentRoomId, i, bestValue, true), progress, goalSatisfied, true);
        }

        if (setup.overrideSingleGoalsSplashIndex == -1)
            yield return splashController.AnimateGoalsImage();
        else
            yield return splashController.AnimateSingleGoalsImage(setup.overrideSingleGoalsSplashIndex);

        if (setup.drawHandCondition == Dialogue.Condition.None)
            yield return HandController.handController.StartCoroutine(HandController.handController.DrawFullHand());

        //Animate passive abilities of every enemy in the room
        yield return StartCoroutine(ShowAbilities());

        TileCreator.tileCreator.RefreshDangerArea();

        endTurnButton.GetComponent<Collider2D>().enabled = true;    //Doesn't allow the end turn button to be pressed during start of game animations

        ScoreController.score.SetTimerPaused(false);
        TutorialController.tutorial.TriggerTutorial(Dialogue.Condition.turn, 1);
    }

    public void SetupDeckAndHand()
    {
        if (setup.overrideCardShuffle)
            DeckController.deckController.PopulateDecks(setup.cardOrder);       //Populate decks with the specified order
        else
        {
            DeckController.deckController.PopulateDecks();                      //Populate decks and shuffle them
            DeckController.deckController.ShuffleDrawPile();
        }
        if (setup.lastRewardCardOnTop && CollectionController.collectionController.GetRecentRewardsCard() != "null")                     //Shuffle last reward card on top of draw pile if that setting is on
            DeckController.deckController.ShuffleCardOnTop(new List<string>() { CollectionController.collectionController.GetRecentRewardsCard() });
        DeckController.deckController.ResetCardValues();
        if (setup.overrideHandSize > 0)
            HandController.handController.SetBonusHandSize(setup.overrideHandSize - HandController.handController.startingHandSize, false);
        else
            HandController.handController.SetBonusHandSize(0, false);
        if (setup.overrideReplaces > 0)
            HandController.handController.SetBonusReplace(setup.overrideReplaces - HandController.handController.maxReplaceCount, false);
        else
            HandController.handController.SetBonusReplace(0, false);

        //Setup replace and hold areas
        if (HandController.handController.maxReplaceCount == 0)
        {
            GameObject.FindGameObjectWithTag("Replace").GetComponent<Collider>().enabled = false;
            GameObject.FindGameObjectWithTag("Replace").GetComponent<Image>().enabled = false;
            GameObject.FindGameObjectWithTag("Replace").transform.GetChild(0).GetComponent<Text>().enabled = false;
            GameObject.FindGameObjectWithTag("Replace").transform.GetChild(1).GetComponent<Text>().enabled = false;
        }
        if (!HandController.handController.allowHold)
        {
            GameObject.FindGameObjectWithTag("Hold").GetComponent<Collider>().enabled = false;
            GameObject.FindGameObjectWithTag("Hold").GetComponent<Image>().enabled = false;
            GameObject.FindGameObjectWithTag("Hold").transform.GetChild(0).GetComponent<Text>().enabled = false;
        }

        UIController.ui.ResetPileCounts(DeckController.deckController.GetDrawPileSize(), DeckController.deckController.GetDiscardPileSize());
    }

    public void FixedUpdate()
    {
        if (showingDamageOverlay)
            damageOverlay.color = new Color(1, 0, 0, 0.5f + MusicController.music.GetBackgroundAmplitude()[0] / 2);     //Flash damage overlay tied to the base of the combat music
    }

    private IEnumerator ShowAbilities()
    {
        yield return new WaitForSeconds(TimeController.time.attackBufferTime * TimeController.time.timerMultiplier);

        yield return StartCoroutine(DisplayAbilityCards());
    }

    public IEnumerator DisplayAbilityCards()
    {
        int numAbilities = 0;
        foreach (EnemyInformationController thisEnemy in TurnController.turnController.GetEnemies().Select(x => x.GetComponent<EnemyInformationController>()))
            if (!thisEnemy.GetHasShownAbilities() && thisEnemy.GetComponent<AbilitiesController>().GetAbilityCards().Count > 0)
            {
                numAbilities++;
                yield return StartCoroutine(thisEnemy.ShowAbilities());
            }
        if (numAbilities > 0)
            TutorialController.tutorial.TriggerTutorial(Dialogue.Condition.EnemyHasPassive, 1);
    }

    public void InitializeRoom()
    {
        if (setup.overrideSeed != -1)
            Random.InitState(setup.overrideSeed);

        GetComponent<SpriteRenderer>().sprite = RoomController.roomController.GetCombatBackground();

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        List<GameObject> enemies = new List<GameObject>();
        blocks = new List<GameObject>();
        List<Vector2> viableSpawnLocations = setup.GetLocations(RoomSetup.BoardType.P);

        //Spawn players first. If specified location, at location, if not, spawn randomly
        foreach (GameObject player in players)
        {
            if ((setup.overrideParty.Length != 0 && !setup.overrideParty.Contains(player.GetComponent<PlayerController>().GetColorTag())) ||
                (setup.overrideParty.Length == 0 && !PartyController.party.partyColors.Contains(player.GetComponent<PlayerController>().GetColorTag())))
            {
                DestroyImmediate(player.gameObject);
                continue;
            }
            Vector2 spawnedLocation = Vector2.zero;
            if (viableSpawnLocations.Count > 0)         //Prevent error if all player locations have been specified and is not random
                spawnedLocation = viableSpawnLocations[Random.Range(0, viableSpawnLocations.Count)];
            //If a more specific spawn location exists, spawn player there instead
            Vector2 specificSpawnLocation = GetSpawnLocation(true, PartyController.party.partyColors.ToList().IndexOf(player.GetComponent<PlayerController>().GetColorTag()));
            if (specificSpawnLocation != new Vector2(-100, -100))
                spawnedLocation = specificSpawnLocation;
            else
                viableSpawnLocations.Remove(spawnedLocation);
            player.GetComponent<PlayerController>().Spawn(spawnedLocation + new Vector2(-3, -2));

            // If the player was dead from before, remove them
            Card.CasterColor colorTag = player.GetComponent<PlayerController>().GetColorTag();

            if (InformationController.infoController.GetIfDead(colorTag))
            {
                Debug.Log(colorTag + " was dead");
                GridController.gridController.ReportPlayerDead(player, colorTag);
                GridController.gridController.RemoveFromPosition(player, player.transform.position);
                ReportDeadChar(colorTag, player);
                GridController.gridController.OnPlayerDeath(player, colorTag);
                //viableSpawnLocations.Add(spawnedLocation);
            }
        }
        //Then spawn enemies
        viableSpawnLocations = setup.GetLocations(RoomSetup.BoardType.E);
        int enemyCounter = 0;
        foreach (GameObject enemy in setup.enemies)
        {
            GameObject thisEnemy = Instantiate(enemy);
            Vector2 spawnedLocation = Vector2.zero;
            if (viableSpawnLocations.Count > 0)         //Prevent error if all enemy locations have been specified and is not random
                spawnedLocation = viableSpawnLocations[Random.Range(0, viableSpawnLocations.Count)];
            //If a more specific spawn location exists, spawn player there instead
            Vector2 specificSpawnLocation = GetSpawnLocation(false, enemyCounter);
            if (specificSpawnLocation != new Vector2(-100, -100))
                spawnedLocation = specificSpawnLocation;
            else
                viableSpawnLocations.Remove(spawnedLocation);
            thisEnemy.GetComponent<EnemyController>().Spawn(spawnedLocation + new Vector2(-3, -2));
            enemies.Add(thisEnemy);
            enemyCounter++;
        }

        //Lastly spawn blocks
        foreach (Vector2 loc in setup.GetLocations(RoomSetup.BoardType.W))
        {
            GameObject thisBlock = Instantiate(block, loc + new Vector2(-3, -2), Quaternion.identity);
            GridController.gridController.ReportPosition(thisBlock, loc + new Vector2(-3, -2));
            blocks.Add(thisBlock);
        }

        //Assign blocked pathing movement for tutorials/enemy fine tuning
        foreach (Vector2 loc in setup.GetLocations(RoomSetup.BoardType.B))
            GridController.gridController.SetPathBlocked(loc + new Vector2(-3, -2), true);

        foreach (GameObject enemy in enemies)
            enemy.GetComponent<EnemyController>().RefreshIntent();
    }

    private Vector2 GetSpawnLocation(bool isplayer, int index)
    {
        Vector2 output = new Vector2(-100, -100);
        List<Vector2> viableSpots = new List<Vector2>();
        if (isplayer)
        {
            switch (index)
            {
                case 0:
                    viableSpots = setup.GetLocations(RoomSetup.BoardType.P1);
                    break;
                case 1:
                    viableSpots = setup.GetLocations(RoomSetup.BoardType.P2);
                    break;
                case 2:
                    viableSpots = setup.GetLocations(RoomSetup.BoardType.P3);
                    break;
            }
        }
        else
        {
            switch (index)
            {
                case 0:
                    viableSpots = setup.GetLocations(RoomSetup.BoardType.E1);
                    break;
                case 1:
                    viableSpots = setup.GetLocations(RoomSetup.BoardType.E2);
                    break;
                case 2:
                    viableSpots = setup.GetLocations(RoomSetup.BoardType.E3);
                    break;
                case 3:
                    viableSpots = setup.GetLocations(RoomSetup.BoardType.E4);
                    break;
                case 4:
                    viableSpots = setup.GetLocations(RoomSetup.BoardType.E5);
                    break;
                case 5:
                    viableSpots = setup.GetLocations(RoomSetup.BoardType.E6);
                    break;
                case 6:
                    viableSpots = setup.GetLocations(RoomSetup.BoardType.E7);
                    break;
                case 7:
                    viableSpots = setup.GetLocations(RoomSetup.BoardType.E8);
                    break;
                case 8:
                    viableSpots = setup.GetLocations(RoomSetup.BoardType.E9);
                    break;
                case 9:
                    viableSpots = setup.GetLocations(RoomSetup.BoardType.E10);
                    break;
            }
        }
        if (viableSpots.Count > 0)
            output = viableSpots[0];
        return output;
    }

    public void RandomizeRoom()
    {
        GetComponent<SpriteRenderer>().sprite = RoomController.roomController.GetCombatBackground();

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        List<GameObject> enemies = new List<GameObject>();
        blocks = new List<GameObject>();
        int counter = 0;
        //Spawn players first. If specified location, at location, if not, spawn randomly
        foreach (GameObject player in players)
        {
            if (!PartyController.party.partyColors.Contains(player.GetComponent<PlayerController>().GetColorTag()))
            {
                Destroy(player.gameObject);
                continue;
            }
            if (setup.GetLocations(RoomSetup.BoardType.P).Count == 0)
                player.GetComponent<PlayerController>().Spawn();
            else
                player.GetComponent<PlayerController>().Spawn(setup.GetLocations(RoomSetup.BoardType.P)[counter]);

            // If the player was dead from before, remove them
            Card.CasterColor colorTag = player.GetComponent<PlayerController>().GetColorTag();
            if (InformationController.infoController.GetIfDead(colorTag))
            {
                GridController.gridController.ReportPlayerDead(player, colorTag);
                GridController.gridController.RemoveFromPosition(player, player.transform.position);
                ReportDeadChar(colorTag, player);
                GridController.gridController.OnPlayerDeath(player, colorTag);
            }
            counter += 1;
        }
        //Then spawn enemies
        foreach (GameObject enemy in setup.enemies)
        {
            GameObject thisEnemy = Instantiate(enemy);
            thisEnemy.GetComponent<EnemyController>().Spawn();
            enemies.Add(thisEnemy);
        }
        for (int i = 0; i < setup.blockNumber; i++)
        {
            int x, y;
            for (int j = 0; j < 100; j++)
            {
                int[] roomRange = GridController.gridController.GetRoomRange();
                x = Random.Range(roomRange[0], roomRange[1]);
                y = Random.Range(roomRange[2], roomRange[3]);
                if (GridController.gridController.GetObjectAtLocation(new Vector2(x, y)).Count == 0)
                {
                    GameObject thisBlock = Instantiate(block, new Vector2(x, y), Quaternion.identity);
                    GridController.gridController.ReportPosition(thisBlock, new Vector2(x, y));
                    blocks.Add(thisBlock);
                    break;
                }
            }
        }
        bool exit = false;
        for (int blockNumber = setup.blockNumber; blockNumber >= 0; blockNumber--)
        {
            for (int i = 0; i < 2; i++)
            {
                exit = true;
                foreach (GameObject player in players)
                {
                    if (deadChars.Contains(player.GetComponent<PlayerController>().GetColorTag()))
                        continue;
                    foreach (GameObject enemy in enemies)
                    {
                        EnemyController enemyController = enemy.GetComponent<EnemyController>();
                        List<Vector2> path = PathFindController.pathFinder.PathFind(enemy.transform.position, player.transform.position, new string[] { "Player", "Enemy" }, enemy.GetComponent<HealthController>().GetOccupiedSpaces(), enemy.GetComponent<HealthController>().size); //Must be enemy position first
                        if (Mathf.Abs((path[path.Count - 1] - (Vector2)player.transform.position).magnitude) > Mathf.Pow(2 * Mathf.Pow((0.5f * enemy.GetComponent<HealthController>().size - 0.5f), 2), 0.5f))
                            exit = false;
                    }
                }
                if (exit)
                    break;
                else
                    RearrangeBlocks(blockNumber);
            }
        }

        foreach (GameObject enemy in enemies)
            enemy.GetComponent<EnemyController>().RefreshIntent();
    }

    public void ResetRoom(bool playerTriggered = true)
    {
        if (playerTriggered && !TurnController.turnController.GetIsPlayerTurn())       //Never allow resetting of the room while it's still the enemy's turn
            return;

        TurnController.turnController.SetPlayerTurn(true);

        foreach (EnemyController enemy in TurnController.turnController.GetEnemies())
            Destroy(enemy.gameObject);
        TurnController.turnController.RemoveAllEnemies();

        foreach (GameObject block in blocks)
            Destroy(block);
        blocks = new List<GameObject>();

        GridController.gridController.ResetGrid();

        ResourceController.resource.ResetReviveUsed();
        TurnController.turnController.ResetManaAndEnergy();
        TurnController.turnController.ResetCardInfo();

        if (setup.GetLocations(RoomSetup.BoardType.P).Count >= 3 && setup.GetLocations(RoomSetup.BoardType.E).Count >= setup.enemies.Length ||                              //If level setup satisfies basic requiremnts, use level plan
        setup.GetLocations(RoomSetup.BoardType.P).Count >= setup.overrideParty.Length && setup.GetLocations(RoomSetup.BoardType.E).Count >= setup.enemies.Length)       //Or if it's a override, use those requirements instead
            InitializeRoom();
        else                                                                                                                                    //If level setup doesn't satisfy basic requirements, randomize
            RandomizeRoom();

        HandController.handController.EmptyHand();
        SetupDeckAndHand();

        RelicController.relic.OnNotify(this, Relic.NotificationType.OnCombatStart, null);

        foreach (UIRevealController.UIElement element in setup.hiddenUIElements)
            UIRevealController.UIReveal.SetElementState(element, false);

        UpdatePlayerDamage();

        SetReplaceDone(false);
        SetTurnButtonDone(false);

        StartCoroutine(ShowAbilities());

        TutorialController.tutorial.DestroyAndReset();
        TutorialController.tutorial.SetDialogue(setup.dialogues);
        TutorialController.tutorial.SetTutorialOverlays(setup.overlays);
        TutorialController.tutorial.TriggerTutorial(Dialogue.Condition.turn, 0);

        TutorialController.tutorial.TriggerTutorial(Dialogue.Condition.AfterRoomReset, 1);
    }

    private void RearrangeBlocks(int blockNumber)
    {
        Debug.Log("############ Rearranged Blocks ###########");

        foreach (GameObject block in blocks)
        {
            GridController.gridController.RemoveFromPosition(block, block.transform.position);
            Destroy(block.gameObject);
        }
        for (int i = 0; i < blockNumber; i++)
        {
            int x, y;
            for (int j = 0; j < 10; j++)
            {
                int[] roomRange = GridController.gridController.GetRoomRange();
                x = Random.Range(roomRange[0], roomRange[1]);
                y = Random.Range(roomRange[2], roomRange[3]);
                if (GridController.gridController.GetObjectAtLocation(new Vector2(x, y)).Count == 0)
                {
                    GameObject thisBlock = Instantiate(block, new Vector2(x, y), Quaternion.identity);
                    GridController.gridController.ReportPosition(thisBlock, new Vector2(x, y));
                    blocks.Add(thisBlock);
                    break;
                }
            }
        }
    }

    private IEnumerator DisplayVictoryText()
    {
        TurnController.turnController.ReportTurnBasedAchievements();
        TutorialController.tutorial.DestroyAndReset();
        InformationController.infoController.SaveCombatInformation();
        CameraController.camera.ScreenShake(0.06f, 0.05f);
        GameController.gameController.splashController.SetSplashImage(CombatIntroSplashController.icons.exclamation, "Victory", StoryModeController.story.GetCurrentRoomSetup().roomName.ToUpper(), CombatIntroSplashController.colors.Red);
        MusicController.music.SetLowPassFilter(false);
        MusicController.music.PlayBackground(victoryFanfair, false);
        yield return StartCoroutine(GameController.gameController.splashController.AnimateSplashImage(9999));

        //Splash the optional goals for the room
        StoryRoomSetup currentStoryRoomSetup = StoryModeController.story.GetCurrentRoomSetup();
        int currentRoomId = StoryModeController.story.GetCurrentRoomID();
        for (int i = 0; i < 3; i++)
        {
            Color c = Color.cyan;
            int bestValue = currentStoryRoomSetup.GetBestValues(currentStoryRoomSetup.bestChallengeValues[i], AchievementSystem.achieve.GetChallengeValue(currentStoryRoomSetup.challenges[i]), i, currentStoryRoomSetup.challengeComparisonType[i]);
            bool goalSatisfied = StoryModeController.story.ChallengeSatisfied(i, bestValue);
            if (goalSatisfied)
            {
                if (StoryModeController.story.GetChallengeValues().ContainsKey(currentRoomId) && StoryModeController.story.GetChallengeItemsBought().ContainsKey(currentRoomId) && StoryModeController.story.GetChallengeItemsBought()[currentRoomId][i])
                    c = StoryModeController.story.GetShopColor();
                else
                    c = StoryModeController.story.GetGoldColor();
            }
            else
            {
                c = StoryModeController.story.GetCompletedColor();
                bestValue = AchievementSystem.achieve.GetChallengeValue(currentStoryRoomSetup.challenges[i]);
            }
            float progress = Mathf.Clamp((float)bestValue / currentStoryRoomSetup.challengeValues[i], 0f, 1f);
            if (currentStoryRoomSetup.challengeValues[i] == 0)
                progress = Mathf.Clamp(bestValue, 0, 1);
            splashController.SetGoalsImage(i, c, currentStoryRoomSetup.GetChallengeText(currentRoomId, i, bestValue, true, false), currentStoryRoomSetup.GetChallengeProgressText(currentRoomId, i, bestValue, true), progress, goalSatisfied, false);
        }

        if (setup.overrideSingleGoalsSplashIndex == -1)
            yield return splashController.AnimateGoalsImage();
        else
            yield return splashController.AnimateSingleGoalsImage(setup.overrideSingleGoalsSplashIndex);
    }

    public IEnumerator Victory()
    {
        ScoreController.score.SetTimerPaused(true);

        GridController.gridController.DisableAllPlayers();
        //HandController.handController.ClearHand();
        HandController.handController.EmptyHand();

        DeckController.deckController.ResetCardValues();

        if (RoomController.roomController.GetCurrentRoomSetup().isBossRoom || RoomController.roomController.selectedLevel == RoomController.roomController.GetNumberofWorldLayers() || RoomController.roomController.GetNumberofWorldLayers() == 1)        //If the room is the boss room (classic) or the last room (story), full rez and heal all chars
        {
            AchievementSystem.achieve.OnNotify(TurnController.turnController.turnID, StoryRoomSetup.ChallengeType.DefeatBossInTurn);
            AchievementSystem.achieve.OnNotify((int)ScoreController.score.GetSecondsInGame(), StoryRoomSetup.ChallengeType.TotalTimeUsed);
            if (!TurnController.turnController.GetIsPlayerTurn())
                AchievementSystem.achieve.OnNotify(1, StoryRoomSetup.ChallengeType.DefeatBossOnEnemyTurn);
            if (PartyController.party.partyColors.Contains(Card.CasterColor.Red) && PartyController.party.partyColors.Contains(Card.CasterColor.Green) && PartyController.party.partyColors.Contains(Card.CasterColor.Blue))
                AchievementSystem.achieve.OnNotify(1, StoryRoomSetup.ChallengeType.UseOriginalTeam);

            ScoreController.score.UpdateBossesDefeated();

            if (bossSprite != null)
                yield return StartCoroutine(BossDeathProcess(bossSprite));
            else
            {
                CameraController.camera.ScreenShake(0.4f, 1f, true);
                yield return new WaitForSeconds(1.5f);
            }

            yield return StartCoroutine(RezAndHealAllPlayers(0.5f));

            yield return new WaitForSeconds(1f);
        }

        yield return StartCoroutine(DisplayVictoryText());

        if (RoomController.roomController.GetCurrentRoomSetup().isBossRoom)
        {
            if (RoomController.roomController.GetWorldLevel() == RoomController.roomController.GetNumberOfWorlds() - 1)
            {
                SceneManager.LoadScene("EndScene");
                yield break;
            }
        }

        if (setup.offerRewardGold)
        {
            int bonusPassiveGold = 0;
            if (StoryModeController.story != null && StoryModeController.story.GetItemsBought().ContainsKey(StoryModeController.RewardsType.PlusXGoldPerRoom))
                bonusPassiveGold = StoryModeController.story.GetItemsBought()[StoryModeController.RewardsType.PlusXGoldPerRoom];
            RewardsMenuController.rewardsMenu.AddReward(RewardsMenuController.RewardType.PassiveGold, null, ResourceController.resource.goldGainPerCombat + bonusPassiveGold);
            if (totalOverkillGold > 0)
                RewardsMenuController.rewardsMenu.AddReward(RewardsMenuController.RewardType.OverkillGold, null, totalOverkillGold);
        }
        if (setup.offerRewardCards)
        {
            if (!(InformationLogger.infoLogger.isStoryMode && RoomController.roomController.selectedLevel == StoryModeController.story.GetCurrentRoomSetup().setups.Count - 1))     //Don't give a card reward if it's the last room for storymode
                RewardsMenuController.rewardsMenu.AddReward(RewardsMenuController.RewardType.Card, null, 0);
            if (!(InformationLogger.infoLogger.isStoryMode && RoomController.roomController.GetCurrentRoomSetup().isBossRoom) && setup.relicReward)
                RewardsMenuController.rewardsMenu.AddReward(RewardsMenuController.RewardType.Relic, null, 0);
        }
        if (setup.offerRewardCards || setup.offerRewardGold)
        {
            RewardsMenuController.rewardsMenu.ShowMenu();
            TutorialController.tutorial.TriggerTutorial(Dialogue.Condition.RewardsMenuShown, 1);
        }
        else if (setup.endRoomUnlockCard != null || setup.endRoomUnlockChar != null || setup.endRoomUnlockAbility != null)
        {
            if (setup.endRoomUnlockAbility != null)
                abilitiesMenu.SetAbility(setup.endRoomUnlockAbility, setup.endRoomUnlockAbilityName);
            if (setup.endRoomUnlockCard != null)
                abilitiesMenu.SetCard(setup.endRoomUnlockCard);
            if (setup.endRoomUnlockChar != null)
                abilitiesMenu.SetCharacter(setup.endRoomUnlockChar);

            foreach (UIRevealController.UIElement element in setup.revealedUIElements)
                UIRevealController.UIReveal.SetElementState(element, true);

            abilitiesMenu.StartDisplaying();
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
            RewardsMenuController.rewardsMenu.ReportRewardTaken(RewardsMenuController.RewardType.BypassRewards);
        }

        if (setup.overrideParty.Length > 0)
            PartyController.party.SetOverrideParty(false);
    }

    public IEnumerator RezAndHealAllPlayers(float rezDelay)
    {
        HealthController player = null;
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Player"))     //At the end of all boss rooms, heal every player to full and resurrect all dead players for free
        {
            player = obj.GetComponent<HealthController>();
            if (player.GetIsSimulation())
                continue;
            player.SetCurrentAttack(InformationController.infoController.GetStartingAttack(player.GetComponent<PlayerController>().GetColorTag()));
            player.SetCurrentArmor(InformationController.infoController.GetStartingArmor(player.GetComponent<PlayerController>().GetColorTag()), false);
            player.SetCurrentVit(InformationController.infoController.GetMaxVit(player.GetComponent<PlayerController>().GetColorTag()));
            foreach (Card.CasterColor deadCharColor in deadChars)
            {
                if (obj.GetComponent<PlayerController>().GetColorTag() == deadCharColor)        //Resurrect all dead players for free
                {
                    player.transform.position = GridController.gridController.GetDeathLocation(deadCharColor);
                    player.GetComponent<HealthController>().charDisplay.transform.position = GridController.gridController.GetDeathLocation(deadCharColor);
                    player.GetComponent<HealthController>().ReportResurrect();
                    player.GetComponent<PlayerMoveController>().UpdateOrigin(player.transform.position);
                    player.GetComponent<PlayerMoveController>().ResetMoveDistance(0);
                    GridController.gridController.RemoveDeathLocation(deadCharColor);
                    GridController.gridController.ReportPosition(player.gameObject, player.transform.position);
                }
            }
            player.charDisplay.hitEffectAnim.SetTrigger("Heal");
            yield return new WaitForSeconds(rezDelay);
        }
        deadChars = new List<Card.CasterColor>();
    }

    public void FinishRoomAndExit(RewardsMenuController.RewardType type, int deckId)
    {
        if (InformationLogger.infoLogger.isStoryMode && RoomController.roomController.selectedLevel == StoryModeController.story.GetCurrentRoomSetup().setups.Count - 1 ||
                InformationLogger.infoLogger.isStoryMode && RoomController.roomController.GetCurrentRoomSetup().isBossRoom)        //If it's story mode's last room, go to end
        {
            AchievementSystem.achieve.OnNotify(1, StoryRoomSetup.ChallengeType.Complete);
            AchievementSystem.achieve.OnNotify(CollectionController.collectionController.GetNumberOfCardsNotStartedInDeck(), StoryRoomSetup.ChallengeType.AddCardsToDeck);
            StoryModeController.story.ReportRoomCompleted();
            RelicController.relic.ResetRelics();

            ScoreController.score.EnableTimerText(false);
            ScoreController.score.SetTimerPaused(true);
            RoomController.roomController.SetRoomJustWon(true);
            SceneManager.LoadScene("StoryModeEndScene");
        }
        else
        {
            if (RoomController.roomController.GetWorldLevel() != 2 && RoomController.roomController.GetCurrentRoomSetup().isBossRoom)                                           //eles go to overworld
                RoomController.roomController.LoadNewWorld(RoomController.roomController.GetWorldLevel() + 1);
            RoomController.roomController.SetViableRoom(new Vector2(-999, -999));
            RoomController.roomController.Refresh();
            InformationLogger.infoLogger.SaveGame(false);

            if (type == RewardsMenuController.RewardType.BypassRewards || !RoomController.roomController.GetCurrentRoomSetup().offerRewardCards)
                GameController.gameController.LoadScene("OverworldScene", false, deckId);
            else
            {
                MusicController.music.SetHighPassFilter(true);
                GameController.gameController.LoadScene("OverworldScene", true, deckId);
            }
        }
    }

    public void ReportOverkillGold(int value)
    {
        totalOverkillGold += value;
    }

    public int GetOverkillGold()
    {
        return totalOverkillGold;
    }

    public void RecordRewardCards(Card chosenCard)
    {
        for (int i = 0; i < rewardCards.Length; i++)
        {
            if (chosenCard.name != rewardCards[i].GetCard().name)
                InformationLogger.infoLogger.SaveRewardsCardInfo(InformationLogger.infoLogger.patchID,
                        InformationLogger.infoLogger.gameID,
                        RoomController.roomController.worldLevel.ToString(),
                        RoomController.roomController.selectedLevel.ToString(),
                        RoomController.roomController.roomName,
                        rewardCards[i].GetCard().GetCard().casterColor.ToString(),
                        rewardCards[i].GetCard().GetCard().name,
                        rewardCards[i].GetCard().GetCard().energyCost.ToString(),
                        rewardCards[i].GetCard().GetCard().manaCost.ToString(),
                        "False",
                        "False");
        }
    }

    public void RecordRewardEquipments(Equipment chosenEquipment)
    {
        for (int i = 0; i < rewardCards.Length; i++)
        {
            if (chosenEquipment.name != rewardCards[i].GetEquipment().name)
                InformationLogger.infoLogger.SaveRewardsCardInfo(InformationLogger.infoLogger.patchID,
                        InformationLogger.infoLogger.gameID,
                        RoomController.roomController.worldLevel.ToString(),
                        RoomController.roomController.selectedLevel.ToString(),
                        RoomController.roomController.roomName,
                        "Equipment",
                        rewardCards[i].GetEquipment().name,
                        "0",
                        "0",
                        "False",
                        "False");
        }
    }

    public void LoadScene(string sceneName, bool goToDeck, int newDeckID)
    {
        RoomController.roomController.Refresh();
        RoomController.roomController.Show();
        if (goToDeck)
            cameraLocation = new Vector3(100, 0, -10);
        else
            cameraLocation = new Vector3(0, 0, -10);
        deckID = newDeckID;
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    //Used by button from combat room's end screen
    public void LoadEndScene()
    {
        RoomController.roomController.SetRoomJustWon(false);
        if (StoryModeController.story != null)
            SceneManager.LoadScene("StoryModeEndScene");
        else
            SceneManager.LoadScene("EndScene");
    }

    private void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        CollectionController.collectionController.SetDeck(deckID);
        if (SceneManager.GetActiveScene().name != "StoryModeScene")
            CollectionController.collectionController.SetPage(CollectionController.collectionController.GetPageOfCard(CollectionController.collectionController.GetRecentRewardsCardCard()), true);
        if (SceneManager.GetActiveScene().name == "OverworldScene")
        {
            CameraController.camera.transform.position = cameraLocation;
            SceneManager.sceneLoaded -= OnLevelFinishedLoading;
        }
    }

    public void ReportDeadChar(Card.CasterColor color, GameObject player)
    {
        deadChars.Add(color);
        deadPlayers.Add(player);

        if (deadChars.Count >= 3)
            Defeat();
    }

    private void Defeat()
    {
        StartCoroutine(RezAndHealAllPlayers(0f));
        TurnController.turnController.StopAllCoroutines();
        RoomController.roomController.SetRoomJustWon(false);
        CanvasController.canvasController.uiCanvas.enabled = false;
        HandController.handController.EmptyHand();
        ScoreController.score.SetTimerPaused(true);
        SetAbandonButton(false);
        CanvasController.canvasController.endGameCanvas.enabled = true;
        CanvasController.canvasController.endGameCanvas.GetComponent<CanvasScaler>().enabled = false;
        CanvasController.canvasController.endGameCanvas.GetComponent<CanvasScaler>().enabled = true;
        CanvasController.canvasController.endGameCanvas.transform.GetChild(2).GetComponent<Collider2D>().enabled = true;
        MusicController.music.SetLowPassFilter(false);
        MusicController.music.SetHighPassFilter(false);
    }

    public List<GameObject> GetLivingPlayers()
    {
        List<GameObject> players = GameObject.FindGameObjectsWithTag("Player").ToList();
        List<GameObject> output = new List<GameObject>();
        foreach (GameObject obj in players)
        {
            if (obj.GetComponent<HealthController>().GetIsSimulation())
                continue;
            if (!deadChars.Contains(obj.GetComponent<PlayerController>().GetColorTag()) && PartyController.party.partyColors.Contains(obj.GetComponent<PlayerController>().GetColorTag()))
                output.Add(obj);
        }
        return output;
    }

    public List<GameObject> GetDeadPlayers()
    {
        return deadPlayers;
    }

    public virtual void ReportResurrectedChar(Card.CasterColor color, GameObject player)
    {
        deadChars.Remove(color);
        deadPlayers.Remove(player);
    }

    public virtual List<Card.CasterColor> GetDeadChars()
    {
        return deadChars;
    }

    //Boss intro animations
    private IEnumerator BossIntroProcess(Sprite bossSprite, string name, string title, Color backgroundColor)
    {
        Time.timeScale = 0; //Pause game during boss intro

        bossIntroFlash.enabled = true;
        bossIntroFlash.color = Color.white;
        bossIntroSprite.enabled = true;
        bossIntroSpriteBack.enabled = true;
        bossName.enabled = false;
        bossTitle.enabled = false;
        bossIntroBackground.enabled = true;
        bossIntroNamePlate.enabled = true;
        bossIntroSprite.sprite = bossSprite;
        bossIntroSpriteBack.sprite = bossSprite;
        bossName.text = name;
        bossTitle.text = title;
        bossName.color = backgroundColor;
        bossIntroBackground.color = backgroundColor;
        bossIntroSpriteBack.color = bossSpriteBackColor;
        bossIntroBackground.gameObject.SetActive(true);

        for (int i = 0; i < 5; i++)
        {
            bossIntroFlash.color = Color.Lerp(new Color(1, 1, 1, 1), new Color(1, 1, 1, 0), i / 4f);
            yield return new WaitForSecondsRealtime(0.1f / 5);
        }

        yield return new WaitForSecondsRealtime(1f);

        bossName.color = new Color(bossName.color.r, bossName.color.g, bossName.color.b, 0);
        bossName.enabled = true;
        for (int i = 0; i < 5; i++)
        {
            bossName.color = Color.Lerp(new Color(bossName.color.r, bossName.color.g, bossName.color.b, 0), new Color(bossName.color.r, bossName.color.g, bossName.color.b, 1), i / 4f);
            yield return new WaitForSecondsRealtime(0.1f / 5);
        }

        yield return new WaitForSecondsRealtime(1f);

        bossTitle.color = new Color(bossTitle.color.r, bossTitle.color.g, bossTitle.color.b, 0);
        bossTitle.enabled = true;
        for (int i = 0; i < 5; i++)
        {
            bossTitle.color = Color.Lerp(new Color(bossTitle.color.r, bossTitle.color.g, bossTitle.color.b, 0), new Color(bossTitle.color.r, bossTitle.color.g, bossTitle.color.b, 1), i / 4f);
            yield return new WaitForSecondsRealtime(0.1f / 5);
        }

        yield return new WaitForSecondsRealtime(1f);

        for (int i = 0; i < 5; i++)
        {
            bossIntroFlash.color = Color.Lerp(new Color(1, 1, 1, 0), new Color(1, 1, 1, 1), i / 4f);
            yield return new WaitForSecondsRealtime(0.1f / 5);
        }

        bossIntroSprite.enabled = false;
        bossIntroSpriteBack.enabled = false;
        bossName.enabled = false;
        bossTitle.enabled = false;
        bossIntroBackground.enabled = false;
        bossIntroNamePlate.enabled = false;

        for (int i = 0; i < 5; i++)
        {
            bossIntroFlash.color = Color.Lerp(new Color(1, 1, 1, 1), new Color(1, 1, 1, 0), i / 4f);
            yield return new WaitForSecondsRealtime(0.1f / 5);
        }

        bossIntroBackground.gameObject.SetActive(false);
        Time.timeScale = 1;
    }

    /// 
    /// Boss Death Animations
    /// 
    private IEnumerator BossDeathProcess(Sprite bossSprite)
    {
        MusicController.music.SetLowPassFilter(false);
        MusicController.music.PlayBackground(bossDefeatMusic, false);
        //30 frames a second

        //2 frame fade in to whtie
        //5 frame hold
        //17 frame fade out to black
        //7 frame circle
        //16 frame hold
        //5 frame fade out flash
        //9 frame fade out flash
        //6 frame ray extend 1-6
        //10 frame fade out flash coincide with ray 3
        //20 frame fade out flash coincide with ray 5
        //10 frame white flash from center
        //20 frame hold on white
        //30 frame fade to black
        //60 frame noise scroll
        //10 frame fade back to normal

        //setting up all the starting conditions
        bossDeathSprite.sprite = bossSprite;
        foreach (LineRenderer line in bossDeathRays)
            line.enabled = false;
        bossDeathBackground.gameObject.SetActive(true);
        bossDeathBackground.enabled = true;
        bossDeathFlash.enabled = true;
        bossDeathSprite.enabled = true;

        //Fade from white
        for (int i = 0; i < 20; i++)
        {
            bossDeathFlash.color = Color.Lerp(new Color(1, 1, 1, 1), new Color(1, 1, 1, 0), i / 19f);
            yield return new WaitForSeconds(0.6f / 20f);
        }

        yield return new WaitForSeconds(0.6f);

        //Flash white twice
        yield return StartCoroutine(BossDeathFlashFadeOut(0.1f));
        yield return StartCoroutine(BossDeathFlashFadeOut(0.2f));

        //Setup the death rays
        foreach (LineRenderer line in bossDeathRays)
        {
            line.SetPositions(new Vector3[] { new Vector2(0, 1f), Random.insideUnitCircle.normalized * 15f });
            line.enabled = false;
        }

        //Shwo the death rays one by one
        for (int i = 0; i < 31; i++)
        {
            if (i % 6 == 0)
            {
                if (i / 6 < bossDeathRays.Count)
                    bossDeathRays[i / 6].enabled = true;
                if (i / 6 == 3)
                    StartCoroutine(BossDeathFlashFadeOut(0.2f));
                //if (i / 6 == 5)
                //    StartCoroutine(BossDeathFlashFadeOut(0.4f));
            }
            yield return new WaitForSeconds(1.3f / 31f);
        }

        bossDeathFlash.color = Color.white;
        //Flash white from center till it fills the screen
        for (int i = 0; i < 5; i++)
        {
            bossDeathFlash.transform.localScale = Vector3.Lerp(new Vector3(1f, 0f, 1f), new Vector3(1f, 1f, 1f), i / 4f);
            yield return new WaitForSeconds(0.1f / 5f);
        }

        //Hide all non background elements
        bossDeathSprite.enabled = false;
        foreach (LineRenderer line in bossDeathRays)
            line.enabled = false;

        yield return new WaitForSeconds(0.6f);

        //Fade back to black
        for (int i = 0; i < 20; i++)
        {
            bossDeathFlash.color = Color.Lerp(new Color(1, 1, 1, 1), new Color(1, 1, 1, 0), i / 19f);
            yield return new WaitForSeconds(1f / 20f);
        }

        yield return new WaitForSeconds(2f);

        //Fade back to normal
        for (int i = 0; i < 10; i++)
        {
            bossDeathBackground.color = Color.Lerp(new Color(0, 0, 0, 1), new Color(0, 0, 0, 0), i / 9f);
            yield return new WaitForSeconds(0.3f / 9f);
        }

        bossDeathBackground.gameObject.SetActive(false);
    }

    private IEnumerator BossDeathFlashFadeOut(float time)
    {
        for (int i = 0; i < 5; i++)
        {
            bossDeathFlash.color = Color.Lerp(new Color(1, 1, 1, 1), new Color(1, 1, 1, 0), i / 4f);
            yield return new WaitForSeconds(time / 5f);
        }
    }

    public void SetTurnButtonDone(bool state)
    {
        if (state)
        {
            endTurnButton.color = doneColor;
            endTurnButton.transform.GetChild(0).GetComponent<Image>().color = doneColor;
            endTurnButton.transform.GetChild(1).GetComponent<Image>().color = doneColor;
        }
        else
        {
            endTurnButton.color = notYetDoneColor;
            endTurnButton.transform.GetChild(0).GetComponent<Image>().color = notYetDoneColor;
            endTurnButton.transform.GetChild(1).GetComponent<Image>().color = notYetDoneColor;
        }
    }

    public void SetReplaceDone(bool state)
    {
        if (state)
        {
            replaceIcon.color = doneColor;
            replaceCount.color = doneColor;
        }
        else
        {
            replaceIcon.color = notYetDoneColor;
            replaceCount.color = notYetDoneColor;
        }
    }

    public void RollAndShowRewardsCards(bool isReroll)
    {
        int rerollsLeft = 0;
        if (StoryModeController.story != null)
            rerollsLeft = StoryModeController.story.GetRewardsRerollLeft();
        if (rerollsLeft <= 0 && isReroll)
            return;
        if (isReroll)
        {
            if (StoryModeController.story == null)
                rerollsLeft--;
            else
            {
                rerollsLeft--;
                StoryModeController.story.UseRewardsReroll(1);
            }
        }

        for (int i = 0; i < rewardCards.Length; i++)
        {
            for (int j = 0; j < 100; j++)
            {
                Card reward = LootController.loot.GetCard();
                if (!pastRewards.Contains(reward) && CollectionController.collectionController.GetCountOfCardInCollection(reward) < 4) //Ensures that all rewards are unique
                {
                    rewardCards[i].transform.parent.GetComponent<CardController>().SetCard(reward, false, true, true);
                    pastRewards.Add(reward);
                    break;
                }
            }
            rewardCards[i].Show();
            rewardCards[i].SetHighLight(true);
            rewardCards[i].GetComponent<LineRenderer>().enabled = false;
            rewardCards[i].transform.parent.gameObject.GetComponent<Collider2D>().enabled = true;
        }

        if (isReroll || rerollsLeft > 0)
        {
            rewardCardRerolls.transform.GetChild(0).GetComponent<Text>().text = "Reroll x" + rerollsLeft;
            rewardCardRerolls.gameObject.SetActive(rerollsLeft > 0);
        }
    }

    public void UpdatePlayerDamage()
    {
        SetIsShowingDamageOverlay(GetDeadPlayers().Count != 0 || GetLivingPlayers().Select(x => x.GetComponent<HealthController>()).Any(x => !x.GetIsSimulation() && (float)x.GetCurrentVit() / (float)x.GetMaxVit() <= 0.35f));
    }

    private void SetIsShowingDamageOverlay(bool state)
    {
        MusicController.music.SetLowPassFilter(state);
        showingDamageOverlay = state;
        if (!state)
            StartCoroutine(FadeDamageOverlay(1));
    }

    public void SetDamageOverlay(float remainingHealthPercentage)
    {
        damageOverlay.color = new Color(1, 0, 0, 1 - remainingHealthPercentage / 2);
        StartCoroutine(FadeDamageOverlay(remainingHealthPercentage / 2));
    }

    private IEnumerator FadeDamageOverlay(float remainingHealthPercentage)
    {
        float elapsedTime = 0;
        while (elapsedTime < 0.4f)
        {
            damageOverlay.color = Color.Lerp(new Color(1, 0, 0, 1 - remainingHealthPercentage), new Color(1, 0, 0, 0), elapsedTime / 0.4f);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        damageOverlay.color = new Color(1, 0, 0, 0);
    }

    public void SetCircleOverlay(bool value, Vector2 location)
    {
        circleHighlight.transform.position = location;
        if (value)
            StartCoroutine(FadeInCircleOverlay());
        else
            StartCoroutine(FadeOutCircleOverlay());
    }

    private IEnumerator FadeInCircleOverlay()
    {
        float elapsedTime = 0;
        while (elapsedTime < 0.1f)
        {
            circleHighlight.color = Color.Lerp(new Color(0, 0, 0, 0), new Color(0, 0, 0, 0.5f), elapsedTime / 0.1f);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator FadeOutCircleOverlay()
    {
        float elapsedTime = 0;
        while (elapsedTime < 0.1f)
        {
            circleHighlight.color = Color.Lerp(new Color(0, 0, 0, 0.5f), new Color(0, 0, 0, 0), elapsedTime / 0.1f);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    public void SetAbandonButton(bool state)
    {
        abandonRunButton.gameObject.SetActive(state);
    }

    public void SetAbandonWarningMenu(bool state)
    {
        StoryModeController.story.SetAbandonWarningMenu(state);
    }

    public HealthController GetSimulationCharacter(HealthController simulationTarget, bool SetSimOnTarget = true)
    {
        HealthController simulationCharacter = simulationCharacters.Dequeue();

        simulationCharacter.SetOriginalSimulationTarget(simulationTarget);

        foreach (BuffFactory buff in simulationCharacter.GetComponent<BuffController>().GetBuffs())
            buff.Revert(simulationCharacter);

        simulationCharacter.GetComponent<BuffController>().SetBuffs(simulationTarget.GetComponent<BuffController>().GetBuffs(), false);

        simulationCharacter.charDisplay.sprite.sprite = simulationTarget.charDisplay.sprite.sprite;
        simulationCharacter.gameObject.tag = simulationTarget.gameObject.tag;
        simulationCharacter.isPlayer = simulationTarget.isPlayer;
        if (simulationTarget.isPlayer)
            simulationCharacter.GetComponent<PlayerMoveController>().SetOriginalPosition(simulationTarget.GetComponent<PlayerMoveController>().GetOriginalPosition());
        simulationCharacter.SetMaxMoveRange(simulationTarget.GetMaxMoveRange());
        simulationCharacter.SetBonusMoveRange(simulationTarget.GetBonusMoveRange());
        simulationCharacter.SetCurrentMoveRange(simulationTarget.GetMoveRangeLeft(), false);
        simulationCharacter.SetBonusArmor(simulationTarget.GetBonusArmor(), null, false);
        simulationCharacter.SetBonusAttack(simulationTarget.GetBonusAttack(), false);
        simulationCharacter.SetBonusVit(simulationTarget.GetBonusVit(), false);
        simulationCharacter.SetCurrentArmor(simulationTarget.GetCurrentArmor(), false);
        simulationCharacter.SetCurrentAttack(simulationTarget.GetCurrentAttack());
        simulationCharacter.SetCurrentVit(simulationTarget.GetCurrentVit());
        simulationCharacter.SetMaxVit(simulationTarget.GetMaxVit());
        simulationCharacter.SetImmuneToEnergy(simulationTarget.GetImmuneToEnergy());
        simulationCharacter.SetImmuneToMana(simulationTarget.GetImmuneToMana());
        simulationCharacter.SetArmorDamageMultiplier(simulationTarget.GetArmorDamageMultiplier());
        simulationCharacter.SetVitDamageMultiplier(simulationTarget.GetVitDamageMultiplier());
        simulationCharacter.SetHealingMultiplier(simulationTarget.GetHealingMultiplier());

        //simulationCharacter.gameObject.tag = simulationTarget.gameObject.tag;
        simulationCharacter.gameObject.tag = "Simulation";

        simulationCharacter.transform.position = simulationTarget.transform.position;

        if (SetSimOnTarget)
            simulationTarget.SetSimCharacter(simulationCharacter);
        return simulationCharacter;
    }

    public void ReportSimulationFinished(HealthController simulationCharacter)
    {
        if (simulationCharacter.GetOriginalSimulationTarget() == null)
            return;

        simulationCharacter.transform.position = new Vector2(100, 100);
        simulationCharacter.ResetOriginalSimulationTarget();
        simulationCharacter.ResetDamageTakenAttempted();
        simulationCharacters.Enqueue(simulationCharacter);
    }

    /*
    public void ShowStackedCharacters(List<HealthController> targets)
    {
        Dictionary<Vector2, List<HealthController>> objLocations = new Dictionary<Vector2, List<HealthController>> { { targets[0].transform.position, targets } };

        bool flipped = false;
        int currentTargetId = 0;
        int maxTargetsPerPosition = targets.Count;

        //Block for displaying the simulation health information
        foreach (HealthController hlthController in targets)
        {
            Vector2 offset = new Vector2(0, 0.4f + currentTargetId * 1.5f);
            if (targets[0].transform.position.y > 2)
            {
                offset = new Vector2(0, -3.1f - currentTargetId * 1.5f);
                flipped = true;
            }

            StartCoroutine(hlthController.ShowDamagePreviewBar(0, hlthController.GetVit(), null, hlthController.charDisplay.sprite.sprite, offset));

            //hlthController.charDisplay.healthBar.SetPositionRaised(true);

            currentTargetId++;
        }

        UIController.ui.combatStats.SetStatusCharactersCount(0, targets.Count);

        //Block for moving the backdrop for when overlapping targets are being targeted
        Vector2 backdropOffset = new Vector2(0, 1f);
        if (flipped)
            backdropOffset = new Vector2(0, -1f);
        SetDamagePrevieBackdrop((Vector2)targets[0].transform.position + backdropOffset, 1, maxTargetsPerPosition, flipped);
    }

    public void HideStackedCharacters(List<HealthController> targets)
    {
        foreach (HealthController hlth in targets)
        {
            hlth.HideHealthBar();
            hlth.ResetHealthBar();
            hlth.charDisplay.healthBar.SetPositionRaised(false);
            hlth.charDisplay.healthBar.SetArmor(hlth.GetArmor());
        }
        UIController.ui.combatStats.SetStatusCharactersCount(0, 1);
        SetDamagePrevieBackdrop(new Vector2(999, 999), 1, 1, false);
    }
*/

    public void SetDamagePrevieBackdrop(Vector2 center, int charWidth, int charHeight, bool flipped)
    {
        characterDamagePreviewBackdrop.transform.position = center;
        characterDamagePreviewBackdrop.rectTransform.sizeDelta = new Vector2(1.5f * charWidth, 1.5f * charHeight);
        if (flipped)
        {
            characterDamagePreviewBackdrop.rectTransform.localScale = new Vector2(1, -1);
            characterDamagePreviewBackdrop.transform.GetChild(0).transform.localScale = new Vector2(0.45f, -0.45f);
        }
        else
        {
            characterDamagePreviewBackdrop.rectTransform.localScale = new Vector2(1, 1);
            characterDamagePreviewBackdrop.transform.GetChild(0).transform.localScale = new Vector2(0.45f, 0.45f);
        }

        if (center == new Vector2(999, 999))
        {
            foreach (GameObject obj in GameController.gameController.GetLivingPlayers())        //Hide all previous health bars
            {
                HealthController hlth = obj.GetComponent<HealthController>();
                hlth.charDisplay.healthBar.character.enabled = false;
                hlth.charDisplay.healthBar.ResetPosition();
            }
            foreach (EnemyController obj in TurnController.turnController.GetEnemies())
            {
                HealthController hlth = obj.GetComponent<HealthController>();
                hlth.charDisplay.healthBar.character.enabled = false;
                hlth.charDisplay.healthBar.ResetPosition();
            }
        }
    }

    public RoomSetup GetRoomSetup()
    {
        return setup;
    }

    public void SetBossInfo(Sprite value, string name, string title, Color spriteBackColor)
    {
        bossSprite = value;
        bossNameText = name;
        bossTitleText = title;
        bossSpriteBackColor = spriteBackColor;
    }

    public void ChangeDoomCounter(int value)
    {
        doomCounter += value;
    }

    public int GetDoomCounter()
    {
        return doomCounter;
    }
}
