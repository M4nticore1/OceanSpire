using System;
using UnityEngine;

public class EventBus
{
    private static EventBus _instance;
    public static EventBus Instance => _instance ??= new EventBus();

    // Main Menu
    public event Action<string> onCreateWorldButtonClicked;
    public event Action<SaveData> onLoadWorldButtonClicked;

    // Constructing
    public event Action<BuildingPlace> onBuildingPlacePressed;
    public event Action<BuildingWidget> onBuildingWidgetBuildClicked;
    public event Action<BuildingWidget> onBuildingWidgetInformationClicked;
    public event Action onConstructionStartPlacing;
    public event Action<Building> onBuildingInitialized;
    public event Action<ConstructionComponent> onConstructionPlaced;
    public event Action<ConstructionComponent> onConstructionBuilt;
    public event Action<ConstructionComponent> onConstructionDemolished;

    // Residents
    public event Action<Creature> onResidentAdded;
    public event Action<Creature> onResidentRemoved;

    // Workers
    public event Action<ResidentWidget> onResidentWidgetClicked;

    // Loot
    public event Action<ItemInstance> onLootAdded;
    public event Action<ItemInstance> onLootRemoved;
    public event Action<ItemInstance> onLootStorageChanged;
    public event Action onStorageCapacityChanged;

    // Context Menu
    public event Action onContextMenuUpgradeButtonClicked;
    public event Action onContextMenuDemolishButtonClicked;
    public event Action onContextMenuWorkersButtonClicked;

    // Building Stats Menu
    public event Action<Building> onCameraEnteredStatsMenuDistance;
    public event Action onCameraExitedStatsMenuDistance;

    // Select Object
    public event Action<SelectComponent> onObjectSelected;
    public event Action onObjectDeselected;

    private EventBus()
    {

    }

    // Constructing
    public void InvokeBuildingPlacePressed(BuildingPlace place)
    {
        onBuildingPlacePressed?.Invoke(place);
    }

    public void InvokeBuildingWidgetBuildClicked(BuildingWidget widget)
    {
        onBuildingWidgetBuildClicked?.Invoke(widget);
    }

    public void InvokeBuildingWidgetInformationClicked(BuildingWidget widget)
    {
        onBuildingWidgetInformationClicked?.Invoke(widget);
    }

    public void InvokeBuildingInitialized(Building building)
    {
        onBuildingInitialized?.Invoke(building);
    }

    public void InvokeConstructionPlaced(ConstructionComponent constructionComponent)
    {
        onConstructionPlaced?.Invoke(constructionComponent);
    }

    public void InvokeConstructionBuilt(ConstructionComponent constructionComponent)
    {
        onConstructionBuilt?.Invoke(constructionComponent);
    }

    public void InvokeConstructionDemolished(ConstructionComponent constructionComponent)
    {
        onConstructionDemolished?.Invoke(constructionComponent);
    }

    // Residents
    public void InvokeResidentAdded(Creature resident)
    {
        onResidentAdded?.Invoke(resident);
    }

    public void InvokeResidentRemoved(Creature resident)
    {
        onResidentRemoved?.Invoke(resident);
    }

    // Workers
    public void InvokeResidentWidgetClicked(ResidentWidget widget)
    {
        onResidentWidgetClicked?.Invoke(widget);
    }

    // Loot
    public void InvokeLootAdded(ItemInstance itemInstance)
    {
        onLootAdded?.Invoke(itemInstance);
    }

    public void InvokeStorageCapacityChanged()
    {
        onStorageCapacityChanged?.Invoke();
    }

    // Context Menu
    public void InvokeContextMenuUpgradeButtonClicked()
    {
        onContextMenuUpgradeButtonClicked?.Invoke();
    }

    public void InvokeContextMenuDemolishButtonClicked()
    {
        onContextMenuUpgradeButtonClicked?.Invoke();
    }

    public void InvokeContextMenuWorkersButtonClicked()
    {
        onContextMenuWorkersButtonClicked?.Invoke();
    }

    // Stats Menu
    public void InvokeCameraEnteredStatsMenuDistance(Building building)
    {
        onCameraEnteredStatsMenuDistance?.Invoke(building);
    }

    public void InvokeCameraExitedStatsMenuDistance()
    {
        onCameraExitedStatsMenuDistance?.Invoke();
    }

    // Select
    public void InvokeObjectSelected(SelectComponent selectComponent)
    {
        onObjectSelected?.Invoke(selectComponent);
    }

    public void InvokeObjectDeselected()
    {
        onObjectDeselected?.Invoke();
    }
}
