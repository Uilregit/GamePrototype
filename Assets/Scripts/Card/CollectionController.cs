using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System.Runtime.InteropServices.WindowsRuntime;

[System.Serializable]
public class EditorCardsWrapper
{
    public List<Card> deck;

    public void SetDeck(List<Card> l)
    {
        deck = new List<Card>();
        deck = l.ConvertAll(x => x);
    }
}

public class EquipmentWrapper
{
    public List<Equipment> equipments = new List<Equipment>();
}

public class CollectionController : MonoBehaviour
{
    public static CollectionController collectionController;

    public bool isSinglePlayer;
    public bool isStoryMode = false;
    public DeckCustomizeCardController[] custCardsDisplay;
    [SerializeField]
    public SelectedCardController[] selectedCardsDisplay;
    public DeckButtonController[] deckButtons;
    public PageButtonController[] pageButtons;
    public FinalizeButtonController finalizeButton;
    [SerializeField]
    public EditorCardsWrapper[] editorDeck;
    public EditorCardsWrapper[] storyModeDeck;
    public EditorCardsWrapper[] debugDeck;
    public EditorCardsWrapper[] multiplayerDeck;
    private ListWrapper[] completeDeck = new ListWrapper[3];
    private ListWrapper[] selectedDeck = new ListWrapper[3];
    private ListWrapper[] newCards = new ListWrapper[3];
    private EquipmentWrapper completeEquipments;
    private Dictionary<string, EquipmentWrapper> selectedEquipments = new Dictionary<string, EquipmentWrapper>();
    private CardController recentRewardsCard;

    private List<Dictionary<string, int>> uniqueCards;
    private Dictionary<string, int> uniqueEquipments;
    private int deckID = 0;
    private int page = 0;

    public Image weaponImage;
    public Image accessoryImage;
    private bool isShowingCards = true;

    private void Awake()
    {
        if (CollectionController.collectionController == null)
            CollectionController.collectionController = this;
        else
        {
            Destroy(this.gameObject);
            return;
        }

        DontDestroyOnLoad(this.gameObject);

        for (int i = 0; i < 3; i++)
        {
            completeDeck[i] = new ListWrapper();
            newCards[i] = new ListWrapper();
            newCards[i].deck = new List<CardController>();
        }

        //Create the completeDeck variable from decks in the editor
        //Only includes decks from colors in the party and sorts them by their color order in the party
        EditorCardsWrapper[] usedDeck;
        if (InformationLogger.infoLogger.debug)
        {
            if (isStoryMode)
                usedDeck = storyModeDeck;
            else
                usedDeck = debugDeck;
        }
        else if (isStoryMode)
            usedDeck = storyModeDeck;
        else if (isSinglePlayer)
            usedDeck = editorDeck;
        else
            usedDeck = multiplayerDeck;

        if (!isSinglePlayer)
            SetupMultiplayerDeck();
        else if (isStoryMode)
        {
            selectedDeck = new ListWrapper[storyModeDeck.Length]; //Deep copy completeDeck to avoid deleting cards in that list
            for (int i = 0; i < storyModeDeck.Length; i++)
            {
                selectedDeck[i] = new ListWrapper();
                List<CardController> temp = new List<CardController>();
                foreach (Card c in storyModeDeck[i].deck)
                {
                    CardController j = this.gameObject.AddComponent<CardController>();
                    j.SetCard(c, true, false);
                    j.SetStartedInDeck(true);
                    temp.Add(j);
                }
                selectedDeck[i].SetDeck(temp);
            }
        }
        else
        {
            for (int i = 0; i < usedDeck.Length; i++)
            {
                if (!PartyController.party.partyColors.Contains(usedDeck[i].deck[0].casterColor))
                    continue;

                List<CardController> temp = new List<CardController>();
                foreach (Card c in usedDeck[i].deck)
                {
                    CardController j = this.gameObject.AddComponent<CardController>();
                    j.SetCard(c, true, false);
                    temp.Add(j);
                }
                completeDeck[PartyController.party.GetPartyIndex(usedDeck[i].deck[0].casterColor)].SetDeck(temp);
            }

            selectedDeck = new ListWrapper[completeDeck.Length]; //Deep copy completeDeck to avoid deleting cards in that list
            for (int i = 0; i < completeDeck.Length; i++)
            {
                selectedDeck[i] = new ListWrapper();
                List<CardController> temp = new List<CardController>();
                foreach (CardController c in completeDeck[i].deck)
                {
                    c.SetStartedInDeck(true);
                    temp.Add(c);
                }
                selectedDeck[i].SetDeck(temp);
            }

            ReCountUniqueCards();
            SetDeck(0);
            ResolveSelectedList();
            FinalizeDeck();
            CheckDeckComplete();
            CheckPageButtons();
            RefreshDecks();
        }
    }

