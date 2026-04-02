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

    // Colliders
    public static bool GetRaycastColliderHit(out RaycastHit hit)
    {
        Vector2 position = GetCurrentInputPosition();
        Ray ray = Camera.main.ScreenPointToRay(position);

        int layerMask = ~LayerMask.GetMask("Ignore Raycast");
        return Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask);
    }

    // UI
    public static void GetRaycastUIResults(List<RaycastResult> results)
    {
        if (!EventSystem.current) {
            Debug.LogWarning("EventSystem is not on the scene.");
            return;
        }

        PointerEventData data = new PointerEventData(EventSystem.current);
        data.position = GetCurrentInputPosition();
        EventSystem.current.RaycastAll(data, results);
    }

    public static RaycastResult GetRaycastUIResult()
    {
        List<RaycastResult> results = new List<RaycastResult>();
        GetRaycastUIResults(results);

        if (results.Count > 0) {
            return results[0];
        }

        return new RaycastResult();
    }

    // Conditions
    public static bool IsUIHovered(GameObject gameObjectToCheck)
    {
        GameObject hovered = GetRaycastUIResult().gameObject;

        if (!hovered) return false;

        return GetRaycastUIResult().gameObject == gameObjectToCheck;
    }
}
