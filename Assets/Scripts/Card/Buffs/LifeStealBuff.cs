using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class LifeStealBuff : Buff
{
    public override void OnApply(HealthController healthController, int value, int duration, bool fromRelic)
    {
        color = new Color(253, 106, 2);
        description = "LifeSteal: Heal for {v}% of damage dealt for {d} turns";
        triggerType = Buff.TriggerType.OnDamageDealt;
        durationType = Buff.DurationType.Turn;

        healthController.buffController.AddBuff(this);
        tempValue = value;
        base.duration = duration;
    }

    public override IEnumerator Trigger(HealthController selfHealthController, HealthController attackerHealthController, int value)
    {
        //buffTrace.Add(this);
        selfHealthController.TakePiercingDamage(-Mathf.CeilToInt(value * tempValue / 100.0f), selfHealthController);
        yield return new WaitForSeconds(TimeController.time.buffTriggerBufferTime * TimeController.time.timerMultiplier);
    }

    public override void Revert(HealthController healthController)
    {
    }

    public override Buff GetCopy()
    {
        Buff output = new LifeStealBuff();
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
