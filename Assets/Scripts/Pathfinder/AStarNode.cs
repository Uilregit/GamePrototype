using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStarNode
{
    public AStarNode parent;
    public Vector2 position;
    public int gCost;
    public float hCost;
    public float fCost { get { return gCost + hCost; } }
}
