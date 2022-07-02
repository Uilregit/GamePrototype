using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using System;
using System.Linq;

public class TileCreator : MonoBehaviour
{
    public static TileCreator tileCreator;

    [Header("Grids")]
    public Sprite selectableTileSprite;
    public Tilemap selectableTileMap;
    public Tilemap[] tileMap;
    public Tilemap dangerAreaTileMap;
    public Tilemap selectedEnemiesDangerAreaTileMap;

    [Header("Select Range Sprites")]
    public Sprite uSprite;
    public Sprite dSprite;
    public Sprite lSprite;
    public Sprite rSprite;
    public Sprite urSprite;
    public Sprite ulSprite;
    public Sprite drSprite;
    public Sprite dlSprite;
    public Sprite udSprite;
    public Sprite lrSprite;
    public Sprite urlSprite;
    public Sprite drlSprite;
    public Sprite rudSprite;
    public Sprite ludSprite;
    public Sprite allSprite;
    public Sprite noneSprite;

    [Header("Path Grids")]
    public Tilemap[] pathTileMap;
    public Tilemap[] moveRangeTileMap;

    [Header("Path Sprites")]
    public Sprite pUStartSprite;
    public Sprite pDStartSprite;
    public Sprite pLStartSprite;
    public Sprite pRStartSprite;
    public Sprite pUEndSprite;
    public Sprite pDEndSprite;
    public Sprite pLEndSprite;
    public Sprite pREndSprite;
    public Sprite pUDSprite;
    public Sprite pLRSprite;
    public Sprite pULSprite;
    public Sprite pDLSprite;
    public Sprite pURSprite;
    public Sprite pDRSprite;

    [Header("Moverange Sprites")]
    public Sprite left0Sprite;
    public Sprite left1Sprite;
    public Sprite left2Sprite;
    public Sprite left3Sprite;
    public Sprite left4Sprite;
    public Sprite left5Sprite;
    public Sprite left6Sprite;
    public Sprite left7Sprite;
    public Sprite left8Sprite;
    public Sprite left9Sprite;
    public Sprite left10Sprite;
    public Sprite left11Sprite;
    public Sprite left12Sprite;
    public Sprite left13Sprite;

    [Header("Buttons")]
    public Image dangerAreaButton;

    private Dictionary<Vector2, Tile>[] tiles;
    private Dictionary<Vector2, int>[] tilePositions;
    private List<Vector2> selectableTilePositions = new List<Vector2>();
    private List<Vector2> dangerAreaPositions = new List<Vector2>();
    private List<Vector2> selectedEnemiesDangerAreaPositions = new List<Vector2>();

    private Dictionary<Vector2, Tile>[] pathTiles;
    private Dictionary<Vector2, Tile>[] moveRangeTiles;
    private Dictionary<Vector2, Tile> dangerAreaTiles = new Dictionary<Vector2, Tile>();
    private Dictionary<Vector2, Tile> selectedEnemiesDangerAreaTiles = new Dictionary<Vector2, Tile>();

    private List<EnemyController> selectedEnemies = new List<EnemyController>();
    private bool dangerAreaEnabled = false;

    //private bool committed = false;

    private GameObject creator;

    // Start is called before the first frame update
    void Start()
    {
        if (TileCreator.tileCreator == null)
            TileCreator.tileCreator = this;
        else
            Destroy(this.gameObject);

        tiles = new Dictionary<Vector2, Tile>[tileMap.Length];
        tilePositions = new Dictionary<Vector2, int>[tileMap.Length];
        for (int i = 0; i < tileMap.Length; i++)
        {
            tiles[i] = new Dictionary<Vector2, Tile>();
            tilePositions[i] = new Dictionary<Vector2, int>();
        }

        pathTiles = new Dictionary<Vector2, Tile>[pathTileMap.Length];
        for (int i = 0; i < pathTileMap.Length; i++)
            pathTiles[i] = new Dictionary<Vector2, Tile>();

        moveRangeTiles = new Dictionary<Vector2, Tile>[moveRangeTileMap.Length];
        for (int i = 0; i < moveRangeTileMap.Length; i++)
            moveRangeTiles[i] = new Dictionary<Vector2, Tile>();

        selectableTilePositions = new List<Vector2>();

        dangerAreaTileMap.GetComponent<TilemapRenderer>().enabled = false;
    }

