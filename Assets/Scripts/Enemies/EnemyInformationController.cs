using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;

public class EnemyInformationController : MonoBehaviour
{
    [SerializeField] private GameObject enemyDisplayCard;
    [SerializeField] private float cardSpacing;
    [SerializeField] private float cardStartingHeight;
    [SerializeField] private float minHeight;
    [SerializeField] private float maxHeight;
    private IEnumerator displayCardCoroutine;

    [SerializeField] private Image intentTypeIndicator;
    private List<Image> currentIntentTypeIndicators;
    [SerializeField] private int[] attackIntentDamageCutoffs;
    [SerializeField] private Sprite[] attackIntent;
    [SerializeField] private Sprite blockIntent;
    [SerializeField] private Sprite buffIntent;
    [SerializeField] private Sprite debuffIntent;
    [SerializeField] private Sprite healIntent;
    [SerializeField] private Sprite knockBackIntent;
    [SerializeField] private Sprite otherIntent;

    private List<Vector2> moveableLocations;
    private List<Vector2> attackableLocations;

    private LineRenderer targetLine;
    public GameObject[] displayedCards;
    private GameObject usedCard;

    private EnemyController enemyController;
    private CharacterAnimationController charAnimController;

    private DateTime clickedTime;

    private bool hasShownAbilities = false;
    private bool isTriggeringCard = false;
    private bool canPathToTarget = false;

    private List<GameObject> stackedObjs = new List<GameObject>();
    private int stackedIndex = 0;

    //private Animator anim;

    // Start is called before the first frame update
    void Awake()
    {
        enemyController = GetComponent<EnemyController>();
        charAnimController = enemyController.GetHealthController().charDisplay.charAnimController;
        targetLine = GetComponent<LineRenderer>();
        moveableLocations = new List<Vector2>();
        attackableLocations = new List<Vector2>();

        Image intentLocation = enemyController.GetHealthController().charDisplay.intentLocation;

        currentIntentTypeIndicators = new List<Image>();
        for (int i = 0; i < enemyController.attacksPerTurn; i++)
        {
            Image im = Instantiate(intentTypeIndicator);

            UIRevealController.UIReveal.ReportElement(UIRevealController.UIElement.Intents, im.gameObject);

            currentIntentTypeIndicators.Add(im);
            currentIntentTypeIndicators[i].transform.SetParent(transform);

            im.transform.SetParent(intentLocation.transform);
            //tx.transform.SetParent(intentLocation.transform);
            if (enemyController.attacksPerTurn % 2 == 0)
            {
                currentIntentTypeIndicators[i].transform.localPosition = new Vector2((i - enemyController.attacksPerTurn / 2 + 0.5f) * 0.35f + 0.3f, 0f);  //Even
            }
            else
            {
                currentIntentTypeIndicators[i].transform.localPosition = new Vector2((i - enemyController.attacksPerTurn / 2) * 0.35f + 0.3f, 0f);  //Odd
            }
        }

        //DrawCards();
    }

