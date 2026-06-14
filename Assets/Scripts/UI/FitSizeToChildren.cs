using UnityEngine;
using UnityEngine.EventSystems;

public class FitSizeToChildren : UIBehaviour
{
    [SerializeField] private float minHeight = 0f;
    [SerializeField] private float extraHeight = 0f;

    private RectTransform rect;

    protected override void Awake()
    {
        base.Awake();

        rect = GetComponent<RectTransform>();
    }

    protected override void Start()
    {
        base.Start();

        UpdateSize();
    }

    public void UpdateSize()
    {
        if (rect.childCount == 0) {
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, minHeight);
            return;
        }

        Canvas.ForceUpdateCanvases();

        float minY = float.MaxValue;
        float maxY = float.MinValue;

        foreach (var child in GameUtils.GetAllChildren(rect)) {
            var childRect = child.GetComponent<RectTransform>();
            if (!childRect) continue;

            // Работаем в локальных координатах rect
            Vector3 childMin = rect.InverseTransformPoint(childRect.TransformPoint(childRect.rect.min));
            Vector3 childMax = rect.InverseTransformPoint(childRect.TransformPoint(childRect.rect.max));

            minY = Mathf.Min(minY, childMin.y, childMax.y);
            maxY = Mathf.Max(maxY, childMin.y, childMax.y);
        }

        float height = maxY - minY + extraHeight;
        height = Mathf.Max(height, minHeight);

        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
    }

}