using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;
using UnityEngine;
using Mirror;

public class MultiplayerInformationController : NetworkBehaviour
{
    [SerializeField] private float cardSpacing;
    [SerializeField] private float minHeight;
    [SerializeField] private float maxHeight;

    public int GetPlayerNumber()
    {
        if (isServer)
            return 0;
        else
            return 1;
    }

    public GameObject GetObjectFromNetID(string netID)
    {
        List<GameObject> possibleObjects = GameObject.FindGameObjectsWithTag("Player").ToList();
        possibleObjects.AddRange(GameObject.FindGameObjectsWithTag("Enemy"));
        foreach (GameObject obj in possibleObjects)
            if (obj.GetComponent<NetworkIdentity>().netId.ToString() == netID)
                return obj;
        return null;
    }

    [ClientRpc]
    public void ReportParty()
    {
        int partyNumber = 0;
        if (!isServer)
            partyNumber = 1;
        SetParty(PartyController.party.GetPlayerColorTexts(), partyNumber);
    }

    [Command]
    public void SetParty(string[] partycolors, int playerNumber)
    {
        List<Card.CasterColor> colors = new List<Card.CasterColor>();
        foreach (string s in partycolors)
            colors.Add(PartyController.party.GetPlayerCasterColor(s));
        MultiplayerGameController.gameController.parties[playerNumber] = colors;
        MultiplayerCoordinator.networkManager.ReportPartyDone();
    }

    [ClientRpc]
    public void SetFinalPartyColors(string[] partycolors, int playerNumber)
    {
        List<Card.CasterColor> colors = new List<Card.CasterColor>();
        foreach (string s in partycolors)
            colors.Add(PartyController.party.GetPlayerCasterColor(s));
        MultiplayerGameController.gameController.parties[playerNumber] = colors;
    }

    [Command]
    public void ReportGrid(byte[] grid, int playerNumber)
    {
        SetGrid(grid);
    }

    [Command]
    public void DebugReportGrid(string s, int playerNumber)
    {
        Debug.Log("player " + playerNumber + "sent this message");
        Debug.Log(s);
    }

    [ClientRpc]
    public void SetGrid(byte[] grid)
    {
        GridController.gridController.GetComponent<MultiplayerGridController>().SetGrid(grid);
    }

    [Command]
    public void ReportEndTurn(bool isServerTurn)
    {
        SetTurn(!isServerTurn);
    }

    [ClientRpc]
    public void SetTurn(bool serverTurn)
    {
        TurnController.turnController.SetEndTurnButtonEnabled(serverTurn == isServer);
        if (serverTurn)
        {
            MultiplayerGameController.gameController.SetTurnPlayerTags(0);
            StartCoroutine(TurnController.turnController.SetMultiplayerTurn(0));
        }
        else
        {
            MultiplayerGameController.gameController.SetTurnPlayerTags(1);
            StartCoroutine(TurnController.turnController.SetMultiplayerTurn(1));
        }
    }

    [Command]
    public void ReportCardUsed(string casterNetID, string cardName, List<Vector2> targetLocs, int playerNumber)
    {
        SetCardUsed(casterNetID, cardName, targetLocs, playerNumber);
    }

    [ClientRpc]
    public void SetCardUsed(string casterNetID, string cardName, List<Vector2> targetLocs, int playerNumber)
    {
        if (playerNumber != GetPlayerNumber())
        {
            GameObject caster = GetObjectFromNetID(casterNetID);

            Card card = LootController.loot.GetCardWithName(cardName);

            if (caster == null || card == null)
            {
                Debug.Log("Error casting card");
                Debug.Log(caster);
                Debug.Log(card);
                Debug.Log(cardName);
                return;
            }

            //Finding the location to show the card
            RaycastHit hit;
            Ray ray = new Ray(Camera.main.transform.position, transform.position - Camera.main.transform.position);
            Physics.Raycast(ray, out hit, LayerMask.GetMask("UI"));
            Vector3 canvasPosition = hit.point;

            //Place card to the opposite horrizontal side of the target from the caster
            float cardLocationX = canvasPosition.x + (cardSpacing / 2 + 0.5f) * Mathf.Sign(canvasPosition.x - targetLocs[0].x);
            cardLocationX = Mathf.Clamp(cardLocationX, -HandController.handController.cardHighlightXBoarder, HandController.handController.cardHighlightXBoarder);
            //Place the card at the caster height between an acceptable range
            float cardLocationY = Mathf.Clamp(canvasPosition.y, minHeight, maxHeight);
            Vector2 cardLocation = new Vector2(cardLocationX, cardLocationY);
            if (cardLocationX != canvasPosition.x + (cardSpacing / 2 + 0.5f) * Mathf.Sign(canvasPosition.x - targetLocs[0].x))
            {
                cardLocationY = canvasPosition.y + (cardSpacing / 2 + 0.5f) * Mathf.Sign(canvasPosition.y - targetLocs[0].y) * 1.3f;
                cardLocation = new Vector2(canvasPosition.x, cardLocationY);
            }

            //Showing the actual card
            MultiplayerCoordinator.networkManager.enemyCard.transform.position = new Vector3(0, 0, 0);

            MultiplayerCoordinator.networkManager.enemyCard.GetComponent<CardController>().SetCaster(caster);
            MultiplayerCoordinator.networkManager.enemyCard.GetComponent<CardController>().SetCard(card, true, true, true);
            MultiplayerCoordinator.networkManager.enemyCard.transform.GetChild(0).GetComponent<LineRenderer>().SetPosition(0, caster.transform.position);
            MultiplayerCoordinator.networkManager.enemyCard.transform.GetChild(0).GetComponent<LineRenderer>().SetPosition(1, targetLocs[0]);
            MultiplayerCoordinator.networkManager.enemyCard.transform.GetChild(0).GetComponent<CardDisplay>().Show();

            if (playerNumber == 1 && isServer)      //only trigger cards when it's on server and from the client
                StartCoroutine(TriggerCard(caster, targetLocs));
            else
                StartCoroutine(DestroyCard());
        }
    }

