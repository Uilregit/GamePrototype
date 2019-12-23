using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* This is the moveController for player characters
 * Called by MouseController
 */
public class PlayerMoveController : MonoBehaviour
{
    private PlayerController player;
    private int movedDistance = 0;
    public Color moveRangeIndicatorColor;
    public Color attackRangeIndicatorColor;

    private Vector2 originalPosition;
    private Vector2 lastGoodPosition;
    [SerializeField]
    private GameObject moveShadow;
    private bool moveable = true;
    private bool attackable = true;
    private bool aboutToAttack = false;
    private HealthController attackTarget;
    private List<Vector2> moveablePositions = new List<Vector2>();
    private List<Vector2> attackablePositions = new List<Vector2>();
    private List<Vector2> path = new List<Vector2>();

    public void Spawn()
    {
        lastGoodPosition = transform.position;
        originalPosition = transform.position;
        moveShadow.GetComponent<SpriteRenderer>().sprite = GetComponent<SpriteRenderer>().sprite;
    }

    //Moves the moveshadow to the rounded unit tile
    public void UpdateMovePosition(Vector2 newlocation)
    {
        Vector2 roundedPosition = new Vector2(Mathf.Round(newlocation.x), Mathf.Round(newlocation.y));
        if (CheckIfPositionValid(roundedPosition))
        {
            if (GridController.gridController.GetObjectAtLocation(roundedPosition) == null)
            {
                moveShadow.transform.position = roundedPosition;
                path.Add(roundedPosition);
                aboutToAttack = false;
            }
            else
            {
                Vector2 attackFromPosition = FindViableAttackPosition(roundedPosition);
                if (GridController.gridController.GetManhattanDistance(attackFromPosition, roundedPosition) <= player.GetAttackRange())
                {
                    moveShadow.transform.position = attackFromPosition;
                    aboutToAttack = true;
                    attackTarget = GridController.gridController.GetObjectAtLocation(roundedPosition).GetComponent<HealthController>();
                }
                else
                {
                    moveShadow.transform.position = lastGoodPosition;
                    aboutToAttack = false;
                }
            }
        }
        else
        {
            aboutToAttack = false;
            moveShadow.transform.position = lastGoodPosition;
        }
    }

    //Finds a viable attack position
    private Vector2 FindViableAttackPosition(Vector2 roundedPosition)
    {
        Vector2 output = roundedPosition;
        //If the player can attack without moving, attack without moving
        if (GridController.gridController.GetManhattanDistance(roundedPosition, lastGoodPosition) <= player.GetAttackRange())
            output = lastGoodPosition;
        else
        {
            bool foundPosition = false;
            //First checks if hovered over path positions are viable positions to attack from
            foreach (Vector2 position in path)
            {
                if (GridController.gridController.GetManhattanDistance(roundedPosition, position) <= player.GetAttackRange() &&
                    moveablePositions.Contains(position) && //Ranged characters are stll limited to their move ranges since path includes attackable but not moveable positions
                    GridController.gridController.GetObjectAtLocation(position) == null)
                {
                    output = position;
                    foundPosition = true;
                    break;
                }
            }
            //If not then checks all moveable positions and returns the first viable one
            if (!foundPosition)
                foreach (Vector2 position in moveablePositions)
                {
                    if (GridController.gridController.GetManhattanDistance(roundedPosition, position) <= player.GetAttackRange() &&
                    GridController.gridController.GetObjectAtLocation(position) == null)
                    {
                        output = position;
                        foundPosition = true;
                        break;
                    }
                }
            //If no viable attack position, then return to the last good position
            if (!foundPosition)
                return lastGoodPosition;
        }
        return output;
    }

    public Vector2 GetMoveLocation()
    {
        return moveShadow.transform.position;
    }

    //Checks if the position is valid to move to by checking the move distance, if the space is empty, and if the position is out of bounds
    private bool CheckIfPositionValid(Vector2 newRoundedPositon)
    {
        /*
        return (GridController.gridController.GetManhattanDistance(originalPosition, newRoundedPositon) <= moveRange-movedDistance &&
            GridController.gridController.GetObjectAtLocation(newRoundedPositon) == null &&
            !GridController.gridController.CheckIfOutOfBounds(newRoundedPositon));
            */
        if (GridController.gridController.GetObjectAtLocation(newRoundedPositon) == null)
            return moveablePositions.Contains(newRoundedPositon);
        else if (GridController.gridController.GetObjectAtLocation(newRoundedPositon).tag == "Enemy")
            return attackablePositions.Contains(newRoundedPositon);
        return false;
    }

    public void UpdateOrigin(Vector2 newOrigin)
    {
        originalPosition = newOrigin;
    }

    public void ResetTurn()
    {
        UpdateOrigin(transform.position);
        movedDistance = 0;
        SetMoveable(true);
        SetAttackable(true);
    }

    public void SetMoveable(bool newMoveable)
    {
        moveable = newMoveable;
    }

    public bool GetMoveable()
    {
        return moveable;
    }

    public void SetAttackable(bool newattackable)
    {
        attackable = newattackable;
    }

    public bool GetAttackable()
    {
        return attackable;
    }

    public void CreateMoveRangeIndicator()
    {
        /* Creates move range tiles and gets a list of all moveable locations
         */
        if (moveable)
        {
            HealthController healthController = GetComponent<HealthController>();
            GridController.gridController.RemoveFromPosition(transform.position);
            TileCreator.tileCreator.CreateTiles(this.gameObject, originalPosition, Card.CastShape.Circle, player.GetMoveRange() + healthController.GetBonusMoveRange() - movedDistance,
                                                moveRangeIndicatorColor, new string[] { "Enemy", "Blockade" }, 0);
            moveablePositions = TileCreator.tileCreator.GetTilePositions();
            if (attackable)
            {
                foreach (Vector2 position in moveablePositions)
                    TileCreator.tileCreator.CreateTiles(this.gameObject, position, Card.CastShape.Circle, player.GetAttackRange(),
                                                        attackRangeIndicatorColor, new string[] { "" }, 1);
                TileCreator.tileCreator.CreateTiles(this.gameObject, originalPosition, Card.CastShape.Circle, player.GetMoveRange() + healthController.GetBonusMoveRange() - movedDistance,
                                                        attackRangeIndicatorColor, new string[] { "Enemy", "Blockade" }, 1);
            }
            attackablePositions = TileCreator.tileCreator.GetTilePositions(1);
        }
    }

    //Destroys the move and attack range indicators
    public void DestroyMoveRrangeIndicator()
    {
        TileCreator.tileCreator.DestryTiles(this.gameObject);
        moveablePositions = new List<Vector2>();
        attackablePositions = new List<Vector2>();
    }

    public void SetPlayerController(PlayerController newPlayer)
    {
        player = newPlayer;
    }

    //Move perminantly sets the player location after action (called from cards)
    public void CommitMove()
    {
        movedDistance += GridController.gridController.GetManhattanDistance(transform.position, originalPosition);
        originalPosition = transform.position;
        path = new List<Vector2>();
    }

    //Moves the player to the location
    public void MoveTo(Vector2 location)
    {
        GridController.gridController.ReportPosition(this.gameObject, location);
        transform.position = location;
        moveShadow.transform.position = location;
        lastGoodPosition = transform.position;
        if (aboutToAttack)
        {
            CommitMove();
            Attack(attackTarget);
        }
    }

    private void Attack(HealthController target)
    {
        if (attackable)
        {
            target.TakeVitDamage(player.GetAttack(), GetComponent<HealthController>());
            attackable = false;
        }
    }
}
