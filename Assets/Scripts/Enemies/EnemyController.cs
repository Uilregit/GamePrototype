using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class EnemyController : MonoBehaviour
{
    private int size;
    private List<Vector2> occupiedSpace;

    [Header("Stats Settings")]
    public int castRange = 1;
    public int maxVit;
    public bool vitVariate = true;
    public int startingArmor;
    public int attack;
    [SerializeField]
    private int moveRange;
    public int randomStartRange;
    public int attacksPerTurn = 1;

    [Header("Color Settings")]
    public Color moveRangeColor;
    public Color attackRangeColor;

    [Header("Attack Settings")]
    public List<Card> attackSequence;
    private List<CardController> attackSequenceControllers;

    public enum MoveType { Kite, Hug }
    public MoveType moveType;

    public enum TargetType { Nearest, Furthest, LowestVit, LowestArmor, HighestArmor, HighestVit, MostMissingHealth, RedPlayer, GreenPlayer, BluePlayer, Self, Default, HighestAttack, LowestAttack };
    public TargetType targetType = TargetType.Nearest;

    public bool isBoss = false;
    public string bossName = "";
    public string bossTitle = "";
    public Color bossSpriteBackColor;

    //public Card[] attackCards;

    //public SpriteRenderer sprite;
    //public SpriteRenderer shadow;

    private HealthController healthController;
    private EnemyInformationController enemyInformation;
    private SpriteRenderer spRenderer;
    //public SpriteRenderer outline;
    private Collider2D col2D;
    private bool isFacingRight = true;

    //private int tauntDuration = 0;
    //private GameObject tauntTarget;
    public GameObject[] desiredTarget;
    private bool sacrificed;

    private int currentAttackSquence = 0;
    protected int attackCardIndex = 0;

    private int amountMovedthisTurn = 0;

    private bool skipInitialIntent = false;

    private Vector2 previousPosition;

    // Start is called before the first frame update
    void Awake()
    {
        TurnController.turnController.ReportEnemy(this);

        col2D = GetComponent<Collider2D>();

        healthController = GetComponent<HealthController>();
        if (vitVariate)
        {
            maxVit = Mathf.RoundToInt(maxVit * (1 + Random.Range(-0.1f, 0.1f)));
            startingArmor += Random.Range(0, 2);
            if (attack != 0)
                attack += Random.Range(0, 2);
        }
        healthController.SetCastRange(castRange);
        healthController.SetMaxVit(maxVit);
        healthController.SetCurrentVit(maxVit);
        //healthController.SetMaxVit(1);
        //healthController.SetCurrentVit(1);
        healthController.SetStartingArmor(startingArmor);
        healthController.SetCurrentAttack(attack);
        healthController.SetMaxMoveRange(moveRange);
        healthController.SetCurrentMoveRange(moveRange, false);

        enemyInformation = GetComponent<EnemyInformationController>();

        desiredTarget = new GameObject[attacksPerTurn];
        currentAttackSquence = Random.Range(0, randomStartRange + 1) * attacksPerTurn;
        attackSequenceControllers = new List<CardController>();

        foreach (Card card in attackSequence) //Set the attack range indicator to be the highest range card
        {
            CardController temp = this.gameObject.AddComponent<CardController>();
            temp.SetCard(card, true, false);
            attackSequenceControllers.Add(temp); //Set the caster for the enemy cards for dynamic damage value display
        }

        size = GetComponent<HealthController>().size;
        occupiedSpace = GetComponent<HealthController>().GetOccupiedSpaces();
        spRenderer = healthController.charDisplay.sprite.GetComponent<SpriteRenderer>();
        previousPosition = transform.position;

        if (isBoss)
            GameController.gameController.SetBossInfo(healthController.charDisplay.sprite.sprite, bossName, bossTitle, bossSpriteBackColor);
    }

    private void Start()
    {
        if (skipInitialIntent)
            enemyInformation.HideIntent();
    }

    public void SetSkipInitialIntent(bool value)
    {
        skipInitialIntent = value;
    }

    public virtual void Spawn()
    {
        float x, y;
        for (int j = 0; j < 100; j++)
        {
            x = Random.Range(GameController.gameController.enemySpawnBox[0], GameController.gameController.enemySpawnBox[1] + 1);
            y = Random.Range(GameController.gameController.enemySpawnBox[2], GameController.gameController.enemySpawnBox[3] + 1);

            List<Vector2> locations = new List<Vector2>();
            foreach (Vector2 vec in occupiedSpace)
            {
                if (size % 2 == 0)
                    locations.Add(new Vector2(x + 0.5f, y + 0.5f) + vec);
                else
                    locations.Add(new Vector2(x, y) + vec);
            }

            if (!GridController.gridController.CheckIfOutOfBounds(locations))
                if (GridController.gridController.GetObjectAtLocation(locations).Count == 0)
                {
                    Spawn(new Vector2(x, y));
                    break;
                }
        }
        enemyInformation.DrawCards();
    }

    public virtual void Spawn(Vector2 location)
    {
        if (size % 2 == 0)
            transform.position = location + new Vector2(0.5f, 0.5f);
        else
            transform.position = location;
        foreach (Vector2 vec in occupiedSpace)
        {
            if (size % 2 == 0)
                GridController.gridController.ReportPosition(this.gameObject, location + new Vector2(0.5f, 0.5f) + vec);
            else
                GridController.gridController.ReportPosition(this.gameObject, location + vec);
        }
        transform.SetParent(GameObject.FindGameObjectWithTag("BoardCanvas").transform);

        enemyInformation.DrawCards();
    }

    //Moves, then if able to attack, attack
    public IEnumerator ExecuteTurn()
    {
        if (sacrificed)
            yield break;

        amountMovedthisTurn = 0;

        col2D.enabled = false;
        enemyInformation.OnMouseUp();
        healthController.charDisplay.outline.enabled = true;

        UIController.ui.combatStats.SetStatus(0, healthController, healthController.charDisplay.sprite.sprite, healthController.GetVit(), healthController.GetMaxVit(), 0, healthController.GetArmor(), 0, healthController.GetCurrentAttack(), healthController.GetBonusAttack(), healthController.GetCurrentMoveRange(), healthController.GetMaxMoveRange() + healthController.GetBonusMoveRange());
        UIController.ui.combatStats.SetStatusEnabled(0, true);

        for (int i = 0; i < attacksPerTurn; i++)
        {
            if (desiredTarget[i].GetComponent<HealthController>().GetIsDead())                  //In case the desired target died before the turn happened
                continue;

            yield return new WaitForSeconds(TimeController.time.enemyMoveStepTime * TimeController.time.timerMultiplier);

            attackCardIndex = currentAttackSquence % attackSequence.Count; //Chose the card to attack with

            if (!healthController.GetStunned() && GetComponent<HealthController>().GetCurrentVit() > 0)
            {
                //move towards target specified by TargetType. If not last attack, use minimized movement
                yield return StartCoroutine(Move(desiredTarget[i], i != attacksPerTurn - 1));

                if (healthController.GetStunned()) //In case that it is stunned while moving, stop turn
                {
                    healthController.charDisplay.outline.enabled = false;
                    yield break;
                }

                //Generate the targeted locations for the queued attack
                List<Vector2> targetLocs = new List<Vector2>();
                if (attackSequence[attackCardIndex].cardEffectName.Any(x => x == Card.EffectType.GravityEffect))
                {
                    targetLocs.AddRange(FindBestGravityLocation(desiredTarget[currentAttackSquence % attacksPerTurn], attackSequence[attackCardIndex].radius));
                }
                else if (attackSequence[attackCardIndex].castType == Card.CastType.AoE)
                    foreach (Vector2 loc in occupiedSpace)
                        targetLocs.AddRange(GridController.gridController.GetLocationsInAoE(GridController.gridController.GetRoundedVector((Vector2)transform.position + loc, 1), attackSequence[attackCardIndex].radius, new string[] { "All" }));
                else if (attackSequence[attackCardIndex].castType == Card.CastType.Player ||
                         attackSequence[attackCardIndex].castType == Card.CastType.Enemy)
                {
                    if (GetDistanceFrom(desiredTarget[i].transform.position) <= healthController.GetTotalCastRange())
                        targetLocs.Add(desiredTarget[i].transform.position);
                    else if (attackSequence[attackCardIndex].castType == Card.CastType.Player)
                    {
                        int attack = enemyInformation.displayedCards[i].GetComponent<CardController>().GetSimulatedTotalAttackValue(i);
                        ScoreController.score.UpdateDamageAvoided(attack);
                    }
                    /*
                    else if (GetDistanceFrom(GetNearest(attackSequence[attackCardIndex].castType)) <= attackSequence[attackCardIndex].range &&
                        tauntDuration <= 0) //If can't attack target, then attack nearest instead
                        targetLocs.Add(GetNearest(attackSequence[attackCardIndex].castType));
                        */
                }

                targetLocs = targetLocs.Distinct().ToList();
                yield return StartCoroutine(Attack(i, targetLocs));
            }
            else
            {
                yield return new WaitForSeconds(TimeController.time.enemyStunnedTurnTime * TimeController.time.timerMultiplier);
            }
            currentAttackSquence += 1;

            yield return new WaitForSeconds(TimeController.time.enemyMoveStepTime * TimeController.time.timerMultiplier);
        }

        healthController.charDisplay.outline.enabled = false;
        col2D.enabled = true;

        enemyInformation.HideIntent();
    }

    public GameObject GetCurrentTarget()
    {
        if (healthController.GetTauntedTarget() != null)          //Taunt target
            return healthController.GetTauntedTarget().gameObject;
        return desiredTarget[currentAttackSquence % attacksPerTurn];
    }

    public GameObject[] GetTargetArray()
    {
        GameObject[] target = new GameObject[attacksPerTurn];
        try
        {
            if (healthController.GetTauntedTarget() != null && healthController.GetTauntedTarget().GetComponent<HealthController>().GetCurrentVit() > 0)          //Taunt target
                for (int i = 0; i < attacksPerTurn; i++)
                    target[i] = healthController.GetTauntedTarget().gameObject;
            else
                for (int i = 0; i < attacksPerTurn; i++)
                    target[i] = GridController.gridController.GetObjectAtLocation(GetCardTarget(attackSequence[attackCardIndex + i].castType, attackCardIndex + i))[0];                     //Get the target according to the card in question
        }
        catch
        {
            //If normal tareting fails, target the nearest player as default
            for (int i = 0; i < attacksPerTurn; i++)
                target[i] = GridController.gridController.GetObjectAtLocation(GetNearest(Card.CastType.Player))[0];
        }
        return target;
    }

    //Gets the target specified by the card's target type
    private Vector2 GetCardTarget(Card.CastType type, int index)
    {
        TargetType currentTargetType = targetType;
        if (attackSequence[attackCardIndex].targetBehaviour != TargetType.Default)
            currentTargetType = attackSequence[index].targetBehaviour;
        //If the card cast type is ambiguous, use the target type instead
        if (type == Card.CastType.AoE || type == Card.CastType.TargetedAoE || type == Card.CastType.Any)
        {
            switch (attackSequence[index].targetType[0])
            {
                case Card.TargetType.Player:
                    type = Card.CastType.Player;
                    break;
                case Card.TargetType.AllPlayers:
                    type = Card.CastType.Player;
                    break;
                case Card.TargetType.Enemy:
                    type = Card.CastType.Enemy;
                    break;
                case Card.TargetType.AllEnemies:
                    type = Card.CastType.Enemy;
                    break;
                case Card.TargetType.Self:
                    type = Card.CastType.Enemy;
                    break;
            }
        }

        if (currentTargetType == TargetType.Self)
            return transform.position;
        else if (currentTargetType == TargetType.Nearest)
            return GetNearest(type);
        else if (currentTargetType == TargetType.Furthest)
            return GetFurthest(type);
        else if (currentTargetType == TargetType.HighestArmor)
            return GetHighestArmor(type);
        else if (currentTargetType == TargetType.LowestArmor)
            return GetLowestArmor(type);
        else if (currentTargetType == TargetType.HighestVit)
            return GetHighestVit(type);
        else if (currentTargetType == TargetType.LowestVit)
            return GetLowestVit(type);
        else if (currentTargetType == TargetType.MostMissingHealth)
            return GetMostMissingHealth(type);
        else if (currentTargetType == TargetType.HighestAttack)
            return GetHighestAttack(type);
        else if (currentTargetType == TargetType.LowestAttack)
            return GetLowestAttack(type);

        return GetNearest(type);
    }

    //Gets a list of pathfinding locations and step in one position after another to the final destination
    private IEnumerator Move(GameObject target, bool minimizeMovement)
    {
        healthController.charDisplay.charAnimController.SetRunning(true);

        if (target == this.gameObject && !minimizeMovement) //If targeting self, still move towards the nearest player
            target = GridController.gridController.GetObjectAtLocation(GetNearest(Card.CastType.Player))[0];

        foreach (Vector2 vec in occupiedSpace)
            GridController.gridController.RemoveFromPosition(this.gameObject, (Vector2)transform.position + vec);

        List<Vector2> traveledPath = new List<Vector2>();
        if (attackSequence[attackCardIndex].cardEffectName.Any(x => x == Card.EffectType.ForcedMovement))           //Movement for if the attack card has a forced movement effect
        {
            int index = 0;
            for (int i = 0; i < attackSequence[attackCardIndex].cardEffectName.Length; i++)
                if (attackSequence[attackCardIndex].cardEffectName[i] == Card.EffectType.ForcedMovement)
                {
                    index = i;
                    break;
                }
            traveledPath = FindBestKnockbackFromPath(target, attackSequence[attackCardIndex].effectValue[index], healthController.GetTotalCastRange());
        }
        else if (attackSequence[attackCardIndex].castType == Card.CastType.AoE)                                     //Movement for if the attack card is AoE cast
            traveledPath = FindMostInAoE(target);
        else                                                                                                        //Default movement path
            traveledPath = FindPath(target, minimizeMovement, healthController.GetTotalCastRange());

        if (InformationLogger.infoLogger.debug)
            foreach (Vector2 loc in traveledPath)
                TileCreator.tileCreator.CreateTiles(this.gameObject, loc, Card.CastShape.Circle, 0, Color.yellow, 2);

        traveledPath = traveledPath.GetRange(1, Mathf.Min(traveledPath.Count - 1, moveRange + GetComponent<HealthController>().GetBonusMoveRange() - amountMovedthisTurn)); //Leave at least 1 space so it never paths ON the target

        BuffController buffController = GetComponent<BuffController>();
        foreach (Vector2 position in traveledPath)
        {
            if (healthController.GetStunned() ||   //In case that it is stunned while moving, stop turn
                (GridController.gridController.GetObjectAtLocation(position).Count != 0 && !healthController.GetPhasedMovement()))  //If position is filled and movement isn't phased, stop turn
            {
                foreach (Vector2 vec in occupiedSpace)
                    GridController.gridController.ReportPosition(this.gameObject, (Vector2)transform.position + vec);
                healthController.charDisplay.charAnimController.SetRunning(false);
                yield break;
            }

            CameraController.camera.ScreenShake(0.06f, 0.05f);

            healthController.charDisplay.onHitSoundController.PlayFootStepSound();
            yield return StartCoroutine(MoveLerp(transform.position, position, TimeController.time.enemyMoveStepTime * TimeController.time.timerMultiplier * 0.7f));

            StartCoroutine(buffController.TriggerBuff(Buff.TriggerType.OnMove, healthController, 1));
            AchievementSystem.achieve.OnNotify(1, StoryRoomSetup.ChallengeType.EnemiesTravelLessThanXSpaces);

            amountMovedthisTurn++;
            healthController.SetCurrentPathLength(amountMovedthisTurn);
            UIController.ui.combatStats.SetStatus(0, healthController, healthController.charDisplay.sprite.sprite, healthController.GetVit(), healthController.GetMaxVit(), 0, healthController.GetArmor(), 0, healthController.GetCurrentAttack(), healthController.GetBonusAttack(), healthController.GetMoveRangeLeft(), healthController.GetMaxMoveRange() + healthController.GetBonusMoveRange());
            UIController.ui.combatStats.SetStatusEnabled(0, true);
            //yield return new WaitForSeconds(TimeController.time.enemyMoveStepTime * TimeController.time.timerMultiplier);
        }

        foreach (Vector2 vec in occupiedSpace)
            GridController.gridController.ReportPosition(this.gameObject, (Vector2)transform.position + vec);

        healthController.charDisplay.charAnimController.SetRunning(false);

        if (InformationLogger.infoLogger.debug)
            TileCreator.tileCreator.DestroyTiles(this.gameObject);

        yield return new WaitForSeconds(0);
    }

    private IEnumerator MoveLerp(Vector2 start, Vector2 end, float duration)
    {
        previousPosition = transform.position;
        if (end.x > start.x && !isFacingRight)
            FlipSpriteX();
        else if (end.x < start.x && isFacingRight)
            FlipSpriteX();

        float elapsedTime = 0;
        while (elapsedTime < duration)
        {
            transform.position = Vector2.Lerp(start, end, Mathf.Pow(elapsedTime, 1.5f) / Mathf.Pow(duration, 1.5f));
            elapsedTime += Time.deltaTime;

            yield return null;
        }
        transform.position = end;   //Always move to the rounded vector2 to prevent floating point errors in the lerp
        yield return new WaitForSeconds(TimeController.time.enemyMoveStepTime * TimeController.time.timerMultiplier * 0.3f);
    }

    //Show the attack card and target line, then trigger the attack card
    private IEnumerator Attack(int displayCardIndex, List<Vector2> target)
    {
        if (target.Count > 0)
        {
            GameController.gameController.SetCircleOverlay(true, transform.position);

            if (target[0].x > transform.position.x && !isFacingRight)
                FlipSpriteX();
            else if (target[0].x < transform.position.x && isFacingRight)
                FlipSpriteX();

            //enemyInformation.ShowUsedCard(attackSequenceControllers[attackCardIndex], transform.position);
            enemyInformation.ShowUsedCard(attackSequenceControllers[attackCardIndex], target[0]);
            enemyInformation.ShowTargetLine(target[0]);
            attackSequence[attackCardIndex].SetCenter(target[0]);

            bool castable = true;
            if (enemyInformation.displayedCards[displayCardIndex].GetComponent<CardController>().GetCard().manaCost > 0 && healthController.GetSilenced() ||
                enemyInformation.displayedCards[displayCardIndex].GetComponent<CardController>().GetCard().manaCost == 0 && healthController.GetDisarmed())
            {
                enemyInformation.GreyOutUsedCard();
                castable = false;
            }

            HealthController simulation = GameController.gameController.GetSimulationCharacter(desiredTarget[0].GetComponent<HealthController>());
            HealthController simulatedSelf = GameController.gameController.GetSimulationCharacter(healthController);
            yield return StartCoroutine(enemyInformation.GetCardController(displayCardIndex).GetComponent<CardEffectsController>().TriggerEffect(this.gameObject, new List<GameObject> { simulation.gameObject }, new List<Vector2> { desiredTarget[0].transform.position }, false, true, simulatedSelf.gameObject));
            HealthController displaySim = simulation;
            if (desiredTarget[0] == this.gameObject)
                displaySim = simulatedSelf;
            displaySim.SetCombatStatsHighlight(1, desiredTarget[0].GetComponent<HealthController>().GetVit() - displaySim.GetVit(), desiredTarget[0].GetComponent<HealthController>().GetArmor() - displaySim.GetArmor(), desiredTarget[0].GetComponent<HealthController>());
            if (desiredTarget[0].GetComponent<HealthController>().GetVit() - displaySim.GetVit() != 0)
                UIController.ui.combatStats.SetArrow(desiredTarget[0].GetComponent<HealthController>().GetVit() - displaySim.GetVit(), CombatStatsHighlightController.numberType.number);
            else if (enemyInformation.GetCardController(displayCardIndex).GetCard().cardEffectName.Any(x => x == Card.EffectType.Buff))
            {
                for (int i = 0; i < enemyInformation.GetCardController(displayCardIndex).GetCard().cardEffectName.Length; i++)
                    if (enemyInformation.GetCardController(displayCardIndex).GetCard().cardEffectName[i] == Card.EffectType.Buff)
                        UIController.ui.combatStats.SetArrow(enemyInformation.GetCardController(displayCardIndex).GetCard().effectDuration[i], CombatStatsHighlightController.numberType.turn, enemyInformation.GetCardController(displayCardIndex).GetCard().buff[i].description.Substring(0, enemyInformation.GetCardController(displayCardIndex).GetCard().buff[i].description.IndexOf(":")));
            }
            else
                UIController.ui.combatStats.SetArrow(0, CombatStatsHighlightController.numberType.number);
            UIController.ui.combatStats.SetStatusCharactersCount(1, target.Count);
            UIController.ui.combatStats.SetDamageArrowEnabled(true);

            yield return new WaitForSeconds(TimeController.time.enemyAttackCardHangTime * TimeController.time.timerMultiplier);
            enemyInformation.DestroyUsedCard();

            if (castable)
                yield return StartCoroutine(enemyInformation.TriggerCard(displayCardIndex, target));

            if (desiredTarget[0] == this.gameObject)
                UIController.ui.combatStats.SetStatus(0, healthController, healthController.charDisplay.sprite.sprite, healthController.GetVit(), healthController.GetMaxVit(), 0, healthController.GetArmor(), 0, healthController.GetCurrentAttack(), healthController.GetBonusAttack(), healthController.GetMoveRangeLeft(), healthController.GetMaxMoveRange() + healthController.GetBonusMoveRange());

            TileCreator.tileCreator.DestroyTiles(this.gameObject, 0);

            GameController.gameController.SetCircleOverlay(false, transform.position);

            UIController.ui.combatStats.SetStatusEnabled(1, false);
            UIController.ui.combatStats.SetDamageArrowEnabled(false);
            GameController.gameController.ReportSimulationFinished(simulation);
            GameController.gameController.ReportSimulationFinished(simulatedSelf);
        }
    }

    //Find best cast location for gravity effect
    private List<Vector2> FindBestGravityLocation(GameObject target, int radius)
    {
        TileCreator.tileCreator.CreateTiles(this.gameObject, target.transform.position, Card.CastShape.Circle, radius, Color.clear, 2);
        TileCreator.tileCreator.CreateTiles(this.gameObject, target.transform.position, Card.CastShape.Circle, radius, Color.black, 1);
        List<Vector2> viableLocs = TileCreator.tileCreator.GetTilePositions(2);
        TileCreator.tileCreator.DestroyTiles(this.gameObject, 2);

        List<TrapController> nearbyTraps = new List<TrapController>();
        foreach (TrapController t in GridController.gridController.traps)
            if (viableLocs.Contains(t.transform.position))
                nearbyTraps.Add(t);

        List<GameObject> nearbyPlayers = GridController.gridController.GetObjectsInAoE(target.transform.position, radius + 1, new string[] { target.tag }); //+1 to radius to include players just outside of range

        Vector2 finalLocation = target.transform.position;      //Default to not moving the player

        if (nearbyPlayers.Count > 1)        //Prioritize knocking players into eachother
        {
            List<Vector2> viablePositions = new List<Vector2>();
            foreach (GameObject obj in nearbyPlayers)
            {
                TileCreator.tileCreator.CreateTiles(this.gameObject, obj.transform.position, attackSequence[attackCardIndex].castShape, attackSequence[attackCardIndex].radius, Color.clear, 2);
                List<Vector2> attackFromLocation = TileCreator.tileCreator.GetTilePositions(2);
                TileCreator.tileCreator.DestroyTiles(this.gameObject, 2);

                foreach (Vector2 loc in attackFromLocation)
                    if (viableLocs.Contains(loc))
                        viablePositions.Add(loc);
            }

            //Return the position that can attack the most targets
            if (viablePositions.Count == 0)
                finalLocation = target.transform.position;
            else
                finalLocation = viablePositions.GroupBy(x => x).OrderByDescending(grp => grp.Count()).Select(grp => grp.Key).First();
        }
        else if (nearbyTraps.Count > 0)                         //Else prioritize knocking players into traps
        {
            finalLocation = nearbyTraps[0].transform.position;
            int maxTrappablePlayers = 0;
            foreach (TrapController t in nearbyTraps)
            {
                int playerInTrapAoE = GridController.gridController.GetObjectsInAoE(t.transform.position, radius, new string[] { target.tag }).Count;
                if (playerInTrapAoE > maxTrappablePlayers)
                {
                    maxTrappablePlayers = playerInTrapAoE;
                    finalLocation = t.transform.position;
                }
            }
        }

        List<Vector2> output = new List<Vector2>();
        output.Add(finalLocation);
        List<Vector2> surroundingLocs = GridController.gridController.GetLocationsInAoE(finalLocation, radius, GetTargetTags(attackSequence[attackCardIndex].castType, attackSequence[attackCardIndex].targetType[0]));
        surroundingLocs.Remove(finalLocation);
        output.AddRange(surroundingLocs);       //Ensure final location is at position index 0 for CardEffectController
        return output;
    }

    //Finds the best movement path for knocking players either onto eachother or into traps
    private List<Vector2> FindBestKnockbackFromPath(GameObject target, int knockbackDistance, int castRange)
    {
        List<Vector2> moveablePositions = GetMoveablePositions();
        TileCreator.tileCreator.CreateTiles(this.gameObject, target.transform.position, Card.CastShape.Plus, castRange, Color.clear, 2);
        List<Vector2> viablePositions = moveablePositions.Intersect(TileCreator.tileCreator.GetTilePositions(2)).ToList();
        TileCreator.tileCreator.DestroyTiles(this.gameObject);

        Vector2 maxLoc = Vector2.zero;
        int maxOverlap = 0;

        List<Vector2> knockToTrapLocs = new List<Vector2>();

        foreach (Vector2 loc in viablePositions)
        {
            if (GridController.gridController.GetObjectAtLocation(loc).Count != 0)      //Check if moved to location is empty in case a previous enemy already moved there
                continue;

            int overlap = 0;
            for (int i = 1; i <= knockbackDistance; i++)
            {
                Vector2 knockedToPosition = ((Vector2)target.transform.position - loc).normalized * i + (Vector2)target.transform.position;
                if (!GridController.gridController.CheckIfOutOfBounds(knockedToPosition))
                    overlap += GridController.gridController.GetObjectAtLocation(knockedToPosition, new string[] { target.tag }).Count;

                if (i == knockbackDistance && GridController.gridController.traps.Any(x => (Vector2)x.transform.position == loc))
                    knockToTrapLocs.Add(loc);
            }
            if (overlap > maxOverlap)
            {
                maxOverlap = overlap;
                maxLoc = loc;
            }
        }
        string[] pathThroughTags = new string[0];
        if (!healthController.GetPhasedMovement())
            pathThroughTags = new string[] { "None" };
        else
            pathThroughTags = new string[] { "Player", "Enemy", "Blockade" };

        Vector2 finalLoc = Vector2.zero;
        if (maxOverlap > 0)                         //Prioritize overlaping targets over knocking to traps
            finalLoc = maxLoc;
        else if (knockToTrapLocs.Count != 0)        //If can't overlap, try knocking into traps
            finalLoc = knockToTrapLocs[Random.Range(0, knockToTrapLocs.Count - 1)];
        else                                        //Last option knock to a random location
        {
            if (viablePositions.Count > 0)
                finalLoc = viablePositions[Random.Range(0, viablePositions.Count - 1)];
            else
            {
                List<Vector2> normalPath = FindFurthestPointInRange(target, pathThroughTags);
                finalLoc = normalPath[normalPath.Count - 1];
            }
        }
        return PathFindController.pathFinder.PathFind(transform.position, finalLoc, pathThroughTags, occupiedSpace, size);
    }

    //Will move to the location to get the most amount of targets in the AoE range
    protected virtual List<Vector2> FindMostInAoE(GameObject target)
    {
        List<Vector2> moveablePositions = GetMoveablePositions();

        //Get all the possible targets
        string[] tags = GetTargetTags(attackSequence[attackCardIndex].castType, attackSequence[attackCardIndex].targetType[0]);

        List<GameObject> possibleTargets = new List<GameObject>();
        if (tags.Contains("Player"))
            possibleTargets.AddRange(GameController.gameController.GetLivingPlayers());
        if (tags.Contains("Enemy"))
        {
            possibleTargets.AddRange(TurnController.turnController.GetEnemies().Select(x => x.gameObject));
            possibleTargets.AddRange(TurnController.turnController.GetQueuedEnemies().Select(x => x.gameObject));   //In case the previous card spawned enemies
            possibleTargets.Remove(this.gameObject);
        }

        //If a pathable position can attack a target, add it to the list. repeat for each target to get counts of how many targets that position can attack
        List<Vector2> viablePositions = new List<Vector2>();

        //If desired target is a single player, then always ensure that player is included in the AoE
        List<GameObject> targets = possibleTargets;
        if (desiredTarget[currentAttackSquence % attacksPerTurn].tag == "Player")
        {
            targets.Add(desiredTarget[currentAttackSquence % attacksPerTurn]);
            targets = targets.Distinct().ToList();
        }

        foreach (GameObject obj in targets)
        {
            TileCreator.tileCreator.CreateTiles(this.gameObject, obj.transform.position, attackSequence[attackCardIndex].castShape, attackSequence[attackCardIndex].radius, Color.clear, 2);
            List<Vector2> attackFromLocation = TileCreator.tileCreator.GetTilePositions(2);
            TileCreator.tileCreator.DestroyTiles(this.gameObject, 2);

            foreach (Vector2 loc in attackFromLocation)
                if (moveablePositions.Contains(loc))
                    viablePositions.Add(loc);
        }

        //Return the position that can attack the most targets
        Vector2 finalLocation;

        List<Vector2> traplessPositions = new List<Vector2>();                                  //Avoid traps if possible
        foreach (Vector2 loc in viablePositions)
            if (!GridController.gridController.traps.Any(x => (Vector2)x.transform.position == loc))
                traplessPositions.Add(loc);
        if (traplessPositions.Count != 0)
            viablePositions = traplessPositions;

        if (viablePositions.Count == 0)
            finalLocation = desiredTarget[currentAttackSquence % attacksPerTurn].transform.position;
        else
        {
            //Find the list of all locations tied for getting the most targets in AoE
            Dictionary<Vector2, int> counts = new Dictionary<Vector2, int>();
            foreach (Vector2 loc in viablePositions)
                if (counts.ContainsKey(loc))
                    counts[loc] += 1;
                else
                    counts[loc] = 1;
            Dictionary<int, List<Vector2>> c = new Dictionary<int, List<Vector2>>();
            foreach (Vector2 loc in counts.Keys)
                if (c.ContainsKey(counts[loc]))
                    c[counts[loc]].Add(loc);
                else
                    c[counts[loc]] = new List<Vector2>() { loc };
            viablePositions = c[c.Keys.Max()];
            //Find the location that's the furthest away from the starting location
            int maxDistance = -1;
            finalLocation = viablePositions[0];
            foreach (Vector2 loc in viablePositions)
                if (GridController.gridController.GetManhattanDistance(loc, transform.position) > maxDistance)
                {
                    maxDistance = GridController.gridController.GetManhattanDistance(loc, transform.position);
                    finalLocation = loc;
                }
        }
        string[] pathThroughTags = new string[0];
        if (!healthController.GetPhasedMovement())
            pathThroughTags = new string[] { "None" };
        else
            pathThroughTags = new string[] { "Player", "Enemy", "Blockade" };

        return PathFindController.pathFinder.PathFind(transform.position, finalLocation, pathThroughTags, occupiedSpace, size);
    }

    private List<Vector2> GetMoveablePositions()
    {
        string[] avoidTags = new string[0];
        if (!healthController.GetPhasedMovement())
            avoidTags = new string[] { "Player", "Enemy", "Blockade" };
        else
            avoidTags = new string[] { };

        //First find all positions that can actually be pathed to
        foreach (Vector2 vec in GetComponent<HealthController>().GetOccupiedSpaces())
            TileCreator.tileCreator.CreateTiles(this.gameObject, (Vector2)transform.position + vec, Card.CastShape.Circle, moveRange + GetComponent<HealthController>().GetBonusMoveRange() - amountMovedthisTurn, Color.clear, avoidTags, 2);
        if (healthController.GetPhasedMovement())
        {
            List<Vector2> destroyLocs = new List<Vector2>();
            foreach (Vector2 loc in TileCreator.tileCreator.GetTilePositions(2))
                if (GridController.gridController.GetObjectAtLocation(loc).Count != 0)
                    destroyLocs.Add(loc);
            TileCreator.tileCreator.DestroySpecificTiles(this.gameObject, destroyLocs, 2);
        }
        List<Vector2> moveablePositions = TileCreator.tileCreator.GetTilePositions(2);
        TileCreator.tileCreator.DestroyTiles(this.gameObject, 2);

        return moveablePositions;
    }

    //Used by turn controller to determine which enemy should move first to avoid blocking issues. Returns the number of steps needed to path to the target, 1000 + phased path if currently blocked
    //Lower the sorting order, the earlier this enemy should get to go
    public int FindPathSortingOrder(GameObject target)
    {
        string[] pathThroughTags = new string[0];
        if (!healthController.GetPhasedMovement())
            pathThroughTags = new string[] { "None" };
        else
            pathThroughTags = new string[] { "Player", "Enemy", "Blockade" };

        int output = FindFurthestPointInRange(target, pathThroughTags).Count;

        if (output == 1)
        {
            output = 10000;                         //If unpathable, the lowest sorting order possible will be 10000 to ensure non blocked enemies will go first
            List<Vector2> path = PathFindController.pathFinder.PathFind(transform.position, target.transform.position, new string[] { "Player", "Enemy" }, occupiedSpace, size);
            foreach (Vector2 loc in path)
                if (GridController.gridController.GetObjectAtLocation(loc, new string[] { "Enemy" }).Count > 0)         //Only count enemy in penialities so player body blocking is not taken into account
                    output += 1000;                 //Penialize enemies for each character in their way to ensure those with less in their way goes first
            output += path.Count;

        }

        return output;
    }

    //Find the closest point to the target with a kite distance using the return of pathinfinding
    protected virtual List<Vector2> FindPath(GameObject target, bool minimizeMovement, int kiteDistance = 0)
    {
        //If hug move type, never have any kite distance
        if (moveType == MoveType.Hug)
            kiteDistance = 0;

        string[] pathThroughTags = new string[0];
        if (!healthController.GetPhasedMovement())
            pathThroughTags = new string[] { "None" };
        else
            pathThroughTags = new string[] { "Player", "Enemy", "Blockade" };

        List<Vector2> output = new List<Vector2>();
        //If minimizing movement, just path to target instead
        if (minimizeMovement)
        {
            output = FindFurthestPointInRange(target, pathThroughTags, true);
            /*
            output = PathFindController.pathFinder.PathFind(this.transform.position, target.transform.position, pathThroughTags, occupiedSpace);
            output = output.GetRange(0, output.Count - 1);   //Doesn't include the last location, which is where the target is, to be consistent with FindFurthestPointInRange
            output = output.GetRange(0, Mathf.Clamp(output.Count - kiteDistance + 1, 1, output.Count));     //If pathing directly to target, kite for the distance specified. +1 so 1 cast range chars can still hit
            */
        }
        else
            output = FindFurthestPointInRange(target, pathThroughTags);

        if (output.Count == 1 && target != this.gameObject && !minimizeMovement)  //If a path could not be found, find path as if other enemies don't exist to avoid paths being blocked by enemies causing unpathable enemies to stay still
        {
            List<Vector2> path = PathFindController.pathFinder.PathFind(transform.position, target.transform.position, new string[] { "Enemy" }, occupiedSpace, size);
            if (InformationLogger.infoLogger.debug)
                foreach (Vector2 loc in path)
                    TileCreator.tileCreator.CreateTiles(this.gameObject, loc, Card.CastShape.Circle, 0, Color.blue, 0);
            int bonuseMoveRange = GetComponent<HealthController>().GetBonusMoveRange();
            output = path.GetRange(0, Mathf.Min(moveRange + bonuseMoveRange + 1, path.Count));


            if (output.Count == 1 && target != this.gameObject)  //If a path still could not be found, find path as if player and enemies don't exist to avoid paths being blocked by enemies causing unpathable enemies to stay still
            {
                path = PathFindController.pathFinder.PathFind(transform.position, target.transform.position, new string[] { "Player", "Enemy" }, occupiedSpace, size);
                if (InformationLogger.infoLogger.debug)
                    foreach (Vector2 loc in path)
                        TileCreator.tileCreator.CreateTiles(this.gameObject, loc, Card.CastShape.Circle, 0, Color.blue, 0);
                bonuseMoveRange = GetComponent<HealthController>().GetBonusMoveRange();
                output = path.GetRange(0, Mathf.Min(moveRange + bonuseMoveRange + 1, path.Count));
            }
        }

        TileCreator.tileCreator.DestroyTiles(this.gameObject, 1);
        if (InformationLogger.infoLogger.debug)
            foreach (Vector2 loc in output)
                TileCreator.tileCreator.CreateTiles(this.gameObject, loc, Card.CastShape.Circle, 0, Color.red, 1);

        return output;
    }

    protected virtual List<Vector2> FindFurthestPointInRange(GameObject target, string[] pathThroughTags, bool findClosestPointInstead = false)
    {
        TileCreator.tileCreator.CreateTiles(this.gameObject, target.transform.position, attackSequence[attackCardIndex].castShape, healthController.GetTotalCastRange(), Color.clear, 2);

        List<Vector2> inRangeLocations = TileCreator.tileCreator.GetTilePositions(2);
        List<Vector2> emptyInRangeLocations = new List<Vector2>();
        List<Vector2> emptyAndTraplessLocations = new List<Vector2>();
        foreach (Vector2 loc in inRangeLocations)                                              //Find all locations that are in range and empty
            if (GridController.gridController.GetObjectAtLocation(loc).Count == 0 && !GridController.gridController.GetPathBlocked(loc))
            {
                emptyInRangeLocations.Add(loc);
                if (!GridController.gridController.traps.Any(x => (Vector2)x.transform.position == loc))    //Also calculates trapless locations in case traps can be avoided
                    emptyAndTraplessLocations.Add(loc);
            }
        inRangeLocations = emptyInRangeLocations;
        if (emptyAndTraplessLocations.Count != 0)                                               //If traps can be avoided, then avoid traps
            inRangeLocations = emptyAndTraplessLocations;

        TileCreator.tileCreator.DestroyTiles(this.gameObject, 2);                              //Get all positions that can cast on the target

        List<Vector2>[] locationsByDistance = new List<Vector2>[Mathf.Max(healthController.GetTotalCastRange() + 1, 1)];      //Sort positions by distance to target, +1 for initializing locationsByDistance
        for (int i = Mathf.Max(healthController.GetTotalCastRange(), 0); i >= 0; i--)
            locationsByDistance[i] = new List<Vector2>();

        foreach (Vector2 loc in inRangeLocations)
            try
            {
                locationsByDistance[GetManhattanDistance(loc, target.transform.position)].Add(loc);
            }
            catch
            {
                Debug.Log("######### ERROR ########");
                Debug.Log(loc);
                Debug.Log(target.transform.position);
                Debug.Log(GetManhattanDistance(loc, target.transform.position) - 1);
                Debug.Log(locationsByDistance.Length);
                Debug.Log(healthController.GetTotalCastRange());
                Debug.Log(Mathf.Max(healthController.GetTotalCastRange(), 1));
            }
        List<Vector2> pathableLocations = new List<Vector2>();                                              //Check if pathable within the allocated move distance or not
        for (int i = Mathf.Max(healthController.GetTotalCastRange(), 0); i >= 0; i--)                                  //Prefer locations further from target if possible
        {
            pathableLocations = new List<Vector2>();
            foreach (Vector2 loc in locationsByDistance[i])
            {
                List<Vector2> path = new List<Vector2>();
                path = PathFindController.pathFinder.PathFind(transform.position, loc, pathThroughTags, occupiedSpace, size);
                //if (path.Count - 1 <= moveRange + GetComponent<HealthController>().GetBonusMoveRange())
                if (path.Count > 1 && path.Count - 1 <= moveRange + GetComponent<HealthController>().GetBonusMoveRange() - amountMovedthisTurn)
                    pathableLocations.Add(loc);
            }
            if (pathableLocations.Count > 0)                                                                //If there are pathable locations in this distance range, move on to the next step
                break;
        }
        Vector2 desiredTargetLocation = new Vector2();
        if (desiredTarget != null)                                                                          //In case the desired target has died/been destroyed
            desiredTargetLocation = desiredTarget[currentAttackSquence % attacksPerTurn].transform.position;
        else
            desiredTargetLocation = GetCurrentTarget().transform.position;

        List<Vector2> finalLocs = new List<Vector2>() { desiredTargetLocation };                            //Find the viable position that's the furthest from the current position, default being the target's location
        if (healthController.GetPhasedMovement() && pathableLocations.Count == 0)                           //If phased movement, and can't find pathable locations, stay in place to avoid moving into blockades
            return new List<Vector2> { transform.position };

        foreach (Vector2 loc in pathableLocations)                                                          //Allows for more movement and more dynamic battles
        {
            if (desiredTargetLocation == (Vector2)target.transform.position)
            {
                if (GetManhattanDistance(loc, transform.position) > GetManhattanDistance(finalLocs[0], transform.position))
                    finalLocs = new List<Vector2>() { loc };
                else if (GetManhattanDistance(loc, transform.position) == GetManhattanDistance(finalLocs[0], transform.position))
                    finalLocs.Add(loc);
            }
            else                                                                                            //If the desired target isn't the target chosen, move to the location closes to the desired target while still attacking something
            {
                if (!findClosestPointInstead)
                {
                    if (GetManhattanDistance(loc, desiredTargetLocation) < GetManhattanDistance(finalLocs[0], desiredTargetLocation))
                        finalLocs = new List<Vector2>() { loc };
                    else if (GetManhattanDistance(loc, desiredTargetLocation) == GetManhattanDistance(finalLocs[0], desiredTargetLocation))
                        finalLocs.Add(loc);
                }
                else
                {
                    if (GetManhattanDistance(loc, desiredTargetLocation) > GetManhattanDistance(finalLocs[0], desiredTargetLocation))
                        finalLocs = new List<Vector2>() { loc };
                    else if (GetManhattanDistance(loc, desiredTargetLocation) == GetManhattanDistance(finalLocs[0], desiredTargetLocation))
                        finalLocs.Add(loc);
                }
            }
        }

        Vector2 finalLoc = finalLocs[Random.Range(0, finalLocs.Count)];                                                     //If there are multiple viable positions with the same distance, chose one at random
        return PathFindController.pathFinder.PathFind(transform.position, finalLoc, pathThroughTags, occupiedSpace, size);  //Return the pathfind to the chosen location
    }

    public void RefreshIntent()
    {
        if (GetComponent<HealthController>().GetCurrentVit() <= 0)
            return;
        attackCardIndex = currentAttackSquence % attackSequence.Count;
        desiredTarget = GetTargetArray();

        enemyInformation.SetCards();                //Ensure that the cards to be triggered are up to date

        List<CardController> currentCards = new List<CardController>();
        for (int i = 0; i < attacksPerTurn; i++)
            currentCards.Add(attackSequenceControllers[attackCardIndex + i]);
        //enemyInformation.ShowIntent(currentCards);
        enemyInformation.RefreshIntent();
    }

    public List<CardController> GetCard()
    {
        List<CardController> output = new List<CardController>();
        for (int i = 0; i < attacksPerTurn; i++)
            output.Add(attackSequenceControllers[(currentAttackSquence + i) % attackSequence.Count]);
        return output;
    }

    private string[] GetTargetTags(Card.CastType type, Card.TargetType firstTargetType)
    {
        string[] tags;
        switch (type)
        {
            case Card.CastType.All:
                tags = new string[] { "Enemy", "Player" };
                break;
            case Card.CastType.Any:
                tags = new string[] { "Enemy", "Player" };
                break;
            case Card.CastType.Enemy:
                tags = new string[] { "Enemy" };
                break;
            default:
                if (firstTargetType == Card.TargetType.Enemy)
                    tags = new string[] { "Enemy" };
                else
                    tags = new string[] { "Player" };
                break;
        }

        return tags;
    }

    private Vector2 GetNearest(Card.CastType type)
    {
        GameObject output = null;

        GameObject[] targets = GameController.gameController.GetLivingPlayers().ToArray();
        if (type == Card.CastType.Enemy || type == Card.CastType.TargetedAoE && attackSequence[attackCardIndex].targetType[0] == Card.TargetType.Enemy)
        {
            targets = new GameObject[TurnController.turnController.GetEnemies().Count];
            for (int i = 0; i < TurnController.turnController.GetEnemies().Count; i++)
                targets[i] = TurnController.turnController.GetEnemies()[i].gameObject;
        }

        output = targets[0];
        int distance = 9999999;
        foreach (GameObject target in targets)
        {
            if (GetDistanceFrom(target.transform.position) < distance && target.GetComponent<HealthController>().GetVit() > 0)
            {
                distance = GetDistanceFrom(target.transform.position);
                output = target;
            }
        }
        return output.transform.position;
    }

    private Vector2 GetFurthest(Card.CastType type)
    {
        GameObject output = null;

        GameObject[] targets = GameController.gameController.GetLivingPlayers().ToArray();
        if (type == Card.CastType.Enemy || type == Card.CastType.TargetedAoE && attackSequence[attackCardIndex].targetType[0] == Card.TargetType.Enemy)
        {
            targets = new GameObject[TurnController.turnController.GetEnemies().Count];
            for (int i = 0; i < TurnController.turnController.GetEnemies().Count; i++)
                targets[i] = TurnController.turnController.GetEnemies()[i].gameObject;
        }

        output = targets[0];
        int distance = -99999999;
        foreach (GameObject target in targets)
        {
            if (GetDistanceFrom(target.transform.position) > distance && target.GetComponent<HealthController>().GetVit() > 0)
            {
                distance = GetDistanceFrom(target.transform.position);
                output = target;
            }
        }
        return output.transform.position;
    }

    private Vector2 GetHighestArmor(Card.CastType type)
    {
        GameObject output = null;

        GameObject[] targets = GameController.gameController.GetLivingPlayers().ToArray();
        if (type == Card.CastType.Enemy || type == Card.CastType.TargetedAoE && attackSequence[attackCardIndex].targetType[0] == Card.TargetType.Enemy)
        {
            targets = new GameObject[TurnController.turnController.GetEnemies().Count];
            for (int i = 0; i < TurnController.turnController.GetEnemies().Count; i++)
                targets[i] = TurnController.turnController.GetEnemies()[i].gameObject;
        }

        output = targets[0];
        int armor = -999999999;
        foreach (GameObject target in targets)
        {
            if (target.GetComponent<HealthController>().GetArmor() > armor && target.GetComponent<HealthController>().GetVit() > 0)
            {
                armor = target.GetComponent<HealthController>().GetArmor();
                output = target;
            }
        }
        return output.transform.position;
    }

    private Vector2 GetLowestArmor(Card.CastType type)
    {
        GameObject output = null;

        GameObject[] targets = GameController.gameController.GetLivingPlayers().ToArray();
        if (type == Card.CastType.Enemy || type == Card.CastType.TargetedAoE && attackSequence[attackCardIndex].targetType[0] == Card.TargetType.Enemy)
        {
            targets = new GameObject[TurnController.turnController.GetEnemies().Count];
            for (int i = 0; i < TurnController.turnController.GetEnemies().Count; i++)
                targets[i] = TurnController.turnController.GetEnemies()[i].gameObject;
        }

        output = targets[0];
        int armor = 999999999;
        foreach (GameObject target in targets)
        {
            if (target.GetComponent<HealthController>().GetArmor() < armor && target.GetComponent<HealthController>().GetVit() > 0)
            {
                armor = target.GetComponent<HealthController>().GetArmor();
                output = target;
            }
        }
        return output.transform.position;
    }

    private Vector2 GetHighestVit(Card.CastType type)
    {
        GameObject output = null;

        GameObject[] targets = GameController.gameController.GetLivingPlayers().ToArray();
        if (type == Card.CastType.Enemy || type == Card.CastType.TargetedAoE && attackSequence[attackCardIndex].targetType[0] == Card.TargetType.Enemy)
        {
            targets = new GameObject[TurnController.turnController.GetEnemies().Count];
            for (int i = 0; i < TurnController.turnController.GetEnemies().Count; i++)
                targets[i] = TurnController.turnController.GetEnemies()[i].gameObject;
        }

        output = targets[0];
        int vit = 0;
        foreach (GameObject target in targets)
        {
            if (target.GetComponent<HealthController>().GetVit() > vit && target.GetComponent<HealthController>().GetVit() > 0)
            {
                vit = target.GetComponent<HealthController>().GetVit();
                output = target;
            }
        }
        return output.transform.position;
    }

    //Find the player with the lowest vitality
    private Vector2 GetLowestVit(Card.CastType type)
    {
        GameObject output = null;

        GameObject[] targets = GameController.gameController.GetLivingPlayers().ToArray();
        if (type == Card.CastType.Enemy || type == Card.CastType.TargetedAoE && attackSequence[attackCardIndex].targetType[0] == Card.TargetType.Enemy)
        {
            targets = new GameObject[TurnController.turnController.GetEnemies().Count];
            for (int i = 0; i < TurnController.turnController.GetEnemies().Count; i++)
                targets[i] = TurnController.turnController.GetEnemies()[i].gameObject;
        }

        output = targets[0];
        int vit = 999999999;
        foreach (GameObject target in targets)
        {
            if (target.GetComponent<HealthController>().GetVit() < vit && target.GetComponent<HealthController>().GetVit() > 0)
            {
                vit = target.GetComponent<HealthController>().GetVit();
                output = target;
            }
        }
        return output.transform.position;
    }

    private Vector2 GetMostMissingHealth(Card.CastType type)
    {
        GameObject output = null;

        GameObject[] targets = GameController.gameController.GetLivingPlayers().ToArray();
        if (type == Card.CastType.Enemy || type == Card.CastType.TargetedAoE && attackSequence[attackCardIndex].targetType[0] == Card.TargetType.Enemy)
        {
            targets = new GameObject[TurnController.turnController.GetEnemies().Count];
            for (int i = 0; i < TurnController.turnController.GetEnemies().Count; i++)
                targets[i] = TurnController.turnController.GetEnemies()[i].gameObject;
        }

        output = targets[0];
        int missingHealth = 0;
        foreach (GameObject target in targets)
        {
            if (target.GetComponent<HealthController>().GetMaxVit() - target.GetComponent<HealthController>().GetCurrentVit() > missingHealth)
            {
                missingHealth = target.GetComponent<HealthController>().GetMaxVit() - target.GetComponent<HealthController>().GetCurrentVit();
                output = target;
            }
        }
        return output.transform.position;
    }

    private Vector2 GetHighestAttack(Card.CastType type)
    {
        GameObject output = null;

        GameObject[] targets = GameController.gameController.GetLivingPlayers().ToArray();
        if (type == Card.CastType.Enemy || type == Card.CastType.TargetedAoE && attackSequence[attackCardIndex].targetType[0] == Card.TargetType.Enemy)
        {
            targets = new GameObject[TurnController.turnController.GetEnemies().Count];
            for (int i = 0; i < TurnController.turnController.GetEnemies().Count; i++)
                targets[i] = TurnController.turnController.GetEnemies()[i].gameObject;
        }

        output = targets[0];
        int highestAttack = 0;
        foreach (GameObject target in targets)
        {
            if (target.GetComponent<HealthController>().GetAttack() > highestAttack)
            {
                highestAttack = target.GetComponent<HealthController>().GetAttack();
                output = target;
            }
        }
        return output.transform.position;
    }

    private Vector2 GetLowestAttack(Card.CastType type)
    {
        GameObject output = null;

        GameObject[] targets = GameController.gameController.GetLivingPlayers().ToArray();
        if (type == Card.CastType.Enemy || type == Card.CastType.TargetedAoE && attackSequence[attackCardIndex].targetType[0] == Card.TargetType.Enemy)
        {
            targets = new GameObject[TurnController.turnController.GetEnemies().Count];
            for (int i = 0; i < TurnController.turnController.GetEnemies().Count; i++)
                targets[i] = TurnController.turnController.GetEnemies()[i].gameObject;
        }

        output = targets[0];
        int lowestAttack = 999999999;
        foreach (GameObject target in targets)
        {
            if (target.GetComponent<HealthController>().GetAttack() < lowestAttack)
            {
                lowestAttack = target.GetComponent<HealthController>().GetAttack();
                output = target;
            }
        }
        return output.transform.position;
    }

    private int GetManhattanDistance(Vector2 startingLoc, Vector2 endingLoc)
    {
        int output = 99999;
        foreach (Vector2 loc in occupiedSpace)
            if (GridController.gridController.GetManhattanDistance(startingLoc + loc, endingLoc) < output)
                output = GridController.gridController.GetManhattanDistance(startingLoc + loc, endingLoc);
        return output;
    }

    protected virtual int GetDistanceFrom(Vector2 target)
    {
        List<Vector2> occupiedLocations = new List<Vector2>();
        foreach (Vector2 vec in occupiedSpace)
            occupiedLocations.Add((Vector2)transform.position + vec);

        int distance = 999999;
        foreach (Vector2 location in occupiedLocations)
            if ((int)(Mathf.Abs(location.x - target.x) + Mathf.Abs(location.y - target.y)) < distance)
                distance = (int)(Mathf.Abs(location.x - target.x) + Mathf.Abs(location.y - target.y));

        return distance;
    }

    protected virtual void OnDestroy()
    {
        foreach (Vector2 vec in occupiedSpace)
            if (!sacrificed)
                GridController.gridController.RemoveFromPosition(this.gameObject, (Vector2)transform.position + vec);
        enemyInformation.DestroyUsedCard();
    }

    public void SetSacrificed(bool state)
    {
        sacrificed = true;
    }

    public bool GetSacrificed()
    {
        return sacrificed;
    }

    public int GetAttackCardIndex()
    {
        return currentAttackSquence % attackSequence.Count;
    }

    private void FlipSpriteX()
    {
        isFacingRight = !isFacingRight;
        spRenderer.flipX = !isFacingRight;
    }

    public Vector2 GetPreviousPosition()
    {
        return previousPosition;
    }

    public void SetPreviousPosition(Vector2 loc)
    {
        previousPosition = loc;
    }

    public bool GetCanPathToTarget()
    {
        return enemyInformation.GetCanPathToTarget();
    }

    public HealthController GetHealthController()
    {
        return healthController;
    }
}
