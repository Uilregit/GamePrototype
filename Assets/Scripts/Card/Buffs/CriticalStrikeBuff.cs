using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CriticalStrikeBuff : Buff
{
    public override void OnApply(HealthController healthController, int value, int duration, bool fromRelic)
    {
        color = Color.red;
        description = "Critical strike: Modify attack by {v} until the next {d} attack(s)";
        triggerType = Buff.TriggerType.OnDamageDealt;
        durationType = Buff.DurationType.Use;

        healthController.buffController.AddBuff(this);
        healthController.SetBonusAttack(value);
        tempValue = value;
        base.duration = duration;
    }

    public override IEnumerator Trigger(HealthController selfHealthController, HealthController attackerHealthController, int value)
    {
        yield return new WaitForSeconds(0);
    }

    public override void Revert(HealthController healthController)
    {
        healthController.SetBonusAttack(-tempValue);
    }

    public override Buff GetCopy()
    {
        Buff output = new CriticalStrikeBuff();
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
