using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIManager : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private CityStorage cityStorage;

    // Widgets
    [Header("Widgets")]
    [SerializeField] private ResourceWidget buildingActionResourceWidgetPrefab = null;

    // Menus
    [Header("Menus")]
    [SerializeField] private ConstructionInformationMenu buildingInformationMenu = null;
    [SerializeField] private WorkersControlMenu workersMenu = null;
    [SerializeField] private StatsMenu statsMenu;
    private Building buildingToShowStats = null;
    [SerializeField] private ContextMenuMaster contextMenuMaster = null;

    public bool isManagementMenuOpened { get; private set; } = false;
    public bool isWorkersMenuOpened { get; private set; } = false;
    private bool isContextMenuOpened = false;

    [Header("Management Buttons")]
    [SerializeField] private CustomButton buildingListsMenuButton = null;
    [SerializeField] private CustomButton storageListsMenuButton = null;

    [Header("Building Action Menu")]
    [SerializeField] private GridLayoutGroup actionResourcesLayourGroup = null;

    private void OnEnable()
    {
        // Stas Menu
        EventBus.onCameraEnteredStatsMenuDistance += OnCameraEnteredStatsMenuDistance;
        EventBus.onCameraExitedStatsMenuDistance += OnCameraExitedStatsMenuDistance;

        // Workers Menu
        EventBus.onWorkersMenuClosed += OnWorkersMenuClosed;
    }

    private void OnDisable()
    {
        // Stas Menu
        EventBus.onCameraEnteredStatsMenuDistance -= OnCameraEnteredStatsMenuDistance;
        EventBus.onCameraExitedStatsMenuDistance -= OnCameraExitedStatsMenuDistance;

        // Workers Menu
        EventBus.onWorkersMenuClosed -= OnWorkersMenuClosed;
    }

    //private void OpenStorageListByCategory(ItemCategory itemCategory)
    //{
    //    // Set initial sibling index to last Button
    //    int lastIndex = (int)lastOpenedStorageListCategory - 1;
    //    storageListButtons[lastIndex].transform.SetSiblingIndex(storageListButtons.Length - lastIndex - 1);

    //    // Hide last list
    //    storageLists[lastIndex].gameObject.SetActive(false);

    //    // Set sibling index to selected button
    //    int index = (int)itemCategory - 1;
    //    storageListButtons[index].transform.SetAsLastSibling();

    //    // Show list
    //    storageLists[index].transform.gameObject.SetActive(true);
    //    storageListsScrollRect.content = storageLists[index].GetComponent<RectTransform>();
    //    lastOpenedStorageListCategory = itemCategory;
    //}

    private void OnWorkersMenuClosed()
    {
        isWorkersMenuOpened = false;
    }

    // Building Stats Menu
    private void OnCameraEnteredStatsMenuDistance(Building building)
    {
        buildingToShowStats = building;
        if (isContextMenuOpened) return;

        OpenStatsMenu(building);
    }

    private void OnCameraExitedStatsMenuDistance()
    {
        buildingToShowStats = null;
        CloseStatsMenu();
    }

    private void OpenStatsMenu(Building building)
    {
        statsMenu.OpenStatsMenu(building);
    }

    private void CloseStatsMenu()
    {
        statsMenu.CloseStatsMenu();
    }
}
