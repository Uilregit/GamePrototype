using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

[System.Serializable]
public class HealthInformation
{
    public int castRange = 1;
    public int bonusCastRange = 0;
    public int maxVit;
    public int currentVit = 0;
    public int currentArmor;
    public int startingArmor;
    public int startingAttack;
    public int currentAttack;
    public int bonusAttack;
    public int bonusVit;
    public int bonusArmor;
    public int knockBackDamage = 0;
    public int bonusMoveRange = 0;
    public bool phasedMovement = false;
    public bool preserveBonusVit = false;
    public bool stunned = false;
    public bool silenced = false;
    public bool disarmed = false;
    public string tauntedTarget = null;
}

public class HealthController : MonoBehaviour //Eventualy split into buff, effect, and health controllers
{
    private bool isDead = false;
    public bool isPlayer = false;
    public int size = 1;
    [SerializeField]
    private bool isSimulation = false;
    private int maxBrokenTurns = 2;
    private int currentBrokenTurns = -99999;
    private List<Vector2> occupiedSpaces;

    private int castRange = 1;
    private int bonusCastRange = 0;
    private int maxVit;
    private int currentVit = 0;
    private int equipVit = 0;
    private int currentArmor;
    private int startingArmor;
    private int equipArmor = 0;
    private int startingAttack;
    private int currentAttack;
    private int equipAttack = 0;
    private int bonusAttack;
    private int bonusVit;
    private int bonusArmor;
    private int knockBackDamage = 0;
    private int bonusMoveRange = 0;
    private bool phasedMovement = false;
    private bool preserveBonusVit = false;
    private bool stunned = false;
    private bool silenced = false;
    private bool disarmed = false;
    private bool inflicted = false;
    private bool immuneToEnergy = false;
    private bool immuneToMana = false;
    private HealthController tauntedTarget = null;

    private bool isBeingKnockedBack = false;
    private Buff knockbackSelfBuff = null;
    private int knockBackSelfValue = 0;
    private int knockBackSelfDuration = 0;
    private Card knockbackSelfCard;
    private Buff knockbackOtherBuff = null;
    private int knockBackOtherValue = 0;
    private int knockBackOtherDuration = 0;
    private Card knockbackOtherCard;

    private List<int> vitDamageMultipliers;
    private List<int> armorDamageMultipliers;
    private List<int> healingMulitpliers;

    private int energyCostReduction = 0;
    private int manaCostReduction = 0;
    private List<int> energyCostCap = new List<int>() { 99999 };
    private List<int> manaCostCap = new List<int>() { 99999 };

    public CharacterDisplayController charDisplay;

    private GameObject creator;
    private AbilitiesController abilitiesController;
    private BuffController buffController;

    private bool wasBroken = false;
    private int maxEndOfTurnDamage = 0;
    private IEnumerator drawBarCoroutine;
    private HealthController simCharacter;
    public string originalSimulationTarget = "";

    //###################################################################################################
    //Health Section-------------------------------------------------------------------------------------
    //###################################################################################################
    public void Awake()
    {
        ResetBuffIcons(new List<BuffFactory>());

        occupiedSpaces = new List<Vector2>();

        if (size == 1)
        {
            occupiedSpaces.Add(new Vector2(0, 0));
        }
        else if (size == 2)
        {
            occupiedSpaces.Add(new Vector2(0.5f, 0.5f));
            occupiedSpaces.Add(new Vector2(-0.5f, -0.5f));
            occupiedSpaces.Add(new Vector2(-0.5f, 0.5f));
            occupiedSpaces.Add(new Vector2(0.5f, -0.5f));
        }

        vitDamageMultipliers = new List<int>();
        armorDamageMultipliers = new List<int>();
        healingMulitpliers = new List<int>();

        abilitiesController = GetComponent<AbilitiesController>();
        buffController = GetComponent<BuffController>();

        abilitiesController.TriggerAbilities(AbilitiesController.TriggerType.OnSpawn);

        if (!isSimulation)
            AchievementSystem.achieve.OnNotify(0, StoryRoomSetup.ChallengeType.TakeLessThanXTotalDamage);
    }

    public BuffController GetBuffController()
    {
        return buffController;
    }

    public List<Vector2> GetOccupiedSpaces()
    {
        return occupiedSpaces;
    }

    public void LoadCombatInformation(Card.CasterColor color)
    {
        int value = InformationController.infoController.GetCurrentVit(color);
        if ((InformationController.infoController.firstRoom == true && value == 0) || color == Card.CasterColor.Enemy)
        {
            SetMaxVit(maxVit);
        }
        else
        {
            try
            {
                int gotMaxVit = InformationController.infoController.GetMaxVit(color);
                SetMaxVit(gotMaxVit);

                int gotCurrentVit = InformationController.infoController.GetCurrentVit(color);
                SetCurrentVit(Mathf.Min(gotCurrentVit, maxVit + equipVit));

                int gotStartingAttack = InformationController.infoController.GetStartingAttack(color);
                SetStartingAttack(startingAttack);

                int gotStartingArmor = InformationController.infoController.GetStartingArmor(color);
                SetStartingArmor(startingArmor);
            }
            catch { }
        }
    }

    public void SetCastRange(int value)
    {
        castRange = value;
    }

    public int GetTotalCastRange()
    {
        return Mathf.Max(castRange + bonusCastRange, 1);    //Ensure at least a cast range of 1
    }

    public void SetBonusCastRange(int value)
    {
        bonusCastRange += value;
    }

    public void SetMaxVit(int newValue)
    {
        maxVit = newValue;
        if (InformationController.infoController.firstRoom == true)
        {
            currentVit = maxVit + equipVit;
            ResetVitText(currentVit);
        }
    }

    public void SetCurrentVit(int newValue)
    {
        currentVit = newValue;
        ResetVitText(newValue);
    }

    public int GetMaxVit()
    {
        return maxVit;
    }

    public int GetCurrentVit()
    {
        return currentVit;
    }

