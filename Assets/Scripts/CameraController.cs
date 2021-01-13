using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraController : MonoBehaviour
{
    public static CameraController camera;

    public float screenShakeMultiplier = 1;

    private bool isShaking;
    private float strength = 0;
    private float duration = 0;
    private float shakingStartTime;
    private Vector3 originalLocation;

    // Start is called before the first frame update
    void Awake()
    {
        if (CameraController.camera == null)
            CameraController.camera = this;
        else
            Destroy(this.gameObject);

        try
        {
            Camera.main.backgroundColor = RoomController.roomController.GetCurrentWorldSetup().cameraBackground;
        }
        catch { }

        screenShakeMultiplier = SettingsController.settings.GetScreenShakeMultiplier();

        originalLocation = transform.position;
        shakingStartTime = Time.time;
    }

    private void FixedUpdate()
    {
        if (isShaking)
        {
            transform.position = originalLocation + Random.insideUnitSphere * strength;
            if (Time.time - shakingStartTime > duration)
            {
                isShaking = false;
                duration = 0;
                strength = 0;
                transform.position = originalLocation;
            }
        }
    }

    public Vector3 ScreenToWorldPoint(Vector3 screenPosition)
    {
        return ScreenToWorldPoint(screenPosition, Vector3.forward, new Vector3(0, 0, 0));
    }

    public Vector3 ScreenToWorldPoint(Vector3 screenPosition, Vector3 normal, Vector3 plane)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        Plane xy = new Plane(normal, plane);
        float distance;
        xy.Raycast(ray, out distance);
        return ray.GetPoint(distance);
    }

    public void ScreenShake(float newStrength, float newDuration, bool overrideMultiplier = false)
    {
        isShaking = true;
        if (newDuration > duration - (Time.time - shakingStartTime))
            shakingStartTime = Time.time;
        if (SystemInfo.deviceType == DeviceType.Handheld)
            strength = Mathf.Max(strength, newStrength * 2);
        else
            strength = Mathf.Max(strength, newStrength);

        if (!overrideMultiplier)
            strength *= screenShakeMultiplier;
        duration = Mathf.Max(duration, newDuration);
    }
}
