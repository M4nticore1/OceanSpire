using System;
using UnityEngine;

public class ContextMenuTarget : MonoBehaviour
{
    [SerializeField] private SelectComponent selectComponent;
    [SerializeField] private bool showContextMenu = true;

    public static event Action<ContextMenuTarget> onTargetSelected;
    public static event Action<ContextMenuTarget> onTargetDelected;
    public static event Action<ContextMenuTarget> onTargetDestroyed;

    private void OnEnable()
    {
        selectComponent.onSelected += OnComponentSelected;
        selectComponent.onDeselected += OnComponentDeselected;
    }

    private void OnDisable()
    {
        selectComponent.onSelected -= OnComponentSelected;
        selectComponent.onDeselected -= OnComponentDeselected;
        onTargetDestroyed?.Invoke(this);
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
        onTargetSelected?.Invoke(this);
    }

    private void Deselect()
    {
        onTargetDelected?.Invoke(this);
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