    public void CreateRangeIndicators(bool visible = true)
    {
        Color moveRangeColor = enemyController.moveRangeColor;
        Color attackRangeColor = enemyController.attackRangeColor;

        if (!visible)
        {
            moveRangeColor = Color.clear;
            attackRangeColor = Color.clear;
        }

        int bonusMoveRange = enemyController.GetHealthController().GetBonusMoveRange();

        string[] avoidTags = new string[0];
        if (!enemyController.GetHealthController().GetPhasedMovement())
            avoidTags = new string[] { "Player", "Blockade" };
        else
            avoidTags = new string[] { };

        foreach (Vector2 vec in enemyController.GetHealthController().GetOccupiedSpaces())
            TileCreator.tileCreator.CreateTiles(this.gameObject, (Vector2)transform.position + vec, Card.CastShape.Circle, enemyController.GetHealthController().GetMaxMoveRange() + bonusMoveRange, moveRangeColor, avoidTags, 1);

        //If phased movement is enabled, still can't end on an occupied spot
        if (enemyController.GetHealthController().GetPhasedMovement())
        {
            List<Vector2> destroyLocs = new List<Vector2>();
            foreach (Vector2 loc in TileCreator.tileCreator.GetTilePositions(1))
                if (GridController.gridController.GetObjectAtLocation(loc).Count != 0)
                    destroyLocs.Add(loc);
            TileCreator.tileCreator.DestroySpecificTiles(this.gameObject, destroyLocs, 1);
        }

        List<Vector2> movePositions = TileCreator.tileCreator.GetTilePositions(1);

        //Create attackable locations
        if (displayedCards[0].GetComponent<CardController>().GetCard().castType == Card.CastType.AoE)
            foreach (Vector2 position in movePositions)
                TileCreator.tileCreator.CreateTiles(this.gameObject, position, Card.CastShape.Circle, displayedCards[0].GetComponent<CardController>().GetCard().radius, attackRangeColor, new string[] { "" }, 2);
        else if (enemyController.castRange > 0)
            foreach (Vector2 position in movePositions)
                TileCreator.tileCreator.CreateTiles(this.gameObject, position, Card.CastShape.Circle, enemyController.castRange, attackRangeColor, new string[] { "" }, 2);

        moveableLocations = movePositions;
        attackableLocations = TileCreator.tileCreator.GetTilePositions(2);

        if (!visible)
        {
            TileCreator.tileCreator.DestroyTiles(this.gameObject, 1);
            TileCreator.tileCreator.DestroyTiles(this.gameObject, 2);
        }

    }

    public void RefreshIntentImage()
    {
        for (int i = 0; i < enemyController.attacksPerTurn; i++)
        {
            CardController card = displayedCards[i].GetComponent<CardController>();
            switch (card.GetCard().indicatorType)
            {
                case Card.IndicatorType.Attack:
                    int attackValue = card.GetSimulatedTotalAttackValue(i);
                    currentIntentTypeIndicators[i].transform.GetChild(2).GetComponent<Image>().sprite = attackIntent[2];     //Default to the largest attack possible
                    /*
                    for (int j = 0; j < attackIntentDamageCutoffs.Length; j++)
                        if (attackValue <= attackIntentDamageCutoffs[j])
                        {
                            currentIntentTypeIndicators[i].sprite = attackIntent[j];                            //If attack value is between specified range, use appropriate attack image
                            break;
                        }
                    */
                    break;
                case Card.IndicatorType.Guard:
                    currentIntentTypeIndicators[i].transform.GetChild(2).GetComponent<Image>().sprite = blockIntent;
                    break;
                case Card.IndicatorType.Buff:
                    currentIntentTypeIndicators[i].transform.GetChild(2).GetComponent<Image>().sprite = buffIntent;
                    break;
                case Card.IndicatorType.Debuff:
                    currentIntentTypeIndicators[i].transform.GetChild(2).GetComponent<Image>().sprite = debuffIntent;
                    break;
                case Card.IndicatorType.Heal:
                    currentIntentTypeIndicators[i].transform.GetChild(2).GetComponent<Image>().sprite = healIntent;
                    break;
                case Card.IndicatorType.Knockback:
                    currentIntentTypeIndicators[i].transform.GetChild(2).GetComponent<Image>().sprite = knockBackIntent;
                    break;
                default:
                    currentIntentTypeIndicators[i].transform.GetChild(2).GetComponent<Image>().sprite = otherIntent;
                    break;
            }
            int cardDynamicNumber = card.GetDynamicNumberOnCard();
            if (cardDynamicNumber == 0)
            {
                currentIntentTypeIndicators[i].transform.GetChild(3).GetComponent<Text>().text = "";
                currentIntentTypeIndicators[i].transform.GetChild(0).gameObject.SetActive(false);
            }
            else
            {
                currentIntentTypeIndicators[i].transform.GetChild(3).GetComponent<Text>().text = cardDynamicNumber.ToString() + card.GetCard().indicatorMultiplier;
                currentIntentTypeIndicators[i].transform.GetChild(0).transform.localScale = new Vector3(currentIntentTypeIndicators[i].transform.GetChild(3).GetComponent<Text>().text.Length * 0.5f + 0.5f, 1, 1);
                currentIntentTypeIndicators[i].transform.GetChild(0).gameObject.SetActive(true);
            }
        }
    }

