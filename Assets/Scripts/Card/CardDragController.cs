using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardDragController : DragController
{
    private LineRenderer line;
    public GameObject moveShadow;
    public float vertThreshold;
    private CardDisplay cardDisplay;

    private bool isCasting = false;
    private Vector2 castLocation;
    private Vector2 originalLocation;

    private CardController cardController;

    // Start is called before the first frame update
    void Start()
    {
        originalLocation = transform.position;
        castLocation = transform.position;
        line = GetComponent<LineRenderer>();
        cardDisplay = GetComponent<CardDisplay>();
        cardController = GetComponent<CardController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.y >= vertThreshold)
        {
            if (!isCasting)
                Cast();
            else
                Aim();
        }
        else if (transform.position.y < vertThreshold && isCasting)
            UnCast();
    }

    private void Cast()
    {
        isCasting = true;
        cardDisplay.Hide();
        line.enabled = true;
        moveShadow.GetComponent<SpriteRenderer>().enabled = true;
    }

    private void UnCast()
    {
        isCasting = false;
        castLocation = originalLocation;
        moveShadow.transform.position = castLocation;
        cardDisplay.Show();
        line.enabled = false;
        moveShadow.GetComponent<SpriteRenderer>().enabled = false;
        moveShadow.transform.localScale = new Vector2(1, 1);
    }

    private void Aim()
    {
        if (cardController.GetCard().castType != Card.CastType.None)
        {
            if (cardController.GetCard().castType == Card.CastType.AoE)
                line.enabled = false;
            else
                line.SetPosition(0, originalLocation);
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
            line.SetPosition(1, mousePosition);
            if (cardController.CheckIfValidCastLocation(RoundVector(mousePosition)))
                castLocation = RoundVector(mousePosition);
            moveShadow.transform.position = castLocation;
        }
        else
        {
            line.enabled = false;
            moveShadow.transform.localScale = new Vector2(100, 100);
        }
    }

    private Vector2 RoundVector(Vector2 input)
    {
        return new Vector2(Mathf.Round(input.x), Mathf.Round(input.y));
    }

    private void OnMouseUp()
    {
        //Shrunken
        transform.position = originalLocation;
        transform.localScale = new Vector2(HandController.handController.cardStartingSize, HandController.handController.cardStartingSize);
        if (!isCasting)
            cardController.DeleteRangeIndicator();

        //Resolve cast
        cardController.DeleteRangeIndicator();
        if (!isCasting)
        {
            transform.position = originalLocation;
        }
        else
        {
            Trigger();
        }
    }

    public override void OnMouseDown()
    {
        //Enlarge for easy viewing
        if (!isCasting)
        {
            transform.localScale = new Vector2(HandController.handController.cardHighlightSize, HandController.handController.cardHighlightSize);
            transform.position = new Vector2(Mathf.Clamp(transform.position.x, -HandController.handController.cardHighlightXBoarder, HandController.handController.cardHighlightXBoarder), HandController.handController.cardHighlightHeight);
            transform.SetAsLastSibling();
        }
        cardController.CreateRangeIndicator();

        //Allow click only if there is enough mana left over
        if (TurnController.turnController.HasEnoughMana(cardController.GetCard().manaCost))
        {
            base.OnMouseDown();
        }
        else
        {
        }
    }

    public override void OnMouseDrag()
    {
        //Allow drag only if there is enough mana left over
        if (TurnController.turnController.HasEnoughMana(cardController.GetCard().manaCost))
        {
            base.OnMouseDrag();
        }
    }

    private void Trigger()
    {
        cardController.DeleteRangeIndicator();
        GameObject target = GridController.gridController.GetObjectAtLocation(castLocation);
        if (cardController.GetCard().castType == Card.CastType.AoE)
        {
            Vector2 casterPosition = cardController.FindCaster(cardController.GetCard().casterColor).transform.position;
            if (GridController.gridController.GetManhattanDistance(castLocation, casterPosition) <= cardController.GetCard().radius)
            {
                string tag = "Enemy";
                if (cardController.GetCard().targetType[0] == Card.TargetType.Player)
                    tag = "Player";
                else if (cardController.GetCard().targetType[0] == Card.TargetType.Any)
                    tag = "";
                GameObject[] targets = GridController.gridController.GetObjectsInAoE(casterPosition, cardController.GetCard().radius, tag);
                Vector2[] locations = new Vector2[targets.Length];
                for (int i = 0; i < targets.Length; i++)
                    locations[i] = targets[i].transform.position;
                OnPlay(locations);
            }
        }
        else if (target != null)
        {
            if (cardController.GetCard().castType == Card.CastType.Enemy && target.tag == "Enemy" ||
                cardController.GetCard().castType == Card.CastType.Player && target.tag == "Player" ||
                cardController.GetCard().castType == Card.CastType.Any && (target.tag == "Player" || target.tag == "Enemy"))
                OnPlay(target.transform.position);
            else
            {
                UnCast();
                transform.position = originalLocation;
                return;
            }
        }
        else if (cardController.GetCard().castType == Card.CastType.None)
        {
            OnPlay(transform.position);
        }
        else if (cardController.GetCard().castType == Card.CastType.EmptySpace)
        {
            if (!GridController.gridController.CheckIfOutOfBounds(castLocation) &&
                GridController.gridController.GetObjectAtLocation(castLocation) == null)
                OnPlay(castLocation);
            else
            {
                UnCast();
                transform.position = originalLocation;
                return;
            }
        }
        else
        {
            UnCast();
            transform.position = originalLocation;
            return;
        }
        TurnController.turnController.ReportPlayedCard(cardController.GetCard());
        GameObject.FindGameObjectWithTag("Hand").GetComponent<HandController>().RemoveCard(cardController);
    }

    private void OnPlay(Vector2 location)
    {
        //Use the mana of the card if it's not a enemy card
        if (cardController.GetCard().casterColor != Card.CasterColor.Enemy)
            TurnController.turnController.UseMana(cardController.GetCard().manaCost);

        cardController.TriggerEffect(location);
        Destroy(this.gameObject);
    }

    private void OnPlay(Vector2[] locations)
    {
        //Use the mana of the card if it's not a enemy card
        if (cardController.GetCard().casterColor != Card.CasterColor.Enemy)
            TurnController.turnController.UseMana(cardController.GetCard().manaCost);

        foreach (Vector2 location in locations)
            cardController.TriggerEffect(location);
        Destroy(this.gameObject);
    }

    public void SetOriginalLocation(Vector2 location)
    {
        originalLocation = location;
    }
}
