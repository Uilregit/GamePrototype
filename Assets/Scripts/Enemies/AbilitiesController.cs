﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public class AbilitiesController : MonoBehaviour
{
    public enum TargetType
    {
        Self = 0,
        AllPlayers = 10,
        AllEnemies = 20,
        HandController = 30,
        Creator = 100,
    }

    public enum ConditionType
    {
        None = 0,
        AnotherCopyAlive = 50
    }

    public enum TriggerType
    {
        OnDeath = 0,
        OnSacrifice = 1,
        OnBreak = 5,
        OnBelow0Health = 6,
        AtEndOfTurn = 10,
        OnSpawn = 1000,
    }
    public enum AbilityType
    {
        VitChange = 0,
        ArmorChange = 1,
        AttackChange = 2,

        FullHeal = 10,
        Break = 11,

        PhasedMovement = 20,

        Revive = 99,

        GetSynergizedCards = 999
    }

    public List<string> abilityNames = new List<string>();
    public List<Sprite> abilitySprites = new List<Sprite>();
    public List<TargetType> targetTypes = new List<TargetType>();
    public List<ConditionType> conditionTypes = new List<ConditionType>();
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
    /*
    private void Update()
    {
        float t = Time.time - startTime;
        if (hasAbility)
            sprite.color = Color.Lerp(Color.white, new Color(0.6f, 0.6f, 0.6f), (Mathf.Sin(3 * t) + 1) / 2.0f);
    }
    */

    public void TriggerAbilities(TriggerType type)
    {
        for (int i = 0; i < abilityTypes.Count; i++)
        {
            if (triggerTypes[i] == type && CheckIfConditionMet(conditionTypes[i]))
            {
                List<GameObject> target = GetTargets(targetTypes[i]);

                foreach (GameObject obj in target)
                {
                    if (obj == null)    //If trying to access an object that's already dead
                        continue;

                    switch (abilityTypes[i])
                    {
                        case AbilityType.VitChange:
                            obj.GetComponent<HealthController>().TakePiercingDamage(-abilityValue[i], GetComponent<HealthController>());
                            break;
                        case AbilityType.ArmorChange:
                            obj.GetComponent<HealthController>().SetBonusArmor(abilityValue[i], null, true);
                            break;
                        case AbilityType.AttackChange:
                            obj.GetComponent<HealthController>().SetBonusAttack(abilityValue[i], true);
                            break;
                        case AbilityType.Break:
                            obj.GetComponent<HealthController>().TakeArmorDamage(9999999, null);
                            break;
                        case AbilityType.FullHeal:
                            obj.GetComponent<HealthController>().TakePiercingDamage(obj.GetComponent<HealthController>().GetCurrentVit() - obj.GetComponent<HealthController>().GetMaxVit(), obj.GetComponent<HealthController>());
                            break;
                        case AbilityType.Revive:
                            GameObject newObj = GameObject.Instantiate(RoomController.roomController.GetObjectPrefab(obj), obj.transform.position, Quaternion.identity);

                            newObj.transform.parent = CanvasController.canvasController.boardCanvas.transform;
                            newObj.GetComponent<HealthController>().SetCreator(this.gameObject);
                            try
                            {
                                newObj.GetComponent<EnemyController>().SetSkipInitialIntent(true);
                                newObj.GetComponent<EnemyController>().Spawn(obj.transform.position);
                            }
                            catch
                            {
                                newObj.GetComponent<PlayerController>().Spawn(obj.transform.position);
                            }
                            break;
                        case AbilityType.PhasedMovement:
                            obj.GetComponent<HealthController>().SetPhasedMovement(true);
                            break;
                        case AbilityType.GetSynergizedCards:
                            string color = PartyController.party.GetPlayerColorTexts()[Random.Range(0, 2)];
                            List<Card> synergizedCards = LootController.loot.GetSynergizedCards(color);
                            List<int> indexes = new List<int>();
                            while (indexes.Count < abilityValue[i])
                            {
                                int newIndex = Random.Range(0, synergizedCards.Count - 1);
                                if (!indexes.Contains(newIndex))
                                    indexes.Add(newIndex);
                            }
                            foreach (int index in indexes)
                            {
                                Card c = synergizedCards[index].GetCopy();
                                c.exhaust = true;
                                CardController cc = HandController.handController.gameObject.AddComponent<CardController>();
                                cc.SetCard(c, true, false);
                                cc.SetManaCostCap(0);
                                cc.SetEnergyCostCap(0);
                                HandController.handController.CreateSpecificCard(cc);
                            }
                            HandController.handController.StartCoroutine(HandController.handController.ResolveDrawQueue());
                            break;
                    }
                }
            }
        }
    }

    private bool CheckIfConditionMet(ConditionType type)
    {
        switch (type)
        {
            case ConditionType.None:
                return true;
            case ConditionType.AnotherCopyAlive:
                if (TurnController.turnController.GetEnemies().Any(x => x.gameObject.name.Contains(this.gameObject.name) && x.GetComponent<HealthController>().GetCurrentVit() > 0))
                    return true;
                else
                    return false;
        }
        return false;
    }

    private List<GameObject> GetTargets(TargetType type)
    {
        switch (type)
        {
            case TargetType.Creator:
                return new List<GameObject>() { GetComponent<HealthController>().GetCreator() };
            case TargetType.AllEnemies:
                List<GameObject> output = new List<GameObject>();
                foreach (EnemyController obj in TurnController.turnController.GetEnemies())
                    output.Add(obj.gameObject);
                return output;
            case TargetType.AllPlayers:
                return GameController.gameController.GetLivingPlayers();
            case TargetType.HandController:
                return new List<GameObject>() { HandController.handController.gameObject };
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
                    s += "<b>Deathrattle:</b> ";
                    break;
                case TriggerType.OnSacrifice:
                    s += "<b>Sacrifice:</b> ";
                    break;
                case TriggerType.AtEndOfTurn:
                    s += "<b>End of turn:</b> ";
                    break;
            }

            switch (conditionTypes[i])
            {
                case ConditionType.AnotherCopyAlive:
                    s += "If Another Copy Is Alive, ";
                    break;
            }

            switch (targetTypes[i])
            {
                case TargetType.Creator:
                    s += "Summoner ";
                    break;
                case TargetType.AllEnemies:
                    s += "All Enemies ";
                    break;
                case TargetType.AllPlayers:
                    s += "All Players ";
                    break;
                    //case TargetType.Self:
                    //    s += "You ";
                    //    break;
            }

            if (abilityValue[i] > 0)
                s += "Gain " + abilityValue[i] + " ";
            else if (abilityValue[i] < 0)
                s += "Lose " + Mathf.Abs(abilityValue[i]) + " ";

            switch (abilityTypes[i])
            {
                case AbilityType.AttackChange:
                    s += "ATK";
                    break;
                case AbilityType.ArmorChange:
                    s += "Armor";
                    break;
                case AbilityType.VitChange:
                    s += "Health";
                    break;
                case AbilityType.FullHeal:
                    s += "Restore To Full Health";
                    break;
                case AbilityType.Break:
                    s += "Become Broken";
                    break;
                case AbilityType.Revive:
                    s += "Revive";
                    break;
                case AbilityType.GetSynergizedCards:
                    s += "Cards From The Future That Costs (0)";
                    break;
                case AbilityType.PhasedMovement:
                    s += "Permanent Phased Movement";
                    break;
            }

            output.Add(s);
        }
        return output;
    }

    public List<string> GetAbilityNames()
    {
        return abilityNames;
    }

    public List<Sprite> GetAbilitySprites()
    {
        return abilitySprites;
    }

    public List<Card> GetAbilityCards()
    {
        List<Card> output = new List<Card>();
        List<string> abilityStrings = GetAbilityStrings();
        for (int i = 0; i < abilityTypes.Count; i++)
        {
            Card passive = new Card();
            passive.casterColor = Card.CasterColor.Passive;
            passive.name = abilityNames[i];
            passive.art = abilitySprites[i];
            passive.description = "<b>Passive</b>|" + abilityStrings[i];
            output.Add(passive);
        }
        return output;
    }
}
