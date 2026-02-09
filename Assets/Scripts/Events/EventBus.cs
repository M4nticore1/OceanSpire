using System;
using UnityEngine;

public static class EventBus
{
    // Main Menu
    public static event Action<string> onCreateWorldButtonClicked;
    public static event Action<SaveData> onLoadWorldButtonClicked;

    // Buildings
    public static event Action<BuildingPlace> onBuildingPlacePressed;
    public static event Action<BuildingWidget> onBuildingWidgetBuildClicked;
    public static event Action<BuildingWidget> onBuildingWidgetInformationClicked;
    public static event Action<Building> onBuildingStartPlacing;
    public static event Action<Building> onBuildingFinishPlacing;
    public static event Action<Building> onBuildingInitialized;
    public static event Action<Building> onBuildingPlaced;

    public static event Action<BuildingModule> onBuildingModuleInited;
    public static event Action<BuildingModule> onBuildingModuleUpgraded;
    public static event Action<BuildingModule> onBuildingModuleDemolished;

    // Production Module
    public static event Action<ProductionBuildingModule> onProductionModuleClicked;

    // Residents
    public static event Action<Human> onCitizenAdded;
    public static event Action<Human> onResidentRemoved;

    // Workers
    public static event Action<CitizenWidget> onCitizenWidgetClicked;
    public static event Action onSetedInteractBuilding;
    public static event Action onRemovedInteractBuilding;

    // Loot
    public static event Action<ItemInstance> onMainStorageItemAmountChanged;
    public static event Action<ItemInstance> onItemRemoved;
    public static event Action<ItemInstance> onLootStorageChanged;
    public static event Action onStorageCapacityChanged;

    // Context Menu
    public static event Action onContextMenuUpgradeButtonClicked;
    public static event Action onContextMenuDemolishButtonClicked;
    public static event Action onContextMenuWorkersButtonClicked;

    // Building Stats Menu
    public static event Action<Building> onCameraEnteredStatsMenuDistance;
    public static event Action onCameraExitedStatsMenuDistance;

    // Select Object
    public static event Action<SelectComponent> onObjectSelected;
    public static event Action onObjectDeselected;

    // Settings
    public static event Action onPostProcessingToggleChanged;
    public static event Action<int> onGeneralVolumeSliderMoved;
    public static event Action<int> onMusicVolumeSliderMoved;

    // Buildings
    public static void InvokeBuildingPlacePressed(BuildingPlace place)
    {
        onBuildingPlacePressed?.Invoke(place);
    }

    public static void InvokeBuildingWidgetBuildClicked(BuildingWidget widget)
    {
        onBuildingWidgetBuildClicked?.Invoke(widget);
    }

    public static void InvokeBuildingWidgetInformationClicked(BuildingWidget widget)
    {
        onBuildingWidgetInformationClicked?.Invoke(widget);
    }

    public static void InvokeOnBuildingStartPlacing(Building building)
    {
        onBuildingStartPlacing?.Invoke(building);
    }

    public static void InvokeOnBuildingFinishPlacing(Building building)
    {
        onBuildingFinishPlacing?.Invoke(building);
    }

    public static void InvokeBuildingInitialized(Building building)
    {
        onBuildingInitialized?.Invoke(building);
    }

    public static void InvokeBuildingPlaced(Building building)
    {
        onBuildingPlaced?.Invoke(building);
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
    public static void InvokeProductionModuleClicked(ProductionBuildingModule module)
    {
        onProductionModuleClicked?.Invoke(module);
    }

    // Residents
    public static void InvokeCitizenAdded(Human resident)
    {
        onCitizenAdded?.Invoke(resident);
    }

    public static void InvokeResidentRemoved(Human resident)
    {
        onResidentRemoved?.Invoke(resident);
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

    public static void InvokeStorageCapacityChanged()
    {
        onStorageCapacityChanged?.Invoke();
    }

    // Context Menu
    public static void InvokeContextMenuUpgradeButtonClicked()
    {
        onContextMenuUpgradeButtonClicked?.Invoke();
    }

    public static void InvokeContextMenuDemolishButtonClicked()
    {
        onContextMenuUpgradeButtonClicked?.Invoke();
    }

    public static void InvokeContextMenuWorkersButtonClicked()
    {
        onContextMenuWorkersButtonClicked?.Invoke();
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
    public static void InvokeObjectSelected(SelectComponent selectComponent)
    {
        onObjectSelected?.Invoke(selectComponent);
    }

    public static void InvokeObjectDeselected()
    {
        onObjectDeselected?.Invoke();
    }
}
