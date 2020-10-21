using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VitDamageOverTime : Buff
{
    public override void OnApply(HealthController healthController, int value, int duration, bool fromRelic)
    {
        color = new Color(128, 0, 128);
        description = "Ignite: Take {v} damage at the end of the next {d} turn(s)";
        triggerType = Buff.TriggerType.AtEndOfTurn;
        durationType = Buff.DurationType.Turn;

        healthController.buffController.AddBuff(this);
        tempValue = value;
        base.duration = duration;
    }

    public override IEnumerator Trigger(HealthController selfHealthController, HealthController attackerHealthController, int value)
    {
        selfHealthController.TakeVitDamage(tempValue, selfHealthController);
        yield return new WaitForSeconds(0);
    }

    public override void Revert(HealthController healthController)
    {
    }

    public override Buff GetCopy()
    {
        Buff output = new VitDamageOverTime();
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
