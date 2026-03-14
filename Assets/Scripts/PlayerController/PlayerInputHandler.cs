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

    public InputAction cameraMoveIA { get; private set; } = null;
    private InputAction cameraZoomIA = null;

    public Vector2 CameraMoveInput => cameraMoveIA.ReadValue<Vector2>();

    // Primary Interaction Delta
    public Vector2 primaryInteractionStartPosition { get; private set; } = Vector2.zero;
    public Vector2 primaryInteractionPosition => primaryTouchPositionIA.ReadValue<Vector2>();
    public Vector2 primaryInteractionDelta => primaryTouchDeltaIA.ReadValue<Vector2>();

    // Secondary Interaction Delta
    public Vector2 secondaryInteractionStartPosition { get; private set; } = Vector2.zero;
    public Vector2 secondaryInteractionPosition => secondaryTouchPositionIA.ReadValue<Vector2>();
    public Vector2 secondaryInteractionDelta => primaryTouchDeltaIA.ReadValue<Vector2>();

    public event Action onPrimaryInteractionPressed; 
    public event Action onPrimaryInteractionReleased;
    public event Action onCameraMovePerformed;
    public event Action onSecondaryInteractionPressed; 
    public event Action onSecondaryInteractionReleased;
    public event Action<float> onCameraZoomPerformed;

    private void Awake()
    {
        SetInputSystem();
    }

    private void OnEnable()
    {
        touchInputActionMap.Enable();

        // Primary Interaction
        primaryTouchPressIA.performed += OnPrimaryTouchPressed;
        primaryTouchPressIA.canceled += OnPrimaryTouchReleased;

        // Secondary Interaction
        secondaryTouchPressIA.performed += OnSecondaryTouchPressed;
        secondaryTouchPressIA.canceled += OnSecondaryTouchReleased;

        // Camera
        cameraMoveIA.performed += OnCameraMovePerformed;
        cameraZoomIA.performed += OnCameraZoomPerformed;
    }

    private void OnDisable()
    {
        touchInputActionMap.Disable();

        // Primary Interaction
        primaryTouchPressIA.performed -= OnPrimaryTouchPressed;
        primaryTouchPressIA.canceled -= OnPrimaryTouchReleased;

        // Secondary Interaction
        secondaryTouchPressIA.performed -= OnSecondaryTouchPressed;
        secondaryTouchPressIA.canceled -= OnSecondaryTouchReleased;

        // Camera
        cameraMoveIA.performed -= OnCameraMovePerformed;
        cameraZoomIA.performed -= OnCameraZoomPerformed;
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
                Debug.Log("void PlayerController : SetInputSystem() touchInputActionMap is NULL");
        }
        else
            Debug.Log("void PlayerController : SetInputSystem() inputActions is NULL");
    }

    // Primary Touch
    private void OnPrimaryTouchPressed(InputAction.CallbackContext context)
    {
        isPrimaryInteractionPressed = true;
        primaryInteractionStartPosition = primaryTouchPositionIA.ReadValue<Vector2>();
        onPrimaryInteractionPressed?.Invoke();
    }

    private void OnPrimaryTouchReleased(InputAction.CallbackContext context)
    {
        isPrimaryInteractionPressed = false;
        onPrimaryInteractionReleased?.Invoke();
    }

    // Secondary Touch
    private void OnSecondaryTouchPressed(InputAction.CallbackContext context)
    {
        isSecondaryInteractionPressed = true;
        secondaryInteractionStartPosition = secondaryTouchPositionIA.ReadValue<Vector2>();
        onSecondaryInteractionPressed?.Invoke();
    }

    private void OnSecondaryTouchReleased(InputAction.CallbackContext context)
    {
        isSecondaryInteractionPressed = false;
        onSecondaryInteractionReleased?.Invoke();
    }

    // Camera Move
    private void OnCameraMovePerformed(InputAction.CallbackContext context)
    {
        onCameraMovePerformed?.Invoke();
    }

    // Camera Zoom
    private void OnCameraZoomPerformed(InputAction.CallbackContext context)
    {
        float value = context.ReadValue<float>();
        onCameraZoomPerformed?.Invoke(value);
    }
}
