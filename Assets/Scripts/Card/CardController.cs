using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Mirror;

public class CardController : MonoBehaviour
{
    private Card card;
    private Card resurrectCard;
    public bool isResurrectCard = false;
    public CardDisplay cardDisplay;
    private CardEffectsController cardEffects;
    private CardDragController cardDrag;
    private List<Vector2> targets = new List<Vector2>();

    private int manaCostDiscount;
    private int energyCostDiscount;

    private int manaCostCap = 99999;
    private int energyCostCap = 99999;

    private GameObject caster;
    private HealthController casterHealthController;

    private bool startedInDeck = false;

    // Start is called before the first frame update
    void Awake()
    {
        cardEffects = this.gameObject.GetComponent<CardEffectsController>();
        cardDrag = this.gameObject.GetComponent<CardDragController>();
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

    public void SetCard(Card newCard, bool containsEffects = true, bool show = true, bool dynamicText = false)
    {
        card = newCard;
        if (!show)
            return;
        if (!containsEffects)
            cardDisplay.SetCard(this, dynamicText);
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
        manaCostCap = newCard.manaCostCap;
        energyCostCap = newCard.energyCostCap;
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
        bool proceed = false;
        try
        {
            if (!GameController.gameController.GetDeadChars().Contains(card.casterColor))
                proceed = true;
        }
        catch
        {
            if (!MultiplayerGameController.gameController.GetDeadChars().Contains(card.casterColor))
                proceed = true;
        }
        if (proceed)
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
            else if (card.castType == Card.CastType.None)
                TileCreator.tileCreator.CreateTiles(this.gameObject, caster.transform.position, Card.CastShape.Circle, 20, PartyController.party.GetPlayerColor(card.casterColor));
            else
                TileCreator.tileCreator.CreateTiles(this.gameObject, caster.transform.position, card.castShape, GetCaster().GetComponent<HealthController>().GetTotalCastRange() + card.range, PartyController.party.GetPlayerColor(card.casterColor));

            List<Vector2> castableLocations = TileCreator.tileCreator.GetTilePositions();
            if (card.castType == Card.CastType.TargetedAoE)
            {
                TileCreator.tileCreator.CreateTiles(this.gameObject, caster.transform.position, card.castShape, GetCaster().GetComponent<HealthController>().GetTotalCastRange() + card.range + card.radius, Color.clear, 1);
                castableLocations = TileCreator.tileCreator.GetTilePositions(1);
                TileCreator.tileCreator.DestroyTiles(this.gameObject, 1);
            }
            foreach (Vector2 loc in castableLocations)
            {
                //Only allow taunted target to be targeted if one exists
                if (caster.GetComponent<HealthController>().GetTauntedTarget() != null)
                    if (castableLocations.Contains(caster.GetComponent<HealthController>().GetTauntedTarget().transform.position))
                    {
                        TileCreator.tileCreator.CreateSelectableTile(caster.GetComponent<HealthController>().GetTauntedTarget().transform.position);
                        break;
                    }

                //If the card can't be casted on self, then remove that from the castable locations
                if (!card.canCastOnSelf && loc == (Vector2)caster.transform.position)
                    continue;

                //Highlight castable locations for cards
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
                    case Card.CastType.TargetedAoE:
                        List<string> tags = new List<string>();
                        if (card.targetType.Any(x => x == Card.TargetType.Any))
                        {
                            tags.Add("Enemy");
                            tags.Add("Player");
                        }
                        else
                        {
                            if (card.targetType.Any(x => x == Card.TargetType.AllEnemies || x == Card.TargetType.Enemy))
                                tags.Add("Enemy");
                            if (card.targetType.Any(x => x == Card.TargetType.AllPlayers || x == Card.TargetType.Player))
                                tags.Add("Player");
                        }
                        if (GridController.gridController.GetObjectAtLocation(loc, tags.ToArray()).Count > 0)
                            TileCreator.tileCreator.CreateSelectableTile(loc);
                        break;
                    case Card.CastType.EmptyTargetedAoE:
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
        List<Vector2> castableLocations = TileCreator.tileCreator.GetTilePositions();       //Allows indicator on ALL locations in range, not just castable locations
        if (card.castShape == Card.CastShape.None)
            return true;
        else
            return castableLocations.Contains(castLocation);
    }

    //If there is enough mana to play the card, show highlight around the card
    public void ResetPlayability(int energy, int mana)
    {
        bool highlight = false;
        bool casterIsAlive = true;

        try
        {
            if (GameController.gameController.GetDeadChars().Contains(card.casterColor))
                casterIsAlive = false;
        }
        catch
        {
            if (MultiplayerGameController.gameController.GetDeadChars().Contains(card.casterColor))
                casterIsAlive = false;
        }

        if ((casterIsAlive && isResurrectCard) ||  //If this was a resurrect card and the player rezed, change it back to what it was
            (ResourceController.resource.GetLives() == 0 && isResurrectCard))                                 //or if the team is out of lives, change it back too
        {
            isResurrectCard = false;
            cardDisplay.SetCard(this);
            cardEffects.SetCard(this);
        }

        if (energy >= GetNetEnergyCost() && mana >= GetNetManaCost() && casterIsAlive)
        {
            cardDisplay.SetHighLight(true);
            highlight = true;
        }
        else if (!casterIsAlive && ResourceController.resource.GetLives() > 0)
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

        if (caster.GetComponent<HealthController>().GetStunned())
        {
            cardDisplay.SetHighLight(false);
            cardDisplay.SetDisableStats("Stunned");
            highlight = false;
        }
        if (card.manaCost > 0 && GetCaster().GetComponent<HealthController>().GetSilenced())
        {
            cardDisplay.SetHighLight(false);
            cardDisplay.SetDisableStats("Silenced");
            highlight = false;
        }
        if (card.manaCost == 0 && GetCaster().GetComponent<HealthController>().GetDisarmed())
        {
            cardDisplay.SetHighLight(false);
            cardDisplay.SetDisableStats("Disarmed");
            highlight = false;
        }

        if (highlight)
        {
            ResetConditionHighlight();              //Only allow conditional highlight if the card is playable
            cardDisplay.disabledStatusText.enabled = false;
            cardDisplay.disabledStatusText.text = "";
        }
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
                cardDisplay.SetConditionHighlight(casterHealthController.GetBonusArmor() > 0);
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

        GameObject[] players = GameController.gameController.GetLivingPlayers().ToArray(); //Else return caster based on color. Will have to change if more colored players are added

        try     //Multiplayer
        {
            players[0].GetComponent<PlayerController>().GetColorTag();
        }
        catch 
        {
            caster = players[0];
            foreach (GameObject player in players)
            {
                if (player.GetComponent<MultiplayerPlayerController>().GetColorTag() == thisCard.casterColor)
                {
                    caster = player;
                    casterHealthController = player.GetComponent<HealthController>();
                }
            }
            return caster;
        }

        try //Singleplayer
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
        GameObject[] players = GameController.gameController.GetLivingPlayers().ToArray();
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

    public int GetEnergyCostCap()
    {
        return energyCostCap;
    }

    public int GetManaCostCap()
    {
        return manaCostCap;
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

    public bool GetStartedInDeck()
    {
        return startedInDeck;
    }

    public void SetStartedInDeck(bool value)
    {
        startedInDeck = value;
    }
}
