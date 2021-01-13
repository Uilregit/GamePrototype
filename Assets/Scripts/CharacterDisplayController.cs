using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterDisplayController : MonoBehaviour
{
    public SpriteRenderer sprite;
    public SpriteRenderer shadow;
    public SpriteRenderer outline;

    public HealthBarController healthBar;
    public Text vitText;
    public Text armorText;
    public Text attackText;

    public List<SpriteRenderer> statTextBacks;
    public SpriteRenderer highlight;

    public List<Image> buffIcons;
    public CharacterAnimationController charAnimController;
    public Animator hitEffectAnim;

    // Start is called before the first frame update
    void Awake()
    {
        shadow.sprite = sprite.sprite;
    }

    public void Hide()
    {
        sprite.enabled = false;
        shadow.enabled = false;
        outline.enabled = false;
        vitText.enabled = false;
        armorText.enabled = false;
        attackText.enabled = false;
        highlight.enabled = false;
        foreach (Image i in buffIcons)
        {
            i.enabled = false;
            i.transform.GetChild(0).GetComponent<Text>().enabled = false;
        }
        foreach (SpriteRenderer i in statTextBacks)
            i.enabled = false;
        transform.parent.GetComponent<Collider2D>().enabled = false;

        try
        {
            transform.parent.GetComponent<EnemyInformationController>().HideIntent();
        }
        catch { }
    }
}
