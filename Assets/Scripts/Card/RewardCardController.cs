using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardCardController : MonoBehaviour
{
    private float clickedTime;
    private float clickSelectDuration = 0.2f;
    private Vector3 localScale;
    private Vector3 originalLocation;

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
        transform.GetChild(0).GetComponent<CardDisplay>().cardSounds.PlayCastSound();
        transform.GetChild(0).GetComponent<CardDisplay>().SetCard(transform.GetChild(0).GetComponent<CardDisplay>().GetCard(), true);
        StopAllCoroutines();
        transform.localScale = localScale;
        transform.position = originalLocation;
        transform.GetChild(0).GetComponent<CardDisplay>().SetToolTip(false);
        if (Time.time - clickedTime < clickSelectDuration)
        {
            if (transform.GetChild(0).GetComponent<CardDisplay>().GetEquipment() == null)
            {
                CollectionController.collectionController.AddRewardsCard(transform.GetChild(0).GetComponent<CardDisplay>().GetCard());
                int deckId = PartyController.party.GetPartyIndex(transform.GetChild(0).GetComponent<CardDisplay>().GetCard().GetCard().casterColor);
                RewardsMenuController.rewardsMenu.SetDeckID(deckId);
            }
            else
                CollectionController.collectionController.AddRewardsEquipment(transform.GetChild(0).GetComponent<CardDisplay>().GetEquipment());

            RewardsMenuController.rewardsMenu.ReportRewardTaken(RewardsMenuController.RewardType.Card);
            RewardsMenuController.rewardsMenu.HideRewardCards();
            RewardsMenuController.rewardsMenu.SetItemsClickable(true);
        }
    }

    private IEnumerator EnlargeCard()
    {
        yield return new WaitForSeconds(0.3f);
        transform.GetChild(0).GetComponent<CardDisplay>().cardSounds.PlaySelectSound();
        transform.SetAsLastSibling();
        transform.position = new Vector3(Mathf.Clamp(originalLocation.x, HandController.handController.cardHighlightXBoarder * -1, HandController.handController.cardHighlightXBoarder), originalLocation.y + HandController.handController.GetCardHighlightHeight() * 1.1f, 0);
        transform.localScale = new Vector3(HandController.handController.GetCardHighlightSize() * 1.1f, HandController.handController.GetCardHighlightSize() * 1.1f, 1);
        transform.GetChild(0).GetComponent<CardDisplay>().SetCard(transform.GetChild(0).GetComponent<CardDisplay>().GetCard(), false);
        transform.GetChild(0).GetComponent<CardDisplay>().SetToolTip(true);
    }
}
