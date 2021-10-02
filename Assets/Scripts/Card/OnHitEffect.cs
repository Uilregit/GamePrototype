using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class OnHitEffect : ScriptableObject
{
    public Card.HitEffect effectName;
    public Color color;
    public ParticleSystem particleSystem;
}
