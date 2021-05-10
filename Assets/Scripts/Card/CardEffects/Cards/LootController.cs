using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LootController : MonoBehaviour
{
    public static LootController loot;

    public Card ResurrectCard;

    public CardLootTable lootTable;
    public int rarePercentage = 30;

    private List<Card> rareCards = new List<Card>();
    private List<Card> commonCards = new List<Card>();
    private List<Card> starterCards = new List<Card>();
    private List<Card> starterAttackCards = new List<Card>();

    private List<Card> allEnergyCards = new List<Card>();
    private List<Card> allManaCards = new List<Card>();
    /*
    private List<Card> allRareCards = new List<Card>();
    private List<Card> allCommonCards = new List<Card>();
    private List<Card> allStarterCards = new List<Card>();
    private List<Card> allStarterAttackCards = new List<Card>();
    */
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

        ResetPartyLootTable();
    }

    public void ResetPartyLootTable()
    {
        foreach (Card card in lootTable.cardLoot)
        {
            if (card.manaCost > 0)
                allManaCards.Add(card);
            else
                allEnergyCards.Add(card);

            if (!PartyController.party.partyColors.Contains(card.casterColor))
                continue;

            if (card.rarity == Card.Rarity.Rare)
                rareCards.Add(card);
            else if (card.rarity == Card.Rarity.Common)
                commonCards.Add(card);
            else if (card.rarity == Card.Rarity.Starter)
                starterCards.Add(card);

            if (card.rarity == Card.Rarity.StarterAttack)
                starterAttackCards.Add(card);
        }
    }

    public Card GetCard(Card.Rarity rarity = Card.Rarity.Common)
    {
        if (rarity == Card.Rarity.Rare) //If a specific rarity is specified
            return GetRareCard();
        else                            //Else roll based on rarity distribution
        {
            int roll = Random.Range(0, 100);
            if (roll < rarePercentage)
                return GetRareCard();
            else
                return GetCommonCard();
        }
    }

    private Card GetRareCard()
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
        if (cards.Contains(commonCards[index]))                 //If card already in deck, reroll
            index = Random.Range(0, commonCards.Count);         //Not meant to eliminate duplicates, just half chance of getting a card you already have to promote diverse decks
        return commonCards[index];
    }

    public List<Card> GetAllCards(Card.CasterColor color)
    {
        List<Card> output = new List<Card>();
        foreach (Card card in lootTable.cardLoot)
            if (card.casterColor == color)
                output.Add(card);
        return output;
    }

    public Card GetCardWithName(string name)
    {
        foreach (Card c in lootTable.cardLoot)
            if (c.name == name)
                return c;
        foreach (Card c in lootTable.tokenCards)
            if (c.name == name)
                return c;
        return null;
    }

    public Card GetStarterCard(Card.CasterColor color)
    {
        List<Card> viableCards = new List<Card>();
        foreach (Card c in starterCards)
            if (c.casterColor == color)
                viableCards.Add(c);
        int index = Random.Range(0, viableCards.Count);
        return viableCards[index];
    }

    public Card GetStarterAttackCard(Card.CasterColor color)
    {
        List<Card> viableCards = new List<Card>();
        foreach (Card c in starterAttackCards)
            if (c.casterColor == color)
                viableCards.Add(c);
        int index = Random.Range(0, viableCards.Count);
        return viableCards[index];
    }

    public Card GetANYEnergyCard()
    {
        int index = Random.Range(0, allEnergyCards.Count);
        return allEnergyCards[index];
    }

    public Card GetANYManaCard()
    {
        int index = Random.Range(0, allManaCards.Count);
        return allManaCards[index];
    }
}
