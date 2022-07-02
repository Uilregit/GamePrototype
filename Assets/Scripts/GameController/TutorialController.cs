using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TutorialController : MonoBehaviour
{
    public static TutorialController tutorial;

    public Canvas tutorialCanvas;
    public Canvas tutorialUICanvas;

    public Dialogue.Speaker[] speakerOrder;

    public Sprite[] neutralEmote;
    public Sprite[] happyEmote;
    public Sprite[] winkEmote;
    public Sprite[] kissEmote;
    public Sprite[] angryEmote;
    public Sprite[] frustratedEmote;
    public Sprite[] confusedEmote;
    public Sprite[] surprisedEmote;
    public Sprite[] sadEmote;
    public Sprite[] sleepEmote;
    public Sprite[] sunglassesEmote;

    public Image background;
    public Image emoticon;
    public Text text;
    public Image skipDialogueButton;

    public GameObject popupTutorial;
    private TutorialOverlay popupOverlay = null;
    public Text popupTitle;
    public Image popupImage;
    public Image popUpLargeImage;
    public Text popupDescription;
    public GameObject popUpSocialsButtons;
    private int popupID = -1;

    public List<TutorialOverlay> passiveTutorials = new List<TutorialOverlay>();

    public Image feedbackMenu;
    public GameObject feedbackInputs;
    public Image[] feedbackTypes;
    public Text feedbackTypeDescription;
    public Color selectedColor;
    public Color unselectedColor;
    public InputField comments;

    private bool handDrawn = false;

    private int feedbackType = -1;

    private List<Dialogue> currentDialogue = new List<Dialogue>();
    private List<TutorialOverlay> currentTutorialOverlays = new List<TutorialOverlay>();
    private Dictionary<TutorialOverlay, GameObject> existingOverlays = new Dictionary<TutorialOverlay, GameObject>();
    private bool dialogueClicked = false;
    private bool dialogueSkipped = false;

    private List<int> completedTutIDs = new List<int> { -1 };
    private List<int> completedPassiveTutIDs = new List<int> { -1 };
    private List<int> repeatablePassiveTutorials = new List<int> { 999999, 100001, 100002, 100003, 100004, 100005, 100006 };

    private List<string> latestErrorLogs = new List<string>();

    private void Start()
    {
        if (TutorialController.tutorial == null)
            tutorial = this;
        else
        {
            Destroy(this.gameObject);
            return;
        }

        DontDestroyOnLoad(this);
        DontDestroyOnLoad(tutorialCanvas.gameObject);
        DontDestroyOnLoad(tutorialUICanvas.gameObject);

        InformationLogger.infoLogger.LoadPlayerPreferences();                //Load player preferences on startup

        Application.logMessageReceived += OnLocMessageReceived;
    }

    private void OnLocMessageReceived(string logString, string stackTrace, LogType type)
    {
        if (type == LogType.Error || type == LogType.Exception)
        {
            string log = "[" + System.DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss") + "] " + logString + " # Stack Trace: # " + stackTrace;

            log = log.Replace("\n", " # ");
            log = log.Replace(",", "");

            latestErrorLogs.Insert(0, log);
            latestErrorLogs = latestErrorLogs.GetRange(0, Mathf.Min(5, latestErrorLogs.Count));
        }
    }

    public void SetDialogue(List<Dialogue> tut)
    {
        currentDialogue = new List<Dialogue>();
        foreach (Dialogue d in tut)
            currentDialogue.Add(d);
    }

    public void SetTutorialOverlays(List<TutorialOverlay> overlays)
    {
        currentTutorialOverlays = new List<TutorialOverlay>();
        completedTutIDs = new List<int> { -1 };

        foreach (TutorialOverlay o in overlays)
            currentTutorialOverlays.Add(o);
    }

    public void TriggerTutorial(Dialogue.Condition condition, int value, string stringValue = "")
    {
        //Handles conversation pop ups
        Conversation convo = null;
        Dialogue matchedDialogue = null;
        foreach (Dialogue tut in currentDialogue)
        {
            Conversation thisConvo = tut.GetConversation(condition, value);
            if (thisConvo != null)
            {
                convo = thisConvo;
                matchedDialogue = tut;
            }
        }
        if (convo != null)
        {
            Time.timeScale = 0;
            StartCoroutine(DisplayTexts(convo));
            currentDialogue.Remove(matchedDialogue);
        }

        List<TutorialOverlay> matchedOverlays = new List<TutorialOverlay>();
        //Destroy all overlays with the ending conditions met
        foreach (TutorialOverlay overlay in existingOverlays.Keys)
            if (overlay.IfConditionsMet(condition, value, false, stringValue))
            {
                completedTutIDs.Add(overlay.ID);
                UIRevealController.UIReveal.SetElementState(overlay.OnEndUIReveal, true);
                Destroy(existingOverlays[overlay]);
            }

        //Handles on screen overlays
        foreach (TutorialOverlay overlay in currentTutorialOverlays)
        {
            //Create all overlays with the starting conditions met
            if (overlay.IfConditionsMet(condition, value, true, stringValue) &&
                (completedTutIDs.Contains(overlay.prerequisiteID) || completedPassiveTutIDs.Contains(overlay.prerequisiteID)) &&
                !completedTutIDs.Contains(overlay.ID))
            {
                if (overlay.slowTime)
                    StartCoroutine(SlowTime(overlay.slowDuration));

                if (overlay.isPopup)
                {
                    DisplayOverlayTutorial(overlay);
                }
                else
                {
                    GameObject thisOverlay = Instantiate(overlay.overlay);
                    if (overlay.isUILevel)
                        thisOverlay.transform.SetParent(tutorialUICanvas.transform, false);
                    else
                        thisOverlay.transform.SetParent(tutorialCanvas.transform);
                    existingOverlays[overlay] = thisOverlay;
                    matchedOverlays.Add(overlay);
                }
                UIRevealController.UIReveal.SetElementState(overlay.OnStartUIReveal, true);
            }
        }

        foreach (TutorialOverlay overlay in matchedOverlays)
            currentTutorialOverlays.Remove(overlay);

        //Handles passive tutorial pop up overlays
        if (popupOverlay == null)       //Only allows for 1 pop up overlay at a time
            if (StoryModeController.story == null || StoryModeController.story.GetCurrentRoomSetup() == null || !StoryModeController.story.GetCurrentRoomSetup().noAchievements)   //Never show passive tutorials in tutorial rooms
                foreach (TutorialOverlay overlay in passiveTutorials)
                {
                    if (overlay.IfConditionsMet(condition, value, true, stringValue) &&
                        (!completedPassiveTutIDs.Contains(overlay.ID) || repeatablePassiveTutorials.Contains(overlay.ID)))
                        DisplayOverlayTutorial(overlay);
                }

        //If tutorial room doesn't draw the full hand at the start, trigger it when appropriate
        if (GameController.gameController != null && HandController.handController != null && !handDrawn)
            if (condition == GameController.gameController.GetRoomSetup().drawHandCondition && value == GameController.gameController.GetRoomSetup().drawHandConditionValue)
            {
                handDrawn = true;
                HandController.handController.StartCoroutine(HandController.handController.DrawFullHand());
            }
    }

    public bool GetHasOverlayWithCondition(Dialogue.Condition condition, int value)
    {
        foreach (TutorialOverlay overlay in currentTutorialOverlays)
            if (overlay.startingCondition == condition && overlay.startingValue == value && !completedTutIDs.Contains(overlay.ID))
                return true;
        return false;
    }

    private void DisplayOverlayTutorial(TutorialOverlay overlay)
    {
        Time.timeScale = 0;
        popupOverlay = overlay;
        popupTitle.text = overlay.popupTitle;
        popupImage.sprite = overlay.popupImage;
        popUpLargeImage.sprite = overlay.popupImage;
        popupImage.enabled = !overlay.popupLargeImage;
        popUpLargeImage.enabled = overlay.popupLargeImage;
        popupDescription.text = overlay.popupDescription;
        popupTutorial.gameObject.SetActive(true);
        popUpSocialsButtons.SetActive(overlay.joinSocialsButtonsEnabled);
        popupID = overlay.ID;
        UIRevealController.UIReveal.SetElementState(popupOverlay.OnStartUIReveal, true);
    }

    public void PopupHide()
    {
        Time.timeScale = 1;
        popupTutorial.gameObject.SetActive(false);
        completedPassiveTutIDs.Add(popupID);
        completedTutIDs.Add(popupID);
        InformationLogger.infoLogger.SavePlayerPreferences();
        UIRevealController.UIReveal.SetElementState(popupOverlay.OnEndUIReveal, true);
        popupOverlay = null;
        TriggerTutorial(Dialogue.Condition.PopupEnded, popupID);
    }

    public bool GetPopupIsShowing()
    {
        return popupOverlay != null;
    }

    public void DiscordButtonPressed()
    {
        Application.OpenURL("https://discord.gg/yyUThk3bYg");
    }

    public void TwitterButtonPressed()
    {
        Application.OpenURL("https://twitter.com/deck_romancer");
    }

    public void ResetCompletedPassiveTutorialIDs()
    {
        completedPassiveTutIDs = new List<int> { -1 };
        InformationLogger.infoLogger.SavePlayerPreferences();
    }

    //Called at the end of the room to reset everything
    public void DestroyAndReset()
    {
        foreach (TutorialOverlay overlay in existingOverlays.Keys)
            Destroy(existingOverlays[overlay]);

        existingOverlays = new Dictionary<TutorialOverlay, GameObject>();
        popupTutorial.gameObject.SetActive(false);
        Time.timeScale = 1;
    }

    private IEnumerator DisplayTexts(Conversation conversation)
    {
        Time.timeScale = 0;
        SetEnabled(true);
        for (int i = 0; i < conversation.texts.Length; i++)
        {
            emoticon.sprite = GetEmoticon(conversation.speaker[i], conversation.emoticon[i]);
            text.enabled = false;
            text.text = conversation.texts[i];
            skipDialogueButton.gameObject.SetActive(StoryModeController.story.GetCompletedRooms().Contains(StoryModeController.story.GetCurrentRoomID()));  //Allow dialogue skips in rooms already completed
            yield return new WaitForSecondsRealtime(0.1f);  //Flash text between every dialogue to indicate text change
            text.enabled = true;

            while (!dialogueClicked && !dialogueSkipped)
            {
                yield return new WaitForSecondsRealtime(0.1f);
            }

            if (dialogueSkipped)
            {
                dialogueSkipped = false;
                break;
            }

            dialogueClicked = false;
        }
        SetEnabled(false);
        Time.timeScale = 1;
        TriggerTutorial(Dialogue.Condition.DialogueDone, 1);
    }

    public void ClickedDialogue()
    {
        dialogueClicked = true;
    }

    public void ClickedSkipDialogue()
    {
        dialogueSkipped = true;
    }

    private Sprite GetEmoticon(Dialogue.Speaker person, Dialogue.Emotion emote)
    {
        int index = speakerOrder.ToList().IndexOf(person);
        Sprite output = neutralEmote[index];
        switch (emote)
        {
            case Dialogue.Emotion.Happy:
                output = happyEmote[index];
                break;
            case Dialogue.Emotion.Wink:
                output = winkEmote[index];
                break;
            case Dialogue.Emotion.Kiss:
                output = kissEmote[index];
                break;
            case Dialogue.Emotion.Angry:
                output = angryEmote[index];
                break;
            case Dialogue.Emotion.Frustrated:
                output = frustratedEmote[index];
                break;
            case Dialogue.Emotion.Confused:
                output = confusedEmote[index];
                break;
            case Dialogue.Emotion.Surprised:
                output = surprisedEmote[index];
                break;
            case Dialogue.Emotion.Sad:
                output = sadEmote[index];
                break;
            case Dialogue.Emotion.Sleep:
                output = sleepEmote[index];
                break;
            case Dialogue.Emotion.Sunglasses:
                output = sunglassesEmote[index];
                break;
        }

        return output;
    }

    private void SetEnabled(bool state)
    {
        background.gameObject.SetActive(state);
        tutorialUICanvas.gameObject.SetActive(!state);
        tutorialCanvas.gameObject.SetActive(!state);

        if (ScoreController.score != null)
            ScoreController.score.SetTimerPaused(state);
    }

    public bool GetEnabled()
    {
        return background.gameObject.active;
    }

    private IEnumerator SlowTime(float duration)
    {
        float oldTimeScale = Time.timeScale;
        Time.timeScale = 0.5f;
        yield return new WaitForSeconds(duration);
        Time.timeScale = oldTimeScale;
    }

    public List<int> GetCompletedPassiveTutorials()
    {
        return completedPassiveTutIDs;
    }

    public void SetCompletedassiveTutorials(List<int> newIds)
    {
        if (newIds == null)
            newIds = new List<int> { -1 };
        completedPassiveTutIDs = newIds;
    }

    public void OpenFeedback()
    {
        Time.timeScale = 0;

        feedbackMenu.gameObject.SetActive(true);
    }

    public void CloseFeedback()
    {
        feedbackType = -1;
        for (int i = 0; i < feedbackTypes.Length; i++)
            feedbackTypes[i].color = unselectedColor;
        comments.text = "";
        feedbackInputs.SetActive(false);
        feedbackMenu.gameObject.SetActive(false);

        Time.timeScale = 1;
    }

    public void ReportFeedbackType(int value)
    {
        feedbackType = value;
        for (int i = 0; i < feedbackTypes.Length; i++)
            if (i == value)
                feedbackTypes[i].color = selectedColor;
            else
                feedbackTypes[i].color = unselectedColor;

        switch (value)
        {
            case 0:
                comments.placeholder.GetComponent<Text>().text = "Please be as specific as possible in bug reports. Include steps to recreate the bug if you can.";
                break;
            case 1:
                comments.placeholder.GetComponent<Text>().text = "We appreciate any and all feedback, positive or negative. Help us make the game we're aiming for.";
                break;
            case 2:
                comments.placeholder.GetComponent<Text>().text = "Is there a functionality you have in mind? An accessibility option that'll benefit you? Let us know!";
                break;
            case 3:
                comments.placeholder.GetComponent<Text>().text = "Any other type of responce is welcome too! Let us know what you think.";
                break;
        }

        feedbackInputs.SetActive(true);
    }

    public void SubmitFeedback()
    {
        if (!InformationLogger.infoLogger.debug)
        {
            string feedbackTypeString = "";
            switch (feedbackType)
            {
                case 0:
                    feedbackTypeString = "Bug Report";
                    break;
                case 1:
                    feedbackTypeString = "Feedback";
                    break;
                case 2:
                    feedbackTypeString = "Feature Request";
                    break;
                case 3:
                    feedbackTypeString = "Other";
                    break;
            }

            int worldLevel = -1;
            int selectedLevel = -1;
            string roomName = "null";
            try
            {
                worldLevel = RoomController.roomController.worldLevel;
                selectedLevel = RoomController.roomController.selectedLevel;
                roomName = RoomController.roomController.roomName;
            }
            catch
            {
                roomName = SceneManager.GetActiveScene().name;
            }

            int totalDamage = -1;
            int damageArmored = -1;
            int damageOverhealProtected = -1;
            int damageAvoided = -1;
            float secondsInGame = -1;

            try
            {
                totalDamage = ScoreController.score.GetDamage();
                damageArmored = ScoreController.score.GetDamageArmored();
                damageOverhealProtected = ScoreController.score.GetDamageOverhealProtected();
                damageAvoided = ScoreController.score.GetDamageAvoided();
                secondsInGame = ScoreController.score.GetSecondsInGame();
            }
            catch { }

            int achievement1Value = -1;
            int achievement2Value = -1;
            int achievement3Value = -1;

            try
            {
                achievement1Value = AchievementSystem.achieve.GetChallengeValue(StoryModeController.story.GetCurrentRoomSetup().challenges[0]);
                achievement2Value = AchievementSystem.achieve.GetChallengeValue(StoryModeController.story.GetCurrentRoomSetup().challenges[1]);
                achievement3Value = AchievementSystem.achieve.GetChallengeValue(StoryModeController.story.GetCurrentRoomSetup().challenges[2]);
            }
            catch { }

            InformationLogger.infoLogger.SaveSinglePlayerRoomInfo(InformationLogger.infoLogger.patchID,
                System.DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"),
                worldLevel.ToString(),
                selectedLevel.ToString(),
                roomName,
                "null",
                "-1",
                "-1",
                "-1",
                totalDamage.ToString(),
                damageArmored.ToString(),
                damageOverhealProtected.ToString(),
                damageAvoided.ToString(),
                ((int)secondsInGame).ToString(),
                "-1",
                achievement1Value.ToString(),
                achievement2Value.ToString(),
                achievement3Value.ToString(),
                PartyController.party.GetPartyString(),
                "False",
                "True",
                "-1",
                feedbackTypeString,
                comments.text,
                GetErrorLogs());
        }

        CloseFeedback();
    }

    public string GetErrorLogs()
    {
        string output = "";
        foreach (string log in latestErrorLogs)
            output += log + " ## ";

        return output;
    }
}
