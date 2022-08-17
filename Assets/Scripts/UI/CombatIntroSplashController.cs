using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CombatIntroSplashController : MonoBehaviour
{
    [Header("Splash Image")]
    public GameObject splashObject;
    public Text splashTitle;
    public Text additionalTitle;
    public Image iconImage;
    public Image[] splashHighlights;
    public Image[] splashLowLights;
    [Header("Goals")]
    public GameObject[] goalObjects;
    public Text[] goalTexts;
    public Text[] goalAdditionalTitles;
    public Image[] goalIcons;
    public Text[] goalProgressTexts;
    public Image[] goalProgressBars;
    public Text[] goalCompletedTexts;
    public Text[] goalFailedTexts;
    public Image[] goalsCap;
    public Image[] goalsCheckmark;
    public Image[] goalsXMark;
    [Header("Bottom Tray")]
    public GameObject singleGoalsImage;
    public GameObject trippleGoalsImage;
    public Image singleGoals;
    public List<Image> trippleGoals;
    [Header("Colors")]
    public Color redHighlight;
    public Color redLowlight;
    public Color greenHighlight;
    public Color greenLowlight;
    public Color purpleHighlight;
    public Color purpleLowlight;
    public Color redBarColor;
    public Color greenBarColor;
    public Color yellowBarColor;

    [Header("Icons")]
    public Sprite exclamationIcon;
    public Sprite hourglassIcon;

    [Header("Skipping")]
    public Image skippingImage;

    [Header("Buttons")]
    public Image goalsButton;

    private List<bool> goalsCompleted = new List<bool> { false, false, false };
    //private List<bool> goalsCompletedThisRound = new List<bool> { false, false, false };
    private List<bool> goalsCompletedShouldAnimate = new List<bool> { false, false, false };
    private List<bool> goalsFailed = new List<bool> { false, false, false };
    //private List<bool> goalsFailedThisRound = new List<bool> { false, false, false };
    private List<bool> goalsFailedShouldAnimate = new List<bool> { false, false, false };
    private List<float> originalGoalsPercentages = new List<float> { 0, 0, 0 };

    private bool skipPressed = false;

    public enum icons
    {
        exclamation = 1,
        hourglass = 2,
    }

    public enum colors
    {
        Red = 1,
        Green = 2,
        Purple = 3
    }

    public void SetSplashImage(icons icon, string title, string optionalTitle, colors color)
    {
        iconImage.sprite = GetIcon(icon);
        splashTitle.text = title;
        additionalTitle.text = optionalTitle.ToUpper();
        foreach (Image img in splashHighlights)
            img.color = GetHighlightColor(color);
        foreach (Image img in splashLowLights)
            img.color = GetLowlightColor(color);

        singleGoalsImage.gameObject.SetActive(GameController.gameController.GetRoomSetup().overrideSingleGoalsSplashIndex != -1);
        trippleGoalsImage.gameObject.SetActive(GameController.gameController.GetRoomSetup().overrideSingleGoalsSplashIndex == -1);

        List<Image> achievementIcons = trippleGoals;
        if (GameController.gameController.GetRoomSetup().overrideSingleGoalsSplashIndex != -1)
            achievementIcons[GameController.gameController.GetRoomSetup().overrideSingleGoalsSplashIndex] = singleGoals;
        StoryModeController.story.SetAchievementImages(achievementIcons);
        StoryModeController.story.RefreshAchievementValues();
    }

    public void SetGoalsImage(int index, Color goalIconColor, string goalDescription, string goalProcress, float goalProcessPercentage, bool goalSatisfied, bool isStartOfRound, bool endOfRound)
    {
        bool goalSatisfiedSaveGame = StoryModeController.story.ChallengeSatisfied(index);
        StoryRoomSetup currentStoryRoomSetup = StoryModeController.story.GetCurrentRoomSetup();
        int bestValue = currentStoryRoomSetup.GetBestValues(currentStoryRoomSetup.bestChallengeValues[index], AchievementSystem.achieve.GetChallengeValue(currentStoryRoomSetup.challenges[index]), index, currentStoryRoomSetup.challengeComparisonType[index]);
        bool goalSatisfiedCurrently = StoryModeController.story.ChallengeSatisfied(index, bestValue);
        StoryRoomSetup.ChallengeComparisonType comparisonType = StoryModeController.story.GetCurrentRoomSetup().challengeComparisonType[index];

        goalIcons[index].color = goalIconColor;
        goalTexts[index].text = goalDescription;
        goalProgressTexts[index].text = goalProcress;
        goalProgressBars[index].transform.localScale = new Vector3(goalProcessPercentage, 1, 1);

        if (!isStartOfRound)
        {
            /*
            if (goalSatisfied && !goalsCompleted[index])
                goalsCompletedThisRound[index] = true;
            if (!goalSatisfied && !goalsFailed[index])
                goalsFailedThisRound[index] = true;
            */
        }
        else
            originalGoalsPercentages[index] = goalProcessPercentage;
        goalsCompleted[index] = goalSatisfied;
        goalsFailed[index] = !goalSatisfied;

        switch (comparisonType)
        {
            case StoryRoomSetup.ChallengeComparisonType.GreaterThan:
                {
                    goalProgressBars[index].color = greenBarColor;
                    goalsCap[index].gameObject.SetActive(true);
                    goalsCheckmark[index].gameObject.SetActive(true);
                    goalsXMark[index].gameObject.SetActive(false);
                    break;
                }
            case StoryRoomSetup.ChallengeComparisonType.LessThan:
                {
                    goalProgressBars[index].color = redBarColor;
                    goalsCap[index].gameObject.SetActive(true);
                    goalsCheckmark[index].gameObject.SetActive(false);
                    goalsXMark[index].gameObject.SetActive(true);
                    break;
                }
            default:
                {
                    goalProgressBars[index].color = yellowBarColor;
                    goalsCap[index].gameObject.SetActive(false);
                    break;
                }
        }

        goalCompletedTexts[index].gameObject.SetActive(false);
        goalFailedTexts[index].gameObject.SetActive(false);

        //Setting the complete and failed texts
        if (goalSatisfiedSaveGame)
        {
            goalCompletedTexts[index].gameObject.SetActive(true);
            goalFailedTexts[index].gameObject.SetActive(false);
            goalsCompletedShouldAnimate[index] = true;
            goalsFailedShouldAnimate[index] = false;
        }
        else if (GameController.gameController.GetIfBossRoom() && endOfRound)        //last room end of round
        {
            goalsCompletedShouldAnimate[index] = goalSatisfiedCurrently;
            goalsFailedShouldAnimate[index] = !goalSatisfiedCurrently;
        }
        else
        {
            goalsCompletedShouldAnimate[index] = false;
            goalsFailedShouldAnimate[index] = false;
        }
    }

    public IEnumerator AnimateSplashImage(float duration = 2f)
    {
        GameController.gameController.cutsceneCanvas.gameObject.SetActive(true);

        Vector3 originalPosition = new Vector3(-10, 0, 0);
        splashObject.transform.localPosition = originalPosition;
        splashObject.SetActive(true);

        //Animate the card coming in
        for (int i = 0; i < 5; i++)
        {
            splashObject.transform.localPosition = Vector3.Lerp(originalPosition, Vector3.zero, i / 4f);
            yield return new WaitForSeconds(0.1f / 5);
        }

        //Wait till elapsed time has gone or skipped
        skippingImage.gameObject.SetActive(true);
        float startingTime = Time.time;
        while (!skipPressed && Time.time - startingTime < duration * TimeController.time.timerMultiplier)
            yield return new WaitForSeconds(0.1f);
        skipPressed = false;
        skippingImage.gameObject.SetActive(false);

        //Animate the card going back out
        for (int i = 0; i < 5; i++)
        {
            splashObject.transform.localPosition = Vector3.Lerp(Vector3.zero, originalPosition, i / 4f);
            yield return new WaitForSeconds(0.1f / 5);
        }
        splashObject.SetActive(false);

        GameController.gameController.cutsceneCanvas.gameObject.SetActive(false);
    }

    public IEnumerator AnimateGoalsImage(float duration = 9999f)
    {
        GameController.gameController.cutsceneCanvas.gameObject.SetActive(true);

        goalsButton.color = GameController.gameController.notYetDoneColor;

        //Animate the cards coming in
        List<Vector3> originalPosition = new List<Vector3>() { new Vector3(-10, goalObjects[0].transform.localPosition.y, 0), new Vector3(10, goalObjects[1].transform.localPosition.y, 0), new Vector3(-10, goalObjects[2].transform.localPosition.y, 0) };
        for (int index = 0; index < 3; index++)
        {
            yield return StartCoroutine(AnimateGoalSlideIn(index, originalPosition[index]));
        }
        yield return new WaitForSeconds(0.3f);

        //Animate the completed word if the goal was completed this round
        for (int index = 0; index < 3; index++)
        {
            yield return StartCoroutine(AnimateGoalComplete(index));
        }

        //Wait till elapsed time has gone or skipped
        skippingImage.gameObject.SetActive(true);
        float startingTime = Time.time;
        while (!skipPressed && Time.time - startingTime < duration * TimeController.time.timerMultiplier)
            yield return new WaitForSeconds(0.1f);
        skipPressed = false;
        skippingImage.gameObject.SetActive(false);

        //Animate the card going back out
        for (int index = 0; index < 3; index++)
        {
            yield return StartCoroutine(AnimateGoalSlideOut(index, originalPosition[index]));
        }

        goalsButton.color = GameController.gameController.doneColor;

        GameController.gameController.cutsceneCanvas.gameObject.SetActive(false);
    }

    public IEnumerator AnimateSingleGoalsImage(int index, bool startOfRound, bool endOfRound, float duration = 9999f)
    {
        GameController.gameController.cutsceneCanvas.gameObject.SetActive(true);

        goalsButton.color = GameController.gameController.notYetDoneColor;

        SetGoalsImage(0, goalIcons[index].color, goalTexts[index].text, goalProgressTexts[index].text, goalProgressBars[index].transform.localScale.x, goalsCompleted[index], startOfRound, endOfRound);
        Vector3 originalPosition = new Vector3(-10, 0, 0);
        goalAdditionalTitles[0].text = "OPTIONAL GOAL";
        goalObjects[0].SetActive(true);

        //Animate the card coming in
        yield return StartCoroutine(AnimateGoalSlideIn(0, originalPosition));

        yield return new WaitForSeconds(0.3f);

        //Animate the completed word if the goal was completed this round
        yield return StartCoroutine(AnimateGoalComplete(0));

        //Wait till elapsed time has gone or skipped
        skippingImage.gameObject.SetActive(true);
        float startingTime = Time.time;
        while (!skipPressed && Time.time - startingTime < duration * TimeController.time.timerMultiplier)
            yield return new WaitForSeconds(0.1f);
        skipPressed = false;
        skippingImage.gameObject.SetActive(false);

        //Animate the card going back out
        yield return StartCoroutine(AnimateGoalSlideOut(0, originalPosition));
        goalAdditionalTitles[0].text = "OPTIONAL GOALS";

        goalsButton.color = GameController.gameController.doneColor;

        GameController.gameController.cutsceneCanvas.gameObject.SetActive(false);
    }

    private IEnumerator AnimateGoalSlideIn(int index, Vector2 originalPosition)
    {
        goalObjects[index].transform.localPosition = originalPosition;
        goalObjects[index].SetActive(true);

        //Animate the card coming in
        for (int i = 0; i < 5; i++)
        {
            goalObjects[index].transform.localPosition = Vector3.Lerp(originalPosition, new Vector3(0, originalPosition.y, 0), i / 4f);
            yield return new WaitForSeconds(0.1f / 5);
        }
    }

    private IEnumerator AnimateGoalSlideOut(int index, Vector2 originalPosition)
    {
        for (int i = 0; i < 5; i++)
        {
            goalObjects[index].transform.localPosition = Vector3.Lerp(new Vector3(0, goalObjects[index].transform.localPosition.y, 0), originalPosition, i / 4f);
            yield return new WaitForSeconds(0.1f / 5);
        }
        goalObjects[index].SetActive(false);
    }

    private IEnumerator AnimateGoalComplete(int index)
    {
        //Reset the goals progress to it's original size
        float newProgress = goalProgressBars[index].transform.localScale.x;
        goalProgressBars[index].transform.localScale = new Vector3(originalGoalsPercentages[index], 1, 1);

        //Animate the goals 
        for (int i = 0; i < 5; i++)
        {
            goalProgressBars[index].transform.localScale = Vector3.Lerp(new Vector3(originalGoalsPercentages[index], 1, 1), new Vector3(newProgress, 1, 1), i / 4f);
            yield return new WaitForSeconds(0.1f / 5);
        }

        //Animate the COMPLETE or FAILED texts
        if (!goalCompletedTexts[index].gameObject.active && !goalFailedTexts[index].gameObject.active)  //If neither COMPLETE or FAILED text is already shown
        {
            //Animate the COMPLETE text
            if (goalsCompletedShouldAnimate[index])
            {
                Vector3 originalScale = goalCompletedTexts[index].transform.localScale;
                goalCompletedTexts[index].transform.localScale = originalScale * 3;
                goalCompletedTexts[index].gameObject.SetActive(true);
                for (int i = 0; i < 5; i++)
                {
                    goalCompletedTexts[index].transform.localScale = Vector3.Lerp(originalScale * 3, originalScale, i / 4f);
                    yield return new WaitForSeconds(0.1f / 5);
                }
            }
            //Animate the FAILED text
            if (goalsFailedShouldAnimate[index])
            {
                Vector3 originalScale = goalFailedTexts[index].transform.localScale;
                goalFailedTexts[index].transform.localScale = originalScale * 3;
                goalFailedTexts[index].gameObject.SetActive(true);
                for (int i = 0; i < 5; i++)
                {
                    goalFailedTexts[index].transform.localScale = Vector3.Lerp(originalScale * 3, originalScale, i / 4f);
                    yield return new WaitForSeconds(0.1f / 5);
                }
            }
        }
    }

    public void ShowGoalsButtonPressed()
    {
        if (GameController.gameController.GetRoomSetup().overrideSingleGoalsSplashIndex == -1)
            StartCoroutine(AnimateGoalsImage());
        else
            StartCoroutine(AnimateSingleGoalsImage(GameController.gameController.GetRoomSetup().overrideSingleGoalsSplashIndex, false, false));
    }

    private Color GetHighlightColor(colors c)
    {
        switch (c)
        {
            case colors.Red:
                return redHighlight;
            case colors.Green:
                return greenHighlight;
            case colors.Purple:
                return purpleHighlight;
            default:
                return redHighlight;
        }
    }

    private Color GetLowlightColor(colors c)
    {
        switch (c)
        {
            case colors.Red:
                return redLowlight;
            case colors.Green:
                return greenLowlight;
            case colors.Purple:
                return purpleLowlight;
            default:
                return redLowlight;
        }
    }

    private Sprite GetIcon(icons icon)
    {
        switch (icon)
        {
            case icons.exclamation:
                return exclamationIcon;
            case icons.hourglass:
                return hourglassIcon;
            default:
                return exclamationIcon;
        }
    }

    public void SkipPressed()
    {
        skipPressed = true;
    }
}
