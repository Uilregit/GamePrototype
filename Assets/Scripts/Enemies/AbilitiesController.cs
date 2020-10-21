using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilitiesController : MonoBehaviour
{
    public enum TargetType
    {
        Creator
    }
    public enum TriggerType
    {
        OnDeath, OnSacrifice
    }
    public enum AbilityType
    {
        VitChange, ShieldChange, AttackChange
    }

    public List<TargetType> targetTypes = new List<TargetType>();
    public List<TriggerType> triggerTypes = new List<TriggerType>();
    public List<AbilityType> abilityTypes = new List<AbilityType>();
    public List<int> abilityValue = new List<int>();

    private SpriteRenderer sprite;
    private bool hasAbility;
    private float startTime;

    public void Awake()
    {
        /*
        try
        {
            sprite = GetComponent<PlayerController>().sprite;
        }
        catch
        {
            sprite = GetComponent<EnemyController>().sprite;
        }
        */

        sprite = GetComponent<HealthController>().charDisplay.sprite;

        if (targetTypes.Count > 0)
            hasAbility = true;
        else
            hasAbility = false;
        startTime = Time.time;
    }

    private void Update()
    {
        float t = Time.time - startTime;
        if (hasAbility)
            sprite.color = Color.Lerp(Color.white, new Color(0.6f, 0.6f, 0.6f), (Mathf.Sin(3 * t) + 1) / 2.0f);
    }

    public void TriggerAbilities(TriggerType type)
    {
        for (int i = 0; i < abilityTypes.Count; i++)
        {
            if (triggerTypes[i] == type)
            {
                List<GameObject> target = GetTargets(targetTypes[i]);

                foreach (GameObject obj in target)
                {
                    if (obj == null)    //If trying to access an object that's already dead
                        continue;

                    switch (abilityTypes[i])
                    {
                        case AbilityType.VitChange:
                            obj.GetComponent<HealthController>().TakePiercingDamage(abilityValue[i], GetComponent<HealthController>());
                            break;
                        case AbilityType.ShieldChange:
                            obj.GetComponent<HealthController>().SetBonusShield(abilityValue[i]);
                            break;
                        case AbilityType.AttackChange:
                            obj.GetComponent<HealthController>().SetBonusAttack(abilityValue[i]);
                            break;
                    }
                }
            }
        }
    }

    private List<GameObject> GetTargets(TargetType type)
    {
        switch (type)
        {
            case TargetType.Creator:
                return new List<GameObject>() { GetComponent<HealthController>().GetCreator() };
            default:
                return new List<GameObject>() { this.gameObject };
        }
    }

    public List<string> GetAbilityStrings()
    {
        List<string> output = new List<string>();
        for (int i = 0; i < triggerTypes.Count; i++)
        {
            string s = "";
            switch (triggerTypes[i])
            {
                case TriggerType.OnDeath:
                    s += "Deathrattle: ";
                    break;
                case TriggerType.OnSacrifice:
                    s += "Sacrifice: ";
                    break;
            }

            switch (targetTypes[i])
            {
                case TargetType.Creator:
                    s += "Summoner ";
                    break;
            }

            if (abilityValue[i] > 0)
                s += "Gain " + abilityValue[i] + " ";
            else
                s += "Lose " + Mathf.Abs(abilityValue[i]) + " ";

            switch (abilityTypes[i])
            {
                case AbilityType.AttackChange:
                    s += "ATK";
                    break;
                case AbilityType.ShieldChange:
                    s += "Armor";
                    break;
                case AbilityType.VitChange:
                    s += "Health";
                    break;
            }

            output.Add(s);
        }
        return output;
    }
}
