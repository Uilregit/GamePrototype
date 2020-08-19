using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProtectBuff : Buff
{
    public override Color GetIconColor()
    {
        return Color.white;
    }

    public override string GetDescription()
    {
        return "Protect: Prevent armor from decreasing for {d} turns";
    }

    public override void OnApply(HealthController healthController, int value, int duration, bool fromRelic)
    {
        healthController.AddStartOfTurnBuff(this, duration);
    }

    public override void Trigger(HealthController healthController)
    {
    }

    public override void Revert(HealthController healthController)
    {
    }
}
