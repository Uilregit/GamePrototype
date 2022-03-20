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
    public HealthController healthController;
    private int movedDistance = 0;
    public Color moveRangeIndicatorColor;
    public Color attackRangeIndicatorColor;

    private Vector2 originalPosition;
    private Vector2 lastGoodPosition;
    private Vector2 previousPosition;
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

    //Achievement tracking
    private List<Vector2> castFromLocation = new List<Vector2>();

    private void Awake()
    {
        col2D = GetComponent<Collider2D>();
        healthController = GetComponent<HealthController>();
    }

    public void Spawn()
    {
        lastGoodPosition = transform.position;
        previousPosition = transform.position;
        lastHoverLocation = transform.position;
        originalPosition = transform.position;
        movedDistance = 0;
        path = new List<Vector2>();
        TileCreator.tileCreator.DestroyTiles(this.gameObject);
        TileCreator.tileCreator.DestroyPathTiles(PartyController.party.GetPartyIndex(player.GetColorTag()));
        //moveShadow.GetComponent<SpriteRenderer>().sprite = GetComponent<PlayerController>().sprite.sprite;
        moveShadow.GetComponent<SpriteRenderer>().enabled = false;
    }

    //Moves the moveshadow to the rounded unit tile
    public void UpdateMovePosition(Vector2 newlocation)
    {
        Vector2 roundedPosition = GridController.gridController.GetRoundedVector(newlocation, GetComponent<HealthController>().size);
        int moveRangeLeft = Mathf.Max(player.GetMoveRange() + healthController.GetBonusMoveRange() - movedDistance, 0);

        if (CheckIfPositionValid(roundedPosition))          //If it's a valid pathable position, draw path preview to location
        {
            if (GridController.gridController.GetObjectAtLocation(roundedPosition).Count == 0)
            {
                moveShadow.transform.position = roundedPosition;
                if (lastHoverLocation != roundedPosition)
                {
                    lastHoverLocation = roundedPosition;

                    for (int i = 0; i < 10; i++)
                    {
                        string[] avoidTag = new string[] { "Player" };
                        if (healthController.GetPhasedMovement())
                            avoidTag = new string[] { "Player", "Enemy", "Blockade" };
                        path = PathFindController.pathFinder.PathFind(originalPosition, roundedPosition, avoidTag, new List<Vector2> { Vector2.zero }, 1);

                        if (path.Count < moveRangeLeft)
                            break;
                    }
                    TileCreator.tileCreator.DestroyPathTiles(PartyController.party.GetPartyIndex(player.GetColorTag()));
                    TileCreator.tileCreator.CreatePathTiles(PartyController.party.GetPartyIndex(player.GetColorTag()), path, moveRangeLeft - path.Count + 1, moveRangeIndicatorColor);
                }
            }
        }
        else                                                //If final loc is not valid, draw path preview to last good position
        {
            moveShadow.transform.position = lastGoodPosition;
            if (lastHoverLocation != lastGoodPosition)
            {
                lastHoverLocation = lastGoodPosition;

                for (int i = 0; i < 10; i++)
                {
                    string[] avoidTag = new string[] { "Player" };
                    if (healthController.GetPhasedMovement())
                        avoidTag = new string[] { "Player", "Enemy", "Blockade" };
                    path = PathFindController.pathFinder.PathFind(originalPosition, lastGoodPosition, avoidTag, new List<Vector2> { Vector2.zero }, 1);
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
        previousPosition = lastGoodPosition;
        lastGoodPosition = newOrigin;
    }

    public void ResetMoveDistance(int value)
    {
        movedDistance = value;
    }

    public void TeleportTo(Vector2 newOrigin)
    {
        previousPosition = lastGoodPosition;
        lastGoodPosition = newOrigin;
        originalPosition = newOrigin;
        path = new List<Vector2>();

        foreach (EnemyController enemy in TurnController.turnController.GetEnemies())
            enemy.GetComponent<EnemyInformationController>().RefreshIntent();
    }

    public void ResetTurn()
    {
        castFromLocation = new List<Vector2>();

        UpdateOrigin(transform.position);
        movedDistance = 0;
        SetMoveable(true);
        GetComponent<HealthController>().AtStartOfTurn();
    }

    public void ReportTurnBasedAchievements()
    {
        AchievementSystem.achieve.OnNotify(castFromLocation.Distinct().ToList().Count, StoryRoomSetup.ChallengeType.CastLocationsPerTurn);
        AchievementSystem.achieve.OnNotify(movedDistance, StoryRoomSetup.ChallengeType.CharDistMovedPerTurn);
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
            if (healthController.GetStunned())
            {
                TileCreator.tileCreator.CreateTiles(this.gameObject, originalPosition, Card.CastShape.Circle, 0, PartyController.party.GetPlayerColor(player.GetColorTag()), new string[] { "Enemy", "Blockade" }, 0);
            }
            else
            {
                if (!healthController.GetPhasedMovement())
                {
                    TileCreator.tileCreator.CreateTiles(this.gameObject, originalPosition, Card.CastShape.Circle, Mathf.Max(player.GetMoveRange() + healthController.GetBonusMoveRange() - movedDistance, 0),
                                                        PartyController.party.GetPlayerColor(player.GetColorTag()), new string[] { "Enemy", "Blockade" }, 0);
                    if (SettingsController.settings.GetRemainingMoveRangeIndicator() && path.Count > 1)       //If the option is enabled and player moved, create remaining move range indicator
                        TileCreator.tileCreator.CreateTiles(this.gameObject, lastGoodPosition, Card.CastShape.Circle, Mathf.Max(player.GetMoveRange() + healthController.GetBonusMoveRange() - movedDistance - Mathf.Max(path.Count, 1) + 1, 0),
                                                            PartyController.party.GetPlayerColor(player.GetColorTag()) * new Color(0.7f, 0.7f, 0.7f, 0.7f), new string[] { "Enemy", "Blockade" }, 1);
                }
                else    //If phased movement, then player can move through, but not on enemies
                {
                    TileCreator.tileCreator.CreateTiles(this.gameObject, originalPosition, Card.CastShape.Circle, Mathf.Max(player.GetMoveRange() + healthController.GetBonusMoveRange() - movedDistance, 0),
                                                    PartyController.party.GetPlayerColor(player.GetColorTag()), new string[] { }, 0);    //Does not avoid Enemies for move range calculation
                    List<Vector2> destroyLocs = new List<Vector2>();
                    foreach (Vector2 loc in TileCreator.tileCreator.GetTilePositions(0))        //Remove all positions with enemies and blockades, can't move onto them
                        if (GridController.gridController.GetObjectAtLocation(loc, new string[] { "Enemy", "Blockade" }).Count > 0)
                            destroyLocs.Add(loc);
                    TileCreator.tileCreator.DestroySpecificTiles(this.gameObject, destroyLocs, 0);

                    if (SettingsController.settings.GetRemainingMoveRangeIndicator() && path.Count > 1)       //If the option is enabled and player moved, create remaining move range indicator
                    {
                        TileCreator.tileCreator.CreateTiles(this.gameObject, originalPosition, Card.CastShape.Circle, Mathf.Max(player.GetMoveRange() + healthController.GetBonusMoveRange() - movedDistance - Mathf.Max(path.Count, 1) + 1, 0),
                                                    PartyController.party.GetPlayerColor(player.GetColorTag()) * new Color(0.7f, 0.7f, 0.7f, 0.7f), new string[] { "Blockade" }, 1);    //Does not avoid Enemies for move range calculation
                        destroyLocs = new List<Vector2>();
                        foreach (Vector2 loc in TileCreator.tileCreator.GetTilePositions(1))        //Remove all positions with enemies, can't move onto them
                            if (GridController.gridController.GetObjectAtLocation(loc, new string[] { "Enemy" }).Count > 0)
                                destroyLocs.Add(loc);
                        TileCreator.tileCreator.DestroySpecificTiles(this.gameObject, destroyLocs, 1);
                    }
                }

                HealthController taunt = healthController.GetTauntedTarget();
                if (taunt != null)      //If the player is taunted
                {
                    TileCreator.tileCreator.DestroyTiles(this.gameObject, 1);
                    int baseMovement = PathFindController.pathFinder.PathFind(originalPosition, taunt.transform.position, new string[] { "Player" }, healthController.GetOccupiedSpaces(), 1).Count;    //Find all positions the player could move if not taunted
                    baseMovement = Mathf.Max(baseMovement, PathFindController.pathFinder.PathFind(lastGoodPosition, taunt.transform.position, new string[] { "Player" }, healthController.GetOccupiedSpaces(), 1).Count);   //If the taunted target has been force moved, use lastGoodPosition or original location, whichever one is further away
                    List<Vector2> destroyLocs = new List<Vector2>();
                    foreach (Vector2 loc in TileCreator.tileCreator.GetTilePositions(0))                                                                                                                //Remove all move positions that'll take the player further to the taunted target
                        if (PathFindController.pathFinder.PathFind(loc, taunt.transform.position, new string[] { "Player" }, healthController.GetOccupiedSpaces(), 1).Count > baseMovement)
                            destroyLocs.Add(loc);
                    TileCreator.tileCreator.DestroySpecificTiles(this.gameObject, destroyLocs, 0);

                    if (!healthController.GetPhasedMovement())
                        TileCreator.tileCreator.CreateTiles(this.gameObject, originalPosition, Card.CastShape.Circle, Mathf.Max(player.GetMoveRange() + healthController.GetBonusMoveRange() - movedDistance, 0), //Draw faded tiles on where the player could have moved if not taunted
                                                    PartyController.party.GetPlayerColor(player.GetColorTag()) * new Color(0.7f, 0.7f, 0.7f, 0.7f), new string[] { "Enemy", "Blockade" }, 1);
                    else    //If phased movement, then player can move through, but not on enemies
                    {
                        TileCreator.tileCreator.CreateTiles(this.gameObject, originalPosition, Card.CastShape.Circle, Mathf.Max(player.GetMoveRange() + healthController.GetBonusMoveRange() - movedDistance, 0), //Draw faded tiles on where the player could have moved if not taunted
                                                    PartyController.party.GetPlayerColor(player.GetColorTag()) * new Color(0.7f, 0.7f, 0.7f, 0.7f), new string[] { }, 1);    //Does not avoid Enemies for move range calculation
                        destroyLocs = new List<Vector2>();
                        foreach (Vector2 loc in TileCreator.tileCreator.GetTilePositions(1))        //Remove all positions with enemies, can't move onto them
                            if (GridController.gridController.GetObjectAtLocation(loc, new string[] { "Enemy" }).Count > 0)
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
        TileCreator.tileCreator.DestroyPathTiles(PartyController.party.GetPartyIndex(player.GetColorTag()));
        //movedDistance = player.GetMoveRange() + GetComponent<HealthController>().GetBonusMoveRange(); //Disable movement after action
    }

    public void ReportCast()
    {
        castFromLocation.Add(transform.position);
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
        previousPosition = lastGoodPosition;
        lastGoodPosition = transform.position;

        foreach (EnemyController enemy in TurnController.turnController.GetEnemies())
            enemy.GetComponent<EnemyInformationController>().RefreshIntent();

        Vector2 gridLocation = GridController.gridController.GetGridLocation(location);
        TutorialController.tutorial.TriggerTutorial(Dialogue.Condition.PlayerPosition, (int)(gridLocation.x * 10 + gridLocation.y));
        TutorialController.tutorial.TriggerTutorial(Dialogue.Condition.PlayerMoved, 1);

        HandController.handController.ResetCardPlayability(TurnController.turnController.GetCurrentEnergy(), TurnController.turnController.GetCurrentMana());
    }

    public int GetMovedDistance()
    {
        return movedDistance + GridController.gridController.GetManhattanDistance(moveShadow.transform.position, originalPosition);
    }

    private void OnMouseDown()
    {
        if (TutorialController.tutorial.GetEnabled())
            return;

        CameraController.camera.ScreenShake(0.03f, 0.1f);

        clickedTime = DateTime.Now;
        clickedLocation = CameraController.camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));

        healthController.ShowHealthBar();
    }

    public void OnMouseUp()
    {
        if (TutorialController.tutorial.GetEnabled())
            return;

        CameraController.camera.ScreenShake(0.03f, 0.1f);

        GetComponent<HealthController>().HideHealthBar();

        if ((DateTime.Now - clickedTime).TotalSeconds < 0.2 && ((Vector2)CameraController.camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0)) - clickedLocation).magnitude <= 0.3)
        {
            List<CardController> cards = new List<CardController>();
            CharacterInformationController.charInfoController.SetDescription(GetComponent<HealthController>().charDisplay.sprite.sprite, healthController, cards, healthController.GetBuffController().GetBuffs(), CollectionController.collectionController.GetEquipmentList(player.GetColorTag()), GetComponent<AbilitiesController>());
            CharacterInformationController.charInfoController.Show();
        }

        if ((Vector2)transform.position != lastGoodPosition)
            for (int i = 0; i < GridController.gridController.GetManhattanDistance(transform.position, lastGoodPosition) + 1; i++)
                StartCoroutine(healthController.GetBuffController().TriggerBuff(Buff.TriggerType.OnMove, healthController, GridController.gridController.GetManhattanDistance(transform.position, lastGoodPosition)));

        HandController.handController.ResetCardDisplays();
        healthController.charDisplay.healthBar.ResetPosition();
    }

    public Vector2 GetPreviousPosition()
    {
        return previousPosition;
    }
}
