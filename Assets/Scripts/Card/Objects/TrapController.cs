using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapController : MonoBehaviour
{
    private Card card;
    private int effectIndex;

    public void SetValues(Card newCard, int newEffectIndex)
    {
        card = newCard;
        effectIndex = newEffectIndex;
    }

    public virtual void Trigger(GameObject trappedObject)
    {
        trappedObject.GetComponent<HealthController>().SetStunned(true);        //Apply ministun to stop object's turn
        trappedObject.GetComponent<HealthController>().AddEndOfTurnBuff(new StunDebuff(), 0);

        EffectFactory factory = new EffectFactory();
        Effect effect = factory.GetEffect(card.cardEffectName[effectIndex]);
        effect.Process(this.gameObject, null, trappedObject, card, effectIndex);
        Destroy(this.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (card.targetType[effectIndex] == Card.TargetType.Player && collision.gameObject.tag == "Player" ||
            card.targetType[effectIndex] == Card.TargetType.Enemy && collision.gameObject.tag == "Enemy")
        {
            Trigger(collision.gameObject);
        }
    }
}
