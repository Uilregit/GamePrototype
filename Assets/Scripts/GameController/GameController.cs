using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public static GameController gameController;
    public Text text;
    public float victoryTextDuration;
    public CardDisplay[] rewardCards;

    [SerializeField]
    GameObject block;
    List<GameObject> blocks;
    public int[] playerSpawnBox = new int[4];
    public int[] enemySpawnBox = new int[4];

    private void Start()
    {
        if (GameController.gameController == null)
            GameController.gameController = this;
        else
            Destroy(this.gameObject);

        //Hide the reward cards
        for (int i = 0; i < rewardCards.Length; i++)
        {
            rewardCards[i].Hide();
            rewardCards[i].gameObject.GetComponent<Collider2D>().enabled = false;
        }
        RandomizeRoom();
        DeckController.deckController.PopulateDecks();
        HandController.handController.DrawFullHand();
    }

    public void RandomizeRoom()
    {
        RoomSetup setup = RoomController.roomController.GetRoomSetup();
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        List<GameObject> enemies = new List<GameObject>();
        blocks = new List<GameObject>();
        int counter = 0;
        foreach (GameObject player in players)
        {
            if (setup.playwerSpawnLocations.Count == 0)
                player.GetComponent<PlayerController>().Spawn();
            else
                player.GetComponent<PlayerController>().Spawn(setup.playwerSpawnLocations[counter]);
            counter += 1;
        }
        foreach (GameObject enemy in setup.enemies)
        {
            Debug.Log("Spawned enemy");
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
                if (GridController.gridController.GetObjectAtLocation(new Vector2(x, y)) == null)
                {
                    GameObject thisBlock = Instantiate(block, new Vector2(x, y), Quaternion.identity);
                    blocks.Add(thisBlock);
                    break;
                }
                else
                {
                    Debug.Log("Tried block at: " + x.ToString() + ", " + y.ToString());
                }
            }
        }
        bool exit = false;
        for (int i = 0; i < 1000; i++)
        {
            exit = true;
            foreach (GameObject player in players)
                foreach (GameObject enemy in enemies)
                {
                    List<Vector2> path = PathFindController.pathFinder.PathFind(player.transform.position, enemy.transform.position);
                    if (path[path.Count - 1] != (Vector2)enemy.transform.position)
                    {
                        RearrangeBlocks();
                        exit = false;
                    }
                }
            if (exit)
                break;
        }
    }

    private void RearrangeBlocks()
    {
        RoomSetup setup = RoomController.roomController.GetRoomSetup();
        foreach (GameObject block in blocks)
        {
            GridController.gridController.RemoveFromPosition(block.transform.position);
            Destroy(block.gameObject);
        }
        for (int i = 0; i < setup.blockNumber; i++)
        {
            int x, y;
            for (int j = 0; j < 100; j++)
            {
                int[] roomRange = GridController.gridController.GetRoomRange();
                x = Random.Range(roomRange[0], roomRange[1]);
                y = Random.Range(roomRange[2], roomRange[3]);
                if (GridController.gridController.GetObjectAtLocation(new Vector2(x, y)) == null)
                {
                    GameObject thisBlock = Instantiate(block, new Vector2(x, y), Quaternion.identity);
                    blocks.Add(thisBlock);
                    break;
                }
                else
                {
                    Debug.Log("Tried block at: " + x.ToString() + ", " + y.ToString());
                }
            }
        }
    }

    private IEnumerator DisplayVictoryText()
    {
        InformationController.infoController.SaveCombatInformation();
        text.text = "VICTORY";
        text.enabled = true;
        yield return new WaitForSeconds(victoryTextDuration);
        text.enabled = false;
    }

    public IEnumerator Victory()
    {
        GridController.gridController.DisableAllPlayers();
        HandController.handController.ClearHand();

        yield return StartCoroutine(DisplayVictoryText());

        for (int i = 0; i < rewardCards.Length; i++)
        {
            rewardCards[i].SetCard(LootController.loot.GetCard());
            rewardCards[i].Show();
            rewardCards[i].SetHighLight(false);
            rewardCards[i].gameObject.GetComponent<Collider2D>().enabled = true;
        }
    }

    public void LoadMapScene()
    {
        RoomController.roomController.Refresh();
        RoomController.roomController.Show();
        SceneManager.LoadScene("OverworldScene", LoadSceneMode.Single);
    }
}