    public void CreateTiles(GameObject newCreator, Vector2 startLocation, Card.CastShape shape, int range, Color color, int layer = 0)
    {
        CreateTiles(newCreator, startLocation, shape, range, color, new string[] { "None" }, layer);
    }

    public void CreateTiles(GameObject newCreator, Vector2 startLocation, Card.CastShape shape, int range, Color color, string[] avoidTag, int layer = 0)
    {
        //if (!committed)
        //{
        creator = newCreator;
        InstantiateTiles(startLocation, shape, range, color, avoidTag, layer, true);
        RefreshTiles(layer);
        //}
    }

    public void ToggleDangerArea()
    {
        dangerAreaEnabled = !dangerAreaEnabled;
        dangerAreaTileMap.GetComponent<TilemapRenderer>().enabled = dangerAreaEnabled;
        if (dangerAreaEnabled)
            dangerAreaButton.color = GameController.gameController.notYetDoneColor;
        else
            dangerAreaButton.color = GameController.gameController.doneColor;
    }

    public void RefreshDangerArea()
    {
        foreach (Vector2 position in dangerAreaPositions)
            dangerAreaTileMap.SetTile(Vector3Int.RoundToInt(position), null);

        dangerAreaPositions = new List<Vector2>();
        dangerAreaTiles = new Dictionary<Vector2, Tile>();
        foreach (EnemyController enemy in TurnController.turnController.GetEnemies())
        {
            dangerAreaPositions.AddRange(enemy.GetEnemyInformationController().GetAttackableLocations());
        }
        dangerAreaPositions = dangerAreaPositions.Distinct().ToList();

        foreach (Vector2 location in dangerAreaPositions)
        {
            Tile tile = ScriptableObject.CreateInstance<Tile>();
            dangerAreaTiles[location] = tile;
            tile.color = Color.red;
            dangerAreaTileMap.SetTile(Vector3Int.RoundToInt(location), tile);
        }

        RefreshTiles(dangerAreaTileMap, dangerAreaTiles, dangerAreaPositions);
        RefreshSelectedEnemiesDangerArea();
    }

    private void RefreshSelectedEnemiesDangerArea()
    {
        foreach (Vector2 position in selectedEnemiesDangerAreaPositions)
            selectedEnemiesDangerAreaTileMap.SetTile(Vector3Int.RoundToInt(position), null);

        selectedEnemiesDangerAreaPositions = new List<Vector2>();
        selectedEnemiesDangerAreaTiles = new Dictionary<Vector2, Tile>();

        List<EnemyController> aliveEnemies = new List<EnemyController>();
        foreach (EnemyController enemy in selectedEnemies)
            if (TurnController.turnController.GetEnemies().Contains(enemy))
                aliveEnemies.Add(enemy);
        selectedEnemies = aliveEnemies;
        foreach (EnemyController enemy in selectedEnemies)
        {
            selectedEnemiesDangerAreaPositions.AddRange(enemy.GetEnemyInformationController().GetAttackableLocations());
        }
        selectedEnemiesDangerAreaPositions = selectedEnemiesDangerAreaPositions.Distinct().ToList();

        foreach (Vector2 location in selectedEnemiesDangerAreaPositions)
        {
            Tile tile = ScriptableObject.CreateInstance<Tile>();
            selectedEnemiesDangerAreaTiles[location] = tile;
            tile.color = Color.red;
            selectedEnemiesDangerAreaTileMap.SetTile(Vector3Int.RoundToInt(location), tile);
        }

        RefreshTiles(selectedEnemiesDangerAreaTileMap, selectedEnemiesDangerAreaTiles, selectedEnemiesDangerAreaPositions);
    }

    public void AddSelectedEnemy(EnemyController enemy)
    {
        if (selectedEnemies.Contains(enemy))
        {
            selectedEnemies.Remove(enemy);
            enemy.GetHealthController().charDisplay.sprite.color = Color.white;
        }
        else
        {
            selectedEnemies.Add(enemy);
            enemy.GetHealthController().charDisplay.sprite.color = Color.red;
        }

        RefreshSelectedEnemiesDangerArea();
    }