    public void ResolveSelectedList()
    {
        for (int i = 0; i < 3; i++)
            foreach (CardController card in selectedDeck[i].deck)
            {
                int deckIndex = PartyController.party.GetPartyIndex(card.GetCard().casterColor);
                uniqueCards[deckIndex][card.GetCard().name] -= 1;
            }
    }

    public void ResolveSelectedEquipmentList()
    {
        ReCountUniqueEquipments();

        foreach (string partyColor in PartyController.party.GetPotentialPlayerTexts())
            if (selectedEquipments.ContainsKey(partyColor))
                foreach (Equipment equip in selectedEquipments[partyColor].equipments)
                    uniqueEquipments[equip.equipmentName] -= 1;
    }

    public CardController GetCardWithName(string name)
    {
        for (int i = 0; i < 3; i++)
            foreach (CardController card in completeDeck[i].deck)
                if (card.GetCard().name == name)
                    return card;
        return null;
    }

    public void RefreshDecks()
    {
        for (int i = 0; i < custCardsDisplay.Length; i++)  //Display selectable cards
        {
            if (i + custCardsDisplay.Length * page < uniqueCards[deckID].Keys.Count)
            {
                custCardsDisplay[i].Show();
                string name = uniqueCards[deckID].Keys.ToArray<string>()[i + custCardsDisplay.Length * page];
                custCardsDisplay[i].SetCard(GetCardWithName(name));
                custCardsDisplay[i].SetIsShowingCard(true);
                custCardsDisplay[i].SetCount(uniqueCards[deckID][name]);
                if (uniqueCards[deckID][name] == 0)
                {
                    custCardsDisplay[i].GetComponent<Collider2D>().enabled = false;
                    custCardsDisplay[i].greyOut.enabled = true;
                }
                else
                {
                    custCardsDisplay[i].GetComponent<Collider2D>().enabled = true;
                    custCardsDisplay[i].greyOut.enabled = false;
                }
                custCardsDisplay[i].highlight.enabled = newCards[deckID].deck.Contains(GetCardWithName(name));
            }
            else
                custCardsDisplay[i].Hide();
        }

        RefreshSelectDecks();
    }

    public void RefreshSelectDecks()
    {
        selectedDeck[deckID].deck = selectedDeck[deckID].deck.OrderBy(o => o.GetCard().manaCost).ThenBy(o => o.GetCard().energyCost).ThenBy(o => o.GetCard().name).ToList();
        for (int j = 0; j < 8; j++)
        {
            if (j < selectedDeck[deckID].deck.Count)
            {
                selectedCardsDisplay[j].SetCard(selectedDeck[deckID].deck[j]);
                selectedCardsDisplay[j].Show();
            }
            else
                selectedCardsDisplay[j].Hide();
        }
    }