    public int GetBonusVit()
    {
        return bonusVit;
    }

    public int GetEquipVit()
    {
        return equipVit;
    }

    public int GetEquipArmor()
    {
        return equipArmor;
    }

    public int GetEquipAttack()
    {
        return equipAttack;
    }

    public void SetInstantKill()
    {
        currentVit = 0;
        bonusVit = 0;
    }

    public int GetCurrentAttack()
    {
        return currentAttack;
    }

    public int GetBonusAttack()
    {
        return bonusAttack;
    }

    public void SetStartingArmor(int newvalue)
    {
        currentBrokenTurns = -99999;
        startingArmor = newvalue;
        currentArmor = newvalue + equipArmor;
        ResetArmorText(currentArmor);
    }

    public void SetCurrentArmor(int newvalue, bool relative)
    {
        if (relative)
            currentArmor += newvalue;
        else
            currentArmor = newvalue;
        ResetArmorText(currentArmor);
    }

    public int GetStartingArmor()
    {
        return startingArmor;
    }

    public int GetCurrentArmor()
    {
        return currentArmor;
    }

    public void SetStartingAttack(int newvalue)
    {
        startingAttack = newvalue;
        SetCurrentAttack(startingAttack + equipAttack);
    }

    public int GetStartingAttack()
    {
        return startingAttack;
    }

    public void SetCurrentAttack(int newValue)
    {
        currentAttack = newValue;
        ResetAttackText(currentAttack);
    }

    public void SetBonusAttack(int newValue, bool relative)
    {
        if (relative)
            bonusAttack += newValue;
        else
            bonusAttack = newValue;
        ResetAttackText(GetAttack());
        try
        {
            GetComponent<EnemyInformationController>().RefreshIntent();
        }
        catch { };
        //HandController.handController.ResetCardDisplays();
    }

    public void SetBonusVit(int newValue, bool relative)
    {
        if (relative)
            bonusVit += newValue;
        else
            bonusVit = newValue;
        ResetVitText(currentVit + bonusVit);
    }

    public void SetBonusArmor(int newValue, List<Relic> relicTrace, bool relative)
    {
        ShowArmorDamageNumber(-newValue);
        if (relative)
            bonusArmor = Mathf.Max(-currentArmor, bonusArmor + newValue);
        else
            bonusArmor = newValue;

        ResetArmorText(currentArmor + bonusArmor);

        if (newValue > 0)
            RelicController.relic.OnNotify(this.gameObject, Relic.NotificationType.OnTempArmorGain, relicTrace);
        else if (newValue < 0)
            RelicController.relic.OnNotify(this.gameObject, Relic.NotificationType.OnTempArmorLoss, relicTrace);
    }

    public int GetBonusArmor()
    {
        return bonusArmor;
    }

    public void SetEquipVit(int value)
    {
        equipVit = value;
    }

    public void SetEquipArmor(int value)
    {
        equipArmor = value;
    }

    public void SetEquipAttack(int value)
    {
        equipAttack = value;
    }

    public void SetManaCostCap(int value)
    {
        manaCostCap.Add(value);
    }

    public void RemoveManaCostCap(int value)
    {
        manaCostCap.Remove(value);
    }

    public int GetManaCostCap()
    {
        return manaCostCap.Min();
    }

    public void SetEnergyCostCap(int value)
    {
        energyCostCap.Add(value);
    }

    public void RemoveEnergyCostCap(int value)
    {
        energyCostCap.Remove(value);
    }

    public int GetEnergyCostCap()
    {
        return energyCostCap.Min();
    }

    public void SetManaCostReduction(int value)
    {
        manaCostReduction += value;
    }

    public int GetManaCostReduction()
    {
        return manaCostReduction;
    }

    public void SetEnergyCostReduction(int value)
    {
        energyCostReduction += value;
    }

    public int GetEnergyCostReduction()
    {
        return energyCostReduction;
    }

    public GameObject CheckDeath()
    {
        if (currentVit <= 0)
        {
            if (isPlayer)
            {
                if (StoryModeController.story.GetCurrentRoomSetup().roomName == "World Tut-1")
                    TutorialController.tutorial.TriggerTutorial(Dialogue.Condition.PlayerDeath, 1);
                else
                    TutorialController.tutorial.TriggerTutorial(Dialogue.Condition.FirstDeath, 1);
            }

            if (StoryModeController.story.GetCurrentRoomSetup().skipFinalRewards && isPlayer)
            {
                GridController.gridController.StopAllCoroutines();
                TurnController.turnController.StopAllCoroutines();
                GameController.gameController.ResetRoom(false);
                return null;
            }
            OnDeath();
            return this.gameObject;
        }
        return null;
    }

    public void OnDeath()
    {
        //Calls all buff related achievements as they wouldn't have been reverted yet
        foreach (BuffFactory buff in buffController.GetBuffs())
            buff.ReportAchievements();

        try
        {
            if (MultiplayerGameController.gameController != null)
            {
                MultiplayerPlayerController player = GetComponent<MultiplayerPlayerController>();
                int playerNumber = 0;
                if (gameObject.tag == "Enemy")
                    playerNumber = 1;
                MultiplayerGameController.gameController.ReportDeadChar(player.GetColorTag(), playerNumber);
            }
            else
            {
                PlayerController player = GetComponent<PlayerController>();
                GameController.gameController.ReportDeadChar(player.GetColorTag(), player.gameObject);

                //GridController.gridController.RemoveFromPosition(this.gameObject, transform.position);
            }
            HandController.handController.ResetCardPlayability(TurnController.turnController.GetCurrentEnergy(), TurnController.turnController.GetCurrentMana());
        }
        catch
        {
            GameController.gameController.ReportOverkillGold(0 - GetVit());
            ScoreController.score.UpdateOverkill(0 - GetVit());
        }

        abilitiesController.TriggerAbilities(AbilitiesController.TriggerType.OnDeath);
        //Destruction of the object in Gridcontroller.CheckDeath();
    }

