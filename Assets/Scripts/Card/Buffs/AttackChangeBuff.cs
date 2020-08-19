using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackChangeBuff : Buff
{
    public override Color GetIconColor()
    {
        return Color.red;
    }

    public override string GetDescription()
    {
        return "Attack: Modify attack by {v} for {d} turns".Replace("{v}", tempValue.ToString("+#;-#;0"));
    }

    public override void OnApply(HealthController healthController,int value, int duration, bool fromRelic)
    {
        healthController.SetBonusAttack(value);
        tempValue = value;
        healthController.AddEndOfTurnBuff(this, duration);
    }

    public override void Trigger(HealthController healthController)
    {

    }

    public override void Revert(HealthController healthController)
    {
        healthController.SetBonusAttack(-tempValue);
    }
}
