using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;

public class MultiplayerInformationController : NetworkBehaviour
{
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
}