    //Destroys tiles in ALL layers
    public void DestroyTiles(GameObject destroyer)
    {
        //if (destroyer == creator) //Only allow the creator to destroy the tiles
        //{
        //Sets all tiles in all layers to null
        for (int i = 0; i < tileMap.Length; i++)
            foreach (Vector2 position in tilePositions[i].Keys)
                tileMap[i].SetTile(Vector3Int.RoundToInt(position), null);
        //Reset all tiles and positions to default
        tiles = new Dictionary<Vector2, Tile>[tileMap.Length];
        tilePositions = new Dictionary<Vector2, int>[tileMap.Length];
        for (int i = 0; i < tileMap.Length; i++)
        {
            tiles[i] = new Dictionary<Vector2, Tile>();
            tilePositions[i] = new Dictionary<Vector2, int>();
        }
        //}
    }

    public void DestroyTiles(GameObject destroyer, int layer)
    {
        //if (destroyer == creator) //Only allow the creator to destroy the tiles
        //{
        foreach (Vector2 position in tilePositions[layer].Keys)
            tileMap[layer].SetTile(Vector3Int.RoundToInt(position), null);
        //Reset all tiles and positions to default
        tiles[layer] = new Dictionary<Vector2, Tile>();
        tilePositions[layer] = new Dictionary<Vector2, int>();
        //}
    }

    public void DestroySpecificTiles(GameObject destroyer, List<Vector2> locations, int layer = 0)
    {
        //if (destroyer == creator)
        //{
        foreach (Vector2 loc in locations)
        {
            tileMap[layer].SetTile(Vector3Int.RoundToInt(loc), null);
            tiles[layer].Remove(loc);
            tilePositions[layer].Remove(loc);
        }
        RefreshTiles(layer);
        //}
    }

    public void CreateSelectableTile(Vector2 location)
    {
        Tile tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = selectableTileSprite;
        tile.color = new Color(1, 0.8f, 0);
        selectableTilePositions.Add(location);
        selectableTileMap.SetTile(Vector3Int.RoundToInt(location), tile);
    }

    public List<Vector2> GetSelectableLocations()
    {
        return selectableTilePositions;
    }

    public void DestroySelectableTiles()
    {
        foreach (Vector2 loc in selectableTilePositions)
            selectableTileMap.SetTile(Vector3Int.RoundToInt(loc), null);
        selectableTilePositions = new List<Vector2>();
    }

