using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealAttacker : Buff
{
    public override void OnApply(HealthController healthController, int value, int duration, bool fromRelic)
    {
        color = new Color(0, 200, 100);
        description = "Heal Attacker: Attacker heals {v}% of their damage done for {d} turns";
        triggerType = Buff.TriggerType.OnDamageRecieved;
        durationType = Buff.DurationType.Turn;

        base.duration = duration;
        healthController.buffController.AddBuff(this);
        tempValue = value;
    }

    public override IEnumerator Trigger(HealthController selfHealthController, HealthController attackerHealthController, int value)
    {
        attackerHealthController.TakePiercingDamage(-Mathf.CeilToInt(value * tempValue / 100.0f), null);
        yield return new WaitForSeconds(TimeController.time.buffTriggerBufferTime * TimeController.time.timerMultiplier);
    }

    public override void Revert(HealthController healthController)
    {
    }

    public override Buff GetCopy()
    {
        Buff output = new HealAttacker();
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
