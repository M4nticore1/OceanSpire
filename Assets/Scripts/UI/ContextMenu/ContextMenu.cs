using UnityEngine;
using UnityEngine.EventSystems;

public class ContextMenu : UIBehaviour
{
    [SerializeField] private SlidePanel slidePanel;
    [SerializeField] private Transform contextMenuRoot;

    [SerializeField] private LocalizationItem levelLocalization;

    protected override void OnEnable()
    {
        base.OnEnable();

        SelectManager.onComponentSelected += OnComponentSelected;
        SelectManager.onComponentDeselected += OnComponentDeselected;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        SelectManager.onComponentSelected -= OnComponentSelected;
        SelectManager.onComponentDeselected -= OnComponentDeselected;
    }

    // Open/Close
    private void Open()
    {
        slidePanel.Open();
    }

    private void Close()
    {
        slidePanel.Close();
    }

    // Events
    private void OnComponentSelected(SelectComponent selected)
    {
        Open();
    }

    private void OnComponentDeselected(SelectComponent selected)
    {
        Close();
    }
}