    //Refresh intent without having to call with card. Used for healthcontroller forced movement to update target color
    public void RefreshIntent()
    {
        if (enemyController.GetHealthController().GetStunned() || enemyController.GetHealthController().GetVit() < 0)
            return;
        if (enemyController.GetHealthController().GetIsSimulation())
            return;
        if (!UIRevealController.UIReveal.GetElementState(UIRevealController.UIElement.Intents))
            return;
        CreateRangeIndicators(false); //Refresh the attackable and moveable locations
        RefreshIntentColors();
        RefreshIntentImage();
    }

    //Refresh the colors of the intent to reflect the target's color
    private void RefreshIntentColors()
    {
        CreateRangeIndicators(false);                   //Refresh attackrange positions for color to ensure accuracy
        for (int i = 0; i < enemyController.attacksPerTurn; i++)
        {
            Color intentColor = Color.white;
            Vector2 targetLocation = enemyController.desiredTarget[i].transform.position;
            Color targetColor = Color.white;

            try
            {
                targetColor = enemyController.desiredTarget[i].GetComponent<EnemyController>().moveRangeColor;
                intentColor = targetColor * 1.5f; //Brighten by 50% to ensure visibility when put against the stats texts from enemies above
            }
            catch
            {
                targetColor = enemyController.desiredTarget[i].GetComponent<PlayerMoveController>().moveRangeIndicatorColor;
                intentColor = targetColor * 1.5f;
                targetLocation = enemyController.desiredTarget[i].GetComponent<PlayerMoveController>().moveShadow.transform.position;
            }

            canPathToTarget = true;
            //If target not in range, darken
            if (!attackableLocations.Contains(new Vector2(Mathf.Round(targetLocation.x), Mathf.Round(targetLocation.y))) && enemyController.desiredTarget[i] != this.gameObject) //If target is self, always highlight
            {
                canPathToTarget = false;
                if (intentColor == Color.black)
                    intentColor.a = 0.2f;
                else
                    intentColor.a = 0.5f;
            }

            currentIntentTypeIndicators[i].transform.GetChild(4).gameObject.SetActive(!canPathToTarget);
            if (!canPathToTarget)
                TutorialController.tutorial.TriggerTutorial(Dialogue.Condition.EnemyIntentOutOfRange, 1);

            currentIntentTypeIndicators[i].gameObject.SetActive(true);
            currentIntentTypeIndicators[i].transform.GetChild(1).GetComponent<Image>().color = intentColor;
            if ((intentColor.r + intentColor.g + intentColor.b) / 3f > 0.8f && intentColor.r == intentColor.g && intentColor.g == intentColor.b)    //Color correction for when white is being targeted
                currentIntentTypeIndicators[i].transform.GetChild(1).GetComponent<Image>().color = new Color(0.8f, 0.8f, 0.8f, 1);
            currentIntentTypeIndicators[i].transform.GetChild(3).GetComponent<Text>().color = intentColor;
            if ((intentColor.r + intentColor.g + intentColor.b) / 3f < 0.2f && intentColor.r == intentColor.g && intentColor.g == intentColor.b)    //Color correction for when black is being targeted
                currentIntentTypeIndicators[i].transform.GetChild(3).GetComponent<Text>().color = new Color(0.5f, 0.5f, 0.5f, 1);
            if ((targetColor.a + targetColor.b + targetColor.g) / 3.0f < 0.4f) //If the target color is too dark, make the outline white instead
                currentIntentTypeIndicators[i].GetComponent<Outline>().effectColor = new Color(0.85f, 0.85f, 0.85f);
            else
                currentIntentTypeIndicators[i].GetComponent<Outline>().effectColor = Color.black;
        }
    }

    public void ShowIntent()
    {
        if (!UIRevealController.UIReveal.GetElementState(UIRevealController.UIElement.Intents) || enemyController.GetHealthController().GetVit() <= 0)
            return;

        for (int i = 0; i < enemyController.attacksPerTurn; i++)
            currentIntentTypeIndicators[i].gameObject.SetActive(true);
    }

    public void HideIntent()
    {
        for (int i = 0; i < enemyController.attacksPerTurn; i++)
            currentIntentTypeIndicators[i].gameObject.SetActive(false);
    }

    public Image GetIntent()
    {
        return currentIntentTypeIndicators[0];
    }

