using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class ContextMenu : UIBehaviour
{
    [SerializeField] private SlidePanel slidePanel;
    [SerializeField] private Transform contextMenuRoot;

    [SerializeField] private LocalizationItem levelLocalization;

    public ContextMenuTarget selectedTarget { get; private set; }

    public event Action onOpened;

    protected override void OnEnable()
    {
        base.OnEnable();

        ContextMenuTarget.onTargetSelected += OnTargetSelected;
        ContextMenuTarget.onTargetDelected += OnTargetDeselected;
        ContextMenuTarget.onTargetDestroyed += OnTargetDestroyed;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        ContextMenuTarget.onTargetSelected -= OnTargetSelected;
        ContextMenuTarget.onTargetDelected -= OnTargetDeselected;
        ContextMenuTarget.onTargetDestroyed -= OnTargetDestroyed;
    }

    private void Open()
    {
        slidePanel.Open();
        onOpened?.Invoke();
    }

    private void Close()
    {
        slidePanel.Close();
    }

    private void SetSelectedTarget(ContextMenuTarget target)
    {
        selectedTarget = target;
    }

    private void OnTargetSelected(ContextMenuTarget target)
    {
        SetSelectedTarget(target);
        Open();
    }

    private void OnTargetDeselected(ContextMenuTarget target)
    {
        if (!ShouldClose(target)) return;

        SetSelectedTarget(null);
        Close();
    }

    private void OnTargetDestroyed(ContextMenuTarget target)
    {
        if (!ShouldClose(target)) return;

        SetSelectedTarget(null);
        Close();
    }

    private bool ShouldClose(ContextMenuTarget target)
    {
        return target == selectedTarget;
    }
}