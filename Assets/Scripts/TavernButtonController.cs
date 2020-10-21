using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TavernButtonController : MonoBehaviour
{
    public Image[] buttonPartyColors;

    public void Awake ()
    {
        for (int i = 0; i < PartyController.party.partyColors.Length; i++)
            buttonPartyColors[i].color = PartyController.party.GetPlayerColor(PartyController.party.partyColors[i]);
    }

    public void OnClick()
    {
        SceneManager.LoadScene("TavernScene", LoadSceneMode.Single);
    }
}
