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
    public FinalizeButtonController cardButton;
    public FinalizeButtonController gearButton;
    [SerializeField]
    public EditorCardsWrapper[] editorDeck;
    public EditorCardsWrapper[] storyModeDeck;
    public EditorCardsWrapper[] debugDeck;
    public EditorCardsWrapper[] multiplayerDeck;
    public Text[] statTexts;
    public Text[] statChangeTexts;
    public Color plusColor;
    public Color minusColor;
    private Dictionary<string, ListWrapper> completeDeck = new Dictionary<string, ListWrapper>();
    private Dictionary<string, ListWrapper> selectedDeck = new Dictionary<string, ListWrapper>();
    private Dictionary<string, ListWrapper> newCards = new Dictionary<string, ListWrapper>();
    private EquipmentWrapper newEquipments = new EquipmentWrapper();
    private EquipmentWrapper completeEquipments;
    private List<string> debugEquipments = new List<string>() { "Echo Blade", "Throwing Knife" };
    private List<string> debugCards = new List<string>();
    private Dictionary<string, EquipmentWrapper> selectedEquipments = new Dictionary<string, EquipmentWrapper>();
    private CardController recentRewardsCard;
    private Equipment recentRewardsEquipment;

    private Dictionary<string, Dictionary<string, int>> uniqueCards;
    private Dictionary<string, int> uniqueEquipments;
    private int deckID = 0;
    private int page = 0;
    private List<Card.CasterColor> allColorsOrder = new List<Card.CasterColor>();

    public Image weaponImage;
    public EquipmentDragCardController weaponDragCard;
    public Image accessoryImage;
    public EquipmentDragCardController accessoryDragCard;
    public Image lockedImage;
    public Image errorMessage;
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
            newEquipments = new EquipmentWrapper();
            selectedDeck[color] = new ListWrapper();
        }

        ResetAllColorsOrder();

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
            SetStoryModeDefaultDeckAsSelectedDeck();
        }
        else
        {
            completeEquipments = new EquipmentWrapper();
            completeEquipments.equipments = new List<Equipment>();
            selectedEquipments = new Dictionary<string, EquipmentWrapper>();

            for (int i = 0; i < usedDeck.Length; i++)
            {
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

                selectedEquipments[color] = new EquipmentWrapper();
                selectedEquipments[color].equipments = new List<Equipment>();
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
            ReCountUniqueEquipments();
            SetDeck(0);
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
            if (i + custCardsDisplay.Length * page < uniqueCards[PartyController.party.GetPlayerColorText(allColorsOrder[deckID])].Keys.Count)
            {
                custCardsDisplay[i].Show();
                string name = uniqueCards[PartyController.party.GetPlayerColorText(allColorsOrder[deckID])].Keys.ToArray<string>()[i + custCardsDisplay.Length * page];
                custCardsDisplay[i].SetCard(GetCardWithName(name));
                custCardsDisplay[i].SetIsShowingCard(true);
                custCardsDisplay[i].SetCount(uniqueCards[PartyController.party.GetPlayerColorText(allColorsOrder[deckID])][name]);
                if (uniqueCards[PartyController.party.GetPlayerColorText(allColorsOrder[deckID])][name] == 0)
                {
                    custCardsDisplay[i].GetComponent<Collider2D>().enabled = false;
                    custCardsDisplay[i].greyOut.enabled = true;
                }
                else
                {
                    custCardsDisplay[i].GetComponent<Collider2D>().enabled = true;
                    custCardsDisplay[i].greyOut.enabled = false;
                }
                custCardsDisplay[i].highlight.enabled = newCards[PartyController.party.GetPlayerColorText(allColorsOrder[deckID])].deck.Contains(GetCardWithName(name));
            }
            else
                custCardsDisplay[i].Hide();
        }

        RefreshSelectDecks();
        RefreshSelectedEquipments();
        CheckPageButtons();
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
            if (selectedDeck[PartyController.party.GetPlayerColorText(allColorsOrder[deckID])].deck[i] == null)
                numOfBlanks++;
            else
                weaponCards.Add(selectedDeck[PartyController.party.GetPlayerColorText(allColorsOrder[deckID])].deck[i]);
        weaponCards = weaponCards.OrderBy(o => o.GetCard().manaCost).ThenBy(o => o.GetCard().energyCost).ThenBy(o => o.GetCard().name).ToList();
        for (int i = 0; i < numOfBlanks; i++)
            weaponCards.Add(null);
        numOfBlanks = 0;

        //Reorder all the accessory slot cards
        for (int i = weaponCardSlots; i < weaponCardSlots + accessoryCardSlots; i++)
            if (selectedDeck[PartyController.party.GetPlayerColorText(allColorsOrder[deckID])].deck[i] == null)
                numOfBlanks++;
            else
                accessoryCards.Add(selectedDeck[PartyController.party.GetPlayerColorText(allColorsOrder[deckID])].deck[i]);
        accessoryCards = accessoryCards.OrderBy(o => o.GetCard().manaCost).ThenBy(o => o.GetCard().energyCost).ThenBy(o => o.GetCard().name).ToList();
        for (int i = 0; i < numOfBlanks; i++)
            accessoryCards.Add(null);
        numOfBlanks = 0;

        //Reorder all the basic slot cards
        for (int i = weaponCardSlots + accessoryCardSlots; i < 8; i++)
            if (selectedDeck[PartyController.party.GetPlayerColorText(allColorsOrder[deckID])].deck[i] == null)
                numOfBlanks++;
            else
                basicCards.Add(selectedDeck[PartyController.party.GetPlayerColorText(allColorsOrder[deckID])].deck[i]);
        basicCards = basicCards.OrderBy(o => o.GetCard().manaCost).ThenBy(o => o.GetCard().energyCost).ThenBy(o => o.GetCard().name).ToList();
        for (int i = 0; i < numOfBlanks; i++)
            basicCards.Add(null);
        numOfBlanks = 0;

        //Remake the selected deck
        selectedDeck[PartyController.party.GetPlayerColorText(allColorsOrder[deckID])].deck = new List<CardController>();
        selectedDeck[PartyController.party.GetPlayerColorText(allColorsOrder[deckID])].deck.AddRange(weaponCards);
        selectedDeck[PartyController.party.GetPlayerColorText(allColorsOrder[deckID])].deck.AddRange(accessoryCards);
        selectedDeck[PartyController.party.GetPlayerColorText(allColorsOrder[deckID])].deck.AddRange(basicCards);

        for (int j = 0; j < 8; j++)
        {
            if (selectedDeck[PartyController.party.GetPlayerColorText(allColorsOrder[deckID])].deck[j] != null)
            {
                selectedCardsDisplay[j].SetCard(selectedDeck[PartyController.party.GetPlayerColorText(allColorsOrder[deckID])].deck[j]);
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
        ReCountUniqueEquipments();      //Refresh unique equipments just in case

        //Order the equiped equipments by colors, with the selected player color equipments always being first
        List<Equipment> equipedEquipment = new List<Equipment>();
        List<string> equipedColor = new List<string>();
        if (selectedEquipments.ContainsKey(PartyController.party.GetPlayerColorText(allColorsOrder[deckID])))        //Add the equipped equipments from party colors so they show up first
            foreach (Equipment e in selectedEquipments[PartyController.party.GetPlayerColorText(allColorsOrder[deckID])].equipments.OrderByDescending(x => x.isWeapon))
            {
                equipedEquipment.Add(e);
                equipedColor.Add(PartyController.party.GetPlayerColorText(allColorsOrder[deckID]));
            }
        foreach (string color in PartyController.party.GetPotentialPlayerTexts())                       //Add the equipped equipments from non party colors so they show up after the party color equipments
            if (selectedEquipments.ContainsKey(color) && color != PartyController.party.GetPlayerColorText(allColorsOrder[deckID]))
                foreach (Equipment e in selectedEquipments[color].equipments.OrderByDescending(x => x.isWeapon))
                {
                    equipedEquipment.Add(e);
                    equipedColor.Add(color);
                }

        //<equipmentName, count>
        Dictionary<string, int> resolvedUniqueEquipments = new Dictionary<string, int>();               //Using the resolved unique equipment list that removes equipments that have all be equipped by players
        foreach (string key in uniqueEquipments.Keys)
            if (uniqueEquipments[key] > 0)
                resolvedUniqueEquipments[key] = uniqueEquipments[key];
        foreach (string key in uniqueEquipments.Keys)
            if (uniqueEquipments[key] <= 0)
                resolvedUniqueEquipments[key] = 0;

        //Draws all the cards in the select area
        for (int i = 0; i < custCardsDisplay.Length; i++)
        {
            custCardsDisplay[i].highlight.enabled = false;
            //Drawing the equiped equipments
            if (page == 0 || page == 1 && equipedEquipment.Count > 6)
            {
                if (resolvedUniqueEquipments.Keys.Count == 0)
                    ShowErrorMessage("No available gear");
                else if (equipedEquipment.Count == 0)
                    ShowErrorMessage("No equipped gear");

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
                else
                    custCardsDisplay[i].Hide();
            }
            else
            {
                //Drawing the unequiped equipments
                if (i + custCardsDisplay.Length * (page - 1) < resolvedUniqueEquipments.Keys.Count || equipedEquipment.Count > 6 && i + custCardsDisplay.Length * (page - 2) < resolvedUniqueEquipments.Keys.Count)
                {
                    custCardsDisplay[i].Show();
                    string name = "error";
                    if (equipedEquipment.Count <= 6)
                        name = resolvedUniqueEquipments.Keys.ToArray<string>()[i + custCardsDisplay.Length * (page - 1)];
                    else
                        name = resolvedUniqueEquipments.Keys.ToArray<string>()[i + custCardsDisplay.Length * (page - 2)];

                    Equipment equip = LootController.loot.GetEquipment(name);
                    custCardsDisplay[i].GetComponentInChildren<CardDisplay>().SetEquipment(equip, Card.CasterColor.Passive);
                    custCardsDisplay[i].SetIsShowingCard(false);
                    custCardsDisplay[i].SetEquipment(equip, Card.CasterColor.Passive);
                    custCardsDisplay[i].SetCount(resolvedUniqueEquipments[name]);
                    if (resolvedUniqueEquipments[name] <= 0)
                    {
                        custCardsDisplay[i].GetComponent<Collider2D>().enabled = false;
                        custCardsDisplay[i].greyOut.enabled = true;
                    }
                    else
                    {
                        custCardsDisplay[i].GetComponent<Collider2D>().enabled = true;
                        custCardsDisplay[i].greyOut.enabled = false;
                    }
                    custCardsDisplay[i].highlight.enabled = newEquipments.equipments.Contains(equip);
                }
                else
                    custCardsDisplay[i].Hide();
            }
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
        int accessoryCardSlots = 0;
        if (selectedEquipments.ContainsKey(PartyController.party.GetPlayerColorText(allColorsOrder[deckID])))
        {
            foreach (Equipment equip in selectedEquipments[PartyController.party.GetPlayerColorText(allColorsOrder[deckID])].equipments)
                if (equip.isWeapon)
                    weaponCardSlots = equip.numOfCardSlots;
                else
                    accessoryCardSlots = equip.numOfCardSlots;

            foreach (Equipment equip in selectedEquipments[PartyController.party.GetPlayerColorText(allColorsOrder[deckID])].equipments)
            {
                if (equip.isWeapon)
                {
                    weaponImage.gameObject.SetActive(true);
                    weaponImage.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Image>().sprite = equip.art;
                    weaponImage.rectTransform.sizeDelta = new Vector2(equip.numOfCardSlots * 0.6f, weaponImage.rectTransform.sizeDelta.y);
                    weaponDragCard.SetEquipment(equip, PartyController.party.GetPlayerCasterColor(PartyController.party.GetPlayerColorText(allColorsOrder[deckID])));
                }
                else
                {
                    accessoryImage.gameObject.SetActive(true);
                    accessoryImage.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Image>().sprite = equip.art;
                    accessoryImage.rectTransform.sizeDelta = new Vector2(equip.numOfCardSlots * 0.6f, accessoryImage.rectTransform.sizeDelta.y);
                    accessoryImage.transform.localPosition = weaponImage.transform.localPosition + new Vector3(weaponCardSlots * 0.6f, 0);
                    accessoryDragCard.SetEquipment(equip, PartyController.party.GetPlayerCasterColor(PartyController.party.GetPlayerColorText(allColorsOrder[deckID])));
                }
            }
        }
        if (SceneManager.GetActiveScene().name == "StoryModeScene")
        {
            lockedImage.gameObject.SetActive(true);
            lockedImage.rectTransform.sizeDelta = new Vector2((8 - weaponCardSlots - accessoryCardSlots) * 0.6f, lockedImage.rectTransform.sizeDelta.y);
            lockedImage.transform.localPosition = new Vector3(weaponImage.transform.localPosition.x + (weaponCardSlots + accessoryCardSlots) * 0.6f, weaponImage.transform.localPosition.y, 0);
        }
        else
            lockedImage.gameObject.SetActive(false);
    }

    public void NextPage()
    {
        SetPage(page + 1);
    }

    public void PreviousPage()
    {
        SetPage(page - 1);
    }

    public void SetPage(int p)
    {
        HideErrorMessage();
        page = p;

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
            if ((custCardsDisplay.Length * (page + 1) < uniqueCards[PartyController.party.GetPlayerColorText(allColorsOrder[deckID])].Keys.Count && isShowingCards))
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
            int numOfUnselectedEquipments = 0;
            foreach (string key in uniqueEquipments.Keys)
                //if (uniqueEquipments[key] > 0)
                numOfUnselectedEquipments++;

            int selectedEquipmentPages = 1;
            if (numOfSelectedEquipments > 6)
                selectedEquipmentPages = 0;

            if (custCardsDisplay.Length * (page + selectedEquipmentPages) < numOfUnselectedEquipments + custCardsDisplay.Length)
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
        if (!selectedDeck[newCard.GetCard().casterColor.ToString()].deck.GetRange(startingIndex, range).Any(x => x == null))
            RemoveCard(selectedDeck[newCard.GetCard().casterColor.ToString()].deck[index], index, false);

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
                string equipmentName = "None";
                if (newCard.GetAttachedEquipment() != null)
                    equipmentName = newCard.GetAttachedEquipment().equipmentName;
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
                                "False",
                                equipmentName);
            }
            catch { }
        }

        CheckDeckComplete();
        RefreshDecks();
    }

    public void SwapCards(int index1, int index2)
    {
        //Save the card controllers to be swapped
        CardController card1 = selectedDeck[PartyController.party.GetPlayerColorText(allColorsOrder[deckID])].deck[index1];
        CardController card2 = selectedDeck[PartyController.party.GetPlayerColorText(allColorsOrder[deckID])].deck[index2];

        //Remove both cards
        if (card2 != null)
            RemoveCard(card2, index2, false);
        if (GetIfViableSelectSlot(card2.GetCard(), index1) && GetIfSelectedSlotHasEmpty(index1))         //If swap is not needed and card can just be added to the new area, do so
            AddCard(card2, index1);
        else                                                        //Else swap the two cards
        {
            if (card1 != null)
                RemoveCard(card1, index1, false);

            //Add in both cards if they're going into a viable slot
            if (card1 != null && GetIfViableSelectSlot(card1.GetCard(), index2))
                AddCard(card1, index2);
            if (card2 != null && GetIfViableSelectSlot(card2.GetCard(), index1))
                AddCard(card2, index1);
        }
        SetIsShowingCards(true);
        SetPage(0);
    }

    public bool GetIfViableSelectSlot(Card c, int index)
    {
        Vector2 range = CollectionController.collectionController.GetWeaponAndAccessorySlots();
        int slots = (int)(range.x + range.y);
        if (SceneManager.GetActiveScene().name == "StoryModeScene")
            return ((new List<Card.Rarity> { Card.Rarity.StarterAttack, Card.Rarity.StarterDefence, Card.Rarity.StarterSpecial }.Contains(c.rarity)) || index < slots);  //Only allow cards to be selected in their respective slots in the story mode scene
        else
            return true;
    }

    public bool GetIfSelectedSlotHasEmpty(int index)
    {
        Vector2 ranges = GetSelectCardCategoryIndexRanges(index);
        int startingIndex = (int)ranges.x;
        int range = (int)ranges.y;

        for (int i = startingIndex; i < startingIndex + range; i++)
            if (selectedDeck[PartyController.party.GetPlayerColorText(allColorsOrder[deckID])].deck[i] == null)
                return true;
        return false;
    }

    public void AddEquipment(Equipment newEquipment, Card.CasterColor color)
    {
        Equipment currentlyEquipped = null;
        int weaponCardSlots = 0;
        int accessoryCardSlots = 0;
        int swappedWeaponCardSlots = 0;
        int swappedAccessoryCardSlots = 0;

        //Chekcs if there is already a weapon/accessory that's equiped
        if (selectedEquipments.ContainsKey(PartyController.party.GetPlayerColorText(allColorsOrder[deckID])))
            foreach (Equipment e in selectedEquipments[PartyController.party.GetPlayerColorText(allColorsOrder[deckID])].equipments)
            {
                if (e.isWeapon == newEquipment.isWeapon)
                    currentlyEquipped = e;
                if (e.isWeapon)
                    weaponCardSlots = e.numOfCardSlots;
                else
                    accessoryCardSlots = e.numOfCardSlots;
            }
        else
            selectedEquipments[PartyController.party.GetPlayerColorText(allColorsOrder[deckID])] = new EquipmentWrapper();

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
            if (currentlyEquipped != null)                                  //If a wapon/accessory is already equiped, remove that before adding the new one
            {
                selectedEquipments[PartyController.party.GetPlayerColorText(allColorsOrder[deckID])].equipments.Remove(currentlyEquipped);
                cardSlotDiff = currentlyEquipped.numOfCardSlots - newEquipment.numOfCardSlots;
            }
            selectedEquipments[PartyController.party.GetPlayerColorText(allColorsOrder[deckID])].equipments.Add(newEquipment);
        }
        else
        {
            if (color == allColorsOrder[deckID])         //If the newly added equipment was already equipped, unequip it
            {
                selectedEquipments[PartyController.party.GetPlayerColorText(allColorsOrder[deckID])].equipments.Remove(currentlyEquipped);
                cardSlotDiff = currentlyEquipped.numOfCardSlots;
            }
            else                                                            //If the newly added equipment is from a diffrent color, swap it with the current selected one
            {
                Equipment swap = currentlyEquipped;
                selectedEquipments[PartyController.party.GetPlayerColorText(allColorsOrder[deckID])].equipments.Remove(currentlyEquipped);
                selectedEquipments[PartyController.party.GetPlayerColorText(allColorsOrder[deckID])].equipments.Add(newEquipment);
                selectedEquipments[color.ToString()].equipments.Remove(newEquipment);
                swappedCardSlotDiff = newEquipment.numOfCardSlots;
                if (currentlyEquipped != null)                              //The selected color only get's the old current color equipment if current color actually have a weapon/accessory in that slot
                {
                    selectedEquipments[color.ToString()].equipments.Add(swap);
                    swappedCardSlotDiff -= swap.numOfCardSlots;
                    cardSlotDiff = currentlyEquipped.numOfCardSlots - newEquipment.numOfCardSlots;
                }
            }
        }

        //When removing cards, remove starter cards first
        //When adding/removing/swapping weapons, accessory cards should stay attached to accessories

        //Handling current selected color's equipments
        if (currentlyEquipped == null)
            cardSlotDiff = -newEquipment.numOfCardSlots;
        if (cardSlotDiff != 0)
        {
            if (newEquipment.isWeapon)
            {
                //Reorder the cards so null cards are removed first, then starter cards, then bought cards. Convenience feature
                ReorderSelectedCardsByRarity(PartyController.party.GetPlayerColorText(allColorsOrder[deckID]), 0, weaponCardSlots);

                if (accessoryCardSlots > 0)
                    for (int i = 0; i < accessoryCardSlots; i++)
                    {
                        int slot1 = weaponCardSlots + accessoryCardSlots - cardSlotDiff - i - 1;
                        int slot2 = weaponCardSlots - i;

                        //Move all the equipment cards to their new location to avoid being removed
                        CardController swap = selectedDeck[PartyController.party.GetPlayerColorText(allColorsOrder[deckID])].deck[slot1];
                        selectedDeck[PartyController.party.GetPlayerColorText(allColorsOrder[deckID])].deck[slot1] = selectedDeck[PartyController.party.GetPlayerColorText(allColorsOrder[deckID])].deck[slot2];
                        selectedDeck[PartyController.party.GetPlayerColorText(allColorsOrder[deckID])].deck[slot2] = swap;
                    }
            }
            else
                //Reorder the cards so null cards are removed first, then starter cards, then bought cards. Convenience feature
                ReorderSelectedCardsByRarity(PartyController.party.GetPlayerColorText(allColorsOrder[deckID]), weaponCardSlots, accessoryCardSlots);
            //Remove the swapped out old weapon cards or let over accessory cards, which ever the current situation calls for
            for (int i = weaponCardSlots + accessoryCardSlots; i >= weaponCardSlots + accessoryCardSlots - cardSlotDiff; i--)
                if (selectedDeck[PartyController.party.GetPlayerColorText(allColorsOrder[deckID])].deck[i] != null && !new List<Card.Rarity> { Card.Rarity.StarterAttack, Card.Rarity.StarterDefence, Card.Rarity.StarterSpecial }.Contains(selectedDeck[PartyController.party.GetPlayerColorText(allColorsOrder[deckID])].deck[i].GetCard().rarity))     //Don't remove blank or starter cards
                    RemoveCard(selectedDeck[PartyController.party.GetPlayerColorText(allColorsOrder[deckID])].deck[i], i, false);
        }

        //Handling the equipments of the other color if swapped equipments with that color
        if (swappedCardSlotDiff != 0)
        {
            if (newEquipment.isWeapon)
            {
                //Reorder the cards so null cards are removed first, then starter cards, then bought cards. Convenience feature
                ReorderSelectedCardsByRarity(PartyController.party.GetPlayerColorText(allColorsOrder[deckID]), 0, swappedWeaponCardSlots);

                if (swappedAccessoryCardSlots > 0)
                    for (int i = 0; i < swappedAccessoryCardSlots; i++)
                    {
                        int slot1 = swappedWeaponCardSlots + swappedAccessoryCardSlots - swappedCardSlotDiff - i - 1;
                        int slot2 = swappedWeaponCardSlots - i;

                        //Move all the equipment cards to their new location to avoid being removed
                        CardController swap = selectedDeck[color.ToString()].deck[slot1];
                        selectedDeck[color.ToString()].deck[slot1] = selectedDeck[color.ToString()].deck[slot2];
                        selectedDeck[color.ToString()].deck[slot2] = swap;
                    }
            }
            else
                //Reorder the cards so null cards are removed first, then starter cards, then bought cards. Convenience feature
                ReorderSelectedCardsByRarity(PartyController.party.GetPlayerColorText(allColorsOrder[deckID]), swappedWeaponCardSlots, swappedAccessoryCardSlots);
            for (int i = swappedWeaponCardSlots + swappedAccessoryCardSlots; i >= swappedWeaponCardSlots + swappedAccessoryCardSlots - swappedCardSlotDiff; i--)
                if (selectedDeck[color.ToString()].deck[i] != null && !new List<Card.Rarity> { Card.Rarity.StarterAttack, Card.Rarity.StarterDefence, Card.Rarity.StarterSpecial }.Contains(selectedDeck[color.ToString()].deck[i].GetCard().rarity))     //Don't remove blank or starter cards
                    RemoveCard(selectedDeck[color.ToString()].deck[i], i, false);
        }

        ReCountUniqueEquipments();
        RefreshEquipments();
        RefreshSelectDecks();
        ResetStatsTexts();
        if (CheckDeckComplete() && SceneManager.GetActiveScene().name != "OverworldScene")
            InformationLogger.infoLogger.SaveStoryModeGame();
    }

    private void ReorderSelectedCardsByRarity(string color, int startIndex, int range)
    {
        List<CardController> starterCards = new List<CardController>();
        List<CardController> otherCards = new List<CardController>();
        List<CardController> nullCards = new List<CardController>();
        foreach (CardController c in selectedDeck[color].deck.GetRange(startIndex, range))
            if (c != null)
            {
                if (new List<Card.Rarity> { Card.Rarity.StarterAttack, Card.Rarity.StarterDefence, Card.Rarity.StarterSpecial }.Contains(c.GetCard().rarity))
                    starterCards.Add(c);
                else
                    otherCards.Add(c);
            }
            else
                nullCards.Add(c);

        List<CardController> finalCards = otherCards;
        finalCards.AddRange(starterCards);
        finalCards.AddRange(nullCards);
        for (int i = startIndex; i < startIndex + range; i++)
            selectedDeck[color].deck[i] = finalCards[i - startIndex];
    }

    public void RemoveCard(CardController newCard, int index, bool refreshDecks = true)
    {
        selectedDeck[newCard.GetCard().casterColor.ToString()].deck[index] = null;
        uniqueCards[newCard.GetCard().casterColor.ToString()][newCard.GetCard().name] += 1;

        try
        {
            string equipmentName = "None";
            if (newCard.GetAttachedEquipment() != null)
                equipmentName = newCard.GetAttachedEquipment().equipmentName;
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
                            "False",
                            equipmentName);
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
        FinalizeDeck();
        CheckDeckComplete();
        CheckPageButtons();
        RefreshDecks();
    }

    public void SetupStoryModeDeck()
    {
        ReCountUniqueCards();
        ReCountUniqueEquipments();
        SetDeck(0);
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
                {
                    string equipmentName = "None";
                    if (card.GetAttachedEquipment() != null)
                        equipmentName = card.GetAttachedEquipment().equipmentName;
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
                            "True",
                            equipmentName);
                }
            }

            //If equipments were recently selected
            if (recentRewardsEquipment != null)
            {
                List<string> selectedEquipNames = new List<string>();
                foreach (string color in selectedEquipments.Keys)
                    foreach (Equipment e in selectedEquipments[color].equipments)
                        selectedEquipNames.Add(e.equipmentName);
                if (selectedEquipNames.Contains(recentRewardsEquipment.equipmentName))
                {
                    InformationLogger.infoLogger.SaveRewardsCardInfo(InformationLogger.infoLogger.patchID,
                                InformationLogger.infoLogger.gameID,
                                RoomController.roomController.worldLevel.ToString(),
                                RoomController.roomController.selectedLevel.ToString(),
                                RoomController.roomController.roomName,
                                "Equipment",
                                recentRewardsEquipment.equipmentName,
                                "0",
                                "0",
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
                                "Equipment",
                                recentRewardsEquipment.equipmentName,
                                "0",
                                "0",
                                "True",
                                "False");
                }

                recentRewardsEquipment = null; //Reset recent to null since not every room rewards equipments
            }
        }
    }

    public void ResetDeck()
    {
        SetDeck(deckID);
    }

    public void SetDeck(int newDeck)
    {
        HideErrorMessage();
        if (isShowingCards)
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

        List<Card.CasterColor> otherColors = new List<Card.CasterColor>();
        foreach (Card.CasterColor color in PartyController.party.unlockedPlayerColors)
            if (!PartyController.party.partyColors.Contains(color))
                otherColors.Add(color);

        ResetAllColorsOrder();

        for (int i = 0; i < 3; i++)
        {
            deckButtons[i].GetComponent<Image>().color = PartyController.party.GetPlayerColor(PartyController.party.partyColors[i]);

            if (isShowingCards)
                deckButtons[i].GetComponent<Outline>().enabled = newCards[PartyController.party.GetPlayerColorTexts()[i]].deck.Count != 0;
            else
                gearButton.GetComponent<Outline>().enabled = newEquipments.equipments.Count != 0;
        }

        if (SceneManager.GetActiveScene().name == "StoryModeScene" || SceneManager.GetActiveScene().name == "TavernScene")         //In story mode planning, show other unlocked characters' tabs
        {
            for (int i = 3; i < 3 + otherColors.Count; i++)
            {
                deckButtons[i].gameObject.SetActive(true);
                deckButtons[i].GetComponent<Image>().color = PartyController.party.GetPlayerColor(otherColors[i - 3]);

                if (isShowingCards)
                    deckButtons[i].GetComponent<Outline>().enabled = newCards[PartyController.party.GetPlayerColorText(otherColors[i - 3])].deck.Count != 0;
                else
                    gearButton.GetComponent<Outline>().enabled = newEquipments.equipments.Count != 0;
            }

            for (int i = 3 + otherColors.Count; i < 6; i++)
                deckButtons[i].gameObject.SetActive(false);
        }
        else                                                                //In all other instances, classic mode, in combat, etc, only show party character tabs
            for (int i = 3; i < 6; i++)
                deckButtons[i].gameObject.SetActive(false);

        CheckPageButtons();
        ResetStatsTexts();
    }

    public void ResetAllColorsOrder()
    {
        List<Card.CasterColor> otherColors = new List<Card.CasterColor>();
        foreach (Card.CasterColor color in PartyController.party.unlockedPlayerColors)
            if (!PartyController.party.partyColors.Contains(color))
                otherColors.Add(color);

        allColorsOrder = new List<Card.CasterColor>();
        allColorsOrder.AddRange(PartyController.party.partyColors);
        allColorsOrder.AddRange(otherColors);
    }

    public bool CheckDeckComplete()
    {
        int totalNumOfBlanks = 0;
        foreach (string color in PartyController.party.GetPotentialPlayerTexts())
        {
            bool colorHasBlanks = false;
            foreach (CardController card in selectedDeck[color].deck)
                if (card == null)
                {
                    totalNumOfBlanks++;
                    colorHasBlanks = true;
                }

            if (allColorsOrder.IndexOf(PartyController.party.GetPlayerCasterColor(color)) != -1)
                deckButtons[allColorsOrder.IndexOf(PartyController.party.GetPlayerCasterColor(color))].transform.GetChild(0).GetComponent<Image>().enabled = colorHasBlanks;
        }

        if (isStoryMode)
        {
            if (totalNumOfBlanks > 0)
                StoryModeController.story.ShowMenuNotification(3, true, false);
            else if (totalNumOfBlanks == 0)
                StoryModeController.story.ShowMenuNotification(3, false, false);
        }
        return totalNumOfBlanks == 0;
    }

    public bool CheckDeckComplete(Card.CasterColor color)
    {
        foreach (CardController card in selectedDeck[color.ToString()].deck)
            if (card == null)
                return false;

        return true;
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
        RefreshDecks();
        SetIsShowingCards(true);
    }

    public void AddRewardsEquipment(Equipment e, bool isRewardsEquipment = true)
    {
        if (isRewardsEquipment)
            GameController.gameController.RecordRewardEquipments(e);
        recentRewardsEquipment = e;

        newEquipments.equipments.Add(e);
        completeEquipments.equipments.Add(e);
        ReCountUniqueEquipments();
        RefreshEquipments();
        SetIsShowingCards(false);
    }

    public void RemoveCardFromNew(CardController newCard)
    {
        int deckIndex = PartyController.party.GetPartyIndex(newCard.GetCard().casterColor);
        if (newCards[newCard.GetCard().casterColor.ToString()].deck.Contains(newCard))
            newCards[newCard.GetCard().casterColor.ToString()].deck.Remove(newCard);

        for (int i = 0; i < deckButtons.Length; i++)
            if (i < allColorsOrder.Count)
            {
                if (newCards[PartyController.party.GetPlayerColorText(allColorsOrder[i])].deck.Count == 0)
                    deckButtons[i].GetComponent<Outline>().enabled = false;
                else
                    deckButtons[i].GetComponent<Outline>().enabled = true;
            }
            else
                deckButtons[i].GetComponent<Outline>().enabled = false;
    }

    public void RemoveEquipmentFromNew(Equipment e)
    {
        if (newEquipments.equipments.Contains(e))
            newEquipments.equipments.Remove(e);

        gearButton.GetComponent<Outline>().enabled = newEquipments.equipments.Count() != 0;
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
                if (new List<Card.Rarity> { Card.Rarity.StarterAttack, Card.Rarity.StarterDefence, Card.Rarity.StarterSpecial }.Contains(card.GetCard().rarity))         //Add the starter cards first
                {
                    if (temp.ContainsKey(card.GetCard().name))
                        temp[card.GetCard().name] += 1;
                    else
                        temp[card.GetCard().name] = 1;
                }
            }
            foreach (CardController card in completeDeck[color].deck)
            {
                if (!new List<Card.Rarity> { Card.Rarity.StarterAttack, Card.Rarity.StarterDefence, Card.Rarity.StarterSpecial }.Contains(card.GetCard().rarity))        //Add the other cards later
                {
                    if (temp.ContainsKey(card.GetCard().name))
                        temp[card.GetCard().name] += 1;
                    else
                        temp[card.GetCard().name] = 1;
                }
            }
            uniqueCards[color] = temp;
        }

        ResolveSelectedList();
    }

    public void ReCountUniqueEquipments()
    {
        uniqueEquipments = new Dictionary<string, int>();

        completeEquipments.equipments = completeEquipments.equipments.OrderByDescending(x => x.isWeapon).ThenByDescending(x => x.numOfCardSlots).ThenByDescending(x => x.atkChange + x.armorChange / 2f + x.healthChange / 5f).ThenBy(x => x.equipmentName).ToList();

        foreach (Equipment equip in completeEquipments.equipments)
        {
            if (uniqueEquipments.ContainsKey(equip.equipmentName))
                uniqueEquipments[equip.equipmentName] += 1;
            else
                uniqueEquipments[equip.equipmentName] = 1;
        }

        ResolveSelectedEquipmentList();
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

        if (InformationLogger.infoLogger.debug)
            foreach (string equip in debugCards)
                if (!value.ContainsKey(equip))
                    value[equip] = 3;

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

        /*
        if (InformationLogger.infoLogger.debug)
        {
            foreach (string equip in debugEquipments)
                if (!completeEquipmentNames.ContainsKey(equip))
                    completeEquipmentNames[equip] = 3;
        }
        */
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
    }

    public void SetInCombatDecks()
    {
        SetIsShowingCards(true);

        Dictionary<string, int> combatSelectedDeck = new Dictionary<string, int>();
        //Make copy of the selected deck to prevent modyfing selected cards in runs to change that in story mode
        foreach (string card in GetSelectedDeckDict().Keys)
        {
            Card c = LootController.loot.GetCardWithName(card);
            combatSelectedDeck[card] = GetSelectedDeckDict()[card];
        }
        //Give all copies of the starter cards in deck even if some are not equipped
        for (int i = 0; i < 6; i++)
            foreach (Card starterCards in storyModeDeck[i].deck)
                if (PartyController.party.partyColors.Contains(starterCards.casterColor))
                {
                    if (new List<Card.Rarity> { Card.Rarity.StarterAttack, Card.Rarity.StarterDefence }.Contains(starterCards.rarity))
                        combatSelectedDeck[starterCards.name] = 3;
                    else if (starterCards.rarity == Card.Rarity.StarterSpecial)
                        combatSelectedDeck[starterCards.name] = 2;
                }

        SetCompleteDeck(combatSelectedDeck);     //Create a new list of complete deck with the card names from select deck
        foreach (string color in selectedDeck.Keys)
            foreach (CardController c in selectedDeck[color].deck)
                c.SetStartedInDeck(true);

        Dictionary<string, int> selectedEquipmentNames = new Dictionary<string, int>();
        Dictionary<string, string[]> selectedEquipments = GetSelectEquipments();
        Dictionary<string, string[]> combatSelectedEquipments = new Dictionary<string, string[]>();
        foreach (string color in selectedEquipments.Keys)
            if (PartyController.party.partyColors.Contains(PartyController.party.GetPlayerCasterColor(color)))
            {
                combatSelectedEquipments[color] = selectedEquipments[color];
                foreach (string equip in selectedEquipments[color])
                {
                    if (selectedEquipmentNames.ContainsKey(equip))
                        selectedEquipmentNames[equip] += 1;
                    else
                        selectedEquipmentNames[equip] = 1;
                }
            }
        SetSelectedEquipments(combatSelectedEquipments);
        SetCompleteEquipments(selectedEquipmentNames);

        ReCountUniqueCards();
        RefreshDecks();
        RefreshEquipments();
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

    public void SetSelectCardWhiteOut(bool state, int index, Color c)
    {
        if (!state)
        {
            foreach (SelectedCardController card in selectedCardsDisplay)
                card.SetWhiteOut(false, Color.white);
            return;
        }

        //Obtain the range of indexes the new card can be put into
        Vector2 ranges = GetSelectCardCategoryIndexRanges(index);
        int startingIndex = (int)ranges.x;
        int range = (int)ranges.y;

        for (int i = startingIndex; i < startingIndex + range; i++)
            selectedCardsDisplay[i].SetWhiteOut(state, c);
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
        if (selectedEquipments.ContainsKey(PartyController.party.GetPlayerColorText(allColorsOrder[deckID])))
            foreach (Equipment e in selectedEquipments[PartyController.party.GetPlayerColorText(allColorsOrder[deckID])].equipments)
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
        //<color, equipmentnames[]>
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

        if (InformationLogger.infoLogger.debug)
            foreach (string e in debugEquipments)
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
    public void SetStoryModeDefaultDeckAsSelectedDeck()
    {
        selectedEquipments = new Dictionary<string, EquipmentWrapper>();
        completeEquipments = new EquipmentWrapper();
        completeEquipments.equipments = new List<Equipment>();

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
            completeDeck[color].deck = temp;

            selectedEquipments[color] = new EquipmentWrapper();
            selectedEquipments[color].equipments = new List<Equipment>();
        }

        ReCountUniqueCards();
        RefreshDecks();
    }

    public void SetIsShowingCards(bool state)
    {
        isShowingCards = state;
        if (isShowingCards)
        {
            StoryModeController.story.ShowMenuSelected(3);
            cardButton.Enable(false);
            gearButton.Enable(true);
        }
        else
        {
            StoryModeController.story.ShowMenuSelected(2);
            cardButton.Enable(true);
            gearButton.Enable(false);
        }
        CheckPageButtons();
    }

    public void ShowErrorMessage(string errorText)
    {
        errorMessage.color = new Color(errorMessage.color.r, errorMessage.color.g, errorMessage.color.b, 1);
        Color textColor = errorMessage.transform.GetChild(0).GetComponent<Text>().color;
        errorMessage.transform.GetChild(0).GetComponent<Text>().text = errorText;
        errorMessage.transform.GetChild(0).GetComponent<Text>().color = new Color(textColor.r, textColor.g, textColor.b, 1);
        errorMessage.transform.SetAsLastSibling();
        StartCoroutine(FadeErrorMessage());
    }

    public void HideErrorMessage()
    {
        StopAllCoroutines();
        errorMessage.color = new Color(errorMessage.color.r, errorMessage.color.g, errorMessage.color.b, 0);
        Color textColor = errorMessage.transform.GetChild(0).GetComponent<Text>().color;
        errorMessage.transform.GetChild(0).GetComponent<Text>().color = new Color(textColor.r, textColor.g, textColor.b, 0);
    }

    private IEnumerator FadeErrorMessage()
    {
        yield return new WaitForSeconds(3.33f);     //0.33s per word for 10 words
        errorMessage.color = new Color(errorMessage.color.r, errorMessage.color.g, errorMessage.color.b, 0);
        Color textColor = errorMessage.transform.GetChild(0).GetComponent<Text>().color;
        errorMessage.transform.GetChild(0).GetComponent<Text>().color = new Color(textColor.r, textColor.g, textColor.b, 0);
    }

    public void ResetStatsTexts(Equipment equipment = null, bool equipping = true)
    {
        Card.CasterColor color = PartyController.party.GetPlayerCasterColor(PartyController.party.GetPlayerColorText(allColorsOrder[deckID]));

        Vector2 s = GetWeaponAndAccessorySlots();
        int weaponCardSlots = (int)s.x;
        int accessoryCardSlots = (int)s.y;

        int atk = PartyController.party.GetStartingAttack(color);
        int armor = PartyController.party.GetStartingArmor(color);
        int health = PartyController.party.GetStartingHealth(color);
        int cardSlotChange = 0;
        int atkChange = 0;
        int armorChange = 0;
        int healthChange = 0;

        int weaponAtk = 0;
        int weaponArmor = 0;
        int weaponHealth = 0;
        int accessoryAtk = 0;
        int accessoryArmor = 0;
        int accessoryHealth = 0;

        foreach (Equipment e in selectedEquipments[PartyController.party.GetPlayerColorText(allColorsOrder[deckID])].equipments)
        {
            if (e.isWeapon)
            {
                weaponAtk = e.atkChange;
                weaponArmor = e.armorChange;
                weaponHealth = e.healthChange;
            }
            else
            {
                accessoryAtk = e.atkChange;
                accessoryArmor = e.armorChange;
                accessoryHealth = e.healthChange;

            }
        }

        if (equipment != null)
        {
            if (!equipping)
            {
                cardSlotChange = -equipment.numOfCardSlots;
                atkChange = -equipment.atkChange;
                armorChange = -equipment.armorChange;
                healthChange = -equipment.healthChange;
            }
            else
            {
                if (equipment.isWeapon)
                {
                    cardSlotChange = equipment.numOfCardSlots - weaponCardSlots;
                    atkChange = equipment.atkChange - weaponAtk;
                    armorChange = equipment.armorChange - weaponArmor;
                    healthChange = equipment.healthChange - weaponHealth;
                }
                else
                {
                    cardSlotChange = equipment.numOfCardSlots - accessoryCardSlots;
                    atkChange = equipment.atkChange - accessoryAtk;
                    armorChange = equipment.armorChange - accessoryArmor;
                    healthChange = equipment.healthChange - accessoryHealth;
                }
            }
        }

        statTexts[0].text = (weaponCardSlots + accessoryCardSlots).ToString();
        statTexts[1].text = (atk + weaponAtk + accessoryAtk).ToString();
        statTexts[2].text = (armor + weaponArmor + accessoryArmor).ToString();
        statTexts[3].text = (health + weaponHealth + accessoryHealth).ToString();

        if (cardSlotChange != 0)
        {
            statChangeTexts[0].text = cardSlotChange.ToString("+0;-#");
            if (cardSlotChange > 0)
                statChangeTexts[0].color = plusColor;
            else
                statChangeTexts[0].color = minusColor;
        }
        else
            statChangeTexts[0].text = "";
        if (atkChange != 0)
        {
            statChangeTexts[1].text = atkChange.ToString("+0;-#");
            if (atkChange > 0)
                statChangeTexts[1].color = plusColor;
            else
                statChangeTexts[1].color = minusColor;
        }
        else
            statChangeTexts[1].text = "";
        if (armorChange != 0)
        {
            statChangeTexts[2].text = armorChange.ToString("+0;-#");
            if (armorChange > 0)
                statChangeTexts[2].color = plusColor;
            else
                statChangeTexts[2].color = minusColor;
        }
        else
            statChangeTexts[2].text = "";
        if (healthChange != 0)
        {
            statChangeTexts[3].text = healthChange.ToString("+0;-#");
            if (healthChange > 0)
                statChangeTexts[3].color = plusColor;
            else
                statChangeTexts[3].color = minusColor;
        }
        else
            statChangeTexts[3].text = "";
    }

    public int GetEquipmentAttack(Card.CasterColor color)
    {
        int output = 0;
        foreach (Equipment e in selectedEquipments[color.ToString()].equipments)
            output += e.atkChange;
        return output;
    }
}