    public int GetVit()
    {
        return currentVit + bonusVit;
    }

    public int GetArmor()
    {
        return currentArmor + bonusArmor;
    }

    public int GetAttack()
    {
        return Mathf.Max(currentAttack + bonusAttack, 0); //Attack can't fall below 0
    }

    public void SetStunned(bool value)
    {
        stunned = value;
        if (stunned)
            try
            {
                GetComponent<EnemyInformationController>().HideIntent();
            }
            catch { }
    }

    public bool GetStunned()
    {
        return stunned;
    }

    public void SetPhasedMovement(bool value)
    {
        phasedMovement = value;
    }

    public bool GetPhasedMovement()
    {
        return phasedMovement;
    }

    public void SetSilenced(bool state)
    {
        silenced = state;
    }

    public bool GetSilenced()
    {
        return silenced;
    }

    public void SetPreserveBonusVit(bool state)
    {
        preserveBonusVit = state;
    }

    public bool GetPreserveBonusVit()
    {
        return preserveBonusVit;
    }

    public void SetDisarmed(bool state)
    {
        disarmed = state;
    }

    public bool GetDisarmed()
    {
        return disarmed;
    }

    public void SetInflicted(bool state)
    {
        inflicted = state;
    }

    public bool GetInflicted()
    {
        return inflicted;
    }

    public void SetImmuneToEnergy(bool state)
    {
        immuneToEnergy = state;
    }

    public bool GetImmuneToEnergy()
    {
        return immuneToEnergy;
    }

    public void SetImmuneToMana(bool state)
    {
        immuneToMana = state;
    }

    public bool GetImmuneToMana()
    {
        return immuneToMana;
    }

    private void ResetVitText(int value)
    {
        charDisplay.vitText.text = value.ToString();
        if (value > maxVit + equipVit)
            charDisplay.vitText.GetComponent<Outline>().effectColor = new Color(0, 1, 0, 0.5f);
        else if (value == maxVit + equipVit)
            charDisplay.vitText.GetComponent<Outline>().effectColor = new Color(1, 1, 1, 0.5f);
        else
            charDisplay.vitText.GetComponent<Outline>().effectColor = new Color(0, 0, 0, 0.5f);
    }

    private void ResetArmorText(int value)
    {
        charDisplay.armorText.text = value.ToString();
        if (value == 0)
            charDisplay.sprite.color = Color.red;
        else
            charDisplay.sprite.color = Color.white;
        if (bonusArmor == 0)
            charDisplay.armorText.GetComponent<Outline>().effectColor = new Color(0, 0, 0, 0.5f);
        else
            charDisplay.armorText.GetComponent<Outline>().effectColor = new Color(0, 1, 0, 0.5f);
    }

    private void ResetAttackText(int value)
    {
        charDisplay.attackText.text = value.ToString();
        if (bonusAttack > 0)
            charDisplay.attackText.GetComponent<Outline>().effectColor = new Color(0, 1, 0, 0.5f);
        else if (bonusAttack < 0)
            charDisplay.attackText.GetComponent<Outline>().effectColor = new Color(1, 0, 0, 0.5f);
        else
            charDisplay.attackText.GetComponent<Outline>().effectColor = new Color(0, 0, 0, 0.5f);
    }
    //###################################################################################################
    //Effect Section-------------------------------------------------------------------------------------
    //###################################################################################################
    public void TakeVitDamage(int value, HealthController attacker, List<BuffFactory> traceList = null, List<Relic> relicTrace = null, bool isEndOfTurn = false)
    {
        int oldHealth = currentVit + bonusVit;
        if (value > 0)
        {
            int multiplier = 1;
            if (GetArmor() == 0)
                multiplier = 2;
            foreach (int i in vitDamageMultipliers)
            {
                multiplier *= i;
                if (i == 0 && !isSimulation)
                    SetStatusText("Immune", Color.yellow);
            }

            int damage = Mathf.Max((value * multiplier - GetArmor()), 1);
            if (multiplier == 0)
                damage = 0;

            if (damage != 0)
            {
                if (GetComponent<PlayerController>() != null)
                {
                    ScoreController.score.UpdateDamageArmored(Mathf.Max(GetArmor() - 1, 0));
                    ScoreController.score.UpdateDamageOverhealProtected(Mathf.Min(damage, bonusVit));
                }
                currentVit = currentVit + Mathf.Min(0, bonusVit - damage); //Always takes at least 1 damage;
                bonusVit = Mathf.Max(0, bonusVit - damage);
            }
            ResetVitText(currentVit + bonusVit);

            if (GetArmor() > 0 && damage != 0)
                TakeArmorDamage(1, attacker, traceList);

            if (!isSimulation)
                if (damage == 0)
                    charDisplay.onHitSoundController.PlayArmorSound(Card.SoundEffect.Immunity, 0);
                else if (GetArmor() == 0)
                    charDisplay.onHitSoundController.PlayArmorSound(Card.SoundEffect.ArmorBroken, 0);

            OnDamage(attacker, damage, oldHealth, traceList, isEndOfTurn);

            if (damage == 1)
                RelicController.relic.OnNotify(this.gameObject, Relic.NotificationType.OnTook1VitDamage, relicTrace);
        }
        else
        {
            OnDamage(attacker, 0, oldHealth, traceList);
        }
    }

    public void SetStatusText(string text, Color color)
    {
        charDisplay.healthBar.SetStatusText(text, color);
    }

    public int GetSimulatedVitDamage(int value)
    {
        int multiplier = 1;
        if (currentArmor == 0)
            multiplier *= 2;
        foreach (int i in vitDamageMultipliers)
            multiplier *= i;
        return Mathf.Max((value * multiplier - GetArmor()), 1);
    }

