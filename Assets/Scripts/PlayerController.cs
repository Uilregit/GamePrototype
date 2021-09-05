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

        //Assess all passive stat bonuses from the player color
        List<Equipment> equipments = new List<Equipment>();
        int bonusVit = 0;
        int bonusArmor = 0;
        int bonusAtk = 0;
        int bonusCastRange = 0;
        int bonusMoveRange = 0;
        int bonusHandSize = 0;
        int bonusReplace = 0;
        if (!healthController.GetIsSimulation() && CollectionController.collectionController.GetSelectEquipments().ContainsKey(colorTag.ToString()))
            foreach (string e in CollectionController.collectionController.GetSelectEquipments()[colorTag.ToString()])
                equipments.Add(LootController.loot.GetEquipment(e));

        foreach (Equipment e in equipments)
        {
            bonusArmor += e.armorChange;
            bonusVit += e.healthChange;
            bonusAtk += e.atkChange;
            bonusCastRange += e.castRangeChange;
            bonusMoveRange += e.moveRangeChange;
            bonusHandSize += e.handSizeChange;
            bonusReplace += e.replaceChange;
        }

        attack = PartyController.party.GetStartingAttack(colorTag);
        startingArmor = PartyController.party.GetStartingArmor(colorTag);
        maxVit = PartyController.party.GetStartingHealth(colorTag);

        healthController.SetCastRange(castRange + bonusCastRange);
        healthController.SetMaxVit(maxVit + bonusVit);
        healthController.SetStartingArmor(startingArmor + bonusArmor);
        healthController.SetStartingAttack(attack + bonusAtk);
        healthController.SetBonusMoveRange(bonusMoveRange);
        HandController.handController.SetBonusReplace(bonusReplace, true);
        HandController.handController.SetBonusHandSize(bonusHandSize, true);
        if (colorTag != Card.CasterColor.Gray)                //Doesn't load info for simulated objects
            try
            {
                if (InformationController.infoController.GetMaxVit(colorTag) != -1)     //If InformationController has not be reset to default values, use those stats instead
                    healthController.LoadCombatInformation(colorTag); //Must go after SetMaxVit
            }
            catch { }

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
