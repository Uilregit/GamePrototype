using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardCardController : MonoBehaviour
{
    private float clickedTime;
    private float clickSelectDuration = 0.2f;
    private Vector2 localScale;
    private Vector2 originalLocation;

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
            CollectionController.collectionController.AddRewardsCard(GetComponent<CardDisplay>().GetCard());
            int deckId = PartyController.party.GetPartyIndex(GetComponent<CardDisplay>().GetCard().GetCard().casterColor);
            RewardsMenuController.rewardsMenu.SetDeckID(deckId);

            RewardsMenuController.rewardsMenu.ReportRewardTaken(0);
            RewardsMenuController.rewardsMenu.HideRewardCards();
            RewardsMenuController.rewardsMenu.SetItemsClickable(true);
        }
    }

    private IEnumerator EnlargeCard()
    {
        yield return new WaitForSeconds(0.3f);
        transform.SetAsLastSibling();
        transform.position = new Vector2(Mathf.Clamp(originalLocation.x, HandController.handController.cardHighlightXBoarder * -1, HandController.handController.cardHighlightXBoarder), originalLocation.y + HandController.handController.cardHighlightHeight);
        transform.localScale = new Vector2(HandController.handController.cardHighlightSize, HandController.handController.cardHighlightSize);
    }
}
