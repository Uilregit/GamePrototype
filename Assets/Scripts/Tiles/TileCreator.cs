using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;

public class TileCreator : MonoBehaviour
{
    public static TileCreator tileCreator;

    public Sprite tileSprite;
    public Tilemap[] tileMap;
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

    private Dictionary<Vector2, Tile>[] tiles;
    private Dictionary<Vector2, int>[] tilePositions;
    private bool committed = false;

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
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void CreateTiles(GameObject newCreator, Vector2 startLocation, Card.CastShape shape, int range, Color color, int layer = 0)
    {
        CreateTiles(newCreator, startLocation, shape, range, color, new string[] { "None" }, layer);
    }

    public void CreateTiles(GameObject newCreator, Vector2 startLocation, Card.CastShape shape, int range, Color color, string[] avoidTag, int layer = 0)
    {
        if (!committed)
        {
            creator = newCreator;
            InstantiateTiles(startLocation, shape, range, color, avoidTag, layer);
            RefreshTiles(layer);
        }
    }

    //Destroys tiles in ALL layers
    public void DestryTiles(GameObject destroyer)
    {
        if (destroyer == creator) //Only allow the creator to destroy the tiles
        {
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
        }
    }

    private void InstantiateTiles(Vector2 startLocation, Card.CastShape shape, int range, Color color, string[] avoidTag, int layer)
    {
        if (range >= 0)
        {
            //if there is an object that needs to be avoided, don't create a tile here
            if (!Array.Exists(avoidTag, element => element == "None") && GridController.gridController.GetObjectAtLocation(startLocation) != null)
            {
                if (Array.Exists(avoidTag, element => element == GridController.gridController.GetObjectAtLocation(startLocation).tag))
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
                    tile.sprite = tileSprite;
                    tile.color = color;
                    tileMap[layer].SetTile(Vector3Int.RoundToInt(startLocation), tile);
                    int x = (int)startLocation.x;
                    int y = (int)startLocation.y;
                    InstantiateTiles(new Vector2(x - 1, y), shape, range - 1, color, avoidTag, layer);
                    InstantiateTiles(new Vector2(x + 1, y), shape, range - 1, color, avoidTag, layer);
                    InstantiateTiles(new Vector2(x, y - 1), shape, range - 1, color, avoidTag, layer);
                    InstantiateTiles(new Vector2(x, y + 1), shape, range - 1, color, avoidTag, layer);
                }
            }
            else if (shape == Card.CastShape.Plus)
            {
                for (int i = 1; i < range + 1; i++)
                {
                    Tile tile1 = ScriptableObject.CreateInstance<Tile>();
                    tiles[layer][startLocation + Vector2.right * i] = tile1;
                    tilePositions[layer][startLocation + Vector2.right * i] = range;
                    tile1.sprite = tileSprite;
                    tile1.color = color;
                    tileMap[layer].SetTile(Vector3Int.RoundToInt(startLocation + Vector2.right * i), tile1);

                    Tile tile2 = ScriptableObject.CreateInstance<Tile>();
                    tiles[layer][startLocation + Vector2.left * i] = tile2;
                    tilePositions[layer][startLocation + Vector2.left * i] = range;
                    tile2.sprite = tileSprite;
                    tile2.color = color;
                    tileMap[layer].SetTile(Vector3Int.RoundToInt(startLocation + Vector2.left * i), tile2);

                    Tile tile3 = ScriptableObject.CreateInstance<Tile>();
                    tiles[layer][startLocation + Vector2.up * i] = tile3;
                    tilePositions[layer][startLocation + Vector2.up * i] = range;
                    tile3.sprite = tileSprite;
                    tile3.color = color;
                    tileMap[layer].SetTile(Vector3Int.RoundToInt(startLocation + Vector2.up * i), tile3);

                    Tile tile4 = ScriptableObject.CreateInstance<Tile>();
                    tiles[layer][startLocation + Vector2.down * i] = tile4;
                    tilePositions[layer][startLocation + Vector2.down * i] = range;
                    tile4.sprite = tileSprite;
                    tile4.color = color;
                    tileMap[layer].SetTile(Vector3Int.RoundToInt(startLocation + Vector2.down * i), tile4);
                }
            }
        }
    }

    private void RefreshTiles(int layer)
    {
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
                tiles[layer][location].color = new Color(1, 1, 1, 0);
            else
                tiles[layer][location].sprite = allSprite;

            tileMap[layer].RefreshTile(Vector3Int.RoundToInt(location));
        }
    }

    public void SetCommitment(bool newCommitment)
    {
        committed = newCommitment;
    }

    public List<Vector2> GetTilePositions(int layer = 0)
    {
        List<Vector2> output = new List<Vector2>();
        foreach (Vector2 vec in tilePositions[layer].Keys)
            output.Add(vec);
        return output;
    }
}
