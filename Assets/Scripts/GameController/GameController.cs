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

    public Text text;
    public CardDisplay[] rewardCards;

    [SerializeField]
    GameObject block;
    List<GameObject> blocks;

    [Header("(x1, x2, y1, y2)")]
    public int[] playerSpawnBox = new int[4]; //(x1,x2,y1,y2)
    public int[] enemySpawnBox = new int[4];

    [Header("Buffs")]
    public Buff attackBuff;
    public Buff armorBuff;
    public Buff stunBuff;

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

    private void Start()
    {
        if (GameController.gameController == null)
            GameController.gameController = this;
        else
            Destroy(this.gameObject);

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

        if (setup.GetLocations(RoomSetup.BoardType.P).Count >= 3 && setup.GetLocations(RoomSetup.BoardType.E).Count >= setup.enemies.Length)    //If level setup satisfies basic requiremnts, use level plan
            InitializeRoom();
        else                                                                                                                                    //If level setup doesn't satisfy basic requirements, randomize
            RandomizeRoom();

        DeckController.deckController.PopulateDecks();
        DeckController.deckController.ResetCardValues();
        DeckController.deckController.ShuffleDrawPile();
        HandController.handController.SetBonusHandSize(0, false);

        InformationController.infoController.SaveCombatInformation();           //Save combat information at start of room as well in case char dies in the first room

        cameraLocation = new Vector3(0, 0, -10);

        SceneManager.sceneLoaded += OnLevelFinishedLoading;

        if (InformationLogger.infoLogger.debug)
            GridController.gridController.DebugGrid();

        RelicController.relic.OnNotify(this, Relic.NotificationType.OnCombatStart, null);

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

        StartCoroutine(ShowAbilities());
    }

    private IEnumerator ShowAbilities()
    {
        yield return HandController.handController.StartCoroutine(HandController.handController.DrawFullHand());

        yield return new WaitForSeconds(TimeController.time.attackBufferTime * TimeController.time.timerMultiplier);

        foreach (EnemyController thisEnemy in TurnController.turnController.GetEnemies())
            yield return StartCoroutine(thisEnemy.GetComponent<EnemyInformationController>().ShowAbilities());
    }

    public void InitializeRoom()
    {
        GetComponent<SpriteRenderer>().sprite = RoomController.roomController.GetCombatBackground();

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        List<GameObject> enemies = new List<GameObject>();
        blocks = new List<GameObject>();
        List<Vector2> viableSpawnLocations = setup.GetLocations(RoomSetup.BoardType.P);
        //Spawn players first. If specified location, at location, if not, spawn randomly
        foreach (GameObject player in players)
        {
            if (!PartyController.party.partyColors.Contains(player.GetComponent<PlayerController>().GetColorTag()))
            {
                Destroy(player.gameObject);
                continue;
            }

            Vector2 spawnedLocation = viableSpawnLocations[Random.Range(0, viableSpawnLocations.Count)];
            player.GetComponent<PlayerController>().Spawn(spawnedLocation + new Vector2(-3, -2));
            viableSpawnLocations.Remove(spawnedLocation);

            // If the player was dead from before, remove them
            Card.CasterColor colorTag = player.GetComponent<PlayerController>().GetColorTag();
            if (InformationController.infoController.GetIfDead(colorTag))
            {
                GridController.gridController.RemoveFromPosition(player, player.transform.position);
                ReportDeadChar(colorTag, player);
                GridController.gridController.OnPlayerDeath(player, colorTag);
                viableSpawnLocations.Add(spawnedLocation);
            }
        }
        //Then spawn enemies
        viableSpawnLocations = setup.GetLocations(RoomSetup.BoardType.E);
        foreach (GameObject enemy in setup.enemies)
        {
            GameObject thisEnemy = Instantiate(enemy);
            Vector2 spawnedLocation = viableSpawnLocations[Random.Range(0, viableSpawnLocations.Count)];
            viableSpawnLocations.Remove(spawnedLocation);
            thisEnemy.GetComponent<EnemyController>().Spawn(spawnedLocation + new Vector2(-3, -2));
            enemies.Add(thisEnemy);
        }

        //Lastly spawn blocks
        foreach (Vector2 loc in setup.GetLocations(RoomSetup.BoardType.W))
        {
            GameObject thisBlock = Instantiate(block, loc + new Vector2(-3, -2), Quaternion.identity);
            GridController.gridController.ReportPosition(thisBlock, loc + new Vector2(-3, -2));
            blocks.Add(thisBlock);
        }

        foreach (GameObject enemy in enemies)
            enemy.GetComponent<EnemyController>().RefreshIntent();
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
        InformationController.infoController.SaveCombatInformation();
        CameraController.camera.ScreenShake(0.06f, 0.05f);
        text.text = "VICTORY";
        text.enabled = true;
        yield return new WaitForSeconds(TimeController.time.victoryTextDuration * TimeController.time.timerMultiplier);
        text.enabled = false;
    }

    public IEnumerator Victory()
    {
        GridController.gridController.DisableAllPlayers();
        //HandController.handController.ClearHand();
        HandController.handController.EmptyHand();

        DeckController.deckController.ResetCardValues();

        if (RoomController.roomController.GetCurrentRoomSetup().isBossRoom || RoomController.roomController.selectedLevel == RoomController.roomController.GetNumberofWorldLayers())        //If the room is the boss room (classic) or the last room (story), full rez and heal all chars
        {
            AchievementSystem.achieve.OnNotify(TurnController.turnController.turnID, StoryRoomSetup.ChallengeType.DefeatBossInTurn);
            AchievementSystem.achieve.OnNotify((int)ScoreController.score.GetSecondsInGame(), StoryRoomSetup.ChallengeType.TotalTimeUsed);
            if (!TurnController.turnController.GetIsPlayerTurn())
                AchievementSystem.achieve.OnNotify(1, StoryRoomSetup.ChallengeType.DefeatBossOnEnemyTurn);

            ScoreController.score.UpdateBossesDefeated();

            CameraController.camera.ScreenShake(0.4f, 2f, true);
            yield return new WaitForSeconds(2.5f);

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
                yield return new WaitForSeconds(0.5f);
            }
            deadChars = new List<Card.CasterColor>();
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

        int bonusPassiveGold = 0;
        if (StoryModeController.story != null && StoryModeController.story.GetItemsBought().ContainsKey(StoryModeController.RewardsType.PlusXGoldPerRoom))
            bonusPassiveGold = StoryModeController.story.GetItemsBought()[StoryModeController.RewardsType.PlusXGoldPerRoom];
        RewardsMenuController.rewardsMenu.AddReward(RewardsMenuController.RewardType.PassiveGold, null, ResourceController.resource.goldGainPerCombat + bonusPassiveGold);
        if (totalOverkillGold > 0)
            RewardsMenuController.rewardsMenu.AddReward(RewardsMenuController.RewardType.OverkillGold, null, totalOverkillGold);
        if (!(InformationLogger.infoLogger.isStoryMode && RoomController.roomController.selectedLevel == StoryModeController.story.GetCurrentRoomSetup().setups.Count - 1))     //Don't give a card reward if it's the last room for storymode
            RewardsMenuController.rewardsMenu.AddReward(RewardsMenuController.RewardType.Card, null, 0);
        if (setup.relicReward)
            RewardsMenuController.rewardsMenu.AddReward(RewardsMenuController.RewardType.Relic, null, 0);
        RewardsMenuController.rewardsMenu.ShowMenu();
    }

    public void ReportOverkillGold(int value)
    {
        totalOverkillGold += value;
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

    public void LoadScene(string sceneName, bool goToDeck, int newDeckID)
    {
        RoomController.roomController.Refresh();
        RoomController.roomController.Show();
        if (goToDeck)
            cameraLocation = new Vector3(8, 0, -10);
        else
            cameraLocation = new Vector3(0, 0, -10);
        deckID = newDeckID;
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    //Used by button from combat room's end screen
    public void LoadEndScene()
    {
        if (StoryModeController.story != null)
            SceneManager.LoadScene("StoryModeEndScene");
        else
            SceneManager.LoadScene("EndScene");
    }

    private void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        CollectionController.collectionController.SetDeck(deckID);
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
        {
            TurnController.turnController.StopAllCoroutines();
            CanvasController.canvasController.uiCanvas.enabled = false;
            HandController.handController.EmptyHand();
            CanvasController.canvasController.endGameCanvas.enabled = true;
            CanvasController.canvasController.endGameCanvas.GetComponent<CanvasScaler>().enabled = false;
            CanvasController.canvasController.endGameCanvas.GetComponent<CanvasScaler>().enabled = true;
            CanvasController.canvasController.endGameCanvas.transform.GetChild(2).GetComponent<Collider2D>().enabled = true;
        }
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
            StartCoroutine(FadeInDamageOverlay());
        else
            StartCoroutine(FadeOutDamageOverlay());
    }

    private IEnumerator FadeInDamageOverlay()
    {
        float elapsedTime = 0;
        while (elapsedTime < 0.1f)
        {
            circleHighlight.color = Color.Lerp(new Color(0, 0, 0, 0), new Color(0, 0, 0, 0.5f), elapsedTime / 0.1f);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        damageOverlay.color = new Color(0, 0, 0, 0.5f);
    }

    private IEnumerator FadeOutDamageOverlay()
    {
        float elapsedTime = 0;
        while (elapsedTime < 0.1f)
        {
            circleHighlight.color = Color.Lerp(new Color(0, 0, 0, 0.5f), new Color(0, 0, 0, 0), elapsedTime / 0.1f);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        damageOverlay.color = new Color(0, 0, 0, 0);
    }

    public HealthController GetSimulationCharacter(HealthController simulationTarget, bool SetSimOnTarget = true)
    {
        HealthController simulationCharacter = simulationCharacters.Dequeue();

        simulationCharacter.originalSimulationTarget = simulationTarget.gameObject.name;

        foreach (BuffFactory buff in simulationCharacter.GetComponent<BuffController>().GetBuffs())
            buff.Revert(simulationCharacter);

        simulationCharacter.GetComponent<BuffController>().SetBuffs(simulationTarget.GetComponent<BuffController>().GetBuffs(), false);

        simulationCharacter.gameObject.tag = simulationTarget.gameObject.tag;
        simulationCharacter.SetBonusArmor(simulationTarget.GetBonusArmor(), null, false);
        simulationCharacter.SetBonusAttack(simulationTarget.GetBonusAttack(), false);
        simulationCharacter.SetBonusVit(simulationTarget.GetBonusVit(), false);
        simulationCharacter.SetCurrentArmor(simulationTarget.GetCurrentArmor(), false);
        simulationCharacter.SetCurrentAttack(simulationTarget.GetCurrentAttack());
        simulationCharacter.SetCurrentVit(simulationTarget.GetCurrentVit());

        //simulationCharacter.gameObject.tag = simulationTarget.gameObject.tag;
        simulationCharacter.gameObject.tag = "Simulation";

        simulationCharacter.transform.position = simulationTarget.transform.position;

        if (SetSimOnTarget)
            simulationTarget.SetSimCharacter(simulationCharacter);

        return simulationCharacter;
    }

    public void ReportSimulationFinished(HealthController simulationCharacter)
    {
        simulationCharacter.transform.position = new Vector2(100, 100);

        simulationCharacters.Enqueue(simulationCharacter);
    }

    /*
    public void RestartGame()
    {
        RoomController.roomController.RestartGame();
        DeckController.deckController.RestartGame();
        InformationController.infoController.RestartGame();
    }
    */
}
