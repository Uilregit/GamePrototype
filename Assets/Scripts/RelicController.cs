using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RelicController : Observer
{
    public static RelicController relic;

    public RelicLootTable lootTable;
    public List<Relic> relics = new List<Relic>();

    private List<int> validChoices;

    private void Awake()
    {
        if (RelicController.relic == null)
            RelicController.relic = this;
        else
            Destroy(this.gameObject);

        DontDestroyOnLoad(this.gameObject);

        validChoices = new List<int>();
        for (int i = 0; i < lootTable.relics.Count; i++)
            validChoices.Add(i);
    }

    public override void OnNotify(object value, Relic.NotificationType notificationType, List<Relic> traceList)
    {
        for (int i = 0; i < relics.Count; i++)
            if (relics[i].condition == notificationType)
            {
                if (traceList != null && traceList.Contains(relics[i]))
                    continue;
                else if (traceList == null)
                    traceList = new List<Relic>();
                traceList.Add(relics[i]);
                relics[i].Process(value, traceList);
                StartCoroutine(RelicDisplayController.relicDisplay.ShowRelicTrigger(i));
            }
        HandController.handController.ResetCardDisplays();
    }

    public override void OnNotify(int value, StoryRoomSetup.ChallengeType notificationType)
    {
        throw new System.NotImplementedException();
    }

    public Relic GetRandomRelic()
    {
        int index = Random.Range(0, validChoices.Count);

        Relic output = lootTable.relics[validChoices[index]];

        return output;
    }

    public void AddRelic(Relic relic)
    {
        validChoices.Remove(lootTable.relics.IndexOf(relic));
        relics.Add(relic);
        if (RelicDisplayController.relicDisplay != null)                //Check for relicDisplay since it doesn't exist in hte main menu scene
            RelicDisplayController.relicDisplay.RefreshRelicDisplays();
    }

    public int[] GetValidChoices()
    {
        return validChoices.ToArray();
    }

    public void SetValidChoices(int[] value)
    {
        validChoices = value.ToList<int>();
    }

    public void SetRelics(string[] relicNames)
    {
        List<Relic> newRelicList = new List<Relic>();
        foreach (Relic r in lootTable.relics)
            if (relicNames.Contains(r.relicName))
                newRelicList.Add(r);
        relics = newRelicList;

        RelicDisplayController.relicDisplay.RefreshRelicDisplays();
    }
}
