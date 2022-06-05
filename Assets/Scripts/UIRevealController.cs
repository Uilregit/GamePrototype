using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIRevealController : MonoBehaviour
{
    public static UIRevealController UIReveal;
    public enum UIElement
    {
        None = 1,
        Replace = 5,
        EnergyBar = 10,
        ManaBar = 11,
        EndTurnButton = 20,

        Intents = 31,
        Armor = 32,

        Gold = 51,
        Achievements = 52,
        Overkill = 53,

        Timer = 61,

        CombatCharacterStats = 101,

        OverworldDeckButton = 501,
        OverworldCollectionEquipmentAndDeckButton = 510,

        StoryModeBottomBar = 1001,
        StoryModeBottomCardMenu = 1002,
        StoryModeBottomEquipmentMenu = 1003,
        StoryModeBottomPartyMenu = 1004,
        StoryModeBottomSkillsMenu = 1005,

        StoryModeMapSceneAchievement = 1101,
        StoryModeMapSceneItem = 1102,
    }

    private Dictionary<UIElement, List<GameObject>> elements = new Dictionary<UIElement, List<GameObject>>();
    private Dictionary<UIElement, bool> elementStates = new Dictionary<UIElement, bool>();

    private void Awake()
    {
        if (UIRevealController.UIReveal == null)
        {
            UIReveal = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
            Destroy(this.gameObject);
    }

    public void ReportElement(UIElement elementName, GameObject obj)
    {
        if (!elements.ContainsKey(elementName))
            elements.Add(elementName, new List<GameObject> { obj });
        else
            elements[elementName].Add(obj);

        if (UnlocksController.unlock.GetUnlocks().uiElementUnlocked != null && UnlocksController.unlock.GetUnlocks().uiElementUnlocked.ContainsKey(elementName))
            obj.SetActive(UnlocksController.unlock.GetUnlocks().uiElementUnlocked[elementName]);
        else
            obj.SetActive(false);
    }

    public void SetElementState(UIElement elementName, bool state)
    {
        if (elements.ContainsKey(elementName))
            foreach (GameObject obj in elements[elementName])
                if (obj != null)
                    obj.SetActive(state);

        elementStates[elementName] = state;

        if (state && elements.ContainsKey(elementName))
        {
            if (UnlocksController.unlock.GetUnlocks().uiElementUnlocked == null)
                UnlocksController.unlock.GetUnlocks().uiElementUnlocked = new Dictionary<UIElement, bool>();
            if (UnlocksController.unlock.GetUnlocks().uiElementUnlocked.ContainsKey(elementName))
                UnlocksController.unlock.GetUnlocks().uiElementUnlocked[elementName] = true;
            else
                UnlocksController.unlock.GetUnlocks().uiElementUnlocked.Add(elementName, true);
            InformationLogger.infoLogger.SaveUnlocks();
        }
    }

    public bool GetElementState(UIElement elementName)
    {
        if (elementStates.ContainsKey(elementName))
            return elementStates[elementName];
        return true;
    }
}
