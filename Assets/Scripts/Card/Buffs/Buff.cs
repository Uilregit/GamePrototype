using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Buff
{
    protected int tempValue = 0;
    public virtual Color GetIconColor()
    {
        return Color.white;
    }

    public abstract string GetDescription();
    public abstract void OnApply(HealthController healthController, int value, int duration, bool fromRelic);
    public abstract void Trigger(HealthController healthController);
    public abstract void Revert(HealthController healthController);
}
