using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharManaCostCapTurnBuff : Buff
{
    public override void OnApply(HealthController healthController, int value, int duration, bool fromRelic)
    {
        color = Color.yellow;
        description = "Mana Cap: Your mana cards that costs more than {v} cost {v} for {d} turn(s)";
        triggerType = Buff.TriggerType.AtEndOfTurn;
        durationType = Buff.DurationType.Turn;

        base.duration = duration;
        healthController.buffController.AddBuff(this);
        healthController.SetManaCostCap(value);
        tempValue = value;
    }

    public override IEnumerator Trigger(HealthController selfHealthController, HealthController attackerHealthController, int value)
    {
        yield return new WaitForSeconds(0);
    }

    public override void Revert(HealthController healthController)
    {
        healthController.RemoveManaCostCap(tempValue);
    }

    public override Buff GetCopy()
    {
        Buff output = new CharManaCostCapTurnBuff();
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
