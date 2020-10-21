using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackChangeBuff : Buff
{
    public override void OnApply(HealthController healthController, int value, int duration, bool fromRelic)
    {
        base.color = Color.red;
        base.description = "Attack: Modify attack by {+-v} for {d} turn(s)";
        triggerType = Buff.TriggerType.AtStartOfTurn;
        durationType = Buff.DurationType.Turn;

        healthController.buffController.AddBuff(this);
        healthController.SetBonusAttack(value);
        tempValue = value;
        base.duration = duration;
    }

    public override IEnumerator Trigger(HealthController selfHealthController, HealthController attackerHealthController, int value)
    {
        yield return new WaitForSeconds(0);
    }

    public override void Revert(HealthController healthController)
    {
        healthController.SetBonusAttack(-tempValue);
    }

    public override void AddToValue(HealthController healthController, int addition)
    {
        healthController.SetBonusAttack(addition);
        base.AddToValue(healthController, addition);
    }

    public override void MultiplyValue(HealthController healthController, int multiplier)
    {
        healthController.SetBonusAttack(Mathf.CeilToInt(tempValue * multiplier / 100.0f) - tempValue);
        base.MultiplyValue(healthController, multiplier);
    }

    public override Buff GetCopy()
    {
        Buff output = new AttackChangeBuff();
        output.tempValue = tempValue;
        output.duration = duration;
        output.value = value;
        output.color = color;
        output.description = description;
        output.triggerType = triggerType;
        output.durationType = durationType;

        return output;
    }
}
