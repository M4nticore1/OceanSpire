using System;
using System.Collections.Generic;
using System.Linq;
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
        // Preparing
        List<List<Building>> allPaths = new List<List<Building>>();
        allPaths.Add(new List<Building>());
        buildingsPath.Clear();

        if (startPlace || targetBuilding as TowerBuilding) {
            int pathIndex = 0;

            if (!startPlace)
                startPlace = manager.BuiltFloors[0].RoomBuildingPlaces[BuildingsManager.FirstBuildCityBuildingPlace];

            // Main
            HashSet<BuildingPlace> visitedBuildings = new HashSet<BuildingPlace>();
            bool found = FindPath(manager, startPlace, targetBuilding, allPaths, ref pathIndex, visitedBuildings);

            if (found) {
                buildingsPath = allPaths[allPaths.Count - 1].ToList();
            }

            return found;
        }
        else {
            buildingsPath.Add(targetBuilding);
            return true;
        }
    }

    private static bool FindPath(BuildingsManager manager, BuildingPlace startPlace, Building targetBuilding, List<List<Building>> buildingPaths, ref int pathIndex, HashSet<BuildingPlace> visitedBuildings, int enterPathIndex = 0, int pathLength = 0)
    {
        if (!startPlace) {
            Debug.LogError("startBuilding is null");
            return false;
        }

        if (!visitedBuildings.Add(startPlace)) return false;

        // Connect path with parent path
        if (pathIndex > 0 && buildingPaths[pathIndex].Count == 0) {
            for (int i = 0; i < pathLength; i++) {
                List<Building> path = buildingPaths[enterPathIndex];
                Building building = path[i];
                buildingPaths[pathIndex].Add(building);
            }
        }

        // Add this building as new
        if (startPlace.PlacedBuilding) {
            buildingPaths[pathIndex].Add(startPlace.PlacedBuilding);
        }

        if (startPlace.PlacedBuilding == targetBuilding) {
            return true;
        }

        bool hasStartElevator = startPlace.PlacedBuilding?.GetComponent<ElevatorModule>();
        int enterIndex = pathIndex;
        int currentPathLength = buildingPaths[enterPathIndex].Count;
        int buildingsCount = 0;

        // Get new paths
        foreach (BuildingPlace direction in hasStartElevator ? startPlace.NeighborPlaces(NeighborMask.All) : startPlace.NeighborPlaces(NeighborMask.Horizontal)) {
            if (!direction?.PlacedBuilding) {
                if (targetBuilding as GroundBuilding && direction.floorIndex == 0 && direction.PlaceIndex == BuildingsManager.FirstBuildCityBuildingPlace) {
                    return true;
                }

                continue;
            }

            if (visitedBuildings.Contains(direction)) continue;

            if (buildingsCount > 0) {
                pathIndex++;
                buildingPaths.Add(new List<Building>());
            }

            if (FindPath(manager, manager.BuiltFloors[direction.floorIndex].RoomBuildingPlaces[direction.PlaceIndex], targetBuilding, buildingPaths, ref pathIndex, visitedBuildings, enterIndex, currentPathLength))
                return true;

            buildingsCount++;
        }

        return false;
    }
}
