using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewAbilitiesMenu : MonoBehaviour
{
    public GameObject abilitiesMenu;

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

    public void SetAbility(Sprite img, string ablName)
    {
        thisAbility = img;
        thisAbilityName = ablName;
    }

    public void StartDisplaying()
    {
        GameController.gameController.rewardCanvas.gameObject.SetActive(true);
        abilitiesMenu.SetActive(true);

        if (thisAbility != null)
        {
            title.text = "New Ability!";
            abilityName.text = thisAbilityName;
            abilityName.gameObject.SetActive(true);
            ability.gameObject.SetActive(true);
            card.gameObject.SetActive(false);
            character.gameObject.SetActive(false);
            ability.sprite = thisAbility;
            gameObject.SetActive(true);
            StartCoroutine(ZoomIn());
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
            StartCoroutine(ZoomIn());
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
            StartCoroutine(ZoomIn());
            thisCard = null;
        }
    }

    private IEnumerator ZoomIn()
    {
        for (int i = 0; i < 5; i++)
        {
            transform.localScale = Vector3.Lerp(new Vector3(0, 0, 1), new Vector3(1, 1, 1), i / 4);
            yield return new WaitForSecondsRealtime(0.3f / 5f);
        }
    }

    public void ContinueButtonPressed()
    {
        if (thisCard != null)
            StartDisplaying();
        else
        {
            GameController.gameController.FinishRoomAndExit(RewardsMenuController.RewardType.BypassRewards, 0);
            abilitiesMenu.SetActive(false);
            GameController.gameController.rewardCanvas.gameObject.SetActive(false);
        }
    }
}
