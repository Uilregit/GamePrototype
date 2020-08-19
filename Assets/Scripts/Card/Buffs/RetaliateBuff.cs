using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RetaliateBuff : Buff
{
    public override Color GetIconColor()
    {
        return Color.magenta;
    }

    public override string GetDescription()
    {
        return "Retaliate: Attacker takes {v} damage per hit for {d} turns".Replace("{v}", tempValue.ToString("+#;-#;0"));
    }

    public override void OnApply(HealthController healthController, int value, int duration, bool fromRelic)
    {
        healthController.SetRetaliate(value);
        tempValue = value;
        healthController.AddOnDamageBuff(this, duration);
    }

    public override void Trigger(HealthController healthController)
    {

    }

    public override void Revert(HealthController healthController)
    {
        healthController.SetRetaliate(-tempValue);
    }
}
