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
    [SerializeField] private ContextMenuManager contextMenuMaster = null;
    private ContextMenuBase openedContextMenu = null;

    public bool isManagementMenuOpened { get; private set; } = false;
    public bool isWorkersMenuOpened { get; private set; } = false;
    private bool isContextMenuOpened = false;

    BuildingCategory lastOpenedBuildingsListCategory = BuildingCategory.Construction;
    ItemCategory lastOpenedStorageListCategory = ItemCategory.Building;

    // Menu Buttons
    [Header("Screen Buttons")]
    [SerializeField] private CustomButton openConstructionMenuButton = null;
    [SerializeField] private CustomButton openStorageMenuButton = null;
    [SerializeField] private CustomButton closeManagementMenuButton = null;

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

        EventBus.onBuildingWidgetBuildClicked += OnBuildingWidgetBuildClicked;

        // Stas Menu
        EventBus.onCameraEnteredStatsMenuDistance += OnCameraEnteredStatsMenuDistance;
        EventBus.onCameraExitedStatsMenuDistance += OnCameraExitedStatsMenuDistance;

        // Building List Buttons
        System.Array buildingCategoriesEnum = System.Enum.GetValues(typeof(BuildingCategory));
        for (int i = 0; i < buildingCategoriesEnum.Length; i++) {
            int index = i;
            buildingsButtonSelectCallbacks[index] = () => OnBuildingsListButtonClicked((BuildingCategory)buildingCategoriesEnum.GetValue(index));
            buildingListButtons[index].onSelected += buildingsButtonSelectCallbacks[index];
        }

        // Storage List Buttons
        System.Array itemCategoriesEnum = System.Enum.GetValues(typeof(ItemCategory));
        for (int i = 0; i < storageListButtons.Length; i++) {
            int index = i;
            storageButtonSelectCallbacks[index] = () => OpenStorageListByCategory((ItemCategory)itemCategoriesEnum.GetValue(index + 1));
            storageListButtons[index].onSelected += storageButtonSelectCallbacks[index];
        }

        // Workers Menu
        EventBus.onWorkersMenuClosed += OnWorkersMenuClosed;
    }

    private void OnDisable()
    {
        openConstructionMenuButton.onReleased -= OnBuildingsMenuButtonReleased;
        openStorageMenuButton.onReleased -= OnStorageMenuButtonReleased;

        buildingListsMenuButton.onReleased -= OnBuildingListsButtonReleased;
        storageListsMenuButton.onReleased -= OnStorageListsButtonReleased;

        closeManagementMenuButton.onReleased -= CloseManagementMenu;

        EventBus.onBuildingWidgetBuildClicked -= OnBuildingWidgetBuildClicked;

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

        // Workers Menu
        EventBus.onWorkersMenuClosed -= OnWorkersMenuClosed;
    }

    private void Start()
    {
        CreateBuildingWidgets();
        CreateItemWidgets();

        managementMenu.SetActive(false);
        buildingListsMenu.SetActive(false);

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

        //ResetLastOpenedListCategoried();
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

        CloseBuildingsListByCategory(lastOpenedBuildingsListCategory);
        ResetLastOpenedListCategoried();
        OpenBuildingsListByCategory(lastOpenedBuildingsListCategory);

        CloseStorageMenu();
        buildingListsMenuButton.SetState(CustomSelectableState.Selected);
        buildingListsMenuButton.FinishTransitionAnimation();
        storageListsMenuButton.FinishTransitionAnimation();

        buildingListButtons[(int)lastOpenedBuildingsListCategory].SetState(CustomSelectableState.Selected);
    }

    private void OnBuildingListsButtonReleased()
    {
        OpenBuildingsMenu();
        CloseStorageMenu();
    }

    private void OpenBuildingsMenu()
    {
        buildingListsMenu.SetActive(true);
        isBuildingListsMenuOpened = true;
    }

    private void CloseBuildingsMenu()
    {
        isBuildingListsMenuOpened = false;
        buildingListsMenu.SetActive(false);
    }

    private void OnBuildingsListButtonClicked(BuildingCategory category)
    {
        CloseBuildingsListByCategory(lastOpenedBuildingsListCategory);
        OpenBuildingsListByCategory(category);

        int index = (int)category;
        buildingListsScrollRect.content = buildingLists[index].GetComponent<RectTransform>();
        lastOpenedBuildingsListCategory = category;
    }

    private void OpenBuildingsListByCategory(BuildingCategory category)
    {
        int index = (int)category;
        buildingListButtons[index].transform.SetAsLastSibling();
        buildingLists[index].gameObject.SetActive(true);
    }

    private void CloseBuildingsListByCategory(BuildingCategory category)
    {
        int lastIndex = (int)category;

        buildingListButtons[lastIndex].transform.SetSiblingIndex(buildingListButtons.Length - lastIndex - 1);
        buildingLists[lastIndex].gameObject.SetActive(false);
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

    // Storage Menu
    private void OnStorageMenuButtonReleased()
    {
        OpenManagementMenu();
        OpenStorageMenu();
        CloseBuildingsMenu();
        storageListsMenuButton.SetState(CustomSelectableState.Selected);
        storageListsMenuButton.FinishTransitionAnimation();
        buildingListsMenuButton.FinishTransitionAnimation();

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
            ItemInstance item = cityStorage.Inventory.items[i].item;
            ItemData itemData = item.ItemData;

            if (itemData.ItemCategory == ItemCategory.Society)
                continue;

            ItemCategory itemCategory = itemData.ItemCategory;

            ResourceWidget storageResourceWidget = Instantiate(storageResourceWidgetPrefab, storageLists[(int)itemCategory - 1].transform);
            widgets.Add(storageResourceWidget);

            storageResourceWidget.Init(item);
        }
    }

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

    // Events
    private void OnBuildingWidgetBuildClicked(BuildingWidget widget)
    {
        CloseManagementMenu();
    }
}
