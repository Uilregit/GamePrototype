using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class RoomSetup : ScriptableObject
{
    public GameObject[] enemies;
    public int blockNumber;
    public List<Vector2> playwerSpawnLocations;
}