    //Create attack and move range and Display cards
    private void OnMouseDown()
    {
        if (TutorialController.tutorial.GetEnabled())
            return;

        clickedTime = DateTime.Now;

        displayCardCoroutine = ShowCards();
        if (GridController.gridController.GetObjectAtLocation(transform.position).Count > 1)
        {
            stackedObjs = GridController.gridController.GetObjectAtLocation(transform.position);
            UIController.ui.combatStats.SetStatusCharactersCount(0, stackedObjs.Count);

            Vector2 mousePosition = CameraController.camera.ScreenToWorldPoint(Input.mousePosition);
            int newStackedIndex = (int)Mathf.Floor((mousePosition.x + 0.5f - Mathf.Floor(mousePosition.x + 0.5f)) * stackedObjs.Count);
            stackedObjs[newStackedIndex].GetComponent<HealthController>().SetCombatStatsHighlight(0);

            UIController.ui.combatStats.SetStatusCharactersIndex(0, newStackedIndex + 1);
            try
            {
                stackedObjs[newStackedIndex].GetComponent<EnemyInformationController>().StartCoroutine(stackedObjs[newStackedIndex].GetComponent<EnemyInformationController>().ShowCards());
                stackedObjs[newStackedIndex].GetComponent<EnemyInformationController>().CreateRangeIndicators();
            }
            catch { }
        }
        else
        {
            CreateRangeIndicators();
            enemyController.GetHealthController().SetCombatStatsHighlight(0);

            //foreach (GameObject obj in GridController.gridController.GetObjectAtLocation(transform.position))
            //    obj.GetComponent<HealthController>().ShowHealthBar();     //Re-eabled for character based health bars

            StartCoroutine(displayCardCoroutine);
        }
    }

    private void OnMouseDrag()
    {
        if (stackedObjs.Count < 1)
            return;

        Vector2 mousePosition = CameraController.camera.ScreenToWorldPoint(Input.mousePosition);
        int newStackedIndex = (int)Mathf.Floor((mousePosition.x + 0.5f - Mathf.Floor(mousePosition.x + 0.5f)) * stackedObjs.Count);
        if (newStackedIndex != stackedIndex)
        {
            StopCoroutine(displayCardCoroutine);
            HideCards();
            TileCreator.tileCreator.DestroyTiles(stackedObjs[stackedIndex]);
            try
            {
                stackedObjs[stackedIndex].GetComponent<EnemyInformationController>().StopAllCoroutines();
                stackedObjs[stackedIndex].GetComponent<EnemyInformationController>().HideCards();
            }
            catch { }
            stackedIndex = newStackedIndex;
            stackedObjs[newStackedIndex].GetComponent<HealthController>().SetCombatStatsHighlight(0);
            stackedObjs[newStackedIndex].GetComponent<EnemyInformationController>().CreateRangeIndicators();
            UIController.ui.combatStats.SetStatusCharactersIndex(0, newStackedIndex + 1);
            try
            {
                stackedObjs[newStackedIndex].GetComponent<EnemyInformationController>().StartCoroutine(stackedObjs[newStackedIndex].GetComponent<EnemyInformationController>().ShowCards());
            }
            catch { }
        }
    }

    //Destroy attack and move range
    public void OnMouseUp()
    {
        if (TutorialController.tutorial.GetEnabled())
            return;

        foreach (GameObject obj in GridController.gridController.GetObjectAtLocation(transform.position))
            obj.GetComponent<HealthController>().HideHealthBar();

        TileCreator.tileCreator.DestroyTiles(this.gameObject);
        if (displayCardCoroutine != null)
            StopCoroutine(displayCardCoroutine);
        HideCards();

        /*
        if ((DateTime.Now - clickedTime).TotalSeconds < 0.2)
            SetCharacterInfoDescription();
        */
        if ((DateTime.Now - clickedTime).TotalSeconds < 0.2 && UIRevealController.UIReveal.GetElementState(UIRevealController.UIElement.Intents))
            foreach (GameObject obj in GridController.gridController.GetObjectAtLocation(transform.position, new string[] { "Enemy" }))
                TileCreator.tileCreator.AddSelectedEnemy(obj.GetComponent<EnemyController>());

        enemyController.GetHealthController().charDisplay.healthBar.SetPositionRaised(false);

        UIController.ui.combatStats.SetStatusCharactersCount(0, 1);
        try
        {
            stackedObjs[stackedIndex].GetComponent<EnemyInformationController>().StopAllCoroutines();
            stackedObjs[stackedIndex].GetComponent<EnemyInformationController>().HideCards();
        }
        catch { }
        stackedObjs = new List<GameObject>();
    }

