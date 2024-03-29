﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class EnemyController : MonoBehaviour
{
    private int size;
    private List<Vector2> occupiedSpace;

    [Header("Stats Settings")]
    public int maxVit;
    public int startingShield;
    public int attack;
    public int moveRange;
    public int attackRange;
    public int vitAttackValue;
    public int shieldAttackValue;
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

    public enum TargetType { Nearest, Furthest, LowestVit, LowestShield, HighestShield, HighestVit, MostMissingHealth, RedPlayer, GreenPlayer, BluePlayer, Self, Default, HighestAttack, LowestAttack };
    public TargetType targetType = TargetType.Nearest;

    //public Card[] attackCards;

    //public SpriteRenderer sprite;
    //public SpriteRenderer shadow;

    private HealthController healthController;
    private EnemyInformationController enemyInformation;
    //public SpriteRenderer outline;
    private Collider2D col2D;

    private int tauntDuration = 0;
    private GameObject tauntTarget;
    public GameObject[] desiredTarget;
    private bool sacrificed;

    private int currentAttackSquence = 0;
    protected int attackCardIndex = 0;

    private bool skipInitialIntent = false;

    // Start is called before the first frame update
    void Awake()
    {
        TurnController.turnController.ReportEnemy(this);

        col2D = GetComponent<Collider2D>();

        healthController = GetComponent<HealthController>();
        maxVit = Mathf.RoundToInt(maxVit * (1 + Random.Range(-0.1f, 0.1f)));
        healthController.SetMaxVit(maxVit);
        healthController.SetCurrentVit(maxVit);
        healthController.SetStartingShield(startingShield);
        healthController.SetAttack(attack);

        enemyInformation = GetComponent<EnemyInformationController>();

        desiredTarget = new GameObject[attacksPerTurn];
        currentAttackSquence = Random.Range(0, randomStartRange + 1) * attacksPerTurn;
        attackRange = 0;
        attackSequenceControllers = new List<CardController>();

        foreach (Card card in attackSequence) //Set the attack range indicator to be the highest range card
        {
            CardController temp = this.gameObject.AddComponent<CardController>();
            temp.SetCard(card, true, false);
            attackSequenceControllers.Add(temp); //Set the caster for the enemy cards for dynamic damage value display
            if (card.range > attackRange)
                attackRange = card.range;
        }

        size = GetComponent<HealthController>().size;
        occupiedSpace = GetComponent<HealthController>().GetOccupiedSpaces();
    }

    private void Start()
    {
        if (!skipInitialIntent)
            RefreshIntent(); //Keeps refresh intent in start to let blockades spawn before doing attackable range calculations
        else
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
    }

    //Moves, then if able to attack, attack
    public IEnumerator ExecuteTurn()
    {
        if (sacrificed)
            yield break;

        col2D.enabled = false;
        enemyInformation.OnMouseUp();
        healthController.charDisplay.outline.enabled = true;

        for (int i = 0; i < attacksPerTurn; i++)
        {
            if (desiredTarget[i] == null)                  //In case the desired target died before the turn happened
                desiredTarget[i] = GetTarget();

            yield return new WaitForSeconds(TimeController.time.enemyMoveStepTime * TimeController.time.timerMultiplier);

            attackCardIndex = currentAttackSquence % attackSequence.Count; //Chose the card to attack with

            if (!healthController.GetStunned() && GetComponent<HealthController>().GetCurrentVit() > 0)
            {
                //move towards target specified by TargetType
                //GameObject target = GetTarget();
                yield return StartCoroutine(Move(desiredTarget[i]));

                if (healthController.GetStunned()) //In case that it is stunned while moving, stop turn
                {
                    tauntDuration -= 1;
                    healthController.charDisplay.outline.enabled = false;
                    yield break;
                }

                //after moving, attack target if in range. If not, then attack nearest target in range
                //Don't waste attack on turns on the way to it's target
                List<Vector2> targetLocs = new List<Vector2>();

                if (attackSequence[attackCardIndex].castType == Card.CastType.AoE)
                    foreach (Vector2 loc in occupiedSpace)
                        targetLocs.AddRange(GridController.gridController.GetLocationsInAoE((Vector2)transform.position + loc, attackSequence[attackCardIndex].radius, new string[] { "All" }));
                else if (attackSequence[attackCardIndex].castType == Card.CastType.Player ||
                         attackSequence[attackCardIndex].castType == Card.CastType.Enemy)
                {
                    if (GetDistanceFrom(desiredTarget[i].transform.position) <= attackSequence[attackCardIndex].range)
                        targetLocs.Add(desiredTarget[i].transform.position);
                    else if (attackSequence[attackCardIndex].castType == Card.CastType.Player)
                    {
                        int attack = enemyInformation.displayedCards[i].GetComponent<CardController>().GetSimulatedTotalAttackValue(attackCardIndex);
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
        tauntDuration -= 1;
        if (tauntDuration <= 0)
            tauntTarget = null;
    }

    public GameObject GetCurrentTarget()
    {
        if (tauntDuration > 0)          //Taunt target
            return tauntTarget;
        return desiredTarget[currentAttackSquence % attackSequence.Count];
    }

    //Returns the viable target of the enemy. If taunted, is the taunter, if target isn't pathable, checks if there's an attackable target, else default target
    public GameObject GetTarget()
    {
        GameObject target;
        if (tauntDuration > 0)          //Taunt target
            target = tauntTarget;
        else
        {
            target = GridController.gridController.GetObjectAtLocation(GetCardTarget(attackSequence[attackCardIndex].castType))[0];                     //Get the target according to the card in question

            /*
            List<GameObject> attackableTargets = enemyInformation.GetAttackableTargets(GetTargetTags(attackSequence[attackCardIndex].castType, attackSequence[attackCardIndex].targetType[0]));     //Will go to the nearest viable target to the actual target if
            int distance = 99999999;                                                                                                                //the actual target isn't pathable
            GameObject newTarget = target;
            foreach (GameObject obj in attackableTargets)                                                                                            //Gets a list of attackable targets, chage target to closest to the desired one
            {
                if (GridController.gridController.GetManhattanDistance(target.transform.position, obj.transform.position) < distance)
                {
                    distance = GridController.gridController.GetManhattanDistance(target.transform.position, obj.transform.position);
                    newTarget = obj;
                }
            }
            target = newTarget;
            */
        }

        return target;
    }

    public GameObject[] GetTargetArray()
    {
        GameObject[] target = new GameObject[attacksPerTurn];
        if (tauntDuration > 0 && tauntTarget != null)          //Taunt target
            for (int i = 0; i < attacksPerTurn; i++)
                target[i] = tauntTarget;
        else
        {
            tauntDuration = 0;
            for (int i = 0; i < attacksPerTurn; i++)
                target[i] = GridController.gridController.GetObjectAtLocation(GetCardTarget(attackSequence[attackCardIndex + i].castType))[0];                     //Get the target according to the card in question
        }
        return target;
    }

    //Gets the target specified by the card's target type
    private Vector2 GetCardTarget(Card.CastType type)
    {
        TargetType currentTargetType = targetType;
        if (attackSequence[attackCardIndex].targetBehaviour != TargetType.Default)
            currentTargetType = attackSequence[attackCardIndex].targetBehaviour;

        if (currentTargetType == TargetType.Self)
            return transform.position;
        else if (currentTargetType == TargetType.Nearest)
            return GetNearest(type);
        else if (currentTargetType == TargetType.Furthest)
            return GetFurthest(type);
        else if (currentTargetType == TargetType.HighestShield)
            return GetHighestShield(type);
        else if (currentTargetType == TargetType.LowestShield)
            return GetLowestShield(type);
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
    private IEnumerator Move(GameObject target)
    {
        if (target == this.gameObject) //If targeting self, still move towards the nearest player
            target = GridController.gridController.GetObjectAtLocation(GetNearest(Card.CastType.Player))[0];

        foreach (Vector2 vec in occupiedSpace)
            GridController.gridController.RemoveFromPosition(this.gameObject, (Vector2)transform.position + vec);

        List<Vector2> traveledPath = new List<Vector2>();
        if (attackSequence[attackCardIndex].castType == Card.CastType.AoE)
            traveledPath = FindMostInAoE(target);
        else
            traveledPath = FindPath(target, attackSequence[attackCardIndex].range);

        if (RoomController.roomController.debug)
            foreach (Vector2 loc in traveledPath)
                TileCreator.tileCreator.CreateTiles(this.gameObject, loc, Card.CastShape.Circle, 0, Color.yellow, 2);

        traveledPath = traveledPath.GetRange(1, Mathf.Min(traveledPath.Count - 1, moveRange + GetComponent<HealthController>().GetBonusMoveRange())); //Leave at least 1 space so it never paths ON the target

        foreach (Vector2 position in traveledPath)
        {
            if (healthController.GetStunned() ||   //In case that it is stunned while moving, stop turn
                GridController.gridController.GetObjectAtLocation(position).Count != 0)  //If position is filled, stop turn
            {
                foreach (Vector2 vec in occupiedSpace)
                    GridController.gridController.ReportPosition(this.gameObject, (Vector2)transform.position + vec);
                yield break;
            }

            transform.position = position;
            yield return new WaitForSeconds(TimeController.time.enemyMoveStepTime * TimeController.time.timerMultiplier);
        }

        foreach (Vector2 vec in occupiedSpace)
            GridController.gridController.ReportPosition(this.gameObject, (Vector2)transform.position + vec);
    }

    //Show the attack card and target line, then trigger the attack card
    private IEnumerator Attack(int displayCardIndex, List<Vector2> target)
    {
        if (target.Count > 0)
        {
            enemyInformation.ShowUsedCard(attackSequenceControllers[attackCardIndex], target[0]);
            enemyInformation.ShowTargetLine(target[0]);

            yield return new WaitForSeconds(TimeController.time.enemyAttackCardHangTime * TimeController.time.timerMultiplier);
            yield return StartCoroutine(enemyInformation.TriggerCard(displayCardIndex, target));

            enemyInformation.DestroyUsedCard();
        }
    }

    //Will move to the location to get the most amount of targets in the AoE range
    protected virtual List<Vector2> FindMostInAoE(GameObject target)
    {
        //First find all positions that can actually be pathed to
        foreach (Vector2 vec in GetComponent<HealthController>().GetOccupiedSpaces())
            TileCreator.tileCreator.CreateTiles(this.gameObject, (Vector2)transform.position + vec, Card.CastShape.Circle, moveRange + GetComponent<HealthController>().GetBonusMoveRange(), Color.clear, new string[] { "Player", "Enemy", "Blockade" }, 2);
        List<Vector2> moveablePositions = TileCreator.tileCreator.GetTilePositions(2);
        TileCreator.tileCreator.DestryTiles(this.gameObject, 2);

        //Get all the possible targets
        string[] tags = GetTargetTags(attackSequence[attackCardIndex].castType, attackSequence[attackCardIndex].targetType[0]);
        List<GameObject> possibleTargets = new List<GameObject>();
        foreach (string t in tags)
            possibleTargets.AddRange(GameObject.FindGameObjectsWithTag(t));

        //If a pathable position can attack a target, add it to the list. repeat for each target to get counts of how many targets that position can attack
        List<Vector2> viablePositions = new List<Vector2>();
        foreach (GameObject obj in possibleTargets)
        {
            TileCreator.tileCreator.CreateTiles(this.gameObject, obj.transform.position, attackSequence[attackCardIndex].castShape, attackSequence[attackCardIndex].radius, Color.clear, 2);
            List<Vector2> attackFromLocation = TileCreator.tileCreator.GetTilePositions(2);
            TileCreator.tileCreator.DestryTiles(this.gameObject, 2);

            foreach (Vector2 loc in attackFromLocation)
                if (moveablePositions.Contains(loc))
                    viablePositions.Add(loc);
        }

        //Return the position that can attack the most targets
        Vector2 finalLocation;
        if (viablePositions.Count == 0)
            finalLocation = GetNearest(attackSequence[attackCardIndex].castType);
        else
            finalLocation = viablePositions.GroupBy(x => x).OrderByDescending(grp => grp.Count()).Select(grp => grp.Key).First();
        return PathFindController.pathFinder.PathFind(transform.position, finalLocation, new string[] { "None" }, occupiedSpace, size);
    }

    //Find the closest point to the target with a kite distance using the return of pathinfinding
    protected virtual List<Vector2> FindPath(GameObject target, int kiteDistance = 0)
    {
        List<Vector2> output = FindFurthestPointInRange(target);

        if (output.Count == 1)
        {
            List<Vector2> path = PathFindController.pathFinder.PathFind(transform.position, target.transform.position, new string[] { "None" }, occupiedSpace, size);
            if (RoomController.roomController.debug)
                foreach (Vector2 loc in path)
                    TileCreator.tileCreator.CreateTiles(this.gameObject, loc, Card.CastShape.Circle, 0, Color.blue, 0);
            int bonuseMoveRange = GetComponent<HealthController>().GetBonusMoveRange();
            output = path.GetRange(0, Mathf.Min(moveRange + bonuseMoveRange + 1, path.Count));
        }

        TileCreator.tileCreator.DestryTiles(this.gameObject, 1);
        if (RoomController.roomController.debug)
            foreach (Vector2 loc in output)
                TileCreator.tileCreator.CreateTiles(this.gameObject, loc, Card.CastShape.Circle, 0, Color.red, 1);

        return output;
    }

    protected virtual List<Vector2> FindFurthestPointInRange(GameObject target)
    {
        TileCreator.tileCreator.CreateTiles(this.gameObject, target.transform.position, attackSequence[attackCardIndex].castShape, attackSequence[attackCardIndex].range, Color.clear, 2);
        List<Vector2> inRangeLocations = TileCreator.tileCreator.GetTilePositions(2);
        List<Vector2> emptyInRangeLocations = new List<Vector2>();
        foreach (Vector2 loc in inRangeLocations)
            if (GridController.gridController.GetObjectAtLocation(loc).Count == 0)
                emptyInRangeLocations.Add(loc);
        inRangeLocations = emptyInRangeLocations;
        TileCreator.tileCreator.DestryTiles(this.gameObject, 2);                                            //Get all positions that can cast on the target

        List<Vector2>[] locationsByDistance = new List<Vector2>[Mathf.Max(attackSequence[attackCardIndex].range, 1)];      //Sort positions by distance to target
        for (int i = Mathf.Max(attackSequence[attackCardIndex].range - 1, 0); i >= 0; i--)
            locationsByDistance[i] = new List<Vector2>();

        foreach (Vector2 loc in inRangeLocations)
            try
            {
                locationsByDistance[GetManhattanDistance(loc, target.transform.position) - 1].Add(loc);
            }
            catch
            {
                Debug.Log("######### ERROR ########");
                Debug.Log(loc);
                Debug.Log(target.transform.position);
                Debug.Log(GetManhattanDistance(loc, target.transform.position) - 1);
                Debug.Log(locationsByDistance.Length);
                Debug.Log(attackSequence[attackCardIndex].range);
                Debug.Log(Mathf.Max(attackSequence[attackCardIndex].range, 1));
            }

        List<Vector2> pathableLocations = new List<Vector2>();                                              //Check if pathable within the allocated move distance or not
        for (int i = Mathf.Max(attackSequence[attackCardIndex].range - 1, 0); i >= 0; i--)                                  //Prefer locations further from target if possible
        {
            pathableLocations = new List<Vector2>();
            foreach (Vector2 loc in locationsByDistance[i])
            {
                List<Vector2> path = new List<Vector2>();
                path = PathFindController.pathFinder.PathFind(transform.position, loc, new string[] { "None" }, occupiedSpace, size);
                if (path.Count - 1 <= moveRange + GetComponent<HealthController>().GetBonusMoveRange())
                    pathableLocations.Add(path[path.Count - 1]);
            }
            if (pathableLocations.Count > 0)                                                                //If there are pathable locations in this distance range, move on to the next step
                break;
        }

        Vector2 desiredTargetLocation = new Vector2();
        if (desiredTarget != null)                                                                          //In case the desired target has died/been destroyed
            desiredTargetLocation = desiredTarget[currentAttackSquence % attacksPerTurn].transform.position;
        else
            desiredTargetLocation = GetTarget().transform.position;

        List<Vector2> finalLocs = new List<Vector2>() { transform.position };                               //Find the viable position that's the furthest from the current position
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
                if (GetManhattanDistance(loc, desiredTargetLocation) < GetManhattanDistance(finalLocs[0], desiredTargetLocation))
                    finalLocs = new List<Vector2>() { loc };
                else if (GetManhattanDistance(loc, desiredTargetLocation) == GetManhattanDistance(finalLocs[0], desiredTargetLocation))
                    finalLocs.Add(loc);
            }

        }

        Vector2 finalLoc = finalLocs[Random.Range(0, finalLocs.Count)];                                     //If there are multiple viable positions with the same distance, chose one at random

        return PathFindController.pathFinder.PathFind(transform.position, finalLoc, new string[] { "None" }, occupiedSpace, size);  //Return the pathfind to the chosen location
    }

    public void RefreshIntent()
    {
        if (GetComponent<HealthController>().GetCurrentVit() <= 0)
            return;
        attackCardIndex = currentAttackSquence % attackSequence.Count;
        attackRange = attackSequence[attackCardIndex].range;
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
            case Card.CastType.AoE:
                if (firstTargetType == Card.TargetType.Enemy)
                    tags = new string[] { "Enemy" };
                else
                    tags = new string[] { "Player" };
                break;
            default:
                tags = new string[] { "Player" };
                break;
        }

        return tags;
    }

    private Vector2 GetNearest(Card.CastType type)
    {
        GameObject output = null;

        string tag = "Player";
        if (type == Card.CastType.Enemy)
            tag = "Enemy";
        GameObject[] targets = GameObject.FindGameObjectsWithTag(tag);
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

        string tag = "Player";
        if (type == Card.CastType.Enemy)
            tag = "Enemy";
        GameObject[] targets = GameObject.FindGameObjectsWithTag(tag);
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

    private Vector2 GetHighestShield(Card.CastType type)
    {
        GameObject output = null;

        string tag = "Player";
        if (type == Card.CastType.Enemy)
            tag = "Enemy";
        GameObject[] targets = GameObject.FindGameObjectsWithTag(tag);
        output = targets[0];
        int armor = -999999999;
        foreach (GameObject target in targets)
        {
            if (target.GetComponent<HealthController>().GetShield() > armor && target.GetComponent<HealthController>().GetVit() > 0)
            {
                armor = target.GetComponent<HealthController>().GetShield();
                output = target;
            }
        }
        return output.transform.position;
    }

    private Vector2 GetLowestShield(Card.CastType type)
    {
        GameObject output = null;

        string tag = "Player";
        if (type == Card.CastType.Enemy)
            tag = "Enemy";
        GameObject[] targets = GameObject.FindGameObjectsWithTag(tag);
        output = targets[0];
        int armor = 999999999;
        foreach (GameObject target in targets)
        {
            if (target.GetComponent<HealthController>().GetShield() < armor && target.GetComponent<HealthController>().GetVit() > 0)
            {
                armor = target.GetComponent<HealthController>().GetShield();
                output = target;
            }
        }
        return output.transform.position;
    }

    private Vector2 GetHighestVit(Card.CastType type)
    {
        GameObject output = null;

        string tag = "Player";
        if (type == Card.CastType.Enemy)
            tag = "Enemy";
        GameObject[] targets = GameObject.FindGameObjectsWithTag(tag);
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

        string tag = "Player";
        if (type == Card.CastType.Enemy)
            tag = "Enemy";
        GameObject[] targets = GameObject.FindGameObjectsWithTag(tag);
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

        string tag = "Player";
        if (type == Card.CastType.Enemy)
            tag = "Enemy";
        GameObject[] targets = GameObject.FindGameObjectsWithTag(tag);
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

        string tag = "Player";
        if (type == Card.CastType.Enemy)
            tag = "Enemy";
        GameObject[] targets = GameObject.FindGameObjectsWithTag(tag);
        output = targets[0];
        int highestAttack = 0;
        foreach (GameObject target in targets)
        {
            if (target.GetComponent<HealthController>().GetAttack() - target.GetComponent<HealthController>().GetAttack() > highestAttack)
            {
                highestAttack = target.GetComponent<HealthController>().GetMaxVit() - target.GetComponent<HealthController>().GetCurrentVit();
                output = target;
            }
        }
        return output.transform.position;
    }

    private Vector2 GetLowestAttack(Card.CastType type)
    {
        GameObject output = null;

        string tag = "Player";
        if (type == Card.CastType.Enemy)
            tag = "Enemy";
        GameObject[] targets = GameObject.FindGameObjectsWithTag(tag);
        output = targets[0];
        int lowestAttack = 999999999;
        foreach (GameObject target in targets)
        {
            if (target.GetComponent<HealthController>().GetAttack() - target.GetComponent<HealthController>().GetAttack() < lowestAttack)
            {
                lowestAttack = target.GetComponent<HealthController>().GetMaxVit() - target.GetComponent<HealthController>().GetCurrentVit();
                output = target;
            }
        }
        return output.transform.position;
    }

    public void SetTaunt(GameObject target, int duration)
    {
        tauntTarget = target;
        for (int i = 0; i < desiredTarget.Length; i++)
            desiredTarget[i] = target;
        tauntDuration = duration;
        try
        {
            GetComponent<EnemyInformationController>().RefreshIntent();
        }
        catch { }
    }

    public GameObject GetTaunt()
    {
        return tauntTarget;
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
        TurnController.turnController.RemoveEnemy(this);
        foreach (Vector2 vec in occupiedSpace)
            if (!sacrificed)
                GridController.gridController.RemoveFromPosition(this.gameObject, (Vector2)transform.position + vec);
        enemyInformation.DestroyUsedCard();
    }
    /*
    //Scores the simulated outcome, highest score should be the most desired outcome
    private int ScoreSimulatedOutCome(SimHealthController simH, int turn)
    {
        int score = 0;
        if (simH.currentVit <= 0)
        {
            score += 99999;                 //Max out score 
            score -= turn;                  //Gives a penalty for higher turn number. Early lethals are better
        }
        else
        {
            score -= simH.currentVit * 1;   //1 point per health left
            score -= simH.currentShield * 2;//2 points per shield left
        }
        return score;
    }
    */

    /*
    //Chose index of the card to attack with
    private int ChoseAttackCard(GameObject target)
    {
        SimHealthController simH = new SimHealthController();
        simH.SetValues(target.GetComponent<HealthController>().GetSimulatedSelf());
        int bestScore = -999999;
        int firstCard = 0;

        for (int i = 0; i < attackCards.Length; i++)
        {
            SimHealthController simH1 = new SimHealthController();
            simH1.SetValues(enemyInformation.SimulateTriggerCard(i, target, simH));
            if (ScoreSimulatedOutCome(simH1, 1) > bestScore)
            {
                firstCard = i;
                bestScore = ScoreSimulatedOutCome(simH1, 1);
            }
            for (int j = 0; j < attackCards.Length; j++)
            {
                SimHealthController simH2 = new SimHealthController();
                simH2.SetValues(enemyInformation.SimulateTriggerCard(j, target, simH1));
                if (ScoreSimulatedOutCome(simH2, 2) > bestScore)
                {

                    firstCard = i;
                    bestScore = ScoreSimulatedOutCome(simH2, 2);
                }
                for (int k = 0; k < attackCards.Length; k++)
                {
                    SimHealthController simH3 = new SimHealthController();
                    simH3.SetValues(enemyInformation.SimulateTriggerCard(k, target, simH2));
                    if (ScoreSimulatedOutCome(simH3, 3) > bestScore)
                    {

                        firstCard = i;
                        bestScore = ScoreSimulatedOutCome(simH3, 3);
                    }
                    for (int l = 0; l < attackCards.Length; l++)
                    {
                        SimHealthController simH4 = new SimHealthController();
                        simH4.SetValues(enemyInformation.SimulateTriggerCard(l, target, simH3));
                        if (ScoreSimulatedOutCome(simH4, 4) > bestScore)
                        {

                            firstCard = i;
                            bestScore = ScoreSimulatedOutCome(simH4, 4);
                        }
                    }
                }
            }
        }
        return firstCard;
    }
    */

    /*
    //Use recursion to find the best card sequence to use simulating "iteration" number of turns
    //Dictionary of bestScore, then the SHC object storing the simulation data
    private Dictionary<int, SimHealthController> RecurseBestCardCombo(int bestScore, SimHealthController simH, GameObject target, int iteration)
    {
        Dictionary<int, SimHealthController> output = new Dictionary<int, SimHealthController>();

        for (int l = 0; l < attackCards.Length; l++)
        {
            SimHealthController thisSimH = new SimHealthController();
            thisSimH.SetValues(enemyInformation.SimulateTriggerCard(l, target, simH));
            if (ScoreSimulatedOutCome(thisSimH, iteration) > bestScore)
            {

                thisSimH.cardSequence.Add(l);
                bestScore = ScoreSimulatedOutCome(thisSimH, iteration);
                output[bestScore] = thisSimH;
            }
            if (iteration >= 1)
            {
                Dictionary<int, SimHealthController> nextIteration = RecurseBestCardCombo(bestScore, thisSimH, target, iteration - 1);
                foreach (var item in nextIteration)
                    if (!output.Keys.Contains(item.Key))
                        output.Add(item.Key, item.Value);
            }
        }
        return output;
    }
    */
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
}
