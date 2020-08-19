using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoubleDamageDebuff : Buff
{
    public override Color GetIconColor()
    {
        return Color.blue;
    }

    public override string GetDescription()
    {
        return "Shattered: Next {d} instance of damage dealt to this target is doubled";
    }

    public override void OnApply(HealthController healthController, int value, int duration, bool fromRelic)
    {
        healthController.AddOnDamangeDebuff(this, duration);
    }

    public override void Trigger(HealthController healthController)
    {
    }

    public override void Revert(HealthController healthController)
    {
    }
}
