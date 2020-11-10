using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapController : MonoBehaviour
{
    private GameObject caster;
    private Card card;
    private int effectIndex;

    public void SetValues(GameObject newCaster, Card newCard, int newEffectIndex)
    {
        caster = newCaster;
        card = newCard;
        effectIndex = newEffectIndex;
    }

    public virtual void Trigger(List<GameObject> trappedObjects)
    {
        foreach (GameObject trappedObject in trappedObjects)
        {
            trappedObject.GetComponent<HealthController>().SetStunned(true);        //Apply ministun to stop object's turn
            trappedObject.GetComponent<BuffController>().AddBuff(GameController.gameController.stunBuff);
        }
        EffectFactory factory = new EffectFactory();
        Effect effect = factory.GetEffect(card.cardEffectName[effectIndex]);
        effect.Process(caster, null, new List<Vector2>() { trappedObjects[0].transform.position }, card, effectIndex);
        Destroy(this.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (card.targetType[effectIndex] == Card.TargetType.Player && collision.gameObject.tag == "Player" ||
            card.targetType[effectIndex] == Card.TargetType.Enemy && collision.gameObject.tag == "Enemy")
        {
            Trigger(GridController.gridController.GetObjectAtLocation(collision.transform.position, new string[] { "Enemy" }));
        }
    }
}
