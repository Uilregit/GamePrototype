using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AttackRangeHighlightController : MonoBehaviour
{
    public static AttackRangeHighlightController attackRangeHighlight;

    private Canvas backgroundCanvas;
    public Image gridImage;
    public float flipDelay;

    private GameObject[,] crosses;
    private int xSize = 14;
    private int ySize = 20;
    private int xOffset = 5;
    private int yOffset = 5;
    private Image[,] gridImages;
    private AttackRangeGridTile[,] tiles;

    private List<GameObject> highlightedCharacters = new List<GameObject>();
    private GameObject thisCaster;

    // Use this for initialization
    void Start()
    {
        if (AttackRangeHighlightController.attackRangeHighlight == null)
            AttackRangeHighlightController.attackRangeHighlight = this;
        else
        {
            Destroy(this.gameObject);
            return;
        }
        backgroundCanvas = GetComponent<Canvas>();
        InstantiateGrid();
    }

    public void HideAllTiles()
    {
        StopAllCoroutines();
        foreach (AttackRangeGridTile t in tiles)
            t.SetActive(false);

        thisCaster.GetComponent<HealthController>().charDisplay.SetHighlight(false);
        foreach (GameObject obj in highlightedCharacters)
            obj.GetComponent<HealthController>().charDisplay.SetHighlight(false);
        highlightedCharacters = new List<GameObject>();
    }

    public IEnumerator HighlightAttackRange(Vector2 center, GameObject caster, int iteration, bool isOriginal)
    {
        int xLoc = (int)center.x;
        int yLoc = (int)center.y;

        if (isOriginal)
        {
            thisCaster = caster;
            caster.GetComponent<HealthController>().charDisplay.SetHighlight(true);
            foreach (GameObject obj in GridController.gridController.GetObjectAtLocation(TileCreator.tileCreator.GetSelectableLocations()))
            {
                obj.GetComponent<HealthController>().charDisplay.SetHighlight(true);
                highlightedCharacters.Add(obj);
            }
        }

        if (iteration > 0)
        {
            try
            {
                if (!tiles[xLoc + xOffset + 2, yLoc + yOffset + 3].GetActive() && !isOriginal)
                    tiles[xLoc + xOffset + 2, yLoc + yOffset + 3].SetActive(true);
            }
            catch { }
            yield return new WaitForSeconds(flipDelay / 10f);
            try
            {
                if (xLoc + xOffset + 2 > 0 && !tiles[xLoc - 1 + xOffset + 2, yLoc + yOffset + 3].GetActive())
                    StartCoroutine(HighlightAttackRange(new Vector2(xLoc - 1, yLoc), caster, 99, false));
                if (xLoc + xOffset + 2 < xSize - 1 && !tiles[xLoc + 1 + xOffset + 2, yLoc + yOffset + 3].GetActive())
                    StartCoroutine(HighlightAttackRange(new Vector2(xLoc + 1, yLoc), caster, 99, false));
                if (yLoc + yOffset + 3 > 0 && !tiles[xLoc + xOffset + 2, yLoc - 1 + yOffset + 3].GetActive())
                    StartCoroutine(HighlightAttackRange(new Vector2(xLoc, yLoc - 1), caster, 99, false));
                if (yLoc + yOffset + 3 < ySize - 1 && !tiles[xLoc + xOffset + 2, yLoc + 1 + yOffset + 3].GetActive())
                    StartCoroutine(HighlightAttackRange(new Vector2(xLoc, yLoc + 1), caster, 99, false));
            }
            catch { }
        }
    }

    IEnumerator Flip(int xLoc, int yLoc, int iteration)
    {
        if (iteration > 0)
        {
            if (tiles[xLoc + xOffset, yLoc + yOffset].GetCanFlip())
                tiles[xLoc + xOffset, yLoc + yOffset].Flip();
            yield return new WaitForSeconds(flipDelay);
            if (xLoc + xOffset > 0 && tiles[xLoc - 1 + xOffset, yLoc + yOffset].GetCanFlip())
                StartCoroutine(Flip(xLoc - 1, yLoc, iteration - 1));
            if (xLoc + xOffset < xSize - 1 && tiles[xLoc + 1 + xOffset, yLoc + yOffset].GetCanFlip())
                StartCoroutine(Flip(xLoc + 1, yLoc, iteration - 1));
            if (yLoc + yOffset > 0 && tiles[xLoc + xOffset, yLoc - 1 + yOffset].GetCanFlip())
                StartCoroutine(Flip(xLoc, yLoc - 1, iteration - 1));
            if (yLoc + yOffset < ySize - 1 && tiles[xLoc + xOffset, yLoc + 1 + yOffset].GetCanFlip())
                StartCoroutine(Flip(xLoc, yLoc + 1, iteration - 1));
        }
    }

    private void InstantiateGrid()
    {
        gridImages = new Image[xSize, ySize];
        tiles = new AttackRangeGridTile[xSize, ySize];
        for (int i = 0; i < xSize; i++)
        {
            for (int j = 0; j < ySize; j++)
            {
                gridImages[i, j] = Instantiate(gridImage, new Vector2(i - xSize / 2, j - ySize / 2 + 2), Quaternion.identity);
                tiles[i, j] = gridImages[i, j].GetComponent<AttackRangeGridTile>();
                //gridImages[i, j].transform.localScale = new Vector2(GameController.director.size / 0.5f, GameController.director.size / 0.5f);
                gridImages[i, j].transform.SetParent(backgroundCanvas.transform);
                gridImages[i, j].transform.SetAsFirstSibling();
            }
        }
    }
}
