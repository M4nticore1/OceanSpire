using System;
using System.Collections.Generic;
using UnityEngine;

public static class PathFinder
{
    public static bool TryFindBuildingPath(BuildingPlace startPlace, Building targetBuilding, out List<Building> buildingsPath)
    {
        buildingsPath = null;

        if (startPlace || targetBuilding is TowerBuilding) {
            if (!startPlace)
                startPlace = BuildingsManager.Instance.EntranceBuildingPlace;

            BuildingPlace targetBuildingPlace = null;
            if (targetBuilding as TowerBuilding)
                targetBuildingPlace = (targetBuilding as TowerBuilding).BuildingPlace;
            else if (targetBuilding as GroundBuilding)
                targetBuildingPlace = BuildingsManager.Instance.EntranceBuildingPlace;

            if (TryFindTowerPath(startPlace, targetBuildingPlace, out buildingsPath)) {
                if (targetBuilding as GroundBuilding) {
                    buildingsPath.Add(targetBuilding);
                }

                return true;
            }
        }
        else if (buildingsPath != null && targetBuilding as GroundBuilding) {
            buildingsPath = new();
            buildingsPath.Add(targetBuilding);
            return true;
        }

        return false;
    }

    public static bool TryFindBuildingPath(BuildingPlace startPlace, Func<Building, bool> targetBuildingCondition, out List<Building> buildingsPath)
    {
        TowerBuilding targetBuilding = null;

        for (int i = 0; i < BuildingsManager.Instance.BuiltFloors.Count; i++) {
            for (int j = 0; j < BuildingsManager.RoomsCountPerFloor; j++) {
                var room = BuildingsManager.Instance.BuiltFloors[i].RoomBuildingPlaces[j].PlacedBuilding;
                if (room && targetBuildingCondition(room)) {
                    targetBuilding = room;
                    break;
                }
            }
        }

        return TryFindBuildingPath(startPlace, targetBuilding, out buildingsPath);
    }

    public static bool TryFindTowerPath(BuildingPlace startPlace, BuildingPlace targetPlace, out List<Building> path)
    {
        path = null;

        if (!startPlace) {
            Debug.Log("StartPlace not found to find path");
            return false;
        }

        if (!targetPlace) {
            Debug.Log("TargetPlace not found to find path");
            return false;
        }

        if (!startPlace.PlacedBuilding && startPlace != BuildingsManager.Instance.EntranceBuildingPlace)
            return false;

        if (!targetPlace.PlacedBuilding && targetPlace != BuildingsManager.Instance.EntranceBuildingPlace)
            return false;

        Queue<(BuildingPlace place, List<Building> currentPath)> queue = new();
        HashSet<BuildingPlace> visited = new();

        queue.Enqueue((startPlace, new List<Building>()));
        visited.Add(startPlace);

        while (queue.Count > 0) {
            var (place, currentPath) = queue.Dequeue();
            var newPath = new List<Building>(currentPath);

            if (place.PlacedBuilding)
                newPath.Add(place.PlacedBuilding);

            if (place == targetPlace) {
                path = newPath;
                return true;
            }

            bool hasElevator = place.PlacedBuilding && place.PlacedBuilding.GetComponent<ElevatorModule>();
            NeighborMask mask = hasElevator ? NeighborMask.All : NeighborMask.Horizontal;

            foreach (var neighborPlace in place.GetNeighborPlaces(mask)) {
                if (!neighborPlace) continue;
                if (!neighborPlace.PlacedBuilding && neighborPlace != BuildingsManager.Instance.EntranceBuildingPlace) continue;
                if (visited.Contains(neighborPlace)) continue;

                if (hasElevator && place.FloorIndex != neighborPlace.FloorIndex && !neighborPlace.PlacedBuilding.GetComponent<ElevatorModule>())
                    continue;

                visited.Add(neighborPlace);
                queue.Enqueue((neighborPlace, newPath));
            }
        }

        return false;
    }
}