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
        NotificationsManager.SendNotification(GetLabelText(), GetBodyText(), GetSubtitleText(), GetFireTimeInSeconds());
    }

    protected override bool ShouldSendNotification()
    {
        var buildingsUnderConstruction = GetBuildingsUnderConstruction();
        if (buildingsUnderConstruction.Count == 0) return false;

        return true;
    }

    protected override int GetFireTimeInSeconds()
    {
        var buildingsUnderConstruction = GetBuildingsUnderConstruction();

        if (buildingsUnderConstruction.Count == 1) {
            var building = buildingsUnderConstruction[0];
            var time = building.ConstructionComponent.GetRemainingConstructionTime();

            if (time != null) {
                return time.Value;
            }
            else {
                return 0;
            }
        }
        else if (buildingsUnderConstruction.Count > 1) {
            return GetMaxConstructionTimeBuilding(buildingsUnderConstruction).ConstructionComponent.GetRemainingConstructionTime().Value;
        }

        return 0;
    }

    protected override string GetBodyText()
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
         
            return localizationManager.GetLocalizedText(BodyLocalizationItem);
        }

        return localizationManager.GetLocalizedText(BodyLocalizationItem);
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
        var groundBuildings = buildingsManager.GerGroundBuildings().Cast<Building>().ToList();

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
                Debug.LogError("Building is not valid");
                continue;
            }

            if (!building.ConstructionComponent.GetUnderConstruction()) continue;

            if (building.ConstructionComponent.GetRemainingConstructionTime() == null) {
                Debug.LogError("Building under construction has no construction remaining time");
                continue;
            }

            constructionBuilding.Add(building);
        }

        return constructionBuilding;
    }
}