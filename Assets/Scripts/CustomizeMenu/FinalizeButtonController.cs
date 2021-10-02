using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FinalizeButtonController : MonoBehaviour
{
    public Color enableColor;
    public Color disableColor;
    public ButtonType buttonType;
    public FinalizeButtonController cardButton;
    public FinalizeButtonController gearButton;

    public enum ButtonType
    {
        Back,
        Card,
        Gear
    }

    private void Awake()
    {
        if (buttonType == ButtonType.Card)
        {
            cardButton.Enable(false);
            gearButton.Enable(true);
        }
    }

    private void OnMouseDown()
    {
        if (SceneManager.GetActiveScene().name != "OverworldScene" && SceneManager.GetActiveScene().name != "ShopScene")
            return;

        switch (buttonType)
        {
            case ButtonType.Back:
                if (!CollectionController.collectionController.CheckDeckComplete())
                {
                    string incompeteColor = "";
                    foreach (Card.CasterColor color in PartyController.party.partyColors)
                        if (!CollectionController.collectionController.CheckDeckComplete(color))
                        {
                            incompeteColor = color.ToString();
                            break;
                        }
                    CollectionController.collectionController.ShowErrorMessage("Incomplete " + incompeteColor + " Deck");
                }
                else
                {
                    CollectionController.collectionController.FinalizeDeck();
                    try
                    {
                        CollectionController.collectionController.LogInformation();
                    }
                    catch { }
                    CameraController.camera.transform.position = new Vector3(0, 0, -10);
                }
                break;
            case ButtonType.Card:
                CollectionController.collectionController.SetIsShowingCards(true);
                CollectionController.collectionController.SetPage(0);
                cardButton.Enable(false);
                gearButton.Enable(true);
                break;
            case ButtonType.Gear:
                CollectionController.collectionController.SetIsShowingCards(false);
                CollectionController.collectionController.SetPage(0);
                cardButton.Enable(true);
                gearButton.Enable(false);
                break;
        }
    }

    public void Enable(bool state)
    {
        if (state)
            GetComponent<Image>().color = enableColor;
        else
            GetComponent<Image>().color = disableColor;
        GetComponent<Collider2D>().enabled = state;
    }
}