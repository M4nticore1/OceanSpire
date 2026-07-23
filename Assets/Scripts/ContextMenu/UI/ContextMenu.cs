using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ContextMenu : UIBehaviour
{
    [SerializeField] private SlidePanel slidePanel;
    [SerializeField] private Transform contextMenuRoot;
    [SerializeField] private LayoutGroup layoutGroup;

    [SerializeField] private LocalizationItem levelLocalization;

    private List<ContextElement> showedContextElements = new();
    public ContextMenuTarget selectedTarget { get; private set; }

    public event Action onOpened;

    protected override void OnEnable()
    {
        base.OnEnable();

        ContextMenuTarget.OnTargetSelected += OnTargetSelected;
        ContextMenuTarget.OnTargetDeselected += OnTargetDeselected;
        ContextMenuTarget.OnTargetDisabled += OnTargetDestroyed;

        ContextElement.OnElementShowed += OnContextElementShowed;
        ContextElement.OnElementHidden += OnContextElementHidden;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        ContextMenuTarget.OnTargetSelected -= OnTargetSelected;
        ContextMenuTarget.OnTargetDeselected -= OnTargetDeselected;
        ContextMenuTarget.OnTargetDisabled -= OnTargetDestroyed;

        ContextElement.OnElementShowed -= OnContextElementShowed;
        ContextElement.OnElementHidden -= OnContextElementHidden;
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

    private void OnContextElementShowed(ContextElement contextElement)
    {
        if (!contextElement) return;

        if (showedContextElements.Contains(contextElement)) {
            Debug.LogError($"[{nameof(ContextMenu)}] Context Element {contextElement} is already in list!");
            return;
        }

        showedContextElements.Add(contextElement);

        int targetIndexInHierarchy = 0;
        for (int i = 0; i < showedContextElements.Count; i++) {
            var element = showedContextElements[i];
            if (!element) continue;
            if (element == contextElement) continue;
            if (element.SiblingIndex >= contextElement.SiblingIndex) continue;

            targetIndexInHierarchy++;
        }

        contextElement.transform.SetParent(layoutGroup.transform);
        contextElement.transform.SetSiblingIndex(targetIndexInHierarchy);
    }

    private void OnContextElementHidden(ContextElement contextElement)
    {
        if (!contextElement) return;

        showedContextElements.Remove(contextElement);
        contextElement.transform.SetParent(contextMenuRoot);
    }

    private bool ShouldClose(ContextMenuTarget target)
    {
        return target == selectedTarget;
    }
}