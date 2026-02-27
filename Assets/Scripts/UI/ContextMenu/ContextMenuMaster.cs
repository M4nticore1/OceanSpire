using UnityEngine;
using UnityEngine.EventSystems;

public class ContextMenuMaster : UIBehaviour
{
    [SerializeField] private SlidePanel slidePanel = null;
    [SerializeField] private RectTransform contextMenuRoot = null;
    private ContextMenuUI currentContextMenu = null;

    [Header("Buildings")]
    [SerializeField] private ContextMenuUI buildingContextMenu = null;
    [SerializeField] private ContextMenuUI productionBuildingContextMenu = null;
    [SerializeField] private ContextMenuUI storageBuildingContextMenu = null;
    [SerializeField] private ContextMenuUI pierBuildingContextMenu = null;

    [Header("Boats")]
    [SerializeField] private ContextMenuUI boatContextMenu = null;

    private bool isOpened = false;
    private GameObject currentTargetObject = null;

    protected override void OnEnable()
    {
        base.OnEnable();

        EventBus.onPlayerClicked += OnPlayerClicked;
        EventBus.onSelectedBuilding += OnSelectedBuilding;
        EventBus.onDeselectedBuilding += OnDeselectedBuilding;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        EventBus.onPlayerClicked -= OnPlayerClicked;
        EventBus.onSelectedBuilding -= OnSelectedBuilding;
        EventBus.onDeselectedBuilding -= OnDeselectedBuilding;
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

    private void CreateContextMenu(ContextMenuUI menuToSpawn)
    {
        currentContextMenu = Instantiate(menuToSpawn, contextMenuRoot.transform);
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
        if (currentContextMenu) {
            DestroyCurrentContextMenu();
        }

        ContextMenuUI menuToSpawn = GetContextMenuForBuilding(building);

        CreateContextMenu(menuToSpawn);
        currentContextMenu.Init(building);
        OpenContextMenu();

        currentTargetObject = building.gameObject;
    }

    private void OnDeselectedBuilding(Building building)
    {
        if (building != currentTargetObject) return;

        CloseContextMenu();
        currentTargetObject = null;
    }

    private ContextMenuUI GetContextMenuForBuilding(Building building)
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