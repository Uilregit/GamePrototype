﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

    public CardDisplay card;

    private StatsBoostType statsBoost;
    private Relic relic;
    private Equipment equipment;

    private IEnumerator cardEnlargeCoroutine;
    private float cardEnlargeStartTime;

    // Start is called before the first frame update
    void Start()
    {
        /*
        int index = Random.Range(0, 3);
        if (index == 0)
            statsBoost = StatsBoostType.AttackBoost;
        else if (index == 1)
            statsBoost = StatsBoostType.ArmorBoost;
        else if (index == 2)
            statsBoost = StatsBoostType.HealthBoost;
        */
        equipment = LootController.loot.GetRandomEquipment();
        for (int i = 0; i < 100; i++)
            if (CollectionController.collectionController.GetCountOfEquipmentInCollection(equipment) != 0)
                equipment = LootController.loot.GetRandomEquipment();
            else
                break;

        relic = RelicController.relic.GetRandomRelic();
        card.SetEquipment(equipment, Card.CasterColor.Enemy);

        RefreshDisplays();
    }

    private void RefreshDisplays()
    {
        /*
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
        */

        item1.transform.GetChild(1).GetComponent<Image>().sprite = equipment.art;
        item1.transform.GetChild(1).GetComponent<Image>().color = Color.white;
        item1.transform.GetChild(2).GetComponent<Text>().text = equipment.equipmentName;
        string equipText = "Weapon\n";
        if (!equipment.isWeapon)
            equipText = "Accessory\n";
        equipText += equipment.numOfCardSlots + " card slots\n";
        if (equipment.equipmentDescription != "")
            equipText += equipment.equipmentDescription.Replace("|", "").Replace("<s>", "").Replace("</s>", "") + "\n";
        if (equipment.atkChange != 0)
            equipText += " Atk: " + equipment.atkChange.ToString("+0; -#");
        if (equipment.armorChange != 0)
            equipText += " Armor: " + equipment.armorChange.ToString("+0; -#");
        if (equipment.healthChange != 0)
            equipText += " Health: " + equipment.healthChange.ToString("+0; -#");
        item1.transform.GetChild(3).GetComponent<Text>().text = equipText;

        item2.transform.GetChild(1).GetComponent<Image>().sprite = relic.art;
        item2.transform.GetChild(1).GetComponent<Image>().color = relic.color;
        item2.transform.GetChild(2).GetComponent<Text>().text = relic.relicName;
        item2.transform.GetChild(3).GetComponent<Text>().text = relic.description;
    }

    public void ChoseOption1()
    {
        /*
        if (statsBoost == StatsBoostType.AttackBoost)
        {
            InformationController.infoController.ChangeCombatInfo(0, 1, 0, 0);
        }
        else if (statsBoost == StatsBoostType.ArmorBoost)
        {
            InformationController.infoController.ChangeCombatInfo(0, 0, 2, 0);
        }
        else if (statsBoost == StatsBoostType.HealthBoost)
        {
            InformationController.infoController.ChangeCombatInfo(0, 0, 0, 5);
        }
        */
        MusicController.music.SetHighPassFilter(false);

        CollectionController.collectionController.AddRewardsEquipment(equipment, false);

        Camera.main.transform.position = new Vector3(0, 0, -10);
        RoomController.roomController.SetViableRoom(new Vector2(-999, -999));
        InformationLogger.infoLogger.SaveGame(false);
        RoomController.roomController.Refresh();
        RoomController.roomController.Show();
        SceneManager.LoadScene("OverworldScene", LoadSceneMode.Single);
    }

    public void ChoseOption2()
    {
        MusicController.music.SetHighPassFilter(false);

        RelicController.relic.AddRelic(relic);
        Camera.main.transform.position = new Vector3(0, 0, -10);
        RoomController.roomController.SetViableRoom(new Vector2(-999, -999));
        InformationLogger.infoLogger.SaveGame(false);
        RoomController.roomController.Refresh();
        RoomController.roomController.Show();
        SceneManager.LoadScene("OverworldScene", LoadSceneMode.Single);
    }

    public void ShowSelectedCard()
    {
        cardEnlargeCoroutine = SelectedCard();
        cardEnlargeStartTime = Time.time;
        StartCoroutine(cardEnlargeCoroutine);
    }

    private IEnumerator SelectedCard()
    {
        yield return new WaitForSeconds(0.3f);
        card.gameObject.SetActive(true);
    }

    public void HideSelectedCard()
    {
        if (Time.time - cardEnlargeStartTime < 0.3f)
            ChoseOption1();
        card.gameObject.SetActive(false);
    }
}
