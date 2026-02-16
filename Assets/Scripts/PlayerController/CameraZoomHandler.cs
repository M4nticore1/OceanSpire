using Unity.Mathematics;
using UnityEngine;

public class CameraZoomHandler : MonoBehaviour
{
    [SerializeField] private PlayerInputHandler playerInputHandler;

    private float currentCameraArmLength = 0.0f;
    private const float minCameraArmLength = 25.0f;
    private const float maxCameraArmLength = 100.0f;

    private const float nearCameraArmBoundaryPadding = 10.0f;
    private const float farCameraArmBoundaryPadding = 20.0f;
    private const float cameraArmReturnSpeed = 4.0f;

    private float cameraArmMoveMultiplier = 1.0f;

    private float zoomVelocity = 0.0f;
    private const float zoomSensitivity = 0.1f;
    private const float stopZoomingSpeed = 25.0f;
    private float currentCameraZoomVelocity = 0f;
    private const float zoomForce = 6f;

    public void ProcessZoom()
    {
        if (currentCameraArmLength > maxCameraArmLength && zoomVelocity < 0)
            cameraArmMoveMultiplier = 1.0f - ((currentCameraArmLength - maxCameraArmLength) / farCameraArmBoundaryPadding);
        else if (currentCameraArmLength < minCameraArmLength && zoomVelocity > 0)
            cameraArmMoveMultiplier = 1.0f - (math.abs(minCameraArmLength - currentCameraArmLength) / nearCameraArmBoundaryPadding);
        else
            cameraArmMoveMultiplier = 1;

        cameraArmMoveMultiplier = math.clamp(cameraArmMoveMultiplier, 0, 1);
        currentCameraArmLength -= zoomVelocity * zoomSensitivity * cameraArmMoveMultiplier;
        currentCameraArmLength = math.clamp(currentCameraArmLength, minCameraArmLength - nearCameraArmBoundaryPadding, maxCameraArmLength + farCameraArmBoundaryPadding);
    }

    public void ProcessStopZooming()
    {
        zoomVelocity = math.lerp(zoomVelocity, 0, stopZoomingSpeed * Time.deltaTime);
        currentCameraArmLength -= zoomVelocity * zoomSensitivity * cameraArmMoveMultiplier;

        float targetLength = currentCameraArmLength > maxCameraArmLength ? maxCameraArmLength : currentCameraArmLength < minCameraArmLength ? minCameraArmLength : currentCameraArmLength;
        if (currentCameraArmLength != targetLength) {
            currentCameraArmLength = math.lerp(currentCameraArmLength, targetLength, cameraArmReturnSpeed * Time.deltaTime);
        }
    }

    public void AddZoomVelocity(Vector3 value)
    {
        float multiplier = 1f;

        if (currentCameraArmLength < minCameraArmLength && currentCameraZoomVelocity > 0)
            multiplier = 1f - math.clamp((minCameraArmLength - currentCameraArmLength) / nearCameraArmBoundaryPadding, 0f, 1f);
        else if (currentCameraArmLength > maxCameraArmLength && currentCameraZoomVelocity < 0)
            multiplier = 1f - math.clamp((currentCameraArmLength - maxCameraArmLength) / farCameraArmBoundaryPadding, 0f, 1f);

        currentCameraArmLength -= zoomForce * currentCameraZoomVelocity * multiplier;
    }

    private void ApplyZoom()
    {
        Vector3 position = transform.localPosition + new Vector3(0, 0, currentCameraArmLength);
        transform.localPosition = position;
    }

    private void ResetZoom()
    {
        Vector3 position = transform.localPosition - new Vector3(0, 0, currentCameraArmLength);
        transform.localPosition = position;
    }
}