    private IEnumerator TriggerCard(GameObject caster, List<Vector2> targetLocs)
    {
        yield return StartCoroutine(MultiplayerCoordinator.networkManager.enemyCard.GetComponent<CardEffectsController>().TriggerEffect(caster, targetLocs, false));
        MultiplayerCoordinator.networkManager.enemyCard.transform.GetChild(0).GetComponent<CardDisplay>().Hide();
    }

    private IEnumerator DestroyCard()
    {
        yield return new WaitForSeconds(1f);
        MultiplayerCoordinator.networkManager.enemyCard.transform.GetChild(0).GetComponent<CardDisplay>().Hide();
    }

    [Command]
    public void ClientCardProcess(string casterNetID, List<Vector2> location, string cardName, int effectIndex, int tempValue, int tempduration)
    {
        ExecuteClientCardProcess(casterNetID, location, cardName, effectIndex, tempValue, tempduration);
    }

    [ClientRpc]
    public void ExecuteClientCardProcess(string casterNetID, List<Vector2> location, string cardName, int effectIndex, int tempValue, int tempduration)
    {
        if (!isServer)
        {
            GameObject caster = GetObjectFromNetID(casterNetID);
            Card card = LootController.loot.GetCardWithName(cardName);
            card.SetTempEffectValue(tempValue);
            card.SetTempDuration(tempduration);
            MultiplayerCoordinator.networkManager.enemyCard.GetComponent<CardController>().SetCaster(caster);
            MultiplayerCoordinator.networkManager.enemyCard.GetComponent<CardController>().SetCard(card, true, true, true);
            EffectFactory factory = new EffectFactory();
            Effect[] effects = factory.GetEffects(card.cardEffectName);
            StartCoroutine(effects[effectIndex].ProcessCard(caster, MultiplayerCoordinator.networkManager.enemyCard.GetComponent<CardEffectsController>(), location, card, effectIndex));
            //StartCoroutine(MultiplayerCoordinator.networkManager.enemyCard.GetComponent<CardEffectsController>().TriggerEffect(caster, location, false, true));
        }
    }

    [Command]
    public void ReportHealthController(string netID, byte[] healthInformation, int playerNumber)
    {
        SetHealthController(netID, healthInformation, playerNumber);
    }

    [ClientRpc]
    public void SetHealthController(string netID, byte[] healthInformation, int playerNumber)
    {
        if (!isServer)
        {
            MemoryStream stream = new MemoryStream();
            stream.Write(healthInformation, 0, healthInformation.Length);
            stream.Seek(0, SeekOrigin.Begin);
            BinaryFormatter formatter = new BinaryFormatter();
            HealthInformation value = (HealthInformation)formatter.Deserialize(stream);

            GetObjectFromNetID(netID).GetComponent<HealthController>().SetHealthInformation(value);
        }
    }

    [Command]
    public void ReportBuffs(string netID, byte[] buffs, int playerNumber)
    {
        SetBuffs(netID, buffs, playerNumber);
    }

    [ClientRpc]
    public void SetBuffs(string netID, byte[] buffs, int playerNumber)
    {
        if (!isServer)
        {
            MemoryStream stream = new MemoryStream();
            stream.Write(buffs, 0, buffs.Length);
            stream.Seek(0, SeekOrigin.Begin);
            BinaryFormatter formatter = new BinaryFormatter();
            BuffInfo value = (BuffInfo)formatter.Deserialize(stream);

            GetObjectFromNetID(netID).GetComponent<BuffController>().SetDummyBuffs(value);
        }
    }


    [Command]
    public void FinalizeGrid(int playerNumber)
    {
        foreach (GameObject obj in MultiplayerGameController.gameController.GetLivingPlayers(playerNumber))
        {
            GridController.gridController.RemoveFromPosition(obj, obj.transform.position);
            GridController.gridController.ReportPosition(obj, obj.transform.position);
        }
    }
}
