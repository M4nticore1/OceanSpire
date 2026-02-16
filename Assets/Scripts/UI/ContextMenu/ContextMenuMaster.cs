using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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

    protected override void OnEnable()
    {
        base.OnEnable();

        EventBus.onSelectedComponent += OnSelectedComponent;
        EventBus.onDeselectedComponent += OnDeselectedComponent;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        EventBus.onSelectedComponent -= OnSelectedComponent;
        EventBus.onDeselectedComponent -= OnDeselectedComponent;
    }

    private void OpenContextMenu(SelectComponent selectComponent)
    {
        slidePanel.OpenSlidePanel();

        if (currentContextMenu) {
            DestroyCurrentContextMenu();
        }

        ContextMenuUI menuToCreate = CalculateContextMenu(selectComponent);
        currentContextMenu = CreateContextMenu(menuToCreate, selectComponent);
    }

    private void CloseContextMenu()
    {
        slidePanel.CloseSlidePanel();
    }

    private ContextMenuUI CalculateContextMenu(SelectComponent component)
    {
        Building building = component.GetComponent<Building>();
        Human entity = component.GetComponent<Human>();
        Boat boat = component.GetComponent<Boat>();

        if (building) {
            if (building.GetComponent<ProductionBuildingModule>()) {
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
        else if (entity) {

        }
        else if (boat) {
            return boatContextMenu;
        }

        return null;
    }

    private ContextMenuUI CreateContextMenu(ContextMenuUI menuToSpawn, SelectComponent selectComponent)
    {
        ContextMenuUI menu = Instantiate(menuToSpawn, contextMenuRoot.transform);
        menu.Init(selectComponent);
        return menu;
    }

    private void DestroyCurrentContextMenu()
    {
        Destroy(currentContextMenu.gameObject);
        currentContextMenu = null;
    }

    private void OnSelectedComponent(SelectComponent component)
    {
        OpenContextMenu(component);
    }

    private void OnDeselectedComponent(SelectComponent component)
    {
        SelectComponent selectedComponent = SelectManager.Instance.selectedComponent;
        if (selectedComponent && selectedComponent != component) return;

        CloseContextMenu();
    }
}
