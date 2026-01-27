using System;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIManager : MonoBehaviour
{
    [SerializeField] readonly private PlayerController playerController;

    // Widgets
    [Header("Widgets")]
    [SerializeField] private BuildingWidget buildingWidgetPrefab = null;
    [SerializeField] private ResourceWidget storageResourceWidgetPrefab = null;
    [SerializeField] private ResourceWidget buildingActionResourceWidgetPrefab = null;
    [SerializeField] private ResourceWidget[] storageResourceWidgets = { };
    [SerializeField] private BuildingCharacteristicWidget buildingCharacteristicWidget = null;

    private List<List<BuildingWidget>> spawnedBuildingWidgets = new List<List<BuildingWidget>>();
    private List<ResourceWidget> spawnedBuildingActionResourceWidgets = new List<ResourceWidget>();

    // Menus
    [Header("Menus")]
    [SerializeField] private GameObject managementMenu = null;
    [SerializeField] private GameObject buildingListsMenu = null;
    [SerializeField] private GameObject storageListsMenu = null;
    [SerializeField] private ConstructionInformationMenu buildingInformationMenu = null;
    private bool isManagementMenuOpened = false;
    private bool isBuildingListsMenuOpened = false;
    private bool isStorageListsMenuOpened = false;
    private bool isContextMenuOpened = false;

    BuildingCategory lastOpenedBuildingsListCategory = BuildingCategory.Construction;
    ItemCategory lastOpenedStorageListCategory = ItemCategory.Building;

    // Menu Buttons
    [Header("Screen Buttons")]
    [SerializeField] private CustomSelectable openConstructionMenuButton = null;
    [SerializeField] private CustomSelectable openStorageMenuButton = null;
    [SerializeField] private CustomSelectable closeManagementMenuButton = null;
    [SerializeField] private CustomSelectable stopPlacingBuildingButton = null;

    [Header("Management Buttons")]
    [SerializeField] private CustomSelectable buildingListsMenuButton = null;
    [SerializeField] private CustomSelectable storageListsMenuButton = null;

    // Buildings
    [Header("Building Lists")]
    [SerializeField] private GridLayoutGroup[] buildingLists;
    [SerializeField] private CustomSelectable[] buildingListButtons;
    [SerializeField] private ScrollRect buildingListsScrollRect = null;
    [SerializeField] private ScrollRect storageListsScrollRect = null;

    // Storage List
    [Header("Storage Lists")]
    [SerializeField] private GridLayoutGroup[] storageLists;
    [SerializeField] private CustomSelectable[] storageListButtons;

    private List<bool> itemsToUpdate = new List<bool>();

    [Header("Building Stats Panel")]
    [SerializeField] private RectTransform buildingStatsPanel = null;
    [SerializeField] private TextMeshProUGUI buildingStatsPanelNameText = null;
    [SerializeField] private TextMeshProUGUI buildingStatsPanelWorkersCountText = null;
    private bool isInfoMenuOpened = false;
    private Vector2 buildingStatsPanelOpenedPosition = Vector2.zero;

    [Header("Context Menu")]
    [SerializeField] private RectTransform contextMenuTransform = null;
    [SerializeField] private Button closeBuildingWorkersMenuButton = null;
    private Vector2 buildingManagementMenuCurrentPosition = Vector2.zero;
    private const float buildingManagementMenuToggleSpeed = 15.0f;
    private ContextMenu openedContextMenu = null;

    [Header("Building Context Menu")]
    [SerializeField] private ContextMenu buildingContextMenu = null;
    [SerializeField] private ContextMenu productionContextMenu = null;
    [SerializeField] private ContextMenu storageContextMenu = null;

    [Header("Boat Context Menu")]
    [SerializeField] private ContextMenu boatContextMenu = null;

    [Header("Building Action Menu")]
    [SerializeField] private RectTransform buildingResourcesMenuPanel = null;
    [SerializeField] private RectTransform buildingResourcesMenuBackground = null;
    [SerializeField] private Button closeBuildingResourcesMenuBackgroundButton = null;
    [SerializeField] private Button closeBuildingResourcesMenuButton = null;
    [SerializeField] private GridLayoutGroup actionResourcesLayourGroup = null;
    private const float buildingResourcesMenuPanelToggleSpeed = 10.0f;
    [HideInInspector] public bool isBuildingResourcesMenuOpened = false;
    private Vector2 buildingResourcesMenuOpenedPosition = Vector2.zero;
    private Vector2 buildingResourcesMenuClosedPosition = Vector2.zero;
    private Vector2 buildingResourcesMenuCurrentPosition = Vector2.zero;

    [Header("Upgrade Menu")]
    [SerializeField] private RectTransform upgradeBuildingMenu = null;
    [SerializeField] private Button upgradeBuildingButton = null;

    [Header("Demolish Menu")]
    [SerializeField] private RectTransform demolishBuildingMenu = null;
    [SerializeField] private Button demolishBuildingButton = null;

    [Header("Repair Menu")]
    [SerializeField] private RectTransform repairBuildingMenu = null;
    [SerializeField] private Button repairBuildingButton = null;
    [SerializeField] private TextMeshProUGUI repairBuildingNameText = null;

    [Header("Building Workers Menu")]
    [SerializeField] private ResidentWidget residentWidgetPrefab = null;

    private List<ResidentWidget> spawnedBuildingWorkerEmptyWidgets = new List<ResidentWidget>();
    private List<ResidentWidget> spawnedResidentWidgets = new List<ResidentWidget>();

    [SerializeField] private RectTransform selectBuildingWorkersMenu = null;
    [SerializeField] private RectTransform buildingWorkersMenu = null;
    [SerializeField] private RectTransform unemployedResidentsMenu = null;
    [SerializeField] private RectTransform employedResidentsMenu = null;

    [SerializeField] private RectTransform haveNoUnemployedResidentsText = null;
    [SerializeField] private RectTransform haveNoEmployedResidentsText = null;

    [SerializeField] private GridLayoutGroup buildingWorkersList = null;
    [SerializeField] private GridLayoutGroup unemployedResidentsList = null;
    [SerializeField] private GridLayoutGroup employedResidentsList = null;

    [Header("FPS")]
    [SerializeField] private TextMeshProUGUI fpsText = null;

    int maxBuildingWorkersCount = 0;
    int residentWidgetsColumnCount = 0;

    int frames = 0;
    double lastFPSCounterTime = 0d;

    // Colors
    [Header("Colors")]
    [SerializeField] private ColorHolder blueColor;
    [SerializeField] private ColorHolder lightBlueColor;
    [SerializeField] private ColorHolder darkBlueColor;

    public static event Action OnBuildStopPlacing;

    private Action[] buildingsButtonSelectCallbacks;
    private Action[] storageButtonSelectCallbacks;
    private Action[] buildingsButtonDeselectCallbacks;
    private Action[] storageButtonDeselectCallbacks;

    private void Awake()
    {
        buildingsButtonSelectCallbacks = new Action[buildingListButtons.Length];
        storageButtonSelectCallbacks = new Action[storageListButtons.Length];
        buildingsButtonDeselectCallbacks = new Action[buildingListButtons.Length];
        storageButtonDeselectCallbacks = new Action[storageListButtons.Length];
    }

    private void OnEnable()
    {
        GameManager.OnConstructionPlaced += OnConstructionPlaced;
        GameManager.OnLootAdded += OnLootAdded;
        GameManager.OnStorageCapacityUpdated += OnStorageCapacityUpdated;
        GameManager.Instance.OnResidentAdded += OnResidentAdded;

        EventBus.Instance.onBuildingWidgetBuildClicked += OnBuildingWidgetBuildClicked;

        openConstructionMenuButton.onReleased += OnConstructionMenuButtonReleased;
        openStorageMenuButton.onReleased += OnStorageMenuButtonReleased;

        buildingListsMenuButton.onReleased += OnConstructionListsButtonReleased;
        storageListsMenuButton.onReleased += OnStorageListsButtonReleased;

        closeManagementMenuButton.onReleased += CloseManagementMenu;
        stopPlacingBuildingButton.onReleased += StopPlacingBuilding;

        closeBuildingResourcesMenuButton.onClick.AddListener(CloseBuildingActionMenu);
        closeBuildingResourcesMenuBackgroundButton.onClick.AddListener(CloseBuildingActionMenu);
        closeBuildingWorkersMenuButton.onClick.AddListener(CloseBuildingWorkersMenu);

        upgradeBuildingButton.onClick.AddListener(TryToUpgradeBuilding);
        demolishBuildingButton.onClick.AddListener(TryToDemolishBuilding);
        repairBuildingButton.onClick.AddListener(TryToUpgradeBuilding);

        // Building List Buttons
        System.Array buildingCategoriesEnum = System.Enum.GetValues(typeof(BuildingCategory));
        for (int i = 0; i < buildingCategoriesEnum.Length; i++) {
            int index = i;
            buildingsButtonSelectCallbacks[index] = () => OpenBuildingsListByCategory((BuildingCategory)buildingCategoriesEnum.GetValue(index));
            buildingsButtonDeselectCallbacks[index] = () => CloseBuildingsListByCategory((BuildingCategory)buildingCategoriesEnum.GetValue(index));
            buildingListButtons[index].onSelected += buildingsButtonSelectCallbacks[index];
            buildingListButtons[index].onDeselected += buildingsButtonDeselectCallbacks[index];
        }

        // Storage List Buttons
        System.Array itemCategoriesEnum = System.Enum.GetValues(typeof(ItemCategory));
        for (int i = 0; i < storageListButtons.Length; i++) {
            int index = i;
            storageButtonSelectCallbacks[index] = () => OpenStorageListByCategory((ItemCategory)itemCategoriesEnum.GetValue(index + 1));
            storageButtonDeselectCallbacks[index] = () => CloseStorageListByCategory((ItemCategory)itemCategoriesEnum.GetValue(index + 1));
            storageListButtons[index].onSelected += storageButtonSelectCallbacks[index];
            storageListButtons[index].onDeselected += storageButtonDeselectCallbacks[index];
        }
    }

    private void OnDisable()
    {
        GameManager.OnConstructionPlaced -= OnConstructionPlaced;
        GameManager.OnLootAdded -= OnLootAdded;
        GameManager.OnStorageCapacityUpdated -= OnStorageCapacityUpdated;
        GameManager.Instance.OnResidentAdded -= OnResidentAdded;

        EventBus.Instance.onBuildingWidgetBuildClicked -= OnBuildingWidgetBuildClicked;

        openConstructionMenuButton.onReleased -= OnConstructionMenuButtonReleased;
        openStorageMenuButton.onReleased -= OnStorageMenuButtonReleased;

        buildingListsMenuButton.onReleased -= OnConstructionListsButtonReleased;
        storageListsMenuButton.onReleased -= OnStorageListsButtonReleased;

        closeManagementMenuButton.onReleased -= CloseManagementMenu;
        stopPlacingBuildingButton.onReleased -= StopPlacingBuilding;

        closeBuildingResourcesMenuButton.onClick.RemoveListener(CloseBuildingActionMenu);
        closeBuildingResourcesMenuBackgroundButton.onClick.RemoveListener(CloseBuildingActionMenu);
        closeBuildingWorkersMenuButton.onClick.RemoveListener(CloseBuildingWorkersMenu);

        upgradeBuildingButton.onClick.RemoveListener(TryToUpgradeBuilding);
        demolishBuildingButton.onClick.RemoveListener(TryToDemolishBuilding);
        repairBuildingButton.onClick.RemoveListener(TryToUpgradeBuilding);

        // Building List Buttons
        System.Array buildingCategoriesEnum = System.Enum.GetValues(typeof(BuildingCategory));
        for (int i = 0; i < buildingCategoriesEnum.Length; i++) {
            int index = i;
            buildingListButtons[index].onReleased -= buildingsButtonSelectCallbacks[index];
            buildingListButtons[index].onDeselected -= buildingsButtonDeselectCallbacks[index];
        }

        // Storage List Buttons
        System.Array itemCategoriesEnum = System.Enum.GetValues(typeof(ItemCategory));
        for (int i = 0; i < storageListButtons.Length; i++) {
            int index = i;
            storageListButtons[index].onReleased -= storageButtonSelectCallbacks[index];
            storageListButtons[index].onDeselected -= storageButtonDeselectCallbacks[index];
        }
    }

    private void Start()
    {
        CreateBuildingWidgets();
        CreateItemWidgets();

        buildingResourcesMenuOpenedPosition.y = Screen.height / 2 + (buildingResourcesMenuPanel.rect.size.y / 2);
        buildingResourcesMenuCurrentPosition.y = 0;

        buildingStatsPanelOpenedPosition = buildingStatsPanel.anchoredPosition;
        buildingStatsPanel.anchoredPosition = Vector2.zero;

        for (int i = 0; i < GameManager.Instance.lootList.Items.Length; i++)
            itemsToUpdate.Add(false);

        managementMenu.SetActive(false);
        buildingListsMenu.SetActive(false);
        selectBuildingWorkersMenu.gameObject.SetActive(false);
        stopPlacingBuildingButton.gameObject.SetActive(false);

        foreach (GridLayoutGroup rect in buildingLists) {
            rect.gameObject.SetActive(false);
        }

        foreach (GridLayoutGroup rect in storageLists) {
            rect.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        // Building Management Menu
        if (isContextMenuOpened)
            buildingManagementMenuCurrentPosition.y = math.lerp(buildingManagementMenuCurrentPosition.y, contextMenuTransform.rect.size.y, buildingManagementMenuToggleSpeed * Time.deltaTime);
        else
            buildingManagementMenuCurrentPosition.y = math.lerp(buildingManagementMenuCurrentPosition.y, 0, buildingManagementMenuToggleSpeed * Time.deltaTime);

		contextMenuTransform.anchoredPosition = buildingManagementMenuCurrentPosition;

        if (isBuildingResourcesMenuOpened)
            buildingResourcesMenuCurrentPosition.y = math.lerp(buildingResourcesMenuCurrentPosition.y, buildingResourcesMenuOpenedPosition.y, buildingResourcesMenuPanelToggleSpeed * Time.deltaTime);
        else
            buildingResourcesMenuCurrentPosition.y = math.lerp(buildingResourcesMenuCurrentPosition.y, 0, buildingResourcesMenuPanelToggleSpeed * Time.deltaTime);

        buildingResourcesMenuPanel.anchoredPosition = buildingResourcesMenuCurrentPosition;

        UpdateBuildingStatsPanelPosition();
        FPSCounter();
    }

    // Management Menu
    private void CreateItemWidgets()
    {
        List<ResourceWidget> widgets = new();
        int count = GameManager.Instance.lootList.Items.Length;
        for (int i = 0; i < count; i++)
        {
            ItemData itemData = GameManager.Instance.lootList.Items[i];
            if (itemData.ItemCategory == ItemCategory.Society) continue;

            ItemCategory itemCategory = itemData.ItemCategory;

            ResourceWidget storageResourceWidget = Instantiate(storageResourceWidgetPrefab, storageLists[(int)itemCategory - 1].transform);
            widgets.Add(storageResourceWidget);

            ItemInstance item = GameManager.Instance.items[i];
            storageResourceWidget.SetItem(item);
        }
        storageResourceWidgets = widgets.ToArray(); 
    }

    private void OpenManagementMenu()
    {
        isManagementMenuOpened = true;
        managementMenu.SetActive(true);
    }

    public void CloseManagementMenu()
    {
        isManagementMenuOpened = false;
        managementMenu.SetActive(false);
    }

    private void OnConstructionMenuButtonReleased()
    {
        OpenManagementMenu();
        OnConstructionListsButtonReleased();
        UpdateLootStorageMenu();
        buildingListsMenuButton.SetState(CustomSelectableState.Selected);
        buildingListsMenuButton.SetStateTransitionAlpha(1);
        storageListsMenuButton.SetStateTransitionAlpha(0);
        buildingListButtons[0].Select();
    }

    private void OnConstructionListsButtonReleased()
    {
        UpdateBuildingWidgetsResourcesAmount();

        isBuildingListsMenuOpened = true;
        isStorageListsMenuOpened = false;

        buildingListsMenu.SetActive(true);
        storageListsMenu.SetActive(false);

        buildingListsMenuButton.Select();
        storageListsMenuButton.Deselect();

        OpenBuildingsListByCategory(lastOpenedBuildingsListCategory);
    }

    private void OpenBuildingsListByCategory(BuildingCategory buildingCategory)
    {
        CustomSelectable selectedButton = null;
        int index = (int)buildingCategory;
        buildingLists[index].gameObject.SetActive(true);
        selectedButton = buildingListButtons[index];
        selectedButton.transform.SetAsLastSibling();
        buildingListsScrollRect.content = buildingLists[(int)buildingCategory].GetComponent<RectTransform>();
        lastOpenedBuildingsListCategory = buildingCategory;
    }

    private void CloseBuildingsListByCategory(BuildingCategory buildingCategory)
    {
        int index = (int)buildingCategory;
        buildingLists[index].gameObject.SetActive(false);
        buildingListButtons[index].transform.SetSiblingIndex(buildingListButtons.Length - index);
    }

    private void OnStorageMenuButtonReleased()
    {
        OpenManagementMenu();
        OnStorageListsButtonReleased();
        storageListsMenuButton.SetState(CustomSelectableState.Selected);
        storageListsMenuButton.SetStateTransitionAlpha(1);
        buildingListsMenuButton.SetStateTransitionAlpha(0);

    }

    private void OnStorageListsButtonReleased()
    {
        isBuildingListsMenuOpened = false;
        isStorageListsMenuOpened = true;

        buildingListsMenu.SetActive(false);
        storageListsMenu.SetActive(true);

        buildingListsMenuButton.Deselect();
        storageListsMenuButton.Select();

        OpenStorageListByCategory(lastOpenedStorageListCategory);
    }

    private void OpenStorageListByCategory(ItemCategory itemCategory)
    {
        CustomSelectable selectedButton = null;
        int index = (int)itemCategory - 1;
        storageLists[index].gameObject.SetActive(true);
        selectedButton = storageListButtons[index];
        selectedButton.transform.SetAsLastSibling();
        lastOpenedStorageListCategory = itemCategory;
        storageListsScrollRect.content = buildingLists[index].GetComponent<RectTransform>();
    }

    private void CloseStorageListByCategory(ItemCategory itemCategory)
    {
        int index = (int)itemCategory - 1;
        storageLists[index].gameObject.SetActive(false);
        storageListButtons[index].transform.SetSiblingIndex(storageListButtons.Length - index);
    }

    private void CreateBuildingWidgets()
    {

        int length = Enum.GetValues(typeof(BuildingCategory)).Length;
        for (int i = 0; i < length; i++) {
            spawnedBuildingWidgets.Add(new List<BuildingWidget>());
        }

        foreach (var building in GameManager.Instance.buildingsList.buildings) {
            if (!building.BuildingData.IsDemolishable) continue;

            BuildingCategory buildingCategory = building.BuildingData.BuildingCategory;
            BuildingWidget spawnedBuildingWidget = null;
            spawnedBuildingWidget = Instantiate(buildingWidgetPrefab, transform);

            int categoryIndex = (int)building.BuildingData.BuildingCategory;
            spawnedBuildingWidgets[categoryIndex].Add(spawnedBuildingWidget);

            ConstructionComponent construction = building.GetComponent<ConstructionComponent>();
            spawnedBuildingWidget.InitializeBuildingWidget(construction);

            spawnedBuildingWidget.transform.SetParent(buildingLists[(int)buildingCategory].transform);
        }

        for (int i = 0; i < length; i++) {
            RectTransform rectTransform = buildingLists[i].GetComponent<RectTransform>();
            Vector2 initialSizeDelta = rectTransform.rect.size;
            Vector2 size = buildingLists[i].transform.childCount * (buildingLists[i].cellSize + buildingLists[i].spacing) - buildingLists[i].spacing;

            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);

            if (rectTransform.sizeDelta.y < initialSizeDelta.y)
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, initialSizeDelta.y);
        }
    }

    // Loot
    private void OnLootAdded(ItemInstance item)
    {
        if (isStorageListsMenuOpened) {
            int id = item.ItemData.ItemId;
            int amount = item.Amount;
            int maxAmount = GameManager.Instance.totalStorageCapacity[id];
            storageResourceWidgets[id].SetAmountAndMaxAmount(amount, maxAmount);
        }
    }

    private void OnStorageCapacityUpdated()
    {
        if (isStorageListsMenuOpened)
            UpdateLootStorageMenu();
    }

    private void UpdateLootStorageMenu()
    {
        // Update Storage Menu
        for (int i = 0; i < storageResourceWidgets.Length; i++) {
            int amount = GameManager.Instance.items[i].Amount;
            int maxAmount = GameManager.Instance.totalStorageCapacity[i];

            if (storageResourceWidgets.Length > i)
                storageResourceWidgets[i].SetAmountAndMaxAmount(amount, maxAmount);
            else
                Debug.LogError("storageResourceWidgets.Count > indexes[i]");
        }

        // Update Construction Menu
        for (int i = 0; i < spawnedBuildingWidgets.Count; i++)
        {
            for (int j = 0; j < spawnedBuildingWidgets[i].Count; j++)
            {
                spawnedBuildingWidgets[i][j].UpdateResourcesToBuild();
            }
        }
    }

    public void UpdateBuildingWidgetsResourcesAmount()
    {
        for (int i = 0; i < spawnedBuildingWidgets.Count; i++)
        {
            for (int j = 0; j < spawnedBuildingWidgets[i].Count; j++)
            {
                spawnedBuildingWidgets[i][j].UpdateResourcesToBuild();
            }
        }
    }

    // Context Menus
    public void OpenContextMenu(ISelectable selectedObject)
    {
        if (openedContextMenu)
            openedContextMenu.gameObject.SetActive(false);

        MonoBehaviour mb = selectedObject as MonoBehaviour;
        isContextMenuOpened = true;
        isInfoMenuOpened = false;

        Building building = mb.GetComponent<Building>();
        Creature entity = mb.GetComponent<Creature>();
        Boat boat = mb.GetComponent<Boat>();

        if (building) {
            if (building.productionComponent)
                openedContextMenu = productionContextMenu;
            else if (building.storageComponent)
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
            openedContextMenu.Open(mb);
        }
    }

    public void CloseContextMenu()
    {
        isContextMenuOpened = false;
    }

    public void OnBuildingUpgraded(Building building)
    {
        //buildingManagementMenuNameText.SetText(building.buildingData.buildingName);
        //buildingManagementMenuLevelText.SetText("Level " + (building.levelIndex + 1).ToString());

        //List<int> indexes = new List<int>();

        //List<ItemInstance> previousResourcesToUpgrade = building.buildingLevelsData[building.levelIndex].resourcesToBuild;

        //for (int i = 0; i < previousResourcesToUpgrade.Count; i++)
        //    indexes.Add(previousResourcesToUpgrade[i].ItemData.ItemId);

        //UpdateStorageItemsByIndexes(indexes);

        //selectedBuilding = null;
    }

    // Building Stats Panels
    public void OpenBuildingStatsPanel(Building building)
    {
        isInfoMenuOpened = true;
        buildingStatsPanelNameText.SetText(building.BuildingData.BuildingName);
        buildingStatsPanelWorkersCountText.SetText(building.workers.Count + "/" + building.ConstructionLevelsData[building.LevelIndex].maxResidentsCount);
    }

    public void CloseBuildingStatsPanel()
    {
        isInfoMenuOpened = false;
    }

    private void UpdateBuildingStatsPanelPosition()
    {
        if (isInfoMenuOpened)
        {
            buildingStatsPanel.anchoredPosition = Vector3.Lerp(buildingStatsPanel.anchoredPosition, buildingStatsPanelOpenedPosition, buildingManagementMenuToggleSpeed * Time.deltaTime);
        }
        else
        {
            buildingStatsPanel.anchoredPosition = Vector3.Lerp(buildingStatsPanel.anchoredPosition, Vector2.zero, buildingManagementMenuToggleSpeed * Time.deltaTime);
        }
    }

    // Building Action Menu
    public void OpenBuildingActionMenu()
    {
        isBuildingResourcesMenuOpened = true;
        buildingResourcesMenuBackground.gameObject.SetActive(true);
    }

    public void CloseBuildingActionMenu()
    {
        isBuildingResourcesMenuOpened = false;
        buildingResourcesMenuBackground.gameObject.SetActive(false);
    }

    public void OpenUpgradeBuildingMenu()
    {
        Building building = (playerController.selectedObject as MonoBehaviour).GetComponent<Building>();
        if (!building) return;

        OpenBuildingActionMenu();
        CleanResourceToUpgradeWidgets();

        int nextLevelIndex = building.LevelIndex + 1;
        ItemInstance[] resourcesToUpgrade = building.ConstructionLevelsData[nextLevelIndex].ResourcesToBuild;

        for (int i = 0; i < resourcesToUpgrade.Length; i++)
        {
            ResourceWidget resourceWidget = Instantiate(buildingActionResourceWidgetPrefab, actionResourcesLayourGroup.transform);
            spawnedBuildingActionResourceWidgets.Add(resourceWidget);

            int id = resourcesToUpgrade[i].ItemData.ItemId;
            int amount = resourcesToUpgrade[i].Amount;
            int maxAmount = GameManager.Instance.totalStorageCapacity[id];
            resourceWidget.SetAmountAndMaxAmount(amount, maxAmount);
        }

        upgradeBuildingMenu.gameObject.SetActive(true);
        demolishBuildingMenu.gameObject.SetActive(false);
        repairBuildingMenu.gameObject.SetActive(false);
    }

    public void OpenDemolishBuildingMenu()
    {
        Building building = (playerController.selectedObject as MonoBehaviour).GetComponent<Building>();
        if (!building) return;

        OpenBuildingActionMenu();
        CleanResourceToUpgradeWidgets();

        int levelIndex = building.LevelIndex;
        ItemInstance[] resourcesToUpgrade = building.ConstructionLevelsData[levelIndex].ResourcesToBuild;

        for (int i = 0; i < resourcesToUpgrade.Length; i++)
        {
            ResourceWidget resourceWidget = Instantiate(buildingActionResourceWidgetPrefab, actionResourcesLayourGroup.transform);
            spawnedBuildingActionResourceWidgets.Add(resourceWidget);

            int amount = (int)math.ceil(resourcesToUpgrade[i].Amount * GameManager.demolitionResourceRefundRate);
            resourceWidget.SetAmount(amount);
        }

        upgradeBuildingMenu.gameObject.SetActive(false);
        demolishBuildingMenu.gameObject.SetActive(true);
        repairBuildingMenu.gameObject.SetActive(false);
    }

    // Building Workers Menu
    public void OpenBuildingWorkersMenu()
    {
        Building building = (playerController.selectedObject as MonoBehaviour).GetComponent<Building>();
        if (!building) return;

        maxBuildingWorkersCount = building.ConstructionLevelsData[building.LevelIndex].maxResidentsCount;

        residentWidgetsColumnCount = (int)(buildingWorkersList.GetComponent<RectTransform>().rect.width / buildingWorkersList.cellSize.x);

        selectBuildingWorkersMenu.gameObject.SetActive(true);

        // Set parents of resident widgets
        int buildingWorkerWidgetIndex = 0;
        for (int i = 0; i < GameManager.Instance.residents.Count; i++) {
            spawnedResidentWidgets[i].InitializeResidentWidget(GameManager.Instance.residents[i], building, this);

            if (GameManager.Instance.residents[i].workBuilding) {
                if (GameManager.Instance.residents[i].workBuilding == building)
                {
                    spawnedResidentWidgets[i].transform.SetParent(buildingWorkersList.transform);
                    spawnedResidentWidgets[i].transform.SetSiblingIndex(buildingWorkerWidgetIndex);
                    buildingWorkerWidgetIndex++;
                }
                else {
                    spawnedResidentWidgets[i].transform.SetParent(employedResidentsList.transform);
                }
            }
            else {
                spawnedResidentWidgets[i].transform.SetParent(unemployedResidentsList.transform);
            }
        }

        // Create empty resident widgets
        int emptyResidentWidgetsCount = spawnedBuildingWorkerEmptyWidgets.Count;
        if (emptyResidentWidgetsCount < maxBuildingWorkersCount)
        {
            for (int i = emptyResidentWidgetsCount; i < maxBuildingWorkersCount; i++)
            {
                ResidentWidget emptyResidentWidget = Instantiate(residentWidgetPrefab);
                emptyResidentWidget.InitializeResidentWidget(null, building, this);
                spawnedBuildingWorkerEmptyWidgets.Add(emptyResidentWidget);
                emptyResidentWidget.transform.SetParent(buildingWorkersList.transform);
                emptyResidentWidget.transform.localScale = Vector3.one;
            }
        }
        else
        {
            for (int i = emptyResidentWidgetsCount - 1; i >= maxBuildingWorkersCount; i--)
            {
                Destroy(spawnedBuildingWorkerEmptyWidgets[i].gameObject);
                spawnedBuildingWorkerEmptyWidgets.RemoveAt(i);
            }
        }

        for (int i = 0; i < building.workers.Count; i++)
        {
            spawnedBuildingWorkerEmptyWidgets[i].gameObject.SetActive(false);
        }

        for (int i = building.workers.Count; i < maxBuildingWorkersCount; i++)
        {
            spawnedBuildingWorkerEmptyWidgets[i].gameObject.SetActive(true);
        }

        UpdateWorkerListsSize();
    }

    private void OnResidentAdded(Creature resident)
    {
        ResidentWidget residentWidget = Instantiate(residentWidgetPrefab, unemployedResidentsList.transform);
        residentWidget.transform.localScale = Vector3.one;
        spawnedResidentWidgets.Add(residentWidget);
    }

    private void SetWorkerListSize(RectTransform workersMenu, GridLayoutGroup gridLayoutGroup, RectTransform haveNoResidentsText, int residentsCount, int residentWidgetsColumnCount)
    {
        gridLayoutGroup.constraintCount = residentWidgetsColumnCount;
        RectTransform menuRectTransform = gridLayoutGroup.GetComponent<RectTransform>();
        int WidgetsRowCount = (int)math.ceil((float)residentsCount / (float)residentWidgetsColumnCount);
        int YSize = 0;
        Vector2 ListSize = Vector2.zero;

        //Debug.Log(residentsCount);

        if (residentsCount > 0)
        {
            YSize = (int)((menuRectTransform.offsetMin.y - menuRectTransform.offsetMax.y) + (gridLayoutGroup.cellSize.y * WidgetsRowCount) + (gridLayoutGroup.spacing.y * (WidgetsRowCount - 1)));


            if (haveNoResidentsText)
                haveNoResidentsText.gameObject.SetActive(false);
        }
        else
        {
            YSize = (int)(menuRectTransform.offsetMin.y - menuRectTransform.offsetMax.y);

            if (haveNoResidentsText)
            {
                YSize += (int)haveNoResidentsText.sizeDelta.y;
                haveNoResidentsText.gameObject.SetActive(true);
            }
        }

        workersMenu.sizeDelta = new Vector2(workersMenu.sizeDelta.x, YSize);
    }

    public void UpdateWorkerListsSize()
    {
        Building building = (playerController.selectedObject as MonoBehaviour).GetComponent<Building>();
        if (!building) return;

        // Building workers
        SetWorkerListSize(buildingWorkersMenu, buildingWorkersList, null, maxBuildingWorkersCount, residentWidgetsColumnCount);
        // Unemployed residents
        SetWorkerListSize(unemployedResidentsMenu, unemployedResidentsList, haveNoUnemployedResidentsText, GameManager.Instance.unemployedResidentsCount, residentWidgetsColumnCount);
        // Employed residents
        SetWorkerListSize(employedResidentsMenu, employedResidentsList, haveNoEmployedResidentsText, GameManager.Instance.employedResidentCount - building.workers.Count, residentWidgetsColumnCount);
    }

    public void SelectBuildingWorker(ResidentWidget residentWidget)
    {
        Building building = (playerController.selectedObject as MonoBehaviour).GetComponent<Building>();
        if (!building) return;

        Creature resident = residentWidget.resident;
        int workersCount = building.workers.Count;

        if (resident.workBuilding) {
            if (resident.workBuilding == building) {
                residentWidget.transform.SetParent(buildingWorkersList.transform);
                residentWidget.transform.SetSiblingIndex(workersCount - 1);

                int index = building.workers.Count - 1;
                spawnedBuildingWorkerEmptyWidgets[index].gameObject.SetActive(false);
            }
            else {
                residentWidget.transform.SetParent(employedResidentsList.transform);

                if (workersCount < maxBuildingWorkersCount) {
                    int index = maxBuildingWorkersCount - (maxBuildingWorkersCount - workersCount);
                    spawnedBuildingWorkerEmptyWidgets[index].gameObject.SetActive(true);
                }
            }
        }
        else {
            residentWidget.transform.SetParent(unemployedResidentsList.transform);

            if (workersCount < maxBuildingWorkersCount) {
                int index = maxBuildingWorkersCount - (maxBuildingWorkersCount - workersCount);
                spawnedBuildingWorkerEmptyWidgets[index].gameObject.SetActive(true);
            }
        }

        UpdateWorkerListsSize();
    }

    private void CloseBuildingWorkersMenu()
    {
        selectBuildingWorkersMenu.gameObject.SetActive(false);
    }

    // Repair Building Menu
    public void OpenRepairBuildingMenu(Building building)
    {
        CloseContextMenu();
        OpenBuildingActionMenu();
        CleanResourceToUpgradeWidgets();

        repairBuildingNameText.SetText(building.BuildingData.BuildingName + " (Ruin)");

        int nextLevelIndex = 0;
        ItemInstance[] resourcesToUpgrade = building.ConstructionLevelsData[nextLevelIndex].ResourcesToBuild;

        for (int i = 0; i < resourcesToUpgrade.Length; i++)
        {
            ResourceWidget resourceWidget = Instantiate(buildingActionResourceWidgetPrefab, actionResourcesLayourGroup.transform);
            spawnedBuildingActionResourceWidgets.Add(resourceWidget);

            int amount = resourcesToUpgrade[i].Amount;
            resourceWidget.SetAmount(amount);
        }

        upgradeBuildingMenu.gameObject.SetActive(false);
        demolishBuildingMenu.gameObject.SetActive(false);
        repairBuildingMenu.gameObject.SetActive(true);
    }

    // Upgrade Building Menu
    private void TryToUpgradeBuilding()
    {
        GameManager.Instance.TryToUpgradeConstruction((playerController.selectedObject as MonoBehaviour).GetComponent<Building>());
    }

    private void TryToDemolishBuilding()
    {
        OpenBuildingActionMenu();

        //selectedBuilding.OnConstructionDemolised();


        CloseContextMenu();
    }

    private void CleanResourceToUpgradeWidgets()
    {
        for (int i = 0; i < spawnedBuildingActionResourceWidgets.Count; i++)
        {
            Destroy(spawnedBuildingActionResourceWidgets[i].gameObject);
        }

        spawnedBuildingActionResourceWidgets.Clear();
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

    private void OnConstructionPlaced()
    {
        if (stopPlacingBuildingButton)
            stopPlacingBuildingButton.gameObject.SetActive(false);
    }

    private void StopPlacingBuilding()
    {
        if (stopPlacingBuildingButton)
            stopPlacingBuildingButton.gameObject.SetActive(false);

        OnBuildStopPlacing?.Invoke();
    }

    private void FPSCounter()
    {
        frames++;
        double time = Time.timeAsDouble;

        if (time >= lastFPSCounterTime + 0.5)
        {
            double delta = time - lastFPSCounterTime;
            float fps = frames / (float)delta;
            fpsText.text = $"FPS: {(int)fps}";
            frames = 0;
            lastFPSCounterTime = time;
        }
    }
}