    public SimHealthController SimulateTakeVitDamage(SimHealthController simH, int value)
    {
        SimHealthController output = new SimHealthController(); //Java is pass by reference, make new object
        output.SetValues(simH);
        int multiplier = 1;
        if (currentArmor == 0)
            multiplier = 2;
        if (value > 0)
            output.currentVit -= Mathf.Max(value - GetArmor(), 0) * multiplier;//ToBeChangedLater
        return output;
    }

    public void TakeArmorDamage(int value, HealthController attacker, List<BuffFactory> traceList = null, List<Relic> relicTrace = null)
    {
        int damage = value;
        if (value > 0)
        {
            foreach (int i in armorDamageMultipliers)
            {
                damage *= i;
            }
            if (damage > 0)
                damage = value;
        }

        ShowArmorDamageNumber(damage);

        if (bonusArmor > 0)
        {
            currentArmor -= Mathf.Max(0, damage - bonusArmor);
            currentArmor = Mathf.Max(currentArmor, 0);
            bonusArmor = Mathf.Max(0, bonusArmor - damage);
        }
        else
            currentArmor = Mathf.Max(0, currentArmor - damage);
        ResetArmorText(currentArmor + bonusArmor);

        if (GetArmor() <= 0)
        {
            if (!wasBroken)
            {
                currentBrokenTurns = 0;
                wasBroken = true;

                if (!isSimulation)
                {
                    charDisplay.onHitSoundController.PlayArmorSound(Card.SoundEffect.ArmorBreak, 0);

                    charDisplay.healthBar.SetStatusText("Broken", new Color(255, 102, 0));
                    if (!isPlayer)
                    {
                        AchievementSystem.achieve.OnNotify(1, StoryRoomSetup.ChallengeType.BreakEnemies);
                        TutorialController.tutorial.TriggerTutorial(Dialogue.Condition.EnemyBroken, 1);
                    }
                    else
                        AchievementSystem.achieve.OnNotify(1, StoryRoomSetup.ChallengeType.BeBroken);
                }
                charDisplay.sprite.color = new Color(1, 0, 0);

                abilitiesController.TriggerAbilities(AbilitiesController.TriggerType.OnBreak);

                try
                {
                    GetComponent<EnemyInformationController>().RefreshIntent();
                    RelicController.relic.OnNotify(this, Relic.NotificationType.OnEnemyBroken, relicTrace);
                    ScoreController.score.UpdateEnemiesBroken();
                }
                catch
                {
                    RelicController.relic.OnNotify(this, Relic.NotificationType.OnPlayerBroken, relicTrace);
                }
            }
        }
        else
        {
            wasBroken = false;
        }

        if (damage > 0 && !isSimulation)
            if (GetArmor() > 0)
                charDisplay.onHitSoundController.PlayArmorSound(Card.SoundEffect.ArmorHit, 1);
        StartCoroutine(buffController.TriggerBuff(Buff.TriggerType.OnShieldDamageRecieved, this, damage, traceList));
    }

    public int GetSimulatedArmorDamage(int value)
    {
        if (value > 0)
        {
            foreach (int i in armorDamageMultipliers)
                value *= i;
            if (value == 0)
                return 0;
            else
                return Mathf.Min(value, currentArmor);
        }
        return value;
    }

    public SimHealthController SimulateTakeArmorDamage(SimHealthController simH, int value)
    {
        SimHealthController output = new SimHealthController(); //Java is pass by reference, make new object
        output.SetValues(simH);
        output.currentArmor = Mathf.Max(currentArmor - value, 0);
        return output;
    }

    //Simply remove value from health
    public void TakePiercingDamage(int value, HealthController attacker, List<BuffFactory> traceList = null, List<Relic> relicTrace = null)
    {
        int oldHealth = currentVit + bonusVit;
        int damage = 0;
        if (value > 0)
        {
            damage = value;
            foreach (int i in vitDamageMultipliers)
                damage *= i;
        }
        else
        {
            int oldcurrentVit = currentVit;
            damage = value;
            foreach (int i in healingMulitpliers)
                damage *= i;
            currentVit = Mathf.Min(maxVit + equipVit, currentVit + bonusVit - damage);
            bonusVit = Mathf.Max(0, oldcurrentVit + bonusVit - damage - maxVit - equipVit);     //Excess healing is moved to bonusVit

            if (bonusVit > 0 && !isSimulation)
                TutorialController.tutorial.TriggerTutorial(Dialogue.Condition.Overheal, 1);

            if (damage < 0 && isSimulation)
                StartCoroutine(buffController.TriggerBuff(Buff.TriggerType.OnHealingRecieved, this, damage, traceList));
        }

        if (damage > 0)
        {
            currentVit = currentVit + Mathf.Min(0, bonusVit - damage); //Always takes at least 1 damage;
            bonusVit = Mathf.Max(0, bonusVit - damage);
        }
        ResetVitText(currentVit + bonusVit);

        OnDamage(attacker, damage, oldHealth, traceList);
    }

    public int GetSimulatedPiercingDamage(int value)
    {
        if (value < 0)
        {
            foreach (int i in healingMulitpliers)
                value *= i;
            return value;
        }
        return value;
    }

    public SimHealthController SimulateTakePiercingDamage(SimHealthController simH, int value)
    {
        SimHealthController output = new SimHealthController(); //Java is pass by reference, make new object
        output.SetValues(simH);
        output.currentVit -= Mathf.Max(value, 0);
        return output;
    }

    public void ChangeAttack(int value)
    {
        currentAttack = Mathf.Max(currentAttack - value, 0); //Attack can never be lower than 0
        charDisplay.attackText.text = (currentAttack + bonusAttack).ToString();
    }

    public SimHealthController SimulateChangeAttack(SimHealthController simH, int value)
    {
        SimHealthController output = new SimHealthController(); //Java is pass by reference, make new object
        output.SetValues(simH);
        output.currentAttack = Mathf.Max(output.currentAttack - value, 0); //Attack can never be lower than 0
        return output;
    }

