using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ShopDoneButtonController : MonoBehaviour
{
    private int deckID;
    private Vector3 cameraLocation;
    private void Awake()
    {
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
        cameraLocation = new Vector3(0, 0, -10);
    }

    public void OnMouseDown()
    {
        ShopController.shop.RecordShopInformation();
        //GameController.gameController.LoadScene("OverworldScene", ShopController.shop.GetBoughtCard(), ShopController.shop.GetLatestDeckID()); //Don't go to deck editing if a card was not bought

        if (ShopController.shop.GetBoughtCard())
            cameraLocation = new Vector3(8, 0, -10);
        else
            cameraLocation = new Vector3(0, 0, -10);
        deckID = ShopController.shop.GetLatestDeckID();
        RoomController.roomController.SetViableRoom(new Vector2(-999, -999));
        InformationLogger.infoLogger.SaveGame(false);
        RoomController.roomController.Refresh();
        RoomController.roomController.Show();
        SceneManager.LoadScene("OverworldScene", LoadSceneMode.Single);
    }

    private void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        CollectionController.collectionController.SetDeck(deckID);
        if (SceneManager.GetActiveScene().name == "OverworldScene")
        {
            CameraController.camera.transform.position = cameraLocation;
            SceneManager.sceneLoaded -= OnLevelFinishedLoading;
            Destroy(this.gameObject);
        }
        /*
        else
            CameraController.camera.transform.position = new Vector3(0, 0, -10);
        */
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }
}
