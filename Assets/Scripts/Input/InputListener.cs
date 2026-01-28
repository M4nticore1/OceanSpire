using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputListener : MonoBehaviour
{
    public static InputListener Instance { get; private set; }
    public event Action onPressed;
    public event Action onReleased;

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
        if (Touchscreen.current != null) {
            bool wasPressed = Touchscreen.current.primaryTouch.press.wasPressedThisFrame;
            bool wasReleased = Touchscreen.current.primaryTouch.press.wasReleasedThisFrame;
            if (wasPressed)
                onPressed?.Invoke();
            if (wasReleased)
                onReleased?.Invoke();
        }
        if (Mouse.current != null) {
            bool wasPressed = Mouse.current.leftButton.wasPressedThisFrame;
            bool wasReleased = Mouse.current.leftButton.wasReleasedThisFrame;
            if (wasPressed)
                onPressed?.Invoke();
            if (wasReleased)
                onReleased?.Invoke();
        }
    }
}
