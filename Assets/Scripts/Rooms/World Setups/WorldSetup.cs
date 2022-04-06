using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class WorldSetup : ScriptableObject
{
    public Sprite combatBackground;
    public Color cameraBackground;
    public Color roomBackground;
    public List<Vector2> roomLocations;
    public List<RoomSetup> innitialRooms;
    public List<RoomSetup> midRooms;
    public List<RoomSetup> lateRooms;
    public List<RoomSetup> bossRooms;

    public float dissapearPercentage;
    public int numOfShopPerWorld;
    public int minShopLocation;
    public int maxShopLocation;
    public int numOfShrinePerWorld;
    public int minShrineLocation;
    public int maxShrineLocation;
    public float shopPercentage;
    public float shrinePercentage;
}
