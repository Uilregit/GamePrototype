using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopController : MonoBehaviour
{
    public static ShopController shop;

    public List<ShopCardController> shopCards;

    private List<Card> allShopCards;
    private List<Card> chosenCards;
    private List<Equipment> allShopEquipments;
    private List<Equipment> chosenEquipments;

    private int latestDeckID;
    private bool boughtCard = false;

    private void Start()
    {
        if (ShopController.shop == null)
            ShopController.shop = this;
        else
            Destroy(this.gameObject);

        allShopCards = new List<Card>();
        allShopEquipments = new List<Equipment>();
        chosenCards = new List<Card>();
        chosenEquipments = new List<Equipment>();

        foreach (ShopCardController card in shopCards)
        {
            if (Random.Range(0.0f, 1.0f) <= ResourceController.resource.shopEquipmentPercentage)
            {
                Equipment thisEquipment = LootController.loot.GetRandomEquipment();
                for (int j = 0; j < 100; j++)
                {
                    thisEquipment = LootController.loot.GetRandomEquipment();
                    if (!allShopEquipments.Contains(thisEquipment) && CollectionController.collectionController.GetCountOfEquipmentInCollection(thisEquipment) == 0)     //Shops will never offer equipment you already have
                        break;
                }
                card.GetComponent<CardController>().SetEquipment(thisEquipment, Card.CasterColor.Passive);
                card.SetEquipment(card.GetComponent<CardController>().GetEquipment());
                allShopEquipments.Add(thisEquipment);

                card.SetPrice(ResourceController.resource.commonEquipmentPrice);
            }
            else
            {
                Card thisCard = LootController.loot.GetCard();
                for (int j = 0; j < 100; j++)
                {
                    thisCard = LootController.loot.GetCard();
                    if (!allShopCards.Contains(thisCard) && CollectionController.collectionController.GetCountOfCardInCollection(thisCard) < 4) //Ensures that all rewards are unique
                        break;
                }
                card.GetComponent<CardController>().SetCard(thisCard, false);
                card.SetCard(card.GetComponent<CardController>());
                allShopCards.Add(thisCard);

                card.SetPrice(ResourceController.resource.commonCardPrice);
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
            card.ResetBuyable();

        latestDeckID = PartyController.party.GetPartyIndex(c.GetCard().casterColor);
    }

    public void ReportBoughtEquipment(Equipment e)
    {
        chosenEquipments.Add(e);
        allShopEquipments.Remove(e);

        foreach (ShopCardController card in shopCards)
            card.ResetBuyable();
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
        //Record info for cards
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

        //Record info for equipments
        chosenCosts = new List<int>();
        unpickedCosts = new List<int>();
        foreach (Equipment e in chosenEquipments)
            if (e.rarity == Card.Rarity.Common)
                chosenCosts.Add(ResourceController.resource.commonEquipmentPrice);
            else
                chosenCosts.Add(ResourceController.resource.rareEquipmentPrice);

        foreach (Equipment e in allShopEquipments)
            if (e.rarity == Card.Rarity.Common)
                unpickedCosts.Add(ResourceController.resource.commonEquipmentPrice);
            else
                unpickedCosts.Add(ResourceController.resource.rareEquipmentPrice);

        for (int i = 0; i < chosenEquipments.Count; i++)
        {
            InformationLogger.infoLogger.SaveShopCardInfo(InformationLogger.infoLogger.patchID,
                    InformationLogger.infoLogger.gameID,
                    RoomController.roomController.worldLevel.ToString(),
                    RoomController.roomController.selectedLevel.ToString(),
                    RoomController.roomController.roomName,
                    "Equipment",
                    chosenEquipments[i].name,
                    "0",
                    "0",
                    "True",
                    chosenCosts[i].ToString());
        }
        for (int i = 0; i < allShopEquipments.Count; i++)
        {
            InformationLogger.infoLogger.SaveShopCardInfo(InformationLogger.infoLogger.patchID,
                    InformationLogger.infoLogger.gameID,
                    RoomController.roomController.worldLevel.ToString(),
                    RoomController.roomController.selectedLevel.ToString(),
                    RoomController.roomController.roomName,
                    "Equipment",
                    allShopEquipments[i].name,
                    "0",
                    "0",
                    "False",
                    unpickedCosts[i].ToString());
        }
    }
}
