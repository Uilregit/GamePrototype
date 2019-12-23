using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Buff
{
    protected int tempValue = 0;
    public abstract void OnApply(HealthController healthController, int value, int duration);
    public abstract void Trigger(HealthController healthController);
    public abstract void Revert(HealthController healthController);
}
