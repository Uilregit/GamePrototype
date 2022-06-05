using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SmallRoom : MonoBehaviour
{
    public Text roundNumber;
    public Image background;
    public Image backgroundColor;
    public Image mainEnemyIcon;
    public Image lockedButton;
    public Image lockedTitleButton;
    public Image completedButton;
    public Image[] enemyIcons;
    public Text[] enemyCounts;
    public LineRenderer line;

    public GameObject bossBorder;

    private Vector2 location;

    private bool destroyed = false;

    private bool selectable = false;
    private RoomController.roomType type = RoomController.roomType.combat;

    private RoomSetup setup;

    private int seed;
    private bool shouldEnterRoom = false;

    private void Update()
    {
        if (shouldEnterRoom)
            if (SceneManager.GetActiveScene().name == "OverworldScene")
            {
                shouldEnterRoom = false;
                Enter();
            }
    }

    private void OnMouseDown()
    {
        if (TutorialController.tutorial.GetEnabled())
            return;

        if (selectable)
        {
            Enter();
        }
    }

    public void Enter()
    {
        if (TutorialController.tutorial.GetEnabled() || !selectable)
            return;

        RoomController.roomController.SetCurrentRoomSetup(setup);
        if (setup != null)
            RoomController.roomController.roomName = setup.roomName;

        RoomController.roomController.SetViableRoom(location);
        InformationLogger.infoLogger.SaveGame(false);

        RoomController.roomController.selectedLevel = (int)location.y;
        //setup = RoomController.roomController.GetRoomSetup();
        //RoomController.roomController.SetCurrentRoomSetup(setup);

        RoomController.roomController.Hide();
        RoomController.roomController.SetPreviousRoom(this);
        RoomController.roomController.SetCurrentSmallRoom(this);

        Random.InitState(seed);

        string roomString = "Error";
        if (type == RoomController.roomType.combat)
            roomString = "CombatScene";
        else if (type == RoomController.roomType.shop)
            roomString = "ShopScene";
        else if (type == RoomController.roomType.shrine)
            roomString = "ShrineScene";

        if (roomString == "CombatScene")
            InformationLogger.infoLogger.SaveTimeInfo(InformationLogger.infoLogger.patchID,
                                InformationLogger.infoLogger.gameID,
                                RoomController.roomController.worldLevel.ToString(),
                                RoomController.roomController.selectedLevel.ToString(),
                                roomString,
                                RoomController.roomController.roomName,
                                "0P");
        else
            InformationLogger.infoLogger.SaveTimeInfo(InformationLogger.infoLogger.patchID,
                                InformationLogger.infoLogger.gameID,
                                RoomController.roomController.worldLevel.ToString(),
                                RoomController.roomController.selectedLevel.ToString(),
                                roomString,
                                roomString);

        if (roomString == "ShopScene" || roomString == "ShrineScene")
            MusicController.music.SetHighPassFilter(true);
        else if (setup.overrideParty != null && setup.overrideParty.Length > 0)
            PartyController.party.SetOverrideParty(true);

        RoomController.roomController.EnterRoom(roomString);
    }

    public void SetRoomType(RoomController.roomType newType)
    {
        type = newType;
    }

    public RoomController.roomType GetRoomType()
    {
        return type;
    }

    public void SetSetup(RoomSetup newSetup, int round, int maxRound)
    {
        setup = newSetup;
        background.sprite = RoomController.roomController.GetCombatBackground();
        roundNumber.text = "Round " + round + "/" + maxRound;

        Dictionary<EnemyController, int> enemies = new Dictionary<EnemyController, int>();
        EnemyController mainEnemy = null;
        foreach (GameObject enemy in newSetup.enemies)
        {
            EnemyController thisEnemy = enemy.GetComponent<EnemyController>();
            if (enemies.ContainsKey(thisEnemy))
                enemies[thisEnemy] += 1;
            else
                enemies[thisEnemy] = 1;

            if (mainEnemy == null)
                mainEnemy = thisEnemy;
            else if (thisEnemy.attack > mainEnemy.attack)
                mainEnemy = thisEnemy;
            else if (thisEnemy.attack == mainEnemy.attack && thisEnemy.startingArmor + thisEnemy.maxVit > mainEnemy.startingArmor + mainEnemy.maxVit)
                mainEnemy = thisEnemy;
        }

        mainEnemyIcon.sprite = mainEnemy.transform.GetChild(0).GetComponent<CharacterDisplayController>().sprite.sprite;

        int counter = 0;
        foreach (EnemyController thisEnemy in enemies.Keys)
        {
            enemyIcons[counter].sprite = thisEnemy.transform.GetChild(0).GetComponent<CharacterDisplayController>().sprite.sprite;
            enemyCounts[counter].text = "x " + enemies[thisEnemy];

            if (counter + 1 == enemies.Keys.Count && enemies.Keys.Count % 2 == 1)
                enemyIcons[counter].transform.localPosition = new Vector3(-0.4f, enemyIcons[counter].transform.localPosition.y);

            counter++;
        }
        for (int i = counter; i < 4; i++)
        {
            enemyIcons[i].enabled = false;
            enemyCounts[i].enabled = false;
        }

        bossBorder.SetActive(newSetup.isBossRoom);
    }

    public void SetSelectable(bool state)
    {

        selectable = state;
        lockedButton.gameObject.SetActive(!state);
        /*  Used to enter room immediatel if it's the first room
        if (selectable && (RoomController.roomController.GetCurrentRoomSetup() == null))
            shouldEnterRoom = true;
        */
    }

    public void SetCompleted(bool state)
    {
        completedButton.gameObject.SetActive(state);
    }

    public void SetType(RoomController.roomType value)
    {
        type = value;
        switch (value)
        {
            case RoomController.roomType.shop:
                roundNumber.text = "Shop";
                mainEnemyIcon.gameObject.SetActive(false);
                foreach (Image enemy in enemyIcons)
                    enemy.gameObject.SetActive(false);
                break;
            case RoomController.roomType.shrine:
                roundNumber.text = "Shrine";
                mainEnemyIcon.gameObject.SetActive(false);
                foreach (Image enemy in enemyIcons)
                    enemy.gameObject.SetActive(false);
                break;
        }
    }

    public void Hide()
    {
        GetComponent<Image>().enabled = false;
        selectable = false;
        line.enabled = false;
    }

    public void Show()
    {
        GetComponent<Image>().enabled = true;
        //selectable = true;
        line.enabled = true;
    }

    public void SetColor(Color color)
    {
        //GetComponent<Image>().color = color;
        backgroundColor.color = color;
        roundNumber.transform.parent.GetComponent<Image>().color = color;
        roundNumber.transform.parent.transform.GetChild(0).GetComponent<Image>().color = color;
        roundNumber.transform.parent.transform.GetChild(1).GetComponent<Image>().color = color;
    }

    public void SetWorldColor(Color color)
    {
        backgroundColor.color = color;
        roundNumber.transform.parent.GetComponent<Image>().color = color;
        roundNumber.transform.parent.transform.GetChild(0).GetComponent<Image>().color = color;
        roundNumber.transform.parent.transform.GetChild(1).GetComponent<Image>().color = color;
    }

    public Vector2 GetLocation()
    {
        return location;
    }

    public void SetLocation(Vector2 vec)
    {
        location = vec;
    }

    public void SetDestroyed(bool value)
    {
        destroyed = value;
    }

    public void SetSeed(int value)
    {
        seed = value;
    }

    public int GetSeed()
    {
        return seed;
    }

    public void SetPreviousRoom(Vector3 loc, Color lineColor)
    {
        Vector3[] positions = new Vector3[4];

        positions[0] = location - location;
        positions[1] = new Vector3(location.x, (location.y + loc.y) / 2f, 0) - new Vector3(location.x, location.y);
        positions[2] = new Vector3(loc.x, (location.y + loc.y) / 2f, 0) - new Vector3(location.x, location.y);
        positions[3] = loc - new Vector3(location.x, location.y);

        if (RoomController.roomController.GetRoomScrollController().GetIsHorizontal())
            for (int i = 0; i < 4; i++)
                positions[i] = new Vector3(positions[i].y * 3.5f, 0, 0);
        else
            for (int i = 0; i < 4; i++)
                positions[i] = new Vector3(positions[i].x * 3.5f, positions[i].y * 6f, positions[i].z * 1f);

        line.SetPositions(positions);
        line.startColor = lineColor;
        line.endColor = lineColor;
    }

    public void SetLockedTitle()
    {
        lockedTitleButton.gameObject.SetActive(true);
    }
}