    private void InstantiateTiles(Vector2 startLocation, Card.CastShape shape, int range, Color color, string[] avoidTag, int layer, bool isFirstTile)
    {
        if (range >= 0)
        {
            //if there is an object that needs to be avoided, don't create a tile here
            if (!GridController.gridController.CheckIfOutOfBounds(startLocation) && !Array.Exists(avoidTag, element => element == "None") &&
                GridController.gridController.GetObjectAtLocation(startLocation).Count != 0)
            {
                if (GridController.gridController.GetObjectAtLocation(startLocation).Any(x => avoidTag.Contains(x.tag)) &&
                    !creator.GetComponent<HealthController>().GetOccupiedSpaces().Any(x => x + (Vector2)creator.transform.position == startLocation) &&  //Always ignore creator for blocking
                    !isFirstTile) //Always create on the first time (in case an enemy was knocked there)
                    return;
            }

            if (shape == Card.CastShape.Circle)
            {
                // if this location doesn't have a tile yet or this is a better rout and it's still in bounds, create tile and recurse
                if ((!tilePositions[layer].ContainsKey(startLocation) || range > tilePositions[layer][startLocation]) && !GridController.gridController.CheckIfOutOfBounds(startLocation))
                {
                    Tile tile = ScriptableObject.CreateInstance<Tile>();
                    tiles[layer][startLocation] = tile;
                    tilePositions[layer][startLocation] = range;
                    tile.color = color;
                    tileMap[layer].SetTile(Vector3Int.RoundToInt(startLocation), tile);
                    int x = (int)startLocation.x;
                    int y = (int)startLocation.y;
                    InstantiateTiles(new Vector2(x - 1, y), shape, range - 1, color, avoidTag, layer, false);
                    InstantiateTiles(new Vector2(x + 1, y), shape, range - 1, color, avoidTag, layer, false);
                    InstantiateTiles(new Vector2(x, y - 1), shape, range - 1, color, avoidTag, layer, false);
                    InstantiateTiles(new Vector2(x, y + 1), shape, range - 1, color, avoidTag, layer, false);
                }
            }
            else if (shape == Card.CastShape.Plus)
            {
                for (int i = 1; i < range + 1; i++)
                {
                    if (!GridController.gridController.CheckIfOutOfBounds(startLocation + Vector2.right * i))
                    {
                        Tile tile1 = ScriptableObject.CreateInstance<Tile>();
                        tiles[layer][startLocation + Vector2.right * i] = tile1;
                        tilePositions[layer][startLocation + Vector2.right * i] = range;
                        tile1.color = color;
                        tileMap[layer].SetTile(Vector3Int.RoundToInt(startLocation + Vector2.right * i), tile1);
                    }

                    if (!GridController.gridController.CheckIfOutOfBounds(startLocation + Vector2.left * i))
                    {
                        Tile tile2 = ScriptableObject.CreateInstance<Tile>();
                        tiles[layer][startLocation + Vector2.left * i] = tile2;
                        tilePositions[layer][startLocation + Vector2.left * i] = range;
                        tile2.color = color;
                        tileMap[layer].SetTile(Vector3Int.RoundToInt(startLocation + Vector2.left * i), tile2);
                    }

                    if (!GridController.gridController.CheckIfOutOfBounds(startLocation + Vector2.up * i))
                    {
                        Tile tile3 = ScriptableObject.CreateInstance<Tile>();
                        tiles[layer][startLocation + Vector2.up * i] = tile3;
                        tilePositions[layer][startLocation + Vector2.up * i] = range;
                        tile3.color = color;
                        tileMap[layer].SetTile(Vector3Int.RoundToInt(startLocation + Vector2.up * i), tile3);
                    }

                    if (!GridController.gridController.CheckIfOutOfBounds(startLocation + Vector2.down * i))
                    {
                        Tile tile4 = ScriptableObject.CreateInstance<Tile>();
                        tiles[layer][startLocation + Vector2.down * i] = tile4;
                        tilePositions[layer][startLocation + Vector2.down * i] = range;
                        tile4.color = color;
                        tileMap[layer].SetTile(Vector3Int.RoundToInt(startLocation + Vector2.down * i), tile4);
                    }
                }
            }
        }
    }

    public void RefreshTiles(int layer)
    {
        /*
        foreach (Vector2 location in tilePositions[layer].Keys)
        {
            bool u, d, l, r;
            u = d = l = r = false;

            if (!tilePositions[layer].ContainsKey(location + new Vector2(0, 1)))
                u = true;
            if (!tilePositions[layer].ContainsKey(location + new Vector2(0, -1)))
                d = true;
            if (!tilePositions[layer].ContainsKey(location + new Vector2(1, 0)))
                r = true;
            if (!tilePositions[layer].ContainsKey(location + new Vector2(-1, 0)))
                l = true;

            if (u && !d && !r && !l)
                tiles[layer][location].sprite = uSprite;
            else if (!u && d && !r && !l)
                tiles[layer][location].sprite = dSprite;
            else if (!u && !d && r && !l)
                tiles[layer][location].sprite = rSprite;
            else if (!u && !d && !r && l)
                tiles[layer][location].sprite = lSprite;
            else if (u && !d && r && !l)
                tiles[layer][location].sprite = urSprite;
            else if (u && !d && !r && l)
                tiles[layer][location].sprite = ulSprite;
            else if (!u && d && r && !l)
                tiles[layer][location].sprite = drSprite;
            else if (!u && d && !r && l)
                tiles[layer][location].sprite = dlSprite;
            else if (u && d && !r && !l)
                tiles[layer][location].sprite = udSprite;
            else if (!u && !d && r && l)
                tiles[layer][location].sprite = lrSprite;
            else if (u && !d && r && l)
                tiles[layer][location].sprite = urlSprite;
            else if (!u && d && r && l)
                tiles[layer][location].sprite = drlSprite;
            else if (u && d && r && !l)
                tiles[layer][location].sprite = rudSprite;
            else if (u && d && !r && l)
                tiles[layer][location].sprite = ludSprite;
            else if (!u && !d && !r && !l)
                tiles[layer][location].sprite = noneSprite;
            else
                tiles[layer][location].sprite = allSprite;

            tileMap[layer].RefreshTile(Vector3Int.RoundToInt(location));
        }
        */
        RefreshTiles(tileMap[layer], tiles[layer], tilePositions[layer].Keys.ToList());
    }

