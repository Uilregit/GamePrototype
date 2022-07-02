using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarController : MonoBehaviour
{
    [Header("Color Settings")]
    public Color damageColor;
    public Color damageImageColor;
    //public Color damageBonusHealthColor;
    public Color healingColor;
    //public Color healingBonusHealthColor;
    public Color brokenColor;
    //public Color damageOverTimeColor;
    public Color armorDefaultColor;
    public Color armorDownColor;
    public Color armorUpColor;
    public Color armorBrokenColor;

    [Header("Health Bar")]
    [SerializeField]
    GameObject healthBarObject;
    [SerializeField]
    Image barImage;
    [SerializeField]
    Image backImage;
    [SerializeField]
    Image bonusHealthBar;
    [SerializeField]
    Image damageBarImage;
    [SerializeField]
    Image damageOverTimeBarImage;
    [SerializeField]
    GameObject healthBarTick;
    [SerializeField]
    GameObject healthBarTickContainer;
    [SerializeField]
    Text healthNumber;
    [SerializeField]
    Image armorIcon;
    [SerializeField]
    Text armorNumber;
    [SerializeField]
    Image attackIcon;
    [SerializeField]
    Text attackNumber;
    [SerializeField]
    Text breakText;
    [SerializeField]
    Text healthBarStatusText;
    //[SerializeField]
    //Image bonusHealthDamageBar;
    [SerializeField]
    Image skullIcon;
    [SerializeField]
    Sprite armorSprite;
    [SerializeField]
    Sprite armorBrokenSprite;

    [Header("Damage FXs")]
    public Animator bloodSplatter;

    [Header("Damage Image")]
    public Text statusText;
    public Image damageImage;
    public Image damageImage2;
    public Text damageText;

    [Header("Armor Damage Image")]
    public Image armorDamageImage;
    public Image armorDamageImage2;
    public Text armorDamageText;
    public Animator armorBreakAnim;

    [Header("Armor Damage Image")]
    public Image attackDamageImage;
    public Image attackDamageImage2;
    public Text attackDamageText;

    [Header("Damage Sprites")]
    public Sprite normalDamageSprite;
    public Sprite brokenDamageSprite;
    public Sprite armorDamageSprite;

    [Header("Sprite")]
    public SpriteRenderer character;

    private List<Image> healthBarTicks = new List<Image>();

    private Vector2 damageImagePosition;
    private Vector2 armorDamageImagePosition;
    private Vector2 attackImagePosition;
    private Vector2 originalDamageImageScale;
    private Vector2 originalDamagetextScale;
    private Vector2 originalArmorDamageImageScale;
    private Vector2 originalArmorDamagetextScale;
    private Vector2 originalAttackImageScale;
    private Vector2 originalAttackTextScale;
    private int oldDamageInt = 0;
    private int oldArmorDamgeInt = 0;
    private int oldArmorAmount = 0;
    private int oldAttackChangeInt = 0;
    private int oldAttackAmount = 0;
    private bool hidingDamageImage = false;
    private bool hidingArmorDamageImage = false;
    private bool hidingAttackImage = false;

    private Vector3 startingLocalPosition;

    private IEnumerator healthBarHide;
    private IEnumerator healthImageHide;
    private IEnumerator statusTextHide;
    private IEnumerator armorDamageNumberHide;
    private IEnumerator resetArmorImageHide;
    private IEnumerator attackNumberHide;
    private IEnumerator resetAttackImageHide;
    //private bool runningCoroutine = false;

    private StatusTypes[] statusPriority = new StatusTypes[] { StatusTypes.Stacked, StatusTypes.Stunned, StatusTypes.Silenced, StatusTypes.Disarmed, StatusTypes.Taunted };
    private Dictionary<StatusTypes, bool> currentStatusTypes = new Dictionary<StatusTypes, bool>();

    private bool raised = false;
    private bool positionSet = false;

    public enum StatusTypes
    {
        Stunned = 0,
        Silenced = 1,
        Disarmed = 2,
        Taunted = 3,
        Stacked = 4,
    }

    // Start is called before the first frame update
    void Awake()
    {
        damageImagePosition = damageImage.transform.localPosition;
        armorDamageImagePosition = armorDamageImage.transform.localPosition;
        attackImagePosition = attackDamageImage.transform.localPosition;

        originalDamageImageScale = damageImage.transform.localScale;
        originalDamagetextScale = damageText.transform.localScale;

        originalArmorDamageImageScale = armorDamageImage.transform.localScale;
        originalArmorDamagetextScale = armorDamageText.transform.localScale;

        originalAttackImageScale = attackDamageImage.transform.localScale;
        originalAttackTextScale = attackDamageText.transform.localScale;

        startingLocalPosition = healthBarObject.transform.localPosition;

        UIRevealController.UIReveal.ReportElement(UIRevealController.UIElement.Armor, armorIcon.gameObject);
        UIRevealController.UIReveal.ReportElement(UIRevealController.UIElement.Armor, armorNumber.gameObject);
        //bonusHealthDamageBar.enabled = false;
    }

    public void SetHealthBarTicks(int maxHealth)
    {
        foreach (Image img in healthBarTicks)
            Destroy(img.gameObject);
        healthBarTicks = new List<Image>();

        int counter = 1;
        while (counter * 10 < maxHealth)
        {
            GameObject temp = Instantiate(healthBarTick);
            temp.transform.SetParent(healthBarTickContainer.transform);
            temp.transform.localPosition = new Vector3(counter * 10 / (float)maxHealth * 0.65f - 0.179f, -0.103f, 0f);
            if (counter % 5 == 0)
            {
                temp.transform.localScale = new Vector3(2f, 1f, 1f);
                temp.GetComponent<Image>().color = Color.black;
            }
            healthBarTicks.Add(temp.GetComponent<Image>());
            counter++;
        }
    }

    public void SetBar(int initialHealth, int damage, int endOfTurnDamage, int maxHealth, Vector2 center, int size, float scale, int index, bool broken, bool permanent = false, bool simulated = false)
    {
        if (healthBarHide != null)
            StopCoroutine(healthBarHide);

        SetHealthBarTicks(Mathf.Max(new int[] { maxHealth, initialHealth - damage, initialHealth - damage - endOfTurnDamage }));    //Set health bar ticks to either the max health, or in the case of healing (negative deamage), overhealed max health

        damage = Mathf.Min(damage, initialHealth);          //Always ensure that damage bar doesn't become bigger than the remaining health bar, even in overkill

        int maxSize = Mathf.Max(new int[] { maxHealth, initialHealth - damage, initialHealth - damage - endOfTurnDamage, initialHealth }); //The max between either damage, overheal, or the initial overhealed amount
        int bonusHealth = Mathf.Max(0, maxSize - maxHealth);

        float HPPercentage = Mathf.Clamp((float)initialHealth - damage, 0.0f, maxHealth) / (float)maxSize;  //Clamped between 0 and max health. Parts over max health is for bonus health
        float bonusHealthPercentage = (float)bonusHealth / (float)maxSize;
        float damagePercentage = 0;
        if (damage >= 0)
            //damagePercentage = Mathf.Clamp((float)damage, initialHealth - maxSize, initialHealth) / (float)maxSize;
            damagePercentage = Mathf.Min((float)damage, (float)maxSize) / (float)maxSize;
        else
        {
            damagePercentage = Mathf.Max((float)damage, (float)initialHealth - maxHealth) / (float)maxSize;    //Healing never goes over max health, that portion is bonushealth's area
            if (initialHealth < 0)
                damagePercentage = -(float)Mathf.Max(0f, initialHealth - damage) / (float)maxSize;
        }

        //float damageOverTimePercentage = Mathf.Clamp((float)endOfTurnDamage, initialHealth - maxSize, initialHealth) / (float)maxSize;
        float damageOverTimePercentage = Mathf.Min((float)endOfTurnDamage, initialHealth - damage) / (float)maxSize;
        //float bonusHealthDamagePercentage = Mathf.Min(damage, bonusHealth) / (float) maxSize;

        /*
        Debug.Log(maxHealth + "|" + maxSize);
        Debug.Log(initialHealth + "|" + damage + "|" + endOfTurnDamage + "|" + bonusHealth);
        Debug.Log(HPPercentage + "|" + damagePercentage + "|" + damageOverTimePercentage + "|" + bonusHealthPercentage);
        */

        if (initialHealth <= 0 && initialHealth - damage - endOfTurnDamage <= 0)
            damagePercentage = 0;

        //backImage.rectTransform.position = center + new Vector2(0, 1.15f + index * 0.25f) * size;
        backImage.rectTransform.localScale = new Vector2(scale, 1);

        barImage.rectTransform.localScale = new Vector2(HPPercentage, 1);

        skullIcon.enabled = initialHealth - damage - endOfTurnDamage <= 0;

        if (initialHealth - damage - endOfTurnDamage > 0 || initialHealth > 0)  //If overkill, don't show any other bar
        {
            bonusHealthBar.rectTransform.localScale = new Vector2(bonusHealthPercentage, 1);
            bonusHealthBar.rectTransform.position = barImage.rectTransform.position + new Vector3((HPPercentage) * bonusHealthBar.rectTransform.sizeDelta.x, 0, 0) * size;

            damageBarImage.rectTransform.localScale = new Vector2(damagePercentage, 1);
            if (damage > 0)
                damageBarImage.rectTransform.position = barImage.rectTransform.position + new Vector3((HPPercentage + bonusHealthPercentage) * bonusHealthBar.rectTransform.sizeDelta.x, 0, 0) * size;
            else
                damageBarImage.rectTransform.position = barImage.rectTransform.position + new Vector3(HPPercentage * bonusHealthBar.rectTransform.sizeDelta.x, 0, 0) * size;

            damageOverTimeBarImage.rectTransform.localScale = new Vector3(damageOverTimePercentage, 1);
            if (endOfTurnDamage > 0)
                damageOverTimeBarImage.rectTransform.position = barImage.rectTransform.position + new Vector3((HPPercentage + bonusHealthPercentage) * bonusHealthBar.rectTransform.sizeDelta.x, 0, 0) * size;
            else
                damageOverTimeBarImage.rectTransform.position = barImage.rectTransform.position + new Vector3(HPPercentage * bonusHealthBar.rectTransform.sizeDelta.x, 0, 0) * size;

            //bonusHealthDamageBar.rectTransform.localScale = new Vector2(bonusHealthDamagePercentage, 1);
            //bonusHealthDamageBar.rectTransform.position = barImage.rectTransform.position + new Vector3(0.02f + (HPPercentage + bonusHealthPercentage) * bonusHealthBar.rectTransform.sizeDelta.x, 0, 0) * size;

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
            if (endOfTurnDamage > 0)
                damageOverTimeBarImage.color = damageColor * new Color(0.5f, 0.5f, 0.5f);
            else
                damageOverTimeBarImage.color = healingColor * new Color(0.5f, 0.5f, 0.5f);

            damageBarImage.enabled = true;
            damageOverTimeBarImage.enabled = true;
            bonusHealthBar.enabled = true;
        }
        backImage.enabled = true;
        barImage.enabled = true;
        healthBarTickContainer.SetActive(true);
        //bonusHealthDamageBar.enabled = true;
        if (!permanent)
        {
            healthBarHide = HideHealthBar();
            StartCoroutine(healthBarHide);
        }
        healthBarObject.SetActive(true);
    }

    public void SetCharacter(Sprite sprite, Vector2 center)
    {
        //character.transform.position = center + new Vector2(0, 1);
        character.sprite = sprite;
        character.enabled = true;
    }

    //Called by other scripts to hide healthbars when letting go
    public void RemoveHealthBar()
    {
        healthBarObject.SetActive(false);
        character.enabled = false;
        /*
        backImage.enabled = false;
        damageBarImage.enabled = false;
        damageOverTimeBarImage.enabled = false;
        barImage.enabled = false;
        bonusHealthBar.enabled = false;
        healthBarTickContainer.SetActive(false);
        skullIcon.enabled = false;
        */
        //bonusHealthDamageBar.enabled = false;
    }

    //Used to initialize bar when characer is created
    public void ShowHealthBar()
    {
        healthBarObject.SetActive(true);

        damageBarImage.transform.localScale = new Vector3(0, 1, 1);
        damageOverTimeBarImage.transform.localScale = new Vector3(0, 1, 1);
        bonusHealthBar.transform.localScale = new Vector3(0, 1, 1);
        /*
        backImage.enabled = true;
        barImage.enabled = true;
        healthBarTickContainer.SetActive(true);
        healthNumber.enabled = true;
        armorNumber.enabled = true;
        */
    }

    private IEnumerator HideHealthBar()
    {
        yield return new WaitForSeconds(TimeController.time.barShownDuration);
        damageBarImage.transform.localScale = new Vector3(0, 1, 1);
        skullIcon.enabled = false;
        //RemoveHealthBar();
    }

    public void SetDamageImage(int initialHealth, int damage, int maxHealth, Vector2 center, int size, float scale, int index, bool broken)
    {
        damageImage.transform.localScale = originalDamageImageScale;
        damageImage2.transform.localScale = originalDamageImageScale;
        damageText.transform.localScale = originalDamagetextScale;
        if (hidingDamageImage)
        {
            StopCoroutine(healthImageHide);
            hidingDamageImage = false;
            if (Mathf.Sign(damage) == Mathf.Sign(oldDamageInt))
                damage += oldDamageInt;
        }

        oldDamageInt = damage;
        if (backImage.enabled)
        {
            if (damage > 0)
                if (broken)
                    bloodSplatter.SetTrigger("bigSplatter");
                else
                    bloodSplatter.SetTrigger("mediumSplatter");
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
                Color c = armorBrokenColor;
                damageImage.color = c;
                damageText.color = c;
                damageImage.sprite = armorDamageSprite;
                damageImage2.sprite = brokenDamageSprite;
            }
            else                                                        //Not Broken Damage
            {
                damageImage.color = damageImageColor;
                damageText.color = damageImageColor;
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

        healthImageHide = HideDamageNumber();
        StartCoroutine(healthImageHide);
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

    public void SetArmorDamageImage(int armorValue, int armorDamage, bool broken)
    {
        if (hidingArmorDamageImage)
        {
            armorDamageImage.transform.localScale = originalArmorDamageImageScale;
            armorDamageImage2.transform.localScale = originalArmorDamageImageScale;
            armorDamageText.transform.localScale = originalArmorDamagetextScale;

            if (Mathf.Sign(armorDamage) == Mathf.Sign(oldArmorDamgeInt))
                armorDamage += oldArmorDamgeInt;
            armorValue = oldArmorAmount;

            StopCoroutine(armorDamageNumberHide);
            //StopCoroutine(resetArmorImageHide);
        }

        oldArmorDamgeInt = armorDamage;
        oldArmorAmount = armorValue;

        armorDamageImage.transform.localPosition = armorDamageImagePosition;
        armorDamageImage2.transform.localPosition = armorDamageImagePosition;
        armorDamageText.transform.localPosition = armorDamageImagePosition;

        armorDamageImage.enabled = true;
        armorDamageImage2.enabled = true;
        armorDamageText.enabled = true;

        armorDamageNumberHide = HideArmorDamageNumber();
        resetArmorImageHide = ResetArmorDamageImage(armorValue, armorDamage);
        StartCoroutine(resetArmorImageHide);
        StartCoroutine(armorDamageNumberHide);
    }

    private IEnumerator ResetArmorDamageImage(int armorValue, int armorDamage)
    {
        hidingArmorDamageImage = true;

        armorDamageText.color = armorDefaultColor;
        armorDamageImage.color = armorDefaultColor;
        armorDamageText.text = armorValue.ToString();
        yield return new WaitForSeconds(TimeController.time.numberShownDuration / 3.0f);

        armorDamageText.text = Mathf.Max(armorValue - armorDamage, 0).ToString();
        armorDamageImage.transform.localScale = originalArmorDamageImageScale * 1.25f;
        armorDamageImage2.transform.localScale = originalArmorDamageImageScale * 1.25f;
        armorDamageText.transform.localScale = originalArmorDamagetextScale * 1.25f;
        if (armorValue - armorDamage <= 0)
        {
            armorDamageImage.color = brokenColor;
            armorDamageText.color = brokenColor;
            if (armorValue > 0)
                armorBreakAnim.SetTrigger("PlayAnim");
        }
        else if (armorDamage > 0)
        {
            armorDamageText.color = armorDownColor;
            armorDamageImage.color = armorDownColor;
        }
        else if (armorDamage < 0)
        {
            armorDamageText.color = armorUpColor;
            armorDamageImage.color = armorUpColor;
        }
        yield return new WaitForSeconds(TimeController.time.numberExpandDuration);

        armorDamageImage.transform.localScale = originalArmorDamageImageScale;
        armorDamageImage2.transform.localScale = originalArmorDamageImageScale;
        armorDamageText.transform.localScale = originalArmorDamagetextScale;
    }

    private IEnumerator HideArmorDamageNumber()
    {
        yield return new WaitForSeconds(TimeController.time.numberShownDuration);
        armorDamageImage.enabled = false;
        armorDamageImage2.enabled = false;
        armorDamageText.enabled = false;

        hidingArmorDamageImage = false;
    }

    public void SetBonusAttack(int atk)
    {
        attackIcon.enabled = atk != 0;
        attackNumber.enabled = atk != 0;
        attackNumber.text = atk.ToString("+#;-#;0");
    }

    public void SetAttackChangeImage(int attackValue, int attackChange)
    {
        if (hidingAttackImage)
        {
            attackDamageImage.transform.localScale = originalAttackImageScale;
            attackDamageImage2.transform.localScale = originalAttackImageScale;
            attackDamageText.transform.localScale = originalAttackTextScale;

            if (Mathf.Sign(attackChange) == Mathf.Sign(oldAttackChangeInt))
                attackChange += oldAttackChangeInt;
            attackValue = oldAttackAmount;

            StopCoroutine(attackNumberHide);
        }

        oldAttackChangeInt = attackChange;
        oldAttackAmount = attackValue;

        attackDamageImage.transform.localPosition = attackImagePosition;
        attackDamageImage2.transform.localPosition = attackImagePosition;
        attackDamageText.transform.localPosition = attackImagePosition;

        attackDamageImage.enabled = true;
        attackDamageImage2.enabled = true;
        attackDamageText.enabled = true;

        attackNumberHide = HideAttackDamageNumber();
        resetAttackImageHide = ResetAttackImage(attackValue, attackChange);
        StartCoroutine(attackNumberHide);
        StartCoroutine(resetAttackImageHide);
    }

    private IEnumerator ResetAttackImage(int attackValue, int attackChange)
    {
        hidingAttackImage = true;

        attackDamageText.text = attackValue.ToString("+#;-#;0");
        yield return new WaitForSeconds(TimeController.time.numberShownDuration / 3.0f);

        attackDamageText.text = Mathf.Max(attackValue - attackChange, 0).ToString("+#;-#;0");
        attackDamageImage.transform.localScale = originalAttackImageScale * 1.25f;
        attackDamageImage2.transform.localScale = originalAttackImageScale * 1.25f;
        attackDamageText.transform.localScale = originalAttackTextScale * 1.25f;
        yield return new WaitForSeconds(TimeController.time.numberExpandDuration);

        attackDamageImage.transform.localScale = originalAttackImageScale;
        attackDamageImage2.transform.localScale = originalAttackImageScale;
        attackDamageText.transform.localScale = originalAttackTextScale;
    }

    private IEnumerator HideAttackDamageNumber()
    {
        yield return new WaitForSeconds(TimeController.time.numberShownDuration);
        attackDamageImage.enabled = false;
        attackDamageImage2.enabled = false;
        attackDamageText.enabled = false;

        hidingAttackImage = false;
    }

    public void SetStatusText(string text, Color color)
    {
        if (statusTextHide != null)
            StopCoroutine(statusTextHide);

        statusText.text = text;
        statusText.color = color;
        statusText.enabled = true;

        statusTextHide = HideStatustText();
        StartCoroutine(statusTextHide);
    }

    public IEnumerator HideStatustText()
    {
        yield return new WaitForSeconds(TimeController.time.numberShownDuration);
        statusText.enabled = false;
    }

    public void FadeInEffect(float alpha, float time)
    {
        SpriteRenderer img = transform.parent.parent.GetComponent<HealthController>().charDisplay.passiveEffectAnim.GetComponent<SpriteRenderer>();
        StartCoroutine(FadeInEffect(img, alpha, img.color.a, time));
    }

    private IEnumerator FadeInEffect(SpriteRenderer img, float startingAlpha, float endingAlpha, float time)
    {
        for (int i = 0; i < 50; i++)
        {
            img.color = Color.Lerp(new Color(img.color.r, img.color.g, img.color.b, startingAlpha), new Color(img.color.r, img.color.g, img.color.b, endingAlpha), i / 49);
            yield return new WaitForSeconds(time / 50f);
        }
    }

    public void SetPositionRaised(bool state)
    {
        raised = state;

        if (state)
            healthBarObject.transform.localPosition = startingLocalPosition + new Vector3(0, 1.15f, 0);
        else
            healthBarObject.transform.localPosition = startingLocalPosition;
    }

    public bool GetPositionRaised()
    {
        return raised;
    }

    public void SetPosition(Vector2 location)
    {
        healthBarObject.transform.localPosition = location;
        positionSet = true;
    }

    public void ResetPosition()
    {
        if (positionSet)
            SetPositionRaised(false);
        positionSet = false;
    }

    public void SetHealth(int value)
    {
        if (!UIRevealController.UIReveal.GetElementState(UIRevealController.UIElement.Overkill))
            value = Mathf.Max(0, value);
        healthNumber.text = value.ToString();
    }

    public void SetArmor(int value)
    {
        armorNumber.text = value.ToString();
        armorNumber.enabled = value > 0;
        armorIcon.enabled = value >= 0;
        breakText.enabled = value == 0;

        if (value > 0)
        {
            armorIcon.color = armorDefaultColor;
            armorIcon.sprite = armorSprite;
        }
        else if (value == 0)
        {
            armorIcon.color = armorBrokenColor;
            armorIcon.sprite = armorBrokenSprite;
        }
    }

    public void SetHealthBarStatusText(StatusTypes status, bool state)
    {
        currentStatusTypes[status] = state;

        healthBarStatusText.text = GetHealthStatusText();
        healthBarStatusText.enabled = true;
    }

    private string GetHealthStatusText()
    {
        foreach (StatusTypes status in statusPriority)
            if (currentStatusTypes.ContainsKey(status) && currentStatusTypes[status])
                return GetHealthStatusString(status);
        return "";
    }

    private string GetHealthStatusString(StatusTypes status)
    {
        switch (status)
        {
            case StatusTypes.Stunned:
                return "STUNNED";
            case StatusTypes.Silenced:
                return "SILENCED";
            case StatusTypes.Disarmed:
                return "DISARMED";
            case StatusTypes.Taunted:
                return "TAUNTED";
            case StatusTypes.Stacked:
                return "STACKED";
        }
        return "";
    }
}
