using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffController : MonoBehaviour
{
    private HealthController selfHealthController;

    private List<BuffFactory> buffList;
    private List<BuffFactory> queuedBuffList;

    private int triggerTickets = 0;


    // Start is called before the first frame update
    void Awake()
    {
        selfHealthController = GetComponent<HealthController>();
        buffList = new List<BuffFactory>();
        queuedBuffList = new List<BuffFactory>();
    }

    public IEnumerator TriggerBuff(Buff.TriggerType type, HealthController healthController, int value, List<BuffFactory> traceList = null)
    {
        triggerTickets += 1;
        foreach (BuffFactory buff in buffList)
        {
            if (buff.GetTriggerType() == type)
            {
                if (traceList != null && traceList.Contains(buff)) //Prevent infinite loops of buffs triggering itself in chains. (heal on damage, damage on heal, triggering eachother in a loop)
                    continue;

                yield return StartCoroutine(buff.Trigger(selfHealthController, healthController, value, traceList, null));

                if (buff.GetDurationType() == Buff.DurationType.Use)                                            //Reduce duration for all use buffs
                    buff.duration -= 1;

                if (type != Buff.TriggerType.AtEndOfTurn && type != Buff.TriggerType.AtStartOfTurn)
                    yield return new WaitForSeconds(TimeController.time.buffTriggerBufferTime * TimeController.time.timerMultiplier);   //Only triggered buffs causes a pause
                else if (new List<Buff.BuffEffectType>() { Buff.BuffEffectType.ArmorDamage, Buff.BuffEffectType.BonusArmor }.Contains(buff.GetBuff().onApplyEffects) ||
                    new List<Buff.BuffEffectType>() { Buff.BuffEffectType.ArmorDamage, Buff.BuffEffectType.BonusArmor, Buff.BuffEffectType.PiercingDamage, Buff.BuffEffectType.VitDamage }.Contains(buff.GetBuff().onTriggerEffects))
                    yield return new WaitForSeconds(TimeController.time.buffTriggerBufferTime * TimeController.time.timerMultiplier);   //Only end of turn buffs that has UI changes causes a pause
            }

            if (buff.GetDurationType() == Buff.DurationType.Turn && type == Buff.TriggerType.AtStartOfTurn) //All non start or end of turn, turn buffs (ie lifesteal)
                buff.duration -= 1;

            if (buff.duration <= 0) //Bonus duration used for when player puts buff on enemy or when enemy puts buff on player, avoids 1 extra turn issue
                buff.Revert(healthController);
        }

        triggerTickets -= 1;
        if (triggerTickets == 0)
        {
            RemoveFinishedBuffs();
            buffList.AddRange(queuedBuffList);      //If all buffs are done triggering, add the queued buffs if there are any
            queuedBuffList = new List<BuffFactory>();
        }

        if (buffList != null)
            healthController.ResetBuffIcons(buffList);

        HandController.handController.ResetCardDisplays();
    }

    private void RemoveFinishedBuffs()
    {
        List<BuffFactory> finalList = new List<BuffFactory>();
        foreach (BuffFactory buff in buffList)
            if (buff.duration > 0)
                finalList.Add(buff);
        ;
        buffList = finalList;
    }

    public void Cleanse(HealthController healthController)
    {
        foreach (BuffFactory buff in buffList)
            buff.Revert(healthController);

        buffList = new List<BuffFactory>();
        healthController.ResetBuffIcons(buffList);
    }

    public void AddBuff(BuffFactory buff)
    {
        if (triggerTickets == 0)
        {
            buffList.Add(buff);
            selfHealthController.ResetBuffIcons(buffList);
        }
        else
            queuedBuffList.Add(buff);   //If buffs are still triggering, add to queue so buffs are added AFTER all buffs are done triggering
    }

    public void AddBuff(Buff dummy) //Exists only for testing, used in trap controllers delete later
    {

    }

    public List<BuffFactory> GetBuffs()
    {
        return buffList;
    }
}
