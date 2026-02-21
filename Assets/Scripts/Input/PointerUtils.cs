using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public static class PointerUtils
{
    public static Vector2 GetCurrentInputPosition()
    {
        if (Touchscreen.current != null) {
            if (Touchscreen.current.primaryTouch.press.wasPressedThisFrame || Touchscreen.current.primaryTouch.press.wasReleasedThisFrame) {
                return Touchscreen.current.primaryTouch.position.ReadValue();
            }
        }

        if (Mouse.current != null) {
            return Mouse.current.position.ReadValue();
        }

        return Vector2.zero;
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

    public static bool IsGameObjectHovered(GameObject gameObjectToCheck) => GetCurrentRaycastResult().gameObject == gameObjectToCheck;
}
