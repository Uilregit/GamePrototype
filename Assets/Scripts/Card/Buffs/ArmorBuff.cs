using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmorBuff : Buff
{
    public override void OnApply(HealthController healthController, int value, int duration, bool fromRelic)
    {
        color = Color.blue;
        description = "Armor: Modify armor by {v} for {d} turn(s)";
        triggerType = Buff.TriggerType.AtStartOfTurn;
        durationType = Buff.DurationType.Turn;

        base.duration = duration;
        healthController.buffController.AddBuff(this);
        healthController.SetBonusShield(value, fromRelic);
        tempValue = value;
    }

    public override IEnumerator Trigger(HealthController selfHealthController, HealthController attackerHealthController, int value)
    {
        yield return new WaitForSeconds(0);
    }

    public override void Revert(HealthController healthController)
    {
        healthController.SetBonusShield(-tempValue);
    }

    public override void AddToValue(HealthController healthController, int addition)
    {
        healthController.SetBonusShield(addition, false);
        base.AddToValue(healthController, addition);
    }

    public override void MultiplyValue(HealthController healthController, int multiplier)
    {
        healthController.SetBonusShield(Mathf.CeilToInt(tempValue * multiplier / 100.0f) - tempValue, false);
        base.MultiplyValue(healthController, multiplier);
    }

    public override Buff GetCopy()
    {
        Buff output = new ArmorBuff();
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
