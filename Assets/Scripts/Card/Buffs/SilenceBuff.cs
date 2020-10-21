using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SilenceBuff : Buff
{
    public override void OnApply(HealthController healthController, int value, int duration, bool fromRelic)
    {
        color = Color.white;
        description = "Silence: Can't play mana cards for {d} turns";
        triggerType = Buff.TriggerType.AtEndOfTurn;
        durationType = Buff.DurationType.Turn;

        base.duration = duration;
        healthController.buffController.AddBuff(this);
        healthController.SetSilenced(true);
        HandController.handController.ResetCardPlayability(TurnController.turnController.GetCurrentEnergy(), TurnController.turnController.GetCurrentMana());
    }

    public override IEnumerator Trigger(HealthController selfHealthController, HealthController attackerHealthController, int value)
    {
        yield return new WaitForSeconds(0);
    }

    public override void Revert(HealthController healthController)
    {
        healthController.SetSilenced(false);
    }

    public override Buff GetCopy()
    {
        Buff output = new SilenceBuff();
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
