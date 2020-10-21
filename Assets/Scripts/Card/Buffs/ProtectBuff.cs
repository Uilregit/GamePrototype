using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProtectBuff : Buff
{
    public override void OnApply(HealthController healthController, int value, int duration, bool fromRelic)
    {
        color = Color.cyan;
        description = "Protect: Prevent armor from decreasing for {d} turns";
        triggerType = Buff.TriggerType.AtEndOfTurn;
        durationType = Buff.DurationType.Turn;

        base.duration = duration;
        healthController.buffController.AddBuff(this);
        healthController.AddShieldDamageMultiplier(0);
    }

    public override IEnumerator Trigger(HealthController selfHealthController, HealthController attackerHealthController, int value)
    {
        yield return new WaitForSeconds(0);
    }

    public override void Revert(HealthController healthController)
    {
        healthController.RemoveShieldDamageMultiplier(0);
    }

    public override Buff GetCopy()
    {
        Buff output = new ProtectBuff();
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
