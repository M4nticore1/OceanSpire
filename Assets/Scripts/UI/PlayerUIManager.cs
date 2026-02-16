using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIManager : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;

    // Widgets
    [Header("Widgets")]
    [SerializeField] private BuildingWidget buildingWidgetPrefab = null;
    [SerializeField] private ResourceWidget storageResourceWidgetPrefab = null;
    [SerializeField] private ResourceWidget buildingActionResourceWidgetPrefab = null;

    private List<BuildingWidget> spawnedBuildingWidgets = new List<BuildingWidget>();
    private List<ResourceWidget> spawnedBuildingActionResourceWidgets = new List<ResourceWidget>();

    // Menus
    [Header("Menus")]
    [SerializeField] private GameObject managementMenu = null;
    [SerializeField] private GameObject buildingListsMenu = null;
    [SerializeField] private GameObject storageListsMenu = null;
    [SerializeField] private ConstructionInformationMenu buildingInformationMenu = null;
    [SerializeField] private WorkersMenuManager workersMenu = null;
    [SerializeField] private StatsMenu statsMenu;
    private Building buildingToShowStats = null;
    [SerializeField] private ContextMenuMaster contextMenuMaster = null;
    private ContextMenuUI openedContextMenu = null;
    private bool isManagementMenuOpened = false;
    private bool isContextMenuOpened = false;

    BuildingCategory lastOpenedBuildingsListCategory = BuildingCategory.Construction;
    ItemCategory lastOpenedStorageListCategory = ItemCategory.Building;

    // Menu Buttons
    [Header("Screen Buttons")]
    [SerializeField] private CustomButton openConstructionMenuButton = null;
    [SerializeField] private CustomButton openStorageMenuButton = null;
    [SerializeField] private CustomButton closeManagementMenuButton = null;
    [SerializeField] private CustomButton stopPlacingBuildingButton = null;

    [Header("Management Buttons")]
    [SerializeField] private CustomButton buildingListsMenuButton = null;
    [SerializeField] private CustomButton storageListsMenuButton = null;

    // Buildings
    [Header("Building Lists")]
    [SerializeField] private GridLayoutGroup[] buildingLists;
    [SerializeField] private CustomButton[] buildingListButtons;
    [SerializeField] private ScrollRect buildingListsScrollRect = null;
    [SerializeField] private ScrollRect storageListsScrollRect = null;
    private bool isBuildingListsMenuOpened = false;

    // Storage List
    [Header("Storage Lists")]
    [SerializeField] private GridLayoutGroup[] storageLists;
    [SerializeField] private CustomButton[] storageListButtons;
    private bool isStorageListsMenuOpened = false;

    [Header("Building Action Menu")]
    [SerializeField] private GridLayoutGroup actionResourcesLayourGroup = null;

    private Action[] buildingsButtonSelectCallbacks;
    private Action[] storageButtonSelectCallbacks;

    private void Awake()
    {
        buildingsButtonSelectCallbacks = new Action[buildingListButtons.Length];
        storageButtonSelectCallbacks = new Action[storageListButtons.Length];
    }

    private void OnEnable()
    {
        openConstructionMenuButton.onReleased += OnBuildingsMenuButtonReleased;
        openStorageMenuButton.onReleased += OnStorageMenuButtonReleased;

        buildingListsMenuButton.onReleased += OnBuildingListsButtonReleased;
        storageListsMenuButton.onReleased += OnStorageListsButtonReleased;

        closeManagementMenuButton.onReleased += CloseManagementMenu;
        stopPlacingBuildingButton.onReleased += StopPlacingBuilding;

        // Constructing
        EventBus.onBuildingWidgetBuildClicked += OnBuildingWidgetBuildClicked;
        EventBus.onBuildingPlaced += OnBuildingPlaced;

        // Loot
        //EventBus.onLootAdded += OnLootAdded;

        // Context Menu
        EventBus.onContextMenuUpgradeButtonClicked += OnContextMenuUpgradeButtonClicked;
        EventBus.onContextMenuDemolishButtonClicked += OnContextMenuDemolishButtonClicked;
        EventBus.onContextMenuWorkersButtonClicked += OnContextMenuWorkersButtonClicked;

        // Stas Menu
        EventBus.onCameraEnteredStatsMenuDistance += OnCameraEnteredStatsMenuDistance;
        EventBus.onCameraExitedStatsMenuDistance += OnCameraExitedStatsMenuDistance;

        // Building List Buttons
        System.Array buildingCategoriesEnum = System.Enum.GetValues(typeof(BuildingCategory));
        for (int i = 0; i < buildingCategoriesEnum.Length; i++) {
            int index = i;
            buildingsButtonSelectCallbacks[index] = () => OpenBuildingsListByCategory((BuildingCategory)buildingCategoriesEnum.GetValue(index));
            buildingListButtons[index].onSelected += buildingsButtonSelectCallbacks[index];
        }

        // Storage List Buttons
        System.Array itemCategoriesEnum = System.Enum.GetValues(typeof(ItemCategory));
        for (int i = 0; i < storageListButtons.Length; i++) {
            int index = i;
            storageButtonSelectCallbacks[index] = () => OpenStorageListByCategory((ItemCategory)itemCategoriesEnum.GetValue(index + 1));
            storageListButtons[index].onSelected += storageButtonSelectCallbacks[index];
        }
    }

    private void OnDisable()
    {
        openConstructionMenuButton.onReleased -= OnBuildingsMenuButtonReleased;
        openStorageMenuButton.onReleased -= OnStorageMenuButtonReleased;

        buildingListsMenuButton.onReleased -= OnBuildingListsButtonReleased;
        storageListsMenuButton.onReleased -= OnStorageListsButtonReleased;

        closeManagementMenuButton.onReleased -= CloseManagementMenu;
        stopPlacingBuildingButton.onReleased -= StopPlacingBuilding;

        // Constructing
        EventBus.onBuildingWidgetBuildClicked -= OnBuildingWidgetBuildClicked;
        EventBus.onBuildingPlaced -= OnBuildingPlaced;
        //EventBus.onStorageCapacityChanged -= OnStorageCapacityUpdated;

        // Loot
        //EventBus.onLootAdded -= OnLootAdded;

        // Context Menu
        EventBus.onContextMenuUpgradeButtonClicked -= OnContextMenuUpgradeButtonClicked;
        EventBus.onContextMenuDemolishButtonClicked -= OnContextMenuDemolishButtonClicked;
        EventBus.onContextMenuWorkersButtonClicked -= OnContextMenuWorkersButtonClicked;

        // Stas Menu
        EventBus.onCameraEnteredStatsMenuDistance -= OnCameraEnteredStatsMenuDistance;
        EventBus.onCameraExitedStatsMenuDistance -= OnCameraExitedStatsMenuDistance;

        // Building List Buttons
        System.Array buildingCategoriesEnum = System.Enum.GetValues(typeof(BuildingCategory));
        for (int i = 0; i < buildingCategoriesEnum.Length; i++) {
            int index = i;
            buildingListButtons[index].onReleased -= buildingsButtonSelectCallbacks[index];
        }

        // Storage List Buttons
        System.Array itemCategoriesEnum = System.Enum.GetValues(typeof(ItemCategory));
        for (int i = 0; i < storageListButtons.Length; i++) {
            int index = i;
            storageListButtons[index].onReleased -= storageButtonSelectCallbacks[index];
        }
    }

    private void Start()
    {
        CreateBuildingWidgets();
        CreateItemWidgets();

        managementMenu.SetActive(false);
        buildingListsMenu.SetActive(false);
        workersMenu.CloseWorkersMenu();
        stopPlacingBuildingButton.gameObject.SetActive(false);

        foreach (GridLayoutGroup rect in buildingLists) {
            rect.gameObject.SetActive(false);
        }

        foreach (GridLayoutGroup rect in storageLists) {
            rect.gameObject.SetActive(false);
        }
    }

    // Management Menu
    private void OpenManagementMenu()
    {
        managementMenu.SetActive(true);
        isManagementMenuOpened = true;
    }

    private void CloseManagementMenu()
    {
        managementMenu.SetActive(false);
        isManagementMenuOpened = false;

        ResetLastOpenedListCategoried();
    }

    private void ResetLastOpenedListCategoried()
    {
        lastOpenedBuildingsListCategory = BuildingCategory.Construction;
        lastOpenedStorageListCategory = ItemCategory.Building;
    }

    // Buildings Menu
    private void OnBuildingsMenuButtonReleased()
    {
        OpenManagementMenu();
        OpenBuildingsMenu();
        CloseStorageMenu();
        buildingListsMenuButton.SetState(CustomSelectableState.Selected);
        buildingListsMenuButton.SetStateTransitionAlpha(1f);
        storageListsMenuButton.SetStateTransitionAlpha(1f);
    }

    private void OnBuildingListsButtonReleased()
    {
        OpenBuildingsMenu();
        CloseStorageMenu();
    }

    private void OpenBuildingsMenu()
    {
        buildingListsMenu.SetActive(true);
        buildingListButtons[(int)lastOpenedBuildingsListCategory].SetState(CustomSelectableState.Selected);
        isBuildingListsMenuOpened = true;
        UpdateBuildingsMenuResourcesAmount();
    }

    private void CloseBuildingsMenu()
    {
        isBuildingListsMenuOpened = false;
        buildingListsMenu.SetActive(false);
    }

    private void OpenBuildingsListByCategory(BuildingCategory buildingCategory)
    {
        // Set initial sibling index to last Button
        int lastIndex = (int)lastOpenedBuildingsListCategory;
        buildingListButtons[lastIndex].transform.SetSiblingIndex(buildingListButtons.Length - lastIndex - 1);

        // Hide last list
        buildingLists[lastIndex].gameObject.SetActive(false);

        // Set sibling index to selected button
        int index = (int)buildingCategory;
        buildingListButtons[index].transform.SetAsLastSibling();

        // Show list
        buildingLists[index].gameObject.SetActive(true);
        buildingListsScrollRect.content = buildingLists[index].GetComponent<RectTransform>();
        lastOpenedBuildingsListCategory = buildingCategory;
    }

    private void CreateBuildingWidgets()
    {
        int categoriesCount = Enum.GetValues(typeof(BuildingCategory)).Length;

        foreach (var building in BuildingsList.Instance.Buildings) {
            if (!building) {
                Debug.LogError("building is NULL");
                continue;
            }
            if (!building.BuildingData) {
                Debug.LogError($"Building {building} does not have a Building Data");
                continue;
            }

            if (!building.BuildingData.IsDemolishable) continue;

            BuildingCategory buildingCategory = building.BuildingData.BuildingCategory;
            BuildingWidget spawnedBuildingWidget = null;
            spawnedBuildingWidget = Instantiate(buildingWidgetPrefab, transform);
            spawnedBuildingWidget.transform.SetParent(buildingLists[(int)buildingCategory].transform);
            spawnedBuildingWidgets.Add(spawnedBuildingWidget);

            spawnedBuildingWidget.Init(building);
        }

        for (int i = 0; i < categoriesCount; i++) {
            RectTransform rectTransform = buildingLists[i].GetComponent<RectTransform>();
            Vector2 initialSizeDelta = rectTransform.rect.size;
            Vector2 size = buildingLists[i].transform.childCount * (buildingLists[i].cellSize + buildingLists[i].spacing) - buildingLists[i].spacing;

            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);

            if (rectTransform.sizeDelta.y < initialSizeDelta.y) {
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, initialSizeDelta.y);
            }
        }
    }

    private void UpdateBuildingsMenuResourcesAmount()
    {
        foreach (var widget in spawnedBuildingWidgets) {
            widget.UpdateResourcesToBuild();
        }
    }

    // Storage Menu
    private void OnStorageMenuButtonReleased()
    {
        OpenManagementMenu();
        OpenStorageMenu();
        CloseBuildingsMenu();
        storageListsMenuButton.SetState(CustomSelectableState.Selected);
        storageListsMenuButton.SetStateTransitionAlpha(1f);
        buildingListsMenuButton.SetStateTransitionAlpha(1f);

    }

    private void OnStorageListsButtonReleased()
    {
        OpenStorageMenu();
        CloseBuildingsMenu();
    }

    private void OpenStorageMenu()
    {
        storageListsMenu.SetActive(true);
        //OpenStorageListByCategory(lastOpenedStorageListCategory);
        storageListButtons[(int)lastOpenedStorageListCategory - 1].SetState(CustomSelectableState.Selected);
        isStorageListsMenuOpened = true;
        //UpdateStorageMenuLootAmount();
    }

    private void CloseStorageMenu()
    {
        isStorageListsMenuOpened = false;
        storageListsMenu.SetActive(false);
    }

    private void OpenStorageListByCategory(ItemCategory itemCategory)
    {
        // Set initial sibling index to last Button
        int lastIndex = (int)lastOpenedStorageListCategory - 1;
        storageListButtons[lastIndex].transform.SetSiblingIndex(storageListButtons.Length - lastIndex - 1);

        // Hide last list
        storageLists[lastIndex].gameObject.SetActive(false);

        // Set sibling index to selected button
        int index = (int)itemCategory - 1;
        storageListButtons[index].transform.SetAsLastSibling();

        // Show list
        storageLists[index].transform.gameObject.SetActive(true);
        storageListsScrollRect.content = storageLists[index].GetComponent<RectTransform>();
        lastOpenedStorageListCategory = itemCategory;
    }

    private void CreateItemWidgets()
    {
        List<ResourceWidget> widgets = new();
        int count = ItemsList.Instance.Items.Length;
        for (int i = 0; i < count; i++) {
            ItemData itemData = ItemsList.Instance.Items[i];
            if (itemData.ItemCategory == ItemCategory.Society) continue;

            ItemCategory itemCategory = itemData.ItemCategory;

            ResourceWidget storageResourceWidget = Instantiate(storageResourceWidgetPrefab, storageLists[(int)itemCategory - 1].transform);
            widgets.Add(storageResourceWidget);

            ItemInstance item = CityManager.Instance.Inventory.items[i].item;
            storageResourceWidget.SetItem(item);
        }
        //storageResourceWidgets = widgets.ToArray();
    }

    // Workers
    private void OnContextMenuWorkersButtonClicked()
    {
        workersMenu.OpenWorkersMenu();
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

    // Building Action Menu
    private void OnContextMenuUpgradeButtonClicked()
    {
        Building building = SelectManager.Instance.selectedComponent.GetComponent<Building>();
        if (!building) return;

        CleanResourceToUpgradeWidgets();

        int nextLevelIndex = building.LevelIndex + 1;
        ItemInstance[] resourcesToUpgrade = building.ConstructionLevelsData[nextLevelIndex].ResourcesToBuild;

        for (int i = 0; i < resourcesToUpgrade.Length; i++)
        {
            ResourceWidget resourceWidget = Instantiate(buildingActionResourceWidgetPrefab, actionResourcesLayourGroup.transform);
            spawnedBuildingActionResourceWidgets.Add(resourceWidget);

            int id = resourcesToUpgrade[i].ItemData.ItemId;
            int amount = resourcesToUpgrade[i].Amount;
            int maxAmount = CityManager.Instance.Inventory.items[id].maxAmount;
            resourceWidget.SetAmount(amount, maxAmount);
        }
    }

    private void OnContextMenuDemolishButtonClicked()
    {
        Building building = SelectManager.Instance.selectedComponent.GetComponent<Building>();
        if (!building) return;

        CleanResourceToUpgradeWidgets();

        int levelIndex = building.LevelIndex;
        ItemInstance[] resourcesToUpgrade = building.ConstructionLevelsData[levelIndex].ResourcesToBuild;

        for (int i = 0; i < resourcesToUpgrade.Length; i++)
        {
            ResourceWidget resourceWidget = Instantiate(buildingActionResourceWidgetPrefab, actionResourcesLayourGroup.transform);
            spawnedBuildingActionResourceWidgets.Add(resourceWidget);

            int amount = (int)math.ceil(resourcesToUpgrade[i].Amount * CityManager.demolitionResourceRefundRate);
            resourceWidget.SetAmount(amount);
        }
    }

    // Repair Building Menu
    public void OpenRepairBuildingMenu(Building building)
    {
        CleanResourceToUpgradeWidgets();

        int nextLevelIndex = 0;
        ItemInstance[] resourcesToUpgrade = building.ConstructionLevelsData[nextLevelIndex].ResourcesToBuild;

        for (int i = 0; i < resourcesToUpgrade.Length; i++)
        {
            ResourceWidget resourceWidget = Instantiate(buildingActionResourceWidgetPrefab, actionResourcesLayourGroup.transform);
            spawnedBuildingActionResourceWidgets.Add(resourceWidget);

            int amount = resourcesToUpgrade[i].Amount;
            resourceWidget.SetAmount(amount);
        }
    }

    // Upgrade Building Menu
    private void OnUpgradeButtonClicked()
    {
        //CityManager.Instance.TryToUpgradeConstruction(SelectManager.Instance.selectedComponent.GetComponent<Building>());
    }

    private void CleanResourceToUpgradeWidgets()
    {
        for (int i = 0; i < spawnedBuildingActionResourceWidgets.Count; i++)
        {
            Destroy(spawnedBuildingActionResourceWidgets[i].gameObject);
        }

        spawnedBuildingActionResourceWidgets.Clear();
    }

    // Demolish Building
    private void OnDemolishButtonClicked()
    {

    }

    // Placing Building
    private void OnBuildingWidgetBuildClicked(BuildingWidget widget)
    {
        if (stopPlacingBuildingButton)
            stopPlacingBuildingButton.gameObject.SetActive(true);
        else
            Debug.Log("stopPlacingBuildingButton is NULL");

        CloseManagementMenu();
    }

    private void OnBuildingPlaced(Building building)
    {
        if (stopPlacingBuildingButton)
            stopPlacingBuildingButton.gameObject.SetActive(false);
    }

    private void StopPlacingBuilding()
    {
        if (stopPlacingBuildingButton)
            stopPlacingBuildingButton.gameObject.SetActive(false);
    }
}
