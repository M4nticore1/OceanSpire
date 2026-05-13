using System;
using System.Collections.Generic;
using UnityEngine;

public static class PathFinder
{
    public static bool TryGetPathToBuilding(BuildingPlace startPlace, Building targetBuilding, ref List<Building> buildingsPath)
    {
        return TryGetPathToBuilding_Internal(startPlace, targetBuilding, ref buildingsPath);
    }

    public static bool TryGetPathToBuilding(BuildingPlace startPlace, Func<Building, bool> targetBuildingCondition, ref List<Building> buildingsPath)
    {
        Building targetBuilding = null;

        for (int i = 0; i < BuildingsManager.Instance.BuiltFloors.Count; i++) {
            Building hall = BuildingsManager.Instance.BuiltFloors[i].HallBuildingPlace.PlacedBuilding;
            if (hall && targetBuildingCondition(hall)) {
                targetBuilding = hall;
                break;
            }

            for (int j = 0; j < BuildingsManager.RoomsCountPerFloor; j++) {
                Building room = BuildingsManager.Instance.BuiltFloors[i].RoomBuildingPlaces[j].PlacedBuilding;
                if (room && targetBuildingCondition(room)) {
                    targetBuilding = room;
                    break;
                }
            }

            if (targetBuilding)
                break;
        }

        return TryGetPathToBuilding_Internal(startPlace, targetBuilding, ref buildingsPath);
    }

    private static bool TryGetPathToBuilding_Internal(BuildingPlace startPlace, Building targetBuilding, ref List<Building> buildingsPath)
    {
        buildingsPath.Clear();

        if (startPlace || targetBuilding is TowerBuilding) {
            if (!startPlace) {
                startPlace = BuildingsManager.Instance.BuiltFloors[0].RoomBuildingPlaces[BuildingsManager.FirstBuildCityBuildingPlace];
            }

            // Find path
            List<Building> path = FindPath(startPlace, targetBuilding);

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

    private static List<Building> FindPath(BuildingPlace startPlace, Building targetBuilding)
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

            foreach (BuildingPlace neighborPlace in place.NeighborPlaces(mask)) {
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
