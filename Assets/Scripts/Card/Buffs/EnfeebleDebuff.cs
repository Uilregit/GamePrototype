using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnfeebleDebuff : Buff
{
    public override Color GetIconColor()
    {
        return Color.cyan;
    }

    public override string GetDescription()
    {
        return "Enfeeble: Take {v} more damage per hit for {d} turns".Replace("{v}", tempValue.ToString("+#;-#;0"));
    }

    public override void OnApply(HealthController healthController, int value, int duration, bool fromRelic)
    {
        healthController.SetEnfeeble(value);
        tempValue = value;
        healthController.AddEndOfTurnDebuff(this, duration);
    }

    public override void Trigger(HealthController healthController)
    {

    }

    public override void Revert(HealthController healthController)
    {
        healthController.SetEnfeeble(-tempValue);
    }
}