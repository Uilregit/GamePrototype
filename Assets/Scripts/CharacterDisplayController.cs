using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Experimental.Rendering.Universal;

public class CharacterDisplayController : MonoBehaviour
{
    public SpriteRenderer sprite;
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
    public Light2D pointLight;

    // Start is called before the first frame update
    void Awake()
    {
        shadow.sprite = sprite.sprite;
    }

    public void TriggerOnHitEffect(Card.HitEffect triggerName, string triggerOverride = "")
    {
        if (triggerOverride == "")
            hitEffectAnim.SetTrigger(triggerName.ToString());
        else
            hitEffectAnim.SetTrigger(triggerOverride);
        OnHitEffect effect = LootController.loot.GetOnHitEffect(triggerName);
        if (effect != null && pointLight.intensity == 0)
            StartCoroutine(ShineLight(effect.color));
    }

    public IEnumerator ShineLight(Color color)
    {
        pointLight.color = color;
        for (int i = 0; i < 50; i++)
        {
            pointLight.intensity = 0.5f * (49f - i) / 50f;
            yield return new WaitForSeconds(0.2f / 50);

        }
    }
}
