using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopController : MonoBehaviour
{
    public static ShopController shop;

    public List<ShopCardController> shopCards;

    public List<Card> allShopCards;
    public List<Card> chosenCards;

    private int latestDeckID;
    private bool boughtCard = false;

    private void Start()
    {
        if (ShopController.shop == null)
            ShopController.shop = this;
        else
            Destroy(this.gameObject);

        allShopCards = new List<Card>();
        chosenCards = new List<Card>();

        foreach (ShopCardController card in shopCards)
        {
            for (int j = 0; j < 100; j++)
            {
                Card thisCard = LootController.loot.GetCard();
                if (!allShopCards.Contains(thisCard) && CollectionController.collectionController.GetCountOfCardInCollection(thisCard) < 4) //Ensures that all rewards are unique
                {
                    card.GetComponent<CardController>().SetCard(thisCard, false);
                    card.SetCard(card.GetComponent<CardController>());
                    allShopCards.Add(thisCard);

                    card.SetPrice(ResourceController.resource.commonCardPrice);
                    break;
                }
            }

            card.ResetBuyable();
        }
    }

    public void ReportBoughtCard(CardController c)
    {
        boughtCard = true;

        chosenCards.Add(c.GetCard());
        allShopCards.Remove(c.GetCard());

        foreach (ShopCardController card in shopCards)
        {
            card.ResetBuyable();
        }

        latestDeckID = PartyController.party.GetPartyIndex(c.GetCard().casterColor);
    }

    public int GetLatestDeckID()
    {
        return latestDeckID;
    }

    public bool GetBoughtCard()
    {
        return boughtCard;
    }

    public void RecordShopInformation()
    {
        List<int> chosenCosts = new List<int>();
        List<int> unpickedCosts = new List<int>();
        foreach (Card c in chosenCards)
            if (c.rarity == Card.Rarity.Common)
                chosenCosts.Add(ResourceController.resource.commonCardPrice);
            else
                chosenCosts.Add(ResourceController.resource.rareCardPrice);

        foreach (Card c in allShopCards)
            if (c.rarity == Card.Rarity.Common)
                unpickedCosts.Add(ResourceController.resource.commonCardPrice);
            else
                unpickedCosts.Add(ResourceController.resource.rareCardPrice);

        for (int i = 0; i < chosenCards.Count; i++)
        {
            InformationLogger.infoLogger.SaveShopCardInfo(InformationLogger.infoLogger.patchID,
                    InformationLogger.infoLogger.gameID,
                    RoomController.roomController.worldLevel.ToString(),
                    RoomController.roomController.selectedLevel.ToString(),
                    RoomController.roomController.roomName,
                    chosenCards[i].casterColor.ToString(),
                    chosenCards[i].name,
                    chosenCards[i].energyCost.ToString(),
                    chosenCards[i].manaCost.ToString(),
                    "True",
                    chosenCosts[i].ToString());
        }
        for (int i = 0; i < allShopCards.Count; i++)
        {
            InformationLogger.infoLogger.SaveShopCardInfo(InformationLogger.infoLogger.patchID,
                    InformationLogger.infoLogger.gameID,
                    RoomController.roomController.worldLevel.ToString(),
                    RoomController.roomController.selectedLevel.ToString(),
                    RoomController.roomController.roomName,
                    allShopCards[i].casterColor.ToString(),
                    allShopCards[i].name,
                    allShopCards[i].energyCost.ToString(),
                    allShopCards[i].manaCost.ToString(),
                    "False",
                    unpickedCosts[i].ToString());
        }
    }
}
