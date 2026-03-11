using System;
using System.Collections.Generic;
using UnityEngine;

public static class PathFinder
{
    public static bool TryGetPathToBuilding(BuildingsManager manager, BuildingPlace startPlace, Building targetBuilding, ref List<Building> buildingsPath)
    {
        return TryGetPathToBuilding_Internal(manager, startPlace, targetBuilding, ref buildingsPath);
    }

    public static bool TryGetPathToBuilding(BuildingsManager manager, BuildingPlace startPlace, Func<Building, bool> targetBuildingCondition, ref List<Building> buildingsPath)
    {
        Building targetBuilding = null;
        for (int i = 0; i < manager.BuiltFloors.Count; i++) {
            Building hall = manager.BuiltFloors[i].HallBuildingPlace.PlacedBuilding;
            if (hall && targetBuildingCondition(hall)) {
                targetBuilding = hall;
                break;
            }

            for (int j = 0; j < BuildingsManager.RoomsCountPerFloor; j++) {
                Building room = manager.BuiltFloors[i].RoomBuildingPlaces[j].PlacedBuilding;
                if (room && targetBuildingCondition(room)) {
                    targetBuilding = room;
                    break;
                }
            }

            if (targetBuilding)
                break;
        }

        return TryGetPathToBuilding_Internal(manager, startPlace, targetBuilding, ref buildingsPath);
    }

    private static bool TryGetPathToBuilding_Internal(BuildingsManager manager, BuildingPlace startPlace, Building targetBuilding, ref List<Building> buildingsPath)
    {
        buildingsPath.Clear();

        if (startPlace || targetBuilding is TowerBuilding) {
            if (!startPlace) {
                startPlace = manager.BuiltFloors[0].RoomBuildingPlaces[BuildingsManager.FirstBuildCityBuildingPlace];
            }

            // Find path
            List<Building> path = FindPath(manager, startPlace, targetBuilding);

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

    private static List<Building> FindPath(BuildingsManager manager, BuildingPlace startPlace, Building targetBuilding)
    {
        if (startPlace == null) {
            Debug.LogError("startPlace is null");
            return null;
        }

        Queue<(BuildingPlace place, List<Building> path)> queue = new();
        HashSet<BuildingPlace> visited = new();

        queue.Enqueue((startPlace, new List<Building>()));
        visited.Add(startPlace);

        while (queue.Count > 0) {
            var (place, path) = queue.Dequeue();
            List<Building> currentPath = new List<Building>(path);

            if (place.PlacedBuilding)
                currentPath.Add(place.PlacedBuilding);

            if (place.PlacedBuilding == targetBuilding)
                return currentPath;

            bool hasElevator = place.PlacedBuilding && place.PlacedBuilding.GetComponent<ElevatorModule>();
            NeighborMask mask = hasElevator ? NeighborMask.All : NeighborMask.Horizontal;

            foreach (BuildingPlace neighbor in place.NeighborPlaces(mask)) {
                // Check place
                if (!neighbor || visited.Contains(neighbor))
                    continue;

                // Check an emptiness of place
                bool isSpecialEmpty = neighbor.floorIndex == 0 && neighbor.PlaceIndex == BuildingsManager.FirstBuildCityBuildingPlace;
                if (!neighbor.PlacedBuilding && !isSpecialEmpty)
                    continue;

                // Check non elevator vertical building
                if (hasElevator && place.floorIndex != neighbor.floorIndex && !neighbor.PlacedBuilding.GetComponent<ElevatorModule>())
                    continue;

                visited.Add(neighbor);
                queue.Enqueue((neighbor, currentPath));
            }
        }

        return null;
    }
}
