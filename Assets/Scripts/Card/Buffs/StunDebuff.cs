using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StunDebuff : Buff
{
    public override void OnApply(HealthController healthController, int value, int duration)
    {
        healthController.SetStunned(true);
        healthController.AddEndOfTurnBuff(this, duration);
    }

    public override void Trigger(HealthController healthController)
    {
        
    }

    public override void Revert(HealthController healthController)
    {
        healthController.SetStunned(false);
    }
}
