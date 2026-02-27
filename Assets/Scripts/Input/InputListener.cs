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

            if (wasPressed) {
                HandlePress();
            }

            if (wasReleased) {
                HandleRelease();
            }
        }

        if (Mouse.current != null) {
            bool wasPressed = Mouse.current.leftButton.wasPressedThisFrame;
            bool wasReleased = Mouse.current.leftButton.wasReleasedThisFrame;

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
        onPressed?.Invoke();
    }

    private void HandleRelease()
    {
        onReleased?.Invoke();
    }
}
