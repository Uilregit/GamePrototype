﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewAbilitiesMenu : MonoBehaviour
{
    public Text title;
    public Image lightRays1;
    public Image lightRays2;
    public CardDisplay card;
    public SpriteRenderer character;
    public Image ability;
    public Text abilityName;

    private Card thisCard;
    private Sprite thisCharacter;
    private Sprite thisAbility;
    private string thisAbilityName;
    private float rotation1 = 0;
    private float rotation2 = 90;

    // Start is called before the first frame update
    void Awake()
    {
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (gameObject.active)
        {
            rotation1 += 0.3f;
            rotation2 -= 0.1f;
            lightRays1.transform.rotation = Quaternion.Euler(0, 0, rotation1);
            lightRays2.transform.rotation = Quaternion.Euler(0, 0, rotation2);
        }
    }

    public void SetCard(Card c)
    {
        thisCard = c;
    }

    public void SetCharacter(Sprite characterSprite)
    {
        thisCharacter = characterSprite;
    }

    public void SetAbility (Sprite img, string ablName)
    {
        thisAbility = img;
        thisAbilityName = ablName;
    }

    public void StartDisplaying()
    {
        if (thisAbility != null)
        {
            Debug.Log("Displaying new ability");
            title.text = "New Ability!";
            abilityName.text = thisAbilityName;
            abilityName.gameObject.SetActive(true);
            ability.gameObject.SetActive(true);
            card.gameObject.SetActive(false);
            character.gameObject.SetActive(false);
            ability.sprite = thisAbility;
            gameObject.SetActive(true);
            thisAbility = null;
        }
        else if (thisCharacter != null)
        {
            title.text = "New Friend!";
            abilityName.gameObject.SetActive(false);
            ability.gameObject.SetActive(false);
            card.gameObject.SetActive(false);
            character.gameObject.SetActive(true);
            character.sprite = thisCharacter;
            gameObject.SetActive(true);
            thisCharacter = null;
        }
        else if (thisCard != null)
        {
            title.text = "New Card!";
            abilityName.gameObject.SetActive(false);
            ability.gameObject.SetActive(false);
            character.gameObject.SetActive(false);
            card.gameObject.SetActive(true);
            card.SetCard(thisCard, true);
            card.SetHighLight(true);
            gameObject.SetActive(true);
            thisCard = null;
        }
    }

    public void ContinueButtonPressed()
    {
        if (thisCard != null)
            StartDisplaying();
        else
            GameController.gameController.FinishRoomAndExit(RewardsMenuController.RewardType.BypassRewards, 0);
    }
}