    //Force the object to move towards the finalLocation, value number of times
    //Value can be positive (towards) or negative (away) from the finalLocation
    //If there is an object in the way stop movement before colision and deal piercing knockback damage to both objects
    public IEnumerator ForcedMovement(Vector2 castFromLocation, Vector2 knockbackDirection, int steps, bool canOverlap = true, List<Buff> buffTrace = null)
    {
        if (size > 1)
            yield break; //Show resist on UI
        isBeingKnockedBack = true;

        Vector2 originalLocation = transform.position;
        for (int i = 1; i <= Mathf.Abs(steps); i++)
        {
            Vector2 knockedToCenter = originalLocation + knockbackDirection.normalized * i * Mathf.Sign(steps);

            List<Vector2> aboutToBePositions = new List<Vector2>();
            foreach (Vector2 loc in occupiedSpaces)
                aboutToBePositions.Add(knockedToCenter + loc);

            if (GridController.gridController.CheckIfOutOfBounds(aboutToBePositions) || aboutToBePositions.Contains(castFromLocation)) //If knocked out of bounds, or about to be pulled onto the caster, don't
            {
                GridController.gridController.ResetOverlapOrder(transform.position);
                yield break;
            }

            List<GameObject> objectInWay = GridController.gridController.GetObjectAtLocation(aboutToBePositions, new string[] { "Blockade" });      //Only stop if hitting a blockade

            if (GridController.gridController.CheckIfOutOfBounds(aboutToBePositions) ||
                GridController.gridController.GetObjectAtLocation(aboutToBePositions, new string[] { "Blockade" }).Count > 0)        //If going to be knocked out of bounds or into a barrier, prevent movement
            {
                GridController.gridController.ResetOverlapOrder(transform.position);
                yield break;
            }
            else if (!canOverlap && GridController.gridController.GetObjectAtLocation(aboutToBePositions, new string[] { "Player", "Enemy" }).Count != 0)      //If can't overlap, also stop if ANY object is in the way
            {
                ApplyKnockBackBuffs(aboutToBePositions);
                GridController.gridController.ResetOverlapOrder(transform.position);
                yield break;
            }

            Vector2 previousPosition = transform.position;
            if (!isSimulation)
            {
                foreach (Vector2 loc in occupiedSpaces)
                    GridController.gridController.RemoveFromPosition(this.gameObject, (Vector2)transform.position + loc);

                //Also knock back all overlapped objects if they're not already being overlapped
                foreach (GameObject obj in GridController.gridController.GetObjectAtLocation(previousPosition, new string[] { "Player", "Enemy" }))
                    if (!obj.GetComponent<HealthController>().GetIsBeingKnockedBack())
                        obj.GetComponent<HealthController>().StartCoroutine(obj.GetComponent<HealthController>().ForcedMovement(castFromLocation, knockbackDirection, steps - 1, canOverlap, buffTrace));

                foreach (Vector2 loc in aboutToBePositions)
                    GridController.gridController.ReportPosition(this.gameObject, loc);

                yield return StartCoroutine(LerpToPosition(transform.position, knockedToCenter, 0.05f * TimeController.time.timerMultiplier));
            }
            else
                transform.position = knockedToCenter;

            try
            {
                try
                {
                    GetComponent<PlayerMoveController>().UpdateOrigin(transform.position);
                    GetComponent<PlayerMoveController>().ChangeMoveDistance(-1); //To compensate for the forced movement using moverange due to commit move
                }
                catch
                {
                    GetComponent<EnemyInformationController>().RefreshIntent();
                    GetComponent<EnemyController>().SetPreviousPosition(previousPosition);
                }
            }
            catch { }

            StartCoroutine(buffController.TriggerBuff(Buff.TriggerType.OnMove, this, 1));

            if (objectInWay.Count != 0)    //If knocked to position is occupied then stop (still allow overlap, but stop after)
            {
                ApplyKnockBackBuffs(aboutToBePositions);
                GridController.gridController.ResetOverlapOrder(transform.position);
                yield break;
            }
        }
        isBeingKnockedBack = false;
        GridController.gridController.ResetOverlapOrder(transform.position);
    }

    private IEnumerator LerpToPosition(Vector2 startingLoc, Vector2 endingLoc, float duration)
    {
        for (int i = 0; i < 3; i++)
        {
            transform.position = Vector2.Lerp(startingLoc, endingLoc, i / 2f);
            yield return new WaitForSeconds(duration / 3f);
        }
        transform.position = endingLoc;
    }

    public void ApplyKnockBackBuffs(List<Vector2> aboutToBePositions)
    {
        if (knockbackOtherBuff != null)
        {
            foreach (GameObject targ in GridController.gridController.GetObjectAtLocation(aboutToBePositions))
            {
                HealthController targetH = targ.GetComponent<HealthController>();
                BuffFactory buff = new BuffFactory();
                //card.buff[effectIndex].SetDrawnCards(card.cards);
                buff.SetBuff(knockbackOtherBuff);
                buff.card = knockbackOtherCard;
                buff.OnApply(targetH, this, knockBackOtherValue, knockBackOtherDuration, knockbackOtherCard, false, null, null);
                if (buff.GetTriggerEffectType() == Buff.BuffEffectType.VitDamage || buff.GetTriggerEffectType() == Buff.BuffEffectType.PiercingDamage && !isSimulation)
                    GetComponent<BuffController>().StartCoroutine(GetComponent<BuffController>().TriggerBuff(Buff.TriggerType.OnDamageDealt, this, 0));
            }
            knockbackOtherBuff = null;
        }
        if (knockbackSelfBuff != null)
        {
            BuffFactory buff = new BuffFactory();
            //card.buff[effectIndex].SetDrawnCards(card.cards);
            buff.SetBuff(knockbackSelfBuff);
            buff.card = knockbackSelfCard;
            buff.OnApply(this, this, knockBackSelfValue, knockBackSelfDuration, knockbackSelfCard, false, null, null);
            if (buff.GetTriggerEffectType() == Buff.BuffEffectType.VitDamage || buff.GetTriggerEffectType() == Buff.BuffEffectType.PiercingDamage && !isSimulation)
                GetComponent<BuffController>().StartCoroutine(GetComponent<BuffController>().TriggerBuff(Buff.TriggerType.OnDamageDealt, this, 0));

            knockbackSelfBuff = null;
        }
    }

