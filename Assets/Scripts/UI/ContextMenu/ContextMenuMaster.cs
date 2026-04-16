using UnityEngine;
using UnityEngine.EventSystems;

public class ContextMenuMaster : UIBehaviour
{
    [SerializeField] private SlidePanel slidePanel;
    [SerializeField] private Transform contextMenuRoot;

    protected override void OnEnable()
    {
        base.OnEnable();

        SelectManager.onComponentSelected += OnComponentSelected;
        SelectManager.onComponentDeselected += OnComponentDeselected;
        EventBus.onPlayerClicked += OnPlayerClicked;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        SelectManager.onComponentSelected -= OnComponentSelected;
        SelectManager.onComponentDeselected -= OnComponentDeselected;
        EventBus.onPlayerClicked -= OnPlayerClicked;
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

    private void OnPlayerClicked(GameObject clicked)
    {
        SelectComponent selectComponent = clicked?.GetComponent<SelectComponent>();
        if (selectComponent && selectComponent.isSelected) return;

        Close();
    }

    protected void OnBuildingDemolished(Building building)
    {
        Building selectedBuilding = SelectManager.Instance.GetSelectedBuilding();
        if (selectedBuilding != building) return;

        Close();
    }
}