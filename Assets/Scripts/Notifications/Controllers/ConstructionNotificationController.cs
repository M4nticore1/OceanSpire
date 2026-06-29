using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ConstructionNotificationController : NotificationController
{
    [Header("Construction Finished")]
    [SerializeField] private BuildingsManager buildingsManager;

    [SerializeField] private LocalizationItem constructionFinishedBodyLocalizationItem;
    [SerializeField] private LocalizationItem upgradeFinishedBodyLocalizationItem;

    protected override void ApplyNotifications()
    {
        NotificationsManager.SendNotification(GetNotificationLabel(), GetNotificationBodyText(), GetNotificationSubtitleText(), GetFireTimeInSeconds());
    }

    protected override bool ShouldSendNotification()
    {
        var buildingsUnderConstruction = GetBuildingsUnderConstruction();

        return buildingsUnderConstruction.Count >= 1;
    }

    protected override int GetFireTimeInSeconds()
    {
        var buildingsUnderConstruction = GetBuildingsUnderConstruction();

        if (buildingsUnderConstruction.Count == 1) {
            var building = buildingsUnderConstruction[0];

            return building.ConstructionComponent.GetRemainingConstructionTime().Value;
        }
        else if (buildingsUnderConstruction.Count > 1) {
            return GetMaxConstructionTimeBuilding(buildingsUnderConstruction).ConstructionComponent.GetRemainingConstructionTime().Value;
        }

        return 0;
    }

    protected override string GetNotificationBodyText()
    {
        var localizationManager = LocalizationManager.Instance;
        if (localizationManager == null) {
            Debug.LogError("localizationManager is not valid");
            return null;
        }

        var buildingsUnderConstruction = GetBuildingsUnderConstruction();

        if (buildingsUnderConstruction.Count == 1) {
            var building = buildingsUnderConstruction[0];

            if (building.UpgradeComponent.IsUnderUpgrade) {
                return localizationManager.GetText(upgradeFinishedBodyLocalizationItem, building);
            }
            else {
                return localizationManager.GetText(constructionFinishedBodyLocalizationItem, building);
            }
        }
        else if (buildingsUnderConstruction.Count > 1) {
         
            return localizationManager.GetText(BodyLocalizationItem);
        }

        return localizationManager.GetText(BodyLocalizationItem);
    }

    private Building GetMaxConstructionTimeBuilding(List<Building> buildings)
    {
        return buildings
        .OrderByDescending(b => b.ConstructionComponent.GetRemainingConstructionTime() ?? 0)
        .FirstOrDefault();
    }

    private List<Building> GetBuildingsUnderConstruction()
    {
        var constructionBuilding = new List<Building>();
        var groundBuildings = buildingsManager.GroundBuildings().Cast<Building>().ToList();

        var towerBuildings = new List<Building>();
        foreach (var floor in buildingsManager.BuiltFloors) {
            foreach (var place in floor.RoomBuildingPlaces) {
                var building = place.PlacedBuilding;
                if (!building) continue;

                towerBuildings.Add(building);
            }
        }

        constructionBuilding.AddRange(GetBuildingsUnderConstruction(groundBuildings));
        constructionBuilding.AddRange(GetBuildingsUnderConstruction(towerBuildings));

        return constructionBuilding;
    }

    protected List<Building> GetBuildingsUnderConstruction(List<Building> buildings)
    {
        var constructionBuilding = new List<Building>();

        foreach (var building in buildings) {
            if (!building) {
                Debug.LogError("buildin is not valid");
            }

            if (!building.ConstructionComponent.IsUnderConstruction) continue;

            constructionBuilding.Add(building);
        }

        return constructionBuilding;
    }
}