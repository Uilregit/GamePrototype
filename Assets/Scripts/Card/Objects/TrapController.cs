using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapController : MonoBehaviour
{
    public Color trapColor;
    private GameObject caster;
    private Card card;
    private int effectIndex;
    private int duration;

    public void SetValues(GameObject newCaster, Card newCard, int newEffectIndex)
    {
        caster = newCaster;
        card = newCard;
        effectIndex = newEffectIndex;
        duration = card.effectDuration[newEffectIndex - 1] * 2 + 1;
    }

    public int GetDuration()
    {
        return duration;
    }

    public int GetEffectValue()
    {
        return card.effectValue[effectIndex];
    }

    public virtual IEnumerator Trigger()
    {
        /*
        foreach (GameObject trappedObject in trappedObjects)
        {
            trappedObject.GetComponent<HealthController>().SetStunned(true);        //Apply ministun to stop object's turn
            trappedObject.GetComponent<BuffController>().AddBuff(GameController.gameController.stunBuff);
        }
        */

        if (GridController.gridController.GetObjectAtLocation(transform.position).Count > 0)        //Pause if characters are trapped for visual clarity
        {
            EffectFactory factory = new EffectFactory();
            Effect effect = factory.GetEffect(card.cardEffectName[effectIndex]);

            yield return StartCoroutine(effect.Process(caster, null, new List<Vector2>() { transform.position }, card, effectIndex));
            yield return new WaitForSeconds(0.5f * TimeController.time.timerMultiplier);
        }

        Debug.Log("finished trap trigger");
    }

    public void ReduceDuration()
    {
        duration -= 1;
        if (duration <= 0)
        {
            this.gameObject.SetActive(false);
        }
        else if (duration < 3)
            GetComponent<SpriteRenderer>().color = trapColor * new Color(0.5f, 0.5f, 0.5f, 1);
    }

    /*
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (card.targetType[effectIndex] == Card.TargetType.Player && collision.gameObject.tag == "Player" ||
            card.targetType[effectIndex] == Card.TargetType.Enemy && collision.gameObject.tag == "Enemy")
        {
            Trigger(GridController.gridController.GetObjectAtLocation(collision.transform.position, new string[] { "Enemy" }));
        }
    }
    */
}
