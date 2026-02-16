using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    private bool isPrimaryInteractionPressed = false;
    private bool isSecondaryInteractionPressed = false;

    [SerializeField] private InputActionAsset mainInputActionsAsset = null;
    private InputActionMap touchInputActionMap = null;

    private InputAction mousePositionIA = null;

    private InputAction primaryInteractionPressIA = null;
    private InputAction primaryInteractionPositionIA = null;
    private InputAction primaryInteractionDeltaIA = null;
    private InputAction secondaryInteractionPressIA = null;
    private InputAction secondaryInteractionPositionIA = null;
    private InputAction secondaryInteractionDeltaIA = null;

    public InputAction cameraMoveKeyboardButtonIA { get; private set; } = null;
    public InputAction cameraMoveMouseButtonIA { get; private set; } = null;
    public InputAction cameraMoveTouchscreenButtonIA { get; private set; } = null;

    private InputAction cameraMoveKeyboardIA = null;
    private InputAction cameraMoveMouseIA = null;
    private InputAction cameraMoveTouchscreenIA = null;
    private InputAction cameraZoomIA = null;

    public bool isKeyboardMoveButtonPressed => cameraMoveKeyboardButtonIA.IsPressed();
    public bool isMouseMoveButtonPressed => cameraMoveMouseButtonIA.IsPressed();
    public bool isTouchscreenMoveButtonPressed => cameraMoveTouchscreenButtonIA.IsPressed();

    //private bool isMoving => isKeyboardMoveButtonPressed || isMouseMoveButtonPressed || isTouchscreenMoveButtonPressed;
    public Vector2 keyboardCameraMoveInput => cameraMoveKeyboardIA.ReadValue<Vector2>();
    public Vector2 mouseCameraMoveInput => cameraMoveMouseIA.ReadValue<Vector2>();
    public Vector2 touchscreenCameraMoveInput => cameraMoveTouchscreenIA.ReadValue<Vector2>();

    // Primary Interaction Delta
    private Vector2 primaryInteractionStartPosition = Vector2.zero;
    private Vector2 primaryInteractionPosition = Vector2.zero;
    private Vector2 primaryInteractionDelta => primaryInteractionDeltaIA.ReadValue<Vector2>();

    // Secondary Interaction Delta
    private Vector2 secondaryInteractionStartPosition = Vector2.zero;
    private Vector2 secondaryInteractionPosition = Vector2.zero;
    private Vector2 secondaryInteractionDelta = Vector2.zero;

    public event Action<Vector2> onPrimaryInteractionPressed; 
    public event Action<Vector2> onPrimaryInteractionReleased; 
    public event Action<Vector2> onSecondaryInteractionPressed; 
    public event Action<Vector2> onSecondaryInteractionReleased;
    public event Action<float> onCameraZoomPerformed;

    private void Awake()
    {
        SetInputSystem();
    }

    private void OnEnable()
    {
        touchInputActionMap.Enable();

        // Primary Interaction
        primaryInteractionPressIA.performed += OnPrimaryInteractionPressed;
        primaryInteractionPressIA.canceled += OnPrimaryInteractionReleased;

        // Secondary Interaction
        secondaryInteractionPressIA.performed += OnSecondaryTouchPressed;
        secondaryInteractionPressIA.canceled += OnSecondaryTouchReleased;

        secondaryInteractionPositionIA.performed += OnSecondaryInteractionPosition;
        secondaryInteractionPositionIA.canceled += OnSecondaryInteractionPosition;

        secondaryInteractionDeltaIA.performed += OnSecondaryInteractionDelta;
        secondaryInteractionDeltaIA.canceled += OnSecondaryInteractionDelta;

        // Camera
        cameraZoomIA.performed += OnCameraZoomPerformed;
    }

    private void OnDisable()
    {
        touchInputActionMap.Disable();

        // Primary Interaction
        primaryInteractionPressIA.performed -= OnPrimaryInteractionPressed;
        primaryInteractionPressIA.canceled -= OnPrimaryInteractionReleased;

        // Secondary Interaction
        secondaryInteractionPressIA.performed -= OnSecondaryTouchPressed;
        secondaryInteractionPressIA.canceled -= OnSecondaryTouchReleased;

        secondaryInteractionPositionIA.performed -= OnSecondaryInteractionPosition;
        secondaryInteractionPositionIA.canceled -= OnSecondaryInteractionPosition;

        secondaryInteractionDeltaIA.performed -= OnSecondaryInteractionDelta;
        secondaryInteractionDeltaIA.canceled -= OnSecondaryInteractionDelta;

        // Camera
        cameraZoomIA.performed -= OnCameraZoomPerformed;
    }

    private void SetInputSystem()
    {
        if (mainInputActionsAsset != null) {
            touchInputActionMap = mainInputActionsAsset.FindActionMap("Gameplay");

            if (touchInputActionMap != null) {
                mousePositionIA = touchInputActionMap.FindAction("MousePosition");

                primaryInteractionPressIA = touchInputActionMap.FindAction("PrimaryInteractionPress");
                primaryInteractionPositionIA = touchInputActionMap.FindAction("PrimaryInteractionPosition");
                primaryInteractionDeltaIA = touchInputActionMap.FindAction("PrimaryInteractionDelta");

                secondaryInteractionPressIA = touchInputActionMap.FindAction("SecondaryInteractionPress");
                secondaryInteractionPositionIA = touchInputActionMap.FindAction("SecondaryInteractionPosition");
                secondaryInteractionDeltaIA = touchInputActionMap.FindAction("SecondaryInteractionDelta");

                cameraMoveKeyboardIA = touchInputActionMap.FindAction("CameraMoveKeyboard");
                cameraMoveMouseIA = touchInputActionMap.FindAction("CameraMoveMouse");
                cameraMoveTouchscreenIA = touchInputActionMap.FindAction("CameraMoveTouchScreen");
                cameraZoomIA = touchInputActionMap.FindAction("CameraZoom");

                cameraMoveKeyboardButtonIA = touchInputActionMap.FindAction("CameraMoveKeyboardButton");
                cameraMoveMouseButtonIA = touchInputActionMap.FindAction("CameraMoveMouseButton");
                cameraMoveTouchscreenButtonIA = touchInputActionMap.FindAction("CameraMoveTouchscreenButton");
            }
            else
                Debug.Log("void PlayerController : SetInputSystem() touchInputActionMap is NULL");
        }
        else
            Debug.Log("void PlayerController : SetInputSystem() inputActions is NULL");
    }

    private void OnPrimaryInteractionPressed(InputAction.CallbackContext context)
    {
        var device = context.control.device;
        Vector2 position = device is Touchscreen ? primaryInteractionPositionIA.ReadValue<Vector2>() : mousePositionIA.ReadValue<Vector2>();

        isPrimaryInteractionPressed = true;
        onPrimaryInteractionPressed?.Invoke(position);
    }

    private void OnPrimaryInteractionReleased(InputAction.CallbackContext context)
    {
        var device = context.control.device;
        Vector2 position = device is Touchscreen ? primaryInteractionPositionIA.ReadValue<Vector2>() : mousePositionIA.ReadValue<Vector2>();

        isPrimaryInteractionPressed = false;
        onPrimaryInteractionReleased?.Invoke(position);
    }

    private void OnSecondaryTouchPressed(InputAction.CallbackContext context)
    {
        secondaryInteractionStartPosition = secondaryInteractionPositionIA.ReadValue<Vector2>();
        isSecondaryInteractionPressed = true;
    }

    private void OnSecondaryTouchReleased(InputAction.CallbackContext context)
    {
        secondaryInteractionPosition = Vector2.zero;
        secondaryInteractionDelta = Vector2.zero;
        isSecondaryInteractionPressed = false;
    }

    private void OnSecondaryInteractionPosition(InputAction.CallbackContext context)
    {
        Vector2 value = context.ReadValue<Vector2>();
        secondaryInteractionPosition = value;
    }

    private void OnSecondaryInteractionDelta(InputAction.CallbackContext context)
    {
        Vector2 value = context.ReadValue<Vector2>();
        secondaryInteractionDelta = value;
    }

    private void OnCameraZoomPerformed(InputAction.CallbackContext context)
    {
        float value = context.ReadValue<float>();
        onCameraZoomPerformed?.Invoke(value);
    }
}