    public void RefreshEquipments()
    {
        ResolveSelectedEquipmentList();

        //Order the equiped equipments by colors, with the selected player color equipments always being first
        List<Equipment> equipedEquipment = new List<Equipment>();
        List<string> equipedColor = new List<string>();
        if (selectedEquipments.ContainsKey(PartyController.party.GetPlayerColorTexts()[deckID]))
            foreach (Equipment e in selectedEquipments[PartyController.party.GetPlayerColorTexts()[deckID]].equipments)
            {
                equipedEquipment.Add(e);
                equipedColor.Add(PartyController.party.GetPlayerColorTexts()[deckID]);
            }
        foreach (string color in PartyController.party.GetPotentialPlayerTexts())
            if (selectedEquipments.ContainsKey(color) && color != PartyController.party.GetPlayerColorTexts()[deckID])
                foreach (Equipment e in selectedEquipments[color].equipments)
                {
                    equipedEquipment.Add(e);
                    equipedColor.Add(color);
                }

        //Draws all the cards in the select area
        for (int i = 0; i < custCardsDisplay.Length; i++)
        {
            //Drawing the equiped equipments
            if (i + custCardsDisplay.Length * page < equipedEquipment.Count)
            {
                custCardsDisplay[i].Show();
                custCardsDisplay[i].GetComponentInChildren<CardDisplay>().SetEquipment(equipedEquipment[i + custCardsDisplay.Length * page], PartyController.party.GetPlayerCasterColor(equipedColor[i + custCardsDisplay.Length * page]));
                custCardsDisplay[i].SetIsShowingCard(false);
                custCardsDisplay[i].SetEquipment(equipedEquipment[i + custCardsDisplay.Length * page], PartyController.party.GetPlayerCasterColor(PartyController.party.GetPlayerColorTexts()[deckID]));
                custCardsDisplay[i].SetCount(1);

                custCardsDisplay[i].GetComponent<Collider2D>().enabled = true;
                custCardsDisplay[i].greyOut.enabled = false;
            }
            //Drawing the unequiped equipments
            else if (i + custCardsDisplay.Length * page < uniqueEquipments.Keys.Count + equipedEquipment.Count)
            {
                custCardsDisplay[i].Show();
                string name = uniqueEquipments.Keys.ToArray<string>()[i + custCardsDisplay.Length * page - equipedEquipment.Count];
                if (uniqueEquipments[name] < 1)
                {
                    custCardsDisplay[i].Hide();
                    continue;
                }
                Equipment equip = LootController.loot.GetEquipment(name);
                custCardsDisplay[i].GetComponentInChildren<CardDisplay>().SetEquipment(equip, Card.CasterColor.Passive);
                custCardsDisplay[i].SetIsShowingCard(false);
                custCardsDisplay[i].SetEquipment(equip, PartyController.party.GetPlayerCasterColor(PartyController.party.GetPlayerColorTexts()[deckID]));
                custCardsDisplay[i].SetCount(uniqueEquipments[name]);
                if (uniqueEquipments[name] <= 0)
                {
                    custCardsDisplay[i].GetComponent<Collider2D>().enabled = false;
                    custCardsDisplay[i].greyOut.enabled = true;
                }
                else
                {
                    custCardsDisplay[i].GetComponent<Collider2D>().enabled = true;
                    custCardsDisplay[i].greyOut.enabled = false;
                }
            }
            else
                custCardsDisplay[i].Hide();
        }

        //Draws the weapon and accessory image that shows how many customizable card slot there are in the selected card area
        weaponImage.gameObject.SetActive(false);
        accessoryImage.gameObject.SetActive(false);
        int weaponCardSlots = 0;
        if (selectedEquipments.ContainsKey(PartyController.party.GetPlayerColorTexts()[deckID]))
        {
            foreach (Equipment equip in selectedEquipments[PartyController.party.GetPlayerColorTexts()[deckID]].equipments)
                if (equip.isWeapon)
                    weaponCardSlots = equip.numOfCardSlots;

            foreach (Equipment equip in selectedEquipments[PartyController.party.GetPlayerColorTexts()[deckID]].equipments)
            {
                if (equip.isWeapon)
                {
                    weaponImage.gameObject.SetActive(true);
                    weaponImage.rectTransform.sizeDelta = new Vector2(equip.numOfCardSlots * 0.6f, accessoryImage.rectTransform.sizeDelta.y);
                }
                else
                {
                    accessoryImage.gameObject.SetActive(true);
                    accessoryImage.rectTransform.sizeDelta = new Vector2(equip.numOfCardSlots * 0.6f, accessoryImage.rectTransform.sizeDelta.y);
                    accessoryImage.transform.localPosition = weaponImage.transform.localPosition + new Vector3(weaponCardSlots * 0.6f, 0);
                }
            }
        }

        CheckPageButtons();
    }

