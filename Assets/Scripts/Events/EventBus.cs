using System;
using System.Text;
using UnityEngine;

public static class EventBus
{
    // Main Menu
    public static event Action<string> onCreateWorldButtonClicked;
    public static event Action<WorldData> onLoadWorldButtonClicked;

    // City
    public static event Action onNavMeshBaked;

    // Buildings
    public static event Action<BuildingWidget> onBuildingWidgetBuildClicked;
    public static event Action<BuildingWidget> onBuildingWidgetInformationClicked;
    public static event Action<Building> onStartedPlacingBuilding;
    public static event Action<Building> onBuildingCreated;
    public static event Action<Building> onBuildingInited;
    public static event Action<Building> onBuildingDemolished;

    // Building Module
    public static event Action<BuildingModule> onBuildingModuleInited;
    public static event Action<BuildingModule> onBuildingModuleUpgraded;
    public static event Action<BuildingModule> onBuildingModuleDemolished;

    public static event Action onStopPlacingBuildingButtonClicked;

    // Production Module
    public static event Action<ProductionModule> onClickedProductionModule;

    // Boats
    public static event Action<Boat> onBoatCreated;
    public static event Action<Boat> onBoatUnloaded;
    public static event Action<int, int> onBoatUnloadedItem;

    // Residents
    public static event Action<Human> onCitizenInited;
    public static event Action<Human> onCitizenDeleted;

    // Workers
    public static event Action<CitizenWidget> onCitizenWidgetClicked;
    public static event Action onSetedInteractBuilding;
    public static event Action onRemovedInteractBuilding;

    // Loot
    public static event Action<ItemInstance> onMainStorageItemAmountChanged;
    public static event Action<StorageItem> onMainStorageItemMaxAmountChanged;

    public static event Action<ItemInstance> onItemRemoved;
    public static event Action<ItemInstance> onLootStorageChanged;

    // Context Menu
    public static event Action onClickedContextUpgradeButton;
    public static event Action onClickedContextDemolishButton;
    public static event Action onClickedWorkersButton;

    // Building Stats Menu
    public static event Action<Building> onCameraEnteredStatsMenuDistance;
    public static event Action onCameraExitedStatsMenuDistance;

    // Select
    public static event Action<SelectComponent> onSelectedComponent;
    public static event Action<SelectComponent> onDeselectedComponent;
    public static event Action<Building> onSelectedBuilding;
    public static event Action<Building> onDeselectedBuilding;
    public static event Action<Boat> onSelectedBoat;
    public static event Action<Boat> onDeselectedBoat;

    // Settings
    public static event Action onPostProcessingToggleChanged;
    public static event Action<int> onGeneralVolumeSliderMoved;
    public static event Action<int> onMusicVolumeSliderMoved;

    // Input Listener
    public static event Action<GameObject> onPlayerClicked;

    // Click
    public static event Action<LootContainer> onClickedOnLootContainer;

    // UI
    public static event Action onWorkersMenuClosed;

    // City
    public static void InvokNavMeshBaked()
    {
        onNavMeshBaked?.Invoke();
    }

    // Buildings
    public static void InvokeBuildingWidgetBuildClicked(BuildingWidget widget)
    {
        onBuildingWidgetBuildClicked?.Invoke(widget);
    }

    public static void InvokeBuildingWidgetInformationClicked(BuildingWidget widget)
    {
        onBuildingWidgetInformationClicked?.Invoke(widget);
    }

    public static void InvokeBuildingStartPlacing(Building building)
    {
        onStartedPlacingBuilding?.Invoke(building);
    }

    public static void InvokeBuildingCreated(Building building)
    {
        onBuildingCreated?.Invoke(building);
    }

    public static void InvokeBuildingInited(Building building)
    {
        onBuildingInited?.Invoke(building);
    }

    public static void InvokeBuildingDemolished(Building building)
    {
        onBuildingDemolished?.Invoke(building);
    }

    public static void InvokeStopPlacingBuildingButtonClicked()
    {
        onStopPlacingBuildingButtonClicked?.Invoke();
    }

