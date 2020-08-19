﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManaCostReductionBuff : Buff
{
    public override Color GetIconColor()
    {
        return Color.blue;
    }

    public override string GetDescription()
    {
        if (tempValue > 0)
            return "Mana Reduction: All mana cards cost {v} less for {d} turns".Replace("{v}", tempValue.ToString());
        else
            return "Mana Reduction: All mana cards cost {v} more for {d} turns".Replace("{v}", (-tempValue).ToString());
    }

    public override void OnApply(HealthController healthController, int value, int duration, bool fromRelic)
    {
        TurnController.turnController.SetManaReduction(value);
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
        TurnController.turnController.RemoveManaReduction(tempValue);
    }
}