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

public class CollectionController : MonoBehaviour
{
    public static CollectionController collectionController;

    public DeckCustomizeCardController[] custCardsDisplay;
    [SerializeField]
    public SelectedCardController[] selectedCardsDisplay;
    public DeckButtonController[] deckButtons;
    public PageButtonController[] pageButtons;
    public FinalizeButtonController finalizeButton;
    [SerializeField]
    public EditorCardsWrapper[] editorDeck;
    public EditorCardsWrapper[] debugDeck;
    private ListWrapper[] completeDeck = new ListWrapper[3];
    private ListWrapper[] selectedDeck = new ListWrapper[3];
    private ListWrapper[] newCards = new ListWrapper[3];
    private CardController recentRewardsCard;

    private List<Dictionary<string, int>> uniqueCards;
    private int deckID = 0;
    private int page = 0;

    private void Awake()
    {
        if (CollectionController.collectionController == null)
            CollectionController.collectionController = this;
        else
            Destroy(this.gameObject);

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
            usedDeck = debugDeck;
        else
            usedDeck = editorDeck;

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
                temp.Add(c);
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

    public void ResolveSelectedList()
    {
        for (int i = 0; i < 3; i++)
            foreach (CardController card in selectedDeck[i].deck)
            {
                int deckIndex = PartyController.party.GetPartyIndex(card.GetCard().casterColor);
                uniqueCards[deckIndex][card.GetCard().name] -= 1;
            }
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
                custCardsDisplay[i].GetComponent<DeckCustomizeCardController>().SetCard(GetCardWithName(name));
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

        for (int i = 0; i < 3; i++) //Display selected cards
        {
            selectedDeck[i].deck = selectedDeck[i].deck.OrderBy(o => o.GetCard().manaCost).ThenBy(o => o.GetCard().energyCost).ThenBy(o => o.GetCard().name).ToList();
            for (int j = 0; j < 8; j++)
            {
                if (j < selectedDeck[i].deck.Count)
                {
                    selectedCardsDisplay[i * 8 + j].SetCard(selectedDeck[i].deck[j]);
                    selectedCardsDisplay[i * 8 + j].Show();
                    if (i == deckID)
                        selectedCardsDisplay[i * 8 + j].Brighten();
                    else
                        selectedCardsDisplay[i * 8 + j].Darken();
                }
                else
                    selectedCardsDisplay[i * 8 + j].Hide();
            }
        }
    }

    public void NextPage()
    {
        page += 1;
        RefreshDecks();
        CheckPageButtons();
    }

    public void PreviousPage()
    {
        page -= 1;
        RefreshDecks();
        CheckPageButtons();
    }

    public void CheckPageButtons()
    {
        if (page == 0)
            pageButtons[0].Enable(false);
        else
            pageButtons[0].Enable(true);

        if (custCardsDisplay.Length * (page + 1) < uniqueCards[deckID].Keys.Count)
            pageButtons[1].Enable(true);
        else
            pageButtons[1].Enable(false);
    }

    public void AddCard(CardController newCard)
    {
        int deckIndex = PartyController.party.GetPartyIndex(newCard.GetCard().casterColor);
        if (selectedDeck[deckIndex].deck.Count < 8)
        {
            selectedDeck[deckIndex].deck.Add(newCard);
            uniqueCards[deckIndex][newCard.GetCard().name] -= 1;
        }

        InformationLogger.infoLogger.SaveDeckInfo(InformationLogger.infoLogger.patchID,
                        InformationLogger.infoLogger.gameID,
                        RoomController.roomController.selectedLevel.ToString(),
                        newCard.GetCard().casterColor.ToString(),
                        newCard.GetCard().name,
                        newCard.GetCard().energyCost.ToString(),
                        newCard.GetCard().manaCost.ToString(),
                        "True",
                        "False",
                        "False");

        CheckDeckComplete();
        RefreshDecks();
    }

    public void RemoveCard(CardController newCard)
    {
        int deckIndex = PartyController.party.GetPartyIndex(newCard.GetCard().casterColor);
        selectedDeck[deckIndex].deck.Remove(newCard);
        uniqueCards[deckIndex][newCard.GetCard().name] += 1;

        InformationLogger.infoLogger.SaveDeckInfo(InformationLogger.infoLogger.patchID,
                        InformationLogger.infoLogger.gameID,
                        RoomController.roomController.selectedLevel.ToString(),
                        newCard.GetCard().casterColor.ToString(),
                        newCard.GetCard().name,
                        newCard.GetCard().energyCost.ToString(),
                        newCard.GetCard().manaCost.ToString(),
                        "False",
                        "True",
                        "False");

        CheckDeckComplete();
        RefreshDecks();
    }

    public void FinalizeDeck()
    {
        DeckController.deckController.SetDecks(selectedDeck);
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
        RefreshDecks();
        foreach (DeckButtonController i in deckButtons)
            i.GetComponent<RectTransform>().localScale = new Vector2(1, 1);
        deckButtons[deckID].GetComponent<RectTransform>().localScale = new Vector2(1, 1.3f);

        for (int i = 0; i < deckButtons.Length; i++)
            deckButtons[i].GetComponent<Outline>().enabled = newCards[i].deck.Count != 0;

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
                    temp[card.GetCard().name] = temp[card.GetCard().name] + 1;
                else
                    temp[card.GetCard().name] = 1;
            }
            uniqueCards.Add(temp);
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

    public string[][] GetCompleteDeckNames()
    {
        string[][] output = new string[completeDeck.Length][];

        for (int i = 0; i < completeDeck.Length; i++)
        {
            output[i] = new string[completeDeck[i].deck.Count];
            for (int j = 0; j < completeDeck[i].deck.Count; j++)
                output[i][j] = completeDeck[i].deck[j].GetCard().name;
        }
        return output;
    }

    public string[][] GetSelectedDeckNames()
    {
        string[][] output = new string[selectedDeck.Length][];
        for (int i = 0; i < selectedDeck.Length; i++)
        {
            output[i] = new string[selectedDeck[i].deck.Count];
            for (int j = 0; j < selectedDeck[i].deck.Count; j++)
                output[i][j] = selectedDeck[i].deck[j].GetCard().name;
        }
        return output;
    }

    public string[][] GetNewCardDeckNames()
    {
        string[][] output = new string[newCards.Length][];
        for (int i = 0; i < newCards.Length; i++)
        {
            output[i] = new string[newCards[i].deck.Count];
            for (int j = 0; j < newCards[i].deck.Count; j++)
                output[i][j] = newCards[i].deck[j].GetCard().name;
        }
        return output;
    }

    public void SetCompleteDeck(string[][] completeDeckNames)
    {
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
    }
    public void SetSelectedDeck(string[][] selectedDeckNames)
    {
        for (int i = 0; i < selectedDeckNames.Length; i++)
        {
            selectedDeck[i].deck = new List<CardController>();
            for (int j = 0; j < selectedDeckNames[i].Length; j++)
                selectedDeck[i].deck.Add(GetCardWithName(selectedDeckNames[i][j]));
        }
    }

    public void SetNewCardsDeck(string[][] newCardDeckNames)
    {
        for (int i = 0; i < newCardDeckNames.Length; i++)
        {
            newCards[i].deck = new List<CardController>();
            for (int j = 0; j < newCardDeckNames[i].Length; j++)
                newCards[i].deck.Add(GetCardWithName(newCardDeckNames[i][j]));
        }
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
}
