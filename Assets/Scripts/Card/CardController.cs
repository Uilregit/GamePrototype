using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class CardController : MonoBehaviour
{
    private Card card;
    private Card resurrectCard;
    public bool isResurrectCard = false;
    private CardDisplay cardDisplay;
    private CardEffectsController cardEffects;
    private CardDragController cardDrag;
    private List<Vector2> targets = new List<Vector2>();

    private int manaCostDiscount;
    private int energyCostDiscount;

    private int manaCostCap = 99999;
    private int energyCostCap = 99999;

    private GameObject caster;
    private HealthController casterHealthController;

    // Start is called before the first frame update
    void Awake()
    {
        cardDisplay = this.gameObject.GetComponent<CardDisplay>();
        cardEffects = this.gameObject.GetComponent<CardEffectsController>();
        cardDrag = this.gameObject.GetComponent<CardDragController>();
    }

    public void TriggerEffect()
    {
        Debug.Log("cardcontroller trigger");
        /*
        if (card.casterColor == Card.CasterColor.Gray)
            foreach (Vector2 target in targets)
            {
                List<Vector2> temp = new List<Vector2>();
                temp.Add(target);
                GameController.gameController.StartCoroutine(cardEffects.TriggerEffect(FindClosestCaster(card.casterColor, target), temp));
            }
        else
        */
        GameController.gameController.StartCoroutine(cardEffects.TriggerEffect(FindCaster(card), targets));
    }

    public void TriggerOnPlayEffect()
    {
        cardEffects.TriggerOnPlayEffect(FindCaster(card), targets);
    }

    public void SetTarget(Vector2 newTarget)
    {
        targets = new List<Vector2>();
        targets.Add(newTarget);
    }

    public void SetTarget(List<Vector2> newTargets)
    {
        targets = newTargets;
    }

    public List<Vector2> GetTarget()
    {
        return targets;
    }

    public void SetCardDisplay(CardDisplay display)
    {
        cardDisplay = display;
    }

    public void SetCardEffects(CardEffectsController effect)
    {
        cardEffects = effect;
    }

    public void SetCard(Card newCard, bool containsEffects = true, bool show = true)
    {
        card = newCard;
        if (!show)
            return;
        if (!containsEffects)
            cardDisplay.SetCard(this, false);
        else
        {
            cardDisplay.SetCard(this);
            cardEffects.SetCard(this);
        }
    }

    public void SetCardController(CardController newCard)
    {
        card = newCard.card;
        manaCostDiscount = newCard.manaCostDiscount;
        energyCostDiscount = newCard.energyCostDiscount;
        caster = newCard.caster;
        targets = newCard.targets;
        SetCard(newCard.GetCard());
    }

    public Card GetCard()
    {
        if (isResurrectCard)
            return resurrectCard;
        else
            return card;
    }

    public void SetLocation(Vector2 startingLocation)
    {
        transform.position = startingLocation;
        cardDrag.SetOriginalLocation(startingLocation);
    }

    public void CreateRangeIndicator()
    {
        if (!GameController.gameController.GetDeadChars().Contains(card.casterColor))
        {
            /*
            if (card.casterColor == Card.CasterColor.Gray)
            {
                foreach (Card.CasterColor color in new List<Card.CasterColor> { Card.CasterColor.Red, Card.CasterColor.Green, Card.CasterColor.Blue })
                {
                    GameObject caster = FindCaster(color);
                    if (card.castType == Card.CastType.AoE)
                        TileCreator.tileCreator.CreateTiles(this.gameObject, caster.transform.position, card.castShape, card.radius, HandController.handController.GetCasterColor(card.casterColor));
                    else
                        TileCreator.tileCreator.CreateTiles(this.gameObject, caster.transform.position, card.castShape, card.range, HandController.handController.GetCasterColor(card.casterColor));
                }
            }
            else
            {
            */

            GameObject caster = FindCaster(card);
            if (card.castType == Card.CastType.AoE)
                TileCreator.tileCreator.CreateTiles(this.gameObject, caster.transform.position, card.castShape, card.radius, PartyController.party.GetPlayerColor(card.casterColor));
            else
                TileCreator.tileCreator.CreateTiles(this.gameObject, caster.transform.position, card.castShape, GetCaster().GetComponent<HealthController>().GetTotalCastRange(), PartyController.party.GetPlayerColor(card.casterColor));

            if (!card.canCastOnSelf)
                TileCreator.tileCreator.DestroySpecificTiles(this.gameObject, new List<Vector2>() { (Vector2)caster.transform.position });

            List<Vector2> castableLocations = TileCreator.tileCreator.GetTilePositions();
            foreach (Vector2 loc in castableLocations)
            {
                if (!card.canCastOnSelf && loc == (Vector2)caster.transform.position)
                    continue;

                switch (card.castType)
                {
                    case Card.CastType.Any:
                        if (GridController.gridController.GetObjectAtLocation(loc, new string[] { "Enemy", "Player" }).Count > 0)
                            TileCreator.tileCreator.CreateSelectableTile(loc);
                        break;
                    case Card.CastType.AoE:
                        TileCreator.tileCreator.CreateSelectableTile(loc);
                        break;
                    case Card.CastType.EmptySpace:
                        if (GridController.gridController.GetObjectAtLocation(loc).Count == 0)
                            TileCreator.tileCreator.CreateSelectableTile(loc);
                        break;
                    case Card.CastType.Enemy:
                        if (GridController.gridController.GetObjectAtLocation(loc, new string[] { "Enemy" }).Count > 0)
                            TileCreator.tileCreator.CreateSelectableTile(loc);
                        break;
                    case Card.CastType.Player:
                        if (GridController.gridController.GetObjectAtLocation(loc, new string[] { "Player" }).Count > 0)
                            TileCreator.tileCreator.CreateSelectableTile(loc);
                        break;
                }
            }
            //}
        }
        else if (ResourceController.resource.GetLives() > 0)
        {
            TileCreator.tileCreator.CreateTiles(this.gameObject, GridController.gridController.GetDeathLocation(card.casterColor), resurrectCard.castShape, GetCaster().GetComponent<HealthController>().GetTotalCastRange(), PartyController.party.GetPlayerColor(card.casterColor));
        }
    }

    public void DeleteRangeIndicator()
    {
        TileCreator.tileCreator.DestroyTiles(this.gameObject);
        TileCreator.tileCreator.DestroySelectableTiles();
    }

    //If a cast tile has been made for the location, it's castable
    public bool CheckIfValidCastLocation(Vector2 castLocation)
    {
        List<Vector2> castableLocations = TileCreator.tileCreator.GetTilePositions();
        if (card.castShape == Card.CastShape.None)
            return true;
        else
            return castableLocations.Contains(castLocation);
    }

    //If there is enough mana to play the card, show highlight around the card
    public void ResetPlayability(int energy, int mana)
    {
        bool highlight = false;

        if ((!GameController.gameController.GetDeadChars().Contains(card.casterColor) && isResurrectCard) ||  //If this was a resurrect card and the player rezed, change it back to what it was
            (ResourceController.resource.GetLives() == 0 && isResurrectCard))                                 //or if the team is out of lives, change it back too
        {
            isResurrectCard = false;
            cardDisplay.SetCard(this);
            cardEffects.SetCard(this);
        }

        if (energy >= GetNetEnergyCost() && mana >= GetNetManaCost() && !GameController.gameController.GetDeadChars().Contains(card.casterColor))
        {
            if (card.manaCost > 0 && GetCaster().GetComponent<HealthController>().GetSilenced())
                cardDisplay.SetHighLight(false);
            else if (card.manaCost == 0 && GetCaster().GetComponent<HealthController>().GetDisarmed())
                cardDisplay.SetHighLight(false);
            else
            {
                cardDisplay.SetHighLight(true);
                highlight = true;
            }
        }
        else if (GameController.gameController.GetDeadChars().Contains(card.casterColor) && ResourceController.resource.GetLives() > 0)
        {
            isResurrectCard = true;
            resurrectCard = LootController.loot.ResurrectCard.GetCopy();
            resurrectCard.casterColor = card.casterColor;
            resurrectCard.castShape = Card.CastShape.Circle;
            cardDisplay.SetCard(this);
            cardEffects.SetCard(this);
            if (energy >= GetNetEnergyCost() && mana >= GetNetManaCost())
            {
                cardDisplay.SetHighLight(true);
                highlight = true;
            }
            else
                cardDisplay.SetHighLight(false);
        }
        else
            cardDisplay.SetHighLight(false);

        if (highlight)
            ResetConditionHighlight();              //Only allow conditional highlight if the card is playable
        else
            cardDisplay.SetConditionHighlight(false);
    }

    public void ResetConditionHighlight()
    {
        switch (card.highlightCondition)
        {
            case (Card.HighlightCondition.None):
                cardDisplay.SetConditionHighlight(false);
                break;
            case (Card.HighlightCondition.HasBonusATK):
                cardDisplay.SetConditionHighlight(casterHealthController.GetBonusAttack() > 0);
                break;
            case (Card.HighlightCondition.HasBonusArmor):
                cardDisplay.SetConditionHighlight(casterHealthController.GetBonusShield() > 0);
                break;
            case (Card.HighlightCondition.HasBonusVit):
                cardDisplay.SetConditionHighlight(casterHealthController.GetBonusVit() > 0);
                break;
            case (Card.HighlightCondition.HasEnergyCardInDrawDeck):
                cardDisplay.SetConditionHighlight(DeckController.deckController.GetNumberOfEnergyCardsInDraw() != 0);
                break;
            case (Card.HighlightCondition.HasManaCardInDrawDeck):
                cardDisplay.SetConditionHighlight(DeckController.deckController.GetNumberOfManaCardsInDraw() != 0);
                break;
            case (Card.HighlightCondition.PlayedCardsThisTurn):
                cardDisplay.SetConditionHighlight(TurnController.turnController.GetNumerOfCardsPlayedInTurn() != 0);
                break;
        }
    }

    public GameObject FindCaster(Card thisCard)
    {
        if (thisCard.casterColor == Card.CasterColor.Enemy)   //If it's an enemy card, return caster stored in the card
            return caster;

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player"); //Else return caster based on color. Will have to change if more colored players are added
        try
        {
            caster = players[0];
            foreach (GameObject player in players)
            {
                if (player.GetComponent<PlayerController>().GetColorTag() == thisCard.casterColor)
                {
                    caster = player;
                    casterHealthController = player.GetComponent<HealthController>();
                }
            }
            return caster;
        }
        catch
        {
            return null;
        }
    }

    private GameObject FindClosestCaster(Card.CasterColor casterColorTag, Vector2 location)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject caster = players[0];
        foreach (GameObject player in players)
            if ((player.GetComponent<PlayerController>().GetColorTag() == casterColorTag ||
                casterColorTag == Card.CasterColor.Gray) &&
                GridController.gridController.GetManhattanDistance(caster.transform.position, location) >
                GridController.gridController.GetManhattanDistance(player.transform.position, location))
                caster = player;
        casterHealthController = caster.GetComponent<HealthController>();
        return caster;
    }

    public void SetCaster(GameObject obj)
    {
        caster = obj;
        casterHealthController = obj.GetComponent<HealthController>();
    }

    public GameObject GetCaster()
    {
        return caster;
    }

    public void SetEnergyCostDiscount(int value)
    {
        energyCostDiscount += value;
    }

    public void ResetEnergyCostDiscount()
    {
        energyCostDiscount = 0;
    }

    public void SetManaCostDiscount(int value)
    {
        manaCostDiscount += value;
    }

    public void ResetManaCostDiscount()
    {
        manaCostDiscount = 0;
    }

    public void SetManaCostCap(int value)
    {
        manaCostCap = value;
    }

    public void SetEnergyCostCap(int value)
    {
        energyCostCap = value;
    }

    public void ResetManaCostCap()
    {
        manaCostCap = 99999;
    }

    public void ResetEnergyCostCap()
    {
        energyCostCap = 99999;
    }

    public int GetEnergyCostDiscount()
    {
        return energyCostDiscount;
    }

    public int GetManaCostDiscount()
    {
        return manaCostDiscount;
    }

    public int GetNetManaCost()
    {
        Card currentCard = card;
        if (isResurrectCard)
            currentCard = resurrectCard;

        if (currentCard.manaCost == 0)
            return 0;
        HealthController hlth = caster.GetComponent<HealthController>();
        int cost = Mathf.Min(currentCard.manaCost, hlth.GetManaCostCap(), TurnController.turnController.GetManaCostCap(), manaCostCap); //Set cost to the minimum cap for the card
        //Reduce all reduction sources from the cap
        cost -= GetManaCostDiscount();
        cost -= caster.GetComponent<HealthController>().GetManaCostReduction();
        cost -= TurnController.turnController.GetManaReduction();
        return Mathf.Max(cost, 0); //Cost can never be below 0
    }

    public int GetNetEnergyCost()
    {
        Card currentCard = card;
        if (isResurrectCard)
            currentCard = resurrectCard;

        if (currentCard.energyCost == 0 && currentCard.manaCost != 0)
            return 0;
        HealthController hlth = caster.GetComponent<HealthController>();
        int cost = Mathf.Min(currentCard.energyCost, hlth.GetEnergyCostCap(), TurnController.turnController.GetEnergyCostCap(), energyCostCap); //Set cost to the minimum cap for the card
        //Reduce all reduction sources from the cap
        cost -= GetEnergyCostDiscount();
        cost -= caster.GetComponent<HealthController>().GetEnergyCostReduction();
        cost -= TurnController.turnController.GetEnergyReduction();
        return Mathf.Max(cost, 0); //Cost can never be below 0
    }

    public int GetSimulatedTotalAttackValue(int attackCardIndex)
    {
        return cardEffects.GetSimulatedAttackValue(caster, new List<Vector2> { caster.GetComponent<EnemyController>().desiredTarget[attackCardIndex].transform.position });
    }
}
