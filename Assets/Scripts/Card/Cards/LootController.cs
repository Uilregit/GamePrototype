using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootController : MonoBehaviour
{
    public static LootController loot;

    public CardLootTable lootTable;
    public int rarePercentage = 30;

    private List<Card> rareCards = new List<Card>();
    private List<Card> commonCards = new List<Card>();
    // Start is called before the first frame update
    void Start()
    {
        if (LootController.loot == null)
            LootController.loot = this;
        else
            Destroy(this.gameObject);

        foreach (Card card in lootTable.cardLoot)
        {
            if (card.rarity == Card.Rarity.Rare)
                rareCards.Add(card);
            else
                commonCards.Add(card);
        }
    }

    public Card GetCard (Card.Rarity rarity = Card.Rarity.Common)
    {
        if (rarity == Card.Rarity.Rare) //If a specific rarity is specified
            return GetRareCard();
        else                            //Else roll based on rarity distribution
        {
            int roll = Random.Range(0, 100);
            if (roll <= rarePercentage)
                return GetRareCard();
            else
                return GetCommonCard();
        }
    }

    private Card GetRareCard ()
    {
        int index = Random.Range(0, rareCards.Count);
        return rareCards[index];
    }

    private Card GetCommonCard()
    {
        int index = Random.Range(0, commonCards.Count);
        return commonCards[index];
    }
}
