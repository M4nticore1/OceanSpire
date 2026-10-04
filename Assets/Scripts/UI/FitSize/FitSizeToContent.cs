using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public abstract class FitSizeToContent : MonoBehaviour
{
    private RectTransform rectTransform;
    public RectTransform RectTransform => rectTransform != null ? rectTransform : GetComponent<RectTransform>();

    [SerializeField] private bool fitHorizontal = true;
    [SerializeField] private bool fitVertical = true;

    [SerializeField] private Vector2 minSize = Vector2.zero;
    public Vector2 MinSize => minSize;

    [SerializeField] private Vector2 extraSize = Vector2.zero;
    public Vector2 ExtraSize => extraSize;

    [Header("Depricated")]
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
        rectTransform = GetComponent<RectTransform>();
    }

    protected virtual void OnEnable()
    {
        Subscribe();
        UpdateSize();
    }

    protected virtual void OnDisable()
    {
        Unsubscribe();
    }

    protected virtual void OnTransformChildrenChanged()
    {
        UpdateSize();
    }

    protected virtual void Subscribe()
    {

    }

    protected virtual void Unsubscribe()
    {

    }

    protected abstract Vector2 GetSize();

    public void RunUpdateSizeEndOfFrame()
    {
        if (!ShouldUpdateSize()) {
            Debug.LogError($"[{nameof(FitSizeToContent)}] Should not to update size at {this}!");
            return;
        }

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

    public void TryUpdateSize()
    {
        if (!ShouldUpdateSize()) return;

        UpdateSize();
    }

    public void UpdateSize()
    {
        if (RectTransform == null) {
            Debug.LogError($"[{nameof(FitSizeToContent)}] Rect is not valid at {this}!");
            return;
        }

        var size = GetSize();

        if (fitHorizontal) {
            RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
        }

        if (fitVertical) {
            RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
        }
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
        if (this == null) return false;
        if (!gameObject.activeInHierarchy) return false;

        return true;
    }

    private IEnumerator UpdateSizeCoroutine()
    {
        yield return null;
        yield return new WaitForSeconds(3);

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
            widget.UpdateSize();

            EditorUtility.SetDirty(widget);
        }
    }
}
#endif