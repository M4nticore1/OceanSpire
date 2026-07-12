using UnityEngine;
using UnityEngine.EventSystems;

public class FitSizeToChildren : MonoBehaviour
{
    [SerializeField] private float minHeight = 0f;
    [SerializeField] private float extraHeight = 0f;

    private RectTransform rect;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    private void Start()
    {
        UpdateSize();
    }

    private void OnEnable()
    {
        UpdateSize();
    }

    private void OnTransformChildrenChanged()
    {
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