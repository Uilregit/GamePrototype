using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackChangeOnHealBuff : Buff
{
    public override void OnApply(HealthController healthController, int value, int duration, bool fromRelic)
    {
        color = Color.yellow;
        description = "Life force: Modify ATK by {v} everytime you are healed for {d} turns";
        triggerType = Buff.TriggerType.OnHealingRecieved;
        durationType = Buff.DurationType.Turn;

        base.duration = duration;
        healthController.buffController.AddBuff(this);
        tempValue = value;
    }

    public override IEnumerator Trigger(HealthController selfHealthController, HealthController attackerHealthController, int value)
    {
        new AttackChangeBuff().OnApply(selfHealthController, tempValue, 1, false);
        yield return new WaitForSeconds(0);
    }

    public override void Revert(HealthController healthController)
    {
    }

    public override Buff GetCopy()
    {
        Buff output = new AttackChangeOnHealBuff();
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