    public void SetKnockBackSelfBuff(Buff newBuff, int value, int duration, Card card)
    {
        knockbackSelfBuff = newBuff;
        knockBackSelfValue = value;
        knockBackSelfDuration = duration;
        knockbackSelfCard = card;
    }

    public void SetKnockBackOtherBuff(Buff newBuff, int value, int duration, Card card)
    {
        knockbackOtherBuff = newBuff;
        knockBackOtherValue = value;
        knockBackOtherDuration = duration;
        knockbackOtherCard = card;
    }

    public void SetKnockBackDamage(int value)
    {
        knockBackDamage = value;
    }

    public int GetKnockBackDamage()
    {
        return knockBackDamage;
    }

    public bool GetIsBeingKnockedBack()
    {
        return isBeingKnockedBack;
    }

    public SimHealthController GetSimulatedSelf()
    {
        SimHealthController simSelf = new SimHealthController();
        simSelf.currentVit = currentVit;
        simSelf.currentArmor = currentArmor;
        simSelf.maxVit = maxVit + equipVit;
        return simSelf;
    }

    //###################################################################################################
    //Buff section---------------------------------------------------------------------------------------
    //###################################################################################################
    public void SetBonusMoveRange(int value)
    {
        bonusMoveRange += value;
    }

    public int GetBonusMoveRange()
    {
        return bonusMoveRange;
    }

    public void AddVitDamageMultiplier(int i)
    {
        vitDamageMultipliers.Add(i);
    }

    public void AddArmorDamageMultiplier(int i)
    {
        armorDamageMultipliers.Add(i);
    }

    public void RemoveVitDamageMultiplier(int i)
    {
        vitDamageMultipliers.Remove(i);
    }

    public void RemoveArmorDamageMultiplier(int i)
    {
        armorDamageMultipliers.Remove(i);
    }

    public void AddHealingMultiplier(int i)
    {
        healingMulitpliers.Add(i);
    }

    public void RemoveHealingMultiplier(int i)
    {
        healingMulitpliers.Remove(i);
    }

    public List<int> GetVitDamageMultiplier()
    {
        return vitDamageMultipliers;
    }

    public void SetVitDamageMultiplier(List<int> value)
    {
        vitDamageMultipliers = value;
    }

    public List<int> GetArmorDamageMultiplier()
    {
        return armorDamageMultipliers;
    }

    public void SetArmorDamageMultiplier(List<int> value)
    {
        armorDamageMultipliers = value;
    }

    public List<int> GetHealingMultiplier()
    {
        return healingMulitpliers;
    }

    public void SetHealingMultiplier(List<int> value)
    {
        healingMulitpliers = value;
    }

    public void ResolveBroken()
    {
        currentBrokenTurns++;
        if (currentBrokenTurns == maxBrokenTurns)
        {
            if (currentArmor < startingArmor && GetCurrentVit() > 0)        //Trigger break recovery only if the character is not dead
            {
                charDisplay.hitEffectAnim.SetTrigger("BreakRecovery");
                SetStartingArmor(startingArmor);
                TutorialController.tutorial.TriggerTutorial(Dialogue.Condition.BreakRecovery, 1);
            }
            else
            {
                currentBrokenTurns = -99999;
                ResetArmorText(currentArmor);
            }
        }
        else if (GetCurrentArmor() == 0 && GetCurrentVit() > 0)
            TutorialController.tutorial.TriggerTutorial(Dialogue.Condition.Break2ndTurn, 1);
    }

    public void AtStartOfTurn()
    {
        if (!preserveBonusVit)
            bonusVit = 0;
        ResetVitText(currentVit + bonusVit);
    }

    public void AtEndOfTurn()
    {
        if (isPlayer)
            AchievementSystem.achieve.OnNotify(GetVit() + GetArmor(), StoryRoomSetup.ChallengeType.HealthAndArmorCombinedPerTurn);
    }

    public bool GetIfDamageWouldKill(int damage)
    {
        return GetVit() > 0 && GetVit() <= damage;
    }

    public void OnDamage(HealthController attacker, int damage, int oldHealth, List<BuffFactory> buffTrace = null, bool isEndOfTurn = false)
    {
        //Handheld.Vibrate();

        if (!isPlayer && damage > 0)
        {
            ScoreController.score.UpdateDamage(damage);
            TutorialController.tutorial.TriggerTutorial(Dialogue.Condition.EnemyDamageTaken, damage);
        }

        ShowHealthBar(damage, oldHealth, false, null, isEndOfTurn);

        if (damage > 0 && !isSimulation)
        {
            StartCoroutine(buffController.TriggerBuff(Buff.TriggerType.OnDamageRecieved, attacker, damage, buffTrace));
            abilitiesController.TriggerAbilities(AbilitiesController.TriggerType.OnDamageTaken);

            if (isPlayer)
            {
                AchievementSystem.achieve.OnNotify(damage, StoryRoomSetup.ChallengeType.TakeLessThanXTotalDamage);
                if (damage <= bonusVit)
                    AchievementSystem.achieve.OnNotify(1, StoryRoomSetup.ChallengeType.BonusHealthCompleteBlock);
            }
        }
        else if (damage == 0 && !isSimulation)
            StartCoroutine(buffController.TriggerBuff(Buff.TriggerType.OnDamageBlocked, this, damage, buffTrace));
        else
        {
            if (!attacker.isPlayer && isPlayer && !isSimulation)
                AchievementSystem.achieve.OnNotify(damage, StoryRoomSetup.ChallengeType.HealedByEnemy);
        }

        if (currentVit + bonusVit <= 0)
            try
            {
                GetComponent<EnemyInformationController>().HideIntent();
            }
            catch { }
        else
            try
            {
                GetComponent<EnemyInformationController>().RefreshIntent();
            }
            catch { }

        if (oldHealth > 0 && oldHealth - damage <= 0 && !isSimulation)   //Trigger on health below 0 actions
        {
            if (isPlayer && Random.Range(0, 100) <= 20)                   //Give all players a 20% chance of defying death and retain health
            {
                SetCurrentVit(1);
                charDisplay.healthBar.SetStatusText("Defied", new Color(224, 37, 0));
                TutorialController.tutorial.TriggerTutorial(Dialogue.Condition.DefyDeath, 1);
            }
            else
            {
                abilitiesController.TriggerAbilities(AbilitiesController.TriggerType.OnBelow0Health);
                if (!attacker.isPlayer && !isPlayer && attacker != this && !isSimulation)
                    AchievementSystem.achieve.OnNotify(1, StoryRoomSetup.ChallengeType.EnemyFriendlyKill);
            }
        }

        if (!isSimulation && isPlayer && damage > 0)
            GameController.gameController.UpdatePlayerDamage();

        if (!isSimulation && !isPlayer && GetCurrentVit() < 0)
            TutorialController.tutorial.TriggerTutorial(Dialogue.Condition.EnemyOverkill, GetCurrentVit());

        GridController.gridController.ResetOverlapOrder(transform.position);
    }

