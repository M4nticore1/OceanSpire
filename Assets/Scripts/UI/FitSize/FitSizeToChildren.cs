using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class FitSizeToChildren : FitSizeToContent
{
    protected override float GetHeight()
    {
        var children = GetIncludedChildren();

        if (children.Count == 0) {
            return MinHeight;
        }

        Canvas.ForceUpdateCanvases();

        var corners = new Vector3[4];
        var lowestY = float.MaxValue;

        foreach (var child in children) {
            var childRect = child.GetComponent<RectTransform>();

            if (childRect == null)
                continue;

            childRect.GetWorldCorners(corners);

            for (int i = 0; i < corners.Length; i++) {
                var y = RectTransform.InverseTransformPoint(corners[i]).y;
                lowestY = Mathf.Min(lowestY, y);
            }
        }

        if (lowestY == float.MaxValue) {
            return MinHeight;
        }

        var pivotY = RectTransform.pivot.y;
        var currentHeight = RectTransform.rect.height;
        var requiredHeight = currentHeight;

        if (pivotY > 0f) {
            requiredHeight = (RectTransform.rect.yMax - lowestY) / pivotY;
        }
        else {
            requiredHeight = currentHeight + (RectTransform.rect.yMin - lowestY);
        }

        requiredHeight += ExtraHeight;
        requiredHeight = Mathf.Max(requiredHeight, MinHeight);

        return requiredHeight;
    }
}