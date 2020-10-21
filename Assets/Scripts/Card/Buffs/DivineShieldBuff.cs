using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DivineShieldBuff : Buff
{
    public override void OnApply(HealthController healthController, int value, int duration, bool fromRelic)
    {
        color = Color.cyan;
        description = "Divine Shield: Completely prevent the next {d} instances of damage";
        triggerType = Buff.TriggerType.OnDamageRecieved;
        durationType = Buff.DurationType.Use;

        base.duration = duration;
        healthController.buffController.AddBuff(this);
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
        Buff output = new DivineShieldBuff();
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
