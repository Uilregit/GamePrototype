using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TavernController : MonoBehaviour
{
    public Image[] party;
    public Image[] reserves;

    private int selectedIndex = -1;
    // Start is called before the first frame update
    void Awake()
    {
        foreach (Image img in reserves)
            img.enabled = false;

        for (int i = 0; i < PartyController.party.partyColors.Length; i++)
            party[i].color = PartyController.party.GetPlayerColor(PartyController.party.partyColors[i]);
    }

    public void StartEditing(int index)
    {
        selectedIndex = index;

        int i = 0;
        foreach (Card.CasterColor c in PartyController.party.potentialPlayerColors)
            if (!PartyController.party.partyColors.Contains(c))
            {
                reserves[i].color = PartyController.party.GetPlayerColor(c);
                reserves[i].GetComponent<TavernIconsController>().SetColor(c);
                reserves[i].enabled = true;
                i++;
            }
    }

    public void ReportSelected(Card.CasterColor newColor)
    {
        party[selectedIndex].color = PartyController.party.GetPlayerColor(newColor);
        PartyController.party.partyColors[selectedIndex] = newColor;

        foreach (Image img in reserves)
            img.enabled = false;
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenuScene", LoadSceneMode.Single);
    }
}
