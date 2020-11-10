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

    public List<Image> buffIcons;

    private Animator anim;

    // Start is called before the first frame update
    void Awake()
    {
        shadow.sprite = sprite.sprite;
        anim = GetComponent<Animator>();
    }

    public void SetCasting(bool state)
    {
        anim.SetBool("isCasting", state);
    }

    public void TriggerAttack()
    {
        anim.SetTrigger("isAttacking");
    }

    public void TriggerDeath()
    {
        anim.SetTrigger("isDead");
    }

    public void Destroy()
    {
        try
        {
            Card.CasterColor color = transform.parent.GetComponent<PlayerController>().GetColorTag();
            GridController.gridController.OnPlayerDeath(this.transform.parent.gameObject, color);
        }
        catch
        {
            Destroy(this.transform.parent.gameObject);
        }
    }
}
