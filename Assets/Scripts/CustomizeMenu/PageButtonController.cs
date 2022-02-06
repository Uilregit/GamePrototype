﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PageButtonController : MonoBehaviour
{
    public Color enabledColor;
    public Color disabledColor;

    public enum direction { Forward, Backward };

    public direction dir;

    private void OnMouseDown()
    {
        if (TutorialController.tutorial.GetEnabled())
            return;

        MusicController.music.PlaySFX(MusicController.music.paperMoveSFX[Random.Range(0, MusicController.music.paperMoveSFX.Count)]);
        if (dir == direction.Forward)
            CollectionController.collectionController.NextPage();
        else
            CollectionController.collectionController.PreviousPage();
    }

    public void Enable(bool state)
    {
        GetComponent<Collider2D>().enabled = state;
        if (state)
            GetComponent<Image>().color = enabledColor;
        else
            GetComponent<Image>().color = disabledColor;
    }
}
