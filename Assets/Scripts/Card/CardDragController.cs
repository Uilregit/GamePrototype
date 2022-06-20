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
    private Vector2 lastGoodPosition;
    private Vector3 originalLocation;
    private Vector3 previousMousePosition;

    private CardController cardController;
    private RectTransform rectTransform;
    private Vector3 originalRotation;
    private Vector3 desiredRotation;

    private enum State { Default, Highlighted, Aiming };
    private State currentState = default;

    private List<HealthController> knockbackPreviews = new List<HealthController>();

    private List<GameObject> simulationObjs = new List<GameObject>();
    private Dictionary<HealthController, HealthController> simulations = new Dictionary<HealthController, HealthController>();
    private GameObject simulationCaster = null;
    private List<HealthController> stackedObjs = new List<HealthController>();
    private int stackedIndex = -1;

    private Coroutine showingToolTip;
    private Coroutine showingSimulatedHealthBar = null;

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
            rectTransform.localScale = new Vector3(HandController.handController.GetCardHighlightSize(), HandController.handController.GetCardHighlightSize(), 1);

            if (desiredRotation.magnitude > 2)
            {
                desiredRotation *= 2;
                if (desiredRotation.magnitude > 30)
                    desiredRotation = desiredRotation.normalized * 30;
                if (desiredRotation.magnitude > 30 && !cardDisplay.cardSounds.source.isPlaying)
                    cardDisplay.cardSounds.PlayDragSound();

                rectTransform.rotation = Quaternion.Slerp(rectTransform.rotation, Quaternion.Euler(desiredRotation + originalRotation), Time.deltaTime * 35); //Uses fixedDeltaTime (default 50 calls per second)
            }
            else
                rectTransform.rotation = Quaternion.Slerp(rectTransform.rotation, Quaternion.Euler(originalRotation), Time.fixedDeltaTime * 10);

            showingToolTip = StartCoroutine(ShowToolTip());
        }
        else if (currentState == State.Aiming)
        {
            if (!cardDisplay.cardSounds.source.isPlaying)
                cardDisplay.cardSounds.PlayCastingSound();

            if (showingToolTip != null)
                StopCoroutine(showingToolTip);
            cardDisplay.SetToolTip(false);

            Vector3 lookAtLocation = CameraController.camera.ScreenToWorldPoint(Input.mousePosition) - transform.position + new Vector3(0, 0, 2);
            rectTransform.rotation = Quaternion.Slerp(rectTransform.rotation, Quaternion.LookRotation(lookAtLocation, new Vector3(0, 0, -1)) * Quaternion.Euler(new Vector3(90, 0, 0)), Time.deltaTime * 40);
        }
        else if (currentState == State.Default)
        {
            if (showingToolTip != null)
                StopCoroutine(showingToolTip);
            cardDisplay.SetToolTip(false);

            rectTransform.rotation = Quaternion.Slerp(rectTransform.rotation, Quaternion.Euler(originalRotation), Time.deltaTime * 40);
        }
    }

    private IEnumerator ShowToolTip()
    {
        yield return new WaitForSeconds(0.5f);
        if (currentState == State.Highlighted && isHolding == false)
            cardDisplay.SetToolTip(true);
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
        try
        {
            foreach (GameObject player in GameController.gameController.GetLivingPlayers())
                player.GetComponent<Collider2D>().enabled = false;
        }
        catch
        {
            foreach (GameObject player in MultiplayerGameController.gameController.GetLivingPlayers())
                player.GetComponent<Collider2D>().enabled = false;
        }
        CameraController.camera.ScreenShake(0.06f, 0.05f);
        currentState = State.Aiming;
        line.enabled = true;
        transform.localScale = new Vector3(HandController.handController.cardAimSize, HandController.handController.cardAimSize, 1);
    }

    private void UnCast()
    {
        try
        {
            foreach (GameObject player in GameController.gameController.GetLivingPlayers())
                player.GetComponent<Collider2D>().enabled = true;
        }
        catch
        {
            foreach (GameObject player in MultiplayerGameController.gameController.GetLivingPlayers())
                player.GetComponent<Collider2D>().enabled = true;
        }

        cardDisplay.cardSounds.PlayUncastSound();
        CameraController.camera.ScreenShake(0.03f, 0.05f);
        currentState = State.Highlighted;
        isTriggeringEffect = false;
        castLocation = originalLocation;
        rectTransform.localScale = new Vector3(HandController.handController.GetCardHighlightSize(), HandController.handController.GetCardHighlightSize(), 1);
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
        if (card.castType != Card.CastType.None && !TutorialController.tutorial.GetPopupIsShowing())
        {
            //if (card.castType == Card.CastType.AoE)

            int castTileSize = 0;
            if (card.castType == Card.CastType.TargetedAoE || card.castType == Card.CastType.EmptyTargetedAoE)
                castTileSize = card.radius;
            Vector2 mousePosition = CameraController.camera.ScreenToWorldPoint(Input.mousePosition);

            if (GridController.gridController.GetRoundedVector(mousePosition, 1) != lastGoodPosition)
            {
                lastGoodPosition = GridController.gridController.GetRoundedVector(mousePosition, 1);
                if (TileCreator.tileCreator.GetSelectableLocations().Contains(GridController.gridController.GetRoundedVector(mousePosition, 1)))
                    CameraController.camera.ScreenShake(0.03f, 0.05f);

                //Resetting UI indicators
                TileCreator.tileCreator.DestroyTiles(this.gameObject, 1);
                GameController.gameController.SetDamagePrevieBackdrop(new Vector2(999, 999), 1, 1, false);
                HideKnockbackPreview();

                castLocation = originalLocation;

                if (cardController.CheckIfValidCastLocation(GridController.gridController.GetRoundedVector(mousePosition, 1)))
                {
                    TileCreator.tileCreator.CreateTiles(this.gameObject, GridController.gridController.GetRoundedVector(mousePosition, 1), Card.CastShape.Circle, castTileSize, new Color(0.6f, 0, 0), 1);
                    castLocation = GridController.gridController.GetRoundedVector(mousePosition, 1);

                    if (showingSimulatedHealthBar != null)
                    {
                        StopCoroutine(showingSimulatedHealthBar);
                        cardController.StopAllCoroutines();
                        ResetSimulationObjects();
                    }
                    cardController.GetComponent<CardEffectsController>().SetCastLocation(castLocation);
                    card.SetCenter(castLocation);
                    if (GridController.gridController.GetObjectAtLocation(TileCreator.tileCreator.GetTilePositions(1)).Count > 0)
                    {
                        showingSimulatedHealthBar = StartCoroutine(ShowSimulatedHealthBar(TileCreator.tileCreator.GetTilePositions(1)));
                        SetHealthBars(false, GridController.gridController.GetObjectAtLocation(TileCreator.tileCreator.GetTilePositions(1)));
                    }
                    else
                    {
                        HideSimulatedHealthBars();
                        SetHealthBars(true, new List<GameObject>());
                    }
                }
                else
                {
                    HideSimulatedHealthBars();
                    SetHealthBars(true, new List<GameObject>());
                }
            }
            else if (stackedObjs.Count > 1 && cardController.CheckIfValidCastLocation(GridController.gridController.GetRoundedVector(mousePosition, 1)))    //Handles scrubbing for stacked enemies
            {
                int newStackedIndex = (int)Mathf.Floor((mousePosition.x + 0.5f - Mathf.Floor(mousePosition.x + 0.5f)) * stackedObjs.Count);
                if (newStackedIndex != stackedIndex)
                {
                    stackedIndex = newStackedIndex;
                    SetCombatStats(1, stackedObjs[newStackedIndex], simulations[stackedObjs[newStackedIndex]], false);
                    UIController.ui.combatStats.SetStatusCharactersIndex(1, newStackedIndex + 1);
                    SetDamageArrows(stackedObjs[newStackedIndex], simulations[stackedObjs[newStackedIndex]]);
                }
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
        if (TutorialController.tutorial.GetEnabled())
            return;

        ResetSimulationObjects();
        if (simulationCaster != null)
            GameController.gameController.ReportSimulationFinished(simulationCaster.GetComponent<HealthController>());

        desiredRotation = Vector2.zero;

        cardController.GetCaster().GetComponent<HealthController>().charDisplay.charAnimController.SetCasting(false);
        UIController.ui.ResetManaBar(TurnController.turnController.GetCurrentMana());
        GameController.gameController.SetDamagePrevieBackdrop(new Vector2(999, 999), 1, 1, false);
        HideKnockbackPreview();

        //Sort UI layering
        transform.SetParent(CanvasController.canvasController.uiCanvas.transform);

        if (!TutorialController.tutorial.GetPopupIsShowing())
        {
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
                    UncastAndResetCard();

                    if (isHeld)
                    {
                        HandController.handController.UnholdCard(true);
                        isHeld = false;
                    }
                }
                else
                {
                    Trigger();
                }
            }
        }
        else
            UncastAndResetCard();

        cardController.DeleteRangeIndicator();
        if (cardController.GetNetEnergyCost() > 0)
            UIController.ui.SetEnergyGlow(false);
        currentState = State.Default;

        AttackRangeHighlightController.attackRangeHighlight.HideAllTiles();
    }

    private void UncastAndResetCard()
    {
        //Shrunken
        transform.localScale = new Vector3(HandController.handController.cardStartingSize, HandController.handController.cardStartingSize, 1);
        transform.position = originalLocation;
        offset = Vector2.zero;
        HandController.handController.StartCoroutine(HandController.handController.ResetCardPositions());
    }

    public override void OnMouseDown()
    {
        if (TutorialController.tutorial.GetEnabled())
            return;

        try
        {
            if (cardController.GetCard().castType == Card.CastType.TargetedAoE)
                TutorialController.tutorial.TriggerTutorial(Dialogue.Condition.CastTypeTargetedAoESelected, 1);
            else if (cardController.GetCard().castType == Card.CastType.AoE)
                TutorialController.tutorial.TriggerTutorial(Dialogue.Condition.CastTypeAoESelected, 1);
            else
                TutorialController.tutorial.TriggerTutorial(Dialogue.Condition.CastTypeNormalSelected, 1);
        }
        catch { }

        cardDisplay.cardSounds.PlaySelectSound();
        cardController.GetCaster().GetComponent<HealthController>().SetCombatStatsHighlight(0);

        currentState = State.Highlighted;
        previousMousePosition = Input.mousePosition;
        //Enlarge for easy viewing

        //Sort UI layering
        transform.SetParent(CanvasController.canvasController.selectedCardCanvas.transform);

        transform.localScale = new Vector3(HandController.handController.GetCardHighlightSize(), HandController.handController.GetCardHighlightSize(), 1);
        float x = Mathf.Clamp(CameraController.camera.ScreenToWorldPoint(Input.mousePosition).x, -HandController.handController.cardHighlightXBoarder, HandController.handController.cardHighlightXBoarder);
        float y = CameraController.camera.ScreenToWorldPoint(Input.mousePosition).y + HandController.handController.GetCardHighlightHeight();
        transform.position = new Vector2(x, y);
        transform.SetAsLastSibling();

        cardController.GetCaster().GetComponent<HealthController>().charDisplay.charAnimController.SetCasting(true);

        cardController.CreateRangeIndicator();
        //Must go after create range indicator
        AttackRangeHighlightController.attackRangeHighlight.StartCoroutine(AttackRangeHighlightController.attackRangeHighlight.HighlightAttackRange(cardController.GetCaster().transform.position, cardController.GetCaster(), 99, true));

        if (cardController.GetNetManaCost() > 0)
            UIController.ui.SetAnticipatedManaLoss(cardController.GetNetManaCost());
        if (cardController.GetNetEnergyCost() > 0)
        {
            UIController.ui.SetEnergyGlow(true);
            UIController.ui.SetAnticipatedManaGain(cardController.GetNetEnergyCost());
        }

        base.OnMouseDown();
    }

    public override void OnMouseDrag()
    {
        if (TutorialController.tutorial.GetEnabled())
            return;

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
                if (showingToolTip != null)
                    StopCoroutine(showingToolTip);
                cardDisplay.SetToolTip(false);

                isHolding = true;
                Vector3 cardOffset = new Vector3(-0.65f, 0.75f, 0);
                if (trn.tag == "Hold")
                    cardOffset = new Vector3(0.65f, 0.75f, 0);
                transform.position = trn.position + cardOffset;
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
                    }
                    else
                    {
                        newLocation = offset + (Vector2)CameraController.camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
                        newLocation.y = Mathf.Min(HandController.handController.cardCastVertThreshold - 0.01f, newLocation.y);
                    }
                    transform.position = newLocation;
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
        HideSimulatedHealthBars();

        isTriggeringEffect = true;

        List<Vector2> targetedLocs = GetTargetLocations();
        if (targetedLocs.Count == 0)
        {
            UncastAndResetCard();
            UnCast();
            SetHealthBars(true, new List<GameObject>());
            return;
        }

        cardDisplay.cardSounds.PlayCastSound();
        if (card.exhaust)
            cardDisplay.FadeOut(0.5f * TimeController.time.timerMultiplier, Color.clear);

        line.enabled = false;

        if (cardController.GetCaster().GetComponent<HealthController>().GetTauntedTarget() != null)
            if (!targetedLocs.Contains(cardController.GetCaster().GetComponent<HealthController>().GetTauntedTarget().transform.position))
                targetedLocs.Add(cardController.GetCaster().GetComponent<HealthController>().GetTauntedTarget().transform.position);        //Alwasy ensure that the taunted target is included in cast

        cardController.GetCaster().GetComponent<HealthController>().charDisplay.charAnimController.TriggerAttack(this.gameObject, targetedLocs, null);

        //StartCoroutine(OnPlay(targetedLocs));
    }

    private IEnumerator ShowSimulatedHealthBar(List<Vector2> locations)
    {
        HideSimulatedHealthBars();
        SetHealthBars(true, new List<GameObject>());
        HideKnockbackPreview();

        int numPositionsWithMultiTargets = 0;
        int maxTargetsPerPosition = 0;
        List<Vector2> positionsWithTargets = new List<Vector2>();

        foreach (Vector2 loc in locations)           //Show damage that would have been done
        {
            List<GameObject> targets = GridController.gridController.GetObjectAtLocation(loc, new string[] { "Player", "Enemy" });
            if (targets.Count > 1)
            {
                numPositionsWithMultiTargets++;
                maxTargetsPerPosition = Mathf.Max(maxTargetsPerPosition, targets.Count);
            }
            if (targets.Count > 0)
                positionsWithTargets.Add(loc);
        }

        int currentLocationId = 0;
        Vector2 multiloc = locations[0];
        bool flipped = false;

        //Block for creating the simulation objects and processing their simulations
        simulations = new Dictionary<HealthController, HealthController>();
        Dictionary<Vector2, List<HealthController>> objLocations = new Dictionary<Vector2, List<HealthController>>();
        ResetSimulationObjects();
        simulationCaster = null;
        foreach (GameObject obj in GridController.gridController.GetObjectAtLocation(locations, new string[] { "Player", "Enemy" }))
        {
            HealthController hlthController = obj.GetComponent<HealthController>();

            if (hlthController.GetIsSimulation())
                continue;

            HealthController simulation = GameController.gameController.GetSimulationCharacter(hlthController, true);

            simulations.Add(hlthController, simulation);

            if (objLocations.ContainsKey((Vector2)obj.transform.position))
                objLocations[(Vector2)obj.transform.position].Add(hlthController);
            else
                objLocations[(Vector2)obj.transform.position] = new List<HealthController> { hlthController };

            simulationObjs.Add(simulation.gameObject);

            if (obj == cardController.GetCaster())
                simulationCaster = simulation.gameObject;
        }
        if (simulationCaster == null && !cardController.GetCaster().GetComponent<HealthController>().GetIsSimulation())
        {
            simulationCaster = GameController.gameController.GetSimulationCharacter(cardController.GetCaster().GetComponent<HealthController>()).gameObject;
            simulationObjs.Add(simulationCaster);       //Added simulationcaster to simulated objs to be return in case this coroutine is stopped early
        }

        if (objLocations.Keys.Count != 0)
        {
            yield return StartCoroutine(cardController.GetComponent<CardEffectsController>().TriggerEffect(cardController.FindCaster(cardController.GetCard()), simulationObjs, locations, false, true, simulationCaster));

            bool alreadySetHighlight = false;

            //Block for displaying the simulation health information
            foreach (Vector2 loc in objLocations.Keys)           //Show damage that would have been done
            {
                List<HealthController> targets = objLocations[loc];
                if (targets.Count == 1)                 //If single enemy per location, then show health previews normally
                {
                    HealthController hlthController = targets[0];
                    HealthController simulation = simulations[hlthController];
                    Vector2 offset = new Vector2((-(numPositionsWithMultiTargets - 1) / 2f + currentLocationId) * 1.5f, 0.4f);

                    if (TileCreator.tileCreator.GetSelectableLocations().Contains(loc))
                    {
                        if (!alreadySetHighlight)    //Must come before show health bar since that advances all buffs by a turn
                        {
                            SetCombatStats(1, hlthController, simulation, true);
                            alreadySetHighlight = true;
                        }
                        SetDamageArrows(hlthController, simulation);
                    }
                    hlthController.ShowHealthBar(hlthController.GetVit() - simulation.GetVit(), hlthController.GetVit(), true, simulation);

                    ShowKnockbackPreview(hlthController, simulation);
                    hlthController.charDisplay.healthBar.SetPositionRaised(true);
                }
                else if (targets.Count > 1)             //If enemies are stacked, show health previews with yellow backdrop
                {
                    int currentTargetId = 0;
                    multiloc = loc;

                    foreach (HealthController hlthController in targets)
                    {
                        HealthController simulation = simulations[hlthController];

                        Vector2 offset = new Vector2((-(numPositionsWithMultiTargets - 1) / 2f + currentLocationId) * 1.5f, 0.4f + currentTargetId * 1.5f);
                        if (positionsWithTargets.Any(x => x.x == loc.x && x.y > loc.y && x != loc))
                        {
                            offset = new Vector2((-(numPositionsWithMultiTargets - 1) / 2f + currentLocationId) * 1.5f, -3.1f - currentTargetId * 1.5f);
                            flipped = true;
                        }

                        if (currentTargetId == 0 && TileCreator.tileCreator.GetSelectableLocations().Contains(loc))    //Must come before show health bar since that advances all buffs by a turn
                        {
                            if (!alreadySetHighlight)
                            {
                                SetCombatStats(1, hlthController, simulation, false);
                                alreadySetHighlight = true;
                            }
                            SetDamageArrows(hlthController, simulation);
                        }

                        StartCoroutine(hlthController.ShowDamagePreviewBar(hlthController.GetVit() - simulation.GetVit(), hlthController.GetVit(), simulation, hlthController.charDisplay.sprite.sprite, offset));
                        //hlthController.charDisplay.healthBar.SetPositionRaised(true);
                        ShowKnockbackPreview(hlthController, simulation);

                        currentTargetId++;
                    }
                }
                if (targets.Count > 1)
                    currentLocationId++;
            }

            UIController.ui.combatStats.SetStatusCharactersCount(1, simulations.Count);
            if (simulations.Count > 1)
                stackedObjs = simulations.Keys.ToList();
            else
            {
                stackedObjs = new List<HealthController>();
                stackedIndex = -1;
            }

            //Block for moving the backdrop for when overlapping targets are being targeted
            Vector2 backdropOffset = new Vector2(0, 1f);
            if (flipped)
                backdropOffset = new Vector2(0, -1f);
            if (numPositionsWithMultiTargets > 0)
                GameController.gameController.SetDamagePrevieBackdrop(multiloc + backdropOffset, numPositionsWithMultiTargets, maxTargetsPerPosition, flipped);
            else
                GameController.gameController.SetDamagePrevieBackdrop(new Vector2(999, 999), 1, 1, false);
        }

        if (stackedObjs.Count == 0)
        {
            ResetSimulationObjects();
            GameController.gameController.ReportSimulationFinished(simulationCaster.GetComponent<HealthController>());
        }

        showingSimulatedHealthBar = null;
    }

    private void SetCombatStats(int index, HealthController hlthController, HealthController simulation, bool refresh)
    {
        simulation.SetCombatStatsHighlight(index, hlthController.GetVit() - simulation.GetVit(), hlthController.GetArmor() - simulation.GetArmor(), refresh, hlthController);
    }

    private void SetDamageArrows(HealthController hlthController, HealthController simulation)
    {
        if (hlthController.GetVit() - simulation.GetVit() != 0)
            UIController.ui.combatStats.SetArrow(hlthController.GetVit() - simulation.GetVit(), CombatStatsHighlightController.numberType.number);
        else if (card.cardEffectName.Any(x => x == Card.EffectType.Buff))
        {
            for (int i = 0; i < card.cardEffectName.Length; i++)
                if (card.cardEffectName[i] == Card.EffectType.Buff)
                    UIController.ui.combatStats.SetArrow(card.effectDuration[i], CombatStatsHighlightController.numberType.turn, card.buff[i].description.Substring(0, card.buff[i].description.IndexOf(":")));
        }
        else
            UIController.ui.combatStats.SetArrow(0, CombatStatsHighlightController.numberType.number);
        UIController.ui.combatStats.SetDamageArrowEnabled(true);
    }

    private void HideSimulatedHealthBars()
    {
        foreach (GameObject obj in GameController.gameController.GetLivingPlayers())        //Hide all previous health bars
        {
            HealthController hlth = obj.GetComponent<HealthController>();
            if (hlth.charDisplay.healthBar.GetPositionRaised())
            {
                hlth.HideHealthBar();
                hlth.ResetHealthBar();
                hlth.charDisplay.healthBar.SetPositionRaised(false);
                hlth.charDisplay.healthBar.SetArmor(obj.GetComponent<HealthController>().GetArmor());
            }
        }
        foreach (EnemyController obj in TurnController.turnController.GetEnemies())
        {
            HealthController hlth = obj.GetComponent<HealthController>();
            if (obj.GetComponent<HealthController>().charDisplay.healthBar.GetPositionRaised())
            {
                hlth.HideHealthBar();
                hlth.ResetHealthBar();
                hlth.charDisplay.healthBar.SetPositionRaised(false);
                hlth.charDisplay.healthBar.SetArmor(obj.GetComponent<HealthController>().GetArmor());
            }
        }

        UIController.ui.combatStats.SetStatusEnabled(1, false);
        UIController.ui.combatStats.SetDamageArrowEnabled(false);

        stackedObjs = new List<HealthController>();
        stackedIndex = -1;
    }

    public void SetHealthBars(bool state, List<GameObject> avoidedObjects)
    {
        foreach (GameObject obj in GameController.gameController.GetLivingPlayers())        //Hide all previous health bars
        {
            if (avoidedObjects.Contains(obj))
                continue;
            HealthController hlth = obj.GetComponent<HealthController>();
            if (state)
                hlth.charDisplay.healthBar.ShowHealthBar();
            else
                hlth.charDisplay.healthBar.RemoveHealthBar();
        }
        foreach (EnemyController obj in TurnController.turnController.GetEnemies())
        {
            if (state)
                obj.GetComponent<EnemyInformationController>().ShowIntent();
            else if (avoidedObjects.Contains(obj.gameObject))
                obj.GetComponent<EnemyInformationController>().HideIntent();

            if (avoidedObjects.Contains(obj.gameObject))
                continue;
            HealthController hlth = obj.GetComponent<HealthController>();
            if (state)
                hlth.charDisplay.healthBar.ShowHealthBar();
            else
                hlth.charDisplay.healthBar.RemoveHealthBar();
        }
    }

    private void ResetSimulationObjects()
    {
        //Might be overkill returning simulations that would have been returned at the end of the simulated health previews process
        //Or remove from there and keep all simulation controlls here

        foreach (GameObject obj in simulationObjs)
            GameController.gameController.ReportSimulationFinished(obj.GetComponent<HealthController>());
        simulationObjs = new List<GameObject>();
    }

    private void ShowKnockbackPreview(HealthController original, HealthController simulation)
    {
        if (original.transform.position != simulation.transform.position && (original.transform.position - simulation.transform.position).magnitude <= 14)
        {
            Vector3[] positions = new Vector3[4];
            positions[0] = original.transform.position;
            positions[3] = simulation.transform.position;
            positions[1] = simulation.transform.position - (simulation.transform.position - original.transform.position).normalized * 0.5f;
            positions[2] = simulation.transform.position - (simulation.transform.position - original.transform.position).normalized * 0.5f;
            original.charDisplay.lineRenderer.SetPositions(positions);
            original.charDisplay.lineRenderer.enabled = true;

            knockbackPreviews.Add(original);
        }
    }

    private void HideKnockbackPreview()
    {
        foreach (HealthController obj in knockbackPreviews)
            obj.charDisplay.lineRenderer.enabled = false;

        knockbackPreviews = new List<HealthController>();
    }

    public List<Vector2> GetTargetLocations()
    {
        card = cardController.GetCard();
        cardController.GetComponent<CardEffectsController>().SetCastLocation(castLocation);
        card.SetCenter(castLocation);
        List<GameObject> target = GridController.gridController.GetObjectAtLocation(castLocation, new string[] { "Player", "Enemy" });
        if (!card.canCastOnSelf)
            target.Remove(cardController.GetCaster());
        List<Vector2> targetedLocs = new List<Vector2>();
        if (card.castType == Card.CastType.TargetedAoE)
        {
            string[] tags = new string[] { "Player", "Enemy" };
            if (card.targetType[0] == Card.TargetType.AllEnemies || card.targetType[0] == Card.TargetType.Enemy)
                tags = new string[] { "Enemy" };
            else if (card.targetType[0] == Card.TargetType.AllEnemies || card.targetType[0] == Card.TargetType.Enemy)
                tags = new string[] { "Player" };
            target.AddRange(GridController.gridController.GetObjectsInAoE(castLocation, card.radius, tags));
        }
        else if (card.castType == Card.CastType.EmptyTargetedAoE)       //For EmptyTargetedAoE, only the center needs to be empty
        {
            string[] tags = new string[] { "Player", "Enemy" };
            if (card.targetType[0] == Card.TargetType.AllEnemies || card.targetType[0] == Card.TargetType.Enemy)
                tags = new string[] { "Enemy" };
            else if (card.targetType[0] == Card.TargetType.AllEnemies || card.targetType[0] == Card.TargetType.Enemy)
                tags = new string[] { "Player" };
            target.AddRange(GridController.gridController.GetObjectAtLocation(castLocation, tags));
        }
        if (card.castType == Card.CastType.AoE)
        {
            Vector2 casterPosition = cardController.FindCaster(card).transform.position;
            if (GridController.gridController.GetManhattanDistance(castLocation, casterPosition) <= card.radius)
            {
                string[] tags = new string[] { "Player", "Enemy" };
                if (card.targetType[0] == Card.TargetType.AllEnemies || card.targetType[0] == Card.TargetType.Enemy)
                    tags = new string[] { "Enemy" };
                else if (card.targetType[0] == Card.TargetType.AllEnemies || card.targetType[0] == Card.TargetType.Enemy)
                    tags = new string[] { "Player" };
                targetedLocs.AddRange(GridController.gridController.GetLocationsInAoE(casterPosition, card.radius, tags));
            }
            else
            {
                UnCast();
                transform.position = originalLocation;
                return new List<Vector2>();
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
                return new List<Vector2>();
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
                return new List<Vector2>();
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
            return new List<Vector2>();
        }

        return targetedLocs;
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

        if (!card.exhaust)
            StartCoroutine(UIController.ui.AnimateDiscardCardProcess(card, transform.position, transform.localScale));
        cardDisplay.Hide();

        yield return StartCoroutine(cardController.GetComponent<CardEffectsController>().TriggerEffect(cardController.FindCaster(cardController.GetCard()), locations));

        TurnController.turnController.ReportPlayedCard(card, cardController.GetNetEnergyCost(), cardController.GetNetManaCost(), cardController.GetEnergyCostDiscount(), cardController.GetManaCostDiscount(), cardController.GetEnergyCostCap(), cardController.GetManaCostCap());
        GameObject.FindGameObjectWithTag("Hand").GetComponent<HandController>().RemoveCard(cardController);

        try //Singleplayer
        {
            foreach (GameObject player in GameController.gameController.GetLivingPlayers())
                player.GetComponent<Collider2D>().enabled = true;
        }
        catch //Multiplayer
        {
            foreach (GameObject player in MultiplayerGameController.gameController.GetLivingPlayers())
                player.GetComponent<Collider2D>().enabled = true;
        }

        //If the card just cast is a resurrect card, revert it before shuffling it into the discard pile
        if (cardController.isResurrectCard)
        {
            cardController.isResurrectCard = false;
            cardController.ResetCard();
        }

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
