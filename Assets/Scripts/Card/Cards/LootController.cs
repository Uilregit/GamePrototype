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
    private List<Card> starterCards = new List<Card>();
    // Start is called before the first frame update
    void Awake()
    {
        if (LootController.loot == null)
            LootController.loot = this;
        else
        {
            Destroy(this.gameObject);
            return;
        }

        DontDestroyOnLoad(this.gameObject);

        foreach (Card card in lootTable.cardLoot)
        {
            if (card.rarity == Card.Rarity.Rare)
                rareCards.Add(card);
            else if (card.rarity == Card.Rarity.Common)
                commonCards.Add(card);
            else if (card.rarity == Card.Rarity.Starter)
                starterCards.Add(card);
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
        List<Card> cards = new List<Card>();
        foreach (CardController c in DeckController.deckController.GetDeck())
            cards.Add(c.GetCard());
        if (cards.Contains(commonCards[index])) //If card already in deck, reroll
            index = Random.Range(0, commonCards.Count);
        return commonCards[index];
    }

    public Card GetCardWithName(string name)
    {
        foreach (Card c in lootTable.cardLoot)
            if (c.name == name)
                return c;
        return null;
    }
}
