using System;
using UnityEngine;

public class ContextMenuManager : MonoBehaviour
{
    public static ContextMenuManager Instance { get; private set; }

    public ContextMenuTarget ContextMenuTarget { get; private set; }

    public event Action<ContextMenuTarget> OnContextMenuTargetSelected;
    public event Action<ContextMenuTarget> OnContextMenuTargetDeselected;

    private void Awake()
    {
        if (Instance) {
            Debug.Log("Another ContextMenuManager is on the scene!");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        ContextMenuTarget.OnTargetSelected += HandleContextTargetSelected;
        ContextMenuTarget.OnTargetDeselected += HandleContextTargetDeselected;
    }

    private void OnDisable()
    {
        ContextMenuTarget.OnTargetSelected -= HandleContextTargetSelected;
        ContextMenuTarget.OnTargetDeselected -= HandleContextTargetDeselected;
    }

    private void HandleContextTargetSelected(ContextMenuTarget target)
    {
        if (!target) return;

        ContextMenuTarget = target;
        OnContextMenuTargetSelected?.Invoke(target);
    }

    private void HandleContextTargetDeselected(ContextMenuTarget target)
    {
        if (!target) return;

        if (target == ContextMenuTarget) {
            ContextMenuTarget = null;
        }

        OnContextMenuTargetDeselected?.Invoke(target);
    }
}