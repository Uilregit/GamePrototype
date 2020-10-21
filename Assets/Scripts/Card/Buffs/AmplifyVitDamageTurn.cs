using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmplifyVitDamageTurn : Buff
{
    public override void OnApply(HealthController healthController, int value, int duration, bool fromRelic)
    {
        color = Color.cyan;
        description = "Amplify Damage: Take {v} times more damage for {d} turns";
        triggerType = Buff.TriggerType.AtEndOfTurn;
        durationType = Buff.DurationType.Turn;

        base.duration = duration;
        healthController.buffController.AddBuff(this);
        healthController.AddVitDamageMultiplier(value);
        tempValue = value;
    }

    public override IEnumerator Trigger(HealthController selfHealthController, HealthController attackerHealthController, int value)
    {
        yield return new WaitForSeconds(0);
    }

    public override void Revert(HealthController healthController)
    {
        healthController.RemoveVitDamageMultiplier(-tempValue);
    }

    public override Buff GetCopy()
    {
        Buff output = new AmplifyVitDamageTurn();
        output.tempValue = tempValue;
        output.duration = duration;
        output.value = value;
        output.color = color;
        output.description = description;
        output.triggerType = triggerType;
        output.durationType = durationType;

        return output;
    }
}