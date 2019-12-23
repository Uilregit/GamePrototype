using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthController : MonoBehaviour //Eventualy split into buff, effect, and health controllers
{
    private int maxVit;
    private int currentVit = 0;
    private int currentShield;
    private int currentAttack;
    private int bonusVit;
    private int bonusShield;
    private int bonusAttack;
    private int knockBackDamage = 0;
    private int bonusMoveRange = 0;
    private int enfeeble = 0;
    private int retaliate = 0;
    private bool stunned = false;

    private Dictionary<Buff, int> endOfTurnBuffs = new Dictionary<Buff, int>();
    private Dictionary<Buff, int> startOfTurnBuffs = new Dictionary<Buff, int>();
    private Dictionary<Buff, int> onDamageBuffs = new Dictionary<Buff, int>();
    private Dictionary<Buff, int> endOfTurnDebuffs = new Dictionary<Buff, int>();
    private Dictionary<Buff, int> startOfTurnDebuffs = new Dictionary<Buff, int>();
    private Dictionary<Buff, int> onDamageDebuffs = new Dictionary<Buff, int>();
    private List<Buff> oneTimeBuffs = new List<Buff>(); //AKA permanent buffs
    private List<Buff> oneTimeDebuffs = new List<Buff>();

    public Text vitText;
    public Text shieldText;
    public Text attackText;

    //###################################################################################################
    //Health Section-------------------------------------------------------------------------------------
    //###################################################################################################
    public void LoadCombatInformation(Card.CasterColor color)
    {
        Debug.Log(color);
        int value = InformationController.infoController.GetCurrentVit(color);
        if ((InformationController.infoController.firstRoom == true && value == 0) || color == Card.CasterColor.Enemy)
        {
            currentVit = maxVit;
            ResetVitText(maxVit);
        }
        else
        {
            currentVit = value;
            ResetVitText(value);
        }
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

    public int GetCurrentAttack()
    {
        return currentAttack;
    }

    public void SetMaxShield(int newvalue)
    {
        currentShield = newvalue;
        ResetShieldText(currentShield);
    }

    public int GetCurrentShield()
    {
        return currentShield;
    }

    public void SetAttack(int newValue)
    {
        currentAttack = newValue;
        attackText.text = currentAttack.ToString();
    }

    public void SetBonusAttack(int newValue)
    {
        bonusAttack += newValue;
        ResetAttackText(currentAttack + bonusAttack);
    }

    public void SetBonusVit(int newValue)
    {
        bonusVit += newValue;
        ResetVitText(currentVit + bonusVit);
    }

    public void SetBonusShield(int newValue)
    {
        bonusShield += newValue;
        ResetShieldText(currentShield + bonusShield);
    }

    public void OnDeath()
    {
        Destroy(this.gameObject);
    }

    public int GetVit()
    {
        return currentVit + bonusVit;
    }

    public int GetShield()
    {
        return currentShield + bonusShield;
    }

    public int GetAttack()
    {
        return currentAttack + bonusAttack;
    }

    public void SetStunned(bool value)
    {
        stunned = value;
    }

    public bool GetStunned()
    {
        return stunned;
    }

    public void SetEnfeeble(int value)
    {
        enfeeble += value;
    }

    public void SetRetaliate(int value)
    {
        retaliate += value;
    }

    private void ResetVitText(int value)
    {
        vitText.text = value.ToString();
        if (currentVit == maxVit)
            vitText.GetComponent<Outline>().effectColor = new Color(1, 1, 1, 0.5f);
        else
            vitText.GetComponent<Outline>().effectColor = new Color(0, 0, 0, 0.5f);
    }

    private void ResetShieldText(int value)
    {
        shieldText.text = value.ToString();
        if (currentShield == 0)
            shieldText.enabled = false;
        else
            shieldText.enabled = true;
    }

    private void ResetAttackText(int value)
    {
        attackText.text = value.ToString();
        if (bonusAttack > 0)
            attackText.GetComponent<Outline>().effectColor = new Color(0, 1, 0, 0.5f);
        else if (bonusAttack < 0)
            attackText.GetComponent<Outline>().effectColor = new Color(1, 0, 0, 0.5f);
        else
            attackText.GetComponent<Outline>().effectColor = new Color(0, 0, 0, 0.5f);
    }
    //###################################################################################################
    //Effect Section-------------------------------------------------------------------------------------
    //###################################################################################################
    public void TakeVitDamage(int value, HealthController attacker)
    {
        if (value > 0)
        {
            Debug.Log("took " + Mathf.Max(value - GetShield(), 0) + " vit damage");
            currentVit -= Mathf.Max(value - GetShield(), 0) + enfeeble;
            TakeShieldDamage(Mathf.Min(currentShield, value));
            ResetVitText(currentVit + bonusVit);

            OnDamage(attacker);

            if (currentVit <= 0)
                OnDeath();
        }
    }

    public SimHealthController SimulateTakeVitDamage(SimHealthController simH, int value)
    {
        SimHealthController output = new SimHealthController(); //Java is pass by reference, make new object
        output.SetValues(simH);
        if (value > 0)
            output.currentVit -= Mathf.Max(value - output.currentShield, 0);//ToBeChangedLater
        return output;
    }

    public void TakeShieldDamage(int value)
    {
        Debug.Log("took " + value + " shield damage");
        currentShield = Mathf.Max(currentShield - value - enfeeble, 0);
        ResetShieldText(currentShield + bonusShield);
    }

    public SimHealthController SimulateTakeShieldDamage(SimHealthController simH, int value)
    {
        SimHealthController output = new SimHealthController(); //Java is pass by reference, make new object
        output.SetValues(simH);
        output.currentShield = Mathf.Max(output.currentShield - value, 0);
        return output;
    }

    //Simply remove value from health
    public void TakePiercingDamage(int value, HealthController attacker)
    {
        if (value > 0)
            currentVit -= value + enfeeble;
        else
            currentVit = Mathf.Min(maxVit, currentVit - value); //Enfeeble does not amplify healing, and can't heal more than maxVit
        ResetVitText(currentVit + bonusVit);

        OnDamage(attacker);

        if (currentVit <= 0)
            OnDeath();
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
        attackText.text = (currentAttack + bonusAttack).ToString();
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
    public void ForcedMovement(Vector2 finalLocation, int steps)
    {
        Vector2 originalLocation = transform.position;
        for (int i = 1; i <= Mathf.Abs(steps); i++)
        {
            Vector2 knockedToPosition = originalLocation + ((Vector2)transform.position - finalLocation).normalized * i * Mathf.Sign(steps);
            GameObject objectInWay = GridController.gridController.GetObjectAtLocation(knockedToPosition);
            if (knockedToPosition != finalLocation)
                if (objectInWay == null && !GridController.gridController.CheckIfOutOfBounds(knockedToPosition))    //If knocked to position is occupied or is out of bounds
                {
                    GridController.gridController.RemoveFromPosition(transform.position);
                    transform.position = knockedToPosition;
                    GridController.gridController.ReportPosition(this.gameObject, transform.position);
                }
                else
                {
                    TakePiercingDamage(knockBackDamage, null);
                    try
                    {
                        objectInWay.GetComponent<HealthController>().TakePiercingDamage(knockBackDamage, this);
                    }
                    catch
                    {
                    }
                    return;
                }
        }
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
        simSelf.currentShield = currentShield;
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

    public void AddStartOfTurnBuff(Buff buff, int duration)
    {
        if (!startOfTurnBuffs.ContainsKey(buff))
            startOfTurnBuffs.Add(buff, duration);
    }

    public void AddStartOfTurnDebuff(Buff buff, int duration)
    {
        if (!startOfTurnDebuffs.ContainsKey(buff))
            startOfTurnDebuffs.Add(buff, duration);
    }

    public void AddEndOfTurnBuff(Buff buff, int duration)
    {
        if (!endOfTurnBuffs.ContainsKey(buff))
            endOfTurnBuffs.Add(buff, duration);
    }

    public void AddEndOfTurnDebuff(Buff buff, int duration)
    {
        if (!endOfTurnDebuffs.ContainsKey(buff))
            endOfTurnDebuffs.Add(buff, duration);
    }

    public void AddOneTimeBuff(Buff buff)
    {
        oneTimeBuffs.Add(buff);
    }

    public void AddOneTimeDebuff(Buff buff)
    {
        oneTimeDebuffs.Add(buff);
    }

    public void AddOnDamageBuff(Buff buff, int duration)
    {
        if (!onDamageBuffs.ContainsKey(buff))
            onDamageBuffs.Add(buff, duration);
    }

    public void AddOnDamangeDebuff(Buff buff, int duration)
    {
        if (!onDamageDebuffs.ContainsKey(buff))
            onDamageDebuffs.Add(buff, duration);
    }

    public void AtEndOfTurn()
    {
        endOfTurnBuffs = ResolveBuffAndReturn(endOfTurnBuffs);
        endOfTurnDebuffs = ResolveBuffAndReturn(endOfTurnDebuffs);
    }

    public void AtStartOfTurn()
    {
        startOfTurnBuffs = ResolveBuffAndReturn(startOfTurnBuffs);
        startOfTurnDebuffs = ResolveBuffAndReturn(startOfTurnDebuffs);
        onDamageBuffs = ResolveBuffAndReturn(onDamageBuffs);
        onDamageDebuffs = ResolveBuffAndReturn(onDamageDebuffs);
    }

    public void OnDamage(HealthController attacker)
    {
        if (retaliate > 0)
            attacker.TakeVitDamage(retaliate, null);
    }

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
        return newBuffs;
    }
}
