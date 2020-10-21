using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrierBuff : Buff
{
    public override void OnApply(HealthController healthController, int value, int duration, bool fromRelic)
    {
        color = Color.blue;
        description = "Barrier: Block the next {d} instance of damage dealt to this target completely";
        triggerType = Buff.TriggerType.AtEndOfTurn;
        durationType = Buff.DurationType.Turn;

        healthController.buffController.AddBuff(this);
        base.duration = duration;
        healthController.AddVitDamageMultiplier(0);
        healthController.AddShieldDamageMultiplier(0);
    }

    public override IEnumerator Trigger(HealthController selfHealthController, HealthController attackerHealthController, int value)
    {
        yield return new WaitForSeconds(0);
    }

    public override void Revert(HealthController healthController)
    {
        healthController.RemoveVitDamageMultiplier(0);
        healthController.RemoveShieldDamageMultiplier(0);
    }

    public override Buff GetCopy()
    {
        Buff output = new BarrierBuff();
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
