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

    [HideInInspector]
    public int equipBonusVit = 0;
    [HideInInspector]
    public int equipBonusArmor = 0;
    [HideInInspector]
    public int equipBonusAtk = 0;
    [HideInInspector]
    public int equipBonusCastRange = 0;
    [HideInInspector]
    public int equipBonusMoveRange = 0;
    [HideInInspector]
    public int equipBonusHandSize = 0;
    [HideInInspector]
    public int equipBonusReplace = 0;

    // Start is called before the first frame update
    public virtual void Awake()
    {
        healthController = GetComponent<HealthController>();

        moveController = GetComponent<PlayerMoveController>();
        moveController.SetPlayerController(this);
    }

    public void SetupStats()
    {
        //Assess all passive stat bonuses from the player color
        List<Equipment> equipments = new List<Equipment>();

        if (colorTag != Card.CasterColor.Gray)                //Doesn't load info for simulated objects
        {
            if (!healthController.GetIsSimulation() && CollectionController.collectionController.GetSelectEquipments().ContainsKey(colorTag.ToString()))
                foreach (string e in CollectionController.collectionController.GetSelectEquipments()[colorTag.ToString()])
                    equipments.Add(LootController.loot.GetEquipment(e));

            foreach (Equipment e in equipments)
            {
                equipBonusArmor += e.armorChange;
                equipBonusVit += e.healthChange;
                equipBonusAtk += e.atkChange;
                equipBonusCastRange += e.castRangeChange;
                equipBonusMoveRange += e.moveRangeChange;
                equipBonusHandSize += e.handSizeChange;
                equipBonusReplace += e.replaceChange;
            }

            healthController.SetEquipArmor(equipBonusArmor);
            healthController.SetEquipAttack(equipBonusAtk);
            healthController.SetEquipVit(equipBonusVit);

            attack = PartyController.party.GetStartingAttack(colorTag);
            startingArmor = PartyController.party.GetStartingArmor(colorTag);
            maxVit = PartyController.party.GetStartingHealth(colorTag);

            healthController.SetCastRange(castRange + equipBonusCastRange);
            healthController.SetMaxVit(maxVit);
            healthController.SetStartingArmor(startingArmor);
            healthController.SetStartingAttack(attack);
            healthController.SetBonusMoveRange(equipBonusMoveRange);

            HandController.handController.SetBonusReplace(equipBonusReplace, true);
            HandController.handController.SetBonusHandSize(equipBonusHandSize, true);
            try
            {
                if (InformationController.infoController.GetMaxVit(colorTag) != -1)     //If InformationController has not be reset to default values, use those stats instead
                    healthController.LoadCombatInformation(colorTag); //Must go after SetMaxVit
            }
            catch { }

            healthController.GetComponent<BuffController>().Cleanse(healthController, false, true);
        }
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

        SetupStats();

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
