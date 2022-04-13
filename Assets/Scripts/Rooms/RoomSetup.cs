using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu]
public class RoomSetup : ScriptableObject
{
    public bool isBossRoom = false;
    public bool offerRewardGold = true;
    public bool offerRewardCards = true;
    public bool relicReward = false;
    public string roomName;
    public GameObject[] enemies;
    public int blockNumber;
    [HideInInspector]
    public BoardType[] level1 = new BoardType[7];
    [HideInInspector]
    public BoardType[] level2 = new BoardType[7];
    [HideInInspector]
    public BoardType[] level3 = new BoardType[7];
    [HideInInspector]
    public BoardType[] level4 = new BoardType[7];
    [HideInInspector]
    public BoardType[] level5 = new BoardType[7];
    [HideInInspector]
    public BoardType[] level6 = new BoardType[7];
    [HideInInspector]
    public BoardType[] level7 = new BoardType[7];

    public List<Dialogue> dialogues;
    public List<TutorialOverlay> overlays;
    public bool lastRewardCardOnTop = false;
    public bool overrideCardShuffle = false;
    public Card[] cardOrder;
    public int overrideHandSize = -1;
    public int overrideReplaces = -1;
    public Card.CasterColor[] overrideParty = new Card.CasterColor[0];
    public int overrideSeed = -1;

    public List<UIRevealController.UIElement> hiddenUIElements;

    public List<Vector2> GetLocations(BoardType type)
    {
        List<Vector2> output = new List<Vector2>();
        for (int j = 0; j < 7; j++)
            if (level1[j] == type)
                output.Add(new Vector2(6 - j, 6));          //j on x and 6-j to transpose and flip on x and y axis to match actual layout
        for (int j = 0; j < 7; j++)
            if (level2[j] == type)
                output.Add(new Vector2(6 - j, 5));
        for (int j = 0; j < 7; j++)
            if (level3[j] == type)
                output.Add(new Vector2(6 - j, 4));
        for (int j = 0; j < 7; j++)
            if (level4[j] == type)
                output.Add(new Vector2(6 - j, 3));
        for (int j = 0; j < 7; j++)
            if (level5[j] == type)
                output.Add(new Vector2(6 - j, 2));
        for (int j = 0; j < 7; j++)
            if (level6[j] == type)
                output.Add(new Vector2(6 - j, 1));
        for (int j = 0; j < 7; j++)
            if (level7[j] == type)
                output.Add(new Vector2(6 - j, 0));
        return output;
    }

    public enum BoardType
    {
        O = 0,
        W = 1,
        P = 2,
        E = 3,

        B = 100,
    }
}
