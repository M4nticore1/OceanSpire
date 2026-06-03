using System;
using System.Collections.Generic;
using UnityEngine;

public static class PathFinder
{
    public static bool TryFindBuildingPath(BuildingPlace startPlace, Building targetBuilding, ref List<Building> buildingsPath)
    {
        buildingsPath.Clear();

        if (startPlace || targetBuilding is TowerBuilding) {
            if (!startPlace) {
                startPlace = BuildingsManager.Instance.BuiltFloors[0].RoomBuildingPlaces[BuildingsManager.FirstBuildCityBuildingPlace];
            }

            // Find path
            List<Building> path = FindBuildingPath(startPlace, targetBuilding);

            if (path != null) {
                buildingsPath.AddRange(path);
                return true;
            }
            return false;
        }
        else {
            buildingsPath.Add(targetBuilding);
            return true;
        }
    }

    public static bool TryFindBuildingPath(BuildingPlace startPlace, Func<Building, bool> targetBuildingCondition, ref List<Building> buildingsPath)
    {
        Building targetBuilding = null;

        for (int i = 0; i < BuildingsManager.Instance.BuiltFloors.Count; i++) {
            var hall = BuildingsManager.Instance.BuiltFloors[i].HallBuildingPlace.PlacedBuilding;

            if (hall && targetBuildingCondition(hall)) {
                targetBuilding = hall;
                break;
            }

            for (int j = 0; j < BuildingsManager.RoomsCountPerFloor; j++) {
                var room = BuildingsManager.Instance.BuiltFloors[i].RoomBuildingPlaces[j].PlacedBuilding;
                if (room && targetBuildingCondition(room)) {
                    targetBuilding = room;
                    break;
                }
            }

            if (targetBuilding)
                break;
        }

        return TryFindBuildingPath(startPlace, targetBuilding, ref buildingsPath);
    }

    private static List<Building> FindBuildingPath(BuildingPlace startPlace, Building targetBuilding)
    {
        if (!startPlace) {
            Debug.LogError("startPlace is null");
            return null;
        }

        if (!startPlace.PlacedBuilding)
            return null;

        Queue<(BuildingPlace place, List<Building> path)> queue = new();
        HashSet<BuildingPlace> visited = new();

        queue.Enqueue((startPlace, new List<Building>()));
        visited.Add(startPlace);

        while (queue.Count > 0) {
            var (place, path) = queue.Dequeue();
            var currentPath = new List<Building>(path);

            if (place.PlacedBuilding)
                currentPath.Add(place.PlacedBuilding);

            if (place.PlacedBuilding == targetBuilding)
                return currentPath;

            bool hasElevator = place.PlacedBuilding && place.PlacedBuilding.GetComponent<ElevatorModule>();
            NeighborMask mask = hasElevator ? NeighborMask.All : NeighborMask.Horizontal;

            foreach (BuildingPlace neighborPlace in place.GetNeighborPlaces(mask)) {
                // Check place
                if (!neighborPlace || visited.Contains(neighborPlace))
                    continue;

                // Check an emptiness of place
                bool isSpecialEmpty = neighborPlace.FloorIndex == 0 && neighborPlace.PlaceIndex == BuildingsManager.FirstBuildCityBuildingPlace;
                if (!neighborPlace.PlacedBuilding && !isSpecialEmpty) {
                    continue;
                }
                else if (isSpecialEmpty && targetBuilding as GroundBuilding) {
                    currentPath.Add(targetBuilding);
                    return currentPath;
                }

                // Check elevator
                if (hasElevator && place.FloorIndex != neighborPlace.FloorIndex && !neighborPlace.PlacedBuilding.GetComponent<ElevatorModule>()) continue;
                if (neighborPlace.PlacedBuilding && neighborPlace.PlacedBuilding.ConstructionComponent.IsUnderConstruction) continue;

                visited.Add(neighborPlace);
                queue.Enqueue((neighborPlace, currentPath));
            }
        }

        return null;
    }
}