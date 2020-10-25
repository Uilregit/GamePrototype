using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class CardDragController : DragController
{
    private LineRenderer line;
    public GameObject moveShadow;
    private CardDisplay cardDisplay;
    private bool active = true;
    private bool isHeld = false;

    private BoxCollider2D col;
    private Vector2 colliderSize;

    private bool isCasting = false;
    private bool isTriggeringEffect = false;
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
        col = GetComponent<BoxCollider2D>();

        colliderSize = col.size;
    }

    // Update is called once per frame
    void Update()
    {
        if (active)
            if (transform.position.y >= HandController.handController.cardCastVertThreshold)
            {
                if (!isTriggeringEffect)
                    if (!isCasting)
                        Cast();
                    else
                        Aim();
            }
            else if (transform.position.y < HandController.handController.cardCastVertThreshold && isCasting)
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
        isTriggeringEffect = false;
        castLocation = originalLocation;
        transform.localScale = new Vector2(HandController.handController.cardStartingSize, HandController.handController.cardStartingSize);
        moveShadow.transform.position = castLocation;
        cardDisplay.Show();
        line.enabled = false;
        moveShadow.GetComponent<SpriteRenderer>().enabled = false;
        moveShadow.transform.localScale = new Vector2(1, 1);

        if (isHeld)
        {
            HandController.handController.UnholdCard(true);
            isHeld = false;
        }

        //UIController.ui.ResetManaBar(TurnController.turnController.GetCurrentMana());
        HandController.handController.ResetCardPositions();
    }

    private void Aim()
    {
        moveShadow.transform.localScale = new Vector2(1 / moveShadow.transform.parent.localScale.x, 1 / moveShadow.transform.parent.localScale.x);
        Card card = cardController.GetCard();
        if (card.castType != Card.CastType.None)
        {
            //if (card.castType == Card.CastType.AoE)

            if (card.castType == Card.CastType.TargetedAoE)
            {
                TileCreator.tileCreator.DestroyTiles(this.gameObject, 1);
                if (cardController.CheckIfValidCastLocation(GridController.gridController.GetRoundedVector(Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0)), 1)))
                    TileCreator.tileCreator.CreateTiles(this.gameObject, GridController.gridController.GetRoundedVector(Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0)), 1), card.castShape, card.radius, Color.red, 1);
            }
            //else
            line.enabled = true;
            line.SetPosition(0, originalLocation);
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
            line.SetPosition(1, mousePosition);
            if (cardController.CheckIfValidCastLocation(GridController.gridController.GetRoundedVector(mousePosition, 1)))
                castLocation = GridController.gridController.GetRoundedVector(mousePosition, 1);
            else
                castLocation = originalLocation;
            moveShadow.transform.position = castLocation;
        }
        else
        {
            moveShadow.transform.localScale = new Vector2(100, 100);
            line.enabled = false;
        }
    }

    private void Hold()
    {
        cardController.DeleteRangeIndicator();
        HandController.handController.HoldCard(cardController);
    }

    private void Replace()
    {
        cardController.DeleteRangeIndicator();
        HandController.handController.ReplaceCard(cardController);
    }

    private void OnMouseUp()
    {
        cardController.GetCaster().GetComponent<PlayerController>().SetCasting(false);
        UIController.ui.ResetManaBar(TurnController.turnController.GetCurrentMana());

        //Resolve cast
        cardController.DeleteRangeIndicator();

        //Sort UI layering
        transform.SetParent(CanvasController.canvasController.uiCanvas.transform);
        cardDisplay.cardName.sortingOrder = 1;

        // For hold and replace
        Transform trn = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0)), Vector2.zero, 0.001f, LayerMask.GetMask("Raycast")).transform;
        if (trn != null)
        {
            if (trn.tag == "Hold" && HandController.handController.GetHeldCard() == null)
                Hold();
            else if (trn.tag == "Hold" && HandController.handController.GetHeldCard() != null)
            {
                //Shrunken
                transform.localScale = new Vector2(HandController.handController.cardStartingSize, HandController.handController.cardStartingSize);
                transform.position = originalLocation;
                offset = Vector2.zero;
            }
            else if (trn.tag == "Replace")
                Replace();
        }
        //For casting
        else
        {
            if (!isCasting)
            {
                //Shrunken
                transform.localScale = new Vector2(HandController.handController.cardStartingSize, HandController.handController.cardStartingSize);
                transform.position = originalLocation;
                offset = Vector2.zero;

                if (isHeld)
                {
                    HandController.handController.UnholdCard(true);
                    isHeld = false;
                }

                //UIController.ui.ResetManaBar(TurnController.turnController.GetCurrentMana());
            }
            else
            {
                Trigger();
            }
        }
    }

    public override void OnMouseDown()
    {
        //Enlarge for easy viewing
        if (!isCasting)
        {
            //Sort UI layering
            transform.SetParent(CanvasController.canvasController.selectedCardCanvas.transform);
            cardDisplay.cardName.sortingOrder = 3;

            transform.localScale = new Vector2(HandController.handController.cardHighlightSize, HandController.handController.cardHighlightSize);
            float x = Mathf.Clamp(Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0)).x, -HandController.handController.cardHighlightXBoarder, HandController.handController.cardHighlightXBoarder);
            float y = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0)).y + HandController.handController.cardHighlightHeight;
            transform.position = new Vector2(x, y);
            transform.SetAsLastSibling();

            cardController.GetCaster().GetComponent<PlayerController>().SetCasting(true);
        }
        cardController.CreateRangeIndicator();

        if (cardController.GetNetManaCost() > 0)
            UIController.ui.SetAnticipatedManaLoss(cardController.GetNetManaCost());
        if (cardController.GetNetEnergyCost() > 0)
            UIController.ui.SetAnticipatedManaGain(cardController.GetNetEnergyCost());

        base.OnMouseDown();
    }

    public override void OnMouseDrag()
    {
        Transform trn = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0)), Vector2.zero, 0.001f, LayerMask.GetMask("Raycast")).transform;
        if (trn != null)
        {
            if (trn.tag == "Replace" || (trn.tag == "Hold" && (HandController.handController.GetHeldCard() == this || HandController.handController.GetHeldCard() == null)))
            {
                transform.position = trn.position;
                transform.localScale = new Vector2(HandController.handController.cardHoldSize, HandController.handController.cardHoldSize);
            }
            else
                base.OnMouseDrag();
        }
        else
        {
            transform.localScale = new Vector2(HandController.handController.cardHighlightSize, HandController.handController.cardHighlightSize);

            //Allow drag above threshold only if there is enough mana left over
            if (TurnController.turnController.HasEnoughResources(cardController.GetNetEnergyCost(), cardController.GetNetManaCost()) &&
                (!GameController.gameController.GetDeadChars().Contains(cardController.GetCard().casterColor) || ResourceController.resource.GetLives() > 0))       //And there's enough lives to cast
            {
                if (!isCasting)
                {
                    if (TurnController.turnController.GetIsPlayerTurn())
                    {
                        newLocation = offset + (Vector2)Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
                        transform.position = newLocation;
                    }
                }
                else
                    base.OnMouseDrag();
            }
            else
            {
                newLocation = offset + (Vector2)Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
                newLocation.y = Mathf.Min(HandController.handController.cardCastVertThreshold - 0.01f, newLocation.y);
                transform.position = newLocation;
            }
        }
    }

    private void Trigger()
    {
        isTriggeringEffect = true;

        cardController.DeleteRangeIndicator();

        Card card = cardController.GetCard();
        cardController.GetComponent<CardEffectsController>().SetCastLocation(castLocation);
        List<GameObject> target = GridController.gridController.GetObjectAtLocation(castLocation, new string[] { "Player", "Enemy" });
        List<Vector2> targetedLocs = new List<Vector2>();
        if (card.castType == Card.CastType.TargetedAoE)
            target.AddRange(GridController.gridController.GetObjectsInAoE(castLocation, card.radius, new string[] { "All" }));
        if (card.castType == Card.CastType.AoE)
        {
            Vector2 casterPosition = cardController.FindCaster(card).transform.position;
            if (GridController.gridController.GetManhattanDistance(castLocation, casterPosition) <= card.radius)
                targetedLocs.AddRange(GridController.gridController.GetLocationsInAoE(casterPosition, card.radius, new string[] { "All" }));
            else
            {
                UnCast();
                transform.position = originalLocation;
                return;
            }
        }
        else if (target.Count != 0)
        {
            if (card.castType == Card.CastType.Enemy && target.Any(x => x.tag == "Enemy") ||
                card.castType == Card.CastType.Player && target.Any(x => x.tag == "Player") ||
                card.castType == Card.CastType.Any && (target.Any(x => x.tag == "Player") || target.Any(x => x.tag == "Enemy")))
            {
                List<Vector2> locations = new List<Vector2>();
                for (int i = 0; i < target.Count; i++)
                    foreach (Vector2 loc in target[i].GetComponent<HealthController>().GetOccupiedSpaces())
                        locations.Add(GridController.gridController.GetRoundedVector(target[i].transform.position, target[i].GetComponent<HealthController>().size) + loc);
                locations = locations.Distinct().ToList();
                targetedLocs.AddRange(locations);
            }
            else if (card.castType == Card.CastType.TargetedAoE)
            {
                card.SetCenter(castLocation);
                List<Vector2> locations = new List<Vector2>();
                for (int i = 0; i < target.Count; i++)
                    foreach (Vector2 loc in target[i].GetComponent<HealthController>().GetOccupiedSpaces())
                        locations.Add(GridController.gridController.GetRoundedVector(target[i].transform.position, target[i].GetComponent<HealthController>().size) + loc);
                locations = locations.Distinct().ToList();
                targetedLocs.AddRange(locations);
            }
            else
            {
                UnCast();
                transform.position = originalLocation;
                return;
            }
        }
        else if (card.castType == Card.CastType.None)
            targetedLocs.Add(transform.position);
        else if (card.castType == Card.CastType.EmptySpace)
        {
            if (!GridController.gridController.CheckIfOutOfBounds(castLocation) &&
                GridController.gridController.GetObjectAtLocation(castLocation).Count == 0)
                targetedLocs.Add(castLocation);
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
        line.enabled = false;
        cardController.DeleteRangeIndicator();
        moveShadow.GetComponent<SpriteRenderer>().enabled = false;

        cardController.GetCaster().GetComponent<PlayerController>().TriggerAttack();

        StartCoroutine(OnPlay(targetedLocs));
        TurnController.turnController.ReportPlayedCard(card, cardController.GetNetEnergyCost(), cardController.GetNetManaCost());
        GameObject.FindGameObjectWithTag("Hand").GetComponent<HandController>().RemoveCard(cardController);
        //HandController.handController.ResetCardPlayability(TurnController.turnController.GetCurrentEnergy(), TurnController.turnController.GetCurrentMana());
    }

    private IEnumerator OnPlay(Vector2 location)
    {
        List<Vector2> locations = new List<Vector2>();
        locations.Add(location);
        yield return StartCoroutine(OnPlay(locations));
    }

    private IEnumerator OnPlay(List<Vector2> locations)
    {
        //Get a unique list of locations to avoid double casting
        locations = locations.Distinct().ToList();
        //Use the mana of the card if it's not a enemy card
        if (cardController.GetCard().casterColor != Card.CasterColor.Enemy)
            TurnController.turnController.UseResources(cardController.GetNetEnergyCost(), cardController.GetNetManaCost());

        //cardController.SetTarget(locations);

        if (isHeld)
        {
            HandController.handController.UnholdCard(false);
            isHeld = false;
        }

        //cardController.TriggerEffect();  //disable for attack queue
        cardDisplay.Hide();

        yield return StartCoroutine(cardController.GetComponent<CardEffectsController>().TriggerEffect(cardController.FindCaster(cardController.GetCard()), locations));

        Destroy(this.gameObject);
    }

    public void SetOriginalLocation(Vector2 location)
    {
        originalLocation = location;
    }

    public void SetActive(bool state)
    {
        active = state;
    }

    public void SetHeld(bool state)
    {
        isHeld = state;
    }
}
