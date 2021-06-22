﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class CharacterInformationController : MonoBehaviour
{
    public static CharacterInformationController charInfoController;

    public Image characterImage;
    public Text healthText;
    public Text armorText;
    public Text attackText;
    public Text attackQueueText;
    public List<CardDisplay> passiveCards;
    public List<Text> abilityTexts;
    public List<CardDisplay> attackCards;
    public List<BuffDescriptionController> buffDescriptions;
    public Collider2D returnButton;
    private int numOfCards = 0;

    private void Awake()
    {
        if (CharacterInformationController.charInfoController == null)
            CharacterInformationController.charInfoController = this;
        else
            Destroy(this.gameObject);

        Hide();
    }

    public void SetDescription(Sprite character, HealthController healthController, List<CardController> cards, List<BuffFactory> buffList, List<Equipment> equipments, AbilitiesController abilitiesController)
    {
        int currentHealth = healthController.GetVit();
        int maxHealth = healthController.GetMaxVit();
        int attack = healthController.GetAttack();
        int armor = healthController.GetArmor();
        numOfCards = cards.Count;

        //Stats section
        characterImage.sprite = character;
        healthText.text = "Health: {c}/{m}".Replace("{c}", currentHealth.ToString()).Replace("{m}", maxHealth.ToString());
        attackText.text = "Attack: {a}".Replace("{a}", attack.ToString());
        armorText.text = "Armor: {s}".Replace("{s}", armor.ToString());

        if (cards.Count == 0)
            attackQueueText.enabled = false;
        else
            attackQueueText.enabled = true;

        //Attack cards section
        for (int i = 0; i < attackCards.Count; i++)
        {
            if (i < cards.Count)
            {
                attackCards[i].transform.parent.GetComponent<CardController>().SetCaster(healthController.gameObject);
                attackCards[i].SetCard(cards[i]);
                attackCards[i].Show();
                attackCards[i].GetComponent<LineRenderer>().enabled = false;
            }
            else
                attackCards[i].Hide();
        }

        //Buff section
        for (int i = 0; i < buffDescriptions.Count; i++)
        {
            if (i < buffList.Count)
            {
                buffDescriptions[i].SetBuff(buffList[i], buffList[i].duration);
                buffDescriptions[i].Show();
            }
            else
                buffDescriptions[i].Hide();
        }

        //Abilities section
        List<Card> abilityCards = abilitiesController.GetAbilityCards();
        int count = abilityCards.Count;
        if (equipments != null)
            count = equipments.Count;
        for (int i = 0; i < passiveCards.Count; i++)
        {
            if (i < count)
            {
                CardController temp = this.gameObject.AddComponent<CardController>();
                temp.SetCardDisplay(passiveCards[i]);
                if (equipments == null)
                {
                    temp.SetCard(abilityCards[i], false, true);
                    passiveCards[i].SetCard(temp);
                }
                else
                {
                    temp.GetCardDisplay().SetEquipment(equipments[i], healthController.GetComponent<PlayerController>().GetColorTag());
                    passiveCards[i].SetEquipment(equipments[i], healthController.GetComponent<PlayerController>().GetColorTag());
                }

                
                passiveCards[i].Show();
                passiveCards[i].GetComponent<LineRenderer>().enabled = false;
            }
            else
            {
                passiveCards[i].Hide();
            }
        }
    }

    public void Show()
    {
        GetComponent<Canvas>().enabled = true;
        GetComponent<CanvasScaler>().enabled = false;
        GetComponent<CanvasScaler>().enabled = true;
        returnButton.enabled = false;
        returnButton.enabled = true;
        for (int i = 0; i < attackCards.Count; i++)
        {
            if (i < numOfCards)
            {
                attackCards[i].Show();
                attackCards[i].GetComponent<LineRenderer>().enabled = false;
            }
            else
                attackCards[i].Hide();
        }
    }

    public void Hide()
    {
        GetComponent<Canvas>().enabled = false;
        foreach (CardDisplay display in attackCards)
            display.Hide();
        returnButton.enabled = false;
    }
}
