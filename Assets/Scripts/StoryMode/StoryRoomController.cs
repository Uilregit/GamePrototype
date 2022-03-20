using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoryRoomController : MonoBehaviour
{
    public int roomId;
    public int unlockRequirementID;
    public bool unlockRequire3Stars = false;
    public bool unockRequiresAllStars = false;
    public bool startHidden = false;
    public StoryRoomSetup setup;
    public StoryRoomType roomType;
    public Image connector;
    public Image[] colorsCompleted;

    private bool isHighlighted = false;
    private float outlineSize;
    public Outline outline;

    public enum StoryRoomType
    {
        Combat = 0,
        Boss = 5,
        Shop = 10,
        SecretShop = 15,
        Arena = 50,
        NakedArena = 60,
        NewWorld = 100,
        PreviousWorld = 101
    }

    public void Start()
    {
        outlineSize = outline.effectDistance.x;
    }

    public void Update()
    {
        if (isHighlighted)
        {
            float effectDistance = Mathf.Lerp(outlineSize, outlineSize * 1.4f, MusicController.music.GetBackgroundAmplitude()[0]);
            outline.effectDistance = new Vector2(effectDistance, effectDistance);
        }
    }

    public void SetHighlighted(bool state)
    {
        isHighlighted = state;
        outline.enabled = state;
    }

    public void SetColorsCompleted(List<Card.CasterColor> colors)
    {
        List<Card.CasterColor> colorOrder = new List<Card.CasterColor> { Card.CasterColor.White, Card.CasterColor.Red, Card.CasterColor.Orange, Card.CasterColor.Green, Card.CasterColor.Blue, Card.CasterColor.Black };
        int counter = 0;
        for (int i = 0; i < 6; i++)
        {
            if (colors.Contains(colorOrder[i]))
            {
                colorsCompleted[i].enabled = true;
                colorsCompleted[i].color = PartyController.party.GetPlayerColor(colorOrder[i]) * new Color(1, 1, 1, 0.5f);
                colorsCompleted[i].transform.localScale = new Vector2(1, 1f / colors.Count);
                if (colors.Count % 2 == 1)
                    colorsCompleted[i].transform.localPosition = new Vector2(0, (float)counter * 0.6f / colors.Count - 0.6f / colors.Count * ((colors.Count - 1) / 2));
                else
                    colorsCompleted[i].transform.localPosition = new Vector2(0, (float)counter * 0.6f / colors.Count - 0.3f + 0.6f / colors.Count / 2);

                counter++;
            }
            else
                colorsCompleted[i].enabled = false;
        }
    }
}
