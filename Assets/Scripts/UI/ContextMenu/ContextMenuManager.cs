using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class ContextMenuManager : UIBehaviour
{
    [SerializeField] private SlidePanel slidePanel = null;
    [SerializeField] private Transform contextMenuRoot = null;
    private ContextMenuBase currentContextMenu = null;

    [Header("Buildings")]
    [SerializeField] private BuildingContextMenu buildingContextMenu = null;
    [SerializeField] private BuildingContextMenu productionBuildingContextMenu = null;
    [SerializeField] private BuildingContextMenu storageBuildingContextMenu = null;
    [SerializeField] private BuildingContextMenu pierBuildingContextMenu = null;

    [Header("Boats")]
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

    private void DestroyCurrentContextMenu()
    {
        Destroy(currentContextMenu.gameObject);
        currentContextMenu = null;
    }

    // Events
    private void OnPlayerClicked(GameObject clicked)
    {
        SelectComponent selectComponent = clicked?.GetComponent<SelectComponent>();

        if (selectComponent && selectComponent.isSelected) return;

        CloseContextMenu();
    }

    private void OnSelectedBuilding(Building building)
    {
        if (!building) {
            Debug.LogError("Building is not valid.");
            return;
        }

        if (currentContextMenu) {
            DestroyCurrentContextMenu();
        }

        BuildingContextMenu menuToSpawn = GetContextMenuForBuilding(building);

        currentContextMenu = ContextMenuFactory.CreateContextMenu(menuToSpawn, building, contextMenuRoot);
        OpenContextMenu();

        currentSelectedObject = building.gameObject;
    }

    private void OnDeselectedBuilding(Building building)
    {
        if (building.gameObject != currentSelectedObject) return;

        CloseContextMenu();
        currentSelectedObject = null;
    }

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

    // Get Context Menu
    private BuildingContextMenu GetContextMenuForBuilding(Building building)
    {
        if (building.GetComponent<ProductionModule>()) {
            return productionBuildingContextMenu;
        }
        else if (building.GetComponent<StorageBuildingModule>()) {
            return storageBuildingContextMenu;
        }
        else if (building.GetComponent<PierModule>()) {
            return pierBuildingContextMenu;
        }
        else {
            return buildingContextMenu;
        }
    }
}