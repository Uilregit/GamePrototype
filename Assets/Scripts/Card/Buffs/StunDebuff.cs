using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StunDebuff : Buff
{
    public override Color GetIconColor()
    {
        return new Color(136,0,21);
    }

    public override string GetDescription()
    {
        return "Stun: Can't act for {d} turns";
    }

    public override void OnApply(HealthController healthController, int value, int duration, bool fromRelic)
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
