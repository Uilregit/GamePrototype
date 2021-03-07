using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Mirror;

public class MultiplayerCoordinator : NetworkManager
{
    public static MultiplayerCoordinator networkManager;

    public MultiplayerInformationController[] players = new MultiplayerInformationController[2];
    private NetworkConnection[] connections = new NetworkConnection[2];
    public MultiplayerGameController gameController;
    public GridController[] grid = new GridController[2];
    public GameObject enemyCard;

    private int partyInfosGotten = 0;

    private void Awake()
    {
        MultiplayerCoordinator.networkManager = this;
        enemyCard.transform.GetChild(0).GetComponent<CardDisplay>().Hide();
    }

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        GameObject player = Instantiate(playerPrefab);
        NetworkServer.AddPlayerForConnection(conn, player);
        players[numPlayers - 1] = player.GetComponent<MultiplayerInformationController>();
        players[numPlayers - 1].ReportParty();
        connections[numPlayers - 1] = conn;
    }

    public void ReportPartyDone()
    {
        partyInfosGotten += 1;
        if (partyInfosGotten == 2)
        {
            players[0].SetFinalPartyColors(GetPartyColorTexts(MultiplayerGameController.gameController.parties[0]), 0);
            players[0].SetFinalPartyColors(GetPartyColorTexts(MultiplayerGameController.gameController.parties[1]), 1);
            players[1].SetFinalPartyColors(GetPartyColorTexts(MultiplayerGameController.gameController.parties[0]), 0);
            players[1].SetFinalPartyColors(GetPartyColorTexts(MultiplayerGameController.gameController.parties[1]), 1);
            gameController.SetPlayer2Connection(connections[1]);
            gameController.SetPlayerNumber();
            gameController.SetLocalPlayerColors();
            gameController.StartGame();
        }
    }

    public string[] GetPartyColorTexts(List<Card.CasterColor> colors)
    {
        List<string> output = new List<string>();
        foreach (Card.CasterColor c in colors)
            output.Add(PartyController.party.GetPlayerColorText(c));
        return output.ToArray();
    }
}
