using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnfeebleDebuff : Buff
{
    public override void OnApply(HealthController healthController, int value, int duration)
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