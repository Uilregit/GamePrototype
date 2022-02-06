using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LootController : MonoBehaviour
{
    public static LootController loot;

    public Card ResurrectCard;

    public CardLootTable cardLootTable;
    public EquipmentLootTable equipmentLootTable;
    public int rarePercentage = 30;

    public List<OnHitEffect> onHitEffects = new List<OnHitEffect>();

    private List<Card> rareCards = new List<Card>();
    private List<Card> commonCards = new List<Card>();
    private List<Card> starterDefenceCards = new List<Card>();
    private List<Card> starterAttackCards = new List<Card>();
    private List<Card> starterSpecialCards = new List<Card>();

    private List<Card> allEnergyCards = new List<Card>();
    private List<Card> allManaCards = new List<Card>();
    private List<Card> allRareCards = new List<Card>();
    private List<Card> allCommonCards = new List<Card>();

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
        Debug.Log("Party is: " + PartyController.party.partyColors[0] + "|" + PartyController.party.partyColors[1] + "|" + PartyController.party.partyColors[2] + "|");
        foreach (Card card in cardLootTable.cardLoot)
        {
            if (card.manaCost > 0)
                allManaCards.Add(card);
            else
                allEnergyCards.Add(card);

            if (card.rarity == Card.Rarity.Rare)
                allRareCards.Add(card);
            else if (card.rarity == Card.Rarity.Common)
                allCommonCards.Add(card);

            if (!PartyController.party.partyColors.Contains(card.casterColor))
                continue;

            if (card.rarity == Card.Rarity.Rare)
                rareCards.Add(card);
            else if (card.rarity == Card.Rarity.Common)
                commonCards.Add(card);
            else if (card.rarity == Card.Rarity.StarterDefence)
                starterDefenceCards.Add(card);
            else if (card.rarity == Card.Rarity.StarterSpecial)
                starterSpecialCards.Add(card);
            else if (card.rarity == Card.Rarity.StarterAttack)
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

    public Card GetUnlockedCard()
    {
        Card output = null;
        int roll = Random.Range(0, 100);
        while (output == null || !PartyController.party.unlockedPlayerColors.Contains(output.casterColor))
            if (roll < rarePercentage)
            {
                int index = Random.Range(0, allRareCards.Count);
                output = allRareCards[index];
            }
            else
            {
                int index = Random.Range(0, allCommonCards.Count);
                output = allCommonCards[index];
            }
        return output;
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
        foreach (Card card in cardLootTable.cardLoot)
            if (card.casterColor == color)
                output.Add(card);
        return output;
    }

    public Card GetCardWithName(string name)
    {
        foreach (Card c in cardLootTable.cardLoot)
            if (c.name == name)
                return c;
        foreach (Card c in cardLootTable.tokenCards)
            if (c.name == name)
                return c;
        return null;
    }

    public Card GetStarterCard(Card.CasterColor color)
    {
        List<Card> viableCards = new List<Card>();
        List<Card> starterCards = starterAttackCards;
        starterCards.AddRange(starterDefenceCards);
        starterCards.AddRange(starterSpecialCards);
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

    public Card GetStarterDefenceCard(Card.CasterColor color)
    {
        List<Card> viableCards = new List<Card>();
        foreach (Card c in starterDefenceCards)
            if (c.casterColor == color)
                viableCards.Add(c);
        int index = Random.Range(0, viableCards.Count);
        return viableCards[index];
    }

    public Card GetStarterSpecialCard(Card.CasterColor color)
    {
        List<Card> viableCards = new List<Card>();
        foreach (Card c in starterSpecialCards)
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

    public Equipment GetEquipment(string name)
    {
        Equipment output = null;
        foreach (Equipment e in equipmentLootTable.equipmentLoot)
            if (e.equipmentName == name)
                output = e;
        return output;
    }

    public Equipment GetRandomEquipment()
    {
        int index = Random.Range(0, equipmentLootTable.equipmentLoot.Length);
        return equipmentLootTable.equipmentLoot[index];
    }

    //Returns all cards that synergize with that color by looking at the required materials to craft that card. (cards that synergize with the color would require that color's material to craf)
    public List<Card> GetSynergizedCards(string color)
    {
        List<Card> output = new List<Card>();
        List<StoryModeController.RewardsType> colorMaterial = GetColorRewardMaterialTypes(color);
        foreach (Card c in cardLootTable.cardLoot)
            if (((c.casterColor.ToString() == color && c.materials.Count == 2) ||                                                   //If the card is the chosen color, check that it's synergizing with itself
                (c.casterColor.ToString() != color && PartyController.party.GetPlayerColorTexts().Contains(c.casterColor.ToString()) && c.materials.Any(x => colorMaterial.Contains(x)))) && //If the card is of another color, check that it's synergizing with the chosen color
                new List<Card.Rarity>() { Card.Rarity.Common, Card.Rarity.Rare, Card.Rarity.Legendary }.Contains(c.rarity))         //Makes sure that starter cards are never given
                output.Add(c);
        return output;
    }

    private List<StoryModeController.RewardsType> GetColorRewardMaterialTypes(string color)
    {
        List<StoryModeController.RewardsType> output = new List<StoryModeController.RewardsType>();
        switch (color)
        {
            case "Red":
                output.Add(StoryModeController.RewardsType.RubyCrystal);
                output.Add(StoryModeController.RewardsType.RubyGem);
                output.Add(StoryModeController.RewardsType.RubyShard);
                break;
            case "Blue":
                output.Add(StoryModeController.RewardsType.SapphireCrystal);
                output.Add(StoryModeController.RewardsType.SapphireGem);
                output.Add(StoryModeController.RewardsType.SapphireShard);
                break;
            case "Green":
                output.Add(StoryModeController.RewardsType.EmeraldCrystal);
                output.Add(StoryModeController.RewardsType.EmeraldGem);
                output.Add(StoryModeController.RewardsType.EmeraldShard);
                break;
            case "Orange":
                output.Add(StoryModeController.RewardsType.TopazCrystal);
                output.Add(StoryModeController.RewardsType.TopazGem);
                output.Add(StoryModeController.RewardsType.TopazShard);
                break;
            case "White":
                output.Add(StoryModeController.RewardsType.QuartzCrystal);
                output.Add(StoryModeController.RewardsType.QuartzGem);
                output.Add(StoryModeController.RewardsType.QuartzShard);
                break;
            case "Black":
                output.Add(StoryModeController.RewardsType.OnyxCrystal);
                output.Add(StoryModeController.RewardsType.OnyxGem);
                output.Add(StoryModeController.RewardsType.OnyxShard);
                break;
        }
        return output;
    }

    public OnHitEffect GetOnHitEffect(Card.HitEffect triggerName)
    {
        foreach (OnHitEffect effect in onHitEffects)
            if (effect.effectName == triggerName)
                return effect;
        return null;
    }
}
