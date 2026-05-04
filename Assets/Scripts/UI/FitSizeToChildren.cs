using UnityEngine;
using UnityEngine.EventSystems;

public class FitSizeToChildren : UIBehaviour
{
    [SerializeField] private float minHeight = 0f;

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

        float top = float.MinValue;
        float bottom = float.MaxValue;

        foreach (RectTransform child in rect) {
            Vector3[] corners = new Vector3[4];
            child.GetWorldCorners(corners);

            float childTop = corners[1].y;
            float childBottom = corners[0].y;

            if (childTop > top) top = childTop;
            if (childBottom < bottom) bottom = childBottom;
        }

        float localTop = rect.InverseTransformPoint(new Vector3(0, top, 0)).y;
        float localBottom = rect.InverseTransformPoint(new Vector3(0, bottom, 0)).y;

        float height = localTop - localBottom;

        height = Mathf.Max(height, minHeight);

        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
    }
}