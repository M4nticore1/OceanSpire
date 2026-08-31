using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public struct PointerRaycastHit
{
    public GameObject gameObject;
    public float distance;

    public RaycastHit? colliderHit;
    public RaycastResult? uiHit;
}

public static class PointerUtils
{
    // Position
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

    public static bool GetRaycastHit(out PointerRaycastHit hit)
    {
        hit = default;
        var position = GetCurrentInputPosition();

        var camera = Camera.main;
        if (camera == null) return false;

        var ray = camera.ScreenPointToRay(position);

        // 3D
        var layerMask = ~LayerMask.GetMask("Ignore Raycast");
        bool hasColliderHit = Physics.Raycast(ray, out RaycastHit colliderHit, Mathf.Infinity, layerMask);

        // UI
        var uiResults = new List<RaycastResult>();
        GetRaycastUIResults(uiResults);

        RaycastResult? closestWorldUI = null;
        float closestUIDistance = Mathf.Infinity;

        foreach (var uiResult in uiResults) {
            var canvas = uiResult.module.GetComponent<Canvas>();

            if (canvas == null)
                continue;

            // Screen Space UI имеет безусловный приоритет
            if (canvas.renderMode != RenderMode.WorldSpace) {
                hit.gameObject = uiResult.gameObject;
                hit.distance = 0f;
                hit.uiHit = uiResult;

                return true;
            }

            // World Space UI
            var distance = Vector3.Distance( ray.origin, uiResult.worldPosition);
            if (distance < closestUIDistance) {
                closestUIDistance = distance;
                closestWorldUI = uiResult;
            }
        }

        // Сравниваем World Space UI и Collider
        if (closestWorldUI.HasValue && (!hasColliderHit || closestUIDistance < colliderHit.distance)) {
            hit.gameObject = closestWorldUI.Value.gameObject;
            hit.distance = closestUIDistance;
            hit.uiHit = closestWorldUI;

            return true;
        }

        if (hasColliderHit) {
            hit.gameObject = colliderHit.collider.gameObject;
            hit.distance = colliderHit.distance;
            hit.colliderHit = colliderHit;

            return true;
        }

        return false;
    }

    // Colliders
    public static bool GetRaycastColliderHit(out RaycastHit hit)
    {
        var position = GetCurrentInputPosition();
        var ray = Camera.main.ScreenPointToRay(position);
        var layerMask = ~LayerMask.GetMask("Ignore Raycast");

        return Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask);
    }

    // UI
    public static void GetRaycastUIResults(List<RaycastResult> results)
    {
        var eventSystem = EventSystem.current;
        if (eventSystem == null) {
            Debug.LogError($"[{nameof(PointerUtils)}] Event System is not on the scene!");
            return;
        }

        var data = new PointerEventData(eventSystem);
        data.position = GetCurrentInputPosition();
        eventSystem.RaycastAll(data, results);
    }

    public static RaycastResult GetRaycastUIResult()
    {
        var results = new List<RaycastResult>();
        GetRaycastUIResults(results);

        if (results.Count > 0) {
            return results[0];
        }

        return new RaycastResult();
    }

    // Conditions
    public static bool IsUIHovered(GameObject goToCheck)
    {
        var hovered = GetRaycastUIResult().gameObject;
        if (hovered == null) return false;

        return hovered == goToCheck;
    }
}
