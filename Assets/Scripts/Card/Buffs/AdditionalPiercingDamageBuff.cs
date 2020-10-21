using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdditionalPiercingDamageBuff : Buff
{
    public override void OnApply(HealthController healthController, int value, int duration, bool fromRelic)
    {
        color = Color.red;
        description = "Additional Damage: Take {v} additional piercing damage after recieving damage for {d} turns";
        triggerType = Buff.TriggerType.OnDamageRecieved;
        durationType = Buff.DurationType.Turn;

        base.duration = duration;
        healthController.buffController.AddBuff(this);
        tempValue = value;
    }

    public override IEnumerator Trigger(HealthController selfHealthController, HealthController attackerHealthController, int value)
    {
        selfHealthController.TakePiercingDamage(tempValue, attackerHealthController);
        yield return new WaitForSeconds(0);
    }

    public override void Revert(HealthController healthController)
    {
    }

    public override Buff GetCopy()
    {
        Buff output = new AdditionalPiercingDamageBuff();
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
