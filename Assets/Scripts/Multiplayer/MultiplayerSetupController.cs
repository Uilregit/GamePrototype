using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MultiplayerSetupController : TavernController
{
    public Canvas deckCanvas;
    public Image deckButton;

    private int selectedIndex = -1;
    // Start is called before the first frame update
    void Awake()
    {
        foreach (Image img in reserves)
        {
            img.enabled = false;
            img.transform.GetChild(0).GetComponent<Text>().enabled = false;
        }

        for (int i = 0; i < PartyController.party.partyColors.Length; i++)
        {
            party[i].color = PartyController.party.GetPlayerColor(PartyController.party.partyColors[i]);
            party[i].transform.GetChild(0).GetComponent<Text>().text = "Lv." + PartyController.party.GetPartyLevelInfo(PartyController.party.partyColors[i])[0].ToString();
        }
    }

    public new void StartEditing(int index)
    {
        selectedIndex = index;

        int i = 0;
        foreach (Card.CasterColor c in PartyController.party.unlockedPlayerColors)
            if (!PartyController.party.partyColors.Contains(c))
            {
                reserves[i].color = PartyController.party.GetPlayerColor(c);
                reserves[i].GetComponent<TavernIconsController>().SetColor(c);
                reserves[i].enabled = true;
                reserves[i].transform.GetChild(0).GetComponent<Text>().text = "Lv." + PartyController.party.GetPartyLevelInfo(c)[0].ToString();
                reserves[i].transform.GetChild(0).GetComponent<Text>().enabled = true;
                i++;
            }
    }

    public new void ReportSelected(int index)
    {
        List<Card.CasterColor> colors = new List<Card.CasterColor>();
        foreach (Card.CasterColor c in PartyController.party.unlockedPlayerColors)
            if (!PartyController.party.partyColors.Contains(c))
                colors.Add(c);
        Card.CasterColor newColor = colors[index];
        Debug.Log("report selected");
        party[selectedIndex].color = PartyController.party.GetPlayerColor(newColor);
        party[selectedIndex].transform.GetChild(0).GetComponent<Text>().text = "Lv." + PartyController.party.GetPartyLevelInfo(newColor)[0].ToString();
        PartyController.party.partyColors[selectedIndex] = newColor;

        foreach (Image img in reserves)
        {
            img.enabled = false;
            img.transform.GetChild(0).GetComponent<Text>().enabled = false;
        }
        CollectionController.collectionController.FinalizeMultiplayerDeck();
    }

    public void GoToDeckMenu()
    {
        Camera.main.transform.position = new Vector3(8, 0, -10);
    }
    /*
    public void GoToMainMenu()
    {
        CollectionController.collectionController.FinalizeDeck();
        CameraController.camera.transform.position = new Vector3(0, 0, -10);
    }
    */
    public void FindMatchButton()
    {
        //DeckController.deckController.SetDecks()
        CollectionController.collectionController.FinalizeMultiplayerDeck();
        SceneManager.LoadScene("MultiplayerScene", LoadSceneMode.Single);
    }
}
