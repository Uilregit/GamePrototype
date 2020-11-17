﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimationController : MonoBehaviour
{
    private Animator anim;
    private GameObject attacker;
    private List<Vector2> targets;
    private CardEffectsController effectsController;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void SetRunning(bool state)
    {
        anim.SetBool("isRunning", state);
    }

    public void SetCasting(bool state)
    {
        anim.SetBool("isCasting", state);
    }

    public void TriggerAttack(GameObject newAttacker, List<Vector2> newTargets, CardEffectsController newEffectsController)
    {
        attacker = newAttacker;
        targets = newTargets;
        effectsController = newEffectsController;
        anim.SetTrigger("isAttacking");
    }

    public void ProcessCard()
    {
        try
        {
            StartCoroutine(attacker.GetComponent<CardDragController>().OnPlay(targets));
        }
        catch
        {
            StartCoroutine(effectsController.TriggerEffect(attacker, targets));
        }
    }

    public void TriggerDeath()
    {
        anim.SetTrigger("isDead");
    }

    public void Destroy()
    {
        try
        {
            Card.CasterColor color = transform.parent.parent.GetComponent<PlayerController>().GetColorTag();
            GridController.gridController.OnPlayerDeath(this.transform.parent.parent.gameObject, color);
        }
        catch
        {
            Destroy(this.transform.parent.parent.gameObject);
        }
    }
}