    public void SetCharacterInfoDescription()
    {
        List<CardController> cards = new List<CardController>();
        cards.AddRange(enemyController.GetCard());
        foreach (CardController c in cards)
            c.SetCaster(this.gameObject);
        CharacterInformationController.charInfoController.SetDescription(enemyController.GetHealthController().charDisplay.sprite.sprite, enemyController.GetHealthController(), cards, enemyController.GetHealthController().GetBuffController().GetBuffs(), null, GetComponent<AbilitiesController>());
        CharacterInformationController.charInfoController.Show();
        TutorialController.tutorial.TriggerTutorial(Dialogue.Condition.EnemyTapped, 1);
    }

    public void DrawCards()
    {
        int numCards = GetComponent<EnemyController>().attacksPerTurn;
        //int numCards = enemyController.attackSequence.Count;
        displayedCards = new GameObject[numCards];

        RaycastHit hit;
        Ray ray = new Ray(Camera.main.transform.position, transform.position - Camera.main.transform.position);
        Physics.Raycast(ray, out hit, LayerMask.GetMask("UI"));
        Vector3 canvasPosition = hit.point;

        //Odd number of cards
        if (numCards % 2 == 1)
        {
            for (int i = 0; i < numCards; i++)
            {
                //Place card evenly spaced centered around the middle
                float cardLocationX = HandController.handController.cardHighlightXBoarder * Mathf.Sign(-transform.position.x) + (i - numCards / 2) * cardSpacing;
                //Place the card between an acceptable range to be on the oppopsite vertical side of the caster
                float cardLocationY = Mathf.Clamp(canvasPosition.y + Mathf.Sign(canvasPosition.y - 1) * -1 * cardStartingHeight, minHeight, maxHeight);

                Vector2 cardLocation = new Vector2(cardLocationX, cardLocationY);

                displayedCards[i] = Instantiate(enemyDisplayCard);
                displayedCards[i].transform.SetParent(CanvasController.canvasController.uiCanvas.transform);
                displayedCards[i].GetComponent<RectTransform>().position = cardLocation;
                displayedCards[i].GetComponent<RectTransform>().rotation = CameraController.camera.transform.rotation;
                displayedCards[i].GetComponent<CardController>().SetCaster(this.gameObject);
                displayedCards[i].GetComponent<CardController>().SetCard(enemyController.attackSequence[i]);
            }
        }
        //Even number of cards
        else
        {
            for (int i = 0; i < numCards; i++)
            {
                //Place card evenly spaced centered around the middle
                float cardLocationX = canvasPosition.x + (i - numCards / 2 + 0.5f) * cardSpacing;
                //Place the card between an acceptable range to be on the oppopsite vertical side of the caster
                float cardLocationY = Mathf.Clamp(canvasPosition.y + Mathf.Sign(canvasPosition.y - 1) * -1 * cardStartingHeight, minHeight, maxHeight);

                Vector2 cardLocation = new Vector2(cardLocationX, cardLocationY);

                displayedCards[i] = Instantiate(enemyDisplayCard);
                displayedCards[i].transform.SetParent(CanvasController.canvasController.uiCanvas.transform);
                displayedCards[i].GetComponent<RectTransform>().position = cardLocation;
                displayedCards[i].GetComponent<RectTransform>().rotation = CameraController.camera.transform.rotation;
                displayedCards[i].GetComponent<CardController>().SetCaster(this.gameObject);
                displayedCards[i].GetComponent<CardController>().SetCard(enemyController.attackSequence[i]);
            }
        }
        HideCards();
    }

