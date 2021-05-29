using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoryRoomController : MonoBehaviour
{
    public int roomId;
    public int unlockRequirementID;
    public bool unlockRequire3Stars = false;
    public bool startHidden = false;
    public StoryRoomSetup setup;
    public StoryRoomType roomType;

    public enum StoryRoomType
    {
        Combat = 0,
        Boss = 5,
        Shop = 10,
        Arena = 50,
        NewWorld = 100
    }
}