    private void RefreshTiles(Tilemap map, Dictionary<Vector2, Tile> tiles, List<Vector2> positions)
    {
        foreach (Vector2 location in positions)
        {
            bool u, d, l, r;
            u = d = l = r = false;

            if (!tiles.ContainsKey(location + new Vector2(0, 1)))
                u = true;
            if (!tiles.ContainsKey(location + new Vector2(0, -1)))
                d = true;
            if (!tiles.ContainsKey(location + new Vector2(1, 0)))
                r = true;
            if (!tiles.ContainsKey(location + new Vector2(-1, 0)))
                l = true;

            if (u && !d && !r && !l)
                tiles[location].sprite = uSprite;
            else if (!u && d && !r && !l)
                tiles[location].sprite = dSprite;
            else if (!u && !d && r && !l)
                tiles[location].sprite = rSprite;
            else if (!u && !d && !r && l)
                tiles[location].sprite = lSprite;
            else if (u && !d && r && !l)
                tiles[location].sprite = urSprite;
            else if (u && !d && !r && l)
                tiles[location].sprite = ulSprite;
            else if (!u && d && r && !l)
                tiles[location].sprite = drSprite;
            else if (!u && d && !r && l)
                tiles[location].sprite = dlSprite;
            else if (u && d && !r && !l)
                tiles[location].sprite = udSprite;
            else if (!u && !d && r && l)
                tiles[location].sprite = lrSprite;
            else if (u && !d && r && l)
                tiles[location].sprite = urlSprite;
            else if (!u && d && r && l)
                tiles[location].sprite = drlSprite;
            else if (u && d && r && !l)
                tiles[location].sprite = rudSprite;
            else if (u && d && !r && l)
                tiles[location].sprite = ludSprite;
            else if (!u && !d && !r && !l)
                tiles[location].sprite = noneSprite;
            else
                tiles[location].sprite = allSprite;

            map.RefreshTile(Vector3Int.RoundToInt(location));
        }
    }

    public List<Vector2> GetTilePositions(int layer = 0)
    {
        List<Vector2> output = new List<Vector2>();
        foreach (Vector2 vec in tilePositions[layer].Keys)
            output.Add(vec);
        return output;
    }

    public void CreatePathTiles(int layer, List<Vector2> path, int moveRangeLeft, Color color)
    {
        Color tileColor = new Color(color.r, color.g, color.b);

        if (path.Count > 1)
        {
            Tile tile;
            foreach (Vector2 loc in path)
            {
                tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = selectableTileSprite;
                tile.color = tileColor;
                pathTiles[layer][loc] = tile;
                pathTileMap[layer].SetTile(Vector3Int.RoundToInt(loc), tile);
            }

            if (moveRangeLeft >= 0)
            {
                tile = ScriptableObject.CreateInstance<Tile>();
                tile.color = tileColor;
                tile.sprite = selectableTileSprite;
                moveRangeTiles[layer][path[0]] = tile;
                moveRangeTileMap[layer].SetTile(Vector3Int.RoundToInt(path[0]), tile);
            }

            RefreshPathTiles(layer, path[0], path[path.Count - 1], path.Count, moveRangeLeft);
        }
        else
            DestroyPathTiles(layer);
    }

    public List<Vector2> GetPathTilePositions(int layer)
    {
        return pathTiles[layer].Keys.ToList();
    }

