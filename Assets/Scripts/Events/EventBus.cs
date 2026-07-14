using System;
using UnityEngine;

public static class EventBus
{
    // Main Menu
    public static event Action<WorldData> OnLoadWorldButtonClicked;

    // City
    public static event Action OnNavMeshBaked;

    // Buildings
    public static event Action<Building> OnConstructionStarted;
    public static event Action OnConstructionStopped;

    // Boats
    public static event Action<Boat> OnBoatUnloaded;

    // Loot
    public static event Action<ItemInstance> OnMainStorageItemAmountChanged;
    public static event Action<StorageItem> OnMainStorageItemMaxAmountChanged;

    public static event Action<ItemInstance> OnItemRemoved;

    // Building Stats Menu
    public static event Action<Building> OnCameraEnteredStatsMenuDistance;
    public static event Action OnCameraExitedStatsMenuDistance;

    // Settings
    public static event Action OnPostProcessingToggleChanged;
    public static event Action<int> OnGeneralVolumeSliderMoved;
    public static event Action<int> OnMusicVolumeSliderMoved;

    // Input Listener
    public static event Action<GameObject> OnPlayerClicked;

    // Click
    public static event Action<DriftingLoot> OnClickedOnLootContainer;

    // UI
    public static event Action OnWorkersMenuClosed;

    // City
    public static void InvokeNavMeshBaked()
    {
        OnNavMeshBaked?.Invoke();
    }

    // Buildings
    public static void InvokeBuildingPlacingStarted(Building building)
    {
        OnConstructionStarted?.Invoke(building);
    }

    public static void InvokeConstructionStopped()
    {
        OnConstructionStopped?.Invoke();
    }

    // Boats
    public static void InvokeBoatExitedUnloadingState(Boat boat)
    {
        OnBoatUnloaded?.Invoke(boat);
    }

    // Loot
    public static void InvokeMainStorageAmountChanged(ItemInstance itemInstance)
    {
        OnMainStorageItemAmountChanged?.Invoke(itemInstance);
    }

    public static void InvokeMainStorageMaxAmountChanged(StorageItem itemInstance)
    {
        OnMainStorageItemMaxAmountChanged?.Invoke(itemInstance);
    }

    // Stats Menu
    public static void InvokeCameraEnteredStatsMenuDistance(Building building)
    {
        OnCameraEnteredStatsMenuDistance?.Invoke(building);
    }

    public static void InvokeCameraExitedStatsMenuDistance()
    {
        OnCameraExitedStatsMenuDistance?.Invoke();
    }

    // Input Listener
    public static void InvokeClicked(GameObject gameObject)
    {
        OnPlayerClicked?.Invoke(gameObject);
    }

    // Clicks

    public static void InvokeClickedOnLootContainer(DriftingLoot loot)
    {
        OnClickedOnLootContainer?.Invoke(loot);
    }

    // UI
    public static void InvokeWorkersMenuClosed()
    {
        OnWorkersMenuClosed?.Invoke();
    }
}
