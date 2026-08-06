using System.Collections.Generic;
using UnityEngine;

public class CraftsFinishedNotificationController : NotificationController
{
    [Header("Crafts")]
    [SerializeField] private BuildingsManager buildingsManager;

    protected override void ApplyNotifications()
    {
        NotificationsManager.SendNotification(GetLabelText(), GetBodyText(), GetSubtitleText(), GetFireTimeInSeconds());
    }

    protected override int GetFireTimeInSeconds()
    {
        return GetMaxRemainingCraftTime();
    }

    protected override bool ShouldSendNotification()
    {
        if (!buildingsManager) {
            Debug.LogError($"[{nameof(CraftsFinishedNotificationController)}] Buildings Manager is not valid");
            return false;
        }

        var crafts = GetCurrentCrafts();
        if (crafts.Count == 0) return false;

        return true;
    }

    private int GetMaxRemainingCraftTime()
    {
        var maxFireTime = 0;

        foreach (var craft in GetCurrentCrafts()) {
            var remainingTime = craft.GetRemainingCraftingTimeByCraftingTime();
            if (remainingTime < maxFireTime) continue;

            maxFireTime = remainingTime;
        }

        return maxFireTime;
    }

    private List<CraftItemInstance> GetCurrentCrafts()
    {
        var craftItems = new List<CraftItemInstance>();

        foreach (var floor in buildingsManager.BuiltFloors) {
            if (!floor) {
                Debug.LogError($"[{nameof(CraftsFinishedNotificationController)}] Floor is not valid");
                continue;
            }

            foreach (var place in floor.RoomBuildingPlaces) {
                if (!place) {
                    Debug.LogError($"[{nameof(CraftsFinishedNotificationController)}] Building Place is not valid");
                    continue;
                }

                var building = place.PlacedBuilding;
                if (!building) continue;

                if (!building.TryGetComponent<CraftingModule>(out var craftingModule)) continue;

                var selectedCraft = craftingModule.SelectedCraftItem;
                if (selectedCraft == null) {
                    Debug.LogError($"[{nameof(CraftsFinishedNotificationController)}] Selected Craft is not valid at module {craftingModule}");
                    continue;
                }

                if (selectedCraft.FinishTime == null) continue;

                var finishTime = selectedCraft.FinishTime;
                if (finishTime == null) continue;

                craftItems.Add(selectedCraft);
            }
        }

        return craftItems;
    }
}