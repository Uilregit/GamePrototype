using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardController : MonoBehaviour
{
    private Card card;
    private CardDisplay cardDisplay;
    private CardEffectsController cardEffects;
    private CardDragController cardDrag;

    // Start is called before the first frame update
    void Awake()
    {
        cardDisplay = GetComponent<CardDisplay>();
        cardEffects = GetComponent<CardEffectsController>();
        cardDrag = GetComponent<CardDragController>();
    }

    public void TriggerEffect(Vector2 location)
    {
        if (card.casterColor == Card.CasterColor.Gray)
            cardEffects.TriggerEffect(FindClosestCaster(card.casterColor, location), location);
        else
            cardEffects.TriggerEffect(FindCaster(card.casterColor), location);
    }

    public void SetCard(Card newCard)
    {
        card = newCard;
        cardDisplay.SetCard(card);
        cardEffects.SetCard(card);
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
            GameObject caster = FindCaster(card.casterColor);
            if (card.castType == Card.CastType.AoE)
                TileCreator.tileCreator.CreateTiles(this.gameObject, caster.transform.position, card.castShape, card.radius, HandController.handController.GetCasterColor(card.casterColor));
            else
                TileCreator.tileCreator.CreateTiles(this.gameObject, caster.transform.position, card.castShape, card.range, HandController.handController.GetCasterColor(card.casterColor));
        }
    }

    public void DeleteRangeIndicator()
    {
        TileCreator.tileCreator.DestryTiles(this.gameObject);
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
    public void ResetPlayability(int mana)
    {
        if (mana >= card.manaCost)
            cardDisplay.SetHighLight(true);
        else
            cardDisplay.SetHighLight(false);
    }

    public GameObject FindCaster(Card.CasterColor casterColorTag)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject caster = players[0];
        foreach (GameObject player in players)
        {
            if (player.GetComponent<PlayerController>().GetColorTag() == casterColorTag)
                caster = player;
        }
        return caster;
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
}
