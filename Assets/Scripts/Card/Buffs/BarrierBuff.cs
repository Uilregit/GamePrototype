using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrierBuff : Buff
{
    public override Color GetIconColor()
    {
        return Color.blue;
    }

    public override string GetDescription()
    {
        return "Barrier: Block the next {d} instance of damage dealt to this target completely";
    }

    public override void OnApply(HealthController healthController, int value, int duration, bool fromRelic)
    {
        healthController.AddOnDamageBuff(this, duration);
    }

    public override void Trigger(HealthController healthController)
    {
    }

    public override void Revert(HealthController healthController)
    {
    }
}
