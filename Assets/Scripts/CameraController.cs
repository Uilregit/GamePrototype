using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraController : MonoBehaviour
{
    public static CameraController camera;

    // Start is called before the first frame update
    void Awake()
    {
        if (CameraController.camera == null)
            CameraController.camera = this;
        else
            Destroy(this.gameObject);
    }

    public Vector3 ScreenToWorldPoint(Vector3 screenPosition)
    {
        return ScreenToWorldPoint(screenPosition, new Vector3(0, 0, 0));
    }

    public Vector3 ScreenToWorldPoint(Vector3 screenPosition, Vector3 plane)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        Plane xy = new Plane(Vector3.forward, plane);
        float distance;
        xy.Raycast(ray, out distance);
        return ray.GetPoint(distance);
    }
}
