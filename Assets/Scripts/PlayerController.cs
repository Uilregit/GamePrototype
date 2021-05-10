using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Card.CasterColor colorTag;
    [SerializeField] private int castRange = 1;
    [SerializeField] private int maxVit;
    [SerializeField] private int startingArmor;
    [SerializeField] private int attack;
    [SerializeField] private int moveRange;
    [SerializeField] private int attackRange;
    private HealthController healthController;
    private PlayerMoveController moveController;

    // Start is called before the first frame update
    public virtual void Awake()
    {
        healthController = GetComponent<HealthController>();

        healthController.SetCastRange(castRange);
        healthController.SetMaxVit(maxVit);
        healthController.SetStartingArmor(startingArmor);
        healthController.SetStartingAttack(attack);
        if (colorTag != Card.CasterColor.Gray)                //Doesn't load info for simulated objects
            healthController.LoadCombatInformation(colorTag); //Must go after SetMaxVit

        moveController = GetComponent<PlayerMoveController>();
        moveController.SetPlayerController(this);
    }

    public void Spawn()
    {
        int x;
        int y;
        for (int j = 0; j < 100; j++)
        {
            x = Random.Range(GameController.gameController.playerSpawnBox[0], GameController.gameController.playerSpawnBox[1] + 1);
            y = Random.Range(GameController.gameController.playerSpawnBox[2], GameController.gameController.playerSpawnBox[3] + 1);
            if (GridController.gridController.GetObjectAtLocation(new Vector2(x, y)).Count == 0)
            {
                Vector2 location = new Vector2(x, y);
                Spawn(location);
                break;
            }
        }

    }

    public void Spawn(Vector2 location)
    {
        if (!PartyController.party.partyColors.Contains(colorTag))
            Destroy(this.gameObject);
        transform.position = location;
        GridController.gridController.ReportPosition(this.gameObject, location);
        GetComponent<PlayerMoveController>().Spawn();
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
