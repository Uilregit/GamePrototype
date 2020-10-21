using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreserveBuff : Buff
{
    public override void OnApply(HealthController healthController, int value, int duration, bool fromRelic)
    {
        color = Color.green;
        description = "Preserve: Does not reset bonus health at the beginning of turns for {d} more turns";
        triggerType = Buff.TriggerType.AtStartOfTurn;
        durationType = Buff.DurationType.Turn;

        base.duration = duration;
        healthController.buffController.AddBuff(this);
        healthController.SetPreserveBonusVit(true);
    }

    public override IEnumerator Trigger(HealthController selfHealthController, HealthController attackerHealthController, int value)
    {
        yield return new WaitForSeconds(0);
    }

    public override void Revert(HealthController healthController)
    {
        healthController.SetPreserveBonusVit(false);
    }

    public override Buff GetCopy()
    {
        Buff output = new PreserveBuff();
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
