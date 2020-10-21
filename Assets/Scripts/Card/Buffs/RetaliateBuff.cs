using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RetaliateBuff : Buff
{
    public override void OnApply(HealthController healthController, int value, int duration, bool fromRelic)
    {
        color = Color.magenta;
        description = "Retaliate: Attacker takes {v} damage per hit for {d} turns";
        triggerType = Buff.TriggerType.OnDamageRecieved;
        durationType = Buff.DurationType.Turn;

        base.duration = duration;
        healthController.buffController.AddBuff(this);
        //healthController.SetRetaliate(value);
        tempValue = value;
    }

    public override IEnumerator Trigger(HealthController selfHealthController, HealthController attackerHealthController, int value)
    {
        Debug.Log("triggered retaliate");
        attackerHealthController.TakeVitDamage(tempValue, null);
        yield return new WaitForSeconds(TimeController.time.buffTriggerBufferTime * TimeController.time.timerMultiplier);
    }

    public override void Revert(HealthController healthController)
    {
        Debug.Log("reverted retaliate");
        //healthController.SetRetaliate(-tempValue);
    }

    public override Buff GetCopy()
    {
        Buff output = new RetaliateBuff();
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
