using UnityEngine;

public class FOVController : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float minFov = 60f;
    [SerializeField] private float maxFov = 90f;

    [Header("AspectRatio")]
    [SerializeField] private Vector2 firstScreenAspectRatio;
    [SerializeField] private Vector2 secondScreenAspectRatio;

    private float lastWidth = 0;
    private float lastHeight = 0;

    private void Update()
    {
        float width = Screen.width;
        float height = Screen.height;

        if (width == lastWidth && height == lastHeight) return;

        float currentAlpha = width / height;
        float firstAlpha = firstScreenAspectRatio.x / firstScreenAspectRatio.y;
        float secondAlpha = secondScreenAspectRatio.x / secondScreenAspectRatio.y;
        float alpha = (currentAlpha - firstAlpha) / (secondAlpha - firstAlpha);
        alpha = Mathf.Clamp01(alpha);

        float fov = Mathf.Lerp(minFov, maxFov, alpha);
        mainCamera.fieldOfView = fov;

        lastWidth = width;
        lastHeight = height;
    }
}