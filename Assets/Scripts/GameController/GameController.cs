using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

public class GameController : MonoBehaviour
{
    public static GameController gameController;

    [Header("Color Settings")]
    public Color redColor;
    public Color blueColor;
    public Color greenColor;
    public Color allPlayersColor;
    public Color enemyColor;
    public Color allEnemiesColor;

    public Text text;
    public CardDisplay[] rewardCards;

    [SerializeField]
    GameObject block;
    List<GameObject> blocks;

    [Header("(x1, x2, y1, y2)")]
    public int[] playerSpawnBox = new int[4]; //(x1,x2,y1,y2)
    public int[] enemySpawnBox = new int[4];

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

        cameraLocation = new Vector3(0, 0, -10);

        SceneManager.sceneLoaded += OnLevelFinishedLoading;

        if (RoomController.roomController.debug)
            GridController.gridController.DebugGrid();

        RelicController.relic.OnNotify(this, Relic.NotificationType.OnCombatStart);
    }

    public void RandomizeRoom()
    {
        setup = RoomController.roomController.GetCurrentRoomSetup();

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        List<GameObject> enemies = new List<GameObject>();
        blocks = new List<GameObject>();
        int counter = 0;
        //Spawn players first. If specified location, at location, if not, spawn randomly
        foreach (GameObject player in players)
        {
            if (setup.playwerSpawnLocations.Count == 0)
                player.GetComponent<PlayerController>().Spawn();
            else
                player.GetComponent<PlayerController>().Spawn(setup.playwerSpawnLocations[counter]);
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
                    foreach (GameObject enemy in enemies)
                    {
                        EnemyController enemyController = enemy.GetComponent<EnemyController>();
                        List<Vector2> path = PathFindController.pathFinder.PathFind(enemy.transform.position, player.transform.position, new string[] { "Player", "Enemy" }, enemy.GetComponent<HealthController>().GetOccupiedSpaces(), enemy.GetComponent<HealthController>().size); //Must be enemy position first
                        if (Mathf.Abs((path[path.Count - 1] - (Vector2)player.transform.position).magnitude) > Mathf.Pow(2 * Mathf.Pow((0.5f * enemy.GetComponent<HealthController>().size - 0.5f), 2), 0.5f))
                            exit = false;
                    }
                if (exit)
                    break;
                else
                    RearrangeBlocks(blockNumber);
            }
        }
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
        text.text = "VICTORY";
        text.enabled = true;
        yield return new WaitForSeconds(TimeController.time.victoryTextDuration * TimeController.time.timerMultiplier);
        text.enabled = false;

        if (RoomController.roomController.selectedLevel >= 9)
        {
            ScoreController.score.UpdateBossesDefeated();
            SceneManager.LoadScene("EndScene");
        }
    }

    public IEnumerator Victory()
    {
        GridController.gridController.DisableAllPlayers();
        HandController.handController.ClearHand();

        DeckController.deckController.ResetCardValues();

        yield return StartCoroutine(DisplayVictoryText());

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
        else
            CameraController.camera.transform.position = new Vector3(0, 0, -10);
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

    public List<Card.CasterColor> GetDeadChars()
    {
        return deadChars;
    }

    public Color GetColor(Card card)
    {
        Card.TargetType target = card.targetType[0];
        switch (target)
        {
            case Card.TargetType.AllEnemies:
                return allEnemiesColor;
            case Card.TargetType.AllPlayers:
                return allPlayersColor;
            case Card.TargetType.Enemy:
                return enemyColor;
            default:
                return new Color(0, 0, 0, 0);
        }
    }
    /*
    public void RestartGame()
    {
        RoomController.roomController.RestartGame();
        DeckController.deckController.RestartGame();
        InformationController.infoController.RestartGame();
    }
    */

    public void LoadEndGameScene()
    {
        SceneManager.LoadScene("EndScene");
    }
}
