using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;
using Mirror;

public class MultiplayerGameController : NetworkBehaviour
{
    public static MultiplayerGameController gameController;

    //public Sprite background;
    public Image damageOverlay;
    public Image circleHighlight;

    public Text text;
    public CardDisplay[] rewardCards;

    [SerializeField]
    private int playerNumber = 0;
    [SerializeField]
    private List<GameObject> p1Players;
    [SerializeField]
    private List<GameObject> p2Players;
    private List<GameObject>[] players;
    public List<Card.CasterColor>[] parties;

    [Header("(x1, x2, y1, y2)")]
    public int[] playerSpawnBox = new int[4]; //(x1,x2,y1,y2)
    public int[] enemySpawnBox = new int[4];

    [Header("Buffs")]
    public Buff attackBuff;
    public Buff armorBuff;
    public Buff stunBuff;

    private List<Card.CasterColor>[] deadChars;

    public void Awake()
    {
        MultiplayerGameController.gameController = this;

        deadChars = new List<Card.CasterColor>[2];
        deadChars[0] = new List<Card.CasterColor>();
        deadChars[1] = new List<Card.CasterColor>();
        players = new List<GameObject>[2];
        players[0] = p1Players;
        players[1] = p2Players;
        parties = new List<Card.CasterColor>[2];
        parties[0] = new List<Card.CasterColor>();
        parties[1] = new List<Card.CasterColor>();

        //Hide the reward cards
        for (int i = 0; i < rewardCards.Length; i++)
        {
            rewardCards[i].Hide();
            rewardCards[i].transform.parent.GetComponent<Collider2D>().enabled = false;
        }

        DeckController.deckController.PopulateDecks();
        DeckController.deckController.ResetCardValues();
        DeckController.deckController.ShuffleDrawPile();

        if (InformationLogger.infoLogger.debug)
            GridController.gridController.DebugGrid();

        RelicController.relic.OnNotify(this, Relic.NotificationType.OnCombatStart, null);

        //Setup replace and hold areas
        if (HandController.handController.maxReplaceCount == 0)
        {
            GameObject.FindGameObjectWithTag("Replace").GetComponent<Collider>().enabled = false;
            GameObject.FindGameObjectWithTag("Replace").GetComponent<Image>().enabled = false;
            GameObject.FindGameObjectWithTag("Replace").transform.GetChild(0).GetComponent<Text>().enabled = false;
            GameObject.FindGameObjectWithTag("Replace").transform.GetChild(1).GetComponent<Text>().enabled = false;
        }
        if (!HandController.handController.allowHold)
        {
            GameObject.FindGameObjectWithTag("Hold").GetComponent<Collider>().enabled = false;
            GameObject.FindGameObjectWithTag("Hold").GetComponent<Image>().enabled = false;
            GameObject.FindGameObjectWithTag("Hold").transform.GetChild(0).GetComponent<Text>().enabled = false;
        }
    }

    private IEnumerator DisplayVictoryText()
    {
        CameraController.camera.ScreenShake(0.06f, 0.05f);
        text.text = "VICTORY";
        text.enabled = true;
        yield return new WaitForSeconds(TimeController.time.victoryTextDuration * TimeController.time.timerMultiplier);
        text.enabled = false;
    }

    private IEnumerator DisplayDefeatText()
    {
        CameraController.camera.ScreenShake(0.06f, 0.05f);
        text.text = "DEFEAT";
        text.enabled = true;
        yield return new WaitForSeconds(TimeController.time.victoryTextDuration * TimeController.time.timerMultiplier);
        text.enabled = false;
    }

    public IEnumerator Victory()
    {
        GridController.gridController.DisableAllPlayers();
        //HandController.handController.ClearHand();
        HandController.handController.EmptyHand();

        DeckController.deckController.ResetCardValues();

        CameraController.camera.ScreenShake(0.4f, 2f, true);
        yield return new WaitForSeconds(2.5f);


        yield return StartCoroutine(DisplayVictoryText());
    }

    public IEnumerator Defeat()
    {
        GridController.gridController.DisableAllPlayers();
        //HandController.handController.ClearHand();
        HandController.handController.EmptyHand();

        DeckController.deckController.ResetCardValues();

        CameraController.camera.ScreenShake(0.4f, 2f, true);
        yield return new WaitForSeconds(2.5f);


        yield return StartCoroutine(DisplayDefeatText());
    }

    public void ReportDeadChar(Card.CasterColor color)
    {
        int playerNumber = 0;
        if (!isServer)
            playerNumber = 1;

        deadChars[playerNumber].Add(color);
        if (deadChars[playerNumber].Count >= 3)
        {
            TurnController.turnController.StopAllCoroutines();
            CanvasController.canvasController.uiCanvas.enabled = false;
            HandController.handController.EmptyHand();
            CanvasController.canvasController.endGameCanvas.enabled = true;
            CanvasController.canvasController.endGameCanvas.GetComponent<CanvasScaler>().enabled = false;
            CanvasController.canvasController.endGameCanvas.GetComponent<CanvasScaler>().enabled = true;
            CanvasController.canvasController.endGameCanvas.transform.GetChild(2).GetComponent<Collider2D>().enabled = true;
        }
    }

