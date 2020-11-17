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

    //[SerializeField] private Image targetColorIndicator;
    [SerializeField] private Image intentTypeIndicator;
    [SerializeField] private Text intentMultiplier;
    private List<Image> currentIntentTypeIndicators;
    private List<Text> currentIntentMultipliers;
    //[SerializeField] private Text intentValueIndicator;
    [SerializeField] private int[] attackIntentDamageCutoffs;
    [SerializeField] private Sprite[] attackIntent;
    [SerializeField] private Sprite blockIntent;
    [SerializeField] private Sprite buffIntent;
    [SerializeField] private Sprite debuffIntent;
    [SerializeField] private Sprite otherIntent;

    private List<Vector2> moveableLocations;
    private List<Vector2> attackableLocations;

    private LineRenderer targetLine;
    public GameObject[] displayedCards;
    private GameObject usedCard;

    private EnemyController enemyController;
    private CharacterAnimationController charAnimController;

    private DateTime clickedTime;

    //private Animator anim;

    // Start is called before the first frame update
    void Awake()
    {
        //anim = GetComponent<HealthController>().charDisplay.sprite.GetComponent<Animator>();
        charAnimController = GetComponent<HealthController>().charDisplay.charAnimController;
        enemyController = GetComponent<EnemyController>();
        targetLine = GetComponent<LineRenderer>();
        moveableLocations = new List<Vector2>();
        attackableLocations = new List<Vector2>();

        currentIntentTypeIndicators = new List<Image>();
        currentIntentMultipliers = new List<Text>();
        for (int i = 0; i < enemyController.attacksPerTurn; i++)
        {
            Image im = Instantiate(intentTypeIndicator);
            Text tx = Instantiate(intentMultiplier);

            currentIntentTypeIndicators.Add(im);
            currentIntentTypeIndicators[i].transform.SetParent(transform);
            currentIntentMultipliers.Add(tx);
            currentIntentMultipliers[i].transform.SetParent(transform);
            currentIntentMultipliers[i].enabled = true;

            if (enemyController.attacksPerTurn % 2 == 0)
            {
                currentIntentTypeIndicators[i].transform.position = (Vector2)transform.position + new Vector2((i - enemyController.attacksPerTurn / 2 + 0.5f) * 0.35f, 0.5f);  //Even
                currentIntentMultipliers[i].transform.position = (Vector2)transform.position + new Vector2((i - enemyController.attacksPerTurn / 2 + 0.5f) * 0.35f, 0.5f) + new Vector2(0.3f, -0.1f);
            }
            else
            {
                currentIntentTypeIndicators[i].transform.position = (Vector2)transform.position + new Vector2((i - enemyController.attacksPerTurn / 2) * 0.35f, 0.5f);  //Odd
                currentIntentMultipliers[i].transform.position = (Vector2)transform.position + new Vector2((i - enemyController.attacksPerTurn / 2) * 0.35f, 0.5f) + new Vector2(0.3f, -0.1f);
            }
        }

        //DrawCards();
    }

    //Create attack and move range and Display cards
    private void OnMouseDown()
    {
        clickedTime = DateTime.Now;

        CreateRangeIndicators();

        foreach (GameObject obj in GridController.gridController.GetObjectAtLocation(transform.position))
            obj.GetComponent<HealthController>().ShowHealthBar();

        displayCardCoroutine = ShowCards();
        StartCoroutine(displayCardCoroutine);
    }

    private void CreateRangeIndicators(bool visible = true)
    {
        Color moveRangeColor = enemyController.moveRangeColor;
        Color attackRangeColor = enemyController.attackRangeColor;

        if (!visible)
        {
            moveRangeColor = Color.clear;
            attackRangeColor = Color.clear;
        }

        int bonusMoveRange = GetComponent<HealthController>().GetBonusMoveRange();

        foreach (Vector2 vec in GetComponent<HealthController>().GetOccupiedSpaces())
            TileCreator.tileCreator.CreateTiles(this.gameObject, (Vector2)transform.position + vec, Card.CastShape.Circle, enemyController.moveRange + bonusMoveRange, moveRangeColor, new string[] { "Player", "Blockade" }, 1);

        List<Vector2> movePositions = TileCreator.tileCreator.GetTilePositions(1);
        //TileCreator.tileCreator.DestroyTiles(this.gameObject, 1);

        /*
        foreach (Vector2 vec in movePositions)
        {
            int counter = 0;
            List<Vector2> path = PathFindController.pathFinder.PathFind(transform.position, vec, new string[] { "Enemy" }, GetComponent<HealthController>().GetOccupiedSpaces(), GetComponent<HealthController>().size);
            foreach (Vector2 loc in path)
            {
                if (counter > enemyController.moveRange + bonusMoveRange)
                    break;
                List<Vector2> v = new List<Vector2>();
                foreach (Vector2 space in GetComponent<HealthController>().GetOccupiedSpaces())
                    v.Add(loc + space);

                List<GameObject> colissions = GridController.gridController.GetObjectAtLocation(v, new string[] { "Player", "Blockade" });
                if (colissions.Count == 0)
                    foreach (Vector2 space in GetComponent<HealthController>().GetOccupiedSpaces())
                        TileCreator.tileCreator.CreateTiles(this.gameObject, loc + space, Card.CastShape.Circle, 0, moveRangeColor, new string[] { "None" }, 1);
                counter += 1;
            }
        }

        movePositions = TileCreator.tileCreator.GetTilePositions(1);
        TileCreator.tileCreator.RefreshTiles(1);
                */

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
    /*

    //Show intent and set the card
    public void ShowIntent()
    {
        CreateRangeIndicators(false); //Refresh the attackable and moveable locations
        RefreshIntentColors();
        RefreshIntentImage();
    }
    */

    public void RefreshIntentImage()
    {
        for (int i = 0; i < enemyController.attacksPerTurn; i++)
        {
            CardController card = displayedCards[i].GetComponent<CardController>();
            switch (card.GetCard().indicatorType)
            {
                case Card.IndicatorType.Attack:
                    int attackValue = card.GetSimulatedTotalAttackValue(i);
                    currentIntentTypeIndicators[i].sprite = attackIntent[attackIntentDamageCutoffs.Length - 1];     //Default to the largest attack possible
                    for (int j = 0; j < attackIntentDamageCutoffs.Length; j++)
                        if (attackValue <= attackIntentDamageCutoffs[j])
                        {
                            currentIntentTypeIndicators[i].sprite = attackIntent[j];                            //If attack value is between specified range, use appropriate attack image
                            break;
                        }
                    break;
                case Card.IndicatorType.Guard:
                    currentIntentTypeIndicators[i].sprite = blockIntent;
                    break;
                case Card.IndicatorType.Buff:
                    currentIntentTypeIndicators[i].sprite = buffIntent;
                    break;
                case Card.IndicatorType.Debuff:
                    currentIntentTypeIndicators[i].sprite = debuffIntent;
                    break;
                default:
                    currentIntentTypeIndicators[i].sprite = otherIntent;
                    break;
            }
            currentIntentMultipliers[i].text = card.GetCard().indicatorMultiplier;
        }
    }

    //Refresh intent without having to call with card. Used for healthcontroller forced movement to update target color
    public void RefreshIntent()
    {
        if (GetComponent<HealthController>().GetStunned())
            return;
        CreateRangeIndicators(false); //Refresh the attackable and moveable locations
        RefreshIntentColors();
        RefreshIntentImage();
    }

    //Refresh the colors of the intent to reflect the target's color
    private void RefreshIntentColors()
    {
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

            if (!attackableLocations.Contains(new Vector2(Mathf.Round(targetLocation.x), Mathf.Round(targetLocation.y))) && enemyController.desiredTarget[i] != this.gameObject) //If target is self, always highlight
            {
                if (intentColor == Color.black)
                    intentColor.a = 0.2f;
                else
                    intentColor.a = 0.5f;
            }

            currentIntentTypeIndicators[i].color = intentColor;
            if ((targetColor.a + targetColor.b + targetColor.g) / 3.0f < 0.4f) //If the target color is too dark, make the outline white instead
                currentIntentTypeIndicators[i].GetComponent<Outline>().effectColor = new Color(0.85f, 0.85f, 0.85f);
            else
                currentIntentTypeIndicators[i].GetComponent<Outline>().effectColor = Color.black;
        }
    }

    public void HideIntent()
    {
        for (int i = 0; i < enemyController.attacksPerTurn; i++)
        {
            currentIntentTypeIndicators[i].color = new Color(0, 0, 0, 0);
            currentIntentMultipliers[i].text = "";
        }

    }

    //Destroy attack and move range
    public void OnMouseUp()
    {
        foreach (GameObject obj in GridController.gridController.GetObjectAtLocation(transform.position))
            obj.GetComponent<HealthController>().HideHealthBar();

        TileCreator.tileCreator.DestroyTiles(this.gameObject);
        if (displayCardCoroutine != null)
            StopCoroutine(displayCardCoroutine);
        HideCards();

        if ((DateTime.Now - clickedTime).TotalSeconds < 0.2)
        {
            HealthController hlth = GetComponent<HealthController>();
            List<CardController> cards = new List<CardController>();
            cards.AddRange(enemyController.GetCard());
            foreach (CardController c in cards)
                c.SetCaster(this.gameObject);
            CharacterInformationController.charInfoController.SetDescription(GetComponent<HealthController>().charDisplay.sprite.sprite, hlth, cards, hlth.GetBuffController().GetBuffs(), GetComponent<AbilitiesController>());
            CharacterInformationController.charInfoController.Show();
        }
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
                float cardLocationX = canvasPosition.x + (i - numCards / 2) * cardSpacing;
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
                displayedCards[i].transform.rotation = CameraController.camera.transform.rotation;
                displayedCards[i].transform.SetParent(CanvasController.canvasController.uiCanvas.transform);
                displayedCards[i].transform.position = CameraController.camera.ScreenToWorldPoint(cardLocation, CanvasController.canvasController.uiCanvas.transform.rotation * Vector3.forward, CanvasController.canvasController.uiCanvas.transform.position);
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
        foreach (GameObject card in displayedCards)
            card.GetComponent<CardDisplay>().Hide();
    }

    private IEnumerator ShowCards()
    {
        yield return new WaitForSeconds(TimeController.time.timeTillCardDisplay * TimeController.time.timerMultiplier);
        for (int i = 0; i < displayedCards.Length; i++)
        {
            displayedCards[i].GetComponent<CardDisplay>().Show();
            displayedCards[i].GetComponent<LineRenderer>().enabled = false;
        }
    }

    public IEnumerator TriggerCard(int cardIndex, List<Vector2> targets)
    {
        CardController cc = displayedCards[cardIndex].GetComponent<CardController>();

        /*
        if (enemyController.GetComponent<HealthController>().GetTauntedTarget() != null)         //If this enemy is taunted, cast all self cards as if they were any types
        {
            Card card = cc.GetCard().GetCopy();
            for (int i = 0; i < card.targetType.Length; i++)
                if (card.targetType[i] == Card.TargetType.Self)
                    card.targetType[i] = Card.TargetType.Any;
            cc.SetCard(card, true, false);
        }

        */
        if (enemyController.GetComponent<HealthController>().GetTauntedTarget() != null)
            if (!targets.Contains(enemyController.GetComponent<HealthController>().GetTauntedTarget().transform.position))
                targets.Add(enemyController.GetComponent<HealthController>().GetTauntedTarget().transform.position);            //Alwasy include the taunted target's location in cast
        charAnimController.TriggerAttack(this.gameObject, targets, cc.GetComponent<CardEffectsController>());

        yield return new WaitForSeconds(TimeController.time.enemyAttackCardHangTime*TimeController.time.timerMultiplier);
        //yield return StartCoroutine(cc.GetComponent<CardEffectsController>().TriggerEffect(this.gameObject, targets));
    }

    public SimHealthController SimulateTriggerCard(int cardIndex, GameObject target, SimHealthController simH)
    {
        return displayedCards[cardIndex].GetComponent<CardEffectsController>().SimulateTriggerEffect(this.gameObject, target.transform.position, simH);
    }

    public void ShowUsedCard(CardController card, Vector2 target)
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

        if (card.GetCard().castType == Card.CastType.AoE)
        {
            TileCreator.tileCreator.CreateTiles(this.gameObject, GridController.gridController.GetRoundedVector(transform.position, 1), Card.CastShape.Circle, card.GetCard().radius, GetComponent<EnemyController>().attackRangeColor, 0);
        }
    }

    public void GreyOutUsedCard()
    {
        usedCard.GetComponent<CardDisplay>().SetHighLight(false);
    }

    public void DestroyUsedCard()
    {
        HideTargetLine();
        TileCreator.tileCreator.DestroyTiles(this.gameObject, 0);
        Destroy(usedCard);
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
        //Gets the moverange of this enemy ignoring all collisions
        int bonusMoveRange = GetComponent<HealthController>().GetBonusMoveRange();
        foreach (Vector2 vec in GetComponent<HealthController>().GetOccupiedSpaces())
            TileCreator.tileCreator.CreateTiles(this.gameObject, (Vector2)transform.position + vec, Card.CastShape.Circle, enemyController.moveRange + bonusMoveRange, Color.clear, new string[] { "Player", "Enemy", "Blockade" }, 2);
        List<Vector2> movePositions = TileCreator.tileCreator.GetTilePositions(2);
        TileCreator.tileCreator.DestroyTiles(this.gameObject, 2);

        //Get the locations of all points that this enemy can move to assuming no enemy in the way
        // ~ probably can remove by taking out Enemy in the avoid tags in line 333 above
        foreach (Vector2 vec in movePositions)
        {
            int counter = 0;
            List<Vector2> path = PathFindController.pathFinder.PathFind(transform.position, vec, new string[] { "Enemy" }, GetComponent<HealthController>().GetOccupiedSpaces(), GetComponent<HealthController>().size);
            foreach (Vector2 loc in path)
            {
                if (counter > enemyController.moveRange + bonusMoveRange)
                    break;
                List<Vector2> v = new List<Vector2>();
                foreach (Vector2 space in GetComponent<HealthController>().GetOccupiedSpaces())
                    v.Add(loc + space);

                List<GameObject> colissions = GridController.gridController.GetObjectAtLocation(v, new string[] { "Player", "Blockade" });
                if (colissions.Count == 0)
                    foreach (Vector2 space in GetComponent<HealthController>().GetOccupiedSpaces())
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

        return GridController.gridController.GetObjectAtLocation(attackablePositions, tags);
    }
}
