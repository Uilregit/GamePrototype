﻿using System.Collections;
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
    public Text shieldText;
    public Text attackText;

    public List<Image> buffIcons;

    // Start is called before the first frame update
    void Awake()
    {
        shadow.sprite = sprite.sprite;
    }
}
