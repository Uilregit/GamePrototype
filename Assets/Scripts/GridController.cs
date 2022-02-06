using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;

public class GridController : MonoBehaviour
{
    public static GridController gridController;

    public float jitter;

    [SerializeField]
    private int xSize, ySize, xOffset, yOffset;
    //private GameObject[,] objects;
    public List<GameObject>[,] objects;
    public bool[,] pathBlocks;

    private Dictionary<Card.CasterColor, Vector2> deathLocation;

    public List<TrapController> traps = new List<TrapController>();

    // Start is called before the first frame update
    void Awake()
    {
        if (GridController.gridController == null)
            GridController.gridController = this;
        else
            Destroy(this.gameObject);

        ResetGrid();
    }

    public void ResetGrid()
    {
        objects = new List<GameObject>[xSize, ySize];
        pathBlocks = new bool[xSize, ySize];
        for (int x = 0; x < xSize; x++)
            for (int y = 0; y < ySize; y++)
            {
                objects[x, y] = new List<GameObject>();
                pathBlocks[x, y] = false;
            }

        deathLocation = new Dictionary<Card.CasterColor, Vector2>();
    }

    public int[] GetRoomRange()
    {
        int[] output = { -xOffset, xSize - xOffset, -yOffset, ySize - yOffset };
        return output;
    }

    //Returns the hashset of all locations that can be moved to
    private HashSet<Vector2> GetMovableLocationSet(HashSet<Vector2> clearedLocations, Vector2 startingLocation, int moveAmount)
    {
        //Recurse to find all grid locations that are movable
        if (objects[(int)startingLocation.x, (int)startingLocation.y] == null) //Skip grid locations with objects in them
        {
            clearedLocations.Add(startingLocation);
            clearedLocations.UnionWith(GetMovableLocationSet(clearedLocations, new Vector2(startingLocation.x - 1, startingLocation.y - 1), moveAmount - 1));
            clearedLocations.UnionWith(GetMovableLocationSet(clearedLocations, new Vector2(startingLocation.x - 1, startingLocation.y + 1), moveAmount - 1));
            clearedLocations.UnionWith(GetMovableLocationSet(clearedLocations, new Vector2(startingLocation.x + 1, startingLocation.y - 1), moveAmount - 1));
            clearedLocations.UnionWith(GetMovableLocationSet(clearedLocations, new Vector2(startingLocation.x + 1, startingLocation.y + 1), moveAmount - 1));
        }
        return clearedLocations;
    }

    //Adds the reporting object into the location inside of the object grid
    public void ReportPosition(GameObject obj, Vector2 location)
    {
        int xLoc = Mathf.RoundToInt(location.x);
        int yLoc = Mathf.RoundToInt(location.y);

        objects[xLoc + xOffset, yLoc + yOffset].Add(obj);

        if (objects[xLoc + xOffset, yLoc + yOffset].Count > 1 && objects[xLoc + xOffset, yLoc + yOffset].Any(x => x.tag == "Player"))
            foreach (GameObject o in objects[xLoc + xOffset, yLoc + yOffset])
                if (o.tag == "Player")
                    o.GetComponent<Collider2D>().enabled = true;
                else
                    o.GetComponent<Collider2D>().enabled = false;

        if (objects[xLoc + xOffset, yLoc + yOffset].Count > 1)
        {
            AchievementSystem.achieve.OnNotify(objects[xLoc + xOffset, yLoc + yOffset].Count, StoryRoomSetup.ChallengeType.CharsStacked);
            int enemyCount = objects[xLoc + xOffset, yLoc + yOffset].Select(x => !x.GetComponent<HealthController>().isPlayer).Count();
            if (enemyCount > 1)
                TutorialController.tutorial.TriggerTutorial(Dialogue.Condition.EnemiesStacked, enemyCount);
        }

        /*
        if (objects[xLoc + xOffset, yLoc + yOffset].Count > 1)
        {
            Vector3 jitterVec = (Vector3)Random.insideUnitCircle * jitter;
            try
            {
                obj.GetComponent<PlayerController>().sprite.transform.position = obj.transform.position + jitterVec;
                obj.GetComponent<PlayerController>().shadow.transform.position = obj.transform.position + jitterVec;
            }
            catch
            {
                obj.GetComponent<EnemyController>().sprite.transform.position = obj.transform.position + jitterVec;
                obj.GetComponent<EnemyController>().shadow.transform.position = obj.transform.position + jitterVec;
            }
        }
        */
        try
        {
            GetComponent<MultiplayerGridController>().SetGrid(objects);
            //MultiplayerInformationController.player.DebugReportGrid(DebugGrid(), MultiplayerInformationController.player.GetPlayerNumber());
            int playerNumber = ClientScene.localPlayer.GetComponent<MultiplayerInformationController>().GetPlayerNumber();
            ClientScene.localPlayer.GetComponent<MultiplayerInformationController>().ReportGrid(GetComponent<MultiplayerGridController>().GetGrid(), playerNumber);
        }
        catch { }
    }

