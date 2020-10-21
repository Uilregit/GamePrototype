using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public abstract class Buff : MonoBehaviour
{
    public int tempValue = 0;

    public int duration;
    public int value;
    public Color color;
    public string description = "";

    public enum TriggerType
    {
        AtEndOfTurn = 0,
        AtStartOfTurn = 1,

        OnDamageRecieved = 100,
        OnHealingRecieved = 101,
        OnDamageDealt = 110,

        OnMove = 200,

        OnCardPlayed = 300
    }
    public TriggerType triggerType;

    public enum DurationType
    {
        Turn = 0,
        Use = 1
    }
    public DurationType durationType;

    public virtual Color GetIconColor()
    {
        return color;
    }

    public virtual string GetDescription()
    {
        return description.Replace("{+-v}", tempValue.ToString("+#;-#;0")).Replace("{|v|}", Mathf.Abs(tempValue).ToString()).Replace("{v}", tempValue.ToString()); 
    }
    public abstract void OnApply(HealthController healthController, int value, int duration, bool fromRelic);
    public abstract IEnumerator Trigger(HealthController selfHealthController, HealthController attackerHealthController, int value);
    public abstract void Revert(HealthController healthController);
    public abstract Buff GetCopy();

    public virtual void MultiplyValue(HealthController health, int multiplier)
    {
        tempValue = Mathf.CeilToInt( tempValue* multiplier/100.0f);
    }

    public virtual void AddToValue(HealthController health, int addition)
    {
        tempValue += addition;
    }
}
