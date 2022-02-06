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
    public Text drawPileCount;
    public Text discardPileCount;

    [Header("Manifest Cards")]
    public Text choose1Text;
    public List<ManifestCardController> manifestCards;
    public Image hideButton;
    private Effect manifestEffect;

    public GameObject replace;
    public GameObject hold;

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
}