    public void SetCards()
    {
        List<CardController> cards = enemyController.GetCard();
        for (int i = 0; i < cards.Count; i++)
        {
            displayedCards[i].GetComponent<CardController>().SetCard(cards[i].GetCard());
            displayedCards[i].GetComponent<CardController>().SetCaster(this.gameObject);
            //displayedCards[i].GetComponent<CardDisplay>().SetCard(cards[i]);
            displayedCards[i].GetComponent<CardEffectsController>().SetCard(cards[i]);
        }
    }

    private void HideCards()
    {
        for (int i = 0; i < displayedCards.Length; i++)
        {
            displayedCards[i].transform.GetChild(0).GetComponent<CardDisplay>().Hide();
            displayedCards[i].transform.GetChild(0).GetComponent<CardDisplay>().SetToolTip(false, i, displayedCards.Length);
        }
    }

    private IEnumerator ShowCards()
    {
        yield return new WaitForSeconds(TimeController.time.timeTillCardDisplay * TimeController.time.timerMultiplier);

        int numCards = GetComponent<EnemyController>().attacksPerTurn;
        RaycastHit hit;
        Ray ray = new Ray(Camera.main.transform.position, transform.position - Camera.main.transform.position);
        Physics.Raycast(ray, out hit, LayerMask.GetMask("UI"));
        Vector3 canvasPosition = hit.point;

        for (int i = 0; i < displayedCards.Length; i++)
        {
            //Place card evenly spaced centered around the middle
            float cardLocationX = HandController.handController.cardHighlightXBoarder * Mathf.Sign(-transform.position.x) + i * cardSpacing;
            if (transform.position.x == 0)
                cardLocationX = -HandController.handController.cardHighlightXBoarder + i * cardSpacing;
            //Place the card between an acceptable range to be on the oppopsite vertical side of the caster
            //float cardLocationY = Mathf.Clamp(canvasPosition.y + Mathf.Sign(canvasPosition.y - 1) * -1 * cardStartingHeight, minHeight, maxHeight);
            float cardLocationY = canvasPosition.y;

            Vector2 cardLocation = new Vector2(cardLocationX, cardLocationY);
            displayedCards[i].GetComponent<RectTransform>().position = cardLocation;
            displayedCards[i].GetComponent<RectTransform>().rotation = CameraController.camera.transform.rotation;

            displayedCards[i].transform.GetChild(0).GetComponent<CardDisplay>().Show();
            displayedCards[i].transform.GetChild(0).GetComponent<CardDisplay>().SetToolTip(true, i, displayedCards.Length);
            displayedCards[i].transform.GetChild(0).GetComponent<LineRenderer>().enabled = false;
        }
        TutorialController.tutorial.TriggerTutorial(Dialogue.Condition.EnemyHeld, 1);
    }

    public IEnumerator TriggerCard(int cardIndex, List<Vector2> targets)
    {
        CardController cc = displayedCards[cardIndex].GetComponent<CardController>();

        if (enemyController.GetHealthController().GetTauntedTarget() != null)
            if (!targets.Contains(enemyController.GetHealthController().GetTauntedTarget().transform.position))
                targets.Add(enemyController.GetHealthController().GetTauntedTarget().transform.position);            //Alwasy include the taunted target's location in cast
        charAnimController.TriggerAttack(this.gameObject, targets, cc.GetComponent<CardEffectsController>());

        isTriggeringCard = true;

        while (isTriggeringCard)
            yield return new WaitForSeconds(0.2f);


        yield return new WaitForSeconds(TimeController.time.enemyAttackCardHangTime * TimeController.time.timerMultiplier);
        //yield return StartCoroutine(cc.GetComponent<CardEffectsController>().TriggerEffect(this.gameObject, targets));
    }

    public CardController GetCardController(int cardIndex)
    {
        return displayedCards[cardIndex].GetComponent<CardController>();
    }

    public void CardTriggerFinished()
    {
        isTriggeringCard = false;
    }

    public SimHealthController SimulateTriggerCard(int cardIndex, GameObject target, SimHealthController simH)
    {
        return displayedCards[cardIndex].GetComponent<CardEffectsController>().SimulateTriggerEffect(this.gameObject, target.transform.position, simH);
    }

