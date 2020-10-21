using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BonusHealingBuff : Buff
{
    public override void OnApply(HealthController healthController, int value, int duration, bool fromRelic)
    {
        color = Color.green;
        description = "Bonus Healing: Heal {v} more whenever you are healed for {d} turns";
        triggerType = Buff.TriggerType.OnHealingRecieved;
        durationType = Buff.DurationType.Turn;

        healthController.buffController.AddBuff(this);
        tempValue = value;
        base.duration = duration;
    }

    public override IEnumerator Trigger(HealthController selfHealthController, HealthController attackerHealthController, int value)
    {
        selfHealthController.TakePiercingDamage(-value, selfHealthController);
        yield return new WaitForSeconds(TimeController.time.buffTriggerBufferTime * TimeController.time.timerMultiplier);
    }

    public override void Revert(HealthController healthController)
    {
    }

    public override Buff GetCopy()
    {
        Buff output = new BonusHealingBuff();
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
