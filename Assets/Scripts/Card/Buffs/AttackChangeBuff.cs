using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackChangeBuff : Buff
{
    public override void OnApply(HealthController healthController,int value, int duration)
    {
        healthController.SetBonusAttack(value);
        tempValue = value;
        healthController.AddEndOfTurnBuff(this, duration);
    }

    public override void Trigger(HealthController healthController)
    {

    }

    public override void Revert(HealthController healthController)
    {
        healthController.SetBonusAttack(-tempValue);
    }
}