    public void ShowUsedCard(CardController card, Vector2 target, bool isWhiteText = false)
    {
        RaycastHit hit;
        Ray ray = new Ray(Camera.main.transform.position, transform.position - Camera.main.transform.position);
        Physics.Raycast(ray, out hit, LayerMask.GetMask("UI"));
        Vector3 canvasPosition = hit.point;

        //Place card to the opposite horrizontal side of the target from the caster
        float cardLocationX = canvasPosition.x + (cardSpacing / 2 + 0.5f) * Mathf.Sign(canvasPosition.x - target.x);
        cardLocationX = Mathf.Clamp(cardLocationX, -HandController.handController.cardHighlightXBoarder, HandController.handController.cardHighlightXBoarder);
        //Place the card at the caster height between an acceptable range
        float cardLocationY = Mathf.Clamp(canvasPosition.y, minHeight, maxHeight);
        Vector2 cardLocation = new Vector2(cardLocationX, cardLocationY);
        if (cardLocationX != canvasPosition.x + (cardSpacing / 2 + 0.5f) * Mathf.Sign(canvasPosition.x - target.x))
        {
            cardLocationY = canvasPosition.y + (cardSpacing / 2 + 0.5f) * Mathf.Sign(canvasPosition.y - target.y) * 1.3f;
            cardLocation = new Vector2(canvasPosition.x, cardLocationY);
        }

        usedCard = Instantiate(enemyDisplayCard);
        usedCard.transform.rotation = CameraController.camera.transform.rotation;
        usedCard.transform.SetParent(CanvasController.canvasController.uiCanvas.transform);
        usedCard.transform.position = cardLocation;
        card.SetCaster(this.gameObject);
        usedCard.GetComponent<CardController>().SetCaster(this.gameObject);
        usedCard.GetComponent<CardController>().SetCard(card.GetCard());
        if (isWhiteText)
            usedCard.GetComponent<CardController>().GetCardDisplay().description.color = Color.white;

        if (card.GetCard().castType == Card.CastType.AoE)
        {
            TileCreator.tileCreator.CreateTiles(this.gameObject, GridController.gridController.GetRoundedVector(transform.position, 1), Card.CastShape.Circle, card.GetCard().radius, GetComponent<EnemyController>().attackRangeColor, 0);
        }
        else if (card.GetCard().castType == Card.CastType.TargetedAoE || card.GetCard().castType == Card.CastType.EmptyTargetedAoE)
            TileCreator.tileCreator.CreateTiles(this.gameObject, target, Card.CastShape.Circle, card.GetCard().radius, GetComponent<EnemyController>().attackRangeColor, 0);
        else
            TileCreator.tileCreator.CreateTiles(this.gameObject, target, Card.CastShape.Circle, 0, GetComponent<EnemyController>().attackRangeColor, 0);

        usedCard.GetComponentInChildren<CardDisplay>().FadeIn(0.1f * TimeController.time.timerMultiplier, PartyController.party.GetPlayerColor(Card.CasterColor.Enemy));
    }

    public void GreyOutUsedCard()
    {
        usedCard.GetComponent<CardDisplay>().SetHighLight(false);
    }

    public void DestroyUsedCard()
    {
        HideTargetLine();
        if (usedCard != null)
        {
            usedCard.GetComponentInChildren<CardDisplay>().FadeOut(0.5f * TimeController.time.timerMultiplier, PartyController.party.GetPlayerColor(Card.CasterColor.Enemy));
            Destroy(usedCard, 0.5f);
        }
    }

    public void ShowTargetLine(Vector2 target)
    {
        targetLine.SetPosition(0, transform.position);
        targetLine.SetPosition(1, target);
        targetLine.enabled = true;
    }

    private void HideTargetLine()
    {
        targetLine.enabled = false;
    }

    //Gets a list of all gameobjects that this enemy can attack this turn
    public List<GameObject> GetAttackableTargets(string[] tags)
    {
        return GridController.gridController.GetObjectAtLocation(GetAttackableLocations(), tags);
    }