    public void NextPage()
    {
        page += 1;
        if (isShowingCards)
            RefreshDecks();
        else
            RefreshEquipments();
        CheckPageButtons();
    }

    public void PreviousPage()
    {
        page -= 1;
        if (isShowingCards)
            RefreshDecks();
        else
            RefreshEquipments();
        CheckPageButtons();
    }

    public void CheckPageButtons()
    {
        if (page == 0)
            pageButtons[0].Enable(false);
        else
            pageButtons[0].Enable(true);

        if (isShowingCards)
        {
            if ((custCardsDisplay.Length * (page + 1) < uniqueCards[deckID].Keys.Count && isShowingCards))
                pageButtons[1].Enable(true);
            else
                pageButtons[1].Enable(false);
        }
        else
        {
            int numOfSelectedEquipments = 0;
            foreach (string key in selectedEquipments.Keys)
                foreach (Equipment e in selectedEquipments[key].equipments)
                    numOfSelectedEquipments++;

            if (custCardsDisplay.Length * (page + 1) < uniqueEquipments.Keys.Count + numOfSelectedEquipments && !isShowingCards)
                pageButtons[1].Enable(true);
            else
                pageButtons[1].Enable(false);
        }
    }

    public void AddCard(CardController newCard)
    {
        int deckIndex = PartyController.party.GetPartyIndex(newCard.GetCard().casterColor);
        if (selectedDeck[deckIndex].deck.Count < 8)
        {
            selectedDeck[deckIndex].deck.Add(newCard);
            uniqueCards[deckIndex][newCard.GetCard().name] -= 1;
        }

        try
        {
            InformationLogger.infoLogger.SaveDeckInfo(InformationLogger.infoLogger.patchID,
                            InformationLogger.infoLogger.gameID,
                            RoomController.roomController.worldLevel.ToString(),
                            RoomController.roomController.selectedLevel.ToString(),
                            newCard.GetCard().casterColor.ToString(),
                            newCard.GetCard().name,
                            newCard.GetCard().energyCost.ToString(),
                            newCard.GetCard().manaCost.ToString(),
                            "True",
                            "False",
                            "False");
        }
        catch { }

        CheckDeckComplete();
        RefreshDecks();
    }

    public void AddEquipment(Equipment equip, Card.CasterColor color)
    {
        Equipment temp = null;
        //Chekcs if there is already a weapon/accessory that's equiped
        if (selectedEquipments.ContainsKey(PartyController.party.partyColors[deckID].ToString()))
            foreach (Equipment e in selectedEquipments[PartyController.party.partyColors[deckID].ToString()].equipments)
            {
                if (e.isWeapon == equip.isWeapon)
                {
                    temp = equip;
                    break;
                }
            }
        else
            selectedEquipments[PartyController.party.partyColors[deckID].ToString()] = new EquipmentWrapper();

        if (temp != null)
        {
            selectedEquipments[PartyController.party.partyColors[deckID].ToString()].equipments.Remove(temp);
        }
        selectedEquipments[PartyController.party.partyColors[deckID].ToString()].equipments.Add(equip);

        RefreshEquipments();
    }

    public void RemoveCard(CardController newCard)
    {
        int deckIndex = PartyController.party.GetPartyIndex(newCard.GetCard().casterColor);
        selectedDeck[deckIndex].deck.Remove(newCard);
        uniqueCards[deckIndex][newCard.GetCard().name] += 1;

        try
        {
            InformationLogger.infoLogger.SaveDeckInfo(InformationLogger.infoLogger.patchID,
                            InformationLogger.infoLogger.gameID,
                            RoomController.roomController.worldLevel.ToString(),
                            RoomController.roomController.selectedLevel.ToString(),
                            newCard.GetCard().casterColor.ToString(),
                            newCard.GetCard().name,
                            newCard.GetCard().energyCost.ToString(),
                            newCard.GetCard().manaCost.ToString(),
                            "False",
                            "True",
                            "False");
        }
        catch { }

        CheckDeckComplete();
        RefreshDecks();
    }

