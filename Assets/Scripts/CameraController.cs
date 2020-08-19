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
}