    public List<Vector2> GetAttackableLocations()
    {
        if (enemyController.GetHealthController().GetVit() <= 0 || enemyController.GetHealthController().GetStunned())
            return new List<Vector2>();

        /*
        //Gets the moverange of this enemy ignoring all collisions
        int bonusMoveRange = enemyController.GetHealthController().GetBonusMoveRange();
        foreach (Vector2 vec in enemyController.GetHealthController().GetOccupiedSpaces())
            TileCreator.tileCreator.CreateTiles(this.gameObject, (Vector2)transform.position + vec, Card.CastShape.Circle, enemyController.GetHealthController().GetMaxMoveRange() + bonusMoveRange, Color.clear, new string[] { "Player", "Enemy", "Blockade" }, 2);
        List<Vector2> movePositions = TileCreator.tileCreator.GetTilePositions(2);
        TileCreator.tileCreator.DestroyTiles(this.gameObject, 2);

        //Get the locations of all points that this enemy can move to assuming no enemy in the way
        // ~ probably can remove by taking out Enemy in the avoid tags in line 333 above
        foreach (Vector2 vec in movePositions)
        {
            int counter = 0;
            List<Vector2> path = PathFindController.pathFinder.PathFind(transform.position, vec, new string[] { "Enemy" }, enemyController.GetHealthController().GetOccupiedSpaces(), enemyController.GetHealthController().size);
            foreach (Vector2 loc in path)
            {
                if (counter > enemyController.GetHealthController().GetMaxMoveRange() + bonusMoveRange)
                    break;
                List<Vector2> v = new List<Vector2>();
                foreach (Vector2 space in enemyController.GetHealthController().GetOccupiedSpaces())
                    v.Add(loc + space);

                List<GameObject> colissions = GridController.gridController.GetObjectAtLocation(v, new string[] { "Player", "Blockade" });
                if (colissions.Count == 0)
                    foreach (Vector2 space in enemyController.GetHealthController().GetOccupiedSpaces())
                        TileCreator.tileCreator.CreateTiles(this.gameObject, loc + space, Card.CastShape.Circle, 0, Color.clear, new string[] { "None" }, 2);
                counter += 1;
            }
        }

        movePositions = TileCreator.tileCreator.GetTilePositions(2);
        TileCreator.tileCreator.DestroyTiles(this.gameObject, 2);

        //Get the locations of all attack points from all moveable locations
        if (displayedCards[0].GetComponent<CardController>().GetCard().castType == Card.CastType.AoE)
            foreach (Vector2 position in movePositions)
                TileCreator.tileCreator.CreateTiles(this.gameObject, position, Card.CastShape.Circle, displayedCards[0].GetComponent<CardController>().GetCard().radius, Color.clear, new string[] { "" }, 2);
        else if (enemyController.castRange > 0)
            foreach (Vector2 position in movePositions)
                TileCreator.tileCreator.CreateTiles(this.gameObject, position, Card.CastShape.Circle, enemyController.castRange, Color.yellow, new string[] { "" }, 2);

        List<Vector2> attackablePositions = TileCreator.tileCreator.GetTilePositions(2);
        TileCreator.tileCreator.DestroyTiles(this.gameObject, 2);
        */

        return attackableLocations;
    }

    public List<Vector2> GetMoveablePositions()
    {
        return moveableLocations;
    }

    public IEnumerator ShowAbilities()
    {
        if (!hasShownAbilities)
        {
            List<Card> abilityCards = GetComponent<AbilitiesController>().GetAbilityCards();
            for (int i = 0; i < abilityCards.Count; i++)
            {
                CardController temp = this.gameObject.AddComponent<CardController>();
                temp.SetCardDisplay(displayedCards[0].transform.GetChild(0).GetComponent<CardDisplay>());
                temp.SetCard(abilityCards[i], false, true);
                ShowUsedCard(temp, transform.position, true);
                enemyController.GetHealthController().charDisplay.hitEffectAnim.SetTrigger("Glow");
                yield return new WaitForSeconds(TimeController.time.enemyAttackCardHangTime * TimeController.time.timerMultiplier);

                DestroyUsedCard();
                TileCreator.tileCreator.DestroyTiles(this.gameObject);

                SetCards();

                yield return new WaitForSeconds(TimeController.time.enemyExecutionStagger * TimeController.time.timerMultiplier);

            }
        }
        hasShownAbilities = true;
    }

    public bool GetHasShownAbilities()
    {
        return hasShownAbilities;
    }

    public bool GetCanPathToTarget()
    {
        return canPathToTarget;
    }
}
