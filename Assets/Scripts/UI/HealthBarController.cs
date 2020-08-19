using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarController : MonoBehaviour
{
    [Header("Color Settings")]
    public Color damageColor;
    //public Color damageBonusHealthColor;
    public Color healingColor;
    //public Color healingBonusHealthColor;
    public Color brokenColor;
    public Color shieldDefaultColor;
    public Color shieldDownColor;
    public Color shieldUpColor;

    [Header("Health Bar")]
    [SerializeField]
    Image barImage;
    Image backImage;
    [SerializeField]
    Image bonusHealthBar;
    [SerializeField]
    Image damageBarImage;
    //[SerializeField]
    //Image bonusHealthDamageBar;

    [Header("Damage Image")]
    public Text statusText;
    public Image damageImage;
    public Image damageImage2;
    public Text damageText;

    [Header("Shield Damage Image")]
    public Image shieldDamageImage;
    public Image shieldDamageImage2;
    public Text shieldDamageText;

    [Header("Damage Sprites")]
    public Sprite normalDamageSprite;
    public Sprite brokenDamageSprite;
    public Sprite shieldDamageSprite;

    private Vector2 damageImagePosition;
    private Vector2 shieldDamageImagePosition;
    private Vector2 originalDamageImageScale;
    private Vector2 originalDamagetextScale;
    private Vector2 originalShieldDamageImageScale;
    private Vector2 originalShieldDamagetextScale;
    private int oldDamageInt = 0;
    private int oldShieldDamgeInt = 0;
    private int oldShieldAmount = 0;
    private bool hidingDamageImage = false;
    private bool hidingShieldDamageImage = false;

    //private bool runningCoroutine = false;

    // Start is called before the first frame update
    void Start()
    {
        damageImagePosition = damageImage.transform.position - transform.position;
        shieldDamageImagePosition = shieldDamageImage.transform.position - transform.position;

        backImage = GetComponent<Image>();
        backImage.enabled = false;
        damageBarImage.enabled = false;
        barImage.enabled = false;
        bonusHealthBar.enabled = false;

        originalDamageImageScale = damageImage.transform.localScale;
        originalDamagetextScale = damageText.transform.localScale;

        originalShieldDamageImageScale = shieldDamageImage.transform.localScale;
        originalShieldDamagetextScale = shieldDamageText.transform.localScale;
        //bonusHealthDamageBar.enabled = false;
    }

    public void SetBar(int initialHealth, int damage, int maxHealth, Vector2 center, int size, float scale, int index, bool broken, bool permanent = false)
    {
        StopCoroutine(HideHealthBar());

        int maxSize = Mathf.Max(new int[] { maxHealth, initialHealth - damage, initialHealth }); //The max between either damage, overheal, or the initial overhealed amount
        int bonusHealth = Mathf.Max(0, initialHealth - damage - maxHealth);

        float HPPercentage = Mathf.Clamp((float)initialHealth - damage, 0.0f, maxHealth) / (float)maxSize;
        float bonusHealthPercentage = (float)bonusHealth / (float)maxSize;
        float damagePercentage = Mathf.Clamp((float)damage, initialHealth - maxSize, initialHealth) / (float)maxSize;
        //float bonusHealthDamagePercentage = Mathf.Min(damage, bonusHealth) / (float) maxSize;

        if (initialHealth <= 0)
            damagePercentage = 0;

        backImage.rectTransform.position = center + new Vector2(0, 0.4f + index * 0.2f) * size;
        backImage.rectTransform.localScale = new Vector2(scale, 1);

        barImage.rectTransform.localScale = new Vector2(HPPercentage, 1);

        bonusHealthBar.rectTransform.localScale = new Vector2(bonusHealthPercentage, 1);
        bonusHealthBar.rectTransform.position = barImage.rectTransform.position + new Vector3(0.02f + (HPPercentage) * 0.86f, 0, 0) * size;

        damageBarImage.rectTransform.localScale = new Vector2(damagePercentage, 1);
        damageBarImage.rectTransform.position = barImage.rectTransform.position + new Vector3(0.02f + (HPPercentage + bonusHealthPercentage) * 0.86f, 0, 0) * size;

        //bonusHealthDamageBar.rectTransform.localScale = new Vector2(bonusHealthDamagePercentage, 1);
        //bonusHealthDamageBar.rectTransform.position = barImage.rectTransform.position + new Vector3(0.02f + (HPPercentage + bonusHealthPercentage) * 0.86f, 0, 0) * size;

        //backImage.color = Color.black;
        if (broken)
            damageBarImage.color = brokenColor;
        else
        {
            damageBarImage.color = damageColor;
            //bonusHealthDamageBar.color = damageBonusHealthColor;
        }
        if (damage < 0)
        {
            damageBarImage.color = healingColor;
            //bonusHealthDamageBar.color = healingBonusHealthColor;
        }

        backImage.enabled = true;
        damageBarImage.enabled = true;
        bonusHealthBar.enabled = true;
        barImage.enabled = true;
        //bonusHealthDamageBar.enabled = true;
        if (!permanent)
            StartCoroutine(HideHealthBar());
    }

    //Called by other scripts to hide healthbars when letting go
    public void RemoveHealthBar()
    {
        backImage.enabled = false;
        damageBarImage.enabled = false;
        barImage.enabled = false;
        bonusHealthBar.enabled = false;
        //bonusHealthDamageBar.enabled = false;
    }

    private IEnumerator HideHealthBar()
    {
        yield return new WaitForSeconds(TimeController.time.barShownDuration);
        backImage.enabled = false;
        damageBarImage.enabled = false;
        barImage.enabled = false;
        bonusHealthBar.enabled = false;
        //bonusHealthDamageBar.enabled = false;
    }

    public void SetDamageImage(int initialHealth, int damage, int maxHealth, Vector2 center, int size, float scale, int index, bool broken)
    {
        damageImage.transform.localScale = originalDamageImageScale;
        damageImage2.transform.localScale = originalDamageImageScale;
        damageText.transform.localScale = originalDamagetextScale;
        if (hidingDamageImage)
        {
            StopCoroutine(HideDamageNumber());
            damage += oldDamageInt;
        }

        oldDamageInt = damage;
        if (backImage.enabled)
        {
            StartCoroutine(ResetDamageImage());
            //return;
        }

        if (damage < 0)                                                 //Healing
        {
            damageImage.color = healingColor;
            damageText.color = healingColor;
            damageImage.sprite = normalDamageSprite;
            damageImage2.sprite = normalDamageSprite;
        }
        else
        {
            if (broken)                                //Broken Damage
            {
                Color c = brokenColor;
                damageImage.color = c;
                damageText.color = c;
                damageImage.sprite = shieldDamageSprite;
                damageImage2.sprite = brokenDamageSprite;
            }
            else                                                        //Not Broken Damage
            {
                damageImage.color = damageColor;
                damageText.color = damageColor;
                damageImage.sprite = normalDamageSprite;
                damageImage2.sprite = normalDamageSprite;
            }
        }

        Vector2 newPosition = (Vector2)transform.position + damageImagePosition;
        damageImage.transform.position = newPosition;
        damageImage2.transform.position = newPosition;

        damageImage.enabled = true;
        damageImage2.enabled = true;
        damageText.enabled = true;
        damageText.text = Mathf.Abs(damage).ToString();

        StartCoroutine(HideDamageNumber());
    }

    private IEnumerator ResetDamageImage()
    {
        damageImage.transform.localScale = originalDamageImageScale * 1.25f;
        damageImage2.transform.localScale = originalDamageImageScale * 1.25f;
        damageText.transform.localScale = originalDamagetextScale * 1.25f;
        yield return new WaitForSeconds(TimeController.time.numberExpandDuration);
        damageImage.transform.localScale = originalDamageImageScale;
        damageImage2.transform.localScale = originalDamageImageScale;
        damageText.transform.localScale = originalDamagetextScale;
    }

    public IEnumerator HideDamageNumber()
    {
        hidingDamageImage = true;
        yield return new WaitForSeconds(TimeController.time.numberShownDuration);
        damageImage.enabled = false;
        damageImage2.enabled = false;
        damageText.enabled = false;
        hidingDamageImage = false;
    }

    public void SetShieldDamageImage(int shieldValue, int shieldDamage, bool broken)
    {
        if (hidingShieldDamageImage)
        {
            shieldDamageImage.transform.localScale = originalShieldDamageImageScale;
            shieldDamageImage2.transform.localScale = originalShieldDamageImageScale;
            shieldDamageText.transform.localScale = originalShieldDamagetextScale;

            shieldDamage += oldShieldDamgeInt;
            shieldValue = oldShieldAmount;

            StopCoroutine(HideShieldDamageNumber());
            StopCoroutine(ResetShieldDamageImage(shieldValue, shieldDamage));
        }

        oldShieldDamgeInt = shieldDamage;
        oldShieldAmount = shieldValue;

        Vector2 newPosition = (Vector2)transform.position + shieldDamageImagePosition;
        shieldDamageImage.transform.position = newPosition;
        shieldDamageImage2.transform.position = newPosition;

        shieldDamageImage.enabled = true;
        shieldDamageImage2.enabled = true;
        shieldDamageText.enabled = true;

        StartCoroutine(ResetShieldDamageImage(shieldValue, shieldDamage));

        StartCoroutine(HideShieldDamageNumber());
    }

    private IEnumerator ResetShieldDamageImage(int shieldValue, int shieldDamage)
    {
        hidingShieldDamageImage = true;

        shieldDamageText.color = shieldDefaultColor;
        shieldDamageImage.color = shieldDefaultColor;
        shieldDamageText.text = shieldValue.ToString();
        yield return new WaitForSeconds(TimeController.time.numberShownDuration / 3.0f);

        shieldDamageText.text = Mathf.Max(shieldValue - shieldDamage, 0).ToString();
        shieldDamageImage.transform.localScale = originalShieldDamageImageScale * 1.25f;
        shieldDamageImage2.transform.localScale = originalShieldDamageImageScale * 1.25f;
        shieldDamageText.transform.localScale = originalShieldDamagetextScale * 1.25f;
        if (shieldValue - shieldDamage <= 0)
        {
            shieldDamageImage.color = brokenColor;
            shieldDamageText.color = brokenColor;
        }
        else if (shieldDamage > 0)
        {
            shieldDamageText.color = shieldDownColor;
            shieldDamageImage.color = shieldDownColor;
        }
        else if (shieldDamage < 0)
        {
            shieldDamageText.color = shieldUpColor;
            shieldDamageImage.color = shieldUpColor;
        }
        yield return new WaitForSeconds(TimeController.time.numberExpandDuration);

        shieldDamageImage.transform.localScale = originalShieldDamageImageScale;
        shieldDamageImage2.transform.localScale = originalShieldDamageImageScale;
        shieldDamageText.transform.localScale = originalShieldDamagetextScale;
    }

    public IEnumerator HideShieldDamageNumber()
    {
        yield return new WaitForSeconds(TimeController.time.numberShownDuration);
        shieldDamageImage.enabled = false;
        shieldDamageImage2.enabled = false;
        shieldDamageText.enabled = false;

        hidingShieldDamageImage = false;
    }

    public void SetStatusText(string text, Color color)
    {
        StopCoroutine(HideStatustText());

        statusText.text = text;
        statusText.color = color;
        statusText.enabled = true;

        StartCoroutine(HideStatustText());
    }

    public IEnumerator HideStatustText()
    {
        yield return new WaitForSeconds(TimeController.time.numberShownDuration);
        statusText.enabled = false;
    }
}