    private void ShowArmorDamageNumber(int armorDamage)
    {
        if (isSimulation)
            return;

        charDisplay.healthBar.SetArmorDamageImage(GetArmor(), armorDamage, GetArmor() <= 0);
    }

    public void ShowHealthBar(int damage = 0, int oldHealth = -1, bool simulated = false, HealthController simHlthController = null, bool isEndOfTurn = false)
    {
        if (isSimulation)
            return;

        drawBarCoroutine = DarwBar(damage, oldHealth, simulated, simHlthController, isEndOfTurn);
        StartCoroutine(drawBarCoroutine);
    }

    public IEnumerator DarwBar(int damage = 0, int oldHealth = -1, bool simulated = false, HealthController simHlthController = null, bool isEndOfTurn = false)
    {
        int maxSize = size;
        Vector2 center = transform.position;
        List<Vector2> overlappingAreas = new List<Vector2>();
        foreach (Vector2 loc in occupiedSpaces)
            overlappingAreas.Add((Vector2)transform.position + loc);
        foreach (GameObject obj in GridController.gridController.GetObjectAtLocation(overlappingAreas))
            if (obj.GetComponent<HealthController>().size > maxSize)
            {
                maxSize = obj.GetComponent<HealthController>().size;
                center = obj.transform.position;
            }

        if (!simulated && oldHealth != -1)
            charDisplay.healthBar.SetDamageImage(oldHealth, damage, maxVit + equipVit, center, maxSize, maxSize / size, GridController.gridController.GetIndexAtPosition(this.gameObject, transform.position), GetArmor() <= 0);
        if (oldHealth == -1)
            oldHealth = currentVit + bonusVit;

        if (simHlthController == null)
            simHlthController = GameController.gameController.GetSimulationCharacter(this);

        if (!isSimulation)
        {
            yield return StartCoroutine(simHlthController.GetComponent<BuffController>().TriggerBuff(Buff.TriggerType.AtEndOfTurn, simHlthController, 0, null, 0));
            yield return StartCoroutine(simHlthController.GetComponent<BuffController>().TriggerBuff(Buff.TriggerType.AtStartOfTurn, simHlthController, 0, null, 0));         //Triggering both start and end of turn for player and enemy damage over time
        }
        int endOfTurnDamage = GetVit() - simHlthController.GetVit();

        /*
        Debug.Log(gameObject.name);
        Debug.Log("damage: " + damage);
        Debug.Log("original: " + GetAttack() + "|" + GetArmor() + "|" + GetVit());
        Debug.Log("simu: " + simHlthController.GetAttack() + "|" + simHlthController.GetArmor() + "|" + simHlthController.GetVit());
        */

        if (isEndOfTurn || simulated)
            endOfTurnDamage -= damage;

        if (!isEndOfTurn)
            maxEndOfTurnDamage = endOfTurnDamage;
        else
            maxEndOfTurnDamage -= damage;

        //maxEndOfTurnDamage = Mathf.Clamp(maxEndOfTurnDamage, 0, GetVit() - damage);         //Clamped so damage bar never runs to negative

        charDisplay.healthBar.SetBar(oldHealth, damage, maxEndOfTurnDamage, maxVit + equipVit, center, maxSize, maxSize / size, GridController.gridController.GetIndexAtPosition(this.gameObject, transform.position), GetArmor() <= 0, simulated, simulated);

        if (simCharacter != null)
        {
            GameController.gameController.ReportSimulationFinished(simCharacter);
            simCharacter = null;
        }
    }

    public IEnumerator ShowDamagePreviewBar(int damage, int oldHealth, HealthController simHlthController, Sprite sprite, Vector2 castLocation)
    {
        yield return StartCoroutine(simHlthController.GetComponent<BuffController>().TriggerBuff(Buff.TriggerType.AtEndOfTurn, simHlthController, 0, null, 0));
        yield return StartCoroutine(simHlthController.GetComponent<BuffController>().TriggerBuff(Buff.TriggerType.AtStartOfTurn, simHlthController, 0, null, 0));         //Triggering both start and end of turn for player and enemy damage over time
        int endOfTurnDamage = GetVit() - simHlthController.GetVit() - damage;

        charDisplay.healthBar.SetBar(Mathf.Max(0, oldHealth), damage, endOfTurnDamage, maxVit + equipVit, castLocation + new Vector2(0, 0.5f), 1, 1, 1, GetArmor() <= 0, true, true);
        charDisplay.healthBar.SetCharacter(sprite, castLocation);
    }

