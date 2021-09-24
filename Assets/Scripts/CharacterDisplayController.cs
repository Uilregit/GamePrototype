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
    public Animator passiveEffectAnim;

    // Start is called before the first frame update
    void Awake()
    {
        shadow.sprite = sprite.sprite;
    }
}
