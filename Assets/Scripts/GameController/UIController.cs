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

    public List<Image> manaIcons;
    private int manaCount = 0;

    [Header("Deck UI Settings")]
    public Text drawPileCount;
    public Text discardPileCount;

    [Header("Manifest Cards")]
    public List<ManifestCardController> manifestCards;
    private Effect manifestEffect;

    // Start is called before the first frame update
    void Awake()
    {
        if (UIController.ui == null)
            UIController.ui = this;
        else
            Destroy(this.gameObject);

        foreach (Image icon in manaIcons)
            icon.GetComponent<Outline>().effectColor = anticipatedLooseOutlineColor;

        for (int i = 0; i < manifestCards.Count; i++)
        {
            manifestCards[i].GetComponent<Collider2D>().enabled = false;
            manifestCards[i].transform.GetChild(0).GetComponent<CardDisplay>().Hide();
        }
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
                    manaIcons[i].color = obtainedManaColor;
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

        /*
for (int i = 0; i < 10; i++)
    if (i < manaCount)
        manaIcons[i].GetComponent<Outline>().enabled = false;
    else if (i < manaCount + amount)
    {
        manaIcons[i].GetComponent<Outline>().effectColor = anticipatedGainColor;
        manaIcons[i].GetComponent<Outline>().enabled = true;
    }
    else
        manaIcons[i].GetComponent<Outline>().enabled = false;
       */
    }

    public IEnumerator GainMana(int loc)
    {
        manaIcons[loc].color = anticipatedLooseIconColor;
        yield return new WaitForSeconds(TimeController.time.manaGainFlickerPeriod);
        manaIcons[loc].color = obtainedManaColor;
    }

    public void SetAnticipatedManaLoss(int amount)
    {
        for (int i = 0; i < 10; i++)
            if (i >= manaCount - amount && i < Mathf.Max(manaCount, amount))
                if (i < manaCount)
                    manaIcons[i].color = anticipatedLooseIconColor;
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
        manifestEffect = effect;
        for (int i = 0; i < cards.Count; i++)
        {
            manifestCards[i].SetCard(cards[i]);
            manifestCards[i].GetComponent<Collider2D>().enabled = true;
            manifestCards[i].transform.GetChild(0).GetComponent<CardDisplay>().Show();
            manifestCards[i].transform.GetChild(0).GetComponent<LineRenderer>().enabled = false;
        }
    }

    public void ReportChosenManifestCard(CardController card)
    {
        manifestEffect.chosenCard = card;

        for (int i = 0; i < manifestCards.Count; i++)
        {
            manifestCards[i].GetComponent<Collider2D>().enabled = false;
            manifestCards[i].transform.GetChild(0).GetComponent<CardDisplay>().Hide();
        }
    }
}
