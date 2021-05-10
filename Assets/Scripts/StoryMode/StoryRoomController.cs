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
}