    //Remove the object at the object grid location
    public void RemoveFromPosition(GameObject obj, Vector2 location)
    {
        int xLoc = Mathf.RoundToInt(location.x);
        int yLoc = Mathf.RoundToInt(location.y);

        objects[xLoc + xOffset, yLoc + yOffset].Remove(obj);

        if (obj.tag == "Player" && objects[xLoc + xOffset, yLoc + yOffset].Count > 0)
            foreach (GameObject o in objects[xLoc + xOffset, yLoc + yOffset])
                o.GetComponent<Collider2D>().enabled = true;

        try
        {
            GetComponent<MultiplayerGridController>().SetGrid(objects);
            //MultiplayerInformationController.player.DebugReportGrid(DebugGrid(), MultiplayerInformationController.player.GetPlayerNumber());
            int playerNumber = ClientScene.localPlayer.GetComponent<MultiplayerInformationController>().GetPlayerNumber();
            ClientScene.localPlayer.GetComponent<MultiplayerInformationController>().ReportGrid(GetComponent<MultiplayerGridController>().GetGrid(), playerNumber);
        }
        catch { }
    }

    //Return the object at the grid location. If nothing, returns null
    public List<GameObject> GetObjectAtLocation(Vector2 location, string[] withTags)
    {
        int xLoc = Mathf.RoundToInt(location.x);
        int yLoc = Mathf.RoundToInt(location.y);

        if (withTags.Contains("All"))
            withTags = new string[] { "Player", "Enemy" };

        List<GameObject> output = new List<GameObject>();
        if (CheckIfOutOfBounds(location))
            return output;
        foreach (GameObject obj in objects[(xLoc + xOffset), (yLoc + yOffset)])
            if (withTags.Contains(obj.tag))
                output.Add(obj);
        return output.Distinct().ToList();
    }

    public List<GameObject> GetObjectAtLocation(Vector2 location)
    {
        int xLoc = Mathf.RoundToInt(location.x);
        int yLoc = Mathf.RoundToInt(location.y);

        if (CheckIfOutOfBounds(location))
            return new List<GameObject>();
        return objects[(xLoc + xOffset), (yLoc + yOffset)].Distinct().ToList();
    }

    public List<GameObject> GetObjectAtLocation(List<Vector2> locations, string[] withTags)
    {
        List<GameObject> output = new List<GameObject>();

        foreach (Vector2 location in locations)
            output.AddRange(GetObjectAtLocation(location, withTags));

        return output.Distinct().ToList();
    }

    public List<GameObject> GetObjectAtLocation(List<Vector2> locations)
    {
        List<GameObject> output = new List<GameObject>();

        foreach (Vector2 location in locations)
            output.AddRange(GetObjectAtLocation(location, new string[] { "Player", "Enemy" }));

        return output.Distinct().ToList();
    }

    public List<GameObject> GetObjectsInAoE(Vector2 center, int range, string[] tag)
    {
        TileCreator.tileCreator.CreateTiles(this.gameObject, center, Card.CastShape.Circle, range, Color.green, 2);
        List<Vector2> locations = TileCreator.tileCreator.GetTilePositions(2);
        TileCreator.tileCreator.DestroyTiles(this.gameObject, 2);

        return GetObjectAtLocation(locations, tag).Distinct().ToList();
    }

