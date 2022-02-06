using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class TutorialOverlay : ScriptableObject
{
    public int ID;
    public int prerequisiteID = -1;
    public bool isUILevel = false;
    public bool isPopup = false;
    public bool slowTime = false;
    public float slowDuration = 0;
    public GameObject overlay;
    public Dialogue.Condition startingCondition;
    public int startingValue;
    public string startingStringValue;
    public StoryRoomSetup.ChallengeComparisonType startingComparisonType = StoryRoomSetup.ChallengeComparisonType.EqualTo;
    public UIRevealController.UIElement OnStartUIReveal = UIRevealController.UIElement.None;
    public Dialogue.Condition endingCondition;
    public int endingValue;
    public string endingStringValue;
    public StoryRoomSetup.ChallengeComparisonType endingComparisonType = StoryRoomSetup.ChallengeComparisonType.EqualTo;
    public UIRevealController.UIElement OnEndUIReveal = UIRevealController.UIElement.None;
    public string popupTitle;
    public Sprite popupImage;
    [TextArea]
    public string popupDescription;

    public bool IfConditionsMet(Dialogue.Condition con, int value, bool isStartingCondition, string stringValue = "")
    {
        Dialogue.Condition usedCondition = startingCondition;
        int usedValue = startingValue;
        string usedStringValue = startingStringValue;
        StoryRoomSetup.ChallengeComparisonType usedComparisonType = startingComparisonType;
        if (!isStartingCondition)
        {
            usedCondition = endingCondition;
            usedValue = endingValue;
            usedStringValue = endingStringValue;
            usedComparisonType = endingComparisonType;
        }

        if (con != usedCondition)
            return false;

        switch (usedComparisonType)
        {
            case StoryRoomSetup.ChallengeComparisonType.EqualTo:
                if (stringValue != "" && usedStringValue != "")
                    return stringValue == usedStringValue;
                return value == usedValue;
            case StoryRoomSetup.ChallengeComparisonType.GreaterThan:
                return value >= usedValue;
            case StoryRoomSetup.ChallengeComparisonType.LessThan:
                return value <= usedValue;
            case StoryRoomSetup.ChallengeComparisonType.NotEqualTo:
                return value != usedValue;
            default:
                return false;
        }
    }
}
