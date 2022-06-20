using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class RoomScrollController : MonoBehaviour
{
    public GameObject scrollBar;
    public GameObject scrollRoomIcon;

    public Color finihsedColor;
    public Color currentColor;
    public Color lockedColor;

    private int maxNumberOfRooms = 0;
    private int currentRoomNumber = 0;
    private Vector2 latestUnlockedRoom = Vector2.zero;
    private Dictionary<int, List<Vector2>> roomLocations = new Dictionary<int, List<Vector2>>();

    private List<GameObject> scrollRoomIcons = new List<GameObject>();

    protected Vector2 offset = Vector2.zero;
    protected Vector2 newLocation;
    private Vector3 desiredPosition = new Vector3(0, 0, 0);
    private Vector3 mouseDownPosition = Vector3.zero;

    private bool isHorizontal = true;
    private bool isDragging = false;

    private bool isZoomedIn = true;

    private void Update()
    {
        if (!isDragging && desiredPosition != transform.position)
        {
            if ((desiredPosition - transform.position).magnitude < 0.01)
                transform.position = desiredPosition;
            else
                transform.position = Vector3.Lerp(transform.position, desiredPosition, 0.3f);
        }
    }

    public void OnMouseDown()
    {
        isDragging = true;
        offset = transform.position - CameraController.camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
        mouseDownPosition = CameraController.camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
    }

    public void OnMouseDrag()
    {
        newLocation = offset + (Vector2)CameraController.camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
        if (isHorizontal)
        {
            transform.position = new Vector3(newLocation.x, 0, 0);
            SetCurrentRoom((int)Mathf.Round(transform.position.x / -3.5f / transform.localScale.x));
        }
        else
        {
            transform.position = newLocation;
            SetCurrentRoom((int)Mathf.Round(transform.position.y / -6f / transform.localScale.x));
        }
    }
    public void OnMouseUp()
    {
        isDragging = false;
        if (isHorizontal)
        {
            /*
            if (CameraController.camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0)).x - mouseDownPosition.x > 0.5)
                desiredPosition = new Vector3(x, 0, 0);
            else if (CameraController.camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0)).x - mouseDownPosition.x < - 0.5)
                desiredPosition = new Vector3(x, 0, 0);
            */
            float x = Mathf.Clamp(Mathf.Round(transform.position.x / -3.5f / transform.localScale.x), 0, maxNumberOfRooms - 1) * -3.5f * transform.localScale.x;
            desiredPosition = new Vector3(x, 0, 0);
        }
        else
        {
            List<float> previousLayerX = new List<float>();
            int previousLayer = (int)Mathf.Clamp(Mathf.Round(transform.position.y / -6f / transform.localScale.x), 0, maxNumberOfRooms - 1) - 1;
            foreach (Vector2 loc in roomLocations[Mathf.Max(previousLayer, 0)])
                previousLayerX.Add(loc.x);
            float x = RoundToNearestNumber(transform.position.x / -3.5f / transform.localScale.x, previousLayerX) * -3.5f * transform.localScale.x;
            float y = Mathf.Clamp(Mathf.Round(transform.position.y / -6f / transform.localScale.x), 0, maxNumberOfRooms - 1) * -6f * transform.localScale.x;
            if (previousLayer == roomLocations.Keys.Max())
                x = 0;
            desiredPosition = new Vector3(x, y, 0);
        }
    }

    public void SetNumOfRooms(int value, bool horizontal)
    {
        maxNumberOfRooms = value;
        isHorizontal = horizontal;
        scrollRoomIcons = new List<GameObject>();
        for (int i = 0; i < value; i++)
        {
            GameObject temp = Instantiate(scrollRoomIcon);
            temp.transform.SetParent(scrollBar.transform);
            if (isHorizontal)
                temp.transform.localPosition = new Vector3((-(value - 1) / 2f + i) * 0.5f, -2.99f, 0);
            else
                temp.transform.localPosition = new Vector3(2f, (-(value - 1) / 2f + i) * 0.5f, 0);
            scrollRoomIcons.Add(temp);
        }
        SetCurrentRoom(0);
    }

    public void SetLatestUnlockedRoom(Vector2 value)
    {
        ResetScrollProgress((int)value.y + 1);

        if (isHorizontal)
            desiredPosition = new Vector3((int)(value.y + 1) * -3.5f * transform.localScale.x, 0, 0);
        else
            desiredPosition = new Vector3(-value.x * transform.localScale.x, ((int)value.y + 1) * -6f * transform.localScale.x, 0);

        transform.localPosition = desiredPosition;

        SetCurrentRoom((int)value.y + 1);
    }

    public void SetCurrentRoom(int value)
    {
        currentRoomNumber = value;

        for (int i = 0; i < scrollRoomIcons.Count; i++)
        {
            if (i == currentRoomNumber)
                scrollRoomIcons[i].transform.localScale = new Vector2(1.5f, 1.5f);
            else
                scrollRoomIcons[i].transform.localScale = new Vector2(1f, 1f);
        }
    }

    public void ResetScrollProgress(int value)
    {
        for (int i = 0; i < scrollRoomIcons.Count; i++)
        {
            if (i < value)
                scrollRoomIcons[i].GetComponent<Image>().color = finihsedColor;
            else if (i == value)
                scrollRoomIcons[i].GetComponent<Image>().color = currentColor;
            else
                scrollRoomIcons[i].GetComponent<Image>().color = lockedColor;
        }
    }


    public void Zoom()
    {
        if (isZoomedIn)
        {
            transform.localScale = new Vector2(0.4f, 0.4f);
            desiredPosition *= 0.4f;
            transform.position *= 0.4f;
        }
        else
        {
            transform.localScale = new Vector2(1, 1);
            desiredPosition *= 2.5f;
            transform.position *= 2.5f;
        }
        isZoomedIn = !isZoomedIn;
    }

    public void SetRoomLocations(List<Vector2> roomLocs)
    {
        roomLocations = new Dictionary<int, List<Vector2>>();
        foreach (Vector2 loc in roomLocs)
            if (roomLocations.ContainsKey((int)loc.y))
                roomLocations[(int)loc.y].Add(loc);
            else
                roomLocations[(int)loc.y] = new List<Vector2> { loc };
    }

    private float RoundToNearestNumber(float value, List<float> targets)
    {
        float output = value;
        float minDist = 999999999999999999;
        foreach (float target in targets)
            if (Mathf.Abs(value - target) < minDist)
            {
                minDist = Mathf.Abs(value - target);
                output = target;
            }

        return output;
    }

    public bool GetIsHorizontal()
    {
        return isHorizontal;
    }
}