    public void FinalizeDeck()
    {
        DeckController.deckController.SetDecks(selectedDeck);
    }

    public void SetupMultiplayerDeck()
    {
        EditorCardsWrapper[] usedDeck;
        usedDeck = multiplayerDeck;
        selectedDeck = new ListWrapper[completeDeck.Length];

        for (int i = 0; i < usedDeck.Length; i++)
        {
            if (!PartyController.party.partyColors.Contains(usedDeck[i].deck[0].casterColor))
                continue;

            List<CardController> temp = new List<CardController>();
            List<Card> allCards = LootController.loot.GetAllCards(usedDeck[i].deck[0].casterColor);
            foreach (Card c in allCards)
                for (int x = 0; x < 3; x++)
                {
                    CardController j = this.gameObject.AddComponent<CardController>();
                    j.SetCard(c, true, false);
                    temp.Add(j);
                }

            completeDeck[PartyController.party.GetPartyIndex(usedDeck[i].deck[0].casterColor)].SetDeck(temp);

            temp = new List<CardController>();
            foreach (Card c in usedDeck[i].deck)
            {
                CardController j = this.gameObject.AddComponent<CardController>();
                j.SetCard(c, true, false);
                j.SetStartedInDeck(true);
                temp.Add(j);
            }
            selectedDeck[PartyController.party.GetPartyIndex(usedDeck[i].deck[0].casterColor)] = new ListWrapper();
            selectedDeck[PartyController.party.GetPartyIndex(usedDeck[i].deck[0].casterColor)].SetDeck(temp);
        }

        ReCountUniqueCards();
        SetDeck(0);
        ResolveSelectedList();
        FinalizeDeck();
        CheckDeckComplete();
        CheckPageButtons();
        RefreshDecks();
    }

    public void SetupStoryModeDeck()
    {
        ReCountUniqueCards();
        SetDeck(0);
        ResolveSelectedList();
        FinalizeDeck();
        CheckDeckComplete();
        CheckPageButtons();
        RefreshDecks();
    }

    public void LogInformation()
    {
        if (RoomController.roomController.selectedLevel != -1)
        {
            List<string> selectedCardNames = new List<string>();
            for (int i = 0; i < selectedDeck.Length; i++)
                foreach (CardController c in selectedDeck[i].deck)
                    selectedCardNames.Add(c.GetCard().name);
            if (selectedCardNames.Contains(recentRewardsCard.GetCard().name))
            {
                InformationLogger.infoLogger.SaveRewardsCardInfo(InformationLogger.infoLogger.patchID,
                            InformationLogger.infoLogger.gameID,
                            RoomController.roomController.worldLevel.ToString(),
                            RoomController.roomController.selectedLevel.ToString(),
                            RoomController.roomController.roomName,
                            recentRewardsCard.GetCard().casterColor.ToString(),
                            recentRewardsCard.GetCard().name,
                            recentRewardsCard.GetCard().energyCost.ToString(),
                            recentRewardsCard.GetCard().manaCost.ToString(),
                            "True",
                            "True");
            }
            else
            {
                InformationLogger.infoLogger.SaveRewardsCardInfo(InformationLogger.infoLogger.patchID,
                            InformationLogger.infoLogger.gameID,
                            RoomController.roomController.worldLevel.ToString(),
                            RoomController.roomController.selectedLevel.ToString(),
                            RoomController.roomController.roomName,
                            recentRewardsCard.GetCard().casterColor.ToString(),
                            recentRewardsCard.GetCard().name,
                            recentRewardsCard.GetCard().energyCost.ToString(),
                            recentRewardsCard.GetCard().manaCost.ToString(),
                            "True",
                            "False");
            }
            for (int i = 0; i < selectedDeck.Length; i++)
            {
                foreach (CardController card in selectedDeck[i].deck)
                    InformationLogger.infoLogger.SaveDeckInfo(InformationLogger.infoLogger.patchID,
                            InformationLogger.infoLogger.gameID,
                            RoomController.roomController.worldLevel.ToString(),
                            RoomController.roomController.selectedLevel.ToString(),
                            card.GetCard().casterColor.ToString(),
                            card.GetCard().name,
                            card.GetCard().energyCost.ToString(),
                            card.GetCard().manaCost.ToString(),
                            "False",
                            "False",
                            "True");
            }
        }
    }

