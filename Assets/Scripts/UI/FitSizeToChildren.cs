using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem.HID;

public class FitSizeToChildren : MonoBehaviour
{
    [SerializeField] private RectTransform rect;
    [SerializeField] private float minHeight = 0f;
    [SerializeField] private float extraHeight = 0f;

    [SerializeField] private List<GameObject> includedTransforms = new();
    [SerializeField] private List<GameObject> includedHierarchyTransforms = new();

    [SerializeField] private List<GameObject> excludedTransforms = new();
    [SerializeField] private List<GameObject> excludedHierarchyTransforms = new();

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
        if (!ShouldUpdateSize())
            return;

        var children = new List<GameObject>();

        if (includedTransforms.Count > 0 || includedHierarchyTransforms.Count > 0) {
            children.AddRange(includedTransforms);

            foreach (var root in includedHierarchyTransforms) {
                if (root == null)
                    continue;

                children.Add(root);
                children.AddRange(GameUtils.GetAllChildren(root.transform));
            }
        }
        else {
            children.AddRange(GameUtils.GetAllChildren(rect));
        }

        // Exclude specific objects and hierarchies.
        children.RemoveAll(child =>
        {
            if (child == null)
                return true;

            if (excludedTransforms.Contains(child))
                return true;

            foreach (var excluded in excludedHierarchyTransforms) {
                if (excluded == null)
                    continue;

                if (child == excluded || child.transform.IsChildOf(excluded.transform))
                    return true;
            }

            return false;
        });

        if (children.Count == 0) {
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, minHeight);

            return;
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
                var y = rect.InverseTransformPoint(corners[i]).y;
                lowestY = Mathf.Min(lowestY, y);
            }
        }

        if (lowestY == float.MaxValue) {
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, minHeight);

            return;
        }

        var pivotY = rect.pivot.y;
        var currentHeight = rect.rect.height;
        var requiredHeight = currentHeight;

        if (pivotY > 0f) {
            requiredHeight = (rect.rect.yMax - lowestY) / pivotY;
        }
        else {
            requiredHeight = currentHeight + (rect.rect.yMin - lowestY);
        }

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
        if (!enabled) return false;
        if (!gameObject.activeSelf) return false;
        if (!gameObject.activeInHierarchy) return false;

        return true;
    }

    private IEnumerator UpdateSizeCoroutine()
    {
        yield return new WaitForEndOfFrame();

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