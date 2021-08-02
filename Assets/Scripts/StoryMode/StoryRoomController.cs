using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoryRoomController : MonoBehaviour
{
    public int roomId;
    public int unlockRequirementID;
    public bool unlockRequire3Stars = false;
    public bool startHidden = false;
    public StoryRoomSetup setup;
    public StoryRoomType roomType;
    public Image connector;

    public enum StoryRoomType
    {
        Combat = 0,
        Boss = 5,
        Shop = 10,
        SecretShop = 15,
        Arena = 50,
        NakedArena = 60,
        NewWorld = 100,
        PreviousWorld = 101
    }
}
