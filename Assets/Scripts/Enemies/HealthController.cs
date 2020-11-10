using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class HealthController : MonoBehaviour //Eventualy split into buff, effect, and health controllers
{
    public int size = 1;
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
    private bool preserveBonusVit = false;
    private int enfeeble = 0;
    private int retaliate = 0;
    private bool stunned = false;
    private bool silenced = false;
    private bool disarmed = false;

    private List<int> vitDamageMultipliers;
    private List<int> armorDamageMultipliers;
    private List<int> healingMulitpliers;

    private int energyCostReduction = 0;
    private int manaCostReduction = 0;
    private List<int> energyCostCap = new List<int>() { 99999 };
    private List<int> manaCostCap = new List<int>() { 99999 };

    public CharacterDisplayController charDisplay;
    /*
    public HealthBarController healthBar;
    public Text vitText;
    public Text armorText;
    public Text attackText;
    public List<Image> buffIcons;
    */

    private GameObject creator;
    private AbilitiesController abilitiesController;
    public BuffController buffController;

    private bool wasBroken = false;

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
            int gotMaxVit = InformationController.infoController.GetMaxVit(color);
            maxVit = gotMaxVit;

            int gotCurrentVit = InformationController.infoController.GetCurrentVit(color);
            currentVit = gotCurrentVit;
            ResetVitText(gotCurrentVit);

            int gotStartingAttack = InformationController.infoController.GetStartingAttack(color);
            startingAttack = gotStartingAttack;
            SetAttack(startingAttack);
            ResetAttackText(startingAttack);

            int gotStartingArmor = InformationController.infoController.GetStartingArmor(color);
            startingArmor = gotStartingArmor;

            SetCurrentArmor(startingArmor, false);
            ResetArmorText(gotStartingArmor);
        }
    }

    public void SetCastRange(int value)
    {
        castRange = value;
    }

    public int GetTotalCastRange()
    {
        return castRange + bonusCastRange;
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
        SetAttack(startingAttack);
    }

    public int GetStartingAttack()
    {
        return startingAttack;
    }

    public void SetAttack(int newValue)
    {
        currentAttack = newValue;
        charDisplay.attackText.text = currentAttack.ToString();
    }

    public void SetBonusAttack(int newValue)
    {
        bonusAttack += newValue;
        ResetAttackText(GetAttack());
        try
        {
            GetComponent<EnemyInformationController>().RefreshIntent();
        }
        catch { };
        //HandController.handController.ResetCardDisplays();
    }

    public void SetBonusVit(int newValue)
    {
        bonusVit += newValue;
        ResetVitText(currentVit + bonusVit);
    }

    public void SetBonusArmor(int newValue, bool fromRelic = false)
    {
        ShowArmorDamageNumber(-newValue);
        bonusArmor = Mathf.Max(0, bonusArmor + newValue);

        ResetArmorText(currentArmor + bonusArmor);

        if (!fromRelic)             //To prevent infinite loops on relics giving armor triggered by armor gain
            if (newValue > 0)
                RelicController.relic.OnNotify(this.gameObject, Relic.NotificationType.OnTempArmorGain);
            else if (newValue < 0)
                RelicController.relic.OnNotify(this.gameObject, Relic.NotificationType.OnTempArmorLoss);
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
            PlayerController player = GetComponent<PlayerController>();
            GameController.gameController.ReportDeadChar(player.GetColorTag());
            HandController.handController.ResetCardPlayability(TurnController.turnController.GetCurrentEnergy(), TurnController.turnController.GetCurrentMana());
            //GridController.gridController.RemoveFromPosition(this.gameObject, transform.position);
        }
        catch
        {
            GetComponent<EnemyInformationController>().TriggerDeath();
            GameController.gameController.ReportOverkillGold(0 - GetVit());
            ScoreController.score.UpdateOverkill(0 - GetVit());
        }

        abilitiesController.TriggerAbilities(AbilitiesController.TriggerType.OnDeath);
        //Destruction of the object in Gridcontroller.CheckDeaht();
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

    public void SetEnfeeble(int value)
    {
        enfeeble += value;
    }

    public int GetEnfeeble()
    {
        return enfeeble;
    }

    public void SetRetaliate(int value)
    {
        retaliate += value;
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
    public void TakeVitDamage(int value, HealthController attacker, List<BuffFactory> traceList = null)
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
                if (i == 0)
                    charDisplay.healthBar.SetStatusText("Immune", Color.yellow);
            }

            int damage = Mathf.Max((value * multiplier - GetArmor()), 1);

            if (damage != 0)
            {
                try
                {
                    GetComponent<PlayerController>();
                    ScoreController.score.UpdateDamageArmored(Mathf.Max(GetArmor() - 1, 0));
                    ScoreController.score.UpdateDamageOverhealProtected(Mathf.Min(damage, bonusVit));
                }
                catch { }
                currentVit = currentVit + Mathf.Min(0, bonusVit - damage); //Always takes at least 1 damage;
                bonusVit = Mathf.Max(0, bonusVit - damage);
            }
            ResetVitText(currentVit + bonusVit);

            OnDamage(attacker, damage, oldHealth, traceList);

            if (GetArmor() > 0 && damage != 0)
                TakeArmorDamage(1, attacker, traceList);

            if (damage == 1)
                RelicController.relic.OnNotify(this.gameObject, Relic.NotificationType.OnTook1VitDamage);
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

    public void TakeArmorDamage(int value, HealthController attacker, List<BuffFactory> traceList = null)
    {
        int damage = value;
        if (value > 0)
        {
            foreach (int i in armorDamageMultipliers)
            {
                Debug.Log(i);
                damage *= i;
            }
            if (damage > 0)
                damage = value + enfeeble;
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

                charDisplay.healthBar.SetStatusText("Broken", new Color(255, 102, 0));
                charDisplay.sprite.color = new Color(1, 0, 0);

                try
                {
                    GetComponent<EnemyInformationController>().RefreshIntent();
                    RelicController.relic.OnNotify(this, Relic.NotificationType.OnEnemyBroken);
                    ScoreController.score.UpdateEnemiesBroken();
                }
                catch
                {
                    RelicController.relic.OnNotify(this, Relic.NotificationType.OnPlayerBroken);
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
                return Mathf.Min(value + enfeeble, currentArmor);
        }
        return value;
    }

    public SimHealthController SimulateTakeArmorDamage(SimHealthController simH, int value)
    {
        SimHealthController output = new SimHealthController(); //Java is pass by reference, make new object
        output.SetValues(simH);
        output.currentArmor = Mathf.Max(currentArmor - value - enfeeble, 0);
        return output;
    }

    //Simply remove value from health
    public void TakePiercingDamage(int value, HealthController attacker, List<BuffFactory> traceList = null)
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
    public void ForcedMovement(Vector2 castFromLocation, int steps, List<Buff> buffTrace = null)
    {
        if (size > 1)
            return; //Show resist on UI

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
                return;
            }

            List<GameObject> objectInWay = GridController.gridController.GetObjectAtLocation(aboutToBePositions);

            if (GridController.gridController.CheckIfOutOfBounds(aboutToBePositions) ||
                GridController.gridController.GetObjectAtLocation(aboutToBePositions).Any(x => x.tag == "Blockade")) //If going to be knocked out of bounds or into a barrier, prevent movement
            {
                GridController.gridController.ResetOverlapOrder(transform.position);
                return;
            }

            foreach (Vector2 loc in occupiedSpaces)
                GridController.gridController.RemoveFromPosition(this.gameObject, (Vector2)transform.position + loc);
            transform.position = knockedToCenter;
            StartCoroutine(buffController.TriggerBuff(Buff.TriggerType.OnMove, this, 1));
            foreach (Vector2 loc in aboutToBePositions)
                GridController.gridController.ReportPosition(this.gameObject, loc);

            try
            {
                GetComponent<PlayerMoveController>().UpdateOrigin(transform.position);
                GetComponent<PlayerMoveController>().ChangeMoveDistance(-1); //To compensate for the forced movement using moverange due to commit move
            }
            catch
            {
                GetComponent<EnemyInformationController>().RefreshIntent();
            }

            if (objectInWay.Count != 0)    //If knocked to position is occupied then stop (still allow overlap, but stop after)
            {
                GridController.gridController.ResetOverlapOrder(transform.position);
                return;
            }
        }
        GridController.gridController.ResetOverlapOrder(transform.position);
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

    /*
    public void AddStartOfTurnBuff(Buff buff, int duration)
    {
        if (!startOfTurnBuffs.ContainsKey(buff))
            startOfTurnBuffs.Add(buff, duration);
        else
            startOfTurnBuffs[buff] = Mathf.Max(duration, startOfTurnBuffs[buff]);
        ResetBuffIcons();
    }

    public void AddStartOfTurnDebuff(Buff buff, int duration)
    {
        if (!startOfTurnDebuffs.ContainsKey(buff))
            startOfTurnDebuffs.Add(buff, duration);
        else
            startOfTurnDebuffs[buff] = Mathf.Max(duration, startOfTurnDebuffs[buff]);
        ResetBuffIcons();
    }

    public void AddEndOfTurnBuff(Buff buff, int duration)
    {
        if (!endOfTurnBuffs.ContainsKey(buff))
            endOfTurnBuffs.Add(buff, duration);
        else
            endOfTurnBuffs[buff] = Mathf.Max(duration, endOfTurnBuffs[buff]);
        ResetBuffIcons();
    }

    public void AddEndOfTurnDebuff(Buff buff, int duration)
    {
        if (!endOfTurnDebuffs.ContainsKey(buff))
            endOfTurnDebuffs.Add(buff, duration);
        else
            endOfTurnDebuffs[buff] = Mathf.Max(duration, endOfTurnDebuffs[buff]);
        ResetBuffIcons();
    }

    public void AddOneTimeBuff(Buff buff)
    {
        oneTimeBuffs.Add(buff);
        ResetBuffIcons();
    }

    public void AddOneTimeDebuff(Buff buff)
    {
        oneTimeDebuffs.Add(buff);
        ResetBuffIcons();
    }

    public void AddOnDamageBuff(Buff buff, int duration)
    {
        if (!onDamageBuffs.ContainsKey(buff))
            onDamageBuffs.Add(buff, duration);
        else
            onDamageBuffs[buff] = Mathf.Max(duration, onDamageBuffs[buff]);
        ResetBuffIcons();
    }

    public void AddOnDamangeDebuff(Buff buff, int duration)
    {
        if (!onDamageDebuffs.ContainsKey(buff))
            onDamageDebuffs.Add(buff, duration);
        else
            onDamageDebuffs[buff] = Mathf.Max(duration, onDamageDebuffs[buff]);
        ResetBuffIcons();
    }
    */

    public void ResolveBroken()
    {
        currentBrokenTurns++;
        if (currentBrokenTurns == maxBrokenTurns)
        {
            if (currentArmor < startingArmor)
                SetStartingArmor(startingArmor);
            else
            {
                currentBrokenTurns = -99999;
                ResetArmorText(currentArmor);
            }
        }
    }

    public void AtStartOfTurn()
    {
        if (!preserveBonusVit)
            bonusVit = 0;
        ResetVitText(currentVit + bonusVit);


        /*
        startOfTurnBuffs = ResolveBuffAndReturn(startOfTurnBuffs);
        startOfTurnDebuffs = ResolveBuffAndReturn(startOfTurnDebuffs);
        onDamageBuffs = ResolveBuffAndReturn(onDamageBuffs);
        onDamageDebuffs = ResolveBuffAndReturn(onDamageDebuffs);
        ResetBuffIcons();
        */
    }

    public void OnDamage(HealthController attacker, int damage, int oldHealth, List<BuffFactory> buffTrace = null)
    {
        //Handheld.Vibrate();

        try
        {
            GetComponent<EnemyController>();
            ScoreController.score.UpdateDamage(damage);
        }
        catch { }

        ShowDamageNumber(damage, oldHealth);
        if (retaliate > 0)
            attacker.TakeVitDamage(retaliate, null);

        /*
        onDamageBuffs = ResolveBuffAndReturn(onDamageBuffs);
        onDamageDebuffs = ResolveBuffAndReturn(onDamageDebuffs);
        */
        if (damage > 0)
            StartCoroutine(buffController.TriggerBuff(Buff.TriggerType.OnDamageRecieved, attacker, damage, buffTrace));

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

        GridController.gridController.ResetOverlapOrder(transform.position);
    }

    private void ShowDamageNumber(int damage, int oldHealth)
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
        charDisplay.healthBar.SetBar(oldHealth, damage, maxVit, center, maxSize, maxSize / size, GridController.gridController.GetIndexAtPosition(this.gameObject, transform.position), GetArmor() <= 0);
        charDisplay.healthBar.SetDamageImage(oldHealth, damage, maxVit, center, maxSize, maxSize / size, GridController.gridController.GetIndexAtPosition(this.gameObject, transform.position), GetArmor() <= 0);
    }

    private void ShowArmorDamageNumber(int armorDamage)
    {
        charDisplay.healthBar.SetArmorDamageImage(GetArmor(), armorDamage, GetArmor() <= 0);
    }

    public void ShowHealthBar()
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
        charDisplay.healthBar.SetBar(currentVit + bonusVit, 0, maxVit, center, maxSize, maxSize / size, GridController.gridController.GetIndexAtPosition(this.gameObject, transform.position), currentArmor <= 0, true);
    }

    public void HideHealthBar()
    {
        charDisplay.healthBar.RemoveHealthBar();
    }
    /*
    public void Cleanse()
    {
        foreach (KeyValuePair<Buff, int> buff in startOfTurnBuffs)
        {
            buff.Key.Revert(this);
        }
        foreach (KeyValuePair<Buff, int> buff in endOfTurnBuffs)
        {
            buff.Key.Revert(this);
        }
        foreach (Buff buff in oneTimeBuffs)
        {
            buff.Revert(this);
        }
        foreach (KeyValuePair<Buff, int> buff in onDamageBuffs)
        {
            buff.Key.Revert(this);
        }
        startOfTurnBuffs = new Dictionary<Buff, int>();
        endOfTurnBuffs = new Dictionary<Buff, int>();
        oneTimeBuffs = new List<Buff>();
        ResetBuffIcons();
    }

    //Tick every buff down by 1 turn and reverts all expired buffs
    private Dictionary<Buff, int> ResolveBuffAndReturn(Dictionary<Buff, int> buffList)
    {
        Dictionary<Buff, int> newBuffs = new Dictionary<Buff, int>();
        foreach (KeyValuePair<Buff, int> buff in buffList)
        {
            buff.Key.Trigger(this);
            if (buff.Value > 1)
                newBuffs[buff.Key] = buff.Value - 1;
            else
                buff.Key.Revert(this);
        }
        ResetBuffIcons();
        return newBuffs;
    }
    */

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
}
