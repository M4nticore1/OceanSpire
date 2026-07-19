using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class CameraZoomHandler : MonoBehaviour
{
    [SerializeField] private InputStateManager inputStateManager;
    [SerializeField] private PlayerInputHandler inputHandler;

    private float currentArmLength = 0f;
    [SerializeField] private float minCameraArmLength = 10f;
    [SerializeField] private float maxCameraArmLength = 200f;

    private const float nearArmBoundaryPadding = 10f;
    private const float farArmBoundaryPadding = 20f;
    private const float cameraArmReturnSpeed = 4f;

    private float cameraArmMoveMultiplier = 1f;

    private float zoomVelocity = 0f;
    private const float pitchSensitivity = 4f;
    private const float stopZoomingSpeed = 12f;

    private float lastPitch = 0;

    private void OnEnable()
    {
        inputHandler.onCameraZoomPerformed += OnCameraZoomPerformed;
    }

    private void OnDisable()
    {
        inputHandler.onCameraZoomPerformed -= OnCameraZoomPerformed;
    }

    private void Start()
    {
        currentArmLength = -transform.localPosition.z;
    }

    private void Update()
    {
        if (ShouldMove()) {
            ProcessTouchscreenZoom();
        }
        else {
            ResetPitch();
            ProcessStopZooming();
            ProcessPadding();
        }

        ApplyZoom();
    }

    private void AddZoomVelocity(float value)
    {
        if (inputStateManager.isGameplayInputBlocked) return;

        float multiplier = 1f;

        if (currentArmLength <= minCameraArmLength + nearArmBoundaryPadding && value > 0) {
            multiplier = math.clamp((currentArmLength - minCameraArmLength) / nearArmBoundaryPadding, 0f, 1f);
        }
        else if (currentArmLength >= maxCameraArmLength - farArmBoundaryPadding && value < 0) {
            multiplier = math.clamp((maxCameraArmLength - currentArmLength) / farArmBoundaryPadding, 0f, 1f);
        }

        zoomVelocity = value * multiplier;
    }

    private void ProcessTouchscreenZoom()
    {
        if (Touchscreen.current == null) return;

        TouchControl primaryTouch = Touchscreen.current.touches[0];
        TouchControl secondaryTouch = Touchscreen.current.touches[1];

        if (!primaryTouch.press.isPressed || !secondaryTouch.press.isPressed) {
            ResetPitch();
            return;
        }

        float currentDistance = Vector2.Distance(primaryTouch.position.ReadValue(), secondaryTouch.position.ReadValue());

        if (lastPitch == 0) {
            lastPitch = currentDistance;
            return;
        }

        float delta = currentDistance - lastPitch;
        AddZoomVelocity(delta * pitchSensitivity);
        lastPitch = currentDistance;
    }

    private void ProcessStopZooming()
    {
        zoomVelocity = math.lerp(zoomVelocity, 0, stopZoomingSpeed * Time.deltaTime);
        //zoomVelocity = math.clamp(zoomVelocity, 0, zoomVelocity);
    }

    private void ProcessPadding()
    {
        if (currentArmLength > minCameraArmLength + nearArmBoundaryPadding && currentArmLength < maxCameraArmLength - farArmBoundaryPadding) return;

        float targetArmLength = math.abs(minCameraArmLength - currentArmLength) < math.abs(maxCameraArmLength - currentArmLength) ? minCameraArmLength + nearArmBoundaryPadding : maxCameraArmLength - farArmBoundaryPadding;

        currentArmLength = math.lerp(currentArmLength, targetArmLength, cameraArmReturnSpeed * Time.deltaTime);
    }

    private void ApplyZoom()
    {
        currentArmLength -= zoomVelocity * Time.deltaTime;
        currentArmLength = math.clamp(currentArmLength, minCameraArmLength, maxCameraArmLength);
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, -currentArmLength);
    }

    private void ResetPitch()
    {
        lastPitch = 0;
    }

    private void OnCameraZoomPerformed(float value)
    {
        AddZoomVelocity(value);
    }

    private bool ShouldMove()
    {
        if (!inputHandler.isPrimaryInteractionPressed) return false;
        if (!inputHandler.isSecondaryInteractionPressed) return false;
        if (inputStateManager.isGameplayInputBlocked) return false;

        return true;
    }
}
