using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;

public class MultiplayerInformationController : NetworkBehaviour
{
    private bool isServerTurn = true;

    public int GetPlayerNumber()
    {
        if (isServer)
            return 0;
        else
            return 1;
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
    public void ReportCardUsed(string casterNetID, string cardName, List<Vector2> targetLocs, int playerNumber)
    {
        SetCardUsed(casterNetID, cardName, targetLocs, playerNumber);
    }

    [ClientRpc]
    public void SetCardUsed(string casterNetID, string cardName, List<Vector2> targetLocs, int playerNumber)
    {
        if (playerNumber != GetPlayerNumber())
        {

        }
    }

    [Command]
    public void ReportEndTurn()
    {
        SetTurn(!isServerTurn);
    }

    [ClientRpc]
    public void SetTurn(bool serverTurn)
    {
        Debug.Log("turn set");
        Debug.Log(serverTurn);
        isServerTurn = serverTurn;
        TurnController.turnController.SetEndTurnButtonEnabled(serverTurn == isServer);
        if (isServerTurn)
            StartCoroutine(TurnController.turnController.SetMultiplayerTurn(0));
        else
            StartCoroutine(TurnController.turnController.SetMultiplayerTurn(1));
    }
}
