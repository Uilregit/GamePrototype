using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

[CreateAssetMenu]
public class RoomSetup : ScriptableObject
{
    [Header("Setup Information")]
    public bool isBossRoom = false;
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

    [Header("Tutorial Overrides")]
    public bool offerRewardGold = true;
    public bool offerRewardCards = true;
    public bool relicReward = false;
    public int overrideSingleGoalsSplashIndex = -1;
    public List<Dialogue> dialogues;
    public List<TutorialOverlay> overlays;
    public bool lastRewardCardOnTop = false;
    public bool overrideCardShuffle = false;
    public Card[] cardOrder;
    public int overrideHandSize = -1;
    public int overrideReplaces = -1;
    public Card.CasterColor[] overrideParty = new Card.CasterColor[0];
    public int overrideSeed = -1;

    [Header("Draw Hand Conditions")]
    public Dialogue.Condition drawHandCondition;
    public int drawHandConditionValue;

    [Header("Hidden UI Elements")]
    public List<UIRevealController.UIElement> hiddenUIElements;
    public Card endRoomUnlockCard;
    public Sprite endRoomUnlockChar;
    public Sprite endRoomUnlockAbility;
    public string endRoomUnlockAbilityName;
    public List<UIRevealController.UIElement> revealedUIElements;

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

    public List<Vector2> GetAnyPlayerPositions()
    {
        List<BoardType> playerTypes = new List<BoardType> { BoardType.P, BoardType.P1, BoardType.P2, BoardType.P3 };
        List<Vector2> output = new List<Vector2>();
        for (int j = 0; j < 7; j++)
            if (playerTypes.Contains(level1[j]))
                output.Add(new Vector2(6 - j, 6));          //j on x and 6-j to transpose and flip on x and y axis to match actual layout
        for (int j = 0; j < 7; j++)
            if (playerTypes.Contains(level2[j]))
                output.Add(new Vector2(6 - j, 5));
        for (int j = 0; j < 7; j++)
            if (playerTypes.Contains(level3[j]))
                output.Add(new Vector2(6 - j, 4));
        for (int j = 0; j < 7; j++)
            if (playerTypes.Contains(level4[j]))
                output.Add(new Vector2(6 - j, 3));
        for (int j = 0; j < 7; j++)
            if (playerTypes.Contains(level5[j]))
                output.Add(new Vector2(6 - j, 2));
        for (int j = 0; j < 7; j++)
            if (playerTypes.Contains(level6[j]))
                output.Add(new Vector2(6 - j, 1));
        for (int j = 0; j < 7; j++)
            if (playerTypes.Contains(level7[j]))
                output.Add(new Vector2(6 - j, 0));
        return output;
    }

    public List<Vector2> GetAnyEnemyPositions()
    {
        List<BoardType> enemyTypes = new List<BoardType> { BoardType.E, BoardType.E1, BoardType.E2, BoardType.E3, BoardType.E4, BoardType.E5, BoardType.E6, BoardType.E7, BoardType.E8, BoardType.E9, BoardType.E10 };
        List<Vector2> output = new List<Vector2>();
        for (int j = 0; j < 7; j++)
            if (enemyTypes.Contains(level1[j]))
                output.Add(new Vector2(6 - j, 6));          //j on x and 6-j to transpose and flip on x and y axis to match actual layout
        for (int j = 0; j < 7; j++)
            if (enemyTypes.Contains(level2[j]))
                output.Add(new Vector2(6 - j, 5));
        for (int j = 0; j < 7; j++)
            if (enemyTypes.Contains(level3[j]))
                output.Add(new Vector2(6 - j, 4));
        for (int j = 0; j < 7; j++)
            if (enemyTypes.Contains(level4[j]))
                output.Add(new Vector2(6 - j, 3));
        for (int j = 0; j < 7; j++)
            if (enemyTypes.Contains(level5[j]))
                output.Add(new Vector2(6 - j, 2));
        for (int j = 0; j < 7; j++)
            if (enemyTypes.Contains(level6[j]))
                output.Add(new Vector2(6 - j, 1));
        for (int j = 0; j < 7; j++)
            if (enemyTypes.Contains(level7[j]))
                output.Add(new Vector2(6 - j, 0));
        return output;
    }

    public enum BoardType
    {
        O = 0,
        W = 1,
        P = 2,
        E = 3,

        P1 = 51,
        P2 = 52,
        P3 = 53,

        E1 = 71,
        E2 = 72,
        E3 = 73,
        E4 = 74,
        E5 = 75,
        E6 = 76,
        E7 = 77,
        E8 = 78,
        E9 = 79,
        E10 = 80,

        B = 100,
    }
}
