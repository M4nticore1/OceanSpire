using System;
using UnityEngine;

public static class EventBus
{
    // Main Menu
    public static event Action<string> onCreateWorldButtonClicked;
    public static event Action<SaveData> onLoadWorldButtonClicked;

    // Constructing
    public static event Action<BuildingPlace> onBuildingPlacePressed;
    public static event Action<BuildingWidget> onBuildingWidgetBuildClicked;
    public static event Action<BuildingWidget> onBuildingWidgetInformationClicked;
    public static event Action onConstructionStartPlacing;
    public static event Action<Building> onBuildingInitialized;
    public static event Action<ConstructionComponent> onConstructionPlaced;
    public static event Action<ConstructionComponent> onConstructionBuilt;
    public static event Action<ConstructionComponent> onConstructionDemolished;

    // Production Module
    public static event Action<ProductionBuildingModule> onProductionModuleClicked;

    // Residents
    public static event Action<Creature> onResidentAdded;
    public static event Action<Creature> onResidentRemoved;

    // Workers
    public static event Action<ResidentWidget> onResidentWidgetClicked;

    // Loot
    public static event Action<ItemInstance> onItemAdded;
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

    // Constructing
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

    public static void InvokeBuildingInitialized(Building building)
    {
        onBuildingInitialized?.Invoke(building);
    }

    public static void InvokeConstructionPlaced(ConstructionComponent constructionComponent)
    {
        onConstructionPlaced?.Invoke(constructionComponent);
    }

    public static void InvokeConstructionBuilt(ConstructionComponent constructionComponent)
    {
        onConstructionBuilt?.Invoke(constructionComponent);
    }

    public static void InvokeConstructionDemolished(ConstructionComponent constructionComponent)
    {
        onConstructionDemolished?.Invoke(constructionComponent);
    }

    // Production Module
    public static void InvokeProductionModuleClicked(ProductionBuildingModule module)
    {
        onProductionModuleClicked?.Invoke(module);
    }

    // Residents
    public static void InvokeResidentAdded(Creature resident)
    {
        onResidentAdded?.Invoke(resident);
    }

    public static void InvokeResidentRemoved(Creature resident)
    {
        onResidentRemoved?.Invoke(resident);
    }

    // Workers
    public static void InvokeResidentWidgetClicked(ResidentWidget widget)
    {
        onResidentWidgetClicked?.Invoke(widget);
    }

    // Loot
    public static void InvokeItemAdded(ItemInstance itemInstance)
    {
        Debug.Log("InvokeItemAdded");
        onItemAdded?.Invoke(itemInstance);
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
