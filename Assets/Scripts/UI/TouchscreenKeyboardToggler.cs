using UnityEngine;
using UnityEngine.EventSystems;

public class TouchscreenKeyboardToggler : MonoBehaviour, IPointerUpHandler
{
    TouchscreenKeyboardManager keyboardManager;

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!TouchScreenKeyboard.isSupported) return;

        //keyboardManager.OpenKeyboard();
    }
}
