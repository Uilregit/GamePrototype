using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridController : MonoBehaviour
{
    public static GridController gridController;

    [SerializeField]
    private int xSize, ySize, xOffset, yOffset;
    private GameObject[,] objects;

    // Start is called before the first frame update
    void Awake()
    {
        if (GridController.gridController == null)
            GridController.gridController = this;
        else
            Destroy(this.gameObject);

        objects = new GameObject[xSize, ySize];
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

        objects[xLoc + xOffset, yLoc + yOffset] = obj;
    }

    //Remove the object at the object grid location
    public void RemoveFromPosition(Vector2 location)
    {
        int xLoc = Mathf.RoundToInt(location.x);
        int yLoc = Mathf.RoundToInt(location.y);

        objects[xLoc + xOffset, yLoc + yOffset] = null;
    }

    //Return the object at the grid location. If nothing, returns null
    public GameObject GetObjectAtLocation(Vector2 location)
    {
        int xLoc = Mathf.RoundToInt(location.x);
        int yLoc = Mathf.RoundToInt(location.y);

        try
        {
            return objects[xLoc + xOffset, yLoc + yOffset];
        }
        catch
        {
            return null;
        }
    }

    public GameObject[] GetObjectsInAoE(Vector2 center, int range, string tag)
    {
        List<GameObject> output = new List<GameObject>();
        foreach (GameObject obj in objects)
            if (obj != null)
                if (obj.tag.Contains(tag) && GetManhattanDistance(obj.transform.position, center) <= range)
                    output.Add(obj);
        return output.ToArray();
    }

    //Returns true for out of bounds positions, false if not
    public bool CheckIfOutOfBounds(Vector2 location)
    {
        int xLoc = Mathf.RoundToInt(location.x);
        int yLoc = Mathf.RoundToInt(location.y);

        try
        {
            GameObject test = objects[xLoc + xOffset, yLoc + yOffset];
            return false;
        }
        catch
        {
            return true;
        }
    }

    public int GetManhattanDistance(Vector2 loc1, Vector2 loc2)
    {
        return (int)(Mathf.Abs(loc1.x - loc2.x) + Mathf.Abs(loc1.y - loc2.y));
    }

    public void DisableAllPlayers()
    {
        //Disable Player and card movement, trigger all end of turn effects
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
            player.GetComponent<PlayerMoveController>().SetMoveable(false);
    }
}
