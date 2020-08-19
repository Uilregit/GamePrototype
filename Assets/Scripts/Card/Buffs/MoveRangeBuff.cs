using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveRangeBuff : Buff
{
    public override Color GetIconColor()
    {
        return Color.yellow;
    }

    public override string GetDescription()
    {
        return "Haste: Modify move range by {v} for {d} turns".Replace("{v}", tempValue.ToString("+#;-#;0"));
    }

    public override void OnApply(HealthController healthController, int value, int duration, bool fromRelic)
    {
        healthController.SetBonusMoveRange(value);
        tempValue = value;
        healthController.AddEndOfTurnBuff(this, duration);
    }

    public override void Trigger(HealthController healthController)
    {

    }

    public override void Revert(HealthController healthController)
    {
        healthController.SetBonusMoveRange(-tempValue);
    }
}