    public void SetDeck(int newDeck)
    {
        page = 0;
        deckID = newDeck;
        if (isShowingCards)
        {
            RefreshDecks();
            RefreshSelectDecks();
        }
        else
        {
            RefreshEquipments();
        }
        foreach (DeckButtonController i in deckButtons)
            i.GetComponent<RectTransform>().localScale = new Vector2(1, 1);
        deckButtons[deckID].GetComponent<RectTransform>().localScale = new Vector2(1, 1.3f);

        for (int i = 0; i < deckButtons.Length; i++)
        {
            deckButtons[i].GetComponent<Outline>().enabled = newCards[i].deck.Count != 0;
            deckButtons[i].GetComponent<Image>().color = PartyController.party.GetPlayerColor(PartyController.party.partyColors[i]);
        }

        CheckPageButtons();
    }

    public void CheckDeckComplete()
    {
        if (selectedDeck[0].deck.Count == selectedDeck[1].deck.Count && selectedDeck[1].deck.Count == selectedDeck[2].deck.Count && selectedDeck[1].deck.Count == 8)
            finalizeButton.Enable(true);
        else
            finalizeButton.Enable(false);
    }

    public void AddRewardsCard(CardController newCard, bool isRewardsCard = true)
    {
        if (isRewardsCard)
            GameController.gameController.RecordRewardCards(newCard.GetCard());
        recentRewardsCard = newCard;
        int deckIndex = PartyController.party.GetPartyIndex(newCard.GetCard().casterColor);

        CardController c = this.gameObject.AddComponent<CardController>();
        c.SetCard(newCard.GetCard(), true, false);

        if (!newCards[deckIndex].deck.Contains(c))
            newCards[deckIndex].deck.Add(c);
        completeDeck[deckIndex].deck.Add(c);
        ReCountUniqueCards();
        ResolveSelectedList();
        RefreshDecks();
    }

    public void RemoveCardFromNew(CardController newCard)
    {
        int deckIndex = PartyController.party.GetPartyIndex(newCard.GetCard().casterColor);
        if (newCards[deckIndex].deck.Contains(newCard))
            newCards[deckIndex].deck.Remove(newCard);

        for (int i = 0; i < deckButtons.Length; i++)
            if (newCards[i].deck.Count == 0)
                deckButtons[i].GetComponent<Outline>().enabled = false;
            else
                deckButtons[i].GetComponent<Outline>().enabled = true;
    }

    public void ReCountUniqueCards()
    {
        uniqueCards = new List<Dictionary<string, int>>();

        for (int i = 0; i < 3; i++)
        {
            Dictionary<string, int> temp = new Dictionary<string, int>();

            completeDeck[i].deck = completeDeck[i].deck.OrderBy(o => o.GetCard().manaCost).ThenBy(o => o.GetCard().energyCost).ThenBy(o => o.GetCard().name).ToList();
            foreach (CardController card in completeDeck[i].deck)
            {
                if (temp.ContainsKey(card.GetCard().name))
                    temp[card.GetCard().name] += 1;
                else
                    temp[card.GetCard().name] = 1;
            }
            uniqueCards.Add(temp);
        }
    }

    public void ReCountUniqueEquipments()
    {
        uniqueEquipments = new Dictionary<string, int>();

        foreach (Equipment equip in completeEquipments.equipments)
        {
            if (uniqueEquipments.ContainsKey(equip.equipmentName))
                uniqueEquipments[equip.equipmentName] += 1;
            else
                uniqueEquipments[equip.equipmentName] = 1;
        }
    }

