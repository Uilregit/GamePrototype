using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdditionalHealingBuff : Buff
{
    public override void OnApply(HealthController healthController, int value, int duration, bool fromRelic)
    {
        color = Color.green;
        description = "Additional healing: Restore {v} more health when healed for {d} turns";
        triggerType = Buff.TriggerType.OnHealingRecieved;
        durationType = Buff.DurationType.Turn;

        base.duration = duration;
        healthController.buffController.AddBuff(this);
        tempValue = value;
    }

    public override IEnumerator Trigger(HealthController selfHealthController, HealthController attackerHealthController, int value)
    {
        selfHealthController.TakePiercingDamage(-value, selfHealthController);
        yield return new WaitForSeconds(0);
    }

    public override void Revert(HealthController healthController)
    {
    }

    public override Buff GetCopy()
    {
        Buff output = new AdditionalHealingBuff();
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