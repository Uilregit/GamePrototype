using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldDamageOverTime : Buff
{
    public override void OnApply(HealthController healthController, int value, int duration, bool fromRelic)
    {
        color = Color.blue;
        description = "Acid: Lose {v} armor at the end of the turn for {d} turns";
        triggerType = Buff.TriggerType.AtEndOfTurn;
        durationType = Buff.DurationType.Turn;

        healthController.buffController.AddBuff(this);
        tempValue = value;
        base.duration = duration;
    }

    public override IEnumerator Trigger(HealthController selfHealthController, HealthController attackerHealthController, int value)
    {
        selfHealthController.TakeShieldDamage(Mathf.CeilToInt(-value * tempValue / 100.0f), null);
        yield return new WaitForSeconds(0);
    }

    public override void Revert(HealthController healthController)
    {
    }

    public override Buff GetCopy()
    {
        Buff output = new ShieldDamageOverTime();
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
