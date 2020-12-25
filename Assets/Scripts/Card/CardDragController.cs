using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class CardDragController : DragController
{
    private LineRenderer line;
    //public GameObject moveShadow;
    private CardDisplay cardDisplay;
    private Card card;
    private bool active = true;
    private bool isHeld = false;

    private BoxCollider2D col;
    private Vector2 colliderSize;

    //private bool isSelected = false;
    //private bool isCasting = false;
    private bool isHolding;
    private bool isTriggeringEffect = false;
    private Vector2 castLocation;
    private Vector3 originalLocation;
    private Vector3 previousMousePosition;

    private CardController cardController;
    private RectTransform rectTransform;
    private Vector3 originalRotation;
    private Vector3 desiredRotation;

    private enum State { Default, Highlighted, Aiming };
    private State currentState = default;

    // Start is called before the first frame update
    void Start()
    {
        originalLocation = transform.position;
        castLocation = transform.position;
        line = transform.GetChild(0).GetComponent<LineRenderer>();
        cardDisplay = transform.GetChild(0).GetComponent<CardDisplay>();
        cardController = GetComponent<CardController>();
        col = GetComponent<BoxCollider2D>();
        rectTransform = GetComponent<RectTransform>();
        originalRotation = rectTransform.rotation.eulerAngles;
        if (originalRotation.x > 180)
            originalRotation.x = originalRotation.x - 360;
        if (originalRotation.y > 180)
            originalRotation.y = originalRotation.y - 360;
        desiredRotation = Vector2.zero;

        colliderSize = col.size;
    }

    //In fixedupdate to ensure equal card wiggle regardless of framerate
    private void FixedUpdate()
    {
        if (currentState == State.Highlighted && !isHolding)
        {
            rectTransform.localScale = new Vector3(HandController.handController.cardHighlightSize, HandController.handController.cardHighlightSize, 1);

            if (desiredRotation.magnitude > 2)
            {
                desiredRotation *= 2;
                if (desiredRotation.magnitude > 30)
                    desiredRotation = desiredRotation.normalized * 30;
                rectTransform.rotation = Quaternion.Slerp(rectTransform.rotation, Quaternion.Euler(desiredRotation + originalRotation), Time.deltaTime * 35); //Uses fixedDeltaTime (default 50 calls per second)
            }
            else
                rectTransform.rotation = Quaternion.Slerp(rectTransform.rotation, Quaternion.Euler(originalRotation), Time.fixedDeltaTime * 10);
        }
        else if (currentState == State.Aiming)
        {
            Vector3 lookAtLocation = CameraController.camera.ScreenToWorldPoint(Input.mousePosition) - transform.position + new Vector3(0, 0, 2);
            rectTransform.rotation = Quaternion.Slerp(rectTransform.rotation, Quaternion.LookRotation(lookAtLocation, new Vector3(0, 0, -1)) * Quaternion.Euler(new Vector3(90, 0, 0)), Time.deltaTime * 40);
        }
        else if (currentState == State.Default)
        {
            rectTransform.rotation = Quaternion.Slerp(rectTransform.rotation, Quaternion.Euler(originalRotation), Time.deltaTime * 40);
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (active)
            if (transform.position.y >= HandController.handController.cardCastVertThreshold)
            {
                if (!isTriggeringEffect)
                    if (currentState == State.Highlighted)
                        Cast();
                    else
                        Aim();
            }
            else if (transform.position.y < HandController.handController.cardCastVertThreshold && currentState == State.Aiming)
                UnCast();
    }

    private void Cast()
    {
        foreach (GameObject player in GameController.gameController.GetLivingPlayers())
            player.GetComponent<Collider2D>().enabled = false;

        CameraController.camera.ScreenShake(0.06f, 0.05f);
        currentState = State.Aiming;
        line.enabled = true;
        transform.localScale = new Vector3(HandController.handController.cardAimSize, HandController.handController.cardAimSize, 1);
    }

    private void UnCast()
    {
        foreach (GameObject player in GameController.gameController.GetLivingPlayers())
            player.GetComponent<Collider2D>().enabled = true;

        CameraController.camera.ScreenShake(0.03f, 0.05f);
        currentState = State.Highlighted;
        isTriggeringEffect = false;
        castLocation = originalLocation;
        rectTransform.localScale = new Vector3(HandController.handController.cardHighlightSize, HandController.handController.cardHighlightSize, 1);
        cardDisplay.Show();
        line.enabled = false;

        if (isHeld)
        {
            HandController.handController.UnholdCard(true);
            isHeld = false;
        }

        HandController.handController.StartCoroutine(HandController.handController.ResetCardPositions());
    }

    private void Aim()
    {
        card = cardController.GetCard();
        if (card.castType != Card.CastType.None)
        {
            //if (card.castType == Card.CastType.AoE)

            int castTileSize = 0;
            if (card.castType == Card.CastType.TargetedAoE || card.castType == Card.CastType.EmptyTargetedAoE)
                castTileSize = card.radius;
            Vector2 mousePosition = CameraController.camera.ScreenToWorldPoint(Input.mousePosition);

            if (GridController.gridController.GetRoundedVector(mousePosition, 1) != castLocation)
            {
                if (TileCreator.tileCreator.GetSelectableLocations().Contains(GridController.gridController.GetRoundedVector(mousePosition, 1)))
                    CameraController.camera.ScreenShake(0.03f, 0.05f);

                TileCreator.tileCreator.DestroyTiles(this.gameObject, 1);
                if (cardController.CheckIfValidCastLocation(GridController.gridController.GetRoundedVector(mousePosition, 1)))
                    TileCreator.tileCreator.CreateTiles(this.gameObject, GridController.gridController.GetRoundedVector(mousePosition, 1), card.castShape, castTileSize, new Color(0.6f, 0, 0), 1);


                if (cardController.CheckIfValidCastLocation(GridController.gridController.GetRoundedVector(mousePosition, 1)))
                    castLocation = GridController.gridController.GetRoundedVector(mousePosition, 1);
                else
                    castLocation = originalLocation;
            }

            transform.position = originalLocation + new Vector3(0, 0.5f, -1f);
            line.enabled = true;
            line.SetPosition(0, new Vector3(transform.position.x, transform.position.y, -1));
            Color color = PartyController.party.GetPlayerColor(card.casterColor);
            line.startColor = new Color(color.r, color.g, color.b, 0.8f);
            line.endColor = new Color(color.r, color.g, color.b, 0f);
            line.startWidth = 1.0f;
            line.endWidth = 0.3f;
            //line.SetWidth(1.3f, 0.3f);
            line.SetPosition(1, mousePosition);
        }
        else
        {
            line.enabled = false;
            transform.rotation = Quaternion.Euler(originalRotation);
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
        desiredRotation = Vector2.zero;

        cardController.GetCaster().GetComponent<HealthController>().charDisplay.charAnimController.SetCasting(false);
        UIController.ui.ResetManaBar(TurnController.turnController.GetCurrentMana());

        //Sort UI layering
        transform.SetParent(CanvasController.canvasController.uiCanvas.transform);
        //cardDisplay.cardName.sortingOrder = 1;

        // For hold and replace
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(ray, out hit, 1000000, LayerMask.GetMask("Raycast"));
        Transform trn = hit.transform;
        if (trn != null)
        {
            if (trn.tag == "Hold" && HandController.handController.GetHeldCard() == null)
                Hold();
            else if (trn.tag == "Hold" && HandController.handController.GetHeldCard() != null)
            {
                //Shrunken
                transform.localScale = new Vector3(HandController.handController.cardStartingSize, HandController.handController.cardStartingSize, 1);
                transform.position = originalLocation;
                offset = Vector2.zero;
            }
            else if (trn.tag == "Replace")
                Replace();
        }
        //For casting
        else
        {
            if (currentState == State.Highlighted)
            {
                //Shrunken
                transform.localScale = new Vector3(HandController.handController.cardStartingSize, HandController.handController.cardStartingSize, 1);
                transform.position = originalLocation;
                offset = Vector2.zero;
                HandController.handController.StartCoroutine(HandController.handController.ResetCardPositions());

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

        cardController.DeleteRangeIndicator();
        currentState = State.Default;
    }

    public override void OnMouseDown()
    {
        currentState = State.Highlighted;
        previousMousePosition = Input.mousePosition;
        //Enlarge for easy viewing

        //Sort UI layering
        transform.SetParent(CanvasController.canvasController.selectedCardCanvas.transform);

        transform.localScale = new Vector3(HandController.handController.cardHighlightSize, HandController.handController.cardHighlightSize, 1);
        float x = Mathf.Clamp(CameraController.camera.ScreenToWorldPoint(Input.mousePosition).x, -HandController.handController.cardHighlightXBoarder, HandController.handController.cardHighlightXBoarder);
        float y = CameraController.camera.ScreenToWorldPoint(Input.mousePosition).y + HandController.handController.cardHighlightHeight;
        transform.position = new Vector2(x, y);
        transform.SetAsLastSibling();

        cardController.GetCaster().GetComponent<HealthController>().charDisplay.charAnimController.SetCasting(true);

        cardController.CreateRangeIndicator();

        if (cardController.GetNetManaCost() > 0)
            UIController.ui.SetAnticipatedManaLoss(cardController.GetNetManaCost());
        if (cardController.GetNetEnergyCost() > 0)
            UIController.ui.SetAnticipatedManaGain(cardController.GetNetEnergyCost());

        base.OnMouseDown();
    }

    public override void OnMouseDrag()
    {
        desiredRotation = new Vector2((Input.mousePosition - previousMousePosition).y, (previousMousePosition - Input.mousePosition).x);
        previousMousePosition = Input.mousePosition;

        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(ray, out hit, 1000000, LayerMask.GetMask("Raycast"));
        Transform trn = hit.transform;
        if (trn != null)
        {
            if (trn.tag == "Replace" || (trn.tag == "Hold" && (HandController.handController.GetHeldCard() == this || HandController.handController.GetHeldCard() == null)))
            {
                isHolding = true;
                transform.position = trn.position;
                transform.localScale = new Vector3(HandController.handController.cardHoldSize, HandController.handController.cardHoldSize, 1);
                rectTransform.rotation = Quaternion.Euler(originalRotation);
            }
            else
                base.OnMouseDrag();
        }
        else
        {
            isHolding = false;
            //transform.localScale = new Vector3(HandController.handController.cardHighlightSize, HandController.handController.cardHighlightSize,1);

            //Allow drag above threshold only if there is enough mana left over
            //if (TurnController.turnController.HasEnoughResources(cardController.GetNetEnergyCost(), cardController.GetNetManaCost()) &&
            //    (!GameController.gameController.GetDeadChars().Contains(cardController.GetCard().casterColor) || ResourceController.resource.GetLives() > 0))       //And there's enough lives to cast
            if (cardDisplay.GetHighlight())
            {
                if (currentState == State.Highlighted)
                {
                    if (TurnController.turnController.GetIsPlayerTurn())
                    {
                        newLocation = offset + (Vector2)CameraController.camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
                        transform.position = newLocation;
                    }
                }
                else
                    base.OnMouseDrag();
            }
            else
            {
                newLocation = offset + (Vector2)CameraController.camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
                newLocation.y = Mathf.Min(HandController.handController.cardCastVertThreshold - 0.01f, newLocation.y);
                transform.position = newLocation;
            }
        }
    }

    private void Trigger()
    {
        isTriggeringEffect = true;

        card = cardController.GetCard();
        cardController.GetComponent<CardEffectsController>().SetCastLocation(castLocation);
        List<GameObject> target = GridController.gridController.GetObjectAtLocation(castLocation, new string[] { "Player", "Enemy" });
        List<Vector2> targetedLocs = new List<Vector2>();
        if (card.castType == Card.CastType.TargetedAoE)
            target.AddRange(GridController.gridController.GetObjectsInAoE(castLocation, card.radius, new string[] { "All" }));
        else if (card.castType == Card.CastType.EmptyTargetedAoE)       //For EmptyTargetedAoE, only the center needs to be empty
            target.AddRange(GridController.gridController.GetObjectAtLocation(castLocation, new string[] { "All" }));
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
        else if (card.castType == Card.CastType.EmptyTargetedAoE)
        {
            targetedLocs.Add(castLocation);
            TileCreator.tileCreator.CreateTiles(this.gameObject, castLocation, Card.CastShape.Circle, card.radius, Color.green, 2);
            List<Vector2> locations = TileCreator.tileCreator.GetTilePositions(2);
            TileCreator.tileCreator.DestroyTiles(this.gameObject, 2);
            locations.Remove(castLocation);
            targetedLocs.AddRange(locations);
        }
        else
        {
            UnCast();
            transform.position = originalLocation;
            return;
        }
        line.enabled = false;

        if (cardController.GetCaster().GetComponent<HealthController>().GetTauntedTarget() != null)
            if (!targetedLocs.Contains(cardController.GetCaster().GetComponent<HealthController>().GetTauntedTarget().transform.position))
                targetedLocs.Add(cardController.GetCaster().GetComponent<HealthController>().GetTauntedTarget().transform.position);        //Alwasy ensure that the taunted target is included in cast

        cardController.GetCaster().GetComponent<HealthController>().charDisplay.charAnimController.TriggerAttack(this.gameObject, targetedLocs, null);

        //StartCoroutine(OnPlay(targetedLocs));
    }

    private IEnumerator OnPlay(Vector2 location)
    {
        List<Vector2> locations = new List<Vector2>();
        locations.Add(location);
        yield return StartCoroutine(OnPlay(locations));
    }

    public IEnumerator OnPlay(List<Vector2> locations)
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

        TurnController.turnController.ReportPlayedCard(card, cardController.GetNetEnergyCost(), cardController.GetNetManaCost());
        GameObject.FindGameObjectWithTag("Hand").GetComponent<HandController>().RemoveCard(cardController);

        foreach (GameObject player in GameController.gameController.GetLivingPlayers())
            player.GetComponent<Collider2D>().enabled = true;

        Destroy(this.gameObject);
    }

    public void SetOriginalLocation(Vector2 location)
    {
        originalLocation = location;
    }

    public void SetActive(bool state)
    {
        active = state;
        GetComponent<Collider2D>().enabled = state;
    }

    public void SetHeld(bool state)
    {
        isHeld = state;
    }
}
