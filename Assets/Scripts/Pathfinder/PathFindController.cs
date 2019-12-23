using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    // Update is called once per frame
    void Update()
    {

    }

    //Use A* pathfinding to return the full path
    public List<Vector2> PathFind(Vector2 startingLoc, Vector2 endingLoc)
    {
        List<Vector2> output = new List<Vector2>();

        int hCost = GetHCost(startingLoc, endingLoc);
        int gCost = 0;
        AStarNode startingNode = new AStarNode();
        startingNode.position = startingLoc;
        startingNode.gCost = gCost;
        startingNode.hCost = hCost;

        openList.Add(startingNode);

        AStarNode currentNode = openList[0];
        for (int iteration = 0; iteration < 1000; iteration++)   //Runs for a max of 1000 iterations to prevent infinite loops if unpathable
        {
            currentNode = openList[0];
            for (int i = 1; i < openList.Count; i++)
            {
                if (openList[i].fCost < currentNode.fCost ||
                    (openList[i].fCost == currentNode.fCost && openList[i].hCost < currentNode.hCost))
                {
                    currentNode = openList[i];
                }
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            if (currentNode.position == endingLoc) //If pathfinding finds the ending, then return path list and end early
            {
                output = GetFinalPath(startingNode, currentNode);
                return output;
            }

            foreach (AStarNode neighborNode in GetNeighbours(currentNode, endingLoc))
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

        AStarNode finalNode = closedList[0];
        foreach (AStarNode node in closedList)
            if (node.hCost < finalNode.hCost)
                finalNode = node;
        output = GetFinalPath(startingNode, finalNode);  //returns the path to the node closest to the target if unable to path to ending location (ie unpathable)
        return output;
    }

    //Return 4 nodes at neighbouring positions if they are valid positions for pathfinding
    private List<AStarNode> GetNeighbours(AStarNode lastNode, Vector2 endingLoc)
    {
        List<AStarNode> output = new List<AStarNode>();
        List <Vector2> directions = new List<Vector2>{ new Vector2(0, 1) , new Vector2(0, -1) , new Vector2(1,0) , new Vector2(-1,0) };

        for (int i = 0; i < 4; i++)
        {
            Vector2 direction = directions[Random.Range(0, directions.Count)]; //Add a random direction into the list first
            directions.Remove(direction);                                      //Allows for more randomness in eqi-distant paths
            if ((GridController.gridController.GetObjectAtLocation(lastNode.position + direction) == null ||
                lastNode.position + direction == endingLoc)&&
                !GridController.gridController.CheckIfOutOfBounds(lastNode.position + direction))
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

    private int GetHCost(Vector2 startingLoc, Vector2 endingLoc)
    {
        return (int)(Mathf.Abs(startingLoc.x - endingLoc.x) + Mathf.Abs(startingLoc.y - endingLoc.y));
    }

    private List<Vector2> GetFinalPath(AStarNode firstNode, AStarNode finalNode)
    {
        List<Vector2> output = new List<Vector2>();
        AStarNode currentNode = finalNode;

        while (currentNode != firstNode)
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

    private bool ContainsPosition (List<AStarNode> list, AStarNode node)
    {
        foreach (AStarNode listNode in list)
        {
            if (listNode.position == node.position)
                return true;
        }
        return false;
    }
}
