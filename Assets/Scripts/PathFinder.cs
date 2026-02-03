using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class PathFinder
{
    //public List<List<Building>> allPaths = new List<List<Building>>();
    //public List<BuildingPath> allPaths2 = new List<BuildingPath>();

    public static bool TryGetPathToBuilding(CityManager city, Building startBuilding, Building targetBuilding, ref List<Building> buildingsPath)
    {
        return TryGetPathToBuilding_Internal(city, startBuilding, targetBuilding, ref buildingsPath);
    }

    public static bool TryGetPathToBuilding(CityManager city, Building startBuilding, Func<Building, bool> targetBuildingCondition, ref List<Building> buildingsPath)
    {
        Building targetBuilding = null;
        for (int i = 0; i < city.BuiltFloors.Count; i++) {
            Building hall = city.BuiltFloors[i].hallBuildingPlace.placedBuilding;
            if (hall && targetBuildingCondition(hall)) {
                targetBuilding = hall;
                break;
            }

            for (int j = 0; j < CityManager.roomsCountPerFloor; j++) {
                Building room = city.BuiltFloors[i].roomBuildingPlaces[j].placedBuilding;
                if (room && targetBuildingCondition(room)) {
                    targetBuilding = room;
                    break;
                }
            }

            if (targetBuilding)
                break;
        }

        return TryGetPathToBuilding_Internal(city, startBuilding, targetBuilding, ref buildingsPath);
    }

    private static bool TryGetPathToBuilding_Internal(CityManager city, Building startBuilding, Building targetBuilding, ref List<Building> buildingsPath)
    {
        // Preparing
        List<List<Building>> allPaths = new List<List<Building>>();
        buildingsPath.Clear();

        int pathIndex = 0;

        allPaths.Add(new List<Building>());

        List<bool> checkedBuildingPlaces = new List<bool>();
        for (int i = 0; i < city.BuiltFloors.Count * CityManager.roomsCountPerFloor; i++)
            checkedBuildingPlaces.Add(false);

        TowerBuilding startTowerBuilding = startBuilding as TowerBuilding;
        if (!startBuilding)
            startBuilding = city.BuiltFloors[0].roomBuildingPlaces[CityManager.firstBuildCityBuildingPlace].placedBuilding;

        // Main
        HashSet<Building> visitedBuildings = new HashSet<Building>();
        bool found = FindPath(city, startBuilding, targetBuilding, allPaths, ref pathIndex, visitedBuildings);

        //for (int i = 0; i < allPaths.Count; i++) {
        //    allPaths2.Add(new BuildingPath());
        //    for (int j = 0; j < allPaths[i].Count; j++) {
        //        allPaths2[i].paths.Add(allPaths[i][j]);
        //    }
        //}

        if (found) {
            buildingsPath = allPaths[allPaths.Count - 1].ToList();
        }

        return found;
    }

    private static bool FindPath(CityManager city, Building startBuilding, Building targetBuilding, List<List<Building>> buildingPaths, ref int pathIndex, HashSet<Building> visitedBuildings, int enterPathIndex = 0, int pathLength = 0)
    {
        if (!startBuilding) {
            Debug.LogError("startBuilding == NULL");
            return false;
        }

        if (!visitedBuildings.Add(startBuilding)) return false;

        // Connect path with parent path
        if (pathIndex > 0 && buildingPaths[pathIndex].Count == 0) {
            for (int i = 0; i < pathLength; i++) {
                buildingPaths[pathIndex].Add(buildingPaths[enterPathIndex][i]);
            }
        }

        // Add this building as new
        buildingPaths[pathIndex].Add(startBuilding);
        if (startBuilding == targetBuilding)
            return true;

        TowerBuilding startTowerBuilding = startBuilding as TowerBuilding;
        if (!startTowerBuilding) {
            Debug.LogError("startTowerBuilding is NULL");
            return false;
        }

        bool hasStartElevator = startTowerBuilding.GetComponent<ElevatorBuildingModule>();
        int enterIndex = pathIndex;
        int currentPathLength = buildingPaths[enterPathIndex].Count;
        // Get new paths
        int buildingsCount = 0;
        foreach (TowerBuilding direction in hasStartElevator ? startTowerBuilding.NeighborBuildings(NeighborMask.All) : startTowerBuilding.NeighborBuildings(NeighborMask.Horizontal)) {
            if (!direction) continue;
            if (visitedBuildings.Contains(direction)) continue;

            if (buildingsCount > 0) {
                pathIndex++;
                buildingPaths.Add(new List<Building>());
            }

            if (FindPath(city, city.BuiltFloors[direction.floorIndex].roomBuildingPlaces[direction.placeIndex].placedBuilding, targetBuilding, buildingPaths, ref pathIndex, visitedBuildings, enterIndex, currentPathLength))
                return true;
            buildingsCount++;
        }
        return false;
    }
}
