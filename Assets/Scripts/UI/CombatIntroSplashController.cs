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

    [Header("Colors")]
    public Color redHighlight;
    public Color redLowlight;
    public Color greenHighlight;
    public Color greenLowlight;
    public Color purpleHighlight;
    public Color purpleLowlight;

    [Header("Icons")]
    public Sprite exclamationIcon;
    public Sprite hourglassIcon;

    [Header("Skipping")]
    public Image skippingImage;

    [Header("Buttons")]
    public Image goalsButton;

    private List<bool> goalsCompleted = new List<bool> { false, false, false };
    private List<bool> goalsCompletedThisRound = new List<bool> { false, false, false };
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
    }

    public void SetGoalsImage(int index, Color goalIconColor, string goalDescription, string goalProcress, float goalProcessPercentage, bool goalSatisfied, bool isStartOfRound)
    {
        goalIcons[index].color = goalIconColor;
        goalTexts[index].text = goalDescription;
        goalProgressTexts[index].text = goalProcress;
        goalProgressBars[index].transform.localScale = new Vector3(goalProcessPercentage, 1, 1);

        if (!isStartOfRound)
        {
            if (goalSatisfied && !goalsCompleted[index])
                goalsCompletedThisRound[index] = true;
        }
        else
            originalGoalsPercentages[index] = goalProcessPercentage;
        goalsCompleted[index] = goalSatisfied;
    }

    public IEnumerator AnimateSplashImage(float duration = 2f)
    {
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
    }

    public IEnumerator AnimateGoalsImage(float duration = 9999f)
    {
        goalsButton.color = GameController.gameController.notYetDoneColor;

        //Animate the cards coming in
        List<Vector3> originalPosition = new List<Vector3>() { new Vector3(-10, 0, 0), new Vector3(10, 0, 0), new Vector3(-10, 0, 0) };
        for (int index = 0; index < 3; index++)
        {
            originalPosition[index] = new Vector3(originalPosition[index].x, goalObjects[index].transform.localPosition.y, 0);
            goalObjects[index].transform.localPosition = originalPosition[index];
            goalCompletedTexts[index].gameObject.SetActive(goalsCompleted[index] && !goalsCompletedThisRound[index]);
            goalObjects[index].SetActive(true);
            for (int i = 0; i < 5; i++)
            {
                goalObjects[index].transform.localPosition = Vector3.Lerp(originalPosition[index], new Vector3(0, originalPosition[index].y, 0), i / 4f);
                yield return new WaitForSeconds(0.1f / 5);
            }
        }
        yield return new WaitForSeconds(0.3f);

        //Animate the completed word if the goal was completed this round
        for (int index = 0; index < 3; index++)
        {
            float newProgress = goalProgressBars[index].transform.localScale.x;
            goalProgressBars[index].transform.localScale = new Vector3(originalGoalsPercentages[index], 1, 1);

            for (int i = 0; i < 5; i++)
            {
                goalProgressBars[index].transform.localScale = Vector3.Lerp(new Vector3(originalGoalsPercentages[index], 1, 1), new Vector3(newProgress, 1, 1), i / 4f);
                yield return new WaitForSeconds(0.1f / 5);
            }

            if (goalsCompletedThisRound[index])
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
            for (int i = 0; i < 5; i++)
            {
                goalObjects[index].transform.localPosition = Vector3.Lerp(new Vector3(0, originalPosition[index].y, 0), originalPosition[index], i / 4f);
                yield return new WaitForSeconds(0.1f / 5);
            }
            goalObjects[index].SetActive(false);
        }

        goalsButton.color = GameController.gameController.doneColor;
    }

    public IEnumerator AnimateSingleGoalsImage(int index, float duration = 9999f)
    {
        goalsButton.color = GameController.gameController.notYetDoneColor;

        float newProgress = goalProgressBars[index].transform.localScale.x;
        goalProgressBars[index].transform.localScale = new Vector3(originalGoalsPercentages[index], 1, 1);

        SetGoalsImage(0, goalIcons[index].color, goalTexts[index].text, goalProgressTexts[index].text, goalProgressBars[index].transform.localScale.x, goalsCompleted[index], false);
        Vector3 originalPosition = new Vector3(-10, 0, 0);
        goalObjects[0].transform.localPosition = originalPosition;
        goalCompletedTexts[0].gameObject.SetActive(goalsCompleted[0] && !goalsCompletedThisRound[0]);
        goalAdditionalTitles[0].text = "OPTIONAL GOAL";
        goalObjects[0].SetActive(true);

        //Animate the card coming in
        for (int i = 0; i < 5; i++)
        {
            goalObjects[0].transform.localPosition = Vector3.Lerp(originalPosition, Vector3.zero, i / 4f);
            yield return new WaitForSeconds(0.1f / 5);
        }
        yield return new WaitForSeconds(0.3f);

        for (int i = 0; i < 5; i++)
        {
            goalProgressBars[index].transform.localScale = Vector3.Lerp(new Vector3(originalGoalsPercentages[index], 1, 1), new Vector3(newProgress, 1, 1), i / 4f);
            yield return new WaitForSeconds(0.1f / 5);
        }

        //Animate the completed word if the goal was completed this round
        if (goalsCompletedThisRound[index])
        {
            Vector3 originalScale = goalCompletedTexts[0].transform.localScale;
            goalCompletedTexts[0].transform.localScale = originalScale * 3;
            goalCompletedTexts[0].gameObject.SetActive(true);
            for (int i = 0; i < 5; i++)
            {
                goalCompletedTexts[0].transform.localScale = Vector3.Lerp(originalScale * 3, originalScale, i / 4f);
                yield return new WaitForSeconds(0.1f / 5);
            }
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
            goalObjects[0].transform.localPosition = Vector3.Lerp(Vector3.zero, originalPosition, i / 4f);
            yield return new WaitForSeconds(0.1f / 5);
        }
        goalObjects[0].SetActive(false);
        goalAdditionalTitles[0].text = "OPTIONAL GOALS";

        goalsButton.color = GameController.gameController.doneColor;
    }

    public void ShowGoalsButtonPressed()
    {
        if (GameController.gameController.GetRoomSetup().overrideSingleGoalsSplashIndex == -1)
            StartCoroutine(AnimateGoalsImage());
        else
            StartCoroutine(AnimateSingleGoalsImage(GameController.gameController.GetRoomSetup().overrideSingleGoalsSplashIndex));
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
