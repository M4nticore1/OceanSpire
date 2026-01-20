using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public static class InputUtils
{
    public static Vector2 GetCurrentInputPosition()
    {
        Vector2 position = new Vector2();
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed) {
            position = Touchscreen.current.primaryTouch.position.ReadValue();
        }
        else if (Mouse.current != null) {
            position = Mouse.current.position.ReadValue();
        }
        return position;
    }

    public static void GetCurrentRaycastResults(List<RaycastResult> results)
    {
        PointerEventData data = new PointerEventData(EventSystem.current);
        data.position = GetCurrentInputPosition();
        EventSystem.current.RaycastAll(data, results);
    }

    public static RaycastResult GetCurrentRaycastResult()
    {
        List<RaycastResult> results = new List<RaycastResult>();
        GetCurrentRaycastResults(results);
        if (results.Count > 0) {
            return results[0];
        }
        return new RaycastResult();
    }
}
