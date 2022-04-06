using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Experimental.Rendering.Universal;

public class CharacterDisplayController : MonoBehaviour
{
    public SpriteRenderer sprite;
    public SpriteRenderer squetchSprite;
    public SpriteRenderer deathSprite;
    public SpriteRenderer shadow;
    public SpriteRenderer outline;

    public HealthBarController healthBar;
    public Text vitText;
    public Text armorText;
    public Text attackText;

    public Image intentLocation;

    public List<SpriteRenderer> statTextBacks;
    public SpriteRenderer highlight;

    public List<Image> buffIcons;
    public CharacterAnimationController charAnimController;
    public Animator hitEffectAnim;
    public Animator passiveEffectAnim;
    public OnHitSoundController onHitSoundController;
    public SpriteRenderer pointLight;

    public LineRenderer lineRenderer;

    private Vector3 spritePosition = Vector3.zero;
    private bool isBeingShoved = false;

    // Start is called before the first frame update
    void Awake()
    {
        shadow.sprite = sprite.sprite;
        squetchSprite.sprite = sprite.sprite;

        deathSprite.material = new Material(deathSprite.material);
        deathSprite.material.SetFloat("_NoiseSize", 5f);
    }

    public void TriggerOnHitEffect(Card.HitEffect triggerName, string triggerOverride = "")
    {
        if (triggerOverride == "")
            hitEffectAnim.SetTrigger(triggerName.ToString());
        else
            hitEffectAnim.SetTrigger(triggerOverride);
        OnHitEffect effect = LootController.loot.GetOnHitEffect(triggerName);
        if (effect != null && pointLight.color.a == 0)
            StartCoroutine(ShineLight(effect.color));

        switch (triggerName)
        {
            case Card.HitEffect.Buff:
                SquetchUp(Color.yellow);
                break;
            case Card.HitEffect.Debuff:
                SquetchDown(Color.blue);
                break;
            case Card.HitEffect.Cleanse:
                SquetchUp(Color.cyan);
                break;
            case Card.HitEffect.Heal:
                SquetchUp(Color.green);
                break;
        }
    }

    public IEnumerator ShineLight(Color color)
    {
        for (int i = 0; i < 50; i++)
        {
            pointLight.color = new Color(color.r, color.g, color.b, 0.5f * (49f - i) / 50f);
            yield return new WaitForSeconds(0.2f / 50);

        }
    }

    public void SquetchUp(Color color)
    {
        squetchSprite.sprite = sprite.sprite;
        squetchSprite.flipX = sprite.flipX;

        sprite.enabled = false;
        squetchSprite.enabled = true;

        StartCoroutine(SquetchUpProcess());
    }

    public void SquetchDown(Color color)
    {
        squetchSprite.sprite = sprite.sprite;
        squetchSprite.flipX = sprite.flipX;

        sprite.enabled = false;
        squetchSprite.enabled = true;

        StartCoroutine(SquetchDownProcess());
    }

    public void ShoveSprite(Vector2 direction)
    {
        squetchSprite.sprite = sprite.sprite;
        squetchSprite.flipX = sprite.flipX;

        StartCoroutine(ShoveSpriteProcess(direction));
    }

    private IEnumerator SquetchUpProcess()
    {
        for (int i = 0; i < 5; i++)
        {
            squetchSprite.transform.localScale = Vector3.Lerp(new Vector3(1, 1, 1), new Vector3(0.5f, 1.5f, 1f), i / 4f);
            squetchSprite.transform.localPosition = Vector3.Lerp(new Vector3(0, 0, 0), new Vector3(0, 0.1f, 0), i / 4f);
            yield return new WaitForSeconds(0.05f / 10f);
        }

        for (int i = 0; i < 5; i++)
        {
            squetchSprite.transform.localScale = Vector3.Lerp(new Vector3(0.5f, 1.5f, 1), new Vector3(1.2f, 0.8f, 1f), i / 4f);
            squetchSprite.transform.localPosition = Vector3.Lerp(new Vector3(0, 0.1f, 0), new Vector3(0, -0.04f, 0), i / 4f);
            yield return new WaitForSeconds(0.1f / 10f);
        }

        for (int i = 0; i < 5; i++)
        {
            squetchSprite.transform.localScale = Vector3.Lerp(new Vector3(1.2f, 0.8f, 1), new Vector3(1, 1, 1f), i / 4f);
            squetchSprite.transform.localPosition = Vector3.Lerp(new Vector3(0, -0.04f, 0), new Vector3(0, 0, 0), i / 4f);
            yield return new WaitForSeconds(0.05f / 10f);
        }

        sprite.enabled = true;
        squetchSprite.enabled = false;
    }

    private IEnumerator SquetchDownProcess()
    {
        for (int i = 0; i < 5; i++)
        {
            squetchSprite.transform.localScale = Vector3.Lerp(new Vector3(1, 1, 1), new Vector3(1.5f, 0.5f, 1f), i / 4f);
            squetchSprite.transform.localPosition = Vector3.Lerp(spritePosition, spritePosition + new Vector3(0, -0.1f, 0), i / 4f);
            yield return new WaitForSeconds(0.025f / 5f);
        }

        for (int i = 0; i < 5; i++)
        {
            squetchSprite.transform.localScale = Vector3.Lerp(new Vector3(1.5f, 0.5f, 1), new Vector3(0.8f, 1.2f, 1f), i / 4f);
            squetchSprite.transform.localPosition = Vector3.Lerp(spritePosition + new Vector3(0, -0.1f, 0), spritePosition + new Vector3(0, 0.04f, 0), i / 4f);
            yield return new WaitForSeconds(0.05f / 5f);
        }

        for (int i = 0; i < 5; i++)
        {
            squetchSprite.transform.localScale = Vector3.Lerp(new Vector3(0.8f, 1.2f, 1), new Vector3(1, 1, 1f), i / 4f);
            squetchSprite.transform.localPosition = Vector3.Lerp(spritePosition + new Vector3(0, 0.04f, 0), spritePosition, i / 4f);
            yield return new WaitForSeconds(0.025f / 5f);
        }

        if (!isBeingShoved)
        {
            sprite.enabled = true;
            squetchSprite.enabled = false;
        }
    }

    private IEnumerator ShoveSpriteProcess(Vector2 direction)
    {
        isBeingShoved = true;
        for (int i = 0; i < 10; i++)
        {
            spritePosition = Vector3.Lerp(new Vector3(0, 0, 0), direction.normalized * 0.5f, i / 9f);
            squetchSprite.transform.localPosition = spritePosition;
            sprite.transform.localPosition = spritePosition;
            yield return new WaitForSeconds(0.1f / 10f);
        }

        yield return new WaitForSeconds(0.2f);

        sprite.enabled = true;
        squetchSprite.enabled = false;

        for (int i = 0; i < 5; i++)
        {
            spritePosition = Vector3.Lerp(direction.normalized * 0.5f, new Vector3(0, 0, 0), i / 4f);
            squetchSprite.transform.localPosition = spritePosition;
            sprite.transform.localPosition = spritePosition;
            yield return new WaitForSeconds(0.05f / 5f);
        }

        isBeingShoved = false;
        spritePosition = Vector3.zero;
    }
}