    // Modules
    public static void InvokeBuildingModuleInited(BuildingModule module)
    {
        onBuildingModuleInited?.Invoke(module);
    }

    public static void InvokeBuildingModuleUpgraded(BuildingModule module)
    {
        onBuildingModuleUpgraded?.Invoke(module);
    }

    public static void InvokeBuildingModuleDemolished(BuildingModule module)
    {
        onBuildingModuleDemolished?.Invoke(module);
    }

    // Production Module
    public static void InvokeClickedProductionModule(ProductionModule module)
    {
        onClickedProductionModule?.Invoke(module);
    }

    // Boats
    public static void InvokeBoatCreated(Boat boat)
    {
        onBoatCreated?.Invoke(boat);
    }

    public static void InvokeBoatExitedUnloadingState(Boat boat)
    {
        onBoatUnloaded?.Invoke(boat);
    }

    public static void InvokeBoatUnloadedItem(int id, int amount)
    {
        onBoatUnloadedItem?.Invoke(id, amount);
    }

    // Residents
    public static void InvokeCitizenInited(Human resident)
    {
        onCitizenInited?.Invoke(resident);
    }

    public static void InvokeCitizenDeleted(Human resident)
    {
        onCitizenDeleted?.Invoke(resident);
    }

    // Workers
    public static void InvokeCitizenWidgetClicked(CitizenWidget widget)
    {
        onCitizenWidgetClicked?.Invoke(widget);
    }

    public static void InvokeSetedInteractBuilding()
    {
        onSetedInteractBuilding?.Invoke();
    }

    public static void InvokeRemovedInteractBuilding()
    {
        onRemovedInteractBuilding?.Invoke();
    }

    // Loot
    public static void InvokeMainStorageAmountChanged(ItemInstance itemInstance)
    {
        onMainStorageItemAmountChanged?.Invoke(itemInstance);
    }

    public static void InvokeMainStorageMaxAmountChanged(StorageItem itemInstance)
    {
        onMainStorageItemMaxAmountChanged?.Invoke(itemInstance);
    }

    // Context Menu
    public static void InvokeUpgradeButtonClicked()
    {
        onClickedContextUpgradeButton?.Invoke();
    }

    public static void InvokeDemolishButtonClicked()
    {
        onClickedContextDemolishButton?.Invoke();
    }

    public static void InvokeWorkersButtonClicked()
    {
        onClickedWorkersButton?.Invoke();
    }

    // Stats Menu
    public static void InvokeCameraEnteredStatsMenuDistance(Building building)
    {
        onCameraEnteredStatsMenuDistance?.Invoke(building);
    }

    public static void InvokeCameraExitedStatsMenuDistance()
    {
        onCameraExitedStatsMenuDistance?.Invoke();
    }

    // Select
    public static void InvokeSelectedObject(SelectComponent selectComponent)
    {
        onSelectedComponent?.Invoke(selectComponent);
    }

    public static void InvokeDeselectedObject(SelectComponent selectComponent)
    {
        onDeselectedComponent?.Invoke(selectComponent);
    }

    public static void InvokeSelectedBuilding(Building building)
    {
        onSelectedBuilding?.Invoke(building);
    }

    public static void InvokeDeselectedBuilding(Building building)
    {
        onDeselectedBuilding?.Invoke(building);
    }

    public static void InvokeSelectedBoat(Boat boat)
    {
        onSelectedBoat?.Invoke(boat);
    }

    public static void InvokeDeselectedBoat(Boat boat)
    {
        onDeselectedBoat?.Invoke(boat);
    }

    // Input Listener
    public static void InvokeClicked(GameObject gameObject)
    {
        onPlayerClicked?.Invoke(gameObject);
    }

    // Clicks

    public static void InvokeClickedOnLootContainer(LootContainer loot)
    {
        onClickedOnLootContainer?.Invoke(loot);
    }

    // UI
    public static void InvokeWorkersMenuClosed()
    {
        onWorkersMenuClosed?.Invoke();
    }
}
