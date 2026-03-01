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
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        EventBus.onPlayerClicked -= OnPlayerClicked;
        EventBus.onSelectedBuilding -= OnSelectedBuilding;
        EventBus.onDeselectedBuilding -= OnDeselectedBuilding;
        EventBus.onSelectedBoat -= OnSelectedBoat;
        EventBus.onDeselectedBoat -= OnDeselectedBoat;
    }

    // Open/Close
    private void OpenContextMenu()
    {
        slidePanel.OpenSlidePanel();
        isOpened = true;
    }

    private void CloseContextMenu()
    {
        slidePanel.CloseSlidePanel();
        isOpened = false;
    }

    // Events
    private void OnPlayerClicked(GameObject clicked)
    {
        SelectComponent selectComponent = clicked?.GetComponent<SelectComponent>();

        if (selectComponent && selectComponent.isSelected) return;

        CloseContextMenu();
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
        OpenContextMenu();

        currentSelectedObject = building.gameObject;
    }

    private void OnDeselectedBuilding(Building building)
    {
        if (building.gameObject != currentSelectedObject) return;

        CloseContextMenu();
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
        OpenContextMenu();

        currentSelectedObject = boat.gameObject;
    }

    private void OnDeselectedBoat(Boat boat)
    {
        if (boat.gameObject != currentSelectedObject) return;

        CloseContextMenu();
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