    public List<Vector2> GetLocationsInAoE(Vector2 center, int range, string[] tag)
    {
        List<Vector2> output = new List<Vector2>();
        foreach (GameObject obj in GetObjectsInAoE(center, range, tag))
            output.Add(obj.transform.position);
        return output.Distinct().ToList();
    }

    public List<Vector2> GetEmptyLocationsInAoE(Vector2 center, int range)
    {
        List<Vector2> output = new List<Vector2>();
        for (int x = 0; x < xSize; x++)
            for (int y = 0; y < ySize; y++)
                if (objects[x, y].Count == 0 && GetManhattanDistance(new Vector2(x - xOffset, y - yOffset), center) <= range)
                    output.Add(new Vector2(x - xOffset, y - yOffset));
        return output.Distinct().ToList();
    }

    public List<Vector2> GetEmptyTrapLocationsInAoE(Vector2 center, int range)
    {
        TileCreator.tileCreator.CreateTiles(this.gameObject, center, Card.CastShape.Circle, range, Color.clear, 2);
        List<Vector2> output = TileCreator.tileCreator.GetTilePositions(2);
        TileCreator.tileCreator.DestroyTiles(this.gameObject, 2);

        foreach (TrapController t in traps)
            if (GetManhattanDistance(t.transform.position, center) <= range)
                output.Remove(t.transform.position);

        return output;
    }

    public void SetPathBlocked(Vector2 location, bool state)
    {
        int xLoc = Mathf.RoundToInt(location.x);
        int yLoc = Mathf.RoundToInt(location.y);

        pathBlocks[(xLoc + xOffset), (yLoc + yOffset)] = state;
    }

    public bool GetPathBlocked(Vector2 location)
    {
        int xLoc = Mathf.RoundToInt(location.x);
        int yLoc = Mathf.RoundToInt(location.y);

        return pathBlocks[(xLoc + xOffset), (yLoc + yOffset)];
    }

    //Returns true for out of bounds positions, false if not
    public bool CheckIfOutOfBounds(Vector2 location)
    {
        int xLoc = Mathf.RoundToInt(location.x);
        int yLoc = Mathf.RoundToInt(location.y);

        try
        {
            List<GameObject> test = objects[xLoc + xOffset, yLoc + yOffset];
            return false;
        }
        catch
        {
            return true;
        }
    }

    public bool CheckIfOutOfBounds(List<Vector2> locations)
    {
        foreach (Vector2 location in locations)
            if (CheckIfOutOfBounds(location))
                return true;
        return false;
    }

    public int GetManhattanDistance(Vector2 loc1, Vector2 loc2)
    {
        return (int)(Mathf.Abs(loc1.x - loc2.x) + Mathf.Abs(loc1.y - loc2.y));
    }

    public void DisableAllPlayers()
    {
        //Disable Player and card movement, trigger all end of turn effects
        GameObject[] players = GameController.gameController.GetLivingPlayers().ToArray();
        foreach (GameObject player in players)
            player.GetComponent<PlayerMoveController>().SetMoveable(false);
    }

