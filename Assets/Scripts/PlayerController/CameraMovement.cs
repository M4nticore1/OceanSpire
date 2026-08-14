using Unity.Mathematics;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private BuildingsManager buildingsManager;
    [SerializeField] private PlayerInputHandler playerInputHandler;

    private Vector2 startPressPosition = Vector2.zero;
    private Vector2 cameraMoveVelocity = Vector2.zero;

    [Header("Position")]
    [SerializeField] private float minBottomPosition = 5.0f;

    [Header("Padding")]
    [SerializeField] private float cameraTopBoundaryPadding = 10.0f;
    [SerializeField] private float cameraBottomBoundaryPadding = 5.0f;
    [SerializeField] private float cameraVerticalReturnSpeed = 5.0f;

    [Header("Speed")]
    [SerializeField] private float cameraMoveLerpSpeed = 10.0f;
    [SerializeField] private float cameraStopMoveSpeed = 6.0f;

    [Header("Dead zone")]
    [SerializeField] private float movingThreshold = 0.0005f;
    private bool inDeadZone = false;

    private void OnEnable()
    {
        playerInputHandler.OnPrimaryInteractionPressed += OnPrimaryInteractionPressed;
    }

    private void OnDisable()
    {
        playerInputHandler.OnPrimaryInteractionPressed -= OnPrimaryInteractionPressed;
    }

    private void Update()
    {
        if (ShouldMove()) {
            ApplyVelocity();
        }
        else {
            ReturnVelocity();
        }

        ReturnVerticalPosition();
        ApplyMove();
        ApplySquareMove();
    }

    public void Init(Quaternion rotation)
    {
        transform.rotation = rotation;
        ApplySquareMove();
    }

    private void ApplyVelocity()
    {
        cameraMoveVelocity = Vector2.Lerp(cameraMoveVelocity, new Vector2(playerInputHandler.CameraMoveInput.x, playerInputHandler.CameraMoveInput.y * GetVerticalMoveMultiplier()), cameraMoveLerpSpeed * Time.deltaTime);
    }

    private void ReturnVelocity()
    {
        cameraMoveVelocity = Vector2.Lerp(cameraMoveVelocity, Vector2.zero, cameraStopMoveSpeed * Time.deltaTime);
    }

    private float GetVerticalMoveMultiplier()
    {
        float multiplier = 1f;
        float cameraHeight = math.abs(transform.position.y);

        if (transform.position.y > buildingsManager.CurrentCityHeight && cameraMoveVelocity.y > 0f)
            multiplier = 1f - math.clamp((cameraHeight - buildingsManager.CurrentCityHeight) / cameraTopBoundaryPadding, 0f, 1f);
        else if (transform.position.y < minBottomPosition && cameraMoveVelocity.y < 0f)
            multiplier = 1f - math.clamp((minBottomPosition - transform.position.y) / cameraBottomBoundaryPadding, 0f, 1f);

        return multiplier;
    }

    private void ReturnVerticalPosition()
    {
        if (playerInputHandler.cameraMoveIA.IsPressed()) return;

        var cameraPosition = transform.position;

        float targetHeight = transform.position.y;
        if (transform.position.y > buildingsManager.CurrentCityHeight)
            targetHeight = buildingsManager.CurrentCityHeight;
        else if (transform.position.y < minBottomPosition)
            targetHeight = minBottomPosition;

        transform.position = math.lerp(transform.position, new Vector3(cameraPosition.x, targetHeight, cameraPosition.z), cameraVerticalReturnSpeed * Time.deltaTime);
    }

    private void ApplyMove()
    {
        transform.position += new Vector3(0, cameraMoveVelocity.y, 0) * Time.deltaTime;

        transform.position = new Vector3(
            transform.position.x,
            math.clamp(transform.position.y, minBottomPosition - cameraBottomBoundaryPadding, buildingsManager.CurrentCityHeight + cameraTopBoundaryPadding),
            transform.position.z
        );

        var eulers = transform.eulerAngles;
        eulers.y += cameraMoveVelocity.x * Time.deltaTime;
        transform.eulerAngles = eulers;
    }

    private void ApplySquareMove()
    {
        float alpha = 1f - transform.eulerAngles.y / 360f + 0.125f;
        Vector2 pos = GetSquareLoop(alpha, 16f, 0.5f);
        transform.position = new Vector3(pos.x, transform.position.y, pos.y);
    }

    private Vector2 GetSquareLoop(float t, float fullSize, float corner)
    {
        t = Mathf.Repeat(t, 1f);
        float halfSize = fullSize / 2;

        float seg = 1f / 4f;

        if (t < seg) { // Bottom to Right
            float k = t / seg;
            return new Vector2(math.lerp(-halfSize, halfSize, GetSquareSmooth(k, corner)), -halfSize);
        }
        else if (t < seg * 2f) { // Right to Top
            float k = (t - seg) / seg;
            return new Vector2(halfSize, math.lerp(-halfSize, halfSize, GetSquareSmooth(k, corner)));
        }
        else if (t < seg * 3f) { // Top to Left
            float k = (t - seg * 2f) / seg;
            return new Vector2(math.lerp(halfSize, -halfSize, GetSquareSmooth(k, corner)), halfSize);
        }
        else { // Left to Bottom
            float k = (t - seg * 3f) / seg;
            return new Vector2(-halfSize, math.lerp(halfSize, -halfSize, GetSquareSmooth(k, corner)));
        }
    }

    private float GetSquareSmooth(float x, float corner)
    {
        return Mathf.SmoothStep(0f, 1f, Mathf.Lerp(x, x * x * (3 - 2 * x), corner));
    }

    private bool ShouldMove()
    {
        if (InputStateManager.Instance.IsGameplayInputBlocked) return false;
        if (playerInputHandler.CameraMoveInput.sqrMagnitude <= 0) return false;

        if (inDeadZone) {
            if (CheckIfExitedDeadZone()) {
                inDeadZone = false;
            }
            else {
                return false;
            }
        }

        return true;
    }

    private bool CheckIfExitedDeadZone()
    {
        if (!playerInputHandler.isPrimaryInteractionPressed) return true;

        var delta = playerInputHandler.primaryInteractionPosition - startPressPosition;
        var normalizedDelta = new Vector2(delta.x / Screen.width, delta.y / Screen.height);

        return normalizedDelta.sqrMagnitude >= movingThreshold;
    }

    private void OnPrimaryInteractionPressed()
    {
        startPressPosition = playerInputHandler.primaryInteractionPosition;
        inDeadZone = true;
    }
}