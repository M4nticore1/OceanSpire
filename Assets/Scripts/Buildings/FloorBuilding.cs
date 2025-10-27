using System.Collections.Generic;
using UnityEngine;

public class FloorBuilding : Building
{
    // Building Places
    public List<BuildingPlace> roomBuildingPlaces;
    public BuildingPlace hallBuildingPlace;
    public BuildingPlace floorBuildingPlace;

    public override void Place(BuildingPlace buildingPlace, int levelIndex, bool isUnderConstruction, int interiorIndex)
    {
        base.Place(buildingPlace, levelIndex, isUnderConstruction, interiorIndex);

        InitializeBuilding(buildingPlace);
    }

    public override void InitializeBuilding(BuildingPlace buildingPlace)
    {
        base.InitializeBuilding(buildingPlace);

        floorBuildingPlace.InitializeBuildingPlace(GetFloorIndex() + 1);
        hallBuildingPlace.InitializeBuildingPlace(GetFloorIndex());
        for (int i = 0; i < CityManager.roomsCountPerFloor; i++)
            roomBuildingPlaces[i].InitializeBuildingPlace(GetFloorIndex());
    }

    public void ShowBuildingPlacesByType(Building building)
    {
        BuildingData buildingData = building.buildingData;

        bool hasPlaceAbove = false;
        bool hasPlaceBelow = false;

        int buildingHeight = buildingData.buildingFloors;

        //if (cityManager.buildedFloorsCount >= floorIndex + buildingHeight)
        //{
        //    for (int i = 0; i < buildingHeight; i++)
        //    {
        //        if (cityManager.spawnedFloors[floorIndex + i].hallBuildingPlace.isBuildingPlaced)
        //        {
        //            hasPlaceUp = false;

        //            break;
        //        }
        //    }
        //}
        //else
        //{
        //    hasPlaceUp = false;
        //}

        //if (floorIndex - buildingHeight >= 0)
        //{
        //    for (int i = floorIndex; i > floorIndex - buildingHeight; i--)
        //    {
        //        if (!cityManager.spawnedFloors[i].hallBuildingPlace.isBuildingPlaced)
        //        {

        //        }
        //        else
        //        {
        //            hasPlaceDown = false;
        //            break;
        //        }
        //    }
        //}
        //else
        //{
        //    hasPlaceDown = false;
        //}

        if (buildingData.buildingType == BuildingType.Room)
        {
            for (int i = 0; i < roomBuildingPlaces.Count; i++)
            {
                if (roomBuildingPlaces[i].emptyBuildingPlacesAbove >= buildingHeight - 1)
                    hasPlaceAbove = true;
                if (roomBuildingPlaces[i].emptyBuildingPlacesBelow >= buildingHeight - 1)
                    hasPlaceBelow = true;

                if (!roomBuildingPlaces[i].placedBuilding)
                {
                    if (hasPlaceAbove || hasPlaceBelow)
                        roomBuildingPlaces[i].ShowBuildingPlace(BuildingPlaceState.Valid);
                    else
                        roomBuildingPlaces[i].ShowBuildingPlace(BuildingPlaceState.Invalid);

                    if (!hasPlaceAbove)
                    {
                        for (int j = 1; j <= buildingData.buildingFloors; j++)
                        {
                            BuildingPlace currentBuildingPlace = cityManager.builtFloors[GetFloorIndex() - j].roomBuildingPlaces[i];

                            if (currentBuildingPlace.emptyBuildingPlacesAbove == buildingData.buildingFloors - 1)
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }
        else if (buildingData.buildingType == BuildingType.Hall)
        {
            if (!hallBuildingPlace.placedBuilding && cityManager.currentRoomsNumberOnFloor[GetFloorIndex()] == 0)
            {
                if(hallBuildingPlace.emptyBuildingPlacesAbove >= buildingHeight - 1)
                    hasPlaceAbove = true;
                if (hallBuildingPlace.emptyBuildingPlacesBelow >= buildingHeight - 1)
                    hasPlaceBelow = true;

                if (hasPlaceAbove || hasPlaceBelow)
                    hallBuildingPlace.ShowBuildingPlace(BuildingPlaceState.Valid);
                else
                    hallBuildingPlace.ShowBuildingPlace(BuildingPlaceState.Invalid);
            }
        }
        else if (buildingData.buildingType == BuildingType.FloorFrame)
        {
            if (!floorBuildingPlace.placedBuilding)
            {
                floorBuildingPlace.ShowBuildingPlace(BuildingPlaceState.Valid);
            }
        }
    }

    public void HideBuildingPlacesByType(BuildingType buildingType)
    {
        if (buildingType == BuildingType.Room)
        {
            for (int i = 0; i < roomBuildingPlaces.Count; i++)
            {
                roomBuildingPlaces[i].HideBuildingPlace();
            }
        }
        else if (buildingType == BuildingType.Hall)
        {
            hallBuildingPlace.HideBuildingPlace();
        }
        //else if (buildingType == BuildingType.Elevator)
        //{
        //    for (int i = 0; i < elevatorsBuildingPlaces.Count; i++)
        //    {
        //        elevatorsBuildingPlaces[i].HideBuildingPlace();
        //    }
        //}
        else if (buildingType == BuildingType.FloorFrame)
        {
            floorBuildingPlace.HideBuildingPlace();
        }
    }

    public void HideAllBuildingPlaces()
    {
        for (int i = 0; i < roomBuildingPlaces.Count; i++)
        {
            roomBuildingPlaces[i].HideBuildingPlace();
        }

        hallBuildingPlace.HideBuildingPlace();

        floorBuildingPlace.HideBuildingPlace();
    }
}
