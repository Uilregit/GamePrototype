using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyCostCapTurnBuff : Buff
{
    public override Color GetIconColor()
    {
        return Color.blue;
    }

    public override string GetDescription()
    {
        return "Energy Cap: All energy cards that cost more than {v} cost {v} for {d} turns".Replace("{v}", tempValue.ToString());
    }

    public override void OnApply(HealthController healthController, int value, int duration, bool fromRelic)
    {
        TurnController.turnController.SetEnergyCostCap(value);
        tempValue = value;
        if (value > 0)
            healthController.AddStartOfTurnBuff(this, duration);
        else
            healthController.AddStartOfTurnDebuff(this, duration);
    }

    public override void Trigger(HealthController healthController)
    {

    }

    public override void Revert(HealthController healthController)
    {
        TurnController.turnController.RemoveEnergyCostCap(tempValue);
    }
}
