using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

public class GameController : MonoBehaviour
{
    public static GameController gameController;

    public Sprite background;
    public Image damageOverlay;

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

    //Stats
    private int totalOverkillGold;

    private void Start()
    {
        if (GameController.gameController == null)
            GameController.gameController = this;
        else
            Destroy(this.gameObject);

        deadChars = new List<Card.CasterColor>();

        //Hide the reward cards
        for (int i = 0; i < rewardCards.Length; i++)
        {
            rewardCards[i].Hide();
            rewardCards[i].gameObject.GetComponent<Collider2D>().enabled = false;
        }
        RandomizeRoom();

        DeckController.deckController.ResetCardValues();
        DeckController.deckController.PopulateDecks();
        DeckController.deckController.ShuffleDrawPile();
        HandController.handController.DrawFullHand();

        InformationController.infoController.SaveCombatInformation();           //Save combat information at start of room as well in case char dies in the first room

        cameraLocation = new Vector3(0, 0, -10);

        SceneManager.sceneLoaded += OnLevelFinishedLoading;

        if (InformationLogger.infoLogger.debug)
            GridController.gridController.DebugGrid();

        RelicController.relic.OnNotify(this, Relic.NotificationType.OnCombatStart);

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
    }

    public void RandomizeRoom()
    {
        GetComponent<SpriteRenderer>().sprite = RoomController.roomController.GetCombatBackground();
        setup = RoomController.roomController.GetCurrentRoomSetup();

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
            if (setup.playwerSpawnLocations.Count == 0)
                player.GetComponent<PlayerController>().Spawn();
            else
                player.GetComponent<PlayerController>().Spawn(setup.playwerSpawnLocations[counter]);

            // If the player was dead from before, remove them
            Card.CasterColor colorTag = player.GetComponent<PlayerController>().GetColorTag();
            if (InformationController.infoController.GetIfDead(colorTag))
            {
                GridController.gridController.RemoveFromPosition(player, player.transform.position);
                ReportDeadChar(colorTag);
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
        HandController.handController.ClearHand();

        DeckController.deckController.ResetCardValues();

        yield return StartCoroutine(DisplayVictoryText());
        if (RoomController.roomController.GetCurrentRoomSetup().isBossRoom)
        {
            ScoreController.score.UpdateBossesDefeated();
            if (RoomController.roomController.GetWorldLevel() == 2)
            {
                SceneManager.LoadScene("EndScene");
                yield break;
            }
            else
                RoomController.roomController.LoadNewWorld(RoomController.roomController.GetWorldLevel() + 1);
        }
        RewardsMenuController.rewardsMenu.AddReward(RewardsMenuController.RewardType.PassiveGold, null, ResourceController.resource.goldGainPerCombat);
        if (totalOverkillGold > 0)
            RewardsMenuController.rewardsMenu.AddReward(RewardsMenuController.RewardType.OverkillGold, null, totalOverkillGold);
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
            cameraLocation = new Vector3(7, 0, -10);
        else
            cameraLocation = new Vector3(0, 0, -10);
        deckID = newDeckID;
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    private void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        CollectionController.collectionController.SetDeck(deckID);
        if (SceneManager.GetActiveScene().name == "OverworldScene")
            CameraController.camera.transform.position = cameraLocation;
        /*
        else
            CameraController.camera.transform.position = new Vector3(0, 0, -10);
        */
    }

    public void ReportDeadChar(Card.CasterColor color)
    {
        deadChars.Add(color);
        if (deadChars.Count >= 3)
        {
            TurnController.turnController.StopAllCoroutines();
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
            if (!deadChars.Contains(obj.GetComponent<PlayerController>().GetColorTag()))
                output.Add(obj);
        return output;
    }

    public void ReportResurrectedChar(Card.CasterColor color)
    {
        deadChars.Remove(color);
    }

    public List<Card.CasterColor> GetDeadChars()
    {
        return deadChars;
    }

    public void SetDamageOverlay(float remainingHealthPercentage)
    {
        Debug.Log("trace");
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
    /*
    public void RestartGame()
    {
        RoomController.roomController.RestartGame();
        DeckController.deckController.RestartGame();
        InformationController.infoController.RestartGame();
    }
    */
}
