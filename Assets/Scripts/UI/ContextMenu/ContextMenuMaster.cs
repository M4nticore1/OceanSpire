using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ContextMenuMaster : UIBehaviour
{
    [SerializeField] private SlidePanel slidePanel = null;
    private ContextMenu openedContextMenu = null;

    [Header("Buildings")]
    [SerializeField] private ContextMenu buildingContextMenu = null;
    [SerializeField] private ContextMenu productionContextMenu = null;
    [SerializeField] private ContextMenu storageContextMenu = null;

    [Header("Boats")]
    [SerializeField] private ContextMenu boatContextMenu = null;

    public void OpenContextMenu(SelectComponent selectedObject)
    {
        slidePanel.OpenSlidePanel();
        if (openedContextMenu) {
            openedContextMenu.gameObject.SetActive(false);
            openedContextMenu = null;
        }

        Building building = selectedObject.GetComponent<Building>();
        Creature entity = selectedObject.GetComponent<Creature>();
        Boat boat = selectedObject.GetComponent<Boat>();

        if (building) {
            if (building.GetComponent<ProductionBuildingModule>())
                openedContextMenu = productionContextMenu;
            else if (building.GetComponent<StorageBuildingModule>())
                openedContextMenu = storageContextMenu;
            else
                openedContextMenu = buildingContextMenu;
        }
        else if (entity) {

        }
        else if (boat) {
            openedContextMenu = boatContextMenu;
        }

        if (openedContextMenu) {
            openedContextMenu.gameObject.SetActive(true);
            openedContextMenu.Open(selectedObject);
        }
    }

    public void CloseContextMenu()
    {
        slidePanel.CloseSlidePanel();
    }
}
