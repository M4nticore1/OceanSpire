using System;
using System.Collections.Generic;
using UnityEngine;

public static class PathFinder
{
    public static bool TryFindBuildingPath(BuildingPlace startPlace, Building targetBuilding, out List<Building> buildingsPath)
    {
        buildingsPath = null;
        if (targetBuilding == null) return false;

        if (startPlace == null) {
            startPlace = BuildingsManager.Instance.EntranceBuildingPlace;
        }

        BuildingPlace targetBuildingPlace = null;
        if (targetBuilding is TowerBuilding tower) {
            targetBuildingPlace = tower.BuildingPlace;
        }
        else if (targetBuilding is GroundBuilding) {
            targetBuildingPlace = BuildingsManager.Instance.EntranceBuildingPlace;
        }

        bool pathFound = TryFindTowerPath(startPlace, targetBuildingPlace, out buildingsPath);
        if (!pathFound) {
            buildingsPath = null;
            return false;
        }

        if (targetBuilding is GroundBuilding) {
            buildingsPath ??= new List<Building>();

            if (!buildingsPath.Contains(targetBuilding)) {
                buildingsPath.Add(targetBuilding);
            }
            return true;
        }

        return true;
    }

    public static bool TryFindBuildingPath(BuildingPlace startPlace, Func<Building, bool> targetBuildingCondition, out List<Building> buildingsPath)
    {
        TowerBuilding targetBuilding = null;

        var builtFloors = BuildingsManager.Instance.BuiltFloors;

        foreach (var floor in builtFloors) {
            if (targetBuilding != null) break;
            if (floor == null) continue;

            foreach (var room in floor.RoomBuildingPlaces) {
                if (room == null) continue;
                var building = room.PlacedBuilding;
                if (building == null) continue;
                if (!targetBuildingCondition(building)) continue;

                targetBuilding = building as TowerBuilding;
                break;
            }
        }

        return TryFindBuildingPath(startPlace, targetBuilding, out buildingsPath);
    }

    public static bool TryFindTowerPath(BuildingPlace startPlace, BuildingPlace targetPlace, out List<Building> path)
    {
        path = null;

        if (startPlace == null) {
            Debug.LogError("StartPlace not found to find path");
            return false;
        }

        if (targetPlace == null) {
            Debug.LogError("TargetPlace not found to find path");
            return false;
        }

        if (startPlace.PlacedBuilding == null && startPlace != BuildingsManager.Instance.EntranceBuildingPlace)
            return false;

        if (targetPlace.PlacedBuilding == null && targetPlace != BuildingsManager.Instance.EntranceBuildingPlace)
            return false;

        var queue = new Queue<(BuildingPlace place, List<Building> currentPath)>();
        var visited = new HashSet<BuildingPlace>();

        queue.Enqueue((startPlace, new List<Building>()));
        visited.Add(startPlace);

        while (queue.Count > 0) {
            var (place, currentPath) = queue.Dequeue();
            var newPath = new List<Building>(currentPath);

            if (place.PlacedBuilding != null)
                newPath.Add(place.PlacedBuilding);

            if (place == targetPlace) {
                path = newPath;
                return true;
            }

            bool hasElevator = place.PlacedBuilding != null && place.PlacedBuilding.GetComponent<ElevatorModule>() != null;
            var mask = hasElevator ? NeighborMask.All : NeighborMask.Horizontal;

            foreach (var neighborPlace in place.GetNeighborPlaces(mask)) {
                if (neighborPlace == null) continue;
                if (neighborPlace.PlacedBuilding == null && neighborPlace != BuildingsManager.Instance.EntranceBuildingPlace) continue;
                if (visited.Contains(neighborPlace)) continue;

                bool neighborHasElevator = neighborPlace.PlacedBuilding != null && neighborPlace.PlacedBuilding.GetComponent<ElevatorModule>() != null;

                if (hasElevator && place.FloorIndex != neighborPlace.FloorIndex && !neighborHasElevator)
                    continue;

                visited.Add(neighborPlace);
                queue.Enqueue((neighborPlace, newPath));
            }
        }

        return false;
    }
}