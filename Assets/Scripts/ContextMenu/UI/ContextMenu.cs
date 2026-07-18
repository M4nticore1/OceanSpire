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

        ContextMenuTarget.OnTargetSelected += OnTargetSelected;
        ContextMenuTarget.OnTargetDeselected += OnTargetDeselected;
        ContextMenuTarget.OnTargetDisabled += OnTargetDestroyed;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        ContextMenuTarget.OnTargetSelected -= OnTargetSelected;
        ContextMenuTarget.OnTargetDeselected -= OnTargetDeselected;
        ContextMenuTarget.OnTargetDisabled -= OnTargetDestroyed;
    }

    private void Open()
    {
        slidePanel.Show();
        onOpened?.Invoke();
    }

    private void Close()
    {
        slidePanel.Hide();
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