    public void SetSimCharacter(HealthController simuChar)
    {
        simCharacter = simuChar;
    }

    public HealthController GetSimCharacter()
    {
        return simCharacter;
    }

    public void HideHealthBar()
    {
        if (drawBarCoroutine != null)
            StopCoroutine(drawBarCoroutine);
        charDisplay.healthBar.RemoveHealthBar();

        if (simCharacter != null)
        {
            GameController.gameController.ReportSimulationFinished(simCharacter);
            simCharacter = null;
        }
    }

    public void ResetBuffIcons(List<BuffFactory> allBuffs)
    {
        allBuffs = allBuffs.OrderByDescending(x => x.duration).ToList<BuffFactory>();

        int maxIcons = 5;

        for (int i = 0; i < maxIcons; i++)
        {
            if (i < allBuffs.Count)
            {
                charDisplay.buffIcons[i].color = allBuffs[i].GetIconColor();
                charDisplay.buffIcons[i].transform.GetChild(0).GetComponent<Text>().text = allBuffs[i].GetDescription();
            }
            else
            {
                charDisplay.buffIcons[i].color = new Color(0, 0, 0, 0);
                charDisplay.buffIcons[i].transform.GetChild(0).GetComponent<Text>().text = "";
            }
        }
    }

    public void SetStatTexts(bool state)
    {
        charDisplay.vitText.enabled = state;
        charDisplay.armorText.enabled = state;
        charDisplay.attackText.enabled = state;
    }

    public void SetCreator(GameObject obj)
    {
        creator = obj;
    }

    public GameObject GetCreator()
    {
        return creator;
    }

    public void SetTauntTarget(HealthController health)
    {
        tauntedTarget = health;
        try
        {
            for (int i = 0; i < GetComponent<EnemyController>().desiredTarget.Length; i++)
            {
                if (GetComponent<EnemyController>().desiredTarget[i].GetComponent<HealthController>().isPlayer && health.isPlayer && GetComponent<EnemyController>().desiredTarget[i] != health.gameObject && !isSimulation)
                    AchievementSystem.achieve.OnNotify(1, StoryRoomSetup.ChallengeType.TauntAwayFromAlly);
                GetComponent<EnemyController>().desiredTarget[i] = tauntedTarget.gameObject;
            }
        }
        catch { }
    }

    public HealthController GetTauntedTarget()
    {
        return tauntedTarget;
    }

    public void ReportDead()
    {
        isDead = true;
    }

    public void ReportResurrect()
    {
        isDead = false;
    }

    public bool GetIsDead()
    {
        return isDead;
    }

    //Used by multiplayer only
    public byte[] GetHealthInformation()
    {
        HealthInformation output = new HealthInformation();

        output.castRange = castRange;
        output.bonusCastRange = bonusCastRange;
        output.maxVit = maxVit;
        output.currentVit = currentVit;
        output.currentArmor = currentArmor;
        output.startingArmor = startingArmor;
        output.startingAttack = startingAttack;
        output.currentAttack = currentAttack;
        output.bonusAttack = bonusAttack;
        output.bonusVit = bonusVit;
        output.bonusArmor = bonusArmor;
        output.knockBackDamage = knockBackDamage;
        output.bonusMoveRange = bonusMoveRange;
        output.phasedMovement = phasedMovement;
        output.preserveBonusVit = preserveBonusVit;
        output.stunned = stunned;
        output.silenced = silenced;
        output.disarmed = disarmed;
        if (tauntedTarget != null)
            output.tauntedTarget = tauntedTarget.GetComponent<NetworkIdentity>().netId.ToString();
        else
            output.tauntedTarget = "none";

        MemoryStream stream = new MemoryStream();
        BinaryFormatter formatter = new BinaryFormatter();

        formatter.Serialize(stream, output);

        byte[] final = stream.ToArray();
        return final;
    }

    //Used by multiplayer only
    public void SetHealthInformation(HealthInformation info)
    {
        if (currentArmor + bonusArmor != info.currentArmor + info.bonusArmor)
            ShowArmorDamageNumber((currentArmor + bonusArmor) - (info.currentArmor + info.bonusArmor));

        if (currentVit + bonusVit != info.currentVit + info.bonusVit)
            ShowHealthBar((currentVit + bonusVit) - (info.currentVit + info.bonusVit), currentVit + bonusVit);

        castRange = info.castRange;
        bonusCastRange = info.bonusCastRange;
        maxVit = info.maxVit;
        currentVit = info.currentVit;
        currentArmor = info.currentArmor;
        startingArmor = info.startingArmor;
        startingAttack = info.startingAttack;
        currentAttack = info.currentAttack;
        bonusAttack = info.bonusAttack;
        bonusVit = info.bonusVit;
        bonusArmor = info.bonusArmor;
        knockBackDamage = info.knockBackDamage;
        bonusMoveRange = info.bonusMoveRange;
        phasedMovement = info.phasedMovement;
        preserveBonusVit = info.preserveBonusVit;
        stunned = info.stunned;
        silenced = info.silenced;
        disarmed = info.disarmed;

        if (info.tauntedTarget != "none")
            tauntedTarget = ClientScene.localPlayer.GetComponent<MultiplayerInformationController>().GetObjectFromNetID(info.tauntedTarget).GetComponent<HealthController>();
        else
            tauntedTarget = null;

        ResetAttackText(currentAttack + bonusAttack);
        ResetArmorText(currentArmor + bonusArmor);
        ResetVitText(currentVit + bonusVit);

        HandController.handController.ResetCardPlayability(TurnController.turnController.GetCurrentEnergy(), TurnController.turnController.GetCurrentMana());
    }

    public Vector2 GetPreviousPosition()
    {
        try
        {
            return GetComponent<PlayerMoveController>().GetPreviousPosition();
        }
        catch
        {
            return GetComponent<EnemyController>().GetPreviousPosition();
        }
    }

    public bool GetIsSimulation()
    {
        return isSimulation;
    }
}
