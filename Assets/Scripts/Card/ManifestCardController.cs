﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManifestCardController : MonoBehaviour
{
    private float clickedTime;
    private float clickSelectDuration = 0.2f;
    private Vector3 localScale;
    private Vector3 originalLocation;
    private CardController thisCard;
    private bool hasCard;

    private void Awake()
    {
        localScale = transform.localScale;
        originalLocation = transform.position;
    }

    private void OnMouseDown()
    {
        clickedTime = Time.time;
        StartCoroutine(EnlargeCard());
    }

    private void OnMouseUp()
    {
        StopAllCoroutines();
        transform.localScale = localScale;
        transform.position = originalLocation;
        if (Time.time - clickedTime < clickSelectDuration)
        {
            UIController.ui.ReportChosenManifestCard(thisCard);
        }
    }

    private IEnumerator EnlargeCard()
    {
        yield return new WaitForSeconds(0.3f);
        transform.SetAsLastSibling();
        transform.position = new Vector3(Mathf.Clamp(originalLocation.x, HandController.handController.cardHighlightXBoarder * -1, HandController.handController.cardHighlightXBoarder), originalLocation.y + HandController.handController.cardHighlightHeight, 0);
        transform.localScale = new Vector3(HandController.handController.cardHighlightSize, HandController.handController.cardHighlightSize, 1);
    }
    public void SetCard(CardController card)
    {
        thisCard = card;
        transform.GetChild(0).GetComponent<CardDisplay>().SetCard(card, true);
        hasCard = true;
    }

    public bool GetHasCard()
    {
        return hasCard;
    }
}