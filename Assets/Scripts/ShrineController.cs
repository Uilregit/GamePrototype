using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShrineController : MonoBehaviour
{
    public enum StatsBoostType { AttackBoost, ArmorBoost, HealthBoost }
    public Sprite attackImage;
    public Color attackColor;
    public Sprite armorImage;
    public Color armorColor;
    public Sprite healthImage;
    public Color healthColor;

    public GameObject item1;
    public GameObject item2;

    private StatsBoostType statsBoost;
    private Relic relic;

    // Start is called before the first frame update
    void Start()
    {
        int index = Random.Range(0, 3);
        if (index == 0)
            statsBoost = StatsBoostType.AttackBoost;
        else if (index == 1)
            statsBoost = StatsBoostType.ArmorBoost;
        else if (index == 2)
            statsBoost = StatsBoostType.HealthBoost;

        relic = RelicController.relic.GetRandomRelic();

        RefreshDisplays();
    }

    private void RefreshDisplays()
    {
        if (statsBoost == StatsBoostType.AttackBoost)
        {
            item1.transform.GetChild(1).GetComponent<Image>().sprite = attackImage;
            item1.transform.GetChild(1).GetComponent<Image>().color = attackColor;
            item1.transform.GetChild(2).GetComponent<Text>().text = "Attack Boost";
            item1.transform.GetChild(3).GetComponent<Text>().text = "Give all allies +1 ATK for the rest of the run";
        }
        else if (statsBoost == StatsBoostType.ArmorBoost)
        {
            item1.transform.GetChild(1).GetComponent<Image>().sprite = armorImage;
            item1.transform.GetChild(1).GetComponent<Image>().color = armorColor;
            item1.transform.GetChild(2).GetComponent<Text>().text = "Armor Boost";
            item1.transform.GetChild(3).GetComponent<Text>().text = "Give all allies +2 Armor for the rest of the run";
        }
        else if (statsBoost == StatsBoostType.HealthBoost)
        {
            item1.transform.GetChild(1).GetComponent<Image>().sprite = healthImage;
            item1.transform.GetChild(1).GetComponent<Image>().color = healthColor;
            item1.transform.GetChild(2).GetComponent<Text>().text = "Health Boost";
            item1.transform.GetChild(3).GetComponent<Text>().text = "Give all allies +5 Health for the rest of the run";
        }

        item2.transform.GetChild(1).GetComponent<Image>().sprite = relic.art;
        item2.transform.GetChild(1).GetComponent<Image>().color = relic.color;
        item2.transform.GetChild(2).GetComponent<Text>().text = relic.relicName;
        item2.transform.GetChild(3).GetComponent<Text>().text = relic.description;
    }

    public void ChoseOption1()
    {
        if (statsBoost == StatsBoostType.AttackBoost)
        {
            InformationController.infoController.ChangeCombatInfo(1, 0, 0);
        }
        else if (statsBoost == StatsBoostType.ArmorBoost)
        {
            InformationController.infoController.ChangeCombatInfo(0, 2, 0);
        }
        else if (statsBoost == StatsBoostType.HealthBoost)
        {
            InformationController.infoController.ChangeCombatInfo(0, 0, 5);
        }

        GameController.gameController.LoadScene("OverworldScene", false, 0);
    }

    public void ChoseOption2()
    {
        RelicController.relic.AddRelic(relic);
        GameController.gameController.LoadScene("OverworldScene", false, 0);
    }
}
