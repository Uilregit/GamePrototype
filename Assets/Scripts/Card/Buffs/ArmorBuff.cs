using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmorBuff : Buff
{
    public override void OnApply(HealthController healthController, int value, int duration)
    {
        healthController.SetBonusShield(value);
        tempValue = value;
        healthController.AddStartOfTurnBuff(this, duration);
    }

    public override void Trigger(HealthController healthController)
    {

    }

    public override void Revert(HealthController healthController)
    {
        healthController.SetBonusShield(-tempValue);
    }
}
