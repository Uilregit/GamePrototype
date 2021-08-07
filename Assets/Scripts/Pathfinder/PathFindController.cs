using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PathFindController : MonoBehaviour
{
    public static PathFindController pathFinder;

    private List<AStarNode> openList;
    private List<AStarNode> closedList;

    // Start is called before the first frame update
    void Awake()
    {
        if (PathFindController.pathFinder == null)
            PathFindController.pathFinder = this;
        else
            Destroy(this.gameObject);

        openList = new List<AStarNode>();
        closedList = new List<AStarNode>();
    }

    //Use A* pathfinding to return the full path
    public List<Vector2> PathFind(Vector2 startingLoc, Vector2 endingLoc, string[] pathThroughTag, List<Vector2> occupiedSpaces, int objectSize = 1)
    {
        List<Vector2> output = new List<Vector2>();

        float hCost = GetHCost(startingLoc, endingLoc);
        int gCost = 0;
        AStarNode startingNode = new AStarNode();
        startingNode.position = startingLoc;
        startingNode.gCost = gCost;
        startingNode.hCost = hCost;

        openList.Add(startingNode);

        AStarNode currentNode = openList[0];
        for (int iteration = 0; iteration < 200; iteration++)   //Runs for a max of 200 iterations to prevent infinite loops if unpathable
        {
            if (openList.Count == 0) //If trapped in an enclosed space, return early
                return new List<Vector2>() { startingLoc };

            //Cycle through all positions in open list to find the least costly node
            currentNode = openList[0];
            for (int i = 1; i < openList.Count; i++)
            {
                if (openList[i].fCost < currentNode.fCost ||
                    (openList[i].fCost == currentNode.fCost && openList[i].hCost < currentNode.hCost))
                {
                    currentNode = openList[i];
                }
            }

            //Go to the least costly node
            openList.Remove(currentNode);
            closedList.Add(currentNode);

            //If the current node is the final location, then return the path
            if (Mathf.Abs((currentNode.position - endingLoc).magnitude) <= 0.5f) //If pathfinding finds the ending, then return path list and end early | distance less than one to account for size 2 objects
            {
                output = GetFinalPath(startingNode, currentNode);
                return output;
            }

            foreach (AStarNode neighborNode in GetNeighbours(currentNode, endingLoc, pathThroughTag, occupiedSpaces, objectSize))
            {
                if (currentNode.gCost + 1 < neighborNode.gCost || !ContainsPosition(openList, neighborNode))
                {
                    neighborNode.gCost = currentNode.gCost + 1;
                    neighborNode.parent = currentNode;
                    if (!ContainsPosition(openList, neighborNode))
                        openList.Add(neighborNode);
                }
            }
        }
        return new List<Vector2>() { startingLoc };
    }

    //Return 4 nodes at neighbouring positions if they are valid positions for pathfinding
    private List<AStarNode> GetNeighbours(AStarNode lastNode, Vector2 endingLoc, string[] pathThroughTag, List<Vector2> occupiedSpaces, int objectSize)
    {
        List<AStarNode> output = new List<AStarNode>();
        List<Vector2> directions = new List<Vector2> { new Vector2(0, 1), new Vector2(0, -1), new Vector2(1, 0), new Vector2(-1, 0) };

        for (int i = 0; i < 4; i++)
        {
            Vector2 direction = directions[Random.Range(0, directions.Count)]; //Goes to a random direction in the list
            directions.Remove(direction);                                      //Allows for more randomness in eqi-distant paths

            List<Vector2> oldBoardSpaces = new List<Vector2>();
            foreach (Vector2 vec in occupiedSpaces)
                oldBoardSpaces.Add(lastNode.position + vec);

            List<Vector2> newBoardSpaces = new List<Vector2>();
            foreach (Vector2 vec in occupiedSpaces)
                if (!oldBoardSpaces.Contains(lastNode.position + direction + vec))
                    newBoardSpaces.Add(lastNode.position + direction + vec);

            if (!GridController.gridController.CheckIfOutOfBounds(newBoardSpaces) &&
                (
                    (GridController.gridController.GetObjectAtLocation(newBoardSpaces, new string[] { "Player", "Enemy", "Blockade" }).Count == 0 ||
                     GridController.gridController.GetObjectAtLocation(newBoardSpaces, new string[] { "Player", "Enemy", "Blockade" }).All(x => pathThroughTag.Contains(x.tag)))
                ) ||
                newBoardSpaces.Any(x => x == endingLoc))
            {
                AStarNode upNode = new AStarNode();
                upNode.position = lastNode.position + direction;
                upNode.gCost = lastNode.gCost + 1;
                upNode.hCost = GetHCost(upNode.position, endingLoc);
                output.Add(upNode);
            }
        }
        return output;
    }

    private float GetHCost(Vector2 startingLoc, Vector2 endingLoc)
    {
        return (Mathf.Abs(startingLoc.x - endingLoc.x) + Mathf.Abs(startingLoc.y - endingLoc.y));
    }

    private List<Vector2> GetFinalPath(AStarNode firstNode, AStarNode finalNode)
    {
        List<Vector2> output = new List<Vector2>();
        AStarNode currentNode = finalNode;

        while (currentNode.position != firstNode.position)
        {
            output.Add(currentNode.position);
            currentNode = currentNode.parent;
        }
        output.Add(currentNode.position);//Add back the first node's position

        output.Reverse();

        //Reset for the next pathfind call
        openList = new List<AStarNode>();
        closedList = new List<AStarNode>();

        return output;
    }

    private bool ContainsPosition(List<AStarNode> list, AStarNode node)
    {
        foreach (AStarNode listNode in list)
            if (listNode.position == node.position)
                return true;
        return false;
    }
}
