using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStarNode
{
    public AStarNode parent;
    public Vector2 position;
    public int gCost;
    public int hCost;
    public int fCost { get { return gCost + hCost; } }
}
