using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputListener : MonoBehaviour
{
    public static InputListener Instance { get; private set; }

    public GameObject startPressedObject { get; private set; }
    public Vector2 startPosition { get; private set; }
    public Vector2 lastPosition { get; private set; }

    public event Action OnPressed;
    public event Action OnReleased;

    private void Awake()
    {
        if (Instance != null) {
            Debug.LogError("Another InputListener is already in the scene!");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        if (Pointer.current != null && Pointer.current.press != null) {
            bool wasPressed = Pointer.current.press.wasPressedThisFrame;
            bool wasReleased = Pointer.current.press.wasReleasedThisFrame;

            if (wasPressed) {
                HandlePress();
            }

            if (wasReleased) {
                HandleRelease();
            }
        }
    }

    private void HandlePress()
    {
        var raycastResult = PointerUtils.GetRaycastUIResult();
        startPressedObject = raycastResult.isValid ? raycastResult.gameObject : null;

        startPosition = PointerUtils.GetCurrentInputPosition();

        OnPressed?.Invoke();
    }

    private void HandleRelease()
    {
        OnReleased?.Invoke();

        lastPosition = PointerUtils.GetCurrentInputPosition();
        startPressedObject = null;
    }
}