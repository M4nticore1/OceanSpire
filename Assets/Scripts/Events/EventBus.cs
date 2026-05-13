using System;
using System.Text;
using UnityEngine;

public static class EventBus
{
    // Main Menu
    public static event Action<WorldData> onLoadWorldButtonClicked;

    // City
    public static event Action onNavMeshBaked;

    // Buildings
    public static event Action<BuildingWidget> onBuildingWidgetBuildClicked;
    public static event Action<BuildingWidget> onBuildingWidgetInformationClicked;
    public static event Action<Building> onStartedPlacingBuilding;
    public static event Action<Building> onBuildingCreated;

    public static event Action onStopPlacingBuildingButtonClicked;

    // Production Module
    public static event Action<ProductionModule> onClickedProductionModule;

    // Boats
    public static event Action<Boat> onBoatCreated;
    public static event Action<Boat> onBoatUnloaded;
    public static event Action<int, int> onBoatUnloadedItem;

    // Loot
    public static event Action<ItemInstance> onMainStorageItemAmountChanged;
    public static event Action<StorageItem> onMainStorageItemMaxAmountChanged;

    public static event Action<ItemInstance> onItemRemoved;

    // Building Stats Menu
    public static event Action<Building> onCameraEnteredStatsMenuDistance;
    public static event Action onCameraExitedStatsMenuDistance;

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
    public static void InvokeNavMeshBaked()
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

    public static void InvokeStopPlacingBuildingButtonClicked()
    {
        onStopPlacingBuildingButtonClicked?.Invoke();
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

    // Loot
    public static void InvokeMainStorageAmountChanged(ItemInstance itemInstance)
    {
        onMainStorageItemAmountChanged?.Invoke(itemInstance);
    }

    public static void InvokeMainStorageMaxAmountChanged(StorageItem itemInstance)
    {
        onMainStorageItemMaxAmountChanged?.Invoke(itemInstance);
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
