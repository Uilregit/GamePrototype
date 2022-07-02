using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public static UIController ui;

    [Header("Mana Color Settings")]
    public Color obtainedManaColor;
    public Color gainedManaFlickerColor;
    public Color missingManaColor;
    public Color anticipatedGainColor;
    public Color anticipatedLooseIconColor;
    public Color anticipatedLooseOutlineColor;

    public Image energyIcon;
    public List<Image> manaIcons;
    private int manaCount = 0;

    [Header("Deck UI Settings")]
    public List<Image> drawPileCards;
    public List<Image> discardPileCards;
    public List<Image> cardShuffleAnimationCards;
    public Text drawPileCount;
    public Text discardPileCount;
    private float drawPileHeight = 0;
    private float discardPileHeight = 0;

    public CardController flipCard;

    [Header("Manifest Cards")]
    public Text choose1Text;
    public List<ManifestCardController> manifestCards;
    public Image hideButton;
    private Effect manifestEffect;

    public GameObject replace;
    public GameObject hold;

    public CombatStatsHighlightController combatStats;

    // Start is called before the first frame update
    void Awake()
    {
        if (UIController.ui == null)
            UIController.ui = this;
        else
            Destroy(this.gameObject);

        energyIcon.material = new Material(energyIcon.material);
        energyIcon.material.SetFloat("_Intensity", 0f);
        foreach (Image icon in manaIcons)
        {
            icon.GetComponent<Outline>().effectColor = anticipatedLooseOutlineColor;
            icon.material = new Material(icon.material);
            icon.material.SetFloat("_Intensity", 0f);
        }

        for (int i = 0; i < manifestCards.Count; i++)
        {
            manifestCards[i].GetComponent<Collider2D>().enabled = false;
            manifestCards[i].transform.GetChild(0).GetComponent<CardDisplay>().Hide();
        }

        HandController.handController.SetHoldAndReplace(hold, replace);
    }

    public void ResetPileCounts(int drawPile, int discardPile)
    {
        drawPileCount.text = drawPile.ToString();
        discardPileCount.text = discardPile.ToString();
        for (int i = 0; i < drawPileCards.Count; i++)
            drawPileCards[i].enabled = drawPile / 5f > i;
        for (int i = 0; i < discardPileCards.Count; i++)
            discardPileCards[i].enabled = discardPile / 5f > i;

        drawPileHeight = 0.07f * (Mathf.Ceil(drawPile / 5f) - 1);
        discardPileHeight = 0.07f * (Mathf.Ceil(discardPile / 5f) - 1);
    }

    public void ResetManaBar(int amount)
    {
        for (int i = 0; i < 10; i++)
        {
            if (i < amount)
            {
                if (i > manaCount - 1)
                    StartCoroutine(GainMana(i));
                else
                {
                    manaIcons[i].color = obtainedManaColor;
                    StartCoroutine(FadeManaGlow(i, 0f));
                }
            }
            else
                manaIcons[i].color = missingManaColor;
            manaIcons[i].GetComponent<Outline>().enabled = false;
        }
        manaCount = amount;
    }

    public void SetAnticipatedManaGain(int amount)
    {
        for (int i = 0; i < 10; i++)
            if (i < manaCount)
                manaIcons[i].color = obtainedManaColor;
            else if (i < manaCount + amount)
                manaIcons[i].color = anticipatedGainColor;
            else
                manaIcons[i].color = missingManaColor;
    }

    public IEnumerator GainMana(int loc)
    {
        manaIcons[loc].color = anticipatedLooseIconColor;
        StartCoroutine(FadeManaGlow(loc, 1f));
        yield return new WaitForSeconds(TimeController.time.manaGainFlickerPeriod);
        manaIcons[loc].color = obtainedManaColor;
        StartCoroutine(FadeManaGlow(loc, 0f));
    }

    public void SetAnticipatedManaLoss(int amount)
    {
        for (int i = 0; i < 10; i++)
            if (i >= manaCount - amount && i < Mathf.Max(manaCount, amount))
                if (i < manaCount)
                {
                    manaIcons[i].color = anticipatedLooseIconColor;
                    StartCoroutine(FadeManaGlow(i, 0.5f));
                }
                else
                {
                    manaIcons[i].GetComponent<Outline>().effectColor = anticipatedLooseOutlineColor;
                    manaIcons[i].GetComponent<Outline>().enabled = true;
                }
            else
                manaIcons[i].GetComponent<Outline>().enabled = false;
    }

    public void SetManifestCards(List<CardController> cards, Effect effect)
    {
        choose1Text.enabled = true;
        hideButton.enabled = true;
        hideButton.transform.GetChild(0).GetComponent<Text>().enabled = true;

        manifestEffect = effect;
        for (int i = 0; i < cards.Count; i++)
        {
            manifestCards[i].SetCard(cards[i]);
            manifestCards[i].GetComponent<Collider2D>().enabled = true;
            manifestCards[i].transform.GetChild(0).GetComponent<CardDisplay>().Show();
            manifestCards[i].transform.GetChild(0).GetComponent<LineRenderer>().enabled = false;
        }
    }

    public void ManifestHideButtonPressed()
    {
        choose1Text.enabled = !choose1Text.enabled;
        foreach (ManifestCardController card in manifestCards)
        {
            if (card.GetHasCard())
                if (card.GetComponent<Collider2D>().enabled)
                {
                    card.GetComponent<Collider2D>().enabled = false;
                    card.transform.GetChild(0).GetComponent<CardDisplay>().Hide();
                }
                else
                {
                    card.GetComponent<Collider2D>().enabled = true;
                    card.transform.GetChild(0).GetComponent<CardDisplay>().Show();
                    card.transform.GetChild(0).GetComponent<LineRenderer>().enabled = false;
                }
        }
    }

    public void ReportChosenManifestCard(CardController card)
    {
        choose1Text.enabled = false;
        hideButton.enabled = false;
        hideButton.transform.GetChild(0).GetComponent<Text>().enabled = false;

        manifestEffect.chosenCard = card;
        for (int i = 0; i < manifestCards.Count; i++)
        {
            manifestCards[i].GetComponent<Collider2D>().enabled = false;
            manifestCards[i].transform.GetChild(0).GetComponent<CardDisplay>().Hide();
        }
    }

    public void SetEnergyGlow(bool state)
    {
        if (state)
            StartCoroutine(FadeEnergyGlow(1.0f));
        else
            StartCoroutine(FadeEnergyGlow(0f));
    }

    private IEnumerator FadeEnergyGlow(float value)
    {
        float startingVal = energyIcon.material.GetFloat("_Intensity");
        for (int i = 0; i < 10; i++)
        {
            energyIcon.material.SetFloat("_Intensity", Mathf.Lerp(startingVal, value, i / 9.0f));
            yield return new WaitForSeconds(0.1f / 10);
        }
    }

    private IEnumerator FadeManaGlow(int icon, float value)
    {
        float startingVal = manaIcons[icon].material.GetFloat("_Intensity");
        for (int i = 0; i < 10; i++)
        {
            manaIcons[icon].material.SetFloat("_Intensity", Mathf.Lerp(startingVal, value, i / 9.0f));
            yield return new WaitForSeconds(0.1f / 10);
        }
    }

    public IEnumerator AnimateDiscardCardProcess(Card card, Vector3 position, Vector3 scale)
    {
        flipCard.SetCard(card, false, true, true);
        if (!flipCard.cardDisplay.GetIsFacingUp())
            flipCard.cardDisplay.PlaceFaceUp();
        flipCard.transform.localScale = scale;
        flipCard.transform.position = position;

        Vector3 originalLocalPosition = flipCard.transform.localPosition;

        flipCard.cardDisplay.FlipDown(0.08f);
        yield return new WaitForSeconds(0.2f);
        for (int i = 0; i < 10; i++)
        {
            flipCard.transform.localPosition = Vector3.Lerp(originalLocalPosition, new Vector3(3.5f, -4.65f, 0f) + new Vector3(0, discardPileHeight, 0), i / 9f);
            flipCard.transform.localScale = Vector3.Lerp(scale, new Vector3(0.35f, 0.35f, 1), i / 9f);
            yield return new WaitForSeconds(0.1f / 10f);
        }
        ResetPileCounts(DeckController.deckController.GetDrawPileSize(), DeckController.deckController.GetDiscardPileSize() + 1);

        flipCard.transform.position = new Vector3(100, 0, 0);
    }

    public IEnumerator AnimateShuffleCardProcess()
    {
        foreach (Image card in cardShuffleAnimationCards)
            card.enabled = true;
        foreach (Image card in drawPileCards)
            card.enabled = false;
        foreach (Image card in discardPileCards)
            card.enabled = false;

        Vector3 cardStartingPosition = cardShuffleAnimationCards[0].transform.localPosition;
        //Move all discard pile cards into the draw pile
        Vector3 cardOriginalPosition = cardShuffleAnimationCards[0].transform.localPosition;
        int maxCardsInPile = Mathf.Max(discardPileCards.Count, drawPileCards.Count);
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < cardShuffleAnimationCards.Count; j++)
                cardShuffleAnimationCards[j].transform.localPosition = Vector3.Lerp(cardOriginalPosition + new Vector3(0, 0.07f * j, 0), new Vector3(-7f, cardOriginalPosition.y, cardOriginalPosition.z), (i - j) / 6f);
            ResetPileCounts((int)Mathf.Lerp(0, maxCardsInPile, i / 9f), (int)Mathf.Lerp(maxCardsInPile, 0, i / 9f));
            yield return new WaitForSeconds(0.3f / 9f);
        }

        //Animating shuffling
        //Animate the first card shuffling right
        cardOriginalPosition = cardShuffleAnimationCards[2].transform.localPosition;
        Quaternion cardOriginalRotation = cardShuffleAnimationCards[2].transform.rotation;
        for (int i = 0; i < 5; i++)
        {
            cardShuffleAnimationCards[2].transform.localPosition = Vector3.Lerp(cardOriginalPosition, cardOriginalPosition + new Vector3(1.5f, 0, 0), i / 4f);
            cardShuffleAnimationCards[2].transform.rotation = Quaternion.Lerp(cardOriginalRotation, cardOriginalRotation * Quaternion.Euler(0, 0, -20f), i / 4f);
            yield return new WaitForSeconds(0.05f / 5f);
        }
        cardShuffleAnimationCards[2].transform.SetAsLastSibling();
        for (int i = 0; i < 5; i++)
        {
            cardShuffleAnimationCards[2].transform.localPosition = Vector3.Lerp(cardOriginalPosition + new Vector3(1.5f, 0, 0), cardOriginalPosition, i / 4f);
            cardShuffleAnimationCards[2].transform.rotation = Quaternion.Lerp(cardOriginalRotation * Quaternion.Euler(0, 0, -20f), cardOriginalRotation, i / 4f);
            yield return new WaitForSeconds(0.05f / 5f);
        }
        //Animate the second card shuffling left
        cardOriginalPosition = cardShuffleAnimationCards[3].transform.localPosition;
        cardOriginalRotation = cardShuffleAnimationCards[3].transform.rotation;
        for (int i = 0; i < 5; i++)
        {
            cardShuffleAnimationCards[3].transform.localPosition = Vector3.Lerp(cardOriginalPosition, cardOriginalPosition - new Vector3(1.5f, 0, 0), i / 4f);
            cardShuffleAnimationCards[3].transform.rotation = Quaternion.Lerp(cardOriginalRotation, cardOriginalRotation * Quaternion.Euler(0, 0, 20f), i / 4f);
            yield return new WaitForSeconds(0.05f / 5f);
        }
        cardShuffleAnimationCards[3].transform.SetAsLastSibling();
        for (int i = 0; i < 5; i++)
        {
            cardShuffleAnimationCards[3].transform.localPosition = Vector3.Lerp(cardOriginalPosition - new Vector3(1.5f, 0, 0), cardOriginalPosition, i / 4f);
            cardShuffleAnimationCards[3].transform.rotation = Quaternion.Lerp(cardOriginalRotation * Quaternion.Euler(0, 0, 20f), cardOriginalRotation, i / 4f);
            yield return new WaitForSeconds(0.05f / 5f);
        }

        //Resetting the animation cards
        for (int j = 0; j < cardShuffleAnimationCards.Count; j++)
        {
            cardShuffleAnimationCards[j].enabled = false;
            cardShuffleAnimationCards[j].transform.localPosition = cardStartingPosition + new Vector3(0, 0.07f * j, 0);
        }
    }
}