    public IEnumerator CheckDeath()
    {
        List<GameObject> deadObjects = new List<GameObject>();
        List<GameObject> viableObjects = new List<GameObject>();
        foreach (List<GameObject> obje in objects)
            viableObjects.AddRange(obje);

        if (MultiplayerGameController.gameController != null)
        {
            viableObjects = MultiplayerGameController.gameController.GetLivingPlayers(0);
            viableObjects.AddRange(MultiplayerGameController.gameController.GetLivingPlayers(1));
        }
        foreach (GameObject obj in viableObjects)
            if (obj != null)
                try
                {
                    GameObject o = obj.GetComponent<HealthController>().CheckDeath();
                    if (o != null)
                        deadObjects.Add(o);
                }
                catch { }

        //Resolve enemy deaths
        foreach (GameObject o in deadObjects)
        {
            if (o.transform.position != new Vector3(1000, 1000))
            {
                RemoveFromPosition(o, o.transform.position);
                o.GetComponent<HealthController>().charDisplay.charAnimController.TriggerDeath();
                try
                {
                    TurnController.turnController.RemoveEnemy(o.GetComponent<EnemyController>());
                    o.GetComponent<HealthController>().ReportDead();
                }
                catch { }
            }
        }

        //Resolve sacrificed enemies
        foreach (EnemyController ec in TurnController.turnController.GetEnemies())
        {
            if (ec.GetSacrificed())
            {
                ec.GetComponent<HealthController>().charDisplay.charAnimController.TriggerDeath();
                TurnController.turnController.RemoveEnemy(ec);
                ec.GetComponent<HealthController>().ReportDead();
            }
        }

        yield return new WaitForEndOfFrame();
        if (GameController.gameController != null)
        {
            if (TurnController.turnController.GetNumberOfEnemies() <= 0)
            {
                TurnController.turnController.ResetCurrentEnergy();
                TurnController.turnController.ResetEnergyDisplay();
                TurnController.turnController.StopAllCoroutines();
                yield return StartCoroutine(GameController.gameController.Victory());
            }
        }

        yield return new WaitForSeconds(0);
    }

    public void ReportPlayerDead(GameObject obj, Card.CasterColor color)
    {
        deathLocation[color] = obj.transform.position;
    }

    public void OnPlayerDeath(GameObject obj, Card.CasterColor color)
    {
        obj.transform.position = new Vector2(1000, 1000);     //Move out of the way for possible resurrection
        obj.GetComponent<HealthController>().ReportDead();
    }

    public Vector2 GetDeathLocation(Card.CasterColor color)
    {
        return deathLocation[color];
    }

    public void RemoveDeathLocation(Card.CasterColor color)
    {
        deathLocation.Remove(color);
    }

    public void ResolveOverlap()
    {
        for (int x = 0; x < xSize; x++)
            for (int y = 0; y < ySize; y++)
                if (objects[x, y].Count > 1)                    //Iterate through all objects in all grids, resolve overlap if there is more than 1 object in the position
                {
                    GameObject temp;
                    while (objects[x, y].Count > 1)             //Iterate through all objects in the grid except the last one
                    {
                        temp = objects[x, y][0];

                        Vector2 selectedLocation = new Vector2();
                        List<Vector2> occupiedSpaces = temp.GetComponent<HealthController>().GetOccupiedSpaces();

                        selectedLocation = FindNearestEmptyLocation(temp.transform.position, occupiedSpaces, temp.GetComponent<HealthController>().size);

                        foreach (Vector2 loc in occupiedSpaces)
                            RemoveFromPosition(temp, (Vector2)temp.transform.position + loc);
                        temp.transform.position = GetRoundedVector(selectedLocation, temp.GetComponent<HealthController>().size);
                        foreach (Vector2 loc in occupiedSpaces)
                            ReportPosition(temp, (Vector2)temp.transform.position + loc);
                        /*
                        try
                        {
                            temp.GetComponent<PlayerController>().sprite.transform.position = temp.transform.position;
                            temp.GetComponent<PlayerController>().shadow.transform.position = temp.transform.position;
                        }
                        catch
                        {
                            temp.GetComponent<EnemyController>().sprite.transform.position = temp.transform.position;
                            temp.GetComponent<EnemyController>().shadow.transform.position = temp.transform.position;
                        }
                        */
                        temp.GetComponent<HealthController>().SetStatTexts(true);       //Renable stat texts after no longer being overlapped
                    }
                    objects[x, y][0].GetComponent<HealthController>().SetStatTexts(true);
                }
                else if (objects[x, y].Count == 1)
                {
                    try
                    {
                        objects[x, y][0].GetComponent<HealthController>().SetStatTexts(true);
                    }
                    catch { };
                }
    }