    public int GetCountOfCardInCollection(Card card)
    {
        int deckIndex = PartyController.party.GetPartyIndex(card.casterColor);

        int counter = 0;
        foreach (CardController c in completeDeck[deckIndex].deck)
            if (c.GetCard().name == card.name)
                counter++;

        return counter;
    }

    public string[] GetCompleteDeckNames()
    {
        List<string> output = new List<string>();
        for (int i = 0; i < completeDeck.Length; i++)
            for (int j = 0; j < completeDeck[i].deck.Count; j++)
                output.Add(completeDeck[i].deck[j].GetCard().name);

        return output.ToArray();
        /*
        string[][] output = new string[completeDeck.Length][];

        for (int i = 0; i < completeDeck.Length; i++)
        {
            output[i] = new string[completeDeck[i].deck.Count];
            for (int j = 0; j < completeDeck[i].deck.Count; j++)
                output[i][j] = completeDeck[i].deck[j].GetCard().name;
        }
        return output;
        */
    }

    public string[] GetSelectedDeckNames()
    {
        List<string> output = new List<string>();
        for (int i = 0; i < selectedDeck.Length; i++)
            for (int j = 0; j < selectedDeck[i].deck.Count; j++)
                output.Add(selectedDeck[i].deck[j].GetCard().name);

        return output.ToArray();
        /*
        string[][] output = new string[selectedDeck.Length][];
        for (int i = 0; i < selectedDeck.Length; i++)
        {
            output[i] = new string[selectedDeck[i].deck.Count];
            for (int j = 0; j < selectedDeck[i].deck.Count; j++)
                output[i][j] = selectedDeck[i].deck[j].GetCard().name;
        }
        return output;
        */
    }

    public string[] GetNewCardDeckNames()
    {
        List<string> output = new List<string>();
        for (int i = 0; i < newCards.Length; i++)
            for (int j = 0; j < newCards[i].deck.Count; j++)
                output.Add(newCards[i].deck[j].GetCard().name);

        return output.ToArray();
        /*
        string[][] output = new string[newCards.Length][];
        for (int i = 0; i < newCards.Length; i++)
        {
            output[i] = new string[newCards[i].deck.Count];
            for (int j = 0; j < newCards[i].deck.Count; j++)
                output[i][j] = newCards[i].deck[j].GetCard().name;
        }
        return output;
        */
    }

    public void SetCompleteDeck(string[] completeDeckNames)
    {
        for (int i = 0; i < 3; i++)
        {
            completeDeck[i] = new ListWrapper();
            completeDeck[i].deck = new List<CardController>();
        }

        foreach (string name in completeDeckNames)
        {
            CardController cardController = this.gameObject.AddComponent<CardController>();
            cardController.SetCard(LootController.loot.GetCardWithName(name), true, false);
            completeDeck[PartyController.party.GetPartyIndex(cardController.GetCard().casterColor)].deck.Add(cardController);
        }
        /*
        for (int i = 0; i < completeDeckNames.Length; i++)
        {
            completeDeck[i].deck = new List<CardController>();
            for (int j = 0; j < completeDeckNames[i].Length; j++)
            {
                CardController cardController = this.gameObject.AddComponent<CardController>();
                cardController.SetCard(LootController.loot.GetCardWithName(completeDeckNames[i][j]), true, false);
                completeDeck[i].deck.Add(cardController);
            }
        }
        */
    }
    public void SetSelectedDeck(string[] selectedDeckNames)
    {
        selectedDeck = new ListWrapper[3];
        for (int i = 0; i < 3; i++)
        {
            selectedDeck[i] = new ListWrapper();
            selectedDeck[i].deck = new List<CardController>();
        }

        foreach (string name in selectedDeckNames)
        {
            CardController cardController = this.gameObject.AddComponent<CardController>();
            cardController.SetCard(LootController.loot.GetCardWithName(name), true, false);
            selectedDeck[PartyController.party.GetPartyIndex(cardController.GetCard().casterColor)].deck.Add(cardController);
        }
        /*
        for (int i = 0; i < selectedDeckNames.Length; i++)
        {
            selectedDeck[i].deck = new List<CardController>();
            for (int j = 0; j < selectedDeckNames[i].Length; j++)
                selectedDeck[i].deck.Add(GetCardWithName(selectedDeckNames[i][j]));
        }
        */
    }