    public List<GameObject> GetLivingPlayers()
    {
        int playerNumber = 0;
        if (!isServer)
            playerNumber = 1;

        return GetLivingPlayers(playerNumber);
    }

    public List<GameObject> GetLivingPlayers(int playerNumber)
    {
        string tag = "Player";
        if (playerNumber == 1)
            tag = "Enemy";
        List<GameObject> players = GameObject.FindGameObjectsWithTag(tag).ToList();
        List<GameObject> output = new List<GameObject>();
        foreach (GameObject obj in players)
            if (!deadChars[playerNumber].Contains(obj.GetComponent<MultiplayerPlayerController>().GetColorTag()))
                output.Add(obj);
        return output;
    }

    public void ReportResurrectedChar(Card.CasterColor color)
    {
        deadChars[playerNumber].Remove(color);
    }

    public List<Card.CasterColor> GetDeadChars()
    {
        return deadChars[playerNumber];
    }

    public void SetDamageOverlay(float remainingHealthPercentage)
    {
        damageOverlay.color = new Color(1, 0, 0, 1 - remainingHealthPercentage / 2);
        StartCoroutine(FadeDamageOverlay(remainingHealthPercentage / 2));
    }

    private IEnumerator FadeDamageOverlay(float remainingHealthPercentage)
    {
        float elapsedTime = 0;
        while (elapsedTime < 0.4f)
        {
            damageOverlay.color = Color.Lerp(new Color(1, 0, 0, 1 - remainingHealthPercentage), new Color(1, 0, 0, 0), elapsedTime / 0.4f);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        damageOverlay.color = new Color(1, 0, 0, 0);
    }

    public void SetCircleOverlay(bool value, Vector2 location)
    {
        circleHighlight.transform.position = location;
        if (value)
            StartCoroutine(FadeInDamageOverlay());
        else
            StartCoroutine(FadeOutDamageOverlay());
    }

    private IEnumerator FadeInDamageOverlay()
    {
        float elapsedTime = 0;
        while (elapsedTime < 0.1f)
        {
            circleHighlight.color = Color.Lerp(new Color(0, 0, 0, 0), new Color(0, 0, 0, 0.5f), elapsedTime / 0.1f);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        damageOverlay.color = new Color(0, 0, 0, 0.5f);
    }

    private IEnumerator FadeOutDamageOverlay()
    {
        float elapsedTime = 0;
        while (elapsedTime < 0.1f)
        {
            circleHighlight.color = Color.Lerp(new Color(0, 0, 0, 0.5f), new Color(0, 0, 0, 0), elapsedTime / 0.1f);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        damageOverlay.color = new Color(0, 0, 0, 0);
    }

    [ClientRpc]
    public void SetPlayerNumber()
    {
        if (isServer)
            playerNumber = 0;
        else
            playerNumber = 1;

        //parties[playerNumber] = PartyController.party.partyColors.ToList();
        for (int i = 0; i < 2; i++)
        {
            List<GameObject> removeList = new List<GameObject>();
            foreach (GameObject obj in players[i])
            {
                if (!parties[i].Contains(obj.GetComponent<MultiplayerPlayerController>().GetColorTag()))
                {
                    obj.gameObject.SetActive(false);
                    removeList.Add(obj);
                }
                else if (i == playerNumber)
                {
                    obj.GetComponent<MultiplayerPlayerController>().Spawn(i);
                }
            }

            foreach (GameObject obj in removeList)
            {
                players[i].Remove(obj);
            }
        }
    }

    public void SetPlayer2Connection(NetworkConnection conn)
    {
        foreach (GameObject obj in players[1])
        {
            if (parties[1].Contains(obj.GetComponent<MultiplayerPlayerController>().GetColorTag()))
            {
                obj.GetComponent<NetworkIdentity>().RemoveClientAuthority();
                obj.GetComponent<NetworkIdentity>().AssignClientAuthority(conn);
            }
        }

        GridController.gridController.GetComponent<NetworkIdentity>().RemoveClientAuthority();
        GridController.gridController.GetComponent<NetworkIdentity>().AssignClientAuthority(conn);
    }

    [ClientRpc]
    public void SetLocalPlayerColors()
    {
        List<GameObject> p = new List<GameObject>();
        if (isServer)
            p = players[1];
        else
            p = players[0];
        foreach (GameObject obj in p)
        {
            obj.transform.GetChild(0).GetChild(4).GetComponent<SpriteRenderer>().color = PartyController.party.GetPlayerColor(Card.CasterColor.Enemy) * new Color(1, 1, 1, 0.24f);
            obj.transform.GetChild(0).GetChild(4).GetChild(0).GetComponent<SpriteRenderer>().color = PartyController.party.GetPlayerColor(Card.CasterColor.Enemy);
        }
    }

    [ClientRpc]
    public void StartGame()
    {
        DeckController.deckController.PopulateDecks();
        DeckController.deckController.ResetCardValues();
        DeckController.deckController.ShuffleDrawPile();

        HandController.handController.StartCoroutine(HandController.handController.DrawFullHand());

        //Set turn to server side
        TurnController.turnController.SetEndTurnButtonEnabled(isServer);
        StartCoroutine(TurnController.turnController.SetMultiplayerTurn(0));
    }
}
