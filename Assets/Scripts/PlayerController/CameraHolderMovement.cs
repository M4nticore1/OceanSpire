using Unity.Mathematics;
using UnityEngine;

public class CameraHolderMovement : MonoBehaviour
{
    [SerializeField] private PlayerInputHandler playerInputHandler;

    // Move Velocity
    private Vector2 cameraMoveVelocity => keyboardCameraMoveVelocity * keyboardCameraMoveSensitivity + mouseCameraMoveVelocity * mouseCameraMoveSensitivity + touchscreenCameraMoveVelocity * touchscreenCameraMoveSensitivity;
    private Vector2 keyboardCameraMoveVelocity;
    private Vector2 mouseCameraMoveVelocity;
    private Vector2 touchscreenCameraMoveVelocity;

    // Return
    private const float cameraVerticalBoundaryPadding = 10.0f;
    private const float cameraVerticalReturnSpeed = 5.0f;

    // Sensitivities
    private const float keyboardCameraMoveSensitivity = 100f;
    private const float mouseCameraMoveSensitivity = 10f;
    private const float touchscreenCameraMoveSensitivity = 4f;

    // Stop Moving
    private const float cameraStopMoveSpeed = 6.0f;

    private void Update()
    {
        ProcessCameraMove();
    }

    private void ProcessCameraMove()
    {
        // Apply Velocity
        ApplyMoveVelocity(ref keyboardCameraMoveVelocity, playerInputHandler.isKeyboardMoveButtonPressed, playerInputHandler.keyboardCameraMoveInput);
        ApplyMoveVelocity(ref mouseCameraMoveVelocity, playerInputHandler.isMouseMoveButtonPressed, playerInputHandler.mouseCameraMoveInput);
        ApplyMoveVelocity(ref touchscreenCameraMoveVelocity, playerInputHandler.isTouchscreenMoveButtonPressed, playerInputHandler.touchscreenCameraMoveInput);

        // Move
        ReturnVerticalPosition();
        ApplyMove();
        ApplySquareMove();
    }

    // Apply Velocity
    private void ApplyMoveVelocity(ref Vector2 velocity, bool isPressed, Vector2 input)
    {
        velocity = isPressed ? input : Vector2.Lerp(velocity, Vector2.zero, cameraStopMoveSpeed * Time.deltaTime);
    }

    // Vertical Move
    private float GetVerticalMoveMultiplier()
    {
        float multiplier = 1f;
        float cameraHeight = math.abs(transform.position.y);
        if (transform.position.y > CityManager.Instance.currentCityHeight && cameraMoveVelocity.y > 0f)
            multiplier = 1f - math.clamp((cameraHeight - CityManager.Instance.currentCityHeight) / cameraVerticalBoundaryPadding, 0f, 1f);
        else if (transform.position.y < 0f && cameraMoveVelocity.y < 0f)
            multiplier = 1f - math.clamp(cameraHeight / cameraVerticalBoundaryPadding, 0f, 1f);

        return multiplier;
    }

    // Return Vertical Position
    private void ReturnVerticalPosition()
    {
        Vector3 cameraPosition = transform.position;
        float targetHeight = transform.position.y > CityManager.Instance.currentCityHeight ? CityManager.Instance.currentCityHeight : transform.position.y < 0f ? 0f : transform.position.y;

        transform.position = math.lerp(transform.position, new Vector3(cameraPosition.x, targetHeight, cameraPosition.z), cameraVerticalReturnSpeed * Time.deltaTime);
    }

    private void ApplyMove()
    {
        transform.position += new Vector3(0, cameraMoveVelocity.y, 0) * GetVerticalMoveMultiplier() * Time.deltaTime;
        Vector3 eulers = transform.eulerAngles;
        eulers.y += cameraMoveVelocity.x * Time.deltaTime;
        transform.eulerAngles = eulers;
    }

    private void ApplySquareMove()
    {
        float alpha = 1f - transform.eulerAngles.y / 360f + 0.125f;
        Vector2 pos = GetSquareLoop(alpha, 16f, 0.5f);
        transform.position = new Vector3(pos.x, transform.position.y, pos.y);
    }

    // Square Move
    private Vector2 GetSquareLoop(float t, float fullSize, float corner)
    {
        t = Mathf.Repeat(t, 1f);
        float halfSize = fullSize / 2;

        float seg = 1f / 4f;

        if (t < seg) {  // Bottom → Right
            float k = t / seg;
            return new Vector2(math.lerp(-halfSize, halfSize, GetSquareSmooth(k, corner)), -halfSize);
        }
        else if (t < seg * 2f) { // Right → Top
            float k = (t - seg) / seg;
            return new Vector2(halfSize, math.lerp(-halfSize, halfSize, GetSquareSmooth(k, corner)));
        }
        else if (t < seg * 3f) { // Top → Left
            float k = (t - seg * 2f) / seg;
            return new Vector2(math.lerp(halfSize, -halfSize, GetSquareSmooth(k, corner)), halfSize);
        }
        else { // Left → Bottom
            float k = (t - seg * 3f) / seg;
            return new Vector2(-halfSize, math.lerp(halfSize, -halfSize, GetSquareSmooth(k, corner)));
        }
    }

    private float GetSquareSmooth(float x, float corner)
    {
        return Mathf.SmoothStep(0f, 1f, Mathf.Lerp(x, x * x * (3 - 2 * x), corner));
    }
}