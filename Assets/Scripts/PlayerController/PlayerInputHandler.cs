using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    [SerializeField] private InputActionAsset mainInputActionsAsset = null;
    private InputActionMap touchInputActionMap = null;

    public bool isPrimaryInteractionPressed { get; private set; } = false;
    public bool isSecondaryInteractionPressed { get; private set; } = false;

    private InputAction primaryTouchPressIA = null;
    private InputAction primaryTouchPositionIA = null;
    private InputAction primaryTouchDeltaIA = null;
    private InputAction secondaryTouchPressIA = null;
    private InputAction secondaryTouchPositionIA = null;
    private InputAction secondaryTouchDeltaIA = null;

    public InputAction cameraMoveIA { get; private set; }
    public InputAction cameraZoomIA { get; private set; }

    public Vector2 CameraMoveInput => cameraMoveIA.ReadValue<Vector2>();
    public float CameraZoomInput => cameraZoomIA.ReadValue<float>();

    // Primary Interaction Delta
    public Vector2 primaryInteractionStartPosition { get; private set; } = Vector2.zero;
    public Vector2 primaryInteractionPosition => primaryTouchPositionIA.ReadValue<Vector2>();
    public Vector2 primaryInteractionDelta => primaryTouchDeltaIA.ReadValue<Vector2>();

    // Secondary Interaction Delta
    public Vector2 secondaryInteractionStartPosition { get; private set; } = Vector2.zero;
    public Vector2 secondaryInteractionPosition => secondaryTouchPositionIA.ReadValue<Vector2>();
    public Vector2 secondaryInteractionDelta => primaryTouchDeltaIA.ReadValue<Vector2>();

    public event Action OnPrimaryInteractionPressed; 
    public event Action OnPrimaryInteractionReleased;
    public event Action OnCameraMovePerformed;
    public event Action OnSecondaryInteractionPressed; 
    public event Action OnSecondaryInteractionReleased;
    public event Action<float> OnCameraZoomPerformed;

    private void Awake()
    {
        SetInputSystem();
    }

    private void OnEnable()
    {
        touchInputActionMap.Enable();

        // Primary Interaction
        primaryTouchPressIA.performed += HandlePrimaryTouchPressed;
        primaryTouchPressIA.canceled += HandlePrimaryTouchReleased;

        // Secondary Interaction
        secondaryTouchPressIA.performed += HandleSecondaryTouchPressed;
        secondaryTouchPressIA.canceled += HandleSecondaryTouchReleased;

        // Camera
        cameraMoveIA.performed += HandleCameraMovePerformed;
        cameraZoomIA.performed += HandleCameraZoomPerformed;
    }

    private void OnDisable()
    {
        touchInputActionMap.Disable();

        // Primary Interaction
        primaryTouchPressIA.performed -= HandlePrimaryTouchPressed;
        primaryTouchPressIA.canceled -= HandlePrimaryTouchReleased;

        // Secondary Interaction
        secondaryTouchPressIA.performed -= HandleSecondaryTouchPressed;
        secondaryTouchPressIA.canceled -= HandleSecondaryTouchReleased;

        // Camera
        cameraMoveIA.performed -= HandleCameraMovePerformed;
        cameraZoomIA.performed -= HandleCameraZoomPerformed;
    }

    private void SetInputSystem()
    {
        if (mainInputActionsAsset != null) {
            touchInputActionMap = mainInputActionsAsset.FindActionMap("Gameplay");

            if (touchInputActionMap != null) {
                primaryTouchPressIA = touchInputActionMap.FindAction("PrimaryInteractionPress");
                primaryTouchPositionIA = touchInputActionMap.FindAction("PrimaryInteractionPosition");
                primaryTouchDeltaIA = touchInputActionMap.FindAction("PrimaryInteractionDelta");

                secondaryTouchPressIA = touchInputActionMap.FindAction("SecondaryInteractionPress");
                secondaryTouchPositionIA = touchInputActionMap.FindAction("SecondaryInteractionPosition");
                secondaryTouchDeltaIA = touchInputActionMap.FindAction("SecondaryInteractionDelta");

                cameraZoomIA = touchInputActionMap.FindAction("CameraZoom");
                cameraMoveIA = touchInputActionMap.FindAction("CameraMove");
            }
            else
                Debug.LogError("void PlayerController : SetInputSystem() touchInputActionMap is NULL");
        }
        else
            Debug.LogError("void PlayerController : SetInputSystem() inputActions is NULL");
    }

    // Primary Touch
    private void HandlePrimaryTouchPressed(InputAction.CallbackContext context)
    {
        isPrimaryInteractionPressed = true;
        primaryInteractionStartPosition = primaryTouchPositionIA.ReadValue<Vector2>();
        OnPrimaryInteractionPressed?.Invoke();
    }

    private void HandlePrimaryTouchReleased(InputAction.CallbackContext context)
    {
        isPrimaryInteractionPressed = false;
        OnPrimaryInteractionReleased?.Invoke();
    }

    // Secondary Touch
    private void HandleSecondaryTouchPressed(InputAction.CallbackContext context)
    {
        isSecondaryInteractionPressed = true;
        secondaryInteractionStartPosition = secondaryTouchPositionIA.ReadValue<Vector2>();
        OnSecondaryInteractionPressed?.Invoke();
    }

    private void HandleSecondaryTouchReleased(InputAction.CallbackContext context)
    {
        isSecondaryInteractionPressed = false;
        OnSecondaryInteractionReleased?.Invoke();
    }

    // Camera Move
    private void HandleCameraMovePerformed(InputAction.CallbackContext context)
    {
        OnCameraMovePerformed?.Invoke();
    }

    // Camera Zoom
    private void HandleCameraZoomPerformed(InputAction.CallbackContext context)
    {
        float value = context.ReadValue<float>();
        OnCameraZoomPerformed?.Invoke(value);
    }
}
