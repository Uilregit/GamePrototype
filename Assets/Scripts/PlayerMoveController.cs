using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.Linq;
using System;

/* This is the moveController for player characters
 * Called by MouseController
 */
public class PlayerMoveController : MonoBehaviour
{
    private PlayerController player;
    private HealthController healthController;
    private int movedDistance = 0;
    public Color moveRangeIndicatorColor;
    public Color attackRangeIndicatorColor;

    private Vector2 originalPosition;
    private Vector2 lastGoodPosition;
    private Vector2 lastHoverLocation;
    [SerializeField]
    public GameObject moveShadow;
    private bool moveable = true;
    private List<Vector2> moveablePositions = new List<Vector2>();
    private List<Vector2> attackablePositions = new List<Vector2>();
    private List<Vector2> path = new List<Vector2>();
    private Collider2D col2D;

    private DateTime clickedTime;
    private Vector2 clickedLocation;

    private void Awake()
    {
        col2D = GetComponent<Collider2D>();
        healthController = GetComponent<HealthController>();
    }

    public void Spawn()
    {
        lastGoodPosition = transform.position;
        lastHoverLocation = transform.position;
        originalPosition = transform.position;
        //moveShadow.GetComponent<SpriteRenderer>().sprite = GetComponent<PlayerController>().sprite.sprite;
        moveShadow.GetComponent<SpriteRenderer>().enabled = false;
    }

    //Moves the moveshadow to the rounded unit tile
    public void UpdateMovePosition(Vector2 newlocation)
    {
        Vector2 roundedPosition = GridController.gridController.GetRoundedVector(newlocation, GetComponent<HealthController>().size);
        int moveRangeLeft = Mathf.Max(player.GetMoveRange() + healthController.GetBonusMoveRange() - movedDistance, 0);

        if (CheckIfPositionValid(roundedPosition))
        {
            if (GridController.gridController.GetObjectAtLocation(roundedPosition).Count == 0)
            {
                moveShadow.transform.position = roundedPosition;
                if (lastHoverLocation != roundedPosition)
                {
                    lastHoverLocation = roundedPosition;

                    path = PathFindController.pathFinder.PathFind(originalPosition, roundedPosition, new string[] { "Player", "Enemy" }, new List<Vector2> { Vector2.zero }, 1);
                    TileCreator.tileCreator.DestroyPathTiles(player.GetPlayerID());
                    TileCreator.tileCreator.CreatePathTiles(player.GetPlayerID(), path, moveRangeLeft - GridController.gridController.GetManhattanDistance(originalPosition, roundedPosition), moveRangeIndicatorColor);
                }
            }
        }
        else
        {
            moveShadow.transform.position = lastGoodPosition;
            if (lastHoverLocation != lastGoodPosition)
            {
                lastHoverLocation = lastGoodPosition;

                path = PathFindController.pathFinder.PathFind(originalPosition, lastGoodPosition, new string[] { "Player" }, new List<Vector2> { Vector2.zero }, 1);
                TileCreator.tileCreator.DestroyPathTiles(player.GetPlayerID());
                TileCreator.tileCreator.CreatePathTiles(player.GetPlayerID(), path, moveRangeLeft - GridController.gridController.GetManhattanDistance(originalPosition, lastGoodPosition), moveRangeIndicatorColor);
            }
        }
    }

    /*
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
                    GridController.gridController.GetObjectAtLocation(position).Count == 0)
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
                    GridController.gridController.GetObjectAtLocation(position).Count == 0)
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
    */

    public Vector2 GetMoveLocation()
    {
        return moveShadow.transform.position;
    }

    //Checks if the position is valid to move to by checking the move distance, if the space is empty, and if the position is out of bounds
    private bool CheckIfPositionValid(Vector2 newRoundedPositon)
    {
        if (GridController.gridController.CheckIfOutOfBounds(newRoundedPositon))
            return false;
        else if (GridController.gridController.GetObjectAtLocation(newRoundedPositon).Count == 0)
            return moveablePositions.Contains(newRoundedPositon);
        else if (GridController.gridController.GetObjectAtLocation(newRoundedPositon).Any(x => x.tag == "Enemy"))
            return attackablePositions.Contains(newRoundedPositon);
        return false;
    }

    public void UpdateOrigin(Vector2 newOrigin)
    {
        CommitMove();
        lastGoodPosition = newOrigin;
    }

    public void TeleportTo(Vector2 newOrigin)
    {
        lastGoodPosition = newOrigin;
        originalPosition = newOrigin;
        path = new List<Vector2>();
    }

    public void ResetTurn()
    {
        UpdateOrigin(transform.position);
        movedDistance = 0;
        SetMoveable(true);
        GetComponent<HealthController>().AtStartOfTurn();
    }

    public void SetMoveable(bool newMoveable)
    {
        col2D.enabled = newMoveable;
        moveable = newMoveable;
    }

    public bool GetMoveable()
    {
        return moveable;
    }

    public void CreateMoveRangeIndicator()
    {
        moveShadow.GetComponent<SpriteRenderer>().enabled = true;
        /* Creates move range tiles and gets a list of all moveable locations
         */
        if (moveable)
        {
            GridController.gridController.RemoveFromPosition(this.gameObject, transform.position);
            TileCreator.tileCreator.CreateTiles(this.gameObject, originalPosition, Card.CastShape.Circle, Mathf.Max(player.GetMoveRange() + healthController.GetBonusMoveRange() - movedDistance, 0),
                                                moveRangeIndicatorColor, new string[] { "Enemy", "Blockade" }, 0);
            moveablePositions = TileCreator.tileCreator.GetTilePositions();
        }
    }

    //Destroys the move and attack range indicators
    public void DestroyMoveRrangeIndicator()
    {
        moveShadow.GetComponent<SpriteRenderer>().enabled = false;
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
        movedDistance += GridController.gridController.GetManhattanDistance(transform.position, originalPosition); //Allow movement after action
        originalPosition = transform.position;
        path = new List<Vector2>();
        TileCreator.tileCreator.DestroyPathTiles(player.GetPlayerID());
        //movedDistance = player.GetMoveRange() + GetComponent<HealthController>().GetBonusMoveRange(); //Disable movement after action
    }

    public void ChangeMoveDistance(int value)
    {
        movedDistance += value;
    }

    //Moves the player to the location
    public void MoveTo(Vector2 location)
    {
        GridController.gridController.ReportPosition(this.gameObject, location);
        transform.position = location;
        moveShadow.transform.position = location;
        lastGoodPosition = transform.position;

        foreach (EnemyController enemy in TurnController.turnController.GetEnemies())
        {
            enemy.GetComponent<EnemyInformationController>().RefreshIntent();
        }
    }

    private void OnMouseDown()
    {
        clickedTime = DateTime.Now;
        clickedLocation = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));

        GetComponent<HealthController>().ShowHealthBar();
    }

    public void OnMouseUp()
    {
        GetComponent<HealthController>().HideHealthBar();

        if ((DateTime.Now - clickedTime).TotalSeconds < 0.2 && ((Vector2)Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0)) - clickedLocation).magnitude <= 0.3)
        {
            HealthController hlth = GetComponent<HealthController>();
            List<CardController> cards = new List<CardController>();
            CharacterInformationController.charInfoController.SetDescription(GetComponent<HealthController>().charDisplay.sprite.sprite, hlth, cards, hlth.GetBuffs(), GetComponent<AbilitiesController>());
            CharacterInformationController.charInfoController.Show();
        }
    }
}
