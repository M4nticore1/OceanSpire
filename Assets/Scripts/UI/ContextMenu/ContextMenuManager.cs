using UnityEngine;
using UnityEngine.EventSystems;

public class ContextMenuManager : UIBehaviour
{
    [SerializeField] private SlidePanel slidePanel = null;
    [SerializeField] private Transform contextMenuRoot = null;
    private ContextMenuBase currentContextMenu = null;

    [SerializeField] private BuildingContextMenu buildingContextMenu = null;
    [SerializeField] private BoatContextMenu boatContextMenu = null;

    private bool isOpened = false;
    private GameObject currentSelectedObject = null;

    protected override void OnEnable()
    {
        base.OnEnable();

        EventBus.onPlayerClicked += OnPlayerClicked;
        EventBus.onSelectedBuilding += OnSelectedBuilding;
        EventBus.onDeselectedBuilding += OnDeselectedBuilding;
        EventBus.onSelectedBoat += OnSelectedBoat;
        EventBus.onDeselectedBoat += OnDeselectedBoat;
        EventBus.onBuildingDemolished += OnBuildingDemolished;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        EventBus.onPlayerClicked -= OnPlayerClicked;
        EventBus.onSelectedBuilding -= OnSelectedBuilding;
        EventBus.onDeselectedBuilding -= OnDeselectedBuilding;
        EventBus.onSelectedBoat -= OnSelectedBoat;
        EventBus.onDeselectedBoat -= OnDeselectedBoat;
        EventBus.onBuildingDemolished -= OnBuildingDemolished;
    }

    // Open/Close
    private void Open()
    {
        slidePanel.Open();
        isOpened = true;
    }

    private void Close()
    {
        slidePanel.Close();
        isOpened = false;
    }

    // Events
    private void OnPlayerClicked(GameObject clicked)
    {
        SelectComponent selectComponent = clicked?.GetComponent<SelectComponent>();

        if (selectComponent && selectComponent.isSelected) return;

        Close();
    }

    protected void OnBuildingDemolished(Building building)
    {
        SelectComponent selectComponent = SelectManager.Instance.selectedComponent;
        if (!selectComponent) return;

        Building selectedBuilding = selectComponent.GetComponent<Building>();
        if (selectedBuilding != building) return;

        Close();
    }

    // Building
    private void OnSelectedBuilding(Building building)
    {
        if (!building) {
            Debug.LogError("Building is not valid.");
            return;
        }

        if (currentContextMenu) {
            DestroyCurrentContextMenu();
        }

        currentContextMenu = ContextMenuFactory.CreateContextMenu(buildingContextMenu, building, contextMenuRoot);
        Open();

        currentSelectedObject = building.gameObject;
    }

    private void OnDeselectedBuilding(Building building)
    {
        if (building.gameObject != currentSelectedObject) return;

        Close();
        currentSelectedObject = null;
    }

    // Boat
    private void OnSelectedBoat(Boat boat)
    {
        if (!boat) {
            Debug.LogError("Boat is not valid.");
            return;
        }

        if (currentContextMenu) {
            DestroyCurrentContextMenu();
        }

        currentContextMenu = ContextMenuFactory.CreateContextMenu(boatContextMenu, boat, contextMenuRoot);
        Open();

        currentSelectedObject = boat.gameObject;
    }

    private void OnDeselectedBoat(Boat boat)
    {
        if (boat.gameObject != currentSelectedObject) return;

        Close();
        currentSelectedObject = null;
    }

    // Utins
    private void DestroyCurrentContextMenu()
    {
        if (!currentContextMenu) return;

        Destroy(currentContextMenu.gameObject);
        currentContextMenu = null;
    }
}