using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharEnergyCostReductionBuff : Buff
{
    public override void OnApply(HealthController healthController, int value, int duration, bool fromRelic)
    {
        color = Color.blue;
        description = "Energy Reduction: Your energy cards cost {v} less for {d} turn(s)";
        triggerType = Buff.TriggerType.AtEndOfTurn;
        durationType = Buff.DurationType.Turn;

        base.duration = duration;
        healthController.buffController.AddBuff(this);
        healthController.SetEnergyCostReduction(value);
        tempValue = value;
    }

    public override IEnumerator Trigger(HealthController selfHealthController, HealthController attackerHealthController, int value)
    {
        yield return new WaitForSeconds(0);
    }

    public override void Revert(HealthController healthController)
    {
        healthController.SetEnergyCostReduction(-tempValue);
    }

    public override Buff GetCopy()
    {
        Buff output = new CharEnergyCostReductionBuff();
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