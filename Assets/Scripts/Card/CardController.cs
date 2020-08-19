using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardController : MonoBehaviour
{
    private Card card;
    private CardDisplay cardDisplay;
    private CardEffectsController cardEffects;
    private CardDragController cardDrag;
    private List<Vector2> targets = new List<Vector2>();

    private int manaCostDiscount;
    private int energyCostDiscount;

    private GameObject caster;

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
                TileCreator.tileCreator.CreateTiles(this.gameObject, caster.transform.position, card.castShape, card.radius, HandController.handController.GetCasterColor(card.casterColor));
            else
                TileCreator.tileCreator.CreateTiles(this.gameObject, caster.transform.position, card.castShape, card.range, HandController.handController.GetCasterColor(card.casterColor));
            List<Vector2> castableLocations = TileCreator.tileCreator.GetTilePositions();
            foreach (Vector2 loc in castableLocations)
            {
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
    }

    public void DeleteRangeIndicator()
    {
        TileCreator.tileCreator.DestryTiles(this.gameObject);
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
        if (energy >= GetNetEnergyCost() && mana >= GetNetManaCost() && !GameController.gameController.GetDeadChars().Contains(card.casterColor))
            cardDisplay.SetHighLight(true);
        else
            cardDisplay.SetHighLight(false);
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
                    caster = player;
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
        return caster;
    }

    public void SetCaster(GameObject obj)
    {
        caster = obj;
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
        if (card.manaCost == 0)
            return 0;
        return Mathf.Max(Mathf.Min(card.manaCost, TurnController.turnController.GetManaCostCap()) - GetManaCostDiscount() - TurnController.turnController.GetManaReduction(), 0);
    }

    public int GetNetEnergyCost()
    {
        if (card.energyCost == 0 && card.manaCost != 0)
            return 0;
        return Mathf.Max(Mathf.Min(card.energyCost, TurnController.turnController.GetEnergyCostCap()) - GetEnergyCostDiscount() - TurnController.turnController.GetEnergyReduction(), 0);
    }

    public int GetSimulatedTotalAttackValue(int attackCardIndex)
    {
        return cardEffects.GetSimulatedAttackValue(caster, new List<Vector2> { caster.GetComponent<EnemyController>().desiredTarget[attackCardIndex].transform.position });
    }
}
