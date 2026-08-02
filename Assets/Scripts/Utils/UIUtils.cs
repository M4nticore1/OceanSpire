using UnityEngine;

public class UIUtils
{
    public static Vector3 WorldToCanvasPosition(
        Vector3 worldPosition,
        Canvas canvas)
    {
        var screenPoint = Camera.main.WorldToScreenPoint(worldPosition);
        var cameraForCanvas = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : Camera.main;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.GetComponent<RectTransform>(),
            screenPoint,
            cameraForCanvas,
            out Vector2 localPoint
        );

        return localPoint;
    }
}