using Unity.Mathematics;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private InputStateManager inputStateManager;
    [SerializeField] private BuildingsManager buildingsManager;
    [SerializeField] private PlayerInputHandler playerInputHandler;

    private Vector2 cameraMoveVelocity = Vector2.zero;

    // Return
    private const float cameraVerticalBoundaryPadding = 10.0f;
    private const float cameraVerticalReturnSpeed = 5.0f;

    // Stop Moving
    private const float cameraStopMoveSpeed = 6.0f;

    public void Tick()
    {
        if (playerInputHandler.CameraMoveInput.sqrMagnitude > 0 && !inputStateManager.isGameplayInputBlocked) {
            ApplyVelocity();
        }
        else {
            ReturnVelocity();
        }
        ReturnVerticalPosition();
        ApplyMove();
        ApplySquareMove();
    }

    // Velocity
    private void ApplyVelocity()
    {
        cameraMoveVelocity = playerInputHandler.CameraMoveInput * GetVerticalMoveMultiplier();
    }

    private void ReturnVelocity()
    {
        cameraMoveVelocity = Vector2.Lerp(cameraMoveVelocity, Vector2.zero, cameraStopMoveSpeed * Time.deltaTime);
    }

    private float GetVerticalMoveMultiplier()
    {
        float multiplier = 1f;
        float cameraHeight = math.abs(transform.position.y);
        if (transform.position.y > buildingsManager.currentCityHeight && cameraMoveVelocity.y > 0f)
            multiplier = 1f - math.clamp((cameraHeight - buildingsManager.currentCityHeight) / cameraVerticalBoundaryPadding, 0f, 1f);
        else if (transform.position.y < 0f && cameraMoveVelocity.y < 0f)
            multiplier = 1f - math.clamp(cameraHeight / cameraVerticalBoundaryPadding, 0f, 1f);

        return multiplier;
    }

    // Return Vertical Position
    private void ReturnVerticalPosition()
    {
        if (playerInputHandler.cameraMoveIA.IsPressed()) return;

        Vector3 cameraPosition = transform.position;
        float targetHeight = transform.position.y > buildingsManager.currentCityHeight ? buildingsManager.currentCityHeight : transform.position.y < 0f ? 0f : transform.position.y;

        transform.position = math.lerp(transform.position, new Vector3(cameraPosition.x, targetHeight, cameraPosition.z), cameraVerticalReturnSpeed * Time.deltaTime);
    }

    private void ApplyMove()
    {
        transform.position += new Vector3(0, cameraMoveVelocity.y, 0) * Time.deltaTime;
        transform.position = new Vector3(transform.position.x, math.clamp(transform.position.y, -cameraVerticalBoundaryPadding, buildingsManager.currentCityHeight + cameraVerticalBoundaryPadding), transform.position.z);

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