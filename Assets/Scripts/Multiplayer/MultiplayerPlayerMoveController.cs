using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.Linq;
using System;
using Mirror;

public class MultiplayerPlayerMoveController : NetworkBehaviour
{

    private MultiplayerPlayerController player;
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

                    for (int i = 0; i < 10; i++)
                    {
                        path = PathFindController.pathFinder.PathFind(originalPosition, roundedPosition, new string[] { "Player", "Enemy" }, new List<Vector2> { Vector2.zero }, 1);
                        if (path.Count < moveRangeLeft)
                            break;
                    }
                    TileCreator.tileCreator.DestroyPathTiles(PartyController.party.GetPartyIndex(player.GetColorTag()));
                    TileCreator.tileCreator.CreatePathTiles(PartyController.party.GetPartyIndex(player.GetColorTag()), path, moveRangeLeft - GridController.gridController.GetManhattanDistance(originalPosition, roundedPosition), moveRangeIndicatorColor);
                }
            }
        }
        else
        {
            moveShadow.transform.position = lastGoodPosition;
            if (lastHoverLocation != lastGoodPosition)
            {
                lastHoverLocation = lastGoodPosition;

                for (int i = 0; i < 10; i++)
                {
                    path = PathFindController.pathFinder.PathFind(originalPosition, lastGoodPosition, new string[] { "Player" }, new List<Vector2> { Vector2.zero }, 1);
                    if (path.Count < moveRangeLeft)
                        break;
                }
                TileCreator.tileCreator.DestroyPathTiles(PartyController.party.GetPartyIndex(player.GetColorTag()));
                TileCreator.tileCreator.CreatePathTiles(PartyController.party.GetPartyIndex(player.GetColorTag()), path, moveRangeLeft - GridController.gridController.GetManhattanDistance(originalPosition, lastGoodPosition), moveRangeIndicatorColor);
            }
        }
    }

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

    public void ResetMoveDistance(int value)
    {
        movedDistance = value;
    }

    public void TeleportTo(Vector2 newOrigin)
    {
        lastGoodPosition = newOrigin;
        originalPosition = newOrigin;
        path = new List<Vector2>();

        foreach (EnemyController enemy in TurnController.turnController.GetEnemies())
            enemy.GetComponent<EnemyInformationController>().RefreshIntent();
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
        string selfTag = "Player";
        string otherTag = "Enemy";

        moveShadow.GetComponent<SpriteRenderer>().enabled = true;
        /* Creates move range tiles and gets a list of all moveable locations
         */
        if (moveable)
        {
            if (healthController.GetStunned())
            {
                GridController.gridController.RemoveFromPosition(this.gameObject, transform.position);
                TileCreator.tileCreator.CreateTiles(this.gameObject, originalPosition, Card.CastShape.Circle, 0, PartyController.party.GetPlayerColor(player.GetColorTag()), new string[] { otherTag, "Blockade" }, 0);
            }
            else
            {
                GridController.gridController.RemoveFromPosition(this.gameObject, transform.position);

                if (!healthController.GetPhasedMovement())
                {
                    TileCreator.tileCreator.CreateTiles(this.gameObject, originalPosition, Card.CastShape.Circle, Mathf.Max(player.GetMoveRange() + healthController.GetBonusMoveRange() - movedDistance, 0),
                                                        PartyController.party.GetPlayerColor(player.GetColorTag()), new string[] { otherTag, "Blockade" }, 0);
                    if (SettingsController.settings.GetRemainingMoveRangeIndicator() && path.Count > 1)       //If the option is enabled and player moved, create remaining move range indicator
                        TileCreator.tileCreator.CreateTiles(this.gameObject, lastGoodPosition, Card.CastShape.Circle, Mathf.Max(player.GetMoveRange() + healthController.GetBonusMoveRange() - movedDistance - Mathf.Max(path.Count, 1) + 1, 0),
                                                            PartyController.party.GetPlayerColor(player.GetColorTag()) * new Color(0.7f, 0.7f, 0.7f, 0.7f), new string[] { otherTag, "Blockade" }, 1);
                }
                else    //If phased movement, then player can move through, but not on enemies
                {
                    TileCreator.tileCreator.CreateTiles(this.gameObject, originalPosition, Card.CastShape.Circle, Mathf.Max(player.GetMoveRange() + healthController.GetBonusMoveRange() - movedDistance, 0),
                                                    PartyController.party.GetPlayerColor(player.GetColorTag()), new string[] { "Blockade" }, 0);    //Does not avoid Enemies for move range calculation
                    List<Vector2> destroyLocs = new List<Vector2>();
                    foreach (Vector2 loc in TileCreator.tileCreator.GetTilePositions(0))        //Remove all positions with enemies, can't move onto them
                        if (GridController.gridController.GetObjectAtLocation(loc, new string[] { otherTag }).Count > 0)
                            destroyLocs.Add(loc);
                    TileCreator.tileCreator.DestroySpecificTiles(this.gameObject, destroyLocs, 0);

                    if (SettingsController.settings.GetRemainingMoveRangeIndicator() && path.Count > 1)       //If the option is enabled and player moved, create remaining move range indicator
                    {
                        TileCreator.tileCreator.CreateTiles(this.gameObject, originalPosition, Card.CastShape.Circle, Mathf.Max(player.GetMoveRange() + healthController.GetBonusMoveRange() - movedDistance - Mathf.Max(path.Count, 1) + 1, 0),
                                                    PartyController.party.GetPlayerColor(player.GetColorTag()) * new Color(0.7f, 0.7f, 0.7f, 0.7f), new string[] { "Blockade" }, 1);    //Does not avoid Enemies for move range calculation
                        destroyLocs = new List<Vector2>();
                        foreach (Vector2 loc in TileCreator.tileCreator.GetTilePositions(1))        //Remove all positions with enemies, can't move onto them
                            if (GridController.gridController.GetObjectAtLocation(loc, new string[] { otherTag }).Count > 0)
                                destroyLocs.Add(loc);
                        TileCreator.tileCreator.DestroySpecificTiles(this.gameObject, destroyLocs, 1);
                    }
                }

                HealthController taunt = healthController.GetTauntedTarget();
                if (taunt != null)      //If the player is taunted
                {
                    TileCreator.tileCreator.DestroyTiles(this.gameObject, 1);
                    int baseMovement = PathFindController.pathFinder.PathFind(originalPosition, taunt.transform.position, new string[] { selfTag }, healthController.GetOccupiedSpaces(), 1).Count;    //Find all positions the player could move if not taunted
                    baseMovement = Mathf.Max(baseMovement, PathFindController.pathFinder.PathFind(lastGoodPosition, taunt.transform.position, new string[] { selfTag }, healthController.GetOccupiedSpaces(), 1).Count);   //If the taunted target has been force moved, use lastGoodPosition or original location, whichever one is further away
                    List<Vector2> destroyLocs = new List<Vector2>();
                    foreach (Vector2 loc in TileCreator.tileCreator.GetTilePositions(0))                                                                                                                //Remove all move positions that'll take the player further to the taunted target
                        if (PathFindController.pathFinder.PathFind(loc, taunt.transform.position, new string[] { selfTag }, healthController.GetOccupiedSpaces(), 1).Count > baseMovement)
                            destroyLocs.Add(loc);
                    TileCreator.tileCreator.DestroySpecificTiles(this.gameObject, destroyLocs, 0);

                    if (!healthController.GetPhasedMovement())
                        TileCreator.tileCreator.CreateTiles(this.gameObject, originalPosition, Card.CastShape.Circle, Mathf.Max(player.GetMoveRange() + healthController.GetBonusMoveRange() - movedDistance, 0), //Draw faded tiles on where the player could have moved if not taunted
                                                    PartyController.party.GetPlayerColor(player.GetColorTag()) * new Color(0.7f, 0.7f, 0.7f, 0.7f), new string[] { otherTag, "Blockade" }, 1);
                    else    //If phased movement, then player can move through, but not on enemies
                    {
                        TileCreator.tileCreator.CreateTiles(this.gameObject, originalPosition, Card.CastShape.Circle, Mathf.Max(player.GetMoveRange() + healthController.GetBonusMoveRange() - movedDistance, 0), //Draw faded tiles on where the player could have moved if not taunted
                                                    PartyController.party.GetPlayerColor(player.GetColorTag()) * new Color(0.7f, 0.7f, 0.7f, 0.7f), new string[] { "Blockade" }, 1);    //Does not avoid Enemies for move range calculation
                        destroyLocs = new List<Vector2>();
                        foreach (Vector2 loc in TileCreator.tileCreator.GetTilePositions(1))        //Remove all positions with enemies, can't move onto them
                            if (GridController.gridController.GetObjectAtLocation(loc, new string[] { otherTag }).Count > 0)
                                destroyLocs.Add(loc);
                        TileCreator.tileCreator.DestroySpecificTiles(this.gameObject, destroyLocs, 1);
                    }
                }
            }
            moveablePositions = TileCreator.tileCreator.GetTilePositions();
        }
    }

    //Destroys the move and attack range indicators
    public void DestroyMoveRrangeIndicator()
    {
        moveShadow.GetComponent<SpriteRenderer>().enabled = false;
        TileCreator.tileCreator.DestroyTiles(this.gameObject);
        moveablePositions = new List<Vector2>();
        attackablePositions = new List<Vector2>();
    }

    public void SetPlayerController(MultiplayerPlayerController newPlayer)
    {
        player = newPlayer;
    }

    //Move perminantly sets the player location after action (called from cards)
    public void CommitMove()
    {
        movedDistance += GridController.gridController.GetManhattanDistance(transform.position, originalPosition); //Allow movement after action
        originalPosition = transform.position;
        path = new List<Vector2>();
        TileCreator.tileCreator.DestroyPathTiles(PartyController.party.GetPartyIndex(player.GetColorTag()));
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
            enemy.GetComponent<EnemyInformationController>().RefreshIntent();

        HandController.handController.ResetCardPlayability(TurnController.turnController.GetCurrentEnergy(), TurnController.turnController.GetCurrentMana());
    }

    public int GetMovedDistance()
    {
        return movedDistance + GridController.gridController.GetManhattanDistance(moveShadow.transform.position, originalPosition);
    }

    private void OnMouseDown()
    {
        CameraController.camera.ScreenShake(0.03f, 0.1f);

        clickedTime = DateTime.Now;
        clickedLocation = CameraController.camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));

        healthController.ShowHealthBar();
    }

    public void CreateEnemyRangeIndicator()
    {
        string selfTag = "Player";
        if (gameObject.tag == "Player")
            selfTag = "Enemy";

        string otherTag = "Enemy";

        Color moveRangeColor = PartyController.party.GetPlayerColor(Card.CasterColor.Enemy);
        Color attackRangeColor = Color.red;

        int bonusMoveRange = GetComponent<HealthController>().GetBonusMoveRange();

        string[] avoidTags = new string[0];
        if (!GetComponent<HealthController>().GetPhasedMovement())
            avoidTags = new string[] { selfTag, "Blockade" };
        else
            avoidTags = new string[] { "Blockade" };

        foreach (Vector2 vec in GetComponent<HealthController>().GetOccupiedSpaces())
            TileCreator.tileCreator.CreateTiles(this.gameObject, (Vector2)transform.position + vec, Card.CastShape.Circle, player.GetMoveRange() + bonusMoveRange, moveRangeColor, avoidTags, 1);

        if (GetComponent<HealthController>().GetPhasedMovement())
        {
            List<Vector2> destroyLocs = new List<Vector2>();
            foreach (Vector2 loc in TileCreator.tileCreator.GetTilePositions(1))
                if (GridController.gridController.GetObjectAtLocation(loc).Count != 0)
                    destroyLocs.Add(loc);
            TileCreator.tileCreator.DestroySpecificTiles(this.gameObject, destroyLocs, 1);
        }

        List<Vector2> movePositions = TileCreator.tileCreator.GetTilePositions(1);

        //Create attackable locations
        foreach (Vector2 position in movePositions)
            TileCreator.tileCreator.CreateTiles(this.gameObject, position, Card.CastShape.Circle, player.GetAttackRange(), attackRangeColor, new string[] { "" }, 2);
    }

    public void OnMouseUp()
    {
        CameraController.camera.ScreenShake(0.03f, 0.1f);

        TileCreator.tileCreator.DestroyTiles(this.gameObject);
        GetComponent<HealthController>().HideHealthBar();

        if ((DateTime.Now - clickedTime).TotalSeconds < 0.2 && ((Vector2)CameraController.camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0)) - clickedLocation).magnitude <= 0.3)
        {
            List<CardController> cards = new List<CardController>();
            CharacterInformationController.charInfoController.SetDescription(GetComponent<HealthController>().charDisplay.sprite.sprite, healthController, cards, healthController.GetBuffController().GetBuffs(), GetComponent<AbilitiesController>());
            CharacterInformationController.charInfoController.Show();
        }

        if (gameObject.tag == "Enemy")
            return;

        if ((Vector2)transform.position != lastGoodPosition)
            StartCoroutine(healthController.GetBuffController().TriggerBuff(Buff.TriggerType.OnMove, healthController, GridController.gridController.GetManhattanDistance(transform.position, lastGoodPosition)));

        HandController.handController.ResetCardDisplays();
    }
}
