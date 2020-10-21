using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StunDebuff : Buff
{
    public override void OnApply(HealthController healthController, int value, int duration, bool fromRelic)
    {
        color = new Color(136, 0, 21);
        description = "Stun: Can't act for {d} turn(s)";
        triggerType = Buff.TriggerType.AtStartOfTurn;
        durationType = Buff.DurationType.Turn;

        base.duration = duration;
        healthController.buffController.AddBuff(this);
        healthController.SetStunned(true);
    }

    public override IEnumerator Trigger(HealthController selfHealthController, HealthController attackerHealthController, int value)
    {
        yield return new WaitForSeconds(0);
    }

    public override void Revert(HealthController healthController)
    {
        Debug.Log(healthController);
        healthController.SetStunned(false);
    }

    public override Buff GetCopy()
    {
        Buff output = new StunDebuff();
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
