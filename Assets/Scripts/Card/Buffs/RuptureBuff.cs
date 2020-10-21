using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuptureBuff : Buff
{
    public override void OnApply(HealthController healthController, int value, int duration, bool fromRelic)
    {
        color = Color.green;
        description = "Rupture: Take {v} piercing damage on every move for {d} turns";
        triggerType = Buff.TriggerType.OnMove;
        durationType = Buff.DurationType.Turn;

        base.duration = duration;
        healthController.buffController.AddBuff(this);
        tempValue = value;
    }

    public override IEnumerator Trigger(HealthController selfHealthController, HealthController attackerHealthController, int value)
    {
        selfHealthController.TakePiercingDamage(tempValue, selfHealthController);
        yield return new WaitForSeconds(0);
    }

    public override void Revert(HealthController healthController)
    {
    }

    public override Buff GetCopy()
    {
        Buff output = new RuptureBuff();
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
