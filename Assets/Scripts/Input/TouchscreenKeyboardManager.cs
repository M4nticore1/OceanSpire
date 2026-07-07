using UnityEngine;
using UnityEngine.InputSystem;

public class TouchscreenKeyboardManager : MonoBehaviour
{
    private TouchScreenKeyboard keyboard;
    private string inputText;

    private void OnEnable()
    {
        if (!TouchScreenKeyboard.isSupported) return;

        InputListener.Instance.OnReleased += OnPointerReleased;
    }

    private void OnDisable()
    {
        if (!TouchScreenKeyboard.isSupported) return;

        InputListener.Instance.OnReleased -= OnPointerReleased;
    }

    private void Start()
    {
        OpenKeyboard();
    }

    private void Update()
    {
        if (!TouchScreenKeyboard.isSupported) return;

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) {
            CloseKeyboard();
        }
    }

    public void OpenKeyboard()
    {
        keyboard = TouchScreenKeyboard.Open(inputText, TouchScreenKeyboardType.Default);
    }

    public void CloseKeyboard()
    {
        if (keyboard == null) return;

        keyboard.active = false;
        keyboard = null;
    }

    private void OnPointerReleased()
    {
        GameObject hitedGO = PointerUtils.GetRaycastUIResult().gameObject;

        if (hitedGO != null && hitedGO.GetComponent<TouchscreenKeyboardToggler>() != null) {
            OpenKeyboard();
        }
        else {
            CloseKeyboard();
        }
    }
}