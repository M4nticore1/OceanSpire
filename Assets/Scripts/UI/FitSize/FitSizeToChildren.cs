using UnityEngine;

public class FitSizeToChildren : FitSizeToContent
{
    protected override Vector2 GetSize()
    {
        var children = GetIncludedChildren();

        if (children.Count == 0) {
            return MinSize;
        }

        Canvas.ForceUpdateCanvases();

        var corners = new Vector3[4];

        var lowestX = float.MaxValue;
        var lowestY = float.MaxValue;

        foreach (var child in children) {
            var childRect = child.GetComponent<RectTransform>();

            if (childRect == null)
                continue;

            childRect.GetWorldCorners(corners);

            for (int i = 0; i < corners.Length; i++) {
                var x = RectTransform.InverseTransformPoint(corners[i]).x;
                lowestX = Mathf.Min(lowestX, x);

                var y = RectTransform.InverseTransformPoint(corners[i]).y;
                lowestY = Mathf.Min(lowestY, y);
            }
        }

        if (lowestY == float.MaxValue) {
            return MinSize;
        }

        var pivot = RectTransform.pivot;
        var currentSize = RectTransform.rect.size;
        var requiredSize = currentSize;

        if (pivot.x > 0f) {
            requiredSize.x = (RectTransform.rect.xMax - lowestX) / pivot.x;
        }
        else {
            requiredSize.x = currentSize.x + (RectTransform.rect.xMin - lowestX);
        }

        if (pivot.y > 0f) {
            requiredSize.y = (RectTransform.rect.yMax - lowestY) / pivot.y;
        }
        else {
            requiredSize.y = currentSize.y + (RectTransform.rect.yMin - lowestY);
        }

        requiredSize.x += ExtraHeight;
        requiredSize.y += ExtraHeight;

        requiredSize.x = Mathf.Max(requiredSize.x, MinSize.x);
        requiredSize.y = Mathf.Max(requiredSize.y, MinSize.y);

        return requiredSize;
    }
}