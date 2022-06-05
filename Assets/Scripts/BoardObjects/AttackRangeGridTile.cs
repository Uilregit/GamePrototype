using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AttackRangeGridTile : MonoBehaviour
{
    public float flipDuration;
    public float flipCoolDown;

    private bool isActive = false;
    private bool canFlip = true;
    private float timer = 0;
    private Image self;
    private float normalScale;

    // Use this for initialization
    void Awake()
    {
        timer = flipCoolDown + flipDuration;
        self = GetComponent<Image>();
        normalScale = 1; // GameController.director.size / 0.5f;
        self.rectTransform.localScale = new Vector3(0, 0, 1);
        //self.rectTransform.localScale = new Vector2(0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer <= flipDuration)
            //self.rectTransform.localScale = Vector2.Lerp(new Vector2(normalScale, normalScale), new Vector2(-normalScale, normalScale), timer / flipDuration);
            self.rectTransform.localScale = Vector2.Lerp(new Vector2(0, 0), new Vector2(normalScale, normalScale), timer / flipDuration);
        //self.rectTransform.localScale = Vector2.Lerp(new Vector2(normalScale, normalScale), new Vector2(0, 0), timer / flipDuration);
        //self.rectTransform.localScale = Vector2.Lerp(new Vector2(2 - normalScale, 2 - normalScale), new Vector2(normalScale, normalScale), timer / flipDuration);
        else if (timer < flipDuration + flipCoolDown)
            //self.rectTransform.localScale = new Vector2(0, 0);
            self.rectTransform.localScale = new Vector2(normalScale, normalScale);
        else
            canFlip = true;
    }

    public void Flip()
    {
        if (canFlip && self.rectTransform.localScale == new Vector3(1, 1, 1))
            timer = 0;
        if (canFlip)
            canFlip = false;
    }

    public void Flip(float time)
    {
        if (canFlip && self.rectTransform.localScale == new Vector3(1, 1, 1))
            timer = -time;
        if (canFlip)
            canFlip = false;
    }

    public void SetActive(bool state)
    {
        isActive = state;
        if (!TileCreator.tileCreator.GetSelectableLocations().Contains(transform.position) && !TileCreator.tileCreator.GetTilePositions(0).Contains(transform.position) && !TileCreator.tileCreator.GetTilePositions(1).Contains(transform.position) && !TileCreator.tileCreator.GetTilePositions(2).Contains(transform.position))
        {
            if (isActive)
                self.rectTransform.localScale = new Vector3(1, 1, 1);
            else
                self.rectTransform.localScale = new Vector3(0, 0, 1);
        }
    }

    public bool GetActive()
    {
        return isActive;
    }

    public bool GetCanFlip()
    {
        return canFlip;
    }
}
