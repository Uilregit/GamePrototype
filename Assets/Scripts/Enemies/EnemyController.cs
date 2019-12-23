using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class EnemyController : MonoBehaviour
{
    public int maxVit;
    public int startingShield;
    public int attack;
    public int moveRange;
    public int attackRange;
    public int vitAttackValue;
    public int shieldAttackValue;
    public int randomStartRange;
    public List<Card> attackSquence;
    public Color moveRangeColor;
    public Color attackRangeColor;
    public float attackCardHangTime;
    public float moveStepTime;

    public enum MoveType { Kite, Hug }
    public MoveType moveType;

    public enum TargetType { Nearest, Furthest, LowestVit, LowestShield, HighestShield, HighestVit, RedPlayer, GreenPlayer, BluePlayer };
    public TargetType targetType = TargetType.Nearest;

    //public Card[] attackCards;

    private HealthController healthController;
    private EnemyInformationController enemyInformation;
    private Outline outline;

    private int tauntDuration = 0;
    private GameObject tauntTarget;

    private int currentAttackSquence = 0;
    private int attackCardIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        TurnController.turnController.ReportEnemy(this);
        outline = GetComponent<Outline>();

        healthController = GetComponent<HealthController>();
        healthController.SetMaxVit(maxVit);
        healthController.SetCurrentVit(maxVit);
        healthController.SetMaxShield(startingShield);
        healthController.SetAttack(attack);

        enemyInformation = GetComponent<EnemyInformationController>();

        currentAttackSquence = Random.Range(0, randomStartRange + 1);
        attackRange = 0;
        foreach (Card card in attackSquence) //Set the attack range indicator to be the highest range card
            if (card.range > attackRange)
                attackRange = card.range;
    }

    public void Spawn()
    {
        int x, y;
        for (int j = 0; j < 100; j++)
        {
            x = Random.Range(GameController.gameController.enemySpawnBox[0], GameController.gameController.enemySpawnBox[1] + 1);
            y = Random.Range(GameController.gameController.enemySpawnBox[2], GameController.gameController.enemySpawnBox[3] + 1);
            if (GridController.gridController.GetObjectAtLocation(new Vector2(x, y)) == null)
            {
                Vector2 location = new Vector2(x, y);
                Spawn(location);
                break;
            }
            else
            {
                Debug.Log("Tried enemy at: " + x.ToString() + ", " + y.ToString());
            }
        }
    }

    public void Spawn(Vector2 location)
    {
        transform.position = location;
        GridController.gridController.ReportPosition(this.gameObject, location);
        transform.SetParent(GameObject.FindGameObjectWithTag("BoardCanvas").transform);
    }

    //Moves, then if able to attack, attack
    public IEnumerator ExecuteTurn()
    {
        outline.enabled = true;
        attackCardIndex = currentAttackSquence % attackSquence.Count; //Chose the card to attack with

        if (!healthController.GetStunned())
        {
            //move towards target specified by TargetType
            GameObject target;
            if (tauntDuration > 0)          //Taunt target
                target = tauntTarget;
            else
                target = GetTarget(attackSquence[attackCardIndex].castType);
            yield return StartCoroutine(Move(target));

            if (healthController.GetStunned()) //In case that it is stunned while moving, stop turn
            {
                tauntDuration -= 1;
                outline.enabled = false;
                yield break;
            }

            //after moving, attack target if in range. If not, then attack nearest target in range
            //Don't waste attack on turns on the way to it's target
            if (attackSquence[attackCardIndex].targetType[0] == Card.TargetType.Player ||
                attackSquence[attackCardIndex].targetType[0] == Card.TargetType.Enemy)
            {
                if (GetDistanceFrom(target.transform.position) <= attackSquence[attackCardIndex].range)
                    yield return StartCoroutine(Attack(attackCardIndex, target));
                else if (GetDistanceFrom(GetNearest(attackSquence[attackCardIndex].castType).transform.position) <= attackSquence[attackCardIndex].range &&
                    tauntDuration <= 0) //If can't attack target, then attack nearest instead
                {
                    target = GetNearest(attackSquence[attackCardIndex].castType);
                    yield return StartCoroutine(Attack(attackCardIndex, target));
                }
            }
            else if (attackSquence[attackCardIndex].targetType[0] == Card.TargetType.Self)
                yield return StartCoroutine(Attack(attackCardIndex, this.gameObject));
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
        }

        currentAttackSquence += 1;
        tauntDuration -= 1;
        outline.enabled = false;
    }

    //Gets a list of pathfinding locations and step in one position after another to the final destination
    private IEnumerator Move(GameObject target)
    {
        GridController.gridController.RemoveFromPosition(transform.position);

        List<Vector2> traveledPath = FindClosestPoint(target, attackSquence[attackCardIndex].range);
        foreach (Vector2 position in traveledPath)
        {
            if (healthController.GetStunned())  //In case that it is stunned while moving, stop turn
            {
                Debug.Log("got out");
                GridController.gridController.ReportPosition(this.gameObject, transform.position);
                yield break;
            }

            transform.position = position;
            yield return new WaitForSeconds(moveStepTime);
        }

        GridController.gridController.ReportPosition(this.gameObject, transform.position);
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

    //Show the attack card and target line, then trigger the attack card
    private IEnumerator Attack(int attackCardIndex, GameObject target)
    {
        enemyInformation.ShowUsedCard(attackSquence[attackCardIndex], target);
        enemyInformation.ShowTargetLine(target);

        yield return new WaitForSeconds(attackCardHangTime);

        TriggerAttackCard(attackCardIndex, target);
        enemyInformation.DestroyUsedCard();
    }

    //Trigger the attack card on the target
    private void TriggerAttackCard(int attackCardIndex, GameObject target)
    {
        enemyInformation.TriggerCard(attackCardIndex, target);
    }

    private void AttackVit(HealthController targetHealthController)
    {
        targetHealthController.TakeVitDamage(vitAttackValue, GetComponent<HealthController>());
    }

    private void AttackShield(HealthController targetHealthController)
    {
        targetHealthController.TakeShieldDamage(shieldAttackValue);
    }

    private GameObject GetTarget(Card.CastType type)
    {
        if (targetType == TargetType.Nearest)
            return GetNearest(type);
        else if (targetType == TargetType.Furthest)
            return GetFurthest(type);
        else if (targetType == TargetType.HighestShield)
            return GetHighestShield(type);
        else if (targetType == TargetType.LowestShield)
            return GetLowestShield(type);
        else if (targetType == TargetType.HighestVit)
            return GetHighestVit(type);
        else if (targetType == TargetType.LowestVit)
            return GetLowestVit(type);

        return null;
    }

    private GameObject GetNearest(Card.CastType type)
    {
        GameObject output = null;

        string tag = "Player";
        if (type == Card.CastType.Enemy)
            tag = "Enemy";
        GameObject[] targets = GameObject.FindGameObjectsWithTag(tag);
        int distance = 9999999;
        foreach (GameObject target in targets)
        {
            if (GetDistanceFrom(target.transform.position) < distance)
            {
                distance = GetDistanceFrom(target.transform.position);
                output = target;
            }
        }
        return output;
    }

    private GameObject GetFurthest(Card.CastType type)
    {
        GameObject output = null;

        string tag = "Player";
        if (type == Card.CastType.Enemy)
            tag = "Enemy";
        GameObject[] targets = GameObject.FindGameObjectsWithTag(tag);
        int distance = -99999999;
        foreach (GameObject target in targets)
        {
            if (GetDistanceFrom(target.transform.position) > distance)
            {
                distance = GetDistanceFrom(target.transform.position);
                output = target;
            }
        }
        return output;
    }

    private GameObject GetHighestShield(Card.CastType type)
    {
        GameObject output = null;

        string tag = "Player";
        if (type == Card.CastType.Enemy)
            tag = "Enemy";
        GameObject[] targets = GameObject.FindGameObjectsWithTag(tag);
        int armor = -999999999;
        foreach (GameObject target in targets)
        {
            if (target.GetComponent<HealthController>().GetShield() > armor)
            {
                armor = target.GetComponent<HealthController>().GetShield();
                output = target;
            }
        }
        return output;
    }

    private GameObject GetLowestShield(Card.CastType type)
    {
        GameObject output = null;

        string tag = "Player";
        if (type == Card.CastType.Enemy)
            tag = "Enemy";
        GameObject[] targets = GameObject.FindGameObjectsWithTag(tag);
        int armor = 999999999;
        foreach (GameObject target in targets)
        {
            if (target.GetComponent<HealthController>().GetShield() < armor)
            {
                armor = target.GetComponent<HealthController>().GetShield();
                output = target;
            }
        }
        return output;
    }

    private GameObject GetHighestVit(Card.CastType type)
    {
        GameObject output = null;

        string tag = "Player";
        if (type == Card.CastType.Enemy)
            tag = "Enemy";
        GameObject[] targets = GameObject.FindGameObjectsWithTag(tag);
        int vit = 0;
        foreach (GameObject target in targets)
        {
            if (target.GetComponent<HealthController>().GetVit() > vit)
            {
                vit = target.GetComponent<HealthController>().GetShield();
                output = target;
            }
        }
        return output;
    }

    //Find the player with the lowest vitality
    private GameObject GetLowestVit(Card.CastType type)
    {
        GameObject output = null;

        string tag = "Player";
        if (type == Card.CastType.Enemy)
            tag = "Enemy";
        GameObject[] targets = GameObject.FindGameObjectsWithTag(tag);
        int vit = 999999999;
        foreach (GameObject target in targets)
        {
            if (target.GetComponent<HealthController>().GetVit() < vit)
            {
                vit = target.GetComponent<HealthController>().GetShield();
                output = target;
            }
        }
        return output;
    }

    private void OnDestroy()
    {
        TurnController.turnController.RemoveEnemy(this);
        GridController.gridController.RemoveFromPosition(transform.position);
    }

    private int GetDistanceFrom(Vector2 target)
    {
        return (int)(Mathf.Abs(transform.position.x - target.x) + Mathf.Abs(transform.position.y - target.y));
    }

    //Find the closest point to the target with a kite distance using the return of pathinfinding
    private List<Vector2> FindClosestPoint(GameObject target, int kiteDistance = 0)
    {
        List<Vector2> path = PathFindController.pathFinder.PathFind(transform.position, target.transform.position);
        List<Vector2> output = path.GetRange(0, 1);
        int bonuseMoveRange = GetComponent<HealthController>().GetBonusMoveRange();

        if (path.Count > moveRange + bonuseMoveRange + attackSquence[attackCardIndex].range)//If target is out of reach
            output = path.GetRange(0, moveRange + bonuseMoveRange + 1);
        else if (path.Count < attackSquence[attackCardIndex].range)//If target is too close
        {
            output = path.GetRange(0, 1); //Currently doesn't move, may change later, will affect ranged enemies
        }
        else//If target is between min and max moverange
            output = path.GetRange(0, path.Count - attackSquence[attackCardIndex].range);

        return output;
    }

    public void SetTaunt(GameObject target, int duration)
    {
        tauntTarget = target;
        tauntDuration = duration;
    }
}