    public void RefreshPathTiles(int layer, Vector2 startingLoc, Vector2 endingLoc, int len, int moveRangeLeft)
    {
        foreach (Vector2 location in pathTiles[layer].Keys)
        {
            bool u, d, l, r;
            u = d = l = r = false;

            bool starting, ending;
            starting = location == startingLoc;
            ending = location == endingLoc;

            if (pathTiles[layer].ContainsKey(location + new Vector2(0, 1)))
                u = true;
            if (pathTiles[layer].ContainsKey(location + new Vector2(0, -1)))
                d = true;
            if (pathTiles[layer].ContainsKey(location + new Vector2(1, 0)))
                r = true;
            if (pathTiles[layer].ContainsKey(location + new Vector2(-1, 0)))
                l = true;

            if (starting)
            {
                if (u)
                    pathTiles[layer][location].sprite = pUStartSprite;
                else if (d)
                    pathTiles[layer][location].sprite = pDStartSprite;
                else if (l)
                    pathTiles[layer][location].sprite = pLStartSprite;
                else if (r)
                    pathTiles[layer][location].sprite = pRStartSprite;
            }
            else if (ending)
            {
                if (u)
                    pathTiles[layer][location].sprite = pUEndSprite;
                else if (d)
                    pathTiles[layer][location].sprite = pDEndSprite;
                else if (l)
                    pathTiles[layer][location].sprite = pLEndSprite;
                else if (r)
                    pathTiles[layer][location].sprite = pREndSprite;
            }
            else
            {
                if (u && d)
                    pathTiles[layer][location].sprite = pUDSprite;
                else if (l && r)
                    pathTiles[layer][location].sprite = pLRSprite;
                else if (u && l)
                    pathTiles[layer][location].sprite = pULSprite;
                else if (u && r)
                    pathTiles[layer][location].sprite = pURSprite;
                else if (d && l)
                    pathTiles[layer][location].sprite = pDLSprite;
                else if (d && r)
                    pathTiles[layer][location].sprite = pDRSprite;
            }
            pathTileMap[layer].RefreshTile(Vector3Int.RoundToInt(location));
        }

        //if (moveRangeLeft >= 0)
        switch (moveRangeLeft)
        {
            case (0):
                moveRangeTiles[layer][startingLoc].sprite = left0Sprite;
                break;
            case (1):
                moveRangeTiles[layer][startingLoc].sprite = left1Sprite;
                break;
            case (2):
                moveRangeTiles[layer][startingLoc].sprite = left2Sprite;
                break;
            case (3):
                moveRangeTiles[layer][startingLoc].sprite = left3Sprite;
                break;
            case (4):
                moveRangeTiles[layer][startingLoc].sprite = left4Sprite;
                break;
            case (5):
                moveRangeTiles[layer][startingLoc].sprite = left5Sprite;
                break;
            case (6):
                moveRangeTiles[layer][startingLoc].sprite = left6Sprite;
                break;
            case (7):
                moveRangeTiles[layer][startingLoc].sprite = left7Sprite;
                break;
            case (8):
                moveRangeTiles[layer][startingLoc].sprite = left8Sprite;
                break;
            case (9):
                moveRangeTiles[layer][startingLoc].sprite = left9Sprite;
                break;
            case (10):
                moveRangeTiles[layer][startingLoc].sprite = left10Sprite;
                break;
            case (11):
                moveRangeTiles[layer][startingLoc].sprite = left11Sprite;
                break;
            case (12):
                moveRangeTiles[layer][startingLoc].sprite = left12Sprite;
                break;
            default:
                moveRangeTiles[layer][startingLoc].sprite = left13Sprite;
                break;
        }
        moveRangeTileMap[layer].RefreshTile(Vector3Int.RoundToInt(startingLoc));
    }

    public void DestroyPathTiles(int layer)
    {
        foreach (Vector2 loc in pathTiles[layer].Keys)
            pathTileMap[layer].SetTile(Vector3Int.RoundToInt(loc), null);
        pathTiles[layer] = new Dictionary<Vector2, Tile>();

        foreach (Vector2 loc in moveRangeTiles[layer].Keys)
            moveRangeTileMap[layer].SetTile(Vector3Int.RoundToInt(loc), null);
        moveRangeTiles[layer] = new Dictionary<Vector2, Tile>();
    }
}
