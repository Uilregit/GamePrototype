using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffController : MonoBehaviour
{
    private HealthController selfHealthController;

    private List<Buff> buffList;

    private int triggerTickets = 0;

    // Start is called before the first frame update
    void Awake()
    {
        selfHealthController = GetComponent<HealthController>();
        buffList = new List<Buff>();
    }

    public IEnumerator TriggerBuff(Buff.TriggerType type, HealthController healthController, int value)
    {
        triggerTickets += 1;
        foreach (Buff buff in buffList)
        {
            if (buff.triggerType == type)
            {
                yield return StartCoroutine(buff.Trigger(selfHealthController, healthController, value));

                if (buff.durationType == Buff.DurationType.Use)
                    buff.duration -= 1;
                else if (buff.durationType == Buff.DurationType.Turn && (type == Buff.TriggerType.AtEndOfTurn || type == Buff.TriggerType.AtStartOfTurn))
                    buff.duration -= 1;
                else if (buff.durationType == Buff.DurationType.Turn && type == Buff.TriggerType.AtStartOfTurn)     //Reduce duration for all turn buffs that arent triggered at turn start/end
                    buff.duration -= 1;

                yield return new WaitForSeconds(TimeController.time.buffTriggerBufferTime * TimeController.time.timerMultiplier);   //Only triggered buffs causes a pause
            }

            if (buff.duration <= 0)
                buff.Revert(healthController);
        }

        triggerTickets -= 1;
        if (triggerTickets == 0)
            RemoveFinishedBuffs();

        healthController.ResetBuffIcons(buffList);
        HandController.handController.ResetCardDisplays();
    }

    private void RemoveFinishedBuffs()
    {
        List<Buff> removeList = new List<Buff>();
        foreach (Buff buff in buffList)
            if (buff.duration <= 0)
                removeList.Add(buff);

        foreach (Buff buff in removeList)
        {
            buffList.Remove(buff);
            Destroy(buff);
        }
    }

    public void Cleanse(HealthController healthController)
    {
        foreach (Buff buff in buffList)
            buff.Revert(healthController);

        buffList = new List<Buff>();
        healthController.ResetBuffIcons(buffList);
    }

    public void AddBuff(Buff buff)
    {
        buffList.Add(buff);
        selfHealthController.ResetBuffIcons(buffList);
    }

    public List<Buff> GetBuffs()
    {
        Debug.Log(buffList.Count);
        return buffList;
    }
}