    public Vector2 FindNearestEmptyLocation(Vector2 startingLoc, List<Vector2> occupiedLocations, int size)
    {
        Dictionary<int, Vector2> locations = new Dictionary<int, Vector2>();
        for (int x = -3; x < 3; x++)
            for (int y = -3; y < 3; y++)
            {
                Vector2 newLoc = GetRoundedVector(startingLoc, size) + new Vector2(x, y);
                //Debug.Log(newLoc);
                List<Vector2> newLocs = new List<Vector2>();
                foreach (Vector2 loc in occupiedLocations)
                    newLocs.Add(newLoc + loc);
                if (!CheckIfOutOfBounds(newLocs) && GetObjectAtLocation(newLocs, new string[] { "Player", "Enemy", "Blockade" }).Count == 0)
                    locations[GetManhattanDistance(startingLoc, newLoc)] = newLoc;
            }
        //DebugPlus.LogOnScreen(locations.Keys.ToString()).Duration(10);
        return locations[locations.Keys.Min()];
    }

    public string DebugGrid()
    {
        //DebugPlus.LogOnScreen("########").Duration(10);
        string s = "";
        for (int y = ySize - 1; y >= 0; y--)
        {
            for (int x = 0; x < xSize; x++)
                s += objects[x, y].Count;
            s += "\n";
            //DebugPlus.LogOnScreen(s).Duration(10);
        }
        //Debug.Log(s);
        return s;
    }

    public Vector2 GetRoundedVector(Vector2 input, int size)
    {
        return new Vector2(Mathf.Round(input.x * size) / size, Mathf.Round(input.y * size) / size);
    }

    public int GetIndexAtPosition(GameObject obj, Vector2 location)
    {
        List<GameObject> objects = GetObjectAtLocation(location);
        for (int i = 0; i < objects.Count; i++)
            if (objects[i] == obj)
                return i;
        return 0;

    }

    public void ResetOverlapOrder(Vector2 position)
    {
        List<GameObject> objects = GetObjectAtLocation(position);

        int minHealth = 999999;
        bool anyBroken = false;
        GameObject topObject = null;

        foreach (GameObject obj in objects)
        {
            if (obj.GetComponent<HealthController>() == null)
                continue;

            int health = obj.GetComponent<HealthController>().GetCurrentVit();
            int armor = obj.GetComponent<HealthController>().GetCurrentArmor();

            try
            {
                obj.GetComponent<PlayerController>();
                obj.transform.SetAsLastSibling();
                topObject = obj;
                minHealth = -9999;              //Ensure no enemy will ever be on top of a player
                anyBroken = true;               //Ensure no enemy, even when broken, will ever be on top of a player
            }
            catch
            {
                if (health == 0)                                 //If the object is already dead, send to back
                    obj.transform.SetAsFirstSibling();
                else if (armor == 0)                            //If the object is broken, either set to top if it's the first broken enemy, or sort by health if there are multiple broken enemies
                {
                    if (anyBroken)
                    {
                        if (health < minHealth)
                        {
                            minHealth = health;
                            obj.transform.SetAsLastSibling();
                            topObject = obj;
                        }
                    }
                    else
                    {
                        anyBroken = true;
                        minHealth = health;
                        obj.transform.SetAsLastSibling();
                        topObject = obj;
                    }
                }
                else                                        //If the object is not broken, then sort by lowest health being on top
                {
                    if (health < minHealth)
                    {
                        minHealth = health;
                        obj.transform.SetAsLastSibling();
                        topObject = obj;
                    }
                }
            }
            foreach (GameObject o in objects)           //Only allow the top object to display it's stats
            {
                if (topObject == o)
                    o.GetComponent<HealthController>().SetStatTexts(true);
                else
                    o.GetComponent<HealthController>().SetStatTexts(false);
            }
        }
    }

    public void SetGrid(List<GameObject>[,] value)
    {
        objects = value;
        for (int x = 0; x < xSize; x++)
            for (int y = 0; y < ySize; y++)
                foreach (GameObject obj in value[x, y])
                    obj.transform.position = new Vector3(x - xOffset, y - yOffset, 0);
        //DebugGrid();
    }

    public Vector2 GetGridLocation(Vector2 transformPosition)
    {
        return new Vector2((int)transformPosition.x + xOffset, (int)transformPosition.y + yOffset);
    }
}
