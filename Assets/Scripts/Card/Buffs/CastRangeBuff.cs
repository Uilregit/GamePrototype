using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastRangeBuff : Buff
{
    public override void OnApply(HealthController healthController, int value, int duration, bool fromRelic)
    {
        color = Color.blue;
        description = "Cast Range: Modify cast range by {v} for {d} turns";
        triggerType = Buff.TriggerType.AtEndOfTurn;
        durationType = Buff.DurationType.Turn;

        base.duration = duration;
        healthController.buffController.AddBuff(this);
        healthController.SetBonusCastRange(value);
        tempValue = value;
    }

    public override IEnumerator Trigger(HealthController selfHealthController, HealthController attackerHealthController, int value)
    {
        yield return new WaitForSeconds(0);
    }

    public override void Revert(HealthController healthController)
    {
        healthController.SetBonusCastRange(-tempValue);
    }

    public override Buff GetCopy()
    {
        Buff output = new CastRangeBuff();
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
