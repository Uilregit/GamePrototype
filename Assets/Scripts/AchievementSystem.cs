using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementSystem : Observer
{
    public static AchievementSystem achieve;

    private Dictionary<StoryRoomSetup.ChallengeType, int> achievements = new Dictionary<StoryRoomSetup.ChallengeType, int>();

    private void Awake()
    {
        if (AchievementSystem.achieve == null)
            AchievementSystem.achieve = this;
        else
            Destroy(this.gameObject);

        DontDestroyOnLoad(this.gameObject);
    }

    public override void OnNotify(object value, Relic.NotificationType notificationType, List<Relic> traceList)
    {
        throw new System.NotImplementedException();
    }

    public override void OnNotify(int value, StoryRoomSetup.ChallengeType notificationType)
    {
        switch (GetChallengeValueType(notificationType))
        {
            case (StoryRoomSetup.ChallengeValueType.ValueAsIs):
                achievements[notificationType] = value;
                break;
            case (StoryRoomSetup.ChallengeValueType.Sum):
                if (!achievements.ContainsKey(notificationType))
                    achievements[notificationType] = 0;
                achievements[notificationType] += value;
                break;
            case (StoryRoomSetup.ChallengeValueType.Count):
                if (!achievements.ContainsKey(notificationType))
                    achievements[notificationType] = 0;
                achievements[notificationType] += 1;
                break;
            case (StoryRoomSetup.ChallengeValueType.Max):
                if (!achievements.ContainsKey(notificationType))
                    achievements[notificationType] = 0;
                achievements[notificationType] = Mathf.Max(value, achievements[notificationType]);
                break;
            case (StoryRoomSetup.ChallengeValueType.Min):
                if (!achievements.ContainsKey(notificationType))
                    achievements[notificationType] = 999999999;
                achievements[notificationType] = Mathf.Min(value, achievements[notificationType]);
                break;
        }

        StoryRoomSetup setup = StoryModeController.story.GetCurrentRoomSetup();
        for (int i = 0; i < setup.challenges.Length; i++)
        {
            if (setup.challenges[i] == notificationType)
            {
                Debug.Log(notificationType + "| " + GetChallengeValueType(notificationType) + " | " + value + " --> " + achievements[notificationType]);
            }
        }
    }

    public int GetChallengeValue(StoryRoomSetup.ChallengeType notificationType)
    {
        if (!achievements.ContainsKey(notificationType))
            return -1;
        return achievements[notificationType];
    }

    public void ResetAchievements()
    {
        achievements = new Dictionary<StoryRoomSetup.ChallengeType, int>();
    }


    public StoryRoomSetup.ChallengeValueType GetChallengeValueType(StoryRoomSetup.ChallengeType challenge)
    {
        StoryRoomSetup setup = StoryModeController.story.GetCurrentRoomSetup();
        for (int i = 0; i < setup.challenges.Length; i++)
        {
            if (setup.challenges[i] == challenge)
                return setup.valueType[i];
        }

        return StoryRoomSetup.ChallengeValueType.ValueAsIs;
    }
}
