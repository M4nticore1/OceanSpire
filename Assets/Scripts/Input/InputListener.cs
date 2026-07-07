using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputListener : MonoBehaviour
{
    public static InputListener Instance { get; private set; }
    public event Action OnPressed;
    public event Action OnReleased;

    public GameObject startPressedObject { get; private set; }
    public Vector2 startPosition { get; private set; }
    public Vector2 lastPosition { get; private set; }

    private void Awake()
    {
        if (Instance) {
            Debug.LogWarning("Another InputListener is already in the scene!");
            Destroy(gameObject);
        }
        else {
            Instance = this;
        }
    }

    private void Update()
    {
        if (Pointer.current != null) {
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
        startPressedObject = PointerUtils.GetRaycastUIResult().gameObject;
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