using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public abstract class FitSizeToContent : MonoBehaviour
{
    [SerializeField] private RectTransform rect;
    public RectTransform RectTransform => rect;

    [SerializeField] private float minHeight = 0f;
    public float MinHeight => minHeight;

    [SerializeField] private float extraHeight = 0f;
    public float ExtraHeight => extraHeight;

    [SerializeField] private List<GameObject> includedTransforms = new();
    [SerializeField] private List<GameObject> includedHierarchyTransforms = new();

    [SerializeField] private List<GameObject> excludedTransforms = new();
    [SerializeField] private List<GameObject> excludedHierarchyTransforms = new();

    private Coroutine updateSizeCoroutine;

    protected virtual void Awake()
    {
        if (rect == null) {
            rect = GetComponent<RectTransform>();
        }
    }

    protected virtual void OnEnable()
    {
        UpdateSize();
    }

    protected virtual void OnTransformChildrenChanged()
    {
        UpdateSize();
    }

    protected abstract float GetHeight();

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

    public void UpdateSize()
    {
        if (!ShouldUpdateSize()) return;

        if (rect == null) {
            Debug.LogError($"[{nameof(FitSizeToContent)}] Rect is not valid at {this}!");
            return;
        }

        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, GetHeight());
    }

    protected List<GameObject> GetIncludedChildren()
    {
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
            children.AddRange(GameUtils.GetAllChildren(RectTransform));
        }

        children.RemoveAll(child =>
        {
            if (child == null || !child.activeSelf || !child.activeInHierarchy)
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

        return children;
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
[CustomEditor(typeof(FitSizeToContent), true)]
public class FitSizeToContentEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var widget = (FitSizeToContent)target;

        GUILayout.Space(10);

        if (GUILayout.Button("Update Size")) {
            widget.UpdateSizeDelay();

            EditorUtility.SetDirty(widget);
        }
    }
}
#endif