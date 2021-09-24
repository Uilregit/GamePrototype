using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Linq;
using UnityEngine;
using Mirror;

[System.Serializable]
public class BuffInfo
{
    public List<float> colorsR = new List<float>();
    public List<float> colorsG = new List<float>();
    public List<float> colorsB = new List<float>();
    public List<string> descriptions = new List<string>();
    public List<int> duration = new List<int>();
    public List<int> value = new List<int>();
}

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

    //Note: If buffs aren't triggering, it's because startcoroutine is needed
    public IEnumerator TriggerBuff(Buff.TriggerType type, HealthController healthController, int value, List<BuffFactory> traceList = null, float waitTimeMultiplier = 1)
    {
        triggerTickets += 1;
        foreach (BuffFactory buff in buffList)
        {
            if (buff.GetDurationType() == Buff.DurationType.Turn && type == Buff.TriggerType.AtStartOfTurn) //All non start or end of turn, turn buffs (ie lifesteal)
                buff.duration -= 1;

            if (buff.GetTriggerType() == type)
            {
                if (buff.GetDurationType() == Buff.DurationType.Use)                                            //Reduce duration for all use buffs
                    buff.duration -= 1;

                //Checks immunity to the card. If immune, don't trigger
                Effect effect = new EffectFactory().GetEffect(buff.card.cardEffectName[0]);
                if (buff.GetBuff().onTriggerEffects != Buff.BuffEffectType.None)
                    if (effect.CheckImmunity(buff.GetCaster().gameObject, null, new List<GameObject> { healthController.gameObject }, buff.card, 0, waitTimeMultiplier).Count == 0)
                    {
                        if (buff.duration <= 0) //Bonus duration used for when player puts buff on enemy or when enemy puts buff on player, avoids 1 extra turn issue
                            buff.Revert(healthController);
                        continue;
                    }

                if (traceList != null && traceList.Any(x => x.GetTriggerEffectType() == buff.GetTriggerEffectType())) //Prevent infinite loops of buffs triggering itself in chains. (heal on damage, damage on heal, triggering eachother in a loop)
                    continue;

                yield return StartCoroutine(buff.Trigger(selfHealthController, healthController, value, traceList, null));

                if (MultiplayerGameController.gameController != null)   //For multiplayer sync
                {
                    ClientScene.localPlayer.GetComponent<MultiplayerInformationController>().ReportHealthController(selfHealthController.GetComponent<NetworkIdentity>().netId.ToString(), selfHealthController.GetHealthInformation(), ClientScene.localPlayer.GetComponent<MultiplayerInformationController>().GetPlayerNumber());
                    ClientScene.localPlayer.GetComponent<MultiplayerInformationController>().ReportHealthController(healthController.GetComponent<NetworkIdentity>().netId.ToString(), healthController.GetHealthInformation(), ClientScene.localPlayer.GetComponent<MultiplayerInformationController>().GetPlayerNumber());
                }

                if (type != Buff.TriggerType.AtEndOfTurn && type != Buff.TriggerType.AtStartOfTurn)
                    yield return new WaitForSeconds(TimeController.time.buffTriggerBufferTime * TimeController.time.timerMultiplier * waitTimeMultiplier);   //Only triggered buffs causes a pause
                else if (new List<Buff.BuffEffectType>() { Buff.BuffEffectType.ArmorDamage, Buff.BuffEffectType.BonusArmor }.Contains(buff.GetBuff().onApplyEffects) ||
                    new List<Buff.BuffEffectType>() { Buff.BuffEffectType.ArmorDamage, Buff.BuffEffectType.BonusArmor, Buff.BuffEffectType.PiercingDamage, Buff.BuffEffectType.VitDamage }.Contains(buff.GetBuff().onTriggerEffects))
                    yield return new WaitForSeconds(TimeController.time.buffTriggerBufferTime * TimeController.time.timerMultiplier * waitTimeMultiplier);   //Only end of turn buffs that has UI changes causes a pause
            }

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

        if (MultiplayerGameController.gameController != null)
            ClientScene.localPlayer.GetComponent<MultiplayerInformationController>().ReportBuffs(GetComponent<NetworkIdentity>().netId.ToString(), GetBuffInfo(), ClientScene.localPlayer.GetComponent<MultiplayerInformationController>().GetPlayerNumber());

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

    public void Cleanse(HealthController healthController, bool sendInfo = true)
    {
        //If the target is inflicted, they can't be cleansed
        if (healthController.GetInflicted())
            return;

        foreach (BuffFactory buff in buffList)
            buff.Revert(healthController);

        if (MultiplayerGameController.gameController != null && sendInfo)
            ClientScene.localPlayer.GetComponent<MultiplayerInformationController>().ReportBuffs(GetComponent<NetworkIdentity>().netId.ToString(), GetBuffInfo(), ClientScene.localPlayer.GetComponent<MultiplayerInformationController>().GetPlayerNumber());

        buffList = new List<BuffFactory>();
        healthController.ResetBuffIcons(buffList);
    }

    public void AddBuff(BuffFactory buff)
    {
        if (triggerTickets == 0)
        {
            buffList.Add(buff);
            selfHealthController.ResetBuffIcons(buffList);

            if (MultiplayerGameController.gameController != null && !buff.GetIsDummy())
                ClientScene.localPlayer.GetComponent<MultiplayerInformationController>().ReportBuffs(GetComponent<NetworkIdentity>().netId.ToString(), GetBuffInfo(), ClientScene.localPlayer.GetComponent<MultiplayerInformationController>().GetPlayerNumber());
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

    //Used to get simulated damage values for UI
    public void SetBuffs(List<BuffFactory> buffs, bool additive)
    {
        if (!additive)
            buffList = new List<BuffFactory>();
        foreach (BuffFactory b in buffs)
            buffList.Add(b.GetCopy());
    }

    public void SetDummyBuffs(BuffInfo buffs)
    {
        Cleanse(GetComponent<HealthController>(), false);
        for (int i = 0; i < buffs.colorsR.Count; i++)
        {
            BuffFactory buff = new BuffFactory();
            buff.SetBuff(MultiplayerGameController.gameController.dummyBuff);
            Color color = new Color(buffs.colorsR[i], buffs.colorsG[i], buffs.colorsB[i]);
            buff.SetDummyInfo(color, buffs.descriptions[i]);
            buff.OnApply(GetComponent<HealthController>(), GetComponent<HealthController>(), buffs.value[i], buffs.duration[i], new Card(), false, null, null);
        }
    }

    public byte[] GetBuffInfo()
    {
        BuffInfo output = new BuffInfo();

        foreach (BuffFactory buff in buffList)
        {
            output.colorsR.Add(buff.GetBuff().color.r);
            output.colorsG.Add(buff.GetBuff().color.g);
            output.colorsB.Add(buff.GetBuff().color.b);
            output.descriptions.Add(buff.GetDescription());
            output.duration.Add(buff.duration);
            output.value.Add(0);
        }

        MemoryStream stream = new MemoryStream();
        BinaryFormatter formatter = new BinaryFormatter();

        formatter.Serialize(stream, output);

        byte[] final = stream.ToArray();
        return final;
    }
}
