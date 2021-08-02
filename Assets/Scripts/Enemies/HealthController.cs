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
    private int currentArmor;
    private int startingArmor;
    private int startingAttack;
    private int currentAttack;
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
    private HealthController tauntedTarget = null;

    private Buff knockbackSelfBuff = null;
    private int knockBackSelfValue = 0;
    private int knockBackSelfDuration = 0;
    private string knockbackSelfCardName = "missing";
    private Buff knockbackOtherBuff = null;
    private int knockBackOtherValue = 0;
    private int knockBackOtherDuration = 0;
    private string knockbackOtherCardName = "missing";

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
            currentVit = maxVit;
            ResetVitText(maxVit);
        }
        else
        {
            try
            {
                int gotMaxVit = InformationController.infoController.GetMaxVit(color);
                maxVit = gotMaxVit;

                int gotCurrentVit = InformationController.infoController.GetCurrentVit(color);
                currentVit = gotCurrentVit;
                ResetVitText(gotCurrentVit);

                int gotStartingAttack = InformationController.infoController.GetStartingAttack(color);
                startingAttack = gotStartingAttack;
                SetCurrentAttack(startingAttack);
                ResetAttackText(startingAttack);

                int gotStartingArmor = InformationController.infoController.GetStartingArmor(color);
                startingArmor = gotStartingArmor;


                SetCurrentArmor(startingArmor, false);
                ResetArmorText(gotStartingArmor);
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
            currentVit = maxVit;
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
        currentArmor = newvalue;
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
        SetCurrentAttack(startingAttack);
    }

    public int GetStartingAttack()
    {
        return startingAttack;
    }

    public void SetCurrentAttack(int newValue)
    {
        currentAttack = newValue;
        charDisplay.attackText.text = currentAttack.ToString();
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
            OnDeath();
            return this.gameObject;
        }
        return null;
    }

    public void OnDeath()
    {
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

    private void ResetVitText(int value)
    {
        charDisplay.vitText.text = value.ToString();
        if (value > maxVit)
            charDisplay.vitText.GetComponent<Outline>().effectColor = new Color(0, 1, 0, 0.5f);
        else if (value == maxVit)
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
                    charDisplay.healthBar.SetStatusText("Immune", Color.yellow);
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

            OnDamage(attacker, damage, oldHealth, traceList, isEndOfTurn);

            if (damage == 1)
                RelicController.relic.OnNotify(this.gameObject, Relic.NotificationType.OnTook1VitDamage, relicTrace);
        }
        else
        {
            OnDamage(attacker, 0, oldHealth, traceList);
        }
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
                    charDisplay.healthBar.SetStatusText("Broken", new Color(255, 102, 0));
                    AchievementSystem.achieve.OnNotify(1, StoryRoomSetup.ChallengeType.BreakEnemies);
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

        if (damage > 0)
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
            currentVit = Mathf.Min(maxVit, currentVit + bonusVit - damage);
            bonusVit = Mathf.Max(0, oldcurrentVit + bonusVit - damage - maxVit);     //Excess healing is moved to bonusVit

            if (damage < 0)
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
    public IEnumerator ForcedMovement(Vector2 castFromLocation, int steps, bool canOverlap = true, List<Buff> buffTrace = null)
    {
        if (size > 1)
            yield break; //Show resist on UI

        Vector2 originalLocation = transform.position;
        for (int i = 1; i <= Mathf.Abs(steps); i++)
        {
            Vector2 knockedToCenter = originalLocation + ((Vector2)transform.position - castFromLocation).normalized * i * Mathf.Sign(steps);

            List<Vector2> aboutToBePositions = new List<Vector2>();
            foreach (Vector2 loc in occupiedSpaces)
                aboutToBePositions.Add(knockedToCenter + loc);

            if (GridController.gridController.CheckIfOutOfBounds(aboutToBePositions) || aboutToBePositions.Contains(castFromLocation)) //If knocked out of bounds, or about to be pulled onto the caster, don't
            {
                GridController.gridController.ResetOverlapOrder(transform.position);
                yield break;
            }

            List<GameObject> objectInWay = GridController.gridController.GetObjectAtLocation(aboutToBePositions);

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

            if (!isSimulation)
                foreach (Vector2 loc in occupiedSpaces)
                    GridController.gridController.RemoveFromPosition(this.gameObject, (Vector2)transform.position + loc);
            Vector2 previousPosition = transform.position;
            transform.position = knockedToCenter;
            if (!isSimulation)
                foreach (Vector2 loc in aboutToBePositions)
                    GridController.gridController.ReportPosition(this.gameObject, loc);

            yield return new WaitForSeconds(0.1f * TimeController.time.timerMultiplier);

            if (MultiplayerGameController.gameController == null)   //Singleplayer
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
            else    //Multiplayer
            {
                GetComponent<MultiplayerPlayerMoveController>().UpdateOrigin(transform.position);
                GetComponent<MultiplayerPlayerMoveController>().ChangeMoveDistance(-1); //To compensate for the forced movement using moverange due to commit move
            }

            StartCoroutine(buffController.TriggerBuff(Buff.TriggerType.OnMove, this, 1));

            if (objectInWay.Count != 0)    //If knocked to position is occupied then stop (still allow overlap, but stop after)
            {
                ApplyKnockBackBuffs(aboutToBePositions);
                GridController.gridController.ResetOverlapOrder(transform.position);
                yield break;
            }
        }
        GridController.gridController.ResetOverlapOrder(transform.position);
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
                buff.cardName = knockbackOtherCardName;
                try
                {
                    buff.casterColor = GetComponent<PlayerController>().GetColorTag().ToString();
                }
                catch
                {
                    buff.casterColor = Card.CasterColor.Enemy.ToString();
                }
                buff.casterName = this.name;
                buff.OnApply(targetH, this, knockBackOtherValue, knockBackOtherDuration, false, null, null);
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
            buff.cardName = knockbackSelfCardName;
            try
            {
                buff.casterColor = GetComponent<PlayerController>().GetColorTag().ToString();
            }
            catch
            {
                buff.casterColor = Card.CasterColor.Enemy.ToString();
            }
            buff.casterName = this.name;
            buff.OnApply(this, this, knockBackSelfValue, knockBackSelfDuration, false, null, null);
            if (buff.GetTriggerEffectType() == Buff.BuffEffectType.VitDamage || buff.GetTriggerEffectType() == Buff.BuffEffectType.PiercingDamage && !isSimulation)
                GetComponent<BuffController>().StartCoroutine(GetComponent<BuffController>().TriggerBuff(Buff.TriggerType.OnDamageDealt, this, 0));

            knockbackSelfBuff = null;
        }
    }

    public void SetKnockBackSelfBuff(Buff newBuff, int value, int duration, string cardName)
    {
        knockbackSelfBuff = newBuff;
        knockBackSelfValue = value;
        knockBackSelfDuration = duration;
        knockbackSelfCardName = cardName;
    }

    public void SetKnockBackOtherBuff(Buff newBuff, int value, int duration, string cardName)
    {
        knockbackOtherBuff = newBuff;
        knockBackOtherValue = value;
        knockBackOtherDuration = duration;
        knockbackOtherCardName = cardName;
    }

    public void SetKnockBackDamage(int value)
    {
        knockBackDamage = value;
    }

    public int GetKnockBackDamage()
    {
        return knockBackDamage;
    }

    public SimHealthController GetSimulatedSelf()
    {
        SimHealthController simSelf = new SimHealthController();
        simSelf.currentVit = currentVit;
        simSelf.currentArmor = currentArmor;
        simSelf.maxVit = maxVit;
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

    public void ResolveBroken()
    {
        currentBrokenTurns++;
        if (currentBrokenTurns == maxBrokenTurns)
        {
            if (currentArmor < startingArmor)
            {
                charDisplay.hitEffectAnim.SetTrigger("BreakRecovery");
                SetStartingArmor(startingArmor);
            }
            else
            {
                currentBrokenTurns = -99999;
                ResetArmorText(currentArmor);
            }
            if (MultiplayerGameController.gameController != null) //Multiplayer component
                ClientScene.localPlayer.GetComponent<MultiplayerInformationController>().ReportHealthController(GetComponent<NetworkIdentity>().netId.ToString(), GetHealthInformation(), ClientScene.localPlayer.GetComponent<MultiplayerInformationController>().GetPlayerNumber());
        }
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

        if (GetComponent<EnemyController>() != null)
            ScoreController.score.UpdateDamage(damage);

        ShowHealthBar(damage, oldHealth, false, null, isEndOfTurn);

        if (damage > 0)
        {
            StartCoroutine(buffController.TriggerBuff(Buff.TriggerType.OnDamageRecieved, attacker, damage, buffTrace));
            if (damage <= bonusVit && isPlayer)
                AchievementSystem.achieve.OnNotify(1, StoryRoomSetup.ChallengeType.BonusHealthCompleteBlock);
        }
        else if (damage == 0)
            StartCoroutine(buffController.TriggerBuff(Buff.TriggerType.OnDamageBlocked, this, damage, buffTrace));
        else
        {
            if (!attacker.isPlayer && isPlayer)
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
            abilitiesController.TriggerAbilities(AbilitiesController.TriggerType.OnBelow0Health);
            if (!attacker.isPlayer && !isPlayer && attacker != this)
                AchievementSystem.achieve.OnNotify(1, StoryRoomSetup.ChallengeType.EnemyFriendlyKill);
        }

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
            charDisplay.healthBar.SetDamageImage(oldHealth, damage, maxVit, center, maxSize, maxSize / size, GridController.gridController.GetIndexAtPosition(this.gameObject, transform.position), GetArmor() <= 0);
        if (oldHealth == -1)
            oldHealth = currentVit + bonusVit;

        if (simHlthController == null)
            simHlthController = GameController.gameController.GetSimulationCharacter(this);

        yield return StartCoroutine(simHlthController.GetComponent<BuffController>().TriggerBuff(Buff.TriggerType.AtEndOfTurn, simHlthController, 0, null, 0));
        yield return StartCoroutine(simHlthController.GetComponent<BuffController>().TriggerBuff(Buff.TriggerType.AtStartOfTurn, simHlthController, 0, null, 0));         //Triggering both start and end of turn for player and enemy damage over time
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

        charDisplay.healthBar.SetBar(oldHealth, damage, maxEndOfTurnDamage, maxVit, center, maxSize, maxSize / size, GridController.gridController.GetIndexAtPosition(this.gameObject, transform.position), GetArmor() <= 0, simulated, simulated);

        if (simCharacter != null)
        {
            GameController.gameController.ReportSimulationFinished(simCharacter);
            simCharacter = null;
        }
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
                if (GetComponent<EnemyController>().desiredTarget[i].GetComponent<HealthController>().isPlayer && health.isPlayer && GetComponent<EnemyController>().desiredTarget[i] != health.gameObject)
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
