using System.Collections;
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
    public Text shieldText;
    public Text attackText;
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

    public void SetDescription(Sprite character, HealthController healthController, List<CardController> cards, List<Buff> buffList, AbilitiesController abilitiesController)
    {
        int currentHealth = healthController.GetVit();
        int maxHealth = healthController.GetMaxVit();
        int attack = healthController.GetAttack();
        int shield = healthController.GetShield();
        numOfCards = cards.Count;

        //Stats section
        characterImage.sprite = character;
        healthText.text = "Health: {c}/{m}".Replace("{c}", currentHealth.ToString()).Replace("{m}", maxHealth.ToString());
        armorText.text = "Attack: {a}".Replace("{a}", attack.ToString());
        shieldText.text = "Armor: {s}".Replace("{s}", shield.ToString());

        if (cards.Count == 0)
            attackText.enabled = false;
        else
            attackText.enabled = true;

        //Attack cards section
        for (int i = 0; i < attackCards.Count; i++)
        {
            if (i < cards.Count)
            {
                attackCards[i].GetComponent<CardController>().SetCaster(healthController.gameObject);
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
        List<string> abilityStrings = abilitiesController.GetAbilityStrings();
        for (int i = 0; i < abilityTexts.Count; i++)
        {
            if (i < abilityStrings.Count)
                abilityTexts[i].text = abilityStrings[i];
            else
                abilityTexts[i].text = "";
        }
    }

    public void Show()
    {
        GetComponent<Canvas>().enabled = true;
        GetComponent<CanvasScaler>().enabled = false;
        GetComponent<CanvasScaler>().enabled = true;
        returnButton.enabled = false;
        returnButton.enabled = true;
        for (int i = 0; i < attackCards.Count; i ++)
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
