using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffFactory : MonoBehaviour
{
    private Buff buff;

    public int duration;
    public int cardValue;

    private Color dummyColor;
    private string dummyDescription;
    private bool isDummy;

    public string cardName = "";
    public string casterColor = "";
    public string casterName;
    public int triggerCount = 1;

    private bool reverted = false;
    private HealthController casterHealthController;

    public void SetBuff(Buff newBuff)
    {
        buff = newBuff;
        if (buff.GetTriggerType() == Buff.TriggerType.AtEndOfTurn || buff.GetTriggerType() == Buff.TriggerType.AtStartOfTurn)
            buff.durationType = Buff.DurationType.Use;
    }

    public Buff.BuffEffectType GetTriggerEffectType()
    {
        return buff.onTriggerEffects;
    }

    public virtual void OnApply(HealthController healthController, HealthController newCasterHealthController, int value, int newDuration, bool fromRelic, List<BuffFactory> traceList, List<Relic> relicTrace)
    {
        casterHealthController = newCasterHealthController;
        healthController.GetBuffController().AddBuff(this);
        cardValue = value;
        duration = newDuration;
        if (healthController != null && newCasterHealthController != null)
            if (healthController.isPlayer != newCasterHealthController.isPlayer && buff.GetTriggerType() == Buff.TriggerType.AtStartOfTurn && !buff.triggerModificationProtection)   //Bonus duration used for when player puts buff on enemy or when enemy puts buff on player, avoids 1 extra turn issue
                buff.SetTriggerType(Buff.TriggerType.AtEndOfTurn);

        int vitDamage = 0;
        int armorDamage = 0;

        switch (buff.onApplyEffects)
        {
            case Buff.BuffEffectType.None:
                break;
            case Buff.BuffEffectType.VitDamage:
                vitDamage += healthController.GetSimulatedVitDamage(GetValue(value));
                armorDamage += healthController.GetSimulatedArmorDamage(GetValue(value));
                healthController.TakeVitDamage(GetValue(cardValue), healthController, traceList, relicTrace);
                break;
            case Buff.BuffEffectType.PiercingDamage:
                vitDamage += healthController.GetSimulatedPiercingDamage(GetValue(value));
                healthController.TakePiercingDamage(GetValue(cardValue), healthController, traceList, relicTrace);
                break;
            case Buff.BuffEffectType.ArmorDamage:
                armorDamage += healthController.GetSimulatedArmorDamage(GetValue(value));
                healthController.TakeArmorDamage(GetValue(cardValue), healthController, traceList, relicTrace);
                break;
            case Buff.BuffEffectType.VitDamageMultiplier:
                healthController.AddVitDamageMultiplier(cardValue);
                break;
            case Buff.BuffEffectType.HealingMultiplier:
                healthController.AddHealingMultiplier(cardValue);
                break;
            case Buff.BuffEffectType.ArmorDamageMultiplier:
                healthController.AddArmorDamageMultiplier(cardValue);
                break;
            case Buff.BuffEffectType.AllDamageMultiplier:
                healthController.AddVitDamageMultiplier(cardValue);
                healthController.AddArmorDamageMultiplier(cardValue);
                break;
            case Buff.BuffEffectType.PhasedMovement:
                healthController.SetPhasedMovement(true);
                break;
            case Buff.BuffEffectType.BonusAttack:
                healthController.SetBonusAttack(cardValue, true);
                break;
            case Buff.BuffEffectType.BonusArmor:
                healthController.SetBonusArmor(cardValue, relicTrace, true);
                break;
            case Buff.BuffEffectType.BonusCastRange:
                healthController.SetBonusCastRange(cardValue);
                break;
            case Buff.BuffEffectType.BonusMoveRange:
                healthController.SetBonusMoveRange(cardValue);
                break;
            case Buff.BuffEffectType.CharEnergyCap:
                healthController.SetEnergyCostCap(cardValue);
                break;
            case Buff.BuffEffectType.CharManaCap:
                healthController.SetManaCostCap(cardValue);
                break;
            case Buff.BuffEffectType.CharEnergyReduction:
                healthController.SetEnergyCostReduction(cardValue);
                break;
            case Buff.BuffEffectType.CharManaReduction:
                healthController.SetManaCostReduction(cardValue);
                break;
            case Buff.BuffEffectType.PartyEnergyCap:
                TurnController.turnController.SetEnergyCostCap(cardValue);
                break;
            case Buff.BuffEffectType.PartyManaCap:
                TurnController.turnController.SetManaCostCap(cardValue);
                break;
            case Buff.BuffEffectType.PartyEnergyReduction:
                TurnController.turnController.SetEnergyReduction(cardValue);
                break;
            case Buff.BuffEffectType.PartyManaReduction:
                TurnController.turnController.SetManaReduction(cardValue);
                break;
            case Buff.BuffEffectType.CardDrawChange:
                TurnController.turnController.SetCardDrawChange(cardValue);
                break;
            case Buff.BuffEffectType.BonusCast:
                try
                {
                    healthController.GetComponent<PlayerController>();
                    TurnController.turnController.SetPlayerBonusCast(cardValue);
                }
                catch
                {
                    TurnController.turnController.SetEnemyBonusCast(cardValue);
                }
                break;
            case Buff.BuffEffectType.Disarm:
                healthController.SetDisarmed(true);
                break;
            case Buff.BuffEffectType.Silence:
                healthController.SetSilenced(true);
                break;
            case Buff.BuffEffectType.Stun:
                healthController.SetStunned(true);
                break;
            case Buff.BuffEffectType.Preserve:
                healthController.SetPreserveBonusVit(true);
                break;
            case Buff.BuffEffectType.Taunt:
                healthController.SetTauntTarget(casterHealthController);
                try { healthController.GetComponent<EnemyInformationController>().RefreshIntent(); }
                catch { }
                break;
            case Buff.BuffEffectType.ApplyBuff:
                BuffFactory thisBuff = new BuffFactory();
                thisBuff.SetBuff(buff.appliedBuff);
                thisBuff.OnApply(healthController, casterHealthController, cardValue, buff.appliedBuffDuration, false, traceList, relicTrace);
                break;
            default:
                Debug.Log(buff.onApplyEffects);
                Debug.Log("Apply Not implimented");
                break;
        }

        foreach (EnemyController enemy in TurnController.turnController.GetEnemies())
            enemy.GetComponent<EnemyInformationController>().RefreshIntent();

        try
        {
            InformationLogger.infoLogger.SaveCombatInfo(InformationLogger.infoLogger.patchID,
                                        InformationLogger.infoLogger.gameID,
                                        RoomController.roomController.worldLevel.ToString(),
                                        RoomController.roomController.selectedLevel.ToString(),
                                        RoomController.roomController.roomName,
                                        TurnController.turnController.turnID.ToString(),
                                        TurnController.turnController.GetNumerOfCardsPlayedInTurn().ToString(),
                                        casterColor,
                                        cardName,
                                        "False",
                                        "False",
                                        "False",
                                        "True",
                                        casterName,
                                        1.ToString(),
                                        healthController.name,
                                        vitDamage.ToString(),
                                        armorDamage.ToString(),
                                        0.ToString(),
                                        0.ToString(),
                                        triggerCount.ToString());
        }
        catch { }
    }

    //public abstract IEnumerator Trigger(HealthController selfHealthController, HealthController attackerHealthController, int value, List<Buff> buffTrace);
    public virtual IEnumerator Trigger(HealthController selfHealthController, HealthController attackerHealthController, int value, List<BuffFactory> traceList, List<Relic> relicTrace)
    {
        HealthController target = null;
        if (buff.onTriggerTarget == Buff.TriggerTarget.Self)
            target = selfHealthController;
        else if (buff.onTriggerTarget == Buff.TriggerTarget.Attacker)
            target = attackerHealthController;
        else if (buff.onTriggerTarget == Buff.TriggerTarget.Caster)
            target = casterHealthController;

        if (target == null)
            yield break;

        if (traceList == null)
            traceList = new List<BuffFactory>();
        traceList.Add(this);

        int vitDamage = 0;
        int armorDamage = 0;

        switch (buff.onTriggerEffects)
        {
            case Buff.BuffEffectType.None:
                break;
            case Buff.BuffEffectType.VitDamage:
                vitDamage += target.GetSimulatedVitDamage(GetValue(value));
                armorDamage += target.GetSimulatedArmorDamage(GetValue(value));
                target.TakeVitDamage(GetValue(value), selfHealthController, traceList, null, (GetTriggerType() == Buff.TriggerType.AtEndOfTurn || GetTriggerType() == Buff.TriggerType.AtStartOfTurn));
                break;
            case Buff.BuffEffectType.PiercingDamage:
                vitDamage += target.GetSimulatedPiercingDamage(GetValue(value));
                target.TakePiercingDamage(GetValue(value), selfHealthController, traceList);
                break;
            case Buff.BuffEffectType.ArmorDamage:
                armorDamage += target.GetSimulatedArmorDamage(GetValue(value));
                target.TakeArmorDamage(GetValue(value), selfHealthController, traceList);
                break;
            case Buff.BuffEffectType.VitDamageMultiplier:
                target.AddVitDamageMultiplier(GetValue(value));
                break;
            case Buff.BuffEffectType.HealingMultiplier:
                target.AddHealingMultiplier(GetValue(value));
                break;
            case Buff.BuffEffectType.ArmorDamageMultiplier:
                target.AddArmorDamageMultiplier(GetValue(value));
                break;
            case Buff.BuffEffectType.ApplyBuff:
                BuffFactory thisBuff = new BuffFactory();
                thisBuff.SetBuff(buff.appliedBuff);
                thisBuff.OnApply(target, attackerHealthController, GetValue(value), buff.appliedBuffDuration, false, traceList, relicTrace);
                break;
            case Buff.BuffEffectType.DrawCard:
                List<Card> spawnCards = new List<Card>();
                foreach (Card spawnCard in buff.GetDrawnCards())
                    if (spawnCard != null)
                        spawnCards.Add(spawnCard);
                if (spawnCards.Count == 0)
                    HandController.handController.DrawAnyCard();
                else
                {
                    foreach (Card card in spawnCards)
                    {
                        CardController temp = selfHealthController.gameObject.AddComponent<CardController>();
                        temp.SetCard(card, true, false);
                        HandController.handController.CreateSpecificCard(temp);
                    }
                }
                yield return HandController.handController.StartCoroutine(HandController.handController.ResolveDrawQueue());
                break;
            case Buff.BuffEffectType.CreateFireTrap:
                GameObject obj = GameObject.Instantiate(buff.obj, selfHealthController.GetPreviousPosition(), Quaternion.identity);
                obj.transform.parent = CanvasController.canvasController.boardCanvas.transform;

                foreach (TrapController t in GridController.gridController.traps)
                    if (t.transform.position == obj.transform.position)
                    {
                        obj.gameObject.SetActive(false);
                        break;
                    }
                int index = 0;
                for (int i = 0; i < buff.card.cardEffectName.Length; i++)
                    if (buff.card.cardEffectName[i] == Card.EffectType.CreateObject)
                    {
                        index = i;
                        break;
                    }
                obj.GetComponent<TrapController>().SetValues(selfHealthController.gameObject, buff.card, index + 1);
                GridController.gridController.traps.Add(obj.GetComponent<TrapController>());
                break;
            default:
                Debug.Log(buff.onApplyEffects);
                Debug.Log("Trigger Not implimented");
                break;
        }

        try
        {
            InformationLogger.infoLogger.SaveCombatInfo(InformationLogger.infoLogger.patchID,
                                                InformationLogger.infoLogger.gameID,
                                                RoomController.roomController.worldLevel.ToString(),
                                                RoomController.roomController.selectedLevel.ToString(),
                                                RoomController.roomController.roomName,
                                                TurnController.turnController.turnID.ToString(),
                                                TurnController.turnController.GetNumerOfCardsPlayedInTurn().ToString(),
                                                casterColor,
                                                cardName,
                                                "False",
                                                "False",
                                                "False",
                                                "True",
                                                casterName,
                                                1.ToString(),
                                                selfHealthController.name,
                                                vitDamage.ToString(),
                                                armorDamage.ToString(),
                                                0.ToString(),
                                                0.ToString(),
                                                triggerCount.ToString());
        }
        catch { }
        triggerCount += 1;

        yield return new WaitForSeconds(0);
    }
    public virtual void Revert(HealthController healthController)
    {
        if (reverted)               //Prevent double reverting in special circumstances
            return;

        int vitDamage = 0;
        int armorDamage = 0;

        switch (buff.onApplyEffects)
        {
            case Buff.BuffEffectType.None:
                break;
            case Buff.BuffEffectType.VitDamage:
                vitDamage += healthController.GetSimulatedVitDamage(-cardValue);
                armorDamage += healthController.GetSimulatedArmorDamage(-cardValue);
                healthController.TakeVitDamage(-cardValue, healthController);
                break;
            case Buff.BuffEffectType.PiercingDamage:
                vitDamage += healthController.GetSimulatedPiercingDamage(-cardValue);
                healthController.TakePiercingDamage(-cardValue, healthController);
                break;
            case Buff.BuffEffectType.ArmorDamage:
                armorDamage += healthController.GetSimulatedArmorDamage(-cardValue);
                healthController.TakeArmorDamage(-cardValue, healthController);
                break;
            case Buff.BuffEffectType.VitDamageMultiplier:
                healthController.RemoveVitDamageMultiplier(-cardValue);
                break;
            case Buff.BuffEffectType.HealingMultiplier:
                healthController.RemoveHealingMultiplier(-cardValue);
                break;
            case Buff.BuffEffectType.ArmorDamageMultiplier:
                healthController.RemoveArmorDamageMultiplier(-cardValue);
                break;
            case Buff.BuffEffectType.AllDamageMultiplier:
                healthController.RemoveVitDamageMultiplier(cardValue);
                healthController.RemoveArmorDamageMultiplier(cardValue);
                break;
            case Buff.BuffEffectType.PhasedMovement:
                healthController.SetPhasedMovement(false);
                break;
            case Buff.BuffEffectType.BonusAttack:
                healthController.SetBonusAttack(-cardValue, true);
                break;
            case Buff.BuffEffectType.BonusArmor:
                healthController.SetBonusArmor(-cardValue, null, true);
                break;
            case Buff.BuffEffectType.BonusCastRange:
                healthController.SetBonusCastRange(-cardValue);
                break;
            case Buff.BuffEffectType.CharEnergyCap:
                healthController.SetEnergyCostCap(-cardValue);
                break;
            case Buff.BuffEffectType.CharManaCap:
                healthController.SetManaCostCap(-cardValue);
                break;
            case Buff.BuffEffectType.CharEnergyReduction:
                healthController.SetEnergyCostReduction(-cardValue);
                break;
            case Buff.BuffEffectType.CharManaReduction:
                healthController.SetManaCostReduction(-cardValue);
                break;
            case Buff.BuffEffectType.PartyEnergyCap:
                TurnController.turnController.SetEnergyCostCap(-cardValue);
                break;
            case Buff.BuffEffectType.PartyManaCap:
                TurnController.turnController.SetManaCostCap(-cardValue);
                break;
            case Buff.BuffEffectType.PartyEnergyReduction:
                TurnController.turnController.SetEnergyReduction(-cardValue);
                break;
            case Buff.BuffEffectType.PartyManaReduction:
                TurnController.turnController.SetManaReduction(-cardValue);
                break;
            case Buff.BuffEffectType.CardDrawChange:
                TurnController.turnController.SetCardDrawChange(-cardValue);
                break;
            case Buff.BuffEffectType.BonusCast:
                try
                {
                    healthController.GetComponent<PlayerController>();
                    TurnController.turnController.SetPlayerBonusCast(-cardValue);
                }
                catch
                {
                    TurnController.turnController.SetEnemyBonusCast(-cardValue);
                }
                break;
            case Buff.BuffEffectType.Disarm:
                healthController.SetDisarmed(false);
                break;
            case Buff.BuffEffectType.Silence:
                healthController.SetSilenced(false);
                break;
            case Buff.BuffEffectType.Stun:
                healthController.SetStunned(false);
                break;
            case Buff.BuffEffectType.Preserve:
                healthController.SetPreserveBonusVit(false);
                break;
            case Buff.BuffEffectType.Taunt:
                healthController.SetTauntTarget(null);
                try { healthController.GetComponent<EnemyInformationController>().RefreshIntent(); }
                catch { }
                break;
            case Buff.BuffEffectType.BonusMoveRange:
                healthController.SetBonusMoveRange(-cardValue);
                break;
            default:
                Debug.Log(buff.onApplyEffects);
                Debug.Log("Revert Not implimented");
                break;
        }

        try
        {
            InformationLogger.infoLogger.SaveCombatInfo(InformationLogger.infoLogger.patchID,
                                InformationLogger.infoLogger.gameID,
                                RoomController.roomController.worldLevel.ToString(),
                                RoomController.roomController.selectedLevel.ToString(),
                                RoomController.roomController.roomName,
                                TurnController.turnController.turnID.ToString(),
                                TurnController.turnController.GetNumerOfCardsPlayedInTurn().ToString(),
                                casterColor,
                                cardName,
                                "False",
                                "False",
                                "False",
                                "True",
                                casterName,
                                1.ToString(),
                                healthController.name,
                                vitDamage.ToString(),
                                armorDamage.ToString(),
                                0.ToString(),
                                0.ToString(),
                                triggerCount.ToString());
        }
        catch { }

        reverted = true;
    }

    private int GetValue(int triggerValue)
    {
        int output = 0;
        switch (buff.valueManipulationType)
        {
            case Buff.ValueManipulationType.TriggerValue:
                output = triggerValue;
                break;
            case Buff.ValueManipulationType.NegativeTriggerValue:
                output = -triggerValue;
                break;
            case Buff.ValueManipulationType.CardValue:
                output = cardValue;
                break;
            case Buff.ValueManipulationType.NegativeCardValue:
                output = -cardValue;
                break;
            case Buff.ValueManipulationType.Percentage:
                output = -Mathf.CeilToInt(triggerValue * cardValue / 100.0f);
                break;
            case Buff.ValueManipulationType.NegativePercentage:
                output = -Mathf.CeilToInt(triggerValue * cardValue / 100.0f);
                break;
            default:
                output = triggerValue;
                break;
        }
        return output;
    }

    public virtual BuffFactory GetCopy()
    {
        Buff output = new Buff();
        output.tempValue = buff.tempValue;
        output.duration = buff.duration;
        //output.value = value;
        output.color = buff.color;
        output.description = buff.description;
        output.onApplyEffects = buff.onApplyEffects;
        output.triggerModificationProtection = buff.triggerModificationProtection;
        output.SetTriggerType(buff.GetTriggerType());
        output.onTriggerEffects = buff.onTriggerEffects;
        output.valueManipulationType = buff.valueManipulationType;
        output.durationType = buff.durationType;
        output.appliedBuff = buff.appliedBuff;
        output.appliedBuffDuration = buff.appliedBuffDuration;
        output.effectValue = buff.effectValue;
        output.tempValue = buff.tempValue;
        output.value = buff.value;
        output.obj = buff.obj;
        output.card = buff.card;

        BuffFactory buffFactory = new BuffFactory();
        buffFactory.buff = output;
        buffFactory.duration = duration;
        buffFactory.cardName = cardName;
        buffFactory.cardValue = cardValue;
        return buffFactory;
    }

    public Buff.TriggerType GetTriggerType()
    {
        return buff.GetTriggerType();
    }

    public Buff.DurationType GetDurationType()
    {
        return buff.durationType;
    }


    public virtual Color GetIconColor()
    {
        if (buff.GetTriggerType() == Buff.TriggerType.Dummy)
            return dummyColor;
        return buff.color;
    }

    public virtual string GetDescription()
    {
        if (buff.GetTriggerType() == Buff.TriggerType.Dummy)
            return dummyDescription;
        return buff.description.Replace("{+-v}", cardValue.ToString("+#;-#;0")).Replace("{|v|}", Mathf.Abs(cardValue).ToString()).Replace("{v}", cardValue.ToString());
    }

    public virtual void MultiplyValue(HealthController health, int multiplier)
    {
        int diff = cardValue = Mathf.CeilToInt(cardValue * multiplier / 100.0f) - cardValue;
        switch (buff.onApplyEffects)
        {
            case Buff.BuffEffectType.BonusAttack:
                health.SetBonusArmor(diff, null, true);
                break;
            case Buff.BuffEffectType.BonusArmor:
                health.SetBonusAttack(diff, true);
                break;
            case Buff.BuffEffectType.BonusCastRange:
                health.SetBonusCastRange(diff);
                break;
            case Buff.BuffEffectType.BonusMoveRange:
                health.SetBonusMoveRange(diff);
                break;
            case Buff.BuffEffectType.CharEnergyReduction:
                health.SetEnergyCostReduction(diff);
                break;
            case Buff.BuffEffectType.CharManaReduction:
                health.SetManaCostReduction(diff);
                break;
            case Buff.BuffEffectType.PartyEnergyReduction:
                TurnController.turnController.SetEnergyReduction(diff);
                break;
            case Buff.BuffEffectType.PartyManaReduction:
                TurnController.turnController.SetManaReduction(diff);
                break;
        }
        cardValue = Mathf.CeilToInt(cardValue * multiplier / 100.0f);
    }

    public virtual void AddToValue(HealthController health, int addition)
    {
        int diff = addition;
        switch (buff.onApplyEffects)
        {
            case Buff.BuffEffectType.BonusAttack:
                health.SetBonusArmor(diff, null, true);
                break;
            case Buff.BuffEffectType.BonusArmor:
                health.SetBonusAttack(diff, true);
                break;
            case Buff.BuffEffectType.BonusCastRange:
                health.SetBonusCastRange(diff);
                break;
            case Buff.BuffEffectType.BonusMoveRange:
                health.SetBonusMoveRange(diff);
                break;
            case Buff.BuffEffectType.CharEnergyReduction:
                health.SetEnergyCostReduction(diff);
                break;
            case Buff.BuffEffectType.CharManaReduction:
                health.SetManaCostReduction(diff);
                break;
            case Buff.BuffEffectType.PartyEnergyReduction:
                TurnController.turnController.SetEnergyReduction(diff);
                break;
            case Buff.BuffEffectType.PartyManaReduction:
                TurnController.turnController.SetManaReduction(diff);
                break;
        }
        cardValue += addition;
    }

    public Buff GetBuff()
    {
        return buff;
    }

    public void SetDummyInfo(Color color, string desc)
    {
        dummyColor = color;
        dummyDescription = desc;
        isDummy = true;
    }

    public bool GetIsDummy()
    {
        return isDummy;
    }
}