    public void SetSelectedEquipments(Dictionary<Card.CasterColor, List<string>> selectedEquipmentNames)
    {
        selectedEquipments = new Dictionary<string, EquipmentWrapper>();
        foreach (Card.CasterColor color in selectedEquipmentNames.Keys)
        {
            List<Equipment> equip = new List<Equipment>();
            foreach (string e in selectedEquipmentNames[color])
            {
                equip.Add(LootController.loot.GetEquipment(e));
                uniqueEquipments[e] -= 1;
            }
            selectedEquipments[color.ToString()].equipments = equip;
        }
    }

    public void SetCompleteEquipments(Dictionary<string, int> completeEquipmentNames)
    {
        completeEquipments = new EquipmentWrapper();
        List<Equipment> equip = new List<Equipment>();
        foreach (string e in completeEquipmentNames.Keys)
            for (int i = 0; i < completeEquipmentNames[e]; i++)
                equip.Add(LootController.loot.GetEquipment(e));
        completeEquipments.equipments = equip;

        ReCountUniqueEquipments();
    }

    public void SetNewCardsDeck(string[] newCardDeckNames)
    {
        for (int i = 0; i < 3; i++)
        {
            newCards[i] = new ListWrapper();
            newCards[i].deck = new List<CardController>();
        }

        foreach (string name in newCardDeckNames)
        {
            CardController cardController = this.gameObject.AddComponent<CardController>();
            cardController.SetCard(LootController.loot.GetCardWithName(name), true, false);
            newCards[PartyController.party.GetPartyIndex(cardController.GetCard().casterColor)].deck.Add(cardController);
        }
        /*
        for (int i = 0; i < newCardDeckNames.Length; i++)
        {
            newCards[i].deck = new List<CardController>();
            for (int j = 0; j < newCardDeckNames[i].Length; j++)
                newCards[i].deck.Add(GetCardWithName(newCardDeckNames[i][j]));
        }
        */
    }

    public void SetRecentRewardsCard(string name)
    {
        CardController cardController = this.gameObject.AddComponent<CardController>();
        cardController.SetCard(LootController.loot.GetCardWithName(name), true, false);
        recentRewardsCard = cardController;
    }

    public string GetRecentRewardsCard()
    {
        try
        {
            return recentRewardsCard.GetCard().name;
        }
        catch
        {
            return "null";
        }
    }

    public int GetNumberOfCardsNotStartedInDeck()
    {
        int output = 0;
        for (int i = 0; i < 3; i++)
            foreach (CardController c in selectedDeck[i].deck)
                if (!c.GetStartedInDeck())
                    output++;
        return output;
    }

    ////////////////////////////////////////////
    /////////// Used for story mode ////////////
    ////////////////////////////////////////////
    public Dictionary<string, int> GetSelectedDeckDict()
    {
        Dictionary<string, int> output = new Dictionary<string, int>();
        for (int i = 0; i < 3; i++)
            foreach (CardController card in selectedDeck[i].deck)
                if (output.ContainsKey(card.GetCard().name))
                    output[card.GetCard().name] += 1;
                else
                    output[card.GetCard().name] = 1;

        return output;
    }

    public Dictionary<string, int> GetCompleteDeckDict()
    {
        Dictionary<string, int> output = new Dictionary<string, int>();
        for (int i = 0; i < 3; i++)
            foreach (CardController card in completeDeck[i].deck)
                if (output.ContainsKey(card.GetCard().name))
                    output[card.GetCard().name] += 1;
                else
                    output[card.GetCard().name] = 1;

        return output;
    }

    public void SetIsShowingCards(bool state)
    {
        isShowingCards = state;
    }
}
