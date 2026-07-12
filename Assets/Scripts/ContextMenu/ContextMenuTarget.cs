using System;
using UnityEngine;

public class ContextMenuTarget : MonoBehaviour
{
    [SerializeField] private SelectComponent selectComponent;
    [SerializeField] private bool showContextMenu = true;

    public static event Action<ContextMenuTarget> OnTargetSelected;
    public static event Action<ContextMenuTarget> OnTargetDeselected;
    public static event Action<ContextMenuTarget> OnTargetDisabled;

    private void OnEnable()
    {
        selectComponent.OnSelected += OnComponentSelected;
        selectComponent.OnDeselected += OnComponentDeselected;
    }

    private void OnDisable()
    {
        selectComponent.OnSelected -= OnComponentSelected;
        selectComponent.OnDeselected -= OnComponentDeselected;
        OnTargetDisabled?.Invoke(this);
    }

    public void SetShowContextMenu(bool value)
    {
        showContextMenu = value;
    }

    private void TrySelect()
    {
        if (!showContextMenu) return;

        Select();
    }

    private void Select()
    {
        OnTargetSelected?.Invoke(this);
    }

    private void Deselect()
    {
        OnTargetDeselected?.Invoke(this);
    }

    private void OnComponentSelected()
    {
        TrySelect();
    }

    private void OnComponentDeselected()
    {
        Deselect();
    }
}