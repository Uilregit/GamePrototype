using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RetaliateBuff : Buff
{
    public override void OnApply(HealthController healthController, int value, int duration)
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
