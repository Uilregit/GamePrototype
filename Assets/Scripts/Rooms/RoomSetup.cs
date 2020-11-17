using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class RoomSetup : ScriptableObject
{
    public bool isBossRoom = false;
    public bool relicReward = false;
    public string roomName;
    public GameObject[] enemies;
    public int blockNumber;
    public List<Vector2> playwerSpawnLocations;
}
