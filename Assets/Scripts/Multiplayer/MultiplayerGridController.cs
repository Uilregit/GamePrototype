using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Mirror;

public class MultiplayerGridController : NetworkBehaviour
{
    private List<string>[,] grid = new List<string>[0, 0];
    private int xSize, ySize;

    public void SetSize(int xValue, int yValue)
    {
        xSize = xValue;
        ySize = yValue;
        grid = new List<string>[xSize, ySize];
        for (int x = 0; x < xSize; x++)
            for (int y = 0; y < ySize; y++)
                grid[x, y] = new List<string>();
    }

    //Called only from Gridcontroller
    public void SetGrid(List<GameObject>[,] value)
    {
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                grid[x, y] = new List<string>();
                List<string> ids = new List<string>();
                foreach (GameObject obj in value[x, y])
                    ids.Add(obj.GetComponent<NetworkIdentity>().netId.ToString());
                grid[x, y].AddRange(ids);
            }
        }

        //DebugGrid();
    }

    //Called only from MultiplayerInformationController
    public void SetGrid(byte[] input)
    {
        MemoryStream stream = new MemoryStream();
        stream.Write(input, 0, input.Length);
        stream.Seek(0, SeekOrigin.Begin);
        BinaryFormatter formatter = new BinaryFormatter();
        List<string>[] value = (List<string>[])formatter.Deserialize(stream);

        List<GameObject>[,] output = new List<GameObject>[xSize, ySize];

        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                output[x, y] = new List<GameObject>();
                foreach (string id in value[x * xSize + y])
                    output[x, y].Add(ClientScene.localPlayer.GetComponent<MultiplayerInformationController>().GetObjectFromNetID(id));
            }
        }

        GetComponent<GridController>().SetGrid(output);

        //DebugGrid();
    }

    public byte[] GetGrid()
    {
        MemoryStream stream = new MemoryStream();
        BinaryFormatter formatter = new BinaryFormatter();

        List<string>[] output = new List<string>[xSize * ySize];
        for (int x = 0; x < xSize; x++)
            for (int y = 0; y < ySize; y++)
                output[x * xSize + y] = grid[x, y];

        formatter.Serialize(stream, output);

        byte[] final = stream.ToArray();
        return final;
    }

    public string DebugGrid()
    {
        //DebugPlus.LogOnScreen("########").Duration(10);
        string s = "";
        for (int y = ySize - 1; y >= 0; y--)
        {
            for (int x = 0; x < xSize; x++)
                s += grid[x, y].Count;
            s += "\n";
            //DebugPlus.LogOnScreen(s).Duration(10);
        }
        Debug.Log(s);
        return s;
    }
}
