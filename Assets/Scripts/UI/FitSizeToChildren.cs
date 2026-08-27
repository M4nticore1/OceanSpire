using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class FitSizeToChildren : MonoBehaviour
{
    [SerializeField] private RectTransform rect;
    [SerializeField] private float minHeight = 0f;
    [SerializeField] private float extraHeight = 0f;
    [SerializeField] private List<GameObject> includedTransforms = new();
    [SerializeField] private List<GameObject> excludedTransforms = new();

    private Coroutine updateSizeCoroutine;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    private void Start()
    {
        UpdateSizeDelay();
    }

    private void OnEnable()
    {
        UpdateSizeDelay();
    }

    private void OnTransformChildrenChanged()
    {
        UpdateSizeDelay();
    }

    public void UpdateSize()
    {
        if (!ShouldUpdateSize()) return;

        var children = includedTransforms.Count > 0 ? new List<GameObject>(includedTransforms) : GameUtils.GetAllChildren(rect);
        if (children.Count == 0) {
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, minHeight);
            return;
        }

        foreach (var excluded in excludedTransforms) {
            children.Remove(excluded);
        }

        Canvas.ForceUpdateCanvases();

        var corners = new Vector3[4];
        var lowestY = float.MaxValue;

        foreach (var child in children) {
            var childRect = child.GetComponent<RectTransform>();
            if (childRect == null)
                continue;

            childRect.GetWorldCorners(corners);
            for (int i = 0; i < 4; i++) {
                var y = rect.InverseTransformPoint(corners[i]).y;
                lowestY = Mathf.Min(lowestY, y);
            }
        }

        var rectBottom = rect.rect.yMin;
        var requiredHeight = rect.rect.height + (rectBottom - lowestY);

        requiredHeight += extraHeight;
        requiredHeight = Mathf.Max(requiredHeight, minHeight);

        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, requiredHeight);
    }

    public void UpdateSizeDelay()
    {
        if (!ShouldUpdateSize()) return;

        if (updateSizeCoroutine == null) {
            updateSizeCoroutine = StartCoroutine(UpdateSizeCoroutine());
        }
    }

    public void AddIncludedTransform(GameObject go)
    {
        if (go == null) return;
        if (includedTransforms.Contains(go)) return;

        includedTransforms.Add(go);
    }

    public void RemoveIncludedTransform(GameObject go)
    {
        includedTransforms.Remove(go);
    }

    private bool ShouldUpdateSize()
    {
        if (!gameObject.activeSelf) return false;
        if (!gameObject.activeInHierarchy) return false;

        return true;
    }

    private IEnumerator UpdateSizeCoroutine()
    {
        yield return null;

        updateSizeCoroutine = null;
        Canvas.ForceUpdateCanvases();
        UpdateSize();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(FitSizeToChildren))]
public class MyWidgetEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var widget = (FitSizeToChildren)target;

        GUILayout.Space(10);

        if (GUILayout.Button("Update Size")) {
            widget.UpdateSizeDelay();

            EditorUtility.SetDirty(widget);
        }
    }
}
#endif