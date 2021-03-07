using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Mirror;

public class MultiplayerPlayerController : NetworkBehaviour
{
    [SerializeField] private Card.CasterColor colorTag;
    [SerializeField] private int castRange = 1;
    [SerializeField] private int maxVit;
    [SerializeField] private int startingArmor;
    [SerializeField] private int attack;
    [SerializeField] private int moveRange;
    [SerializeField] private int attackRange;
    private HealthController healthController;
    private MultiplayerPlayerMoveController moveController;
    private Vector2 spawnedLocation;

    // Start is called before the first frame update
    public virtual void Awake()
    {
        healthController = GetComponent<HealthController>();

        healthController.SetCastRange(castRange);
        healthController.SetMaxVit(maxVit);
        healthController.SetStartingArmor(startingArmor);
        healthController.SetStartingAttack(attack);

        moveController = GetComponent<MultiplayerPlayerMoveController>();
        moveController.SetPlayerController(this);
    }

    public void Spawn(int player)
    {
        int x;
        int y;
        for (int j = 0; j < 100; j++)
        {
            if (player == 0)
            {
                x = Random.Range(MultiplayerGameController.gameController.playerSpawnBox[0], MultiplayerGameController.gameController.playerSpawnBox[1] + 1);
                y = Random.Range(MultiplayerGameController.gameController.playerSpawnBox[2], MultiplayerGameController.gameController.playerSpawnBox[3] + 1);
            }
            else
            {
                x = Random.Range(MultiplayerGameController.gameController.enemySpawnBox[0], MultiplayerGameController.gameController.enemySpawnBox[1] + 1);
                y = Random.Range(MultiplayerGameController.gameController.enemySpawnBox[2], MultiplayerGameController.gameController.enemySpawnBox[3] + 1);
            }
            if (GridController.gridController.GetObjectAtLocation(new Vector2(x, y)).Count == 0)
            {
                Vector2 location = new Vector2(x, y);
                Spawn(location);
                break;
            }
        }

    }

    //[ClientRpc]
    public void Spawn(Vector2 location)
    {
        spawnedLocation = location;
        transform.position = location;
        GridController.gridController.ReportPosition(this.gameObject, location);
        GetComponent<MultiplayerPlayerMoveController>().Spawn();
    }

    public void ResetSpawn()
    {
        GridController.gridController.RemoveFromPosition(gameObject, transform.position);
        transform.position = spawnedLocation;
        GridController.gridController.ReportPosition(gameObject, spawnedLocation);
    }

    public Card.CasterColor GetColorTag()
    {
        return colorTag;
    }

    public int GetAttack()
    {
        return healthController.GetCurrentAttack();
    }

    public int GetMoveRange()
    {
        return moveRange;
    }
    public int GetAttackRange()
    {
        return attackRange;
    }

    public int GetCurrentVit()
    {
        return healthController.GetCurrentVit();
    }

    public int GetStartingArmor()
    {
        return startingArmor;
    }

    public HealthController GetHealthController()
    {
        return healthController;
    }
}
