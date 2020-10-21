using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisarmBuff : Buff
{
    public override void OnApply(HealthController healthController, int value, int duration, bool fromRelic)
    {
        color = Color.black;
        description = "Disarm: Can't play energy cards for {d} turns";
        triggerType = Buff.TriggerType.AtEndOfTurn;
        durationType = Buff.DurationType.Turn;

        base.duration = duration;
        healthController.buffController.AddBuff(this);
        healthController.SetDisarmed(true);
        HandController.handController.ResetCardPlayability(TurnController.turnController.GetCurrentEnergy(), TurnController.turnController.GetCurrentMana());
    }

    public override IEnumerator Trigger(HealthController selfHealthController, HealthController attackerHealthController, int value)
    {
        yield return new WaitForSeconds(0);
    }

    public override void Revert(HealthController healthController)
    {
        healthController.SetDisarmed(false);
    }

    public override Buff GetCopy()
    {
        Buff output = new DisarmBuff();
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
