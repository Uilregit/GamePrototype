using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmorBuff : Buff
{
    public override Color GetIconColor()
    {
        return Color.blue;
    }

    public override string GetDescription()
    {
        return "Armor: Modify armor by {v} for {d} turns".Replace("{v}", tempValue.ToString("+#;-#;0"));
    }

    public override void OnApply(HealthController healthController, int value, int duration, bool fromRelic)
    {
        healthController.SetBonusShield(value, fromRelic);
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
