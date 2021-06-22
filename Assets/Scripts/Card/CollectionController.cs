using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
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
    public Image selectedAreaWhiteOut;
    public DeckButtonController[] deckButtons;
    public PageButtonController[] pageButtons;
    public FinalizeButtonController finalizeButton;
    [SerializeField]
    public EditorCardsWrapper[] editorDeck;
    public EditorCardsWrapper[] storyModeDeck;
    public EditorCardsWrapper[] debugDeck;
    public EditorCardsWrapper[] multiplayerDeck;
    private Dictionary<string, ListWrapper> completeDeck = new Dictionary<string, ListWrapper>();
    private Dictionary<string, ListWrapper> selectedDeck = new Dictionary<string, ListWrapper>();
    private Dictionary<string, ListWrapper> newCards = new Dictionary<string, ListWrapper>();
    private EquipmentWrapper completeEquipments;
    private Dictionary<string, EquipmentWrapper> selectedEquipments = new Dictionary<string, EquipmentWrapper>();
    private CardController recentRewardsCard;

    private Dictionary<string, Dictionary<string, int>> uniqueCards;
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

        foreach (string color in PartyController.party.GetPotentialPlayerTexts())
        {
            completeDeck[color] = new ListWrapper();
            newCards[color] = new ListWrapper();
            newCards[color].deck = new List<CardController>();
            selectedDeck[color] = new ListWrapper();
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
            for (int i = 0; i < storyModeDeck.Length; i++)              //Deep copy completeDeck to avoid deleting cards in that list
            {
                List<CardController> temp = new List<CardController>();
                string color = "";
                foreach (Card c in storyModeDeck[i].deck)
                {
                    CardController j = this.gameObject.AddComponent<CardController>();
                    j.SetCard(c, true, false);
                    j.SetStartedInDeck(true);
                    temp.Add(j);

                    color = c.casterColor.ToString();
                }
                selectedDeck[color].SetDeck(temp);
            }
        }
        else
        {
            for (int i = 0; i < usedDeck.Length; i++)
            {
                if (!PartyController.party.partyColors.Contains(usedDeck[i].deck[0].casterColor))
                    continue;

                List<CardController> temp = new List<CardController>();
                string color = "";
                foreach (Card c in usedDeck[i].deck)
                {
                    CardController j = this.gameObject.AddComponent<CardController>();
                    j.SetCard(c, true, false);
                    temp.Add(j);

                    color = c.casterColor.ToString();
                }
                completeDeck[color].SetDeck(temp);
            }

            foreach (string color in completeDeck.Keys)           //Deep copy completeDeck to avoid deleting cards in that list
            {
                selectedDeck[color] = new ListWrapper();
                List<CardController> temp = new List<CardController>();
                foreach (CardController c in completeDeck[color].deck)
                {
                    c.SetStartedInDeck(true);
                    temp.Add(c);
                }
                selectedDeck[color].SetDeck(temp);
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
        foreach (string color in selectedDeck.Keys)
            foreach (CardController card in selectedDeck[color].deck)
                uniqueCards[card.GetCard().casterColor.ToString()][card.GetCard().name] -= 1;
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
        foreach (string color in completeDeck.Keys)
            foreach (CardController card in completeDeck[color].deck)
                if (card.GetCard().name == name)
                    return card;
        return null;
    }

    public void RefreshDecks()
    {
        for (int i = 0; i < custCardsDisplay.Length; i++)  //Display selectable cards
        {
            if (i + custCardsDisplay.Length * page < uniqueCards[PartyController.party.GetPlayerColorTexts()[deckID]].Keys.Count)
            {
                custCardsDisplay[i].Show();
                string name = uniqueCards[PartyController.party.GetPlayerColorTexts()[deckID]].Keys.ToArray<string>()[i + custCardsDisplay.Length * page];
                custCardsDisplay[i].SetCard(GetCardWithName(name));
                custCardsDisplay[i].SetIsShowingCard(true);
                custCardsDisplay[i].SetCount(uniqueCards[PartyController.party.GetPlayerColorTexts()[deckID]][name]);
                if (uniqueCards[PartyController.party.GetPlayerColorTexts()[deckID]][name] == 0)
                {
                    custCardsDisplay[i].GetComponent<Collider2D>().enabled = false;
                    custCardsDisplay[i].greyOut.enabled = true;
                }
                else
                {
                    custCardsDisplay[i].GetComponent<Collider2D>().enabled = true;
                    custCardsDisplay[i].greyOut.enabled = false;
                }
                custCardsDisplay[i].highlight.enabled = newCards[PartyController.party.GetPlayerColorTexts()[deckID]].deck.Contains(GetCardWithName(name));
            }
            else
                custCardsDisplay[i].Hide();
        }

        RefreshSelectDecks();
        RefreshSelectedEquipments();
    }

    public void RefreshSelectDecks()
    {
        Vector2 cardSlots = GetWeaponAndAccessorySlots();
        int weaponCardSlots = (int)cardSlots.x;
        int accessoryCardSlots = (int)cardSlots.y;

        List<CardController> weaponCards = new List<CardController>();
        List<CardController> accessoryCards = new List<CardController>();
        List<CardController> basicCards = new List<CardController>();
        int numOfBlanks = 0;

        //Reorder all the weapon slot cards
        for (int i = 0; i < weaponCardSlots; i++)
            if (selectedDeck[PartyController.party.GetPlayerColorTexts()[deckID]].deck[i] == null)
                numOfBlanks++;
            else
                weaponCards.Add(selectedDeck[PartyController.party.GetPlayerColorTexts()[deckID]].deck[i]);
        weaponCards = weaponCards.OrderBy(o => o.GetCard().manaCost).ThenBy(o => o.GetCard().energyCost).ThenBy(o => o.GetCard().name).ToList();
        for (int i = 0; i < numOfBlanks; i++)
            weaponCards.Add(null);
        numOfBlanks = 0;

        //Reorder all the accessory slot cards
        for (int i = weaponCardSlots; i < weaponCardSlots + accessoryCardSlots; i++)
            if (selectedDeck[PartyController.party.GetPlayerColorTexts()[deckID]].deck[i] == null)
                numOfBlanks++;
            else
                accessoryCards.Add(selectedDeck[PartyController.party.GetPlayerColorTexts()[deckID]].deck[i]);
        accessoryCards = accessoryCards.OrderBy(o => o.GetCard().manaCost).ThenBy(o => o.GetCard().energyCost).ThenBy(o => o.GetCard().name).ToList();
        for (int i = 0; i < numOfBlanks; i++)
            accessoryCards.Add(null);
        numOfBlanks = 0;

        //Reorder all the basic slot cards
        for (int i = weaponCardSlots + accessoryCardSlots; i < 8; i++)
            if (selectedDeck[PartyController.party.GetPlayerColorTexts()[deckID]].deck[i] == null)
                numOfBlanks++;
            else
                basicCards.Add(selectedDeck[PartyController.party.GetPlayerColorTexts()[deckID]].deck[i]);
        basicCards = basicCards.OrderBy(o => o.GetCard().manaCost).ThenBy(o => o.GetCard().energyCost).ThenBy(o => o.GetCard().name).ToList();
        for (int i = 0; i < numOfBlanks; i++)
            basicCards.Add(null);
        numOfBlanks = 0;

        //Remake the selected deck
        selectedDeck[PartyController.party.GetPlayerColorTexts()[deckID]].deck = new List<CardController>();
        selectedDeck[PartyController.party.GetPlayerColorTexts()[deckID]].deck.AddRange(weaponCards);
        selectedDeck[PartyController.party.GetPlayerColorTexts()[deckID]].deck.AddRange(accessoryCards);
        selectedDeck[PartyController.party.GetPlayerColorTexts()[deckID]].deck.AddRange(basicCards);

        for (int j = 0; j < 8; j++)
        {
            if (selectedDeck[PartyController.party.GetPlayerColorTexts()[deckID]].deck[j] != null)
            {
                selectedCardsDisplay[j].SetCard(selectedDeck[PartyController.party.GetPlayerColorTexts()[deckID]].deck[j]);
                selectedCardsDisplay[j].Show();
            }
            else
            {
                selectedCardsDisplay[j].Hide();
            }
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
                custCardsDisplay[i].SetEquipment(equipedEquipment[i + custCardsDisplay.Length * page], PartyController.party.GetPlayerCasterColor(equipedColor[i + custCardsDisplay.Length * page]));
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
                custCardsDisplay[i].SetEquipment(equip, Card.CasterColor.Passive);
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

        RefreshSelectedEquipments();
        CheckPageButtons();
    }

    public void RefreshSelectedEquipments()
    {
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
            if ((custCardsDisplay.Length * (page + 1) < uniqueCards[PartyController.party.GetPlayerColorTexts()[deckID]].Keys.Count && isShowingCards))
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

    public void AddCard(CardController newCard, int index)
    {
        //Obtain the range of indexes the new card can be put into
        Vector2 ranges = GetSelectCardCategoryIndexRanges(index);
        int startingIndex = (int)ranges.x;
        int range = (int)ranges.y;

        //If any of the slots in the available ranges are null, insert the new card there
        if (selectedDeck[newCard.GetCard().casterColor.ToString()].deck.GetRange(startingIndex, range).Any(x => x == null))
        {
            //Find which index to insert into
            int insertIndex = startingIndex;
            for (int i = startingIndex; i < startingIndex + range; i++)
                if (selectedDeck[newCard.GetCard().casterColor.ToString()].deck[i] == null)
                {
                    insertIndex = i;
                    break;
                }

            //Adds a new copy to prevent duplicates of the same cards be tied together
            CardController copy = this.gameObject.AddComponent<CardController>();
            copy.SetCard(newCard.GetCard(), true, false);

            selectedDeck[newCard.GetCard().casterColor.ToString()].deck[insertIndex] = copy;
            uniqueCards[newCard.GetCard().casterColor.ToString()][newCard.GetCard().name] -= 1;
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
        }

        CheckDeckComplete();
        RefreshDecks();
    }

    public bool GetIfViableSelectSlot(Card c, int index)
    {
        Vector2 range = CollectionController.collectionController.GetWeaponAndAccessorySlots();
        int slots = (int)(range.x + range.y);
        if (SceneManager.GetActiveScene().name == "StoryModeScene")
            return ((c.rarity == Card.Rarity.Starter || c.rarity == Card.Rarity.StarterAttack) || index < slots);  //Only allow cards to be selected in their respective slots in the story mode scene
        else
            return true;
    }

    public void AddEquipment(Equipment equip, Card.CasterColor color)
    {
        Equipment temp = null;
        int weaponCardSlots = 0;
        int accessoryCardSlots = 0;
        int swappedWeaponCardSlots = 0;
        int swappedAccessoryCardSlots = 0;
        //Chekcs if there is already a weapon/accessory that's equiped
        if (selectedEquipments.ContainsKey(PartyController.party.partyColors[deckID].ToString()))
            foreach (Equipment e in selectedEquipments[PartyController.party.partyColors[deckID].ToString()].equipments)
            {
                if (e.isWeapon == equip.isWeapon)
                    temp = e;
                if (e.isWeapon)
                    weaponCardSlots = e.numOfCardSlots;
                else
                    accessoryCardSlots = e.numOfCardSlots;
            }
        else
            selectedEquipments[PartyController.party.partyColors[deckID].ToString()] = new EquipmentWrapper();
        if (selectedEquipments.ContainsKey(color.ToString()))
            foreach (Equipment e in selectedEquipments[color.ToString()].equipments)
                if (e.isWeapon)
                    swappedWeaponCardSlots = e.numOfCardSlots;
                else
                    swappedAccessoryCardSlots = e.numOfCardSlots;
        else
            selectedEquipments[color.ToString()] = new EquipmentWrapper();

        int cardSlotDiff = 0;
        int swappedCardSlotDiff = 0;
        if (color == Card.CasterColor.Passive)                              //If the newly added equipment is unselected by any color, add it to select
        {
            if (temp != null)                                               //If a wapon/accessory is already equiped, remove that before adding the new one
            {
                selectedEquipments[PartyController.party.partyColors[deckID].ToString()].equipments.Remove(temp);
                cardSlotDiff = temp.numOfCardSlots - equip.numOfCardSlots;
            }
            selectedEquipments[PartyController.party.partyColors[deckID].ToString()].equipments.Add(equip);
        }
        else
        {
            if (color == PartyController.party.partyColors[deckID])         //If the newly added equipment was already selected, remove it
            {
                selectedEquipments[PartyController.party.partyColors[deckID].ToString()].equipments.Remove(temp);
                cardSlotDiff = temp.numOfCardSlots;
            }
            else                                                            //If the newly added equipment is from a diffrent color, swap it
            {
                Equipment swap = equip;
                selectedEquipments[PartyController.party.partyColors[deckID].ToString()].equipments.Remove(temp);
                selectedEquipments[PartyController.party.partyColors[deckID].ToString()].equipments.Add(equip);
                selectedEquipments[color.ToString()].equipments.Remove(equip);
                swappedCardSlotDiff = equip.numOfCardSlots;
                if (temp != null)                                           //The selected color only get's the old current color equipment if current color actually have a weapon/accessory in that slot
                {
                    selectedEquipments[color.ToString()].equipments.Add(swap);
                    swappedCardSlotDiff -= swap.numOfCardSlots;
                    cardSlotDiff = temp.numOfCardSlots - equip.numOfCardSlots;
                }
            }
        }

        if (temp != null)
        {
            if (cardSlotDiff > 0)
            {
                if (equip.isWeapon)
                {
                    for (int i = weaponCardSlots; i >= weaponCardSlots - cardSlotDiff; i--)
                        if (selectedDeck[PartyController.party.partyColors[deckID].ToString()].deck[i] != null && selectedDeck[PartyController.party.partyColors[deckID].ToString()].deck[i].GetCard().rarity != Card.Rarity.Starter && selectedDeck[PartyController.party.partyColors[deckID].ToString()].deck[i].GetCard().rarity != Card.Rarity.StarterAttack)     //Don't remove blank or starter cards
                            RemoveCard(selectedDeck[PartyController.party.partyColors[deckID].ToString()].deck[i], i, false);
                }
                else
                    for (int i = weaponCardSlots + accessoryCardSlots; i >= weaponCardSlots + accessoryCardSlots - cardSlotDiff; i--)
                        if (selectedDeck[PartyController.party.partyColors[deckID].ToString()].deck[i] != null && selectedDeck[PartyController.party.partyColors[deckID].ToString()].deck[i].GetCard().rarity != Card.Rarity.Starter && selectedDeck[PartyController.party.partyColors[deckID].ToString()].deck[i].GetCard().rarity != Card.Rarity.StarterAttack)     //Don't remove blank or starter cards
                            RemoveCard(selectedDeck[PartyController.party.partyColors[deckID].ToString()].deck[i], i, false);
            }
        }
        int swappedDeckID = PartyController.party.GetPartyIndex(color);
        if (swappedCardSlotDiff > 0)
            if (equip.isWeapon)
            {
                for (int i = swappedWeaponCardSlots; i >= swappedWeaponCardSlots - swappedCardSlotDiff; i--)
                    if (selectedDeck[color.ToString()].deck[i] != null && selectedDeck[color.ToString()].deck[i].GetCard().rarity != Card.Rarity.Starter && selectedDeck[color.ToString()].deck[i].GetCard().rarity != Card.Rarity.StarterAttack)     //Don't remove blank or starter cards
                        RemoveCard(selectedDeck[color.ToString()].deck[i], i, false);
            }
            else
                for (int i = swappedWeaponCardSlots + swappedAccessoryCardSlots; i >= swappedWeaponCardSlots + swappedAccessoryCardSlots - swappedCardSlotDiff; i--)
                    if (selectedDeck[color.ToString()].deck[i] != null && selectedDeck[color.ToString()].deck[i].GetCard().rarity != Card.Rarity.Starter && selectedDeck[color.ToString()].deck[i].GetCard().rarity != Card.Rarity.StarterAttack)     //Don't remove blank or starter cards
                        RemoveCard(selectedDeck[color.ToString()].deck[i], i, false);

        RefreshEquipments();
        RefreshSelectDecks();
        if (CheckDeckComplete() && SceneManager.GetActiveScene().name != "OverworldScene")
            InformationLogger.infoLogger.SaveStoryModeGame();
    }

    public void RemoveCard(CardController newCard, int index, bool refreshDecks = true)
    {
        int deckIndex = PartyController.party.GetPartyIndex(newCard.GetCard().casterColor);
        selectedDeck[newCard.GetCard().casterColor.ToString()].deck[index] = null;
        uniqueCards[newCard.GetCard().casterColor.ToString()][newCard.GetCard().name] += 1;

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
        if (refreshDecks)
        {
            if (StoryModeController.story.GetMenuState() != StoryModeController.MenuState.CardScreen && SceneManager.GetActiveScene().name != "OverworldScene")
                StoryModeController.story.GoToCardScene();
            else
                RefreshDecks();
        }
    }

    public void FinalizeDeck()
    {
        //Reset the equipment attachment for all cards before finalizing
        foreach (string color in selectedDeck.Keys)
        {
            int weaponCardSlots = 0;
            int accessoryCardSlots = 0;

            Equipment weapon = null;
            Equipment accessory = null;

            if (selectedEquipments.ContainsKey(color))
                foreach (Equipment e in selectedEquipments[color].equipments)
                    if (e.isWeapon)
                    {
                        weaponCardSlots = e.numOfCardSlots;
                        weapon = e;
                    }
                    else
                    {
                        accessory = e;
                        accessoryCardSlots = e.numOfCardSlots;
                    }

            for (int j = 0; j < 8; j++)
            {
                //If the card is put into an accessory or weapon slot, attached the weapon to the card
                if (j < weaponCardSlots)
                    selectedDeck[color].deck[j].SetAttachedEquipment(weapon);
                else if (j < weaponCardSlots + accessoryCardSlots)
                    selectedDeck[color].deck[j].SetAttachedEquipment(accessory);
                else
                {
                    try
                    {
                        selectedDeck[color].deck[j].SetAttachedEquipment(null);
                    }
                    catch
                    {
                        Debug.Log(color);
                    }
                }
            }
        }

        DeckController.deckController.SetDecks(selectedDeck);
    }

    public void SetupMultiplayerDeck()
    {
        /*
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
        */
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
        RefreshEquipments();
    }

    public void LogInformation()
    {
        if (RoomController.roomController.selectedLevel != -1)
        {
            List<string> selectedCardNames = new List<string>();
            foreach (string color in selectedDeck.Keys)
                foreach (CardController c in selectedDeck[color].deck)
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
            foreach (string color in selectedDeck.Keys)
            {
                foreach (CardController card in selectedDeck[color].deck)
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
            RefreshSelectDecks();
            RefreshEquipments();
        }
        foreach (DeckButtonController i in deckButtons)
            i.GetComponent<RectTransform>().localScale = new Vector2(1, 1);
        deckButtons[deckID].GetComponent<RectTransform>().localScale = new Vector2(1, 1.3f);

        for (int i = 0; i < deckButtons.Length; i++)
        {
            deckButtons[i].GetComponent<Outline>().enabled = newCards[PartyController.party.GetPlayerColorTexts()[i]].deck.Count != 0;
            deckButtons[i].GetComponent<Image>().color = PartyController.party.GetPlayerColor(PartyController.party.partyColors[i]);
        }

        CheckPageButtons();
    }

    public bool CheckDeckComplete()
    {
        int totalNumOfBlanks = 0;
        foreach (string color in PartyController.party.GetPotentialPlayerTexts())
            foreach (CardController card in selectedDeck[PartyController.party.GetPlayerColorTexts()[deckID]].deck)
                if (card == null)
                    totalNumOfBlanks++;

        if (totalNumOfBlanks > 0)
            StoryModeController.story.ShowMenuNotification(3, true, false);
        else if (totalNumOfBlanks == 0)
        {
            if (StoryModeController.story.GetIsDeckIncomplete() && SceneManager.GetActiveScene().name != "OverworldScene")        //If the deck wasn't full but now is, save the game
                InformationLogger.infoLogger.SaveStoryModeGame();
            StoryModeController.story.ShowMenuNotification(3, false, false);
        }

        return totalNumOfBlanks == 0;
    }

    public void AddRewardsCard(CardController newCard, bool isRewardsCard = true)
    {
        if (isRewardsCard)
            GameController.gameController.RecordRewardCards(newCard.GetCard());
        recentRewardsCard = newCard;
        int deckIndex = PartyController.party.GetPartyIndex(newCard.GetCard().casterColor);

        CardController c = this.gameObject.AddComponent<CardController>();
        c.SetCard(newCard.GetCard(), true, false);

        if (!newCards[newCard.GetCard().casterColor.ToString()].deck.Contains(c))
            newCards[newCard.GetCard().casterColor.ToString()].deck.Add(c);
        completeDeck[newCard.GetCard().casterColor.ToString()].deck.Add(c);
        ReCountUniqueCards();
        ResolveSelectedList();
        RefreshDecks();
    }

    public void RemoveCardFromNew(CardController newCard)
    {
        int deckIndex = PartyController.party.GetPartyIndex(newCard.GetCard().casterColor);
        if (newCards[newCard.GetCard().casterColor.ToString()].deck.Contains(newCard))
            newCards[newCard.GetCard().casterColor.ToString()].deck.Remove(newCard);

        for (int i = 0; i < deckButtons.Length; i++)
            if (newCards[PartyController.party.GetPlayerColorTexts()[i]].deck.Count == 0)
                deckButtons[i].GetComponent<Outline>().enabled = false;
            else
                deckButtons[i].GetComponent<Outline>().enabled = true;
    }

    public void ReCountUniqueCards()
    {
        uniqueCards = new Dictionary<string, Dictionary<string, int>>();

        foreach (string color in PartyController.party.GetPotentialPlayerTexts())
        {
            Dictionary<string, int> temp = new Dictionary<string, int>();

            completeDeck[color].deck = completeDeck[color].deck.OrderBy(o => o.GetCard().manaCost).ThenBy(o => o.GetCard().energyCost).ThenBy(o => o.GetCard().name).ToList();
            foreach (CardController card in completeDeck[color].deck)
            {
                if (card.GetCard().rarity == Card.Rarity.StarterAttack || card.GetCard().rarity == Card.Rarity.Starter)         //Add the starter cards first
                {
                    if (temp.ContainsKey(card.GetCard().name))
                        temp[card.GetCard().name] += 1;
                    else
                        temp[card.GetCard().name] = 1;
                }
            }
            foreach (CardController card in completeDeck[color].deck)
            {
                if (card.GetCard().rarity != Card.Rarity.StarterAttack && card.GetCard().rarity != Card.Rarity.Starter)        //Add the other cards later
                {
                    if (temp.ContainsKey(card.GetCard().name))
                        temp[card.GetCard().name] += 1;
                    else
                        temp[card.GetCard().name] = 1;
                }
            }
            uniqueCards[color] = temp;
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
        int counter = 0;
        foreach (CardController c in completeDeck[card.casterColor.ToString()].deck)
            if (c.GetCard().name == card.name)
                counter++;

        return counter;
    }

    public int GetCountOfEquipmentInCollection(Equipment equip)
    {
        int counter = 0;

        foreach (Equipment e in completeEquipments.equipments)
            if (e.equipmentName == equip.equipmentName)
                counter++;

        return counter;
    }

    public int GetCountOfCardInCollection(string cardName)
    {
        return GetCountOfCardInCollection(LootController.loot.GetCardWithName(cardName));
    }

    public string[] GetCompleteDeckNames()
    {
        List<string> output = new List<string>();
        foreach (string color in completeDeck.Keys)
            for (int j = 0; j < completeDeck[color].deck.Count; j++)
                output.Add(completeDeck[color].deck[j].GetCard().name);

        return output.ToArray();
    }

    public Dictionary<string, string[]> GetSelectedDeckNames()
    {
        Dictionary<string, string[]> output = new Dictionary<string, string[]>();
        foreach (string color in PartyController.party.GetPotentialPlayerTexts())
        {
            output[color] = new string[8];
            if (PartyController.party.GetPotentialPlayerTexts().Contains(color))
            {
                List<string> temp = new List<string>();
                int colorIndex = PartyController.party.GetPartyIndex(PartyController.party.GetPlayerCasterColor(color));
                for (int j = 0; j < selectedDeck[color].deck.Count; j++)
                    temp.Add(selectedDeck[color].deck[j].GetCard().name);
                output[color] = temp.ToArray();
            }
        }

        return output;
    }

    public string[] GetNewCardDeckNames()
    {
        List<string> output = new List<string>();
        foreach (string color in newCards.Keys)
            for (int j = 0; j < newCards[color].deck.Count; j++)
                output.Add(newCards[color].deck[j].GetCard().name);

        return output.ToArray();
    }

    public void SetCompleteDeck(string[] completeDeckNames)
    {
        foreach (string color in completeDeck.Keys)
        {
            completeDeck[color] = new ListWrapper();
            completeDeck[color].deck = new List<CardController>();
        }

        foreach (string name in completeDeckNames)
        {
            CardController cardController = this.gameObject.AddComponent<CardController>();
            cardController.SetCard(LootController.loot.GetCardWithName(name), true, false);
            completeDeck[cardController.GetCard().casterColor.ToString()].deck.Add(cardController);
        }
    }

    public void SetCompleteDeck(Dictionary<string, int> value, bool additive = false)
    {
        if (!additive)
            foreach (string color in PartyController.party.GetPotentialPlayerTexts())
            {
                completeDeck[color] = new ListWrapper();
                completeDeck[color].deck = new List<CardController>();
            }

        foreach (string name in value.Keys)
        {
            for (int i = 0; i < value[name]; i++)
            {
                CardController cardController = this.gameObject.AddComponent<CardController>();
                cardController.SetCard(LootController.loot.GetCardWithName(name), true, false);
                completeDeck[cardController.GetCard().casterColor.ToString()].deck.Add(cardController);
            }
        }
    }

    public void SetSelectedDeck(Dictionary<string, string[]> selectedDeckNames)
    {
        foreach (string color in PartyController.party.GetPotentialPlayerTexts())
        {
            selectedDeck[color] = new ListWrapper();
            selectedDeck[color].deck = new List<CardController>();
        }

        foreach (string color in PartyController.party.GetPotentialPlayerTexts())
            foreach (string cardName in selectedDeckNames[color])
            {
                CardController cardController = this.gameObject.AddComponent<CardController>();
                //CardController cardController = new CardController();
                cardController.SetCard(LootController.loot.GetCardWithName(cardName), true, false);
                selectedDeck[cardController.GetCard().casterColor.ToString()].deck.Add(cardController);
            }
    }

    public void SetCompleteEquipments(Dictionary<string, int> completeEquipmentNames, bool additive = false)
    {
        if (!additive)
        {
            completeEquipments = new EquipmentWrapper();
            completeEquipments.equipments = new List<Equipment>();
        }
        foreach (string newEquipment in completeEquipmentNames.Keys)
            for (int i = 0; i < completeEquipmentNames[newEquipment]; i++)
                completeEquipments.equipments.Add(LootController.loot.GetEquipment(newEquipment));
        ReCountUniqueEquipments();
    }

    public void SetNewCardsDeck(string[] newCardDeckNames)
    {
        foreach (string color in selectedDeck.Keys)
        {
            newCards[color] = new ListWrapper();
            newCards[color].deck = new List<CardController>();
        }

        foreach (string name in newCardDeckNames)
        {
            CardController cardController = this.gameObject.AddComponent<CardController>();
            cardController.SetCard(LootController.loot.GetCardWithName(name), true, false);
            newCards[cardController.GetCard().casterColor.ToString()].deck.Add(cardController);
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

    public void SetInCombatDecks()
    {
        isShowingCards = true;

        SetCompleteDeck(GetSelectedDeckDict());     //Create a new list of complete deck with the card names from select deck
        foreach (string color in selectedDeck.Keys)
            foreach (CardController c in selectedDeck[color].deck)
                c.SetStartedInDeck(true);

        ReCountUniqueCards();
        ResolveSelectedList();
        RefreshDecks();
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
        foreach (string color in PartyController.party.GetPlayerColorTexts())
            foreach (CardController c in selectedDeck[color].deck)
                if (!c.GetStartedInDeck())
                    output++;
        return output;
    }

    public void SetSelectAreaWhiteOut(bool state)
    {
        selectedAreaWhiteOut.enabled = state;
        selectedAreaWhiteOut.transform.SetAsLastSibling();
    }

    public void SetSelectCardWhiteOut(bool state, int index)
    {
        if (!state)
        {
            foreach (SelectedCardController card in selectedCardsDisplay)
                card.SetWhiteOut(false);
            return;
        }

        //Obtain the range of indexes the new card can be put into
        Vector2 ranges = GetSelectCardCategoryIndexRanges(index);
        int startingIndex = (int)ranges.x;
        int range = (int)ranges.y;

        for (int i = startingIndex; i < startingIndex + range; i++)
            selectedCardsDisplay[i].SetWhiteOut(state);
    }

    //Takes in the select card index and returns the startingIndex and range of select cards that belong in the same weapon/accessory/basic categories
    private Vector2 GetSelectCardCategoryIndexRanges(int index)
    {
        Vector2 cardSlots = GetWeaponAndAccessorySlots();
        int weaponCardSlots = (int)cardSlots.x;
        int accessoryCardSlots = (int)cardSlots.y;

        int startingIndex = 0;
        int range = weaponCardSlots;
        if (index >= Mathf.Max(new int[] { weaponCardSlots, accessoryCardSlots, weaponCardSlots + accessoryCardSlots }))
        {
            startingIndex = Mathf.Max(new int[] { weaponCardSlots, accessoryCardSlots, weaponCardSlots + accessoryCardSlots });
            range = 8 - startingIndex;
        }
        else if (Mathf.Min(new int[] { weaponCardSlots, accessoryCardSlots }) == 0)
        {
            if (index >= Mathf.Max(new int[] { weaponCardSlots, accessoryCardSlots }))
            {
                startingIndex = Mathf.Max(new int[] { weaponCardSlots, accessoryCardSlots });
                range = 8 - startingIndex;
            }
            else
            {
                startingIndex = 0;
                range = Mathf.Max(new int[] { weaponCardSlots, accessoryCardSlots });
            }
        }
        else
        {
            if (index >= weaponCardSlots)
            {
                startingIndex = weaponCardSlots;
                range = accessoryCardSlots;
            }
            if (index >= weaponCardSlots + accessoryCardSlots)
            {
                startingIndex = weaponCardSlots + accessoryCardSlots;
                range = 8 - weaponCardSlots - accessoryCardSlots;
            }
        }

        return new Vector2(startingIndex, range);
    }

    //Get the number of weapon and accessory card slots
    public Vector2 GetWeaponAndAccessorySlots()
    {
        int weaponCardSlots = 0;
        int accessoryCardSlots = 0;
        if (selectedEquipments.ContainsKey(PartyController.party.GetPlayerColorTexts()[deckID]))
            foreach (Equipment e in selectedEquipments[PartyController.party.GetPlayerColorTexts()[deckID]].equipments)
                if (e.isWeapon)
                    weaponCardSlots = e.numOfCardSlots;
                else
                    accessoryCardSlots = e.numOfCardSlots;

        return new Vector2(weaponCardSlots, accessoryCardSlots);
    }

    public string[] GetCompleteEquipments()
    {
        List<string> output = new List<string>();

        foreach (Equipment e in completeEquipments.equipments)
            output.Add(e.equipmentName);

        return output.ToArray();
    }

    public Dictionary<string, string[]> GetSelectEquipments()
    {
        Dictionary<string, string[]> output = new Dictionary<string, string[]>();

        foreach (string key in selectedEquipments.Keys)
        {
            List<string> temp = new List<string>();
            foreach (Equipment e in selectedEquipments[key].equipments)
                temp.Add(e.equipmentName);
            output[key] = temp.ToArray();
        }

        return output;
    }

    public List<Equipment> GetEquipmentList(Card.CasterColor color)
    {
        return selectedEquipments[color.ToString()].equipments;
    }

    public void SetCompleteEquipments(string[] value)
    {
        if (value == null)
            return;
        List<Equipment> equipments = new List<Equipment>();
        foreach (string e in value)
            equipments.Add(LootController.loot.GetEquipment(e));

        completeEquipments = new EquipmentWrapper();
        completeEquipments.equipments = equipments;
    }

    public void SetSelectedEquipments(Dictionary<string, string[]> value)
    {
        if (value == null)
            return;
        selectedEquipments = new Dictionary<string, EquipmentWrapper>();
        foreach (string color in PartyController.party.GetPotentialPlayerTexts())
        {
            List<Equipment> equipments = new List<Equipment>();
            if (value.ContainsKey(color))
                foreach (string e in value[color])
                    equipments.Add(LootController.loot.GetEquipment(e));
            selectedEquipments[color] = new EquipmentWrapper();
            selectedEquipments[color].equipments = equipments;
        }
    }

    ////////////////////////////////////////////
    /////////// Used for story mode ////////////
    ////////////////////////////////////////////
    public Dictionary<string, int> GetSelectedDeckDict()
    {
        Dictionary<string, int> output = new Dictionary<string, int>();
        foreach (string color in selectedDeck.Keys)
            foreach (CardController card in selectedDeck[color].deck)
                if (output.ContainsKey(card.GetCard().name))
                    output[card.GetCard().name] += 1;
                else
                    output[card.GetCard().name] = 1;

        return output;
    }

    public Dictionary<string, int> GetCompleteDeckDict()
    {
        Dictionary<string, int> output = new Dictionary<string, int>();
        foreach (string color in completeDeck.Keys)
            foreach (CardController card in completeDeck[color].deck)
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
